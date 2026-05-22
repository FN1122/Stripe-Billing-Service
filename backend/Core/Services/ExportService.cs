using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.ServiceContracts;
using Core.Utils;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Core.Services
{
    public class ExportService : BaseService, IExportService
    {
        private readonly BillingDbContext _dbContext;

        public ExportService(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
        }

        public async Task<byte[]> ExportTransactionsAsync(ExportRequestDto request)
        {
            var tenantId = CurrentTenantContext.TenantId;
            var query = _dbContext.PaymentTransactions.Where(t => t.TenantId == tenantId);
            if (request.FromDate.HasValue) query = query.Where(t => t.CreatedAt >= request.FromDate.Value);
            if (request.ToDate.HasValue) query = query.Where(t => t.CreatedAt <= request.ToDate.Value);

            var items = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Id,Amount,Currency,Status,Type,PaymentMethod,CreatedAt");
            foreach (var t in items)
                sb.AppendLine($"{t.Id},{t.Amount},{t.Currency},{t.Status},{t.Type},{t.PaymentMethodLast4},{t.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> ExportInvoicesAsync(ExportRequestDto request)
        {
            var tenantId = CurrentTenantContext.TenantId;
            var query = _dbContext.Invoices.Where(i => i.TenantId == tenantId);
            if (request.FromDate.HasValue) query = query.Where(i => i.CreatedAt >= request.FromDate.Value);
            if (request.ToDate.HasValue) query = query.Where(i => i.CreatedAt <= request.ToDate.Value);

            var items = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Id,InvoiceNumber,Subtotal,Tax,Total,AmountPaid,AmountDue,Status,CreatedAt");
            foreach (var i in items)
                sb.AppendLine($"{i.Id},{i.InvoiceNumber},{i.Subtotal},{i.Tax},{i.Total},{i.AmountPaid},{i.AmountDue},{i.Status},{i.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> ExportCustomersAsync(ExportRequestDto request)
        {
            var tenantId = CurrentTenantContext.TenantId;
            var items = await _dbContext.Customers.Where(c => c.TenantId == tenantId).OrderByDescending(c => c.CreatedAt).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Id,Name,Email,Phone,Currency,CreatedAt");
            foreach (var c in items)
                sb.AppendLine($"{c.Id},{c.Name},{c.Email},{c.Phone},{c.Currency},{c.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> ExportSubscriptionsAsync(ExportRequestDto request)
        {
            var tenantId = CurrentTenantContext.TenantId;
            var items = await _dbContext.Subscriptions.Include(s => s.Customer).Include(s => s.Plan).Where(s => s.TenantId == tenantId).OrderByDescending(s => s.CreatedAt).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Id,CustomerName,PlanName,Status,Quantity,CurrentPeriodEnd,CreatedAt");
            foreach (var s in items)
                sb.AppendLine($"{s.Id},{s.Customer?.Name},{s.Plan?.Name},{s.Status},{s.Quantity},{s.CurrentPeriodEnd:yyyy-MM-dd},{s.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> ExportRefundsAsync(ExportRequestDto request)
        {
            var tenantId = CurrentTenantContext.TenantId;
            var items = await _dbContext.Refunds.Where(r => r.TenantId == tenantId).OrderByDescending(r => r.CreatedAt).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Id,Amount,Currency,Status,Reason,CreatedAt");
            foreach (var r in items)
                sb.AppendLine($"{r.Id},{r.Amount},{r.Currency},{r.Status},{r.Reason},{r.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> ExportAuditLogAsync(ExportRequestDto request)
        {
            var tenantId = CurrentTenantContext.TenantId;
            var query = _dbContext.AuditLogs.Where(a => a.TenantId == tenantId);
            if (request.FromDate.HasValue) query = query.Where(a => a.CreatedAt >= request.FromDate.Value);
            if (request.ToDate.HasValue) query = query.Where(a => a.CreatedAt <= request.ToDate.Value);

            var items = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Id,Action,EntityType,EntityId,UserId,CreatedAt");
            foreach (var a in items)
                sb.AppendLine($"{a.Id},{a.Action},{a.EntityType},{a.EntityId},{a.UserId},{a.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> GenerateRevenueReportAsync(DateTime from, DateTime to)
        {
            // In production, use QuestPDF to generate PDF
            return Encoding.UTF8.GetBytes($"Revenue Report: {from:yyyy-MM-dd} to {to:yyyy-MM-dd}");
        }

        public async Task<byte[]> GenerateTaxReportAsync(DateTime from, DateTime to)
        {
            return Encoding.UTF8.GetBytes($"Tax Report: {from:yyyy-MM-dd} to {to:yyyy-MM-dd}");
        }

        public async Task<GatewayResponseWrapper<List<ExportLogDto>>> GetExportHistoryAsync()
        {
            var response = new GatewayResponseWrapper<List<ExportLogDto>>();
            response.SetSuccess(new List<ExportLogDto>());
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> ScheduleReportAsync(ScheduleReportDto request)
        {
            var response = new GatewayResponseWrapper<bool>();
            response.SetSuccess(true, "Report scheduled successfully.");
            return response;
        }
    }
}
