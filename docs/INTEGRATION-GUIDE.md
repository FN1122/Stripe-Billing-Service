# Client Integration Guide — Stripe Billing Service

How to integrate your application with the Stripe Billing Service.

---

## Overview

Your application communicates with the billing service via:
1. **API calls** (authenticated with API Key + HMAC) → to create payments, subscriptions, customers
2. **Webhook callbacks** (signed with HMAC) → to receive results asynchronously
3. **User portal** (JWT passthrough) → to give end users access to their billing data

You never handle Stripe directly. The billing service manages all Stripe interactions.

---

## Step 1: Get Your Credentials

After tenant onboarding, you receive:

| Credential | Format | Purpose |
|-----------|--------|---------|
| Public API Key | `pk_live_xxxxxxxx` | Identify your tenant in requests |
| Secret API Key | `sk_live_xxxxxxxx` | Sign requests (HMAC-SHA256) |
| Webhook Secret | `whsec_xxxxxxxx` | Verify inbound webhook callbacks |
| JWT Secret | `jwt_xxxxxxxx` | Sign user portal tokens |

**Store these securely.** Never expose the Secret API Key or Webhook Secret in client-side code.

---

## Step 2: Making API Calls

### Required Headers
Every API request must include:

```
X-Api-Key: pk_live_your_public_key
X-Signature: HMAC-SHA256(body + "|" + timestamp, secret_key)
X-Timestamp: 1709000000 (current Unix timestamp)
X-Idempotency-Key: unique-uuid-per-request
Content-Type: application/json
```

### Generating the HMAC Signature

```python
# Python
import hmac, hashlib, time, uuid, json, requests

API_KEY = "pk_live_xxx"
SECRET_KEY = "sk_live_xxx"
BASE_URL = "https://billing.yourdomain.com/api/v1"

def call_api(method, path, body=None):
    timestamp = str(int(time.time()))
    body_str = json.dumps(body) if body else ""
    
    # Generate HMAC signature
    message = body_str + "|" + timestamp
    signature = hmac.new(
        SECRET_KEY.encode(), message.encode(), hashlib.sha256
    ).hexdigest()
    
    headers = {
        "X-Api-Key": API_KEY,
        "X-Signature": signature,
        "X-Timestamp": timestamp,
        "X-Idempotency-Key": str(uuid.uuid4()),
        "Content-Type": "application/json"
    }
    
    response = requests.request(method, BASE_URL + path, json=body, headers=headers)
    return response.json()
```

```javascript
// Node.js
const crypto = require('crypto');
const axios = require('axios');
const { v4: uuidv4 } = require('uuid');

const API_KEY = 'pk_live_xxx';
const SECRET_KEY = 'sk_live_xxx';
const BASE_URL = 'https://billing.yourdomain.com/api/v1';

async function callApi(method, path, body = null) {
  const timestamp = Math.floor(Date.now() / 1000).toString();
  const bodyStr = body ? JSON.stringify(body) : '';
  
  // Generate HMAC signature
  const message = bodyStr + '|' + timestamp;
  const signature = crypto
    .createHmac('sha256', SECRET_KEY)
    .update(message)
    .digest('hex');
  
  const response = await axios({
    method,
    url: BASE_URL + path,
    data: body,
    headers: {
      'X-Api-Key': API_KEY,
      'X-Signature': signature,
      'X-Timestamp': timestamp,
      'X-Idempotency-Key': uuidv4(),
      'Content-Type': 'application/json'
    }
  });
  
  return response.data;
}
```

```csharp
// C#
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public class BillingClient
{
    private readonly string _apiKey;
    private readonly string _secretKey;
    private readonly HttpClient _httpClient;

    public BillingClient(string apiKey, string secretKey, string baseUrl)
    {
        _apiKey = apiKey;
        _secretKey = secretKey;
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<T> CallAsync<T>(HttpMethod method, string path, object body = null)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var bodyStr = body != null ? JsonSerializer.Serialize(body) : "";
        
        // Generate HMAC signature
        var message = bodyStr + "|" + timestamp;
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        var signature = BitConverter.ToString(hash).Replace("-", "").ToLower();
        
        var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-Api-Key", _apiKey);
        request.Headers.Add("X-Signature", signature);
        request.Headers.Add("X-Timestamp", timestamp);
        request.Headers.Add("X-Idempotency-Key", Guid.NewGuid().ToString());
        
        if (body != null)
            request.Content = new StringContent(bodyStr, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json);
    }
}
```

---

## Step 3: Common API Flows

### Create a Customer
```python
result = call_api("POST", "/customers", {
    "externalReferenceId": "your-user-123",  # Your app's user ID
    "email": "customer@example.com",
    "name": "John Doe",
    "currency": "usd"
})
customer_id = result["data"]["id"]
```

### Create a Checkout Session (One-Time Payment)
```python
result = call_api("POST", "/payments/checkout", {
    "customerId": customer_id,
    "lineItems": [
        {
            "name": "Premium Widget",
            "description": "High-quality widget",
            "amount": 49.99,
            "currency": "usd",
            "quantity": 1
        }
    ],
    "successUrl": "https://yourapp.com/payment/success",
    "cancelUrl": "https://yourapp.com/payment/cancel",
    "mode": "payment"
})
checkout_url = result["data"]["checkoutUrl"]
# Redirect customer to checkout_url
```

### Create a Subscription
```python
result = call_api("POST", "/subscriptions", {
    "customerId": customer_id,
    "planId": "plan-guid-from-admin-dashboard",
    "quantity": 1,
    "trialDays": 14  # Optional: override plan default
})
subscription_id = result["data"]["id"]
```

