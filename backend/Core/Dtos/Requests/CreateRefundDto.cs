namespace Core.Dtos.Requests
{
    public class CreateRefundDto
    {
        public Guid TransactionId { get; set; }
        public decimal? Amount { get; set; } // null = full refund
        public string Reason { get; set; } = "requested_by_customer";
        public string Notes { get; set; }
    }
}
