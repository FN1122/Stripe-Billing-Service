using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/plans")]
    [Authorize(Policy = "AdminOrAbove")]
    public class PlanController : GatewayControllerBase
    {
        private readonly ISubscriptionPlanService _planService;

        public PlanController(ISubscriptionPlanService planService)
        {
            _planService = planService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> List()
        {
            return ToResponse(await _planService.ListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePlanDto request)
        {
            return ToResponse(await _planService.CreateAsync(request));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(Guid id)
        {
            return ToResponse(await _planService.GetAsync(id));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePlanDto request)
        {
            return ToResponse(await _planService.UpdateAsync(id, request));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return ToResponse(await _planService.DeleteAsync(id));
        }

        [HttpPost("sync")]
        public async Task<IActionResult> SyncFromStripe()
        {
            return ToResponse(await _planService.SyncFromStripeAsync());
        }
    }
}
