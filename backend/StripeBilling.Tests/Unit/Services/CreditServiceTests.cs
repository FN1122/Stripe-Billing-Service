using FluentAssertions;
using Moq;
using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.Services;
using Xunit;

namespace StripeBilling.Tests.Unit.Services;

public class CreditServiceTests
{
    private readonly Mock<ITenantContextProvider> _tenantContextMock = new();
    private readonly Mock<ICreditRepository> _creditRepoMock = new();
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();

    private CreditService CreateService()
    {
        _tenantContextMock.Setup(x => x.GetCurrentTenantContext()).Returns(new TenantContext
        {
            TenantId = _tenantId,
            UserId = _userId,
            Role = "Admin"
        });
        return new CreditService(_tenantContextMock.Object, _creditRepoMock.Object);
    }

    [Fact]
    public async Task AddCredit_CalculatesNewBalanceCorrectly()
    {
        // Arrange
        var currentBalance = 5000m; // $50.00
        _creditRepoMock.Setup(r => r.GetBalanceAsync(_tenantId, _customerId)).ReturnsAsync(currentBalance);
        _creditRepoMock.Setup(r => r.CreateAsync(It.IsAny<CustomerCredit>())).ReturnsAsync(Guid.NewGuid());
        var service = CreateService();

        var request = new CreateCreditDto { Amount = 2500, Currency = "usd", Description = "Bonus credit" };

        // Act
        var result = await service.AddCreditAsync(_customerId, request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.BalanceAfter.Should().Be(7500m); // 5000 + 2500
        result.Data.Type.Should().Be("credit");
        result.Data.Amount.Should().Be(2500);
    }

    [Fact]
    public async Task AdjustBalance_NegativeAmount_CreatesDebitType()
    {
        // Arrange
        var currentBalance = 10000m;
        _creditRepoMock.Setup(r => r.GetBalanceAsync(_tenantId, _customerId)).ReturnsAsync(currentBalance);
        _creditRepoMock.Setup(r => r.CreateAsync(It.IsAny<CustomerCredit>())).ReturnsAsync(Guid.NewGuid());
        var service = CreateService();

        var request = new AdjustCreditDto { Amount = -3000, Description = "Manual deduction" };

        // Act
        var result = await service.AdjustBalanceAsync(_customerId, request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data!.Type.Should().Be("debit");
        result.Data.BalanceAfter.Should().Be(7000m); // 10000 - 3000
    }

    [Fact]
    public async Task AdjustBalance_PositiveAmount_CreatesCreditType()
    {
        // Arrange
        _creditRepoMock.Setup(r => r.GetBalanceAsync(_tenantId, _customerId)).ReturnsAsync(0m);
        _creditRepoMock.Setup(r => r.CreateAsync(It.IsAny<CustomerCredit>())).ReturnsAsync(Guid.NewGuid());
        var service = CreateService();

        var request = new AdjustCreditDto { Amount = 1500, Description = "Adjustment" };

        // Act
        var result = await service.AdjustBalanceAsync(_customerId, request);

        // Assert
        result.Data!.Type.Should().Be("credit");
    }

    [Fact]
    public async Task AddCredit_SetsCorrectTenantAndUser()
    {
        // Arrange
        _creditRepoMock.Setup(r => r.GetBalanceAsync(_tenantId, _customerId)).ReturnsAsync(0m);
        CustomerCredit? savedCredit = null;
        _creditRepoMock.Setup(r => r.CreateAsync(It.IsAny<CustomerCredit>()))
            .Callback<CustomerCredit>(c => savedCredit = c)
            .ReturnsAsync(Guid.NewGuid());
        var service = CreateService();

        // Act
        await service.AddCreditAsync(_customerId, new CreateCreditDto { Amount = 100, Currency = "usd" });

        // Assert
        savedCredit.Should().NotBeNull();
        savedCredit!.TenantId.Should().Be(_tenantId);
        savedCredit.CreatedBy.Should().Be(_userId);
        savedCredit.CustomerId.Should().Be(_customerId);
    }

    [Fact]
    public async Task RefundToCredit_ReturnsNotImplemented()
    {
        var service = CreateService();

        var result = await service.RefundToCreditAsync(new RefundToCreditDto());

        result.IsValid.Should().BeFalse();
        result.StatusCode.Should().Be(501);
    }
}
