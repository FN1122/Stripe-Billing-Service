using FluentAssertions;
using Core.Services;
using Xunit;

namespace StripeBilling.Tests.Unit.Services;

public class WebhookSignatureServiceTests
{
    [Fact]
    public void Sign_ReturnsValidHmacSha256()
    {
        var payload = "{\"event\":\"subscription.created\",\"data\":{}}";
        var secret = "whsec_test_signing_secret";

        var signature = WebhookSignatureService.Sign(payload, secret);

        signature.Should().NotBeNullOrWhiteSpace();
        signature.Should().HaveLength(64);
    }

    [Fact]
    public void Verify_ValidSignature_ReturnsTrue()
    {
        var payload = "{\"event\":\"payment.completed\"}";
        var secret = "whsec_verification_secret";

        var signature = WebhookSignatureService.Sign(payload, secret);
        var isValid = WebhookSignatureService.Verify(payload, secret, signature);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void Verify_InvalidSignature_ReturnsFalse()
    {
        var payload = "{\"event\":\"payment.completed\"}";
        var secret = "whsec_verification_secret";

        var isValid = WebhookSignatureService.Verify(payload, secret, "invalid_signature_here");

        isValid.Should().BeFalse();
    }

    [Fact]
    public void Verify_TamperedPayload_ReturnsFalse()
    {
        var originalPayload = "{\"amount\":100}";
        var tamperedPayload = "{\"amount\":999}";
        var secret = "whsec_tamper_test";

        var signature = WebhookSignatureService.Sign(originalPayload, secret);
        var isValid = WebhookSignatureService.Verify(tamperedPayload, secret, signature);

        isValid.Should().BeFalse();
    }

    [Fact]
    public void Sign_DifferentSecrets_ProduceDifferentSignatures()
    {
        var payload = "{\"test\":true}";

        var sig1 = WebhookSignatureService.Sign(payload, "secret_one");
        var sig2 = WebhookSignatureService.Sign(payload, "secret_two");

        sig1.Should().NotBe(sig2);
    }
}
