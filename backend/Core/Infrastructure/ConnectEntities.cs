using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Infrastructure
{
    [Table("ConnectedAccounts")]
    public class ConnectedAccount
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }
        [MaxLength(100)]
        public string? StripeAccountId { get; set; }
        [MaxLength(200)]
        public string? BusinessName { get; set; }
        [MaxLength(256)]
        public string? Email { get; set; }
        [MaxLength(2)]
        public string? Country { get; set; }
        [Required, MaxLength(20)]
        public string Type { get; set; } = "express"; // standard | express | custom
        public bool ChargesEnabled { get; set; }
        public bool PayoutsEnabled { get; set; }
        public bool OnboardingComplete { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public decimal PlatformFeePercent { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PlatformFeeFixed { get; set; }
        [MaxLength(2000)]
        public string? Metadata { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("TenantId")]
        public Tenant? Tenant { get; set; }
    }

    [Table("TransferRecords")]
    public class TransferRecord
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }
        public Guid ConnectedAccountId { get; set; }
        [MaxLength(100)]
        public string? StripeTransferId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        [MaxLength(3)]
        public string Currency { get; set; } = "usd";
        [MaxLength(500)]
        public string? Description { get; set; }
        [MaxLength(20)]
        public string Status { get; set; } = "pending";
        public Guid? SourcePaymentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("TenantId")]
        public Tenant? Tenant { get; set; }
    }
}
