using Newtonsoft.Json;

namespace Common.Events.Business.V1 {
  public class TaskCompleted : AbstractEvent {
    public override Guid EventId => Guid.NewGuid();

    public override string EventName => typeof(TaskCompleted).Name;

    public override string EventDescription => "Task completed event";

    public override int EventVersion => 1;

    [JsonProperty("userId", Required = Required.Always)]
    public Guid UserId { get; set; }
    [JsonProperty("taskId", Required = Required.Always)]
    public Guid TaskId { get; set; }
  }
}