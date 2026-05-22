using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface IWebhookDeliveryRepository
    {
        Task<WebhookDelivery> GetByIdAsync(Guid id);
        Task<List<WebhookDelivery>> GetBySubscriptionIdAsync(Guid subscriptionId);
        Task<List<WebhookDelivery>> GetPendingAsync(int maxCount = 100);
        Task<List<WebhookDelivery>> GetRetryableAsync();
        Task<Guid> CreateAsync(WebhookDelivery delivery);
        Task CreateRangeAsync(List<WebhookDelivery> deliveries);
        Task UpdateAsync(WebhookDelivery delivery);
        IQueryable<WebhookDelivery> Query();
        IQueryable<WebhookDelivery> QueryByTenant(Guid tenantId);
    }
}
