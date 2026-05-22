namespace Core.Dtos.Responses
{
    public class CheckoutResponseDto
    {
        public string SessionId { get; set; }
        public string Url { get; set; }
        public Guid? TransactionId { get; set; }
    }
}
