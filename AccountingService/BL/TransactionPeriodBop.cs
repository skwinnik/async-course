using AccountingService.Db;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.BL {
  public class TransactionPeriodBop {

    private readonly IDbContextFactory<ServiceDbContext> dbContextFactory;

    public TransactionPeriodBop(IDbContextFactory<ServiceDbContext> dbContextFactory) {
      this.dbContextFactory = dbContextFactory;
    }

    public async Task<Guid> OpenPeriod() {
      var serviceDbContext = await this.dbContextFactory.CreateDbContextAsync();
      var period = await serviceDbContext.TransactionPeriods.FirstOrDefaultAsync(t => t.IsOpen);
      if (period != null)
        throw new ApplicationException("Must close another period before opening a new one");

      var newPeriod = await serviceDbContext.TransactionPeriods.AddAsync(new AccountingService.Db.Models.TransactionPeriod {
        IsOpen = true,
        TimeStamp = DateTime.UtcNow,
        Name = $"{DateTime.UtcNow.Day}/{DateTime.UtcNow.Month}/{DateTime.UtcNow.Year}"
      });
      await serviceDbContext.SaveChangesAsync();

      return newPeriod.Entity.Id;
    }

    public async Task<Guid> ClosePeriod() {
      var serviceDbContext = await this.dbContextFactory.CreateDbContextAsync();
      
      var period = await serviceDbContext.TransactionPeriods.SingleOrDefaultAsync(t => t.IsOpen);
      if (period == null)
        throw new ApplicationException($"No open periods");

      period.IsOpen = false;
      await serviceDbContext.SaveChangesAsync();

      return period.Id;
    }
  }
}