namespace Core.Dtos.Responses
{
    public class DunningScheduleResponseDto
    {
        public Guid Id { get; set; }
        public Guid SubscriptionId { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? StripeInvoiceId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CurrentStep { get; set; }
        public int MaxSteps { get; set; }
        public DateTime? NextRetryAt { get; set; }
        public DateTime? LastRetryAt { get; set; }
        public int TotalRetryAttempts { get; set; }
        public DateTime OriginalFailureDate { get; set; }
        public string? FailureReason { get; set; }
        public decimal AmountDue { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime? GracePeriodEndsAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DunningDashboardDto
    {
        public int ActiveDunningCount { get; set; }
        public int RecoveredCount { get; set; }
        public int LostCount { get; set; }
        public decimal RecoveryRate { get; set; }
        public decimal TotalAmountAtRisk { get; set; }
        public decimal TotalRecoveredAmount { get; set; }
        public Dictionary<string, int> ByStep { get; set; } = new();
        public List<DunningScheduleResponseDto> RecentActivity { get; set; } = new();
    }
}
