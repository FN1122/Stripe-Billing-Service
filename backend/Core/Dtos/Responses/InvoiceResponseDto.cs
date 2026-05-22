namespace Core.Dtos.Responses
{
    public class InvoiceResponseDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string StripeInvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountDue { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string InvoicePdfUrl { get; set; }
        public string HostedInvoiceUrl { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
