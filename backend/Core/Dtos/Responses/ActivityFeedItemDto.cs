namespace Core.Dtos.Responses
{
    public class ActivityFeedItemDto
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public decimal? Amount { get; set; }
        public string Currency { get; set; }
        public DateTime Timestamp { get; set; }
        public dynamic Metadata { get; set; }
    }
}
