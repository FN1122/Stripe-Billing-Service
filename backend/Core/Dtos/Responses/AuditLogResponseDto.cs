namespace Core.Dtos.Responses
{
    public class AuditLogResponseDto
    {
        public Guid Id { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public string Action { get; set; }
        public Guid? UserId { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public dynamic Changes { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public dynamic Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
