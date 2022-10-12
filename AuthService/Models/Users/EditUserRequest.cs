namespace AuthService.Models.Users {
  public class EditUserRequest {
    public Guid? Id { get; set; }
    public string? UserName { get; set; }
    public Guid? RoleId { get; set; }
  }
}