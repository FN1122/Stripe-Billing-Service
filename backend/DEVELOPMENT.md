# Development Setup Guide

## Prerequisites

- .NET 9.0 SDK or later
- Visual Studio 2022 / Visual Studio Code / JetBrains Rider
- SQL Server 2022 or SQL Server Express
- SQL Server Management Studio (optional but recommended)
- Git

## Installation Steps

### 1. Clone/Open Project

```bash
cd /path/to/project
```

### 2. Restore Dependencies

```bash
cd backend
dotnet restore
```

### 3. Build Solution

```bash
dotnet build
```

### 4. Configure Database

#### Option A: Using LocalDB (Default)

No additional setup needed. LocalDB is included with Visual Studio.

#### Option B: Using Docker Compose

```bash
docker-compose up -d
```

This starts a SQL Server 2022 instance:
- Server: localhost,1433
- Username: sa
- Password: StripeBillingP@ss123
- Database: StripeBillingDb

### 5. Apply Database Migrations

```bash
cd backend

# Install EF Core tools if not already installed
dotnet tool install --global dotnet-ef

# Create database and apply migrations
dotnet ef database update --project Core --startup-project WebAPI
```

### 6. Configure Application Secrets

Create a `secrets.json` file in the WebAPI project root:

```bash
cd WebAPI
dotnet user-secrets init
dotnet user-secrets set "Stripe:ApiKey" "sk_test_YOUR_KEY"
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_YOUR_SECRET"
```

Or manually create `%APPDATA%\Microsoft\UserSecrets\<project-guid>\secrets.json`:

```json
{
  "Stripe": {
    "ApiKey": "sk_test_your_stripe_test_key",
    "WebhookSecret": "whsec_your_webhook_secret"
  }
}
```

### 7. Run API

```bash
cd WebAPI
dotnet run
```

API will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger: http://localhost:5000/swagger

### 8. Run Tests

```bash
cd backend
dotnet test
```

### 9. Run Tests with Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverageOutputFormat=lcov
```

## Project Setup in IDEs

### Visual Studio 2022

1. Open `StripeBilling.sln`
2. Solution Explorer shows all three projects
3. Set `StripeBilling.API` as startup project
4. Press F5 to run

### Visual Studio Code

1. Install C# extension
2. Open the `backend` folder
3. Terminal: `dotnet run --project WebAPI`

### JetBrains Rider

1. Open `StripeBilling.sln`
2. Right-click WebAPI project → Set as Default Run Configuration
3. Click Run or press Shift+F10

## Environment Configuration

### appsettings.json (Shared)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=StripeBillingDb;Trusted_Connection=true;"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-characters-long",
    "ExpiryMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### appsettings.Development.json (Development Only)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

## Common Tasks

### Add a New Package

```bash
cd <ProjectName>
dotnet add package <PackageName> --version <Version>
```

Example:
```bash
cd Core
dotnet add package AutoMapper --version 13.0.0
```

### Create Database Migration

```bash
cd backend
dotnet ef migrations add MigrationName --project Core --startup-project WebAPI
```

### Update Database with Migration

```bash
cd backend
dotnet ef database update --project Core --startup-project WebAPI
```

### Rollback Database

```bash
cd backend
dotnet ef database update PreviousMigrationName --project Core --startup-project WebAPI
```

### Remove Last Migration

```bash
cd backend
dotnet ef migrations remove --project Core --startup-project WebAPI
```

### Create New Controller

```bash
# Manual approach
touch WebAPI/Controllers/v1/UsersController.cs
```

Content template:
```csharp
using Microsoft.AspNetCore.Mvc;
using StripeBilling.Core.Dtos.Responses;

