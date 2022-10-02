namespace AuthService.Db {
  using Microsoft.EntityFrameworkCore;
  using AuthService.Db.Models;

  public class ServiceDbContext : Microsoft.EntityFrameworkCore.DbContext {
    public ServiceDbContext(DbContextOptions<ServiceDbContext> options) : base(options) {
        this.Database.EnsureCreated();
    }
    public DbSet<Message> Messages => Set<Message>();
  }
}