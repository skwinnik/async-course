using System.ComponentModel.DataAnnotations.Schema;

namespace AnalyticsService.Db.Models {
  public class TransactionPeriod {
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.Empty;
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public string Name { get; set; } = "";
    public bool IsOpen { get; set; } = true;
  }
}