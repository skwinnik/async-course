using System.Net;
using Confluent.Kafka;
using EasyNetQ;
using Microsoft.EntityFrameworkCore;
using AuthService.BackgroundServices;
using AuthService.Db;

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

      services.AddHostedService<RabbitSubscriptionBackgroundService>();
      services.AddHostedService<KafkaSubscriptionBackgroundService>();

      services.AddDbContextFactory<ServiceDbContext>(o => 
        o.UseNpgsql(this.Configuration.SqlConnectionString, 
          x => x.UseAdminDatabase("postgres")));

      ConfigureKafkaServices(services);
    }

    private void ConfigureKafkaServices(IServiceCollection services) {
      var producerConfig = new ProducerConfig {
        BootstrapServers = this.Configuration.KafkaBootstrapServers,
        ClientId = Dns.GetHostName()
      };

      var consumerConfig = new ConsumerConfig {
        BootstrapServers = this.Configuration.KafkaBootstrapServers,
        ClientId = Dns.GetHostName(),
        GroupId = "auth-service",
        AutoOffsetReset = AutoOffsetReset.Earliest
      };
      
      services.AddSingleton(new ProducerBuilder<Null, string>(producerConfig).Build());
      services.AddSingleton(new ConsumerBuilder<Null, string>(consumerConfig).Build());
    }
  }
}