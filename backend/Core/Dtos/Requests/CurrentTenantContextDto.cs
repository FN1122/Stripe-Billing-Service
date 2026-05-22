namespace Core.Dtos.Requests
{
    public class CurrentTenantContextDto
    {
        public Guid TenantId { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; }
        public Guid ApiKeyId { get; set; }
        public List<string> ApiKeyPermissions { get; set; } = new();
    }
}
