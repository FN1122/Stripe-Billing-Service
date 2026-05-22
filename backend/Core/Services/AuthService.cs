using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.ServiceContracts;
using Core.Utils;

namespace Core.Services
{
    public class AuthService : BaseService, IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly IJwtTokenService _jwtService;

        public AuthService(ITenantContextProvider tenantContextProvider, IUserRepository userRepo, IRefreshTokenRepository refreshTokenRepo, IJwtTokenService jwtService) : base(tenantContextProvider)
        {
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _jwtService = jwtService;
        }

        public async Task<GatewayResponseWrapper<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            var response = new GatewayResponseWrapper<LoginResponseDto>();

            var user = await _userRepo.GetByEmailGlobalAsync(request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                response.SetError("Invalid email or password.");
                return response;
            }

            if (!user.IsActive)
            {
                response.SetError("Account has been deactivated.");
                return response;
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepo.UpdateLoginTimestampAsync(user);

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            await _refreshTokenRepo.CreateAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

            response.SetSuccess(new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = new UserResponseDto
                {
                    Id = user.Id, TenantId = user.TenantId, Email = user.Email,
                    FullName = user.FullName, Role = user.Role, IsActive = user.IsActive,
                    LastLoginAt = user.LastLoginAt, CreatedAt = user.CreatedAt
                }
            });
            return response;
        }

        public async Task<GatewayResponseWrapper<LoginResponseDto>> RegisterAsync(RegisterRequestDto request)
        {
            var response = new GatewayResponseWrapper<LoginResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var existingUser = await _userRepo.GetByEmailAsync(tenantId, request.Email);
            if (existingUser != null)
            {
                response.SetError("A user with this email already exists.");
                return response;
            }

            var user = new User
            {
                TenantId = tenantId,
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12),
                Role = request.Role
            };

            await _userRepo.CreateAsync(user);

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            await _refreshTokenRepo.CreateAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

            response.SetSuccess(new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = new UserResponseDto
                {
                    Id = user.Id, TenantId = user.TenantId, Email = user.Email,
                    FullName = user.FullName, Role = user.Role, IsActive = user.IsActive, CreatedAt = user.CreatedAt
                }
            });
            return response;
        }

        public async Task<GatewayResponseWrapper<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var response = new GatewayResponseWrapper<LoginResponseDto>();

            var storedToken = await _refreshTokenRepo.GetByTokenAsync(request.RefreshToken);
            if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
            {
                response.SetError("Invalid or expired refresh token.");
                return response;
            }

            storedToken.IsRevoked = true;
            var newAccessToken = _jwtService.GenerateAccessToken(storedToken.User);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            storedToken.ReplacedByToken = newRefreshToken;
            await _refreshTokenRepo.UpdateAsync(storedToken);

            await _refreshTokenRepo.CreateAsync(new RefreshToken
            {
                UserId = storedToken.UserId,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

            response.SetSuccess(new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = new UserResponseDto
                {
                    Id = storedToken.User.Id, TenantId = storedToken.User.TenantId, Email = storedToken.User.Email,
                    FullName = storedToken.User.FullName, Role = storedToken.User.Role,
                    IsActive = storedToken.User.IsActive, LastLoginAt = storedToken.User.LastLoginAt, CreatedAt = storedToken.User.CreatedAt
                }
            });
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> ChangePasswordAsync(ChangePasswordRequestDto request)
        {
            var response = new GatewayResponseWrapper<bool>();
            var userId = CurrentTenantContext.UserId;

            User user;
            try
            {
                user = await _userRepo.GetByIdAsync(userId);
            }
            catch (FluentValidation.ValidationException)
            {
                response.SetError("Session expired. Please login again.", 401);
                return response;
            }
            if (user == null) { response.SetError("User not found.", 401); return response; }
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash)) { response.SetError("Current password is incorrect."); return response; }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, 12);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user);

            response.SetSuccess(true, "Password changed successfully.");
            return response;
        }

        public async Task<GatewayResponseWrapper<UserResponseDto>> GetCurrentUserAsync()
        {
            var response = new GatewayResponseWrapper<UserResponseDto>();
            var userId = CurrentTenantContext.UserId;

            User user;
            try
            {
                user = await _userRepo.GetByIdAsync(userId);
            }
            catch (FluentValidation.ValidationException)
            {
                response.SetError("User not found or session expired. Please login again.", 401);
                return response;
            }
            if (user == null) { response.SetError("User not found.", 401); return response; }

            response.SetSuccess(new UserResponseDto
            {
                Id = user.Id, TenantId = user.TenantId, Email = user.Email,
                FullName = user.FullName, Role = user.Role, IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt, CreatedAt = user.CreatedAt
            });
            return response;
        }
    }
}
