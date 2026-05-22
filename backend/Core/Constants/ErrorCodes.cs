namespace Core.Constants
{
    public static class ErrorCodes
    {
        public const string RecordNotFound = "RECORD_NOT_FOUND";
        public const string ValidationFailed = "VALIDATION_FAILED";
        public const string UnauthorizedAction = "UNAUTHORIZED_ACTION";
        public const string InvalidTenant = "INVALID_TENANT";
        public const string InvalidCredentials = "INVALID_CREDENTIALS";
        public const string TokenExpired = "TOKEN_EXPIRED";
        public const string TokenInvalid = "TOKEN_INVALID";
        public const string AccountDeactivated = "ACCOUNT_DEACTIVATED";
        public const string ApiKeyInvalid = "API_KEY_INVALID";
        public const string ApiKeyExpired = "API_KEY_EXPIRED";
        public const string ApiKeyDeactivated = "API_KEY_DEACTIVATED";
        public const string ApiKeyPermissionDenied = "API_KEY_PERMISSION_DENIED";
        public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
        public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
        public const string ExternalApiError = "EXTERNAL_API_ERROR";
        public const string FeatureNotAvailable = "FEATURE_NOT_AVAILABLE";
        public const string WebhookDeliveryFailed = "WEBHOOK_DELIVERY_FAILED";
        public const string WebhookSignatureInvalid = "WEBHOOK_SIGNATURE_INVALID";
        public const string DuplicateRequest = "DUPLICATE_REQUEST";
        public const string StripeError = "STRIPE_ERROR";
        public const string RefundExceedsAmount = "REFUND_EXCEEDS_AMOUNT";
        public const string SubscriptionNotActive = "SUBSCRIPTION_NOT_ACTIVE";
        public const string PlanNotAvailable = "PLAN_NOT_AVAILABLE";
    }
}
