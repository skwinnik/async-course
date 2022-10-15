using Common.Events.Streaming.V1;
using EasyNetQ.Consumer;
using Microsoft.EntityFrameworkCore;
using AccountingService.Db;
using AccountingService.Rabbit;
using Common.Events.Business.V1;
using AccountingService.BL;

namespace AccountingService.BackgroundServices {
  public class TransactionPeriodClosedConsumerBackgroundService : BackgroundService {
    private readonly RabbitContainer rabbitContainer;
    private readonly IDbContextFactory<ServiceDbContext> dbContextFactory;
    private readonly TransactionBop transactionBop;
    private readonly TransactionPeriodBop transactionPeriodBop;

    public TransactionPeriodClosedConsumerBackgroundService(RabbitContainer rabbitContainer, 
      IDbContextFactory<ServiceDbContext> dbContextFactory, 
      TransactionBop transactionsBop, 
      TransactionPeriodBop transactionPeriodBop) {
      this.rabbitContainer = rabbitContainer;
      this.dbContextFactory = dbContextFactory;
      this.transactionBop = transactionsBop;
      this.transactionPeriodBop = transactionPeriodBop;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
      try {
        var dayCompletedQueue = await rabbitContainer.Bus.Advanced.QueueDeclareAsync("accounting-service.transactionPeriod", c => c.AsAutoDelete(false).AsDurable(true));
        await this.rabbitContainer.Bus.Advanced.BindAsync(this.rabbitContainer.TransactionPeriodExchange, dayCompletedQueue, "v1.closed", new Dictionary<string, object>());

        rabbitContainer.Bus.Advanced.Consume(c => {
          c
            .ForQueue(dayCompletedQueue, handlers =>
              handlers.Add<string>(this.TransactionPeriodClosedV1Handler()),
              config => config.WithConsumerTag("accounting-service"));
        });

      }
      catch (Exception e) {
        Console.WriteLine(e.Message);
        throw;
      }
    }

    private IMessageHandler<string> TransactionPeriodClosedV1Handler() {
      return async (message, info, cancellationToken) => {
        try {
          using var dbContext = await this.dbContextFactory.CreateDbContextAsync(cancellationToken);
          if (!Common.Events.SchemaRegistry.Business_V1_DayCompleted.TryDeserializeValidated(message.Body, out TransactionPeriodClosedEvent closedPeriod)) {
            Console.WriteLine("Unable to parse Day Completed event");
            Console.WriteLine(message.Body);
            return AckStrategies.NackWithRequeue;
          }

          var newPeriodId = await this.transactionPeriodBop.OpenPeriod();

          var userIds = await dbContext.Users.Select(u => u.UserId).ToListAsync(cancellationToken);
          foreach (var userId in userIds) {
            await this.transactionBop.PaySalary(userId, closedPeriod.Id);
            await this.transactionBop.MoveDebtToNewPeriod(userId, closedPeriod.Id, newPeriodId);
          }

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