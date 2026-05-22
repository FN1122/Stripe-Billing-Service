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
    public class PaymentTransactionRepository : BaseRepository, IPaymentTransactionRepository
    {
        private readonly BillingDbContext _dbContext;
        private readonly IValidator<PaymentTransaction> _validator;

        public PaymentTransactionRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext, IValidator<PaymentTransaction> validator) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<PaymentTransaction> GetByIdAsync(Guid id)
        {
            var tx = await _dbContext.GetPaymentTransactionQueryAsNoTracking().Where(x => x.Id == id).FirstOrDefaultAsync();
            await _validator.ValidateAndThrowAsync(tx, RuleValidator.GET);
            return tx!;
        }

        public async Task<PaymentTransaction> GetByIdWithCustomerAsync(Guid tenantId, Guid id)
        {
            var tx = await _dbContext.PaymentTransactions.Include(t => t.Customer).FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
            await _validator.ValidateAndThrowAsync(tx, RuleValidator.GET);
            return tx!;
        }

        public async Task<PaymentTransaction?> GetByStripePaymentIntentIdAsync(Guid tenantId, string stripePaymentIntentId)
        {
            return await _dbContext.PaymentTransactions.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.StripePaymentIntentId == stripePaymentIntentId);
        }

        public async Task<PaymentTransaction?> GetByStripeChargeIdAsync(Guid tenantId, string stripeChargeId)
        {
            return await _dbContext.PaymentTransactions.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.StripeChargeId == stripeChargeId);
        }

        public async Task<List<PaymentTransaction>> GetByTenantIdAsync(Guid tenantId)
        {
            return await _dbContext.GetPaymentTransactionQueryAsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        public async Task<List<PaymentTransaction>> GetByTenantIdSinceAsync(Guid tenantId, DateTime since)
        {
            return await _dbContext.PaymentTransactions.Where(t => t.TenantId == tenantId && t.CreatedAt >= since).AsNoTracking().ToListAsync();
        }

        public async Task<List<PaymentTransaction>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _dbContext.GetPaymentTransactionQueryAsNoTracking().Where(x => x.CustomerId == customerId).OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        public async Task<decimal> SumSucceededByTenantIdSinceAsync(Guid tenantId, DateTime since)
        {
            return await _dbContext.PaymentTransactions.Where(t => t.TenantId == tenantId && t.Status == "succeeded" && t.CreatedAt >= since).SumAsync(t => t.Amount);
        }

        public async Task<Guid> CreateAsync(PaymentTransaction transaction)
        {
            await _validator.ValidateAndThrowAsync(transaction, RuleValidator.CREATE);
            _dbContext.PaymentTransactions.Add(transaction);
            await _dbContext.SaveChangesAsync();
            return transaction.Id;
        }

        public async Task UpdateAsync(PaymentTransaction transaction)
        {
            await _validator.ValidateAndThrowAsync(transaction, RuleValidator.UPDATE);
            _dbContext.PaymentTransactions.Update(transaction);
            await _dbContext.SaveChangesAsync();
        }

        public IQueryable<PaymentTransaction> Query(Guid tenantId)
        {
            return _dbContext.PaymentTransactions.Include(t => t.Customer).Where(t => t.TenantId == tenantId);
        }
    }
}
