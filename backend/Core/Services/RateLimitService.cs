using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.ServiceContracts;
using Core.Utils;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
    public class RateLimitService : BaseService, IRateLimitService
    {
        private readonly BillingDbContext _dbContext;

        public RateLimitService(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
        }

        public async Task<GatewayResponseWrapper<List<RateLimitResponseDto>>> ListAsync()
        {
            var response = new GatewayResponseWrapper<List<RateLimitResponseDto>>();
            var tenantId = CurrentTenantContext.TenantId;
            var limits = await _dbContext.EndpointRateLimits.Where(r => r.TenantId == tenantId).OrderBy(r => r.Endpoint).ToListAsync();
            response.SetSuccess(limits.Select(MapRateLimit).ToList());
            return response;
        }

        public async Task<GatewayResponseWrapper<RateLimitResponseDto>> CreateAsync(CreateRateLimitDto request)
        {
            var response = new GatewayResponseWrapper<RateLimitResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var existing = await _dbContext.EndpointRateLimits.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Endpoint == request.Endpoint);
            if (existing != null) { response.SetError("Rate limit for this endpoint already exists.", 400); return response; }

            var limit = new EndpointRateLimitConfig
            {
                TenantId = tenantId,
                Endpoint = request.Endpoint,
                RequestsPerMinute = request.RequestsPerMinute,
                BurstLimit = request.BurstLimit
            };

            _dbContext.EndpointRateLimits.Add(limit);
            await _dbContext.SaveChangesAsync();
            response.SetSuccess(MapRateLimit(limit));
            return response;
        }

        public async Task<GatewayResponseWrapper<RateLimitResponseDto>> UpdateAsync(Guid id, UpdateRateLimitDto request)
        {
            var response = new GatewayResponseWrapper<RateLimitResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var limit = await _dbContext.EndpointRateLimits.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);
            if (limit == null) { response.SetError("Rate limit not found.", 404); return response; }

            if (request.RequestsPerMinute.HasValue) limit.RequestsPerMinute = request.RequestsPerMinute.Value;
            if (request.BurstLimit.HasValue) limit.BurstLimit = request.BurstLimit;
            if (request.IsActive.HasValue) limit.IsActive = request.IsActive.Value;
            limit.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            response.SetSuccess(MapRateLimit(limit));
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> DeleteAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;
            var limit = await _dbContext.EndpointRateLimits.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);
            if (limit == null) { response.SetError("Rate limit not found.", 404); return response; }

            _dbContext.EndpointRateLimits.Remove(limit);
            await _dbContext.SaveChangesAsync();
            response.SetSuccess(true, "Rate limit deleted.");
            return response;
        }

        public async Task<GatewayResponseWrapper<List<RateLimitUsageDto>>> GetUsageAsync()
        {
            var response = new GatewayResponseWrapper<List<RateLimitUsageDto>>();
            response.SetSuccess(new List<RateLimitUsageDto>());
            return response;
        }

        private RateLimitResponseDto MapRateLimit(EndpointRateLimitConfig r) => new()
        {
            Id = r.Id, Endpoint = r.Endpoint, RequestsPerMinute = r.RequestsPerMinute,
            BurstLimit = r.BurstLimit, IsActive = r.IsActive,
            CreatedAt = r.CreatedAt, UpdatedAt = r.UpdatedAt
        };
    }
}
