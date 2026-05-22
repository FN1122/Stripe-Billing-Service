using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface IApiKeyService
    {
        Task<GatewayResponseWrapper<ApiKeyCreateResponseDto>> CreateAsync(CreateApiKeyDto request);
        Task<GatewayResponseWrapper<ApiKeyResponseDto>> GetAsync(Guid id);
        Task<GatewayPaginatedListResponseWrapper<ApiKeyResponseDto>> ListAsync(ApiKeyFilterDto filter);
        Task<GatewayResponseWrapper<ApiKeyResponseDto>> UpdateAsync(Guid id, UpdateApiKeyDto request);
        Task<GatewayResponseWrapper<bool>> RevokeAsync(Guid id);
        Task<GatewayResponseWrapper<bool>> RestoreAsync(Guid id);
        Task<GatewayResponseWrapper<bool>> ValidateAsync(string key);
        Task<GatewayResponseWrapper<ApiKeyStatsDto>> GetStatsAsync();
    }
}
