namespace Core.Dtos.Responses
{
    public class PaymentAnalyticsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal NetRevenue { get; set; }
        public int TransactionCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal AverageTransactionValue { get; set; }
        public List<RevenueDataPoint> RevenueByDay { get; set; } = new();
    }
}
