using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/refunds")]
    public class RefundController : GatewayControllerBase
    {
        private readonly IRefundService _refundService;

        public RefundController(IRefundService refundService)
        {
            _refundService = refundService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRefundDto request)
        {
            return ToResponse(await _refundService.CreateAsync(request));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return ToResponse(await _refundService.GetAsync(id));
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] RefundFilterDto filter)
        {
            return ToResponse(await _refundService.ListAsync(filter));
        }

        [HttpPost("{id}/approve")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> Approve(Guid id)
        {
            return ToResponse(await _refundService.ApproveAsync(id, GetUserId()));
        }

        [HttpPost("{id}/reject")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] string reason)
        {
            return ToResponse(await _refundService.RejectAsync(id, reason));
        }

        [HttpGet("stats")]
        [Authorize(Policy = "ManagerOrAbove")]
        public async Task<IActionResult> GetStats()
        {
            return ToResponse(await _refundService.GetStatsAsync());
        }
    }
}
