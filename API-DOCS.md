# API Documentation

## Base URL and Versioning

```
API Version: v1
Base URL: https://api.stripebilling.com/api/v1
Development: http://localhost:5000/api/v1
```

All endpoints are versioned with `/api/v{version}` prefix. Current version is `v1`.

## Authentication Headers

### JWT Token Authentication (Most Endpoints)
```
Authorization: Bearer {jwt_token}
X-Tenant-Id: {tenant_id}
```

### API Key Authentication (Server-to-Server)
```
X-Api-Key: {api_key}
X-Tenant-Id: {tenant_id}
```

### HMAC Signature (Webhook Verification)
```
X-Signature: sha256={computed_hash}
X-Timestamp: {unix_timestamp}
```

### Optional Headers
```
X-Idempotency-Key: {unique_uuid}  # For idempotent requests
X-Request-Id: {correlation_id}    # For request tracing
```

## Authentication Endpoints

### Login
```
POST /auth/login
Content-Type: application/json

Request:
{
  "email": "admin@stripebilling.com",
  "password": "Admin@123"
}

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 86400,
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "admin@stripebilling.com",
      "firstName": "Admin",
      "lastName": "User",
      "role": "Admin",
      "tenantId": "f47ac10b-58cc-4372-a567-0e02b2c3d479"
    }
  },
  "message": "Login successful",
  "timestamp": "2026-02-26T10:30:00Z"
}

Errors:
- 401 Unauthorized: Invalid credentials
- 400 Bad Request: Missing required fields
```

### Logout
```
POST /auth/logout
Authorization: Bearer {token}

Response: 200 OK
{
  "isSuccessful": true,
  "message": "Logout successful"
}
```

### Refresh Token
```
POST /auth/refresh
Authorization: Bearer {old_token}

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 86400
  }
}

Errors:
- 401 Unauthorized: Token expired or invalid
```

## Payment Endpoints

### Create Payment Intent
```
POST /payments/intent
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}
Content-Type: application/json

Request:
{
  "customerId": "cus_123456789",
  "amount": 5000,
  "currency": "usd",
  "description": "Order #12345",
  "metadata": {
    "order_id": "12345",
    "user_id": "usr_789"
  }
}

Response: 201 Created
{
  "isSuccessful": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stripeId": "pi_1234567890",
    "customerId": "cus_123456789",
    "amount": 5000,
    "currency": "usd",
    "status": "requires_payment_method",
    "clientSecret": "pi_1234567890_secret_abc123xyz",
    "createdAt": "2026-02-26T10:30:00Z"
  }
}

Errors:
- 400 Bad Request: Invalid parameters
- 402 Payment Required: Stripe error
```

### Create Checkout Session
```
POST /payments/checkout
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}
Content-Type: application/json

Request:
{
  "customerId": "cus_123456789",
  "lineItems": [
    {
      "priceId": "price_1234567890",
      "quantity": 1
    }
  ],
  "successUrl": "https://example.com/success",
  "cancelUrl": "https://example.com/cancel",
  "metadata": {
    "order_id": "12345"
  }
}

Response: 201 Created
{
  "isSuccessful": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stripeId": "cs_live_a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6",
    "url": "https://checkout.stripe.com/pay/cs_...",
    "createdAt": "2026-02-26T10:30:00Z"
  }
}

Errors:
- 400 Bad Request: Invalid line items
```

### Confirm Payment
```
POST /payments/{paymentId}/confirm
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}
Content-Type: application/json

Request:
{
  "paymentMethodId": "pm_1234567890"
}

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "status": "succeeded",
    "amount": 5000,
    "currency": "usd",
    "createdAt": "2026-02-26T10:30:00Z"
  }
}

Errors:
- 404 Not Found: Payment not found
- 400 Bad Request: Payment cannot be confirmed
```

### Get Payment
```
GET /payments/{paymentId}
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stripeId": "pi_1234567890",
    "customerId": "cus_123456789",
    "amount": 5000,
    "currency": "usd",
    "status": "succeeded",
    "description": "Order #12345",
    "createdAt": "2026-02-26T10:30:00Z"
  }
}

Errors:
- 404 Not Found: Payment not found
```

