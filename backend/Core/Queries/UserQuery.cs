using Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Core.Queries
{
    public static class UserQuery
    {
        public static IQueryable<User> GetUserQuery(this BillingDbContext dbContext)
        {
            return dbContext.Users
                .Include(x => x.Tenant)
                .Include(x => x.RefreshTokens);
        }

        public static IQueryable<User> GetUserQueryAsNoTracking(this BillingDbContext dbContext)
        {
            return dbContext.GetUserQuery().AsNoTracking();
        }
    }
}
