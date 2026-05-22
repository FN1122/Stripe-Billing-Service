using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface IRateLimitService
    {
        Task<GatewayResponseWrapper<List<RateLimitResponseDto>>> ListAsync();
        Task<GatewayResponseWrapper<RateLimitResponseDto>> CreateAsync(CreateRateLimitDto request);
        Task<GatewayResponseWrapper<RateLimitResponseDto>> UpdateAsync(Guid id, UpdateRateLimitDto request);
        Task<GatewayResponseWrapper<bool>> DeleteAsync(Guid id);
        Task<GatewayResponseWrapper<List<RateLimitUsageDto>>> GetUsageAsync();
    }
}
