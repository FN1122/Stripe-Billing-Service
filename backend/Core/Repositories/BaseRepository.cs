using Core.ContextProviders;

namespace Core.Repositories
{
    public abstract class BaseRepository
    {
        protected readonly ITenantContextProvider _tenantContextProvider;
        protected TenantContext CurrentTenantContext => _tenantContextProvider.GetCurrentTenantContext();

        protected BaseRepository(ITenantContextProvider tenantContextProvider)
        {
            _tenantContextProvider = tenantContextProvider;
        }
    }
}
