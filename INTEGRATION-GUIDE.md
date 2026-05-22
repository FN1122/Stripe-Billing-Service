# Integration Guide

## Getting Started

This guide covers integrating your application with the Stripe Billing Service.

### Prerequisites

1. Access to the Stripe Billing Service API
2. API Key and Tenant ID
3. Stripe account with publishable key
4. Understanding of REST APIs and webhooks

### API Endpoints

```
Base URL: https://api.stripebilling.com/api/v1
Development: http://localhost:5000/api/v1
```

## Authentication

### API Key Authentication

For server-to-server integration, use API Key authentication:

```
X-Api-Key: {api_key}
X-Tenant-Id: {tenant_id}
```

### Getting Your API Key

1. Login to dashboard
2. Navigate to: Settings > API Keys
3. Click: "Create New API Key"
4. Copy the key (shown only once)
5. Store securely (e.g., environment variable)

### JWT Token Authentication

For browser-based clients:

1. Call login endpoint: `POST /api/v1/auth/login`
2. Receive JWT token
3. Include in subsequent requests:
   ```
   Authorization: Bearer {token}
   X-Tenant-Id: {tenant_id}
   ```

## HMAC Signature Calculation

Outbound webhooks are signed with HMAC-SHA256. Calculate signature to verify webhook authenticity.

### Signature Components

```
X-Signature: sha256={computed_hash}
X-Timestamp: {unix_timestamp}
```

### Verification Algorithm

```
1. Extract X-Signature and X-Timestamp headers
2. Message = "{X-Timestamp}.{raw_request_body}"
3. Signature = base64(HMAC-SHA256(message, signing_secret))
4. Compare with received signature (timing-safe comparison)
5. Verify timestamp is within 5 minutes (prevent replay)
```

### C# Example

```csharp
using System;
using System.Security.Cryptography;
using System.Text;

public class WebhookVerifier
{
    public static bool VerifySignature(
        string receivedSignature,
        string timestamp,
        string body,
        string signingSecret)
    {
        // Reconstruct message
        var message = $"{timestamp}.{body}";
        
        // Compute expected signature
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(signingSecret)))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            var expectedSignature = Convert.ToBase64String(hash);
            
            // Timing-safe comparison
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(receivedSignature),
                Encoding.UTF8.GetBytes(expectedSignature)
            );
        }
    }
}

// Usage in ASP.NET Core
[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly string _signingSecret = "whsec_...";
    
    [HttpPost("billing")]
    public async Task<IActionResult> HandleWebhook(
        [FromHeader(Name = "X-Signature")] string signature,
        [FromHeader(Name = "X-Timestamp")] string timestamp)
    {
        var body = await new StreamReader(HttpContext.Request.Body)
            .ReadToEndAsync();
        
        // Verify signature
        if (!WebhookVerifier.VerifySignature(signature, timestamp, body, _signingSecret))
        {
            return Unauthorized("Invalid signature");
        }
        
        // Process webhook
        var webhook = JsonSerializer.Deserialize<WebhookEvent>(body);
        // ... handle webhook
        
        return Ok(new { received = true });
    }
}
```

### JavaScript Example

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

// Express.js middleware
const express = require('express');
const app = express();

app.post('/webhooks/billing', express.raw({type: 'application/json'}), (req, res) => {
  const signature = req.headers['x-signature'];
  const timestamp = req.headers['x-timestamp'];
  const body = req.body.toString('utf8');
  
  if (!verifySignature(signature, timestamp, body, process.env.WEBHOOK_SIGNING_SECRET)) {
    return res.status(401).json({ error: 'Invalid signature' });
  }
  
  const webhook = JSON.parse(body);
  // ... handle webhook
  
  res.json({ received: true });
});
```

### Python Example

```python
import hmac
import hashlib
import base64
from flask import Flask, request

