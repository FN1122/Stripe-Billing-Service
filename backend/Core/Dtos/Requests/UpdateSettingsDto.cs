namespace Core.Dtos.Requests
{
    public class UpdateSettingsDto
    {
        public string WebhookCallbackUrl { get; set; }
        public decimal? AutoApproveRefundThreshold { get; set; }
        public int? RefundWindowDays { get; set; }
        public string BrandingLogo { get; set; }
        public string BrandingPrimaryColor { get; set; }
        public string InvoiceHeader { get; set; }
        public string InvoiceFooter { get; set; }
    }
}
