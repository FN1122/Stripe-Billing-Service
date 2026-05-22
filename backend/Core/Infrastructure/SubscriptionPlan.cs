namespace Core.Infrastructure
{
    public class SubscriptionPlan
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string StripeProductId { get; set; } = string.Empty;
        public string StripePriceId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public string Interval { get; set; } = "month";
        public int IntervalCount { get; set; } = 1;
        public int TrialDays { get; set; }
        public string Features { get; set; } = "[]";
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Tenant Tenant { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
