namespace Core.Infrastructure
{
    public class WebhookSubscription
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string WebhookUrl { get; set; } = string.Empty;
        public string TargetUrl { get; set; } = string.Empty;
        public string HmacSecret { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
        public string Events { get; set; } = "[]";
        public string? CustomHeaders { get; set; }
        public string RetryPolicy { get; set; } = "exponential_backoff";
        public int MaxRetries { get; set; } = 5;
        public int Timeout { get; set; } = 30;
        public string? Metadata { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public DateTime? DisabledAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Tenant Tenant { get; set; }
        public ICollection<WebhookDelivery> Deliveries { get; set; } = new List<WebhookDelivery>();
        public ICollection<WebhookDelivery> WebhookDeliveries { get; set; } = new List<WebhookDelivery>();
    }
}
