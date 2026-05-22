using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/dunning")]
    public class DunningController : GatewayControllerBase
    {
        private readonly IDunningService _dunningService;

        public DunningController(IDunningService dunningService)
        {
            _dunningService = dunningService;
        }

        [HttpGet("config")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> GetConfig()
        {
            return ToResponse(await _dunningService.GetConfigAsync());
        }

        [HttpPut("config")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> UpdateConfig([FromBody] DunningConfigDto request)
        {
            return ToResponse(await _dunningService.UpdateConfigAsync(request));
        }

        [HttpGet("schedules")]
        public async Task<IActionResult> GetSchedules([FromQuery] DunningFilterDto filter)
        {
            return ToResponse(await _dunningService.GetSchedulesAsync(filter));
        }

        [HttpGet("schedules/{id}")]
        public async Task<IActionResult> GetSchedule(Guid id)
        {
            return ToResponse(await _dunningService.GetScheduleAsync(id));
        }

        [HttpPost("schedules/{id}/pause")]
        public async Task<IActionResult> PauseSchedule(Guid id)
        {
            return ToResponse(await _dunningService.PauseScheduleAsync(id));
        }

        [HttpPost("schedules/{id}/resume")]
        public async Task<IActionResult> ResumeSchedule(Guid id)
        {
            return ToResponse(await _dunningService.ResumeScheduleAsync(id));
        }

        [HttpPost("schedules/{id}/cancel")]
        public async Task<IActionResult> CancelSchedule(Guid id)
        {
            return ToResponse(await _dunningService.CancelScheduleAsync(id));
        }

        [HttpPost("schedules/{id}/retry")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> ManualRetry(Guid id)
        {
            return ToResponse(await _dunningService.ManualRetryAsync(id));
        }

        [HttpGet("dashboard")]
        [Authorize(Policy = "ManagerOrAbove")]
        public async Task<IActionResult> GetDashboard()
        {
            return ToResponse(await _dunningService.GetDashboardAsync());
        }
    }
}
