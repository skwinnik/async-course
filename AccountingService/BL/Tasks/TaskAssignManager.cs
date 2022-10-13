using Microsoft.EntityFrameworkCore;
using AccountingService.Db;

namespace AccountingService.BL.Tasks {
  public class TaskAssignManager {
    private readonly ServiceDbContext dbContext;

    public TaskAssignManager(ServiceDbContext dbContext) {
      this.dbContext = dbContext;
    }

    public async Task AssignUser(AccountingService.Db.Models.Task task) {
      task.UserId = await this.GetUserToAssign();;
    }

    public async Task<Guid> GetUserToAssign() {
      var users = await dbContext.Users.Where(u => u.RoleName == "user").ToListAsync();
      var inx = new Random().Next(0, users.Count);
      return users[inx].UserId;
    }
  }
}