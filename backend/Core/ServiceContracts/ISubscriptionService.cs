using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface ISubscriptionService
    {
        Task<GatewayResponseWrapper<SubscriptionResponseDto>> CreateAsync(CreateSubscriptionDto request);
        Task<GatewayResponseWrapper<SubscriptionResponseDto>> GetAsync(Guid id);
        Task<GatewayPaginatedListResponseWrapper<SubscriptionResponseDto>> ListAsync(SubscriptionFilterDto filter);
        Task<GatewayResponseWrapper<SubscriptionResponseDto>> UpdateAsync(Guid id, UpdateSubscriptionDto request);
        Task<GatewayResponseWrapper<bool>> CancelAsync(Guid id, CancelSubscriptionDto request);
        Task<GatewayResponseWrapper<bool>> PauseAsync(Guid id);
        Task<GatewayResponseWrapper<bool>> ResumeAsync(Guid id);
        Task<GatewayResponseWrapper<ProrationPreviewDto>> PreviewProrationAsync(Guid id, Guid newPlanId);
    }
}
