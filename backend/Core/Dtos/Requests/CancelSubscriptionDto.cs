namespace Core.Dtos.Requests
{
    public class CancelSubscriptionDto
    {
        public bool CancelAtPeriodEnd { get; set; } = true;
        public string Reason { get; set; }
    }
}
