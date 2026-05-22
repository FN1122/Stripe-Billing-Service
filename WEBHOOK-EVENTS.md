# Webhook Events Documentation

## Inbound Events from Stripe

The service receives webhooks from Stripe and processes them synchronously. All inbound events are verified using Stripe's signature verification mechanism.

### Event Types

#### 1. checkout.session.completed
Triggered when a Stripe Checkout session is completed successfully.

```json
{
  "type": "checkout.session.completed",
  "data": {
    "object": {
      "id": "cs_live_a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6",
      "customer": "cus_123456789",
      "mode": "payment",
      "payment_status": "paid",
      "status": "complete",
      "line_items": [
        {
          "price": {
            "id": "price_1234567890",
            "unit_amount": 5000,
            "currency": "usd"
          },
          "quantity": 1
        }
      ],
      "total_details": {
        "amount_discount": 0,
        "amount_shipping": 0,
        "amount_tax": 0
      },
      "amount_total": 5000,
      "currency": "usd",
      "metadata": {
        "order_id": "12345"
      },
      "created": 1708970400
    }
  }
}
```

**Actions**:
- Create or update Payment record
- Update Customer if new
- Send SignalR notification
- Enqueue outbound webhook: `payment.completed`

---

#### 2. payment_intent.succeeded
Triggered when a Payment Intent is confirmed and payment succeeds.

```json
{
  "type": "payment_intent.succeeded",
  "data": {
    "object": {
      "id": "pi_1234567890",
      "client_secret": "pi_1234567890_secret_abc123xyz",
      "customer": "cus_123456789",
      "amount": 5000,
      "amount_capturable": 0,
      "amount_received": 5000,
      "currency": "usd",
      "status": "succeeded",
      "charges": {
        "object": "list",
        "data": [
          {
            "id": "ch_1234567890",
            "amount": 5000,
            "currency": "usd",
            "status": "succeeded"
          }
        ]
      },
      "metadata": {
        "order_id": "12345"
      },
      "created": 1708970400
    }
  }
}
```

**Actions**:
- Update Payment status to succeeded
- Create Invoice
- Update Customer subscription (if applicable)
- Send SignalR notification
- Enqueue outbound webhook: `payment.completed`

---

#### 3. payment_intent.payment_failed
Triggered when a payment attempt fails.

```json
{
  "type": "payment_intent.payment_failed",
  "data": {
    "object": {
      "id": "pi_1234567890",
      "customer": "cus_123456789",
      "amount": 5000,
      "currency": "usd",
      "status": "requires_payment_method",
      "last_payment_error": {
        "code": "card_declined",
        "message": "Your card was declined",
        "type": "card_error"
      },
      "created": 1708970400
    }
  }
}
```

**Actions**:
- Update Payment status to failed
- Record error details
- Send SignalR notification
- Enqueue outbound webhook: `payment.failed`

---

#### 4. customer.subscription.created
Triggered when a new subscription is created.

```json
{
  "type": "customer.subscription.created",
  "data": {
    "object": {
      "id": "sub_1234567890",
      "customer": "cus_123456789",
      "status": "active",
      "items": {
        "object": "list",
        "data": [
          {
            "id": "si_1234567890",
            "price": {
              "id": "price_1234567890",
              "unit_amount": 9900,
              "currency": "usd",
              "recurring": {
                "interval": "month",
                "interval_count": 1
              }
            },
            "quantity": 1
          }
        ]
      },
      "current_period_start": 1708970400,
      "current_period_end": 1711562400,
      "metadata": {
        "campaign": "march_2026"
      },
      "created": 1708970400
    }
  }
}
```

**Actions**:
- Create Subscription record
- Update Customer active subscription
- Send SignalR notification
- Enqueue outbound webhook: `subscription.created`

---

#### 5. customer.subscription.updated
Triggered when a subscription is updated.

