# Architecture Documentation

## System Overview

The Stripe Billing Service is built on a three-tier architecture with a multi-tenant design pattern. The system ensures complete data isolation between tenants while providing a unified billing platform.

```
┌─────────────────────────────────────────────────────────────────────┐
│                          CLIENT APPLICATIONS                         │
│         (Web, Mobile, Third-party Integrations)                     │
└──────────────────────────────────┬──────────────────────────────────┘
                                   │
                    ┌──────────────┴──────────────┐
                    │                             │
        ┌───────────▼──────────┐     ┌───────────▼─────────────┐
        │   REST API Layer     │     │   WebSocket (SignalR)   │
        │  (Authentication)    │     │  (Real-time Updates)    │
        └───────────┬──────────┘     └───────────┬─────────────┘
                    │                             │
        ┌───────────▼─────────────────────────────▼────────┐
        │         ASP.NET Core 9 Application Layer         │
        │                                                   │
        │  ┌──────────────────────────────────────────┐   │
        │  │      Multi-Tenant Middleware             │   │
        │  │  (Request Processing & Tenant Isolation) │   │
        │  └──────────────────────────────────────────┘   │
        │                                                   │
        │  ┌──────────────────────────────────────────┐   │
        │  │      Controllers & Services              │   │
        │  │  • Payment Processing                    │   │
        │  │  • Subscription Management               │   │
        │  │  • Customer Management                   │   │
        │  │  • Invoice Processing                    │   │
        │  │  • Analytics Computation                 │   │
        │  └──────────────────────────────────────────┘   │
        │                                                   │
        │  ┌──────────────────────────────────────────┐   │
        │  │      Background Services                 │   │
        │  │  • Webhook Dispatcher                    │   │
        │  │  • Retry Logic                           │   │
        │  │  • Scheduled Tasks                       │   │
        │  └──────────────────────────────────────────┘   │
        └───────────┬──────────────────────────────────────┘
                    │
        ┌───────────┴───────────┬──────────────────┐
        │                       │                  │
┌───────▼────────┐  ┌──────────▼──────┐  ┌───────▼─────┐
│  SQL Server    │  │ Stripe API       │  │ Seq Logging │
│  (Data Layer)  │  │ (Payment API)    │  │ (Observ.)   │
└────────────────┘  └─────────────────┘  └─────────────┘
```

## Multi-Tenant Architecture

### Tenant Isolation Strategy

The system implements complete data isolation at multiple layers:

#### 1. Database Level
```
┌────────────────────────────────────────────┐
│          Single SQL Server Database        │
├────────────────────────────────────────────┤
│                                            │
│  ┌──────────────────────────────────────┐ │
│  │   Tenant 1 Data                      │ │
│  │  - Users (with TenantId)            │ │
│  │  - Customers (filtered by TenantId) │ │
│  │  - Payments (filtered by TenantId)  │ │
│  │  - Subscriptions (filtered)         │ │
│  └──────────────────────────────────────┘ │
│                                            │
│  ┌──────────────────────────────────────┐ │
│  │   Tenant 2 Data                      │ │
│  │  - Users (with TenantId)            │ │
│  │  - Customers (filtered by TenantId) │ │
│  │  - Payments (filtered by TenantId)  │ │
│  │  - Subscriptions (filtered)         │ │
│  └──────────────────────────────────────┘ │
│                                            │
│  [Additional Tenants...]                  │
└────────────────────────────────────────────┘
```

#### 2. Query Level
Every database query includes tenant filtering:
```csharp
var payments = context.Payments
    .Where(p => p.TenantId == currentTenantId)
    .ToListAsync();
```

#### 3. API Level
HTTP headers carry tenant context:
```
X-Tenant-Id: tenant_123
Authorization: Bearer {jwt_token}
```

#### 4. Middleware Level
TenantMiddleware extracts and validates tenant context:
```
Request → TenantMiddleware → Extract TenantId from headers
       → Validate user belongs to tenant
       → Store in HttpContext
       → Pass to downstream services
```

## Authentication & Authorization

### Authentication Flow

