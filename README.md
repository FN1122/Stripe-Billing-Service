# Stripe Billing Service

A comprehensive multi-tenant payment integration and subscription billing service built with ASP.NET Core 9, React 18, and Stripe APIs.

## Overview

The Stripe Billing Service is an enterprise-grade solution for managing payments, subscriptions, invoices, and customer data across multiple tenants. It provides a complete billing infrastructure with real-time analytics, webhook management, and a modern React-based dashboard.

### Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         React 19 Frontend                        │
│         (TypeScript + Vite + React Bootstrap + SCSS)            │
└────────────────────────┬────────────────────────────────────────┘
                         │ (REST API + WebSocket)
┌────────────────────────▼────────────────────────────────────────┐
│                    ASP.NET Core 9 API                            │
│        (Multi-tenant + Authentication + Business Logic)          │
├──────────────────────────────────────────────────────────────────┤
│  • Stripe Integration (Payment Intents, Subscriptions, Webhooks) │
│  • Role-Based Access Control                                      │
│  • HMAC-SHA256 Signing for Webhooks                              │
│  • Background Services (Webhook Dispatch, Retries)               │
│  • SignalR Real-Time Updates                                     │
│  • Rate Limiting Middleware                                      │
└──────────────────────────────────────────────────────────────────┘
                         │
        ┌────────────────┴────────────────┐
        │                                 │
┌───────▼────────┐            ┌─────────▼──────────┐
│   SQL Server   │            │  Stripe Platform   │
│   (Database)   │            │  (Payments API)    │
└────────────────┘            └────────────────────┘
        │
┌───────▼────────┐
│   Seq Logging  │
│  (Structured)  │
└────────────────┘
```

## Key Features

### Payment Processing
- **Checkout Sessions**: Create and manage Stripe Checkout sessions
- **Payment Intents**: Direct payment processing with intent confirmation
- **Payment Methods**: Tokenize and manage customer payment methods
- **Refunds**: Full and partial refund processing
- **Disputes**: Track and manage charge disputes

### Subscription Management
- **Subscription Plans**: Create and manage billing plans
- **Customer Subscriptions**: Subscribe/unsubscribe customers to plans
- **Subscription Lifecycle**: Track creation, updates, renewal, and cancellation
- **Billing Cycles**: Automated recurring billing
- **Usage-Based Billing**: Support for metered subscriptions

### Customer Management
- **Customer Profiles**: Create and manage customer records
- **Multi-Tenant Isolation**: Complete data isolation between tenants
- **Customer Portal**: Self-service portal for customers to manage subscriptions
- **Metadata Storage**: Store custom data with customers and payments

### Invoicing & Analytics
- **Invoice Generation**: Automatic invoice creation for payments
- **Invoice Management**: Draft, finalize, and send invoices
- **Revenue Analytics**: MRR, ARR, Churn Rate, Customer Lifetime Value
- **Real-Time Dashboard**: Live analytics via SignalR
- **Export Capabilities**: Report generation and data export

### Webhook System
- **Inbound Webhooks**: Receive events from Stripe platform
- **Outbound Webhooks**: Dispatch events to client applications
- **HMAC-SHA256 Signing**: Secure webhook verification
- **Retry Logic**: Automatic retry with exponential backoff (5 attempts)
- **Webhook Delivery Tracking**: Monitor webhook status and logs

### Security & Authentication
- **JWT Tokens**: Token-based API authentication
- **API Keys**: For server-to-server integration
- **HMAC-SHA256**: Webhook signature verification
- **Role-Based Access Control**: SuperAdmin, Admin, Manager, Viewer roles
- **Multi-Tenant Isolation**: Complete data separation between tenants
- **Rate Limiting**: Prevent abuse with middleware-based rate limiting
- **Idempotency**: Support for idempotent requests

## Tech Stack

### Backend
- **Framework**: ASP.NET Core 9
- **Database**: SQL Server 2022
- **Authentication**: JWT (System.IdentityModel.Tokens.Jwt)
- **Real-time**: SignalR
- **Stripe Integration**: Stripe.net
- **Logging**: Serilog + Seq
- **ORM**: Entity Framework Core
- **Background Services**: Hosted Services
- **Rate Limiting**: Custom Middleware

### Frontend
- **Framework**: React 19
- **Language**: TypeScript
- **Build Tool**: Vite 7
- **UI Framework**: React Bootstrap + SCSS
- **HTTP Client**: Axios
- **State Management**: Context API
- **Real-time**: SignalR Client
- **Charts**: Chart.js + react-chartjs-2
- **Icons**: Lucide React
- **Forms**: React Hook Form + Zod
- **Date Handling**: date-fns

### DevOps
- **Containerization**: Docker & Docker Compose
- **Orchestration**: Docker Compose (local development)
- **Logging**: Seq (structured logging)
- **Database**: SQL Server in Docker

## Quick Start

### Prerequisites
- .NET 9 SDK
- Node.js 20+ and npm
- Docker and Docker Compose
- Stripe account with API keys
- SQL Server (or use Docker)

### Clone Repository
```bash
git clone https://github.com/yourusername/stripe-billing-service.git
cd stripe-billing-service
```

### Docker Compose (Recommended)
```bash
# Copy environment file
cp .env.example .env

