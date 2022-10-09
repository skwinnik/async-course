namespace TaskService.Db.Models {
  public class User {
    public Guid UserId { get; set; } = Guid.Empty;
    public string UserName { get; set; } = "";
    public string RoleName { get; set; } = "";
  }
}