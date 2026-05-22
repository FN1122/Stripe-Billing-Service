namespace Core.Dtos.Requests
{
    public class CreateInvoiceItemDto
    {
        public Guid CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public string Description { get; set; } = string.Empty;
        public Guid? SubscriptionId { get; set; }
        public int Quantity { get; set; } = 1;
        public string TaxBehavior { get; set; } = "unspecified"; // inclusive | exclusive | unspecified
    }
}
