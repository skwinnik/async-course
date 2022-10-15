using Newtonsoft.Json;

namespace Common.Events.Streaming.V1 {

  public class TransactionPeriodEvent : AbstractEvent {
    public class TransactionPeriod {
      [JsonProperty("id", Required = Required.Always)]

      public Guid Id { get; set; } = Guid.Empty;

      [JsonProperty("startTime", Required = Required.Always)]

      public DateTime StartTime { get; set; } = DateTime.UtcNow;

      [JsonProperty("endTime", Required = Required.Default)]
      public DateTime? EndTime { get; set; }

      [JsonProperty("name", Required = Required.Always)]
      public string Name { get; set; } = "";

      [JsonProperty("isOpen", Required = Required.Always)]
      public bool IsOpen { get; set; } = true;
    }

    public override Guid EventId => Guid.NewGuid();

    public override string EventName => typeof(TransactionPeriodEvent).Name;

    public override string EventDescription => "Transaction Period streaming event";

    public override int EventVersion => 1;

    [JsonProperty("transactionPeriod", Required = Required.Always)]
    public TransactionPeriod Payload { get; set; } = null!;


  }


}