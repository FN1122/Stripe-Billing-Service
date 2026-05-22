namespace Core.Dtos.Requests
{
    public class CreatePlanDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public string Interval { get; set; } = "month";
        public int IntervalCount { get; set; } = 1;
        public int TrialDays { get; set; }
        public List<string> Features { get; set; } = new();
        public int SortOrder { get; set; }
    }
}
