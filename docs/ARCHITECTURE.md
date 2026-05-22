# Architecture — Stripe Billing Service

Technical architecture and system design documentation.

---

## System Overview

```
┌─────────────────┐    API Key + HMAC     ┌──────────────────────┐    Stripe.NET     ┌────────────┐
│   Client App    │ ──────────────────────→│  Stripe Billing      │ ────────────────→ │   Stripe   │
│   (Any Stack)   │                        │  Service (.NET 9)    │ ←──────────────── │   API      │
│                 │ ←──────────────────────│                      │   Stripe Webhooks │            │
│                 │   Signed Webhook       │  ┌───────────────┐   │                   └────────────┘
└─────────────────┘                        │  │  SQL Server   │   │
                                           │  │  (Multi-Tenant)│  │
┌─────────────────┐    JWT Passthrough     │  └───────────────┘   │
│  End User       │ ──────────────────────→│                      │
│  (Browser)      │   Customer Portal      │  ┌───────────────┐   │
└─────────────────┘                        │  │  SignalR Hub   │   │
                                           │  │  (Real-Time)   │  │
┌─────────────────┐    JWT Session         │  └───────────────┘   │
│  Admin          │ ──────────────────────→│                      │
│  (Dashboard)    │   React Dashboard      └──────────────────────┘
└─────────────────┘
```

---

## Request → Process → Callback Flow

```
1. Client App ──POST /api/v1/payments/checkout──→ Billing Service
   Headers: X-Api-Key, X-Signature (HMAC), X-Timestamp, X-Idempotency-Key

2. Billing Service validates: API key → HMAC signature → timestamp → idempotency

3. Billing Service ──Creates Checkout Session──→ Stripe API
   Uses tenant's encrypted Stripe credentials (decrypted in memory)

4. Returns checkout URL to Client App

5. Customer pays on Stripe's hosted page (PCI compliant)

6. Stripe ──webhook──→ Billing Service (POST /api/v1/webhooks/stripe)
   Verified with Stripe signing secret per tenant

7. Billing Service processes: records transaction, updates subscription, generates invoice

8. Billing Service ──signed webhook──→ Client App (POST callback URL)
   Headers: X-Webhook-Signature, X-Webhook-Timestamp, X-Webhook-ID

9. React dashboards update in real-time via SignalR
```

---

## Clean Architecture

### Project Structure
```
backend/
├── Core/                          # Domain + Application layers
│   ├── Constants/                 # Role definitions, error codes, event types
│   ├── ContextProviders/          # Tenant context (from HTTP headers / JWT)
│   ├── Dtos/                      # Request + Response DTOs
│   ├── ErrorHandling/             # Global exception handler
│   ├── Infrastructure/            # EF Core entities + DbContext
│   ├── Mappers/                   # Entity ↔ DTO mapping
│   ├── Repositories/              # Data access (BaseRepository pattern)
│   ├── RepositoryContracts/       # Repository interfaces
│   ├── ServiceContracts/          # Service interfaces (21)
│   ├── Services/                  # Business logic implementations (23)
│   ├── Utils/                     # GatewayResponseWrapper, DI extensions
│   └── Validators/                # FluentValidation validators
├── WebAPI/                        # Presentation layer
│   ├── Controllers/v1/            # 20 controllers + GatewayControllerBase
│   ├── Middleware/                 # 5 middleware (pipeline)
│   ├── BackgroundServices/        # Webhook dispatcher + retry
│   ├── Hubs/                      # SignalR DashboardHub
│   └── Program.cs                 # DI, middleware, configuration
└── Tests/                         # xUnit + Moq + FluentAssertions
```

---

## Multi-Tenant Data Isolation

### Row-Level Isolation
Every tenant-scoped table has a `TenantId` column. EF Core global query filter ensures no cross-tenant data access:

```csharp
// In BillingDbContext.OnModelCreating:
modelBuilder.Entity<Customer>().HasQueryFilter(e => e.TenantId == _currentTenantId);
modelBuilder.Entity<Subscription>().HasQueryFilter(e => e.TenantId == _currentTenantId);
// ... applied to all 13 tenant-scoped tables
```

### Tenant Context Flow
```
Request arrives
  → ApiKeyAuthMiddleware: X-Api-Key → lookup tenant → set HttpContext.Items["TenantId"]
  → HmacAuthMiddleware: verify signature with tenant's secret
  → TenantMiddleware: read TenantId from Items or JWT claims
  → HttpTenantContextProvider: provides TenantId to BaseService/BaseRepository
  → EF Core query filter: automatically scopes all queries
```

