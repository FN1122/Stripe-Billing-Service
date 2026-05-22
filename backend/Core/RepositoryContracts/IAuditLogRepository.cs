using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface IAuditLogRepository
    {
        Task<AuditLog> GetByIdAsync(Guid id);
        Task<AuditLog> GetByIdAndTenantAsync(Guid tenantId, Guid id);
        Task<List<AuditLog>> GetByTenantIdAsync(Guid tenantId);
        Task<List<AuditLog>> GetByTenantIdSinceAsync(Guid tenantId, DateTime since);
        Task<Guid> CreateAsync(AuditLog auditLog);
        IQueryable<AuditLog> Query(Guid tenantId);
    }
}
