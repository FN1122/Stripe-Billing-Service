using Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Core.Queries
{
    public static class SubscriptionPlanQuery
    {
        public static IQueryable<SubscriptionPlan> GetSubscriptionPlanQuery(this BillingDbContext dbContext)
        {
            return dbContext.SubscriptionPlans
                .Include(x => x.Tenant)
                .Include(x => x.Subscriptions);
        }

        public static IQueryable<SubscriptionPlan> GetSubscriptionPlanQueryAsNoTracking(this BillingDbContext dbContext)
        {
            return dbContext.GetSubscriptionPlanQuery().AsNoTracking();
        }
    }
}
