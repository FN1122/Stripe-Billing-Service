namespace Core.Dtos.Responses
{
    public class ChurnDataDto
    {
        public decimal MonthlyChurnRate { get; set; }
        public decimal AnnualChurnRate { get; set; }
        public int ChurnedSubscriptions { get; set; }
        public decimal ChurnedMrr { get; set; }
        public decimal RetentionRate { get; set; }
    }
}
