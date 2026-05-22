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
    public class ApiCallLogService : BaseService, IApiCallLogService
    {
        private readonly IApiCallLogRepository _logRepo;

        public ApiCallLogService(ITenantContextProvider tcp, IApiCallLogRepository logRepo) : base(tcp)
        {
            _logRepo = logRepo;
        }

        public async Task<GatewayResponseWrapper<bool>> LogCallAsync(CreateApiCallLogDto request)
        {
            var response = new GatewayResponseWrapper<bool>();
            var log = new ApiCallLog
            {
                TenantId = CurrentTenantContext.TenantId, ApiKeyId = request.ApiKeyId, Method = request.Method,
                Endpoint = request.Endpoint, StatusCode = request.StatusCode, ResponseTime = request.ResponseTime,
                RequestSize = request.RequestSize, ResponseSize = request.ResponseSize, IpAddress = request.IpAddress,
                UserAgent = request.UserAgent, RequestBody = request.RequestBody, ResponseBody = request.ResponseBody,
                ErrorMessage = request.ErrorMessage, Success = request.Success
            };
            await _logRepo.CreateAsync(log);
            response.SetSuccess(true);
            return response;
        }

        public async Task<GatewayResponseWrapper<ApiCallLogResponseDto>> GetAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<ApiCallLogResponseDto>();
            var log = await _logRepo.GetByIdAndTenantAsync(CurrentTenantContext.TenantId, id);
            if (log == null) { response.SetError("API call log not found."); return response; }
            response.SetSuccess(MapApiCallLog(log));
            return response;
        }

        public async Task<GatewayPaginatedListResponseWrapper<ApiCallLogResponseDto>> ListAsync(ApiCallLogFilterDto filter)
        {
            var response = new GatewayPaginatedListResponseWrapper<ApiCallLogResponseDto>();
            var query = _logRepo.Query(CurrentTenantContext.TenantId);
            if (!string.IsNullOrEmpty(filter.Method)) query = query.Where(l => l.Method == filter.Method);
            if (!string.IsNullOrEmpty(filter.Endpoint)) query = query.Where(l => l.Endpoint.Contains(filter.Endpoint));
            if (filter.ApiKeyId.HasValue) query = query.Where(l => l.ApiKeyId == filter.ApiKeyId.Value);
            if (filter.StatusCode.HasValue) query = query.Where(l => l.StatusCode == filter.StatusCode.Value);
            if (filter.Success.HasValue) query = query.Where(l => l.Success == filter.Success.Value);
            if (filter.DateFrom.HasValue) query = query.Where(l => l.CreatedAt >= filter.DateFrom.Value);
            if (filter.DateTo.HasValue) query = query.Where(l => l.CreatedAt <= filter.DateTo.Value);
            if (!string.IsNullOrEmpty(filter.IpAddress)) query = query.Where(l => l.IpAddress == filter.IpAddress);
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(l => l.CreatedAt).Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
            response.SetSuccessWithPagination(items.Select(MapApiCallLog).ToList(), total, filter.Page, filter.PageSize);
            return response;
        }

        public async Task<GatewayResponseWrapper<ApiCallStatsDto>> GetStatsAsync(string period = "24h")
        {
            var response = new GatewayResponseWrapper<ApiCallStatsDto>();
            var hours = period switch { "1h" => 1, "24h" => 24, "7d" => 168, "30d" => 720, _ => 24 };
            var since = DateTime.UtcNow.AddHours(-hours);
            var logs = await _logRepo.GetByTenantIdSinceAsync(CurrentTenantContext.TenantId, since);
            var successLogs = logs.Where(l => l.Success).ToList();
            var errorLogs = logs.Where(l => !l.Success).ToList();
            var stats = new ApiCallStatsDto
            {
                Period = period, TotalCalls = logs.Count, SuccessfulCalls = successLogs.Count, FailedCalls = errorLogs.Count,
                SuccessRate = logs.Count > 0 ? Math.Round((decimal)successLogs.Count / logs.Count * 100, 1) : 0,
                AverageResponseTime = logs.Count > 0 ? Math.Round(logs.Average(l => l.ResponseTime), 0) : 0,
                SlowestEndpoint = logs.OrderByDescending(l => l.ResponseTime).FirstOrDefault()?.Endpoint,
                SlowestResponseTime = logs.OrderByDescending(l => l.ResponseTime).FirstOrDefault()?.ResponseTime ?? 0,
                TotalDataTransferred = logs.Sum(l => l.RequestSize + l.ResponseSize),
                AverageDataTransferred = logs.Count > 0 ? Math.Round((decimal)logs.Sum(l => l.RequestSize + l.ResponseSize) / logs.Count, 0) : 0,
                TopEndpoints = logs.GroupBy(l => l.Endpoint).OrderByDescending(g => g.Count()).Take(10).ToDictionary(g => g.Key, g => g.Count()),
                ByMethod = logs.GroupBy(l => l.Method).ToDictionary(g => g.Key, g => g.Count()),
                ByStatusCode = logs.GroupBy(l => l.StatusCode).OrderBy(g => g.Key).ToDictionary(g => g.Key, g => g.Count()),
                TopIpAddresses = logs.GroupBy(l => l.IpAddress).OrderByDescending(g => g.Count()).Take(10).ToDictionary(g => g.Key, g => g.Count())
            };
            response.SetSuccess(stats);
            return response;
        }

        public async Task<GatewayResponseWrapper<List<ApiCallLogResponseDto>>> GetByEndpointAsync(string endpoint, int limit = 100)
        {
            var response = new GatewayResponseWrapper<List<ApiCallLogResponseDto>>();
            var logs = await _logRepo.Query(CurrentTenantContext.TenantId).Where(l => l.Endpoint == endpoint).OrderByDescending(l => l.CreatedAt).Take(limit).ToListAsync();
            response.SetSuccess(logs.Select(MapApiCallLog).ToList());
            return response;
        }

        public async Task<GatewayResponseWrapper<List<ApiCallLogResponseDto>>> GetByApiKeyAsync(Guid apiKeyId, int limit = 100)
        {
            var response = new GatewayResponseWrapper<List<ApiCallLogResponseDto>>();
            var logs = await _logRepo.Query(CurrentTenantContext.TenantId).Where(l => l.ApiKeyId == apiKeyId).OrderByDescending(l => l.CreatedAt).Take(limit).ToListAsync();
            response.SetSuccess(logs.Select(MapApiCallLog).ToList());
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> DeleteOlderThanAsync(int days)
        {
            var response = new GatewayResponseWrapper<bool>();
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            var logsToDelete = await _logRepo.Query(CurrentTenantContext.TenantId).Where(l => l.CreatedAt < cutoffDate).ToListAsync();
            if (logsToDelete.Any()) await _logRepo.DeleteRangeAsync(logsToDelete);
            response.SetSuccess(true, $"Deleted {logsToDelete.Count} logs older than {days} days.");
            return response;
        }

        public async Task<GatewayResponseWrapper<ApiUsageMetricsDto>> GetUsageMetricsAsync()
        {
            var response = new GatewayResponseWrapper<ApiUsageMetricsDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var logs24h = await _logRepo.GetByTenantIdSinceAsync(tenantId, DateTime.UtcNow.AddHours(-24));
            var logs7d = await _logRepo.GetByTenantIdSinceAsync(tenantId, DateTime.UtcNow.AddDays(-7));
            var logs30d = await _logRepo.GetByTenantIdSinceAsync(tenantId, DateTime.UtcNow.AddDays(-30));
            var metrics = new ApiUsageMetricsDto
            {
                Calls24h = logs24h.Count, Calls7d = logs7d.Count, Calls30d = logs30d.Count,
                SuccessRate24h = logs24h.Count > 0 ? Math.Round((decimal)logs24h.Count(l => l.Success) / logs24h.Count * 100, 1) : 0,
                SuccessRate7d = logs7d.Count > 0 ? Math.Round((decimal)logs7d.Count(l => l.Success) / logs7d.Count * 100, 1) : 0,
                SuccessRate30d = logs30d.Count > 0 ? Math.Round((decimal)logs30d.Count(l => l.Success) / logs30d.Count * 100, 1) : 0,
                AverageResponseTime24h = logs24h.Count > 0 ? Math.Round(logs24h.Average(l => l.ResponseTime), 0) : 0,
                AverageResponseTime7d = logs7d.Count > 0 ? Math.Round(logs7d.Average(l => l.ResponseTime), 0) : 0,
                AverageResponseTime30d = logs30d.Count > 0 ? Math.Round(logs30d.Average(l => l.ResponseTime), 0) : 0,
                DataTransferred24h = logs24h.Sum(l => l.RequestSize + l.ResponseSize),
                DataTransferred7d = logs7d.Sum(l => l.RequestSize + l.ResponseSize),
                DataTransferred30d = logs30d.Sum(l => l.RequestSize + l.ResponseSize),
                MostUsedEndpoint24h = logs24h.GroupBy(l => l.Endpoint).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key,
                MostUsedEndpoint7d = logs7d.GroupBy(l => l.Endpoint).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key,
                MostUsedEndpoint30d = logs30d.GroupBy(l => l.Endpoint).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key
            };
            response.SetSuccess(metrics);
            return response;
        }

        private static ApiCallLogResponseDto MapApiCallLog(ApiCallLog log) => new()
        {
            Id = log.Id, Method = log.Method, Endpoint = log.Endpoint, StatusCode = log.StatusCode,
            ResponseTime = log.ResponseTime, RequestSize = log.RequestSize, ResponseSize = log.ResponseSize,
            IpAddress = log.IpAddress, UserAgent = log.UserAgent, Success = log.Success,
            ErrorMessage = log.ErrorMessage, CreatedAt = log.CreatedAt
        };
    }
}
