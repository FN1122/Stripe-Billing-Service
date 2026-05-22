using Core.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace StripeBilling.API.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public abstract class GatewayControllerBase : ControllerBase
    {
        protected IActionResult ToResponse<T>(GatewayResponseWrapper<T> response)
        {
            return Ok(response);
        }

        protected IActionResult ToResponse<T>(GatewayPaginatedListResponseWrapper<T> response)
        {
            return Ok(response);
        }

        protected Guid GetTenantId()
        {
            // From API Key middleware
            if (HttpContext.Items.TryGetValue("TenantId", out var tenantIdObj) && tenantIdObj != null)
            {
                if (Guid.TryParse(tenantIdObj.ToString(), out var tenantId))
                    return tenantId;
            }
            // From JWT
            var claim = User.FindFirst("TenantId")?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }

        protected Guid GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }

        protected string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }
    }
}
