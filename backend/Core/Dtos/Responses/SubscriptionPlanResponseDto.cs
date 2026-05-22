namespace Core.Dtos.Responses
{
    public class SubscriptionPlanResponseDto
    {
        public Guid Id { get; set; }
        public string StripeProductId { get; set; }
        public string StripePriceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Interval { get; set; }
        public int IntervalCount { get; set; }
        public int TrialDays { get; set; }
        public List<string> Features { get; set; } = new();
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int SubscriberCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
