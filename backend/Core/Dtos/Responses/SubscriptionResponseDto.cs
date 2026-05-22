namespace Core.Dtos.Responses
{
    public class SubscriptionResponseDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public Guid PlanId { get; set; }
        public string PlanName { get; set; }
        public decimal PlanAmount { get; set; }
        public string StripeSubscriptionId { get; set; }
        public string Status { get; set; }
        public int Quantity { get; set; }
        public DateTime CurrentPeriodStart { get; set; }
        public DateTime CurrentPeriodEnd { get; set; }
        public DateTime? TrialEnd { get; set; }
        public bool CancelAtPeriodEnd { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CancellationReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
