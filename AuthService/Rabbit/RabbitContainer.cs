using EasyNetQ;
using EasyNetQ.Topology;

namespace AuthService.Rabbit {
  public class RabbitContainer {
    public IBus Bus { get; set; }
    public Exchange UserExchange { get; set; }
    public RabbitContainer(IBus bus) {
      this.Bus = bus;

      this.UserExchange = bus.Advanced.ExchangeDeclare("User", c =>
        c
            .WithType(ExchangeType.Topic)
            .AsDurable(true)
            .AsAutoDelete(true)
      );
    }
  }
}