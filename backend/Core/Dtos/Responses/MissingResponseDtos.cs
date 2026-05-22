// ============================================================
// Missing DTOs for Responses - All in one file for convenience
// These DTOs are referenced by services but were never created
// ============================================================

namespace Core.Dtos.Responses
{
    // === ApiCallLog Response DTOs ===
    public class ApiCallLogResponseDto
    {
        public Guid Id { get; set; }
        public string Method { get; set; }
        public string Endpoint { get; set; }
        public int StatusCode { get; set; }
        public double ResponseTime { get; set; }
        public long RequestSize { get; set; }
        public long ResponseSize { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ApiCallStatsDto
    {
        public string Period { get; set; }
        public int TotalCalls { get; set; }
        public int SuccessfulCalls { get; set; }
        public int FailedCalls { get; set; }
        public decimal SuccessRate { get; set; }
        public double AverageResponseTime { get; set; }
        public string SlowestEndpoint { get; set; }
        public double SlowestResponseTime { get; set; }
        public long TotalDataTransferred { get; set; }
        public decimal AverageDataTransferred { get; set; }
        public Dictionary<string, int> TopEndpoints { get; set; } = new();
        public Dictionary<string, int> ByMethod { get; set; } = new();
        public Dictionary<int, int> ByStatusCode { get; set; } = new();
        public Dictionary<string, int> TopIpAddresses { get; set; } = new();
    }

    public class ApiUsageMetricsDto
    {
        public int Calls24h { get; set; }
        public int Calls7d { get; set; }
        public int Calls30d { get; set; }
        public decimal SuccessRate24h { get; set; }
        public decimal SuccessRate7d { get; set; }
        public decimal SuccessRate30d { get; set; }
        public double AverageResponseTime24h { get; set; }
        public double AverageResponseTime7d { get; set; }
        public double AverageResponseTime30d { get; set; }
        public long DataTransferred24h { get; set; }
        public long DataTransferred7d { get; set; }
        public long DataTransferred30d { get; set; }
        public string MostUsedEndpoint24h { get; set; }
        public string MostUsedEndpoint7d { get; set; }
        public string MostUsedEndpoint30d { get; set; }
    }

    // === ApiKey Response DTOs ===
    public class ApiKeyCreateResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; } // Only returned on creation
        public string KeyPrefix { get; set; }
        public List<string> Permissions { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ApiKeyStatsDto
    {
        public int TotalKeys { get; set; }
        public int ActiveKeys { get; set; }
        public int RevokedKeys { get; set; }
        public int ExpiredKeys { get; set; }
        public DateTime? MostRecentCreation { get; set; }
        public DateTime? MostRecentUsage { get; set; }
        public int ExpiringInNext30Days { get; set; }
    }

    // === Tenant Response DTOs ===
    public class TenantDetailResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PublicKey { get; set; }
        public bool IsActive { get; set; }
        public string StripePublishableKey { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> Features { get; set; } = new();
        public int ApiKeyCount { get; set; }
        public int WebhookSubscriptionCount { get; set; }
        public int UserCount { get; set; }
        public dynamic Metadata { get; set; }
    }

    public class TenantKeyRotationResponseDto
    {
        public Guid Id { get; set; }
        public string OldPublicKey { get; set; }
        public string NewPublicKey { get; set; }
        public DateTime RotationTime { get; set; }
        public string Message { get; set; }
    }

    public class TenantHealthCheckDto
    {
        public Guid TenantId { get; set; }
        public string Status { get; set; }
        public bool IsStripeConfigured { get; set; }
        public int CustomerCount { get; set; }
        public int SubscriptionCount { get; set; }
        public int TransactionCount24h { get; set; }
        public int SubscriptionCreations24h { get; set; }
        public decimal SuccessRate24h { get; set; }
        public int ApiKeysConfigured { get; set; }
        public int WebhooksConfigured { get; set; }
        public DateTime LastActivityAt { get; set; }
        public DateTime CheckedAt { get; set; }
    }

    // === Settings Response DTOs ===
    public class SettingValueDto
    {
        public string Key { get; set; }
        public dynamic Value { get; set; }
        public string ValueType { get; set; }
        public string Description { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class BillingSettingsDto
    {
        public string DefaultCurrency { get; set; }
        public string TaxCalculationMethod { get; set; }
        public string InvoicePrefix { get; set; }
        public bool EnableAutoRetry { get; set; }
        public int MaxRetryAttempts { get; set; }
        public int GracePeriodDays { get; set; }
        public bool SendInvoiceEmails { get; set; }
        public bool EnableProrations { get; set; }
        public int TrialPeriodDays { get; set; }
        public string VatId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
    }

    public class SecuritySettingsDto
    {
        public bool RequireMfa { get; set; }
        public int ApiKeyExpiration { get; set; }
        public int SessionTimeout { get; set; }
        public bool IpWhitelistEnabled { get; set; }
        public string IpWhitelist { get; set; }
        public bool RequireHttps { get; set; }
        public bool AllowPublicApiKeys { get; set; }
    }

    public class NotificationSettingsDto
    {
        public bool SendPaymentConfirmations { get; set; }
        public bool SendSubscriptionNotifications { get; set; }
        public bool SendRefundNotifications { get; set; }
        public bool SendInvoiceNotifications { get; set; }
        public string NotificationEmail { get; set; }
        public bool EnableWebhookNotifications { get; set; }
        public string NotificationLanguage { get; set; }
    }

    // === Webhook Delivery DTOs ===
    public class WebhookDeliveryDetailResponseDto
    {
        public Guid Id { get; set; }
        public Guid WebhookSubscriptionId { get; set; }
        public string EventType { get; set; }
        public dynamic EventData { get; set; }
        public string TargetUrl { get; set; }
        public string Status { get; set; }
        public int? StatusCode { get; set; }
        public string ResponseBody { get; set; }
        public int RetryCount { get; set; }
        public int MaxRetries { get; set; }
        public DateTime? NextRetryAt { get; set; }
        public string LastError { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? FailedAt { get; set; }
    }

    public class WebhookDeliveryFilterDto
    {
        public string Status { get; set; }
        public string EventType { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class WebhookDeliveryStatsDto
    {
        public int TotalDeliveries { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int PendingCount { get; set; }
        public decimal SuccessRate { get; set; }
        public double AverageRetries { get; set; }
        public Dictionary<string, int> ByEventType { get; set; } = new();
        public Dictionary<string, int> ByStatus { get; set; } = new();
        public int Last24HoursDeliveries { get; set; }
        public decimal Last24HoursSuccessRate { get; set; }
    }

    // === Webhook Subscription Stats ===
    public class WebhookSubscriptionStatsDto
    {
        public int TotalDeliveries { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public int PendingDeliveries { get; set; }
        public decimal SuccessRate { get; set; }
        public double AverageRetries { get; set; }
        public int Deliveries7d { get; set; }
        public decimal SuccessRate7d { get; set; }
        public DateTime? MostRecentDelivery { get; set; }
        public int EstimatedMonthlyWebhooks { get; set; }
    }

    // === Revenue Data Point ===
    public class RevenueDataPoint
    {
        public string Date { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }
}
