using AccountingService.BL;
using AccountingService.Db;
using AccountingService.Rabbit;
using Common.Events;
using EasyNetQ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DayController.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class TransactionPeriodController : ControllerBase {
    private readonly RabbitContainer rabbitContainer;
    private readonly TransactionPeriodBop transactionPeriodBop;
    public TransactionPeriodController(RabbitContainer rabbitContainer, TransactionPeriodBop transactionPeriodBop) {
      this.rabbitContainer = rabbitContainer;
      this.transactionPeriodBop = transactionPeriodBop;
    }

    [HttpGet]
    [Route("[action]")]
    public async Task<ActionResult> Close() {
      var guid = await this.transactionPeriodBop.ClosePeriod();
      if (SchemaRegistry.Business_V1_DayCompleted.TrySerializeValidated(new Common.Events.Business.V1.TransactionPeriodClosedEvent {
        Id = guid
      }, out var jsonDayCompleted)) {
        await this.rabbitContainer.Bus.Advanced.PublishAsync(this.rabbitContainer.TransactionPeriodExchange, "v1.closed", false, new Message<string>(jsonDayCompleted));
      }
      return this.Ok();
    }
  }
}