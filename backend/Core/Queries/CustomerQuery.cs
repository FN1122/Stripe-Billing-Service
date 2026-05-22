using Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Core.Queries
{
    public static class CustomerQuery
    {
        public static IQueryable<Customer> GetCustomerQuery(this BillingDbContext dbContext)
        {
            return dbContext.Customers
                .Include(x => x.Tenant)
                .Include(x => x.Subscriptions).ThenInclude(s => s.Plan)
                .Include(x => x.Transactions)
                .Include(x => x.Invoices);
        }

        public static IQueryable<Customer> GetCustomerQueryAsNoTracking(this BillingDbContext dbContext)
        {
            return dbContext.GetCustomerQuery().AsNoTracking();
        }
    }
}
