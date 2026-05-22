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
    public class DunningService : BaseService, IDunningService
    {
        private readonly IDunningRepository _dunningRepo;

        public DunningService(ITenantContextProvider tenantContextProvider, IDunningRepository dunningRepo) : base(tenantContextProvider)
        {
            _dunningRepo = dunningRepo;
        }

        public async Task<GatewayResponseWrapper<DunningConfigDto>> GetConfigAsync()
        {
            var response = new GatewayResponseWrapper<DunningConfigDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var steps = await _dunningRepo.GetStepsAsync(tenantId);

            var config = new DunningConfigDto
            {
                Steps = steps.Select(s => new DunningStepConfigDto
                {
                    DaysAfterFailure = s.DaysAfterFailure,
                    Action = s.Action,
                    EmailTemplateKey = s.EmailTemplateKey
                }).ToList()
            };

            if (!config.Steps.Any())
            {
                config.Steps = new List<DunningStepConfigDto>
                {
                    new() { DaysAfterFailure = 1, Action = "retry_payment" },
                    new() { DaysAfterFailure = 3, Action = "send_email", EmailTemplateKey = "dunning.reminder" },
                    new() { DaysAfterFailure = 7, Action = "retry_payment" },
                    new() { DaysAfterFailure = 14, Action = "send_email", EmailTemplateKey = "dunning.final_warning" },
                    new() { DaysAfterFailure = 21, Action = "cancel_subscription" }
                };
            }

            response.SetSuccess(config);
            return response;
        }

        public async Task<GatewayResponseWrapper<DunningConfigDto>> UpdateConfigAsync(DunningConfigDto request)
        {
            var response = new GatewayResponseWrapper<DunningConfigDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var steps = request.Steps.Select((s, i) => new DunningStep
            {
                TenantId = tenantId,
                SortOrder = i,
                DaysAfterFailure = s.DaysAfterFailure,
                Action = s.Action,
                EmailTemplateKey = s.EmailTemplateKey
            }).ToList();

            await _dunningRepo.ReplaceStepsAsync(tenantId, steps);
            response.SetSuccess(request);
            return response;
        }

        public async Task<GatewayPaginatedListResponseWrapper<DunningScheduleResponseDto>> GetSchedulesAsync(DunningFilterDto filter)
        {
            var response = new GatewayPaginatedListResponseWrapper<DunningScheduleResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var query = _dunningRepo.Query(tenantId);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(d => d.Status == filter.Status);
            if (filter.CustomerId.HasValue)
                query = query.Where(d => d.CustomerId == filter.CustomerId.Value);

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(d => d.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize).ToListAsync();

            response.SetSuccessWithPagination(items.Select(MapSchedule).ToList(), totalCount, filter.Page, filter.PageSize);
            return response;
        }

        public async Task<GatewayResponseWrapper<DunningScheduleResponseDto>> GetScheduleAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<DunningScheduleResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var schedule = await _dunningRepo.GetByIdAsync(tenantId, id);
            if (schedule == null) { response.SetError("Dunning schedule not found.", 404); return response; }
            response.SetSuccess(MapSchedule(schedule));
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> PauseScheduleAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;
            var schedule = await _dunningRepo.GetByIdAsync(tenantId, id);
            if (schedule == null) { response.SetError("Dunning schedule not found.", 404); return response; }
            schedule.Status = "paused";
            schedule.UpdatedAt = DateTime.UtcNow;
            await _dunningRepo.UpdateAsync(schedule);
            response.SetSuccess(true, "Dunning schedule paused.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> ResumeScheduleAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;
            var schedule = await _dunningRepo.GetByIdAsync(tenantId, id);
            if (schedule == null) { response.SetError("Dunning schedule not found.", 404); return response; }
            if (schedule.Status != "paused") { response.SetError("Only paused schedules can be resumed.", 400); return response; }
            schedule.Status = "active";
            schedule.UpdatedAt = DateTime.UtcNow;
            await _dunningRepo.UpdateAsync(schedule);
            response.SetSuccess(true, "Dunning schedule resumed.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> CancelScheduleAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;
            var schedule = await _dunningRepo.GetByIdAsync(tenantId, id);
            if (schedule == null) { response.SetError("Dunning schedule not found.", 404); return response; }
            schedule.Status = "cancelled";
            schedule.UpdatedAt = DateTime.UtcNow;
            await _dunningRepo.UpdateAsync(schedule);
            response.SetSuccess(true, "Dunning schedule cancelled.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> ManualRetryAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;
            var schedule = await _dunningRepo.GetByIdAsync(tenantId, id);
            if (schedule == null) { response.SetError("Dunning schedule not found.", 404); return response; }

            schedule.TotalRetryAttempts++;
            schedule.LastRetryAt = DateTime.UtcNow;
            schedule.UpdatedAt = DateTime.UtcNow;
            await _dunningRepo.UpdateAsync(schedule);
            response.SetSuccess(true, "Manual retry initiated.");
            return response;
        }

        public async Task<GatewayResponseWrapper<DunningDashboardDto>> GetDashboardAsync()
        {
            var response = new GatewayResponseWrapper<DunningDashboardDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var dashboard = new DunningDashboardDto
            {
                ActiveDunningCount = await _dunningRepo.CountByStatusAsync(tenantId, "active"),
                RecoveredCount = await _dunningRepo.CountByStatusAsync(tenantId, "completed"),
                LostCount = await _dunningRepo.CountByStatusAsync(tenantId, "cancelled"),
                TotalAmountAtRisk = await _dunningRepo.SumAmountByStatusAsync(tenantId, "active"),
                TotalRecoveredAmount = await _dunningRepo.SumAmountByStatusAsync(tenantId, "completed"),
            };

            var total = dashboard.RecoveredCount + dashboard.LostCount;
            dashboard.RecoveryRate = total > 0 ? Math.Round((decimal)dashboard.RecoveredCount / total * 100, 2) : 0;

            var recent = await _dunningRepo.Query(tenantId)
                .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt).Take(10).ToListAsync();
            dashboard.RecentActivity = recent.Select(MapSchedule).ToList();

            response.SetSuccess(dashboard);
            return response;
        }

        public async Task InitiateDunningAsync(Guid tenantId, Guid subscriptionId, Guid customerId, string? invoiceId, decimal amount, string? reason)
        {
            var existing = await _dunningRepo.GetBySubscriptionAsync(tenantId, subscriptionId);
            if (existing != null) return;

            var steps = await _dunningRepo.GetStepsAsync(tenantId);
            var firstStep = steps.FirstOrDefault();

            var schedule = new DunningSchedule
            {
                TenantId = tenantId,
                SubscriptionId = subscriptionId,
                CustomerId = customerId,
                StripeInvoiceId = invoiceId,
                AmountDue = amount,
                FailureReason = reason,
                MaxSteps = steps.Count,
                NextRetryAt = firstStep != null ? DateTime.UtcNow.AddDays(firstStep.DaysAfterFailure) : DateTime.UtcNow.AddDays(1),
                GracePeriodEndsAt = DateTime.UtcNow.AddDays(3)
            };

            await _dunningRepo.CreateAsync(schedule);
        }

        private DunningScheduleResponseDto MapSchedule(DunningSchedule d) => new()
        {
            Id = d.Id,
            SubscriptionId = d.SubscriptionId,
            CustomerId = d.CustomerId,
            CustomerName = d.Customer?.Name,
            CustomerEmail = d.Customer?.Email,
            StripeInvoiceId = d.StripeInvoiceId,
            Status = d.Status,
            CurrentStep = d.CurrentStep,
            MaxSteps = d.MaxSteps,
            NextRetryAt = d.NextRetryAt,
            LastRetryAt = d.LastRetryAt,
            TotalRetryAttempts = d.TotalRetryAttempts,
            OriginalFailureDate = d.OriginalFailureDate,
            FailureReason = d.FailureReason,
            AmountDue = d.AmountDue,
            Currency = d.Currency,
            GracePeriodEndsAt = d.GracePeriodEndsAt,
            CreatedAt = d.CreatedAt
        };
    }
}
