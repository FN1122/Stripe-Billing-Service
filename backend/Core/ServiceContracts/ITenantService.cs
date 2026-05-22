using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface ITenantService
    {
        Task<GatewayResponseWrapper<TenantResponseDto>> CreateAsync(CreateTenantDto request);
        Task<GatewayResponseWrapper<TenantDetailResponseDto>> GetAsync(Guid id);
        Task<GatewayPaginatedListResponseWrapper<TenantResponseDto>> ListAsync(TenantFilterDto filter);
        Task<GatewayResponseWrapper<TenantResponseDto>> UpdateAsync(Guid id, UpdateTenantDto request);
        Task<GatewayResponseWrapper<bool>> SuspendAsync(Guid id, string reason);
        Task<GatewayResponseWrapper<bool>> ActivateAsync(Guid id);
        Task<GatewayResponseWrapper<TenantKeyRotationResponseDto>> RotateKeysAsync(Guid id);
        Task<GatewayResponseWrapper<TenantHealthCheckDto>> GetHealthCheckAsync(Guid id);
        Task<GatewayResponseWrapper<bool>> VerifyStripeConfigurationAsync(Guid id);
    }
}
