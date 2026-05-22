# Stripe Billing Service - Comprehensive Test Suite

This document describes the complete test structure for the Stripe Billing Service.

## Project Structure

```
StripeBilling.Tests/
├── StripeBilling.Tests.csproj (Main test project file)
├── Unit/
│   ├── Services/
│   │   ├── HmacAuthServiceTests.cs
│   │   ├── RefundServiceTests.cs
│   │   ├── SubscriptionServiceTests.cs
│   │   ├── RevenueAnalyticsServiceTests.cs
│   │   └── WebhookSignatureServiceTests.cs
│   └── Middleware/
│       └── RateLimitMiddlewareTests.cs
└── Integration/
    ├── TenantIsolationTests.cs
    ├── PaymentFlowTests.cs
    └── SubscriptionFlowTests.cs
```

## Test Files Overview

### Unit Tests

#### Services

1. **HmacAuthServiceTests.cs** (4 tests)
   - ComputeSignature_ReturnsCorrectHash: Validates HMAC signature generation
   - ComputeSignature_DifferentBodies_ProduceDifferentSignatures: Ensures different inputs produce different signatures
   - ComputeSignature_EmptyBody_ReturnsValidHash: Handles edge case of empty body
   - ComputeSignature_ConsistentResults: Verifies deterministic signature generation

2. **RefundServiceTests.cs** (3 tests)
   - CreateRefund_UnderThreshold_AutoApproves: Tests automatic approval for small refunds
   - CreateRefund_ExceedsTransactionAmount_ReturnsError: Validates refund amount validation
   - RefundEntity_DefaultValues_AreCorrect: Verifies entity initialization

3. **SubscriptionServiceTests.cs** (3 tests)
   - Subscription_TenantIsolation_OnlyReturnsTenantData: Ensures data is properly scoped by tenant
   - Subscription_StatusTransitions_AreValid: Validates all subscription status values
   - Subscription_CancelAtPeriodEnd_SetsCorrectly: Tests subscription cancellation behavior

4. **RevenueAnalyticsServiceTests.cs** (2 tests)
   - MrrCalculation_SumsActiveSubscriptionAmounts: Calculates monthly recurring revenue correctly
   - ChurnRate_CalculatesCorrectly: Computes subscription churn rate accurately

5. **WebhookSignatureServiceTests.cs** (5 tests)
   - Sign_ReturnsValidHmacSha256: Validates signature generation format
   - Verify_ValidSignature_ReturnsTrue: Verifies valid signatures are accepted
   - Verify_InvalidSignature_ReturnsFalse: Rejects invalid signatures
   - Verify_TamperedPayload_ReturnsFalse: Detects payload tampering
   - Sign_DifferentSecrets_ProduceDifferentSignatures: Ensures secret uniqueness

#### Middleware

1. **RateLimitMiddlewareTests.cs** (3 tests)
   - MemoryCache_TracksRequestCounts: Validates request counting mechanism
   - MemoryCache_ExpiresCorrectly: Tests cache expiration
   - RateLimit_IpAndApiKey_UseDifferentKeys: Ensures different rate limit scopes

### Integration Tests

1. **TenantIsolationTests.cs** (3 tests)
   - Customers_AreScopedByTenant: Validates customer data isolation across tenants
   - PaymentTransactions_AreScopedByTenant: Ensures payment data is tenant-scoped
   - AuditLogs_TrackTenantActions: Verifies audit logging respects tenant boundaries

2. **PaymentFlowTests.cs** (2 tests)
   - FullPaymentFlow_CreatesTransactionAndInvoice: Tests complete payment creation workflow
   - RefundFlow_UpdatesTransactionAndCreatesRefundRecord: Validates refund processing

3. **SubscriptionFlowTests.cs** (2 tests)
   - SubscriptionLifecycle_CreateTrialActivateCancel: Tests full subscription lifecycle
   - PlanChange_Upgrade_UpdatesSubscription: Validates subscription plan upgrades

## Testing Frameworks & Libraries

- **xUnit**: Test framework with [Fact] and [Theory] attributes
- **Moq**: Mocking and verification library
- **FluentAssertions**: Fluent assertion API for readability
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database for testing
- **Microsoft.AspNetCore.Mvc.Testing**: ASP.NET Core integration testing

## Key Testing Patterns

### 1. In-Memory Database Testing
Tests use EF Core's InMemoryDatabase for isolated, fast database tests:
```csharp
var options = new DbContextOptionsBuilder<BillingDbContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    .Options;
```

### 2. Tenant Isolation Testing
Multiple tests verify that data is properly scoped by TenantId, preventing cross-tenant data leakage.

### 3. Mocking and Dependency Injection
Services are tested with mocked dependencies using Moq:
```csharp
var mockLogger = new Mock<ILogger<RefundService>>();
var mockTenantContext = new Mock<ITenantContextProvider>();
```

### 4. FluentAssertions
All assertions use fluent syntax for better readability:
```csharp
result.Should().Be(expected);
count.Should().HaveCount(2);
status.Should().BeTrue();
```

## Running the Tests

### Run all tests:
```bash
dotnet test StripeBilling.Tests.csproj
```

### Run specific test class:
```bash
dotnet test --filter "FullyQualifiedName~HmacAuthServiceTests"
```

### Run with coverage:
```bash
dotnet test /p:CollectCoverage=true
```

## Test Coverage Goals

- **Services**: >85% code coverage
- **Middleware**: >80% code coverage
- **Database Operations**: 100% integration test coverage
- **Critical Paths**: 100% unit test coverage

## Adding New Tests

When adding new tests:

1. Follow the naming convention: `[Feature]_[Scenario]_[ExpectedResult]`
2. Use [Fact] for tests without parameters, [Theory] for parameterized tests
3. Follow the Arrange-Act-Assert (AAA) pattern
4. Keep tests focused on a single concern
5. Use descriptive assertion messages
6. Place mocks and fixtures at the class level when reused

## Continuous Integration

These tests are designed to run in CI/CD pipelines:
- Fast execution (in-memory databases)
- No external dependencies
- Deterministic results
- Isolated test data per test
