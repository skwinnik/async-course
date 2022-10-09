namespace Common {

  public enum TaskStatus {
    Pending = 0, Completed = 1
  }
  public class Task {
    public Guid TaskId { get; set; }
    public string TaskDescription { get; set; } = "";
    public TaskStatus TaskStatus { get; set; } = TaskStatus.Pending;
    public Guid UserId { get; set; } = Guid.Empty;
  }
}