using Core.ContextProviders;

namespace Core.Services
{
    public abstract class BaseService
    {
        protected readonly ITenantContextProvider _tenantContextProvider;
        protected TenantContext CurrentTenantContext => _tenantContextProvider.GetCurrentTenantContext();

        protected BaseService(ITenantContextProvider tenantContextProvider)
        {
            _tenantContextProvider = tenantContextProvider;
        }

        protected bool AuthorizeOrError<T>(Utils.GatewayResponseWrapper<T> response, params string[] allowedRoles)
        {
            var role = CurrentTenantContext.Role;
            if (!allowedRoles.Contains(role))
            {
                response.SetError("You do not have permission to perform this action.", 403);
                return false;
            }
            return true;
        }
    }
}