```
┌─────────────────────────────────────────────────────┐
│            Login Request                            │
│  POST /api/v1/auth/login                            │
│  {email, password}                                  │
└────────────────────┬────────────────────────────────┘
                     │
        ┌────────────▼────────────┐
        │ Validate Credentials    │
        │ (Email & Password)      │
        └────────────┬────────────┘
                     │
        ┌────────────▼────────────┐
        │ Load User & Roles       │
        │ Load Tenant Info        │
        └────────────┬────────────┘
                     │
        ┌────────────▼────────────────────────┐
        │ Generate JWT Token                  │
        │ • Claims: UserId, Email, TenantId   │
        │ • Roles: SuperAdmin, Admin, etc.    │
        │ • ExpiresIn: 24 hours               │
        └────────────┬────────────────────────┘
                     │
        ┌────────────▼────────────┐
        │ Return Token & User     │
        │ Info                    │
        └────────────┬────────────┘
                     │
        ┌────────────▼────────────────────────┐
        │ Subsequent API Requests             │
        │ Authorization: Bearer {token}       │
        │ X-Tenant-Id: {tenant_id}            │
        └────────────┬────────────────────────┘
                     │
        ┌────────────▼──────────────────────────┐
        │ JWT Validation Middleware             │
        │ 1. Verify signature                   │
        │ 2. Check expiration                   │
        │ 3. Extract claims                     │
        │ 4. Validate tenant consistency        │
        └────────────┬──────────────────────────┘
                     │
        ┌────────────▼──────────────────────────┐
        │ Set HttpContext.User                  │
        │ (ClaimsPrincipal with all claims)     │
        └──────────────────────────────────────┘
```

### Authorization Levels

#### 1. Role-Based Access Control (RBAC)

| Role | Permissions |
|------|-------------|
| **SuperAdmin** | Full platform access, manage all tenants, user management, system settings |
| **Admin** | All tenant operations, user management, webhook configuration, refund approval |
| **Manager** | Payments, subscriptions, customers, invoices, basic analytics |
| **Viewer** | Read-only access to payments, subscriptions, customers, analytics |

Applied via `[Authorize(Roles = "Admin,Manager")]` attributes.

#### 2. Tenant-Based Access

Every endpoint requires valid tenant context:
```csharp
[Authorize]
[Route("api/v1/[controller]")]
public class CustomersController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCustomers()
    {
        var tenantId = User.FindFirst("TenantId")?.Value;
        // Query only this tenant's customers
    }
}
```

#### 3. API Key Authentication

For server-to-server integration:
```
X-Api-Key: api_key_123456789
X-Tenant-Id: tenant_123
```

API keys are scoped to:
- Specific tenant
- Specific permissions (read, write, refund, etc.)
- IP whitelist (optional)
- Rate limits

### HMAC-SHA256 Webhook Signing

Verify webhook authenticity:

```
X-Signature: sha256=<computed_hash>
X-Timestamp: <request_timestamp>

Signature Computation (SHA256):
  HMAC-SHA256(
    message = "{timestamp}.{raw_body}",
    key = "webhook_signing_secret"
  )

Verification:
  1. Extract X-Signature and X-Timestamp from headers
  2. Compute expected signature with current timestamp
  3. Check if expected matches received (timing-safe comparison)
  4. Verify timestamp is within 5 minutes (replay attack prevention)
```

## Webhook System

### Inbound Webhooks (From Stripe)

```
┌──────────────────────────────────────┐
│     Stripe Platform                  │
│  (Payment Events)                    │
└─────────────────┬────────────────────┘
                  │
                  │ HTTPS POST
                  │
        ┌─────────▼─────────┐
        │ Webhook Handler   │
        │ /api/v1/webhooks/ │
        │    stripe         │
        └─────────┬─────────┘
                  │
        ┌─────────▼──────────────────────┐
        │ Signature Verification         │
        │ (Stripe-Signature header)      │
        └─────────┬──────────────────────┘
                  │
        ┌─────────▼──────────────────────┐
        │ Parse Event Type               │
        │ • payment_intent.succeeded     │
        │ • customer.subscription.created│
        │ • invoice.paid                 │
        │ [14 event types total]         │
        └─────────┬──────────────────────┘
                  │
        ┌─────────▼──────────────────────────────────┐
        │ Process Event                              │
        │ 1. Update database entities                │
        │ 2. Compute analytics                       │
        │ 3. Enqueue outbound webhook                │
        │ 4. Send SignalR notification               │
        └─────────┬──────────────────────────────────┘
                  │
        ┌─────────▼──────────────────────────────────┐
        │ Log Event                                  │
        │ • Structured logging to Seq                │
        │ • Event payload & processing result        │
        └──────────────────────────────────────────┘
```

### Outbound Webhooks (To Clients)

