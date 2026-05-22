using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Core.ContextProviders
{
    public class HttpTenantContextProvider : ITenantContextProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpTenantContextProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public TenantContext GetCurrentTenantContext()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return new TenantContext();

            var tenantContext = new TenantContext();

            // From middleware (API Key auth)
            if (context.Items.TryGetValue("TenantId", out var tenantIdObj) && tenantIdObj != null)
            {
                if (Guid.TryParse(tenantIdObj.ToString(), out var tenantId))
                    tenantContext.TenantId = tenantId;
            }

            if (context.Items.TryGetValue("ApiKeyId", out var apiKeyIdObj) && apiKeyIdObj != null)
            {
                if (Guid.TryParse(apiKeyIdObj.ToString(), out var apiKeyId))
                    tenantContext.ApiKeyId = apiKeyId;
            }

            if (context.Items.TryGetValue("ApiKeyPermissions", out var permissionsObj) && permissionsObj is List<string> permissions)
            {
                tenantContext.ApiKeyPermissions = permissions;
            }

            // From JWT claims
            var user = context.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out var userId))
                    tenantContext.UserId = userId;

                tenantContext.Role = user.FindFirst(ClaimTypes.Role)?.Value ?? "";

                var jwtTenantId = user.FindFirst("TenantId")?.Value;
                if (Guid.TryParse(jwtTenantId, out var jwtTenant))
                    tenantContext.TenantId = jwtTenant;
            }

            return tenantContext;
        }
    }
}
