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
    public class RefundRepository : BaseRepository, IRefundRepository
    {
        private readonly BillingDbContext _dbContext;
        private readonly IValidator<Refund> _validator;

        public RefundRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext, IValidator<Refund> validator) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<Refund> GetByIdAsync(Guid id)
        {
            var refund = await _dbContext.GetRefundQueryAsNoTracking().Where(x => x.Id == id).FirstOrDefaultAsync();
            await _validator.ValidateAndThrowAsync(refund, RuleValidator.GET);
            return refund!;
        }

        public async Task<List<Refund>> GetByTenantIdAsync(Guid tenantId) => await _dbContext.GetRefundQueryAsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.CreatedAt).ToListAsync();

        public async Task<List<Refund>> GetByTransactionIdAsync(Guid transactionId) => await _dbContext.GetRefundQueryAsNoTracking().Where(x => x.TransactionId == transactionId).ToListAsync();

        public async Task<int> CountPendingByTenantIdAsync(Guid tenantId) => await _dbContext.Refunds.Where(r => r.TenantId == tenantId && r.Status == "pending").CountAsync();

        public async Task<Guid> CreateAsync(Refund refund)
        {
            await _validator.ValidateAndThrowAsync(refund, RuleValidator.CREATE);
            _dbContext.Refunds.Add(refund);
            await _dbContext.SaveChangesAsync();
            return refund.Id;
        }

        public async Task UpdateAsync(Refund refund)
        {
            await _validator.ValidateAndThrowAsync(refund, RuleValidator.UPDATE);
            _dbContext.Refunds.Update(refund);
            await _dbContext.SaveChangesAsync();
        }

        public IQueryable<Refund> Query(Guid tenantId) => _dbContext.Refunds.Include(r => r.Customer).Where(r => r.TenantId == tenantId);
    }
}
