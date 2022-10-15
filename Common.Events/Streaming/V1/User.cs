using Newtonsoft.Json;

namespace Common.Events.Streaming.V1 {
  public class UserEvent : AbstractEvent {
    public class User {
      [JsonProperty("id", Required = Required.Always)]
      public Guid Id { get; set; }
      [JsonProperty("name", Required = Required.Always)]
      public string Name { get; set; } = "";
      [JsonProperty("roleName", Required = Required.Always)]
      public string RoleName { get; set; } = "";
    }
    public override Guid EventId => Guid.NewGuid();

    public override string EventName => typeof(UserEvent).Name;

    public override string EventDescription => "User streaming event";

    public override int EventVersion => 1;

    [JsonProperty("user", Required = Required.Always)]
    public User Payload { get; set; } = null!;
  }
}