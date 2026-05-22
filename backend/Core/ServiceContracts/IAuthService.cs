using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface IAuthService
    {
        Task<GatewayResponseWrapper<LoginResponseDto>> LoginAsync(LoginRequestDto request);
        Task<GatewayResponseWrapper<LoginResponseDto>> RegisterAsync(RegisterRequestDto request);
        Task<GatewayResponseWrapper<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<GatewayResponseWrapper<bool>> ChangePasswordAsync(ChangePasswordRequestDto request);
        Task<GatewayResponseWrapper<UserResponseDto>> GetCurrentUserAsync();
    }
}
