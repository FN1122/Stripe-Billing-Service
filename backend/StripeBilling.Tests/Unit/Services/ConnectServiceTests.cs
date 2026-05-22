using FluentAssertions;
using Moq;
using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Infrastructure;
using Core.Services;
using Core.Utils;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace StripeBilling.Tests.Unit.Services;

public class ConnectServiceTests : IDisposable
{
    private readonly Mock<ITenantContextProvider> _tenantContextMock = new();
    private readonly BillingDbContext _dbContext;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public ConnectServiceTests()
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new BillingDbContext(options);
    }

    private ConnectService CreateService()
    {
        _tenantContextMock.Setup(x => x.GetCurrentTenantContext()).Returns(new TenantContext
        {
            TenantId = _tenantId,
            UserId = _userId,
            Role = "Admin"
        });
        return new ConnectService(_tenantContextMock.Object, _dbContext);
    }

    [Fact]
    public async Task CreateAccountAsync_Succeeds()
    {
        // Arrange
        var service = CreateService();
        var request = new CreateConnectedAccountDto
        {
            Email = "merchant@test.com", BusinessName = "My Shop",
            Country = "US", Type = "express", PlatformFeePercent = 2.5m
        };

        // Act
        var result = await service.CreateAccountAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data!.Email.Should().Be("merchant@test.com");
        result.Data.BusinessName.Should().Be("My Shop");
        result.Data.Type.Should().Be("express");
        _dbContext.ConnectedAccounts.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAccountsAsync_ReturnsOnlyTenantAccounts()
    {
        // Arrange
        _dbContext.ConnectedAccounts.AddRange(
            new ConnectedAccount { TenantId = _tenantId, Email = "a@t.com", Type = "express" },
            new ConnectedAccount { TenantId = _tenantId, Email = "b@t.com", Type = "standard" },
            new ConnectedAccount { TenantId = Guid.NewGuid(), Email = "other@t.com", Type = "express" }
        );
        await _dbContext.SaveChangesAsync();
        var service = CreateService();

        // Act
        var result = await service.GetAccountsAsync();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAccountAsync_Existing_ReturnsAccount()
    {
        // Arrange
        var account = new ConnectedAccount { TenantId = _tenantId, Email = "test@t.com", Type = "express" };
        _dbContext.ConnectedAccounts.Add(account);
        await _dbContext.SaveChangesAsync();
        var service = CreateService();

        // Act
        var result = await service.GetAccountAsync(account.Id);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data!.Email.Should().Be("test@t.com");
    }

    [Fact]
    public async Task GetAccountAsync_NotFound_ReturnsError()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.GetAccountAsync(Guid.NewGuid());

        // Assert
        result.IsValid.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateTransferAsync_Succeeds()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var service = CreateService();
        var request = new CreateTransferDto
        {
            ConnectedAccountId = accountId, Amount = 5000, Currency = "usd", Description = "Payout"
        };

        // Act
        var result = await service.CreateTransferAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data!.Amount.Should().Be(5000);
        result.Data.Status.Should().Be("pending");
        _dbContext.TransferRecords.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetTransfersAsync_ReturnsOnlyTenantTransfers()
    {
        // Arrange
        _dbContext.TransferRecords.AddRange(
            new TransferRecord { TenantId = _tenantId, ConnectedAccountId = Guid.NewGuid(), Amount = 100 },
            new TransferRecord { TenantId = Guid.NewGuid(), ConnectedAccountId = Guid.NewGuid(), Amount = 200 }
        );
        await _dbContext.SaveChangesAsync();
        var service = CreateService();

        // Act
        var result = await service.GetTransfersAsync();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data!.Should().HaveCount(1);
        result.Data[0].Amount.Should().Be(100);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
