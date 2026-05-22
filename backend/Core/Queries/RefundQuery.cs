using Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Core.Queries
{
    public static class RefundQuery
    {
        public static IQueryable<Refund> GetRefundQuery(this BillingDbContext dbContext)
        {
            return dbContext.Refunds
                .Include(x => x.Tenant)
                .Include(x => x.Transaction)
                .Include(x => x.Customer);
        }

        public static IQueryable<Refund> GetRefundQueryAsNoTracking(this BillingDbContext dbContext)
        {
            return dbContext.GetRefundQuery().AsNoTracking();
        }
    }
}
