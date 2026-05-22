namespace Core.Dtos.Responses
{
    public class CouponResponseDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string? StripeCouponId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal? AmountOff { get; set; }
        public decimal? PercentOff { get; set; }
        public string? Currency { get; set; }
        public string Duration { get; set; } = string.Empty;
        public int? DurationInMonths { get; set; }
        public int? MaxRedemptions { get; set; }
        public int TimesRedeemed { get; set; }
        public DateTime? RedeemBy { get; set; }
        public bool IsActive { get; set; }
        public List<PromotionCodeResponseDto> PromotionCodes { get; set; } = new();
        public string? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class PromotionCodeResponseDto
    {
        public Guid Id { get; set; }
        public Guid CouponId { get; set; }
        public string? StripePromotionCodeId { get; set; }
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int? MaxRedemptions { get; set; }
        public int TimesRedeemed { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool FirstTimeTransaction { get; set; }
        public decimal? MinimumAmount { get; set; }
        public string? MinimumAmountCurrency { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CouponRedemptionResponseDto
    {
        public Guid Id { get; set; }
        public Guid CouponId { get; set; }
        public Guid? PromotionCodeId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? SubscriptionId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public decimal AmountDiscounted { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime RedeemedAt { get; set; }
    }

    public class CouponStatsDto
    {
        public int TotalCoupons { get; set; }
        public int ActiveCoupons { get; set; }
        public int TotalRedemptions { get; set; }
        public decimal TotalDiscountAmount { get; set; }
        public string? MostUsedCouponName { get; set; }
        public Dictionary<string, int> RedemptionsByMonth { get; set; } = new();
    }
}
