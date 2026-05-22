# Stripe Billing Service - Project Structure Documentation

## Overview
Complete .NET 9.0 backend solution with three main projects for managing Stripe billing integrations.

## Directory Structure

```
backend/
├── Core/                              # Class Library (StripeBilling.Core)
│   ├── Constants/
│   │   └── AppConstants.cs           # Application constants and messages
│   │
│   ├── ContextProviders/             # Database context providers (empty)
│   │
│   ├── Dtos/                         # Data Transfer Objects
│   │   ├── Requests/                 # Request DTOs (empty)
│   │   └── Responses/
│   │       └── ApiResponse.cs        # Generic API response wrapper
│   │
│   ├── ErrorHandling/
│   │   └── Exceptions/
│   │       └── AppException.cs       # Custom exception classes
│   │
│   ├── Infrastructure/
│   │   └── DbContext.cs              # Entity Framework DbContext
│   │
│   ├── Mappers/
│   │   └── MappingProfile.cs         # DTO/Entity mapping configuration
│   │
│   ├── Repositories/                 # Repository implementations (empty)
│   │
│   ├── RepositoryContracts/
│   │   └── IRepository.cs            # Generic repository interface
│   │
│   ├── ServiceContracts/
│   │   └── IBaseService.cs           # Base service interface
│   │
│   ├── Services/
│   │   └── BaseService.cs            # Base service implementation
│   │
│   ├── Utils/                        # Utility functions (empty)
│   │
│   ├── Validators/
│   │   └── BaseValidator.cs          # FluentValidation base class
│   │
│   └── StripeBilling.Core.csproj     # Project file with dependencies

├── WebAPI/                            # ASP.NET Core Web API
│   ├── Controllers/
│   │   └── v1/
│   │       └── HealthController.cs   # Health check endpoint
│   │
│   ├── Middleware/                    # Custom middleware (empty)
│   │
│   ├── BackgroundServices/            # Hosted background services (empty)
│   │
│   ├── Hubs/                          # SignalR hubs (empty)
│   │
│   ├── Program.cs                     # Application entry point
│   ├── appsettings.json               # Base configuration
│   ├── appsettings.Development.json   # Development settings
│   └── StripeBilling.API.csproj       # Project file with dependencies

├── Tests/                             # xUnit Test Project
│   ├── Services/                      # Service unit tests (empty)
│   ├── Controllers/                   # Controller tests (empty)
│   ├── SampleTests.cs                 # Sample test class
│   └── StripeBilling.Tests.csproj     # Project file with test dependencies

├── StripeBilling.sln                  # Solution file
├── Dockerfile                         # Multi-stage Docker build configuration
├── docker-compose.yml                 # Local development Docker Compose
├── .editorconfig                      # Code style configuration
├── .gitignore                         # Git ignore patterns
├── README.md                          # Backend documentation
└── STRUCTURE.md                       # This file

```

## Project Dependencies

### StripeBilling.Core
**Type:** Class Library (.NET 9.0)

**Core Dependencies:**
- Microsoft.EntityFrameworkCore (9.0.0)
- Microsoft.EntityFrameworkCore.SqlServer (9.0.0)
- Microsoft.Extensions.Http (9.0.0)
- Microsoft.AspNetCore.DataProtection (9.0.0)

**Authentication & Security:**
- System.IdentityModel.Tokens.Jwt (8.0.0)
- Microsoft.IdentityModel.Tokens (8.0.0)
- BCrypt.Net-Next (4.0.3)

**Business Logic & Validation:**
- FluentValidation (11.11.0)
- NetCore.AutoRegisterDi (2.1.1)

**Stripe & Serialization:**
- Stripe.net (47.0.0)
- Newtonsoft.Json (13.0.3)

**PDF Generation:**
- QuestPDF (2024.12.0)

**Caching:**
- Microsoft.Extensions.Caching.Memory (9.0.0)

### StripeBilling.API
**Type:** ASP.NET Core Web API (.NET 9.0)

**Web & API Dependencies:**
- Microsoft.AspNetCore.Authentication.JwtBearer (9.0.0)
- Microsoft.AspNetCore.SignalR (1.1.0)
- Swashbuckle.AspNetCore (6.9.0)

**Logging:**
- Serilog.AspNetCore (8.0.3)
- Serilog.Sinks.Console (6.0.0)

**Database:**
- Microsoft.EntityFrameworkCore.Design (9.0.0)

**Project References:**
- StripeBilling.Core (local)

### StripeBilling.Tests
**Type:** xUnit Test Project (.NET 9.0)

**Testing Framework:**
- xunit (2.9.0)
- xunit.runner.visualstudio (2.8.0)
- Microsoft.NET.Test.Sdk (17.12.0)

**Testing Utilities:**
- Moq (4.20.72)
- FluentAssertions (6.12.2)

