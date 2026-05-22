using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface IDashboardService
    {
        Task<GatewayResponseWrapper<ComprehensiveDashboardDto>> GetComprehensiveDashboardAsync();
        Task<GatewayResponseWrapper<DashboardStatsDto>> GetDashboardStatsAsync();
        Task<GatewayResponseWrapper<PaymentsDashboardDto>> GetPaymentsDashboardAsync();
        Task<GatewayResponseWrapper<SubscriptionsDashboardDto>> GetSubscriptionsDashboardAsync();
        Task<GatewayResponseWrapper<CustomersDashboardDto>> GetCustomersDashboardAsync();
        Task<GatewayResponseWrapper<List<AlertDto>>> GetAlertsAsync();
    }
}
