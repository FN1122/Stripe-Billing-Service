using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Infrastructure
{
    [Table("IdempotencyKeys")]
    public class IdempotencyKey
    {
        [Key, MaxLength(200)]
        public string Key { get; set; } = string.Empty;
        public Guid TenantId { get; set; }
        [MaxLength(10)]
        public string HttpMethod { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Endpoint { get; set; } = string.Empty;
        [MaxLength(64)]
        public string? RequestHash { get; set; }
        public int ResponseStatusCode { get; set; }
        public string? ResponseBody { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);
    }

    [Table("CustomerCredits")]
    public class CustomerCredit
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }
        public Guid CustomerId { get; set; }
        [Required, MaxLength(20)]
        public string Type { get; set; } = "credit"; // credit | debit | adjustment
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        [MaxLength(3)]
        public string Currency { get; set; } = "usd";
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        [Required, MaxLength(20)]
        public string Source { get; set; } = "manual"; // manual | refund | promotion | system
        public Guid? ReferenceId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAfter { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("TenantId")]
        public Tenant? Tenant { get; set; }
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }
    }
}