```json
{
  "type": "customer.subscription.updated",
  "data": {
    "object": {
      "id": "sub_1234567890",
      "customer": "cus_123456789",
      "status": "active",
      "items": {
        "object": "list",
        "data": [
          {
            "id": "si_1234567890",
            "price": {
              "id": "price_9999999999",
              "unit_amount": 19900,
              "currency": "usd"
            },
            "quantity": 2
          }
        ]
      },
      "current_period_end": 1711562400,
      "metadata": {
        "campaign": "march_2026"
      }
    }
  }
}
```

**Actions**:
- Update Subscription record
- Update plan and quantity if changed
- Recalculate analytics
- Send SignalR notification
- Enqueue outbound webhook: `subscription.updated`

---

#### 6. customer.subscription.deleted
Triggered when a subscription is canceled.

```json
{
  "type": "customer.subscription.deleted",
  "data": {
    "object": {
      "id": "sub_1234567890",
      "customer": "cus_123456789",
      "status": "canceled",
      "canceled_at": 1708970400,
      "ended_at": 1711562400
    }
  }
}
```

**Actions**:
- Update Subscription status to canceled
- Set canceledAt timestamp
- Update MRR calculations
- Send SignalR notification
- Enqueue outbound webhook: `subscription.canceled`

---

#### 7. invoice.paid
Triggered when an invoice is paid.

```json
{
  "type": "invoice.paid",
  "data": {
    "object": {
      "id": "in_1234567890",
      "customer": "cus_123456789",
      "subscription": "sub_1234567890",
      "amount": 9900,
      "amount_paid": 9900,
      "currency": "usd",
      "status": "paid",
      "paid_at": 1708970400,
      "due_date": 1708384000,
      "lines": {
        "object": "list",
        "data": [
          {
            "type": "subscription",
            "amount": 9900,
            "currency": "usd"
          }
        ]
      },
      "metadata": {
        "invoice_type": "recurring"
      }
    }
  }
}
```

**Actions**:
- Create or update Invoice record
- Mark as paid with paidAt timestamp
- Update Payment status
- Recalculate revenue metrics
- Send SignalR notification
- Enqueue outbound webhook: `invoice.paid`

---

#### 8. invoice.payment_failed
Triggered when invoice payment fails.

```json
{
  "type": "invoice.payment_failed",
  "data": {
    "object": {
      "id": "in_1234567890",
      "customer": "cus_123456789",
      "amount": 9900,
      "currency": "usd",
      "status": "open",
      "last_finalization_error": {
        "message": "Your card was declined"
      }
    }
  }
}
```

**Actions**:
- Update Invoice status
- Record payment failure
- Send payment retry notification to customer
- Send SignalR notification
- Enqueue outbound webhook: `invoice.failed`

---

#### 9. invoice.finalized
Triggered when an invoice is finalized.

```json
{
  "type": "invoice.finalized",
  "data": {
    "object": {
      "id": "in_1234567890",
      "customer": "cus_123456789",
      "status": "open",
      "finalized_at": 1708970400,
      "due_date": 1711562400
    }
  }
}
```

**Actions**:
- Update Invoice status to open
- Set finalized timestamp
- Queue invoice sending if configured

---

#### 10. charge.refunded
Triggered when a charge is refunded.

```json
{
  "type": "charge.refunded",
  "data": {
    "object": {
      "id": "ch_1234567890",
      "customer": "cus_123456789",
      "amount": 5000,
      "amount_refunded": 2500,
      "currency": "usd",
      "refunded": true,
      "refunds": {
        "object": "list",
        "data": [
          {
            "id": "re_1234567890",
            "amount": 2500,
            "status": "succeeded",
            "reason": "requested_by_customer",
            "created": 1708970400
          }
        ]
      }
    }
  }
}
```

**Actions**:
- Create or update Refund record
- Update Payment refund status
- Update revenue metrics
- Send SignalR notification
- Enqueue outbound webhook: `refund.processed`

---

#### 11. charge.dispute.created
Triggered when a dispute is created.

```json
{
  "type": "charge.dispute.created",
  "data": {
    "object": {
      "id": "dp_1234567890",
      "charge": "ch_1234567890",
      "amount": 5000,
      "currency": "usd",
      "reason": "fraudulent",
      "status": "warning_opened",
      "evidence_due_by": 1711562400,
      "created": 1708970400
    }
  }
}
```

