using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface ICouponService
    {
        Task<GatewayResponseWrapper<CouponResponseDto>> CreateCouponAsync(CreateCouponDto request);
        Task<GatewayResponseWrapper<CouponResponseDto>> GetCouponAsync(Guid id);
        Task<GatewayPaginatedListResponseWrapper<CouponResponseDto>> ListCouponsAsync(CouponFilterDto filter);
        Task<GatewayResponseWrapper<CouponResponseDto>> UpdateCouponAsync(Guid id, UpdateCouponDto request);
        Task<GatewayResponseWrapper<CouponResponseDto>> ToggleCouponAsync(Guid id);
        Task<GatewayResponseWrapper<bool>> DeleteCouponAsync(Guid id);
        Task<GatewayResponseWrapper<PromotionCodeResponseDto>> CreatePromotionCodeAsync(Guid couponId, CreatePromotionCodeDto request);
        Task<GatewayResponseWrapper<List<PromotionCodeResponseDto>>> ListPromotionCodesAsync(Guid couponId);
        Task<GatewayResponseWrapper<bool>> DeactivatePromotionCodeAsync(Guid id);
        Task<GatewayResponseWrapper<CouponResponseDto>> ValidateCouponCodeAsync(string code);
        Task<GatewayResponseWrapper<bool>> ApplyCouponToSubscriptionAsync(ApplyCouponDto request);
        Task<GatewayResponseWrapper<bool>> RemoveCouponFromSubscriptionAsync(RemoveCouponDto request);
        Task<GatewayResponseWrapper<List<CouponRedemptionResponseDto>>> GetRedemptionsAsync(Guid couponId);
        Task<GatewayResponseWrapper<CouponStatsDto>> GetStatsAsync();
    }
}
