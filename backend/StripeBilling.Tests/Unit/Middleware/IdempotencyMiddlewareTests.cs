using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Core.Infrastructure;
using StripeBilling.API.Middleware;
using Xunit;

namespace StripeBilling.Tests.Unit.Middleware;

public class IdempotencyMiddlewareTests
{
    private readonly Mock<ILogger<IdempotencyMiddleware>> _loggerMock = new();

    private BillingDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new BillingDbContext(options);
    }

    [Fact]
    public async Task GetRequest_SkipsIdempotencyCheck()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new IdempotencyMiddleware(next, _loggerMock.Object);

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task PostRequest_WithoutIdempotencyHeader_SkipsCheck()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new IdempotencyMiddleware(next, _loggerMock.Object);

        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        // No Idempotency-Key header

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task PostRequest_WithCachedKey_ReturnsCachedResponse()
    {
        // Arrange
        var dbContext = CreateInMemoryContext();
        var key = "test-idempotency-key";
        var cachedEntry = new IdempotencyKey
        {
            Key = key,
            TenantId = Guid.NewGuid(),
            HttpMethod = "POST",
            Endpoint = "/api/v1/credits",
            ResponseStatusCode = 200,
            ResponseBody = "{\"isValid\":true}",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        dbContext.Set<IdempotencyKey>().Add(cachedEntry);
        await dbContext.SaveChangesAsync();

        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new IdempotencyMiddleware(next, _loggerMock.Object);

        var services = new ServiceCollection();
        services.AddSingleton(dbContext);
        var serviceProvider = services.BuildServiceProvider();

        var context = new DefaultHttpContext { RequestServices = serviceProvider };
        context.Request.Method = "POST";
        context.Request.Headers["Idempotency-Key"] = key;
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeFalse(); // should NOT call next
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task DeleteRequest_SkipsIdempotencyCheck()
    {
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new IdempotencyMiddleware(next, _loggerMock.Object);

        var context = new DefaultHttpContext();
        context.Request.Method = "DELETE";
        context.Request.Headers["Idempotency-Key"] = "some-key";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public void IdempotencyKey_DefaultExpiration_Is24Hours()
    {
        var key = new IdempotencyKey();

        // ExpiresAt should default to 24h from creation
        key.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(24), TimeSpan.FromSeconds(5));
    }
}
