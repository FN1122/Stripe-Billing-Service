using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/usage")]
    public class UsageBillingController : GatewayControllerBase
    {
        private readonly IUsageBillingService _usageService;

        public UsageBillingController(IUsageBillingService usageService)
        {
            _usageService = usageService;
        }

        [HttpPost("report")]
        public async Task<IActionResult> ReportUsage([FromBody] CreateUsageRecordDto request)
        {
            return ToResponse(await _usageService.ReportUsageAsync(request));
        }

        [HttpPost("report/batch")]
        public async Task<IActionResult> BatchReportUsage([FromBody] BatchUsageRecordDto request)
        {
            return ToResponse(await _usageService.BatchReportUsageAsync(request));
        }

        [HttpGet]
        public async Task<IActionResult> GetUsage([FromQuery] UsageFilterDto filter)
        {
            return ToResponse(await _usageService.GetUsageAsync(filter));
        }

        [HttpGet("summary/{subscriptionId}")]
        public async Task<IActionResult> GetUsageSummary(Guid subscriptionId)
        {
            return ToResponse(await _usageService.GetUsageSummaryAsync(subscriptionId));
        }

        [HttpPost("meter-events")]
        public async Task<IActionResult> CreateMeterEvent([FromBody] CreateMeterEventDto request)
        {
            return ToResponse(await _usageService.CreateMeterEventAsync(request));
        }

        [HttpGet("meter-events")]
        public async Task<IActionResult> GetMeterEvents([FromQuery] Guid? customerId, [FromQuery] string? eventName, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            return ToResponse(await _usageService.GetMeterEventsAsync(customerId, eventName, page, pageSize));
        }

        [HttpGet("dashboard")]
        [Authorize(Policy = "ManagerOrAbove")]
        public async Task<IActionResult> GetDashboard()
        {
            return ToResponse(await _usageService.GetUsageDashboardAsync());
        }
    }
}