### List Payments
```
GET /payments?page=1&pageSize=10&status=succeeded&customerId={customerId}
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "customerId": "cus_123456789",
        "amount": 5000,
        "currency": "usd",
        "status": "succeeded",
        "createdAt": "2026-02-26T10:30:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 10,
      "totalItems": 100,
      "totalPages": 10,
      "hasNext": true,
      "hasPrevious": false
    }
  }
}
```

## Customer Endpoints

### Create Customer
```
POST /customers
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}
Content-Type: application/json

Request:
{
  "email": "john@example.com",
  "name": "John Doe",
  "description": "Customer from Acme Corp",
  "metadata": {
    "account_id": "123456",
    "source": "api"
  }
}

Response: 201 Created
{
  "isSuccessful": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stripeId": "cus_123456789",
    "email": "john@example.com",
    "name": "John Doe",
    "createdAt": "2026-02-26T10:30:00Z"
  }
}
```

### Get Customer
```
GET /customers/{customerId}
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stripeId": "cus_123456789",
    "email": "john@example.com",
    "name": "John Doe",
    "subscriptions": [
      {
        "id": "sub_123",
        "status": "active",
        "planId": "plan_456"
      }
    ],
    "createdAt": "2026-02-26T10:30:00Z"
  }
}

Errors:
- 404 Not Found: Customer not found
```

### Update Customer
```
PUT /customers/{customerId}
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}
Content-Type: application/json

Request:
{
  "name": "John Doe Updated",
  "metadata": {
    "account_id": "123456",
    "tier": "premium"
  }
}

Response: 200 OK
{
  "isSuccessful": true,
  "data": { ... }
}
```

### List Customers
```
GET /customers?page=1&pageSize=10&email={email}
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "items": [ ... ],
    "pagination": { ... }
  }
}
```

### Delete Customer
```
DELETE /customers/{customerId}
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 204 No Content

Errors:
- 404 Not Found: Customer not found
- 400 Bad Request: Cannot delete customer with active subscriptions
```

### Create Customer Portal Session
```
POST /customers/{customerId}/portal-session
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}
Content-Type: application/json

Request:
{
  "returnUrl": "https://example.com/account"
}

Response: 201 Created
{
  "isSuccessful": true,
  "data": {
    "url": "https://billing.stripe.com/b/aHu6EZ1234567890",
    "expiresAt": "2026-02-26T11:30:00Z"
  }
}

Errors:
- 404 Not Found: Customer not found
```

## Subscription Endpoints

### Create Subscription
```
POST /subscriptions
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}
Content-Type: application/json

Request:
{
  "customerId": "cus_123456789",
  "planId": "plan_456789",
  "quantity": 1,
  "metadata": {
    "campaign": "march_2026"
  }
}

Response: 201 Created
{
  "isSuccessful": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stripeId": "sub_1234567890",
    "customerId": "cus_123456789",
    "planId": "plan_456789",
    "status": "active",
    "currentPeriodStart": "2026-02-26T10:30:00Z",
    "currentPeriodEnd": "2026-03-26T10:30:00Z",
    "canceledAt": null,
    "createdAt": "2026-02-26T10:30:00Z"
  }
}

Errors:
- 404 Not Found: Customer or plan not found
- 400 Bad Request: Invalid parameters
```

### Get Subscription
```
GET /subscriptions/{subscriptionId}
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "data": { ... }
}
```

### Update Subscription
```
PUT /subscriptions/{subscriptionId}
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}
Content-Type: application/json

Request:
{
  "planId": "plan_999",
  "quantity": 2
}

Response: 200 OK
{
  "isSuccessful": true,
  "data": { ... }
}
```

### Cancel Subscription
```
DELETE /subscriptions/{subscriptionId}
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}
Content-Type: application/json

Request:
{
  "cancelAt": "2026-03-26T10:30:00Z"  // Optional: schedule cancellation
}

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "status": "canceled",
    "canceledAt": "2026-02-26T10:30:00Z"
  }
}
```

### List Subscriptions
```
GET /subscriptions?page=1&pageSize=10&status=active&customerId={customerId}
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "items": [ ... ],
    "pagination": { ... }
  }
}
```

## Plan Endpoints

### Create Plan
```
POST /plans
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}
Content-Type: application/json

Request:
{
  "name": "Professional Plan",
  "description": "Professional features",
  "amount": 9900,
  "currency": "usd",
  "billingCycle": "monthly",
  "metadata": {
    "features": "advanced"
  }
}

Response: 201 Created
{
  "isSuccessful": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stripeId": "plan_456789",
    "name": "Professional Plan",
    "amount": 9900,
    "currency": "usd",
    "billingCycle": "monthly",
    "createdAt": "2026-02-26T10:30:00Z"
  }
}
```

