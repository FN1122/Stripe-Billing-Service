# Stripe Billing Service

## Multi-Tenant Stripe Payment Integration & Subscription Billing Microservice

A production-ready, multi-tenant billing microservice that handles all Stripe payment and subscription operations for any application. Integrates via API + webhook callbacks. Includes three role-based React dashboards.

---

## Quick Overview

**What it does:** Your application sends billing requests via API → This service handles everything with Stripe → Sends results back to your app via signed webhooks. Three React dashboards provide full visibility.

**How it works:**
1. Client app sends API request (e.g., create subscription) with API Key + HMAC signature
2. Billing service processes with Stripe using tenant's encrypted credentials
3. Stripe events are received and processed (inbound webhook)
4. Results are sent back to client app via signed callback (outbound webhook)
5. Dashboards update in real-time via SignalR

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 9 + Entity Framework Core 9 |
| Stripe | Stripe.NET (official SDK) |
| Frontend | React 18 + TypeScript + Vite |
| UI | Bootstrap 5 + SCSS + lucide-react |
| Charts | Chart.js + react-chartjs-2 |
| Real-Time | SignalR |
| Database | SQL Server 2022 |
| Auth | JWT + API Key + HMAC-SHA256 (7 auth flows) |
| Deployment | Docker Compose |

---

## Key Features

- **Multi-Tenant:** One deployment serves unlimited clients with isolated data
- **One-Time Payments:** Stripe Checkout + Payment Intents
- **Subscription Billing:** Create, upgrade, downgrade, cancel, pause, resume, trials, dunning
- **Double Webhook System:** Stripe → Service → Client App (both authenticated)
- **Invoice Generation:** Auto-generated with tenant branding + PDF
- **Refund Management:** Full/partial with approval workflow
- **Revenue Analytics:** MRR, ARR, churn, LTV with Chart.js visualizations
- **3 Dashboards:** End User Portal, Client Admin, Super Admin
- **7 Auth Flows:** API Key+HMAC, webhook signing, JWT passthrough, admin login, 2FA, Stripe auth
- **Real-Time:** SignalR for live dashboard updates
- **Audit Trail:** Immutable logging of all admin actions
- **70+ API Endpoints** across 5 auth groups

---

## Quick Start

### Prerequisites
- .NET 9 SDK
- Node.js 20 LTS
- SQL Server 2022 (or Docker)
- Stripe CLI

### Backend
```bash
cd backend
dotnet restore
dotnet ef database update -p Core -s WebAPI
dotnet run --project WebAPI
# API runs at https://localhost:5001
# Swagger at https://localhost:5001/swagger
```

### Frontend
```bash
cd frontend
npm install
npm run dev
# Dashboard at http://localhost:5173
```

### Stripe CLI (webhook testing)
```bash
stripe login
stripe listen --forward-to https://localhost:5001/api/v1/webhooks/stripe
```

### Docker (full stack)
```bash
cd docker
docker-compose up -d
# API: http://localhost:5000
# Frontend: http://localhost:3000
# SQL Server: localhost:1433
```

---

## Default Credentials

| Role | Email | Password |
|------|-------|----------|
| Super Admin | admin@billing.io | Admin@123! |

---

## Project Structure

```
03-Stripe-Billing-Service/
├── Stripe_Billing_Development_Plan.md    # Master development plan
├── docs/                                  # Documentation
│   ├── README.md                         # This file
│   ├── FEATURES.md                       # Complete feature list
│   ├── ARCHITECTURE.md                   # System architecture
│   ├── SETUP.md                          # Development setup guide
│   ├── DEPLOYMENT.md                     # Deployment guides
│   ├── API-DOCS.md                       # API reference (70+ endpoints)
│   ├── WEBHOOK-EVENTS.md                 # Webhook event reference
│   ├── INTEGRATION-GUIDE.md              # Client integration guide
│   └── DOCS-INDEX.md                     # Documentation map
├── backend/
│   ├── StripeBilling.sln
│   ├── Core/                             # Business logic, entities, services
│   ├── WebAPI/                           # Controllers, middleware, Program.cs
│   └── Tests/                            # Unit + integration tests
├── frontend/
│   └── src/                              # React 18 + TypeScript
│       ├── api/                          # 18 API modules
│       ├── components/                   # 15 reusable components
│       ├── contexts/                     # Auth, Toast, Sidebar
│       ├── hooks/                        # Custom React hooks
│       ├── layouts/                      # Auth + Dashboard layouts
│       ├── pages/                        # 16 pages
│       ├── routes/                       # Lazy-loaded routes
│       └── types/                        # 14 TypeScript type files
└── docker/                               # Docker Compose deployment
```

---

## Documentation

| Document | Description |
|----------|-------------|
| [FEATURES.md](./FEATURES.md) | All billing features and capabilities |
| [ARCHITECTURE.md](./ARCHITECTURE.md) | System design and patterns |
| [SETUP.md](./SETUP.md) | Local development setup |
| [DEPLOYMENT.md](./DEPLOYMENT.md) | Production deployment |
| [API-DOCS.md](./API-DOCS.md) | Full API reference |
| [WEBHOOK-EVENTS.md](./WEBHOOK-EVENTS.md) | Webhook event reference |
| [INTEGRATION-GUIDE.md](./INTEGRATION-GUIDE.md) | Client app integration |

---

## Pricing Tiers

| Tier | Price | Build Time | Key Features |
|------|-------|-----------|--------------|
| **Starter** | $400 | 3-4 days | One-time payments, basic portal, single tenant |
| **Standard** | $800 | 5-7 days | + Subscriptions, multi-tenant, admin dashboard, refunds |
| **Advanced** | $1,200 | 7-10 days | + Super admin, analytics, 2FA, audit trail, SignalR, 16 pages |

---

## Reference Project

Built using identical patterns from **02-API-Gateway-Microservice**:
- GatewayResponseWrapper, BaseService, BaseRepository, GatewayControllerBase
- All middleware (Tenant, ApiKey, RateLimit, RequestLogging)
- React: api-client, interceptors, contexts, hooks, layouts, common components
- New additions: HMAC auth, Stripe billing, subscription management, revenue analytics

---

*Confidential — Client Deliverable*
