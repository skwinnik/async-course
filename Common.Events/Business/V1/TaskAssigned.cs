using Newtonsoft.Json;

namespace Common.Events.Business.V1 {
  public class TaskAssigned : AbstractEvent {
    public override Guid EventId => Guid.NewGuid();

    public override string EventName => typeof(TaskAssigned).Name;

    public override string EventDescription => "Task assigned event";

    public override int EventVersion => 1;
    [JsonProperty("userId", Required = Required.Always)]
    public Guid UserId { get; set; }
    [JsonProperty("taskId", Required = Required.Always)]
    public Guid TaskId { get; set; }
  }
}