```
┌──────────────────────────────────────────────┐
│     Application Event Queue                  │
│  (RabbitMQ, SQL Server Service Broker, etc)  │
└──────────────────┬───────────────────────────┘
                   │
        ┌──────────▼──────────┐
        │ Webhook Dispatcher  │
        │ (Background Service)│
        └──────────┬──────────┘
                   │
        ┌──────────▼──────────────────────────┐
        │ Events:                              │
        │ • payment.completed                 │
        │ • payment.failed                    │
        │ • subscription.created              │
        │ • subscription.updated              │
        │ • subscription.canceled             │
        │ • refund.processed                  │
        │ [11 event types total]              │
        └──────────┬──────────────────────────┘
                   │
        ┌──────────▼──────────────────────────────┐
        │ Prepare Webhook                        │
        │ • Serialize event data                 │
        │ • Generate timestamp                   │
        │ • Compute HMAC-SHA256 signature        │
        │ • Set delivery status = pending        │
        └──────────┬──────────────────────────────┘
                   │
        ┌──────────▼──────────────────────────────┐
        │ Send to Client Endpoint                │
        │ POST {webhook_url}                     │
        │ X-Signature: sha256=...                │
        │ X-Timestamp: ...                       │
        └──────────┬──────────────────────────────┘
                   │
        ┌──────────▼──────────────────────────────┐
        │ Response Handling                      │
        │ • 2xx = Success                        │
        │ • 4xx = Permanent Failure              │
        │ • 5xx = Retry (exponential backoff)    │
        └──────────┬──────────────────────────────┘
                   │
        ┌──────────▼──────────────────────────────┐
        │ Retry Policy (if failed)               │
        │ • Attempt 1: immediate                 │
        │ • Attempt 2: +1 minute                 │
        │ • Attempt 3: +5 minutes                │
        │ • Attempt 4: +30 minutes               │
        │ • Attempt 5: +2 hours                  │
        │ • Attempt 6: +24 hours (final)         │
        │                                         │
        │ Max: 6 attempts over 24+ hours          │
        └──────────┬──────────────────────────────┘
                   │
        ┌──────────▼──────────────────────────────┐
        │ Log Delivery                           │
        │ • Status (success/failed)              │
        │ • Response time                        │
        │ • HTTP status code                     │
        │ • Attempt count                        │
        └──────────────────────────────────────────┘
```

## Service Layer Patterns

### Dependency Injection

Services are registered using ASP.NET Core DI:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Repository layer
    services.AddScoped<IPaymentRepository, PaymentRepository>();
    services.AddScoped<ICustomerRepository, CustomerRepository>();
    
    // Service layer
    services.AddScoped<IPaymentService, PaymentService>();
    services.AddScoped<ICustomerService, CustomerService>();
    services.AddScoped<IStripeService, StripeService>();
    services.AddScoped<IWebhookService, WebhookService>();
    
    // Background services
    services.AddHostedService<WebhookDispatcherService>();
    services.AddHostedService<RetryService>();
    
    // Middleware
    services.AddScoped<TenantMiddleware>();
}
```

### Service Architecture

```
Controllers (HTTP Layer)
    ↓
Services (Business Logic)
    ├─ IPaymentService
    ├─ ISubscriptionService
    ├─ ICustomerService
    ├─ IInvoiceService
    ├─ IAnalyticsService
    └─ IStripeService (Stripe API wrapper)
    ↓
Repositories (Data Access)
    ├─ IPaymentRepository
    ├─ ICustomerRepository
    ├─ ISubscriptionRepository
    └─ IInvoiceRepository
    ↓
DbContext (Entity Framework)
    ↓
SQL Server (Database)
```

## Database Schema

### Core Entities & Relationships

```
┌─────────────────┐
│    Tenant       │
│  (Multi-tenant) │
├─────────────────┤
│ Id (Guid)       │
│ Name            │
│ StripeAccountId │
│ Status          │
│ CreatedAt       │
└────────┬────────┘
         │ 1:N
         ├──────────────────────────────┐
         │                              │
    ┌────▼─────────┐          ┌────────▼────┐
    │    User      │          │  Customer   │
    ├──────────────┤          ├─────────────┤
    │ Id (Guid)    │          │ Id (Guid)   │
    │ TenantId     │          │ TenantId    │
    │ Email        │          │ Name        │
    │ PasswordHash │          │ Email       │
    │ Role         │          │ StripeId    │
    │ IsActive     │          │ Metadata    │
    │ CreatedAt    │          │ CreatedAt   │
    └──────────────┘          └────┬────────┘
                                   │ 1:N
                         ┌─────────┴──────────┐
                         │                    │
                  ┌──────▼────────┐    ┌──────▼──────┐
                  │  Payment      │    │Subscription │
                  ├───────────────┤    ├─────────────┤
                  │ Id (Guid)     │    │ Id (Guid)   │
                  │ TenantId      │    │ TenantId    │
                  │ CustomerId    │    │ CustomerId  │
                  │ StripeId      │    │ PlanId      │
                  │ Amount        │    │ StripeId    │
                  │ Currency      │    │ Status      │
                  │ Status        │    │ RenewsAt    │
                  │ CreatedAt     │    │ CanceledAt  │
                  └──────┬────────┘    │ CreatedAt   │
                         │ 1:N         └─────────────┘
                  ┌──────▼────────┐
                  │   Invoice     │
                  ├───────────────┤
                  │ Id (Guid)     │
                  │ TenantId      │
                  │ CustomerId    │
                  │ PaymentId     │
                  │ Status        │
                  │ DueDate       │
                  │ PaidAt        │
                  │ CreatedAt     │
                  └───────────────┘

