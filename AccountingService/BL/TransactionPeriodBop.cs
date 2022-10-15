using AccountingService.Db;
using AccountingService.Rabbit;
using Common.Events;
using EasyNetQ;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.BL {
  public class TransactionPeriodBop {

    private readonly IDbContextFactory<ServiceDbContext> dbContextFactory;
    private readonly RabbitContainer rabbitContainer;

    public TransactionPeriodBop(IDbContextFactory<ServiceDbContext> dbContextFactory, RabbitContainer rabbitContainer) {
      this.dbContextFactory = dbContextFactory;
      this.rabbitContainer = rabbitContainer;
    }

    public async Task<Guid> OpenPeriod() {
      var serviceDbContext = await this.dbContextFactory.CreateDbContextAsync();
      var period = await serviceDbContext.TransactionPeriods.FirstOrDefaultAsync(t => t.IsOpen);
      if (period != null)
        throw new ApplicationException("Must close another period before opening a new one");

      var newPeriod = await serviceDbContext.TransactionPeriods.AddAsync(new AccountingService.Db.Models.TransactionPeriod {
        IsOpen = true,
        StartTime = DateTime.UtcNow,
        Name = $"{DateTime.UtcNow.Day}/{DateTime.UtcNow.Month}/{DateTime.UtcNow.Year} - {DateTime.Now.Hour}:{DateTime.Now.Minute}"
      });
      await serviceDbContext.SaveChangesAsync();

      await OnTransactionPeriodChanged(newPeriod.Entity);

      return newPeriod.Entity.Id;
    }

    public async Task<Guid> ClosePeriod() {
      var serviceDbContext = await this.dbContextFactory.CreateDbContextAsync();

      var period = await serviceDbContext.TransactionPeriods.SingleOrDefaultAsync(t => t.IsOpen);
      if (period == null)
        throw new ApplicationException($"No open periods");

      period.IsOpen = false;
      period.EndTime = DateTime.UtcNow;
      await serviceDbContext.SaveChangesAsync();

      await OnTransactionPeriodChanged(period);

      return period.Id;
    }

    private async Task OnTransactionPeriodChanged(Db.Models.TransactionPeriod period) {
      if (SchemaRegistry.Streaming_V1_TransactionPeriod.TrySerializeValidated(new Common.Events.Streaming.V1.TransactionPeriodEvent {
        Payload = new Common.Events.Streaming.V1.TransactionPeriodEvent.TransactionPeriod {
          Id = period.Id,
          StartTime = period.StartTime,
          EndTime = period.EndTime,
          IsOpen = period.IsOpen,
          Name = period.Name
        }
      }, out var jsonTrans)) {
        await this.rabbitContainer.Bus.Advanced.PublishAsync(this.rabbitContainer.TransactionPeriodExchange, "v1.streaming", false, new Message<string>(jsonTrans));
      }
    }
  }
}