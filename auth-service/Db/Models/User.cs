using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Db.Models {
  public class User {
    public Guid Id { get; set; } = Guid.Empty;
    
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid RoleId {get;set;} = Guid.Empty;
    public virtual Role Role { get; set; } = new Role();
    public string Name { get; set; } = "";
    // I store plain passwords because I don't care 😎
    public string Password { get; set; } = "";
  }
}