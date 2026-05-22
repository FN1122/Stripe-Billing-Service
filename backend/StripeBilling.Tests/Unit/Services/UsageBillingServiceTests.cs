using FluentAssertions;
using Moq;
using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.Services;
using Xunit;

namespace StripeBilling.Tests.Unit.Services;

public class UsageBillingServiceTests
{
    private readonly Mock<ITenantContextProvider> _tenantContextMock = new();
    private readonly Mock<IUsageRepository> _usageRepoMock = new();
    private readonly Mock<ISubscriptionRepository> _subscriptionRepoMock = new();
    private readonly Guid _tenantId = Guid.NewGuid();

    private UsageBillingService CreateService()
    {
        _tenantContextMock.Setup(x => x.GetCurrentTenantContext()).Returns(new TenantContext
        {
            TenantId = _tenantId,
            UserId = Guid.NewGuid(),
            Role = "Admin"
        });
        return new UsageBillingService(_tenantContextMock.Object, _usageRepoMock.Object, _subscriptionRepoMock.Object);
    }

    [Fact]
    public async Task BatchReportUsage_Over100Records_ReturnsError()
    {
        // Arrange
        var service = CreateService();
        var records = Enumerable.Range(0, 101).Select(_ => new CreateUsageRecordDto
        {
            SubscriptionId = Guid.NewGuid(),
            Quantity = 1
        }).ToList();

        var batch = new BatchUsageRecordDto { Records = records };

        // Act
        var result = await service.BatchReportUsageAsync(batch);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("Maximum 100");
    }

    [Fact]
    public async Task BatchReportUsage_WithinLimit_Succeeds()
    {
        // Arrange
        var service = CreateService();
        var subId = Guid.NewGuid();
        var records = Enumerable.Range(0, 5).Select(_ => new CreateUsageRecordDto
        {
            SubscriptionId = subId,
            Quantity = 10
        }).ToList();

        _usageRepoMock.Setup(r => r.CreateRangeAsync(It.IsAny<List<UsageRecord>>())).Returns(Task.CompletedTask);
        var batch = new BatchUsageRecordDto { Records = records };

        // Act
        var result = await service.BatchReportUsageAsync(batch);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data.Should().HaveCount(5);
    }

    [Fact]
    public async Task ReportUsage_WithIdempotencyKey_ReturnsCachedOnDuplicate()
    {
        // Arrange
        var idempotencyKey = "idem-key-123";
        var existingRecord = new UsageRecord
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            SubscriptionId = Guid.NewGuid(),
            Quantity = 50,
            Timestamp = DateTime.UtcNow,
            IdempotencyKey = idempotencyKey
        };

        _usageRepoMock.Setup(r => r.GetByIdempotencyKeyAsync(_tenantId, idempotencyKey)).ReturnsAsync(existingRecord);
        var service = CreateService();

        var request = new CreateUsageRecordDto
        {
            SubscriptionId = Guid.NewGuid(),
            Quantity = 999, // different quantity
            IdempotencyKey = idempotencyKey
        };

        // Act
        var result = await service.ReportUsageAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data!.Quantity.Should().Be(50); // returns existing, not the new request
        _usageRepoMock.Verify(r => r.CreateAsync(It.IsAny<UsageRecord>()), Times.Never);
    }

    [Fact]
    public async Task ReportUsage_WithoutIdempotencyKey_CreatesNewRecord()
    {
        // Arrange
        _usageRepoMock.Setup(r => r.CreateAsync(It.IsAny<UsageRecord>())).ReturnsAsync(Guid.NewGuid());
        var service = CreateService();

        var request = new CreateUsageRecordDto
        {
            SubscriptionId = Guid.NewGuid(),
            Quantity = 42,
            Action = "increment"
        };

        // Act
        var result = await service.ReportUsageAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data!.Quantity.Should().Be(42);
        _usageRepoMock.Verify(r => r.CreateAsync(It.IsAny<UsageRecord>()), Times.Once);
    }

    [Fact]
    public async Task ReportUsage_NullTimestamp_DefaultsToUtcNow()
    {
        // Arrange
        UsageRecord? savedRecord = null;
        _usageRepoMock.Setup(r => r.CreateAsync(It.IsAny<UsageRecord>()))
            .Callback<UsageRecord>(r => savedRecord = r)
            .ReturnsAsync(Guid.NewGuid());
        var service = CreateService();

        var request = new CreateUsageRecordDto
        {
            SubscriptionId = Guid.NewGuid(),
            Quantity = 10,
            Timestamp = null
        };

        // Act
        await service.ReportUsageAsync(request);

        // Assert
        savedRecord.Should().NotBeNull();
        savedRecord!.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
