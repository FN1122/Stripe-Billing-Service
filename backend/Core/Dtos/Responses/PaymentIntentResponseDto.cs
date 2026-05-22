namespace Core.Dtos.Responses
{
    public class PaymentIntentResponseDto
    {
        public string PaymentIntentId { get; set; }
        public string ClientSecret { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public Guid? TransactionId { get; set; }
    }
}
