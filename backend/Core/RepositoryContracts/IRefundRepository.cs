using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface IRefundRepository
    {
        Task<Refund> GetByIdAsync(Guid id);
        Task<List<Refund>> GetByTenantIdAsync(Guid tenantId);
        Task<List<Refund>> GetByTransactionIdAsync(Guid transactionId);
        Task<int> CountPendingByTenantIdAsync(Guid tenantId);
        Task<Guid> CreateAsync(Refund refund);
        Task UpdateAsync(Refund refund);
        IQueryable<Refund> Query(Guid tenantId);
    }
}
