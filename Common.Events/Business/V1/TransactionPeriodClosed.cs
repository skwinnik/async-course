using Newtonsoft.Json;

namespace Common.Events.Business.V1 {
  public class TransactionPeriodClosedEvent : AbstractEvent {
    public override Guid EventId => Guid.NewGuid();

    public override string EventName => typeof(TransactionPeriodClosedEvent).Name;

    public override string EventDescription => "Transaction period closed event";

    public override int EventVersion => 1;

    [JsonProperty("id", Required = Required.Always)]
    public Guid Id { get; set; } = Guid.Empty;
  }
}