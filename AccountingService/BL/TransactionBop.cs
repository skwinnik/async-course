using AccountingService.Db;
using AccountingService.Rabbit;
using Common.Events;
using EasyNetQ;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.BL {
  public class TransactionBop {
    private readonly RabbitContainer rabbitContainer;
    private readonly IDbContextFactory<ServiceDbContext> dbContextFactory;
    private readonly TransactionPeriodBop transactionPeriodBop;

    public TransactionBop(IDbContextFactory<ServiceDbContext> dbContextFactory, TransactionPeriodBop transactionPeriodBop, RabbitContainer rabbitContainer) {
      this.dbContextFactory = dbContextFactory;
      this.transactionPeriodBop = transactionPeriodBop;
      this.rabbitContainer = rabbitContainer;
    }

    public async Task CreditUser(Guid userId, decimal amount, Guid taskId) {
      using var dbContext = await this.dbContextFactory.CreateDbContextAsync();
      var task = await dbContext.Tasks.FindAsync(taskId);
      if (task == null) throw new ApplicationException($"Task with ID {taskId} doesn't exist");

      var transactionPeriodId = (await dbContext.TransactionPeriods.SingleOrDefaultAsync(p => p.IsOpen))?.Id;
      if (transactionPeriodId == null)
        transactionPeriodId = await this.transactionPeriodBop.OpenPeriod();

      var tranDb = await dbContext.Transactions.AddAsync(new Db.Models.Transaction {
        UserId = userId,
        Description = $"Assigned Task - {task.TicketId} - {task.Description}",
        Credit = amount,
        Debit = 0,
        TransactionPeriodId = transactionPeriodId.Value
      });
      await dbContext.SaveChangesAsync();

      await this.OnTransactionCreated(tranDb.Entity);
    }

    public async Task DebitUser(Guid userId, decimal amount, Guid taskId) {
      using var dbContext = await this.dbContextFactory.CreateDbContextAsync();
      var task = await dbContext.Tasks.FindAsync(taskId);
      if (task == null) throw new ApplicationException($"Task with ID {taskId} doesn't exist");

      var transactionPeriodId = (await dbContext.TransactionPeriods.SingleOrDefaultAsync(p => p.IsOpen))?.Id;
      if (transactionPeriodId == null)
        transactionPeriodId = await this.transactionPeriodBop.OpenPeriod();

      var tranDb = await dbContext.Transactions.AddAsync(new Db.Models.Transaction {
        UserId = userId,
        Description = $"Completed Task - {task.TicketId} - {task.Description}",
        Credit = 0,
        Debit = amount,
        TransactionPeriodId = transactionPeriodId.Value
      });
      await dbContext.SaveChangesAsync();

      await this.OnTransactionCreated(tranDb.Entity);
    }

    public async Task PaySalary(Guid userId, Guid transactionPeriodId) {
      using var dbContext = await this.dbContextFactory.CreateDbContextAsync();
      var user = await dbContext.Users.FindAsync(userId);
      if (user == null) throw new ApplicationException($"User with ID {userId} doesn't exist");

      var transactionPeriod = await dbContext.TransactionPeriods.SingleOrDefaultAsync(p => p.Id == transactionPeriodId);
      if (transactionPeriod == null)
        throw new ApplicationException($"TransactionPeriod with ID {transactionPeriodId} doesn't exist");

      var transactions = await dbContext.Transactions.Where(t => t.UserId == userId && t.TransactionPeriodId == transactionPeriodId).ToListAsync();
      decimal amount = transactions.Sum(t => t.Debit - t.Credit);

      if (amount <= 0)
        return;

      var tranDb = await dbContext.Transactions.AddAsync(new Db.Models.Transaction {
        UserId = userId,
        Description = $"Salary - {transactionPeriod.Name}",
        Credit = amount,
        Debit = 0,
        TransactionPeriodId = transactionPeriodId
      });
      await dbContext.SaveChangesAsync();

      await this.OnTransactionCreated(tranDb.Entity);
    }

    public async Task MoveDebtToNewPeriod(Guid userId, Guid oldPeriodId, Guid newPeriodId) {
      using var dbContext = await this.dbContextFactory.CreateDbContextAsync();
      var user = await dbContext.Users.FindAsync(userId);
      if (user == null) throw new ApplicationException($"User with ID {userId} doesn't exist");

      var oldTransactionPeriod = await dbContext.TransactionPeriods.SingleOrDefaultAsync(p => p.Id == oldPeriodId);
      if (oldTransactionPeriod == null)
        throw new ApplicationException($"TransactionPeriod with ID {oldPeriodId} doesn't exist");

      var newTransactionPeriod = await dbContext.TransactionPeriods.SingleOrDefaultAsync(p => p.Id == newPeriodId);
      if (newTransactionPeriod == null)
        throw new ApplicationException($"TransactionPeriod with ID {newPeriodId} doesn't exist");

      var transactions = await dbContext.Transactions.Where(t => t.UserId == userId && t.TransactionPeriodId == oldPeriodId).ToListAsync();
      decimal amount = transactions.Sum(t => t.Debit - t.Credit);
      if (amount >= 0)
        return;

      var tran1 = await dbContext.Transactions.AddAsync(new Db.Models.Transaction {
        UserId = userId,
        Description = $"Move Credit To Next Period - {newTransactionPeriod.Name}",
        Credit = 0,
        Debit = Math.Abs(amount),
        TransactionPeriodId = oldTransactionPeriod.Id
      });

      var tran2 = await dbContext.Transactions.AddAsync(new Db.Models.Transaction {
        UserId = userId,
        Description = $"Moved Credit From Prev Period - {oldTransactionPeriod.Name}",
        Credit = Math.Abs(amount),
        Debit = 0,
        TransactionPeriodId = newTransactionPeriod.Id
      });

      await dbContext.SaveChangesAsync();

      await this.OnTransactionCreated(tran1.Entity);
      await this.OnTransactionCreated(tran2.Entity);
    }

    private async Task OnTransactionCreated(Db.Models.Transaction tran) {
      if (SchemaRegistry.Streaming_V1_Transaction.TrySerializeValidated(new Common.Events.Streaming.V1.TransactionEvent {
        Payload = new Common.Events.Streaming.V1.TransactionEvent.Transaction {
          Id = tran.Id,
          Credit = tran.Credit,
          Debit = tran.Debit,
          Description = tran.Description,
          TimeStamp = tran.TimeStamp,
          TransactionPeriodId = tran.TransactionPeriodId,
          UserId = tran.UserId
        }
      }, out var jsonTrans)) {
        await this.rabbitContainer.Bus.Advanced.PublishAsync(this.rabbitContainer.TransactionExchange, "v1.streaming", false, new Message<string>(jsonTrans));
      }
    }

    public async Task<IList<Db.Models.Transaction>> GetTransactions(Guid userId) {
      using var dbContext = await this.dbContextFactory.CreateDbContextAsync();

      return await dbContext.Transactions.Where(t => t.UserId == userId).ToListAsync();
    }
  }
}