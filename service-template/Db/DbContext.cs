namespace ServiceTemplate.Db {
  using Microsoft.EntityFrameworkCore;
  using ServiceTemplate.Db.Models;

  public class ServiceDbContext : Microsoft.EntityFrameworkCore.DbContext {
    public ServiceDbContext(DbContextOptions<ServiceDbContext> options) : base(options) {
        this.Database.EnsureCreated();
    }
    public DbSet<Message> Messages { get; set; }
  }
}