using Core.ContextProviders;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories
{
    public class EmailRepository : BaseRepository, IEmailRepository
    {
        private readonly BillingDbContext _dbContext;

        public EmailRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
        }

        public async Task<List<EmailTemplate>> GetTemplatesAsync(Guid tenantId)
        {
            return await _dbContext.EmailTemplates.Where(t => t.TenantId == tenantId).OrderBy(t => t.TemplateKey).ToListAsync();
        }

        public async Task<EmailTemplate?> GetTemplateAsync(Guid tenantId, string templateKey)
        {
            return await _dbContext.EmailTemplates.FirstOrDefaultAsync(t => t.TenantId == tenantId && t.TemplateKey == templateKey);
        }

        public async Task<Guid> CreateTemplateAsync(EmailTemplate template)
        {
            _dbContext.EmailTemplates.Add(template);
            await _dbContext.SaveChangesAsync();
            return template.Id;
        }

        public async Task UpdateTemplateAsync(EmailTemplate template)
        {
            _dbContext.EmailTemplates.Update(template);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<EmailLog?> GetEmailLogAsync(Guid tenantId, Guid id)
        {
            return await _dbContext.EmailLogs.FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == id);
        }

        public IQueryable<EmailLog> QueryLogs(Guid tenantId)
        {
            return _dbContext.EmailLogs.Where(l => l.TenantId == tenantId);
        }

        public async Task<Guid> CreateLogAsync(EmailLog log)
        {
            _dbContext.EmailLogs.Add(log);
            await _dbContext.SaveChangesAsync();
            return log.Id;
        }

        public async Task UpdateLogAsync(EmailLog log)
        {
            _dbContext.EmailLogs.Update(log);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> CountByStatusAsync(Guid tenantId, string status)
        {
            return await _dbContext.EmailLogs.CountAsync(l => l.TenantId == tenantId && l.Status == status);
        }
    }
}
