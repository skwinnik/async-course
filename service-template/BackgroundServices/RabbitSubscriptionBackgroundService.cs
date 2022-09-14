using Microsoft.EntityFrameworkCore;
using ServiceTemplate.Db;
using ServiceTemplate.Messages;

namespace ServiceTemplate.BackgroundServices {
  public class RabbitSubscriptionBackgroundService : BackgroundService {
    private readonly EasyNetQ.IBus bus;
    private readonly IDbContextFactory<ServiceDbContext> dbContextFactory;

    public RabbitSubscriptionBackgroundService(EasyNetQ.IBus bus, IDbContextFactory<ServiceDbContext> dbContextFactory) {
      this.bus = bus;
      this.dbContextFactory = dbContextFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
      try {
        var dbContext = await this.dbContextFactory.CreateDbContextAsync(stoppingToken);
        await bus.PubSub.SubscribeAsync<TestMessage>("sub",
          (msg, tkn) => Task.Factory.StartNew(async () => {
            Console.WriteLine(msg.Body);
            await dbContext.Messages.AddAsync(new Db.Models.Message { Body = msg.Body, Source = "Rabbit" });
            await dbContext.SaveChangesAsync(stoppingToken);
          }),
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