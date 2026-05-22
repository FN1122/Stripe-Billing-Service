using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/logs")]
    [Authorize]
    public class LogController : GatewayControllerBase
    {
        private readonly IApiCallLogService _logService;

        public LogController(IApiCallLogService logService)
        {
            _logService = logService;
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] ApiCallLogFilterDto filter)
        {
            return ToResponse(await _logService.ListAsync(filter));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return ToResponse(await _logService.GetAsync(id));
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromQuery] string period = "24h")
        {
            return ToResponse(await _logService.GetStatsAsync(period));
        }

        [HttpGet("usage")]
        public async Task<IActionResult> GetUsageMetrics()
        {
            return ToResponse(await _logService.GetUsageMetricsAsync());
        }
    }
}
