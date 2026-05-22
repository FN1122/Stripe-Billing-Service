using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface IInvoiceRepository
    {
        Task<Invoice> GetByIdAsync(Guid id);
        Task<Invoice?> GetByStripeInvoiceIdAsync(string stripeInvoiceId);
        Task<List<Invoice>> GetByTenantIdAsync(Guid tenantId);
        Task<List<Invoice>> GetByCustomerIdAsync(Guid customerId);
        Task<Guid> CreateAsync(Invoice invoice);
        Task UpdateAsync(Invoice invoice);
        IQueryable<Invoice> Query(Guid tenantId);
    }
}
