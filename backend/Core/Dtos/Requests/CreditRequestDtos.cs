namespace Core.Dtos.Requests
{
    public class CreateCreditDto
    {
        public Guid CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public string Description { get; set; } = string.Empty;
        public string Source { get; set; } = "manual"; // manual | promotion
    }

    public class AdjustCreditDto
    {
        public Guid CustomerId { get; set; }
        public decimal Amount { get; set; } // positive=credit, negative=debit
        public string Description { get; set; } = string.Empty;
    }

    public class RefundToCreditDto
    {
        public Guid RefundId { get; set; }
    }
}
