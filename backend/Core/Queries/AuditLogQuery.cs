using Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Core.Queries
{
    public static class AuditLogQuery
    {
        public static IQueryable<AuditLog> GetAuditLogQuery(this BillingDbContext dbContext)
        {
            return dbContext.AuditLogs;
        }

        public static IQueryable<AuditLog> GetAuditLogQueryAsNoTracking(this BillingDbContext dbContext)
        {
            return dbContext.GetAuditLogQuery().AsNoTracking();
        }
    }
}
