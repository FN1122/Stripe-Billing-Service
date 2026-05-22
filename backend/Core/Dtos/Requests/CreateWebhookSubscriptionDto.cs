namespace Core.Dtos.Requests
{
    public class CreateWebhookSubscriptionDto
    {
        public string WebhookUrl { get; set; }
        public string TargetUrl { get; set; }
        public List<string> Events { get; set; } = new();
        public Dictionary<string, string> CustomHeaders { get; set; }
        public string Description { get; set; }
        public string RetryPolicy { get; set; }
        public int? MaxRetries { get; set; }
        public int? Timeout { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }
}
