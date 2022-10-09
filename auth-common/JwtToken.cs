namespace AuthCommon {
  public class JwtToken {
    public Guid Id { get; set; } = Guid.Empty;
    public string Role { get; set; } = "";
    public DateTime Expire { get; set; } = DateTime.Now;
  }
}