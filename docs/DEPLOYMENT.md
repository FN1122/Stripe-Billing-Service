# Deployment Guide — Stripe Billing Service

Production deployment options for the multi-tenant billing microservice.

---

## Option 1: Docker Compose (Recommended)

### docker-compose.yml
```yaml
version: '3.8'

services:
  api:
    build:
      context: ../backend
      dockerfile: ../docker/Dockerfile.api
    ports:
      - "5000:8080"
    depends_on:
      - db
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=db;Database=StripeBillingDb;User Id=sa;Password=${DB_PASSWORD};TrustServerCertificate=true;
      - Jwt__Secret=${JWT_SECRET}
      - Jwt__Issuer=StripeBillingService
      - Jwt__Audience=StripeBillingDashboard
      - Stripe__DefaultSecretKey=${STRIPE_SECRET_KEY}
      - Stripe__DefaultPublishableKey=${STRIPE_PUBLISHABLE_KEY}
      - Stripe__DefaultWebhookSecret=${STRIPE_WEBHOOK_SECRET}
      - Cors__AllowedOrigins__0=https://billing.yourdomain.com
    restart: unless-stopped

  frontend:
    build:
      context: ../frontend
      dockerfile: ../docker/Dockerfile.frontend
      args:
        - VITE_API_BASE_URL=https://api.yourdomain.com/api/v1
        - VITE_SIGNALR_URL=https://api.yourdomain.com/hubs/dashboard
    ports:
      - "3000:80"
    depends_on:
      - api
    restart: unless-stopped

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    ports:
      - "1433:1433"
    environment:
      - ACCEPT_EULA=Y
      - MSSQL_SA_PASSWORD=${DB_PASSWORD}
    volumes:
      - mssql-data:/var/opt/mssql
    restart: unless-stopped

volumes:
  mssql-data:
```

### Dockerfile.api
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY Core/StripeBilling.Core.csproj Core/
COPY WebAPI/StripeBilling.API.csproj WebAPI/
RUN dotnet restore WebAPI/StripeBilling.API.csproj
COPY . .
RUN dotnet publish WebAPI/StripeBilling.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "StripeBilling.API.dll"]
```

### Dockerfile.frontend
```dockerfile
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
ARG VITE_API_BASE_URL
ARG VITE_SIGNALR_URL
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
```

### Deploy
```bash
# Create .env file
cat > .env << EOF
DB_PASSWORD=YourStrong@Password1
JWT_SECRET=your-256-bit-secret-key-minimum-32-characters
STRIPE_SECRET_KEY=sk_live_...
STRIPE_PUBLISHABLE_KEY=pk_live_...
STRIPE_WEBHOOK_SECRET=whsec_...
EOF

# Deploy
docker-compose up -d

# Apply migrations
docker-compose exec api dotnet ef database update
```

---

## Option 2: Azure App Service + Azure SQL

### Setup
1. Create Azure SQL Database
2. Create App Service (Linux, .NET 9)
3. Create Static Web App (for frontend) or App Service

### Backend Deployment
```bash
# Build
cd backend
dotnet publish WebAPI -c Release -o ./publish

# Deploy via Azure CLI
az webapp deployment source config-zip \
  --resource-group billing-rg \
  --name billing-api \
  --src ./publish.zip

# Set environment variables
az webapp config appsettings set \
  --resource-group billing-rg \
  --name billing-api \
  --settings \
    ConnectionStrings__DefaultConnection="Server=billing-sql.database.windows.net;Database=StripeBillingDb;User Id=admin;Password=...;Encrypt=true;" \
    Jwt__Secret="..." \
    Stripe__DefaultSecretKey="sk_live_..."
```

### Frontend Deployment
```bash
cd frontend
npm run build

# Deploy to Azure Static Web Apps or Blob Storage + CDN
az storage blob upload-batch \
  --account-name billingstorage \
  --source dist \
  --destination '$web'
