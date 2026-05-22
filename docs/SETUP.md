# Setup Guide — Stripe Billing Service

Local development setup for backend, frontend, database, and Stripe CLI.

---

## Prerequisites

| Tool | Version | Download |
|------|---------|----------|
| .NET SDK | 8.0+ | https://dotnet.microsoft.com/download |
| Node.js | 20 LTS | https://nodejs.org |
| SQL Server | 2022 / Express | https://www.microsoft.com/sql-server |
| Stripe CLI | Latest | https://stripe.com/docs/stripe-cli |
| Visual Studio / Rider | Latest | IDE for backend |
| VS Code | Latest | IDE for frontend |
| Git | Latest | https://git-scm.com |
| Docker Desktop | Latest (optional) | https://docker.com |

---

## 1. Clone Repository

```bash
git clone <repo-url> 03-Stripe-Billing-Service
cd 03-Stripe-Billing-Service
```

---

## 2. Database Setup

### Option A: SQL Server Local
1. Install SQL Server 2022 Express
2. Create database: `StripeBillingDb`
3. Note your connection string:
```
Server=localhost;Database=StripeBillingDb;Trusted_Connection=true;TrustServerCertificate=true;
```

### Option B: Docker
```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Password1" \
  -p 1433:1433 --name sql-billing \
  -d mcr.microsoft.com/mssql/server:2022-latest
```
Connection string:
```
Server=localhost,1433;Database=StripeBillingDb;User Id=sa;Password=YourStrong@Password1;TrustServerCertificate=true;
```

---

## 3. Stripe Setup

### Create Stripe Account
1. Go to https://dashboard.stripe.com/register
2. Activate test mode (toggle in dashboard header)

### Get API Keys
1. Dashboard → Developers → API Keys
2. Copy: Publishable key (`pk_test_...`) and Secret key (`sk_test_...`)

### Get Webhook Signing Secret
1. Install Stripe CLI: `brew install stripe/stripe-cli/stripe` (macOS) or download from https://stripe.com/docs/stripe-cli
2. Login: `stripe login`
3. Start listener:
```bash
stripe listen --forward-to https://localhost:5001/api/v1/webhooks/stripe
```
4. Copy the webhook signing secret (`whsec_...`) from the CLI output

### Test Cards
| Number | Result |
|--------|--------|
| 4242 4242 4242 4242 | Succeeds |
| 4000 0000 0000 0002 | Declined |
| 4000 0000 0000 3220 | 3D Secure required |
| 4000 0000 0000 9995 | Insufficient funds |

---

## 4. Backend Setup

### Configure
```bash
cd backend/WebAPI
```

Copy and edit appsettings:
```bash
cp appsettings.example.json appsettings.Development.json
```

Edit `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StripeBillingDb;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "Jwt": {
    "Secret": "your-256-bit-secret-key-minimum-32-characters-long",
    "Issuer": "StripeBillingService",
    "Audience": "StripeBillingDashboard",
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "Stripe": {
    "DefaultSecretKey": "sk_test_...",
    "DefaultPublishableKey": "pk_test_...",
    "DefaultWebhookSecret": "whsec_..."
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

### Install & Run
```bash
cd backend
dotnet restore

# Apply migrations
dotnet ef database update -p Core -s WebAPI

# Run
dotnet run --project WebAPI
```

### Verify
- API: https://localhost:5001
- Swagger: https://localhost:5001/swagger
- Health: https://localhost:5001/health

### Default Login
- Email: `admin@billing.io`
- Password: `Admin@123!`

---

## 5. Frontend Setup

### Configure
```bash
cd frontend
cp .env.example .env
```

Edit `.env`:
```
VITE_API_BASE_URL=https://localhost:5001/api/v1
VITE_SIGNALR_URL=https://localhost:5001/hubs/dashboard
```

### Install & Run
```bash
npm install
npm run dev
```

### Verify
- Dashboard: http://localhost:5173
- Login with: admin@billing.io / Admin@123!

---

## 6. Stripe CLI (Webhook Testing)

In a separate terminal:
```bash
stripe listen --forward-to https://localhost:5001/api/v1/webhooks/stripe
```

Trigger test events:
```bash
# Test payment
stripe trigger payment_intent.succeeded

# Test subscription
stripe trigger customer.subscription.created

# Test invoice
stripe trigger invoice.paid
```

---

## 7. Full Stack with Docker

```bash
cd docker
docker-compose up -d
```

Services:
- API: http://localhost:5000
- Frontend: http://localhost:3000
- SQL Server: localhost:1433 (sa / YourStrong@Password1)

---

## Environment Variables Reference

### Backend (appsettings.json)
| Key | Description | Example |
|-----|-------------|---------|
| ConnectionStrings:DefaultConnection | SQL Server connection | Server=localhost;... |
| Jwt:Secret | JWT signing key (min 32 chars) | your-secret-key |
| Jwt:Issuer | JWT issuer | StripeBillingService |
| Jwt:Audience | JWT audience | StripeBillingDashboard |
| Stripe:DefaultSecretKey | Default Stripe secret | sk_test_... |
| Stripe:DefaultPublishableKey | Default Stripe publishable | pk_test_... |
| Stripe:DefaultWebhookSecret | Stripe webhook signing | whsec_... |
| Cors:AllowedOrigins | Allowed frontend origins | ["http://localhost:5173"] |

### Frontend (.env)
| Key | Description | Example |
|-----|-------------|---------|
| VITE_API_BASE_URL | Backend API URL | https://localhost:5001/api/v1 |
| VITE_SIGNALR_URL | SignalR hub URL | https://localhost:5001/hubs/dashboard |

---

## Troubleshooting

### "Connection refused" on SQL Server
- Ensure SQL Server is running
- Check connection string matches your setup
- For Docker: verify container is running (`docker ps`)

### Stripe webhook events not arriving
- Ensure Stripe CLI is running: `stripe listen --forward-to ...`
- Check the webhook signing secret matches
- Verify the endpoint URL is correct

### CORS errors in browser
- Check `Cors:AllowedOrigins` in appsettings includes your frontend URL
- Restart backend after changing CORS config

### JWT token expired
- Frontend interceptor should auto-refresh
- If stuck: clear localStorage and re-login
