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
    public class RefundService : BaseService, IRefundService
    {
        private readonly IRefundRepository _refundRepo;
        private readonly IPaymentTransactionRepository _paymentRepo;
        private readonly ICustomerRepository _customerRepo;

        public RefundService(
            ITenantContextProvider tenantContextProvider,
            IRefundRepository refundRepo,
            IPaymentTransactionRepository paymentRepo,
            ICustomerRepository customerRepo) : base(tenantContextProvider)
        {
            _refundRepo = refundRepo;
            _paymentRepo = paymentRepo;
            _customerRepo = customerRepo;
        }

        public async Task<GatewayResponseWrapper<RefundResponseDto>> CreateAsync(CreateRefundDto request)
        {
            var response = new GatewayResponseWrapper<RefundResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var transaction = await _paymentRepo.GetByIdAsync(request.TransactionId);
            if (transaction == null) { response.SetError("Payment transaction not found."); return response; }

            var refundAmount = request.Amount ?? transaction.Amount;
            if (refundAmount > transaction.Amount - transaction.AmountRefunded)
            {
                response.SetError("Refund amount exceeds refundable amount.");
                return response;
            }

            var refund = new Refund
            {
                TenantId = tenantId,
                TransactionId = transaction.Id,
                CustomerId = transaction.CustomerId,
                StripeRefundId = "",
                Amount = refundAmount,
                Currency = transaction.Currency,
                Reason = request.Reason ?? "requested_by_customer",
                Notes = request.Notes ?? "",
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            await _refundRepo.CreateAsync(refund);

            response.SetSuccess(MapToDto(refund, null), "Refund created and pending approval.");
            return response;
        }

        public async Task<GatewayResponseWrapper<RefundResponseDto>> GetAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<RefundResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var refund = await _refundRepo.Query(tenantId)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (refund == null) { response.SetError("Refund not found.", 404); return response; }

            response.SetSuccess(MapToDto(refund, refund.Customer));
            return response;
        }

        public async Task<GatewayPaginatedListResponseWrapper<RefundResponseDto>> ListAsync(RefundFilterDto filter)
        {
            var response = new GatewayPaginatedListResponseWrapper<RefundResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var query = _refundRepo.Query(tenantId)
                .Include(r => r.Customer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(r => r.Status == filter.Status);
            if (filter.DateFrom.HasValue)
                query = query.Where(r => r.CreatedAt >= filter.DateFrom);
            if (filter.DateTo.HasValue)
                query = query.Where(r => r.CreatedAt <= filter.DateTo);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            response.SetSuccessWithPagination(
                items.Select(r => MapToDto(r, r.Customer)).ToList(),
                totalCount, filter.Page, filter.PageSize);
            return response;
        }

        public async Task<GatewayResponseWrapper<RefundResponseDto>> ApproveAsync(Guid id, Guid approvedByUserId)
        {
            var response = new GatewayResponseWrapper<RefundResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var refund = await _refundRepo.Query(tenantId)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (refund == null) { response.SetError("Refund not found.", 404); return response; }
            if (refund.Status != "pending") { response.SetError("Refund is not in pending state."); return response; }

            refund.Status = "succeeded";
            refund.ApprovedBy = CurrentTenantContext.Role;
            refund.ApprovedAt = DateTime.UtcNow;
            refund.ProcessedAt = DateTime.UtcNow;
            await _refundRepo.UpdateAsync(refund);

            response.SetSuccess(MapToDto(refund, refund.Customer), "Refund approved.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> RejectAsync(Guid id, string reason)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;

            var refund = await _refundRepo.Query(tenantId).FirstOrDefaultAsync(r => r.Id == id);
            if (refund == null) { response.SetError("Refund not found.", 404); return response; }
            if (refund.Status != "pending") { response.SetError("Refund is not in pending state."); return response; }

            refund.Status = "rejected";
            refund.Notes = string.IsNullOrEmpty(refund.Notes) ? reason : $"{refund.Notes}\nRejected: {reason}";
            await _refundRepo.UpdateAsync(refund);

            response.SetSuccess(true, "Refund rejected.");
            return response;
        }

        public async Task<GatewayResponseWrapper<RefundStatsDto>> GetStatsAsync()
        {
            var response = new GatewayResponseWrapper<RefundStatsDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var refunds = await _refundRepo.Query(tenantId).ToListAsync();

            var stats = new RefundStatsDto
            {
                TotalRefunds = refunds.Count,
                TotalAmount = refunds.Where(r => r.Status == "succeeded").Sum(r => r.Amount),
                PendingCount = refunds.Count(r => r.Status == "pending"),
                RefundRate = 0,
                AvgProcessingTimeHours = (decimal)refunds
                    .Where(r => r.ProcessedAt.HasValue)
                    .Select(r => (r.ProcessedAt.Value - r.CreatedAt).TotalHours)
                    .DefaultIfEmpty(0)
                    .Average()
            };

            response.SetSuccess(stats);
            return response;
        }

        private static RefundResponseDto MapToDto(Refund r, Customer c)
        {
            return new RefundResponseDto
            {
                Id = r.Id,
                TransactionId = r.TransactionId,
                CustomerId = r.CustomerId,
                CustomerName = c?.Name ?? "",
                CustomerEmail = c?.Email ?? "",
                StripeRefundId = r.StripeRefundId ?? "",
                Amount = r.Amount,
                Currency = r.Currency ?? "usd",
                Reason = r.Reason ?? "",
                Notes = r.Notes ?? "",
                Status = r.Status ?? "",
                ApprovedBy = r.ApprovedBy ?? "",
                ApprovedAt = r.ApprovedAt,
                ProcessedAt = r.ProcessedAt,
                CreatedAt = r.CreatedAt
            };
        }
    }
}
