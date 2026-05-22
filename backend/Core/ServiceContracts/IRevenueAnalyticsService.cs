using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface IRevenueAnalyticsService
    {
        Task<GatewayResponseWrapper<MrrDto>> GetMrrAsync();
        Task<GatewayResponseWrapper<ChurnRateDto>> GetChurnRateAsync(string period = "30d");
        Task<GatewayResponseWrapper<LtvDto>> GetLtvAsync();
        Task<GatewayResponseWrapper<RevenueMetricsDto>> GetRevenueMetricsAsync(string period = "30d");
        Task<GatewayResponseWrapper<DashboardStatsDto>> GetDashboardStatsAsync();
        Task<GatewayResponseWrapper<List<ActivityFeedItemDto>>> GetActivityFeedAsync(int limit = 50);
    }
}
