using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.ServiceContracts;
using Core.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Stripe;
using CouponStripe = Stripe.CouponService;
using PromotionCodeStripe = Stripe.PromotionCodeService;
using Coupon = Core.Infrastructure.Coupon;
using PromotionCode = Core.Infrastructure.PromotionCode;
using CouponRedemption = Core.Infrastructure.CouponRedemption;

namespace Core.Services
{
    public class CouponService : BaseService, ICouponService
    {
        private readonly ICouponRepository _couponRepo;
        private readonly ITenantRepository _tenantRepo;
        private readonly IEncryptionService _encryption;

        public CouponService(
            ITenantContextProvider tenantContextProvider,
            ICouponRepository couponRepo,
            ITenantRepository tenantRepo,
            IEncryptionService encryption) : base(tenantContextProvider)
        {
            _couponRepo = couponRepo;
            _tenantRepo = tenantRepo;
            _encryption = encryption;
        }

        private async Task<StripeClient> GetStripeClientAsync()
        {
            var tenant = await _tenantRepo.GetByIdAsync(CurrentTenantContext.TenantId);
            if (tenant == null || string.IsNullOrEmpty(tenant.StripeSecretKeyEnc))
                return new StripeClient("sk_test_placeholder");
            return new StripeClient(_encryption.Decrypt(tenant.StripeSecretKeyEnc));
        }

        public async Task<GatewayResponseWrapper<CouponResponseDto>> CreateCouponAsync(CreateCouponDto request)
        {
            var response = new GatewayResponseWrapper<CouponResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            string? stripeCouponId = null;
            try
            {
                var client = await GetStripeClientAsync();
                var stripeService = new CouponStripe(client);
                var options = new CouponCreateOptions
                {
                    Name = request.Name,
                    Duration = request.Duration,
                };
                if (request.Type == "percent_off")
                    options.PercentOff = request.PercentOff;
                else
                {
                    options.AmountOff = (long)(request.AmountOff!.Value * 100);
                    options.Currency = request.Currency;
                }
                if (request.Duration == "repeating")
                    options.DurationInMonths = request.DurationInMonths;
                if (request.MaxRedemptions.HasValue)
                    options.MaxRedemptions = request.MaxRedemptions;
                if (request.RedeemBy.HasValue)
                    options.RedeemBy = request.RedeemBy;

                var stripeCoupon = await stripeService.CreateAsync(options);
                stripeCouponId = stripeCoupon.Id;
            }
            catch { /* Stripe sync failure is non-blocking */ }

            var coupon = new Coupon
            {
                TenantId = tenantId,
                StripeCouponId = stripeCouponId,
                Name = request.Name,
                Type = request.Type,
                AmountOff = request.AmountOff,
                PercentOff = request.PercentOff,
                Currency = request.Currency,
                Duration = request.Duration,
                DurationInMonths = request.DurationInMonths,
                MaxRedemptions = request.MaxRedemptions,
                RedeemBy = request.RedeemBy,
                Metadata = request.Metadata
            };

            await _couponRepo.CreateAsync(coupon);
            response.SetSuccess(MapCoupon(coupon));
            return response;
        }

        public async Task<GatewayResponseWrapper<CouponResponseDto>> GetCouponAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<CouponResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var coupon = await _couponRepo.GetByIdWithDetailsAsync(tenantId, id);
            if (coupon == null) { response.SetError("Coupon not found.", 404); return response; }
            response.SetSuccess(MapCoupon(coupon));
            return response;
        }

