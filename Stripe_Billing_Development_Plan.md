# Stripe Billing Service — Complete Development Plan
### Project #3: Multi-Tenant Stripe Payment Integration & Subscription Billing Service
**Developer:** Muhammad Nasir
**Estimated Build Time:** 10 Days (Advanced Tier)
**Stack:** ASP.NET Core 9 | React 18 + TypeScript + Vite | SQL Server | EF Core 9 | SignalR | Stripe.NET | Bootstrap 5 + SCSS | Chart.js

---

## Table of Contents

1. [Pre-Development Setup](#1-pre-development-setup)
2. [Day 1 — Foundation & Core Framework](#2-day-1--foundation--core-framework)
3. [Day 2 — Payments & Stripe Integration](#3-day-2--payments--stripe-integration)
4. [Day 3 — Subscriptions & Plans](#4-day-3--subscriptions--plans)
5. [Day 4 — Webhooks (Double Webhook System)](#5-day-4--webhooks-double-webhook-system)
6. [Day 5 — User Portal (React Frontend)](#6-day-5--user-portal-react-frontend)
7. [Day 6 — Admin Dashboard: Core Pages (React)](#7-day-6--admin-dashboard-core-pages-react)
8. [Day 7 — Admin Dashboard: Billing Pages (React)](#8-day-7--admin-dashboard-billing-pages-react)
9. [Day 8 — Super Admin Panel (React)](#9-day-8--super-admin-panel-react)
10. [Day 9 — Revenue Analytics & Real-Time (React)](#10-day-9--revenue-analytics--real-time-react)
11. [Day 10 — Testing, Docs & Deployment](#11-day-10--testing-docs--deployment)
12. [Post-Build Checklist](#12-post-build-checklist)
13. [Risk Mitigation](#13-risk-mitigation)
14. [Folder & File Structure](#14-folder--file-structure)
15. [Upwork Listing Checklist](#15-upwork-listing-checklist)

---

## 1. Pre-Development Setup

> Complete these before Day 1 to avoid blockers during the build sprint.

### Environment Setup
| Tool | Version | Purpose |
|------|---------|---------|
| .NET SDK | 8.0+ | Backend API |
| Node.js | 20 LTS | React frontend build |
| SQL Server | 2022 / Express | Multi-tenant database |
| Visual Studio / Rider | Latest | Backend IDE |
| VS Code | Latest | Frontend IDE |
| Postman | Latest | API testing |
| Stripe CLI | Latest | Webhook testing (`stripe listen`) |
| Git | Latest | Version control |
| Docker Desktop | Latest | Containerized deployment |

### Account & API Keys Needed
- [ ] **Stripe** — Secret key + Publishable key + Webhook signing secret (test mode)
- [ ] **Stripe CLI** — Installed and authenticated (`stripe login`)
- [ ] **GitHub** — Private repo created: `stripe-billing-service`

### NuGet Packages Required
```
Core Project:
  - Microsoft.EntityFrameworkCore (8.0.11)
  - Microsoft.EntityFrameworkCore.SqlServer (8.0.11)
  - Microsoft.Extensions.Http
  - Microsoft.AspNetCore.DataProtection
  - BCrypt.Net-Next (4.0.3)
  - NetCore.AutoRegisterDi (2.1.1)
  - FluentValidation (11.9.0)
  - Stripe.net (45.0.0+)
  - Newtonsoft.Json (13.0.3)
  - QuestPDF (2024.3.0) — Invoice PDF generation
  - System.IdentityModel.Tokens.Jwt (7.0.0)
  - Microsoft.IdentityModel.Tokens (7.0.0)

API Project:
  - Microsoft.AspNetCore.Authentication.JwtBearer (8.0.11)
  - Microsoft.AspNetCore.SignalR (1.1.0)
  - Swashbuckle.AspNetCore (6.5.0)
  - Serilog.AspNetCore (8.0.0)
  - Serilog.Sinks.Console (5.0.0)

Test Project:
  - xunit (2.9.0)
  - Moq (4.20.0)
  - FluentAssertions (6.12.0)
  - Microsoft.EntityFrameworkCore.InMemory (8.0.11)
  - Microsoft.AspNetCore.Mvc.Testing (8.0.11)
```

### Frontend Packages Required
```
  react / react-dom (18.x)
  react-router-dom (6.x)
  axios
  @microsoft/signalr
  react-hook-form
  @hookform/resolvers
  zod
  react-toastify
  chart.js + react-chartjs-2
  lucide-react
  react-bootstrap + bootstrap (5.3)
  sass
  date-fns
  typescript
  vite
  @types/react @types/react-dom
```

### Project Initialization Commands
```bash
# Create project folder
mkdir 03-Stripe-Billing-Service && cd 03-Stripe-Billing-Service

# Backend
mkdir backend && cd backend
dotnet new sln -n StripeBilling
dotnet new webapi -n StripeBilling.API -o WebAPI
dotnet new classlib -n StripeBilling.Core -o Core
dotnet new xunit -n StripeBilling.Tests -o Tests
dotnet sln add WebAPI/StripeBilling.API.csproj Core/StripeBilling.Core.csproj Tests/StripeBilling.Tests.csproj
cd WebAPI && dotnet add reference ../Core/StripeBilling.Core.csproj
cd ../Tests && dotnet add reference ../Core/StripeBilling.Core.csproj ../WebAPI/StripeBilling.API.csproj

# Frontend
cd ../..
npm create vite@latest frontend -- --template react-ts
cd frontend
npm install axios @microsoft/signalr react-router-dom react-bootstrap bootstrap sass
npm install chart.js react-chartjs-2 lucide-react react-toastify date-fns
npm install react-hook-form @hookform/resolvers zod
```

### Reference Project Patterns (from 02-API-Gateway-Microservice)
This project reuses the following **identical** patterns from Project #2:

| Pattern | Source File | Reuse |
|---------|-----------|-------|
| GatewayResponseWrapper<T> | Core/Utils/GatewayResponseWrapper.cs | Identical |
| GatewayControllerBase | Controllers/v1/GatewayControllerBase.cs | Identical |
| BaseService + ITenantContextProvider | Core/Services/BaseService.cs | Identical |
| BaseRepository + tenant scoping | Core/Repositories/BaseRepository.cs | Identical |
| HttpTenantContextProvider | Core/ContextProviders/ | Identical |
| DiRegistrationExtensions | Core/Utils/ | Identical |
| ExceptionHandler | Core/ErrorHandling/ | Identical |
| TenantMiddleware | WebAPI/Middleware/ | Identical |
| ApiKeyAuthMiddleware | WebAPI/Middleware/ | Identical |
| RateLimitMiddleware | WebAPI/Middleware/ | Identical |
| RequestLoggingMiddleware | WebAPI/Middleware/ | Identical |
| JwtTokenService | Core/Services/ | Identical |
| EncryptionService | Core/Services/ | Identical |
| AuthService | Core/Services/ | Identical |
| WebhookDispatchService | Core/Services/ | Identical |
| DashboardHub (SignalR) | WebAPI/Hubs/ | Identical |
| api-client.ts + interceptors.ts | frontend/src/api/ | Identical |
| AuthContext + useAuth | frontend/src/contexts/ | Identical |
| ToastContext + useToast | frontend/src/contexts/ | Identical |
| SidebarContext + useSidebar | frontend/src/contexts/ | Identical |
| DashboardLayout + AuthLayout | frontend/src/layouts/ | Identical |
| DataTable, MetricCard, StatusBadge | frontend/src/components/common/ | Identical |
| ProtectedRoute | frontend/src/routes/ | Identical |

**New additions** specific to this project:
- HmacAuthMiddleware (HMAC-SHA256 signature verification)
- StripePaymentGateway + IPaymentGateway
- SubscriptionService + ISubscriptionService
- SubscriptionPlanService + ISubscriptionPlanService
- CustomerService + ICustomerService
- InvoiceService + IInvoiceService
- RefundService + IRefundService
- StripeWebhookHandler + IStripeWebhookHandler
- RevenueAnalyticsService + IRevenueAnalyticsService
- HmacAuthService + IHmacAuthService
- AuditService + IAuditService
- TenantService + ITenantService
- 6 new entity models (Customer, Subscription, SubscriptionPlan, Invoice, Refund, WebhookEventInbound)
- 6 billing React pages + Analytics page + Audit page
- 6 new reusable components (RevenueChart, SubscriptionBadge, RefundBadge, InvoiceViewer, PlanCard, WebhookStatusBadge)

---

## 2. Day 1 — Foundation & Core Framework

### Goal: Project skeleton, DB schema, all entities, HMAC auth middleware, tenant system ready

### Morning (4 hours)

#### Task 1.1 — Solution Structure
Create solution matching the reference project pattern (Core + WebAPI + Tests):

```
backend/
├── StripeBilling.sln
├── Core/
│   ├── StripeBilling.Core.csproj
│   ├── Constants/
│   │   ├── Roles.cs                          ← From reference (SuperAdmin, Admin, Manager, Viewer)
│   │   ├── ErrorCodes.cs                     ← From reference
│   │   ├── WebhookEvents.cs                  ← NEW: all inbound + outbound event type constants
│   │   └── StripeConstants.cs                ← NEW: Stripe-specific constants
│   ├── ContextProviders/
│   │   ├── ITenantContextProvider.cs          ← From reference (identical)
│   │   └── HttpTenantContextProvider.cs       ← From reference (identical)
│   ├── Dtos/
│   │   ├── Requests/                          ← All request DTOs (30+ files)
│   │   └── Responses/                         ← All response DTOs (25+ files)
│   ├── ErrorHandling/
│   │   ├── ExceptionHandler.cs                ← From reference (identical)
│   │   └── Exceptions/                        ← Custom exception classes
│   ├── Infrastructure/                        ← EF Core entities + DbContext (16 files)
│   ├── Mappers/                               ← Entity ↔ DTO mappers
│   ├── Repositories/
│   │   └── BaseRepository.cs                  ← From reference (identical)
│   ├── RepositoryContracts/                   ← All I*Repository interfaces
│   ├── ServiceContracts/                      ← 21 I*Service interfaces
│   ├── Services/
│   │   └── BaseService.cs                     ← From reference (identical)
│   ├── Utils/
│   │   ├── GatewayResponseWrapper.cs          ← From reference (identical)
│   │   └── DiRegistrationExtensions.cs        ← From reference (identical)
│   └── Validators/                            ← FluentValidation validators
├── WebAPI/
│   ├── StripeBilling.API.csproj
│   ├── Controllers/v1/
│   │   └── GatewayControllerBase.cs           ← From reference (identical)
│   ├── Middleware/
│   │   ├── TenantMiddleware.cs                ← From reference (identical)
│   │   ├── ApiKeyAuthMiddleware.cs            ← From reference (identical)
│   │   ├── HmacAuthMiddleware.cs              ← NEW: HMAC-SHA256 signature verification
│   │   ├── RateLimitMiddleware.cs             ← From reference (identical)
│   │   └── RequestLoggingMiddleware.cs        ← From reference (identical)
│   ├── BackgroundServices/
│   │   ├── WebhookDispatcherService.cs        ← From reference (identical)
│   │   └── WebhookRetryService.cs             ← From reference (identical)
│   ├── Hubs/
│   │   └── DashboardHub.cs                    ← From reference (identical)
│   ├── Program.cs
│   └── appsettings.json
└── Tests/
    ├── StripeBilling.Tests.csproj
    ├── Services/
    └── Controllers/
```

#### Task 1.2 — Copy Identical Files from Reference
```
Copy directly from 02-API-Gateway-Microservice:

Core layer:
  - GatewayResponseWrapper.cs → Core/Utils/
  - DiRegistrationExtensions.cs → Core/Utils/
  - ITenantContextProvider.cs → Core/ContextProviders/
  - HttpTenantContextProvider.cs → Core/ContextProviders/
  - BaseService.cs → Core/Services/
  - BaseRepository.cs → Core/Repositories/
  - ExceptionHandler.cs → Core/ErrorHandling/
  - Roles.cs → Core/Constants/

WebAPI layer:
  - GatewayControllerBase.cs → WebAPI/Controllers/v1/
  - TenantMiddleware.cs → WebAPI/Middleware/
  - ApiKeyAuthMiddleware.cs → WebAPI/Middleware/
  - RateLimitMiddleware.cs → WebAPI/Middleware/
  - RequestLoggingMiddleware.cs → WebAPI/Middleware/
  - DashboardHub.cs → WebAPI/Hubs/

Change only namespaces from ApiGateway.* to Core.* / StripeBilling.*
```

#### Task 1.3 — NEW: HmacAuthMiddleware
```
File: WebAPI/Middleware/HmacAuthMiddleware.cs

This is the KEY DIFFERENTIATOR from the reference project.
Applied to: /api/v1/payments/*, /api/v1/subscriptions/*, /api/v1/customers/*

Required headers:
  X-Api-Key        → pk_live_xxx (identifies tenant, already validated by ApiKeyAuthMiddleware)
  X-Signature      → HMAC-SHA256(requestBody + timestamp, secretKey)
  X-Timestamp      → Unix timestamp (must be within 5 minutes)
  X-Idempotency-Key → Unique per request (prevents duplicate charges)

Verification flow:
  1. Read X-Api-Key → look up tenant → get secretKey hash
  2. Read X-Timestamp → reject if > 5 min drift
  3. Read request body (enable buffering)
  4. Compute expected = HMAC-SHA256(body + "|" + timestamp, secretKey)
  5. Compare with X-Signature (constant-time comparison)
  6. Check X-Idempotency-Key in cache → if exists, return cached response
  7. Store idempotency key with 24h TTL
  8. On success: set TenantId, ApiKeyId, Permissions in HttpContext.Items

Implementation details:
  - Enable request body buffering: context.Request.EnableBuffering()
  - Read body: using var reader = new StreamReader(context.Request.Body, leaveOpen: true)
  - Reset position: context.Request.Body.Position = 0
  - Constant-time comparison: CryptographicOperations.FixedTimeEquals()
  - Idempotency cache: IMemoryCache with 24h expiry
  - On failure: return 401 with GatewayResponseWrapper error
```

### Afternoon (4 hours)

#### Task 1.4 — Entity Models (16 files in Core/Infrastructure/)
```
Create all EF Core entities:

From reference (adapt namespaces):
  - Tenant.cs → Extended with Stripe fields
  - User.cs → Same as reference (AdminUsers)
  - RefreshToken.cs → Identical
  - ApiKey.cs → Identical
  - WebhookSubscription.cs → Identical
  - WebhookDelivery.cs → Identical
  - ApiCallLog.cs → Identical

NEW entities:
  - Customer.cs
  - Subscription.cs
  - SubscriptionPlan.cs
  - PaymentTransaction.cs (extended from reference)
  - Invoice.cs
  - Refund.cs
  - WebhookEventInbound.cs
  - AuditLog.cs
  - TenantSettings.cs (or JSON in Tenant)
```

**Entity Details:**

**Tenant.cs** (extended from reference)
```csharp
public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; }                    // Company name
    public string Slug { get; set; }                    // URL-safe ID (unique)
    public string PublicApiKey { get; set; }             // pk_live_xxx
    public string SecretApiKeyHash { get; set; }         // HMAC verification (SHA256)
    public string WebhookSigningSecret { get; set; }     // Signs outbound callbacks
    public string WebhookCallbackUrl { get; set; }       // Client's callback URL
    public string JwtSigningSecret { get; set; }         // User portal JWT
    public string StripeSecretKeyEnc { get; set; }       // AES-256 encrypted
    public string StripePublishableKey { get; set; }     // Client-side Stripe key
    public string StripeWebhookSecret { get; set; }      // Stripe webhook verify
    public string Settings { get; set; }                 // JSON: branding, refund policy, dunning
    public string Plan { get; set; }                     // starter/standard/advanced
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**Customer.cs** (NEW)
```csharp
public class Customer
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string ExternalReferenceId { get; set; }      // Client's user ID
    public string StripeCustomerId { get; set; }         // cus_xxx
    public string Email { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Currency { get; set; }                 // 3-letter ISO
    public string BillingAddress { get; set; }           // JSON
    public string TaxId { get; set; }
    public string Metadata { get; set; }                 // JSON
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public Tenant Tenant { get; set; }
    public ICollection<Subscription> Subscriptions { get; set; }
    public ICollection<PaymentTransaction> Transactions { get; set; }
    public ICollection<Invoice> Invoices { get; set; }
}
```

**SubscriptionPlan.cs** (NEW)
```csharp
public class SubscriptionPlan
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string StripeProductId { get; set; }          // prod_xxx
    public string StripePriceId { get; set; }            // price_xxx
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Interval { get; set; }                 // month/year/week
    public int IntervalCount { get; set; } = 1;
    public int TrialDays { get; set; }
    public string Features { get; set; }                 // JSON array
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    
    public Tenant Tenant { get; set; }
}
```

**Subscription.cs** (NEW)
```csharp
public class Subscription
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid PlanId { get; set; }
    public string StripeSubscriptionId { get; set; }     // sub_xxx
    public string Status { get; set; }                   // active/trialing/past_due/canceled/paused/unpaid
    public int Quantity { get; set; } = 1;
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public DateTime? TrialStart { get; set; }
    public DateTime? TrialEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string CancellationReason { get; set; }
    public string Metadata { get; set; }                 // JSON
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public Tenant Tenant { get; set; }
    public Customer Customer { get; set; }
    public SubscriptionPlan Plan { get; set; }
}
```

**PaymentTransaction.cs** (extended from reference)
```csharp
public class PaymentTransaction
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public string StripePaymentIntentId { get; set; }    // pi_xxx
    public string StripeChargeId { get; set; }           // ch_xxx
    public string StripeCheckoutSessionId { get; set; }  // cs_xxx
    public decimal Amount { get; set; }
    public decimal AmountRefunded { get; set; }
    public string Currency { get; set; }
    public string Status { get; set; }                   // succeeded/failed/pending/refunded
    public string Type { get; set; }                     // one_time/subscription/refund
    public string PaymentMethod { get; set; }            // card/bank_transfer/etc
    public string PaymentMethodLast4 { get; set; }       // Last 4 digits
    public string PaymentMethodBrand { get; set; }       // visa/mastercard
    public string Description { get; set; }
    public string FailureReason { get; set; }
    public string ReceiptUrl { get; set; }
    public string Metadata { get; set; }                 // JSON
    public DateTime CreatedAt { get; set; }
    
    public Tenant Tenant { get; set; }
    public Customer Customer { get; set; }
}
```

**Invoice.cs** (NEW)
```csharp
public class Invoice
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public string StripeInvoiceId { get; set; }          // in_xxx
    public string InvoiceNumber { get; set; }            // INV-001
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }
    public string Currency { get; set; }
    public string Status { get; set; }                   // draft/open/paid/void/uncollectible
    public string InvoicePdfUrl { get; set; }            // Stripe hosted PDF
    public string HostedInvoiceUrl { get; set; }         // Stripe hosted page
    public string LineItems { get; set; }                // JSON array
    public DateTime? PaidAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Tenant Tenant { get; set; }
    public Customer Customer { get; set; }
}
```

**Refund.cs** (NEW)
```csharp
public class Refund
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TransactionId { get; set; }
    public Guid? CustomerId { get; set; }
    public string StripeRefundId { get; set; }           // re_xxx
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Reason { get; set; }                   // duplicate/fraudulent/requested_by_customer/other
    public string Notes { get; set; }
    public string Status { get; set; }                   // pending/approved/processing/succeeded/failed/rejected
    public string ApprovedBy { get; set; }               // Admin user ID
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Tenant Tenant { get; set; }
    public PaymentTransaction Transaction { get; set; }
    public Customer Customer { get; set; }
}
```

**WebhookEventInbound.cs** (NEW)
```csharp
public class WebhookEventInbound
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string StripeEventId { get; set; }            // evt_xxx (unique - deduplication)
    public string EventType { get; set; }                // checkout.session.completed, etc.
    public string Payload { get; set; }                  // Full JSON from Stripe
    public string Status { get; set; }                   // received/processing/completed/failed
    public string ProcessingError { get; set; }
    public int RetryCount { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    
    public Tenant Tenant { get; set; }
}
```

**AuditLog.cs** (NEW)
```csharp
public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }                  // Nullable for super admin actions
    public Guid UserId { get; set; }
    public string UserEmail { get; set; }
    public string Action { get; set; }                   // refund.approved, key.rotated, tenant.created
    public string EntityType { get; set; }               // Transaction, Subscription, Tenant
    public string EntityId { get; set; }
    public string Details { get; set; }                  // JSON: before/after state
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

#### Task 1.5 — BillingDbContext
```
File: Core/Infrastructure/BillingDbContext.cs

Based on reference GatewayDbContext pattern:
- DbSet for each entity (16 total)
- Global query filter: entity.TenantId == _tenantId (for all tenant-scoped entities)
- Fluent API configurations:
  - Tenant: UNIQUE(Slug), UNIQUE(PublicApiKey)
  - User: UNIQUE(TenantId, Email)
  - Customer: UNIQUE(TenantId, StripeCustomerId), INDEX(TenantId, ExternalReferenceId)
  - Subscription: INDEX(TenantId, Status), INDEX(StripeSubscriptionId)
  - PaymentTransaction: INDEX(TenantId, CreatedAt DESC), INDEX(StripePaymentIntentId)
  - Invoice: INDEX(TenantId, Status), INDEX(StripeInvoiceId)
  - Refund: INDEX(TenantId, Status)
  - WebhookEventInbound: UNIQUE(StripeEventId), INDEX(TenantId, Status)
  - ApiCallLog: INDEX(TenantId, CreatedAt DESC)
  - AuditLog: INDEX(TenantId, CreatedAt DESC), INDEX(UserId)
  - ApiKey: UNIQUE(KeyHash)
  - WebhookDelivery: INDEX(Status, NextRetryAt)
```

#### Task 1.6 — Program.cs
```
File: WebAPI/Program.cs

Based on reference Program.cs. Identical structure with additions:

Identical to reference:
  - Controllers + JSON options (camelCase, enums as strings)
  - CORS (AllowCredentials for SignalR)
  - ExceptionHandler
  - TenantContextProvider (scoped)
  - DbContext with SQL Server (retry on failure)
  - Serilog (Warning level, console)
  - JWT Authentication (with SignalR query string support)
  - Authorization policies: SuperAdminOnly, AdminOrAbove, ManagerOrAbove, AllRoles
  - SignalR hub at /hubs/dashboard
  - DataProtection (encrypt Stripe credentials)
  - Auto-register services + repositories (DiRegistrationExtensions)
  - MemoryCache
  - FluentValidation auto-registration
  - Swagger with JWT auth + API key headers
  - Background services: WebhookDispatcherService, WebhookRetryService
  
New additions:
  - AddHttpClient("Stripe") — typed HTTP client for Stripe API
  - Middleware order: Exception → CORS → Logging → RateLimit → ApiKey → HMAC → Tenant → JWT → Auth
  - HmacAuthMiddleware applied to /api/v1/payments, /subscriptions, /customers paths
  - Seed: default tenant + SuperAdmin user + sample plans
```

#### Task 1.7 — Migrations & Seed Data
```bash
dotnet ef migrations add InitialCreate -p Core -s WebAPI
dotnet ef database update -p Core -s WebAPI
```

Seed data:
- Default tenant: "Demo SaaS" with slug "demo", test Stripe keys
- SuperAdmin user: admin@billing.io / hashed password
- 3 sample subscription plans: Basic ($9/mo), Pro ($29/mo), Enterprise ($99/mo)
- Sample API key pair (pk_test + sk_test)

### Day 1 Checklist
- [ ] Solution created with Core + WebAPI + Tests projects
- [ ] All identical files copied from reference (namespace changed)
- [ ] HmacAuthMiddleware written and tested
- [ ] All 16 entity classes created
- [ ] BillingDbContext with DbSets, indexes, and global query filter
- [ ] Program.cs fully configured
- [ ] Migration created and applied
- [ ] Seed data loaded
- [ ] Health endpoint responding: GET /health
- [ ] Swagger UI accessible
- [ ] Git commit: `feat: project foundation, DB schema, HMAC auth, tenant system`

---

## 3. Day 2 — Payments & Stripe Integration

### Goal: One-time payments via Checkout + PaymentIntent, Stripe webhook receiver, transaction logging

### Morning (4 hours)

#### Task 2.1 — IPaymentGateway + StripePaymentGateway
```
Files:
  - Core/ServiceContracts/IPaymentGateway.cs
  - Core/Services/StripePaymentGateway.cs

Interface methods:
  - CreateCheckoutSessionAsync(CreateCheckoutDto) → GatewayResponseWrapper<CheckoutResponseDto>
  - CreatePaymentIntentAsync(CreateIntentDto) → GatewayResponseWrapper<IntentResponseDto>
  - GetPaymentAsync(Guid id) → GatewayResponseWrapper<PaymentResponseDto>
  - ListPaymentsAsync(PaymentFilterDto) → GatewayResponseWrapper<PaginatedList<PaymentResponseDto>>
  - GetPaymentAnalyticsAsync(string period) → GatewayResponseWrapper<PaymentAnalyticsDto>

StripePaymentGateway implementation:
  1. Load tenant's encrypted Stripe secret key → decrypt with EncryptionService
  2. Create StripeClient with tenant's key
  3. Call Stripe API (CheckoutSessionService or PaymentIntentService)
  4. Save PaymentTransaction entity
  5. Queue outbound webhook event
  6. Return via GatewayResponseWrapper

CreateCheckoutDto:
  - customerId: Guid (or externalReferenceId: string)
  - lineItems: [{ name, description, amount, currency, quantity }]
  - successUrl: string
  - cancelUrl: string
  - mode: "payment" | "subscription"
  - metadata: object

CreateIntentDto:
  - customerId: Guid
  - amount: decimal
  - currency: string
  - paymentMethodId: string (optional)
  - description: string
  - metadata: object
```

#### Task 2.2 — CustomerService
```
Files:
  - Core/ServiceContracts/ICustomerService.cs
  - Core/Services/CustomerService.cs

Methods:
  - CreateAsync(CreateCustomerDto) → Create in DB + Stripe (stripe.customers.create)
  - GetAsync(Guid id) → Customer with subscriptions + recent transactions
  - GetByExternalRefAsync(string externalRefId) → Lookup by client's user ID
  - UpdateAsync(Guid id, UpdateCustomerDto) → Update DB + Stripe sync
  - ListAsync(CustomerFilterDto) → Paginated list
  - CreatePortalSessionAsync(Guid id) → Stripe Customer Portal session URL

Stripe sync:
  - On create: call Stripe customers.create, store StripeCustomerId
  - On update: call Stripe customers.update
  - Store ExternalReferenceId = client's own user ID for mapping
```

### Afternoon (4 hours)

#### Task 2.3 — Stripe Webhook Receiver (Inbound)
```
Files:
  - Core/ServiceContracts/IStripeWebhookHandler.cs
  - Core/Services/StripeWebhookHandler.cs
  - WebAPI/Controllers/v1/WebhookInboundController.cs

Endpoint: POST /api/v1/webhooks/stripe (no JWT/API key — Stripe signature only)

Controller:
  1. Read raw body
  2. Get Stripe-Signature header
  3. For each tenant: try ConstructEvent with their webhook secret
  4. On match: pass to StripeWebhookHandler.ProcessAsync(tenantId, stripeEvent)

StripeWebhookHandler.ProcessAsync:
  - Save WebhookEventInbound (dedup by StripeEventId)
  - Switch on event type:
    - checkout.session.completed → Record transaction, activate subscription
    - payment_intent.succeeded → Update transaction status
    - payment_intent.payment_failed → Log failure, update status
    - invoice.paid → Mark invoice paid
    - invoice.payment_failed → Start dunning
    - (More events added on Day 3 and 4)
  - Queue outbound webhook to client app
  - Push SignalR event to dashboard
```

#### Task 2.4 — PaymentController
```
File: WebAPI/Controllers/v1/PaymentController.cs

Inherits GatewayControllerBase (from reference)
Auth: API Key + HMAC (HmacAuthMiddleware)

Endpoints:
  POST   /api/v1/payments/checkout         → Create Checkout Session
  POST   /api/v1/payments/create-intent    → Create Payment Intent
  GET    /api/v1/payments/{id}             → Get payment details
  GET    /api/v1/payments                  → List payments (paginated, filtered)
  GET    /api/v1/payments/analytics        → Payment analytics (revenue, count, success rate)

All return GatewayResponseWrapper<T> via ToResponse() (identical to reference pattern)
```

#### Task 2.5 — CustomerController
```
File: WebAPI/Controllers/v1/CustomerController.cs

Auth: API Key + HMAC

Endpoints:
  POST   /api/v1/customers                     → Create customer (Stripe sync)
  GET    /api/v1/customers/{id}                → Get customer + subscriptions + transactions
  PUT    /api/v1/customers/{id}                → Update customer (Stripe sync)
  GET    /api/v1/customers                     → List customers (paginated)
  POST   /api/v1/customers/{id}/portal-session → Generate Stripe portal URL
```

#### Task 2.6 — Validators
```
Files:
  - CreateCheckoutValidator: lineItems not empty, amounts > 0, currency valid
  - CreateIntentValidator: amount > 0, currency 3 letters
  - CreateCustomerValidator: email valid, name required
```

### Day 2 Checklist
- [ ] Stripe Checkout Session creation working
- [ ] Payment Intent creation working
- [ ] Customer create/read/update with Stripe sync
- [ ] Stripe webhook endpoint receiving events
- [ ] Stripe signature verification working per tenant
- [ ] PaymentTransaction saved to DB for every operation
- [ ] WebhookEventInbound logged and deduplicated
- [ ] Outbound webhook queued after Stripe events processed
- [ ] Payment analytics endpoint returning revenue data
- [ ] Git commit: `feat: Stripe payments, customer sync, inbound webhooks`

---

## 4. Day 3 — Subscriptions & Plans

### Goal: Full subscription lifecycle — create, upgrade, downgrade, cancel, pause, trials, dunning

### Morning (4 hours)

#### Task 3.1 — ISubscriptionPlanService + Implementation
```
Files:
  - Core/ServiceContracts/ISubscriptionPlanService.cs
  - Core/Services/SubscriptionPlanService.cs

Methods:
  - CreateAsync(CreatePlanDto) → Create in DB + Stripe (products.create + prices.create)
  - GetAsync(Guid id) → Plan detail
  - ListAsync(Guid tenantId) → All plans for tenant
  - UpdateAsync(Guid id, UpdatePlanDto) → Update name/description/features (price immutable in Stripe)
  - DeleteAsync(Guid id) → Soft delete (archive in Stripe)
  - SyncFromStripeAsync() → Import plans from Stripe
  - ToggleActiveAsync(Guid id) → Enable/disable

CreatePlanDto:
  - name, description, amount, currency, interval (month/year/week)
  - intervalCount (1 for monthly, 1 for annual)
  - trialDays, features (string[]), sortOrder
```

#### Task 3.2 — ISubscriptionService + Implementation
```
Files:
  - Core/ServiceContracts/ISubscriptionService.cs
  - Core/Services/SubscriptionService.cs

Methods:
  - CreateAsync(CreateSubscriptionDto) → Create Stripe subscription, save locally
  - GetAsync(Guid id) → Subscription with plan + customer details
  - ListAsync(SubscriptionFilterDto) → Paginated, filterable by status/plan
  - UpdateAsync(Guid id, UpdateSubscriptionDto) → Change plan, quantity
  - CancelAsync(Guid id, CancelSubscriptionDto) → Immediate or at-period-end
  - PauseAsync(Guid id) → Pause billing (Stripe pause_collection)
  - ResumeAsync(Guid id) → Resume paused subscription
  - PreviewProrationAsync(Guid id, Guid newPlanId) → Preview cost change

CreateSubscriptionDto:
  - customerId: Guid (or externalReferenceId)
  - planId: Guid
  - quantity: int (default 1, for per-seat)
  - trialDays: int (override plan default)
  - couponCode: string (optional)
  - metadata: object

UpdateSubscriptionDto:
  - planId: Guid (new plan for upgrade/downgrade)
  - quantity: int
  - prorationBehavior: "create_prorations" | "none" | "always_invoice"

CancelSubscriptionDto:
  - cancelAtPeriodEnd: bool (true = cancel at end, false = immediate)
  - reason: string (cancellation reason)

PreviewProrationAsync:
  - Uses Stripe Invoice.upcoming with subscription_items to show cost preview
  - Returns: currentPlan, newPlan, proratedAmount, effectiveDate
```

### Afternoon (4 hours)

#### Task 3.3 — Expand Stripe Webhook Handler
```
Add to StripeWebhookHandler.ProcessAsync:

  customer.subscription.created → Save Subscription entity, queue subscription.activated
  customer.subscription.updated → Handle plan changes, status changes
    - If plan changed: queue subscription.upgraded or subscription.downgraded
    - If status → past_due: queue subscription.payment_failed
    - If status → canceled: queue subscription.cancelled
  customer.subscription.deleted → Mark cancelled, queue subscription.cancelled
  customer.subscription.trial_will_end → Queue subscription.trial_ending (3 days before)
  invoice.paid → Mark invoice paid, extend subscription period
  invoice.payment_failed → Start dunning sequence, queue subscription.payment_failed
  charge.refunded → Process refund (handled more on Day 4)
```

#### Task 3.4 — SubscriptionController
```
File: WebAPI/Controllers/v1/SubscriptionController.cs

Auth: API Key + HMAC

Endpoints:
  POST   /api/v1/subscriptions                   → Create subscription
  GET    /api/v1/subscriptions/{id}              → Get subscription details
  GET    /api/v1/subscriptions                   → List subscriptions (paginated)
  PUT    /api/v1/subscriptions/{id}              → Update (change plan, quantity)
  DELETE /api/v1/subscriptions/{id}              → Cancel subscription
  POST   /api/v1/subscriptions/{id}/pause        → Pause billing
  POST   /api/v1/subscriptions/{id}/resume       → Resume billing
  GET    /api/v1/subscriptions/{id}/preview      → Preview proration for plan change
```

#### Task 3.5 — PlanController
```
File: WebAPI/Controllers/v1/PlanController.cs

Auth: JWT (Admin or above)

Endpoints:
  GET    /api/v1/plans                → List plans for tenant
  POST   /api/v1/plans                → Create plan (Stripe sync)
  GET    /api/v1/plans/{id}           → Plan detail
  PUT    /api/v1/plans/{id}           → Update plan
  DELETE /api/v1/plans/{id}           → Archive plan
  POST   /api/v1/plans/sync           → Sync from Stripe
```

#### Task 3.6 — Validators
```
  - CreateSubscriptionValidator: customerId required, planId required
  - UpdateSubscriptionValidator: planId or quantity required
  - CreatePlanValidator: name required, amount > 0, interval valid
```

### Day 3 Checklist
- [ ] Subscription plan CRUD with Stripe product/price sync
- [ ] Subscription creation with Stripe
- [ ] Plan upgrade/downgrade with proration preview
- [ ] Cancellation (immediate + at-period-end)
- [ ] Pause/resume subscription
- [ ] Trial period support
- [ ] Webhook handler updated for subscription events
- [ ] All subscription states properly tracked
- [ ] Git commit: `feat: subscription billing, plans, upgrade/downgrade, trials, dunning`

---

## 5. Day 4 — Webhooks (Double Webhook System)

### Goal: Complete outbound webhook delivery, refund management, invoice service, audit logging

### Morning (4 hours)

#### Task 4.1 — Outbound Webhook Events
```
Define all 11 outbound events in Core/Constants/WebhookEvents.cs:

public static class OutboundEvents
{
    public const string PaymentCompleted = "payment.completed";
    public const string PaymentFailed = "payment.failed";
    public const string SubscriptionActivated = "subscription.activated";
    public const string SubscriptionUpgraded = "subscription.upgraded";
    public const string SubscriptionDowngraded = "subscription.downgraded";
    public const string SubscriptionCancelled = "subscription.cancelled";
    public const string SubscriptionTrialEnding = "subscription.trial_ending";
    public const string SubscriptionPaymentFailed = "subscription.payment_failed";
    public const string RefundProcessed = "refund.processed";
    public const string InvoiceGenerated = "invoice.generated";
    public const string CustomerUpdated = "customer.updated";
}

Outbound webhook payload format:
{
  "id": "evt_xxx",
  "type": "payment.completed",
  "tenantId": "...",
  "timestamp": "2026-02-26T...",
  "data": { ... event-specific data ... }
}

Headers sent:
  X-Webhook-Signature: HMAC-SHA256(payload + timestamp, webhookSecret)
  X-Webhook-Timestamp: Unix timestamp
  X-Webhook-ID: Unique event ID
  X-Webhook-Retry: Attempt number (0, 1, 2...)
  Content-Type: application/json
```

#### Task 4.2 — RefundService
```
Files:
  - Core/ServiceContracts/IRefundService.cs
  - Core/Services/RefundService.cs

Methods:
  - CreateAsync(CreateRefundDto) → Validate, create Refund (status=pending if above threshold)
  - GetAsync(Guid id) → Refund detail
  - ListAsync(RefundFilterDto) → Paginated, filterable by status
  - ApproveAsync(Guid id, Guid approvedBy) → Set approved, process via Stripe
  - RejectAsync(Guid id, string reason) → Set rejected
  - ProcessStripeRefundAsync(Guid refundId) → Call Stripe Refund API

Approval workflow:
  - Tenant settings: autoApproveThreshold (e.g., $25)
  - Below threshold → auto-approve → process immediately
  - Above threshold → status=pending → admin approves from dashboard
  - On approval → call Stripe, update PaymentTransaction.AmountRefunded
  - Queue outbound webhook: refund.processed
```

#### Task 4.3 — InvoiceService
```
Files:
  - Core/ServiceContracts/IInvoiceService.cs
  - Core/Services/InvoiceService.cs

Methods:
  - GetAsync(Guid id) → Invoice detail with line items
  - ListAsync(InvoiceFilterDto) → Paginated, filterable by status
  - GetPdfUrlAsync(Guid id) → Return Stripe-hosted PDF URL
  - VoidAsync(Guid id) → Void invoice in Stripe + DB
  - SendEmailAsync(Guid id) → Send invoice email to customer
  - SyncFromStripeAsync(string stripeInvoiceId) → Import/update from Stripe event

Invoices are primarily created by Stripe (subscription cycles, one-time).
Service syncs from Stripe webhook events (invoice.paid, invoice.created).
```

### Afternoon (4 hours)

#### Task 4.4 — AuditService
```
Files:
  - Core/ServiceContracts/IAuditService.cs
  - Core/Services/AuditService.cs

Methods:
  - LogAsync(AuditLogDto) → Create immutable audit entry
  - ListAsync(AuditFilterDto) → Paginated, filterable by action/user/entity/date
  - GetAsync(Guid id) → Entry detail with before/after diff

Called from:
  - RefundService.ApproveAsync → audit("refund.approved", ...)
  - ApiKeyService on key rotation → audit("apiKey.rotated", ...)
  - TenantService on suspend → audit("tenant.suspended", ...)
  - SettingsService on update → audit("settings.updated", { before, after })
  - All destructive admin actions
```

#### Task 4.5 — RefundController + InvoiceController + AuditController
```
RefundController (Auth: API Key + HMAC / JWT Admin):
  POST   /api/v1/refunds                    → Create refund request
  GET    /api/v1/refunds/{id}               → Refund detail
  GET    /api/v1/refunds                    → List refunds
  POST   /api/v1/refunds/{id}/approve       → Approve (Admin JWT)
  POST   /api/v1/refunds/{id}/reject        → Reject (Admin JWT)

InvoiceController (Auth: API Key / JWT):
  GET    /api/v1/invoices                   → List invoices
  GET    /api/v1/invoices/{id}              → Invoice detail
  GET    /api/v1/invoices/{id}/pdf          → PDF URL
  POST   /api/v1/invoices/{id}/void         → Void invoice (Admin)
  POST   /api/v1/invoices/{id}/send         → Send email (Admin)

AuditController (Auth: JWT Admin):
  GET    /api/v1/audit-log                  → List audit entries (paginated)
  GET    /api/v1/audit-log/{id}             → Entry detail
```

#### Task 4.6 — WebhookController (Outbound Management)
```
File: WebAPI/Controllers/v1/WebhookController.cs (from reference — identical)

Endpoints (JWT):
  GET    /api/v1/webhooks/subscriptions               → List registered webhooks
  POST   /api/v1/webhooks/subscriptions               → Register callback URL
  PUT    /api/v1/webhooks/subscriptions/{id}           → Update
  DELETE /api/v1/webhooks/subscriptions/{id}           → Remove
  POST   /api/v1/webhooks/subscriptions/{id}/test      → Test webhook
  GET    /api/v1/webhooks/deliveries                   → Delivery log
  GET    /api/v1/webhooks/deliveries/{id}              → Delivery detail
  POST   /api/v1/webhooks/deliveries/{id}/retry        → Manual retry
```

### Day 4 Checklist
- [ ] All 11 outbound webhook events defined and documented
- [ ] Outbound webhook signing with HMAC-SHA256
- [ ] Refund service with approval workflow
- [ ] Auto-approve below threshold, manual above
- [ ] Refund processed via Stripe API on approval
- [ ] Invoice service syncing from Stripe events
- [ ] Invoice PDF URL retrieval
- [ ] Audit logging on all admin actions
- [ ] Webhook management endpoints (from reference)
- [ ] End-to-end test: payment → Stripe webhook → outbound callback
- [ ] Git commit: `feat: double webhook system, refunds, invoices, audit logging`

---

## 6. Day 5 — User Portal (React Frontend)

### Goal: React project setup, auth flow, user portal pages (JWT passthrough)

### Morning (4 hours)

#### Task 5.1 — React Project Setup
```
frontend/
├── src/
│   ├── api/
│   │   ├── api-client.ts          ← From reference (identical)
│   │   ├── interceptors.ts        ← From reference (identical)
│   │   ├── apiWrapper.ts          ← From reference (identical)
│   │   ├── index.ts               ← Export all APIs
│   │   ├── authApi.ts
│   │   ├── paymentApi.ts          ← NEW
│   │   ├── subscriptionApi.ts     ← NEW
│   │   ├── customerApi.ts         ← NEW
│   │   ├── invoiceApi.ts          ← NEW
│   │   ├── refundApi.ts           ← NEW
│   │   ├── planApi.ts             ← NEW
│   │   ├── dashboardApi.ts
│   │   ├── analyticsApi.ts        ← NEW
│   │   ├── webhookApi.ts
│   │   ├── connectionApi.ts
│   │   ├── apiKeyApi.ts
│   │   ├── userApi.ts
│   │   └── settingsApi.ts
│   ├── components/
│   │   ├── common/
│   │   │   ├── DataTable.tsx       ← From reference
│   │   │   ├── MetricCard.tsx      ← From reference
│   │   │   ├── StatusBadge.tsx     ← From reference
│   │   │   ├── SearchInput.tsx     ← From reference
│   │   │   ├── LoadingSkeleton.tsx  ← From reference
│   │   │   ├── JsonViewer.tsx      ← From reference
│   │   │   ├── CodeSnippet.tsx     ← From reference
│   │   │   ├── EmptyState.tsx      ← From reference
│   │   │   ├── ConfirmDialog.tsx   ← From reference
│   │   │   ├── RevenueChart.tsx    ← NEW
│   │   │   ├── SubscriptionBadge.tsx ← NEW
│   │   │   ├── RefundBadge.tsx     ← NEW
│   │   │   ├── InvoiceViewer.tsx   ← NEW
│   │   │   ├── PlanCard.tsx        ← NEW
│   │   │   └── WebhookStatusBadge.tsx ← NEW
│   │   └── layout/
│   │       ├── Sidebar.tsx
│   │       └── Topbar.tsx
│   ├── contexts/
│   │   ├── AuthContext.tsx          ← From reference (identical)
│   │   ├── ToastContext.tsx         ← From reference (identical)
│   │   └── SidebarContext.tsx       ← From reference (identical)
│   ├── hooks/
│   │   ├── useAuth.ts              ← From reference (identical)
│   │   ├── useToast.ts             ← From reference (identical)
│   │   ├── useSidebar.ts           ← From reference (identical)
│   │   └── useDebounce.ts          ← From reference (identical)
│   ├── layouts/
│   │   ├── AuthLayout.tsx           ← From reference (identical)
│   │   └── DashboardLayout.tsx      ← From reference (adapted sidebar items)
│   ├── pages/
│   │   ├── auth/LoginPage.tsx
│   │   ├── dashboard/DashboardPage.tsx
│   │   ├── billing/
│   │   │   ├── PaymentsPage.tsx
│   │   │   ├── SubscriptionsPage.tsx
│   │   │   ├── CustomersPage.tsx
│   │   │   ├── InvoicesPage.tsx
│   │   │   ├── RefundsPage.tsx
│   │   │   └── PlansPage.tsx
│   │   ├── analytics/RevenueAnalyticsPage.tsx
│   │   ├── gateway/
│   │   │   ├── ApiKeysPage.tsx      ← From reference
│   │   │   ├── ConnectionsPage.tsx  ← From reference
│   │   │   ├── LogsPage.tsx         ← From reference
│   │   │   └── WebhooksPage.tsx     ← From reference
│   │   ├── users/UsersPage.tsx      ← From reference
│   │   ├── settings/SettingsPage.tsx ← Extended
│   │   └── audit/AuditLogPage.tsx
│   ├── routes/
│   │   ├── AppRoutes.tsx
│   │   └── ProtectedRoute.tsx       ← From reference (identical)
│   ├── types/
│   │   ├── common.ts               ← From reference
│   │   ├── auth.ts
│   │   ├── payment.ts              ← NEW
│   │   ├── subscription.ts         ← NEW
│   │   ├── customer.ts             ← NEW
│   │   ├── invoice.ts              ← NEW
│   │   ├── refund.ts               ← NEW
│   │   ├── plan.ts                 ← NEW
│   │   ├── analytics.ts            ← NEW
│   │   ├── apiKey.ts
│   │   ├── connection.ts
│   │   ├── webhook.ts
│   │   ├── log.ts
│   │   └── dashboard.ts
│   ├── utils/
│   │   ├── formatters.ts           ← Currency, date, number formatters
│   │   └── jwt.ts                  ← Token decode, isExpired check
│   ├── App.tsx
│   └── main.tsx
├── index.html
├── package.json
├── tsconfig.json
└── vite.config.ts
```

#### Task 5.2 — Copy Identical Files from Reference
```
From 02-API-Gateway-Microservice/frontend/src/:

api/:
  - api-client.ts → Identical (change base URL in .env)
  - interceptors.ts → Identical (JWT attach, 401 refresh with queue)
  - apiWrapper.ts → Identical (typed response wrapper)

contexts/:
  - AuthContext.tsx → Identical
  - ToastContext.tsx → Identical
  - SidebarContext.tsx → Identical

hooks/:
  - useAuth.ts, useToast.ts, useSidebar.ts, useDebounce.ts → All identical

layouts/:
  - AuthLayout.tsx → Identical
  - DashboardLayout.tsx → Same structure, updated sidebar nav items

components/common/:
  - DataTable.tsx, MetricCard.tsx, StatusBadge.tsx → Identical
  - SearchInput.tsx, LoadingSkeleton.tsx → Identical
  - JsonViewer.tsx, CodeSnippet.tsx → Identical
  - EmptyState.tsx, ConfirmDialog.tsx → Identical

routes/:
  - ProtectedRoute.tsx → Identical

types/:
  - common.ts → Identical (GatewayResponse<T>, PaginatedResponse<T>)
```

#### Task 5.3 — PortalController (Backend)
```
File: WebAPI/Controllers/v1/PortalController.cs

Auth: JWT Passthrough (customer JWT from client app)

Endpoints (user portal — accessible via JWT passthrough):
  GET    /api/v1/portal/me                       → Customer billing summary
  GET    /api/v1/portal/transactions             → Transaction history (own data only)
  GET    /api/v1/portal/subscriptions            → Active subscriptions
  GET    /api/v1/portal/invoices                 → Invoice history
  GET    /api/v1/portal/invoices/{id}/pdf        → Download invoice PDF
  GET    /api/v1/portal/payment-methods          → Saved payment methods (from Stripe)
  POST   /api/v1/portal/payment-methods/setup    → Create Stripe Setup Intent (add card)
  DELETE /api/v1/portal/payment-methods/{id}     → Detach payment method
  PUT    /api/v1/portal/subscriptions/{id}       → Change plan (if tenant allows)
  DELETE /api/v1/portal/subscriptions/{id}       → Cancel (if tenant allows)
  PUT    /api/v1/portal/billing-info             → Update billing address
```

### Afternoon (4 hours)

#### Task 5.4 — LoginPage
```
File: src/pages/auth/LoginPage.tsx

Identical pattern to reference:
- AuthLayout wrapper
- Email + password form (react-hook-form + zod)
- Login via authApi.login()
- On success: store token, redirect to /
- On failure: toast error
- Logo + project branding
```

#### Task 5.5 — TypeScript Types (NEW billing types)
```
src/types/payment.ts:
  export interface PaymentTransaction {
    id: string; tenantId: string; customerId: string;
    amount: number; currency: string; status: string; type: string;
    paymentMethod: string; paymentMethodLast4: string; paymentMethodBrand: string;
    description: string; failureReason: string; receiptUrl: string;
    createdAt: string;
  }
  export interface PaymentAnalytics {
    totalRevenue: number; netRevenue: number; transactionCount: number;
    successRate: number; revenueByDay: { date: string; amount: number }[];
  }
  export interface CreateCheckoutRequest { ... }

src/types/subscription.ts:
  export interface Subscription {
    id: string; customerId: string; planId: string;
    stripeSubscriptionId: string; status: string; quantity: number;
    currentPeriodStart: string; currentPeriodEnd: string;
    trialEnd: string | null; cancelAtPeriodEnd: boolean;
    plan: SubscriptionPlan; customer: Customer;
    createdAt: string;
  }
  export interface ProrationPreview {
    currentPlan: SubscriptionPlan; newPlan: SubscriptionPlan;
    proratedAmount: number; effectiveDate: string;
  }

src/types/customer.ts:
  export interface Customer {
    id: string; externalReferenceId: string; stripeCustomerId: string;
    email: string; name: string; currency: string;
    subscriptionCount: number; totalSpent: number; ltv: number;
    createdAt: string;
  }
  export interface CustomerDetail extends Customer {
    subscriptions: Subscription[];
    recentTransactions: PaymentTransaction[];
    invoices: Invoice[];
  }

src/types/invoice.ts:
  export interface Invoice {
    id: string; customerId: string; stripeInvoiceId: string;
    invoiceNumber: string; subtotal: number; tax: number; total: number;
    amountPaid: number; amountDue: number; currency: string; status: string;
    invoicePdfUrl: string; lineItems: InvoiceLineItem[];
    paidAt: string | null; dueDate: string | null; createdAt: string;
  }

src/types/refund.ts:
  export interface Refund {
    id: string; transactionId: string; customerId: string;
    amount: number; currency: string; reason: string; notes: string;
    status: string; approvedBy: string; createdAt: string;
  }
  export interface RefundStats {
    totalRefunds: number; totalAmount: number; refundRate: number;
    avgProcessingTime: number;
  }

src/types/plan.ts:
  export interface SubscriptionPlan {
    id: string; name: string; description: string;
    amount: number; currency: string; interval: string;
    trialDays: number; features: string[]; isActive: boolean;
  }

src/types/analytics.ts:
  export interface MrrData {
    currentMrr: number; previousMrr: number; mrrGrowth: number;
    newMrr: number; expansionMrr: number; contractionMrr: number; churnedMrr: number;
    mrrHistory: { month: string; mrr: number }[];
  }
  export interface ChurnData { monthlyChurnRate: number; annualChurnRate: number; ... }
  export interface LtvData { averageLtv: number; ... }

src/types/dashboard.ts:
  export interface DashboardStats {
    totalRevenue: number; netRevenue: number; mrrCurrent: number;
    activeSubscriptions: number; totalCustomers: number;
    revenueChange: number; subscriptionChange: number;
  }
```

#### Task 5.6 — API Module Files (NEW)
```
src/api/paymentApi.ts:
  createCheckout, createIntent, getPayment, listPayments, getAnalytics

src/api/subscriptionApi.ts:
  create, get, update, cancel, pause, resume, preview, listByCustomer

src/api/customerApi.ts:
  create, get, update, list, createPortalSession

src/api/invoiceApi.ts:
  list, get, downloadPdf, void, sendEmail, bulkExport

src/api/refundApi.ts:
  create, get, list, approve, reject

src/api/planApi.ts:
  list, create, update, delete, syncStripe

src/api/analyticsApi.ts:
  getMrr, getChurn, getLtv, getSubscriptionMetrics

All using apiWrapper pattern from reference:
  export const listPayments = (params: PaymentFilterDto) =>
    apiGet<PaginatedResponse<PaymentTransaction>>('/payments', { params });
```

#### Task 5.7 — NEW Reusable Components
```
src/components/common/RevenueChart.tsx:
  - Chart.js line chart wrapper
  - Props: data (date/amount pairs), period (7d/30d/90d), height
  - Responsive, tooltip with currency formatting

src/components/common/SubscriptionBadge.tsx:
  - Color-coded badge for subscription states
  - active=green, trialing=blue, past_due=amber, canceled=red, paused=gray

src/components/common/RefundBadge.tsx:
  - pending=amber, approved=blue, processing=blue, succeeded=green, rejected=red, failed=red

src/components/common/InvoiceViewer.tsx:
  - Modal showing invoice details: line items table, totals, PDF download button
  - Props: invoice data, onClose

src/components/common/PlanCard.tsx:
  - Card: plan name, price, interval, features list, edit/toggle buttons
  - Props: plan, onEdit, onToggle

src/components/common/WebhookStatusBadge.tsx:
  - delivered=green, failed=red, retrying=amber, dead_letter=red/bold
```

### Day 5 Checklist
- [ ] React project set up with Vite + TypeScript
- [ ] All reference files copied (api-client, contexts, hooks, layouts, components)
- [ ] Portal backend endpoints working (11 endpoints)
- [ ] Login page functional
- [ ] All TypeScript types defined (14 type files)
- [ ] All API modules created (14 files)
- [ ] 6 new reusable components built
- [ ] Routing skeleton with all 16 routes
- [ ] Git commit: `feat: React setup, portal backend, types, API modules, components`

---

## 7. Day 6 — Admin Dashboard: Core Pages (React)

### Goal: Dashboard home, login, payments page, subscriptions page, customers page

### Morning (4 hours)

#### Task 6.1 — Sidebar Navigation
```
File: src/components/layout/Sidebar.tsx

Nav structure (role-filtered):
  Dashboard           / 
  ─── Billing ───
    Payments          /billing/payments
    Subscriptions     /billing/subscriptions
    Customers         /billing/customers
    Invoices          /billing/invoices
    Refunds           /billing/refunds          (ManagerOrAbove)
    Plans             /billing/plans            (AdminOrAbove)
  ─── Analytics ───
    Revenue           /analytics                (ManagerOrAbove)
  ─── Gateway ───
    API Keys          /gateway/api-keys         (ManagerOrAbove)
    Connections       /gateway/connections       (AdminOrAbove)
    Logs              /gateway/logs
    Webhooks          /gateway/webhooks          (ManagerOrAbove)
  ─── Management ───
    Users             /users                    (AdminOrAbove)
    Settings          /settings                 (AdminOrAbove)
    Audit Log         /audit-log                (AdminOrAbove)

Icons: lucide-react (LayoutDashboard, CreditCard, Repeat, Users, FileText, RotateCcw, 
  BarChart3, Key, Plug, ScrollText, Webhook, Settings, ClipboardList)
```

#### Task 6.2 — DashboardPage
```
File: src/pages/dashboard/DashboardPage.tsx

Layout:
  Row 1: 4 MetricCards
    - Total Revenue (today/month selector) with % change
    - Active Subscriptions with % change
    - MRR (Monthly Recurring Revenue)
    - Total Customers

  Row 2: RevenueChart (Chart.js line)
    - 7d / 30d / 90d toggle buttons
    - Revenue over time

  Row 3: Two columns
    - Left: Recent Transactions (DataTable, last 10)
    - Right: Recent Activity feed (webhook deliveries, refunds, new subs)

Data: dashboardApi.getStats() + dashboardApi.getRevenueChart()
Real-time: SignalR connection for live updates (same pattern as reference)
```

#### Task 6.3 — PaymentsPage
```
File: src/pages/billing/PaymentsPage.tsx

Layout:
  Row 1: 3 MetricCards
    - Total Revenue (period)
    - Successful Payments (count)
    - Failed Payments (count + badge if > 0)

  Row 2: Filters
    - Date range picker (start/end)
    - Status dropdown: all/succeeded/failed/pending/refunded
    - Amount range: min/max
    - Search: customer name/email

  Row 3: DataTable
    - Columns: Date, Customer, Amount, Status (StatusBadge), Method, Actions
    - Sort: by date (desc default), amount
    - Click row → Payment Detail Modal:
      - Transaction ID, Stripe ID, amount, currency, status
      - Customer info, payment method (last4 + brand)
      - Receipt URL link
      - Failure reason (if failed)
      - Refund button (if succeeded)

  Row 4: Pagination

Data: paymentApi.listPayments() with query params
```

### Afternoon (4 hours)

#### Task 6.4 — SubscriptionsPage
```
File: src/pages/billing/SubscriptionsPage.tsx

Layout:
  Row 1: 4 MetricCards
    - Active Subscriptions
    - Trialing
    - Past Due (amber if > 0)
    - Cancelled (this month)

  Row 2: Filters
    - Status: all/active/trialing/past_due/canceled/paused
    - Plan: dropdown of all plans
    - Date range

  Row 3: DataTable
    - Columns: Customer, Plan, Status (SubscriptionBadge), Quantity, Next Invoice, Amount, Actions
    - Click row → Subscription Detail Modal:
      - Full subscription info
      - Plan details
      - Customer info
      - Period dates
      - Actions: Cancel, Pause, Change Plan
      - Plan change → Proration Preview Modal (shows cost preview before confirming)

Data: subscriptionApi.list()
```

#### Task 6.5 — CustomersPage
```
File: src/pages/billing/CustomersPage.tsx

Layout:
  Row 1: 2 MetricCards
    - Total Customers
    - New This Month

  Row 2: SearchInput + filters
    - Search by name/email
    - Has subscription: yes/no/all

  Row 3: DataTable
    - Columns: Name, Email, Subscriptions (count badge), Total Spent, LTV, Created
    - Click row → Customer Detail View (inline expand or separate route):
      - Customer info card
      - Subscriptions DataTable (active, with status badges)
      - Recent Transactions DataTable (last 20)
      - Invoices DataTable
      - Payment Methods list (from Stripe)
      - Edit customer button → modal

Data: customerApi.list(), click detail: customerApi.get(id)
```

### Day 6 Checklist
- [ ] Sidebar navigation with all sections and role filtering
- [ ] Dashboard page with 4 MetricCards + RevenueChart + recent activity
- [ ] Payments page with filters, DataTable, detail modal
- [ ] Subscriptions page with status badges, filters, detail modal with actions
- [ ] Customers page with search, detail view with sub-tables
- [ ] SignalR connection for live dashboard updates
- [ ] Git commit: `feat: React dashboard, payments, subscriptions, customers pages`

---

## 8. Day 7 — Admin Dashboard: Billing Pages (React)

### Goal: Invoices, refunds, plans, API keys, connections, webhooks, logs, users, settings pages

### Morning (4 hours)

#### Task 7.1 — InvoicesPage
```
File: src/pages/billing/InvoicesPage.tsx

Layout:
  Filters: status (all/draft/open/paid/void), date range, customer search
  DataTable: Invoice #, Customer, Amount, Status (StatusBadge), Date, Actions
  Actions per row: View (InvoiceViewer modal), Download PDF, Send Email, Void
  Bulk action: Export selected as ZIP

Data: invoiceApi.list()
```

#### Task 7.2 — RefundsPage
```
File: src/pages/billing/RefundsPage.tsx

Layout:
  Two tabs: Pending Queue | History

  Pending tab:
    DataTable: Customer, Transaction, Amount, Reason, Requested Date, Actions
    Actions: Approve (green button), Reject (red button + reason modal)
    Badge showing count of pending refunds

  History tab:
    Filters: status, date range
    DataTable: Customer, Amount, Reason, Status (RefundBadge), Approved By, Date

  MetricCards (top):
    - Total Refunds (count + amount)
    - Refund Rate (% of revenue)
    - Avg Processing Time

Data: refundApi.list()
```

#### Task 7.3 — PlansPage
```
File: src/pages/billing/PlansPage.tsx

Layout:
  Header: "Subscription Plans" + "Create Plan" button
  Grid of PlanCard components (2-3 per row):
    - Plan name, price, interval
    - Feature list (checkmarks)
    - Subscriber count
    - Active toggle
    - Edit button, Delete button
  
  Create/Edit Plan Modal:
    - Name, Description
    - Price (number input) + Currency (dropdown)
    - Interval (month/year dropdown)
    - Trial days
    - Features (tag-style input, add/remove)
    - Active toggle

  Sync from Stripe button (top right)

Data: planApi.list()
```

### Afternoon (4 hours)

#### Task 7.4 — Gateway Pages (From Reference)
```
These pages are identical to the reference project, just copy and adapt:

ApiKeysPage.tsx:
  - DataTable with key name, prefix, status, last used, total requests
  - Create modal (show key ONCE with copy)
  - Revoke with ConfirmDialog

ConnectionsPage.tsx:
  - Card grid: Stripe connection card
  - Setup modal with encrypted credential fields
  - Test connection button

LogsPage.tsx:
  - DataTable with endpoint, method, status, duration, date
  - Filters: service type, status, date range
  - Log detail modal with request/response JSON (JsonViewer)
  - LogStats MetricCards

WebhooksPage.tsx:
  - Two tabs: Subscriptions (CRUD) + Deliveries
  - Subscriptions: URL, events, status, test button
  - Deliveries: DataTable with status (WebhookStatusBadge), payload, retry button
```

#### Task 7.5 — UsersPage (From Reference)
```
Identical to reference:
  - DataTable: name, email, role (badge), status, last login
  - Invite User modal
  - Edit Role modal
  - Deactivate with ConfirmDialog
```

#### Task 7.6 — SettingsPage (Extended)
```
File: src/pages/settings/SettingsPage.tsx

Tabs:
  General: Webhook callback URL, API version
  Branding: Logo upload, primary color, invoice header/footer
  Billing: Refund policy (auto-approve threshold, window), dunning schedule
  Stripe: Connected Stripe account info, test/live mode toggle

Forms: react-hook-form + zod validation
Save: settingsApi.updateSettings()
```

### Day 7 Checklist
- [ ] Invoices page with status filter, PDF download, void, send email
- [ ] Refunds page with pending queue, approve/reject, history, stats
- [ ] Plans page with card grid, create/edit modal, Stripe sync
- [ ] API Keys page (from reference)
- [ ] Connections page (from reference)
- [ ] Logs page (from reference)
- [ ] Webhooks page (from reference)
- [ ] Users page (from reference)
- [ ] Settings page with billing-specific tabs
- [ ] Git commit: `feat: invoices, refunds, plans, gateway pages, settings`

---

## 9. Day 8 — Super Admin Panel (React)

### Goal: Cross-tenant management dashboard for platform owner

### Morning (4 hours)

#### Task 8.1 — TenantService (Backend)
```
Files:
  - Core/ServiceContracts/ITenantService.cs
  - Core/Services/TenantService.cs

Methods:
  - CreateAsync(CreateTenantDto) → Onboard new tenant, generate all credentials
  - GetAsync(Guid id) → Tenant detail with stats
  - ListAsync(TenantFilterDto) → All tenants with revenue, subscription count
  - UpdateAsync(Guid id, UpdateTenantDto) → Update config
  - SuspendAsync(Guid id) → Deactivate tenant
  - ActivateAsync(Guid id) → Reactivate
  - RotateKeysAsync(Guid id) → Generate new API keys + webhook secret
  - GetSystemHealthAsync() → Platform-wide metrics

CreateTenantDto:
  - name, slug, contactEmail, plan (starter/standard/advanced)
  - stripeSecretKey, stripePublishableKey (or connect via OAuth later)

Credential generation on create:
  - PublicApiKey: pk_live_ + 32 random chars
  - SecretApiKey: sk_live_ + 32 random chars → hash with SHA256 for storage
  - WebhookSigningSecret: whsec_ + 32 random chars
  - JwtSigningSecret: jwt_ + 64 random chars
  - Return all credentials in response (shown ONCE)
```

#### Task 8.2 — Super Admin Controller
```
File: WebAPI/Controllers/v1/SuperAdminController.cs (or TenantController)

Auth: JWT + SuperAdmin role (+ 2FA check)

Endpoints:
  GET    /api/v1/superadmin/tenants                   → List all tenants
  POST   /api/v1/superadmin/tenants                   → Onboard new tenant
  GET    /api/v1/superadmin/tenants/{id}              → Tenant detail + stats
  PUT    /api/v1/superadmin/tenants/{id}              → Update config
  POST   /api/v1/superadmin/tenants/{id}/suspend      → Suspend
  POST   /api/v1/superadmin/tenants/{id}/activate     → Reactivate
  POST   /api/v1/superadmin/tenants/{id}/rotate-keys  → Rotate credentials
  GET    /api/v1/superadmin/analytics                  → Cross-tenant analytics
  GET    /api/v1/superadmin/system-health              → System health metrics
  GET    /api/v1/superadmin/audit-log                  → Platform-wide audit log
```

#### Task 8.3 — RevenueAnalyticsService (Backend)
```
Files:
  - Core/ServiceContracts/IRevenueAnalyticsService.cs
  - Core/Services/RevenueAnalyticsService.cs

Methods:
  - GetMrrAsync(Guid? tenantId) → MRR breakdown (new, expansion, contraction, churned)
  - GetArrAsync(Guid? tenantId) → Annual recurring revenue projection
  - GetChurnAsync(Guid? tenantId) → Monthly + annual churn rate
  - GetLtvAsync(Guid? tenantId) → Average customer LTV
  - GetSubscriptionMetricsAsync(Guid? tenantId) → Active, new, cancelled over time
  - GetRevenueOverTimeAsync(Guid? tenantId, period) → Revenue data points
  - GetCrossTenantAnalyticsAsync() → Platform-wide aggregates (super admin only)

MRR calculation:
  - Sum of all active subscription amounts (monthly equivalent)
  - New MRR: subscriptions created this month
  - Expansion MRR: upgrades this month
  - Contraction MRR: downgrades this month
  - Churned MRR: cancellations this month
  - Net New MRR: New + Expansion - Contraction - Churned
```

### Afternoon (4 hours)

#### Task 8.4 — Super Admin React Pages
```
Only SuperAdmin role sees these pages. Hidden from sidebar for other roles.

Tenant Management Page:
  src/pages/superadmin/TenantsPage.tsx (or integrated into existing pages with role check)
  
  DataTable: Tenant Name, Status, Plan, Revenue, Subscriptions, Health, Created
  Click → Tenant Detail Modal:
    - Tenant info, credentials (masked), Stripe status
    - Revenue stats for this tenant
    - Active subscriptions count
    - Actions: Edit, Suspend/Activate, Rotate Keys
  
  "Onboard Tenant" button → Multi-step form:
    Step 1: Company name, slug, email, plan tier
    Step 2: Stripe credentials (or skip for later)
    Step 3: Review → Create
    Step 4: Credentials display (show once, copy all button)

Cross-Tenant Analytics:
  Integrated into the main Analytics page when role=SuperAdmin:
  - Platform total revenue (across all tenants)
  - Top tenants by revenue (bar chart)
  - Platform MRR growth (line chart)
  - Total active subscriptions across all tenants
  - Platform churn rate

System Health:
  Integrated into Dashboard page when role=SuperAdmin:
  - Webhook delivery success rate (%)
  - Average API response time
  - Stripe API error count
  - Background job queue status
  - Database connection status
```

#### Task 8.5 — Audit Log Page
```
File: src/pages/audit/AuditLogPage.tsx

Layout:
  Filters:
    - Action type dropdown (refund.approved, key.rotated, tenant.created, settings.updated, etc.)
    - User dropdown (admin users)
    - Tenant dropdown (super admin only — cross-tenant)
    - Date range

  DataTable:
    - Columns: Timestamp, User, Action, Entity, Tenant (super admin), IP Address
    - Click row → Detail Modal:
      - Full action details
      - Before/After JSON diff (JsonViewer with two panels)
      - User agent, IP address

Data: GET /api/v1/audit-log (or /superadmin/audit-log for cross-tenant)
```

### Day 8 Checklist
- [ ] TenantService with full onboarding, credential generation
- [ ] Super admin API endpoints
- [ ] Tenant management UI (list, detail, onboard, suspend, rotate keys)
- [ ] Credential display (show once) with copy functionality
- [ ] Cross-tenant analytics (super admin only)
- [ ] System health dashboard (super admin only)
- [ ] Revenue analytics backend (MRR, ARR, churn, LTV calculations)
- [ ] Audit log page with filters and detail view
- [ ] Git commit: `feat: super admin panel, tenant management, cross-tenant analytics, audit log`

---

## 10. Day 9 — Revenue Analytics & Real-Time (React)

### Goal: Full analytics page with Chart.js, SignalR real-time updates, polish all pages

### Morning (4 hours)

#### Task 9.1 — Revenue Analytics Page
```
File: src/pages/analytics/RevenueAnalyticsPage.tsx

Layout:
  Date range selector (top right): 7d / 30d / 90d / 12m / custom

  Section 1: MRR / ARR
    - MetricCard: Current MRR (with % change from last month)
    - MetricCard: ARR Projection
    - Chart.js Line: MRR over 12 months (monthly data points)
    - Chart.js Stacked Bar: MRR Components
      - New MRR (green), Expansion (blue), Contraction (amber), Churned (red)

  Section 2: Revenue Overview
    - MetricCard: Total Revenue (period)
    - MetricCard: Net Revenue (after refunds)
    - MetricCard: Average Transaction Value
    - RevenueChart: Revenue over time (daily/weekly based on period)

  Section 3: Subscription Metrics
    - MetricCard: Active Subscriptions
    - MetricCard: Churn Rate (monthly)
    - MetricCard: Average Duration
    - MetricCard: Average LTV
    - Chart.js Line: New vs Cancelled over time

  Section 4: Payment Health
    - Chart.js Doughnut: Success / Failed ratio
    - MetricCard: Recovery Rate (dunning effectiveness)
    - Chart.js Horizontal Bar: Top failure reasons

  Section 5: Customer Insights (if time permits)
    - Top 10 customers by revenue (DataTable)
    - Chart.js Pie: Payment method distribution

Data: analyticsApi.getMrr(), getChurn(), getLtv(), getSubscriptionMetrics()
```

#### Task 9.2 — Chart.js Configuration
```
Chart library: chart.js + react-chartjs-2

RevenueChart component enhancements:
  - Line chart: gradient fill under line
  - Tooltip: formatted currency ($1,234.56)
  - Responsive container
  - Period toggle (7d/30d/90d)
  - Skeleton loading state

Additional chart components:
  - MrrStackedBar: Stacked bar chart for MRR components
  - SuccessRateDoughnut: Donut with center text (e.g., "94.2%")
  - FailureReasonsBar: Horizontal bar chart
  - SubscriptionTrendLine: Dual-line (new vs cancelled)

Shared chart config:
  - Consistent color palette (primary blue, success green, warning amber, danger red)
  - Currency formatting on axes
  - Responsive sizing
  - Animation on load
```

### Afternoon (4 hours)

#### Task 9.3 — SignalR Real-Time Integration
```
Pattern from reference project (DashboardHub):

Events pushed to React client:
  PaymentReceived     → Dashboard MetricCards refresh, PaymentsPage prepend row
  SubscriptionChanged → Dashboard stats refresh, SubscriptionsPage update row
  RefundRequested     → RefundsPage pending tab badge increment
  WebhookDelivered    → WebhooksPage deliveries tab update
  InvoiceGenerated    → InvoicesPage prepend row

React integration:
  - SignalR connection in DashboardLayout (established once on login)
  - Connection with JWT auth via query string (same as reference)
  - Auto-reconnect on disconnect (same as reference)
  - Event handlers update local state / trigger refetch

File: src/hooks/useSignalR.ts
  - Manages HubConnection lifecycle
  - Subscribes to events
  - Handles reconnection
  - Returns: { connection, isConnected }
```

#### Task 9.4 — Polish & Responsive Design
```
Ensure all 16 pages:
  - Load with skeleton loading states
  - Handle empty states (EmptyState component)
  - Handle errors (toast + inline message)
  - Mobile responsive (Bootstrap breakpoints)
  - Consistent spacing and typography
  - Sidebar collapse on mobile
  - All modals close on backdrop click and Escape key
  - All forms show validation errors inline
  - All destructive actions have ConfirmDialog
  - Currency values formatted consistently (Intl.NumberFormat)
  - Date values formatted consistently (date-fns format)
  - Pagination on all DataTables
```

#### Task 9.5 — AppRoutes Final
```
File: src/routes/AppRoutes.tsx

All routes lazy-loaded (React.lazy + Suspense):

  /login                    → LoginPage               (public)
  /                         → DashboardPage            (AllRoles)
  /billing/payments         → PaymentsPage             (AllRoles)
  /billing/subscriptions    → SubscriptionsPage         (AllRoles)
  /billing/customers        → CustomersPage            (AllRoles)
  /billing/invoices         → InvoicesPage             (AllRoles)
  /billing/refunds          → RefundsPage              (ManagerOrAbove)
  /billing/plans            → PlansPage                (AdminOrAbove)
  /analytics                → RevenueAnalyticsPage      (ManagerOrAbove)
  /gateway/api-keys         → ApiKeysPage              (ManagerOrAbove)
  /gateway/connections      → ConnectionsPage          (AdminOrAbove)
  /gateway/logs             → LogsPage                 (AllRoles)
  /gateway/webhooks         → WebhooksPage             (ManagerOrAbove)
  /users                    → UsersPage                (AdminOrAbove)
  /settings                 → SettingsPage             (AdminOrAbove)
  /audit-log                → AuditLogPage             (AdminOrAbove)
  *                         → 404 NotFound page
```

### Day 9 Checklist
- [ ] Revenue Analytics page with 5 chart sections
- [ ] MRR/ARR line chart with 12-month history
- [ ] MRR components stacked bar chart
- [ ] Subscription trend chart (new vs cancelled)
- [ ] Payment health doughnut + failure reasons bar
- [ ] SignalR real-time working (payment → dashboard update)
- [ ] All 16 pages polished (loading, empty, error states)
- [ ] All 16 routes lazy-loaded
- [ ] Mobile responsive layout
- [ ] Git commit: `feat: revenue analytics, Chart.js visualizations, SignalR real-time, polish`

---

## 11. Day 10 — Testing, Docs & Deployment

### Goal: Production-ready with tests, documentation, Docker, clean delivery

### Morning (4 hours)

#### Task 10.1 — Unit Tests (Backend)
```
Priority test targets:

Tests/Services/HmacAuthServiceTests.cs:
  - TestValidSignature → returns true
  - TestInvalidSignature → returns false
  - TestExpiredTimestamp → returns false
  - TestReplayAttack → returns false (same nonce)

Tests/Services/StripePaymentGatewayTests.cs:
  - TestCreateCheckoutSession → correct Stripe params
  - TestCreatePaymentIntent → correct amount + currency
  - TestTenantIsolation → tenant A cannot access tenant B payments

Tests/Services/SubscriptionServiceTests.cs:
  - TestCreateSubscription → Stripe sub created
  - TestUpgrade → proration calculated
  - TestCancel → status updated
  - TestPause → pause_collection set

Tests/Services/RefundServiceTests.cs:
  - TestAutoApproveUnderThreshold → status=processing
  - TestManualApproveOverThreshold → status=pending
  - TestRejectRefund → status=rejected

Tests/Services/RevenueAnalyticsServiceTests.cs:
  - TestMrrCalculation → correct sum
  - TestChurnCalculation → correct rate
  - TestLtvCalculation → correct average

Tests/Services/WebhookDispatchServiceTests.cs:
  - TestSignatureGeneration → valid HMAC
  - TestRetrySchedule → correct backoff times
  - TestDeadLetter → after max retries
```

#### Task 10.2 — Integration Tests
```
Tests/Controllers/PaymentFlowTests.cs:
  - Full flow: create customer → checkout → Stripe webhook → transaction recorded → outbound webhook queued

Tests/Controllers/SubscriptionFlowTests.cs:
  - Create plan → subscribe → upgrade → cancel

Tests/Controllers/AuthFlowTests.cs:
  - Register → login → access protected → refresh → access again

Tests/Controllers/TenantIsolationTests.cs:
  - Tenant A creates payment → Tenant B cannot see it
  - Verify all 16 tables have tenant isolation
```

#### Task 10.3 — Stripe Test Cards
```
Document for testing:
  4242 4242 4242 4242 → Succeeds
  4000 0000 0000 0002 → Declined
  4000 0000 0000 3220 → 3D Secure required
  4000 0000 0000 9995 → Insufficient funds
  4000 0000 0000 0341 → Attach fails
```

### Afternoon (4 hours)

#### Task 10.4 — Documentation
```
docs/
├── README.md                    → Project overview, tech stack, quick start
├── FEATURES.md                  → Complete feature list (all billing features)
├── ARCHITECTURE.md              → System design, multi-tenant, auth flows
├── SETUP.md                     → Local dev setup (backend + frontend + Stripe CLI)
├── DEPLOYMENT.md                → Docker, Azure, AWS deployment
├── API-DOCS.md                  → Full API reference (70+ endpoints with examples)
├── WEBHOOK-EVENTS.md            → 14 inbound + 11 outbound events with payloads
├── INTEGRATION-GUIDE.md         → How client apps integrate (HMAC signing, webhook handling)
├── DOCS-INDEX.md                → Documentation map
└── Postman/
    ├── StripeBilling.postman_collection.json
    └── StripeBilling.postman_environment.json
```

#### Task 10.5 — Docker Setup
```
docker/
├── Dockerfile.api               → Multi-stage build for .NET 9
├── Dockerfile.frontend          → Node build + nginx serve
└── docker-compose.yml           → API + SQL Server + Frontend + Redis (optional cache)

docker-compose.yml services:
  api:
    build: ../backend
    ports: 5000:8080
    depends_on: db
    environment: ConnectionStrings__DefaultConnection, Jwt__Secret, etc.
  
  frontend:
    build: ../frontend
    ports: 3000:80
    depends_on: api
  
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    ports: 1433:1433
    volumes: mssql-data
```

#### Task 10.6 — Final Polish
```
  - Swagger: all endpoints with descriptions, example payloads, auth requirements
  - Postman collection: all 70+ endpoints organized by group
  - appsettings.example.json: template with placeholder values
  - .gitignore: bin, obj, node_modules, .env, appsettings.Development.json
  - Environment files: .env.example for frontend
  - README with: overview, screenshots, quick start, deployment
  - Clean all TODO comments
  - Verify all error responses are user-friendly GatewayResponseWrapper
  - Security audit: no secrets in code, HTTPS enforced, CORS configured
```

### Day 10 Checklist
- [ ] Unit tests passing (HMAC, payments, subscriptions, refunds, analytics)
- [ ] Integration tests passing (full flows, tenant isolation)
- [ ] All documentation files created
- [ ] Docker Compose running (api + db + frontend)
- [ ] Postman collection with all endpoints
- [ ] Swagger fully documented
- [ ] README with project overview
- [ ] .gitignore and .env.example files
- [ ] Final security audit passed
- [ ] Git commit: `feat: tests, documentation, Docker, production ready`
- [ ] Git tag: `v1.0.0`

---

## 12. Post-Build Checklist

### Before Listing on Upwork
- [ ] Full demo: register → create tenant → connect Stripe → make payment → see webhook → see dashboard
- [ ] Record 3-5 minute demo video (Loom or similar)
- [ ] Screenshots: Dashboard, Payments, Subscriptions, Analytics, Swagger
- [ ] Prepare Upwork catalog with 3 pricing tiers ($400 / $800 / $1,200)
- [ ] Test with Stripe test keys end-to-end
- [ ] Security: HTTPS, encrypted credentials, hashed keys, no secrets in logs/responses
- [ ] Performance: API response < 500ms, dashboard loads < 2s

### After Each Client Delivery
- [ ] Configure client's Stripe credentials
- [ ] Create client's tenant via super admin
- [ ] Deliver API keys + webhook secret securely
- [ ] Run integration tests against client's Stripe account (test mode)
- [ ] Send Postman collection + integration guide
- [ ] 15-30 min walkthrough call (optional)
- [ ] Start 30-day support window

---

## 13. Risk Mitigation

| Risk | Mitigation |
|------|-----------|
| Stripe API rate limits | Implement per-tenant rate limiting, cache plan data, queue bulk operations |
| Webhook delivery failures | Retry with exponential backoff (6 attempts), dead letter queue, manual retry UI |
| PCI compliance | Never touch card data. Stripe Elements / Checkout handles all card input. |
| Credential security | AES-256 encryption for Stripe keys, SHA256 hash for API keys, never log/return plain |
| Tenant data leak | Global EF Core query filter on TenantId, integration tests verify isolation |
| HMAC replay attacks | 5-minute timestamp window, idempotency key cache (24h TTL) |
| Subscription edge cases | Handle all Stripe sub states, test with Stripe test clocks for time-based scenarios |
| Invoice generation | Use Stripe-hosted invoices (PCI compliant, handles tax/multi-currency) |
| Concurrent payments | Idempotency keys prevent duplicate charges on retry |
| Dashboard performance | Paginate all lists, cache analytics, index all query patterns |
| Scope creep | Clear tier definitions, documented features per tier, fixed revision counts |

---

## 14. Folder & File Structure

```
03-Stripe-Billing-Service/
├── Stripe_Billing_Development_Plan.md               ← This file
├── docs/
│   ├── README.md
│   ├── FEATURES.md
│   ├── ARCHITECTURE.md
│   ├── SETUP.md
│   ├── DEPLOYMENT.md
│   ├── API-DOCS.md
│   ├── WEBHOOK-EVENTS.md
│   ├── INTEGRATION-GUIDE.md
│   ├── DOCS-INDEX.md
│   └── Postman/
│       ├── StripeBilling.postman_collection.json
│       └── StripeBilling.postman_environment.json
├── backend/
│   ├── StripeBilling.sln
│   ├── Core/
│   │   ├── StripeBilling.Core.csproj
│   │   ├── Constants/
│   │   │   ├── Roles.cs
│   │   │   ├── ErrorCodes.cs
│   │   │   ├── WebhookEvents.cs
│   │   │   └── StripeConstants.cs
│   │   ├── ContextProviders/
│   │   │   ├── ITenantContextProvider.cs
│   │   │   └── HttpTenantContextProvider.cs
│   │   ├── Dtos/
│   │   │   ├── Requests/
│   │   │   │   ├── Auth/ (LoginDto, RegisterDto, RefreshDto)
│   │   │   │   ├── Payments/ (CreateCheckoutDto, CreateIntentDto)
│   │   │   │   ├── Subscriptions/ (CreateSubscriptionDto, UpdateSubscriptionDto, CancelDto)
│   │   │   │   ├── Customers/ (CreateCustomerDto, UpdateCustomerDto)
│   │   │   │   ├── Plans/ (CreatePlanDto, UpdatePlanDto)
│   │   │   │   ├── Refunds/ (CreateRefundDto)
│   │   │   │   ├── Tenants/ (CreateTenantDto, UpdateTenantDto)
│   │   │   │   ├── Webhooks/ (CreateWebhookSubDto)
│   │   │   │   └── Settings/ (UpdateSettingsDto, UpdateBrandingDto)
│   │   │   └── Responses/
│   │   │       ├── PaymentResponseDto.cs
│   │   │       ├── SubscriptionResponseDto.cs
│   │   │       ├── CustomerResponseDto.cs
│   │   │       ├── InvoiceResponseDto.cs
│   │   │       ├── RefundResponseDto.cs
│   │   │       ├── PlanResponseDto.cs
│   │   │       ├── AnalyticsResponseDtos.cs (MrrDto, ChurnDto, LtvDto)
│   │   │       ├── TenantResponseDto.cs
│   │   │       ├── DashboardResponseDtos.cs
│   │   │       └── ... (shared: LoginResponseDto, UserResponseDto, etc.)
│   │   ├── ErrorHandling/
│   │   │   ├── ExceptionHandler.cs
│   │   │   └── Exceptions/
│   │   │       ├── NotFoundException.cs
│   │   │       ├── UnauthorizedException.cs
│   │   │       ├── ForbiddenException.cs
│   │   │       ├── ConflictException.cs
│   │   │       ├── ValidationException.cs
│   │   │       └── StripeOperationException.cs
│   │   ├── Infrastructure/
│   │   │   ├── BillingDbContext.cs
│   │   │   ├── Tenant.cs
│   │   │   ├── User.cs
│   │   │   ├── RefreshToken.cs
│   │   │   ├── ApiKey.cs
│   │   │   ├── Customer.cs
│   │   │   ├── SubscriptionPlan.cs
│   │   │   ├── Subscription.cs
│   │   │   ├── PaymentTransaction.cs
│   │   │   ├── Invoice.cs
│   │   │   ├── Refund.cs
│   │   │   ├── WebhookSubscription.cs
│   │   │   ├── WebhookDelivery.cs
│   │   │   ├── WebhookEventInbound.cs
│   │   │   ├── ApiCallLog.cs
│   │   │   └── AuditLog.cs
│   │   ├── Mappers/
│   │   │   ├── PaymentMapper.cs
│   │   │   ├── SubscriptionMapper.cs
│   │   │   ├── CustomerMapper.cs
│   │   │   ├── InvoiceMapper.cs
│   │   │   ├── RefundMapper.cs
│   │   │   ├── PlanMapper.cs
│   │   │   └── TenantMapper.cs
│   │   ├── Repositories/
│   │   │   ├── BaseRepository.cs
│   │   │   ├── TenantRepository.cs
│   │   │   ├── UserRepository.cs
│   │   │   ├── CustomerRepository.cs
│   │   │   ├── SubscriptionRepository.cs
│   │   │   ├── SubscriptionPlanRepository.cs
│   │   │   ├── PaymentTransactionRepository.cs
│   │   │   ├── InvoiceRepository.cs
│   │   │   ├── RefundRepository.cs
│   │   │   ├── WebhookSubscriptionRepository.cs
│   │   │   ├── WebhookDeliveryRepository.cs
│   │   │   ├── WebhookEventInboundRepository.cs
│   │   │   ├── ApiKeyRepository.cs
│   │   │   ├── ApiCallLogRepository.cs
│   │   │   └── AuditLogRepository.cs
│   │   ├── RepositoryContracts/
│   │   │   ├── ITenantRepository.cs
│   │   │   ├── IUserRepository.cs
│   │   │   ├── ICustomerRepository.cs
│   │   │   ├── ... (one per repository)
│   │   │   └── IAuditLogRepository.cs
│   │   ├── ServiceContracts/
│   │   │   ├── IAuthService.cs
│   │   │   ├── IJwtTokenService.cs
│   │   │   ├── IEncryptionService.cs
│   │   │   ├── IUserService.cs
│   │   │   ├── IApiKeyService.cs
│   │   │   ├── IPaymentGateway.cs
│   │   │   ├── ISubscriptionService.cs
│   │   │   ├── ISubscriptionPlanService.cs
│   │   │   ├── ICustomerService.cs
│   │   │   ├── IInvoiceService.cs
│   │   │   ├── IRefundService.cs
│   │   │   ├── IStripeWebhookHandler.cs
│   │   │   ├── IWebhookDispatchService.cs
│   │   │   ├── IWebhookSubscriptionService.cs
│   │   │   ├── IDashboardService.cs
│   │   │   ├── IRevenueAnalyticsService.cs
│   │   │   ├── IServiceConnectionService.cs
│   │   │   ├── ISettingsService.cs
│   │   │   ├── IHmacAuthService.cs
│   │   │   ├── IAuditService.cs
│   │   │   └── ITenantService.cs
│   │   ├── Services/
│   │   │   ├── BaseService.cs
│   │   │   ├── AuthService.cs
│   │   │   ├── JwtTokenService.cs
│   │   │   ├── EncryptionService.cs
│   │   │   ├── UserService.cs
│   │   │   ├── ApiKeyService.cs
│   │   │   ├── StripePaymentGateway.cs
│   │   │   ├── SubscriptionService.cs
│   │   │   ├── SubscriptionPlanService.cs
│   │   │   ├── CustomerService.cs
│   │   │   ├── InvoiceService.cs
│   │   │   ├── RefundService.cs
│   │   │   ├── StripeWebhookHandler.cs
│   │   │   ├── WebhookDispatchService.cs
│   │   │   ├── WebhookSubscriptionService.cs
│   │   │   ├── DashboardService.cs
│   │   │   ├── RevenueAnalyticsService.cs
│   │   │   ├── ServiceConnectionService.cs
│   │   │   ├── SettingsService.cs
│   │   │   ├── HmacAuthService.cs
│   │   │   ├── AuditService.cs
│   │   │   └── TenantService.cs
│   │   ├── Utils/
│   │   │   ├── GatewayResponseWrapper.cs
│   │   │   └── DiRegistrationExtensions.cs
│   │   └── Validators/
│   │       ├── CreateCheckoutValidator.cs
│   │       ├── CreateIntentValidator.cs
│   │       ├── CreateSubscriptionValidator.cs
│   │       ├── CreateCustomerValidator.cs
│   │       ├── CreatePlanValidator.cs
│   │       ├── CreateRefundValidator.cs
│   │       ├── CreateTenantValidator.cs
│   │       ├── LoginValidator.cs
│   │       └── ... (one per request DTO)
│   ├── WebAPI/
│   │   ├── StripeBilling.API.csproj
│   │   ├── Controllers/v1/
│   │   │   ├── GatewayControllerBase.cs
│   │   │   ├── SetupController.cs
│   │   │   ├── AuthController.cs
│   │   │   ├── UserController.cs
│   │   │   ├── ApiKeyController.cs
│   │   │   ├── ConnectionController.cs
│   │   │   ├── PaymentController.cs
│   │   │   ├── SubscriptionController.cs
│   │   │   ├── CustomerController.cs
│   │   │   ├── PlanController.cs
│   │   │   ├── InvoiceController.cs
│   │   │   ├── RefundController.cs
│   │   │   ├── WebhookController.cs
│   │   │   ├── WebhookInboundController.cs
│   │   │   ├── DashboardController.cs
│   │   │   ├── AnalyticsController.cs
│   │   │   ├── PortalController.cs
│   │   │   ├── SettingsController.cs
│   │   │   ├── AuditController.cs
│   │   │   ├── LogController.cs
│   │   │   └── HealthController.cs
│   │   ├── Middleware/
│   │   │   ├── TenantMiddleware.cs
│   │   │   ├── ApiKeyAuthMiddleware.cs
│   │   │   ├── HmacAuthMiddleware.cs
│   │   │   ├── RateLimitMiddleware.cs
│   │   │   └── RequestLoggingMiddleware.cs
│   │   ├── BackgroundServices/
│   │   │   ├── WebhookDispatcherService.cs
│   │   │   └── WebhookRetryService.cs
│   │   ├── Hubs/
│   │   │   └── DashboardHub.cs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── appsettings.Development.json
│   └── Tests/
│       ├── StripeBilling.Tests.csproj
│       ├── Services/
│       │   ├── HmacAuthServiceTests.cs
│       │   ├── StripePaymentGatewayTests.cs
│       │   ├── SubscriptionServiceTests.cs
│       │   ├── RefundServiceTests.cs
│       │   ├── RevenueAnalyticsServiceTests.cs
│       │   └── WebhookDispatchServiceTests.cs
│       └── Controllers/
│           ├── PaymentFlowTests.cs
│           ├── SubscriptionFlowTests.cs
│           ├── AuthFlowTests.cs
│           └── TenantIsolationTests.cs
├── frontend/
│   ├── src/
│   │   ├── api/
│   │   │   ├── api-client.ts
│   │   │   ├── interceptors.ts
│   │   │   ├── apiWrapper.ts
│   │   │   ├── index.ts
│   │   │   ├── authApi.ts
│   │   │   ├── paymentApi.ts
│   │   │   ├── subscriptionApi.ts
│   │   │   ├── customerApi.ts
│   │   │   ├── invoiceApi.ts
│   │   │   ├── refundApi.ts
│   │   │   ├── planApi.ts
│   │   │   ├── dashboardApi.ts
│   │   │   ├── analyticsApi.ts
│   │   │   ├── webhookApi.ts
│   │   │   ├── connectionApi.ts
│   │   │   ├── apiKeyApi.ts
│   │   │   ├── userApi.ts
│   │   │   └── settingsApi.ts
│   │   ├── components/
│   │   │   ├── common/
│   │   │   │   ├── DataTable.tsx
│   │   │   │   ├── MetricCard.tsx
│   │   │   │   ├── StatusBadge.tsx
│   │   │   │   ├── SearchInput.tsx
│   │   │   │   ├── LoadingSkeleton.tsx
│   │   │   │   ├── JsonViewer.tsx
│   │   │   │   ├── CodeSnippet.tsx
│   │   │   │   ├── EmptyState.tsx
│   │   │   │   ├── ConfirmDialog.tsx
│   │   │   │   ├── RevenueChart.tsx
│   │   │   │   ├── SubscriptionBadge.tsx
│   │   │   │   ├── RefundBadge.tsx
│   │   │   │   ├── InvoiceViewer.tsx
│   │   │   │   ├── PlanCard.tsx
│   │   │   │   └── WebhookStatusBadge.tsx
│   │   │   └── layout/
│   │   │       ├── Sidebar.tsx
│   │   │       └── Topbar.tsx
│   │   ├── contexts/
│   │   │   ├── AuthContext.tsx
│   │   │   ├── ToastContext.tsx
│   │   │   └── SidebarContext.tsx
│   │   ├── hooks/
│   │   │   ├── useAuth.ts
│   │   │   ├── useToast.ts
│   │   │   ├── useSidebar.ts
│   │   │   ├── useDebounce.ts
│   │   │   └── useSignalR.ts
│   │   ├── layouts/
│   │   │   ├── AuthLayout.tsx
│   │   │   └── DashboardLayout.tsx
│   │   ├── pages/
│   │   │   ├── auth/LoginPage.tsx
│   │   │   ├── dashboard/DashboardPage.tsx
│   │   │   ├── billing/
│   │   │   │   ├── PaymentsPage.tsx
│   │   │   │   ├── SubscriptionsPage.tsx
│   │   │   │   ├── CustomersPage.tsx
│   │   │   │   ├── InvoicesPage.tsx
│   │   │   │   ├── RefundsPage.tsx
│   │   │   │   └── PlansPage.tsx
│   │   │   ├── analytics/RevenueAnalyticsPage.tsx
│   │   │   ├── gateway/
│   │   │   │   ├── ApiKeysPage.tsx
│   │   │   │   ├── ConnectionsPage.tsx
│   │   │   │   ├── LogsPage.tsx
│   │   │   │   └── WebhooksPage.tsx
│   │   │   ├── users/UsersPage.tsx
│   │   │   ├── settings/SettingsPage.tsx
│   │   │   └── audit/AuditLogPage.tsx
│   │   ├── routes/
│   │   │   ├── AppRoutes.tsx
│   │   │   └── ProtectedRoute.tsx
│   │   ├── types/
│   │   │   ├── common.ts
│   │   │   ├── auth.ts
│   │   │   ├── payment.ts
│   │   │   ├── subscription.ts
│   │   │   ├── customer.ts
│   │   │   ├── invoice.ts
│   │   │   ├── refund.ts
│   │   │   ├── plan.ts
│   │   │   ├── analytics.ts
│   │   │   ├── apiKey.ts
│   │   │   ├── connection.ts
│   │   │   ├── webhook.ts
│   │   │   ├── log.ts
│   │   │   └── dashboard.ts
│   │   ├── utils/
│   │   │   ├── formatters.ts
│   │   │   └── jwt.ts
│   │   ├── App.tsx
│   │   └── main.tsx
│   ├── index.html
│   ├── package.json
│   ├── tsconfig.json
│   ├── vite.config.ts
│   └── .env.example
└── docker/
    ├── Dockerfile.api
    ├── Dockerfile.frontend
    └── docker-compose.yml
```

---

## 15. Upwork Listing Checklist

- [ ] Title: "Stripe Payment Integration & Subscription Billing Service"
- [ ] Description: problem/solution, multi-tenant, 3 dashboards
- [ ] 3 pricing tiers: Starter $400 / Standard $800 / Advanced $1,200
- [ ] Screenshots: Dashboard, Analytics, Payments, Subscriptions, Swagger
- [ ] Demo video: 3-5 min walkthrough
- [ ] Tags: Stripe, Payment Integration, Subscription Billing, ASP.NET, React, SaaS
- [ ] Portfolio piece with case study
- [ ] FAQ: "How is this different from Chargebee/Recurly?"

---

*This document serves as the complete development roadmap. Each day has clear goals, tasks, and a checklist. Track progress by checking off items as you complete them.*
