using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface IUserService
    {
        Task<GatewayResponseWrapper<UserResponseDto>> CreateAsync(CreateUserDto request);
        Task<GatewayResponseWrapper<UserResponseDto>> GetAsync(Guid id);
        Task<GatewayResponseWrapper<UserResponseDto>> GetByEmailAsync(string email);
        Task<GatewayPaginatedListResponseWrapper<UserResponseDto>> ListAsync(UserFilterDto filter);
        Task<GatewayResponseWrapper<UserResponseDto>> UpdateAsync(Guid id, UpdateUserDto request);
        Task<GatewayResponseWrapper<bool>> UpdateRoleAsync(Guid id, string role);
        Task<GatewayResponseWrapper<bool>> DeactivateAsync(Guid id);
        Task<GatewayResponseWrapper<bool>> ActivateAsync(Guid id);
        Task<GatewayResponseWrapper<bool>> UpdatePermissionsAsync(Guid id, List<string> permissions);
    }
}
