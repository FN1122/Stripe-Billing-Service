using FluentAssertions;
using Moq;
using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.Services;
using Core.ServiceContracts;
using Core.Utils;
using Xunit;

namespace StripeBilling.Tests.Unit.Services;

public class TenantServiceTests
{
    private readonly Mock<ITenantContextProvider> _tenantContextMock = new();
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IApiKeyRepository> _apiKeyRepoMock = new();
    private readonly Mock<IWebhookSubscriptionRepository> _webhookSubRepoMock = new();
    private readonly Mock<IEncryptionService> _encryptionMock = new();
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    private TenantService CreateService()
    {
        _tenantContextMock.Setup(x => x.GetCurrentTenantContext()).Returns(new TenantContext
        {
            TenantId = _tenantId,
            UserId = _userId,
            Role = "Admin"
        });
        _encryptionMock.Setup(e => e.Encrypt(It.IsAny<string>())).Returns("encrypted");
        return new TenantService(_tenantContextMock.Object, _tenantRepoMock.Object, _apiKeyRepoMock.Object, _webhookSubRepoMock.Object, _encryptionMock.Object);
    }

    [Fact]
    public async Task CreateAsync_NewTenant_Succeeds()
    {
        // Arrange
        _tenantRepoMock.Setup(r => r.GetByNameAsync(It.IsAny<string>())).ReturnsAsync((Tenant?)null);
        Tenant? saved = null;
        _tenantRepoMock.Setup(r => r.CreateAsync(It.IsAny<Tenant>()))
            .Callback<Tenant>(t => saved = t)
            .ReturnsAsync(Guid.NewGuid());
        var service = CreateService();

        // Act
        var result = await service.CreateAsync(new CreateTenantDto { Name = "Test Tenant", Description = "Desc" });

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Test Tenant");
        saved.Should().NotBeNull();
        saved!.PublicKey.Should().StartWith("pk_live_");
        saved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ReturnsError()
    {
        // Arrange
        _tenantRepoMock.Setup(r => r.GetByNameAsync("Existing")).ReturnsAsync(new Tenant { Name = "Existing" });
        var service = CreateService();

        // Act
        var result = await service.CreateAsync(new CreateTenantDto { Name = "Existing" });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task GetAsync_ExistingTenant_ReturnsDetail()
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = _tenantId, Name = "My Tenant", IsActive = true,
            ApiKeys = new List<ApiKey>(), WebhookSubscriptions = new List<WebhookSubscription>(), Users = new List<User>()
        };
        _tenantRepoMock.Setup(r => r.GetByIdWithDetailsAsync(_tenantId)).ReturnsAsync(tenant);
        var service = CreateService();

        // Act
        var result = await service.GetAsync(_tenantId);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data!.Name.Should().Be("My Tenant");
    }

    [Fact]
    public async Task GetAsync_NotFound_ReturnsError()
    {
        // Arrange
        _tenantRepoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>())).ReturnsAsync((Tenant?)null);
        var service = CreateService();

        // Act
        var result = await service.GetAsync(Guid.NewGuid());

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ExistingTenant_UpdatesFields()
    {
        // Arrange
        var tenant = new Tenant { Id = _tenantId, Name = "Old", Description = "Old Desc" };
        _tenantRepoMock.Setup(r => r.GetByIdAsync(_tenantId)).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Tenant>())).Returns(Task.CompletedTask);
        var service = CreateService();

        // Act — update Description only (avoids Query() call for name uniqueness check)
        var result = await service.UpdateAsync(_tenantId, new UpdateTenantDto { Description = "New Desc" });

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data!.Description.Should().Be("New Desc");
    }

    [Fact]
    public async Task SuspendAsync_ActiveTenant_SetsInactive()
    {
        // Arrange
        var tenant = new Tenant { Id = _tenantId, IsActive = true };
        _tenantRepoMock.Setup(r => r.GetByIdAsync(_tenantId)).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Tenant>())).Returns(Task.CompletedTask);
        var service = CreateService();

        // Act
        var result = await service.SuspendAsync(_tenantId, "Policy violation");

        // Assert
        result.IsValid.Should().BeTrue();
        tenant.IsActive.Should().BeFalse();
        tenant.SuspensionReason.Should().Be("Policy violation");
    }

    [Fact]
    public async Task ActivateAsync_SuspendedTenant_SetsActive()
    {
        // Arrange
        var tenant = new Tenant { Id = _tenantId, IsActive = false, SuspensionReason = "Test" };
        _tenantRepoMock.Setup(r => r.GetByIdAsync(_tenantId)).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Tenant>())).Returns(Task.CompletedTask);
        var service = CreateService();

        // Act
        var result = await service.ActivateAsync(_tenantId);

        // Assert
        result.IsValid.Should().BeTrue();
        tenant.IsActive.Should().BeTrue();
        tenant.SuspensionReason.Should().BeNull();
    }

    [Fact]
    public async Task RotateKeysAsync_GeneratesNewKeys()
    {
        // Arrange
        var tenant = new Tenant { Id = _tenantId, PublicKey = "pk_live_old" };
        _tenantRepoMock.Setup(r => r.GetByIdAsync(_tenantId)).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Tenant>())).Returns(Task.CompletedTask);
        var service = CreateService();

        // Act
        var result = await service.RotateKeysAsync(_tenantId);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data!.OldPublicKey.Should().Be("pk_live_old");
        result.Data.NewPublicKey.Should().StartWith("pk_live_");
        result.Data.NewPublicKey.Should().NotBe("pk_live_old");
    }
}
