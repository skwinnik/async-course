using EasyNetQ.Consumer;
using Microsoft.EntityFrameworkCore;
using AccountingService.Db;
using AccountingService.Rabbit;
using Common.Events.Business.V1;
using AccountingService.BL;

namespace AccountingService.BackgroundServices {
  public class TaskStatusConsumerBackgroundService : BackgroundService {
    private readonly RabbitContainer rabbitContainer;
    private readonly IDbContextFactory<ServiceDbContext> dbContextFactory;
    private readonly TransactionsBop transactionsBop;

    public TaskStatusConsumerBackgroundService(RabbitContainer rabbitContainer, IDbContextFactory<ServiceDbContext> dbContextFactory, TransactionsBop transactionsBop) {
      this.rabbitContainer = rabbitContainer;
      this.dbContextFactory = dbContextFactory;
      this.transactionsBop = transactionsBop;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
      try {
        var dbContext = await this.dbContextFactory.CreateDbContextAsync(stoppingToken);
        var taskAssignedQueue = await rabbitContainer.Bus.Advanced.QueueDeclareAsync("accounting-service.task.assigned", c => c.AsAutoDelete(false).AsDurable(true));
        var taskCompletedQueue = await rabbitContainer.Bus.Advanced.QueueDeclareAsync("accounting-service.task.completed", c => c.AsAutoDelete(false).AsDurable(true));
        await this.rabbitContainer.Bus.Advanced.BindAsync(this.rabbitContainer.TaskExchange, taskAssignedQueue, "v1.assigned", new Dictionary<string, object>());
        await this.rabbitContainer.Bus.Advanced.BindAsync(this.rabbitContainer.TaskExchange, taskCompletedQueue, "v1.completed", new Dictionary<string, object>());

        rabbitContainer.Bus.Advanced.Consume(c => {
          c
            .ForQueue(taskAssignedQueue, handlers =>
              handlers.Add<string>(this.TaskAssignedV1Handler(dbContext)),
              config => config.WithConsumerTag("accounting-service"));
        });

        rabbitContainer.Bus.Advanced.Consume(c => {
          c
            .ForQueue(taskCompletedQueue, handlers =>
              handlers.Add<string>(this.TaskCompletedV1Handler(dbContext)),
              config => config.WithConsumerTag("accounting-service"));
        });

      }
      catch (Exception e) {
        Console.WriteLine(e.Message);
        throw;
      }
    }

    private IMessageHandler<string> TaskAssignedV1Handler(ServiceDbContext dbContext) {
      return async (message, info, cancellationToken) => {
        if (!Common.Events.SchemaRegistry.Business_V1_TaskAssigned.TryDeserializeValidated(message.Body, out TaskAssigned result)) {
          Console.WriteLine("Unable to parse Task Assigned event");
          Console.WriteLine(message.Body);
          return AckStrategies.NackWithRequeue;
        }

        var task = await dbContext.Tasks.FindAsync(result.TaskId);
        if (task == null) {
          Console.WriteLine("Unack Assigned " + result.TaskId);
          return AckStrategies.NackWithRequeue;
        }

        await dbContext.TaskAssignedLogs.AddAsync(new Db.Models.TaskAssignedLog { TaskId = result.TaskId, UserId = result.UserId, TimeStamp = result.TimeStamp });
        await dbContext.SaveChangesAsync(cancellationToken);

        await this.transactionsBop.CreditUser(result.UserId, task.Fee, result.TaskId);

        Console.WriteLine("Ack Assigned " + result.TaskId);
        return AckStrategies.Ack;
      };
    }

    private IMessageHandler<string> TaskCompletedV1Handler(ServiceDbContext dbContext) {
      return async (message, info, cancellationToken) => {
        if (!Common.Events.SchemaRegistry.Business_V1_TaskCompleted.TryDeserializeValidated(message.Body, out TaskCompleted result)) {
          Console.WriteLine("Unable to parse Task Completed event");
          Console.WriteLine(message.Body);
          return AckStrategies.NackWithRequeue;
        }

        var task = await dbContext.Tasks.FindAsync(result.TaskId);
        if (task == null) {
          Console.WriteLine("Unack Completed " + result.TaskId + ". Reason: Task unknown");
          return AckStrategies.NackWithRequeue;
        }

        var lastAssigned = await dbContext.TaskAssignedLogs
          .Where(l => l.TaskId == result.TaskId)
          .OrderByDescending(l => l.TimeStamp)
          .FirstOrDefaultAsync();
        
        if (lastAssigned == default || lastAssigned.UserId != result.UserId) {
          Console.WriteLine("Unack Completed " + result.TaskId + ". Reason: last assigned mismatch");
          return AckStrategies.NackWithRequeue;
        }

        await this.transactionsBop.DebitUser(result.UserId, task.Reward, result.TaskId);

        Console.WriteLine("Ack Completed " + result.TaskId);
        return AckStrategies.Ack;
      };
    }
  }
}