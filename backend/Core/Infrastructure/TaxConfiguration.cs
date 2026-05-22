using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Infrastructure
{
    [Table("TaxConfigurations")]
    public class TaxConfiguration
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TenantId { get; set; }

        [Required, MaxLength(30)]
        public string Provider { get; set; } = "stripe_tax"; // stripe_tax | taxjar | avalara

        public bool IsEnabled { get; set; } = false;

        public bool AutoCalculate { get; set; } = true;

        [Required, MaxLength(20)]
        public string DefaultTaxBehavior { get; set; } = "exclusive"; // inclusive | exclusive

        [Column(TypeName = "decimal(5,4)")]
        public decimal? FallbackTaxRate { get; set; }

        [MaxLength(2000)]
        public string? RegistrationNumbers { get; set; } // JSON: [{ country, type, value }]

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("TenantId")]
        public Tenant? Tenant { get; set; }
    }

    [Table("TaxExemptions")]
    public class TaxExemption
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TenantId { get; set; }

        public Guid CustomerId { get; set; }

        [Required, MaxLength(20)]
        public string ExemptionType { get; set; } = "exempt"; // exempt | reverse | none

        [MaxLength(200)]
        public string? CertificateId { get; set; }

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("TenantId")]
        public Tenant? Tenant { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }
    }
}
