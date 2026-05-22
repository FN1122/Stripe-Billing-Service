using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface ISubscriptionRepository
    {
        Task<Subscription> GetByIdAsync(Guid id);
        Task<Subscription> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId);
        Task<Subscription> GetByStripeSubscriptionIdAndTenantAsync(Guid tenantId, string stripeSubscriptionId);
        Task<List<Subscription>> GetByTenantIdAsync(Guid tenantId);
        Task<List<Subscription>> GetByTenantIdWithPlanAsync(Guid tenantId);
        Task<List<Subscription>> GetByCustomerIdAsync(Guid customerId);
        Task<int> CountActiveByTenantIdAsync(Guid tenantId);
        Task<int> CountByTenantIdSinceAsync(Guid tenantId, DateTime since, string status = null);
        Task<Guid> CreateAsync(Subscription subscription);
        Task UpdateAsync(Subscription subscription);
        IQueryable<Subscription> Query(Guid tenantId);
    }
}
