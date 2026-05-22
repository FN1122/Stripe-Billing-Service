namespace Core.Infrastructure
{
    public class Refund
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid TransactionId { get; set; }
        public Guid? CustomerId { get; set; }
        public string StripeRefundId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "pending";
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Tenant Tenant { get; set; }
        public PaymentTransaction Transaction { get; set; }
        public Customer Customer { get; set; }
    }
}