def verify_signature(signature, timestamp, body, secret):
    """Verify HMAC-SHA256 signature"""
    message = f"{timestamp}.{body}".encode('utf-8')
    expected_signature = base64.b64encode(
        hmac.new(secret.encode('utf-8'), message, hashlib.sha256).digest()
    ).decode('utf-8')
    
    # Timing-safe comparison
    return hmac.compare_digest(signature, expected_signature)

app = Flask(__name__)

@app.route('/webhooks/billing', methods=['POST'])
def handle_webhook():
    signature = request.headers.get('X-Signature')
    timestamp = request.headers.get('X-Timestamp')
    body = request.get_data(as_text=True)
    
    if not verify_signature(signature, timestamp, body, 
                          secret=os.environ['WEBHOOK_SIGNING_SECRET']):
        return {'error': 'Invalid signature'}, 401
    
    webhook = request.get_json()
    # ... handle webhook
    
    return {'received': True}, 200
```

## Creating a Checkout Session

### API Call

```bash
curl -X POST http://localhost:5000/api/v1/payments/checkout \
  -H "Authorization: Bearer {token}" \
  -H "X-Tenant-Id: {tenant_id}" \
  -H "Content-Type: application/json" \
  -d '{
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
  }'
```

### Response

```json
{
  "isSuccessful": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "stripeId": "cs_live_a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6",
    "url": "https://checkout.stripe.com/pay/cs_..."
  }
}
```

### React Implementation

```typescript
import React, { useState } from 'react';
import { apiClient } from './api-client';

interface CheckoutSessionResponse {
  url: string;
}

export function CheckoutButton({ customerId, priceId }) {
  const [loading, setLoading] = useState(false);
  
  const handleCheckout = async () => {
    setLoading(true);
    
    try {
      const response = await apiClient.post('/payments/checkout', {
        customerId,
        lineItems: [
          {
            priceId,
            quantity: 1
          }
        ],
        successUrl: `${window.location.origin}/success`,
        cancelUrl: `${window.location.origin}/cancel`
      });
      
      // Redirect to Stripe Checkout
      window.location.href = response.data.url;
    } catch (error) {
      console.error('Checkout failed:', error);
      alert('Failed to create checkout session');
    } finally {
      setLoading(false);
    }
  };
  
  return (
    <button 
      onClick={handleCheckout} 
      disabled={loading}
    >
      {loading ? 'Processing...' : 'Checkout'}
    </button>
  );
}
```

## Managing Subscriptions

### Create Subscription

```javascript
// JavaScript/TypeScript
async function createSubscription(customerId, planId) {
  const response = await fetch('/api/v1/subscriptions', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'X-Tenant-Id': tenantId,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      customerId,
      planId,
      quantity: 1
    })
  });
  
  return response.json();
}

// Usage
const subscription = await createSubscription('cus_123456789', 'plan_456789');
console.log('Subscription created:', subscription.data.id);
```

### Cancel Subscription

```javascript
async function cancelSubscription(subscriptionId) {
  const response = await fetch(
    `/api/v1/subscriptions/${subscriptionId}`,
    {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${token}`,
        'X-Tenant-Id': tenantId,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        // Optional: schedule cancellation instead of immediate
        // cancelAt: '2026-03-26T10:30:00Z'
      })
    }
  );
  
  return response.json();
}
```

### Update Subscription

```javascript
async function updateSubscription(subscriptionId, updates) {
  const response = await fetch(
    `/api/v1/subscriptions/${subscriptionId}`,
    {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token}`,
        'X-Tenant-Id': tenantId,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(updates)
    }
  );
  
  return response.json();
}

// Upgrade to higher plan
await updateSubscription('sub_123456789', {
  planId: 'plan_enterprise',
  quantity: 2
});
```

## Handling Webhooks

### Register Webhook Endpoint

First, register your webhook endpoint:

```bash
curl -X POST http://localhost:5000/api/v1/webhooks/subscriptions \
  -H "Authorization: Bearer {token}" \
  -H "X-Tenant-Id: {tenant_id}" \
  -H "X-Api-Key: {api_key}" \
  -H "Content-Type: application/json" \
  -d '{
    "url": "https://example.com/webhooks/billing",
    "events": [
      "payment.completed",
      "payment.failed",
      "subscription.created",
      "subscription.updated",
      "subscription.canceled",
      "refund.processed"
    ],
    "active": true
  }'
