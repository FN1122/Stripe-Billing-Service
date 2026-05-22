namespace Core.Dtos.Responses
{
    public class CustomerResponseDto
    {
        public Guid Id { get; set; }
        public string ExternalReferenceId { get; set; }
        public string StripeCustomerId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Currency { get; set; }
        public int SubscriptionCount { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
