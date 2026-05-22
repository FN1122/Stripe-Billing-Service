using Core.ContextProviders;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories
{
    public class RefreshTokenRepository : BaseRepository, IRefreshTokenRepository
    {
        private readonly BillingDbContext _dbContext;

        public RefreshTokenRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token) => await _dbContext.RefreshTokens.Include(x => x.User).FirstOrDefaultAsync(x => x.Token == token && !x.IsRevoked);

        public async Task<List<RefreshToken>> GetByUserIdAsync(Guid userId) => await _dbContext.RefreshTokens.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).AsNoTracking().ToListAsync();

        public async Task<Guid> CreateAsync(RefreshToken refreshToken) { _dbContext.RefreshTokens.Add(refreshToken); await _dbContext.SaveChangesAsync(); return refreshToken.Id; }

        public async Task UpdateAsync(RefreshToken refreshToken) { _dbContext.RefreshTokens.Update(refreshToken); await _dbContext.SaveChangesAsync(); }
    }
}
