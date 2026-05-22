namespace Core.Infrastructure
{
    public class WebhookDelivery
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid WebhookSubscriptionId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string EventData { get; set; } = "{}";
        public string TargetUrl { get; set; } = string.Empty;
        public string Payload { get; set; } = "{}";
        public string Status { get; set; } = "Pending";
        public int? HttpStatusCode { get; set; }
        public int? StatusCode { get; set; }
        public string? ResponseBody { get; set; }
        public int? DurationMs { get; set; }
        public int RetryCount { get; set; }
        public int MaxAttempts { get; set; } = 5;
        public int MaxRetries { get; set; } = 5;
        public DateTime? NextRetryAt { get; set; }
        public string? FailureReason { get; set; }
        public string? LastError { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public WebhookSubscription WebhookSubscription { get; set; }
    }
}
