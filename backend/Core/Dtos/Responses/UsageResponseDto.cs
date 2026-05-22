namespace Core.Dtos.Responses
{
    public class UsageRecordResponseDto
    {
        public Guid Id { get; set; }
        public Guid SubscriptionId { get; set; }
        public string? StripeSubscriptionItemId { get; set; }
        public long Quantity { get; set; }
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? IdempotencyKey { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UsageSummaryDto
    {
        public Guid SubscriptionId { get; set; }
        public string? CustomerName { get; set; }
        public long CurrentPeriodUsage { get; set; }
        public long PreviousPeriodUsage { get; set; }
        public decimal UsageChangePercent { get; set; }
        public decimal EstimatedCharge { get; set; }
        public Dictionary<string, long> DailyUsage { get; set; } = new();
    }

    public class MeterEventResponseDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public long Value { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Properties { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UsageDashboardDto
    {
        public long TotalUsageCurrentPeriod { get; set; }
        public int ActiveMeteredSubscriptions { get; set; }
        public decimal EstimatedRevenue { get; set; }
        public List<TopConsumerDto> TopConsumers { get; set; } = new();
        public Dictionary<string, long> UsageTrend { get; set; } = new();
    }

    public class TopConsumerDto
    {
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public long TotalUsage { get; set; }
        public decimal EstimatedCharge { get; set; }
    }
}
