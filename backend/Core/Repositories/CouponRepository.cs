using Core.ContextProviders;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories
{
    public class CouponRepository : BaseRepository, ICouponRepository
    {
        private readonly BillingDbContext _dbContext;

        public CouponRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
        }

        public async Task<Coupon?> GetByIdAsync(Guid tenantId, Guid id)
        {
            return await _dbContext.Coupons
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id);
        }

        public async Task<Coupon?> GetByIdWithDetailsAsync(Guid tenantId, Guid id)
        {
            return await _dbContext.Coupons
                .Include(c => c.PromotionCodes)
                .Include(c => c.Redemptions).ThenInclude(r => r.Customer)
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id);
        }

        public async Task<Coupon?> GetByStripeIdAsync(Guid tenantId, string stripeCouponId)
        {
            return await _dbContext.Coupons
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.StripeCouponId == stripeCouponId);
        }

        public IQueryable<Coupon> Query(Guid tenantId)
        {
            return _dbContext.Coupons
                .Include(c => c.PromotionCodes)
                .Where(c => c.TenantId == tenantId);
        }

        public async Task<Guid> CreateAsync(Coupon coupon)
        {
            _dbContext.Coupons.Add(coupon);
            await _dbContext.SaveChangesAsync();
            return coupon.Id;
        }

        public async Task UpdateAsync(Coupon coupon)
        {
            _dbContext.Coupons.Update(coupon);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<PromotionCode?> GetPromotionCodeAsync(Guid tenantId, Guid id)
        {
            return await _dbContext.PromotionCodes
                .Include(p => p.Coupon)
                .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id);
        }

        public async Task<PromotionCode?> GetPromotionCodeByCodeAsync(Guid tenantId, string code)
        {
            return await _dbContext.PromotionCodes
                .Include(p => p.Coupon)
                .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Code == code && p.IsActive);
        }

        public async Task<List<PromotionCode>> ListPromotionCodesAsync(Guid tenantId, Guid couponId)
        {
            return await _dbContext.PromotionCodes
                .Where(p => p.TenantId == tenantId && p.CouponId == couponId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Guid> CreatePromotionCodeAsync(PromotionCode promotionCode)
        {
            _dbContext.PromotionCodes.Add(promotionCode);
            await _dbContext.SaveChangesAsync();
            return promotionCode.Id;
        }

        public async Task UpdatePromotionCodeAsync(PromotionCode promotionCode)
        {
            _dbContext.PromotionCodes.Update(promotionCode);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Guid> CreateRedemptionAsync(CouponRedemption redemption)
        {
            _dbContext.CouponRedemptions.Add(redemption);
            await _dbContext.SaveChangesAsync();
            return redemption.Id;
        }

        public async Task<List<CouponRedemption>> GetRedemptionsAsync(Guid tenantId, Guid couponId)
        {
            return await _dbContext.CouponRedemptions
                .Include(r => r.Customer)
                .Where(r => r.TenantId == tenantId && r.CouponId == couponId)
                .OrderByDescending(r => r.RedeemedAt)
                .ToListAsync();
        }

        public async Task<int> CountAsync(Guid tenantId)
        {
            return await _dbContext.Coupons.CountAsync(c => c.TenantId == tenantId);
        }

        public async Task<int> CountActiveAsync(Guid tenantId)
        {
            return await _dbContext.Coupons.CountAsync(c => c.TenantId == tenantId && c.IsActive);
        }

        public async Task<int> CountRedemptionsAsync(Guid tenantId)
        {
            return await _dbContext.CouponRedemptions.CountAsync(r => r.TenantId == tenantId);
        }

        public async Task<decimal> SumDiscountAmountAsync(Guid tenantId)
        {
            return await _dbContext.CouponRedemptions
                .Where(r => r.TenantId == tenantId)
                .SumAsync(r => r.AmountDiscounted);
        }
    }
}
