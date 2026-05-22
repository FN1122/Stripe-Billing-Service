using Core.ContextProviders;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories
{
    public class UsageRepository : BaseRepository, IUsageRepository
    {
        private readonly BillingDbContext _dbContext;

        public UsageRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
        }

        public async Task<UsageRecord?> GetByIdAsync(Guid tenantId, Guid id)
        {
            return await _dbContext.UsageRecords.FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Id == id);
        }

        public async Task<UsageRecord?> GetByIdempotencyKeyAsync(Guid tenantId, string key)
        {
            return await _dbContext.UsageRecords.FirstOrDefaultAsync(u => u.TenantId == tenantId && u.IdempotencyKey == key);
        }

        public IQueryable<UsageRecord> Query(Guid tenantId)
        {
            return _dbContext.UsageRecords.Include(u => u.Subscription).ThenInclude(s => s.Customer).Where(u => u.TenantId == tenantId);
        }

        public async Task<Guid> CreateAsync(UsageRecord record)
        {
            _dbContext.UsageRecords.Add(record);
            await _dbContext.SaveChangesAsync();
            return record.Id;
        }

        public async Task CreateRangeAsync(List<UsageRecord> records)
        {
            _dbContext.UsageRecords.AddRange(records);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<long> SumUsageAsync(Guid tenantId, Guid subscriptionId, DateTime from, DateTime to)
        {
            return await _dbContext.UsageRecords
                .Where(u => u.TenantId == tenantId && u.SubscriptionId == subscriptionId && u.Timestamp >= from && u.Timestamp <= to)
                .SumAsync(u => u.Quantity);
        }

        public async Task<MeterEvent?> GetMeterEventByIdAsync(Guid tenantId, Guid id)
        {
            return await _dbContext.MeterEvents.FirstOrDefaultAsync(m => m.TenantId == tenantId && m.Id == id);
        }

        public IQueryable<MeterEvent> QueryMeterEvents(Guid tenantId)
        {
            return _dbContext.MeterEvents.Include(m => m.Customer).Where(m => m.TenantId == tenantId);
        }

        public async Task<Guid> CreateMeterEventAsync(MeterEvent meterEvent)
        {
            _dbContext.MeterEvents.Add(meterEvent);
            await _dbContext.SaveChangesAsync();
            return meterEvent.Id;
        }
    }
}
