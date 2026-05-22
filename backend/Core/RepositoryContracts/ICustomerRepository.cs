using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface ICustomerRepository
    {
        Task<Customer> GetByIdAsync(Guid id);
        Task<Customer> GetByIdWithDetailsAsync(Guid tenantId, Guid id);
        Task<Customer?> GetByStripeCustomerIdAsync(Guid tenantId, string stripeCustomerId);
        Task<Customer?> GetByEmailAsync(Guid tenantId, string email);
        Task<Customer?> GetByExternalRefAsync(Guid tenantId, string externalRefId);
        Task<List<Customer>> GetByTenantIdAsync(Guid tenantId);
        Task<List<Customer>> GetByTenantIdWithDetailsAsync(Guid tenantId);
        Task<int> CountByTenantIdAsync(Guid tenantId);
        Task<int> CountByTenantIdSinceAsync(Guid tenantId, DateTime since);
        Task<Guid> CreateAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        IQueryable<Customer> Query(Guid tenantId);
    }
}
