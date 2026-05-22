# Features — Stripe Billing Service

Complete feature list for the multi-tenant Stripe billing microservice.

---

## Core Architecture

### Multi-Tenant System
- Row-level data isolation with TenantId on every table
- Global EF Core query filter prevents cross-tenant access
- Per-tenant Stripe accounts (encrypted credential storage)
- Per-tenant configuration: branding, refund policy, dunning, webhook URLs
- Tenant onboarding with automatic credential generation

### Authentication (7 Flows)
1. **API Key + HMAC-SHA256** — Client app → Billing Service (inbound API calls)
2. **Webhook Signing** — Billing Service → Client App (outbound callbacks)
3. **JWT Passthrough** — End users accessing billing portal
4. **Email + Password** — Client admin dashboard login
5. **Email + Password + 2FA** — Super admin login (TOTP)
6. **Stripe Webhook Signature** — Stripe → Billing Service verification
7. **Stripe API Key** — Billing Service → Stripe API calls

### Response Pattern
- Unified GatewayResponseWrapper<T> on every endpoint
- Consistent error format with status codes and timestamps
- Pagination support (CurrentPage, PageSize, TotalRecords, TotalPages)

---

## Billing Features

### One-Time Payments
- Stripe Checkout Session (hosted payment page)
- Payment Intent (embedded Stripe Elements)
- Multiple payment methods: Cards, Apple Pay, Google Pay, ACH, SEPA
- 3D Secure / Strong Customer Authentication (automatic)
- Idempotency keys prevent duplicate charges
- Transaction logging with status tracking

### Subscription Billing
- Unlimited subscription plans per tenant
- Pricing models: flat-rate, per-seat, tiered
- Billing intervals: monthly, annual, weekly, custom
- Free trials with configurable duration (card required or no-card)
- Plan upgrades/downgrades with proration preview
- Cancellation: immediate or at-period-end with survey
- Pause/resume billing
- Dunning management: configurable retry schedule with customer notifications
- Quantity updates (per-seat billing)
- Coupon and promo code support

