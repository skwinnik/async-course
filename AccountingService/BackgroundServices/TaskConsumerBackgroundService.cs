using Common.Events.Streaming.V3;
using EasyNetQ.Consumer;
using Microsoft.EntityFrameworkCore;
using AccountingService.Db;
using AccountingService.Rabbit;

namespace AccountingService.BackgroundServices {
  public class TaskConsumerBackgroundService : BackgroundService {
    private readonly RabbitContainer rabbitContainer;
    private readonly IDbContextFactory<ServiceDbContext> dbContextFactory;

    public TaskConsumerBackgroundService(RabbitContainer rabbitContainer, IDbContextFactory<ServiceDbContext> dbContextFactory) {
      this.rabbitContainer = rabbitContainer;
      this.dbContextFactory = dbContextFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
      try {
        var taskStreamingQueue = await rabbitContainer.Bus.Advanced.QueueDeclareAsync("accounting-service.task", c => c.AsAutoDelete(false).AsDurable(true));
        await this.rabbitContainer.Bus.Advanced.BindAsync(this.rabbitContainer.TaskExchange, taskStreamingQueue, "v3.streaming", new Dictionary<string, object>());

        rabbitContainer.Bus.Advanced.Consume(c => {
          c
            .ForQueue(taskStreamingQueue, handlers =>
              handlers.Add<string>(this.TaskStreamingV3Handler()),
              config => config.WithConsumerTag("accounting-service"));
        });

      }
      catch (Exception e) {
        Console.WriteLine(e.Message);
        throw;
      }
    }

    private IMessageHandler<string> TaskStreamingV3Handler() {
      return async (message, info, cancellationToken) => {
        using var dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken);

        try {
          if (!Common.Events.SchemaRegistry.Streaming_V3_Task.TryDeserializeValidated(message.Body, out TaskEvent result)) {
            Console.WriteLine("Unable to parse Task streaming event");
            Console.WriteLine(message.Body);
            return AckStrategies.NackWithRequeue;
          }

          var task = await dbContext.Tasks.FindAsync(result.Payload.Id);
          if (task == null)
            await dbContext.Tasks.AddAsync(new Db.Models.Task {
              Id = result.Payload.Id,
              Description = result.Payload.Description,
              Status = result.Payload.Status,
              TicketId = result.Payload.TicketId,
              Fee = result.Payload.Fee,
              Reward = result.Payload.Reward
            });

          if (task != null) {
            task.Description = result.Payload.Description;
            task.Status = result.Payload.Status;
            task.TicketId = result.Payload.TicketId;
            task.Fee = result.Payload.Fee;
            task.Reward = result.Payload.Reward;
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