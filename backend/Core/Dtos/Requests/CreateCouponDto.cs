namespace Core.Dtos.Requests
{
    public class CreateCouponDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "percent_off"; // percent_off | amount_off
        public decimal? AmountOff { get; set; }
        public decimal? PercentOff { get; set; }
        public string? Currency { get; set; }
        public string Duration { get; set; } = "once"; // once | repeating | forever
        public int? DurationInMonths { get; set; }
        public int? MaxRedemptions { get; set; }
        public DateTime? RedeemBy { get; set; }
        public string? Metadata { get; set; }
    }
}
