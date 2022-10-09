namespace CudCommon {
  public class User {
    public Guid UserId { get; set; }
    public string UserName { get; set; } = "";
    public string RoleName { get; set; } = "";
  }

  public class UserCreated {
    public User User { get; set; } = new User();
  }

  public class UserChanged {
    public User User { get; set; } = new User();
  }
}