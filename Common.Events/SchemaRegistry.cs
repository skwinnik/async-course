using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Common.Events {
  public static class SchemaRegistry {
    public static Schema<Business.V1.TaskAssigned> Business_V1_TaskAssigned => new Schema<Business.V1.TaskAssigned>("./Business/V1/TaskAssigned.json");
    public static Schema<Business.V1.TaskCompleted> Business_V1_TaskCompleted => new Schema<Business.V1.TaskCompleted>("./Business/V1/TaskCompleted.json");
    public static Schema<Business.V1.TransactionPeriodClosedEvent> Business_V1_DayCompleted => new Schema<Business.V1.TransactionPeriodClosedEvent>("./Business/V1/TransactionPeriodClosedEvent.json");
    public static Schema<Streaming.V1.UserEvent> Streaming_V1_User => new Schema<Streaming.V1.UserEvent>("./Streaming/V1/UserEvent.json");
    public static Schema<Streaming.V1.TaskEvent> Streaming_V1_Task => new Schema<Streaming.V1.TaskEvent>("./Streaming/V1/TaskEvent.json");
    public static Schema<Streaming.V2.TaskEvent> Streaming_V2_Task => new Schema<Streaming.V2.TaskEvent>("./Streaming/V2/TaskEvent.json");
    public static Schema<Streaming.V3.TaskEvent> Streaming_V3_Task => new Schema<Streaming.V3.TaskEvent>("./Streaming/V3/TaskEvent.json");
    public static Schema<Streaming.V1.TransactionEvent> Streaming_V1_Transaction => new Schema<Streaming.V1.TransactionEvent>("./Streaming/V1/TransactionEvent.json");

  }

  public class Schema<T> where T : class {
    private JSchema schema;
    public Schema(string schemaPath) {
      this.schema = JSchema.Parse(File.ReadAllText(schemaPath));
    }

    public bool TryDeserializeValidated(string json, out T result) {
      var reader = new JSchemaValidatingReader(new JsonTextReader(new StringReader(json)));
      reader.Schema = schema;
      try {
        result = new JsonSerializer().Deserialize<T>(reader)!;
      }
      catch (Exception e) {
        Console.WriteLine(e);
        result = null!;
        return false;
      }

      if (result == null) {
        return false;
      }

      return true;
    }

    public bool TrySerializeValidated(T obj, out string result) {
      using var stringWriter = new StringWriter();
      var writer = new JSchemaValidatingWriter(new JsonTextWriter(stringWriter));
      writer.Schema = schema;
      new JsonSerializer().Serialize(writer, obj);

      result = stringWriter.ToString();
      if (string.IsNullOrEmpty(result)) {
        return false;
      }

      return true;
    }
  }
}