using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface ISubscriptionPlanRepository
    {
        Task<SubscriptionPlan> GetByIdAsync(Guid id);
        Task<SubscriptionPlan> GetByIdWithSubscriptionsAsync(Guid tenantId, Guid id);
        Task<SubscriptionPlan> GetByStripePriceIdAsync(Guid tenantId, string stripePriceId);
        Task<List<SubscriptionPlan>> GetByTenantIdAsync(Guid tenantId);
        Task<List<SubscriptionPlan>> GetByTenantIdWithSubscriptionsAsync(Guid tenantId);
        Task<Guid> CreateAsync(SubscriptionPlan plan);
        Task UpdateAsync(SubscriptionPlan plan);
        Task DeleteAsync(SubscriptionPlan plan);
    }
}
