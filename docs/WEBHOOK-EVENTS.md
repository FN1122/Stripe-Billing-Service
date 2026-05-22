# Webhook Events Reference — Stripe Billing Service

Complete reference for all inbound (Stripe → Service) and outbound (Service → Client) webhook events.

> **Note:** This document will be expanded with full JSON payload examples during development (Day 4).

---

## Inbound Events: Stripe → Billing Service

Received at: `POST /api/v1/webhooks/stripe`  
Verified with: Stripe signing secret per tenant (`ConstructEvent()`)

### Payment Events
| Stripe Event | Processing Action | Outbound Callback |
|-------------|-------------------|-------------------|
| `checkout.session.completed` | Record transaction, activate subscription if mode=subscription | `payment.completed` |
| `payment_intent.succeeded` | Update transaction status to succeeded | `payment.completed` |
| `payment_intent.payment_failed` | Log failure, update status, start dunning if subscription | `payment.failed` |

### Subscription Events
| Stripe Event | Processing Action | Outbound Callback |
|-------------|-------------------|-------------------|
| `customer.subscription.created` | Save Subscription entity | `subscription.activated` |
| `customer.subscription.updated` | Handle plan change, status change, quantity update | `subscription.upgraded` / `subscription.downgraded` / `subscription.payment_failed` |
| `customer.subscription.deleted` | Mark cancelled, record cancellation date | `subscription.cancelled` |
| `customer.subscription.trial_will_end` | Send trial ending reminder (3 days before) | `subscription.trial_ending` |

### Invoice Events
| Stripe Event | Processing Action | Outbound Callback |
|-------------|-------------------|-------------------|
| `invoice.paid` | Mark invoice paid, extend subscription period | `invoice.generated` |
| `invoice.payment_failed` | Start dunning retry, alert admin | `subscription.payment_failed` |

### Other Events
| Stripe Event | Processing Action | Outbound Callback |
|-------------|-------------------|-------------------|
| `charge.refunded` | Process refund, update transaction AmountRefunded | `refund.processed` |
| `charge.dispute.created` | Alert admin, log dispute | (admin notification only) |
| `customer.updated` | Sync customer data (email, name, address) | `customer.updated` |
| `payment_method.attached` | Update customer payment methods | (no callback) |
| `price.updated` | Sync plan pricing changes | (no callback) |

---

## Outbound Events: Billing Service → Client App

Delivered to: Tenant's registered webhook callback URL  
Signed with: `X-Webhook-Signature: HMAC-SHA256(payload|timestamp, webhookSecret)`

### Event Format
```json
{
  "id": "evt_abc123def456",
  "type": "payment.completed",
  "tenantId": "550e8400-e29b-41d4-a716-446655440000",
  "timestamp": "2026-02-26T10:30:00Z",
  "data": {
    // Event-specific payload (see below)
  }
}
```

### Headers
```
X-Webhook-Signature: sha256=abcdef1234567890...
X-Webhook-Timestamp: 1709000000
X-Webhook-ID: evt_abc123def456
X-Webhook-Retry: 0
Content-Type: application/json
```

### Payment Events

#### `payment.completed`
```json
{
  "transactionId": "guid",
  "customerId": "guid",
  "externalReferenceId": "client-user-123",
  "amount": 29.99,
  "currency": "usd",
  "paymentMethod": "card",
  "paymentMethodLast4": "4242",
  "type": "one_time",
  "stripePaymentIntentId": "pi_xxx",
  "receiptUrl": "https://receipt.stripe.com/..."
}
```

#### `payment.failed`
```json
{
  "transactionId": "guid",
  "customerId": "guid",
  "externalReferenceId": "client-user-123",
  "amount": 29.99,
  "currency": "usd",
  "failureReason": "card_declined",
  "stripePaymentIntentId": "pi_xxx"
}
```

### Subscription Events

#### `subscription.activated`
```json
{
  "subscriptionId": "guid",
  "customerId": "guid",
  "externalReferenceId": "client-user-123",
  "planId": "guid",
  "planName": "Pro Monthly",
  "amount": 29.99,
  "currency": "usd",
  "interval": "month",
  "status": "active",
  "currentPeriodEnd": "2026-03-26T00:00:00Z",
  "trialEnd": null
}
```

#### `subscription.upgraded`
```json
{
  "subscriptionId": "guid",
  "customerId": "guid",
  "externalReferenceId": "client-user-123",
  "previousPlanId": "guid",
  "previousPlanName": "Basic Monthly",
  "newPlanId": "guid",
  "newPlanName": "Pro Monthly",
  "proratedAmount": 15.50,
  "effectiveDate": "2026-02-26T00:00:00Z"
}
```

