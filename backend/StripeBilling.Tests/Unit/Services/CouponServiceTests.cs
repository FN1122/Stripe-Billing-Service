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
using Coupon = Core.Infrastructure.Coupon;
using PromotionCode = Core.Infrastructure.PromotionCode;

namespace StripeBilling.Tests.Unit.Services;

public class CouponServiceTests
{
    private readonly Mock<ITenantContextProvider> _tenantContextMock = new();
    private readonly Mock<ICouponRepository> _couponRepoMock = new();
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IEncryptionService> _encryptionMock = new();
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    private CouponService CreateService()
    {
        _tenantContextMock.Setup(x => x.GetCurrentTenantContext()).Returns(new TenantContext
        {
            TenantId = _tenantId,
            UserId = _userId,
            Role = "Admin"
        });
        _tenantRepoMock.Setup(r => r.GetByIdAsync(_tenantId)).ReturnsAsync(new Tenant { Id = _tenantId });
        return new CouponService(_tenantContextMock.Object, _couponRepoMock.Object, _tenantRepoMock.Object, _encryptionMock.Object);
    }

    [Fact]
    public async Task CreateCouponAsync_PercentOff_Succeeds()
    {
        // Arrange
        Coupon? saved = null;
        _couponRepoMock.Setup(r => r.CreateAsync(It.IsAny<Coupon>()))
            .Callback<Coupon>(c => saved = c)
            .ReturnsAsync(Guid.NewGuid());
        var service = CreateService();
        var request = new CreateCouponDto { Name = "20% Off", Type = "percent_off", PercentOff = 20, Duration = "once" };

        // Act
        var result = await service.CreateCouponAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data!.Name.Should().Be("20% Off");
        result.Data.PercentOff.Should().Be(20);
        saved.Should().NotBeNull();
        saved!.TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public async Task GetCouponAsync_Existing_ReturnsCoupon()
    {
        // Arrange
        var couponId = Guid.NewGuid();
        var coupon = new Coupon { Id = couponId, TenantId = _tenantId, Name = "Test", Type = "percent_off", Duration = "once", PromotionCodes = new List<PromotionCode>() };
        _couponRepoMock.Setup(r => r.GetByIdWithDetailsAsync(_tenantId, couponId)).ReturnsAsync(coupon);
        var service = CreateService();

        // Act
        var result = await service.GetCouponAsync(couponId);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetCouponAsync_NotFound_ReturnsError()
    {
        // Arrange
        _couponRepoMock.Setup(r => r.GetByIdWithDetailsAsync(_tenantId, It.IsAny<Guid>())).ReturnsAsync((Coupon?)null);
        var service = CreateService();

        // Act
        var result = await service.GetCouponAsync(Guid.NewGuid());

        // Assert
        result.IsValid.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateCouponAsync_ExistingCoupon_UpdatesFields()
    {
        // Arrange
        var couponId = Guid.NewGuid();
        var coupon = new Coupon { Id = couponId, TenantId = _tenantId, Name = "Old", IsActive = true, PromotionCodes = new List<PromotionCode>() };
        _couponRepoMock.Setup(r => r.GetByIdAsync(_tenantId, couponId)).ReturnsAsync(coupon);
        _couponRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Coupon>())).Returns(Task.CompletedTask);
        var service = CreateService();

        // Act
        var result = await service.UpdateCouponAsync(couponId, new UpdateCouponDto { Name = "Updated" });

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task ToggleCouponAsync_Active_BecomesInactive()
    {
        // Arrange
        var couponId = Guid.NewGuid();
        var coupon = new Coupon { Id = couponId, TenantId = _tenantId, Name = "Toggle", IsActive = true, PromotionCodes = new List<PromotionCode>() };
        _couponRepoMock.Setup(r => r.GetByIdAsync(_tenantId, couponId)).ReturnsAsync(coupon);
        _couponRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Coupon>())).Returns(Task.CompletedTask);
        var service = CreateService();

        // Act
        var result = await service.ToggleCouponAsync(couponId);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Data!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCouponAsync_ExistingCoupon_SoftDeletes()
    {
        // Arrange
        var couponId = Guid.NewGuid();
        var coupon = new Coupon { Id = couponId, TenantId = _tenantId, Name = "Delete Me", IsActive = true };
        _couponRepoMock.Setup(r => r.GetByIdAsync(_tenantId, couponId)).ReturnsAsync(coupon);
        _couponRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Coupon>())).Returns(Task.CompletedTask);
        var service = CreateService();

        // Act
        var result = await service.DeleteCouponAsync(couponId);

        // Assert
        result.IsValid.Should().BeTrue();
        coupon.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCouponAsync_NotFound_ReturnsError()
    {
        // Arrange
        _couponRepoMock.Setup(r => r.GetByIdAsync(_tenantId, It.IsAny<Guid>())).ReturnsAsync((Coupon?)null);
        var service = CreateService();

        // Act
        var result = await service.DeleteCouponAsync(Guid.NewGuid());

        // Assert
        result.IsValid.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }
}
