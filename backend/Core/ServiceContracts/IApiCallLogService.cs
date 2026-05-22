using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface IApiCallLogService
    {
        Task<GatewayResponseWrapper<bool>> LogCallAsync(CreateApiCallLogDto request);
        Task<GatewayResponseWrapper<ApiCallLogResponseDto>> GetAsync(Guid id);
        Task<GatewayPaginatedListResponseWrapper<ApiCallLogResponseDto>> ListAsync(ApiCallLogFilterDto filter);
        Task<GatewayResponseWrapper<ApiCallStatsDto>> GetStatsAsync(string period = "24h");
        Task<GatewayResponseWrapper<List<ApiCallLogResponseDto>>> GetByEndpointAsync(string endpoint, int limit = 100);
        Task<GatewayResponseWrapper<List<ApiCallLogResponseDto>>> GetByApiKeyAsync(Guid apiKeyId, int limit = 100);
        Task<GatewayResponseWrapper<bool>> DeleteOlderThanAsync(int days);
        Task<GatewayResponseWrapper<ApiUsageMetricsDto>> GetUsageMetricsAsync();
    }
}
