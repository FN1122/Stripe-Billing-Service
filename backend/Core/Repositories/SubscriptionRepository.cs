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
    public class SubscriptionRepository : BaseRepository, ISubscriptionRepository
    {
        private readonly BillingDbContext _dbContext;
        private readonly IValidator<Subscription> _validator;

        public SubscriptionRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext, IValidator<Subscription> validator) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<Subscription> GetByIdAsync(Guid id)
        {
            var subscription = await _dbContext.GetSubscriptionQueryAsNoTracking().Where(x => x.Id == id).FirstOrDefaultAsync();
            await _validator.ValidateAndThrowAsync(subscription, RuleValidator.GET);
            return subscription!;
        }

        public async Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId)
        {
            return await _dbContext.Subscriptions.FirstOrDefaultAsync(x => x.StripeSubscriptionId == stripeSubscriptionId);
        }

        public async Task<Subscription?> GetByStripeSubscriptionIdAndTenantAsync(Guid tenantId, string stripeSubscriptionId)
        {
            return await _dbContext.Subscriptions.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.StripeSubscriptionId == stripeSubscriptionId);
        }

        public async Task<List<Subscription>> GetByTenantIdAsync(Guid tenantId)
        {
            return await _dbContext.GetSubscriptionQueryAsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        public async Task<List<Subscription>> GetByTenantIdWithPlanAsync(Guid tenantId)
        {
            return await _dbContext.Subscriptions.Include(s => s.Plan).Include(s => s.Customer).Where(s => s.TenantId == tenantId).AsNoTracking().ToListAsync();
        }

        public async Task<List<Subscription>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _dbContext.GetSubscriptionQueryAsNoTracking().Where(x => x.CustomerId == customerId).OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        public async Task<int> CountActiveByTenantIdAsync(Guid tenantId)
        {
            return await _dbContext.Subscriptions.Where(s => s.TenantId == tenantId && s.Status == "active").CountAsync();
        }

        public async Task<int> CountByTenantIdSinceAsync(Guid tenantId, DateTime since, string? status = null)
        {
            var query = _dbContext.Subscriptions.Where(s => s.TenantId == tenantId && s.CreatedAt >= since);
            if (!string.IsNullOrEmpty(status)) query = query.Where(s => s.Status == status);
            return await query.CountAsync();
        }

        public async Task<Guid> CreateAsync(Subscription subscription)
        {
            await _validator.ValidateAndThrowAsync(subscription, RuleValidator.CREATE);
            _dbContext.Subscriptions.Add(subscription);
            await _dbContext.SaveChangesAsync();
            return subscription.Id;
        }

        public async Task UpdateAsync(Subscription subscription)
        {
            await _validator.ValidateAndThrowAsync(subscription, RuleValidator.UPDATE);
            _dbContext.Subscriptions.Update(subscription);
            await _dbContext.SaveChangesAsync();
        }

        public IQueryable<Subscription> Query(Guid tenantId)
        {
            return _dbContext.Subscriptions.Include(s => s.Plan).Include(s => s.Customer).Where(s => s.TenantId == tenantId);
        }
    }
}
