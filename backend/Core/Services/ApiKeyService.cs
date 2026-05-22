using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.ServiceContracts;
using Core.Utils;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
    public class ApiKeyService : BaseService, IApiKeyService
    {
        private readonly IApiKeyRepository _apiKeyRepo;
        private readonly IEncryptionService _encryption;

        public ApiKeyService(ITenantContextProvider tcp, IApiKeyRepository apiKeyRepo, IEncryptionService encryption) : base(tcp)
        {
            _apiKeyRepo = apiKeyRepo;
            _encryption = encryption;
        }

        public async Task<GatewayResponseWrapper<ApiKeyCreateResponseDto>> CreateAsync(CreateApiKeyDto request)
        {
            var response = new GatewayResponseWrapper<ApiKeyCreateResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var key = GenerateApiKey();
            var keyEnc = _encryption.Encrypt(key);
            var apiKey = new ApiKey
            {
                TenantId = tenantId, Name = request.Name, Description = request.Description,
                KeyEnc = keyEnc, KeyPrefix = key.Substring(0, 10),
                Permissions = string.Join(",", request.Permissions ?? new List<string> { "read" }),
                IsActive = true, ExpiresAt = request.ExpiresAt
            };
            await _apiKeyRepo.CreateAsync(apiKey);
            response.SetSuccess(new ApiKeyCreateResponseDto
            {
                Id = apiKey.Id, Name = apiKey.Name, Key = key, KeyPrefix = apiKey.KeyPrefix,
                Permissions = request.Permissions ?? new List<string>(), IsActive = apiKey.IsActive,
                ExpiresAt = apiKey.ExpiresAt, CreatedAt = apiKey.CreatedAt
            });
            return response;
        }

        public async Task<GatewayResponseWrapper<ApiKeyResponseDto>> GetAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<ApiKeyResponseDto>();
            var apiKey = await _apiKeyRepo.GetByIdAndTenantAsync(CurrentTenantContext.TenantId, id);
            if (apiKey == null) { response.SetError("API key not found."); return response; }
            response.SetSuccess(MapApiKey(apiKey));
            return response;
        }

        public async Task<GatewayPaginatedListResponseWrapper<ApiKeyResponseDto>> ListAsync(ApiKeyFilterDto filter)
        {
            var response = new GatewayPaginatedListResponseWrapper<ApiKeyResponseDto>();
            var query = _apiKeyRepo.Query(CurrentTenantContext.TenantId);
            if (!string.IsNullOrEmpty(filter.Search)) query = query.Where(k => k.Name.Contains(filter.Search) || k.Description.Contains(filter.Search));
            if (filter.IsActive.HasValue) query = query.Where(k => k.IsActive == filter.IsActive.Value);
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(k => k.CreatedAt).Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
            response.SetSuccessWithPagination(items.Select(MapApiKey).ToList(), total, filter.Page, filter.PageSize);
            return response;
        }

        public async Task<GatewayResponseWrapper<ApiKeyResponseDto>> UpdateAsync(Guid id, UpdateApiKeyDto request)
        {
            var response = new GatewayResponseWrapper<ApiKeyResponseDto>();
            var apiKey = await _apiKeyRepo.GetByIdAndTenantAsync(CurrentTenantContext.TenantId, id);
            if (apiKey == null) { response.SetError("API key not found."); return response; }
            if (request.Name != null) apiKey.Name = request.Name;
            if (request.Description != null) apiKey.Description = request.Description;
            if (request.Permissions != null) apiKey.Permissions = string.Join(",", request.Permissions);
            if (request.ExpiresAt.HasValue) apiKey.ExpiresAt = request.ExpiresAt;
            apiKey.UpdatedAt = DateTime.UtcNow;
            await _apiKeyRepo.UpdateAsync(apiKey);
            response.SetSuccess(MapApiKey(apiKey));
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> RevokeAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var apiKey = await _apiKeyRepo.GetByIdAndTenantAsync(CurrentTenantContext.TenantId, id);
            if (apiKey == null) { response.SetError("API key not found."); return response; }
            apiKey.IsActive = false; apiKey.RevokedAt = DateTime.UtcNow;
            await _apiKeyRepo.UpdateAsync(apiKey);
            response.SetSuccess(true, "API key revoked.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> RestoreAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var apiKey = await _apiKeyRepo.GetByIdAndTenantAsync(CurrentTenantContext.TenantId, id);
            if (apiKey == null) { response.SetError("API key not found."); return response; }
            apiKey.IsActive = true; apiKey.RevokedAt = null;
            await _apiKeyRepo.UpdateAsync(apiKey);
            response.SetSuccess(true, "API key restored.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> ValidateAsync(string key)
        {
            var response = new GatewayResponseWrapper<bool>();
            try
            {
                var prefix = key.Substring(0, 10);
                var apiKey = await _apiKeyRepo.GetByKeyPrefixAsync(prefix);
                if (apiKey == null) { response.SetError("Invalid API key."); return response; }
                if (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt.Value < DateTime.UtcNow) { response.SetError("API key has expired."); return response; }
                var decryptedKey = _encryption.Decrypt(apiKey.KeyEnc);
                if (decryptedKey != key) { response.SetError("Invalid API key."); return response; }
                apiKey.LastUsedAt = DateTime.UtcNow;
                await _apiKeyRepo.UpdateAsync(apiKey);
                response.SetSuccess(true);
            }
            catch { response.SetError("Invalid API key format."); }
            return response;
        }

        public async Task<GatewayResponseWrapper<ApiKeyStatsDto>> GetStatsAsync()
        {
            var response = new GatewayResponseWrapper<ApiKeyStatsDto>();
            var apiKeys = await _apiKeyRepo.GetByTenantIdAsync(CurrentTenantContext.TenantId);
            var stats = new ApiKeyStatsDto
            {
                TotalKeys = apiKeys.Count, ActiveKeys = apiKeys.Count(k => k.IsActive),
                RevokedKeys = apiKeys.Count(k => !k.IsActive),
                ExpiredKeys = apiKeys.Count(k => k.ExpiresAt.HasValue && k.ExpiresAt.Value < DateTime.UtcNow),
                MostRecentCreation = apiKeys.OrderByDescending(k => k.CreatedAt).FirstOrDefault()?.CreatedAt,
                MostRecentUsage = apiKeys.OrderByDescending(k => k.LastUsedAt).FirstOrDefault()?.LastUsedAt,
                ExpiringInNext30Days = apiKeys.Count(k => k.ExpiresAt.HasValue && k.ExpiresAt.Value > DateTime.UtcNow && k.ExpiresAt.Value <= DateTime.UtcNow.AddDays(30))
            };
            response.SetSuccess(stats);
            return response;
        }

        private static string GenerateApiKey()
        {
            const string prefix = "pk_";
            var randomBytes = new byte[32];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider()) { rng.GetBytes(randomBytes); }
            return prefix + Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        private static ApiKeyResponseDto MapApiKey(ApiKey k) => new()
        {
            Id = k.Id, Name = k.Name, Description = k.Description, KeyPrefix = k.KeyPrefix,
            Permissions = !string.IsNullOrEmpty(k.Permissions) ? k.Permissions.Split(',').ToList() : new(),
            IsActive = k.IsActive, LastUsedAt = k.LastUsedAt, ExpiresAt = k.ExpiresAt,
            CreatedAt = k.CreatedAt, RevokedAt = k.RevokedAt
        };
    }
}
