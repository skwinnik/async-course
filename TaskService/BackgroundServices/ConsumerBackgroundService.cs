using Common.Events.Streaming.V1;
using EasyNetQ.Consumer;
using Microsoft.EntityFrameworkCore;
using TaskService.Db;
using TaskService.Rabbit;

namespace TaskService.BackgroundServices {
  public class ConsumerBackgroundService : BackgroundService {
    private readonly RabbitContainer rabbitContainer;
    private readonly IDbContextFactory<ServiceDbContext> dbContextFactory;

    public ConsumerBackgroundService(RabbitContainer rabbitContainer, IDbContextFactory<ServiceDbContext> dbContextFactory) {
      this.rabbitContainer = rabbitContainer;
      this.dbContextFactory = dbContextFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
      try {
        var queue = await rabbitContainer.Bus.Advanced.QueueDeclareAsync("task-service.user", c => c.AsAutoDelete(false).AsDurable(true));
        await this.rabbitContainer.Bus.Advanced.BindAsync(this.rabbitContainer.UserExchange, queue, "v1.streaming", new Dictionary<string, object>());

        rabbitContainer.Bus.Advanced.Consume(c => {
          c.ForQueue(queue, handlers =>
            handlers.Add<string>(this.UserStreamingV1Handler()),
            config => config.WithConsumerTag("task-service"));
        });

      }
      catch (Exception e) {
        Console.WriteLine(e.Message);
        throw;
      }
    }

    private IMessageHandler<string> UserStreamingV1Handler() {
      return async (message, info, cancellationToken) => {
        try {
          using var dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken);
          if (!Common.Events.SchemaRegistry.Streaming_V1_User.TryDeserializeValidated(message.Body, out UserEvent result)) {
            Console.WriteLine("Unable to parse User streaming event");
            Console.WriteLine(message.Body);
            return AckStrategies.NackWithRequeue;
          }

          var user = await dbContext.Users.FindAsync(result.Payload.Id);
          if (user == null)
            await dbContext.Users.AddAsync(new Db.Models.User { UserId = result.Payload.Id, UserName = result.Payload.Name, RoleName = result.Payload.RoleName });

          if (user != null) {
            user.UserName = result.Payload.Name;
            user.RoleName = result.Payload.RoleName;
          }

          await dbContext.SaveChangesAsync(cancellationToken);

          return AckStrategies.Ack;
        }
        catch (Exception e) {
          Console.WriteLine(e);
          await Task.Delay(1000);
          return AckStrategies.NackWithRequeue;
        }
      };
    }
  }
}