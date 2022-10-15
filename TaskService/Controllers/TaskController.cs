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

    public class CreateTaskRequest {
      public string TicketId { get; set; } = "";
      public string Description { get; set; } = "";
    }

    [HttpPost]
    [Authorize("admin", "manager")]
    public async Task<ActionResult> Create([FromBody] CreateTaskRequest request) {
      var task = await this.dbContext.Tasks.AddAsync(new Db.Models.Task {
        Description = request.Description,
        TicketId = request.TicketId,
        UserId = await this.taskAssignManager.GetUserToAssign(),
        Fee = await this.taskPriceManager.GetFee(),
        Reward = await this.taskPriceManager.GetReward(),
      });

      await this.dbContext.SaveChangesAsync();

      if (SchemaRegistry.Streaming_V3_Task.TrySerializeValidated(new Common.Events.Streaming.V3.TaskEvent {
        Payload = new Common.Events.Streaming.V3.TaskEvent.Task {
          Id = task.Entity.Id,
          Description = task.Entity.Description,
          Status = (Common.Events.Streaming.V3.TaskStatus)task.Entity.Status,
          Fee = task.Entity.Fee,
          Reward = task.Entity.Reward,
          TicketId = task.Entity.TicketId
        }
      }, out var jsonTask)) {
        await this.rabbitContainer.Bus.Advanced.PublishAsync(this.rabbitContainer.TaskExchange, "v3.streaming", false, new Message<string>(jsonTask));
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

      if (task.Status == Db.Models.TaskStatus.Completed)
        return this.Ok();

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