```

Response includes `signingSecret` for webhook verification.

### Process Webhook Events

```javascript
// Express.js server
const express = require('express');
const app = express();

app.post('/webhooks/billing', express.raw({type: 'application/json'}), (req, res) => {
  const signature = req.headers['x-signature'];
  const timestamp = req.headers['x-timestamp'];
  const body = req.body.toString('utf8');
  
  // Verify signature
  if (!verifySignature(signature, timestamp, body, process.env.WEBHOOK_SIGNING_SECRET)) {
    return res.status(401).json({ error: 'Invalid signature' });
  }
  
  const webhook = JSON.parse(body);
  
  // Handle different event types
  switch (webhook.event) {
    case 'payment.completed':
      handlePaymentCompleted(webhook.data);
      break;
      
    case 'subscription.created':
      handleSubscriptionCreated(webhook.data);
      break;
      
    case 'subscription.canceled':
      handleSubscriptionCanceled(webhook.data);
      break;
      
    case 'refund.processed':
      handleRefundProcessed(webhook.data);
      break;
  }
  
  // Acknowledge receipt
  res.json({ received: true });
});

function handlePaymentCompleted(data) {
  console.log('Payment completed:', {
    customerId: data.customerId,
    amount: data.amount,
    currency: data.currency,
    timestamp: data.createdAt
  });
  
  // Update local database
  // Send confirmation email
  // Update customer's subscription status
}

function handleSubscriptionCreated(data) {
  console.log('Subscription created:', {
    customerId: data.customerId,
    planName: data.planName,
    renewsAt: data.currentPeriodEnd
  });
  
  // Activate subscription in local database
  // Send welcome email
}

function handleSubscriptionCanceled(data) {
  console.log('Subscription canceled:', {
    customerId: data.customerId,
    reason: data.reason
  });
  
  // Mark subscription as canceled
  // Send cancellation email
  // Update user access
}

function handleRefundProcessed(data) {
  console.log('Refund processed:', {
    amount: data.amount,
    reason: data.reason
  });
  
  // Update payment status
  // Send refund confirmation
}
```

## Customer Portal Integration

### Create Portal Session

```javascript
async function createCustomerPortalSession(customerId, returnUrl) {
  const response = await fetch(
    `/api/v1/customers/${customerId}/portal-session`,
    {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'X-Tenant-Id': tenantId,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        returnUrl: returnUrl || window.location.href
      })
    }
  );
  
  const result = await response.json();
  
  if (result.isSuccessful) {
    // Redirect to Stripe Customer Portal
    window.location.href = result.data.url;
  }
}

