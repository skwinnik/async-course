namespace TaskService {
  public class Program {
    public static void Main(string[] args) =>
      Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(x => x
            .UseKestrel()
            .UseStartup<Startup>())
        .Build()
        .Run();
  }
}
