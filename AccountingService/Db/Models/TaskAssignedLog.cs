namespace AccountingService.Db.Models {
  using System.ComponentModel.DataAnnotations.Schema;
  public class TaskAssignedLog {

    public Guid Id { get; set; } = Guid.Empty;

    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid TaskId { get; set; } = Guid.Empty;
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid UserId { get; set; } = Guid.Empty;

    public DateTime TimeStamp { get; set; } = DateTime.Now;
  }
}