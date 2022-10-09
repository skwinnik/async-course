using Microsoft.EntityFrameworkCore;
using TaskService.Db;
using TaskService.Messages;

namespace TaskService.BackgroundServices {
  public class UserChangedConsumerBackgroundService : BackgroundService {
    private readonly EasyNetQ.IBus bus;
    private readonly IDbContextFactory<ServiceDbContext> dbContextFactory;

    public UserChangedConsumerBackgroundService(EasyNetQ.IBus bus, IDbContextFactory<ServiceDbContext> dbContextFactory) {
      this.bus = bus;
      this.dbContextFactory = dbContextFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
      try {
        var dbContext = await this.dbContextFactory.CreateDbContextAsync(stoppingToken);
        await bus.PubSub.SubscribeAsync<CudCommon.UserChanged>("task-service",
          (msg, tkn) => Task.Factory.StartNew(async () => {
            var existingUser = await dbContext.Users.FindAsync(msg.User.UserId);
            if (existingUser != null)
              dbContext.Users.Remove(existingUser);
              
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