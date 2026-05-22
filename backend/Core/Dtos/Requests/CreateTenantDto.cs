namespace Core.Dtos.Requests
{
    public class CreateTenantDto
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string ContactEmail { get; set; }
        public string Plan { get; set; } = "starter";
        public string StripeSecretKey { get; set; }
        public string StripePublishableKey { get; set; }
        public List<string> Features { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }
}
