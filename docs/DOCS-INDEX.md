# Documentation Index — Stripe Billing Service

## Project Root
| File | Description |
|------|-------------|
| `Stripe_Billing_Development_Plan.md` | **Master development plan** — Day-by-day tasks, checklists, file structure |

## /docs/ Folder
| File | Description |
|------|-------------|
| `README.md` | Project overview, quick start guide, tech stack |
| `FEATURES.md` | All billing features, 70+ endpoints, 16 database tables, 25 webhook events |
| `ARCHITECTURE.md` | System design, multi-tenant, auth flows, middleware pipeline |
| `SETUP.md` | Local development setup (backend + frontend + Stripe CLI + database) |
| `DEPLOYMENT.md` | Docker, Azure, AWS, Railway deployment guides |
| `API-DOCS.md` | Full API reference with request/response examples for all 70+ endpoints |
| `WEBHOOK-EVENTS.md` | 14 inbound Stripe events + 11 outbound client events with payloads |
| `INTEGRATION-GUIDE.md` | How client apps integrate: HMAC signing, webhook verification, portal embedding |
| `DOCS-INDEX.md` | This file — documentation map |

## To Be Created (During Development)
| File | When |
|------|------|
| `API-DOCS.md` | After all endpoints are built (Day 10) |
| `WEBHOOK-EVENTS.md` | After webhook engine is built (Day 4) |
| `INTEGRATION-GUIDE.md` | After core flow is working (Day 4) |
| `Postman/StripeBilling.postman_collection.json` | After all endpoints are tested (Day 10) |
| `Postman/StripeBilling.postman_environment.json` | After all endpoints are tested (Day 10) |

## Complete Feature Specification
The complete feature specification with all database schemas, API endpoints, 
React page layouts, and component specifications was saved as:
- `Stripe_Complete_Documentation.docx` — Full specification document (36 sections)

This document should be referenced during development for:
- Exact endpoint paths, HTTP methods, and auth requirements
- Request/response JSON structures
- Database table definitions with column types and indexes
- React page layouts and component specifications
- TypeScript type definitions
- Pricing tiers and feature comparison matrix

## Reference Project
Patterns reused from `02-API-Gateway-Microservice`:
- GatewayResponseWrapper, BaseService, BaseRepository, GatewayControllerBase
- All middleware (Tenant, ApiKey, RateLimit, RequestLogging)
- React: api-client, interceptors, contexts, hooks, layouts, common components
- See Development Plan Section 1 for complete list of identical vs. new files
