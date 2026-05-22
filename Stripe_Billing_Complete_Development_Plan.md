# Stripe Billing Service — Complete Full-Stack Development Plan

**Project:** Multi-Tenant SaaS Billing Platform
**Stack:** .NET 9 (C# 13) | React 19 + TypeScript + Vite | SQL Server | EF Core 9 | SignalR | Stripe.NET v47 | Bootstrap 5 + SCSS | Chart.js
**Document Version:** 2.0
**Generated:** March 01, 2026

---

## Executive Summary

This plan covers the complete implementation of the Stripe Billing Service, encompassing both the **existing features** (which are partially or fully built) and the **12 proposed new features** from the Feature Documentation. The project is divided into **5 phases across 20 development days**.

### Current State Assessment

| Layer | Status | Notes |
|-------|--------|-------|
| **Backend Entities** | ✅ Complete | All 23 entities including proposed feature entities (Coupon, UsageRecord, TaxConfiguration, PromotionCode, CouponRedemption, MeterEvent) |
| **Backend Services** | ⚠️ Partial | 21 services built for existing features; **no services for proposed features** |
| **Backend Controllers** | ⚠️ Partial | 20 controllers for existing features; **no controllers for proposed features** |
| **Frontend API Layer** | ⚠️ Partial | 16 API modules for existing features; **no API modules for proposed features** |
| **Frontend Pages** | ⚠️ Partial | 16 pages across 9 sections; **no pages for proposed features** |
| **Frontend Components** | ⚠️ Partial | 20 common components; **missing feature-specific components** |

### What Needs to Be Built

**Backend (12 new feature modules):**
- Coupon & Discount Service, DTOs, Controller, Validators
- Usage-Based Billing Service, DTOs, Controller, Validators
- Tax Calculation Service, DTOs, Controller, Validators
- Dunning Management Service, Background Job, Controller
- Credit/Balance Service, DTOs, Controller
- Email Notification Service, Templates, Background Job
- Export & Reporting Service, Controller
- Idempotency Middleware + Key Support
- Subscription Add-ons Service, Controller
- Stripe Connect Service, Controller
- Webhook Event Log Viewer Controller
- Per-Endpoint Rate Limiting Middleware

**Frontend (12 new page modules + enhancement of existing pages):**
- Coupons & Promotions management page
- Usage/Metered Billing dashboard
- Tax Configuration page
- Dunning Management page
- Customer Credits page
- Email Templates & Notification settings page
- Export & Reports center
- Subscription Add-ons page
- Stripe Connect management page
- Webhook Event Log Viewer page
- Enhanced Settings pages for new features
- Enhanced Analytics with new metrics

---

## Phase 1: HIGH Priority Features — Backend (Days 1–5)

### Day 1: Coupon & Discount Management — Backend

**Goal:** Full coupon/promotion code lifecycle with Stripe sync.

#### Backend Tasks

**1.1 DTOs (Core/Dtos/Requests)**
```
CreateCouponDto.cs
├── Name (required, max 200)
├── Type (percent_off | amount_off)
├── AmountOff (decimal?, required if type=amount_off)
├── PercentOff (decimal?, required if type=percent_off, 1-100)
├── Currency (string?, required if amount_off)
├── Duration (once | repeating | forever)
├── DurationInMonths (int?, required if duration=repeating)
├── MaxRedemptions (int?)
├── RedeemBy (DateTime?)
└── Metadata (string? JSON)

UpdateCouponDto.cs
├── Name
├── IsActive
├── MaxRedemptions
├── RedeemBy
└── Metadata

CreatePromotionCodeDto.cs
├── CouponId (Guid, required)
├── Code (string, required, max 50, alphanumeric+dash)
├── MaxRedemptions (int?)
├── ExpiresAt (DateTime?)
├── FirstTimeTransaction (bool)
├── MinimumAmount (decimal?)
├── MinimumAmountCurrency (string?)
└── Metadata (string?)

CouponFilterDto.cs
├── Page, PageSize
├── Search (name)
├── Type (percent_off | amount_off)
├── Duration
├── IsActive
├── SortBy, SortDirection
└── FromDate, ToDate
```

**1.2 Response DTOs (Core/Dtos/Responses)**
```
CouponResponseDto.cs
├── Id, TenantId, StripeCouponId
├── Name, Type, AmountOff, PercentOff, Currency
├── Duration, DurationInMonths
├── MaxRedemptions, TimesRedeemed
├── RedeemBy, IsActive
├── PromotionCodes (list)
├── Metadata, CreatedAt, UpdatedAt

PromotionCodeResponseDto.cs
├── Id, CouponId, StripePromotionCodeId
├── Code, IsActive
├── MaxRedemptions, TimesRedeemed
├── ExpiresAt, FirstTimeTransaction
├── MinimumAmount, MinimumAmountCurrency
├── CreatedAt

CouponRedemptionResponseDto.cs
├── Id, CouponId, PromotionCodeId
├── CustomerId, SubscriptionId
├── DiscountAmount, Currency
├── RedeemedAt

CouponStatsDto.cs
├── TotalCoupons, ActiveCoupons
├── TotalRedemptions, TotalDiscountAmount
├── MostUsedCoupon, RedemptionsByMonth
```

**1.3 Validators (Core/Validators)**
```
CreateCouponValidator.cs
├── Name: NotEmpty, MaxLength(200)
├── Type: Must be "percent_off" or "amount_off"
├── PercentOff: When type=percent_off, must be 1-100
├── AmountOff: When type=amount_off, must be > 0
├── Currency: When type=amount_off, must be valid ISO 4217
├── Duration: Must be "once", "repeating", or "forever"
├── DurationInMonths: When duration=repeating, must be 1-36
└── RedeemBy: Must be future date if provided

CreatePromotionCodeValidator.cs
├── CouponId: NotEmpty
├── Code: NotEmpty, MaxLength(50), Regex(alphanumeric+dash)
└── MinimumAmount: Must be >= 0 if provided
```

**1.4 Repository (Core/Repositories)**
```
CouponRepository.cs
├── GetByIdAsync(tenantId, id)
├── GetByStripeIdAsync(tenantId, stripeCouponId)
├── ListAsync(tenantId, filter) → paginated
├── CreateAsync(coupon)
├── UpdateAsync(coupon)
├── GetPromotionCodeAsync(tenantId, id)
├── GetPromotionCodeByCodeAsync(tenantId, code)
├── ListPromotionCodesAsync(tenantId, couponId)
├── CreatePromotionCodeAsync(promotionCode)
├── CreateRedemptionAsync(redemption)
├── GetRedemptionsAsync(tenantId, couponId)
├── GetStatsAsync(tenantId)
└── ValidateCouponAsync(tenantId, code) → active, not expired, under max
```

**1.5 Service Contract (Core/ServiceContracts)**
```csharp
ICouponService.cs
├── CreateCouponAsync(CreateCouponDto) → ApiResponse<CouponResponseDto>
├── GetCouponAsync(Guid id) → ApiResponse<CouponResponseDto>
├── ListCouponsAsync(CouponFilterDto) → ApiResponse<PagedResult<CouponResponseDto>>
├── UpdateCouponAsync(Guid id, UpdateCouponDto) → ApiResponse<CouponResponseDto>
├── ToggleCouponAsync(Guid id) → ApiResponse<CouponResponseDto>
├── DeleteCouponAsync(Guid id) → ApiResponse<bool>
├── CreatePromotionCodeAsync(CreatePromotionCodeDto) → ApiResponse<PromotionCodeResponseDto>
├── ListPromotionCodesAsync(Guid couponId) → ApiResponse<List<PromotionCodeResponseDto>>
├── DeactivatePromotionCodeAsync(Guid id) → ApiResponse<bool>
├── ValidateCouponCodeAsync(string code) → ApiResponse<CouponResponseDto>
├── ApplyCouponToSubscriptionAsync(Guid subscriptionId, string code) → ApiResponse<bool>
├── RemoveCouponFromSubscriptionAsync(Guid subscriptionId) → ApiResponse<bool>
├── GetRedemptionsAsync(Guid couponId) → ApiResponse<List<CouponRedemptionResponseDto>>
└── GetStatsAsync() → ApiResponse<CouponStatsDto>
```

**1.6 Service Implementation (Core/Services)**
```
CouponService.cs
├── Inject: ICouponRepository, ITenantContextProvider, Stripe.CouponService, Stripe.PromotionCodeService
├── CreateCouponAsync:
│   ├── Create Stripe coupon first (Stripe.CouponCreateOptions)
│   ├── Map response → local entity with StripeCouponId
│   ├── Save to DB
│   └── Audit log entry
├── CreatePromotionCodeAsync:
│   ├── Validate coupon exists and is active
│   ├── Create Stripe promotion code
│   ├── Save locally
│   └── Return with coupon details
├── ApplyCouponToSubscriptionAsync:
│   ├── Validate code → get coupon
│   ├── Apply via Stripe SubscriptionUpdateOptions.Coupon
│   ├── Create redemption record
│   └── Update subscription record locally
├── ValidateCouponCodeAsync:
│   ├── Lookup promotion code by code string
│   ├── Check: isActive, not expired, under maxRedemptions
│   └── Return coupon details or validation error
└── GetStatsAsync:
    ├── Query: total coupons, active, total redemptions
    ├── Sum discount amounts
    └── Group redemptions by month for chart
```

**1.7 Controller (WebAPI/Controllers/v1)**
```csharp
CouponController.cs [Route("api/v1/coupons")]
├── POST   /                      → Create coupon
├── GET    /                      → List coupons (paginated)
├── GET    /{id}                  → Get coupon details
├── PUT    /{id}                  → Update coupon
├── POST   /{id}/toggle           → Toggle active/inactive
├── DELETE /{id}                  → Soft-delete coupon
├── POST   /{id}/promotion-codes  → Create promotion code
├── GET    /{id}/promotion-codes  → List promotion codes
├── POST   /promotion-codes/{id}/deactivate → Deactivate code
├── POST   /validate              → Validate coupon code (body: {code})
├── POST   /apply                 → Apply to subscription (body: {subscriptionId, code})
├── POST   /remove                → Remove from subscription (body: {subscriptionId})
├── GET    /{id}/redemptions      → List redemptions
└── GET    /stats                 → Coupon statistics [ManagerOrAbove]
```

**1.8 Mapper (Core/Mappers)**
```
CouponMapper.cs
├── ToResponseDto(Coupon entity) → CouponResponseDto
├── ToPromotionCodeResponseDto(PromotionCode entity) → PromotionCodeResponseDto
├── ToRedemptionResponseDto(CouponRedemption entity) → CouponRedemptionResponseDto
└── ToEntity(CreateCouponDto dto) → Coupon
```

**1.9 DI Registration (Program.cs additions)**
```csharp
builder.Services.AddScoped<ICouponRepository, CouponRepository>();
builder.Services.AddScoped<ICouponService, CouponService>();
```

**1.10 EF Migration**
```bash
dotnet ef migrations add AddCouponTables -p Core -s WebAPI
dotnet ef database update -p Core -s WebAPI
```

---

### Day 2: Usage-Based / Metered Billing — Backend

**Goal:** Support metered pricing with usage reporting and real-time tracking.

#### Backend Tasks

**2.1 DTOs**
```
CreateUsageRecordDto.cs
├── SubscriptionId (Guid, required)
├── Quantity (long, required, > 0)
├── Timestamp (DateTime? defaults to UtcNow)
├── Action (increment | set, default: increment)
└── IdempotencyKey (string?, max 200)

BatchUsageRecordDto.cs
├── Records: List<CreateUsageRecordDto> (max 100 items)

UsageFilterDto.cs
├── SubscriptionId (Guid?)
├── CustomerId (Guid?)
├── FromDate, ToDate
├── Page, PageSize

CreateMeterEventDto.cs
├── CustomerId (Guid, required)
├── EventName (string, required, max 100)
├── Value (long, required)
├── Timestamp (DateTime?)
└── Properties (string? JSON)
```

**2.2 Response DTOs**
```
UsageRecordResponseDto.cs
├── Id, SubscriptionId, StripeSubscriptionItemId
├── Quantity, Timestamp, Action, IdempotencyKey
├── CreatedAt

UsageSummaryDto.cs
├── SubscriptionId, CustomerName
├── CurrentPeriodUsage, PreviousPeriodUsage
├── UsageChange (%), EstimatedCharge
├── DailyUsage (chart data: date → quantity)

MeterEventResponseDto.cs
├── Id, CustomerId, EventName, Value
├── Timestamp, Properties, CreatedAt
```

**2.3 Service Contract**
```csharp
IUsageBillingService.cs
├── ReportUsageAsync(CreateUsageRecordDto) → ApiResponse<UsageRecordResponseDto>
├── BatchReportUsageAsync(BatchUsageRecordDto) → ApiResponse<List<UsageRecordResponseDto>>
├── GetUsageAsync(UsageFilterDto) → ApiResponse<PagedResult<UsageRecordResponseDto>>
├── GetUsageSummaryAsync(Guid subscriptionId) → ApiResponse<UsageSummaryDto>
├── CreateMeterEventAsync(CreateMeterEventDto) → ApiResponse<MeterEventResponseDto>
├── GetMeterEventsAsync(Guid customerId, string? eventName) → ApiResponse<List<MeterEventResponseDto>>
└── GetUsageDashboardAsync() → ApiResponse<UsageDashboardDto>
```

**2.4 Service Implementation**
```
UsageBillingService.cs
├── ReportUsageAsync:
│   ├── Lookup subscription → get StripeSubscriptionItemId
│   ├── Check idempotency key for duplicates
│   ├── Call Stripe UsageRecordService.CreateAsync
│   ├── Save local record
│   └── Audit log
├── BatchReportUsageAsync:
│   ├── Validate all records
│   ├── Group by subscription
│   ├── Process each group with Stripe
│   └── Return results with any failures noted
├── GetUsageSummaryAsync:
│   ├── Sum current period usage
│   ├── Sum previous period usage
│   ├── Calculate estimated charge (usage × per-unit price)
│   └── Build daily chart data
└── GetUsageDashboardAsync:
    ├── Total usage across all subscriptions
    ├── Top consumers
    ├── Usage trend (30 days)
    └── Estimated revenue from metered billing
```

**2.5 Controller**
```csharp
UsageBillingController.cs [Route("api/v1/usage")]
├── POST   /report              → Report single usage
├── POST   /report/batch        → Batch report (max 100)
├── GET    /                    → List usage records (filtered)
├── GET    /summary/{subscriptionId} → Usage summary for subscription
├── POST   /meter-events        → Create meter event
├── GET    /meter-events        → List meter events
└── GET    /dashboard           → Usage billing dashboard [ManagerOrAbove]
```

**2.6 Subscription Plan Enhancement**
```
Update SubscriptionPlan entity:
├── Add: PricingModel (flat_rate | per_unit | tiered | graduated)
├── Add: UnitLabel (string?, e.g., "API call", "seat", "GB")
├── Add: TieredPricing (string? JSON array of tier objects)
├── Add: MeteredUsage (bool)
├── Add: AggregateUsage (sum | last_during_period | last_ever | max)

Update CreatePlanDto and UpdatePlanDto to include new fields
Update SubscriptionPlanService to handle metered plan creation via Stripe
```

---

### Day 3: Tax Calculation Integration — Backend

**Goal:** Automatic tax calculation with Stripe Tax, jurisdiction support, exemptions.

#### Backend Tasks

**3.1 DTOs**
```
UpdateTaxConfigurationDto.cs
├── TaxProvider (stripe_tax | manual | none)
├── AutomaticTax (bool)
├── DefaultTaxBehavior (inclusive | exclusive)
├── TaxRegistrations: List<TaxRegistrationDto>
│   ├── Country (string, ISO 3166-1)
│   ├── State (string?)
│   ├── TaxId (string, e.g., VAT number)
│   └── Type (vat | gst | sales_tax)

SetCustomerTaxExemptDto.cs
├── TaxExempt (none | exempt | reverse)
├── TaxIds: List<CustomerTaxIdDto>
│   ├── Type (eu_vat | us_ein | au_abn | etc.)
│   └── Value (string)

TaxRateOverrideDto.cs
├── Country (string)
├── State (string?)
├── Rate (decimal, 0-100)
├── Description (string)
├── Inclusive (bool)
```

**3.2 Response DTOs**
```
TaxConfigurationResponseDto.cs
├── TenantId, TaxProvider, AutomaticTax
├── DefaultTaxBehavior
├── TaxRegistrations, CreatedAt, UpdatedAt

TaxCalculationPreviewDto.cs
├── Subtotal, TaxAmount, Total
├── TaxBreakdown: List<TaxLineItem>
│   ├── Jurisdiction, TaxRate, TaxableAmount
│   ├── TaxAmount, Description

TaxReportDto.cs
├── Period (from/to)
├── TotalTaxCollected, ByJurisdiction
├── TaxableRevenue, ExemptRevenue
```

**3.3 Service**
```csharp
ITaxService.cs
├── GetConfigurationAsync() → ApiResponse<TaxConfigurationResponseDto>
├── UpdateConfigurationAsync(UpdateTaxConfigurationDto) → ApiResponse<TaxConfigurationResponseDto>
├── PreviewTaxAsync(Guid customerId, decimal amount, string currency) → ApiResponse<TaxCalculationPreviewDto>
├── SetCustomerTaxExemptAsync(Guid customerId, SetCustomerTaxExemptDto) → ApiResponse<bool>
├── AddTaxIdAsync(Guid customerId, CustomerTaxIdDto) → ApiResponse<bool>
├── RemoveTaxIdAsync(Guid customerId, string taxIdStripeId) → ApiResponse<bool>
├── GetTaxReportAsync(DateTime from, DateTime to) → ApiResponse<TaxReportDto>
└── GetTaxRatesAsync(string country) → ApiResponse<List<TaxRateDto>>
```

**3.4 Controller**
```csharp
TaxController.cs [Route("api/v1/tax")]
├── GET    /config                → Get tax configuration
├── PUT    /config                → Update tax configuration [AdminOrAbove]
├── POST   /preview               → Preview tax calculation
├── POST   /customers/{id}/exempt → Set customer tax exemption
├── POST   /customers/{id}/tax-ids → Add customer tax ID
├── DELETE /customers/{id}/tax-ids/{taxIdId} → Remove tax ID
├── GET    /report                → Tax report for period [AdminOrAbove]
└── GET    /rates                 → Get tax rates by country
```

**3.5 Integration Points**
```
Update CheckoutService: Include automatic_tax parameter
Update SubscriptionService: Include automatic_tax in subscription creation
Update InvoiceService: Tax fields in invoice response
Update PaymentService: Tax amount tracking in transactions
```

---

### Day 4: Dunning Management — Backend

**Goal:** Automated failed payment recovery with configurable retry schedules and customer notifications.

#### Backend Tasks

**4.1 New Entity**
```csharp
DunningSchedule.cs [Table("DunningSchedules")]
├── Id (Guid)
├── TenantId (Guid)
├── SubscriptionId (Guid)
├── CustomerId (Guid)
├── StripeInvoiceId (string?)
├── Status (active | paused | completed | cancelled)
├── CurrentStep (int, 0-based)
├── MaxSteps (int)
├── NextRetryAt (DateTime?)
├── LastRetryAt (DateTime?)
├── TotalRetryAttempts (int)
├── OriginalFailureDate (DateTime)
├── FailureReason (string?)
├── AmountDue (decimal)
├── Currency (string)
├── GracePeriodEndsAt (DateTime?)
├── CreatedAt, UpdatedAt

DunningStep.cs [Table("DunningSteps")]
├── Id (Guid)
├── TenantId (Guid)
├── SortOrder (int)
├── DaysAfterFailure (int)
├── Action (retry_payment | send_email | pause_subscription | cancel_subscription)
├── EmailTemplateKey (string?)
├── IsActive (bool)
├── CreatedAt
```

**4.2 DTOs**
```
DunningConfigDto.cs
├── Steps: List<DunningStepConfigDto>
│   ├── DaysAfterFailure (int)
│   ├── Action (retry_payment | send_email | pause | cancel)
│   └── EmailTemplateKey (string?)
├── GracePeriodDays (int)
├── MaxRetryAttempts (int)
├── AutoCancelAfterMaxRetries (bool)

DunningScheduleResponseDto.cs
├── All entity fields + Customer name, email
├── Steps taken so far
├── Next scheduled action

DunningDashboardDto.cs
├── ActiveDunningCount, RecoveredCount, LostCount
├── RecoveryRate (%), TotalAmountAtRisk
├── TotalRecoveredAmount
├── ByStep: count at each dunning stage
├── RecentActivity: last 10 dunning actions
```

**4.3 Service**
```csharp
IDunningService.cs
├── GetConfigAsync() → ApiResponse<DunningConfigDto>
├── UpdateConfigAsync(DunningConfigDto) → ApiResponse<DunningConfigDto>
├── GetSchedulesAsync(filter) → ApiResponse<PagedResult<DunningScheduleResponseDto>>
├── GetScheduleAsync(Guid id) → ApiResponse<DunningScheduleResponseDto>
├── PauseScheduleAsync(Guid id) → ApiResponse<bool>
├── ResumeScheduleAsync(Guid id) → ApiResponse<bool>
├── CancelScheduleAsync(Guid id) → ApiResponse<bool>
├── ManualRetryAsync(Guid id) → ApiResponse<bool>
├── GetDashboardAsync() → ApiResponse<DunningDashboardDto>
└── InitiateDunningAsync(Guid subscriptionId, string invoiceId, decimal amount, string reason) → internal
```

**4.4 Background Service**
```csharp
DunningProcessorService.cs : BackgroundService
├── ExecuteAsync:
│   ├── Poll every 5 minutes
│   ├── Get all active dunning schedules where NextRetryAt <= UtcNow
│   ├── For each:
│   │   ├── Get current step configuration
│   │   ├── Execute action (retry payment via Stripe, send email, pause/cancel sub)
│   │   ├── If payment succeeds → mark completed, update subscription status
│   │   ├── If payment fails → advance to next step or mark lost
│   │   └── Update NextRetryAt based on next step schedule
│   └── Log all actions to audit
```

**4.5 Webhook Integration**
```
Update StripeWebhookHandler:
├── On invoice.payment_failed:
│   ├── Check if dunning schedule exists for this subscription
│   ├── If not → create new DunningSchedule with step 0
│   └── If exists → verify step progression
├── On invoice.payment_succeeded:
│   ├── If active dunning schedule exists → mark completed
│   ├── Update subscription status back to active
│   └── Send recovery confirmation email (if email service exists)
```

**4.6 Controller**
```csharp
DunningController.cs [Route("api/v1/dunning")]
├── GET    /config           → Get dunning configuration [AdminOrAbove]
├── PUT    /config           → Update dunning configuration [AdminOrAbove]
├── GET    /schedules        → List active dunning schedules
├── GET    /schedules/{id}   → Get schedule details
├── POST   /schedules/{id}/pause   → Pause dunning
├── POST   /schedules/{id}/resume  → Resume dunning
├── POST   /schedules/{id}/cancel  → Cancel dunning
├── POST   /schedules/{id}/retry   → Manual retry [AdminOrAbove]
└── GET    /dashboard        → Dunning dashboard [ManagerOrAbove]
```

---

### Day 5: Idempotency + Credit/Balance System — Backend

**Goal:** Prevent duplicate charges and enable customer credit balances.

#### 5A: Idempotency Key Support

**5A.1 Middleware**
```csharp
IdempotencyMiddleware.cs
├── Intercept POST/PUT requests to payment endpoints
├── Read "Idempotency-Key" header
├── Check cache/DB for existing key
│   ├── If found → return cached response (200 with same body)
│   └── If not found → proceed, cache response after completion
├── Cache TTL: 24 hours
├── Storage: MemoryCache for speed + DB table for durability

IdempotencyKey.cs [Table("IdempotencyKeys")]
├── Key (string, PK, max 200)
├── TenantId (Guid)
├── HttpMethod, Endpoint
├── RequestHash (SHA-256 of request body)
├── ResponseStatusCode (int)
├── ResponseBody (string, JSON)
├── CreatedAt, ExpiresAt
```

**5A.2 Stripe Forwarding**
```
Update StripePaymentGateway:
├── CreateCheckoutSessionAsync: Forward Idempotency-Key to Stripe RequestOptions
├── CreatePaymentIntentAsync: Forward Idempotency-Key to Stripe RequestOptions
├── CreateRefundAsync: Forward Idempotency-Key to Stripe RequestOptions
```

#### 5B: Credit / Balance System

**5B.1 New Entity**
```csharp
CustomerCredit.cs [Table("CustomerCredits")]
├── Id (Guid)
├── TenantId (Guid)
├── CustomerId (Guid)
├── Type (credit | debit | adjustment)
├── Amount (decimal)
├── Currency (string)
├── Description (string)
├── Source (manual | refund | promotion | system)
├── ReferenceId (Guid? — links to refund, coupon, etc.)
├── BalanceAfter (decimal)
├── CreatedBy (Guid? — user who created)
├── CreatedAt
```

**5B.2 DTOs**
```
CreateCreditDto.cs
├── CustomerId (Guid, required)
├── Amount (decimal, required, > 0)
├── Currency (string, required)
├── Description (string, required)
├── Source (manual | promotion)

AdjustCreditDto.cs
├── CustomerId (Guid, required)
├── Amount (decimal, required — positive for credit, negative for debit)
├── Description (string, required)

CreditResponseDto.cs
├── All entity fields + CustomerName, CustomerEmail

CustomerBalanceDto.cs
├── CustomerId, CustomerName
├── CurrentBalance, Currency
├── TotalCredits, TotalDebits
├── RecentTransactions (last 10)
```

**5B.3 Service**
```csharp
ICreditService.cs
├── GetBalanceAsync(Guid customerId) → ApiResponse<CustomerBalanceDto>
├── AddCreditAsync(CreateCreditDto) → ApiResponse<CreditResponseDto>
├── AdjustBalanceAsync(AdjustCreditDto) → ApiResponse<CreditResponseDto>
├── GetHistoryAsync(Guid customerId, page, pageSize) → ApiResponse<PagedResult<CreditResponseDto>>
├── ApplyCreditsToInvoiceAsync(Guid customerId, Guid invoiceId) → ApiResponse<decimal> (amount applied)
├── RefundToCreditAsync(Guid refundId) → ApiResponse<CreditResponseDto>
└── GetCreditsDashboardAsync() → ApiResponse<CreditsDashboardDto>
```

**5B.4 Stripe Integration**
```
CreditService maps to Stripe Customer Balance:
├── AddCreditAsync → Stripe CustomerBalanceTransactionService.CreateAsync (negative amount = credit)
├── AdjustBalanceAsync → Stripe CustomerBalanceTransactionService.CreateAsync
├── GetBalanceAsync → Stripe CustomerService.GetAsync → customer.Balance
└── Auto-apply: Stripe automatically applies customer balance to next invoice
```

**5B.5 Controller**
```csharp
CreditController.cs [Route("api/v1/credits")]
├── GET    /customers/{customerId}/balance  → Get balance
├── POST   /customers/{customerId}/credit   → Add credit [AdminOrAbove]
├── POST   /customers/{customerId}/adjust   → Adjust balance [AdminOrAbove]
├── GET    /customers/{customerId}/history  → Credit history
├── POST   /refund-to-credit               → Convert refund to credit [AdminOrAbove]
└── GET    /dashboard                       → Credits dashboard [ManagerOrAbove]
```

---

## Phase 2: MEDIUM Priority Features — Backend (Days 6–8)

### Day 6: Email Notification Service — Backend

**Goal:** Transactional email delivery with templates and per-tenant configuration.

**6.1 New Entities**
```csharp
EmailTemplate.cs [Table("EmailTemplates")]
├── Id (Guid)
├── TenantId (Guid)
├── TemplateKey (string, max 100) — e.g., "payment.succeeded", "subscription.cancelled"
├── Subject (string, max 500)
├── HtmlBody (string, max 50000)
├── PlainTextBody (string?, max 50000)
├── IsActive (bool)
├── Variables (string? JSON array of supported variables)
├── CreatedAt, UpdatedAt

EmailLog.cs [Table("EmailLogs")]
├── Id (Guid)
├── TenantId (Guid)
├── TemplateKey (string)
├── To (string), Cc (string?), Bcc (string?)
├── Subject (string)
├── Status (queued | sent | delivered | failed | bounced)
├── Provider (sendgrid | ses | resend | smtp)
├── ProviderMessageId (string?)
├── ErrorMessage (string?)
├── SentAt (DateTime?), DeliveredAt (DateTime?)
├── CreatedAt
```

**6.2 Service**
```csharp
IEmailService.cs
├── SendAsync(SendEmailDto) → ApiResponse<EmailLog>
├── SendTemplatedAsync(string templateKey, string to, Dictionary<string,string> variables) → ApiResponse<EmailLog>
├── GetTemplatesAsync() → ApiResponse<List<EmailTemplateResponseDto>>
├── GetTemplateAsync(string templateKey) → ApiResponse<EmailTemplateResponseDto>
├── UpdateTemplateAsync(string templateKey, UpdateEmailTemplateDto) → ApiResponse<EmailTemplateResponseDto>
├── ResetTemplateAsync(string templateKey) → ApiResponse<EmailTemplateResponseDto>
├── PreviewTemplateAsync(string templateKey, Dictionary<string,string> variables) → ApiResponse<string>
├── GetEmailLogsAsync(filter) → ApiResponse<PagedResult<EmailLogResponseDto>>
└── ResendEmailAsync(Guid emailLogId) → ApiResponse<EmailLog>

// Provider interface for swappable providers
IEmailProvider.cs
├── SendAsync(string to, string subject, string htmlBody, string? plainText) → EmailSendResult
```

**6.3 Background Service**
```csharp
EmailQueueService.cs : BackgroundService
├── Process queued emails from DB
├── Retry failed sends up to 3 times
├── Log delivery status updates
```

**6.4 Default Templates (seeded on tenant creation)**
```
Templates to create:
├── payment.succeeded — "Payment Confirmation"
├── payment.failed — "Payment Failed"
├── subscription.created — "Welcome to {PlanName}"
├── subscription.cancelled — "Subscription Cancelled"
├── subscription.trial_ending — "Trial Ending Soon"
├── invoice.created — "New Invoice #{InvoiceNumber}"
├── refund.processed — "Refund Processed"
├── dunning.reminder — "Action Required: Payment Failed"
├── dunning.final_warning — "Final Notice: Subscription at Risk"
└── account.welcome — "Welcome to {TenantName}"
```

**6.5 Integration Points**
```
Wire email sending into existing services:
├── PaymentService → on payment success/failure
├── SubscriptionService → on create/cancel/trial_ending
├── RefundService → on refund approved
├── DunningService → at each dunning step
├── InvoiceService → on invoice created (with PDF attachment via QuestPDF)
```

**6.6 Controller**
```csharp
EmailController.cs [Route("api/v1/emails")]
├── GET    /templates           → List all templates
├── GET    /templates/{key}     → Get template
├── PUT    /templates/{key}     → Update template [AdminOrAbove]
├── POST   /templates/{key}/reset → Reset to default [AdminOrAbove]
├── POST   /templates/{key}/preview → Preview with sample data
├── GET    /logs                → Email delivery logs
├── POST   /logs/{id}/resend   → Resend failed email [AdminOrAbove]
├── POST   /send               → Send ad-hoc email [AdminOrAbove]
└── GET    /stats               → Email delivery stats [ManagerOrAbove]
```

---

### Day 7: Export & Reporting — Backend

**Goal:** CSV/PDF export for all data entities and scheduled report generation.

**7.1 Service**
```csharp
IExportService.cs
├── ExportTransactionsAsync(PaymentFilterDto, format) → byte[] (CSV or PDF)
├── ExportInvoicesAsync(InvoiceFilterDto, format) → byte[]
├── ExportCustomersAsync(CustomerFilterDto, format) → byte[]
├── ExportSubscriptionsAsync(SubscriptionFilterDto, format) → byte[]
├── ExportRefundsAsync(RefundFilterDto, format) → byte[]
├── ExportAuditLogAsync(AuditFilterDto, format) → byte[]
├── GenerateRevenueReportAsync(DateTime from, DateTime to) → byte[] (PDF via QuestPDF)
├── GenerateTaxReportAsync(DateTime from, DateTime to) → byte[] (PDF)
├── GetExportHistoryAsync() → ApiResponse<List<ExportLogDto>>
└── ScheduleReportAsync(ScheduleReportDto) → ApiResponse<bool>
```

**7.2 QuestPDF Integration**
```
RevenueReportDocument.cs : IDocument (QuestPDF)
├── CompanyLogo, TenantName
├── Report Period
├── Revenue Summary Table
├── Revenue by Plan Chart (rendered as image)
├── Transaction Summary
├── Top Customers Table
├── Tax Summary (if tax enabled)
├── Generated timestamp + page numbers
```

**7.3 Controller**
```csharp
ExportController.cs [Route("api/v1/exports")]
├── GET /transactions   → Export transactions (query: format=csv|pdf)
├── GET /invoices       → Export invoices
├── GET /customers      → Export customers
├── GET /subscriptions  → Export subscriptions
├── GET /refunds        → Export refunds
├── GET /audit-log      → Export audit log
├── GET /reports/revenue → Generate revenue report PDF
├── GET /reports/tax     → Generate tax report PDF
├── GET /history         → Export history
└── POST /schedule       → Schedule recurring report [AdminOrAbove]
```

---

### Day 8: Subscription Add-ons + Webhook Event Log Viewer — Backend

**Goal:** One-time charges on subscriptions and browseable webhook history.

#### 8A: Subscription Add-ons & One-Time Charges

**8A.1 DTOs**
```
CreateInvoiceItemDto.cs
├── CustomerId (Guid, required)
├── Amount (decimal, required)
├── Currency (string, required)
├── Description (string, required)
├── SubscriptionId (Guid? — attach to next subscription invoice)
├── Quantity (int, default 1)
├── TaxBehavior (inclusive | exclusive | unspecified)

InvoiceItemResponseDto.cs
├── Id, StripeInvoiceItemId
├── CustomerId, SubscriptionId
├── Amount, Currency, Description
├── Quantity, UnitAmount
├── Period (start/end)
├── CreatedAt
```

**8A.2 Service**
```csharp
IInvoiceItemService.cs
├── CreateAsync(CreateInvoiceItemDto) → ApiResponse<InvoiceItemResponseDto>
├── ListAsync(Guid? customerId, Guid? subscriptionId) → ApiResponse<List<InvoiceItemResponseDto>>
├── DeleteAsync(Guid id) → ApiResponse<bool>
└── GetUpcomingInvoiceAsync(Guid subscriptionId) → ApiResponse<InvoiceResponseDto>
```

**8A.3 Controller**
```csharp
InvoiceItemController.cs [Route("api/v1/invoice-items")]
├── POST   /                           → Create invoice item
├── GET    /                           → List invoice items
├── DELETE /{id}                       → Delete pending invoice item
└── GET    /upcoming/{subscriptionId}  → Preview upcoming invoice
```

#### 8B: Webhook Event Log Viewer

**8B.1 Service**
```csharp
IWebhookEventLogService.cs
├── GetInboundEventsAsync(filter) → ApiResponse<PagedResult<WebhookEventResponseDto>>
├── GetInboundEventAsync(Guid id) → ApiResponse<WebhookEventDetailDto>
├── ReplayEventAsync(Guid id) → ApiResponse<bool>
├── GetDeliveryLogAsync(Guid webhookSubscriptionId, filter) → ApiResponse<PagedResult<WebhookDeliveryResponseDto>>
├── GetDeliveryDetailAsync(Guid deliveryId) → ApiResponse<WebhookDeliveryDetailDto>
├── RetryDeliveryAsync(Guid deliveryId) → ApiResponse<bool>
└── GetEventStatsAsync() → ApiResponse<WebhookEventStatsDto>
```

**8B.2 Controller**
```csharp
WebhookEventController.cs [Route("api/v1/webhook-events")]
├── GET    /inbound           → List inbound Stripe events (filtered)
├── GET    /inbound/{id}      → Get event detail with payload
├── POST   /inbound/{id}/replay → Replay event processing
├── GET    /deliveries        → List outbound deliveries (filtered)
├── GET    /deliveries/{id}   → Get delivery detail
├── POST   /deliveries/{id}/retry → Retry failed delivery
└── GET    /stats             → Event processing statistics [ManagerOrAbove]
```

---

## Phase 3: LOW Priority Features — Backend (Days 9–10)

### Day 9: Stripe Connect Integration — Backend

**9.1 New Entities**
```csharp
ConnectedAccount.cs [Table("ConnectedAccounts")]
├── Id, TenantId, StripeAccountId
├── BusinessName, Email
├── Country, Type (standard | express | custom)
├── ChargesEnabled, PayoutsEnabled
├── OnboardingComplete
├── PlatformFeePercent, PlatformFeeFixed
├── Metadata, CreatedAt, UpdatedAt

TransferRecord.cs [Table("TransferRecords")]
├── Id, TenantId, ConnectedAccountId
├── StripeTransferId, Amount, Currency
├── Description, Status
├── SourcePaymentId
├── CreatedAt
```

**9.2 Service + Controller**
```csharp
IConnectService.cs → ConnectController.cs [Route("api/v1/connect")]
├── POST   /accounts           → Create connected account
├── GET    /accounts            → List connected accounts
├── GET    /accounts/{id}       → Get account details
├── POST   /accounts/{id}/onboarding-link → Generate onboarding link
├── POST   /accounts/{id}/dashboard-link  → Generate dashboard link
├── POST   /transfers           → Create transfer to connected account
├── GET    /transfers           → List transfers
├── GET    /balance             → Get platform balance
└── GET    /payouts             → List payouts to connected accounts
```

### Day 10: Per-Endpoint Rate Limiting — Backend

**10.1 Enhanced Rate Limiting**
```csharp
EndpointRateLimitConfig.cs [Table("EndpointRateLimits")]
├── Id, TenantId
├── Endpoint (string, e.g., "POST /api/v1/payments/*")
├── RequestsPerMinute (int)
├── BurstLimit (int?)
├── IsActive (bool)

EnhancedRateLimitMiddleware.cs
├── Check endpoint-specific limits first
├── Fall back to API key global limit
├── Add response headers:
│   ├── X-RateLimit-Limit
│   ├── X-RateLimit-Remaining
│   ├── X-RateLimit-Reset
│   └── Retry-After (on 429)
```

**10.2 Controller**
```csharp
RateLimitController.cs [Route("api/v1/rate-limits")]
├── GET    /                → List endpoint rate limits
├── POST   /                → Create endpoint rate limit [AdminOrAbove]
├── PUT    /{id}            → Update rate limit [AdminOrAbove]
├── DELETE /{id}            → Delete rate limit [AdminOrAbove]
└── GET    /usage           → Rate limit usage stats
```

---

## Phase 4: Frontend Implementation (Days 11–17)

### Day 11: Frontend Foundation & Shared Infrastructure

**Goal:** Set up API modules, types, and reusable components for all new features.

**11.1 New API Modules (frontend/src/api/)**
```
couponApi.ts
├── createCoupon(data) → POST /api/v1/coupons
├── getCoupons(filter) → GET /api/v1/coupons
├── getCoupon(id) → GET /api/v1/coupons/{id}
├── updateCoupon(id, data) → PUT /api/v1/coupons/{id}
├── toggleCoupon(id) → POST /api/v1/coupons/{id}/toggle
├── deleteCoupon(id) → DELETE /api/v1/coupons/{id}
├── createPromotionCode(couponId, data) → POST /api/v1/coupons/{id}/promotion-codes
├── getPromotionCodes(couponId) → GET /api/v1/coupons/{id}/promotion-codes
├── deactivatePromotionCode(id) → POST /api/v1/coupons/promotion-codes/{id}/deactivate
├── validateCode(code) → POST /api/v1/coupons/validate
├── applyCoupon(subscriptionId, code) → POST /api/v1/coupons/apply
├── getRedemptions(couponId) → GET /api/v1/coupons/{id}/redemptions
└── getStats() → GET /api/v1/coupons/stats

usageApi.ts
├── reportUsage(data) → POST /api/v1/usage/report
├── batchReportUsage(data) → POST /api/v1/usage/report/batch
├── getUsageRecords(filter) → GET /api/v1/usage
├── getUsageSummary(subscriptionId) → GET /api/v1/usage/summary/{id}
├── createMeterEvent(data) → POST /api/v1/usage/meter-events
├── getMeterEvents(filter) → GET /api/v1/usage/meter-events
└── getUsageDashboard() → GET /api/v1/usage/dashboard

taxApi.ts
├── getConfig() → GET /api/v1/tax/config
├── updateConfig(data) → PUT /api/v1/tax/config
├── previewTax(customerId, amount, currency) → POST /api/v1/tax/preview
├── setCustomerExempt(customerId, data) → POST /api/v1/tax/customers/{id}/exempt
├── addTaxId(customerId, data) → POST /api/v1/tax/customers/{id}/tax-ids
├── getTaxReport(from, to) → GET /api/v1/tax/report
└── getTaxRates(country) → GET /api/v1/tax/rates

dunningApi.ts
├── getConfig() → GET /api/v1/dunning/config
├── updateConfig(data) → PUT /api/v1/dunning/config
├── getSchedules(filter) → GET /api/v1/dunning/schedules
├── getSchedule(id) → GET /api/v1/dunning/schedules/{id}
├── pauseSchedule(id) → POST /api/v1/dunning/schedules/{id}/pause
├── resumeSchedule(id) → POST /api/v1/dunning/schedules/{id}/resume
├── cancelSchedule(id) → POST /api/v1/dunning/schedules/{id}/cancel
├── manualRetry(id) → POST /api/v1/dunning/schedules/{id}/retry
└── getDashboard() → GET /api/v1/dunning/dashboard

creditApi.ts
├── getBalance(customerId) → GET /api/v1/credits/customers/{id}/balance
├── addCredit(customerId, data) → POST /api/v1/credits/customers/{id}/credit
├── adjustBalance(customerId, data) → POST /api/v1/credits/customers/{id}/adjust
├── getHistory(customerId, page, pageSize) → GET /api/v1/credits/customers/{id}/history
├── refundToCredit(data) → POST /api/v1/credits/refund-to-credit
└── getDashboard() → GET /api/v1/credits/dashboard

emailApi.ts
├── getTemplates() → GET /api/v1/emails/templates
├── getTemplate(key) → GET /api/v1/emails/templates/{key}
├── updateTemplate(key, data) → PUT /api/v1/emails/templates/{key}
├── resetTemplate(key) → POST /api/v1/emails/templates/{key}/reset
├── previewTemplate(key, variables) → POST /api/v1/emails/templates/{key}/preview
├── getEmailLogs(filter) → GET /api/v1/emails/logs
├── resendEmail(id) → POST /api/v1/emails/logs/{id}/resend
└── getStats() → GET /api/v1/emails/stats

exportApi.ts
├── exportTransactions(filter, format) → GET /api/v1/exports/transactions
├── exportInvoices(filter, format) → GET /api/v1/exports/invoices
├── exportCustomers(filter, format) → GET /api/v1/exports/customers
├── exportSubscriptions(filter, format) → GET /api/v1/exports/subscriptions
├── exportRefunds(filter, format) → GET /api/v1/exports/refunds
├── exportAuditLog(filter, format) → GET /api/v1/exports/audit-log
├── generateRevenueReport(from, to) → GET /api/v1/exports/reports/revenue
├── generateTaxReport(from, to) → GET /api/v1/exports/reports/tax
└── getExportHistory() → GET /api/v1/exports/history

invoiceItemApi.ts
├── create(data) → POST /api/v1/invoice-items
├── list(customerId?, subscriptionId?) → GET /api/v1/invoice-items
├── delete(id) → DELETE /api/v1/invoice-items/{id}
└── getUpcomingInvoice(subscriptionId) → GET /api/v1/invoice-items/upcoming/{id}

webhookEventApi.ts
├── getInboundEvents(filter) → GET /api/v1/webhook-events/inbound
├── getInboundEvent(id) → GET /api/v1/webhook-events/inbound/{id}
├── replayEvent(id) → POST /api/v1/webhook-events/inbound/{id}/replay
├── getDeliveries(filter) → GET /api/v1/webhook-events/deliveries
├── getDelivery(id) → GET /api/v1/webhook-events/deliveries/{id}
├── retryDelivery(id) → POST /api/v1/webhook-events/deliveries/{id}/retry
└── getStats() → GET /api/v1/webhook-events/stats

connectApi.ts (if Stripe Connect enabled)
├── createAccount(data) → POST /api/v1/connect/accounts
├── getAccounts() → GET /api/v1/connect/accounts
├── getAccount(id) → GET /api/v1/connect/accounts/{id}
├── getOnboardingLink(id) → POST /api/v1/connect/accounts/{id}/onboarding-link
├── createTransfer(data) → POST /api/v1/connect/transfers
├── getTransfers() → GET /api/v1/connect/transfers
└── getBalance() → GET /api/v1/connect/balance
```

**11.2 New TypeScript Types (frontend/src/types/)**
```
coupon.ts, usage.ts, tax.ts, dunning.ts, credit.ts,
email.ts, export.ts, invoiceItem.ts, webhookEvent.ts, connect.ts
```

**11.3 New Common Components**
```
components/common/
├── CouponBadge.tsx, UsageChart.tsx, DunningTimeline.tsx
├── CreditBalance.tsx, EmailPreview.tsx, ExportButton.tsx
├── TaxBadge.tsx, CodeCopyField.tsx, ProgressSteps.tsx, AmountInput.tsx
```

---

### Days 12–17: Page Implementation

See the detailed page specifications in the main plan document for:
- Day 12: Coupons & Promotions page
- Day 13: Usage Billing + Tax Configuration pages
- Day 14: Dunning Management + Credits pages
- Day 15: Email Templates + Webhook Event Viewer pages
- Day 16: Export Center + Existing page enhancements
- Day 17: Stripe Connect + Navigation & routing updates

---

## Phase 5: Integration, Testing & Documentation (Days 18–20)

### Day 18: SignalR Real-Time Integration
### Day 19: End-to-End Testing & API Verification
### Day 20: Documentation & Deployment

---

## File Inventory Summary

### New Backend Files (~65 files)

| Category | Files | Location |
|----------|-------|----------|
| DTOs (Request) | 15 | Core/Dtos/Requests/ |
| DTOs (Response) | 12 | Core/Dtos/Responses/ |
| Validators | 6 | Core/Validators/ |
| Repository Contracts | 6 | Core/RepositoryContracts/ |
| Repositories | 6 | Core/Repositories/ |
| Service Contracts | 8 | Core/ServiceContracts/ |
| Services | 8 | Core/Services/ |
| Controllers | 8 | WebAPI/Controllers/v1/ |
| Mappers | 4 | Core/Mappers/ |
| Middleware | 2 | WebAPI/Middleware/ |
| Background Services | 2 | WebAPI/BackgroundServices/ |
| Entities | 6 | Core/Infrastructure/ |

### New Frontend Files (~45 files)

| Category | Files | Location |
|----------|-------|----------|
| API Modules | 10 | src/api/ |
| Type Definitions | 10 | src/types/ |
| Pages | 10 | src/pages/ |
| Components | 10 | src/components/common/ |
| Hooks | 3 | src/hooks/ |
| SCSS Files | 5 | alongside components |

### Modified Files (~30 files)

| File | Changes |
|------|---------|
| Program.cs | DI registration for ~15 new services |
| BillingDbContext.cs | DbSet for 6 new entities |
| AppRoutes.tsx | 10 new route entries |
| DashboardLayout sidebar | New navigation items |
| StripeWebhookHandler.cs | Dunning integration |
| StripePaymentGateway.cs | Idempotency + tax support |
| 8 existing frontend pages | Enhancements |
| SettingsPage.tsx | New tabs |

---

*End of Development Plan*
