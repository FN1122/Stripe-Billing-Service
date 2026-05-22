using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface IDunningRepository
    {
        Task<DunningSchedule?> GetByIdAsync(Guid tenantId, Guid id);
        Task<DunningSchedule?> GetBySubscriptionAsync(Guid tenantId, Guid subscriptionId);
        IQueryable<DunningSchedule> Query(Guid tenantId);
        Task<List<DunningSchedule>> GetDueSchedulesAsync();
        Task<Guid> CreateAsync(DunningSchedule schedule);
        Task UpdateAsync(DunningSchedule schedule);
        Task<List<DunningStep>> GetStepsAsync(Guid tenantId);
        Task ReplaceStepsAsync(Guid tenantId, List<DunningStep> steps);
        Task<int> CountByStatusAsync(Guid tenantId, string status);
        Task<decimal> SumAmountByStatusAsync(Guid tenantId, string status);
    }
}