┌──────────────────┐
│     Plan         │
├──────────────────┤
│ Id (Guid)        │
│ TenantId         │
│ Name             │
│ StripeId         │
│ Amount           │
│ Currency         │
│ BillingCycle     │
│ Metadata         │
│ CreatedAt        │
└──────────────────┘

┌──────────────────┐
│     Refund       │
├──────────────────┤
│ Id (Guid)        │
│ TenantId         │
│ PaymentId        │
│ StripeId         │
│ Amount           │
│ Currency         │
│ Reason           │
│ Status           │
│ CreatedAt        │
└──────────────────┘

┌──────────────────────────┐
│   WebhookDelivery        │
├──────────────────────────┤
│ Id (Guid)                │
│ TenantId                 │
│ EventType                │
│ PayloadJson              │
│ Status (Pending/Sent)    │
│ RetryCount               │
│ NextRetryAt              │
│ LastAttemptAt            │
│ LastResponseStatus       │
│ CreatedAt                │
└──────────────────────────┘

┌──────────────────────────┐
│   WebhookSubscription    │
├──────────────────────────┤
│ Id (Guid)                │
│ TenantId                 │
│ ClientId                 │
│ WebhookUrl               │
│ EventTypes[]             │
│ SigningSecret            │
│ IsActive                 │
│ CreatedAt                │
└──────────────────────────┘
```

## Background Services

### WebhookDispatcherService

Runs continuously in background:

```csharp
public class WebhookDispatcherService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // 1. Query pending webhooks
            var pending = await GetPendingWebhooksAsync();
            
            // 2. For each webhook
            foreach (var webhook in pending)
            {
                try
                {
                    // 3. Send HTTP request to client endpoint
                    var response = await SendWebhookAsync(webhook);
                    
                    // 4. Update delivery status
                    await UpdateDeliveryStatusAsync(webhook, response);
                }
                catch (Exception ex)
                {
                    // 5. Schedule retry
                    await ScheduleRetryAsync(webhook, ex);
                }
            }
            
            // 6. Wait before next iteration (e.g., 30 seconds)
            await Task.Delay(TimeSpan.FromSeconds(30), token);
        }
    }
}
```

### RetryService

Handles exponential backoff retries:

```
Retry Schedule:
  Attempt 1: Immediate
  Attempt 2: +1 minute
  Attempt 3: +5 minutes
  Attempt 4: +30 minutes
  Attempt 5: +2 hours
  Attempt 6: +24 hours (final attempt)

Formula: delay = baseDelay * (2 ^ attemptNumber)
With jitter: delay += random(0, delay * 0.1)
```

## Rate Limiting Strategy

### Middleware-Based Rate Limiting

```csharp
public class RateLimitingMiddleware
{
    private readonly IMemoryCache _cache;
    private const int MaxRequestsPerMinute = 60;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = context.User.FindFirst("UserId")?.Value ?? 
                      context.Request.Headers["X-Api-Key"].ToString();
        
        var key = $"rate-limit-{clientId}";
        var requestCount = _cache.Get<int>(key) ?? 0;
        
        if (requestCount >= MaxRequestsPerMinute)
        {
            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.Headers.Add("Retry-After", "60");
            return;
        }
        
