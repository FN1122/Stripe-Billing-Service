namespace Core.Dtos.Responses
{
    public class WebhookDeliveryResponseDto
    {
        public Guid Id { get; set; }
        public Guid WebhookSubscriptionId { get; set; }
        public string EventType { get; set; }
        public string TargetUrl { get; set; }
        public string Payload { get; set; }
        public string Status { get; set; }
        public int? StatusCode { get; set; }
        public int? HttpStatusCode { get; set; }
        public string ResponseBody { get; set; }
        public int? DurationMs { get; set; }
        public int RetryCount { get; set; }
        public string FailureReason { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
