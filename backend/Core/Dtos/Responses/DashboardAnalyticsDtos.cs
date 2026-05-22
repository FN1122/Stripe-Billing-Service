// ============================================================
// Dashboard, Analytics, and Audit DTOs
// ============================================================

namespace Core.Dtos.Responses
{
    // === Dashboard DTOs ===
    public class ComprehensiveDashboardDto
    {
        public DashboardStatsDto Stats { get; set; }
        public RevenueMetricsDto RevenueMetrics { get; set; }
        public MrrDto Mrr { get; set; }
        public ChurnRateDto ChurnRate { get; set; }
        public List<ActivityFeedItemDto> ActivityFeed { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class PaymentsDashboardDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalRefunded { get; set; }
        public decimal NetRevenue { get; set; }
        public int TransactionCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int RefundedCount { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal RefundRate { get; set; }
        public decimal AverageTransactionValue { get; set; }
        public Dictionary<string, int> TopPaymentMethods { get; set; } = new();
        public List<RevenueDataPoint> DailyRevenue { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }

    public class SubscriptionsDashboardDto
    {
        public int TotalSubscriptions { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int TrialingSubscriptions { get; set; }
        public int PausedSubscriptions { get; set; }
        public int CancelledLast30d { get; set; }
        public decimal ChurnRate { get; set; }
        public decimal TotalMrr { get; set; }
        public decimal MrrGrowth { get; set; }
        public int NewSubscriptionsLast30d { get; set; }
        public List<SubscriptionBreakdownDto> ByPlan { get; set; } = new();
        public int TrialEndingNext7d { get; set; }
        public decimal AverageMonthlyValue { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class SubscriptionBreakdownDto
    {
        public string PlanName { get; set; }
        public int Count { get; set; }
        public decimal Mrr { get; set; }
    }

    public class CustomersDashboardDto
    {
        public int TotalCustomers { get; set; }
        public int NewCustomersLast30d { get; set; }
        public int ReturningCustomers { get; set; }
        public int AtRiskCustomers { get; set; }
        public int CustomersWithSubscriptions { get; set; }
        public int CustomersWithoutSubscriptions { get; set; }
        public decimal AverageLifetimeValue { get; set; }
        public decimal AverageMonthlySpend { get; set; }
        public List<TopCustomerDto> TopCustomers { get; set; } = new();
        public decimal CustomerGrowthRate { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class TopCustomerDto
    {
        public Guid CustomerId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal LifetimeValue { get; set; }
        public int SubscriptionCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AlertDto
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Severity { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // === Revenue Analytics DTOs ===
    public class MrrDto
    {
        public decimal CurrentMrr { get; set; }
        public decimal PreviousMonthMrr { get; set; }
        public decimal MrrGrowth { get; set; }
        public decimal MrrGrowthPercentage { get; set; }
        public int ActiveSubscriptions { get; set; }
        public List<MrrBreakdownDto> ByPlan { get; set; } = new();
        public DateTime CalculatedAt { get; set; }
    }

    public class MrrBreakdownDto
    {
        public Guid PlanId { get; set; }
        public string PlanName { get; set; }
        public int SubscriberCount { get; set; }
        public decimal Mrr { get; set; }
    }

    public class ChurnRateDto
    {
        public string Period { get; set; }
        public decimal ChurnRate { get; set; }
        public int CancelledCount { get; set; }
        public int RetainedCount { get; set; }
        public Dictionary<string, int> CancellationReasons { get; set; } = new();
        public decimal ProjectedMonthlyChurn { get; set; }
        public DateTime CalculatedAt { get; set; }
    }

    public class LtvDto
    {
        public decimal AverageLtv { get; set; }
        public decimal MedianLtv { get; set; }
        public decimal TotalLtv { get; set; }
        public int CustomerCount { get; set; }
        public List<HighValueCustomerDto> HighValueCustomers { get; set; } = new();
        public DateTime CalculatedAt { get; set; }
    }

    public class HighValueCustomerDto
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public decimal Ltv { get; set; }
        public decimal MonthlyValue { get; set; }
        public double Tenure { get; set; }
    }

    public class RevenueMetricsDto
    {
        public string Period { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalRefunded { get; set; }
        public decimal NetRevenue { get; set; }
        public int TransactionCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int RefundedCount { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal RefundRate { get; set; }
        public decimal AverageTransactionValue { get; set; }
        public decimal AverageRefundAmount { get; set; }
        public List<RevenueDataPoint> DailyRevenue { get; set; } = new();
        public DateTime CalculatedAt { get; set; }
    }

    // === Audit Stats DTO ===
    public class AuditStatsDto
    {
        public int TotalEvents { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public Dictionary<string, int> ByEntityType { get; set; } = new();
        public Dictionary<string, int> ByAction { get; set; } = new();
        public Dictionary<Guid, int> TopUsers { get; set; } = new();
    }
}
