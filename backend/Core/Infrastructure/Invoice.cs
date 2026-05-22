namespace Core.Infrastructure
{
    public class Invoice
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? SubscriptionId { get; set; }
        public string StripeInvoiceId { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountDue { get; set; }
        public string Currency { get; set; } = "usd";
        public string Status { get; set; } = "draft";
        public string? InvoicePdfUrl { get; set; }
        public string? HostedInvoiceUrl { get; set; }
        public string? LineItems { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Tenant Tenant { get; set; }
        public Customer Customer { get; set; }
    }
}
