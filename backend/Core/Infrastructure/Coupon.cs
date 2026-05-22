using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Infrastructure
{
    [Table("Coupons")]
    public class Coupon
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TenantId { get; set; }

        [MaxLength(100)]
        public string? StripeCouponId { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Type { get; set; } = "percent_off"; // percent_off | amount_off

        public decimal? AmountOff { get; set; }

        public decimal? PercentOff { get; set; }

        [MaxLength(3)]
        public string? Currency { get; set; }

        [Required, MaxLength(20)]
        public string Duration { get; set; } = "once"; // once | repeating | forever

        public int? DurationInMonths { get; set; }

        public int? MaxRedemptions { get; set; }

        public int TimesRedeemed { get; set; } = 0;

        public DateTime? RedeemBy { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(2000)]
        public string? Metadata { get; set; } // JSON

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("TenantId")]
        public Tenant? Tenant { get; set; }

        public ICollection<PromotionCode> PromotionCodes { get; set; } = new List<PromotionCode>();
        public ICollection<CouponRedemption> Redemptions { get; set; } = new List<CouponRedemption>();
    }
}
