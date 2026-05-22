using Core.ContextProviders;
using Core.Infrastructure;
using Core.ServiceContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Core.Services
{
    public class JwtTokenService : BaseService, IJwtTokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(ITenantContextProvider tenantContextProvider, IConfiguration config) : base(tenantContextProvider)
        {
            _config = config;
        }

        public string GenerateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _config["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32Characters!StripeBilling2026"));

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role),
                new("TenantId", user.TenantId.ToString()),
                new("FullName", user.FullName ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "StripeBilling",
                audience: _config["Jwt:Audience"] ?? "StripeBilling",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60")),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateImpersonationToken(User superAdmin, Guid targetTenantId, string targetTenantName)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _config["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32Characters!StripeBilling2026"));

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, superAdmin.Id.ToString()),
                new(ClaimTypes.Email, superAdmin.Email),
                new(ClaimTypes.Role, "Viewer"), // Read-only access
                new("TenantId", targetTenantId.ToString()),
                new("FullName", superAdmin.FullName ?? ""),
                new("IsImpersonating", "true"),
                new("ImpersonatedTenantName", targetTenantName),
                new("OriginalRole", "SuperAdmin")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "StripeBilling",
                audience: _config["Jwt:Audience"] ?? "StripeBilling",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30), // Short-lived impersonation token
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public bool ValidateToken(string token)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    _config["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32Characters!StripeBilling2026"));

                new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["Jwt:Issuer"] ?? "StripeBilling",
                    ValidAudience = _config["Jwt:Audience"] ?? "StripeBilling",
                    IssuerSigningKey = key
                }, out _);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
