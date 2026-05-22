namespace Core.Dtos.Responses
{
    public class ConnectedAccountResponseDto
    {
        public Guid Id { get; set; }
        public string? StripeAccountId { get; set; }
        public string? BusinessName { get; set; }
        public string? Email { get; set; }
        public string? Country { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool ChargesEnabled { get; set; }
        public bool PayoutsEnabled { get; set; }
        public bool OnboardingComplete { get; set; }
        public decimal PlatformFeePercent { get; set; }
        public decimal PlatformFeeFixed { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TransferResponseDto
    {
        public Guid Id { get; set; }
        public Guid ConnectedAccountId { get; set; }
        public string? StripeTransferId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class PlatformBalanceDto
    {
        public decimal Available { get; set; }
        public decimal Pending { get; set; }
        public string Currency { get; set; } = "usd";
    }
}
