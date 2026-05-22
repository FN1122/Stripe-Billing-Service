using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Infrastructure
{
    [Table("DunningSchedules")]
    public class DunningSchedule
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }
        public Guid SubscriptionId { get; set; }
        public Guid CustomerId { get; set; }
        [MaxLength(100)]
        public string? StripeInvoiceId { get; set; }
        [Required, MaxLength(20)]
        public string Status { get; set; } = "active"; // active | paused | completed | cancelled
        public int CurrentStep { get; set; } = 0;
        public int MaxSteps { get; set; } = 4;
        public DateTime? NextRetryAt { get; set; }
        public DateTime? LastRetryAt { get; set; }
        public int TotalRetryAttempts { get; set; } = 0;
        public DateTime OriginalFailureDate { get; set; } = DateTime.UtcNow;
        [MaxLength(500)]
        public string? FailureReason { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountDue { get; set; }
        [MaxLength(3)]
        public string Currency { get; set; } = "usd";
        public DateTime? GracePeriodEndsAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("TenantId")]
        public Tenant? Tenant { get; set; }
        [ForeignKey("SubscriptionId")]
        public Subscription? Subscription { get; set; }
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }
    }

    [Table("DunningSteps")]
    public class DunningStep
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }
        public int SortOrder { get; set; }
        public int DaysAfterFailure { get; set; }
        [Required, MaxLength(30)]
        public string Action { get; set; } = "retry_payment"; // retry_payment | send_email | pause_subscription | cancel_subscription
        [MaxLength(100)]
        public string? EmailTemplateKey { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("TenantId")]
        public Tenant? Tenant { get; set; }
    }
}