### Get Plan
```
GET /plans/{planId}
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "data": { ... }
}
```

### List Plans
```
GET /plans?page=1&pageSize=10
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "items": [ ... ],
    "pagination": { ... }
  }
}
```

## Refund Endpoints

### Create Refund
```
POST /refunds
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}
Content-Type: application/json

Request:
{
  "paymentId": "550e8400-e29b-41d4-a716-446655440000",
  "amount": 2500,  // Optional: partial refund (omit for full refund)
  "reason": "requested_by_customer",
  "metadata": {
    "ticket_id": "SUP-12345"
  }
}

Response: 201 Created
{
  "isSuccessful": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "stripeId": "re_1234567890",
    "paymentId": "550e8400-e29b-41d4-a716-446655440000",
    "amount": 2500,
    "currency": "usd",
    "status": "succeeded",
    "reason": "requested_by_customer",
    "createdAt": "2026-02-26T10:30:00Z"
  }
}

Errors:
- 404 Not Found: Payment not found
- 400 Bad Request: Refund amount exceeds payment amount
```

### Get Refund
```
GET /refunds/{refundId}
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "data": { ... }
}
```

### List Refunds
```
GET /refunds?page=1&pageSize=10&paymentId={paymentId}&status=succeeded
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "items": [ ... ],
    "pagination": { ... }
  }
}
```

## Invoice Endpoints

### Get Invoice
```
GET /invoices/{invoiceId}
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stripeId": "in_1234567890",
    "customerId": "cus_123456789",
    "amount": 5000,
    "currency": "usd",
    "status": "paid",
    "dueDate": "2026-03-26T10:30:00Z",
    "paidAt": "2026-02-26T11:00:00Z",
    "createdAt": "2026-02-26T10:30:00Z"
  }
}
```

### List Invoices
```
GET /invoices?page=1&pageSize=10&customerId={customerId}&status=paid
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "items": [ ... ],
    "pagination": { ... }
  }
}
```

### Send Invoice
```
POST /invoices/{invoiceId}/send
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "message": "Invoice sent successfully"
}
```

## Webhook Endpoints

### Register Webhook Subscription
```
POST /webhooks/subscriptions
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}
X-Api-Key: {api_key}
Content-Type: application/json

Request:
{
  "url": "https://example.com/webhooks/billing",
  "events": [
    "payment.completed",
    "payment.failed",
    "subscription.created",
    "subscription.updated",
    "subscription.canceled"
  ],
  "active": true
}

Response: 201 Created
{
  "isSuccessful": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "url": "https://example.com/webhooks/billing",
    "events": [ ... ],
    "signingSecret": "whsec_1234567890abcdefgh",
    "active": true,
    "createdAt": "2026-02-26T10:30:00Z"
  }
}
```

### List Webhook Subscriptions
```
GET /webhooks/subscriptions
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "items": [ ... ]
  }
}
```

### Get Webhook Deliveries
```
GET /webhooks/deliveries?page=1&pageSize=10&status=sent
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "event": "payment.completed",
        "url": "https://example.com/webhooks/billing",
        "status": "sent",
        "statusCode": 200,
        "retryCount": 0,
        "sentAt": "2026-02-26T10:30:00Z"
      }
    ],
    "pagination": { ... }
  }
}
```

### Stripe Inbound Webhook Handler
```
POST /webhooks/stripe
Content-Type: application/json
Stripe-Signature: {stripe_signature}

Request Body: Raw JSON from Stripe

Response: 200 OK
{
  "isSuccessful": true,
  "message": "Webhook received"
}

Note: No authentication required, but Stripe signature must be verified
```

## Dashboard Endpoints

### Get Dashboard Summary
```
GET /dashboard/summary
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "totalRevenue": 125000,
    "totalCustomers": 245,
    "activeSubscriptions": 189,
    "monthlyRecurringRevenue": 12500,
    "churnRate": 2.5,
    "averageOrderValue": 512.24
  }
}
```