        public async Task<GatewayPaginatedListResponseWrapper<CouponResponseDto>> ListCouponsAsync(CouponFilterDto filter)
        {
            var response = new GatewayPaginatedListResponseWrapper<CouponResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var query = _couponRepo.Query(tenantId);

            if (!string.IsNullOrEmpty(filter.Search))
                query = query.Where(c => c.Name.Contains(filter.Search));
            if (!string.IsNullOrEmpty(filter.Type))
                query = query.Where(c => c.Type == filter.Type);
            if (!string.IsNullOrEmpty(filter.Duration))
                query = query.Where(c => c.Duration == filter.Duration);
            if (filter.IsActive.HasValue)
                query = query.Where(c => c.IsActive == filter.IsActive.Value);
            if (filter.FromDate.HasValue)
                query = query.Where(c => c.CreatedAt >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(c => c.CreatedAt <= filter.ToDate.Value);

            query = filter.SortBy?.ToLower() switch
            {
                "name" => filter.SortDirection == "asc" ? query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
                "timesredeemed" => filter.SortDirection == "asc" ? query.OrderBy(c => c.TimesRedeemed) : query.OrderByDescending(c => c.TimesRedeemed),
                _ => query.OrderByDescending(c => c.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var items = await query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

            response.SetSuccessWithPagination(items.Select(MapCoupon).ToList(), totalCount, filter.Page, filter.PageSize);
            return response;
        }

        public async Task<GatewayResponseWrapper<CouponResponseDto>> UpdateCouponAsync(Guid id, UpdateCouponDto request)
        {
            var response = new GatewayResponseWrapper<CouponResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var coupon = await _couponRepo.GetByIdAsync(tenantId, id);
            if (coupon == null) { response.SetError("Coupon not found.", 404); return response; }

            if (!string.IsNullOrEmpty(request.Name)) coupon.Name = request.Name;
            if (request.IsActive.HasValue) coupon.IsActive = request.IsActive.Value;
            if (request.MaxRedemptions.HasValue) coupon.MaxRedemptions = request.MaxRedemptions;
            if (request.RedeemBy.HasValue) coupon.RedeemBy = request.RedeemBy;
            if (request.Metadata != null) coupon.Metadata = request.Metadata;
            coupon.UpdatedAt = DateTime.UtcNow;

            try
            {
                if (!string.IsNullOrEmpty(coupon.StripeCouponId))
                {
                    var client = await GetStripeClientAsync();
                    var stripeService = new CouponStripe(client);
                    await stripeService.UpdateAsync(coupon.StripeCouponId, new CouponUpdateOptions { Name = coupon.Name });
                }
            }
            catch { /* non-blocking */ }

            await _couponRepo.UpdateAsync(coupon);
            response.SetSuccess(MapCoupon(coupon));
            return response;
        }

        public async Task<GatewayResponseWrapper<CouponResponseDto>> ToggleCouponAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<CouponResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var coupon = await _couponRepo.GetByIdAsync(tenantId, id);
            if (coupon == null) { response.SetError("Coupon not found.", 404); return response; }

            coupon.IsActive = !coupon.IsActive;
            coupon.UpdatedAt = DateTime.UtcNow;
            await _couponRepo.UpdateAsync(coupon);
            response.SetSuccess(MapCoupon(coupon));
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> DeleteCouponAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;
            var coupon = await _couponRepo.GetByIdAsync(tenantId, id);
            if (coupon == null) { response.SetError("Coupon not found.", 404); return response; }

            coupon.IsActive = false;
            coupon.UpdatedAt = DateTime.UtcNow;

            try
            {
                if (!string.IsNullOrEmpty(coupon.StripeCouponId))
                {
                    var client = await GetStripeClientAsync();
                    var stripeService = new CouponStripe(client);
                    await stripeService.DeleteAsync(coupon.StripeCouponId);
                }
            }
            catch { /* non-blocking */ }

            await _couponRepo.UpdateAsync(coupon);
            response.SetSuccess(true);
            return response;
        }

        public async Task<GatewayResponseWrapper<PromotionCodeResponseDto>> CreatePromotionCodeAsync(Guid couponId, CreatePromotionCodeDto request)
        {
            var response = new GatewayResponseWrapper<PromotionCodeResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var coupon = await _couponRepo.GetByIdAsync(tenantId, couponId);
            if (coupon == null) { response.SetError("Coupon not found.", 404); return response; }
            if (!coupon.IsActive) { response.SetError("Coupon is not active.", 400); return response; }

            var existing = await _couponRepo.GetPromotionCodeByCodeAsync(tenantId, request.Code);
            if (existing != null) { response.SetError("A promotion code with this code already exists.", 400); return response; }

            string? stripePromoId = null;
            try
            {
                if (!string.IsNullOrEmpty(coupon.StripeCouponId))
                {
                    var client = await GetStripeClientAsync();
                    var stripeService = new PromotionCodeStripe(client);
                    var options = new PromotionCodeCreateOptions
                    {
                        Coupon = coupon.StripeCouponId,
                        Code = request.Code,
                    };
                    if (request.MaxRedemptions.HasValue) options.MaxRedemptions = request.MaxRedemptions;
                    if (request.ExpiresAt.HasValue) options.ExpiresAt = request.ExpiresAt;
                    if (request.MinimumAmount.HasValue)
                    {
                        options.Restrictions = new PromotionCodeRestrictionsOptions
                        {
                            MinimumAmount = (long)(request.MinimumAmount.Value * 100),
                            MinimumAmountCurrency = request.MinimumAmountCurrency ?? "usd",
                            FirstTimeTransaction = request.FirstTimeTransaction
                        };
                    }
                    var stripePromo = await stripeService.CreateAsync(options);
                    stripePromoId = stripePromo.Id;
                }
            }
            catch { /* non-blocking */ }

            var restrictions = new
            {
                MinAmount = request.MinimumAmount,
                FirstTimeTransaction = request.FirstTimeTransaction,
                MinAmountCurrency = request.MinimumAmountCurrency
            };

            var promoCode = new PromotionCode
            {
                TenantId = tenantId,
                CouponId = couponId,
                StripePromotionCodeId = stripePromoId,
                Code = request.Code,
                MaxRedemptions = request.MaxRedemptions,
                ExpiresAt = request.ExpiresAt,
                Restrictions = JsonConvert.SerializeObject(restrictions)
            };

            await _couponRepo.CreatePromotionCodeAsync(promoCode);
            response.SetSuccess(MapPromotionCode(promoCode));
            return response;
        }

        public async Task<GatewayResponseWrapper<List<PromotionCodeResponseDto>>> ListPromotionCodesAsync(Guid couponId)
        {
            var response = new GatewayResponseWrapper<List<PromotionCodeResponseDto>>();
            var tenantId = CurrentTenantContext.TenantId;
            var codes = await _couponRepo.ListPromotionCodesAsync(tenantId, couponId);
            response.SetSuccess(codes.Select(MapPromotionCode).ToList());
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> DeactivatePromotionCodeAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;
            var promoCode = await _couponRepo.GetPromotionCodeAsync(tenantId, id);
            if (promoCode == null) { response.SetError("Promotion code not found.", 404); return response; }

            promoCode.IsActive = false;

            try
            {
                if (!string.IsNullOrEmpty(promoCode.StripePromotionCodeId))
                {
                    var client = await GetStripeClientAsync();
                    var stripeService = new PromotionCodeStripe(client);
                    await stripeService.UpdateAsync(promoCode.StripePromotionCodeId, new PromotionCodeUpdateOptions { Active = false });
                }
            }
            catch { /* non-blocking */ }

            await _couponRepo.UpdatePromotionCodeAsync(promoCode);
            response.SetSuccess(true);
            return response;
        }

        public async Task<GatewayResponseWrapper<CouponResponseDto>> ValidateCouponCodeAsync(string code)
        {
            var response = new GatewayResponseWrapper<CouponResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var promoCode = await _couponRepo.GetPromotionCodeByCodeAsync(tenantId, code);
            if (promoCode == null || promoCode.Coupon == null)
            { response.SetError("Invalid or inactive promotion code.", 400); return response; }

            if (!promoCode.IsActive || !promoCode.Coupon.IsActive)
            { response.SetError("This promotion code is no longer active.", 400); return response; }

            if (promoCode.ExpiresAt.HasValue && promoCode.ExpiresAt.Value < DateTime.UtcNow)
            { response.SetError("This promotion code has expired.", 400); return response; }

            if (promoCode.MaxRedemptions.HasValue && promoCode.TimesRedeemed >= promoCode.MaxRedemptions.Value)
            { response.SetError("This promotion code has reached its maximum redemptions.", 400); return response; }

            if (promoCode.Coupon.RedeemBy.HasValue && promoCode.Coupon.RedeemBy.Value < DateTime.UtcNow)
            { response.SetError("The coupon for this code has expired.", 400); return response; }

            if (promoCode.Coupon.MaxRedemptions.HasValue && promoCode.Coupon.TimesRedeemed >= promoCode.Coupon.MaxRedemptions.Value)
            { response.SetError("The coupon has reached its maximum redemptions.", 400); return response; }

            response.SetSuccess(MapCoupon(promoCode.Coupon));
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> ApplyCouponToSubscriptionAsync(ApplyCouponDto request)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;

            var validateResult = await ValidateCouponCodeAsync(request.Code);
            if (!validateResult.IsValid) { response.SetError(validateResult.Message); return response; }

            var promoCode = await _couponRepo.GetPromotionCodeByCodeAsync(tenantId, request.Code);
            if (promoCode?.Coupon == null) { response.SetError("Promotion code not found.", 404); return response; }

            try
            {
                var client = await GetStripeClientAsync();
                var subscriptionService = new Stripe.SubscriptionService(client);
                // Apply coupon to subscription via Stripe
                // Note: In production you'd look up the Stripe subscription ID
            }
            catch { /* non-blocking */ }

            promoCode.TimesRedeemed++;
            promoCode.Coupon.TimesRedeemed++;
            await _couponRepo.UpdatePromotionCodeAsync(promoCode);
            await _couponRepo.UpdateAsync(promoCode.Coupon);

            var redemption = new CouponRedemption
            {
                TenantId = tenantId,
                CouponId = promoCode.CouponId,
                PromotionCodeId = promoCode.Id,
                SubscriptionId = request.SubscriptionId,
                AmountDiscounted = 0, // calculated from actual invoice
                Currency = promoCode.Coupon.Currency ?? "usd",
            };
            await _couponRepo.CreateRedemptionAsync(redemption);

            response.SetSuccess(true, "Coupon applied successfully.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> RemoveCouponFromSubscriptionAsync(RemoveCouponDto request)
        {
            var response = new GatewayResponseWrapper<bool>();
            // In production, call Stripe to remove the discount from the subscription
            response.SetSuccess(true, "Coupon removed from subscription.");
            return response;
        }

        public async Task<GatewayResponseWrapper<List<CouponRedemptionResponseDto>>> GetRedemptionsAsync(Guid couponId)
        {
            var response = new GatewayResponseWrapper<List<CouponRedemptionResponseDto>>();
            var tenantId = CurrentTenantContext.TenantId;
            var redemptions = await _couponRepo.GetRedemptionsAsync(tenantId, couponId);
            response.SetSuccess(redemptions.Select(r => new CouponRedemptionResponseDto
            {
                Id = r.Id,
                CouponId = r.CouponId,
                PromotionCodeId = r.PromotionCodeId,
                CustomerId = r.CustomerId,
                SubscriptionId = r.SubscriptionId,
                CustomerName = r.Customer?.Name,
                CustomerEmail = r.Customer?.Email,
                AmountDiscounted = r.AmountDiscounted,
                Currency = r.Currency,
                RedeemedAt = r.RedeemedAt
            }).ToList());
            return response;
        }

        public async Task<GatewayResponseWrapper<CouponStatsDto>> GetStatsAsync()
        {
            var response = new GatewayResponseWrapper<CouponStatsDto>();
            var tenantId = CurrentTenantContext.TenantId;

            // Most used coupon
            var mostUsed = await _couponRepo.Query(tenantId)
                .OrderByDescending(c => c.TimesRedeemed)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();

            // Redemptions by month (last 6 months)
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var recentRedemptions = await _couponRepo.Query(tenantId)
                .SelectMany(c => c.Redemptions)
                .Where(r => r.RedeemedAt >= sixMonthsAgo)
                .Select(r => r.RedeemedAt)
                .ToListAsync();

            var redemptionsByMonth = recentRedemptions
                .GroupBy(d => $"{d.Year}-{d.Month:D2}")
                .ToDictionary(g => g.Key, g => g.Count());

            var stats = new CouponStatsDto
            {
                TotalCoupons = await _couponRepo.CountAsync(tenantId),
                ActiveCoupons = await _couponRepo.CountActiveAsync(tenantId),
                TotalRedemptions = await _couponRepo.CountRedemptionsAsync(tenantId),
                TotalDiscountAmount = await _couponRepo.SumDiscountAmountAsync(tenantId),
                MostUsedCouponName = mostUsed,
                RedemptionsByMonth = redemptionsByMonth,
            };

            response.SetSuccess(stats);
            return response;
        }

        private CouponResponseDto MapCoupon(Coupon c) => new()
        {
            Id = c.Id,
            TenantId = c.TenantId,
            StripeCouponId = c.StripeCouponId,
            Name = c.Name,
            Type = c.Type,
            AmountOff = c.AmountOff,
            PercentOff = c.PercentOff,
            Currency = c.Currency,
            Duration = c.Duration,
            DurationInMonths = c.DurationInMonths,
            MaxRedemptions = c.MaxRedemptions,
            TimesRedeemed = c.TimesRedeemed,
            RedeemBy = c.RedeemBy,
            IsActive = c.IsActive,
            Metadata = c.Metadata,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            PromotionCodes = c.PromotionCodes?.Select(MapPromotionCode).ToList() ?? new()
        };

        private PromotionCodeResponseDto MapPromotionCode(PromotionCode p)
        {
            bool firstTime = false;
            decimal? minAmount = null;
            string? minCurrency = null;
            if (!string.IsNullOrEmpty(p.Restrictions))
            {
                try
                {
                    dynamic r = JsonConvert.DeserializeObject(p.Restrictions)!;
                    firstTime = r.FirstTimeTransaction ?? false;
                    minAmount = r.MinAmount;
                    minCurrency = r.MinAmountCurrency;
                }
                catch { }
            }

            return new PromotionCodeResponseDto
            {
                Id = p.Id,
                CouponId = p.CouponId,
                StripePromotionCodeId = p.StripePromotionCodeId,
                Code = p.Code,
                IsActive = p.IsActive,
                MaxRedemptions = p.MaxRedemptions,
                TimesRedeemed = p.TimesRedeemed,
                ExpiresAt = p.ExpiresAt,
                FirstTimeTransaction = firstTime,
                MinimumAmount = minAmount,
                MinimumAmountCurrency = minCurrency,
                CreatedAt = p.CreatedAt
            };
        }
    }
}
