namespace TaskService.Db.Models {
  using System.ComponentModel.DataAnnotations.Schema;
  using Common;
  public class Task {
    public Guid Id { get; set; } = Guid.Empty;
    public string? TicketId { get; set; }
    public string Description { get; set; } = "";
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid UserId { get; set; } = Guid.Empty;
  }

  public enum TaskStatus {
    Pending = 0, Completed = 1
  }
}