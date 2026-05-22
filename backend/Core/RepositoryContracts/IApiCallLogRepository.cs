using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface IApiCallLogRepository
    {
        Task<ApiCallLog> GetByIdAsync(Guid id);
        Task<ApiCallLog> GetByIdAndTenantAsync(Guid tenantId, Guid id);
        Task<List<ApiCallLog>> GetByTenantIdAsync(Guid tenantId);
        Task<List<ApiCallLog>> GetByTenantIdSinceAsync(Guid tenantId, DateTime since);
        Task<Guid> CreateAsync(ApiCallLog log);
        Task DeleteRangeAsync(List<ApiCallLog> logs);
        IQueryable<ApiCallLog> Query(Guid tenantId);
    }
}
