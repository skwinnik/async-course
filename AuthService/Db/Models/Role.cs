namespace AuthService.Db.Models {
  public class Role {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
  }
}