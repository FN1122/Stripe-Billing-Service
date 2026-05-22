namespace Core.Infrastructure
{
    public class ApiKey
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string KeyHash { get; set; } = string.Empty;
        public string KeyEnc { get; set; } = string.Empty;
        public string KeyPrefix { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Environment { get; set; } = "test";
        public string? Permissions { get; set; }
        public int RateLimitPerMinute { get; set; } = 60;
        public bool IsActive { get; set; } = true;
        public DateTime? LastUsedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public long TotalRequests { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Tenant Tenant { get; set; }
    }
}
