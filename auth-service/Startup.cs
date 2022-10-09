using EasyNetQ;
using Microsoft.EntityFrameworkCore;
using AuthService.Db;
using AuthService.Code.Auth;

namespace AuthService {
  public class Startup {
    public AppConfiguration Configuration { get; set; }
    public Startup(IConfiguration configuration) {
      this.Configuration = configuration.GetSection("appConfiguration").Get<AppConfiguration>();
    }

    public void Configure(IApplicationBuilder app) {
      app.UseRouting();
      app.UseEndpoints(u => u.MapControllers());

      app.UseSwagger();
      app.UseSwaggerUI();
    }

    public void ConfigureServices(IServiceCollection services) {
      services.AddControllers();
      services.AddEndpointsApiExplorer();
      services.AddSwaggerGen();

      services.AddSingleton<IBus>(s => RabbitHutch.CreateBus(this.Configuration.RabbitConnectionString));

      services.AddDbContextFactory<ServiceDbContext>(o => 
        o.UseNpgsql(this.Configuration.SqlConnectionString, 
          x => x.UseAdminDatabase("postgres")));

      services.AddHttpContextAccessor();
      services.AddScoped<AuthContext>();
      services.AddScoped<UserContext>();
    }
  }
}