namespace Core.Dtos.Responses
{
    public class RefundResponseDto
    {
        public Guid Id { get; set; }
        public Guid TransactionId { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string StripeRefundId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Reason { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
