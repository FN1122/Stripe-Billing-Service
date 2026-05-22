using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/analytics")]
    [Authorize(Policy = "ManagerOrAbove")]
    public class AnalyticsController : GatewayControllerBase
    {
        private readonly IRevenueAnalyticsService _analyticsService;

        public AnalyticsController(IRevenueAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("mrr")]
        public async Task<IActionResult> GetMrr()
        {
            return ToResponse(await _analyticsService.GetMrrAsync());
        }

        [HttpGet("churn")]
        public async Task<IActionResult> GetChurn([FromQuery] string period = "30d")
        {
            return ToResponse(await _analyticsService.GetChurnRateAsync(period));
        }

        [HttpGet("ltv")]
        public async Task<IActionResult> GetLtv()
        {
            return ToResponse(await _analyticsService.GetLtvAsync());
        }

        [HttpGet("subscription-metrics")]
        public async Task<IActionResult> GetSubscriptionMetrics()
        {
            return ToResponse(await _analyticsService.GetDashboardStatsAsync());
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue([FromQuery] string period = "30d")
        {
            return ToResponse(await _analyticsService.GetRevenueMetricsAsync(period));
        }
    }
}
