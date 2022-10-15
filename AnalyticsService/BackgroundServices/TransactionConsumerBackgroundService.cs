using Common.Events.Streaming.V1;
using EasyNetQ.Consumer;
using Microsoft.EntityFrameworkCore;
using AnalyticsService.Db;
using AnalyticsService.Rabbit;

namespace AnalyticsService.BackgroundServices {
  public class TransactionConsumerBackgroundService : BackgroundService {
    private readonly RabbitContainer rabbitContainer;
    private readonly IDbContextFactory<ServiceDbContext> dbContextFactory;

    public TransactionConsumerBackgroundService(RabbitContainer rabbitContainer, IDbContextFactory<ServiceDbContext> dbContextFactory) {
      this.rabbitContainer = rabbitContainer;
      this.dbContextFactory = dbContextFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
      try {
        var transactionStreamingQueue = await rabbitContainer.Bus.Advanced.QueueDeclareAsync("analytics-service.transaction", c => c.AsAutoDelete(false).AsDurable(true));
        await this.rabbitContainer.Bus.Advanced.BindAsync(this.rabbitContainer.TransactionExchange, transactionStreamingQueue, "v1.streaming", new Dictionary<string, object>());

        rabbitContainer.Bus.Advanced.Consume(c => {
          c
            .ForQueue(transactionStreamingQueue, handlers =>
              handlers.Add<string>(this.TransactionStreamingV1Handler()),
              config => config.WithConsumerTag("analytics-service"));
        });

      }
      catch (Exception e) {
        Console.WriteLine(e.Message);
        throw;
      }
    }

    private IMessageHandler<string> TransactionStreamingV1Handler() {
      return async (message, info, cancellationToken) => {
        using var dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken);

        try {
          if (!Common.Events.SchemaRegistry.Streaming_V1_Transaction.TryDeserializeValidated(message.Body, out TransactionEvent result)) {
            Console.WriteLine("Unable to parse Transaction streaming event");
            Console.WriteLine(message.Body);
            return AckStrategies.NackWithRequeue;
          }

          var tran = await dbContext.Transactions.FindAsync(result.Payload.Id);
          if (tran != null)
            return AckStrategies.Ack;

          await dbContext.Transactions.AddAsync(new Db.Models.Transaction {
            Id = result.Payload.Id,
            UserId = result.Payload.UserId,
            Credit = result.Payload.Credit,
            Debit = result.Payload.Debit,
            Description = result.Payload.Description,
            TimeStamp = result.Payload.TimeStamp,
            TransactionPeriodId = result.Payload.TransactionPeriodId,
            TransactionType = result.Payload.TransactionType
          });

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