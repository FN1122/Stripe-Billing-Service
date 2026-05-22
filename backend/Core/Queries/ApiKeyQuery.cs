using Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Core.Queries
{
    public static class ApiKeyQuery
    {
        public static IQueryable<ApiKey> GetApiKeyQuery(this BillingDbContext dbContext)
        {
            return dbContext.ApiKeys
                .Include(x => x.Tenant);
        }

        public static IQueryable<ApiKey> GetApiKeyQueryAsNoTracking(this BillingDbContext dbContext)
        {
            return dbContext.GetApiKeyQuery().AsNoTracking();
        }
    }
}
