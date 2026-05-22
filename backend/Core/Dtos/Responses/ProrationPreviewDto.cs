namespace Core.Dtos.Responses
{
    public class ProrationPreviewDto
    {
        public SubscriptionPlanResponseDto CurrentPlan { get; set; }
        public SubscriptionPlanResponseDto NewPlan { get; set; }
        public decimal ProratedAmount { get; set; }
        public DateTime EffectiveDate { get; set; }
        public decimal ImmediateCharge { get; set; }
        public decimal NextInvoiceAmount { get; set; }
    }
}
