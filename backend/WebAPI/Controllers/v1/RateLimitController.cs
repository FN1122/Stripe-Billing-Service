using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/rate-limits")]
    public class RateLimitController : GatewayControllerBase
    {
        private readonly IRateLimitService _rateLimitService;

        public RateLimitController(IRateLimitService rateLimitService)
        {
            _rateLimitService = rateLimitService;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            return ToResponse(await _rateLimitService.ListAsync());
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> Create([FromBody] CreateRateLimitDto request)
        {
            return ToResponse(await _rateLimitService.CreateAsync(request));
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRateLimitDto request)
        {
            return ToResponse(await _rateLimitService.UpdateAsync(id, request));
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return ToResponse(await _rateLimitService.DeleteAsync(id));
        }

        [HttpGet("usage")]
        public async Task<IActionResult> GetUsage()
        {
            return ToResponse(await _rateLimitService.GetUsageAsync());
        }
    }
}
