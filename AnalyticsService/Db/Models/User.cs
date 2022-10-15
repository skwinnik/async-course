using System.ComponentModel.DataAnnotations.Schema;

namespace AnalyticsService.Db.Models {
  public class User {
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid UserId { get; set; } = Guid.Empty;
    public string UserName { get; set; } = "";
    public string RoleName { get; set; } = "";
  }
}