**Actions**:
- Create Dispute record
- Flag Payment as disputed
- Send alert notification to admin
- Send SignalR notification

---

#### 12. customer.created
Triggered when a customer is created in Stripe.

```json
{
  "type": "customer.created",
  "data": {
    "object": {
      "id": "cus_123456789",
      "email": "john@example.com",
      "name": "John Doe",
      "phone": "+1234567890",
      "address": null,
      "metadata": {
        "account_id": "123456"
      },
      "created": 1708970400
    }
  }
}
```

**Actions**:
- Create Customer record
- Sync metadata
- Send SignalR notification
- Enqueue outbound webhook: `customer.updated`

---

#### 13. customer.updated
Triggered when a customer is updated.

```json
{
  "type": "customer.updated",
  "data": {
    "object": {
      "id": "cus_123456789",
      "email": "john.doe@example.com",
      "name": "John Doe Updated",
      "metadata": {
        "tier": "premium"
      }
    }
  }
}
```

**Actions**:
- Update Customer record
- Update metadata
- Send SignalR notification
- Enqueue outbound webhook: `customer.updated`

---

#### 14. customer.deleted
Triggered when a customer is deleted.

```json
{
  "type": "customer.deleted",
  "data": {
    "object": {
      "id": "cus_123456789"
    }
  }
}
```

**Actions**:
- Mark Customer as deleted
- Cancel active subscriptions
- Archive related data
- Send SignalR notification

---

## Outbound Events to Clients

Outbound webhooks are sent to registered client endpoints with HMAC-SHA256 signing. Clients must verify the signature before processing.

### Event Types

#### 1. payment.completed

Sent when a payment is successfully processed.

```json
{
  "event": "payment.completed",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stripeId": "pi_1234567890",
    "customerId": "cus_123456789",
    "amount": 5000,
    "currency": "usd",
    "status": "succeeded",
    "description": "Order #12345",
    "metadata": {
      "order_id": "12345",
      "user_id": "usr_789"
    },
    "createdAt": "2026-02-26T10:30:00Z"
  },
  "timestamp": 1708970400,
  "id": "evt_1234567890"
}
```

---

#### 2. payment.failed

Sent when a payment attempt fails.

```json
{
  "event": "payment.failed",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stripeId": "pi_1234567890",
    "customerId": "cus_123456789",
    "amount": 5000,
    "currency": "usd",
    "status": "requires_payment_method",
    "error": {
      "code": "card_declined",
      "message": "Your card was declined"
    },
    "createdAt": "2026-02-26T10:30:00Z"
  },
  "timestamp": 1708970400,
  "id": "evt_1234567891"
}
```

---

#### 3. subscription.created

Sent when a subscription is created.

```json
{
  "event": "subscription.created",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stripeId": "sub_1234567890",
    "customerId": "cus_123456789",
    "planId": "plan_456789",
    "planName": "Professional Plan",
    "status": "active",
    "amount": 9900,
    "currency": "usd",
    "billingCycle": "monthly",
    "currentPeriodStart": "2026-02-26T10:30:00Z",
    "currentPeriodEnd": "2026-03-26T10:30:00Z",
    "createdAt": "2026-02-26T10:30:00Z"
  },
  "timestamp": 1708970400,
  "id": "evt_1234567892"
}
```

---

#### 4. subscription.updated

Sent when a subscription is modified.

```json
{
  "event": "subscription.updated",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stripeId": "sub_1234567890",
    "customerId": "cus_123456789",
    "planId": "plan_999999",
    "planName": "Enterprise Plan",
    "status": "active",
    "amount": 29900,
    "currency": "usd",
    "billingCycle": "monthly",
    "quantity": 2,
    "currentPeriodStart": "2026-02-26T10:30:00Z",
    "currentPeriodEnd": "2026-03-26T10:30:00Z",
    "updatedAt": "2026-02-26T11:00:00Z"
  },
  "timestamp": 1708970400,
  "id": "evt_1234567893"
}
```

---

#### 5. subscription.canceled

Sent when a subscription is canceled.

