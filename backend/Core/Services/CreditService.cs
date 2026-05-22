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
    public class CreditService : BaseService, ICreditService
    {
        private readonly ICreditRepository _creditRepo;

        public CreditService(ITenantContextProvider tenantContextProvider, ICreditRepository creditRepo) : base(tenantContextProvider)
        {
            _creditRepo = creditRepo;
        }

        public async Task<GatewayResponseWrapper<CustomerBalanceDto>> GetBalanceAsync(Guid customerId)
        {
            var response = new GatewayResponseWrapper<CustomerBalanceDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var balance = await _creditRepo.GetBalanceAsync(tenantId, customerId);
            var totalCredits = await _creditRepo.SumByTypeAsync(tenantId, customerId, "credit");
            var totalDebits = await _creditRepo.SumByTypeAsync(tenantId, customerId, "debit");

            var recentTxns = await _creditRepo.Query(tenantId, customerId)
                .OrderByDescending(c => c.CreatedAt).Take(10).ToListAsync();

            var dto = new CustomerBalanceDto
            {
                CustomerId = customerId,
                CurrentBalance = balance,
                TotalCredits = totalCredits,
                TotalDebits = Math.Abs(totalDebits),
                RecentTransactions = recentTxns.Select(MapCredit).ToList()
            };

            response.SetSuccess(dto);
            return response;
        }

        public async Task<GatewayResponseWrapper<CreditResponseDto>> AddCreditAsync(Guid customerId, CreateCreditDto request)
        {
            var response = new GatewayResponseWrapper<CreditResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var currentBalance = await _creditRepo.GetBalanceAsync(tenantId, customerId);
            var newBalance = currentBalance + request.Amount;

            var credit = new CustomerCredit
            {
                TenantId = tenantId,
                CustomerId = customerId,
                Type = "credit",
                Amount = request.Amount,
                Currency = request.Currency,
                Description = request.Description,
                Source = request.Source,
                BalanceAfter = newBalance,
                CreatedBy = CurrentTenantContext.UserId
            };

            await _creditRepo.CreateAsync(credit);
            response.SetSuccess(MapCredit(credit));
            return response;
        }

        public async Task<GatewayResponseWrapper<CreditResponseDto>> AdjustBalanceAsync(Guid customerId, AdjustCreditDto request)
        {
            var response = new GatewayResponseWrapper<CreditResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var currentBalance = await _creditRepo.GetBalanceAsync(tenantId, customerId);
            var newBalance = currentBalance + request.Amount;

            var credit = new CustomerCredit
            {
                TenantId = tenantId,
                CustomerId = customerId,
                Type = request.Amount >= 0 ? "credit" : "debit",
                Amount = request.Amount,
                Currency = "usd",
                Description = request.Description,
                Source = "manual",
                BalanceAfter = newBalance,
                CreatedBy = CurrentTenantContext.UserId
            };

            await _creditRepo.CreateAsync(credit);
            response.SetSuccess(MapCredit(credit));
            return response;
        }

        public async Task<GatewayPaginatedListResponseWrapper<CreditResponseDto>> GetHistoryAsync(Guid customerId, int page, int pageSize)
        {
            var response = new GatewayPaginatedListResponseWrapper<CreditResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var query = _creditRepo.Query(tenantId, customerId);

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            response.SetSuccessWithPagination(items.Select(MapCredit).ToList(), totalCount, page, pageSize);
            return response;
        }

        public async Task<GatewayResponseWrapper<CreditResponseDto>> RefundToCreditAsync(RefundToCreditDto request)
        {
            var response = new GatewayResponseWrapper<CreditResponseDto>();
            // In production, look up refund, convert to credit
            response.SetError("Refund-to-credit conversion requires refund lookup implementation.", 501);
            return response;
        }

        public async Task<GatewayResponseWrapper<CreditsDashboardDto>> GetDashboardAsync()
        {
            var response = new GatewayResponseWrapper<CreditsDashboardDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var allCredits = _creditRepo.QueryAll(tenantId);
            var totalIssued = await allCredits.Where(c => c.Type == "credit").SumAsync(c => c.Amount);
            var totalUsed = await allCredits.Where(c => c.Type == "debit" || c.Type == "adjustment").SumAsync(c => Math.Abs(c.Amount));

            var dashboard = new CreditsDashboardDto
            {
                TotalOutstandingCredits = await _creditRepo.TotalOutstandingAsync(tenantId),
                CustomersWithCredits = await _creditRepo.CountCustomersWithCreditsAsync(tenantId),
                TotalCreditsIssued = totalIssued,
                TotalCreditsUsed = totalUsed,
            };

            response.SetSuccess(dashboard);
            return response;
        }

        public async Task<GatewayPaginatedListResponseWrapper<CreditResponseDto>> GetRecentTransactionsAsync(int page, int pageSize)
        {
            var response = new GatewayPaginatedListResponseWrapper<CreditResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var query = _creditRepo.QueryAll(tenantId);

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            response.SetSuccessWithPagination(items.Select(MapCredit).ToList(), totalCount, page, pageSize);
            return response;
        }

        private CreditResponseDto MapCredit(CustomerCredit c) => new()
        {
            Id = c.Id,
            CustomerId = c.CustomerId,
            CustomerName = c.Customer?.Name,
            CustomerEmail = c.Customer?.Email,
            Type = c.Type,
            Amount = c.Amount,
            Currency = c.Currency,
            Description = c.Description,
            Source = c.Source,
            ReferenceId = c.ReferenceId,
            BalanceAfter = c.BalanceAfter,
            CreatedAt = c.CreatedAt
        };
    }
}
