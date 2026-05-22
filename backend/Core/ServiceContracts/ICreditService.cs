using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface ICreditService
    {
        Task<GatewayResponseWrapper<CustomerBalanceDto>> GetBalanceAsync(Guid customerId);
        Task<GatewayResponseWrapper<CreditResponseDto>> AddCreditAsync(Guid customerId, CreateCreditDto request);
        Task<GatewayResponseWrapper<CreditResponseDto>> AdjustBalanceAsync(Guid customerId, AdjustCreditDto request);
        Task<GatewayPaginatedListResponseWrapper<CreditResponseDto>> GetHistoryAsync(Guid customerId, int page, int pageSize);
        Task<GatewayResponseWrapper<CreditResponseDto>> RefundToCreditAsync(RefundToCreditDto request);
        Task<GatewayResponseWrapper<CreditsDashboardDto>> GetDashboardAsync();
        Task<GatewayPaginatedListResponseWrapper<CreditResponseDto>> GetRecentTransactionsAsync(int page, int pageSize);
    }
}