### Cancel a Subscription
```python
result = call_api("DELETE", f"/subscriptions/{subscription_id}", {
    "cancelAtPeriodEnd": True,  # Cancel at end of billing period
    "reason": "too_expensive"
})
```

### Preview Plan Change (Proration)
```python
result = call_api("GET", f"/subscriptions/{subscription_id}/preview?newPlanId={new_plan_id}")
# Returns: current plan, new plan, prorated amount, effective date
```

---

## Step 4: Receiving Webhook Callbacks

### Register Your Callback URL
In the admin dashboard: Settings → General → Webhook Callback URL

Or via API (admin JWT required):
```
POST /api/v1/webhooks/subscriptions
{
  "url": "https://yourapp.com/webhooks/billing",
  "events": ["payment.*", "subscription.*", "refund.*"],
  "isActive": true
}
```

### Handle Incoming Webhooks
```python
# Your webhook endpoint
from flask import Flask, request
import hmac, hashlib

WEBHOOK_SECRET = "whsec_xxx"

@app.route("/webhooks/billing", methods=["POST"])
def handle_billing_webhook():
    payload = request.get_data(as_text=True)
    signature = request.headers.get("X-Webhook-Signature")
    timestamp = request.headers.get("X-Webhook-Timestamp")
    event_id = request.headers.get("X-Webhook-ID")
    
    # 1. Verify signature
    message = payload + "|" + timestamp
    expected = "sha256=" + hmac.new(
        WEBHOOK_SECRET.encode(), message.encode(), hashlib.sha256
    ).hexdigest()
    
    if not hmac.compare_digest(expected, signature):
        return "Invalid signature", 401
    
    # 2. Check timestamp (5-minute window)
    import time
    if abs(time.time() - int(timestamp)) > 300:
        return "Timestamp expired", 401
    
    # 3. Check for duplicate (idempotency)
    if already_processed(event_id):
        return "OK", 200
    
    # 4. Process event
    event = json.loads(payload)
    
    if event["type"] == "payment.completed":
        fulfill_order(event["data"])
    elif event["type"] == "subscription.activated":
        grant_access(event["data"]["externalReferenceId"], event["data"]["planName"])
    elif event["type"] == "subscription.cancelled":
        revoke_access(event["data"]["externalReferenceId"])
    elif event["type"] == "subscription.payment_failed":
        show_payment_banner(event["data"]["externalReferenceId"])
    elif event["type"] == "refund.processed":
        adjust_records(event["data"])
    
    # 5. Return 200 (acknowledges receipt — prevents retry)
    return "OK", 200
```

---

## Step 5: User Portal (JWT Passthrough)

### Generate Portal JWT
When your user wants to view their billing info, generate a short-lived JWT:

```python
import jwt
import time

JWT_SECRET = "jwt_xxx"  # From your tenant credentials

def generate_portal_token(user):
    payload = {
        "tenant_id": "your-tenant-id",
        "customer_reference_id": user.id,  # Your app's user ID
        "email": user.email,
        "name": user.name,
        "exp": int(time.time()) + 900  # 15 minutes
    }
    return jwt.encode(payload, JWT_SECRET, algorithm="HS256")
```

### Redirect to Portal
```html
<!-- Option A: Redirect -->
<a href="https://billing.yourdomain.com/portal?token=JWT_TOKEN">
  View Billing
</a>

<!-- Option B: Iframe -->
<iframe src="https://billing.yourdomain.com/portal?token=JWT_TOKEN"
        width="100%" height="600" frameborder="0"></iframe>
```

The portal automatically shows only that customer's data (transactions, subscriptions, invoices, payment methods).

---

## Response Format

All API responses use the GatewayResponseWrapper format:

### Success
```json
{
  "isValid": true,
  "message": "Payment created successfully",
  "data": { ... },
  "statusCode": 200,
  "errors": null,
  "timestamp": "2026-02-26T10:30:00Z"
}
```

### Error
```json
{
  "isValid": false,
  "message": "Validation failed",
  "data": null,
  "statusCode": 400,
  "errors": [
    { "field": "amount", "message": "Amount must be greater than 0" }
  ],
  "timestamp": "2026-02-26T10:30:00Z"
}
```

### Paginated
```json
{
  "isValid": true,
  "message": "Success",
  "data": {
    "items": [ ... ],
    "currentPage": 1,
    "pageSize": 20,
    "totalRecords": 150,
    "totalPages": 8
  },
  "statusCode": 200,
  "timestamp": "2026-02-26T10:30:00Z"
}
```

---

## Error Codes

| HTTP Code | Meaning | Common Cause |
|-----------|---------|-------------|
| 400 | Bad Request | Validation failed, missing required fields |
| 401 | Unauthorized | Invalid API key, invalid HMAC, expired timestamp |
| 403 | Forbidden | Tenant suspended, insufficient permissions |
| 404 | Not Found | Resource doesn't exist or belongs to another tenant |
| 409 | Conflict | Duplicate idempotency key, duplicate customer |
| 429 | Rate Limited | Too many requests (check Retry-After header) |
| 500 | Server Error | Internal error (contact support) |
| 502 | Stripe Error | Stripe API returned an error |

---

## Best Practices

1. **Always use idempotency keys** — Prevents duplicate charges on network retries
2. **Verify webhook signatures** — Prevents spoofed events
3. **Handle webhook retries** — Return 200 quickly; process asynchronously if needed
4. **Deduplicate webhooks** — Check `X-Webhook-ID` before processing
5. **Use externalReferenceId** — Map billing customers to your app's users
6. **Store customer IDs** — Cache the billing service's customer ID for faster lookups
7. **Handle all subscription states** — active, trialing, past_due, canceled, paused
8. **Test with Stripe test cards** — 4242... for success, 4000...0002 for decline
