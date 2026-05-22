using Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Core.Queries
{
    public static class SubscriptionQuery
    {
        public static IQueryable<Subscription> GetSubscriptionQuery(this BillingDbContext dbContext)
        {
            return dbContext.Subscriptions
                .Include(x => x.Tenant)
                .Include(x => x.Customer)
                .Include(x => x.Plan);
        }

        public static IQueryable<Subscription> GetSubscriptionQueryAsNoTracking(this BillingDbContext dbContext)
        {
            return dbContext.GetSubscriptionQuery().AsNoTracking();
        }
    }
}
