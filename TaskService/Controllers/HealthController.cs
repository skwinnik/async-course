using Microsoft.AspNetCore.Mvc;

namespace TaskService.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class HealthController : ControllerBase {

    [HttpGet]
    [Route("[action]")]
    public ActionResult Ping() {
      return this.Ok();
    }
  }
}