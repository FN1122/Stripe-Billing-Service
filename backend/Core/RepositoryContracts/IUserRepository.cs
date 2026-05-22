using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(Guid id);
        Task<User> GetByEmailAsync(Guid tenantId, string email);
        Task<User> GetByEmailGlobalAsync(string email);
        Task<List<User>> GetByTenantIdAsync(Guid tenantId);
        Task<Guid> CreateAsync(User user);
        Task UpdateAsync(User user);
        Task UpdateLoginTimestampAsync(User user);
        IQueryable<User> Query(Guid tenantId);
    }
}
