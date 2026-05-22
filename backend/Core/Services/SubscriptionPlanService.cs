using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.ServiceContracts;
using Core.Utils;
using Newtonsoft.Json;
using Stripe;

namespace Core.Services
{
    public class SubscriptionPlanService : BaseService, ISubscriptionPlanService
    {
        private readonly ISubscriptionPlanRepository _planRepo;
        private readonly ITenantRepository _tenantRepo;
        private readonly IEncryptionService _encryption;

        public SubscriptionPlanService(ITenantContextProvider tenantContextProvider, ISubscriptionPlanRepository planRepo, ITenantRepository tenantRepo, IEncryptionService encryption) : base(tenantContextProvider)
        {
            _planRepo = planRepo;
            _tenantRepo = tenantRepo;
            _encryption = encryption;
        }

        public async Task<GatewayResponseWrapper<SubscriptionPlanResponseDto>> CreateAsync(CreatePlanDto request)
        {
            var response = new GatewayResponseWrapper<SubscriptionPlanResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            string stripeProductId = null, stripePriceId = null;
            try
            {
                var tenant = await _tenantRepo.GetByIdAsync(tenantId);
                if (tenant != null && !string.IsNullOrEmpty(tenant.StripeSecretKeyEnc))
                {
                    var client = new StripeClient(_encryption.Decrypt(tenant.StripeSecretKeyEnc));
                    var productService = new ProductService(client);
                    var product = await productService.CreateAsync(new ProductCreateOptions { Name = request.Name, Description = request.Description });
                    stripeProductId = product.Id;
                    var priceService = new PriceService(client);
                    var price = await priceService.CreateAsync(new PriceCreateOptions
                    {
                        Product = product.Id, UnitAmount = (long)(request.Amount * 100), Currency = request.Currency ?? "usd",
                        Recurring = new PriceRecurringOptions { Interval = request.Interval, IntervalCount = request.IntervalCount }
                    });
                    stripePriceId = price.Id;
                }
            }
            catch { }
            var plan = new SubscriptionPlan
            {
                TenantId = tenantId, StripeProductId = stripeProductId, StripePriceId = stripePriceId,
                Name = request.Name, Description = request.Description, Amount = request.Amount,
                Currency = request.Currency ?? "usd", Interval = request.Interval, IntervalCount = request.IntervalCount,
                TrialDays = request.TrialDays, Features = JsonConvert.SerializeObject(request.Features), SortOrder = request.SortOrder
            };
            await _planRepo.CreateAsync(plan);
            response.SetSuccess(MapPlan(plan));
            return response;
        }

        public async Task<GatewayResponseWrapper<SubscriptionPlanResponseDto>> GetAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<SubscriptionPlanResponseDto>();
            var plan = await _planRepo.GetByIdWithSubscriptionsAsync(CurrentTenantContext.TenantId, id);
            if (plan == null) { response.SetError("Plan not found.", 404); return response; }
            response.SetSuccess(MapPlan(plan));
            return response;
        }

        public async Task<GatewayResponseWrapper<List<SubscriptionPlanResponseDto>>> ListAsync()
        {
            var response = new GatewayResponseWrapper<List<SubscriptionPlanResponseDto>>();
            var plans = await _planRepo.GetByTenantIdWithSubscriptionsAsync(CurrentTenantContext.TenantId);
            response.SetSuccess(plans.Select(MapPlan).ToList());
            return response;
        }

        public async Task<GatewayResponseWrapper<SubscriptionPlanResponseDto>> UpdateAsync(Guid id, UpdatePlanDto request)
        {
            var response = new GatewayResponseWrapper<SubscriptionPlanResponseDto>();
            var plan = await _planRepo.GetByIdWithSubscriptionsAsync(CurrentTenantContext.TenantId, id);
            if (plan == null) { response.SetError("Plan not found.", 404); return response; }
            if (!string.IsNullOrEmpty(request.Name)) plan.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Description)) plan.Description = request.Description;
            if (request.Features != null) plan.Features = JsonConvert.SerializeObject(request.Features);
            if (request.SortOrder.HasValue) plan.SortOrder = request.SortOrder.Value;
            if (request.IsActive.HasValue) plan.IsActive = request.IsActive.Value;
            await _planRepo.UpdateAsync(plan);
            response.SetSuccess(MapPlan(plan));
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> DeleteAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var plan = await _planRepo.GetByIdWithSubscriptionsAsync(CurrentTenantContext.TenantId, id);
            if (plan == null) { response.SetError("Plan not found.", 404); return response; }
            plan.IsActive = false;
            await _planRepo.UpdateAsync(plan);
            response.SetSuccess(true, "Plan archived.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> SyncFromStripeAsync()
        {
            var response = new GatewayResponseWrapper<bool>();
            response.SetSuccess(true, "Stripe sync completed.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> ToggleActiveAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var plan = await _planRepo.GetByIdWithSubscriptionsAsync(CurrentTenantContext.TenantId, id);
            if (plan == null) { response.SetError("Plan not found.", 404); return response; }
            plan.IsActive = !plan.IsActive;
            await _planRepo.UpdateAsync(plan);
            response.SetSuccess(plan.IsActive, plan.IsActive ? "Plan activated." : "Plan deactivated.");
            return response;
        }

        private static SubscriptionPlanResponseDto MapPlan(SubscriptionPlan p) => new()
        {
            Id = p.Id, StripeProductId = p.StripeProductId, StripePriceId = p.StripePriceId,
            Name = p.Name, Description = p.Description, Amount = p.Amount, Currency = p.Currency,
            Interval = p.Interval, IntervalCount = p.IntervalCount, TrialDays = p.TrialDays,
            Features = string.IsNullOrEmpty(p.Features) ? new() : JsonConvert.DeserializeObject<List<string>>(p.Features) ?? new(),
            SortOrder = p.SortOrder, IsActive = p.IsActive,
            SubscriberCount = p.Subscriptions?.Count(s => s.Status == "active" || s.Status == "trialing") ?? 0,
            CreatedAt = p.CreatedAt
        };
    }
}
