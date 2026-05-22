using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Infrastructure
{
    [Table("PromotionCodes")]
    public class PromotionCode
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TenantId { get; set; }

        public Guid CouponId { get; set; }

        [MaxLength(100)]
        public string? StripePromotionCodeId { get; set; }

        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        public int? MaxRedemptions { get; set; }

        public int TimesRedeemed { get; set; } = 0;

        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(2000)]
        public string? Restrictions { get; set; } // JSON: { minAmount, firstTimeTransaction, customerEmails[] }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("TenantId")]
        public Tenant? Tenant { get; set; }

        [ForeignKey("CouponId")]
        public Coupon? Coupon { get; set; }
    }
}
