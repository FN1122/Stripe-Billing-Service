using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.ServiceContracts;
using Core.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Core.Services
{
    public class TenantService : BaseService, ITenantService
    {
        private readonly ITenantRepository _tenantRepo;
        private readonly IApiKeyRepository _apiKeyRepo;
        private readonly IWebhookSubscriptionRepository _webhookSubRepo;
        private readonly IEncryptionService _encryption;

        public TenantService(ITenantContextProvider tcp, ITenantRepository tenantRepo, IApiKeyRepository apiKeyRepo, IWebhookSubscriptionRepository webhookSubRepo, IEncryptionService encryption) : base(tcp)
        {
            _tenantRepo = tenantRepo;
            _apiKeyRepo = apiKeyRepo;
            _webhookSubRepo = webhookSubRepo;
            _encryption = encryption;
        }

        public async Task<GatewayResponseWrapper<TenantResponseDto>> CreateAsync(CreateTenantDto request)
        {
            var response = new GatewayResponseWrapper<TenantResponseDto>();
            var existing = await _tenantRepo.GetByNameAsync(request.Name);
            if (existing != null) { response.SetError("Tenant with this name already exists."); return response; }

            var publicKey = GeneratePublicKey();
            var secretKey = GenerateSecretKey();
            var tenant = new Tenant
            {
                Name = request.Name, Description = request.Description, PublicKey = publicKey,
                StripeSecretKeyEnc = _encryption.Encrypt(secretKey), IsActive = true,
                Features = request.Features != null ? JsonConvert.SerializeObject(request.Features) : null,
                Metadata = request.Metadata != null ? JsonConvert.SerializeObject(request.Metadata) : null
            };
            await _tenantRepo.CreateAsync(tenant);
            response.SetSuccess(MapTenant(tenant));
            return response;
        }

        public async Task<GatewayResponseWrapper<TenantDetailResponseDto>> GetAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<TenantDetailResponseDto>();
            var tenant = await _tenantRepo.GetByIdWithDetailsAsync(id);
            if (tenant == null) { response.SetError("Tenant not found."); return response; }
            response.SetSuccess(new TenantDetailResponseDto
            {
                Id = tenant.Id, Name = tenant.Name, Description = tenant.Description, PublicKey = tenant.PublicKey,
                IsActive = tenant.IsActive, StripePublishableKey = !string.IsNullOrEmpty(tenant.StripePublishableKey) ? tenant.StripePublishableKey : null,
                CreatedAt = tenant.CreatedAt, UpdatedAt = tenant.UpdatedAt,
                Features = !string.IsNullOrEmpty(tenant.Features) ? JsonConvert.DeserializeObject<List<string>>(tenant.Features) : new(),
                ApiKeyCount = tenant.ApiKeys?.Count ?? 0, WebhookSubscriptionCount = tenant.WebhookSubscriptions?.Count ?? 0,
                UserCount = tenant.Users?.Count ?? 0,
                Metadata = !string.IsNullOrEmpty(tenant.Metadata) ? JsonConvert.DeserializeObject<dynamic>(tenant.Metadata) : null
            });
            return response;
        }

        public async Task<GatewayPaginatedListResponseWrapper<TenantResponseDto>> ListAsync(TenantFilterDto filter)
        {
            var response = new GatewayPaginatedListResponseWrapper<TenantResponseDto>();
            IQueryable<Tenant> query = _tenantRepo.Query()
                .Include(t => t.Customers)
                .Include(t => t.Subscriptions)
                .Include(t => t.PaymentTransactions);
            if (!string.IsNullOrEmpty(filter.Search)) query = query.Where(t => t.Name.Contains(filter.Search) || (t.Description != null && t.Description.Contains(filter.Search)));
            if (filter.IsActive.HasValue) query = query.Where(t => t.IsActive == filter.IsActive.Value);
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(t => t.CreatedAt).Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
            var mapped = items.Select(MapTenant).ToList();
            response.SetSuccessWithPagination(mapped, total, filter.Page, filter.PageSize);
            return response;
        }

        public async Task<GatewayResponseWrapper<TenantResponseDto>> UpdateAsync(Guid id, UpdateTenantDto request)
        {
            var response = new GatewayResponseWrapper<TenantResponseDto>();
            var tenant = await _tenantRepo.GetByIdAsync(id);
            if (tenant == null) { response.SetError("Tenant not found."); return response; }
            if (request.Name != null)
            {
                var existing = await _tenantRepo.Query().FirstOrDefaultAsync(t => t.Id != id && t.Name == request.Name);
                if (existing != null) { response.SetError("Tenant with this name already exists."); return response; }
                tenant.Name = request.Name;
            }
            if (request.Description != null) tenant.Description = request.Description;
            if (request.StripePublishableKey != null) tenant.StripePublishableKey = request.StripePublishableKey;
            if (request.StripeSecretKeyEnc != null) tenant.StripeSecretKeyEnc = _encryption.Encrypt(request.StripeSecretKeyEnc);
            if (request.Features != null) tenant.Features = JsonConvert.SerializeObject(request.Features);
            if (request.Metadata != null) tenant.Metadata = JsonConvert.SerializeObject(request.Metadata);
            tenant.UpdatedAt = DateTime.UtcNow;
            await _tenantRepo.UpdateAsync(tenant);
            response.SetSuccess(MapTenant(tenant));
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> SuspendAsync(Guid id, string reason)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenant = await _tenantRepo.GetByIdAsync(id);
            if (tenant == null) { response.SetError("Tenant not found."); return response; }
            tenant.IsActive = false; tenant.SuspendedAt = DateTime.UtcNow; tenant.SuspensionReason = reason;
            await _tenantRepo.UpdateAsync(tenant);
            response.SetSuccess(true, "Tenant suspended.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> ActivateAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenant = await _tenantRepo.GetByIdAsync(id);
            if (tenant == null) { response.SetError("Tenant not found."); return response; }
            tenant.IsActive = true; tenant.SuspendedAt = null; tenant.SuspensionReason = null;
            await _tenantRepo.UpdateAsync(tenant);
            response.SetSuccess(true, "Tenant activated.");
            return response;
        }

        public async Task<GatewayResponseWrapper<TenantKeyRotationResponseDto>> RotateKeysAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<TenantKeyRotationResponseDto>();
            var tenant = await _tenantRepo.GetByIdAsync(id);
            if (tenant == null) { response.SetError("Tenant not found."); return response; }
            var newPublicKey = GeneratePublicKey(); var newSecretKey = GenerateSecretKey(); var oldPublicKey = tenant.PublicKey;
            tenant.PublicKey = newPublicKey; tenant.StripeSecretKeyEnc = _encryption.Encrypt(newSecretKey);
            tenant.KeyRotationCount = (tenant.KeyRotationCount ?? 0) + 1; tenant.LastKeyRotationAt = DateTime.UtcNow;
            await _tenantRepo.UpdateAsync(tenant);
            response.SetSuccess(new TenantKeyRotationResponseDto { Id = tenant.Id, OldPublicKey = oldPublicKey, NewPublicKey = newPublicKey, RotationTime = DateTime.UtcNow, Message = "Keys rotated successfully. Update your client applications with the new public key." });
            return response;
        }

        public async Task<GatewayResponseWrapper<TenantHealthCheckDto>> GetHealthCheckAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<TenantHealthCheckDto>();
            var tenant = await _tenantRepo.GetByIdWithCollectionsAsync(id);
            if (tenant == null) { response.SetError("Tenant not found."); return response; }
            var last24h = DateTime.UtcNow.AddDays(-1);
            var recentTransactions = tenant.PaymentTransactions?.Where(t => t.CreatedAt >= last24h).ToList() ?? new();
            var recentSubs = tenant.Subscriptions?.Where(s => s.CreatedAt >= last24h).ToList() ?? new();
            var health = new TenantHealthCheckDto
            {
                TenantId = tenant.Id, Status = tenant.IsActive ? "healthy" : "suspended",
                IsStripeConfigured = !string.IsNullOrEmpty(tenant.StripeSecretKeyEnc),
                CustomerCount = tenant.Customers?.Count ?? 0,
                SubscriptionCount = tenant.Subscriptions?.Count(s => s.Status == "active") ?? 0,
                TransactionCount24h = recentTransactions.Count,
                SubscriptionCreations24h = recentSubs.Count,
                SuccessRate24h = recentTransactions.Count > 0 ? Math.Round((decimal)recentTransactions.Count(t => t.Status == "succeeded") / recentTransactions.Count * 100, 1) : 0,
                ApiKeysConfigured = await _apiKeyRepo.CountActiveByTenantIdAsync(id),
                WebhooksConfigured = await _webhookSubRepo.CountActiveByTenantIdAsync(id),
                LastActivityAt = tenant.PaymentTransactions?.OrderByDescending(t => t.CreatedAt).FirstOrDefault()?.CreatedAt ?? DateTime.MinValue,
                CheckedAt = DateTime.UtcNow
            };
            response.SetSuccess(health);
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> VerifyStripeConfigurationAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenant = await _tenantRepo.GetByIdAsync(id);
            if (tenant == null) { response.SetError("Tenant not found."); return response; }
            if (string.IsNullOrEmpty(tenant.StripeSecretKeyEnc)) { response.SetError("Stripe secret key not configured."); return response; }
            try
            {
                var secretKey = _encryption.Decrypt(tenant.StripeSecretKeyEnc);
                var requestOptions = new Stripe.RequestOptions { ApiKey = secretKey };
                var accountService = new Stripe.AccountService();
                var account = await accountService.GetSelfAsync(requestOptions: requestOptions);
                if (account == null) response.SetError("Unable to verify Stripe account.");
                else response.SetSuccess(true, $"Stripe account verified: {account.Email}");
            }
            catch (Exception ex) { response.SetError($"Stripe verification failed: {ex.Message}"); }
            return response;
        }

        private TenantResponseDto MapTenant(Tenant t) => new()
        {
            Id = t.Id,
            Name = t.Name,
            Slug = t.Slug,
            Description = t.Description,
            PublicKey = t.PublicKey,
            Plan = t.Plan,
            IsActive = t.IsActive,
            TotalRevenue = t.PaymentTransactions?.Where(p => p.Status == "succeeded").Sum(p => p.Amount) ?? 0,
            ActiveSubscriptions = t.Subscriptions?.Count(s => s.Status == "active") ?? 0,
            TotalCustomers = t.Customers?.Count ?? 0,
            Features = !string.IsNullOrEmpty(t.Features) ? JsonConvert.DeserializeObject<List<string>>(t.Features) : new(),
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
        };

        private static string GeneratePublicKey() { const string prefix = "pk_live_"; var randomBytes = new byte[32]; using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider()) { rng.GetBytes(randomBytes); } return prefix + Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('='); }
        private static string GenerateSecretKey() { const string prefix = "sk_live_"; var randomBytes = new byte[32]; using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider()) { rng.GetBytes(randomBytes); } return prefix + Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('='); }
    }
}
