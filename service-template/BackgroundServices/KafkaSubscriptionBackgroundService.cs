using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using ServiceTemplate.Db;

namespace ServiceTemplate.BackgroundServices {
  public class KafkaSubscriptionBackgroundService : BackgroundService {
    private readonly IConsumer<Null, string> consumer;
    private readonly IDbContextFactory<ServiceDbContext> dbContextFactory;

    public KafkaSubscriptionBackgroundService(IConsumer<Null, string> consumer,
        IDbContextFactory<ServiceDbContext> dbContextFactory) {
      this.consumer = consumer;
      this.dbContextFactory = dbContextFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) {
      return Task.Factory.StartNew(async () => {
        var dbContext = await dbContextFactory.CreateDbContextAsync(stoppingToken);
        this.consumer.Subscribe(new[] { "test" });
        try {
          while (!stoppingToken.IsCancellationRequested) {
            var consumeResult = this.consumer.Consume(stoppingToken);
            if (consumeResult == null)
              continue;

            Console.WriteLine(consumeResult.Message.Value);
            await dbContext.Messages.AddAsync(new Db.Models.Message { Body = consumeResult.Message.Value, Source = "Kafka" });
            await dbContext.SaveChangesAsync(stoppingToken);
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