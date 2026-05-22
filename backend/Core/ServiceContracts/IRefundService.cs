using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface IRefundService
    {
        Task<GatewayResponseWrapper<RefundResponseDto>> CreateAsync(CreateRefundDto request);
        Task<GatewayResponseWrapper<RefundResponseDto>> GetAsync(Guid id);
        Task<GatewayPaginatedListResponseWrapper<RefundResponseDto>> ListAsync(RefundFilterDto filter);
        Task<GatewayResponseWrapper<RefundResponseDto>> ApproveAsync(Guid id, Guid approvedByUserId);
        Task<GatewayResponseWrapper<bool>> RejectAsync(Guid id, string reason);
        Task<GatewayResponseWrapper<RefundStatsDto>> GetStatsAsync();
    }
}
