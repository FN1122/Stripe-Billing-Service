using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface ICreditRepository
    {
        Task<CustomerCredit?> GetByIdAsync(Guid tenantId, Guid id);
        IQueryable<CustomerCredit> Query(Guid tenantId, Guid customerId);
        IQueryable<CustomerCredit> QueryAll(Guid tenantId);
        Task<Guid> CreateAsync(CustomerCredit credit);
        Task<decimal> GetBalanceAsync(Guid tenantId, Guid customerId);
        Task<decimal> SumByTypeAsync(Guid tenantId, Guid customerId, string type);
        Task<decimal> TotalOutstandingAsync(Guid tenantId);
        Task<int> CountCustomersWithCreditsAsync(Guid tenantId);
    }
}
