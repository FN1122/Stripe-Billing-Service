namespace Core.Constants
{
    public static class WebhookEvents
    {
        public static class Inbound
        {
            public const string CheckoutSessionCompleted = "checkout.session.completed";
            public const string PaymentIntentSucceeded = "payment_intent.succeeded";
            public const string PaymentIntentFailed = "payment_intent.payment_failed";
            public const string InvoicePaid = "invoice.paid";
            public const string InvoicePaymentFailed = "invoice.payment_failed";
            public const string InvoiceCreated = "invoice.created";
            public const string CustomerSubscriptionCreated = "customer.subscription.created";
            public const string CustomerSubscriptionUpdated = "customer.subscription.updated";
            public const string CustomerSubscriptionDeleted = "customer.subscription.deleted";
            public const string CustomerSubscriptionTrialWillEnd = "customer.subscription.trial_will_end";
            public const string ChargeRefunded = "charge.refunded";
            public const string ChargeSucceeded = "charge.succeeded";
            public const string ChargeFailed = "charge.failed";
            public const string CustomerCreated = "customer.created";
        }

        public static class Outbound
        {
            public const string PaymentCompleted = "payment.completed";
            public const string PaymentSucceeded = "payment.succeeded";
            public const string PaymentFailed = "payment.failed";
            public const string SubscriptionActivated = "subscription.activated";
            public const string SubscriptionUpdated = "subscription.updated";
            public const string SubscriptionUpgraded = "subscription.upgraded";
            public const string SubscriptionDowngraded = "subscription.downgraded";
            public const string SubscriptionCancelled = "subscription.cancelled";
            public const string SubscriptionTrialEnding = "subscription.trial_ending";
            public const string SubscriptionPaymentFailed = "subscription.payment_failed";
            public const string RefundProcessed = "refund.processed";
            public const string InvoicePaid = "invoice.paid";
            public const string InvoicePaymentFailed = "invoice.payment_failed";
            public const string InvoiceGenerated = "invoice.generated";
            public const string DisputeCreated = "dispute.created";
            public const string CustomerCreated = "customer.created";
            public const string CustomerUpdated = "customer.updated";
        }

        public static readonly string[] AllOutbound = new[]
        {
            Outbound.PaymentCompleted, Outbound.PaymentSucceeded, Outbound.PaymentFailed,
            Outbound.SubscriptionActivated, Outbound.SubscriptionUpdated, Outbound.SubscriptionUpgraded,
            Outbound.SubscriptionDowngraded, Outbound.SubscriptionCancelled,
            Outbound.SubscriptionTrialEnding, Outbound.SubscriptionPaymentFailed,
            Outbound.RefundProcessed, Outbound.InvoicePaid, Outbound.InvoicePaymentFailed,
            Outbound.InvoiceGenerated, Outbound.DisputeCreated,
            Outbound.CustomerCreated, Outbound.CustomerUpdated
        };
    }
}
