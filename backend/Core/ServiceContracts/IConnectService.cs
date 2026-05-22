using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface IConnectService
    {
        Task<GatewayResponseWrapper<ConnectedAccountResponseDto>> CreateAccountAsync(CreateConnectedAccountDto request);
        Task<GatewayResponseWrapper<List<ConnectedAccountResponseDto>>> GetAccountsAsync();
        Task<GatewayResponseWrapper<ConnectedAccountResponseDto>> GetAccountAsync(Guid id);
        Task<GatewayResponseWrapper<string>> GetOnboardingLinkAsync(Guid id);
        Task<GatewayResponseWrapper<string>> GetDashboardLinkAsync(Guid id);
        Task<GatewayResponseWrapper<TransferResponseDto>> CreateTransferAsync(CreateTransferDto request);
        Task<GatewayResponseWrapper<List<TransferResponseDto>>> GetTransfersAsync();
        Task<GatewayResponseWrapper<PlatformBalanceDto>> GetBalanceAsync();
    }
}
