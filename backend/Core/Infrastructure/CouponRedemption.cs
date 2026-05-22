using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Infrastructure
{
    [Table("CouponRedemptions")]
    public class CouponRedemption
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TenantId { get; set; }

        public Guid CouponId { get; set; }

        public Guid? PromotionCodeId { get; set; }

        public Guid CustomerId { get; set; }

        public Guid? SubscriptionId { get; set; }

        [MaxLength(100)]
        public string? StripeDiscountId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountDiscounted { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; } = "usd";

        public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("TenantId")]
        public Tenant? Tenant { get; set; }

        [ForeignKey("CouponId")]
        public Coupon? Coupon { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }
    }
}
