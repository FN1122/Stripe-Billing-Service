using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories
{
    public class WebhookDeliveryRepository : BaseRepository, IWebhookDeliveryRepository
    {
        private readonly BillingDbContext _dbContext;
        private readonly IValidator<WebhookDelivery> _validator;

        public WebhookDeliveryRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext, IValidator<WebhookDelivery> validator) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<WebhookDelivery> GetByIdAsync(Guid id)
        {
            var delivery = await _dbContext.WebhookDeliveries.FirstOrDefaultAsync(x => x.Id == id);
            await _validator.ValidateAndThrowAsync(delivery, RuleValidator.GET);
            return delivery!;
        }

        public async Task<List<WebhookDelivery>> GetBySubscriptionIdAsync(Guid subscriptionId) => await _dbContext.WebhookDeliveries.Where(x => x.WebhookSubscriptionId == subscriptionId).OrderByDescending(x => x.CreatedAt).AsNoTracking().ToListAsync();

        public async Task<List<WebhookDelivery>> GetPendingAsync(int maxCount = 100)
        {
            var now = DateTime.UtcNow;
            return await _dbContext.WebhookDeliveries.Where(wd => wd.Status == "pending" && wd.NextRetryAt <= now && wd.RetryCount < wd.MaxRetries).OrderBy(wd => wd.CreatedAt).Take(maxCount).ToListAsync();
        }

        public async Task<List<WebhookDelivery>> GetRetryableAsync()
        {
            return await _dbContext.WebhookDeliveries.Where(x => x.Status == "failed" && x.RetryCount < x.MaxRetries && x.NextRetryAt <= DateTime.UtcNow).Include(x => x.WebhookSubscription).OrderBy(x => x.NextRetryAt).ToListAsync();
        }

        public async Task<Guid> CreateAsync(WebhookDelivery delivery)
        {
            await _validator.ValidateAndThrowAsync(delivery, RuleValidator.CREATE);
            _dbContext.WebhookDeliveries.Add(delivery);
            await _dbContext.SaveChangesAsync();
            return delivery.Id;
        }

        public async Task CreateRangeAsync(List<WebhookDelivery> deliveries)
        {
            _dbContext.WebhookDeliveries.AddRange(deliveries);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(WebhookDelivery delivery)
        {
            await _validator.ValidateAndThrowAsync(delivery, RuleValidator.UPDATE);
            _dbContext.WebhookDeliveries.Update(delivery);
            await _dbContext.SaveChangesAsync();
        }

        public IQueryable<WebhookDelivery> Query() => _dbContext.WebhookDeliveries;

        public IQueryable<WebhookDelivery> QueryByTenant(Guid tenantId) => _dbContext.WebhookDeliveries.Include(wd => wd.WebhookSubscription).Where(wd => wd.WebhookSubscription.TenantId == tenantId);
    }
}
