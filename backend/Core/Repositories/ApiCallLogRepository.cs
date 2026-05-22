using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories
{
    public class ApiCallLogRepository : BaseRepository, IApiCallLogRepository
    {
        private readonly BillingDbContext _dbContext;
        private readonly IValidator<ApiCallLog> _validator;

        public ApiCallLogRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext, IValidator<ApiCallLog> validator) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<ApiCallLog> GetByIdAsync(Guid id)
        {
            var log = await _dbContext.ApiCallLogs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            await _validator.ValidateAndThrowAsync(log, RuleValidator.GET);
            return log!;
        }

        public async Task<ApiCallLog> GetByIdAndTenantAsync(Guid tenantId, Guid id)
        {
            var log = await _dbContext.ApiCallLogs.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id && l.TenantId == tenantId);
            await _validator.ValidateAndThrowAsync(log, RuleValidator.GET);
            return log!;
        }

        public async Task<List<ApiCallLog>> GetByTenantIdAsync(Guid tenantId) => await _dbContext.ApiCallLogs.Where(x => x.TenantId == tenantId).OrderByDescending(x => x.CreatedAt).AsNoTracking().ToListAsync();

        public async Task<List<ApiCallLog>> GetByTenantIdSinceAsync(Guid tenantId, DateTime since) => await _dbContext.ApiCallLogs.Where(l => l.TenantId == tenantId && l.CreatedAt >= since).AsNoTracking().ToListAsync();

        public async Task<Guid> CreateAsync(ApiCallLog log)
        {
            await _validator.ValidateAndThrowAsync(log, RuleValidator.CREATE);
            _dbContext.ApiCallLogs.Add(log);
            await _dbContext.SaveChangesAsync();
            return log.Id;
        }

        public async Task DeleteRangeAsync(List<ApiCallLog> logs)
        {
            foreach (var log in logs)
                await _validator.ValidateAndThrowAsync(log, RuleValidator.DELETE);
            _dbContext.ApiCallLogs.RemoveRange(logs);
            await _dbContext.SaveChangesAsync();
        }

        public IQueryable<ApiCallLog> Query(Guid tenantId) => _dbContext.ApiCallLogs.Where(l => l.TenantId == tenantId);
    }
}
