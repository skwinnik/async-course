namespace AccountingService.Db.Models {
  using System.ComponentModel.DataAnnotations.Schema;
  public class Task {

    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.Empty;
    public string TicketId { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Fee { get; set; }
    public decimal Reward { get; set; }
    public Common.Events.Streaming.V3.TaskStatus Status { get; set; } = Common.Events.Streaming.V3.TaskStatus.Pending;
  }
}