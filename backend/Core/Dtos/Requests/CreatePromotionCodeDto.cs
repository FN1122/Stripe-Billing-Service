namespace Core.Dtos.Requests
{
    public class CreatePromotionCodeDto
    {
        public Guid CouponId { get; set; }
        public string Code { get; set; } = string.Empty;
        public int? MaxRedemptions { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool FirstTimeTransaction { get; set; }
        public decimal? MinimumAmount { get; set; }
        public string? MinimumAmountCurrency { get; set; }
        public string? Metadata { get; set; }
    }
}
