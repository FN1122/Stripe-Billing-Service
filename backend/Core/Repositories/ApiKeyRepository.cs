using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories
{
    public class ApiKeyRepository : BaseRepository, IApiKeyRepository
    {
        private readonly BillingDbContext _dbContext;
        private readonly IValidator<ApiKey> _validator;

        public ApiKeyRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext, IValidator<ApiKey> validator) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<ApiKey> GetByIdAsync(Guid id)
        {
            var apiKey = await _dbContext.ApiKeys.Include(k => k.Tenant).FirstOrDefaultAsync(x => x.Id == id);
            await _validator.ValidateAndThrowAsync(apiKey, RuleValidator.GET);
            return apiKey!;
        }

        public async Task<ApiKey> GetByIdAndTenantAsync(Guid tenantId, Guid id)
        {
            var apiKey = await _dbContext.ApiKeys.FirstOrDefaultAsync(k => k.Id == id && k.TenantId == tenantId);
            await _validator.ValidateAndThrowAsync(apiKey, RuleValidator.GET);
            return apiKey!;
        }

        public async Task<ApiKey?> GetByKeyHashAsync(string keyHash) => await _dbContext.ApiKeys.FirstOrDefaultAsync(x => x.KeyHash == keyHash);

        public async Task<ApiKey?> GetByKeyPrefixAsync(string keyPrefix) => await _dbContext.ApiKeys.FirstOrDefaultAsync(k => k.KeyPrefix == keyPrefix && k.IsActive);

        public async Task<List<ApiKey>> GetByTenantIdAsync(Guid tenantId) => await _dbContext.ApiKeys.Where(x => x.TenantId == tenantId).OrderByDescending(x => x.CreatedAt).AsNoTracking().ToListAsync();

        public async Task<int> CountActiveByTenantIdAsync(Guid tenantId) => await _dbContext.ApiKeys.CountAsync(ak => ak.TenantId == tenantId && ak.IsActive);

        public async Task<Guid> CreateAsync(ApiKey apiKey)
        {
            await _validator.ValidateAndThrowAsync(apiKey, RuleValidator.CREATE);
            _dbContext.ApiKeys.Add(apiKey);
            await _dbContext.SaveChangesAsync();
            return apiKey.Id;
        }

        public async Task UpdateAsync(ApiKey apiKey)
        {
            await _validator.ValidateAndThrowAsync(apiKey, RuleValidator.UPDATE);
            _dbContext.ApiKeys.Update(apiKey);
            await _dbContext.SaveChangesAsync();
        }

        public IQueryable<ApiKey> Query(Guid tenantId) => _dbContext.ApiKeys.Where(k => k.TenantId == tenantId);
    }
}
