using System.Text.RegularExpressions;
using Common.Auth;
using Common.Events;
using EasyNetQ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskService.BL.Tasks;
using TaskService.Db;
using TaskService.Rabbit;

namespace TaskService.Controllers {
  [ApiController]
  [Route("[controller]/[action]")]
  public class TaskController : ControllerBase {
    private readonly ServiceDbContext dbContext;
    private readonly TaskAssignManager taskAssignManager;
    private readonly TaskPriceManager taskPriceManager;
    private readonly RabbitContainer rabbitContainer;
    private readonly UserContext userContext;

    public TaskController(ServiceDbContext dbContext,
      TaskAssignManager taskAssignManager,
      TaskPriceManager taskPriceManager,
      RabbitContainer rabbitContainer, UserContext
      userContext) {
      this.dbContext = dbContext;
      this.taskAssignManager = taskAssignManager;
      this.taskPriceManager = taskPriceManager;
      this.rabbitContainer = rabbitContainer;
      this.userContext = userContext;
    }

    [HttpGet]
    [Authorize("user")]
    public async Task<ActionResult> GetCurrentUserTasks() {
      var id = this.userContext.GetCurrentUserId();
      return this.Ok(await this.dbContext.Tasks.Where(t => t.UserId == id).ToListAsync());
    }

    [HttpGet]
    [Authorize("admin", "manager")]
    public async Task<ActionResult> GetAllTasks() {
      return this.Ok(await this.dbContext.Tasks.ToListAsync());
    }

    [HttpPost]
    [Authorize("admin", "manager")]
    public async Task<ActionResult> Create([FromBody] string description) {
      var regex = new Regex(@"(\[(?<ticketId>.*)\])?\s?(?<description>.*)");
      var match = regex.Match(description);
      var task = await this.dbContext.Tasks.AddAsync(new Db.Models.Task {
        Description = match.Groups["description"].Value,
        TicketId = match.Groups["ticketId"]?.Value,
        UserId = await this.taskAssignManager.GetUserToAssign(),
        Fee = await this.taskPriceManager.GetFee(),
        Reward = await this.taskPriceManager.GetReward(),
      });

      await this.dbContext.SaveChangesAsync();

      if (SchemaRegistry.Streaming_V1_Task.TrySerializeValidated(new Common.Events.Streaming.V1.TaskEvent {
        Payload = new Common.Events.Streaming.V1.TaskEvent.Task {
          Id = task.Entity.Id,
          Description = task.Entity.Description,
          Status = (Common.Events.Streaming.V1.TaskStatus)task.Entity.Status,
          UserId = task.Entity.UserId,
          Fee = task.Entity.Fee,
          Reward = task.Entity.Reward,
          TicketId = task.Entity.TicketId
        }
      }, out var jsonTask)) {
        await this.rabbitContainer.Bus.Advanced.PublishAsync(this.rabbitContainer.TaskExchange, "v1.streaming", false, new Message<string>(jsonTask));
      }

      if (SchemaRegistry.Business_V1_TaskAssigned.TrySerializeValidated(new Common.Events.Business.V1.TaskAssigned {
        TaskId = task.Entity.Id,
        UserId = task.Entity.UserId
      }, out var jsonTaskAssigned)) {
        await this.rabbitContainer.Bus.Advanced.PublishAsync(this.rabbitContainer.TaskExchange, "v1.assigned", false, new Message<string>(jsonTaskAssigned));
      }

      return this.Ok();
    }

    [HttpPost]
    [Authorize("admin", "manager")]
    public async Task<ActionResult> Shuffle() {
      var tasks = await this.dbContext.Tasks.Where(t => t.Status == Db.Models.TaskStatus.Pending).ToListAsync();
      for (var i = 0; i < tasks.Count; i++) {
        tasks[i].UserId = await this.taskAssignManager.GetUserToAssign();
      }

      await this.dbContext.SaveChangesAsync();

      for (var i = 0; i < tasks.Count; i++) {
        if (SchemaRegistry.Business_V1_TaskAssigned.TrySerializeValidated(new Common.Events.Business.V1.TaskAssigned {
          TaskId = tasks[i].Id,
          UserId = tasks[i].UserId
        }, out var jsonTaskAssigned)) {
          await this.rabbitContainer.Bus.Advanced.PublishAsync(this.rabbitContainer.TaskExchange, "v1.assigned", false, new Message<string>(jsonTaskAssigned));
        }
      }

      return this.Ok();
    }

    [HttpPost]
    [Authorize("user")]
    public async Task<ActionResult> Complete([FromBody] Guid taskId) {
      var task = await this.dbContext.Tasks.FindAsync(taskId);

      if (task == null)
        return this.BadRequest();

      if (task.UserId != this.userContext.GetCurrentUserId())
        return this.Unauthorized();

      task.Status = Db.Models.TaskStatus.Completed;
      await this.dbContext.SaveChangesAsync();

      if (SchemaRegistry.Business_V1_TaskCompleted.TrySerializeValidated(new Common.Events.Business.V1.TaskCompleted {
        TaskId = task.Id,
        UserId = task.UserId
      }, out var jsonTaskCompleted)) {
        await this.rabbitContainer.Bus.Advanced.PublishAsync(this.rabbitContainer.TaskExchange, "v1.completed", false, new Message<string>(jsonTaskCompleted));
      }

      return this.Ok();
    }
  }
}