namespace Core.Infrastructure
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public Guid? TenantId { get; set; }
        public Guid? UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string? Changes { get; set; }
        public string Status { get; set; } = "success";
        public string? ErrorMessage { get; set; }
        public string? Metadata { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
