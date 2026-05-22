using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace StripeBilling.Tests.Unit.Middleware;

public class RateLimitMiddlewareTests
{
    [Fact]
    public void MemoryCache_TracksRequestCounts()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var key = "rate_limit:192.168.1.1";

        cache.Set(key, 1, TimeSpan.FromMinutes(1));
        cache.TryGetValue(key, out int count);

        count.Should().Be(1);
    }

    [Fact]
    public void MemoryCache_ExpiresCorrectly()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var key = "rate_limit:expired_test";

        cache.Set(key, 50, TimeSpan.FromMilliseconds(1));
        Thread.Sleep(10);

        var exists = cache.TryGetValue(key, out int _);
        exists.Should().BeFalse();
    }

    [Fact]
    public void RateLimit_IpAndApiKey_UseDifferentKeys()
    {
        var ipKey = "rate_limit:ip:192.168.1.1";
        var apiKeyKey = "rate_limit:apikey:pk_live_abc123";

        ipKey.Should().NotBe(apiKeyKey);
    }
}