        _cache.Set(key, requestCount + 1, TimeSpan.FromMinutes(1));
        await _next(context);
    }
}
```

### Response Headers

```
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 45
X-RateLimit-Reset: 1708970400
```

### Limits by Role

| Role | Requests/Min | Burst |
|------|-------------|-------|
| SuperAdmin | 1000 | 2000 |
| Admin | 500 | 1000 |
| Manager | 300 | 600 |
| Viewer | 100 | 200 |
| API Key | 200 | 400 |

## Error Handling Pattern

### GatewayResponseWrapper

All API responses use consistent error format:

```csharp
public class GatewayResponseWrapper<T>
{
    public bool IsSuccessful { get; set; }
    public T Data { get; set; }
    public Error Error { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
}

public class Error
{
    public string Code { get; set; }        // e.g., "STRIPE_API_ERROR"
    public string Message { get; set; }     // User-friendly message
    public Dictionary<string, string> Details { get; set; } // Field-level errors
    public string CorrelationId { get; set; } // For tracing
}
```

### Error Response Example

```json
{
  "isSuccessful": false,
  "data": null,
  "error": {
    "code": "INSUFFICIENT_FUNDS",
    "message": "The payment was declined due to insufficient funds",
    "details": {
      "stripe_error_code": "card_declined",
      "stripe_error_message": "Your card has insufficient funds"
    },
    "correlationId": "550e8400-e29b-41d4-a716-446655440000"
  },
  "message": "Payment processing failed",
  "timestamp": "2026-02-26T10:30:00Z"
}
```

## Real-Time Updates (SignalR)

### Hub Architecture

```
┌─────────────────────────────────────┐
│       SignalR Hub (ASP.NET Core)    │
│  BillingHub : Hub                   │
├─────────────────────────────────────┤
│ Methods:                            │
│ • JoinTenantGroup(tenantId)         │
│ • SendPaymentUpdate(payment)        │
│ • SendAnalyticsUpdate(analytics)    │
│ • SendWebhookStatus(webhook)        │
└──────────────────┬──────────────────┘
                   │
        ┌──────────┴──────────┐
        │                     │
┌───────▼────────────┐  ┌────▼───────────────┐
│  Client Connection │  │  Client Connection │
│  (React Browser)   │  │  (React Browser)   │
└────────────────────┘  └────────────────────┘
```

### Real-Time Event Flow

```
Payment Processed
    ↓
PaymentService.ProcessPaymentAsync()
    ↓
IHubContext<BillingHub>.Clients
    .Group(tenantId)
    .SendAsync("PaymentUpdated", payment)
    ↓
React Component Receives Update
    ↓
Update Dashboard (Charts, Tables, etc.)
```

### Client Subscription Example

```typescript
// React Component
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/api/v1/hubs/billing", {
    headers: {
      Authorization: `Bearer ${token}`,
      "X-Tenant-Id": tenantId
    }
  })
  .withAutomaticReconnect()
  .build();

connection.start();
connection.invoke("JoinTenantGroup", tenantId);

connection.on("PaymentUpdated", (payment) => {
  setPayments(prev => [...prev, payment]);
});

connection.on("AnalyticsUpdated", (analytics) => {
  setAnalytics(analytics);
});
```

## Caching Strategy

### Multi-Level Caching

```
┌────────────────────────────────────────┐
│   In-Memory Cache (IMemoryCache)       │
│   (L1 - Fast, Process-bound)           │
│   • Dashboard metrics                  │
│   • User permissions                   │
│   • Rate limit counters                │
│   TTL: 5-15 minutes                    │
└────────────────────────────────────────┘
                 ↓
┌────────────────────────────────────────┐
│   Distributed Cache (SQL Server)       │
│   (L2 - Medium, Shared)                │
│   • Analytics aggregates               │
│   • Frequently accessed customers      │
│   TTL: 30 minutes - 1 hour             │
└────────────────────────────────────────┘
                 ↓
┌────────────────────────────────────────┐
│   Database Query (SQL Server)          │
│   (L3 - Slow, Authoritative)           │
│   • Primary data source                │
└────────────────────────────────────────┘
```

## Security Considerations

### Data Protection
- All sensitive data encrypted at rest (SQL Server Transparent Data Encryption)
- HTTPS/TLS for all network communications
- Passwords hashed using PBKDF2 with salt
- API keys hashed before storage

### API Security
- CORS configured for allowed origins
- CSRF protection for state-changing operations
- SQL injection prevention via parameterized queries
- XSS prevention via output encoding
- Rate limiting to prevent brute force attacks
- Request validation and sanitization

### Webhook Security
- Stripe signature verification required
- HMAC-SHA256 signature for outbound webhooks
- Timestamp validation (prevent replay attacks)
- IP whitelist (optional)
- Webhook delivery logging and audit trail

---

Last Updated: February 26, 2026
