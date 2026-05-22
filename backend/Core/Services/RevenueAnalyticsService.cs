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
    public class RevenueAnalyticsService : BaseService, IRevenueAnalyticsService
    {
        private readonly ISubscriptionRepository _subscriptionRepo;
        private readonly IPaymentTransactionRepository _transactionRepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly IRefundRepository _refundRepo;

        public RevenueAnalyticsService(ITenantContextProvider tcp, ISubscriptionRepository subscriptionRepo, IPaymentTransactionRepository transactionRepo, ICustomerRepository customerRepo, IRefundRepository refundRepo) : base(tcp)
        {
            _subscriptionRepo = subscriptionRepo;
            _transactionRepo = transactionRepo;
            _customerRepo = customerRepo;
            _refundRepo = refundRepo;
        }

        public async Task<GatewayResponseWrapper<MrrDto>> GetMrrAsync()
        {
            var response = new GatewayResponseWrapper<MrrDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var activeSubscriptions = await _subscriptionRepo.Query(tenantId)
                .Where(s => s.Status == "active")
                .ToListAsync();

            var mrrByPlan = activeSubscriptions
                .GroupBy(s => s.PlanId)
                .Select(g => new MrrBreakdownDto
                {
                    PlanId = g.Key,
                    PlanName = g.FirstOrDefault()?.Plan?.Name,
                    SubscriberCount = g.Count(),
                    Mrr = g.Sum(s => (s.Plan?.Amount ?? 0) * s.Quantity)
                }).ToList();

            var totalMrr = mrrByPlan.Sum(m => m.Mrr);
            var previousMonthMrr = await CalculatePreviousMonthMrr();
            var growth = previousMonthMrr > 0 ? Math.Round(((totalMrr - previousMonthMrr) / previousMonthMrr) * 100, 1) : 0;

            response.SetSuccess(new MrrDto
            {
                CurrentMrr = totalMrr,
                PreviousMonthMrr = previousMonthMrr,
                MrrGrowth = growth,
                MrrGrowthPercentage = previousMonthMrr > 0 ? growth : 0,
                ActiveSubscriptions = activeSubscriptions.Count,
                ByPlan = mrrByPlan,
                CalculatedAt = DateTime.UtcNow
            });
            return response;
        }

        public async Task<GatewayResponseWrapper<ChurnRateDto>> GetChurnRateAsync(string period = "30d")
        {
            var response = new GatewayResponseWrapper<ChurnRateDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var days = period switch { "7d" => 7, "90d" => 90, "12m" => 365, _ => 30 };
            var since = DateTime.UtcNow.AddDays(-days);

            var cancelledSubs = await _subscriptionRepo.Query(tenantId)
                .Where(s => s.Status == "canceled" && s.CancelledAt >= since)
                .ToListAsync();

            var totalActiveSubs = await _subscriptionRepo.CountActiveByTenantIdAsync(tenantId);

            var churnRate = totalActiveSubs > 0
                ? Math.Round((decimal)cancelledSubs.Count / (totalActiveSubs + cancelledSubs.Count) * 100, 1) : 0;

            var cancellationReasons = cancelledSubs
                .GroupBy(s => s.CancellationReason ?? "unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            response.SetSuccess(new ChurnRateDto
            {
                Period = period, ChurnRate = churnRate, CancelledCount = cancelledSubs.Count,
                RetainedCount = totalActiveSubs, CancellationReasons = cancellationReasons,
                ProjectedMonthlyChurn = (totalActiveSubs + cancelledSubs.Count) > 0
                    ? Math.Round(churnRate * (totalActiveSubs + cancelledSubs.Count) / 100, 0) : 0,
                CalculatedAt = DateTime.UtcNow
            });
            return response;
        }

        public async Task<GatewayResponseWrapper<LtvDto>> GetLtvAsync()
        {
            var response = new GatewayResponseWrapper<LtvDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var customers = await _customerRepo.GetByTenantIdWithDetailsAsync(tenantId);

            var customerLtvs = customers.Select(c =>
            {
                var subscriptions = c.Subscriptions?.Where(s => s.Status == "active").ToList() ?? new();
                var avgMonthlySubscriptionValue = subscriptions.Count > 0
                    ? subscriptions.Sum(s => s.Plan?.Amount ?? 0) / subscriptions.Count : 0m;
                var avgTransactionValue = c.Transactions?.Count > 0
                    ? c.Transactions.Where(t => t.Status == "succeeded").Sum(t => t.Amount) / c.Transactions.Count : 0m;
                var tenure = c.CreatedAt > DateTime.MinValue ? Math.Round((DateTime.UtcNow - c.CreatedAt).TotalDays / 30.44, 1) : 0;
                var monthlyValue = avgMonthlySubscriptionValue + avgTransactionValue;
                var ltv = monthlyValue * 36;
                return new { Customer = c, LTV = ltv, MonthlyValue = monthlyValue, Tenure = tenure };
            }).ToList();

            var totalLtv = customerLtvs.Sum(x => x.LTV);
            var avgLtv = customers.Count > 0 ? totalLtv / customers.Count : 0;
            var medianLtv = customers.Count > 0
                ? customerLtvs.OrderBy(x => x.LTV).Skip(customers.Count / 2).FirstOrDefault()?.LTV ?? 0 : 0;

            response.SetSuccess(new LtvDto
            {
                AverageLtv = Math.Round(avgLtv, 2), MedianLtv = Math.Round(medianLtv, 2),
                TotalLtv = Math.Round(totalLtv, 2), CustomerCount = customers.Count,
                HighValueCustomers = customerLtvs.Where(x => x.LTV > avgLtv * 1.5m).Select(x => new HighValueCustomerDto
                {
                    CustomerId = x.Customer.Id, CustomerName = x.Customer.Name, CustomerEmail = x.Customer.Email,
                    Ltv = Math.Round(x.LTV, 2), MonthlyValue = Math.Round(x.MonthlyValue, 2), Tenure = x.Tenure
                }).OrderByDescending(x => x.Ltv).Take(10).ToList(),
                CalculatedAt = DateTime.UtcNow
            });
            return response;
        }

        public async Task<GatewayResponseWrapper<RevenueMetricsDto>> GetRevenueMetricsAsync(string period = "30d")
        {
            var response = new GatewayResponseWrapper<RevenueMetricsDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var days = period switch { "7d" => 7, "90d" => 90, "12m" => 365, _ => 30 };
            var since = DateTime.UtcNow.AddDays(-days);
            var transactions = await _transactionRepo.GetByTenantIdSinceAsync(tenantId, since);

            var succeeded = transactions.Where(t => t.Status == "succeeded").ToList();
            var refunded = transactions.Where(t => t.Status == "refunded").ToList();
            var failed = transactions.Where(t => t.Status == "failed").ToList();

            var dailyRevenue = succeeded
                .GroupBy(t => t.CreatedAt.Date).OrderBy(g => g.Key)
                .Select(g => new RevenueDataPoint { Date = g.Key.ToString("yyyy-MM-dd"), Amount = g.Sum(t => t.Amount), Count = g.Count() })
                .ToList();

            response.SetSuccess(new RevenueMetricsDto
            {
                Period = period, TotalRevenue = succeeded.Sum(t => t.Amount), TotalRefunded = refunded.Sum(t => t.Amount),
                NetRevenue = succeeded.Sum(t => t.Amount) - refunded.Sum(t => t.Amount),
                TransactionCount = transactions.Count, SuccessCount = succeeded.Count,
                FailedCount = failed.Count, RefundedCount = refunded.Count,
                SuccessRate = transactions.Count > 0 ? Math.Round((decimal)succeeded.Count / transactions.Count * 100, 1) : 0,
                RefundRate = succeeded.Count > 0 ? Math.Round((decimal)refunded.Count / succeeded.Count * 100, 1) : 0,
                AverageTransactionValue = succeeded.Count > 0 ? Math.Round(succeeded.Average(t => t.Amount), 2) : 0,
                AverageRefundAmount = refunded.Count > 0 ? Math.Round(refunded.Average(t => t.Amount), 2) : 0,
                DailyRevenue = dailyRevenue, CalculatedAt = DateTime.UtcNow
            });
            return response;
        }

        public async Task<GatewayResponseWrapper<DashboardStatsDto>> GetDashboardStatsAsync()
        {
            var response = new GatewayResponseWrapper<DashboardStatsDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var last24h = DateTime.UtcNow.AddDays(-1);
            var last30d = DateTime.UtcNow.AddDays(-30);

            var customers = await _customerRepo.CountByTenantIdAsync(tenantId);
            var activeSubscriptions = await _subscriptionRepo.CountActiveByTenantIdAsync(tenantId);
            var recentTransactions = await _transactionRepo.GetByTenantIdSinceAsync(tenantId, last24h);
            var recentRevenue = recentTransactions.Where(t => t.Status == "succeeded").Sum(t => t.Amount);
            var monthRevenue = await _transactionRepo.SumSucceededByTenantIdSinceAsync(tenantId, last30d);
            var newCustomers24h = await _customerRepo.CountByTenantIdSinceAsync(tenantId, last24h);
            var newSubscriptions24h = await _subscriptionRepo.CountByTenantIdSinceAsync(tenantId, last24h, "active");
            var churnRate = await GetChurnRateAsync("30d");

            response.SetSuccess(new DashboardStatsDto
            {
                TotalCustomers = customers, ActiveSubscriptions = activeSubscriptions,
                Revenue24h = recentRevenue, Revenue30d = monthRevenue,
                NewCustomers24h = newCustomers24h, NewSubscriptions24h = newSubscriptions24h,
                TransactionSuccess24h = recentTransactions.Count > 0
                    ? Math.Round((decimal)recentTransactions.Count(t => t.Status == "succeeded") / recentTransactions.Count * 100, 1) : 0,
                ChurnRate30d = churnRate.Data?.ChurnRate ?? 0, GeneratedAt = DateTime.UtcNow
            });
            return response;
        }

        public async Task<GatewayResponseWrapper<List<ActivityFeedItemDto>>> GetActivityFeedAsync(int limit = 50)
        {
            var response = new GatewayResponseWrapper<List<ActivityFeedItemDto>>();
            var tenantId = CurrentTenantContext.TenantId;
            var activities = new List<ActivityFeedItemDto>();

            var recentTxns = await _transactionRepo.Query(tenantId)
                .OrderByDescending(t => t.CreatedAt).Take(limit / 3).ToListAsync();
            foreach (var txn in recentTxns)
                activities.Add(new ActivityFeedItemDto { Type = "payment", Title = $"Payment {(txn.Status == "succeeded" ? "succeeded" : "failed")}", Description = $"{txn.Customer?.Name} - {txn.Amount} {txn.Currency}", Status = txn.Status, Timestamp = txn.CreatedAt, Metadata = new { transactionId = txn.Id, customerId = txn.CustomerId } });

            var recentSubs = await _subscriptionRepo.Query(tenantId)
                .OrderByDescending(s => s.CreatedAt).Take(limit / 3).ToListAsync();
            foreach (var sub in recentSubs)
                activities.Add(new ActivityFeedItemDto { Type = "subscription", Title = $"Subscription {sub.Status}", Description = $"{sub.Customer?.Name} - {sub.Plan?.Name}", Status = sub.Status, Timestamp = sub.CreatedAt, Metadata = new { subscriptionId = sub.Id, customerId = sub.CustomerId, planId = sub.PlanId } });

            var recentRefunds = await _refundRepo.Query(tenantId)
                .OrderByDescending(r => r.CreatedAt).Take(limit / 3).ToListAsync();
            foreach (var refund in recentRefunds)
                activities.Add(new ActivityFeedItemDto { Type = "refund", Title = $"Refund {refund.Status}", Description = $"{refund.Customer?.Name} - {refund.Amount} {refund.Currency}", Status = refund.Status, Timestamp = refund.CreatedAt, Metadata = new { refundId = refund.Id, customerId = refund.CustomerId } });

            response.SetSuccess(activities.OrderByDescending(a => a.Timestamp).Take(limit).ToList());
            return response;
        }

        private async Task<decimal> CalculatePreviousMonthMrr()
        {
            var tenantId = CurrentTenantContext.TenantId;
            var firstDayThisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var firstDayLastMonth = firstDayThisMonth.AddMonths(-1);

            return await _subscriptionRepo.Query(tenantId)
                .Where(s => s.CreatedAt < firstDayLastMonth.AddMonths(1) && (s.CancelledAt == null || s.CancelledAt >= firstDayLastMonth))
                .SumAsync(s => (s.Plan.Amount) * s.Quantity);
        }
    }
}
