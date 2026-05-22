using Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Core.Queries
{
    public static class WebhookSubscriptionQuery
    {
        public static IQueryable<WebhookSubscription> GetWebhookSubscriptionQuery(this BillingDbContext dbContext)
        {
            return dbContext.WebhookSubscriptions
                .Include(x => x.Tenant)
                .Include(x => x.WebhookDeliveries);
        }

        public static IQueryable<WebhookSubscription> GetWebhookSubscriptionQueryAsNoTracking(this BillingDbContext dbContext)
        {
            return dbContext.GetWebhookSubscriptionQuery().AsNoTracking();
        }
    }
}
