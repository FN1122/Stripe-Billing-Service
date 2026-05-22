# Stripe Billing Service - Backend

A comprehensive .NET 9.0 backend solution for managing Stripe billing integrations with comprehensive API features, authentication, and testing infrastructure.

## Project Structure

### StripeBilling.Core
Core library containing business logic, data access, and domain models.

**Subdirectories:**
- `Constants/` - Application-wide constants
- `ContextProviders/` - Database context providers
- `Dtos/` - Data Transfer Objects
  - `Requests/` - Request DTOs
  - `Responses/` - Response DTOs
- `ErrorHandling/` - Error handling logic
  - `Exceptions/` - Custom exceptions
- `Infrastructure/` - Database context and EF Core configuration
- `Mappers/` - DTO and entity mappers
- `Repositories/` - Data access implementations
- `RepositoryContracts/` - Repository interfaces
- `ServiceContracts/` - Service interfaces
- `Services/` - Business logic implementations
- `Utils/` - Utility functions and helpers
- `Validators/` - FluentValidation validators

### StripeBilling.API
ASP.NET Core web API project providing REST endpoints.

**Subdirectories:**
- `Controllers/`
  - `v1/` - API version 1 endpoints
- `Middleware/` - Custom middleware components
- `BackgroundServices/` - Hosted background services
- `Hubs/` - SignalR hubs for real-time communication

### StripeBilling.Tests
xUnit test project with comprehensive unit and integration tests.

**Subdirectories:**
- `Services/` - Service tests
- `Controllers/` - Controller tests

## Prerequisites

- .NET 9.0 SDK or later
- SQL Server (LocalDB for development)
- Visual Studio 2022 or Visual Studio Code with C# extensions

## Getting Started

### 1. Setup Database

```bash
cd backend
dotnet ef database update --project Core --startup-project WebAPI
```

### 2. Configure Secrets

Create a `secrets.json` file in the WebAPI project root:

```json
{
  "Stripe:ApiKey": "sk_test_YOUR_STRIPE_KEY",
  "Stripe:WebhookSecret": "whsec_YOUR_WEBHOOK_SECRET"
}
```

### 3. Build Solution

```bash
dotnet build
```

### 4. Run API

```bash
cd WebAPI
dotnet run
```

The API will be available at `http://localhost:5000` and Swagger UI at `http://localhost:5000/swagger`.

## Key Features

- **JWT Authentication** - Secure endpoint protection
- **Stripe Integration** - Complete billing management
- **Entity Framework Core** - Type-safe database access
- **FluentValidation** - Comprehensive data validation
- **Serilog** - Structured logging
- **SignalR** - Real-time communication
- **Swagger/OpenAPI** - API documentation

## NuGet Dependencies

### Core Project
- EntityFrameworkCore (9.0.0)
- EntityFrameworkCore.SqlServer (9.0.0)
- FluentValidation (11.11.0)
- Stripe.net (47.0.0)
- System.IdentityModel.Tokens.Jwt (8.0.0)
- BCrypt.Net-Next (4.0.3)
- QuestPDF (2024.12.0)

### WebAPI Project
- AspNetCore.Authentication.JwtBearer (9.0.0)
- AspNetCore.SignalR (1.1.0)
- Swashbuckle.AspNetCore (6.9.0)
- Serilog.AspNetCore (8.0.3)

### Tests Project
- xunit (2.9.0)
- Moq (4.20.72)
- FluentAssertions (6.12.2)
- EntityFrameworkCore.InMemory (9.0.0)

## Project Roadmap

1. Core entity models and database context
2. Authentication and authorization layer
3. Stripe integration services
4. Customer management endpoints
5. Subscription management endpoints
6. Invoice and billing endpoints
7. Webhook handling
8. Comprehensive test coverage
9. API documentation
10. Deployment configuration

## Configuration Files

- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development-specific settings
- `.gitignore` - Git ignore rules

## Testing

Run unit and integration tests:

```bash
dotnet test
```

Run tests with coverage:

```bash
dotnet test /p:CollectCoverage=true
```

## Build & Deployment

### Development Build
```bash
dotnet build --configuration Debug
```

### Release Build
```bash
dotnet build --configuration Release
dotnet publish -c Release -o ./publish
```

## API Endpoints

### Health Check
- `GET /api/v1/health` - Check API status

Additional endpoints will be documented as features are implemented.

## Logging

Logging is configured using Serilog. Logs are written to:
- Console output
- Log files (when configured)

## Contributing

When adding new features:

1. Create corresponding DTOs in `Core/Dtos/`
2. Define service contracts in `Core/ServiceContracts/`
3. Implement services in `Core/Services/`
4. Create controllers in `WebAPI/Controllers/`
5. Add tests in `Tests/` directory
6. Update this README with new endpoints

## License

All rights reserved.

## Support

For issues and questions, please contact the development team.
