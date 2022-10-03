using Microsoft.AspNetCore.Mvc;
using AuthService.Db;
using Microsoft.EntityFrameworkCore;
using AuthService.Models.Users;

namespace AuthService.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class UsersController : ControllerBase {
    private readonly ServiceDbContext dbContext;

    public UsersController(ServiceDbContext dbContext) {
      this.dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult> Get() {
      return this.Ok(await this.dbContext.Users.Include(u => u.Role).ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] CreateUserRequest user) {
      if (string.IsNullOrEmpty(user.Username))
        return this.BadRequest();
      if (string.IsNullOrEmpty(user.Password))
        return this.BadRequest();
      if (user.RoleId == null)
        return this.BadRequest();

      var role = await this.dbContext.Roles.FindAsync((Guid)user.RoleId);
      if (role == null)
        return this.BadRequest();

      await this.dbContext.Users.AddAsync(new Db.Models.User() { Name = user.Username, Password = user.Password, Role = role });
      await this.dbContext.SaveChangesAsync();
      return this.Ok();
    }
  }
}