namespace Core.Dtos.Responses
{
    public class WebhookEventResponseDto
    {
        public Guid Id { get; set; }
        public string? StripeEventId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class WebhookEventDetailDto
    {
        public Guid Id { get; set; }
        public string? StripeEventId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string PayloadJson { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class WebhookDeliveryDetailDto
    {
        public Guid Id { get; set; }
        public Guid WebhookSubscriptionId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? HttpStatusCode { get; set; }
        public int Attempts { get; set; }
        public DateTime? LastAttemptedAt { get; set; }
        public DateTime? NextRetryAt { get; set; }
        public string PayloadJson { get; set; } = string.Empty;
        public string? ResponseBody { get; set; }
    }

    public class WebhookEventStatsDto
    {
        public int TotalEventsReceived { get; set; }
        public int TotalEventsProcessed { get; set; }
        public int TotalEventsFailed { get; set; }
        public int TotalDeliveriesSucceeded { get; set; }
        public int TotalDeliveriesFailed { get; set; }
        public Dictionary<string, int> EventsByType { get; set; } = new();
    }
}
