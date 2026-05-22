namespace Core.Dtos.Responses
{
    public class LogStatsDto
    {
        public int TotalCalls { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public double AvgDurationMs { get; set; }
        public decimal SuccessRate { get; set; }
    }
}