---

## Middleware Pipeline

```
Request
  │
  ▼
┌─────────────────────────┐
│  ExceptionHandler       │  Global error → GatewayResponseWrapper
├─────────────────────────┤
│  CORS                   │  AllowCredentials for SignalR
├─────────────────────────┤
│  RequestLogging         │  Log every request to ApiCallLogs
├─────────────────────────┤
│  RateLimit              │  Per-API-key rate limiting
├─────────────────────────┤
│  ApiKeyAuth             │  X-Api-Key → SHA256 hash → lookup → set context
├─────────────────────────┤
│  HmacAuth (NEW)         │  X-Signature + X-Timestamp → HMAC-SHA256 verify
├─────────────────────────┤
│  TenantMiddleware       │  Set TenantId from header/JWT/API key
├─────────────────────────┤
│  JWT Authentication     │  JwtBearer + SignalR query string
├─────────────────────────┤
│  Authorization          │  Role policies: SuperAdmin, Admin, Manager, Viewer
└─────────────────────────┘
  │
  ▼
Controller → Service → Repository → Database
```

---

## Authentication Boundaries

### Inbound API (Client App → Service)
```
Headers:
  X-Api-Key: pk_live_xxxxxxxx         → Identifies tenant
  X-Signature: HMAC-SHA256(body|ts)   → Proves authenticity
  X-Timestamp: 1709000000             → 5-min window (anti-replay)
  X-Idempotency-Key: uuid            → Prevents duplicate charges

Verification:
  1. X-Api-Key → lookup tenant → if not found → 401
  2. Tenant active? → if suspended → 403
  3. X-Timestamp → if > 5 min drift → 401
  4. HMAC-SHA256(body + "|" + timestamp, secretKey) → compare → if mismatch → 401
  5. X-Idempotency-Key → if seen → return cached response
  6. Set context: TenantId, ApiKeyId, Permissions
```

### Outbound Webhook (Service → Client App)
```
Headers:
  X-Webhook-Signature: HMAC-SHA256(payload|ts, webhookSecret)
  X-Webhook-Timestamp: 1709000000
  X-Webhook-ID: evt_xxx
  X-Webhook-Retry: 0

Client verification:
  1. Read X-Webhook-Timestamp → reject if > 5 min
  2. Compute expected = HMAC-SHA256(payload + "|" + timestamp, webhookSecret)
  3. Compare with X-Webhook-Signature (constant-time)
```

### JWT Passthrough (End User Portal)
```
Client app generates short-lived JWT (5-15 min) containing:
  tenant_id, customer_reference_id, email, name

Portal validates JWT with tenant's JwtSigningSecret
Shows only that customer's data (row-level isolation)
```

---

## Service Layer (21 Interfaces)

### From Reference (Identical)
| Interface | Responsibility |
|-----------|---------------|
| IAuthService | Login, register, refresh, password reset |
| IJwtTokenService | Generate/validate JWT tokens |
| IEncryptionService | AES encrypt/decrypt Stripe keys |
| IUserService | CRUD admin users, invite, roles |
| IApiKeyService | CRUD API keys, generate, revoke |
| IWebhookDispatchService | Enqueue, build, sign webhooks |
| IWebhookSubscriptionService | CRUD webhook registrations |
| IDashboardService | Stats, charts, activity feed |
| IServiceConnectionService | CRUD Stripe connections |
| ISettingsService | Get/update tenant settings |

### New (Billing-Specific)
| Interface | Responsibility |
|-----------|---------------|
| IPaymentGateway | Checkout, Intent, Confirm, List, Analytics |
| ISubscriptionService | Create, Update, Cancel, Pause, Resume, Preview |
| ISubscriptionPlanService | CRUD plans, Stripe sync, toggle active |
| ICustomerService | CRUD customers, Stripe sync, external ref mapping |
| IInvoiceService | List, Get, PDF, Void, Send, Sync |
| IRefundService | Create, Approve, Reject, List |
| IStripeWebhookHandler | Process inbound Stripe events |
| IRevenueAnalyticsService | MRR, ARR, Churn, LTV, Cohort |
| IHmacAuthService | Validate HMAC, generate secrets |
| IAuditService | Log actions, query audit trail |
| ITenantService | CRUD tenants, onboarding, credentials |

---

## Controller Layer (20 Controllers)

