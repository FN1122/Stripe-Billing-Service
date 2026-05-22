namespace Core.Dtos.Requests
{
    public class CreateApiKeyDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Environment { get; set; } = "test";
        public List<string> Permissions { get; set; } = new();
        public int RateLimitPerMinute { get; set; } = 60;
        public DateTime? ExpiresAt { get; set; }
    }
}
