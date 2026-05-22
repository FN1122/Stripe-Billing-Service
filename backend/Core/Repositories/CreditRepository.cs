using Core.ContextProviders;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories
{
    public class CreditRepository : BaseRepository, ICreditRepository
    {
        private readonly BillingDbContext _dbContext;

        public CreditRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
        }

        public async Task<CustomerCredit?> GetByIdAsync(Guid tenantId, Guid id)
        {
            return await _dbContext.CustomerCredits.Include(c => c.Customer).FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id);
        }

        public IQueryable<CustomerCredit> Query(Guid tenantId, Guid customerId)
        {
            return _dbContext.CustomerCredits.Include(c => c.Customer).Where(c => c.TenantId == tenantId && c.CustomerId == customerId);
        }

        public IQueryable<CustomerCredit> QueryAll(Guid tenantId)
        {
            return _dbContext.CustomerCredits.Include(c => c.Customer).Where(c => c.TenantId == tenantId);
        }

        public async Task<Guid> CreateAsync(CustomerCredit credit)
        {
            _dbContext.CustomerCredits.Add(credit);
            await _dbContext.SaveChangesAsync();
            return credit.Id;
        }

        public async Task<decimal> GetBalanceAsync(Guid tenantId, Guid customerId)
        {
            var credits = await _dbContext.CustomerCredits
                .Where(c => c.TenantId == tenantId && c.CustomerId == customerId && c.Type == "credit")
                .SumAsync(c => c.Amount);
            var debits = await _dbContext.CustomerCredits
                .Where(c => c.TenantId == tenantId && c.CustomerId == customerId && (c.Type == "debit" || c.Type == "adjustment"))
                .SumAsync(c => Math.Abs(c.Amount));
            return credits - debits;
        }

        public async Task<decimal> SumByTypeAsync(Guid tenantId, Guid customerId, string type)
        {
            return await _dbContext.CustomerCredits.Where(c => c.TenantId == tenantId && c.CustomerId == customerId && c.Type == type).SumAsync(c => c.Amount);
        }

        public async Task<decimal> TotalOutstandingAsync(Guid tenantId)
        {
            var credits = await _dbContext.CustomerCredits.Where(c => c.TenantId == tenantId && c.Type == "credit").SumAsync(c => c.Amount);
            var debits = await _dbContext.CustomerCredits.Where(c => c.TenantId == tenantId && (c.Type == "debit" || c.Type == "adjustment")).SumAsync(c => Math.Abs(c.Amount));
            return credits - debits;
        }

        public async Task<int> CountCustomersWithCreditsAsync(Guid tenantId)
        {
            return await _dbContext.CustomerCredits.Where(c => c.TenantId == tenantId).Select(c => c.CustomerId).Distinct().CountAsync();
        }
    }
}
