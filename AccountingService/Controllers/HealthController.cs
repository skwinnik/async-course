using Microsoft.AspNetCore.Mvc;

namespace AccountingService.Controllers {
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