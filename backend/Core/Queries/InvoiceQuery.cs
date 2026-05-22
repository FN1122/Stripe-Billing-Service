using Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Core.Queries
{
    public static class InvoiceQuery
    {
        public static IQueryable<Invoice> GetInvoiceQuery(this BillingDbContext dbContext)
        {
            return dbContext.Invoices
                .Include(x => x.Tenant)
                .Include(x => x.Customer);
        }

        public static IQueryable<Invoice> GetInvoiceQueryAsNoTracking(this BillingDbContext dbContext)
        {
            return dbContext.GetInvoiceQuery().AsNoTracking();
        }
    }
}
