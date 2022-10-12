using Common.Auth;
using EasyNetQ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskService.BL.Tasks;
using TaskService.Db;

namespace TaskService.Controllers {
  [ApiController]
  [Route("[controller]/[action]")]
  public class TaskController : ControllerBase {
    private readonly ServiceDbContext dbContext;
    private readonly TaskAssignManager taskAssignManager;
    private readonly IBus rabbitBus;
    private readonly UserContext userContext;

    public TaskController(ServiceDbContext dbContext,
      TaskAssignManager taskAssignManager,
      IBus rabbitBus, UserContext
      userContext) {
      this.dbContext = dbContext;
      this.taskAssignManager = taskAssignManager;
      this.rabbitBus = rabbitBus;
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
      var task = await this.dbContext.Tasks.AddAsync(new Db.Models.Task {
        Description = description,
        UserId = await this.taskAssignManager.GetUserToAssign()
      });
      await this.dbContext.SaveChangesAsync();

      this.rabbitBus.PubSub.Publish<Common.CudEvents.TaskCreated>(new Common.CudEvents.TaskCreated {
        Task = new Common.Task {
          TaskId = task.Entity.Id,
          TaskDescription = task.Entity.Description,
          TaskStatus = task.Entity.Status,
          UserId = task.Entity.UserId
        }
      });

      this.rabbitBus.PubSub.Publish<Common.BusinessEvents.TaskAssigned>(new Common.BusinessEvents.TaskAssigned {
        Task = new Common.Task {
          TaskId = task.Entity.Id,
          TaskDescription = task.Entity.Description,
          TaskStatus = task.Entity.Status,
          UserId = task.Entity.UserId
        }
      });

      return this.Ok();
    }

    [HttpPost]
    [Authorize("admin", "manager")]
    public async Task<ActionResult> Shuffle() {
      var tasks = await this.dbContext.Tasks.Where(t => t.Status == Common.TaskStatus.Pending).ToListAsync();
      for (var i = 0; i < tasks.Count; i++) {
        tasks[i].UserId = await this.taskAssignManager.GetUserToAssign();
      }

      await this.dbContext.SaveChangesAsync();

      for (var i = 0; i < tasks.Count; i++) {
        this.rabbitBus.PubSub.Publish<Common.BusinessEvents.TaskAssigned>(new Common.BusinessEvents.TaskAssigned {
          Task = new Common.Task {
            TaskId = tasks[i].Id,
            TaskDescription = tasks[i].Description,
            TaskStatus = tasks[i].Status,
            UserId = tasks[i].UserId
          }
        });
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

      task.Status = Common.TaskStatus.Completed;
      await this.dbContext.SaveChangesAsync();

      this.rabbitBus.PubSub.Publish<Common.BusinessEvents.TaskCompleted>(new Common.BusinessEvents.TaskCompleted {
        Task = new Common.Task {
          TaskId = task.Id,
          TaskDescription = task.Description,
          TaskStatus = task.Status,
          UserId = task.UserId
        }
      });

      return this.Ok();
    }
  }
}