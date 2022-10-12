namespace AuthService {
  public class AppConfiguration {
    public string RabbitConnectionString { get; set; } = "";
    public string KafkaBootstrapServers { get; set; } = "";
    public string SqlConnectionString { get; set; } = "";
  }
}