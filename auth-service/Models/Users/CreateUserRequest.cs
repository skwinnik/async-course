namespace AuthService.Models.Users {
  public class CreateUserRequest {
    public string? Username { get; set; }
    public string? Password { get; set; }
    public Guid? RoleId { get; set; }
  }
}