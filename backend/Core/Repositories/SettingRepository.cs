using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories
{
    public class SettingRepository : BaseRepository, ISettingRepository
    {
        private readonly BillingDbContext _dbContext;
        private readonly IValidator<Setting> _validator;

        public SettingRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext, IValidator<Setting> validator) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<Setting?> GetByKeyAsync(Guid tenantId, string key)
        {
            var setting = await _dbContext.Settings.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Key == key);
            if (setting != null) await _validator.ValidateAndThrowAsync(setting, RuleValidator.GET);
            return setting;
        }

        public async Task<List<Setting>> GetByTenantIdAsync(Guid tenantId) => await _dbContext.Settings.Where(x => x.TenantId == tenantId).OrderBy(x => x.Key).AsNoTracking().ToListAsync();

        public async Task<Guid> CreateAsync(Setting setting)
        {
            await _validator.ValidateAndThrowAsync(setting, RuleValidator.CREATE);
            _dbContext.Settings.Add(setting);
            await _dbContext.SaveChangesAsync();
            return setting.Id;
        }

        public async Task UpdateAsync(Setting setting)
        {
            await _validator.ValidateAndThrowAsync(setting, RuleValidator.UPDATE);
            _dbContext.Settings.Update(setting);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Setting setting)
        {
            await _validator.ValidateAndThrowAsync(setting, RuleValidator.DELETE);
            _dbContext.Settings.Remove(setting);
            await _dbContext.SaveChangesAsync();
        }
    }
}
