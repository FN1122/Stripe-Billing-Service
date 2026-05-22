using Core.ContextProviders;
using Core.ServiceContracts;
using Microsoft.AspNetCore.DataProtection;

namespace Core.Services
{
    public class EncryptionService : BaseService, IEncryptionService
    {
        private readonly IDataProtector _protector;

        public EncryptionService(ITenantContextProvider tenantContextProvider, IDataProtectionProvider dataProtectionProvider) : base(tenantContextProvider)
        {
            _protector = dataProtectionProvider.CreateProtector("StripeBilling.ServiceCredentials");
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;
            return _protector.Protect(plainText);
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;
            return _protector.Unprotect(cipherText);
        }
    }
}
