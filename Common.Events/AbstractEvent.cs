using Newtonsoft.Json;

namespace Common.Events {
  public abstract class AbstractEvent {
    [JsonProperty("eventId", Required = Required.Always)]
    public virtual Guid EventId { get; }
    [JsonProperty("eventName", Required = Required.Always)]
    public virtual string EventName { get; } = "";
    [JsonProperty("eventDescription", Required = Required.Always)]
    public virtual string EventDescription { get; } = "";
    [JsonProperty("eventVersion", Required = Required.Always)]
    public virtual int EventVersion { get; }
  }
}