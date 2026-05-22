using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface IWebhookSubscriptionService
    {
        Task<GatewayResponseWrapper<WebhookSubscriptionResponseDto>> CreateAsync(CreateWebhookSubscriptionDto request);
        Task<GatewayResponseWrapper<WebhookSubscriptionResponseDto>> GetAsync(Guid id);
        Task<GatewayPaginatedListResponseWrapper<WebhookSubscriptionResponseDto>> ListAsync(WebhookSubscriptionFilterDto filter);
        Task<GatewayResponseWrapper<WebhookSubscriptionResponseDto>> UpdateAsync(Guid id, UpdateWebhookSubscriptionDto request);
        Task<GatewayResponseWrapper<bool>> DeleteAsync(Guid id);
        Task<GatewayResponseWrapper<bool>> DisableAsync(Guid id);
        Task<GatewayResponseWrapper<bool>> EnableAsync(Guid id);
        Task<GatewayResponseWrapper<bool>> TestAsync(Guid id);
        Task<GatewayResponseWrapper<WebhookSubscriptionStatsDto>> GetStatsAsync(Guid id);
        Task<GatewayResponseWrapper<string>> RotateSecretAsync(Guid id);
    }
}
