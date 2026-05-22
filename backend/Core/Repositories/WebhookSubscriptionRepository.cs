using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories
{
    public class WebhookSubscriptionRepository : BaseRepository, IWebhookSubscriptionRepository
    {
        private readonly BillingDbContext _dbContext;
        private readonly IValidator<WebhookSubscription> _validator;

        public WebhookSubscriptionRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext, IValidator<WebhookSubscription> validator) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<WebhookSubscription> GetByIdAsync(Guid id)
        {
            var ws = await _dbContext.WebhookSubscriptions.FirstOrDefaultAsync(x => x.Id == id);
            await _validator.ValidateAndThrowAsync(ws, RuleValidator.GET);
            return ws!;
        }

        public async Task<WebhookSubscription> GetByIdAndTenantAsync(Guid tenantId, Guid id)
        {
            var ws = await _dbContext.WebhookSubscriptions.FirstOrDefaultAsync(ws => ws.Id == id && ws.TenantId == tenantId);
            await _validator.ValidateAndThrowAsync(ws, RuleValidator.GET);
            return ws!;
        }

        public async Task<WebhookSubscription> GetByIdWithDeliveriesAsync(Guid tenantId, Guid id)
        {
            var ws = await _dbContext.WebhookSubscriptions.Include(ws => ws.WebhookDeliveries).FirstOrDefaultAsync(ws => ws.Id == id && ws.TenantId == tenantId);
            await _validator.ValidateAndThrowAsync(ws, RuleValidator.GET);
            return ws!;
        }

        public async Task<List<WebhookSubscription>> GetByTenantIdAsync(Guid tenantId) => await _dbContext.WebhookSubscriptions.Where(x => x.TenantId == tenantId).OrderByDescending(x => x.CreatedAt).AsNoTracking().ToListAsync();

        public async Task<List<WebhookSubscription>> GetActiveByTenantAndEventAsync(Guid tenantId, string eventType)
        {
            return await _dbContext.WebhookSubscriptions.Where(ws => ws.TenantId == tenantId && ws.IsActive && ws.Events.Contains(eventType)).ToListAsync();
        }

        public async Task<int> CountActiveByTenantIdAsync(Guid tenantId) => await _dbContext.WebhookSubscriptions.CountAsync(ws => ws.TenantId == tenantId && ws.IsActive);

        public async Task<Guid> CreateAsync(WebhookSubscription subscription)
        {
            await _validator.ValidateAndThrowAsync(subscription, RuleValidator.CREATE);
            _dbContext.WebhookSubscriptions.Add(subscription);
            await _dbContext.SaveChangesAsync();
            return subscription.Id;
        }

        public async Task UpdateAsync(WebhookSubscription subscription)
        {
            await _validator.ValidateAndThrowAsync(subscription, RuleValidator.UPDATE);
            _dbContext.WebhookSubscriptions.Update(subscription);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(WebhookSubscription subscription)
        {
            await _validator.ValidateAndThrowAsync(subscription, RuleValidator.DELETE);
            _dbContext.WebhookSubscriptions.Remove(subscription);
            await _dbContext.SaveChangesAsync();
        }

        public IQueryable<WebhookSubscription> Query(Guid tenantId) => _dbContext.WebhookSubscriptions.Where(ws => ws.TenantId == tenantId);
    }
}
