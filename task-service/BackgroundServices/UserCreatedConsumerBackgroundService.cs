using Microsoft.EntityFrameworkCore;
using TaskService.Db;
namespace TaskService.BackgroundServices {
  public class UserCreatedConsumerBackgroundService : BackgroundService {
    private readonly EasyNetQ.IBus bus;
    private readonly IDbContextFactory<ServiceDbContext> dbContextFactory;

    public UserCreatedConsumerBackgroundService(EasyNetQ.IBus bus, IDbContextFactory<ServiceDbContext> dbContextFactory) {
      this.bus = bus;
      this.dbContextFactory = dbContextFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
      try {
        var dbContext = await this.dbContextFactory.CreateDbContextAsync(stoppingToken);
        await bus.PubSub.SubscribeAsync<Common.CudEvents.UserCreated>("task-service",
          (msg, tkn) => Task.Factory.StartNew(async () => {
            await dbContext.Users.AddAsync(new Db.Models.User { UserId = msg.User.UserId, UserName = msg.User.UserName, RoleName = msg.User.RoleName });
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