# Update .env with your Stripe keys and settings
# STRIPE_PUBLIC_KEY=pk_test_...
# STRIPE_SECRET_KEY=sk_test_...

# Start all services
docker-compose up -d

# Wait for services to start (30-60 seconds)
docker-compose logs -f api
```

Access the application:
- **Frontend**: http://localhost:3000
- **API**: http://localhost:58492
- **Seq Logs**: http://localhost:5341

### Manual Setup

#### Backend Setup
```bash
cd backend

# Restore NuGet packages
dotnet restore

# Update database
dotnet ef database update

# Run API server
dotnet run --configuration Development
```

Backend runs on `http://localhost:58492`

#### Frontend Setup
```bash
cd frontend

# Install dependencies
npm install

# Start development server
npm run dev
```

Frontend runs on `http://localhost:5173`

## Default Credentials

### Tenant 1 — TechFlow Solutions

| Role | Email | Password | Name |
|------|-------|----------|------|
| SuperAdmin | superadmin@techflow.com | Demo@123 | Muhammad Nasir |
| Admin | sarah@techflow.com | Demo@123 | Sarah Mitchell |
| Manager | ahmed@techflow.com | Demo@123 | Ahmed Khalil |
| Viewer | viewer@techflow.com | Demo@123 | Lisa Chen |

### Tenant 2 — Sunrise Dental Group

| Role | Email | Password | Name |
|------|-------|----------|------|
| Admin | admin@sunrisedental.com | Demo@123 | Dr. Emily Nguyen |
| Manager | billing@sunrisedental.com | Demo@123 | Rachel Torres |

> **Note**: These are default credentials for development only. Change passwords in production.

## Demo Data Overview

The application seeds realistic demo data on startup, including:

- **2 Tenants** — TechFlow Solutions (SaaS/Tech) and Sunrise Dental Group (Healthcare)
- **6 Users** — Across all 4 roles (SuperAdmin, Admin, Manager, Viewer)
- **15 Customers** — 12 global B2B customers (TechFlow) + 3 patients (Sunrise Dental)
- **7 Subscription Plans** — Starter ($9), Professional ($49), Business ($149), Enterprise ($499), Annual ($470), Standard Care ($29), Premium Care ($79)
- **14 Active Subscriptions** — Including active, trialing, past_due, and canceled states
- **75+ Payment Transactions** — 6 months of recurring payments, one-time charges, and a failed payment
- **70+ Invoices** — Paid and open invoices with tax calculations
- **3 Refunds** — Succeeded, pending, and partial refunds
- **4 Coupons** — Percentage and fixed-amount discounts with promotion codes (WELCOME20, ANNUAL50, PARTNER15, BFRIDAY30)
- **3 Webhook Subscriptions** — With delivery tracking (delivered and failed)
- **4 Dunning Steps** — Automated payment recovery workflow
- **Tax Configuration** — Stripe Tax with EU reverse-charge exemptions
- **6 Email Templates** — Welcome, invoice, payment failure, retry, and cancellation
- **3 Stripe Connect Accounts** — Express and standard accounts with fee configuration
- **Usage Records & Meter Events** — 30 days of API usage metering
- **Audit Logs, API Call Logs, Settings, Rate Limits** — Fully configured per tenant

## API Authentication

### JWT Token Authentication
```bash
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "superadmin@techflow.com",
  "password": "Demo@123"
}

# Response
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 86400,
  "user": { ... }
}

# Use in subsequent requests
Authorization: Bearer {token}
```

### API Key Authentication
```bash
X-Api-Key: your_api_key_here
X-Tenant-Id: tenant_id
```

### HMAC-SHA256 Webhook Verification
```
X-Signature: sha256=<HMAC-SHA256 hash of payload>
X-Timestamp: <request timestamp>
```

