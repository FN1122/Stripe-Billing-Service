namespace Core.Dtos.Requests
{
    public class UpdateTenantDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string WebhookCallbackUrl { get; set; }
        public string Plan { get; set; }
        public string Settings { get; set; }
        public string StripePublishableKey { get; set; }
        public string StripeSecretKeyEnc { get; set; }
        public List<string> Features { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }
}
