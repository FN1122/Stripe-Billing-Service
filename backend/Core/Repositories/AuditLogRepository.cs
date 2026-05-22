using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using Core.Queries;
using Core.RepositoryContracts;
using Core.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories
{
    public class AuditLogRepository : BaseRepository, IAuditLogRepository
    {
        private readonly BillingDbContext _dbContext;
        private readonly IValidator<AuditLog> _validator;

        public AuditLogRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext, IValidator<AuditLog> validator) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<AuditLog> GetByIdAsync(Guid id)
        {
            var log = await _dbContext.AuditLogs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            await _validator.ValidateAndThrowAsync(log, RuleValidator.GET);
            return log!;
        }

        public async Task<AuditLog> GetByIdAndTenantAsync(Guid tenantId, Guid id)
        {
            var log = await _dbContext.AuditLogs.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);
            await _validator.ValidateAndThrowAsync(log, RuleValidator.GET);
            return log!;
        }

        public async Task<List<AuditLog>> GetByTenantIdAsync(Guid tenantId) => await _dbContext.AuditLogs.Where(x => x.TenantId == tenantId).OrderByDescending(x => x.CreatedAt).AsNoTracking().ToListAsync();

        public async Task<List<AuditLog>> GetByTenantIdSinceAsync(Guid tenantId, DateTime since) => await _dbContext.AuditLogs.Where(a => a.TenantId == tenantId && a.CreatedAt >= since).AsNoTracking().ToListAsync();

        public async Task<Guid> CreateAsync(AuditLog auditLog)
        {
            await _validator.ValidateAndThrowAsync(auditLog, RuleValidator.CREATE);
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();
            return auditLog.Id;
        }

        public IQueryable<AuditLog> Query(Guid tenantId) => _dbContext.AuditLogs.Where(a => a.TenantId == tenantId);
    }
}
