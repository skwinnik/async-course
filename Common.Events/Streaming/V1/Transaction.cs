using Newtonsoft.Json;

namespace Common.Events.Streaming.V1 {

  public class TransactionEvent : AbstractEvent {

    public enum TransactionType {
      Task, Salary, Move
    }
    public class Transaction {
      [JsonProperty("id", Required = Required.Always)]
      public Guid Id { get; set; } = Guid.Empty;

      [JsonProperty("userId", Required = Required.Always)]
      public Guid UserId { get; set; }

      [JsonProperty("transactionPeriodId", Required = Required.Always)]
      public Guid TransactionPeriodId { get; set; }

      [JsonProperty("description", Required = Required.Always)]
      public string Description { get; set; } = "";

      [JsonProperty("debit", Required = Required.Always)]
      public decimal Debit { get; set; }

      [JsonProperty("credit", Required = Required.Always)]
      public decimal Credit { get; set; }

      [JsonProperty("transactionType", Required = Required.Always)]
      public TransactionType TransactionType { get; set; }

      [JsonProperty("timestamp", Required = Required.Always)]
      public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    }

    public override Guid EventId => Guid.NewGuid();

    public override string EventName => typeof(TaskEvent).Name;

    public override string EventDescription => "Task streaming event";

    public override int EventVersion => 1;

    [JsonProperty("transaction", Required = Required.Always)]
    public Transaction Payload { get; set; } = null!;


  }


}