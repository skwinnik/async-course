using Confluent.Kafka;
using EasyNetQ;
using Microsoft.AspNetCore.Mvc;
using ServiceTemplate.Messages;

namespace ServiceTemplate.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class TestController : ControllerBase {
    private readonly IBus rabbitBus;
    private readonly IProducer<Null, string> kafkaProducer;

    public TestController(IBus bus, IProducer<Null, string> kafkaProducer) {
      this.rabbitBus = bus;
      this.kafkaProducer = kafkaProducer;
    }

    [Route("SendRabbitMessage")]
    [HttpPost]
    public async Task<ActionResult> SendRabbitMessage([FromBody] string text) {
      await this.rabbitBus.PubSub.PublishAsync<TestMessage>(new TestMessage { Body = text });
      return this.Ok();
    }

    [Route("SendKafkaMessage")]
    [HttpPost]
    public async Task<ActionResult> SendKafkaMessage([FromBody] string text) {
      await this.kafkaProducer.ProduceAsync("test", new Message<Null, string>() { Value = text });
      return this.Ok();
    }
  }
}