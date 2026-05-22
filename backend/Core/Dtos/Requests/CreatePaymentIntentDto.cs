namespace Core.Dtos.Requests
{
    public class CreatePaymentIntentDto
    {
        public Guid CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public string PaymentMethodId { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }
}
