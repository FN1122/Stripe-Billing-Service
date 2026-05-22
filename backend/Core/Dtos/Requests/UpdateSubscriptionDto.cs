namespace Core.Dtos.Requests
{
    public class UpdateSubscriptionDto
    {
        public Guid? PlanId { get; set; }
        public int? Quantity { get; set; }
        public string ProrationBehavior { get; set; } = "create_prorations";
    }
}
