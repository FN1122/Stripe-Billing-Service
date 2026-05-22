using Core.ContextProviders;
using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.ServiceContracts;
using Core.Utils;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
    public class DashboardService : BaseService, IDashboardService
    {
        private readonly ICustomerRepository _customerRepo;
        private readonly ISubscriptionRepository _subscriptionRepo;
        private readonly IPaymentTransactionRepository _transactionRepo;
        private readonly IRefundRepository _refundRepo;
        private readonly IRevenueAnalyticsService _revenueAnalytics;

        public DashboardService(ITenantContextProvider tcp, ICustomerRepository customerRepo, ISubscriptionRepository subscriptionRepo, IPaymentTransactionRepository transactionRepo, IRefundRepository refundRepo, IRevenueAnalyticsService revenueAnalytics) : base(tcp)
        {
            _customerRepo = customerRepo;
            _subscriptionRepo = subscriptionRepo;
            _transactionRepo = transactionRepo;
            _refundRepo = refundRepo;
            _revenueAnalytics = revenueAnalytics;
        }

        public async Task<GatewayResponseWrapper<ComprehensiveDashboardDto>> GetComprehensiveDashboardAsync()
        {
            var response = new GatewayResponseWrapper<ComprehensiveDashboardDto>();
            var dashStats = await GetDashboardStatsAsync();
            var revenueMetrics = await _revenueAnalytics.GetRevenueMetricsAsync("30d");
            var mrrData = await _revenueAnalytics.GetMrrAsync();
            var churnRate = await _revenueAnalytics.GetChurnRateAsync("30d");
            var activityFeed = await _revenueAnalytics.GetActivityFeedAsync(20);
            response.SetSuccess(new ComprehensiveDashboardDto { Stats = dashStats.Data, RevenueMetrics = revenueMetrics.Data, Mrr = mrrData.Data, ChurnRate = churnRate.Data, ActivityFeed = activityFeed.Data, GeneratedAt = DateTime.UtcNow });
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
            var churnRate = await _revenueAnalytics.GetChurnRateAsync("30d");

            var statsDto = new DashboardStatsDto
            {
                TotalCustomers = customers,
                ActiveSubscriptions = activeSubscriptions,
                Revenue24h = recentRevenue,
                Revenue30d = monthRevenue,
                NewCustomers24h = newCustomers24h,
                NewSubscriptions24h = newSubscriptions24h,
                TransactionSuccess24h = recentTransactions.Count > 0
                    ? Math.Round((decimal)recentTransactions.Count(t => t.Status == "succeeded") / recentTransactions.Count * 100, 1)
                    : 0,
                ChurnRate30d = churnRate.Data?.ChurnRate ?? 0,
                GeneratedAt = DateTime.UtcNow
            };

            response.SetSuccess(statsDto);
            return response;
        }

        public async Task<GatewayResponseWrapper<PaymentsDashboardDto>> GetPaymentsDashboardAsync()
        {
            var response = new GatewayResponseWrapper<PaymentsDashboardDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var last30d = DateTime.UtcNow.AddDays(-30);
            var transactions = await _transactionRepo.GetByTenantIdSinceAsync(tenantId, last30d);

            var succeeded = transactions.Where(t => t.Status == "succeeded").ToList();
            var failed = transactions.Where(t => t.Status == "failed").ToList();
            var refunded = transactions.Where(t => t.Status == "refunded").ToList();

            var dailyRevenue = succeeded
                .GroupBy(t => t.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new RevenueDataPoint { Date = g.Key.ToString("yyyy-MM-dd"), Amount = g.Sum(t => t.Amount), Count = g.Count() })
                .ToList();

            var dashboard = new PaymentsDashboardDto
            {
                TotalRevenue = succeeded.Sum(t => t.Amount),
                TotalRefunded = refunded.Sum(t => t.Amount),
                NetRevenue = succeeded.Sum(t => t.Amount) - refunded.Sum(t => t.Amount),
                TransactionCount = transactions.Count,
                SuccessCount = succeeded.Count,
                FailedCount = failed.Count,
                RefundedCount = refunded.Count,
                SuccessRate = transactions.Count > 0 ? Math.Round((decimal)succeeded.Count / transactions.Count * 100, 1) : 0,
                RefundRate = succeeded.Count > 0 ? Math.Round((decimal)refunded.Count / succeeded.Count * 100, 1) : 0,
                AverageTransactionValue = succeeded.Count > 0 ? Math.Round(succeeded.Average(t => t.Amount), 2) : 0,
                TopPaymentMethods = transactions.Where(t => !string.IsNullOrEmpty(t.PaymentMethod))
                    .GroupBy(t => t.PaymentMethod).OrderByDescending(g => g.Count()).Take(5)
                    .ToDictionary(g => g.Key, g => g.Count()),
                DailyRevenue = dailyRevenue,
                GeneratedAt = DateTime.UtcNow
            };

            response.SetSuccess(dashboard);
            return response;
        }

        public async Task<GatewayResponseWrapper<SubscriptionsDashboardDto>> GetSubscriptionsDashboardAsync()
        {
            var response = new GatewayResponseWrapper<SubscriptionsDashboardDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var last30d = DateTime.UtcNow.AddDays(-30);
            var subscriptions = await _subscriptionRepo.GetByTenantIdWithPlanAsync(tenantId);

            var activeSubscriptions = subscriptions.Where(s => s.Status == "active").ToList();
            var trialing = subscriptions.Where(s => s.Status == "trialing").ToList();
            var cancelled = subscriptions.Where(s => s.Status == "canceled" && s.CancelledAt >= last30d).ToList();
            var paused = subscriptions.Where(s => s.Status == "paused").ToList();

            var subscriptionsByPlan = activeSubscriptions
                .GroupBy(s => s.Plan?.Name ?? "Unknown")
                .OrderByDescending(g => g.Count())
                .Select(g => new SubscriptionBreakdownDto { PlanName = g.Key, Count = g.Count(), Mrr = g.Sum(s => (s.Plan?.Amount ?? 0) * s.Quantity) })
                .ToList();

            var mrrGrowth = await CalculateMrrGrowth();

            var dashboard = new SubscriptionsDashboardDto
            {
                TotalSubscriptions = subscriptions.Count,
                ActiveSubscriptions = activeSubscriptions.Count,
                TrialingSubscriptions = trialing.Count,
                PausedSubscriptions = paused.Count,
                CancelledLast30d = cancelled.Count,
                ChurnRate = CalculateChurnRate(subscriptions, cancelled),
                TotalMrr = activeSubscriptions.Sum(s => (s.Plan?.Amount ?? 0) * s.Quantity),
                MrrGrowth = mrrGrowth,
                NewSubscriptionsLast30d = await _subscriptionRepo.CountByTenantIdSinceAsync(tenantId, last30d, "active"),
                ByPlan = subscriptionsByPlan,
                TrialEndingNext7d = trialing.Where(s => s.TrialEnd.HasValue && s.TrialEnd.Value <= DateTime.UtcNow.AddDays(7)).Count(),
                AverageMonthlyValue = activeSubscriptions.Count > 0 ? Math.Round(activeSubscriptions.Average(s => s.Plan?.Amount ?? 0), 2) : 0,
                GeneratedAt = DateTime.UtcNow
            };

            response.SetSuccess(dashboard);
            return response;
        }

        public async Task<GatewayResponseWrapper<CustomersDashboardDto>> GetCustomersDashboardAsync()
        {
            var response = new GatewayResponseWrapper<CustomersDashboardDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var last30d = DateTime.UtcNow.AddDays(-30);
            var customers = await _customerRepo.GetByTenantIdWithDetailsAsync(tenantId);

            var newCustomers30d = customers.Count(c => c.CreatedAt >= last30d);
            var returningCustomers = customers.Count(c => c.Subscriptions.Count > 1);
            var atRiskCustomers = customers.Where(c =>
                c.Subscriptions.Any(s => s.Status == "paused") ||
                (c.Subscriptions.Any(s => s.CancelAtPeriodEnd) && !c.Subscriptions.Any(s => s.Status == "active")))
                .Count();

            var topCustomers = customers
                .Where(c => c.Transactions.Any(t => t.Status == "succeeded"))
                .OrderByDescending(c => c.Transactions.Where(t => t.Status == "succeeded").Sum(t => t.Amount))
                .Take(10)
                .Select(c => new TopCustomerDto
                {
                    CustomerId = c.Id, Name = c.Name, Email = c.Email,
                    LifetimeValue = c.Transactions.Where(t => t.Status == "succeeded").Sum(t => t.Amount),
                    SubscriptionCount = c.Subscriptions.Count(s => s.Status == "active"), CreatedAt = c.CreatedAt
                }).ToList();

            var avgCustomerLifetimeValue = customers.Count > 0
                ? customers.Average(c => c.Transactions.Where(t => t.Status == "succeeded").Sum(t => t.Amount))
                : 0;

            var dashboard = new CustomersDashboardDto
            {
                TotalCustomers = customers.Count,
                NewCustomersLast30d = newCustomers30d,
                ReturningCustomers = returningCustomers,
                AtRiskCustomers = atRiskCustomers,
                CustomersWithSubscriptions = customers.Count(c => c.Subscriptions.Any(s => s.Status == "active")),
                CustomersWithoutSubscriptions = customers.Count(c => !c.Subscriptions.Any(s => s.Status == "active")),
                AverageLifetimeValue = Math.Round(avgCustomerLifetimeValue, 2),
                AverageMonthlySpend = Math.Round(customers.Count > 0
                    ? customers.Average(c => c.Transactions.Where(t => t.CreatedAt >= last30d && t.Status == "succeeded").Sum(t => t.Amount))
                    : 0, 2),
                TopCustomers = topCustomers,
                CustomerGrowthRate = CalculateCustomerGrowthRate(customers),
                GeneratedAt = DateTime.UtcNow
            };

            response.SetSuccess(dashboard);
            return response;
        }

        public async Task<GatewayResponseWrapper<List<AlertDto>>> GetAlertsAsync()
        {
            var response = new GatewayResponseWrapper<List<AlertDto>>();
            var tenantId = CurrentTenantContext.TenantId;
            var alerts = new List<AlertDto>();

            var last24h = DateTime.UtcNow.AddDays(-1);
            var recentTxns = await _transactionRepo.GetByTenantIdSinceAsync(tenantId, last24h);
            if (recentTxns.Count > 0)
            {
                var failureRate = (decimal)recentTxns.Count(t => t.Status == "failed") / recentTxns.Count * 100;
                if (failureRate > 5)
                    alerts.Add(new AlertDto { Type = "warning", Title = "High Payment Failure Rate", Message = $"Payment failure rate is {failureRate:F1}% in the last 24 hours", Severity = "high", CreatedAt = DateTime.UtcNow });
            }

            var expiringTrials = await _subscriptionRepo.Query(tenantId)
                .Where(s => s.Status == "trialing" && s.TrialEnd.HasValue && s.TrialEnd.Value <= DateTime.UtcNow.AddDays(3))
                .CountAsync();
            if (expiringTrials > 0)
                alerts.Add(new AlertDto { Type = "info", Title = "Trials Expiring Soon", Message = $"{expiringTrials} trial periods are expiring in the next 3 days", Severity = "medium", CreatedAt = DateTime.UtcNow });

            var pendingRefunds = await _refundRepo.CountPendingByTenantIdAsync(tenantId);
            if (pendingRefunds > 0)
                alerts.Add(new AlertDto { Type = "info", Title = "Pending Refunds", Message = $"You have {pendingRefunds} pending refunds that need approval", Severity = "low", CreatedAt = DateTime.UtcNow });

            var churnRate = await _revenueAnalytics.GetChurnRateAsync("30d");
            if (churnRate.Data?.ChurnRate > 10)
                alerts.Add(new AlertDto { Type = "warning", Title = "High Churn Rate", Message = $"Monthly churn rate is {churnRate.Data.ChurnRate}%", Severity = "high", CreatedAt = DateTime.UtcNow });

            response.SetSuccess(alerts);
            return response;
        }

        private static decimal CalculateChurnRate(List<Infrastructure.Subscription> subs, List<Infrastructure.Subscription> cancelled)
        {
            var activeCount = subs.Count(s => s.Status == "active");
            var cancelledCount = cancelled.Count;
            var totalCohort = activeCount + cancelledCount;
            if (totalCohort == 0) return 0;
            return Math.Round((decimal)cancelledCount / totalCohort * 100, 1);
        }

        private async Task<decimal> CalculateMrrGrowth()
        {
            var tenantId = CurrentTenantContext.TenantId;
            var thisMonth = DateTime.UtcNow.AddMonths(-1);
            var lastMonth = thisMonth.AddMonths(-1);

            var thisMonthMrr = await _subscriptionRepo.Query(tenantId)
                .Where(s => s.Status == "active" && s.CreatedAt < thisMonth.AddMonths(1))
                .SumAsync(s => s.Plan.Amount * s.Quantity);

            var lastMonthMrr = await _subscriptionRepo.Query(tenantId)
                .Where(s => s.Status == "active" && s.CreatedAt < lastMonth.AddMonths(1))
                .SumAsync(s => s.Plan.Amount * s.Quantity);

            if (lastMonthMrr == 0) return 0;
            return Math.Round(((thisMonthMrr - lastMonthMrr) / lastMonthMrr) * 100, 1);
        }

        private static decimal CalculateCustomerGrowthRate(List<Infrastructure.Customer> customers)
        {
            var last30d = DateTime.UtcNow.AddDays(-30);
            var prior30d = last30d.AddDays(-30);
            var thisMonthNew = customers.Count(c => c.CreatedAt >= last30d && c.CreatedAt < DateTime.UtcNow);
            var lastMonthNew = customers.Count(c => c.CreatedAt >= prior30d && c.CreatedAt < last30d);
            if (lastMonthNew == 0) return thisMonthNew > 0 ? 100 : 0;
            return Math.Round(((decimal)(thisMonthNew - lastMonthNew) / lastMonthNew) * 100, 1);
        }
    }
}
