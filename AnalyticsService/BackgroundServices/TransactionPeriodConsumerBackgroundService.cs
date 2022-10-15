using Common.Events.Streaming.V1;
using EasyNetQ.Consumer;
using Microsoft.EntityFrameworkCore;
using AnalyticsService.Db;
using AnalyticsService.Rabbit;

namespace AnalyticsService.BackgroundServices {
  public class TransactionPeriodConsumerBackgroundService : BackgroundService {
    private readonly RabbitContainer rabbitContainer;
    private readonly IDbContextFactory<ServiceDbContext> dbContextFactory;

    public TransactionPeriodConsumerBackgroundService(RabbitContainer rabbitContainer, IDbContextFactory<ServiceDbContext> dbContextFactory) {
      this.rabbitContainer = rabbitContainer;
      this.dbContextFactory = dbContextFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
      try {
        var transactionStreamingQueue = await rabbitContainer.Bus.Advanced.QueueDeclareAsync("analytics-service.transactionPeriod", c => c.AsAutoDelete(false).AsDurable(true));
        await this.rabbitContainer.Bus.Advanced.BindAsync(this.rabbitContainer.TransactionPeriodExchange, transactionStreamingQueue, "v1.streaming", new Dictionary<string, object>());

        rabbitContainer.Bus.Advanced.Consume(c => {
          c
            .ForQueue(transactionStreamingQueue, handlers =>
              handlers.Add<string>(this.TransactionPeriodStreamingV1Handler()),
              config => config.WithConsumerTag("analytics-service"));
        });

      }
      catch (Exception e) {
        Console.WriteLine(e.Message);
        throw;
      }
    }

    private IMessageHandler<string> TransactionPeriodStreamingV1Handler() {
      return async (message, info, cancellationToken) => {
        using var dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken);

        try {
          if (!Common.Events.SchemaRegistry.Streaming_V1_TransactionPeriod.TryDeserializeValidated(message.Body, out TransactionPeriodEvent result)) {
            Console.WriteLine("Unable to parse Transaction Period streaming event");
            Console.WriteLine(message.Body);
            return AckStrategies.NackWithRequeue;
          }

          var trPeriod = await dbContext.TransactionPeriods.FindAsync(result.Payload.Id);
          if (trPeriod == null)
            await dbContext.TransactionPeriods.AddAsync(new Db.Models.TransactionPeriod {
              Id = result.Payload.Id,
              StartTime = result.Payload.StartTime,
              EndTime = result.Payload.EndTime,
              IsOpen = result.Payload.IsOpen,
              Name = result.Payload.Name
            });

          if (trPeriod != null) {
            trPeriod.StartTime = result.Payload.StartTime;
            trPeriod.EndTime = result.Payload.EndTime;
            trPeriod.IsOpen = result.Payload.IsOpen;
            trPeriod.Name = result.Payload.Name;
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