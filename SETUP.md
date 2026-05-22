# Setup Guide

## Prerequisites

Before starting the installation, ensure you have:

### System Requirements
- **Operating System**: Windows, macOS, or Linux
- **RAM**: 8GB minimum (16GB recommended)
- **Disk Space**: 10GB available

### Software Requirements

#### Backend
- **.NET 9 SDK** (8.0.0 or later)
  - Download: https://dotnet.microsoft.com/download/dotnet/8.0
  - Verify: `dotnet --version`
- **Visual Studio 2022** (optional, for development)
  - Or: Visual Studio Code + C# extension

#### Frontend
- **Node.js** 20.0.0 or later
  - Download: https://nodejs.org/
  - Verify: `node --version` and `npm --version`
- **npm** 10.0.0 or later (comes with Node.js)

#### Database
- **SQL Server 2022** (local or remote)
  - Or use Docker (see Docker setup below)
  - Verify: `sqlcmd -S localhost -E`

#### Stripe Account
- Stripe account (https://stripe.com)
- Publishable Key (pk_test_...)
- Secret Key (sk_test_...)
- Webhook Signing Secret (whsec_...)

#### Development Tools
- **Git** (for version control)
- **Docker & Docker Compose** (optional, for containerized setup)
- **curl** or **Postman** (for API testing)
- **Stripe CLI** (optional, for webhook testing)

### Port Requirements

Ensure these ports are available:
- **3000**: React frontend (development)
- **5000**: ASP.NET Core API
- **5173**: Vite dev server (alternative frontend)
- **5341**: Seq logging dashboard
- **1433**: SQL Server database

## Backend Setup

### 1. Navigate to Backend Directory

```bash
cd stripe-billing-service/backend
```

### 2. Restore NuGet Packages

```bash
dotnet restore
```

This downloads all required dependencies.

### 3. Configure Environment

Create or update `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Warning"
    }
  },
  "Stripe": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_..."
  },
  "Jwt": {
    "Secret": "your-secret-key-minimum-32-characters-long",
    "ExpirationMinutes": 1440,
    "RefreshTokenExpirationDays": 7
  },
  "Database": {
    "ConnectionString": "Server=.;Database=StripeBilling;Trusted_Connection=true;"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:3001",
      "http://localhost:5173"
    ]
  },
  "RateLimit": {
    "RequestsPerMinute": 60
  },
  "Seq": {
    "ServerUrl": "http://localhost:5341"
  }
}
```

### 4. Database Migration

#### Using Entity Framework Core

```bash
# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Apply pending migrations
dotnet ef database update
```

#### Using SQL Scripts

If you prefer manual migration:

```bash
# Generate SQL script
dotnet ef migrations script -o "migration.sql"

# Execute script in SQL Server
sqlcmd -S localhost -i migration.sql
```

### 5. Create Default Users

Run this SQL script to create initial users:

```sql
-- Insert seed data
INSERT INTO [dbo].[Tenants] ([Id], [Name], [Status], [CreatedAt])
VALUES (NEWID(), 'Default Tenant', 'Active', GETUTCDATE());

-- Get the tenant ID
DECLARE @TenantId UNIQUEIDENTIFIER = 
  (SELECT TOP 1 [Id] FROM [dbo].[Tenants] WHERE [Name] = 'Default Tenant');

-- Insert users (passwords are hashed - use bcrypt in production)
-- SuperAdmin
INSERT INTO [dbo].[Users] ([Id], [TenantId], [Email], [PasswordHash], [Role], [IsActive], [CreatedAt])
VALUES (
  NEWID(), 
  @TenantId, 
  'superadmin@stripebilling.com',
  'hashed_password_here',
  'SuperAdmin', 
  1, 
  GETUTCDATE()
);

-- Admin
INSERT INTO [dbo].[Users] ([Id], [TenantId], [Email], [PasswordHash], [Role], [IsActive], [CreatedAt])
VALUES (
  NEWID(), 
  @TenantId, 
  'admin@stripebilling.com',
  'hashed_password_here',
  'Admin', 
  1, 
  GETUTCDATE()
);

-- Manager
INSERT INTO [dbo].[Users] ([Id], [TenantId], [Email], [PasswordHash], [Role], [IsActive], [CreatedAt])
VALUES (
  NEWID(), 
  @TenantId, 
  'manager@stripebilling.com',
  'hashed_password_here',
  'Manager', 
  1, 
  GETUTCDATE()
);

-- Viewer
INSERT INTO [dbo].[Users] ([Id], [TenantId], [Email], [PasswordHash], [Role], [IsActive], [CreatedAt])
VALUES (
  NEWID(), 
  @TenantId, 
  'viewer@stripebilling.com',
  'hashed_password_here',
  'Viewer', 
  1, 
  GETUTCDATE()
);
```

Or use the API seeder if available:

```bash
dotnet run -- --seed-database
```

### 6. Build and Run

```bash
# Build solution
dotnet build

# Run API server
dotnet run --configuration Development
```

The API will be available at: **http://localhost:5000**

You should see output like:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

### 7. Verify Backend

```bash
# Test health endpoint
curl http://localhost:5000/api/v1/health

# Expected response
{"status":"healthy","timestamp":"2026-02-26T10:30:00Z"}
```

## Frontend Setup

### 1. Navigate to Frontend Directory

```bash
cd stripe-billing-service/frontend
```

### 2. Install Dependencies

```bash
npm install
```

This installs all Node.js dependencies from package.json.

### 3. Configure Environment

Create `.env.local` file:

```bash
VITE_API_BASE_URL=http://localhost:5000/api/v1
VITE_STRIPE_PUBLIC_KEY=pk_test_...
VITE_SIGNALR_URL=http://localhost:5000/api/v1/hubs/billing
```

### 4. Development Server

```bash
npm run dev
```

The frontend will start on: **http://localhost:5173** (or next available port)

You should see:
```
  VITE v5.x.x  ready in xx ms

  Local:        http://localhost:5173/
  press h to show help
```

### 5. Build for Production

```bash
npm run build
```

This creates optimized production build in `dist/` directory.

### 6. Preview Production Build

```bash
npm run preview
```

## Docker Setup (Recommended for Development)

### 1. Prerequisites

- Docker 20.10+
- Docker Compose 2.0+
- Git

### 2. Clone and Configure

```bash
# Clone repository
git clone https://github.com/yourusername/stripe-billing-service.git
cd stripe-billing-service

# Copy environment template
cp .env.example .env

# Edit .env with your settings
# Required:
# STRIPE_PUBLIC_KEY=pk_test_...
# STRIPE_SECRET_KEY=sk_test_...
# STRIPE_WEBHOOK_SECRET=whsec_...
# JWT_SECRET_KEY=your-min-32-char-secret-key
nano .env
```

### 3. Start Services

```bash
# Start all services in background
docker-compose up -d

# View logs
docker-compose logs -f api

# Wait for services to start (30-60 seconds)
docker-compose logs api | grep "Now listening"
```

### 4. Access Services

```
Frontend:  http://localhost:3000
API:       http://localhost:5000
Seq Logs:  http://localhost:5341
```

### 5. Database Setup in Docker

The database is initialized automatically via container startup scripts.

### 6. Verify Docker Services

```bash
# List running containers
docker-compose ps

# Expected output:
# NAME                COMMAND             STATUS
# stripe-frontend     "npm run dev"       Up 2 minutes
# stripe-api          "dotnet run ..."    Up 2 minutes
# stripe-db           "sqlservr"          Up 2 minutes
# stripe-seq          "/app/Seq ..."      Up 2 minutes
```

### 7. Stop Services

```bash
# Stop all services
docker-compose down

# Remove volumes (careful - deletes data)
docker-compose down -v
```

## Stripe Webhook Configuration

### Using Stripe CLI (Recommended for Development)

#### 1. Install Stripe CLI

```bash
# macOS
brew install stripe/stripe-cli/stripe

# Windows (Chocolatey)
choco install stripe-cli

# Linux
curl https://files.stripe.com/stripe-cli/install.sh -O
bash install.sh

# Verify installation
stripe --version
```

#### 2. Login to Stripe

```bash
stripe login
```

Follow the browser prompt to authenticate.

#### 3. Forward Webhook Events

```bash
stripe listen --forward-to localhost:5000/api/v1/webhooks/stripe
```

This will output your signing secret:
```
Ready! Your webhook signing secret is whsec_test_... 
```

Update `appsettings.Development.json`:
```json
"Stripe": {
  "WebhookSecret": "whsec_test_..."
}
```

#### 4. Trigger Test Events

In another terminal:

```bash
# Simulate payment success
stripe trigger payment_intent.succeeded

# Simulate subscription created
stripe trigger customer.subscription.created

# List all test events
stripe trigger --help
```

### Using Stripe Dashboard (Production)

1. Go to: https://dashboard.stripe.com/webhooks
2. Click "Add endpoint"
3. Enter: `https://yourapi.com/api/v1/webhooks/stripe`
4. Select event types:
   - Payment Intent events
   - Subscription events
   - Invoice events
   - Charge events
   - Customer events
5. Click "Add events"
6. Copy signing secret to environment configuration

## Environment Configuration

### Complete Environment Variables Reference

```bash
# Stripe
STRIPE_PUBLIC_KEY=pk_test_...           # Publishable key
STRIPE_SECRET_KEY=sk_test_...           # Secret key
STRIPE_WEBHOOK_SECRET=whsec_...         # Webhook signing secret

# Database
DATABASE_CONNECTION_STRING=Server=.;Database=StripeBilling;Trusted_Connection=true;

# JWT
JWT_SECRET_KEY=your-secret-min-32-chars   # Minimum 32 characters
JWT_EXPIRATION_MINUTES=1440                # 24 hours
JWT_REFRESH_TOKEN_EXPIRATION_DAYS=7        # 7 days

# API Configuration
API_BASE_URL=http://localhost:5000
FRONTEND_URL=http://localhost:3000
API_PORT=5000

# CORS
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://localhost:3001,http://localhost:5173

# Logging
SEQ_SERVER_URL=http://localhost:5341
LOG_LEVEL=Information                      # Debug, Information, Warning, Error

# Rate Limiting
RATE_LIMIT_REQUESTS_PER_MINUTE=60          # Per user
RATE_LIMIT_REQUESTS_PER_HOUR=1000          # Per user

# Webhook Configuration
WEBHOOK_SIGNATURE_ALGORITHM=HMACSHA256
WEBHOOK_RETRY_ATTEMPTS=6
WEBHOOK_DISPATCH_INTERVAL_SECONDS=30
WEBHOOK_TIMEOUT_SECONDS=30
WEBHOOK_MAX_RETRIES=6

# Email (optional, for future use)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password

# Feature Flags
ENABLE_CUSTOMER_PORTAL=true
ENABLE_ANALYTICS=true
ENABLE_REAL_TIME_UPDATES=true
ENABLE_WEBHOOK_RETRY=true
ENABLE_EMAIL_NOTIFICATIONS=false
```

## Running in Development Mode

### All Services Together

```bash
# Terminal 1: Backend
cd backend
dotnet run --configuration Development

# Terminal 2: Frontend
cd frontend
npm run dev

# Terminal 3: Stripe Webhook Forwarding
stripe listen --forward-to localhost:5000/api/v1/webhooks/stripe

# Terminal 4: View Logs
docker run -d --name stripe-seq -p 5341:5341 datalust/seq
```

### Individual Service Testing

```bash
# Test backend health
curl http://localhost:5000/api/v1/health

# Test frontend
open http://localhost:3173

# Test API authentication
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@stripebilling.com","password":"Admin@123"}'
```

## Troubleshooting

### Backend Issues

#### Port Already in Use
```bash
# Find process using port 5000
lsof -i :5000

# Kill process
kill -9 <PID>

# Or change port in appsettings
"Kestrel": {
  "Endpoints": {
    "Http": {
      "Url": "http://localhost:5001"
    }
  }
}
```

#### Database Connection Error
```
Error: "Cannot open database 'StripeBilling' requested by the login."
```

Solutions:
1. Verify SQL Server is running: `sqlcmd -S localhost -E`
2. Check connection string in `appsettings.json`
3. Run migrations: `dotnet ef database update`
4. Verify database exists: `sqlcmd -S localhost -E -Q "SELECT name FROM sys.databases"`

#### Stripe API Error
```
Error: "Invalid API Key provided."
```

Solutions:
1. Verify keys in `appsettings.json`
2. Ensure test mode keys (pk_test_*, sk_test_*)
3. Check key hasn't expired
4. Use correct secret key (not publishable key)

### Frontend Issues

#### Port Already in Use
```bash
# Change port in vite.config.ts
export default {
  server: {
    port: 3001
  }
}
```

#### Module Not Found
```bash
# Reinstall dependencies
rm -rf node_modules package-lock.json
npm install
```

#### API Connection Error
```
Error: "Failed to fetch from API"
```

Solutions:
1. Verify backend is running: `curl http://localhost:5000/api/v1/health`
2. Check VITE_API_BASE_URL in `.env.local`
3. Check CORS configuration in backend
4. Verify tenant ID header is sent

### Docker Issues

#### Container Won't Start
```bash
# View container logs
docker-compose logs stripe-api

# Rebuild image
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

#### Database Migration Failed
```bash
# Restart database service
docker-compose restart stripe-db

# Wait 30 seconds then restart API
sleep 30
docker-compose restart stripe-api
```

#### Port Conflict
```bash
# Change ports in docker-compose.yml
ports:
  - "3001:3000"  # Frontend
  - "5001:5000"  # API
  - "5342:5341"  # Seq
```

### Webhook Issues

#### Stripe Signature Verification Failed
```
Error: "Invalid signature"
```

Solutions:
1. Verify webhook secret is correct
2. Ensure raw request body is used for verification
3. Check timestamp is within 5 minutes
4. Verify HMAC calculation algorithm

#### Webhook Not Received
```bash
# Check webhook endpoint is accessible
curl -X POST http://localhost:5000/api/v1/webhooks/stripe \
  -H "Content-Type: application/json" \
  -d '{}'

# Check Stripe CLI is forwarding
stripe logs tail

# Verify firewall allows webhook requests
```

## Next Steps

1. **Read Documentation**
   - Review [README.md](README.md) for project overview
   - Study [ARCHITECTURE.md](ARCHITECTURE.md) for system design
   - Review [API-DOCS.md](API-DOCS.md) for available endpoints

2. **Test Locally**
   - Create test customer via API
   - Create test subscription
   - Trigger webhook events
   - Monitor analytics dashboard

3. **Configure Stripe**
   - Set up webhook signing secret
   - Configure customer portal
   - Enable specific payment methods
   - Set up webhook events

4. **Deploy**
   - See deployment guide for production setup
   - Configure SSL/TLS certificates
   - Set up monitoring and alerts
   - Configure automated backups

## Support

- For issues: Check [SETUP.md](SETUP.md) troubleshooting section
- For API questions: See [API-DOCS.md](API-DOCS.md)
- For architecture questions: See [ARCHITECTURE.md](ARCHITECTURE.md)
- For webhook help: See [WEBHOOK-EVENTS.md](WEBHOOK-EVENTS.md)

---

Last Updated: February 26, 2026
