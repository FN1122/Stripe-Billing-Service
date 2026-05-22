namespace Core.Constants
{
    public static class StripeConstants
    {
        public static class PaymentStatus
        {
            public const string Succeeded = "succeeded";
            public const string Failed = "failed";
            public const string Pending = "pending";
            public const string Refunded = "refunded";
        }

        public static class SubscriptionStatus
        {
            public const string Active = "active";
            public const string Trialing = "trialing";
            public const string PastDue = "past_due";
            public const string Canceled = "canceled";
            public const string Paused = "paused";
            public const string Unpaid = "unpaid";
            public const string Incomplete = "incomplete";
        }

        public static class InvoiceStatus
        {
            public const string Draft = "draft";
            public const string Open = "open";
            public const string Paid = "paid";
            public const string Void = "void";
            public const string Uncollectible = "uncollectible";
        }

        public static class RefundReason
        {
            public const string Duplicate = "duplicate";
            public const string Fraudulent = "fraudulent";
            public const string RequestedByCustomer = "requested_by_customer";
            public const string Other = "other";
        }

        public static class RefundStatus
        {
            public const string Pending = "pending";
            public const string Approved = "approved";
            public const string Processing = "processing";
            public const string Succeeded = "succeeded";
            public const string Failed = "failed";
            public const string Rejected = "rejected";
        }
    }
}
