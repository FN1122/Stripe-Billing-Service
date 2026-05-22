namespace Core.Dtos.Requests
{
    public class CreateConnectedAccountDto
    {
        public string? Email { get; set; }
        public string? BusinessName { get; set; }
        public string? Country { get; set; } = "US";
        public string Type { get; set; } = "express";
        public decimal PlatformFeePercent { get; set; } = 10;
        public decimal PlatformFeeFixed { get; set; } = 0;
    }

    public class CreateTransferDto
    {
        public Guid ConnectedAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public string? Description { get; set; }
        public Guid? SourcePaymentId { get; set; }
    }
}
