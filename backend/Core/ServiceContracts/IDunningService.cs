using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface IDunningService
    {
        Task<GatewayResponseWrapper<DunningConfigDto>> GetConfigAsync();
        Task<GatewayResponseWrapper<DunningConfigDto>> UpdateConfigAsync(DunningConfigDto request);
        Task<GatewayPaginatedListResponseWrapper<DunningScheduleResponseDto>> GetSchedulesAsync(DunningFilterDto filter);
        Task<GatewayResponseWrapper<DunningScheduleResponseDto>> GetScheduleAsync(Guid id);
        Task<GatewayResponseWrapper<bool>> PauseScheduleAsync(Guid id);
        Task<GatewayResponseWrapper<bool>> ResumeScheduleAsync(Guid id);
        Task<GatewayResponseWrapper<bool>> CancelScheduleAsync(Guid id);
        Task<GatewayResponseWrapper<bool>> ManualRetryAsync(Guid id);
        Task<GatewayResponseWrapper<DunningDashboardDto>> GetDashboardAsync();
        Task InitiateDunningAsync(Guid tenantId, Guid subscriptionId, Guid customerId, string? invoiceId, decimal amount, string? reason);
    }
}