```json
{
  "event": "subscription.canceled",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stripeId": "sub_1234567890",
    "customerId": "cus_123456789",
    "planId": "plan_456789",
    "status": "canceled",
    "amount": 9900,
    "currency": "usd",
    "reason": "customer_requested",
    "canceledAt": "2026-02-26T11:00:00Z",
    "endedAt": "2026-03-26T10:30:00Z"
  },
  "timestamp": 1708970400,
  "id": "evt_1234567894"
}
```

---

#### 6. subscription.renewed

Sent when a subscription renews (new billing cycle).

```json
{
  "event": "subscription.renewed",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stripeId": "sub_1234567890",
    "customerId": "cus_123456789",
    "planId": "plan_456789",
    "amount": 9900,
    "currency": "usd",
    "previousPeriodEnd": "2026-03-26T10:30:00Z",
    "currentPeriodStart": "2026-03-26T10:30:00Z",
    "currentPeriodEnd": "2026-04-26T10:30:00Z",
    "renewedAt": "2026-03-26T10:30:00Z"
  },
  "timestamp": 1708970400,
  "id": "evt_1234567895"
}
```

---

#### 7. invoice.generated

Sent when an invoice is generated.

```json
{
  "event": "invoice.generated",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stripeId": "in_1234567890",
    "customerId": "cus_123456789",
    "subscriptionId": "sub_1234567890",
    "amount": 9900,
    "currency": "usd",
    "status": "draft",
    "dueDate": "2026-03-26T10:30:00Z",
    "lineItems": [
      {
        "description": "Professional Plan",
        "amount": 9900,
        "currency": "usd",
        "period": {
          "start": "2026-02-26T10:30:00Z",
          "end": "2026-03-26T10:30:00Z"
        }
      }
    ],
    "createdAt": "2026-02-26T10:30:00Z"
  },
  "timestamp": 1708970400,
  "id": "evt_1234567896"
}
```

---

#### 8. invoice.paid

Sent when an invoice is paid.

```json
{
  "event": "invoice.paid",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stripeId": "in_1234567890",
    "customerId": "cus_123456789",
    "amount": 9900,
    "currency": "usd",
    "status": "paid",
    "dueDate": "2026-03-26T10:30:00Z",
    "paidAt": "2026-03-20T08:15:00Z",
    "createdAt": "2026-02-26T10:30:00Z"
  },
  "timestamp": 1708970400,
  "id": "evt_1234567897"
}
```

---

#### 9. invoice.overdue

Sent when an invoice becomes overdue.

```json
{
  "event": "invoice.overdue",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stripeId": "in_1234567890",
    "customerId": "cus_123456789",
    "amount": 9900,
    "currency": "usd",
    "status": "open",
    "dueDate": "2026-03-26T10:30:00Z",
    "overdueSince": "2026-03-27T00:00:00Z",
    "daysOverdue": 1,
    "createdAt": "2026-02-26T10:30:00Z"
  },
  "timestamp": 1708970400,
  "id": "evt_1234567898"
}
```

---

#### 10. refund.processed

Sent when a refund is processed.

```json
{
  "event": "refund.processed",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "stripeId": "re_1234567890",
    "paymentId": "550e8400-e29b-41d4-a716-446655440000",
    "customerId": "cus_123456789",
    "amount": 2500,
    "currency": "usd",
    "status": "succeeded",
    "reason": "requested_by_customer",
    "metadata": {
      "ticket_id": "SUP-12345"
    },
    "createdAt": "2026-02-26T10:30:00Z"
  },
  "timestamp": 1708970400,
  "id": "evt_1234567899"
}
```

---

#### 11. customer.updated

Sent when customer information is updated.

```json
{
  "event": "customer.updated",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stripeId": "cus_123456789",
    "email": "john@example.com",
    "name": "John Doe",
    "phone": "+1234567890",
    "metadata": {
      "account_id": "123456",
      "tier": "premium"
    },
    "updatedAt": "2026-02-26T10:30:00Z"
  },
  "timestamp": 1708970400,
  "id": "evt_1234567900"
}
```

---

## Webhook Subscription Setup

### Register Webhook Endpoint