#### `subscription.downgraded`
```json
{
  "subscriptionId": "guid",
  "customerId": "guid",
  "externalReferenceId": "client-user-123",
  "previousPlanId": "guid",
  "newPlanId": "guid",
  "effectiveDate": "2026-03-26T00:00:00Z",
  "note": "Takes effect at end of current period"
}
```

#### `subscription.cancelled`
```json
{
  "subscriptionId": "guid",
  "customerId": "guid",
  "externalReferenceId": "client-user-123",
  "planId": "guid",
  "cancelledAt": "2026-02-26T10:00:00Z",
  "effectiveEnd": "2026-03-26T00:00:00Z",
  "reason": "too_expensive"
}
```

#### `subscription.trial_ending`
```json
{
  "subscriptionId": "guid",
  "customerId": "guid",
  "externalReferenceId": "client-user-123",
  "planId": "guid",
  "trialEnd": "2026-03-01T00:00:00Z",
  "daysRemaining": 3
}
```

#### `subscription.payment_failed`
```json
{
  "subscriptionId": "guid",
  "customerId": "guid",
  "externalReferenceId": "client-user-123",
  "amount": 29.99,
  "failureReason": "insufficient_funds",
  "retryDate": "2026-02-28T00:00:00Z",
  "attemptCount": 1
}
```

### Other Events

#### `refund.processed`
```json
{
  "refundId": "guid",
  "transactionId": "guid",
  "customerId": "guid",
  "externalReferenceId": "client-user-123",
  "amount": 29.99,
  "currency": "usd",
  "reason": "requested_by_customer",
  "status": "succeeded"
}
```

#### `invoice.generated`
```json
{
  "invoiceId": "guid",
  "customerId": "guid",
  "externalReferenceId": "client-user-123",
  "invoiceNumber": "INV-2026-0042",
  "total": 29.99,
  "currency": "usd",
  "status": "paid",
  "pdfUrl": "https://..."
}
```

#### `customer.updated`
```json
{
  "customerId": "guid",
  "externalReferenceId": "client-user-123",
  "email": "updated@example.com",
  "name": "Updated Name",
  "changedFields": ["email", "name"]
}
```

---

## Delivery Guarantees

| Property | Value |
|----------|-------|
| Delivery model | At-least-once |
| Timeout | 30 seconds per attempt |
| Retry schedule | 1m, 5m, 30m, 2h, 8h, 24h |
| Max retries | 6 |
| After max retries | Dead letter queue (visible in admin dashboard) |
| Manual retry | Available from admin dashboard |
| Deduplication | Client should check `X-Webhook-ID` for duplicates |

---

## Client Verification Guide

```python
# Python example
import hmac
import hashlib
import time

def verify_webhook(payload: bytes, signature: str, timestamp: str, secret: str) -> bool:
    # Check timestamp (5-minute window)
    if abs(time.time() - int(timestamp)) > 300:
        return False
    
    # Compute expected signature
    message = payload.decode() + "|" + timestamp
    expected = "sha256=" + hmac.new(
        secret.encode(), message.encode(), hashlib.sha256
    ).hexdigest()
    
    # Constant-time comparison
    return hmac.compare_digest(expected, signature)
```

```javascript
// Node.js example
const crypto = require('crypto');

function verifyWebhook(payload, signature, timestamp, secret) {
  // Check timestamp (5-minute window)
  if (Math.abs(Date.now() / 1000 - parseInt(timestamp)) > 300) return false;
  
  // Compute expected signature
  const message = payload + '|' + timestamp;
  const expected = 'sha256=' + crypto
    .createHmac('sha256', secret)
    .update(message)
    .digest('hex');
  
  // Constant-time comparison
  return crypto.timingSafeEqual(Buffer.from(expected), Buffer.from(signature));
}
```

```csharp
// C# example
using System.Security.Cryptography;
using System.Text;

bool VerifyWebhook(string payload, string signature, string timestamp, string secret)
{
    // Check timestamp (5-minute window)
    var ts = DateTimeOffset.FromUnixTimeSeconds(long.Parse(timestamp));
    if (Math.Abs((DateTimeOffset.UtcNow - ts).TotalSeconds) > 300) return false;
    
    // Compute expected signature
    var message = payload + "|" + timestamp;
    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
    var expected = "sha256=" + BitConverter.ToString(hash).Replace("-", "").ToLower();
    
    // Constant-time comparison
    return CryptographicOperations.FixedTimeEquals(
        Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(signature));
}
```
