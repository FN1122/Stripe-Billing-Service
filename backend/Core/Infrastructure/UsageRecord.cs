using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Infrastructure
{
    [Table("UsageRecords")]
    public class UsageRecord
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TenantId { get; set; }

        public Guid SubscriptionId { get; set; }

        [MaxLength(100)]
        public string? StripeSubscriptionItemId { get; set; }

        [MaxLength(100)]
        public string? StripeUsageRecordId { get; set; }

        public long Quantity { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(20)]
        public string Action { get; set; } = "increment"; // increment | set

        [MaxLength(200)]
        public string? IdempotencyKey { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("TenantId")]
        public Tenant? Tenant { get; set; }

        [ForeignKey("SubscriptionId")]
        public Subscription? Subscription { get; set; }
    }

    [Table("MeterEvents")]
    public class MeterEvent
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TenantId { get; set; }

        public Guid CustomerId { get; set; }

        [Required, MaxLength(100)]
        public string EventName { get; set; } = string.Empty;

        public long Value { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(2000)]
        public string? Properties { get; set; } // JSON

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("TenantId")]
        public Tenant? Tenant { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }
    }
}
