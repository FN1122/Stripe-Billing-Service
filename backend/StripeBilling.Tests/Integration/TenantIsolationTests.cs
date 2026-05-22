using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Core.Infrastructure;
using Xunit;

namespace StripeBilling.Tests.Integration;

public class TenantIsolationTests
{
    private BillingDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new BillingDbContext(options);
    }

    [Fact]
    public async Task Customers_AreScopedByTenant()
    {
        var context = CreateInMemoryContext();
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();

        context.Customers.AddRange(
            new Customer { Id = Guid.NewGuid(), TenantId = tenant1, Email = "a@t1.com", Name = "Customer A", CreatedAt = DateTime.UtcNow },
            new Customer { Id = Guid.NewGuid(), TenantId = tenant1, Email = "b@t1.com", Name = "Customer B", CreatedAt = DateTime.UtcNow },
            new Customer { Id = Guid.NewGuid(), TenantId = tenant2, Email = "c@t2.com", Name = "Customer C", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var t1Customers = await context.Customers.Where(c => c.TenantId == tenant1).ToListAsync();
        var t2Customers = await context.Customers.Where(c => c.TenantId == tenant2).ToListAsync();

        t1Customers.Should().HaveCount(2);
        t2Customers.Should().HaveCount(1);
    }

    [Fact]
    public async Task PaymentTransactions_AreScopedByTenant()
    {
        var context = CreateInMemoryContext();
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();

        context.PaymentTransactions.AddRange(
            new PaymentTransaction { Id = Guid.NewGuid(), TenantId = tenant1, Amount = 100m, Currency = "usd", Status = "succeeded", StripePaymentIntentId = "pi_1", CreatedAt = DateTime.UtcNow },
            new PaymentTransaction { Id = Guid.NewGuid(), TenantId = tenant2, Amount = 200m, Currency = "usd", Status = "succeeded", StripePaymentIntentId = "pi_2", CreatedAt = DateTime.UtcNow },
            new PaymentTransaction { Id = Guid.NewGuid(), TenantId = tenant2, Amount = 300m, Currency = "usd", Status = "succeeded", StripePaymentIntentId = "pi_3", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var t1Total = await context.PaymentTransactions.Where(t => t.TenantId == tenant1).SumAsync(t => t.Amount);
        var t2Total = await context.PaymentTransactions.Where(t => t.TenantId == tenant2).SumAsync(t => t.Amount);

        t1Total.Should().Be(100m);
        t2Total.Should().Be(500m);
    }

    [Fact]
    public async Task AuditLogs_TrackTenantActions()
    {
        var context = CreateInMemoryContext();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        context.AuditLogs.AddRange(
            new AuditLog { Id = Guid.NewGuid(), TenantId = tenantId, UserId = userId, Action = "Customer.Created", EntityType = "Customer", EntityId = Guid.NewGuid().ToString(), CreatedAt = DateTime.UtcNow },
            new AuditLog { Id = Guid.NewGuid(), TenantId = tenantId, UserId = userId, Action = "Subscription.Created", EntityType = "Subscription", EntityId = Guid.NewGuid().ToString(), CreatedAt = DateTime.UtcNow },
            new AuditLog { Id = Guid.NewGuid(), TenantId = null, UserId = userId, Action = "Tenant.Created", EntityType = "Tenant", EntityId = Guid.NewGuid().ToString(), CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var tenantLogs = await context.AuditLogs.Where(a => a.TenantId == tenantId).ToListAsync();
        var superAdminLogs = await context.AuditLogs.Where(a => a.TenantId == null).ToListAsync();

        tenantLogs.Should().HaveCount(2);
        superAdminLogs.Should().HaveCount(1);
    }
}
