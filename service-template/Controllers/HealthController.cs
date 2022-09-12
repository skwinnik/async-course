using Microsoft.AspNetCore.Mvc;

namespace ServiceTemplate.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class HealthController : ControllerBase {

    [Route("[action]")]
    public ActionResult Ping() {
      return this.Ok();
    }
  }
}