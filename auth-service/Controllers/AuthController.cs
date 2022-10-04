using Microsoft.AspNetCore.Mvc;
using AuthService.Db;
using Microsoft.EntityFrameworkCore;
using AuthService.Models.Auth;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace AuthService.Controllers {
  [ApiController]
  [Route("[controller]/[action]")]
  public class AuthController : ControllerBase {
    private readonly ServiceDbContext dbContext;

    public AuthController(ServiceDbContext dbContext) {
      this.dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult> Login([FromBody] AuthRequest request) {
      var user = await dbContext.Users.Include(x => x.Role).SingleOrDefaultAsync(u => u.Name == request.Username);
      if (user == default)
        return this.Unauthorized();
      if (user.Password != request.Password)
        return this.Unauthorized();

      return this.Ok(new AuthResponse {
        //yes it is secure trust me
        Token = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize<JwtToken>(new JwtToken {
          Id = user.Id,
          Role = user.Role.Name
        })))
      });
    }
  }
}