using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface IPaymentTransactionRepository
    {
        Task<PaymentTransaction> GetByIdAsync(Guid id);
        Task<PaymentTransaction> GetByIdWithCustomerAsync(Guid tenantId, Guid id);
        Task<PaymentTransaction?> GetByStripePaymentIntentIdAsync(Guid tenantId, string stripePaymentIntentId);
        Task<PaymentTransaction?> GetByStripeChargeIdAsync(Guid tenantId, string stripeChargeId);
        Task<List<PaymentTransaction>> GetByTenantIdAsync(Guid tenantId);
        Task<List<PaymentTransaction>> GetByTenantIdSinceAsync(Guid tenantId, DateTime since);
        Task<List<PaymentTransaction>> GetByCustomerIdAsync(Guid customerId);
        Task<decimal> SumSucceededByTenantIdSinceAsync(Guid tenantId, DateTime since);
        Task<Guid> CreateAsync(PaymentTransaction transaction);
        Task UpdateAsync(PaymentTransaction transaction);
        IQueryable<PaymentTransaction> Query(Guid tenantId);
    }
}
