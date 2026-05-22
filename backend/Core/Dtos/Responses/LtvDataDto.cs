namespace Core.Dtos.Responses
{
    public class LtvDataDto
    {
        public decimal AverageLtv { get; set; }
        public decimal MedianLtv { get; set; }
        public decimal AverageSubscriptionDurationMonths { get; set; }
        public decimal AverageRevenuePerCustomer { get; set; }
    }
}
