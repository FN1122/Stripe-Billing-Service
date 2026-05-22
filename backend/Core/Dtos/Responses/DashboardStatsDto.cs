namespace Core.Dtos.Responses
{
    public class DashboardStatsDto
    {
        public int TotalCustomers { get; set; }
        public int ActiveSubscriptions { get; set; }
        public decimal Revenue24h { get; set; }
        public decimal Revenue30d { get; set; }
        public int NewCustomers24h { get; set; }
        public int NewSubscriptions24h { get; set; }
        public decimal TransactionSuccess24h { get; set; }
        public decimal ChurnRate30d { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
