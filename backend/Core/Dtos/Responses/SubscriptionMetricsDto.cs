namespace Core.Dtos.Responses
{
    public class SubscriptionMetricsDto
    {
        public int ActiveCount { get; set; }
        public int TrialingCount { get; set; }
        public int PastDueCount { get; set; }
        public int CancelledThisMonth { get; set; }
        public int NewThisMonth { get; set; }
        public List<SubscriptionTrendPoint> Trend { get; set; } = new();
    }

    public class SubscriptionTrendPoint
    {
        public string Date { get; set; }
        public int NewCount { get; set; }
        public int CancelledCount { get; set; }
    }
}
