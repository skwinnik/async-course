using AnalyticsService.BL;
using Microsoft.AspNetCore.Mvc;
using Common.Auth;

namespace AnalyticsService.Controllers {

  [ApiController]
  [Route("[controller]/[action]")]
  public class ReportController : ControllerBase {
    private readonly ReportBop reportBop;

    public ReportController(ReportBop reportBop) {
      this.reportBop = reportBop;
    }

    [HttpGet]
    [Authorize("admin")]
    public Task<MostExpensiveTaskReport> GetMostExpensiveTaskReport([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        => this.reportBop.GetMostExpensiveTaskReport(fromDate, toDate);

    [HttpGet]
    [Authorize("admin")]
    public Task<ManagementEarnedReport> GetManagementEarnedReport([FromQuery] Guid transactionPeriodId)
        => this.reportBop.GetManagementEarnedReport(transactionPeriodId);
  }
}