| Controller | Auth | Endpoints |
|-----------|------|-----------|
| SetupController | None/SuperAdmin | initialize, create-tenant |
| AuthController | None/JWT | login, register, refresh, me |
| UserController | AdminOrAbove | CRUD users, invite |
| ApiKeyController | ManagerOrAbove | CRUD API keys |
| ConnectionController | AdminOrAbove | CRUD connections, test |
| PaymentController | API Key+HMAC | checkout, intent, list, analytics |
| SubscriptionController | API Key+HMAC | create, update, cancel, pause, resume, preview |
| CustomerController | API Key+HMAC | create, get, update, list, portal-session |
| PlanController | AdminOrAbove | CRUD plans, sync |
| InvoiceController | API Key/JWT | list, get, pdf, void, send |
| RefundController | API Key/JWT | create, list, approve, reject |
| WebhookController | ManagerOrAbove | subscriptions CRUD, deliveries, retry |
| WebhookInboundController | Stripe Sig | POST /webhooks/stripe |
| DashboardController | AllRoles | stats, revenue-chart, activity |
| AnalyticsController | ManagerOrAbove | mrr, churn, ltv, metrics |
| PortalController | JWT Passthrough | me, transactions, subscriptions, invoices |
| SettingsController | AdminOrAbove | get/update settings, branding |
| AuditController | AdminOrAbove | audit-log |
| LogController | AllRoles | logs, log-stats |
| HealthController | None | GET /health |

---

## Database ERD Summary

```
Tenants (1) ──→ (N) Users
Tenants (1) ──→ (N) ApiKeys
Tenants (1) ──→ (N) Customers
Tenants (1) ──→ (N) SubscriptionPlans
Customers (1) ──→ (N) Subscriptions
Customers (1) ──→ (N) PaymentTransactions
Customers (1) ──→ (N) Invoices
SubscriptionPlans (1) ──→ (N) Subscriptions
PaymentTransactions (1) ──→ (N) Refunds
Tenants (1) ──→ (N) WebhookSubscriptions
WebhookSubscriptions (1) ──→ (N) WebhookDeliveries
Tenants (1) ──→ (N) WebhookEventsInbound
Tenants (1) ──→ (N) ApiCallLogs
Tenants (1) ──→ (N) AuditLog
```

---

## Frontend Architecture

### React 18 + TypeScript + Vite
```
src/
├── api/           # 18 API modules (axios + interceptors)
├── components/    # 15 reusable components
├── contexts/      # AuthContext, ToastContext, SidebarContext
├── hooks/         # useAuth, useToast, useSidebar, useDebounce, useSignalR
├── layouts/       # AuthLayout, DashboardLayout
├── pages/         # 16 lazy-loaded pages
├── routes/        # AppRoutes + ProtectedRoute
├── types/         # 14 TypeScript type files
└── utils/         # Formatters, JWT helpers
```

### State Management
- React Context API (no Redux needed)
- AuthContext: user state, login/logout, token management
- ToastContext: notification system (react-toastify)
- SidebarContext: collapse state
- SignalR: real-time dashboard updates

### API Layer Pattern
```typescript
// api-client.ts: Axios instance with base URL
// interceptors.ts: JWT attach + 401 refresh with queue
// apiWrapper.ts: Typed response wrapper

// Usage:
const { data } = await apiGet<PaginatedResponse<PaymentTransaction>>('/payments', { params });
```

---

## Background Services

### WebhookDispatcherService
- Polls WebhookDeliveries table every 5 seconds for Pending status
- Sends HTTP POST with signed payload
- Updates status to Delivered or schedules retry

### WebhookRetryService
- Polls for Status=Retrying where NextRetryAt <= now
- Exponential backoff: 1m, 5m, 30m, 2h, 8h, 24h
- After 6 retries: marks as PermanentlyFailed (dead letter)

---

## Reference Project Patterns

All patterns from **02-API-Gateway-Microservice** are reused:

| Pattern | Status |
|---------|--------|
| GatewayResponseWrapper<T> | Identical |
| GatewayControllerBase | Identical |
| BaseService + ITenantContextProvider | Identical |
| BaseRepository | Identical |
| DiRegistrationExtensions | Identical |
| ExceptionHandler | Identical |
| All Middleware (except HMAC) | Identical |
| JwtTokenService | Identical |
| EncryptionService | Identical |
| WebhookDispatchService | Identical |
| DashboardHub (SignalR) | Identical |
| React api-client + interceptors | Identical |
| React contexts/hooks/layouts | Identical |
| React common components | Identical |
| ProtectedRoute | Identical |

**New additions:** HmacAuthMiddleware, all billing services, all billing entities, 6 billing pages, analytics page, audit page, 6 new components
