namespace Core.Infrastructure
{
    public class Customer
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string? ExternalReferenceId { get; set; }
        public string? StripeCustomerId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Currency { get; set; } = "usd";
        public string? BillingAddress { get; set; }
        public string? TaxId { get; set; }
        public string? Metadata { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Tenant Tenant { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
