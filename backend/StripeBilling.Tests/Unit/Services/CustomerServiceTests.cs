using FluentAssertions;
using Moq;
using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.ServiceContracts;
using Core.Services;
using Core.Utils;
using Xunit;

namespace StripeBilling.Tests.Unit.Services;

public class CustomerServiceTests
{
    private readonly Mock<ITenantContextProvider> _tenantContextMock = new();
    private readonly Mock<ICustomerRepository> _customerRepoMock = new();
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IEncryptionService> _encryptionMock = new();
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    private CustomerService CreateService()
    {
        _tenantContextMock.Setup(x => x.GetCurrentTenantContext()).Returns(new TenantContext
        {
            TenantId = _tenantId,
            UserId = _userId,
            Role = "Admin"
        });
        // Return a tenant without Stripe key so Stripe calls are skipped
        _tenantRepoMock.Setup(r => r.GetByIdAsync(_tenantId)).ReturnsAsync(new Tenant { Id = _tenantId });
        return new CustomerService(_tenantContextMock.Object, _customerRepoMock.Object, _tenantRepoMock.Object, _encryptionMock.Object);
    }

    [Fact]
    public async Task CreateAsync_NewCustomer_Succeeds()
    {
        // Arrange
        _customerRepoMock.Setup(r => r.GetByEmailAsync(_tenantId, It.IsAny<string>())).ReturnsAsync((Customer?)null);
        Customer? saved = null;
        _customerRepoMock.Setup(r => r.CreateAsync(It.IsAny<Customer>()))
            .Callback<Customer>(c => saved = c)
            .ReturnsAsync(Guid.NewGuid());
        var service = CreateService();
        var request = new CreateCustomerDto { Email = "test@example.com", Name = "Test User", Currency = "usd" };

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be("test@example.com");
        result.Data.Name.Should().Be("Test User");
        saved.Should().NotBeNull();
        saved!.TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ReturnsError()
    {
        // Arrange
        _customerRepoMock.Setup(r => r.GetByEmailAsync(_tenantId, "dup@test.com"))
            .ReturnsAsync(new Customer { Email = "dup@test.com" });
        var service = CreateService();

        // Act
        var result = await service.CreateAsync(new CreateCustomerDto { Email = "dup@test.com", Name = "Dup" });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task GetAsync_ExistingCustomer_ReturnsDetail()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId, TenantId = _tenantId, Email = "c@test.com", Name = "Customer",
            Subscriptions = new List<Subscription>(), Transactions = new List<PaymentTransaction>(), Invoices = new List<Invoice>()
        };
        _customerRepoMock.Setup(r => r.GetByIdWithDetailsAsync(_tenantId, customerId)).ReturnsAsync(customer);
        var service = CreateService();

        // Act
        var result = await service.GetAsync(customerId);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data!.Email.Should().Be("c@test.com");
    }

    [Fact]
    public async Task GetAsync_NotFound_ReturnsError()
    {
        // Arrange
        _customerRepoMock.Setup(r => r.GetByIdWithDetailsAsync(_tenantId, It.IsAny<Guid>())).ReturnsAsync((Customer?)null);
        var service = CreateService();

        // Act
        var result = await service.GetAsync(Guid.NewGuid());

        // Assert
        result.IsValid.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateAsync_ExistingCustomer_UpdatesFields()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId, TenantId = _tenantId, Email = "old@test.com", Name = "Old Name",
            Subscriptions = new List<Subscription>(), Transactions = new List<PaymentTransaction>(), Invoices = new List<Invoice>()
        };
        _customerRepoMock.Setup(r => r.GetByIdWithDetailsAsync(_tenantId, customerId)).ReturnsAsync(customer);
        _customerRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Customer>())).Returns(Task.CompletedTask);
        var service = CreateService();

        // Act
        var result = await service.UpdateAsync(customerId, new UpdateCustomerDto { Name = "New Name", Email = "new@test.com" });

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data!.Name.Should().Be("New Name");
        result.Data.Email.Should().Be("new@test.com");
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsError()
    {
        // Arrange
        _customerRepoMock.Setup(r => r.GetByIdWithDetailsAsync(_tenantId, It.IsAny<Guid>())).ReturnsAsync((Customer?)null);
        var service = CreateService();

        // Act
        var result = await service.UpdateAsync(Guid.NewGuid(), new UpdateCustomerDto { Name = "X" });

        // Assert
        result.IsValid.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }
}
