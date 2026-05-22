using Core.Infrastructure;

namespace Core.ServiceContracts
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(User user);
        string GenerateImpersonationToken(User superAdmin, Guid targetTenantId, string targetTenantName);
        string GenerateRefreshToken();
        bool ValidateToken(string token);
    }
}
