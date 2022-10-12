using Microsoft.AspNetCore.Mvc;
using AuthService.Db;
using Microsoft.EntityFrameworkCore;
using AuthService.Models.Roles;

namespace AuthService.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class RolesController : ControllerBase {
    private readonly ServiceDbContext dbContext;

    public RolesController(ServiceDbContext dbContext) {
      this.dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult> Get() {
      return this.Ok(await this.dbContext.Roles.ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] CreateRoleRequest role) {
      if (string.IsNullOrEmpty(role.Name))
        return this.BadRequest();

      await this.dbContext.Roles.AddAsync(new Db.Models.Role(){ Name = role.Name });
      await this.dbContext.SaveChangesAsync();
      return this.Ok();
    }
  }
}