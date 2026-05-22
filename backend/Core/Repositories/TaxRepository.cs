using Core.ContextProviders;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories
{
    public class TaxRepository : BaseRepository, ITaxRepository
    {
        private readonly BillingDbContext _dbContext;

        public TaxRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
        }

        public async Task<TaxConfiguration?> GetConfigAsync(Guid tenantId)
        {
            return await _dbContext.TaxConfigurations.FirstOrDefaultAsync(t => t.TenantId == tenantId);
        }

        public async Task<Guid> CreateConfigAsync(TaxConfiguration config)
        {
            _dbContext.TaxConfigurations.Add(config);
            await _dbContext.SaveChangesAsync();
            return config.Id;
        }

        public async Task UpdateConfigAsync(TaxConfiguration config)
        {
            _dbContext.TaxConfigurations.Update(config);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<TaxExemption>> GetExemptionsAsync(Guid tenantId, Guid customerId)
        {
            return await _dbContext.TaxExemptions
                .Where(t => t.TenantId == tenantId && t.CustomerId == customerId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Guid> CreateExemptionAsync(TaxExemption exemption)
        {
            _dbContext.TaxExemptions.Add(exemption);
            await _dbContext.SaveChangesAsync();
            return exemption.Id;
        }

        public async Task DeleteExemptionAsync(Guid tenantId, Guid id)
        {
            var exemption = await _dbContext.TaxExemptions.FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id);
            if (exemption != null)
            {
                _dbContext.TaxExemptions.Remove(exemption);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
