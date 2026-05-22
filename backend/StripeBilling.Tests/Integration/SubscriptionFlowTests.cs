using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Core.Infrastructure;
using Xunit;

namespace StripeBilling.Tests.Integration;

public class SubscriptionFlowTests
{
    private BillingDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new BillingDbContext(options);
    }

    [Fact]
    public async Task SubscriptionLifecycle_CreateTrialActivateCancel()
    {
        var context = CreateInMemoryContext();
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var planId = Guid.NewGuid();

        // Create plan
        context.SubscriptionPlans.Add(new SubscriptionPlan
        {
            Id = planId,
            TenantId = tenantId,
            Name = "Pro Plan",
            Amount = 49.99m,
            Currency = "usd",
            Interval = "month",
            IntervalCount = 1,
            TrialPeriodDays = 14,
            IsActive = true
        });

        // Create subscription in trialing
        var subId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        context.Subscriptions.Add(new Subscription
        {
            Id = subId,
            TenantId = tenantId,
            CustomerId = customerId,
            PlanId = planId,
            Status = "trialing",
            StripeSubscriptionId = "sub_lifecycle_test",
            TrialStart = now,
            TrialEnd = now.AddDays(14),
            CurrentPeriodStart = now,
            CurrentPeriodEnd = now.AddDays(14),
            CreatedAt = now
        });
        await context.SaveChangesAsync();

        // Verify trial
        var sub = await context.Subscriptions.FindAsync(subId);
        sub!.Status.Should().Be("trialing");
        sub.TrialEnd.Should().BeAfter(now);

        // Activate after trial
        sub.Status = "active";
        sub.CurrentPeriodStart = now.AddDays(14);
        sub.CurrentPeriodEnd = now.AddDays(44); // ~1 month after trial
        await context.SaveChangesAsync();

        sub = await context.Subscriptions.FindAsync(subId);
        sub!.Status.Should().Be("active");

        // Cancel at period end
        sub.CancelAtPeriodEnd = true;
        sub.CanceledAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        sub = await context.Subscriptions.FindAsync(subId);
        sub!.CancelAtPeriodEnd.Should().BeTrue();
        sub.CanceledAt.Should().NotBeNull();
        sub.Status.Should().Be("active"); // Still active until period end
    }

    [Fact]
    public async Task PlanChange_Upgrade_UpdatesSubscription()
    {
        var context = CreateInMemoryContext();
        var tenantId = Guid.NewGuid();
        var basicPlanId = Guid.NewGuid();
        var proPlanId = Guid.NewGuid();

        context.SubscriptionPlans.AddRange(
            new SubscriptionPlan { Id = basicPlanId, TenantId = tenantId, Name = "Basic", Amount = 9.99m, Currency = "usd", Interval = "month", IntervalCount = 1, IsActive = true },
            new SubscriptionPlan { Id = proPlanId, TenantId = tenantId, Name = "Pro", Amount = 49.99m, Currency = "usd", Interval = "month", IntervalCount = 1, IsActive = true }
        );

        var subId = Guid.NewGuid();
        context.Subscriptions.Add(new Subscription
        {
            Id = subId,
            TenantId = tenantId,
            CustomerId = Guid.NewGuid(),
            PlanId = basicPlanId,
            Status = "active",
            StripeSubscriptionId = "sub_upgrade_test",
            CurrentPeriodStart = DateTime.UtcNow,
            CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1)
        });
        await context.SaveChangesAsync();

        // Upgrade
        var sub = await context.Subscriptions.FindAsync(subId);
        sub!.PlanId = proPlanId;
        await context.SaveChangesAsync();

        sub = await context.Subscriptions.Include(s => s.Plan).FirstAsync(s => s.Id == subId);
        sub.Plan!.Name.Should().Be("Pro");
        sub.Plan.Amount.Should().Be(49.99m);
    }
}
