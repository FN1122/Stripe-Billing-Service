using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/settings")]
    [Authorize(Policy = "AdminOrAbove")]
    public class SettingsController : GatewayControllerBase
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return ToResponse(await _settingsService.GetAllAsync());
        }

        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            return ToResponse(await _settingsService.GetAsync(key));
        }

        [HttpPut("{key}")]
        public async Task<IActionResult> Update(string key, [FromBody] UpdateSettingDto request)
        {
            return ToResponse(await _settingsService.UpdateAsync(key, request));
        }

        [HttpGet("billing")]
        public async Task<IActionResult> GetBillingSettings()
        {
            return ToResponse(await _settingsService.GetBillingSettingsAsync());
        }

        [HttpPut("billing")]
        public async Task<IActionResult> UpdateBillingSettings([FromBody] UpdateBillingSettingsDto request)
        {
            return ToResponse(await _settingsService.UpdateBillingSettingsAsync(request));
        }

        [HttpGet("security")]
        public async Task<IActionResult> GetSecuritySettings()
        {
            return ToResponse(await _settingsService.GetSecuritySettingsAsync());
        }

        [HttpPut("security")]
        public async Task<IActionResult> UpdateSecuritySettings([FromBody] UpdateSecuritySettingsDto request)
        {
            return ToResponse(await _settingsService.UpdateSecuritySettingsAsync(request));
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotificationSettings()
        {
            return ToResponse(await _settingsService.GetNotificationSettingsAsync());
        }

        [HttpPut("notifications")]
        public async Task<IActionResult> UpdateNotificationSettings([FromBody] UpdateNotificationSettingsDto request)
        {
            return ToResponse(await _settingsService.UpdateNotificationSettingsAsync(request));
        }
    }
}
