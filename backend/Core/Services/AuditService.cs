using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.ServiceContracts;
using Core.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Core.Services
{
    public class AuditService : BaseService, IAuditService
    {
        private readonly IAuditLogRepository _auditRepo;

        public AuditService(ITenantContextProvider tcp, IAuditLogRepository auditRepo) : base(tcp)
        {
            _auditRepo = auditRepo;
        }

        public async Task<GatewayResponseWrapper<bool>> LogAsync(CreateAuditLogDto request)
        {
            var response = new GatewayResponseWrapper<bool>();
            var auditLog = new AuditLog
            {
                TenantId = CurrentTenantContext.TenantId, EntityType = request.EntityType, EntityId = request.EntityId,
                Action = request.Action, UserId = request.UserId, IPAddress = request.IPAddress, UserAgent = request.UserAgent,
                Changes = request.Changes != null ? JsonConvert.SerializeObject(request.Changes) : null,
                Status = request.Status ?? "success", ErrorMessage = request.ErrorMessage,
                Metadata = request.Metadata != null ? JsonConvert.SerializeObject(request.Metadata) : null
            };
            await _auditRepo.CreateAsync(auditLog);
            response.SetSuccess(true);
            return response;
        }

        public async Task<GatewayResponseWrapper<AuditLogResponseDto>> GetAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<AuditLogResponseDto>();
            var log = await _auditRepo.GetByIdAndTenantAsync(CurrentTenantContext.TenantId, id);
            if (log == null) { response.SetError("Audit log not found."); return response; }
            response.SetSuccess(MapAuditLog(log));
            return response;
        }

        public async Task<GatewayPaginatedListResponseWrapper<AuditLogResponseDto>> ListAsync(AuditLogFilterDto filter)
        {
            var response = new GatewayPaginatedListResponseWrapper<AuditLogResponseDto>();
            var query = _auditRepo.Query(CurrentTenantContext.TenantId);
            if (!string.IsNullOrEmpty(filter.EntityType)) query = query.Where(a => a.EntityType == filter.EntityType);
            if (!string.IsNullOrEmpty(filter.Action)) query = query.Where(a => a.Action == filter.Action);
            if (filter.UserId.HasValue) query = query.Where(a => a.UserId == filter.UserId.Value);
            if (!string.IsNullOrEmpty(filter.Status)) query = query.Where(a => a.Status == filter.Status);
            if (filter.DateFrom.HasValue) query = query.Where(a => a.CreatedAt >= filter.DateFrom.Value);
            if (filter.DateTo.HasValue) query = query.Where(a => a.CreatedAt <= filter.DateTo.Value);
            query = filter.SortOrder == "asc" ? query.OrderBy(a => a.CreatedAt) : query.OrderByDescending(a => a.CreatedAt);
            var total = await query.CountAsync();
            var items = await query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
            response.SetSuccessWithPagination(items.Select(MapAuditLog).ToList(), total, filter.Page, filter.PageSize);
            return response;
        }

        public async Task<GatewayResponseWrapper<List<AuditLogResponseDto>>> GetEntityHistoryAsync(string entityType, string entityId)
        {
            var response = new GatewayResponseWrapper<List<AuditLogResponseDto>>();
            var logs = await _auditRepo.Query(CurrentTenantContext.TenantId)
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .OrderByDescending(a => a.CreatedAt).ToListAsync();
            response.SetSuccess(logs.Select(MapAuditLog).ToList());
            return response;
        }

        public async Task<GatewayResponseWrapper<AuditStatsDto>> GetStatsAsync()
        {
            var response = new GatewayResponseWrapper<AuditStatsDto>();
            var last30Days = DateTime.UtcNow.AddDays(-30);
            var logs = await _auditRepo.GetByTenantIdSinceAsync(CurrentTenantContext.TenantId, last30Days);
            var stats = new AuditStatsDto
            {
                TotalEvents = logs.Count, SuccessCount = logs.Count(a => a.Status == "success"),
                ErrorCount = logs.Count(a => a.Status == "error"), WarningCount = logs.Count(a => a.Status == "warning"),
                ByEntityType = logs.GroupBy(a => a.EntityType).ToDictionary(g => g.Key, g => g.Count()),
                ByAction = logs.GroupBy(a => a.Action).ToDictionary(g => g.Key, g => g.Count()),
                TopUsers = logs.Where(a => a.UserId.HasValue).GroupBy(a => a.UserId).OrderByDescending(g => g.Count()).Take(5).ToDictionary(g => g.Key.Value, g => g.Count())
            };
            response.SetSuccess(stats);
            return response;
        }

        private static AuditLogResponseDto MapAuditLog(AuditLog log) => new()
        {
            Id = log.Id, EntityType = log.EntityType, EntityId = log.EntityId, Action = log.Action,
            UserId = log.UserId, IPAddress = log.IPAddress, UserAgent = log.UserAgent,
            Changes = !string.IsNullOrEmpty(log.Changes) ? JsonConvert.DeserializeObject<dynamic>(log.Changes) : null,
            Status = log.Status, ErrorMessage = log.ErrorMessage,
            Metadata = !string.IsNullOrEmpty(log.Metadata) ? JsonConvert.DeserializeObject<dynamic>(log.Metadata) : null,
            CreatedAt = log.CreatedAt
        };
    }
}
