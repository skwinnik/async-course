using Common.Events;
using Newtonsoft.Json.Schema.Generation;

internal class Program {
  private static void Main(string[] args) {
    JSchemaGenerator gen = new JSchemaGenerator();

    var eventTypes = typeof(AbstractEvent).Assembly.GetTypes().Where(t => t.BaseType != null && t.BaseType.Name.Contains("AbstractEvent")).ToList();

    foreach (var eventType in eventTypes) {
      var schema = gen.Generate(eventType);
      var eventNameType = "Business";
      var eventVersion = "V1";

      if (!string.IsNullOrEmpty(eventType.Namespace) && eventType.Namespace.Contains("Streaming"))
        eventNameType = "Streaming";

      if (!string.IsNullOrEmpty(eventType.Namespace) && eventType.Namespace.Contains("V2"))
        eventVersion = "V2";

      File.WriteAllTextAsync($"../Common.Events.Schemas/{eventNameType}/{eventVersion}/{eventType.Name}.json", schema.ToString());
    }
  }
}