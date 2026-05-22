using FluentAssertions;
using Moq;
using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.Services;
using Core.Utils;
using Xunit;

namespace StripeBilling.Tests.Unit.Services;

public class TaxServiceTests
{
    private readonly Mock<ITenantContextProvider> _tenantContextMock = new();
    private readonly Mock<ITaxRepository> _taxRepoMock = new();
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    private TaxService CreateService()
    {
        _tenantContextMock.Setup(x => x.GetCurrentTenantContext()).Returns(new TenantContext
        {
            TenantId = _tenantId,
            UserId = _userId,
            Role = "Admin"
        });
        return new TaxService(_tenantContextMock.Object, _taxRepoMock.Object);
    }

    [Fact]
    public async Task GetConfigurationAsync_NoExisting_CreatesDefault()
    {
        // Arrange
        _taxRepoMock.Setup(r => r.GetConfigAsync(_tenantId)).ReturnsAsync((TaxConfiguration?)null);
        TaxConfiguration? created = null;
        _taxRepoMock.Setup(r => r.CreateConfigAsync(It.IsAny<TaxConfiguration>()))
            .Callback<TaxConfiguration>(c => created = c)
            .ReturnsAsync(Guid.NewGuid());
        var service = CreateService();

        // Act
        var result = await service.GetConfigurationAsync();

        // Assert
        result.IsValid.Should().BeTrue();
        created.Should().NotBeNull();
        created!.TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public async Task GetConfigurationAsync_Existing_ReturnsIt()
    {
        // Arrange
        var config = new TaxConfiguration { TenantId = _tenantId, Provider = "taxjar", IsEnabled = true };
        _taxRepoMock.Setup(r => r.GetConfigAsync(_tenantId)).ReturnsAsync(config);
        var service = CreateService();

        // Act
        var result = await service.GetConfigurationAsync();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data!.TaxProvider.Should().Be("taxjar");
        result.Data.AutomaticTax.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateConfigurationAsync_UpdatesFields()
    {
        // Arrange
        var config = new TaxConfiguration { TenantId = _tenantId, Provider = "stripe_tax", IsEnabled = false };
        _taxRepoMock.Setup(r => r.GetConfigAsync(_tenantId)).ReturnsAsync(config);
        _taxRepoMock.Setup(r => r.UpdateConfigAsync(It.IsAny<TaxConfiguration>())).Returns(Task.CompletedTask);
        var service = CreateService();
        var request = new UpdateTaxConfigurationDto { TaxProvider = "avalara", AutomaticTax = true, DefaultTaxBehavior = "inclusive" };

        // Act
        var result = await service.UpdateConfigurationAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data!.TaxProvider.Should().Be("avalara");
        result.Data.AutomaticTax.Should().BeTrue();
        result.Data.DefaultTaxBehavior.Should().Be("inclusive");
    }

    [Fact]
    public async Task PreviewTaxAsync_CalculatesCorrectly()
    {
        // Arrange
        var config = new TaxConfiguration { TenantId = _tenantId, FallbackTaxRate = 0.1m };
        _taxRepoMock.Setup(r => r.GetConfigAsync(_tenantId)).ReturnsAsync(config);
        var service = CreateService();
        var request = new TaxPreviewRequestDto { Amount = 100m };

        // Act
        var result = await service.PreviewTaxAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data!.Subtotal.Should().Be(100m);
        result.Data.TaxAmount.Should().Be(10m);
        result.Data.Total.Should().Be(110m);
    }

    [Fact]
    public async Task SetCustomerTaxExemptAsync_CreatesExemption()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        TaxExemption? saved = null;
        _taxRepoMock.Setup(r => r.CreateExemptionAsync(It.IsAny<TaxExemption>()))
            .Callback<TaxExemption>(e => saved = e)
            .ReturnsAsync(Guid.NewGuid());
        var service = CreateService();

        // Act
        var result = await service.SetCustomerTaxExemptAsync(customerId, new SetCustomerTaxExemptDto { TaxExempt = "exempt" });

        // Assert
        result.IsValid.Should().BeTrue();
        saved.Should().NotBeNull();
        saved!.CustomerId.Should().Be(customerId);
        saved.TenantId.Should().Be(_tenantId);
        saved.ExemptionType.Should().Be("exempt");
    }

    [Fact]
    public async Task RemoveTaxIdAsync_CallsDeleteExemption()
    {
        // Arrange
        var exemptionId = Guid.NewGuid();
        _taxRepoMock.Setup(r => r.DeleteExemptionAsync(_tenantId, exemptionId)).Returns(Task.CompletedTask);
        var service = CreateService();

        // Act
        var result = await service.RemoveTaxIdAsync(Guid.NewGuid(), exemptionId);

        // Assert
        result.IsValid.Should().BeTrue();
        _taxRepoMock.Verify(r => r.DeleteExemptionAsync(_tenantId, exemptionId), Times.Once);
    }
}
