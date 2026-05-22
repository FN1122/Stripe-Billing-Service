using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure
{
    public class BillingDbContext : DbContext
    {
        private Guid? _tenantId;

        public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options) { }

        public void SetTenantId(Guid tenantId) => _tenantId = tenantId;

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<WebhookEventInbound> WebhookEventsInbound { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; }
        public DbSet<WebhookDelivery> WebhookDeliveries { get; set; }
        public DbSet<ApiCallLog> ApiCallLogs { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<PromotionCode> PromotionCodes { get; set; }
        public DbSet<CouponRedemption> CouponRedemptions { get; set; }
        public DbSet<UsageRecord> UsageRecords { get; set; }
        public DbSet<MeterEvent> MeterEvents { get; set; }
        public DbSet<TaxConfiguration> TaxConfigurations { get; set; }
        public DbSet<TaxExemption> TaxExemptions { get; set; }
        public DbSet<DunningSchedule> DunningSchedules { get; set; }
        public DbSet<DunningStep> DunningSteps { get; set; }
        public DbSet<IdempotencyKey> IdempotencyKeys { get; set; }
        public DbSet<CustomerCredit> CustomerCredits { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<ConnectedAccount> ConnectedAccounts { get; set; }
        public DbSet<TransferRecord> TransferRecords { get; set; }
        public DbSet<EndpointRateLimitConfig> EndpointRateLimits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === Tenant ===
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => e.PublicApiKey).IsUnique().HasFilter("[PublicApiKey] IS NOT NULL");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            });

            // === User ===
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.HasOne(e => e.Tenant).WithMany(t => t.Users).HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
            });

            // === RefreshToken ===
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Token);
                entity.HasOne(e => e.User).WithMany(u => u.RefreshTokens).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            // === ApiKey ===
            modelBuilder.Entity<ApiKey>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.KeyHash).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.HasOne(e => e.Tenant).WithMany(t => t.ApiKeys).HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
            });

            // === Customer ===
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.StripeCustomerId }).IsUnique().HasFilter("[StripeCustomerId] IS NOT NULL");
                entity.HasIndex(e => new { e.TenantId, e.ExternalReferenceId });
                entity.HasIndex(e => new { e.TenantId, e.Email });
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.HasOne(e => e.Tenant).WithMany(t => t.Customers).HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
            });

            // === SubscriptionPlan ===
            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.Name });
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Tenant).WithMany(t => t.SubscriptionPlans).HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
            });

            // === Subscription ===
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasIndex(e => e.StripeSubscriptionId);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne(e => e.Tenant).WithMany(t => t.Subscriptions).HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Customer).WithMany(c => c.Subscriptions).HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Plan).WithMany(p => p.Subscriptions).HasForeignKey(e => e.PlanId).OnDelete(DeleteBehavior.Restrict);
            });

            // === PaymentTransaction ===
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.CreatedAt });
                entity.HasIndex(e => e.StripePaymentIntentId);
                entity.HasIndex(e => e.StripeCheckoutSessionId);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.AmountRefunded).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Tenant).WithMany(t => t.PaymentTransactions).HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Customer).WithMany(c => c.Transactions).HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Restrict);
            });

            // === Invoice ===
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasIndex(e => e.StripeInvoiceId);
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Tax).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
                entity.Property(e => e.AmountPaid).HasColumnType("decimal(18,2)");
                entity.Property(e => e.AmountDue).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Customer).WithMany(c => c.Invoices).HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Restrict);
            });

            // === Refund ===
            modelBuilder.Entity<Refund>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Transaction).WithMany(t => t.Refunds).HasForeignKey(e => e.TransactionId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Customer).WithMany().HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Restrict);
            });

            // === WebhookEventInbound ===
            modelBuilder.Entity<WebhookEventInbound>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.StripeEventId).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
            });

            // === AuditLog ===
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.CreatedAt });
                entity.HasIndex(e => e.UserId);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(200);
            });

            // === WebhookSubscription ===
            modelBuilder.Entity<WebhookSubscription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WebhookUrl).IsRequired().HasMaxLength(500);
                entity.HasOne(e => e.Tenant).WithMany(t => t.WebhookSubscriptions).HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
            });

            // === WebhookDelivery ===
            modelBuilder.Entity<WebhookDelivery>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.Status, e.NextRetryAt });
                entity.HasOne(e => e.WebhookSubscription).WithMany(s => s.Deliveries).HasForeignKey(e => e.WebhookSubscriptionId).OnDelete(DeleteBehavior.Cascade);
            });

            // === ApiCallLog ===
            modelBuilder.Entity<ApiCallLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.CreatedAt });
            });

            // === Setting ===
            modelBuilder.Entity<Setting>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.Key }).IsUnique();
                entity.Property(e => e.Key).IsRequired().HasMaxLength(200);
                entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
            });

            // === Coupon ===
            modelBuilder.Entity<Coupon>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.IsActive });
                entity.HasIndex(e => new { e.TenantId, e.StripeCouponId }).HasFilter("[StripeCouponId] IS NOT NULL");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.AmountOff).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PercentOff).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
            });

            // === PromotionCode ===
            modelBuilder.Entity<PromotionCode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Coupon).WithMany(c => c.PromotionCodes).HasForeignKey(e => e.CouponId).OnDelete(DeleteBehavior.Restrict);
            });

            // === CouponRedemption ===
            modelBuilder.Entity<CouponRedemption>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.CouponId });
                entity.Property(e => e.AmountDiscounted).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Coupon).WithMany(c => c.Redemptions).HasForeignKey(e => e.CouponId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Customer).WithMany().HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Restrict);
            });

            // === UsageRecord ===
            modelBuilder.Entity<UsageRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.SubscriptionId, e.Timestamp });
                entity.HasIndex(e => new { e.TenantId, e.IdempotencyKey }).HasFilter("[IdempotencyKey] IS NOT NULL");
                entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Subscription).WithMany().HasForeignKey(e => e.SubscriptionId).OnDelete(DeleteBehavior.Restrict);
            });

            // === MeterEvent ===
            modelBuilder.Entity<MeterEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.CustomerId, e.EventName });
                entity.Property(e => e.EventName).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Customer).WithMany().HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Restrict);
            });

            // === TaxConfiguration ===
            modelBuilder.Entity<TaxConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TenantId).IsUnique();
                entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
            });

            // === TaxExemption ===
            modelBuilder.Entity<TaxExemption>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.CustomerId });
                entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Customer).WithMany().HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Restrict);
            });

            // === DunningSchedule ===
            modelBuilder.Entity<DunningSchedule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasIndex(e => new { e.Status, e.NextRetryAt });
                entity.Property(e => e.AmountDue).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Subscription).WithMany().HasForeignKey(e => e.SubscriptionId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Customer).WithMany().HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Restrict);
            });

            // === DunningStep ===
            modelBuilder.Entity<DunningStep>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.SortOrder });
                entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
            });

            // === IdempotencyKey ===
            modelBuilder.Entity<IdempotencyKey>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.HasIndex(e => e.ExpiresAt);
            });

            // === CustomerCredit ===
            modelBuilder.Entity<CustomerCredit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.CustomerId });
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.BalanceAfter).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Customer).WithMany().HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Restrict);
            });

            // === EmailTemplate ===
            modelBuilder.Entity<EmailTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.TemplateKey }).IsUnique();
                entity.Property(e => e.TemplateKey).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
            });

            // === EmailLog ===
            modelBuilder.Entity<EmailLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.HasIndex(e => new { e.TenantId, e.CreatedAt });
                entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
            });

            // === ConnectedAccount ===
            modelBuilder.Entity<ConnectedAccount>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.StripeAccountId });
                entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
            });

            // === TransferRecord ===
            modelBuilder.Entity<TransferRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.ConnectedAccountId });
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
            });

            // === EndpointRateLimitConfig ===
            modelBuilder.Entity<EndpointRateLimitConfig>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.Endpoint });
                entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
