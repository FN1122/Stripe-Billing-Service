using Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Core.Queries
{
    public static class WebhookDeliveryQuery
    {
        public static IQueryable<WebhookDelivery> GetWebhookDeliveryQuery(this BillingDbContext dbContext)
        {
            return dbContext.WebhookDeliveries
                .Include(x => x.WebhookSubscription);
        }

        public static IQueryable<WebhookDelivery> GetWebhookDeliveryQueryAsNoTracking(this BillingDbContext dbContext)
        {
            return dbContext.GetWebhookDeliveryQuery().AsNoTracking();
        }
    }
}
