namespace ServiceTemplate.BackgroundServices {
  public class SubscriptionBackgroundService : BackgroundService {
    private readonly EasyNetQ.IBus bus;
    public SubscriptionBackgroundService(EasyNetQ.IBus bus) {
      this.bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
      try {
        await bus.PubSub.SubscribeAsync<Test>("sub",
          (msg, tkn) => Task.Factory.StartNew(() => Console.WriteLine("msg!")),
          c => c.WithAutoDelete(),
          stoppingToken);
      }
      catch (Exception e) {
        Console.WriteLine(e.Message);
        throw;
      }
    }

  }

  public class Test { }
}