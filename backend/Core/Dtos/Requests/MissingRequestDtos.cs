// ============================================================
// Missing DTOs for Requests - All in one file for convenience
// These DTOs are referenced by services but were never created
// ============================================================

namespace Core.Dtos.Requests
{
    // === ApiCallLog DTOs ===
    public class CreateApiCallLogDto
    {
        public Guid? ApiKeyId { get; set; }
        public string Method { get; set; }
        public string Endpoint { get; set; }
        public int StatusCode { get; set; }
        public double ResponseTime { get; set; }
        public long RequestSize { get; set; }
        public long ResponseSize { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public string ErrorMessage { get; set; }
        public bool Success { get; set; }
    }

    public class ApiCallLogFilterDto
    {
        public string? Method { get; set; }
        public string? Endpoint { get; set; }
        public Guid? ApiKeyId { get; set; }
        public int? StatusCode { get; set; }
        public bool? Success { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? IpAddress { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // === ApiKey DTOs ===
    public class ApiKeyFilterDto
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class UpdateApiKeyDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Permissions { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    // === User DTOs ===
    public class UserFilterDto
    {
        public string? Search { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // === Settings DTOs ===
    public class UpdateSettingDto
    {
        public object Value { get; set; }
        public string Description { get; set; }
    }

    public class UpdateBillingSettingsDto
    {
        public string DefaultCurrency { get; set; }
        public string TaxCalculationMethod { get; set; }
        public string InvoicePrefix { get; set; }
        public bool? EnableAutoRetry { get; set; }
        public int? MaxRetryAttempts { get; set; }
        public int? GracePeriodDays { get; set; }
        public bool? SendInvoiceEmails { get; set; }
        public bool? EnableProrations { get; set; }
        public int? TrialPeriodDays { get; set; }
        public string VatId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
    }

    public class UpdateSecuritySettingsDto
    {
        public bool? RequireMfa { get; set; }
        public int? ApiKeyExpiration { get; set; }
        public int? SessionTimeout { get; set; }
        public bool? IpWhitelistEnabled { get; set; }
        public string IpWhitelist { get; set; }
        public bool? RequireHttps { get; set; }
        public bool? AllowPublicApiKeys { get; set; }
    }

    public class UpdateNotificationSettingsDto
    {
        public bool? SendPaymentConfirmations { get; set; }
        public bool? SendSubscriptionNotifications { get; set; }
        public bool? SendRefundNotifications { get; set; }
        public bool? SendInvoiceNotifications { get; set; }
        public string NotificationEmail { get; set; }
        public bool? EnableWebhookNotifications { get; set; }
        public string NotificationLanguage { get; set; }
    }

    // === Webhook DTOs ===
    public class WebhookSubscriptionFilterDto
    {
        public string? TargetUrl { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
