namespace Core.Dtos.Responses
{
    public class InvoiceItemResponseDto
    {
        public Guid Id { get; set; }
        public string? StripeInvoiceItemId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? SubscriptionId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
