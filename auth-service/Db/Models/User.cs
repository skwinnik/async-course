namespace AuthService.Db.Models {
  public class User {
    public Guid Id { get; set; } = Guid.NewGuid();
    public virtual Role? Role { get; set; } = null;
    public string Name { get; set; } = "";

    // I store plain passwords because I don't care 😎
    public string Password { get; set; } = "";
  }
}