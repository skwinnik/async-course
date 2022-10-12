using System.Net;
using Common.Auth;
using Confluent.Kafka;
using EasyNetQ;
using Microsoft.EntityFrameworkCore;
using TaskService.BackgroundServices;
using TaskService.BL.Tasks;
using TaskService.Db;

namespace TaskService {
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

      services.AddHostedService<UserCreatedConsumerBackgroundService>();
      services.AddHostedService<UserChangedConsumerBackgroundService>();

      services.AddSingleton<TaskAssignManager>();

      services.AddHttpContextAccessor();
      services.AddScoped<AuthContext>();
      services.AddScoped<UserContext>();

      services.AddDbContextFactory<ServiceDbContext>(o =>
        o.UseNpgsql(this.Configuration.SqlConnectionString,
          x => x.UseAdminDatabase("postgres")));

    }
  }
}