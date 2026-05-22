using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Core.Infrastructure;
using Xunit;

namespace StripeBilling.Tests.Unit.Services;

public class RevenueAnalyticsServiceTests
{
    private BillingDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new BillingDbContext(options);
    }

    [Fact]
    public async Task MrrCalculation_SumsActiveSubscriptionAmounts()
    {
        var context = CreateInMemoryContext();
        var tenantId = Guid.NewGuid();

        var plan1Id = Guid.NewGuid();
        var plan2Id = Guid.NewGuid();
        context.SubscriptionPlans.AddRange(
            new SubscriptionPlan { Id = plan1Id, TenantId = tenantId, Name = "Basic", Amount = 29.99m, Currency = "usd", Interval = "month", IntervalCount = 1, IsActive = true },
            new SubscriptionPlan { Id = plan2Id, TenantId = tenantId, Name = "Pro", Amount = 99.99m, Currency = "usd", Interval = "month", IntervalCount = 1, IsActive = true }
        );

        var custId = Guid.NewGuid();
        context.Subscriptions.AddRange(
            new Subscription { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = custId, PlanId = plan1Id, Status = "active", StripeSubscriptionId = "sub_mrr1", CurrentPeriodStart = DateTime.UtcNow, CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1) },
            new Subscription { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = custId, PlanId = plan1Id, Status = "active", StripeSubscriptionId = "sub_mrr2", CurrentPeriodStart = DateTime.UtcNow, CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1) },
            new Subscription { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = custId, PlanId = plan2Id, Status = "active", StripeSubscriptionId = "sub_mrr3", CurrentPeriodStart = DateTime.UtcNow, CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1) },
            new Subscription { Id = Guid.NewGuid(), TenantId = tenantId, CustomerId = custId, PlanId = plan1Id, Status = "canceled", StripeSubscriptionId = "sub_mrr4", CurrentPeriodStart = DateTime.UtcNow, CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1) }
        );
        await context.SaveChangesAsync();

        var activeSubs = await context.Subscriptions
            .Where(s => s.TenantId == tenantId && s.Status == "active")
            .Include(s => s.Plan)
            .ToListAsync();

        var mrr = activeSubs.Sum(s => s.Plan?.Amount ?? 0);
        mrr.Should().Be(159.97m); // 29.99 + 29.99 + 99.99
    }

    [Fact]
    public async Task ChurnRate_CalculatesCorrectly()
    {
        var context = CreateInMemoryContext();
        var tenantId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var custId = Guid.NewGuid();
        var startOfMonth = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // 10 active at start, 2 canceled during month
        var subs = Enumerable.Range(1, 10).Select(i => new Subscription
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CustomerId = custId,
            PlanId = planId,
            Status = i <= 2 ? "canceled" : "active",
            CanceledAt = i <= 2 ? startOfMonth.AddDays(15) : null,
            StripeSubscriptionId = $"sub_churn_{i}",
            CurrentPeriodStart = startOfMonth,
            CurrentPeriodEnd = startOfMonth.AddMonths(1),
            CreatedAt = startOfMonth.AddDays(-30)
        }).ToList();

        context.Subscriptions.AddRange(subs);
        await context.SaveChangesAsync();

        var total = await context.Subscriptions
            .Where(s => s.TenantId == tenantId && s.CreatedAt < startOfMonth.AddMonths(1))
            .CountAsync();
        var canceled = await context.Subscriptions
            .Where(s => s.TenantId == tenantId && s.Status == "canceled" && s.CanceledAt >= startOfMonth)
            .CountAsync();

        var churnRate = total > 0 ? (decimal)canceled / total * 100 : 0;
        churnRate.Should().Be(20.0m); // 2/10 = 20%
    }
}
