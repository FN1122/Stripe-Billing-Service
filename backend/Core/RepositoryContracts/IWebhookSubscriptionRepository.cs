using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface IWebhookSubscriptionRepository
    {
        Task<WebhookSubscription> GetByIdAsync(Guid id);
        Task<WebhookSubscription> GetByIdAndTenantAsync(Guid tenantId, Guid id);
        Task<WebhookSubscription> GetByIdWithDeliveriesAsync(Guid tenantId, Guid id);
        Task<List<WebhookSubscription>> GetByTenantIdAsync(Guid tenantId);
        Task<List<WebhookSubscription>> GetActiveByTenantAndEventAsync(Guid tenantId, string eventType);
        Task<int> CountActiveByTenantIdAsync(Guid tenantId);
        Task<Guid> CreateAsync(WebhookSubscription subscription);
        Task UpdateAsync(WebhookSubscription subscription);
        Task DeleteAsync(WebhookSubscription subscription);
        IQueryable<WebhookSubscription> Query(Guid tenantId);
    }
}
