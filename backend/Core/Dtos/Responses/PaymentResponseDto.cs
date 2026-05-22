namespace Core.Dtos.Responses
{
    public class PaymentResponseDto
    {
        public Guid Id { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string StripePaymentIntentId { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountRefunded { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentMethodLast4 { get; set; }
        public string PaymentMethodBrand { get; set; }
        public string Description { get; set; }
        public string FailureReason { get; set; }
        public string ReceiptUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
