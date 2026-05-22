# Stripe Billing Service — Complete Development Plan

## Document Info
- **Generated:** March 01, 2026
- **Based on:** Stripe_Billing_Service_Feature_Documentation.docx
- **Stack:** .NET 9 / EF Core 9 / SQL Server / Stripe.net v47 / React 19 / TypeScript / Vite / Bootstrap 5

---

## Table of Contents

1. [Project Status Summary](#1-project-status-summary)
2. [Phase 1 — HIGH Priority Features](#2-phase-1--high-priority-features)
3. [Phase 2 — MEDIUM Priority Features](#3-phase-2--medium-priority-features)
4. [Phase 3 — LOW Priority Features](#4-phase-3--low-priority-features)
5. [Backend Implementation Details](#5-backend-implementation-details)
6. [Frontend Implementation Details](#6-frontend-implementation-details)
7. [Database Migration Plan](#7-database-migration-plan)
8. [Testing Strategy](#8-testing-strategy)
9. [File-by-File Checklist](#9-file-by-file-checklist)

---

## 1. Project Status Summary

### Existing (Complete) — 24 Features

| Feature | Backend | Frontend | Status |
|---------|---------|----------|--------|
| JWT Auth + RBAC | ✅ | ✅ | Complete |
| API Key Auth | ✅ | ✅ | Complete |
| HMAC Signature | ✅ | N/A | Complete |
| Multi-Tenant Management | ✅ | ✅ | Complete |
| Customer Management | ✅ | ✅ | Complete |
| Subscription Plans | ✅ | ✅ | Complete |
| Subscription Lifecycle | ✅ | ✅ | Complete |
| Checkout Sessions | ✅ | ✅ | Complete |
| Payment Intents | ✅ | ✅ | Complete |
| Invoice Management | ✅ | ✅ | Complete |
| Refund Management | ✅ | ✅ | Complete |
| Inbound Webhooks | ✅ | N/A | Complete |
| Outbound Webhooks | ✅ | ✅ | Complete |
| Revenue Analytics | ✅ | ✅ | Complete |
| Dashboards | ✅ | ✅ | Complete |
| Real-Time SignalR | ✅ | ✅ | Complete |
| Settings Management | ✅ | ✅ | Complete |
| API Key Management | ✅ | ✅ | Complete |
| User Management | ✅ | ✅ | Complete |
| Audit Logging | ✅ | ✅ | Complete |
| API Call Logging | ✅ | ✅ | Complete |
| Rate Limiting | ✅ | N/A | Complete |
| Webhook Dispatch BG | ✅ | N/A | Complete |
| Webhook Retry BG | ✅ | N/A | Complete |

### Proposed (New) — 12 Features

| # | Feature | Priority | Category |
|---|---------|----------|----------|
| 1 | Coupon & Discount Management | 🔴 HIGH | Payments |
| 2 | Usage-Based / Metered Billing | 🔴 HIGH | Payments |
| 3 | Tax Calculation Integration | 🔴 HIGH | Payments |
| 4 | Dunning Management | 🔴 HIGH | Recovery |
| 5 | Credit / Balance System | 🟡 MEDIUM | Payments |
| 6 | Email Notification Service | 🟡 MEDIUM | Comms |
| 7 | Export & Reporting | 🟡 MEDIUM | Analytics |
| 8 | Idempotency Key Support | 🟡 MEDIUM | Payments |
| 9 | Subscription Add-ons | 🟢 LOW | Payments |
| 10 | Stripe Connect Integration | 🟢 LOW | Payments |
| 11 | Webhook Event Log Viewer | 🟢 LOW | Webhooks |
| 12 | Per-Endpoint Rate Limiting | 🟢 LOW | Security |

---

## 2. Phase 1 — HIGH Priority Features

### 2.1 Coupon & Discount Management

**Backend:**

| Layer | File | Description |
|-------|------|-------------|
| Entity | `Core/Infrastructure/Coupon.cs` | Coupon entity: Id, TenantId, StripeCouponId, Name, Type (percentage/fixed), AmountOff, PercentOff, Currency, Duration (once/repeating/forever), DurationInMonths, MaxRedemptions, TimesRedeemed, RedeemBy, IsActive, Metadata, CreatedAt |
| Entity | `Core/Infrastructure/PromotionCode.cs` | PromotionCode entity: Id, TenantId, CouponId, StripePromotionCodeId, Code, MaxRedemptions, TimesRedeemed, ExpiresAt, IsActive, Restrictions (JSON), CreatedAt |
| Entity | `Core/Infrastructure/CouponRedemption.cs` | CouponRedemption entity: Id, TenantId, CouponId, PromotionCodeId, CustomerId, SubscriptionId, StripeDiscountId, AmountDiscounted, RedeemedAt |
| DTO Request | `Core/Dtos/Requests/CreateCouponDto.cs` | Name, Type, AmountOff?, PercentOff?, Currency?, Duration, DurationInMonths?, MaxRedemptions?, RedeemBy?, Metadata |
| DTO Request | `Core/Dtos/Requests/UpdateCouponDto.cs` | Name?, Metadata?, IsActive? |
| DTO Request | `Core/Dtos/Requests/CreatePromotionCodeDto.cs` | CouponId, Code, MaxRedemptions?, ExpiresAt?, Restrictions? |
| DTO Request | `Core/Dtos/Requests/ApplyCouponDto.cs` | SubscriptionId or CustomerId, PromotionCode or CouponId |
| DTO Request | `Core/Dtos/Requests/CouponFilterDto.cs` | Page, PageSize, Search?, Type?, Duration?, IsActive? |
| DTO Response | `Core/Dtos/Responses/CouponResponseDto.cs` | All coupon fields + redemption count |
| DTO Response | `Core/Dtos/Responses/PromotionCodeResponseDto.cs` | All promo code fields + coupon details |
| DTO Response | `Core/Dtos/Responses/CouponStatsDto.cs` | TotalCoupons, ActiveCoupons, TotalRedemptions, TotalDiscountAmount, TopCoupons[] |
| Interface | `Core/ServiceContracts/ICouponService.cs` | CreateCouponAsync, GetCouponAsync, ListCouponsAsync, UpdateCouponAsync, DeleteCouponAsync, CreatePromotionCodeAsync, ListPromotionCodesAsync, ApplyCouponAsync, RemoveCouponAsync, GetStatsAsync |
| Service | `Core/Services/CouponService.cs` | Full implementation with Stripe Coupon/PromotionCode API sync |
| Validator | `Core/Validators/CreateCouponValidator.cs` | FluentValidation rules |
| Validator | `Core/Validators/CreatePromotionCodeValidator.cs` | FluentValidation rules |
| Mapper | `Core/Mappers/CouponMapper.cs` | Entity ↔ DTO mapping |
| Repository | `Core/Repositories/CouponRepository.cs` | CRUD + filtering + pagination |
| Repo Contract | `Core/RepositoryContracts/ICouponRepository.cs` | Interface |
| Controller | `WebAPI/Controllers/v1/CouponController.cs` | REST endpoints: POST/GET/PUT/DELETE /api/v1/coupons, POST /api/v1/coupons/{id}/promotion-codes, GET /api/v1/coupons/promotion-codes, POST /api/v1/coupons/apply, DELETE /api/v1/coupons/{id}/remove, GET /api/v1/coupons/stats |

**Frontend:**

| Layer | File | Description |
|-------|------|-------------|
| Type | `src/types/coupon.ts` | Coupon, PromotionCode, CouponRedemption, CouponStats, CreateCouponRequest, CreatePromotionCodeRequest interfaces |
| API | `src/api/couponApi.ts` | All CRUD + apply/remove/stats endpoints |
| Page | `src/pages/billing/CouponsPage.tsx` | List coupons with filters, create/edit modals, promotion code management, stats cards |
| Component | `src/components/common/CouponBadge.tsx` | Badge showing discount type & amount |
| Route | Update `AppRoutes.tsx` | Add `/coupons` route |
| Sidebar | Update `Sidebar.tsx` | Add Coupons nav item under BILLING |

**API Endpoints:**

```
POST   /api/v1/coupons                         Create coupon (+ Stripe)
GET    /api/v1/coupons                          List coupons (paginated, filtered)
GET    /api/v1/coupons/{id}                     Get coupon details
PUT    /api/v1/coupons/{id}                     Update coupon
DELETE /api/v1/coupons/{id}                     Deactivate coupon
POST   /api/v1/coupons/{id}/promotion-codes     Create promotion code
GET    /api/v1/coupons/promotion-codes           List all promotion codes
POST   /api/v1/coupons/apply                    Apply coupon to subscription/customer
DELETE /api/v1/coupons/{subscriptionId}/remove   Remove discount from subscription
GET    /api/v1/coupons/stats                    Coupon usage statistics
```

---

### 2.2 Usage-Based / Metered Billing

**Backend:**

| Layer | File | Description |
|-------|------|-------------|
| Entity | `Core/Infrastructure/UsageRecord.cs` | Id, TenantId, SubscriptionId, SubscriptionItemId, StripeUsageRecordId, Quantity, Timestamp, Action (set/increment), IdempotencyKey, CreatedAt |
| Entity | `Core/Infrastructure/MeterEvent.cs` | Id, TenantId, CustomerId, EventName, Value, Timestamp, Properties (JSON), CreatedAt |
| DTO Request | `Core/Dtos/Requests/ReportUsageDto.cs` | SubscriptionId, Quantity, Timestamp?, Action, IdempotencyKey? |
| DTO Request | `Core/Dtos/Requests/CreateMeteredPlanDto.cs` | Extends CreatePlanDto: BillingScheme (per_unit/tiered), TiersMode?, Tiers[], UsageType (metered/licensed), AggregateUsage (sum/last_during_period/last_ever/max) |
| DTO Request | `Core/Dtos/Requests/UsageFilterDto.cs` | Page, PageSize, SubscriptionId?, CustomerId?, StartDate?, EndDate? |
| DTO Response | `Core/Dtos/Responses/UsageRecordResponseDto.cs` | All usage record fields |
| DTO Response | `Core/Dtos/Responses/UsageSummaryDto.cs` | SubscriptionId, CurrentPeriodUsage, EstimatedCost, UsageByDay[], BillingPeriodStart, BillingPeriodEnd |
| Interface | `Core/ServiceContracts/IUsageBillingService.cs` | ReportUsageAsync, GetUsageSummaryAsync, ListUsageRecordsAsync, CreateMeteredPlanAsync |
| Service | `Core/Services/UsageBillingService.cs` | Stripe UsageRecord API integration, metered subscription item management |
| Validator | `Core/Validators/ReportUsageValidator.cs` | FluentValidation |
| Repository | `Core/Repositories/UsageRecordRepository.cs` | CRUD + aggregation queries |
| Repo Contract | `Core/RepositoryContracts/IUsageRecordRepository.cs` | Interface |
| Controller | `WebAPI/Controllers/v1/UsageController.cs` | REST endpoints |

**Frontend:**

| Layer | File | Description |
|-------|------|-------------|
| Type | `src/types/usage.ts` | UsageRecord, UsageSummary, MeterEvent, ReportUsageRequest interfaces |
| API | `src/api/usageApi.ts` | Report usage, get summaries, list records |
| Page | `src/pages/billing/UsagePage.tsx` | Usage dashboard with charts, report usage form, usage history table |
| Component | `src/components/common/UsageChart.tsx` | Bar/line chart for usage over time |
| Route | Update `AppRoutes.tsx` | Add `/usage` route |
| Sidebar | Update `Sidebar.tsx` | Add Usage nav item under BILLING |

**API Endpoints:**

```
POST   /api/v1/usage/report                    Report usage for subscription
GET    /api/v1/usage/summary/{subscriptionId}   Get usage summary for subscription
GET    /api/v1/usage/records                    List usage records (paginated)
POST   /api/v1/plans/metered                    Create metered billing plan
```

---

### 2.3 Tax Calculation Integration

**Backend:**

| Layer | File | Description |
|-------|------|-------------|
| Entity | `Core/Infrastructure/TaxConfiguration.cs` | Id, TenantId, Provider (stripe_tax/taxjar/avalara), IsEnabled, AutoCalculate, DefaultTaxBehavior (inclusive/exclusive), FallbackTaxRate, RegistrationNumbers (JSON), CreatedAt |
| Entity | `Core/Infrastructure/TaxExemption.cs` | Id, TenantId, CustomerId, ExemptionType (exempt/reverse/none), CertificateId, ValidFrom, ValidTo, CreatedAt |
| DTO Request | `Core/Dtos/Requests/UpdateTaxConfigDto.cs` | Provider?, IsEnabled?, AutoCalculate?, DefaultTaxBehavior?, FallbackTaxRate?, RegistrationNumbers? |
| DTO Request | `Core/Dtos/Requests/CreateTaxExemptionDto.cs` | CustomerId, ExemptionType, CertificateId?, ValidFrom?, ValidTo? |
| DTO Request | `Core/Dtos/Requests/TaxCalculationRequestDto.cs` | CustomerId, LineItems[], ShippingAddress? |
| DTO Response | `Core/Dtos/Responses/TaxConfigResponseDto.cs` | Full config + supported providers |
| DTO Response | `Core/Dtos/Responses/TaxCalculationResponseDto.cs` | LineItems with tax breakdown, TotalTax, TotalAmount |
| DTO Response | `Core/Dtos/Responses/TaxReportDto.cs` | TaxByJurisdiction[], TotalCollected, Period |
| Interface | `Core/ServiceContracts/ITaxService.cs` | GetConfigAsync, UpdateConfigAsync, CalculateTaxAsync, CreateExemptionAsync, ListExemptionsAsync, DeleteExemptionAsync, GetTaxReportAsync |
| Service | `Core/Services/TaxService.cs` | Stripe Tax API integration, tax calculation, exemption management |
| Validator | `Core/Validators/UpdateTaxConfigValidator.cs` | FluentValidation |
| Controller | `WebAPI/Controllers/v1/TaxController.cs` | REST endpoints |

**Frontend:**

| Layer | File | Description |
|-------|------|-------------|
| Type | `src/types/tax.ts` | TaxConfig, TaxExemption, TaxCalculation, TaxReport interfaces |
| API | `src/api/taxApi.ts` | Config, exemptions, calculations, reports |
| Page | `src/pages/settings/TaxSettingsPage.tsx` | Tax config form, exemption management, tax report viewer |
| Route | Update `AppRoutes.tsx` | Add `/settings/tax` route |

**API Endpoints:**

```
GET    /api/v1/tax/config                       Get tax configuration
PUT    /api/v1/tax/config                       Update tax configuration
POST   /api/v1/tax/calculate                    Calculate tax for items
POST   /api/v1/tax/exemptions                   Create tax exemption
GET    /api/v1/tax/exemptions                   List exemptions
DELETE /api/v1/tax/exemptions/{id}              Remove exemption
GET    /api/v1/tax/report                       Tax report for period
```

---

### 2.4 Dunning Management (Failed Payment Recovery)

**Backend:**

| Layer | File | Description |
|-------|------|-------------|
| Entity | `Core/Infrastructure/DunningCampaign.cs` | Id, TenantId, Name, MaxRetryAttempts, RetrySchedule (JSON: [1,3,5,7] days), GracePeriodDays, SuspendAfterMaxRetries, CancelAfterSuspensionDays, IsDefault, IsActive, CreatedAt |
| Entity | `Core/Infrastructure/DunningAttempt.cs` | Id, TenantId, SubscriptionId, CustomerId, InvoiceId, CampaignId, AttemptNumber, Status (pending/succeeded/failed/exhausted), NextRetryAt, LastAttemptAt, StripePaymentIntentId, FailureReason, CustomerNotified, CreatedAt |
| DTO Request | `Core/Dtos/Requests/CreateDunningCampaignDto.cs` | Name, MaxRetryAttempts, RetryScheduleDays[], GracePeriodDays, SuspendAfterMaxRetries, CancelAfterSuspensionDays |
| DTO Request | `Core/Dtos/Requests/UpdateDunningCampaignDto.cs` | Same fields optional |
| DTO Request | `Core/Dtos/Requests/DunningFilterDto.cs` | Page, PageSize, Status?, SubscriptionId?, CustomerId? |
| DTO Response | `Core/Dtos/Responses/DunningCampaignResponseDto.cs` | All campaign fields |
| DTO Response | `Core/Dtos/Responses/DunningAttemptResponseDto.cs` | All attempt fields + customer name + subscription details |
| DTO Response | `Core/Dtos/Responses/DunningStatsDto.cs` | ActiveCases, RecoveredCount, RecoveredAmount, FailedCount, RecoveryRate, AvgRecoveryDays |
| Interface | `Core/ServiceContracts/IDunningService.cs` | CreateCampaignAsync, GetCampaignAsync, ListCampaignsAsync, UpdateCampaignAsync, ListAttemptsAsync, GetStatsAsync, ManualRetryAsync |
| Service | `Core/Services/DunningService.cs` | Campaign management, attempt tracking |
| Background | `WebAPI/BackgroundServices/DunningRetryService.cs` | Polls for due retry attempts, executes Stripe payment retry, updates status, triggers notifications |
| Validator | `Core/Validators/CreateDunningCampaignValidator.cs` | FluentValidation |
| Repository | `Core/Repositories/DunningRepository.cs` | CRUD + filtering |
| Repo Contract | `Core/RepositoryContracts/IDunningRepository.cs` | Interface |
| Controller | `WebAPI/Controllers/v1/DunningController.cs` | REST endpoints |

**Frontend:**

| Layer | File | Description |
|-------|------|-------------|
| Type | `src/types/dunning.ts` | DunningCampaign, DunningAttempt, DunningStats interfaces |
| API | `src/api/dunningApi.ts` | Campaigns, attempts, stats, manual retry |
| Page | `src/pages/billing/DunningPage.tsx` | Campaign list, active dunning cases, recovery stats dashboard |
| Component | `src/components/common/DunningStatusBadge.tsx` | Status badge for dunning attempts |
| Route | Update `AppRoutes.tsx` | Add `/dunning` route |
| Sidebar | Update `Sidebar.tsx` | Add Dunning nav item under BILLING |

**API Endpoints:**

```
POST   /api/v1/dunning/campaigns                Create dunning campaign
GET    /api/v1/dunning/campaigns                 List campaigns
GET    /api/v1/dunning/campaigns/{id}            Get campaign details
PUT    /api/v1/dunning/campaigns/{id}            Update campaign
GET    /api/v1/dunning/attempts                  List dunning attempts (paginated)
POST   /api/v1/dunning/attempts/{id}/retry       Manual retry
GET    /api/v1/dunning/stats                     Recovery statistics
```

---

## 3. Phase 2 — MEDIUM Priority Features

### 3.1 Credit / Balance System

**Backend:**

| Layer | File | Description |
|-------|------|-------------|
| Entity | `Core/Infrastructure/CreditTransaction.cs` | Id, TenantId, CustomerId, Type (credit/debit/adjustment), Amount, Currency, Description, ReferenceType (refund/manual/promotional), ReferenceId, BalanceAfter, CreatedBy, CreatedAt |
| DTO Request | `Core/Dtos/Requests/CreateCreditDto.cs` | CustomerId, Amount, Currency, Description, Type |
| DTO Request | `Core/Dtos/Requests/CreditFilterDto.cs` | Page, PageSize, CustomerId?, Type?, StartDate?, EndDate? |
| DTO Response | `Core/Dtos/Responses/CreditBalanceDto.cs` | CustomerId, CustomerName, Balance, Currency, TotalCredits, TotalDebits |
| DTO Response | `Core/Dtos/Responses/CreditTransactionResponseDto.cs` | All fields |
| Interface | `Core/ServiceContracts/ICreditService.cs` | AddCreditAsync, DebitCreditAsync, GetBalanceAsync, ListTransactionsAsync, RefundToCreditAsync |
| Service | `Core/Services/CreditService.cs` | Stripe Customer Balance API, credit ledger management |
| Controller | `WebAPI/Controllers/v1/CreditController.cs` | REST endpoints |

**Frontend:**

| Layer | File | Description |
|-------|------|-------------|
| Type | `src/types/credit.ts` | CreditTransaction, CreditBalance interfaces |
| API | `src/api/creditApi.ts` | Balance, transactions, add/debit credit |
| Page | `src/pages/billing/CreditsPage.tsx` | Customer credit balances, transaction history, add credit modal |
| Route | Update `AppRoutes.tsx` | Add `/credits` route |
| Sidebar | Update `Sidebar.tsx` | Add Credits nav item under BILLING |

**API Endpoints:**

```
POST   /api/v1/credits                          Add credit to customer
POST   /api/v1/credits/debit                    Debit from customer balance
GET    /api/v1/credits/balance/{customerId}      Get customer credit balance
GET    /api/v1/credits/transactions              List credit transactions
POST   /api/v1/credits/refund-to-credit          Convert refund to credit
```

---

### 3.2 Email Notification Service

**Backend:**

| Layer | File | Description |
|-------|------|-------------|
| Entity | `Core/Infrastructure/EmailTemplate.cs` | Id, TenantId, EventType, Subject, HtmlBody, IsActive, CreatedAt, UpdatedAt |
| Entity | `Core/Infrastructure/EmailLog.cs` | Id, TenantId, Recipient, Subject, EventType, Status (sent/failed/queued), Provider, ErrorMessage, SentAt, CreatedAt |
| DTO Request | `Core/Dtos/Requests/UpdateEmailTemplateDto.cs` | Subject, HtmlBody, IsActive |
| DTO Request | `Core/Dtos/Requests/SendTestEmailDto.cs` | TemplateId, RecipientEmail |
| DTO Request | `Core/Dtos/Requests/EmailLogFilterDto.cs` | Page, PageSize, Status?, EventType?, StartDate?, EndDate? |
| DTO Response | `Core/Dtos/Responses/EmailTemplateResponseDto.cs` | All fields + preview |
| DTO Response | `Core/Dtos/Responses/EmailLogResponseDto.cs` | All fields |
| DTO Response | `Core/Dtos/Responses/EmailStatsDto.cs` | TotalSent, TotalFailed, DeliveryRate, ByEventType[] |
| Interface | `Core/ServiceContracts/IEmailService.cs` | SendAsync, GetTemplatesAsync, UpdateTemplateAsync, SendTestAsync, GetLogsAsync, GetStatsAsync |
| Service | `Core/Services/EmailService.cs` | SendGrid/SES integration, template rendering with variable substitution |
| Background | `WebAPI/BackgroundServices/EmailQueueService.cs` | Processes queued emails |
| Controller | `WebAPI/Controllers/v1/EmailController.cs` | Template management, logs, test send |

**Frontend:**

| Layer | File | Description |
|-------|------|-------------|
| Type | `src/types/email.ts` | EmailTemplate, EmailLog, EmailStats interfaces |
| API | `src/api/emailApi.ts` | Templates, logs, test send, stats |
| Page | `src/pages/settings/EmailSettingsPage.tsx` | Template editor, email logs, delivery stats |
| Route | Update `AppRoutes.tsx` | Add `/settings/emails` route |

**API Endpoints:**

```
GET    /api/v1/emails/templates                  List email templates
GET    /api/v1/emails/templates/{id}             Get template
PUT    /api/v1/emails/templates/{id}             Update template
POST   /api/v1/emails/templates/{id}/test        Send test email
GET    /api/v1/emails/logs                       List email logs
GET    /api/v1/emails/stats                      Email delivery stats
```

---

### 3.3 Export & Reporting

**Backend:**

| Layer | File | Description |
|-------|------|-------------|
| Entity | `Core/Infrastructure/ExportJob.cs` | Id, TenantId, Type (transactions/invoices/customers/subscriptions), Format (csv/excel/pdf), Status (pending/processing/completed/failed), FilePath, FileSize, Filters (JSON), RequestedBy, CompletedAt, ExpiresAt, CreatedAt |
| DTO Request | `Core/Dtos/Requests/CreateExportDto.cs` | Type, Format, Filters, DateRange |
| DTO Request | `Core/Dtos/Requests/ExportFilterDto.cs` | Page, PageSize, Type?, Status? |
| DTO Response | `Core/Dtos/Responses/ExportJobResponseDto.cs` | All fields + download URL |
| Interface | `Core/ServiceContracts/IExportService.cs` | CreateExportAsync, GetExportAsync, ListExportsAsync, DownloadExportAsync |
| Service | `Core/Services/ExportService.cs` | CSV generation, QuestPDF report generation, Excel via ClosedXML |
| Background | `WebAPI/BackgroundServices/ExportProcessorService.cs` | Processes export jobs asynchronously |
| Controller | `WebAPI/Controllers/v1/ExportController.cs` | REST endpoints |

**Frontend:**

| Layer | File | Description |
|-------|------|-------------|
| Type | `src/types/export.ts` | ExportJob, CreateExportRequest interfaces |
| API | `src/api/exportApi.ts` | Create export, list exports, download |
| Page | `src/pages/analytics/ExportsPage.tsx` | Export wizard, job history, download links |
| Component | `src/components/common/ExportButton.tsx` | Reusable export trigger button used on Payments/Invoices/Customers pages |
| Route | Update `AppRoutes.tsx` | Add `/exports` route |
| Sidebar | Update `Sidebar.tsx` | Add Exports nav item under ANALYTICS |

**API Endpoints:**

```
POST   /api/v1/exports                           Create export job
GET    /api/v1/exports                            List export jobs
GET    /api/v1/exports/{id}                       Get export job status
GET    /api/v1/exports/{id}/download              Download export file
```

---

### 3.4 Idempotency Key Support

**Backend (middleware enhancement, no new pages):**

| Layer | File | Description |
|-------|------|-------------|
| Entity | `Core/Infrastructure/IdempotencyRecord.cs` | Id, TenantId, IdempotencyKey, HttpMethod, Endpoint, ResponseStatusCode, ResponseBody, CreatedAt, ExpiresAt |
| Middleware | `WebAPI/Middleware/IdempotencyMiddleware.cs` | Checks `Idempotency-Key` header, returns cached response or forwards to Stripe with key |
| Repository | `Core/Repositories/IdempotencyRepository.cs` | Lookup and store idempotency records |
| Repo Contract | `Core/RepositoryContracts/IIdempotencyRepository.cs` | Interface |

No frontend changes needed — this is transparent to the dashboard. Client SDKs pass `Idempotency-Key` header.

---

## 4. Phase 3 — LOW Priority Features

### 4.1 Subscription Add-ons & One-Time Charges

**Backend:**

| Layer | File | Description |
|-------|------|-------------|
| Entity | `Core/Infrastructure/InvoiceItem.cs` | Id, TenantId, CustomerId, SubscriptionId, StripeInvoiceItemId, Amount, Currency, Description, Quantity, IsPending, CreatedAt |
| DTO Request | `Core/Dtos/Requests/CreateInvoiceItemDto.cs` | CustomerId, SubscriptionId?, Amount, Currency, Description, Quantity |
| DTO Response | `Core/Dtos/Responses/InvoiceItemResponseDto.cs` | All fields |
| Interface | `Core/ServiceContracts/IInvoiceItemService.cs` | CreateAsync, ListPendingAsync, DeleteAsync |
| Service | `Core/Services/InvoiceItemService.cs` | Stripe InvoiceItem API integration |
| Controller | `WebAPI/Controllers/v1/InvoiceItemController.cs` | REST endpoints |

**Frontend:**

| Layer | File | Description |
|-------|------|-------------|
| Type | `src/types/invoiceItem.ts` | InvoiceItem, CreateInvoiceItemRequest interfaces |
| API | `src/api/invoiceItemApi.ts` | Create, list pending, delete |
| Component | `src/components/common/AddChargeModal.tsx` | Modal to add one-time charge to subscription |

**API Endpoints:**

```
POST   /api/v1/invoice-items                     Add one-time charge
GET    /api/v1/invoice-items                      List pending invoice items
DELETE /api/v1/invoice-items/{id}                 Remove pending item
```

---

### 4.2 Stripe Connect Integration

**Backend:**

| Layer | File | Description |
|-------|------|-------------|
| Entity | `Core/Infrastructure/ConnectedAccount.cs` | Id, TenantId, StripeAccountId, BusinessName, Email, Country, PayoutsEnabled, ChargesEnabled, Status, CreatedAt |
| Entity | `Core/Infrastructure/Transfer.cs` | Id, TenantId, ConnectedAccountId, StripeTransferId, Amount, Currency, Description, Status, CreatedAt |
| DTO Request | `Core/Dtos/Requests/CreateConnectedAccountDto.cs` | Email, Country, BusinessType, BusinessName |
| DTO Request | `Core/Dtos/Requests/CreateTransferDto.cs` | ConnectedAccountId, Amount, Currency, Description |
| DTO Response | `Core/Dtos/Responses/ConnectedAccountResponseDto.cs` | All fields + onboarding URL |
| DTO Response | `Core/Dtos/Responses/TransferResponseDto.cs` | All fields |
| Interface | `Core/ServiceContracts/IConnectService.cs` | CreateAccountAsync, GetAccountAsync, ListAccountsAsync, CreateOnboardingLinkAsync, CreateTransferAsync, ListTransfersAsync |
| Service | `Core/Services/ConnectService.cs` | Stripe Connect API |
| Controller | `WebAPI/Controllers/v1/ConnectController.cs` | REST endpoints |

**Frontend:**

| Layer | File | Description |
|-------|------|-------------|
| Type | `src/types/connect.ts` | ConnectedAccount, Transfer interfaces |
| API | `src/api/connectApi.ts` | Accounts, onboarding, transfers |
| Page | `src/pages/billing/ConnectPage.tsx` | Connected accounts management, transfers |
| Route | Update `AppRoutes.tsx` | Add `/connect` route |

**API Endpoints:**

```
POST   /api/v1/connect/accounts                  Create connected account
GET    /api/v1/connect/accounts                   List connected accounts
GET    /api/v1/connect/accounts/{id}              Get account details
POST   /api/v1/connect/accounts/{id}/onboarding   Create onboarding link
POST   /api/v1/connect/transfers                  Create transfer
GET    /api/v1/connect/transfers                   List transfers
```

---

### 4.3 Webhook Event Log Viewer with Replay

**Backend:**

| Layer | File | Description |
|-------|------|-------------|
| DTO Request | `Core/Dtos/Requests/WebhookEventFilterDto.cs` | Page, PageSize, EventType?, Status?, StartDate?, EndDate? |
| DTO Response | `Core/Dtos/Responses/WebhookEventResponseDto.cs` | Id, EventType, Payload (JSON), Status, ProcessedAt, ErrorMessage, CreatedAt |
| Interface | `Core/ServiceContracts/IWebhookEventService.cs` | ListEventsAsync, GetEventAsync, ReplayEventAsync |
| Service | `Core/Services/WebhookEventService.cs` | Query WebhookEventInbound table, replay by re-processing event |
| Controller | `WebAPI/Controllers/v1/WebhookEventController.cs` | REST endpoints |

**Frontend:**

| Layer | File | Description |
|-------|------|-------------|
| Type | `src/types/webhookEvent.ts` | WebhookEvent interface |
| API | `src/api/webhookEventApi.ts` | List, get, replay |
| Page | `src/pages/gateway/WebhookEventsPage.tsx` | Event log viewer with payload inspector and replay button |
| Route | Update `AppRoutes.tsx` | Add `/webhook-events` route |
| Sidebar | Update `Sidebar.tsx` | Add under GATEWAY |

**API Endpoints:**

```
GET    /api/v1/webhook-events                    List inbound webhook events
GET    /api/v1/webhook-events/{id}               Get event details + payload
POST   /api/v1/webhook-events/{id}/replay        Replay event
```

---

### 4.4 Per-Endpoint Rate Limiting

**Backend (configuration + middleware enhancement):**

| Layer | File | Description |
|-------|------|-------------|
| Entity | `Core/Infrastructure/RateLimitRule.cs` | Id, TenantId, Endpoint (pattern), HttpMethod, RequestsPerMinute, BurstLimit, IsActive, CreatedAt |
| DTO Request | `Core/Dtos/Requests/CreateRateLimitRuleDto.cs` | Endpoint, HttpMethod, RequestsPerMinute, BurstLimit |
| DTO Response | `Core/Dtos/Responses/RateLimitRuleResponseDto.cs` | All fields |
| Interface | `Core/ServiceContracts/IRateLimitService.cs` | CreateRuleAsync, ListRulesAsync, UpdateRuleAsync, DeleteRuleAsync |
| Service | `Core/Services/RateLimitService.cs` | CRUD for rules |
| Middleware | Update `RateLimitMiddleware.cs` | Check per-endpoint rules before global, add X-RateLimit-Remaining / X-RateLimit-Limit / X-RateLimit-Reset headers |
| Controller | `WebAPI/Controllers/v1/RateLimitController.cs` | Admin-only CRUD for rate limit rules |

**Frontend:**

| Layer | File | Description |
|-------|------|-------------|
| Type | `src/types/rateLimit.ts` | RateLimitRule interface |
| API | `src/api/rateLimitApi.ts` | CRUD |
| Section in Settings | Update `SettingsPage.tsx` | Add Rate Limiting tab with rules management |

---

## 5. Backend Implementation Details

### New Entity Registration

Add to `BillingDbContext.cs`:

```csharp
// Phase 1
public DbSet<Coupon> Coupons { get; set; }
public DbSet<PromotionCode> PromotionCodes { get; set; }
public DbSet<CouponRedemption> CouponRedemptions { get; set; }
public DbSet<UsageRecord> UsageRecords { get; set; }
public DbSet<MeterEvent> MeterEvents { get; set; }
public DbSet<TaxConfiguration> TaxConfigurations { get; set; }
public DbSet<TaxExemption> TaxExemptions { get; set; }
public DbSet<DunningCampaign> DunningCampaigns { get; set; }
public DbSet<DunningAttempt> DunningAttempts { get; set; }

// Phase 2
public DbSet<CreditTransaction> CreditTransactions { get; set; }
public DbSet<EmailTemplate> EmailTemplates { get; set; }
public DbSet<EmailLog> EmailLogs { get; set; }
public DbSet<ExportJob> ExportJobs { get; set; }
public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }

// Phase 3
public DbSet<InvoiceItem> InvoiceItems { get; set; }
public DbSet<ConnectedAccount> ConnectedAccounts { get; set; }
public DbSet<Transfer> Transfers { get; set; }
public DbSet<RateLimitRule> RateLimitRules { get; set; }
```

### New Background Services

Register in `Program.cs`:

```csharp
// Phase 1
builder.Services.AddHostedService<DunningRetryService>();

// Phase 2
builder.Services.AddHostedService<EmailQueueService>();
builder.Services.AddHostedService<ExportProcessorService>();
```

### New Middleware

Add to pipeline in `Program.cs` (before existing middleware):

```csharp
// Phase 2
app.UseMiddleware<IdempotencyMiddleware>();
```

### NuGet Packages to Add

```xml
<!-- Phase 2 - Email -->
<PackageReference Include="SendGrid" Version="9.*" />
<!-- Phase 2 - Export -->
<PackageReference Include="ClosedXML" Version="0.104.*" />
<!-- QuestPDF already installed -->
```

---

## 6. Frontend Implementation Details

### Updated Sidebar Navigation

```
BILLING
├── Payments
├── Subscriptions
├── Customers
├── Invoices
├── Refunds
├── Plans (existing)
├── Coupons         ← NEW Phase 1
├── Usage           ← NEW Phase 1
├── Dunning         ← NEW Phase 1
├── Credits         ← NEW Phase 2
└── Connect         ← NEW Phase 3

ANALYTICS
├── Analytics
└── Exports         ← NEW Phase 2

GATEWAY
├── API Keys
├── Logs
├── Webhooks
└── Webhook Events  ← NEW Phase 3

MANAGEMENT (Admin+)
├── Users
├── Audit Logs
└── Settings
    ├── General (existing)
    ├── Tax             ← NEW Phase 1
    ├── Email Templates ← NEW Phase 2
    └── Rate Limiting   ← NEW Phase 3

SUPER ADMIN
└── Tenants
```

### New Route Registrations

```tsx
// Phase 1
<Route path="/coupons" element={<ProtectedRoute><CouponsPage /></ProtectedRoute>} />
<Route path="/usage" element={<ProtectedRoute><UsagePage /></ProtectedRoute>} />
<Route path="/dunning" element={<ProtectedRoute><DunningPage /></ProtectedRoute>} />
<Route path="/settings/tax" element={<ProtectedRoute requiredRole="Admin"><TaxSettingsPage /></ProtectedRoute>} />

// Phase 2
<Route path="/credits" element={<ProtectedRoute><CreditsPage /></ProtectedRoute>} />
<Route path="/exports" element={<ProtectedRoute><ExportsPage /></ProtectedRoute>} />
<Route path="/settings/emails" element={<ProtectedRoute requiredRole="Admin"><EmailSettingsPage /></ProtectedRoute>} />

// Phase 3
<Route path="/connect" element={<ProtectedRoute requiredRole="Admin"><ConnectPage /></ProtectedRoute>} />
<Route path="/webhook-events" element={<ProtectedRoute><WebhookEventsPage /></ProtectedRoute>} />
```

---

## 7. Database Migration Plan

### Phase 1 Migrations

```
Migration_001_AddCouponsAndPromotionCodes
  - Coupons table
  - PromotionCodes table
  - CouponRedemptions table
  - Indexes on TenantId, StripeCouponId, IsActive

Migration_002_AddUsageBilling
  - UsageRecords table
  - MeterEvents table
  - Indexes on SubscriptionId, Timestamp

Migration_003_AddTaxConfiguration
  - TaxConfigurations table (one per tenant)
  - TaxExemptions table
  - Indexes on TenantId, CustomerId

Migration_004_AddDunningManagement
  - DunningCampaigns table
  - DunningAttempts table
  - Indexes on SubscriptionId, Status, NextRetryAt
```

### Phase 2 Migrations

```
Migration_005_AddCreditSystem
  - CreditTransactions table
  - Index on CustomerId, TenantId

Migration_006_AddEmailService
  - EmailTemplates table
  - EmailLogs table
  - Indexes on TenantId, EventType, Status

Migration_007_AddExportJobs
  - ExportJobs table
  - Index on TenantId, Status

Migration_008_AddIdempotencyRecords
  - IdempotencyRecords table
  - Unique index on (TenantId, IdempotencyKey)
  - TTL cleanup index on ExpiresAt
```

### Phase 3 Migrations

```
Migration_009_AddInvoiceItems
  - InvoiceItems table

Migration_010_AddStripeConnect
  - ConnectedAccounts table
  - Transfers table

Migration_011_AddRateLimitRules
  - RateLimitRules table
```

---

## 8. Testing Strategy

### Backend Tests

For each new feature, create tests in `Tests/` project:

| Test File | Coverage |
|-----------|----------|
| `CouponServiceTests.cs` | Create, apply, remove, stats, Stripe sync |
| `UsageBillingServiceTests.cs` | Report usage, summaries, metered plans |
| `TaxServiceTests.cs` | Config CRUD, calculation, exemptions |
| `DunningServiceTests.cs` | Campaign CRUD, retry logic, stats |
| `CreditServiceTests.cs` | Add/debit credit, balance, refund-to-credit |
| `EmailServiceTests.cs` | Template rendering, send, log |
| `ExportServiceTests.cs` | CSV/Excel/PDF generation |
| `IdempotencyMiddlewareTests.cs` | Cache hit/miss, expiration |
| `InvoiceItemServiceTests.cs` | Create, list, delete |
| `ConnectServiceTests.cs` | Account creation, onboarding, transfers |
| `WebhookEventServiceTests.cs` | List, replay |

### Frontend Tests (Optional Enhancement)

Use Vitest + React Testing Library:
- Component render tests for new pages
- API client mock tests
- Form validation tests

---

## 9. File-by-File Checklist

### Phase 1 — HIGH Priority (Total: ~55 new files)

**Backend (~35 files):**
- [ ] `Core/Infrastructure/Coupon.cs`
- [ ] `Core/Infrastructure/PromotionCode.cs`
- [ ] `Core/Infrastructure/CouponRedemption.cs`
- [ ] `Core/Infrastructure/UsageRecord.cs`
- [ ] `Core/Infrastructure/MeterEvent.cs`
- [ ] `Core/Infrastructure/TaxConfiguration.cs`
- [ ] `Core/Infrastructure/TaxExemption.cs`
- [ ] `Core/Infrastructure/DunningCampaign.cs`
- [ ] `Core/Infrastructure/DunningAttempt.cs`
- [ ] `Core/Dtos/Requests/CreateCouponDto.cs`
- [ ] `Core/Dtos/Requests/UpdateCouponDto.cs`
- [ ] `Core/Dtos/Requests/CreatePromotionCodeDto.cs`
- [ ] `Core/Dtos/Requests/ApplyCouponDto.cs`
- [ ] `Core/Dtos/Requests/CouponFilterDto.cs`
- [ ] `Core/Dtos/Requests/ReportUsageDto.cs`
- [ ] `Core/Dtos/Requests/CreateMeteredPlanDto.cs`
- [ ] `Core/Dtos/Requests/UsageFilterDto.cs`
- [ ] `Core/Dtos/Requests/UpdateTaxConfigDto.cs`
- [ ] `Core/Dtos/Requests/CreateTaxExemptionDto.cs`
- [ ] `Core/Dtos/Requests/TaxCalculationRequestDto.cs`
- [ ] `Core/Dtos/Requests/CreateDunningCampaignDto.cs`
- [ ] `Core/Dtos/Requests/UpdateDunningCampaignDto.cs`
- [ ] `Core/Dtos/Requests/DunningFilterDto.cs`
- [ ] `Core/Dtos/Responses/CouponResponseDto.cs`
- [ ] `Core/Dtos/Responses/PromotionCodeResponseDto.cs`
- [ ] `Core/Dtos/Responses/CouponStatsDto.cs`
- [ ] `Core/Dtos/Responses/UsageRecordResponseDto.cs`
- [ ] `Core/Dtos/Responses/UsageSummaryDto.cs`
- [ ] `Core/Dtos/Responses/TaxConfigResponseDto.cs`
- [ ] `Core/Dtos/Responses/TaxCalculationResponseDto.cs`
- [ ] `Core/Dtos/Responses/TaxReportDto.cs`
- [ ] `Core/Dtos/Responses/DunningCampaignResponseDto.cs`
- [ ] `Core/Dtos/Responses/DunningAttemptResponseDto.cs`
- [ ] `Core/Dtos/Responses/DunningStatsDto.cs`
- [ ] `Core/ServiceContracts/ICouponService.cs`
- [ ] `Core/ServiceContracts/IUsageBillingService.cs`
- [ ] `Core/ServiceContracts/ITaxService.cs`
- [ ] `Core/ServiceContracts/IDunningService.cs`
- [ ] `Core/Services/CouponService.cs`
- [ ] `Core/Services/UsageBillingService.cs`
- [ ] `Core/Services/TaxService.cs`
- [ ] `Core/Services/DunningService.cs`
- [ ] `Core/Validators/CreateCouponValidator.cs`
- [ ] `Core/Validators/CreatePromotionCodeValidator.cs`
- [ ] `Core/Validators/ReportUsageValidator.cs`
- [ ] `Core/Validators/CreateDunningCampaignValidator.cs`
- [ ] `Core/Validators/UpdateTaxConfigValidator.cs`
- [ ] `Core/Mappers/CouponMapper.cs`
- [ ] `Core/Mappers/UsageMapper.cs`
- [ ] `Core/Mappers/TaxMapper.cs`
- [ ] `Core/Mappers/DunningMapper.cs`
- [ ] `Core/Repositories/CouponRepository.cs`
- [ ] `Core/Repositories/UsageRecordRepository.cs`
- [ ] `Core/Repositories/DunningRepository.cs`
- [ ] `Core/RepositoryContracts/ICouponRepository.cs`
- [ ] `Core/RepositoryContracts/IUsageRecordRepository.cs`
- [ ] `Core/RepositoryContracts/IDunningRepository.cs`
- [ ] `WebAPI/Controllers/v1/CouponController.cs`
- [ ] `WebAPI/Controllers/v1/UsageController.cs`
- [ ] `WebAPI/Controllers/v1/TaxController.cs`
- [ ] `WebAPI/Controllers/v1/DunningController.cs`
- [ ] `WebAPI/BackgroundServices/DunningRetryService.cs`
- [ ] Update `Core/Infrastructure/BillingDbContext.cs`
- [ ] Update `WebAPI/Program.cs`

**Frontend (~20 files):**
- [ ] `src/types/coupon.ts`
- [ ] `src/types/usage.ts`
- [ ] `src/types/tax.ts`
- [ ] `src/types/dunning.ts`
- [ ] `src/api/couponApi.ts`
- [ ] `src/api/usageApi.ts`
- [ ] `src/api/taxApi.ts`
- [ ] `src/api/dunningApi.ts`
- [ ] `src/pages/billing/CouponsPage.tsx`
- [ ] `src/pages/billing/UsagePage.tsx`
- [ ] `src/pages/billing/DunningPage.tsx`
- [ ] `src/pages/settings/TaxSettingsPage.tsx`
- [ ] `src/components/common/CouponBadge.tsx`
- [ ] `src/components/common/UsageChart.tsx`
- [ ] `src/components/common/DunningStatusBadge.tsx`
- [ ] Update `src/routes/AppRoutes.tsx`
- [ ] Update `src/components/layout/Sidebar.tsx`

### Phase 2 — MEDIUM Priority (Total: ~35 new files)

**Backend (~22 files):**
- [ ] `Core/Infrastructure/CreditTransaction.cs`
- [ ] `Core/Infrastructure/EmailTemplate.cs`
- [ ] `Core/Infrastructure/EmailLog.cs`
- [ ] `Core/Infrastructure/ExportJob.cs`
- [ ] `Core/Infrastructure/IdempotencyRecord.cs`
- [ ] All associated DTOs, interfaces, services, validators, repositories
- [ ] `WebAPI/Controllers/v1/CreditController.cs`
- [ ] `WebAPI/Controllers/v1/EmailController.cs`
- [ ] `WebAPI/Controllers/v1/ExportController.cs`
- [ ] `WebAPI/Middleware/IdempotencyMiddleware.cs`
- [ ] `WebAPI/BackgroundServices/EmailQueueService.cs`
- [ ] `WebAPI/BackgroundServices/ExportProcessorService.cs`

**Frontend (~13 files):**
- [ ] `src/types/credit.ts`
- [ ] `src/types/email.ts`
- [ ] `src/types/export.ts`
- [ ] `src/api/creditApi.ts`
- [ ] `src/api/emailApi.ts`
- [ ] `src/api/exportApi.ts`
- [ ] `src/pages/billing/CreditsPage.tsx`
- [ ] `src/pages/settings/EmailSettingsPage.tsx`
- [ ] `src/pages/analytics/ExportsPage.tsx`
- [ ] `src/components/common/ExportButton.tsx`
- [ ] Update `src/routes/AppRoutes.tsx`
- [ ] Update `src/components/layout/Sidebar.tsx`

### Phase 3 — LOW Priority (Total: ~25 new files)

**Backend (~16 files):**
- [ ] `Core/Infrastructure/InvoiceItem.cs`
- [ ] `Core/Infrastructure/ConnectedAccount.cs`
- [ ] `Core/Infrastructure/Transfer.cs`
- [ ] `Core/Infrastructure/RateLimitRule.cs`
- [ ] All associated DTOs, interfaces, services
- [ ] `WebAPI/Controllers/v1/InvoiceItemController.cs`
- [ ] `WebAPI/Controllers/v1/ConnectController.cs`
- [ ] `WebAPI/Controllers/v1/WebhookEventController.cs`
- [ ] `WebAPI/Controllers/v1/RateLimitController.cs`
- [ ] Update `WebAPI/Middleware/RateLimitMiddleware.cs`

**Frontend (~9 files):**
- [ ] `src/types/invoiceItem.ts`, `connect.ts`, `webhookEvent.ts`, `rateLimit.ts`
- [ ] `src/api/invoiceItemApi.ts`, `connectApi.ts`, `webhookEventApi.ts`, `rateLimitApi.ts`
- [ ] `src/pages/billing/ConnectPage.tsx`
- [ ] `src/pages/gateway/WebhookEventsPage.tsx`
- [ ] `src/components/common/AddChargeModal.tsx`
- [ ] Update routes and sidebar

---

## Estimated Effort

| Phase | Backend | Frontend | Total |
|-------|---------|----------|-------|
| Phase 1 (HIGH) | ~5-7 days | ~3-4 days | ~8-11 days |
| Phase 2 (MEDIUM) | ~4-5 days | ~2-3 days | ~6-8 days |
| Phase 3 (LOW) | ~3-4 days | ~2-3 days | ~5-7 days |
| Testing | ~3-4 days | ~1-2 days | ~4-6 days |
| **Total** | **~15-20 days** | **~8-12 days** | **~23-32 days** |

---

*End of Development Plan*
