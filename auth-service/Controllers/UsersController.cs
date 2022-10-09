using Microsoft.AspNetCore.Mvc;
using AuthService.Db;
using Microsoft.EntityFrameworkCore;
using AuthService.Models.Users;
using AuthService.Code.Auth;
using AuthCommon;
using EasyNetQ;

namespace AuthService.Controllers {
  [ApiController]
  [Route("[controller]/[action]")]
  public class UsersController : ControllerBase {
    private readonly ServiceDbContext dbContext;
    private readonly UserContext userContext;
    private readonly IBus rabbitBus;

    public UsersController(ServiceDbContext dbContext, UserContext userContext, IBus rabbitBus) {
      this.dbContext = dbContext;
      this.userContext = userContext;
      this.rabbitBus = rabbitBus;
    }

    [HttpGet]
    public async Task<ActionResult> Get() {
      return this.Ok(await this.dbContext.Users.Include(u => u.Role).ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateUserRequest user) {
      if (string.IsNullOrEmpty(user.Username))
        return this.BadRequest();
      if (string.IsNullOrEmpty(user.Password))
        return this.BadRequest();
      if (user.RoleId == null)
        return this.BadRequest();

      var role = await this.dbContext.Roles.FindAsync((Guid)user.RoleId);
      if (role == null)
        return this.BadRequest();

      var addedUser = await this.dbContext.Users.AddAsync(new Db.Models.User() { Name = user.Username, Password = user.Password, Role = role });
      await this.dbContext.SaveChangesAsync();

      this.rabbitBus.PubSub.Publish<CudCommon.UserCreated>(new CudCommon.UserCreated {
        User = new CudCommon.User {
          UserId = addedUser.Entity.Id,
          UserName = addedUser.Entity.Name,
          RoleName = role.Name
        }
      });

      return this.Ok();
    }

    [HttpPost]
    [Authorize("user", "admin")]
    public async Task<ActionResult> Edit([FromBody] EditUserRequest userRequest) {
      var currentUserId = this.userContext.GetCurrentUserId();
      if (currentUserId == null) return this.Unauthorized();

      if (userRequest.Id == null || userRequest.Id != currentUserId)
        return this.BadRequest();

      var user = await this.dbContext.Users.Include(u => u.Role).SingleOrDefaultAsync(u => u.Id == userRequest.Id);
      if (user == null)
        return this.BadRequest();

      if (userRequest.UserName != null)
        user.Name = userRequest.UserName;

      if (userRequest.RoleId != null) {
        var role = await this.dbContext.Roles.SingleOrDefaultAsync(r => r.Id == userRequest.RoleId);
        if (role == null)
          return this.BadRequest();

        user.RoleId = (Guid)userRequest.RoleId;
        user.Role = role;
      }

      await dbContext.SaveChangesAsync();

      this.rabbitBus.PubSub.Publish<CudCommon.UserChanged>(new CudCommon.UserChanged {
        User = new CudCommon.User {
          UserId = user.Id,
          UserName = user.Name,
          RoleName = user.Role.Name
        }
      });

      return this.Ok();
    }
  }
}