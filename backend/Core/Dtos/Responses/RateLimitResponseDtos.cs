namespace Core.Dtos.Responses
{
    public class RateLimitResponseDto
    {
        public Guid Id { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public int RequestsPerMinute { get; set; }
        public int? BurstLimit { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class RateLimitUsageDto
    {
        public string Endpoint { get; set; } = string.Empty;
        public int CurrentRequests { get; set; }
        public int Limit { get; set; }
        public int Remaining { get; set; }
        public DateTime ResetsAt { get; set; }
    }
}
