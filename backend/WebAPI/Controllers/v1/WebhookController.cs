using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/webhooks")]
    [Authorize]
    public class WebhookController : GatewayControllerBase
    {
        private readonly IWebhookSubscriptionService _webhookService;
        private readonly IWebhookDispatchService _dispatchService;

        public WebhookController(IWebhookSubscriptionService webhookService, IWebhookDispatchService dispatchService)
        {
            _webhookService = webhookService;
            _dispatchService = dispatchService;
        }

        [HttpGet("subscriptions")]
        public async Task<IActionResult> ListSubscriptions([FromQuery] WebhookSubscriptionFilterDto filter)
        {
            return ToResponse(await _webhookService.ListAsync(filter));
        }

        [HttpPost("subscriptions")]
        public async Task<IActionResult> CreateSubscription([FromBody] CreateWebhookSubscriptionDto request)
        {
            return ToResponse(await _webhookService.CreateAsync(request));
        }

        [HttpGet("subscriptions/{id}")]
        public async Task<IActionResult> GetSubscription(Guid id)
        {
            return ToResponse(await _webhookService.GetAsync(id));
        }

        [HttpPut("subscriptions/{id}")]
        public async Task<IActionResult> UpdateSubscription(Guid id, [FromBody] UpdateWebhookSubscriptionDto request)
        {
            return ToResponse(await _webhookService.UpdateAsync(id, request));
        }

        [HttpDelete("subscriptions/{id}")]
        public async Task<IActionResult> DeleteSubscription(Guid id)
        {
            return ToResponse(await _webhookService.DeleteAsync(id));
        }

        [HttpPost("subscriptions/{id}/test")]
        public async Task<IActionResult> TestSubscription(Guid id)
        {
            return ToResponse(await _webhookService.TestAsync(id));
        }

        [HttpGet("subscriptions/{id}/stats")]
        public async Task<IActionResult> GetSubscriptionStats(Guid id)
        {
            return ToResponse(await _webhookService.GetStatsAsync(id));
        }

        [HttpPost("subscriptions/{id}/rotate-secret")]
        public async Task<IActionResult> RotateSecret(Guid id)
        {
            return ToResponse(await _webhookService.RotateSecretAsync(id));
        }

        [HttpGet("subscriptions/{subscriptionId}/deliveries")]
        public async Task<IActionResult> ListDeliveries(Guid subscriptionId, [FromQuery] WebhookDeliveryFilterDto filter)
        {
            return ToResponse(await _dispatchService.ListDeliveriesAsync(subscriptionId, filter));
        }

        [HttpGet("deliveries/{id}")]
        public async Task<IActionResult> GetDelivery(Guid id)
        {
            return ToResponse(await _dispatchService.GetDeliveryAsync(id));
        }

        [HttpPost("deliveries/{id}/retry")]
        public async Task<IActionResult> RetryDelivery(Guid id)
        {
            return ToResponse(await _dispatchService.RetryDeliveryAsync(id));
        }
    }
}
