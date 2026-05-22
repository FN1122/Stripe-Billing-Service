namespace Core.Dtos.Responses
{
    public class CreditResponseDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public Guid? ReferenceId { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CustomerBalanceDto
    {
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public decimal CurrentBalance { get; set; }
        public string Currency { get; set; } = "usd";
        public decimal TotalCredits { get; set; }
        public decimal TotalDebits { get; set; }
        public List<CreditResponseDto> RecentTransactions { get; set; } = new();
    }

    public class CreditsDashboardDto
    {
        public decimal TotalOutstandingCredits { get; set; }
        public int CustomersWithCredits { get; set; }
        public decimal TotalCreditsIssued { get; set; }
        public decimal TotalCreditsUsed { get; set; }
    }
}
