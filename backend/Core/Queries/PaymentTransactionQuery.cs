using Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Core.Queries
{
    public static class PaymentTransactionQuery
    {
        public static IQueryable<PaymentTransaction> GetPaymentTransactionQuery(this BillingDbContext dbContext)
        {
            return dbContext.PaymentTransactions
                .Include(x => x.Tenant)
                .Include(x => x.Customer)
                .Include(x => x.Refunds);
        }

        public static IQueryable<PaymentTransaction> GetPaymentTransactionQueryAsNoTracking(this BillingDbContext dbContext)
        {
            return dbContext.GetPaymentTransactionQuery().AsNoTracking();
        }
    }
}
