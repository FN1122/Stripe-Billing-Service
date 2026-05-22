# Stripe Billing Service - Documentation Index

Welcome to the comprehensive documentation for the Stripe Billing Service. This guide helps you navigate all available documentation.

## Documentation Files

### 1. README.md (Start Here)
**Overview and Quick Start Guide**

- Project description and architecture overview
- Key features list
- Technology stack details
- Quick start with Docker Compose
- Manual setup instructions
- Default credentials
- Environment variables reference
- Common tasks and code examples

**Best for**: New users, project overview, quick start

[Read README.md](README.md)

---

### 2. ARCHITECTURE.md
**Detailed System Design and Architecture**

- Complete system architecture diagram
- Multi-tenant architecture explanation
- Authentication & authorization flows (JWT + API Key + HMAC)
- Webhook system (inbound from Stripe + outbound to clients)
- Service layer patterns and dependency injection
- Database schema with entity relationships
- Background services (webhook dispatch, retries)
- Rate limiting strategy
- Error handling patterns
- Real-time updates via SignalR
- Caching strategy
- Security considerations

**Best for**: Developers, architects, understanding system design

[Read ARCHITECTURE.md](ARCHITECTURE.md)

---

### 3. SETUP.md
**Installation and Configuration Guide**

- Prerequisites and system requirements
- Backend setup step-by-step
- Frontend setup step-by-step
- Database migration instructions
- Docker Compose setup (recommended)
- Stripe webhook configuration (Stripe CLI)
- Environment variables complete reference
- Running in development mode
- Troubleshooting common issues

**Best for**: Setting up the project locally, troubleshooting setup issues

[Read SETUP.md](SETUP.md)

---

### 4. API-DOCS.md
**Complete API Reference**

