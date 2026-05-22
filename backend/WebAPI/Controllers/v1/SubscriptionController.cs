using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/subscriptions")]
    public class SubscriptionController : GatewayControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSubscriptionDto request)
        {
            return ToResponse(await _subscriptionService.CreateAsync(request));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return ToResponse(await _subscriptionService.GetAsync(id));
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] SubscriptionFilterDto filter)
        {
            return ToResponse(await _subscriptionService.ListAsync(filter));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubscriptionDto request)
        {
            return ToResponse(await _subscriptionService.UpdateAsync(id, request));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelSubscriptionDto request)
        {
            return ToResponse(await _subscriptionService.CancelAsync(id, request));
        }

        [HttpPost("{id}/pause")]
        public async Task<IActionResult> Pause(Guid id)
        {
            return ToResponse(await _subscriptionService.PauseAsync(id));
        }

        [HttpPost("{id}/resume")]
        public async Task<IActionResult> Resume(Guid id)
        {
            return ToResponse(await _subscriptionService.ResumeAsync(id));
        }

        [HttpGet("{id}/preview")]
        public async Task<IActionResult> PreviewProration(Guid id, [FromQuery] Guid newPlanId)
        {
            return ToResponse(await _subscriptionService.PreviewProrationAsync(id, newPlanId));
        }
    }
}
