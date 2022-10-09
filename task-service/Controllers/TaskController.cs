using AuthCommon;
using Confluent.Kafka;
using EasyNetQ;
using Microsoft.AspNetCore.Mvc;
using TaskService.Messages;

namespace TaskService.Controllers {
  [ApiController]
  [Route("[controller]/[action]")]
  public class TaskController : ControllerBase {

    public TaskController() {
    }

    [HttpPost]
    [Authorize("admin", "manager")]
    public async Task<ActionResult> Create([FromBody] string description) {
      return this.Ok();
    }
  }
}