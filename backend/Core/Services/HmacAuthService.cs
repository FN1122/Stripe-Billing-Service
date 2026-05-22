using Core.ContextProviders;
using Core.ServiceContracts;
using System.Security.Cryptography;
using System.Text;

namespace Core.Services
{
    public interface IHmacAuthService
    {
        bool VerifySignature(string body, string timestamp, string signature, string secretKey);
        string ComputeSignature(string body, string timestamp, string secretKey);
    }

    public class HmacAuthService : BaseService, IHmacAuthService
    {
        public HmacAuthService(ITenantContextProvider tenantContextProvider) : base(tenantContextProvider) { }

        public string ComputeSignature(string body, string timestamp, string secretKey)
        {
            var payload = $"{body}|{timestamp}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToHexString(hash).ToLower();
        }

        public bool VerifySignature(string body, string timestamp, string signature, string secretKey)
        {
            var expected = ComputeSignature(body, timestamp, secretKey);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expected),
                Encoding.UTF8.GetBytes(signature));
        }
    }
}
