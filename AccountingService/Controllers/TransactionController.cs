using AccountingService.BL;
using AccountingService.Db;
using AccountingService.Rabbit;
using Common.Auth;
using Common.Events;
using EasyNetQ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DayController.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class TransactionController : ControllerBase {
    private readonly RabbitContainer rabbitContainer;
    private readonly UserContext userContext;
    private readonly TransactionBop transactionBop;
    public TransactionController(RabbitContainer rabbitContainer, UserContext userContext, TransactionBop transactionBop) {
      this.rabbitContainer = rabbitContainer;
      this.userContext = userContext;
      this.transactionBop = transactionBop;
    }

    [HttpGet]
    [Route("[action]")]
    [Authorize("user")]
    public async Task<ActionResult> Get() {
      var userId = this.userContext.GetCurrentUserId();
      if (userId == null)
        return this.Unauthorized();

      return this.Ok(await this.transactionBop.GetTransactions(userId.Value));
    }
  }
}