### Customer Management
- Customer create/read/update with Stripe sync
- External reference ID mapping (client's own user ID)
- Customer portal session generation
- Payment method management (add, remove, set default)
- Billing info management (address, tax ID)
- Customer detail with subscriptions, transactions, invoices, LTV

### Invoice System
- Automatic generation for subscription cycles and one-time payments
- Per-tenant branding: logo, colors, header/footer
- Line items with product, quantity, price, subtotal
- Tax support (Stripe Tax or manual rates)
- Multi-currency rendering
- PDF generation and download
- Email delivery
- Void and credit note support
- Bulk PDF export

### Refund Management
- Full refund (100% of charge)
- Partial refund (specific amount)
- Prorated refund (unused subscription time)
- Approval workflow: auto-approve below threshold, manual above
- Admin approval queue with approve/reject
- Stripe Refund API integration
- Refund analytics: total, rate, processing time

### Revenue Analytics (Advanced Tier)
- MRR (Monthly Recurring Revenue) with components: New, Expansion, Contraction, Churned
- ARR (Annual Recurring Revenue) projection
- Churn rate (monthly and annual)
- Customer Lifetime Value (LTV)
- Subscription metrics: active, trialing, past_due, cancelled over time
- Payment health: success/fail ratio, recovery rate, failure reasons
- Revenue over time with period selectors (7d, 30d, 90d, 12m)
- Chart.js visualizations: line, bar, doughnut, stacked

---

## Webhook System (Double Webhook)

### Inbound: Stripe → Billing Service (14 Events)
| Event | Action |
|-------|--------|
| checkout.session.completed | Record payment, activate subscription |
| payment_intent.succeeded | Update transaction status |
| payment_intent.payment_failed | Log failure, start dunning |
| invoice.paid | Mark invoice paid, extend subscription |
| invoice.payment_failed | Start retry, alert admin |
| customer.subscription.created | Record subscription |
| customer.subscription.updated | Handle plan change, proration |
| customer.subscription.deleted | Deactivate subscription |
| customer.subscription.trial_will_end | Send trial ending reminder |
| charge.refunded | Process refund, update records |
| charge.dispute.created | Alert admin, freeze orders |
| customer.updated | Sync customer data |
| payment_method.attached | Update payment methods |
| price.updated | Sync pricing changes |

### Outbound: Billing Service → Client App (11 Events)
| Event | Client Should... |
|-------|-----------------|
| payment.completed | Fulfill order, activate feature |
| payment.failed | Show failed UI, prompt card update |
| subscription.activated | Grant subscription features |
| subscription.upgraded | Upgrade feature access |
| subscription.downgraded | Reduce access at period end |
| subscription.cancelled | Revoke access |
| subscription.trial_ending | Prompt to add payment method |
| subscription.payment_failed | Show payment issue banner |
| refund.processed | Adjust user records |
| invoice.generated | Store invoice reference |
| customer.updated | Sync customer changes |

### Delivery Guarantees
- HMAC-SHA256 signed payloads
- At-least-once delivery with deduplication
- Retry schedule: 1m, 5m, 30m, 2h, 8h, 24h (6 retries)
- Dead letter queue after all retries
- Manual retry from admin dashboard

---

## API Endpoints (70+)

### Client API — API Key + HMAC Auth (20 endpoints)
- Payments: checkout, intent, list, get, analytics
- Subscriptions: create, get, update, cancel, pause, resume, preview
- Customers: create, get, update, list, portal-session
- Refunds: create
- Invoices: list, get/pdf

### User Portal — JWT Passthrough (11 endpoints)
- Billing summary, transactions, subscriptions, invoices
- Payment methods: list, add, remove
- Plan changes, cancellation, billing info

### Admin Dashboard — JWT Session (22 endpoints)
- Dashboard: revenue, chart, subscriptions
- Transactions: list, export, failed, retry
- Customers, refunds (approve/reject), invoices
- Plans: CRUD + sync
- Team: list, invite
- Settings, audit log

### Super Admin — JWT + 2FA (10 endpoints)
- Tenants: list, create, get, update, suspend, activate, rotate-keys
- Cross-tenant analytics, system health, platform audit log

### System (8 endpoints)
- Stripe webhook receiver
- Auth: login, register, refresh, me
- Health check, setup

---

## Database Schema (16 Tables)

| Table | Scope | Purpose |
|-------|-------|---------|
| Tenants | Global | Client organizations |
| Users | Per-tenant | Admin dashboard users |
| RefreshTokens | Per-user | JWT refresh token rotation |
| ApiKeys | Per-tenant | Client app API keys |
| Customers | Per-tenant | Stripe customers synced |
| SubscriptionPlans | Per-tenant | Stripe products/prices |
| Subscriptions | Per-tenant | Active subscriptions |
| PaymentTransactions | Per-tenant | All payment records |
| Invoices | Per-tenant | Stripe invoices synced |
| Refunds | Per-tenant | Refund requests + processing |
| WebhookSubscriptions | Per-tenant | Client callback URLs |
| WebhookDeliveries | Per-tenant | Outbound delivery log |
| WebhookEventsInbound | Per-tenant | Stripe events received |
| ApiCallLogs | Per-tenant | API request audit |
| AuditLog | Per-tenant/Global | Admin action audit trail |

---

## React Frontend (16 Pages + 15 Components)

### Pages
1. Login — Email/password auth
2. Dashboard — Revenue MetricCards, Chart, Activity Feed
3. Payments — Transaction list, filters, detail modal
4. Subscriptions — Status badges, filters, plan change, cancel
5. Customers — Search, detail view with sub-tables
6. Invoices — Status filter, PDF download, void, send
7. Refunds — Pending queue (approve/reject) + history
8. Plans — Card grid, create/edit modal, Stripe sync
9. Revenue Analytics — MRR/ARR/Churn charts, payment health
10. API Keys — Generate, revoke, copy (from reference)
11. Connections — Stripe setup, test connection (from reference)
12. Logs — API call history with filters (from reference)
13. Webhooks — Subscriptions + deliveries + retry (from reference)
14. Users — Team management, invite, roles (from reference)
15. Settings — General, branding, billing, Stripe tabs
16. Audit Log — Action history with before/after diff

### Reusable Components
From reference (9): DataTable, MetricCard, StatusBadge, SearchInput, LoadingSkeleton, JsonViewer, CodeSnippet, EmptyState, ConfirmDialog

New (6): RevenueChart, SubscriptionBadge, RefundBadge, InvoiceViewer, PlanCard, WebhookStatusBadge

---

## Security

- PCI-DSS compliant (card data never touches server — Stripe handles)
- AES-256 encryption for stored Stripe credentials
- SHA-256 hashed API keys (never stored in plain text)
- HMAC-SHA256 request signatures with 5-minute timestamp window
- Idempotency keys with 24h TTL (prevent duplicate charges)
- Rate limiting per API key
- Global EF Core tenant isolation filter
- Immutable audit trail for all admin actions
- CORS configured for SignalR + dashboard origins
- JWT with short expiry (1h) + refresh token rotation
