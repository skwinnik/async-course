using AnalyticsService.Db;
using Microsoft.EntityFrameworkCore;

namespace AnalyticsService.BL {
  public class ReportBop {
    private readonly IDbContextFactory<ServiceDbContext> dbContextFactory;

    public ReportBop(IDbContextFactory<ServiceDbContext> dbContextFactory) {
      this.dbContextFactory = dbContextFactory;
    }

    public async Task<ManagementEarnedReport> GetManagementEarnedReport(Guid transactionPeriodId) {
      var dbContext = await this.dbContextFactory.CreateDbContextAsync();
      var transactions = await dbContext.Transactions.Where(t => t.TransactionPeriodId == transactionPeriodId 
        && t.TransactionType != Common.Events.Streaming.V1.TransactionEvent.TransactionType.Move).ToListAsync();

      var amount = transactions.Sum(t => t.Credit - t.Debit);

      return new ManagementEarnedReport {
        TransactionPeriodId = transactionPeriodId,
        Amount = amount
      };
    }

    public async Task<MostExpensiveTaskReport> GetMostExpensiveTaskReport(DateTime min, DateTime max) {
      var dbContext = await this.dbContextFactory.CreateDbContextAsync();
      var task = await dbContext.Tasks
          .Where(t => t.Status == Common.Events.Streaming.V3.TaskStatus.Completed && t.CompletedAt > min && t.CompletedAt < max)
          .OrderByDescending(t => t.Reward)
          .FirstOrDefaultAsync();

      return new MostExpensiveTaskReport {
        DateMin = min,
        DateMax = max,
        TaskId = task?.Id,
        TaskDescription = task?.Description,
        TaskReward = task?.Reward
      };
    }
  }

  public class ManagementEarnedReport {
    public Guid TransactionPeriodId { get; set; }
    public decimal Amount { get; set; }
  }

  public class MostExpensiveTaskReport {
    public DateTime DateMin { get; set; }
    public DateTime DateMax { get; set; }

    public Guid? TaskId { get; set; }
    public string? TaskDescription { get; set; }
    public decimal? TaskReward { get; set; }
  }
}


