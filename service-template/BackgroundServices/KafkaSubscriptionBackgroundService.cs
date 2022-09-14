using Confluent.Kafka;

namespace ServiceTemplate.BackgroundServices {
  public class KafkaSubscriptionBackgroundService : BackgroundService {
    private readonly IConsumer<Null, string> consumer;

    public KafkaSubscriptionBackgroundService(IConsumer<Null, string> consumer) {
      this.consumer = consumer;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) {
      return Task.Factory.StartNew(() => {
        this.consumer.Subscribe(new[] { "test" });
        try {
          while (!stoppingToken.IsCancellationRequested) {
            var consumeResult = this.consumer.Consume(stoppingToken);
            if (consumeResult == null)
              continue;

            Console.WriteLine(consumeResult.Message.Value);
          }
        }
        catch (Exception e) {
          Console.WriteLine(e.Message);
          throw;
        }
      });
    }
  }
}