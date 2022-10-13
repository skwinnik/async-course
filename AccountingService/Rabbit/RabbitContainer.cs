using EasyNetQ;
using EasyNetQ.Topology;

namespace AccountingService.Rabbit {
  public class RabbitContainer {
    public IBus Bus { get; set; }
    public Exchange UserExchange { get; set; }
    public Exchange TaskExchange { get; set; }
    public RabbitContainer(IBus bus) {
      this.Bus = bus;

      this.UserExchange = bus.Advanced.ExchangeDeclare("User", c =>
        c
            .WithType(ExchangeType.Topic)
            .AsDurable(true)
            .AsAutoDelete(true)
      );
      
      this.TaskExchange = bus.Advanced.ExchangeDeclare("Task", c =>
        c
            .WithType(ExchangeType.Topic)
            .AsDurable(true)
            .AsAutoDelete(true)
      );
    }
  }
}