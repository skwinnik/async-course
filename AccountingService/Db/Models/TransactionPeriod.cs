namespace AccountingService.Db.Models {
  using System.ComponentModel.DataAnnotations.Schema;
  public class TransactionPeriod {
    public Guid Id { get; set; } = Guid.Empty;
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    public string Name { get; set; } = "";
    public bool IsOpen { get; set; } = true;
  }
}