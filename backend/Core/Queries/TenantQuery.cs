using Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Core.Queries
{
    public static class TenantQuery
    {
        public static IQueryable<Tenant> GetTenantQuery(this BillingDbContext dbContext)
        {
            return dbContext.Tenants
                .Include(x => x.Users)
                .Include(x => x.ApiKeys)
                .Include(x => x.Customers)
                .Include(x => x.WebhookSubscriptions);
        }

        public static IQueryable<Tenant> GetTenantQueryAsNoTracking(this BillingDbContext dbContext)
        {
            return dbContext.GetTenantQuery().AsNoTracking();
        }
    }
}
