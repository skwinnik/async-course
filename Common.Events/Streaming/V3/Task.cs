using Newtonsoft.Json;

namespace Common.Events.Streaming.V3 {

  public enum TaskStatus {
    Pending = 0, Completed = 1
  }

  public class TaskEvent : AbstractEvent {
    public class Task {
      [JsonProperty("id", Required = Required.Always)]
      public Guid Id { get; set; }

      [JsonProperty("ticketId", Required = Required.Always)]
      public string TicketId { get; set; } = "";

      [JsonProperty("description", Required = Required.Always)]
      public string Description { get; set; } = "";

      [JsonProperty("fee", Required = Required.Always)]
      public decimal Fee { get; set; }

      [JsonProperty("reward", Required = Required.Always)]
      public decimal Reward { get; set; }

      [JsonProperty("status", Required = Required.Always)]
      public TaskStatus Status { get; set; } = TaskStatus.Pending;
      [JsonProperty("completedAt", Required = Required.Always)]
      public DateTime? CompletedAt { get; set; }
      [JsonProperty("userId", Required = Required.Always)]
      public Guid UserId { get; set; }
    }

    public override Guid EventId => Guid.NewGuid();

    public override string EventName => typeof(TaskEvent).Name;

    public override string EventDescription => "Task streaming event";

    public override int EventVersion => 3;

    [JsonProperty("task", Required = Required.Always)]
    public Task Payload { get; set; } = null!;


  }


}