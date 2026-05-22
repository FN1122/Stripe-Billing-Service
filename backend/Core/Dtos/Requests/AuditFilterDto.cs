namespace Core.Dtos.Requests
{
    public class AuditFilterDto
    {
        public string? Action { get; set; }
        public Guid? UserId { get; set; }
        public Guid? TenantId { get; set; }
        public string? EntityType { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
