using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface IUsageRepository
    {
        Task<UsageRecord?> GetByIdAsync(Guid tenantId, Guid id);
        Task<UsageRecord?> GetByIdempotencyKeyAsync(Guid tenantId, string key);
        IQueryable<UsageRecord> Query(Guid tenantId);
        Task<Guid> CreateAsync(UsageRecord record);
        Task CreateRangeAsync(List<UsageRecord> records);
        Task<long> SumUsageAsync(Guid tenantId, Guid subscriptionId, DateTime from, DateTime to);
        Task<MeterEvent?> GetMeterEventByIdAsync(Guid tenantId, Guid id);
        IQueryable<MeterEvent> QueryMeterEvents(Guid tenantId);
        Task<Guid> CreateMeterEventAsync(MeterEvent meterEvent);
    }
}
