using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface ICouponRepository
    {
        Task<Coupon?> GetByIdAsync(Guid tenantId, Guid id);
        Task<Coupon?> GetByIdWithDetailsAsync(Guid tenantId, Guid id);
        Task<Coupon?> GetByStripeIdAsync(Guid tenantId, string stripeCouponId);
        IQueryable<Coupon> Query(Guid tenantId);
        Task<Guid> CreateAsync(Coupon coupon);
        Task UpdateAsync(Coupon coupon);
        Task<PromotionCode?> GetPromotionCodeAsync(Guid tenantId, Guid id);
        Task<PromotionCode?> GetPromotionCodeByCodeAsync(Guid tenantId, string code);
        Task<List<PromotionCode>> ListPromotionCodesAsync(Guid tenantId, Guid couponId);
        Task<Guid> CreatePromotionCodeAsync(PromotionCode promotionCode);
        Task UpdatePromotionCodeAsync(PromotionCode promotionCode);
        Task<Guid> CreateRedemptionAsync(CouponRedemption redemption);
        Task<List<CouponRedemption>> GetRedemptionsAsync(Guid tenantId, Guid couponId);
        Task<int> CountAsync(Guid tenantId);
        Task<int> CountActiveAsync(Guid tenantId);
        Task<int> CountRedemptionsAsync(Guid tenantId);
        Task<decimal> SumDiscountAmountAsync(Guid tenantId);
    }
}
