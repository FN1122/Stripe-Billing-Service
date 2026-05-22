namespace Core.Dtos.Responses
{
    public class WebhookSubscriptionResponseDto
    {
        public Guid Id { get; set; }
        public string WebhookUrl { get; set; }
        public string TargetUrl { get; set; }
        public List<string> Events { get; set; } = new();
        public Dictionary<string, string> CustomHeaders { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public string Secret { get; set; }
        public string RetryPolicy { get; set; }
        public int MaxRetries { get; set; }
        public int Timeout { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
