using EasyNetQ;
using Microsoft.AspNetCore.Mvc;
using ServiceTemplate.Messages;

namespace ServiceTemplate.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class TestController : ControllerBase {
    private readonly IBus bus;
    public TestController(IBus bus) {
      this.bus = bus;
    }

    [Route("SendMessage")]
    [HttpPost]
    public async Task<ActionResult> SendMessage([FromBody] string text) {
      await this.bus.PubSub.PublishAsync<TestMessage>(new TestMessage { Body = text });
      return this.Ok();
    }
  }
}