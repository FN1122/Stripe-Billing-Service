namespace Core.Dtos.Requests
{
    public class CreateRateLimitDto
    {
        public string Endpoint { get; set; } = string.Empty;
        public int RequestsPerMinute { get; set; } = 60;
        public int? BurstLimit { get; set; }
    }

    public class UpdateRateLimitDto
    {
        public int? RequestsPerMinute { get; set; }
        public int? BurstLimit { get; set; }
        public bool? IsActive { get; set; }
    }
}
