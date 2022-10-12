using Microsoft.EntityFrameworkCore;
using TaskService.Db;

namespace TaskService.BL.Tasks {
  public class TaskAssignManager {
    private readonly ServiceDbContext dbContext;

    public TaskAssignManager(ServiceDbContext dbContext) {
      this.dbContext = dbContext;
    }

    public async Task AssignUser(TaskService.Db.Models.Task task) {
      task.UserId = await this.GetUserToAssign();;
    }

    public async Task<Guid> GetUserToAssign() {
      var users = await dbContext.Users.Where(u => u.RoleName == "user").ToListAsync();
      var inx = new Random().Next(0, users.Count);
      return users[inx].UserId;
    }
  }
}