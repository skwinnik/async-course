using ServiceTemplate.Messages;

namespace ServiceTemplate.BackgroundServices {
  public class RabbitSubscriptionBackgroundService : BackgroundService {
    private readonly EasyNetQ.IBus bus;
    public RabbitSubscriptionBackgroundService(EasyNetQ.IBus bus) {
      this.bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
      try {
        await bus.PubSub.SubscribeAsync<TestMessage>("sub",
          (msg, tkn) => Task.Factory.StartNew(() => Console.WriteLine(msg.Body)),
          c => c.WithAutoDelete(),
          stoppingToken);
      }
      catch (Exception e) {
        Console.WriteLine(e.Message);
        throw;
      }
    }

  }
}