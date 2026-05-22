using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories
{
    public class InvoiceRepository : BaseRepository, IInvoiceRepository
    {
        private readonly BillingDbContext _dbContext;
        private readonly IValidator<Invoice> _validator;

        public InvoiceRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext, IValidator<Invoice> validator) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<Invoice> GetByIdAsync(Guid id)
        {
            var invoice = await _dbContext.Invoices.Include(i => i.Customer).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            await _validator.ValidateAndThrowAsync(invoice, RuleValidator.GET);
            return invoice!;
        }

        public async Task<Invoice?> GetByStripeInvoiceIdAsync(string stripeInvoiceId) => await _dbContext.Invoices.FirstOrDefaultAsync(x => x.StripeInvoiceId == stripeInvoiceId);

        public async Task<List<Invoice>> GetByTenantIdAsync(Guid tenantId) => await _dbContext.Invoices.Include(i => i.Customer).Where(x => x.TenantId == tenantId).OrderByDescending(x => x.CreatedAt).AsNoTracking().ToListAsync();

        public async Task<List<Invoice>> GetByCustomerIdAsync(Guid customerId) => await _dbContext.Invoices.Where(x => x.CustomerId == customerId).OrderByDescending(x => x.CreatedAt).AsNoTracking().ToListAsync();

        public async Task<Guid> CreateAsync(Invoice invoice)
        {
            await _validator.ValidateAndThrowAsync(invoice, RuleValidator.CREATE);
            _dbContext.Invoices.Add(invoice);
            await _dbContext.SaveChangesAsync();
            return invoice.Id;
        }

        public async Task UpdateAsync(Invoice invoice)
        {
            await _validator.ValidateAndThrowAsync(invoice, RuleValidator.UPDATE);
            _dbContext.Invoices.Update(invoice);
            await _dbContext.SaveChangesAsync();
        }

        public IQueryable<Invoice> Query(Guid tenantId) => _dbContext.Invoices.Include(i => i.Customer).Where(i => i.TenantId == tenantId);
    }
}