```

### Cost Estimate: $40–$150/month
- Azure SQL: $15–$50/mo (Basic/Standard)
- App Service: $15–$70/mo (B1/S1)
- Static Web: $0–$10/mo

---

## Option 3: AWS (ECS + RDS)

### Setup
1. Create RDS SQL Server instance
2. Create ECS Fargate cluster
3. Create ECR repository
4. Create S3 + CloudFront for frontend

### Deploy API to ECS
```bash
# Build and push Docker image
aws ecr get-login-password | docker login --username AWS --password-stdin <account>.dkr.ecr.<region>.amazonaws.com
docker build -t billing-api -f docker/Dockerfile.api backend/
docker tag billing-api:latest <account>.dkr.ecr.<region>.amazonaws.com/billing-api:latest
docker push <account>.dkr.ecr.<region>.amazonaws.com/billing-api:latest

# Create ECS service with task definition (set env vars in task def)
```

### Cost Estimate: $50–$200/month

---

## Option 4: Railway / Render (Startups)

### Railway
1. Connect GitHub repo
2. Add SQL Server plugin (or use external)
3. Set environment variables
4. Deploy automatically on push

### Render
1. Create Web Service (Docker)
2. Create PostgreSQL (or connect external SQL Server)
3. Set environment variables
4. Auto-deploy from GitHub

### Cost Estimate: $20–$80/month

---

## Option 5: VPS (Budget)

### Setup on Ubuntu 22.04
```bash
# Install .NET 9
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0

# Install SQL Server
curl https://packages.microsoft.com/keys/microsoft.asc | sudo tee /etc/apt/trusted.gpg.d/microsoft.asc
sudo add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/22.04/mssql-server-2022.list)"
sudo apt-get update
sudo apt-get install -y mssql-server
sudo /opt/mssql/bin/mssql-conf setup

# Install Nginx
sudo apt install nginx

# Deploy backend
cd /var/www/billing-api
dotnet publish -c Release
sudo systemctl enable billing-api.service

# Deploy frontend
cd /var/www/billing-dashboard
npm run build
# Copy dist to nginx web root
```

### Cost Estimate: $10–$50/month (DigitalOcean, Hetzner, Linode)

---

## Stripe Webhook Configuration (Production)

### Set Up Live Webhook
1. Stripe Dashboard → Developers → Webhooks → Add endpoint
2. URL: `https://api.yourdomain.com/api/v1/webhooks/stripe`
3. Select events:
   - checkout.session.completed
   - payment_intent.succeeded
   - payment_intent.payment_failed
   - invoice.paid
   - invoice.payment_failed
   - customer.subscription.created
   - customer.subscription.updated
   - customer.subscription.deleted
   - customer.subscription.trial_will_end
   - charge.refunded
   - charge.dispute.created
   - customer.updated
   - payment_method.attached
   - price.updated
4. Copy signing secret → set as `Stripe:DefaultWebhookSecret`

### For Multi-Tenant
Each tenant has their own Stripe account + webhook endpoint:
- Endpoint URL is the same for all tenants
- Each tenant's Stripe webhook secret is stored encrypted per tenant
- Webhook handler tries each tenant's secret to identify the source

---

## SSL/TLS

### Let's Encrypt (Nginx)
```bash
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d api.yourdomain.com
sudo certbot --nginx -d billing.yourdomain.com
```

### Azure
- App Service: custom domain + managed certificate (free)
- Or bring your own certificate

---

## Environment Checklist

### Production Requirements
- [ ] HTTPS enabled on all endpoints
- [ ] Strong JWT secret (256-bit minimum)
- [ ] Stripe live keys configured (not test)
- [ ] Stripe webhook endpoint registered in live mode
- [ ] Database backup schedule configured
- [ ] CORS restricted to production frontend URL only
- [ ] Rate limiting configured per API key
- [ ] Log level set to Warning (not Debug)
- [ ] appsettings.Development.json NOT deployed
- [ ] .env file NOT committed to Git
- [ ] Database connection uses encrypted connection
- [ ] Super admin 2FA enabled
