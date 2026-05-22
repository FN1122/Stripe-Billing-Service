using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Infrastructure
{
    [Table("EmailTemplates")]
    public class EmailTemplate
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }
        [Required, MaxLength(100)]
        public string TemplateKey { get; set; } = string.Empty;
        [Required, MaxLength(500)]
        public string Subject { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;
        public string? PlainTextBody { get; set; }
        public bool IsActive { get; set; } = true;
        [MaxLength(2000)]
        public string? Variables { get; set; } // JSON array
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("TenantId")]
        public Tenant? Tenant { get; set; }
    }

    [Table("EmailLogs")]
    public class EmailLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }
        [MaxLength(100)]
        public string? TemplateKey { get; set; }
        [Required, MaxLength(256)]
        public string To { get; set; } = string.Empty;
        [MaxLength(256)]
        public string? Cc { get; set; }
        [MaxLength(256)]
        public string? Bcc { get; set; }
        [Required, MaxLength(500)]
        public string Subject { get; set; } = string.Empty;
        [Required, MaxLength(20)]
        public string Status { get; set; } = "queued"; // queued | sent | delivered | failed | bounced
        [MaxLength(30)]
        public string? Provider { get; set; }
        [MaxLength(200)]
        public string? ProviderMessageId { get; set; }
        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("TenantId")]
        public Tenant? Tenant { get; set; }
    }
}
