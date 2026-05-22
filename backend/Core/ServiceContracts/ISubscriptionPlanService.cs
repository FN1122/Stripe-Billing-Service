using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface ISubscriptionPlanService
    {
        Task<GatewayResponseWrapper<SubscriptionPlanResponseDto>> CreateAsync(CreatePlanDto request);
        Task<GatewayResponseWrapper<SubscriptionPlanResponseDto>> GetAsync(Guid id);
        Task<GatewayResponseWrapper<List<SubscriptionPlanResponseDto>>> ListAsync();
        Task<GatewayResponseWrapper<SubscriptionPlanResponseDto>> UpdateAsync(Guid id, UpdatePlanDto request);
        Task<GatewayResponseWrapper<bool>> DeleteAsync(Guid id);
        Task<GatewayResponseWrapper<bool>> SyncFromStripeAsync();
        Task<GatewayResponseWrapper<bool>> ToggleActiveAsync(Guid id);
    }
}
