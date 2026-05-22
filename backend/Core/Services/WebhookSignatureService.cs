using System.Security.Cryptography;
using System.Text;

namespace Core.Services
{
    public static class WebhookSignatureService
    {
        public static string Sign(string payload, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return "sha256=" + Convert.ToHexString(hash).ToLower();
        }

        public static bool Verify(string payload, string secret, string signature)
        {
            var expected = Sign(payload, secret);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expected),
                Encoding.UTF8.GetBytes(signature));
        }
    }
}
