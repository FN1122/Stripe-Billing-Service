namespace Core.Dtos.Responses
{
    public class ApiKeyResponseDto
    {
        public Guid Id { get; set; }
        public string KeyPrefix { get; set; }
        public string PlainKey { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Environment { get; set; }
        public List<string> Permissions { get; set; } = new();
        public int RateLimitPerMinute { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public long TotalRequests { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
