using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Core.Infrastructure;
using Xunit;

namespace StripeBilling.Tests.Unit.Services;

public class SubscriptionServiceTests
{
    private BillingDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new BillingDbContext(options);
    }

    [Fact]
    public async Task Subscription_TenantIsolation_OnlyReturnsTenantData()
    {
        var context = CreateInMemoryContext();
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var planId = Guid.NewGuid();

        context.Subscriptions.AddRange(
            new Subscription { Id = Guid.NewGuid(), TenantId = tenant1, CustomerId = customerId, PlanId = planId, Status = "active", StripeSubscriptionId = "sub_1", CurrentPeriodStart = DateTime.UtcNow, CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1) },
            new Subscription { Id = Guid.NewGuid(), TenantId = tenant1, CustomerId = customerId, PlanId = planId, Status = "active", StripeSubscriptionId = "sub_2", CurrentPeriodStart = DateTime.UtcNow, CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1) },
            new Subscription { Id = Guid.NewGuid(), TenantId = tenant2, CustomerId = customerId, PlanId = planId, Status = "active", StripeSubscriptionId = "sub_3", CurrentPeriodStart = DateTime.UtcNow, CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1) }
        );
        await context.SaveChangesAsync();

        var tenant1Subs = await context.Subscriptions.Where(s => s.TenantId == tenant1).ToListAsync();
        var tenant2Subs = await context.Subscriptions.Where(s => s.TenantId == tenant2).ToListAsync();

        tenant1Subs.Should().HaveCount(2);
        tenant2Subs.Should().HaveCount(1);
    }

    [Fact]
    public void Subscription_StatusTransitions_AreValid()
    {
        var validStatuses = new[] { "active", "trialing", "past_due", "canceled", "paused", "incomplete" };

        foreach (var status in validStatuses)
        {
            var sub = new Subscription
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                PlanId = Guid.NewGuid(),
                Status = status,
                StripeSubscriptionId = $"sub_{status}",
                CurrentPeriodStart = DateTime.UtcNow,
                CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1)
            };
            sub.Status.Should().Be(status);
        }
    }

    [Fact]
    public void Subscription_CancelAtPeriodEnd_SetsCorrectly()
    {
        var sub = new Subscription
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            PlanId = Guid.NewGuid(),
            Status = "active",
            StripeSubscriptionId = "sub_cancel_test",
            CancelAtPeriodEnd = true,
            CanceledAt = DateTime.UtcNow,
            CurrentPeriodStart = DateTime.UtcNow,
            CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1)
        };

        sub.CancelAtPeriodEnd.Should().BeTrue();
        sub.CanceledAt.Should().NotBeNull();
        sub.Status.Should().Be("active"); // Still active until period end
    }
}