**Testing Infrastructure:**
- Microsoft.EntityFrameworkCore.InMemory (9.0.0)
- Microsoft.AspNetCore.Mvc.Testing (9.0.0)

**Project References:**
- StripeBilling.Core (local)
- StripeBilling.API (local)

## Key Files

### Configuration Files
- **StripeBilling.sln** - Visual Studio solution file linking all projects
- **appsettings.json** - Default configuration (connection strings, JWT settings, Stripe credentials)
- **appsettings.Development.json** - Development-specific overrides

### Docker Files
- **Dockerfile** - Multi-stage build for production deployment
- **docker-compose.yml** - Local development environment with SQL Server

### Code Style
- **.editorconfig** - Enforces consistent code formatting
- **.gitignore** - Ignores build artifacts and user files

## Key Classes

### Core Project

**AppConstants.cs**
- API versioning constants
- Error and success messages
- Application-wide strings

**ApiResponse.cs**
- Generic response wrapper `ApiResponse<T>`
- Non-generic response wrapper `ApiResponse`
- Success and error response factory methods

**AppException.cs**
- Base custom exception
- Specialized exceptions: `NotFoundException`, `UnauthorizedException`, `ValidationException`, `StripeException`

**DbContext.cs**
- Entity Framework Core database context
- Entity configuration placeholder

**BaseValidator.cs**
- FluentValidation abstract base class
- Extends `AbstractValidator<T>`

**IBaseService.cs / BaseService.cs**
- Basic service contract and implementation
- Health check functionality

**IRepository.cs**
- Generic repository interface with CRUD operations
- Async/await support

### WebAPI Project

**Program.cs**
- Application startup and configuration
- Serilog setup
- JWT authentication configuration
- CORS policy configuration
- Service registration

**HealthController.cs**
- Simple health check endpoint
- Route: `GET /api/v1/health`
- Returns API status response

### Tests Project

**SampleTests.cs**
- Example unit test
- Demonstrates xUnit test structure

## Naming Conventions

- **Interfaces:** Prefix with `I` (e.g., `IRepository`, `IBaseService`)
- **Exception Classes:** Suffix with `Exception` (e.g., `AppException`, `NotFoundException`)
- **DTOs:** Suffix with `Dto` (e.g., `LoginRequestDto`, `UserResponseDto`)
- **Controllers:** Suffix with `Controller` (e.g., `HealthController`, `UsersController`)
- **Services:** Suffix with `Service` (e.g., `AuthService`, `PaymentService`)
- **Repositories:** Suffix with `Repository` (e.g., `UserRepository`)
- **Validators:** Suffix with `Validator` (e.g., `LoginValidator`)

## Development Workflow

### 1. Adding a New Feature

**Example: User Authentication**

```
Core/
├── Dtos/Requests/
│   └── LoginRequestDto.cs          # New: Request model
├── Dtos/Responses/
│   └── LoginResponseDto.cs         # New: Response model
├── ServiceContracts/
│   └── IAuthService.cs             # New: Service interface
├── Services/
│   └── AuthService.cs              # New: Service implementation
└── Validators/
    └── LoginValidator.cs           # New: FluentValidation class

WebAPI/
└── Controllers/v1/
    └── AuthController.cs           # New: API endpoints

Tests/
├── Services/
│   └── AuthServiceTests.cs         # New: Service tests
└── Controllers/
    └── AuthControllerTests.cs      # New: Controller tests
```

### 2. Database Migrations

```bash
cd backend/Core
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet ef migrations add InitialCreate --startup-project ../WebAPI
dotnet ef database update --startup-project ../WebAPI
```

### 3. Running Tests

```bash
cd backend
dotnet test
```

### 4. Building & Publishing

```bash
# Development
dotnet build

# Release
dotnet publish -c Release -o ./publish

# Docker
docker-compose up
```

## Future Expansion Areas

### Services to Implement
- Customer Service (CRUD operations)
- Subscription Service (subscription management)
- Invoice Service (invoice generation and retrieval)
- Payment Service (payment processing)
- Webhook Service (Stripe event handling)
- Authentication Service (user auth)

### Repositories to Implement
- Customer Repository
- Subscription Repository
- Invoice Repository
- Payment Repository
- User Repository

### Validators to Implement
- Customer validators
- Subscription validators
- Payment validators
- User validators

### DTOs to Implement
- Customer DTOs (Request/Response)
- Subscription DTOs (Request/Response)
- Invoice DTOs (Request/Response)
- Payment DTOs (Request/Response)
- User DTOs (Request/Response)

## Notes

- All projects target .NET 9.0 for latest features and performance improvements
- JWT authentication is pre-configured in Program.cs
- Swagger/OpenAPI documentation is automatically available at `/swagger`
- Database uses SQL Server (LocalDB for development)
- Serilog is configured for structured logging
- Entity Framework Core uses async/await patterns throughout

