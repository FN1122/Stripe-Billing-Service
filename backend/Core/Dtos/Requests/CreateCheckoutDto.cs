namespace Core.Dtos.Requests
{
    public class CreateCheckoutDto
    {
        public Guid? CustomerId { get; set; }
        public string ExternalReferenceId { get; set; }
        public string CustomerEmail { get; set; }
        public List<CheckoutLineItem> LineItems { get; set; } = new();
        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }
        public string Mode { get; set; } = "payment"; // payment | subscription
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class CheckoutLineItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public int Quantity { get; set; } = 1;
        public string StripePriceId { get; set; } // Optional: use existing price
    }
}
