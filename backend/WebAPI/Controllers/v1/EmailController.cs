using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/emails")]
    public class EmailController : GatewayControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates()
        {
            return ToResponse(await _emailService.GetTemplatesAsync());
        }

        [HttpPost("templates")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> CreateTemplate([FromBody] CreateEmailTemplateDto request)
        {
            return ToResponse(await _emailService.CreateTemplateAsync(request));
        }

        [HttpGet("templates/{key}")]
        public async Task<IActionResult> GetTemplate(string key)
        {
            return ToResponse(await _emailService.GetTemplateAsync(key));
        }

        [HttpPut("templates/{key}")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> UpdateTemplate(string key, [FromBody] UpdateEmailTemplateDto request)
        {
            return ToResponse(await _emailService.UpdateTemplateAsync(key, request));
        }

        [HttpPost("templates/{key}/reset")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> ResetTemplate(string key)
        {
            return ToResponse(await _emailService.ResetTemplateAsync(key));
        }

        [HttpPost("templates/{key}/preview")]
        public async Task<IActionResult> PreviewTemplate(string key, [FromBody] PreviewEmailTemplateDto request)
        {
            return ToResponse(await _emailService.PreviewTemplateAsync(key, request.Variables));
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs([FromQuery] EmailLogFilterDto filter)
        {
            return ToResponse(await _emailService.GetEmailLogsAsync(filter));
        }

        [HttpPost("logs/{id}/resend")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> ResendEmail(Guid id)
        {
            return ToResponse(await _emailService.ResendEmailAsync(id));
        }

        [HttpPost("send")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailDto request)
        {
            return ToResponse(await _emailService.SendAsync(request));
        }

        [HttpGet("stats")]
        [Authorize(Policy = "ManagerOrAbove")]
        public async Task<IActionResult> GetStats()
        {
            return ToResponse(await _emailService.GetStatsAsync());
        }
    }
}
