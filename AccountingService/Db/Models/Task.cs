namespace AccountingService.Db.Models {
  using System.ComponentModel.DataAnnotations.Schema;
  using Common;
  public class Task {
    
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.Empty;
    public string? TicketId { get; set; }
    public string Description { get; set; } = "";
    public Common.Events.Streaming.V1.TaskStatus Status { get; set; } = Common.Events.Streaming.V1.TaskStatus.Pending;
    
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid UserId { get; set; } = Guid.Empty;
  }
}