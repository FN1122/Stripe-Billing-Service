using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface IAuditService
    {
        Task<GatewayResponseWrapper<bool>> LogAsync(CreateAuditLogDto request);
        Task<GatewayResponseWrapper<AuditLogResponseDto>> GetAsync(Guid id);
        Task<GatewayPaginatedListResponseWrapper<AuditLogResponseDto>> ListAsync(AuditLogFilterDto filter);
        Task<GatewayResponseWrapper<List<AuditLogResponseDto>>> GetEntityHistoryAsync(string entityType, string entityId);
        Task<GatewayResponseWrapper<AuditStatsDto>> GetStatsAsync();
    }
}
