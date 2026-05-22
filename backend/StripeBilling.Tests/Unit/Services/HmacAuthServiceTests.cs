using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Core.Services;
using Xunit;

namespace StripeBilling.Tests.Unit.Services;

public class HmacAuthServiceTests
{
    private readonly HmacAuthService _sut = new();
    private const string TestSecret = "test-webhook-signing-secret-key";

    [Fact]
    public void ComputeSignature_ReturnsCorrectHash()
    {
        var body = "{\"event\":\"test\"}";
        var timestamp = "1700000000";
        var expected = ComputeExpectedSignature(body, timestamp, TestSecret);

        var result = _sut.ComputeSignature(body, timestamp, TestSecret);

        result.Should().Be(expected);
    }

    [Fact]
    public void ComputeSignature_DifferentBodies_ProduceDifferentSignatures()
    {
        var timestamp = "1700000000";
        var sig1 = _sut.ComputeSignature("{\"a\":1}", timestamp, TestSecret);
        var sig2 = _sut.ComputeSignature("{\"b\":2}", timestamp, TestSecret);

        sig1.Should().NotBe(sig2);
    }

    [Fact]
    public void ComputeSignature_EmptyBody_ReturnsValidHash()
    {
        var result = _sut.ComputeSignature("", "1700000000", TestSecret);

        result.Should().NotBeNullOrWhiteSpace();
        result.Should().HaveLength(64); // SHA256 hex
    }

    [Fact]
    public void ComputeSignature_ConsistentResults()
    {
        var body = "{\"test\":true}";
        var timestamp = "1700000000";

        var result1 = _sut.ComputeSignature(body, timestamp, TestSecret);
        var result2 = _sut.ComputeSignature(body, timestamp, TestSecret);

        result1.Should().Be(result2);
    }

    private static string ComputeExpectedSignature(string body, string timestamp, string secret)
    {
        var payload = $"{body}|{timestamp}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
