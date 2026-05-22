using Core.ContextProviders;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories
{
    public class DunningRepository : BaseRepository, IDunningRepository
    {
        private readonly BillingDbContext _dbContext;

        public DunningRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
        }

        public async Task<DunningSchedule?> GetByIdAsync(Guid tenantId, Guid id)
        {
            return await _dbContext.DunningSchedules
                .Include(d => d.Customer)
                .Include(d => d.Subscription)
                .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == id);
        }

        public async Task<DunningSchedule?> GetBySubscriptionAsync(Guid tenantId, Guid subscriptionId)
        {
            return await _dbContext.DunningSchedules
                .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.SubscriptionId == subscriptionId && d.Status == "active");
        }

        public IQueryable<DunningSchedule> Query(Guid tenantId)
        {
            return _dbContext.DunningSchedules
                .Include(d => d.Customer)
                .Where(d => d.TenantId == tenantId);
        }

        public async Task<List<DunningSchedule>> GetDueSchedulesAsync()
        {
            return await _dbContext.DunningSchedules
                .Include(d => d.Customer)
                .Include(d => d.Subscription)
                .Where(d => d.Status == "active" && d.NextRetryAt != null && d.NextRetryAt <= DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<Guid> CreateAsync(DunningSchedule schedule)
        {
            _dbContext.DunningSchedules.Add(schedule);
            await _dbContext.SaveChangesAsync();
            return schedule.Id;
        }

        public async Task UpdateAsync(DunningSchedule schedule)
        {
            _dbContext.DunningSchedules.Update(schedule);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<DunningStep>> GetStepsAsync(Guid tenantId)
        {
            return await _dbContext.DunningSteps
                .Where(s => s.TenantId == tenantId && s.IsActive)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();
        }

        public async Task ReplaceStepsAsync(Guid tenantId, List<DunningStep> steps)
        {
            var existing = await _dbContext.DunningSteps.Where(s => s.TenantId == tenantId).ToListAsync();
            _dbContext.DunningSteps.RemoveRange(existing);
            _dbContext.DunningSteps.AddRange(steps);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> CountByStatusAsync(Guid tenantId, string status)
        {
            return await _dbContext.DunningSchedules.CountAsync(d => d.TenantId == tenantId && d.Status == status);
        }

        public async Task<decimal> SumAmountByStatusAsync(Guid tenantId, string status)
        {
            return await _dbContext.DunningSchedules
                .Where(d => d.TenantId == tenantId && d.Status == status)
                .SumAsync(d => d.AmountDue);
        }
    }
}
