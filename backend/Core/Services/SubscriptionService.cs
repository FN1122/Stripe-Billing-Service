using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.ServiceContracts;
using Core.Utils;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
    public class SubscriptionService : BaseService, ISubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepo;
        private readonly ISubscriptionPlanRepository _planRepo;
        private readonly ICustomerRepository _customerRepo;

        public SubscriptionService(
            ITenantContextProvider tenantContextProvider,
            ISubscriptionRepository subscriptionRepo,
            ISubscriptionPlanRepository planRepo,
            ICustomerRepository customerRepo) : base(tenantContextProvider)
        {
            _subscriptionRepo = subscriptionRepo;
            _planRepo = planRepo;
            _customerRepo = customerRepo;
        }

        public async Task<GatewayResponseWrapper<SubscriptionResponseDto>> CreateAsync(CreateSubscriptionDto request)
        {
            var response = new GatewayResponseWrapper<SubscriptionResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var plan = await _planRepo.GetByIdAsync(request.PlanId);
            if (plan == null) { response.SetError("Plan not found."); return response; }

            Guid customerId;
            if (request.CustomerId.HasValue)
                customerId = request.CustomerId.Value;
            else
            {
                response.SetError("CustomerId is required.");
                return response;
            }

            var customer = await _customerRepo.GetByIdAsync(customerId);
            if (customer == null) { response.SetError("Customer not found."); return response; }

            var now = DateTime.UtcNow;
            var subscription = new Subscription
            {
                TenantId = tenantId,
                CustomerId = customerId,
                PlanId = plan.Id,
                StripeSubscriptionId = "",
                Status = request.TrialDays.HasValue && request.TrialDays > 0 ? "trialing" : "active",
                Quantity = request.Quantity,
                CurrentPeriodStart = now,
                CurrentPeriodEnd = plan.Interval == "year" ? now.AddYears(1) : now.AddMonths(1),
                TrialStart = request.TrialDays.HasValue ? now : null,
                TrialEnd = request.TrialDays.HasValue ? now.AddDays(request.TrialDays.Value) : null,
                Metadata = request.Metadata != null ? System.Text.Json.JsonSerializer.Serialize(request.Metadata) : "{}",
                CreatedAt = now
            };

            await _subscriptionRepo.CreateAsync(subscription);

            response.SetSuccess(MapToDto(subscription, customer, plan), "Subscription created successfully.");
            return response;
        }

        public async Task<GatewayResponseWrapper<SubscriptionResponseDto>> GetAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<SubscriptionResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var subscription = await _subscriptionRepo.Query(tenantId)
                .Include(s => s.Customer)
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subscription == null) { response.SetError("Subscription not found.", 404); return response; }

            response.SetSuccess(MapToDto(subscription, subscription.Customer, subscription.Plan));
            return response;
        }

        public async Task<GatewayPaginatedListResponseWrapper<SubscriptionResponseDto>> ListAsync(SubscriptionFilterDto filter)
        {
            var response = new GatewayPaginatedListResponseWrapper<SubscriptionResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var query = _subscriptionRepo.Query(tenantId)
                .Include(s => s.Customer)
                .Include(s => s.Plan)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(s => s.Status == filter.Status);
            if (filter.PlanId.HasValue)
                query = query.Where(s => s.PlanId == filter.PlanId);
            if (filter.CustomerId.HasValue)
                query = query.Where(s => s.CustomerId == filter.CustomerId);
            if (filter.DateFrom.HasValue)
                query = query.Where(s => s.CreatedAt >= filter.DateFrom);
            if (filter.DateTo.HasValue)
                query = query.Where(s => s.CreatedAt <= filter.DateTo);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            response.SetSuccessWithPagination(
                items.Select(s => MapToDto(s, s.Customer, s.Plan)).ToList(),
                totalCount, filter.Page, filter.PageSize);
            return response;
        }

        public async Task<GatewayResponseWrapper<SubscriptionResponseDto>> UpdateAsync(Guid id, UpdateSubscriptionDto request)
        {
            var response = new GatewayResponseWrapper<SubscriptionResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var subscription = await _subscriptionRepo.Query(tenantId)
                .Include(s => s.Customer)
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subscription == null) { response.SetError("Subscription not found.", 404); return response; }

            if (request.PlanId.HasValue)
            {
                var newPlan = await _planRepo.GetByIdAsync(request.PlanId.Value);
                if (newPlan == null) { response.SetError("New plan not found."); return response; }
                subscription.PlanId = newPlan.Id;
                subscription.Plan = newPlan;
            }

            if (request.Quantity.HasValue)
                subscription.Quantity = request.Quantity.Value;

            subscription.UpdatedAt = DateTime.UtcNow;
            await _subscriptionRepo.UpdateAsync(subscription);

            response.SetSuccess(MapToDto(subscription, subscription.Customer, subscription.Plan), "Subscription updated.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> CancelAsync(Guid id, CancelSubscriptionDto request)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;

            var subscription = await _subscriptionRepo.Query(tenantId).FirstOrDefaultAsync(s => s.Id == id);
            if (subscription == null) { response.SetError("Subscription not found.", 404); return response; }

            subscription.CancelAtPeriodEnd = request.CancelAtPeriodEnd;
            subscription.CancellationReason = request.Reason;
            subscription.CancelledAt = DateTime.UtcNow;

            if (!request.CancelAtPeriodEnd)
                subscription.Status = "canceled";

            await _subscriptionRepo.UpdateAsync(subscription);
            response.SetSuccess(true, "Subscription cancelled.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> PauseAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;

            var subscription = await _subscriptionRepo.Query(tenantId).FirstOrDefaultAsync(s => s.Id == id);
            if (subscription == null) { response.SetError("Subscription not found.", 404); return response; }

            subscription.Status = "paused";
            subscription.UpdatedAt = DateTime.UtcNow;
            await _subscriptionRepo.UpdateAsync(subscription);

            response.SetSuccess(true, "Subscription paused.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> ResumeAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;

            var subscription = await _subscriptionRepo.Query(tenantId).FirstOrDefaultAsync(s => s.Id == id);
            if (subscription == null) { response.SetError("Subscription not found.", 404); return response; }

            subscription.Status = "active";
            subscription.UpdatedAt = DateTime.UtcNow;
            await _subscriptionRepo.UpdateAsync(subscription);

            response.SetSuccess(true, "Subscription resumed.");
            return response;
        }

        public async Task<GatewayResponseWrapper<ProrationPreviewDto>> PreviewProrationAsync(Guid id, Guid newPlanId)
        {
            var response = new GatewayResponseWrapper<ProrationPreviewDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var subscription = await _subscriptionRepo.Query(tenantId)
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subscription == null) { response.SetError("Subscription not found.", 404); return response; }

            var newPlan = await _planRepo.GetByIdAsync(newPlanId);
            if (newPlan == null) { response.SetError("New plan not found."); return response; }

            var currentPlan = subscription.Plan;
            var daysLeft = (subscription.CurrentPeriodEnd - DateTime.UtcNow).TotalDays;
            var totalDays = (subscription.CurrentPeriodEnd - subscription.CurrentPeriodStart).TotalDays;
            var ratio = totalDays > 0 ? daysLeft / totalDays : 0;

            var creditRemaining = currentPlan.Amount * (decimal)ratio;
            var newCharge = newPlan.Amount * (decimal)ratio;
            var proratedAmount = newCharge - creditRemaining;

            response.SetSuccess(new ProrationPreviewDto
            {
                CurrentPlan = new SubscriptionPlanResponseDto { Id = currentPlan.Id, Name = currentPlan.Name, Amount = currentPlan.Amount, Currency = currentPlan.Currency, Interval = currentPlan.Interval },
                NewPlan = new SubscriptionPlanResponseDto { Id = newPlan.Id, Name = newPlan.Name, Amount = newPlan.Amount, Currency = newPlan.Currency, Interval = newPlan.Interval },
                ProratedAmount = Math.Round(proratedAmount, 2),
                EffectiveDate = DateTime.UtcNow,
                ImmediateCharge = proratedAmount > 0 ? Math.Round(proratedAmount, 2) : 0,
                NextInvoiceAmount = newPlan.Amount
            });
            return response;
        }

        private static SubscriptionResponseDto MapToDto(Subscription s, Customer c, SubscriptionPlan p)
        {
            return new SubscriptionResponseDto
            {
                Id = s.Id,
                CustomerId = s.CustomerId,
                CustomerName = c?.Name ?? "",
                CustomerEmail = c?.Email ?? "",
                PlanId = s.PlanId,
                PlanName = p?.Name ?? "",
                PlanAmount = p?.Amount ?? 0,
                StripeSubscriptionId = s.StripeSubscriptionId ?? "",
                Status = s.Status,
                Quantity = s.Quantity,
                CurrentPeriodStart = s.CurrentPeriodStart,
                CurrentPeriodEnd = s.CurrentPeriodEnd,
                TrialEnd = s.TrialEnd,
                CancelAtPeriodEnd = s.CancelAtPeriodEnd,
                CancelledAt = s.CancelledAt,
                CancellationReason = s.CancellationReason,
                CreatedAt = s.CreatedAt
            };
        }
    }
}
