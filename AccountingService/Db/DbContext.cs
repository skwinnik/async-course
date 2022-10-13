namespace AccountingService.Db {
  using Microsoft.EntityFrameworkCore;
  using AccountingService.Db.Models;

  public class ServiceDbContext : Microsoft.EntityFrameworkCore.DbContext {
    public ServiceDbContext(DbContextOptions<ServiceDbContext> options) : base(options) {
      this.Database.EnsureCreated();
    }
    public DbSet<User> Users => Set<User>();
    public DbSet<Task> Tasks => Set<Task>();
  }
}