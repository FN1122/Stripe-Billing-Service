using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface ITenantRepository
    {
        Task<Tenant> GetByIdAsync(Guid id);
        Task<Tenant> GetByIdWithDetailsAsync(Guid id);
        Task<Tenant> GetByIdWithCollectionsAsync(Guid id);
        Task<Tenant> GetByNameAsync(string name);
        Task<List<Tenant>> GetAllAsync();
        Task<Guid> CreateAsync(Tenant tenant);
        Task UpdateAsync(Tenant tenant);
        IQueryable<Tenant> Query();
    }
}
