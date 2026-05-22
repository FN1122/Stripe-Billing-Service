using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/credits")]
    public class CreditController : GatewayControllerBase
    {
        private readonly ICreditService _creditService;

        public CreditController(ICreditService creditService)
        {
            _creditService = creditService;
        }

        [HttpGet("customers/{customerId}/balance")]
        public async Task<IActionResult> GetBalance(Guid customerId)
        {
            return ToResponse(await _creditService.GetBalanceAsync(customerId));
        }

        [HttpPost("customers/{customerId}/credit")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> AddCredit(Guid customerId, [FromBody] CreateCreditDto request)
        {
            return ToResponse(await _creditService.AddCreditAsync(customerId, request));
        }

        [HttpPost("customers/{customerId}/adjust")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> AdjustBalance(Guid customerId, [FromBody] AdjustCreditDto request)
        {
            return ToResponse(await _creditService.AdjustBalanceAsync(customerId, request));
        }

        [HttpGet("customers/{customerId}/history")]
        public async Task<IActionResult> GetHistory(Guid customerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            return ToResponse(await _creditService.GetHistoryAsync(customerId, page, pageSize));
        }

        [HttpPost("refund-to-credit")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> RefundToCredit([FromBody] RefundToCreditDto request)
        {
            return ToResponse(await _creditService.RefundToCreditAsync(request));
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            return ToResponse(await _creditService.GetRecentTransactionsAsync(page, pageSize));
        }

        [HttpGet("dashboard")]
        [Authorize(Policy = "ManagerOrAbove")]
        public async Task<IActionResult> GetDashboard()
        {
            return ToResponse(await _creditService.GetDashboardAsync());
        }
    }
}
