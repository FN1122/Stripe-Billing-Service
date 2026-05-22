namespace Core.Dtos.Responses
{
    public class TenantCredentialsResponseDto
    {
        public Guid TenantId { get; set; }
        public string PublicApiKey { get; set; }
        public string SecretApiKey { get; set; }
        public string WebhookSigningSecret { get; set; }
        public string JwtSigningSecret { get; set; }
        public string Message { get; set; } = "Save these credentials. They will NOT be shown again.";
    }
}
