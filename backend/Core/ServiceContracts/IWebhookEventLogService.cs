using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface IWebhookEventLogService
    {
        Task<GatewayPaginatedListResponseWrapper<WebhookEventResponseDto>> GetInboundEventsAsync(string? eventType, string? status, int page, int pageSize);
        Task<GatewayResponseWrapper<WebhookEventDetailDto>> GetInboundEventAsync(Guid id);
        Task<GatewayResponseWrapper<bool>> ReplayEventAsync(Guid id);
        Task<GatewayPaginatedListResponseWrapper<WebhookDeliveryResponseDto>> GetDeliveryLogAsync(string? status, int page, int pageSize);
        Task<GatewayResponseWrapper<WebhookDeliveryDetailDto>> GetDeliveryDetailAsync(Guid deliveryId);
        Task<GatewayResponseWrapper<bool>> RetryDeliveryAsync(Guid deliveryId);
        Task<GatewayResponseWrapper<WebhookEventStatsDto>> GetEventStatsAsync();
    }
}
