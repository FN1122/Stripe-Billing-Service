namespace Core.Dtos.Responses
{
    public class CustomerDetailResponseDto : CustomerResponseDto
    {
        public string BillingAddress { get; set; }
        public string TaxId { get; set; }
        public List<SubscriptionResponseDto> Subscriptions { get; set; } = new();
        public List<PaymentResponseDto> RecentTransactions { get; set; } = new();
        public List<InvoiceResponseDto> Invoices { get; set; } = new();
    }
}
