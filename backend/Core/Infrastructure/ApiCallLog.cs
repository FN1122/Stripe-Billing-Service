namespace Core.Infrastructure
{
    public class ApiCallLog
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid? ApiKeyId { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string? RequestBody { get; set; }
        public int ResponseStatusCode { get; set; }
        public int StatusCode { get; set; }
        public string? ResponseBody { get; set; }
        public int? ThirdPartyStatusCode { get; set; }
        public int DurationMs { get; set; }
        public double ResponseTime { get; set; }
        public long RequestSize { get; set; }
        public long ResponseSize { get; set; }
        public string? UserAgent { get; set; }
        public bool Success { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