See [INTEGRATION-GUIDE.md](INTEGRATION-GUIDE.md) for detailed examples.

## Project Structure

```
stripe-billing-service/
├── backend/
│   ├── StripeBillingService.API/
│   │   ├── Controllers/
│   │   ├── Services/
│   │   ├── Models/
│   │   ├── Middleware/
│   │   ├── Extensions/
│   │   └── appsettings.json
│   ├── StripeBillingService.Core/
│   │   ├── Entities/
│   │   ├── Repositories/
│   │   ├── Interfaces/
│   │   └── Constants/
│   ├── StripeBillingService.Infrastructure/
│   │   ├── Data/
│   │   ├── Migrations/
│   │   ├── Services/
│   │   └── BackgroundServices/
│   └── StripeBillingService.sln
├── frontend/
│   ├── src/
│   │   ├── components/
│   │   ├── pages/
│   │   ├── hooks/
│   │   ├── services/
│   │   ├── store/
│   │   ├── types/
│   │   ├── utils/
│   │   └── main.tsx
│   ├── public/
│   ├── vite.config.ts
│   ├── tsconfig.json
│   ├── tailwind.config.js
│   └── package.json
├── docs/
│   ├── README.md (this file)
│   ├── ARCHITECTURE.md
│   ├── API-DOCS.md
│   ├── WEBHOOK-EVENTS.md
│   ├── SETUP.md
│   └── INTEGRATION-GUIDE.md
├── docker-compose.yml
└── .env.example
```

## Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `STRIPE_PUBLIC_KEY` | Stripe publishable key | `pk_test_...` |
| `STRIPE_SECRET_KEY` | Stripe secret key | `sk_test_...` |
| `STRIPE_WEBHOOK_SECRET` | Webhook signing secret | `whsec_...` |
| `JWT_SECRET_KEY` | JWT signing key (min 32 chars) | `your-secret-key-min-32-chars` |
| `DATABASE_CONNECTION_STRING` | SQL Server connection | `Server=sqlserver;Database=StripeBilling;...` |
| `API_BASE_URL` | API server URL | `http://localhost:58492` |
| `FRONTEND_URL` | Frontend URL | `http://localhost:3000` |
| `SEQ_SERVER_URL` | Seq logging server | `http://localhost:5341` |
| `CORS_ALLOWED_ORIGINS` | CORS allowed origins | `http://localhost:3000,http://localhost:3001` |
| `RATE_LIMIT_REQUESTS_PER_MINUTE` | Rate limit threshold | `60` |
| `WEBHOOK_SIGNATURE_ALGORITHM` | HMAC algorithm | `HMACSHA256` |

See [SETUP.md](SETUP.md) for complete environment configuration.

## Common Tasks

### Create a Checkout Session
```typescript
const response = await fetch('/api/v1/payments/checkout', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'X-Tenant-Id': tenantId,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    customerId: 'cus_123',
    planId: 'plan_456',
    quantity: 1
  })
});
```

### Subscribe a Customer
```typescript
const response = await fetch('/api/v1/subscriptions', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'X-Tenant-Id': tenantId,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    customerId: 'cus_123',
    planId: 'plan_456'
  })
});
```

### Handle Webhooks
```typescript
// Inbound webhook from Stripe
POST /api/v1/webhooks/stripe
Content-Type: application/json
Stripe-Signature: {signature}

{
  "type": "payment_intent.succeeded",
  "data": { ... }
}

// Outbound webhook to client
POST {client_webhook_url}
X-Signature: sha256={hmac_signature}
X-Timestamp: {timestamp}

{
  "event": "payment.completed",
  "data": { ... }
}
```

For more examples, see [INTEGRATION-GUIDE.md](INTEGRATION-GUIDE.md).

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

Please ensure:
- Code follows the project's coding standards
- Tests are updated or added
- Documentation is updated
- Commits are descriptive

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For issues, questions, or suggestions:
- Open an issue on GitHub
- Check existing documentation in `/docs`
- Review the [SETUP.md](SETUP.md) for troubleshooting

## Roadmap

- [ ] Advanced subscription management (proration, downgrades)
- [ ] Multi-currency support
- [ ] Custom invoice templates
- [ ] Advanced analytics and reporting
- [ ] Mobile app integration
- [ ] Audit logging
- [ ] PCI compliance improvements
- [ ] Integration with accounting software (QuickBooks, Xero)

---

Last Updated: March 1, 2026
