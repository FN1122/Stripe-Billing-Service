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
    public class TenantRepository : BaseRepository, ITenantRepository
    {
        private readonly BillingDbContext _dbContext;
        private readonly IValidator<Tenant> _validator;

        public TenantRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext, IValidator<Tenant> validator) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<Tenant> GetByIdAsync(Guid id)
        {
            var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(x => x.Id == id);
            await _validator.ValidateAndThrowAsync(tenant, RuleValidator.GET);
            return tenant!;
        }

        public async Task<Tenant> GetByIdWithDetailsAsync(Guid id)
        {
            var tenant = await _dbContext.Tenants.Include(t => t.ApiKeys).Include(t => t.WebhookSubscriptions).Include(t => t.Users).FirstOrDefaultAsync(t => t.Id == id);
            await _validator.ValidateAndThrowAsync(tenant, RuleValidator.GET);
            return tenant!;
        }

        public async Task<Tenant> GetByIdWithCollectionsAsync(Guid id)
        {
            var tenant = await _dbContext.Tenants.Include(t => t.Customers).Include(t => t.Subscriptions).Include(t => t.PaymentTransactions).FirstOrDefaultAsync(t => t.Id == id);
            await _validator.ValidateAndThrowAsync(tenant, RuleValidator.GET);
            return tenant!;
        }

        public async Task<Tenant?> GetByNameAsync(string name) => await _dbContext.Tenants.FirstOrDefaultAsync(x => x.Name == name);

        public async Task<List<Tenant>> GetAllAsync() => await _dbContext.GetTenantQueryAsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync();

        public async Task<Guid> CreateAsync(Tenant tenant)
        {
            await _validator.ValidateAndThrowAsync(tenant, RuleValidator.CREATE);
            _dbContext.Tenants.Add(tenant);
            await _dbContext.SaveChangesAsync();
            return tenant.Id;
        }

        public async Task UpdateAsync(Tenant tenant)
        {
            await _validator.ValidateAndThrowAsync(tenant, RuleValidator.UPDATE);
            _dbContext.Tenants.Update(tenant);
            await _dbContext.SaveChangesAsync();
        }

        public IQueryable<Tenant> Query() => _dbContext.Tenants.AsQueryable();
    }
}
