using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface ISettingRepository
    {
        Task<Setting> GetByKeyAsync(Guid tenantId, string key);
        Task<List<Setting>> GetByTenantIdAsync(Guid tenantId);
        Task<Guid> CreateAsync(Setting setting);
        Task UpdateAsync(Setting setting);
        Task DeleteAsync(Setting setting);
    }
}