- Base URL and versioning
- Authentication headers (JWT, API Key, HMAC)
- All endpoint documentation:
  - Auth endpoints (/api/v1/auth/*)
  - Payment endpoints (/api/v1/payments/*)
  - Customer endpoints (/api/v1/customers/*)
  - Subscription endpoints (/api/v1/subscriptions/*)
  - Plan endpoints (/api/v1/plans/*)
  - Refund endpoints (/api/v1/refunds/*)
  - Invoice endpoints (/api/v1/invoices/*)
  - Webhook endpoints (/api/v1/webhooks/*)
  - Dashboard endpoints (/api/v1/dashboard/*)
  - Analytics endpoints (/api/v1/analytics/*)
  - Super Admin endpoints (/api/v1/superadmin/*)
- Error response formats
- Pagination format
- Request examples (cURL, JavaScript)

**Best for**: API integration, endpoint reference, request/response examples

[Read API-DOCS.md](API-DOCS.md)

---

### 5. WEBHOOK-EVENTS.md
**Webhook Events Reference**

- Inbound events from Stripe (14 event types)
  - checkout.session.completed
  - payment_intent events
  - customer.subscription events
  - invoice events
  - charge events
  - customer events
- Outbound events to clients (11 event types)
  - payment events
  - subscription events
  - invoice events
  - refund events
  - customer events
- Webhook subscription setup
- HMAC-SHA256 signature verification
- Retry policy (5 retries over 24+ hours)
- Webhook delivery statuses
- Monitoring webhook health

**Best for**: Webhook implementation, event handling, webhook monitoring

[Read WEBHOOK-EVENTS.md](WEBHOOK-EVENTS.md)

---

### 6. INTEGRATION-GUIDE.md
**Client Integration Guide**

- Getting started prerequisites
- API Key authentication
- HMAC signature calculation with code examples
  - C# example
  - JavaScript example
  - Python example
- Creating checkout sessions
- Managing subscriptions
  - Create, cancel, update
- Handling webhooks
  - Registration
  - Event processing
  - Full webhook flow example
- Customer portal integration
- Error handling
- Rate limiting strategies
- Complete integration examples
- Best practices

**Best for**: Integrating your application, code examples, best practices

[Read INTEGRATION-GUIDE.md](INTEGRATION-GUIDE.md)

---

## Quick Navigation

### By Role

**Project Manager / Non-Technical**
1. Start with [README.md](README.md) - Project overview
2. Review [ARCHITECTURE.md](ARCHITECTURE.md) - System design overview
3. Check [SETUP.md](SETUP.md) - Getting started

**Backend Developer**
1. [SETUP.md](SETUP.md) - Local setup
2. [ARCHITECTURE.md](ARCHITECTURE.md) - System design
3. [API-DOCS.md](API-DOCS.md) - API implementation
4. [WEBHOOK-EVENTS.md](WEBHOOK-EVENTS.md) - Webhook handling

**Frontend Developer**
1. [README.md](README.md) - Project overview
2. [SETUP.md](SETUP.md) - Local setup (frontend section)
3. [API-DOCS.md](API-DOCS.md) - API endpoints
4. [INTEGRATION-GUIDE.md](INTEGRATION-GUIDE.md) - Integration examples

**DevOps Engineer**
1. [SETUP.md](SETUP.md) - Docker setup section
2. [ARCHITECTURE.md](ARCHITECTURE.md) - Service layer overview
3. [README.md](README.md) - Environment variables

**Integration Partner / External Client**
1. [INTEGRATION-GUIDE.md](INTEGRATION-GUIDE.md) - Start here
2. [API-DOCS.md](API-DOCS.md) - Available endpoints
3. [WEBHOOK-EVENTS.md](WEBHOOK-EVENTS.md) - Webhook events
4. [README.md](README.md) - Authentication details

### By Task

**Setting up locally for development**
- [SETUP.md](SETUP.md)

**Understanding system architecture**
- [ARCHITECTURE.md](ARCHITECTURE.md)

**Implementing API integration**
- [API-DOCS.md](API-DOCS.md)
- [INTEGRATION-GUIDE.md](INTEGRATION-GUIDE.md)

**Handling webhooks**
- [WEBHOOK-EVENTS.md](WEBHOOK-EVENTS.md)
- [INTEGRATION-GUIDE.md](INTEGRATION-GUIDE.md)

**Deploying to production**
- [SETUP.md](SETUP.md) - Docker section
- [README.md](README.md) - Quick start

**Troubleshooting issues**
- [SETUP.md](SETUP.md) - Troubleshooting section

## Key Concepts

### Multi-Tenant Architecture
All documentation covers multi-tenant support. Each tenant has:
- Complete data isolation
- Separate API keys
- Own Stripe account
- Dedicated webhooks

See [ARCHITECTURE.md](ARCHITECTURE.md) for details.

### Authentication Methods
Three authentication methods supported:
1. **JWT Token** - Browser-based clients
2. **API Key** - Server-to-server integration
3. **HMAC-SHA256** - Webhook signature verification

See [API-DOCS.md](API-DOCS.md) and [INTEGRATION-GUIDE.md](INTEGRATION-GUIDE.md) for examples.

### Webhook System
Dual webhook system:
1. **Inbound** - From Stripe to our service
2. **Outbound** - From our service to client apps

See [WEBHOOK-EVENTS.md](WEBHOOK-EVENTS.md) for all events.

### Rate Limiting
Prevent abuse with middleware-based rate limiting:
- Per-user/API-key limits
- Different limits by role
- Exponential backoff retry

See [ARCHITECTURE.md](ARCHITECTURE.md) for strategy details.

## Technologies Used

### Backend
- ASP.NET Core 9
- Entity Framework Core
- SQL Server 2022
- Stripe.net SDK
- SignalR
- Serilog + Seq

### Frontend
- React 18
- TypeScript
- Vite
- TanStack Query
- Tailwind CSS
- SignalR Client

### DevOps
- Docker & Docker Compose
- SQL Server in Docker
- Seq for logging

## Common Issues and Solutions

### Setup Issues
See [SETUP.md](SETUP.md) - Troubleshooting section

### API Integration Issues
See [API-DOCS.md](API-DOCS.md) - Error response formats

### Webhook Issues
See [WEBHOOK-EVENTS.md](WEBHOOK-EVENTS.md) - Troubleshooting section

## Support Resources

1. **Documentation Files** - Comprehensive guides above
2. **Code Examples** - In integration guides and API docs
3. **Sample Requests** - Using cURL and JavaScript
4. **Architecture Diagrams** - In ARCHITECTURE.md

## Version Information

- **Project Version**: 1.0.0
- **API Version**: v1
- **.NET Version**: 8.0
- **Node.js Version**: 20+
- **SQL Server**: 2022+
- **Last Updated**: February 26, 2026

## Documentation Structure

```
stripe-billing-service/
├── README.md                    # Project overview & quick start
├── ARCHITECTURE.md              # System design & architecture
├── SETUP.md                     # Installation & configuration
├── API-DOCS.md                  # API reference
├── WEBHOOK-EVENTS.md            # Webhook documentation
├── INTEGRATION-GUIDE.md          # Integration examples
├── DOCUMENTATION.md             # This file (index)
├── backend/
├── frontend/
├── docs/
└── docker-compose.yml
```

## Getting Help

If you can't find what you need:

1. Check the relevant documentation file listed above
2. Review code examples in [INTEGRATION-GUIDE.md](INTEGRATION-GUIDE.md)
3. Check troubleshooting section in [SETUP.md](SETUP.md)
4. Review error formats in [API-DOCS.md](API-DOCS.md)

---

**Last Updated**: February 26, 2026
**Documentation Version**: 1.0.0
