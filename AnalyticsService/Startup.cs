using Common.Auth;
using EasyNetQ;
using Microsoft.EntityFrameworkCore;
using AnalyticsService.BackgroundServices;
using AnalyticsService.Db;
using AnalyticsService.Rabbit;
using AnalyticsService.BL;

namespace AnalyticsService {
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
      services.AddSingleton<RabbitContainer>();

      services.AddHostedService<UserConsumerBackgroundService>();
      services.AddHostedService<TaskConsumerBackgroundService>();
      services.AddHostedService<TaskStatusConsumerBackgroundService>();
      services.AddHostedService<TransactionPeriodClosedConsumerBackgroundService>();

      services.AddSingleton<TransactionBop>();
      services.AddSingleton<TransactionPeriodBop>();

      services.AddHttpContextAccessor();
      services.AddScoped<AuthContext>();
      services.AddScoped<UserContext>();

      services.AddDbContextFactory<ServiceDbContext>(o =>
        o.UseNpgsql(this.Configuration.SqlConnectionString,
          x => x.UseAdminDatabase("postgres")));

    }
  }
}