// ============================================================
// Audit Request DTOs
// ============================================================

namespace Core.Dtos.Requests
{
    public class CreateAuditLogDto
    {
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public Dictionary<string, object>? Changes { get; set; }
        public string? Status { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class AuditLogFilterDto
    {
        public string? EntityType { get; set; }
        public string? Action { get; set; }
        public Guid? UserId { get; set; }
        public string? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string SortOrder { get; set; } = "desc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
