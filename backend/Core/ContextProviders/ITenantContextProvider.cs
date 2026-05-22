namespace Core.ContextProviders
{
    public interface ITenantContextProvider
    {
        TenantContext GetCurrentTenantContext();
    }

    public class TenantContext
    {
        public Guid TenantId { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public Guid? ApiKeyId { get; set; }
        public List<string> ApiKeyPermissions { get; set; } = new();
    }
}
