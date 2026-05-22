using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface IWebhookDispatchService
    {
        Task EnqueueAsync(Guid tenantId, string eventType, object data);
        Task<List<WebhookDelivery>> GetPendingDeliveriesAsync(int maxCount = 100);
        Task<GatewayResponseWrapper<bool>> MarkAsDeliveredAsync(Guid deliveryId);
        Task<GatewayResponseWrapper<bool>> MarkAsFailedAsync(Guid deliveryId, string errorMessage);
        Task<GatewayPaginatedListResponseWrapper<WebhookDeliveryResponseDto>> ListDeliveriesAsync(Guid subscriptionId, WebhookDeliveryFilterDto filter);
        Task<GatewayResponseWrapper<WebhookDeliveryDetailResponseDto>> GetDeliveryAsync(Guid deliveryId);
        Task<GatewayResponseWrapper<WebhookDeliveryStatsDto>> GetDeliveryStatsAsync(Guid tenantId);
        Task<GatewayResponseWrapper<bool>> RetryDeliveryAsync(Guid deliveryId);
    }
}
