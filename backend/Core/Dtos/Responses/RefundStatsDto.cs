namespace Core.Dtos.Responses
{
    public class RefundStatsDto
    {
        public int TotalRefunds { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal RefundRate { get; set; }
        public int PendingCount { get; set; }
        public decimal AvgProcessingTimeHours { get; set; }
    }
}
