namespace Core.Dtos.Requests
{
    public class CreateSubscriptionDto
    {
        public Guid? CustomerId { get; set; }
        public string ExternalReferenceId { get; set; }
        public Guid PlanId { get; set; }
        public int Quantity { get; set; } = 1;
        public int? TrialDays { get; set; }
        public string CouponCode { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }
}
