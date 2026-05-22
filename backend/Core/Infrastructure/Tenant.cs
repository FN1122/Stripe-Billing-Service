namespace Core.Infrastructure
{
    public class Tenant
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? PublicApiKey { get; set; }
        public string? SecretApiKeyHash { get; set; }
        public string? WebhookSigningSecret { get; set; }
        public string? WebhookCallbackUrl { get; set; }
        public string? JwtSigningSecret { get; set; }
        public string StripeSecretKeyEnc { get; set; } = string.Empty;
        public string? StripePublishableKey { get; set; }
        public string? StripeWebhookSecret { get; set; }
        public string Settings { get; set; } = "{}";
        public string Plan { get; set; } = "free";
        public string? Description { get; set; }
        public string? PublicKey { get; set; }
        public string Features { get; set; } = "[]";
        public string Metadata { get; set; } = "{}";
        public string? SuspensionReason { get; set; }
        public DateTime? SuspendedAt { get; set; }
        public int? KeyRotationCount { get; set; }
        public DateTime? LastKeyRotationAt { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public ICollection<SubscriptionPlan> SubscriptionPlans { get; set; } = new List<SubscriptionPlan>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
        public ICollection<WebhookSubscription> WebhookSubscriptions { get; set; } = new List<WebhookSubscription>();
    }
}
