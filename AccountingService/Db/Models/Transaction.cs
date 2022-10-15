namespace AccountingService.Db.Models {
  using System.ComponentModel.DataAnnotations.Schema;

  public enum TransactionType {
    Task, Salary, Move
  }
  public class Transaction {
    public Guid Id { get; set; } = Guid.Empty;

    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid UserId { get; set; }
    public Guid TransactionPeriodId { get; set; }
    public string Description { get; set; } = "";
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public TransactionType TransactionType { get; set; } = TransactionType.Task;

    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
  }
}