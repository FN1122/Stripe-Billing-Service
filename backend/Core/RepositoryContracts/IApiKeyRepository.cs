using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface IApiKeyRepository
    {
        Task<ApiKey> GetByIdAsync(Guid id);
        Task<ApiKey> GetByIdAndTenantAsync(Guid tenantId, Guid id);
        Task<ApiKey?> GetByKeyHashAsync(string keyHash);
        Task<ApiKey?> GetByKeyPrefixAsync(string keyPrefix);
        Task<List<ApiKey>> GetByTenantIdAsync(Guid tenantId);
        Task<int> CountActiveByTenantIdAsync(Guid tenantId);
        Task<Guid> CreateAsync(ApiKey apiKey);
        Task UpdateAsync(ApiKey apiKey);
        IQueryable<ApiKey> Query(Guid tenantId);
    }
}
