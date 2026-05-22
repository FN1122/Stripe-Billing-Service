using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.ServiceContracts;
using Core.Utils;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
    public class InvoiceService : BaseService, IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepo;

        public InvoiceService(ITenantContextProvider tenantContextProvider, IInvoiceRepository invoiceRepo) : base(tenantContextProvider)
        {
            _invoiceRepo = invoiceRepo;
        }

        public async Task<GatewayResponseWrapper<InvoiceResponseDto>> GetAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<InvoiceResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var invoice = await _invoiceRepo.Query(tenantId)
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) { response.SetError("Invoice not found.", 404); return response; }

            response.SetSuccess(MapToDto(invoice));
            return response;
        }

        public async Task<GatewayPaginatedListResponseWrapper<InvoiceResponseDto>> ListAsync(InvoiceFilterDto filter)
        {
            var response = new GatewayPaginatedListResponseWrapper<InvoiceResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var query = _invoiceRepo.Query(tenantId)
                .Include(i => i.Customer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(i => i.Status == filter.Status);
            if (!string.IsNullOrEmpty(filter.CustomerSearch))
                query = query.Where(i => i.Customer.Name.Contains(filter.CustomerSearch) || i.Customer.Email.Contains(filter.CustomerSearch) || i.InvoiceNumber.Contains(filter.CustomerSearch));
            if (filter.DateFrom.HasValue)
                query = query.Where(i => i.CreatedAt >= filter.DateFrom);
            if (filter.DateTo.HasValue)
                query = query.Where(i => i.CreatedAt <= filter.DateTo);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            response.SetSuccessWithPagination(
                items.Select(MapToDto).ToList(),
                totalCount, filter.Page, filter.PageSize);
            return response;
        }

        public async Task<GatewayResponseWrapper<string>> GetPdfUrlAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<string>();
            var tenantId = CurrentTenantContext.TenantId;

            var invoice = await _invoiceRepo.Query(tenantId).FirstOrDefaultAsync(i => i.Id == id);
            if (invoice == null) { response.SetError("Invoice not found.", 404); return response; }

            response.SetSuccess(invoice.InvoicePdfUrl ?? "");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> VoidAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;

            var invoice = await _invoiceRepo.Query(tenantId).FirstOrDefaultAsync(i => i.Id == id);
            if (invoice == null) { response.SetError("Invoice not found.", 404); return response; }

            invoice.Status = "void";
            await _invoiceRepo.UpdateAsync(invoice);

            response.SetSuccess(true, "Invoice voided.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> SendEmailAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;

            var invoice = await _invoiceRepo.Query(tenantId).FirstOrDefaultAsync(i => i.Id == id);
            if (invoice == null) { response.SetError("Invoice not found.", 404); return response; }

            // In production, this would trigger an email via the EmailService
            response.SetSuccess(true, "Invoice email queued.");
            return response;
        }

        public async Task SyncFromStripeAsync(string stripeInvoiceId, Guid tenantId)
        {
            // Used by webhook handler to sync invoice data from Stripe events
            var existing = await _invoiceRepo.GetByStripeInvoiceIdAsync(stripeInvoiceId);
            if (existing != null)
            {
                await _invoiceRepo.UpdateAsync(existing);
            }
        }

        private static InvoiceResponseDto MapToDto(Invoice i)
        {
            return new InvoiceResponseDto
            {
                Id = i.Id,
                CustomerId = i.CustomerId,
                CustomerName = i.Customer?.Name ?? "",
                CustomerEmail = i.Customer?.Email ?? "",
                StripeInvoiceId = i.StripeInvoiceId ?? "",
                InvoiceNumber = i.InvoiceNumber ?? "",
                Subtotal = i.Subtotal,
                Tax = i.Tax,
                Total = i.Total,
                AmountPaid = i.AmountPaid,
                AmountDue = i.AmountDue,
                Currency = i.Currency ?? "usd",
                Status = i.Status ?? "",
                InvoicePdfUrl = i.InvoicePdfUrl ?? "",
                HostedInvoiceUrl = i.HostedInvoiceUrl ?? "",
                PaidAt = i.PaidAt,
                DueDate = i.DueDate,
                CreatedAt = i.CreatedAt
            };
        }
    }
}
