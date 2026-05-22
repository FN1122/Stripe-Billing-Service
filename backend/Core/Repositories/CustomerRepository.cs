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
    public class CustomerRepository : BaseRepository, ICustomerRepository
    {
        private readonly BillingDbContext _dbContext;
        private readonly IValidator<Customer> _validator;

        public CustomerRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext, IValidator<Customer> validator) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<Customer> GetByIdAsync(Guid id)
        {
            var customer = await _dbContext.GetCustomerQueryAsNoTracking().Where(x => x.Id == id).FirstOrDefaultAsync();
            await _validator.ValidateAndThrowAsync(customer, RuleValidator.GET);
            return customer!;
        }

        public async Task<Customer> GetByIdWithDetailsAsync(Guid tenantId, Guid id)
        {
            var customer = await _dbContext.Customers
                .Include(c => c.Subscriptions).ThenInclude(s => s.Plan)
                .Include(c => c.Transactions.OrderByDescending(t => t.CreatedAt).Take(20))
                .Include(c => c.Invoices.OrderByDescending(i => i.CreatedAt).Take(20))
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
            await _validator.ValidateAndThrowAsync(customer, RuleValidator.GET);
            return customer!;
        }

        public async Task<Customer?> GetByStripeCustomerIdAsync(Guid tenantId, string stripeCustomerId)
        {
            var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.StripeCustomerId == stripeCustomerId);
            if (customer != null) await _validator.ValidateAndThrowAsync(customer, RuleValidator.GET);
            return customer;
        }

        public async Task<Customer?> GetByEmailAsync(Guid tenantId, string email)
        {
            return await _dbContext.Customers.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Email == email);
        }

        public async Task<Customer?> GetByExternalRefAsync(Guid tenantId, string externalRefId)
        {
            var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.ExternalReferenceId == externalRefId);
            if (customer != null) await _validator.ValidateAndThrowAsync(customer, RuleValidator.GET);
            return customer;
        }

        public async Task<List<Customer>> GetByTenantIdAsync(Guid tenantId)
        {
            return await _dbContext.GetCustomerQueryAsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        public async Task<List<Customer>> GetByTenantIdWithDetailsAsync(Guid tenantId)
        {
            return await _dbContext.Customers
                .Include(c => c.Subscriptions)
                .Include(c => c.Transactions)
                .Where(c => c.TenantId == tenantId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> CountByTenantIdAsync(Guid tenantId)
        {
            return await _dbContext.Customers.Where(c => c.TenantId == tenantId).CountAsync();
        }

        public async Task<int> CountByTenantIdSinceAsync(Guid tenantId, DateTime since)
        {
            return await _dbContext.Customers.Where(c => c.TenantId == tenantId && c.CreatedAt >= since).CountAsync();
        }

        public async Task<Guid> CreateAsync(Customer customer)
        {
            await _validator.ValidateAndThrowAsync(customer, RuleValidator.CREATE);
            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync();
            return customer.Id;
        }

        public async Task UpdateAsync(Customer customer)
        {
            await _validator.ValidateAndThrowAsync(customer, RuleValidator.UPDATE);
            _dbContext.Customers.Update(customer);
            await _dbContext.SaveChangesAsync();
        }

        public IQueryable<Customer> Query(Guid tenantId)
        {
            return _dbContext.Customers.Include(c => c.Subscriptions).Include(c => c.Transactions).Where(c => c.TenantId == tenantId);
        }
    }
}
