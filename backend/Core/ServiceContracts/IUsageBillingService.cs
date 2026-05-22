using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface IUsageBillingService
    {
        Task<GatewayResponseWrapper<UsageRecordResponseDto>> ReportUsageAsync(CreateUsageRecordDto request);
        Task<GatewayResponseWrapper<List<UsageRecordResponseDto>>> BatchReportUsageAsync(BatchUsageRecordDto request);
        Task<GatewayPaginatedListResponseWrapper<UsageRecordResponseDto>> GetUsageAsync(UsageFilterDto filter);
        Task<GatewayResponseWrapper<UsageSummaryDto>> GetUsageSummaryAsync(Guid subscriptionId);
        Task<GatewayResponseWrapper<MeterEventResponseDto>> CreateMeterEventAsync(CreateMeterEventDto request);
        Task<GatewayPaginatedListResponseWrapper<MeterEventResponseDto>> GetMeterEventsAsync(Guid? customerId, string? eventName, int page, int pageSize);
        Task<GatewayResponseWrapper<UsageDashboardDto>> GetUsageDashboardAsync();
    }
}
