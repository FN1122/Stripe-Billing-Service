namespace Core.Dtos.Responses
{
    public class SystemHealthDto
    {
        public decimal WebhookDeliverySuccessRate { get; set; }
        public double AverageApiResponseTimeMs { get; set; }
        public int StripeApiErrorCount { get; set; }
        public int PendingWebhookDeliveries { get; set; }
        public bool DatabaseConnected { get; set; }
        public int ActiveTenants { get; set; }
        public int TotalApiCallsToday { get; set; }
    }
}
