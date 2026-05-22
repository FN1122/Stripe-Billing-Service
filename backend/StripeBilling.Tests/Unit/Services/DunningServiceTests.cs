using FluentAssertions;
using Moq;
using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.Services;
using Xunit;

namespace StripeBilling.Tests.Unit.Services;

public class DunningServiceTests
{
    private readonly Mock<ITenantContextProvider> _tenantContextMock = new();
    private readonly Mock<IDunningRepository> _dunningRepoMock = new();
    private readonly Guid _tenantId = Guid.NewGuid();

    private DunningService CreateService()
    {
        _tenantContextMock.Setup(x => x.GetCurrentTenantContext()).Returns(new TenantContext
        {
            TenantId = _tenantId,
            UserId = Guid.NewGuid(),
            Role = "Admin"
        });
        _dunningRepoMock.Setup(r => r.UpdateAsync(It.IsAny<DunningSchedule>())).Returns(Task.CompletedTask);
        _dunningRepoMock.Setup(r => r.CreateAsync(It.IsAny<DunningSchedule>())).ReturnsAsync(Guid.NewGuid());
        return new DunningService(_tenantContextMock.Object, _dunningRepoMock.Object);
    }

    [Fact]
    public async Task PauseSchedule_ActiveSchedule_SetsPausedStatus()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        var schedule = new DunningSchedule { Id = scheduleId, TenantId = _tenantId, Status = "active" };
        _dunningRepoMock.Setup(r => r.GetByIdAsync(_tenantId, scheduleId)).ReturnsAsync(schedule);
        var service = CreateService();

        // Act
        var result = await service.PauseScheduleAsync(scheduleId);

        // Assert
        result.IsValid.Should().BeTrue();
        schedule.Status.Should().Be("paused");
        _dunningRepoMock.Verify(r => r.UpdateAsync(It.Is<DunningSchedule>(s => s.Status == "paused")), Times.Once);
    }

    [Fact]
    public async Task ResumeSchedule_PausedSchedule_SetsActiveStatus()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        var schedule = new DunningSchedule { Id = scheduleId, TenantId = _tenantId, Status = "paused" };
        _dunningRepoMock.Setup(r => r.GetByIdAsync(_tenantId, scheduleId)).ReturnsAsync(schedule);
        var service = CreateService();

        // Act
        var result = await service.ResumeScheduleAsync(scheduleId);

        // Assert
        result.IsValid.Should().BeTrue();
        schedule.Status.Should().Be("active");
    }

    [Fact]
    public async Task ResumeSchedule_ActiveSchedule_ReturnsError()
    {
        // Arrange - schedule is active, not paused
        var scheduleId = Guid.NewGuid();
        var schedule = new DunningSchedule { Id = scheduleId, TenantId = _tenantId, Status = "active" };
        _dunningRepoMock.Setup(r => r.GetByIdAsync(_tenantId, scheduleId)).ReturnsAsync(schedule);
        var service = CreateService();

        // Act
        var result = await service.ResumeScheduleAsync(scheduleId);

        // Assert
        result.IsValid.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CancelSchedule_SetsStatusToCancelled()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = new DunningSchedule { Id = scheduleId, TenantId = _tenantId, Status = "active" };
        _dunningRepoMock.Setup(r => r.GetByIdAsync(_tenantId, scheduleId)).ReturnsAsync(schedule);
        var service = CreateService();

        var result = await service.CancelScheduleAsync(scheduleId);

        result.IsValid.Should().BeTrue();
        schedule.Status.Should().Be("cancelled");
    }

    [Fact]
    public async Task PauseSchedule_NotFound_ReturnsError()
    {
        _dunningRepoMock.Setup(r => r.GetByIdAsync(_tenantId, It.IsAny<Guid>())).ReturnsAsync((DunningSchedule?)null);
        var service = CreateService();

        var result = await service.PauseScheduleAsync(Guid.NewGuid());

        result.IsValid.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task ManualRetry_IncrementsRetryCount()
    {
        var scheduleId = Guid.NewGuid();
        var schedule = new DunningSchedule { Id = scheduleId, TenantId = _tenantId, Status = "active", TotalRetryAttempts = 2 };
        _dunningRepoMock.Setup(r => r.GetByIdAsync(_tenantId, scheduleId)).ReturnsAsync(schedule);
        var service = CreateService();

        await service.ManualRetryAsync(scheduleId);

        schedule.TotalRetryAttempts.Should().Be(3);
        schedule.LastRetryAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetConfig_NoStepsConfigured_ReturnsDefaults()
    {
        _dunningRepoMock.Setup(r => r.GetStepsAsync(_tenantId)).ReturnsAsync(new List<DunningStep>());
        var service = CreateService();

        var result = await service.GetConfigAsync();

        result.IsValid.Should().BeTrue();
        result.Data!.Steps.Should().HaveCount(5); // default 5 steps
        result.Data.Steps[0].Action.Should().Be("retry_payment");
        result.Data.Steps[4].Action.Should().Be("cancel_subscription");
    }

    [Fact]
    public async Task InitiateDunning_ExistingScheduleForSubscription_SkipsCreation()
    {
        var subscriptionId = Guid.NewGuid();
        _dunningRepoMock.Setup(r => r.GetBySubscriptionAsync(_tenantId, subscriptionId))
            .ReturnsAsync(new DunningSchedule());
        var service = CreateService();

        await service.InitiateDunningAsync(_tenantId, subscriptionId, Guid.NewGuid(), null, 100, null);

        _dunningRepoMock.Verify(r => r.CreateAsync(It.IsAny<DunningSchedule>()), Times.Never);
    }
}
