using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/webhook-events")]
    public class WebhookEventController : GatewayControllerBase
    {
        private readonly IWebhookEventLogService _webhookEventService;

        public WebhookEventController(IWebhookEventLogService webhookEventService)
        {
            _webhookEventService = webhookEventService;
        }

        [HttpGet("inbound")]
        public async Task<IActionResult> GetInboundEvents([FromQuery] string? eventType, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            return ToResponse(await _webhookEventService.GetInboundEventsAsync(eventType, status, page, pageSize));
        }

        [HttpGet("inbound/{id}")]
        public async Task<IActionResult> GetInboundEvent(Guid id)
        {
            return ToResponse(await _webhookEventService.GetInboundEventAsync(id));
        }

        [HttpPost("inbound/{id}/replay")]
        public async Task<IActionResult> ReplayEvent(Guid id)
        {
            return ToResponse(await _webhookEventService.ReplayEventAsync(id));
        }

        [HttpGet("deliveries")]
        public async Task<IActionResult> GetDeliveries([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            return ToResponse(await _webhookEventService.GetDeliveryLogAsync(status, page, pageSize));
        }

        [HttpGet("deliveries/{id}")]
        public async Task<IActionResult> GetDeliveryDetail(Guid id)
        {
            return ToResponse(await _webhookEventService.GetDeliveryDetailAsync(id));
        }

        [HttpPost("deliveries/{id}/retry")]
        public async Task<IActionResult> RetryDelivery(Guid id)
        {
            return ToResponse(await _webhookEventService.RetryDeliveryAsync(id));
        }

        [HttpGet("stats")]
        [Authorize(Policy = "ManagerOrAbove")]
        public async Task<IActionResult> GetStats()
        {
            return ToResponse(await _webhookEventService.GetEventStatsAsync());
        }
    }
}
