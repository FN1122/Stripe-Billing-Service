using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/coupons")]
    public class CouponController : GatewayControllerBase
    {
        private readonly ICouponService _couponService;

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCouponDto request)
        {
            return ToResponse(await _couponService.CreateCouponAsync(request));
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] CouponFilterDto filter)
        {
            return ToResponse(await _couponService.ListCouponsAsync(filter));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return ToResponse(await _couponService.GetCouponAsync(id));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCouponDto request)
        {
            return ToResponse(await _couponService.UpdateCouponAsync(id, request));
        }

        [HttpPost("{id}/toggle")]
        public async Task<IActionResult> Toggle(Guid id)
        {
            return ToResponse(await _couponService.ToggleCouponAsync(id));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return ToResponse(await _couponService.DeleteCouponAsync(id));
        }

        [HttpPost("{couponId}/promotion-codes")]
        public async Task<IActionResult> CreatePromotionCode(Guid couponId, [FromBody] CreatePromotionCodeDto request)
        {
            return ToResponse(await _couponService.CreatePromotionCodeAsync(couponId, request));
        }

        [HttpGet("{couponId}/promotion-codes")]
        public async Task<IActionResult> ListPromotionCodes(Guid couponId)
        {
            return ToResponse(await _couponService.ListPromotionCodesAsync(couponId));
        }

        [HttpPost("promotion-codes/{id}/deactivate")]
        public async Task<IActionResult> DeactivatePromotionCode(Guid id)
        {
            return ToResponse(await _couponService.DeactivatePromotionCodeAsync(id));
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateCode([FromBody] ValidateCouponDto request)
        {
            return ToResponse(await _couponService.ValidateCouponCodeAsync(request.Code));
        }

        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] ApplyCouponDto request)
        {
            return ToResponse(await _couponService.ApplyCouponToSubscriptionAsync(request));
        }

        [HttpPost("remove")]
        public async Task<IActionResult> Remove([FromBody] RemoveCouponDto request)
        {
            return ToResponse(await _couponService.RemoveCouponFromSubscriptionAsync(request));
        }

        [HttpGet("{couponId}/redemptions")]
        public async Task<IActionResult> GetRedemptions(Guid couponId)
        {
            return ToResponse(await _couponService.GetRedemptionsAsync(couponId));
        }

        [HttpGet("stats")]
        [Authorize(Policy = "ManagerOrAbove")]
        public async Task<IActionResult> GetStats()
        {
            return ToResponse(await _couponService.GetStatsAsync());
        }
    }
}
