namespace AuthService.Db {
  using Microsoft.EntityFrameworkCore;
  using AuthService.Db.Models;

  public class ServiceDbContext : Microsoft.EntityFrameworkCore.DbContext {
    public ServiceDbContext(DbContextOptions<ServiceDbContext> options) : base(options) {
      this.Database.EnsureCreated();
    }
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      modelBuilder.Entity<User>().HasOne(p => p.Role);
    }
  }
}