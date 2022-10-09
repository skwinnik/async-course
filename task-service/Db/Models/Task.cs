namespace TaskService.Db.Models {
  using Common;
  public class Task {
    public Guid Id { get; set; } = Guid.Empty;
    public string Description { get; set; } = "";
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public Guid UserId { get; set; } = Guid.Empty;
    public virtual User User { get; set; } = new User();
  }
}