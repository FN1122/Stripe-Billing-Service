namespace Core.Dtos.Requests
{
    public class SubscriptionFilterDto
    {
        public string? Status { get; set; }
        public Guid? PlanId { get; set; }
        public Guid? CustomerId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
