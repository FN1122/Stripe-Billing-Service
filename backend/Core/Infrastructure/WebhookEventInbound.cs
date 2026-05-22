namespace Core.Infrastructure
{
    public class WebhookEventInbound
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string StripeEventId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Data { get; set; } = "{}";
        public string Payload { get; set; } = "{}";
        public string Status { get; set; } = "received";
        public string? ProcessingError { get; set; }
        public int RetryCount { get; set; }
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }

        public Tenant Tenant { get; set; }
    }
}