### Get Revenue Chart Data
```
GET /dashboard/revenue?period=month&dateFrom=2026-01-01&dateTo=2026-02-26
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "data": [
    {
      "date": "2026-01-01",
      "revenue": 5000,
      "transactions": 23,
      "subscriptions": 15
    },
    ...
  ]
}
```

## Analytics Endpoints

### Get MRR (Monthly Recurring Revenue)
```
GET /analytics/mrr?date=2026-02-26
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "mrr": 125000,
    "mrrGrowth": 5.2,
    "activeSubscriptions": 189,
    "date": "2026-02-26"
  }
}
```

### Get ARR (Annual Recurring Revenue)
```
GET /analytics/arr?date=2026-02-26
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "arr": 1500000,
    "arrGrowth": 18.5,
    "date": "2026-02-26"
  }
}
```

### Get Churn Rate
```
GET /analytics/churn?period=month&date=2026-02-26
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "churnRate": 2.5,
    "churned": 5,
    "startingSubscriptions": 200,
    "period": "2026-02"
  }
}
```

### Get Customer Lifetime Value (LTV)
```
GET /analytics/ltv?customerId={customerId}
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "ltv": 15000,
    "totalRevenue": 15000,
    "lifespanMonths": 24,
    "averageMonthlyRevenue": 625,
    "customerId": "cus_123456789"
  }
}
```

## Super Admin Endpoints

### List All Tenants
```
GET /superadmin/tenants
Authorization: Bearer {token}  # SuperAdmin role required
Content-Type: application/json

Response: 200 OK
{
  "isSuccessful": true,
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "name": "Tenant 1",
        "stripeAccountId": "acct_1234567890",
        "status": "active",
        "createdAt": "2026-02-26T10:30:00Z"
      }
    ],
    "pagination": { ... }
  }
}
```

### Create Tenant
```
POST /superadmin/tenants
Authorization: Bearer {token}  # SuperAdmin role required
Content-Type: application/json

Request:
{
  "name": "Acme Corporation",
  "stripeAccountId": "acct_1234567890"
}

Response: 201 Created
{
  "isSuccessful": true,
  "data": { ... }
}
```

## Error Response Format

All error responses follow this format:

```json
{
  "isSuccessful": false,
  "data": null,
  "error": {
    "code": "ERROR_CODE",
    "message": "User-friendly error message",
    "details": {
      "field": "specific error information"
    },
    "correlationId": "550e8400-e29b-41d4-a716-446655440000"
  },
  "message": "Operation failed",
  "timestamp": "2026-02-26T10:30:00Z"
}
```

### HTTP Status Codes

| Code | Meaning | Example |
|------|---------|---------|
| 200 | OK | Request succeeded |
| 201 | Created | Resource created |
| 204 | No Content | Successful deletion |
| 400 | Bad Request | Invalid parameters |
| 401 | Unauthorized | Missing/invalid token |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource not found |
| 409 | Conflict | Duplicate resource |
| 422 | Unprocessable Entity | Validation error |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Server error |
| 502 | Bad Gateway | Stripe API error |
| 503 | Service Unavailable | Service maintenance |

## Pagination Format

All list endpoints support pagination:

```
Request:
GET /endpoint?page=1&pageSize=10&sortBy=createdAt&sortOrder=desc

Response:
{
  "isSuccessful": true,
  "data": {
    "items": [ ... ],
    "pagination": {
      "page": 1,
      "pageSize": 10,
      "totalItems": 523,
      "totalPages": 53,
      "hasNext": true,
      "hasPrevious": false
    }
  }
}
```

### Pagination Parameters

| Parameter | Type | Default | Max |
|-----------|------|---------|-----|
| page | integer | 1 | N/A |
| pageSize | integer | 10 | 100 |
| sortBy | string | createdAt | N/A |
| sortOrder | string | desc | asc/desc |

## Request Examples

### Using cURL
```bash
# Create payment
curl -X POST http://localhost:5000/api/v1/payments/intent \
  -H "Authorization: Bearer {token}" \
  -H "X-Tenant-Id: {tenant_id}" \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "cus_123456789",
    "amount": 5000,
    "currency": "usd"
  }'
```

### Using JavaScript/Fetch
```javascript
const response = await fetch('/api/v1/payments/intent', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'X-Tenant-Id': tenantId,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    customerId: 'cus_123456789',
    amount: 5000,
    currency: 'usd'
  })
});

const data = await response.json();
```

---

Last Updated: February 26, 2026
