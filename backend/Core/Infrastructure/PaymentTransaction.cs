namespace Core.Infrastructure
{
    public class PaymentTransaction
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? SubscriptionId { get; set; }
        public string StripePaymentIntentId { get; set; } = string.Empty;
        public string StripeChargeId { get; set; } = string.Empty;
        public string StripeCheckoutSessionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal AmountRefunded { get; set; }
        public string Currency { get; set; } = "usd";
        public string Status { get; set; } = "pending";
        public string Type { get; set; } = "one_time";
        public string PaymentMethod { get; set; } = string.Empty;
        public string? PaymentMethodLast4 { get; set; }
        public string? PaymentMethodBrand { get; set; }
        public string? Description { get; set; }
        public string? FailureReason { get; set; }
        public string? ReceiptUrl { get; set; }
        public string Metadata { get; set; } = "{}";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Tenant Tenant { get; set; }
        public Customer Customer { get; set; }
        public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
    }
}
