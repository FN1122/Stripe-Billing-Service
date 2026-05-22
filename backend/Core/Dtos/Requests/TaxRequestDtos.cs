namespace Core.Dtos.Requests
{
    public class UpdateTaxConfigurationDto
    {
        public string TaxProvider { get; set; } = "stripe_tax"; // stripe_tax | manual | none
        public bool AutomaticTax { get; set; }
        public string DefaultTaxBehavior { get; set; } = "exclusive"; // inclusive | exclusive
        public List<TaxRegistrationDto> TaxRegistrations { get; set; } = new();
    }

    public class TaxRegistrationDto
    {
        public string Country { get; set; } = string.Empty;
        public string? State { get; set; }
        public string TaxId { get; set; } = string.Empty;
        public string Type { get; set; } = "vat"; // vat | gst | sales_tax
    }

    public class SetCustomerTaxExemptDto
    {
        public string TaxExempt { get; set; } = "none"; // none | exempt | reverse
        public List<CustomerTaxIdDto> TaxIds { get; set; } = new();
    }

    public class CustomerTaxIdDto
    {
        public string Type { get; set; } = string.Empty; // eu_vat | us_ein | au_abn etc.
        public string Value { get; set; } = string.Empty;
    }

    public class TaxPreviewRequestDto
    {
        public Guid CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
    }

    public class TaxReportFilterDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
