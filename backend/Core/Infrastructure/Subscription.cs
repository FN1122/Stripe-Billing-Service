namespace Core.Infrastructure
{
    public class Subscription
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid PlanId { get; set; }
        public string StripeSubscriptionId { get; set; } = string.Empty;
        public string Status { get; set; } = "active";
        public int Quantity { get; set; } = 1;
        public DateTime CurrentPeriodStart { get; set; }
        public DateTime CurrentPeriodEnd { get; set; }
        public DateTime? TrialStart { get; set; }
        public DateTime? TrialEnd { get; set; }
        public bool CancelAtPeriodEnd { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public string Metadata { get; set; } = "{}";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Tenant Tenant { get; set; }
        public Customer Customer { get; set; }
        public SubscriptionPlan Plan { get; set; }
    }
}
