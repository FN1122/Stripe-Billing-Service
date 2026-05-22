using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Infrastructure
{
    [Table("EndpointRateLimits")]
    public class EndpointRateLimitConfig
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }
        [Required, MaxLength(200)]
        public string Endpoint { get; set; } = string.Empty; // e.g., "POST /api/v1/payments/*"
        public int RequestsPerMinute { get; set; } = 60;
        public int? BurstLimit { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("TenantId")]
        public Tenant? Tenant { get; set; }
    }
}
