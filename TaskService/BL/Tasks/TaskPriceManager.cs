
namespace TaskService.BL.Tasks {
  public class TaskPriceManager {
    public Task<decimal> GetFee() {
      return Task.FromResult(new Random().Next(1000, 2000) / 100m);
    }

    public Task<decimal> GetReward() {
      return Task.FromResult(new Random().Next(2000, 4000) / 100m);
    }
  }
}