// React component
export function ManageSubscriptionsButton({ customerId }) {
  const handleClick = () => {
    createCustomerPortalSession(customerId);
  };
  
  return (
    <button onClick={handleClick}>
      Manage Subscriptions
    </button>
  );
}
```

## Error Handling

All API responses follow a consistent error format:

```json
{
  "isSuccessful": false,
  "data": null,
  "error": {
    "code": "INSUFFICIENT_FUNDS",
    "message": "The payment was declined due to insufficient funds",
    "details": {
      "stripe_error_code": "card_declined"
    },
    "correlationId": "550e8400-e29b-41d4-a716-446655440000"
  }
}
```

### Handling Errors in Code

```javascript
try {
  const response = await apiClient.post('/payments/intent', {
    customerId,
    amount,
    currency
  });
  
  return response.data;
} catch (error) {
  if (error.response) {
    const { error: apiError } = error.response.data;
    
    // Handle specific error codes
    switch (apiError.code) {
      case 'INSUFFICIENT_FUNDS':
        showError('Your card has insufficient funds');
        break;
        
      case 'CARD_DECLINED':
        showError('Your card was declined. Please try another payment method.');
        break;
        
      case 'AUTHENTICATION_REQUIRED':
        showError('Additional verification required. Please check your email.');
        break;
        
      default:
        showError(apiError.message);
    }
    
    // Log for debugging
    console.error(`API Error [${apiError.code}]:`, apiError);
  } else {
    showError('Network error. Please try again.');
  }
}
```

## Rate Limits

API requests are rate-limited per user/API key:

| Role | Limit | Window |
|------|-------|--------|
| API Key | 200 req/min | 1 minute |
| Admin | 500 req/min | 1 minute |
| Manager | 300 req/min | 1 minute |

When rate limited, responses include headers:
```
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1708970460
Retry-After: 60
```

### Handling Rate Limits

```javascript
async function makeRequestWithRetry(url, options, maxRetries = 3) {
  for (let attempt = 1; attempt <= maxRetries; attempt++) {
    try {
      const response = await fetch(url, options);
      
      if (response.status === 429) {
        const retryAfter = parseInt(response.headers.get('Retry-After') || '60');
        
        if (attempt < maxRetries) {
          console.log(`Rate limited. Retrying in ${retryAfter}s...`);
          await new Promise(resolve => setTimeout(resolve, retryAfter * 1000));
          continue;
        }
      }
      
      return response;
    } catch (error) {
      if (attempt === maxRetries) throw error;
    }
  }
}
```

## Complete Integration Example

### Full Payment Flow

```typescript
import React, { useState } from 'react';

interface PaymentFlowProps {
  tenantId: string;
  token: string;
}

export function PaymentFlow({ tenantId, token }: PaymentFlowProps) {
  const [customer, setCustomer] = useState(null);
  const [checkout, setCheckout] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  
  // Step 1: Create customer
  const createCustomer = async (email: string, name: string) => {
    setLoading(true);
    setError(null);
    
    try {
      const response = await fetch('/api/v1/customers', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'X-Tenant-Id': tenantId,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ email, name })
      });
      
      const result = await response.json();
      
      if (result.isSuccessful) {
        setCustomer(result.data);
      } else {
        setError(result.error.message);
      }
    } catch (err) {
      setError('Failed to create customer');
    } finally {
      setLoading(false);
    }
  };
  
  // Step 2: Create checkout
  const createCheckout = async (priceId: string) => {
    if (!customer) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const response = await fetch('/api/v1/payments/checkout', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'X-Tenant-Id': tenantId,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          customerId: customer.id,
          lineItems: [{ priceId, quantity: 1 }],
          successUrl: `${window.location.origin}/success`,
          cancelUrl: `${window.location.origin}/cancel`
        })
      });
      
      const result = await response.json();
      
      if (result.isSuccessful) {
        setCheckout(result.data);
        window.location.href = result.data.url;
      } else {
        setError(result.error.message);
      }
    } catch (err) {
      setError('Failed to create checkout');
    } finally {
      setLoading(false);
    }
  };
  
  return (
    <div>
      {error && <div className="error">{error}</div>}
      
      <button 
        onClick={() => createCustomer('john@example.com', 'John Doe')}
        disabled={loading || !!customer}
      >
        {customer ? 'Customer Created' : 'Create Customer'}
      </button>
      
      {customer && (
        <button 
          onClick={() => createCheckout('price_1234567890')}
          disabled={loading}
        >
          {loading ? 'Processing...' : 'Proceed to Checkout'}
        </button>
      )}
    </div>
  );
}
```

## Best Practices

1. **Store IDs safely**: Keep customer and subscription IDs in secure database
2. **Verify webhooks**: Always verify HMAC signatures on webhook events
3. **Idempotency**: Use X-Idempotency-Key for requests that can be retried
4. **Error handling**: Implement proper error handling for all API calls
5. **Rate limiting**: Implement exponential backoff for rate-limited responses
6. **Logging**: Log all webhook events for debugging and auditing
7. **Testing**: Use Stripe's test keys (pk_test_, sk_test_) for development
8. **Security**: Never expose API keys or signing secrets in frontend code

---

Last Updated: February 26, 2026
