namespace Core.Dtos.Responses
{
    public class TaxConfigurationResponseDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string TaxProvider { get; set; } = string.Empty;
        public bool AutomaticTax { get; set; }
        public string DefaultTaxBehavior { get; set; } = string.Empty;
        public List<TaxRegistrationItemDto> TaxRegistrations { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class TaxRegistrationItemDto
    {
        public string Country { get; set; } = string.Empty;
        public string? State { get; set; }
        public string TaxId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class TaxCalculationPreviewDto
    {
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public List<TaxLineItemDto> TaxBreakdown { get; set; } = new();
    }

    public class TaxLineItemDto
    {
        public string Jurisdiction { get; set; } = string.Empty;
        public decimal TaxRate { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class TaxReportDto
    {
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public decimal TotalTaxCollected { get; set; }
        public decimal TaxableRevenue { get; set; }
        public decimal ExemptRevenue { get; set; }
        public Dictionary<string, decimal> ByJurisdiction { get; set; } = new();
    }

    public class TaxRateDto
    {
        public string Country { get; set; } = string.Empty;
        public string? State { get; set; }
        public decimal Rate { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool Inclusive { get; set; }
    }
}
