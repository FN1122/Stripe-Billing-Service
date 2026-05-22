using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface IWebhookEventInboundRepository
    {
        Task<WebhookEventInbound> GetByStripeEventIdAsync(Guid tenantId, string stripeEventId);
        Task<Guid> CreateAsync(WebhookEventInbound webhookEvent);
        Task UpdateAsync(WebhookEventInbound webhookEvent);
    }
}
