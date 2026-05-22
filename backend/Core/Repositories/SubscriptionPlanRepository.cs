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
    public class SubscriptionPlanRepository : BaseRepository, ISubscriptionPlanRepository
    {
        private readonly BillingDbContext _dbContext;
        private readonly IValidator<SubscriptionPlan> _validator;

        public SubscriptionPlanRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext, IValidator<SubscriptionPlan> validator) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<SubscriptionPlan> GetByIdAsync(Guid id)
        {
            var plan = await _dbContext.SubscriptionPlans.FirstOrDefaultAsync(x => x.Id == id);
            await _validator.ValidateAndThrowAsync(plan, RuleValidator.GET);
            return plan!;
        }

        public async Task<SubscriptionPlan> GetByIdWithSubscriptionsAsync(Guid tenantId, Guid id)
        {
            var plan = await _dbContext.SubscriptionPlans.Include(p => p.Subscriptions).FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);
            await _validator.ValidateAndThrowAsync(plan, RuleValidator.GET);
            return plan!;
        }

        public async Task<SubscriptionPlan?> GetByStripePriceIdAsync(Guid tenantId, string stripePriceId)
        {
            return await _dbContext.SubscriptionPlans.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.StripePriceId == stripePriceId);
        }

        public async Task<List<SubscriptionPlan>> GetByTenantIdAsync(Guid tenantId)
        {
            return await _dbContext.SubscriptionPlans.Where(x => x.TenantId == tenantId).OrderBy(x => x.SortOrder).AsNoTracking().ToListAsync();
        }

        public async Task<List<SubscriptionPlan>> GetByTenantIdWithSubscriptionsAsync(Guid tenantId)
        {
            return await _dbContext.SubscriptionPlans.Include(p => p.Subscriptions).Where(p => p.TenantId == tenantId).OrderBy(p => p.SortOrder).AsNoTracking().ToListAsync();
        }

        public async Task<Guid> CreateAsync(SubscriptionPlan plan)
        {
            await _validator.ValidateAndThrowAsync(plan, RuleValidator.CREATE);
            _dbContext.SubscriptionPlans.Add(plan);
            await _dbContext.SaveChangesAsync();
            return plan.Id;
        }

        public async Task UpdateAsync(SubscriptionPlan plan)
        {
            await _validator.ValidateAndThrowAsync(plan, RuleValidator.UPDATE);
            _dbContext.SubscriptionPlans.Update(plan);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(SubscriptionPlan plan)
        {
            await _validator.ValidateAndThrowAsync(plan, RuleValidator.DELETE);
            _dbContext.SubscriptionPlans.Remove(plan);
            await _dbContext.SaveChangesAsync();
        }
    }
}
