using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using Core.Queries;
using Core.RepositoryContracts;
using Core.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        private readonly BillingDbContext _dbContext;
        private readonly IValidator<User> _validator;

        public UserRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext, IValidator<User> validator) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            var user = await _dbContext.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(x => x.Id == id);
            await _validator.ValidateAndThrowAsync(user, RuleValidator.GET);
            return user!;
        }

        public async Task<User?> GetByEmailAsync(Guid tenantId, string email) => await _dbContext.Users.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Email == email);

        public async Task<User?> GetByEmailGlobalAsync(string email) => await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);

        public async Task<List<User>> GetByTenantIdAsync(Guid tenantId) => await _dbContext.GetUserQueryAsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.CreatedAt).ToListAsync();

        public async Task<Guid> CreateAsync(User user)
        {
            await _validator.ValidateAndThrowAsync(user, RuleValidator.CREATE);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return user.Id;
        }

        public async Task UpdateAsync(User user)
        {
            await _validator.ValidateAndThrowAsync(user, RuleValidator.UPDATE);
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateLoginTimestampAsync(User user)
        {
            // Bypass validation for login timestamp updates (no auth context available during login)
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        public IQueryable<User> Query(Guid tenantId) => _dbContext.Users.Where(u => u.TenantId == tenantId);
    }
}
