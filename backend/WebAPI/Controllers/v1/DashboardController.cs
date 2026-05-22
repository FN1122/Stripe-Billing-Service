using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/dashboard")]
    [Authorize]
    public class DashboardController : GatewayControllerBase
    {
        private readonly IRevenueAnalyticsService _analyticsService;

        public DashboardController(IRevenueAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            return ToResponse(await _analyticsService.GetDashboardStatsAsync());
        }

        [HttpGet("activity")]
        public async Task<IActionResult> GetActivity([FromQuery] int count = 20)
        {
            return ToResponse(await _analyticsService.GetActivityFeedAsync(count));
        }

        [HttpGet("activity-feed")]
        public async Task<IActionResult> GetActivityFeed([FromQuery] int limit = 20)
        {
            return ToResponse(await _analyticsService.GetActivityFeedAsync(limit));
        }

        [HttpGet("revenue-chart")]
        public async Task<IActionResult> GetRevenueChart([FromQuery] int days = 30)
        {
            var period = days switch
            {
                7 => "7d",
                90 => "90d",
                365 => "12m",
                _ => "30d"
            };
            return ToResponse(await _analyticsService.GetRevenueMetricsAsync(period));
        }

        [HttpGet("recent-transactions")]
        public async Task<IActionResult> GetRecentTransactions([FromQuery] int limit = 5)
        {
            return ToResponse(await _analyticsService.GetActivityFeedAsync(limit));
        }
    }
}