namespace StripeBilling.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponse<List<object>>> GetUsers()
    {
        var response = ApiResponse<List<object>>.SuccessResponse(
            new List<object>(), 
            "Users retrieved successfully"
        );
        return Ok(response);
    }
}
```

### Create New Service

1. Create interface in `Core/ServiceContracts/IMyService.cs`
2. Create implementation in `Core/Services/MyService.cs`
3. Register in `Program.cs`:

```csharp
builder.Services.AddScoped<IMyService, MyService>();
```

### Create New DTO

1. Request: `Core/Dtos/Requests/MyRequestDto.cs`
2. Response: `Core/Dtos/Responses/MyResponseDto.cs`

### Create New Validator

1. Create in `Core/Validators/MyValidator.cs`
2. Extend `BaseValidator<T>`
3. Register in `Program.cs`:

```csharp
builder.Services.AddValidatorsFromAssemblyContaining<MyValidator>();
```

### Run Specific Test

```bash
dotnet test --filter "FullyQualifiedName~TestClassName"
```

Example:
```bash
dotnet test --filter "FullyQualifiedName~SampleTests"
```

## Debugging

### Visual Studio

1. Set breakpoints (F9)
2. Press F5 to start debugging
3. Use Debug menu for step operations

### VS Code

1. Install C# extension
2. Create `.vscode/launch.json` configuration
3. Press F5 to debug

### Terminal Logging

Serilog logs are output to console during development. Check console for request/response logs.

## Docker Development

### Build Docker Image

```bash
docker build -t stripe-billing-api .
```

### Run with Docker Compose

```bash
# Start services
docker-compose up -d

# Stop services
docker-compose down

# View logs
docker-compose logs -f api

# Rebuild containers
docker-compose up -d --build
```

### Access Database in Docker

```bash
docker exec -it <container-id> sqlcmd -S . -U sa -P 'StripeBillingP@ss123'
```

## Troubleshooting

### Port Already in Use

```bash
# Find process using port 5000
lsof -i :5000  # macOS/Linux
netstat -ano | findstr :5000  # Windows

# Kill process
kill -9 <PID>  # macOS/Linux
taskkill /PID <PID> /F  # Windows
```

### Database Connection Issues

1. Verify connection string in `appsettings.json`
2. Check if SQL Server is running
3. Verify credentials are correct
4. For LocalDB: `sqllocaldb info`

### Entity Framework Issues

```bash
# Clear migrations and restart
dotnet ef migrations remove --project Core --startup-project WebAPI
dotnet ef database update --project Core --startup-project WebAPI
```

### Package Restore Issues

```bash
dotnet nuget locals all --clear
dotnet restore
```

## Performance Monitoring

### Enable Query Logging

Add to `Program.cs`:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString)
        .LogTo(Console.WriteLine);
});
```

### Swagger Testing

1. Navigate to http://localhost:5000/swagger
2. Click "Try it out" on any endpoint
3. Modify parameters as needed
4. Click "Execute"

## Code Quality

### Run Static Analysis

```bash
# Install analyzer
dotnet tool install -g roslynator.commandline

# Run analysis
roslynator analyze . --no-msbuild
```

### Format Code

```bash
# Install formatter
dotnet tool install -g dotnet-format

# Format all files
dotnet format
```

## Useful Commands Reference

```bash
# Build
dotnet build
dotnet build --configuration Release

# Run
dotnet run --project WebAPI
dotnet run --project Tests

# Test
dotnet test
dotnet test --filter "TestName"
dotnet test /p:CollectCoverage=true

# Database
dotnet ef migrations add Name --project Core --startup-project WebAPI
dotnet ef database update --project Core --startup-project WebAPI
dotnet ef migrations remove --project Core --startup-project WebAPI

# NuGet
dotnet add package PackageName --version Version
dotnet remove package PackageName
dotnet package search PackageName

# Clean
dotnet clean
dotnet nuget locals all --clear

# Publish
dotnet publish -c Release -o ./publish
```

## Next Steps

1. Implement database migrations
2. Create entity models
3. Add repository implementations
4. Create service implementations
5. Add API controllers and endpoints
6. Write unit and integration tests
7. Configure Stripe webhook handling
8. Set up CI/CD pipeline

## Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Stripe API Documentation](https://stripe.com/docs/api)
- [xUnit.net Documentation](https://xunit.net/docs/getting-started/netcore)
- [FluentValidation](https://fluentvalidation.net)

## Support

For issues:
1. Check error messages in console output
2. Review Serilog logs
3. Check Database connection
4. Verify .NET SDK version: `dotnet --version`

