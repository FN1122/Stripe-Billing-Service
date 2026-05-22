using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/connect")]
    public class ConnectController : GatewayControllerBase
    {
        private readonly IConnectService _connectService;

        public ConnectController(IConnectService connectService)
        {
            _connectService = connectService;
        }

        [HttpPost("accounts")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateConnectedAccountDto request)
        {
            return ToResponse(await _connectService.CreateAccountAsync(request));
        }

        [HttpGet("accounts")]
        public async Task<IActionResult> GetAccounts()
        {
            return ToResponse(await _connectService.GetAccountsAsync());
        }

        [HttpGet("accounts/{id}")]
        public async Task<IActionResult> GetAccount(Guid id)
        {
            return ToResponse(await _connectService.GetAccountAsync(id));
        }

        [HttpPost("accounts/{id}/onboarding-link")]
        public async Task<IActionResult> GetOnboardingLink(Guid id)
        {
            return ToResponse(await _connectService.GetOnboardingLinkAsync(id));
        }

        [HttpPost("accounts/{id}/dashboard-link")]
        public async Task<IActionResult> GetDashboardLink(Guid id)
        {
            return ToResponse(await _connectService.GetDashboardLinkAsync(id));
        }

        [HttpPost("transfers")]
        public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferDto request)
        {
            return ToResponse(await _connectService.CreateTransferAsync(request));
        }

        [HttpGet("transfers")]
        public async Task<IActionResult> GetTransfers()
        {
            return ToResponse(await _connectService.GetTransfersAsync());
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            return ToResponse(await _connectService.GetBalanceAsync());
        }
    }
}
