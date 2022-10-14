using AccountingService.Db;

namespace AccountingService.BL {
  public class TransactionsBop {
    private readonly ServiceDbContext dbContext;

    public TransactionsBop(ServiceDbContext dbContext) {
      this.dbContext = dbContext;
    }

    public async Task CreditUser(Guid userId, decimal amount, Guid taskId) {
      await this.dbContext.Transactions.AddAsync(new Db.Models.Transaction {
        UserId = userId,
        TaskId = taskId,
        Credit = amount,
        Debit = 0,
      });
      await this.dbContext.SaveChangesAsync();
    }

    public async Task DebitUser(Guid userId, decimal amount, Guid taskId) {
      await this.dbContext.Transactions.AddAsync(new Db.Models.Transaction {
        UserId = userId,
        TaskId = taskId,
        Credit = 0,
        Debit = amount,
      });
      await this.dbContext.SaveChangesAsync();
    }
  }
}