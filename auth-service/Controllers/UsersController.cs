using Microsoft.AspNetCore.Mvc;
using AuthService.Db;
using Microsoft.EntityFrameworkCore;
using AuthService.Models.Users;
using AuthService.Code.Auth;
using AuthCommon;

namespace AuthService.Controllers {
  [ApiController]
  [Route("[controller]/[action]")]
  public class UsersController : ControllerBase {
    private readonly ServiceDbContext dbContext;
    private readonly UserContext userContext;

    public UsersController(ServiceDbContext dbContext, UserContext userContext) {
      this.dbContext = dbContext;
      this.userContext = userContext;

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

      await this.dbContext.Users.AddAsync(new Db.Models.User() { Name = user.Username, Password = user.Password, Role = role });
      await this.dbContext.SaveChangesAsync();
      return this.Ok();
    }

    [HttpPost]
    [Authorize("user", "admin")]
    public async Task<ActionResult> Edit([FromBody] EditUserRequest userRequest) {
      var currentUserId = this.userContext.GetCurrentUserId();
      if (currentUserId == null) return this.Unauthorized();

      if (userRequest.Id == null || userRequest.Id != currentUserId)
        return this.BadRequest();

      var user = await this.dbContext.Users.SingleOrDefaultAsync(u => u.Id == userRequest.Id);
      if (user == null)
        return this.BadRequest();

      if (userRequest.UserName != null)
        user.Name = userRequest.UserName;
      
      if (userRequest.RoleId != null) {
        var role = await this.dbContext.Roles.SingleOrDefaultAsync(r => r.Id == userRequest.RoleId);
        if (role == null)
          return this.BadRequest();
        
        user.RoleId = (Guid)userRequest.RoleId;
      }

      await dbContext.SaveChangesAsync();

      return this.Ok();
    }
  }
}