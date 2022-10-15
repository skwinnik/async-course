using AccountingService.Db;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.BL {
  public class TransactionsBop {
    private readonly IDbContextFactory<ServiceDbContext> dbContextFactory;

    public TransactionsBop(IDbContextFactory<ServiceDbContext> dbContextFactory) {
      this.dbContextFactory = dbContextFactory;
    }

    public async Task CreditUser(Guid userId, decimal amount, Guid taskId) {
      using var dbContext = await this.dbContextFactory.CreateDbContextAsync();
      await dbContext.Transactions.AddAsync(new Db.Models.Transaction {
        UserId = userId,
        TaskId = taskId,
        Credit = amount,
        Debit = 0,
      });
      await dbContext.SaveChangesAsync();
    }

    public async Task DebitUser(Guid userId, decimal amount, Guid taskId) {
      using var dbContext = await this.dbContextFactory.CreateDbContextAsync();
      await dbContext.Transactions.AddAsync(new Db.Models.Transaction {
        UserId = userId,
        TaskId = taskId,
        Credit = 0,
        Debit = amount,
      });
      await dbContext.SaveChangesAsync();
    }
  }
}