```bash
POST /api/v1/webhooks/subscriptions
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}
X-Api-Key: {api_key}
Content-Type: application/json

{
  "url": "https://example.com/webhooks/billing",
  "events": [
    "payment.completed",
    "payment.failed",
    "subscription.created",
    "subscription.updated",
    "subscription.canceled",
    "subscription.renewed",
    "invoice.generated",
    "invoice.paid",
    "invoice.overdue",
    "refund.processed",
    "customer.updated"
  ],
  "active": true
}
```

Response includes `signingSecret` needed for signature verification.

## HMAC-SHA256 Signature Verification

### Signature Calculation

All outbound webhooks are signed with HMAC-SHA256. Verify using:

```
Signature = base64(HMAC-SHA256(
  message = "{timestamp}.{raw_body}",
  key = "{signing_secret}"
))
```

### Verification Process

1. Extract `X-Signature` and `X-Timestamp` headers
2. Reconstruct message: `"{timestamp}.{raw_request_body}"`
3. Compute expected signature using HMAC-SHA256
4. Compare with received signature (timing-safe comparison)
5. Verify timestamp is within 5 minutes (prevent replay)

### Example: JavaScript

```javascript
const crypto = require('crypto');

function verifySignature(signature, timestamp, body, secret) {
  const message = `${timestamp}.${body}`;
  const expectedSignature = crypto
    .createHmac('sha256', secret)
    .update(message)
    .digest('base64');
  
  // Timing-safe comparison
  return crypto.timingSafeEqual(
    Buffer.from(signature),
    Buffer.from(expectedSignature)
  );
}

// In Express middleware
app.post('/webhooks/billing', express.raw({type: 'application/json'}), (req, res) => {
  const signature = req.headers['x-signature'];
  const timestamp = req.headers['x-timestamp'];
  const body = req.body.toString('utf8');
  
  if (verifySignature(signature, timestamp, body, process.env.WEBHOOK_SIGNING_SECRET)) {
    // Process webhook
    res.json({ success: true });
  } else {
    res.status(401).json({ error: 'Invalid signature' });
  }
});
```

## Retry Policy

### Delivery Attempts

| Attempt | Delay | Cumulative |
|---------|-------|-----------|
| 1 | Immediate | 0s |
| 2 | +1 minute | 1m |
| 3 | +5 minutes | 6m |
| 4 | +30 minutes | 36m |
| 5 | +2 hours | 2h 36m |
| 6 | +24 hours | 26h 36m |

### Retry Trigger Conditions

- HTTP status code 5xx (server error)
- HTTP status code 429 (rate limited)
- Network timeout
- Connection refused
- DNS resolution failure

### Final Status

After 6 failed attempts over 26+ hours:
- Status marked as `failed`
- Notification sent to tenant admin
- Manual retry available via API

### Exponential Backoff Formula

```
delay = baseDelay * (2 ^ attemptNumber)
finalDelay = delay + random(0, delay * 0.1)  // Add 0% to 10% jitter
```

## Webhook Delivery Statuses

| Status | Meaning |
|--------|---------|
| `pending` | Queued, waiting to be sent |
| `sent` | Successfully delivered (2xx response) |
| `failed` | All retry attempts exhausted |
| `retry` | Waiting for next retry attempt |

## Monitoring Webhook Health

### View Delivery Logs

```bash
GET /api/v1/webhooks/deliveries?status=failed&eventType=payment.completed
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response:
{
  "items": [
    {
      "id": "...",
      "event": "payment.completed",
      "status": "failed",
      "statusCode": 500,
      "retryCount": 6,
      "lastError": "Connection timeout",
      "sentAt": "2026-02-26T10:30:00Z"
    }
  ]
}
```

### Webhook Statistics

```bash
GET /api/v1/webhooks/statistics
Authorization: Bearer {token}
X-Tenant-Id: {tenant_id}

Response:
{
  "totalDelivered": 1250,
  "totalFailed": 3,
  "successRate": 99.76,
  "averageResponseTime": 145,
  "lastDelivery": "2026-02-26T10:30:00Z",
  "uptime": 99.99
}
```

---

Last Updated: February 26, 2026
