namespace Core.Dtos.Responses
{
    public class LogResponseDto
    {
        public Guid Id { get; set; }
        public string ServiceType { get; set; }
        public string Endpoint { get; set; }
        public string Method { get; set; }
        public int ResponseStatusCode { get; set; }
        public int DurationMs { get; set; }
        public string Status { get; set; }
        public string IpAddress { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
