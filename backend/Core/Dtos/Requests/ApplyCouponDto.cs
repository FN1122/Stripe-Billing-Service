namespace Core.Dtos.Requests
{
    public class ApplyCouponDto
    {
        public Guid SubscriptionId { get; set; }
        public string Code { get; set; } = string.Empty;
    }

    public class RemoveCouponDto
    {
        public Guid SubscriptionId { get; set; }
    }

    public class ValidateCouponDto
    {
        public string Code { get; set; } = string.Empty;
    }
}
