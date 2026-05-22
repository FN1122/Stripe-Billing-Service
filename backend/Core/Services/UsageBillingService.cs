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
    public class UsageBillingService : BaseService, IUsageBillingService
    {
        private readonly IUsageRepository _usageRepo;
        private readonly ISubscriptionRepository _subscriptionRepo;

        public UsageBillingService(
            ITenantContextProvider tenantContextProvider,
            IUsageRepository usageRepo,
            ISubscriptionRepository subscriptionRepo) : base(tenantContextProvider)
        {
            _usageRepo = usageRepo;
            _subscriptionRepo = subscriptionRepo;
        }

        public async Task<GatewayResponseWrapper<UsageRecordResponseDto>> ReportUsageAsync(CreateUsageRecordDto request)
        {
            var response = new GatewayResponseWrapper<UsageRecordResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            if (!string.IsNullOrEmpty(request.IdempotencyKey))
            {
                var existing = await _usageRepo.GetByIdempotencyKeyAsync(tenantId, request.IdempotencyKey);
                if (existing != null)
                {
                    response.SetSuccess(MapUsageRecord(existing), "Duplicate request detected, returning existing record.");
                    return response;
                }
            }

            var record = new UsageRecord
            {
                TenantId = tenantId,
                SubscriptionId = request.SubscriptionId,
                Quantity = request.Quantity,
                Timestamp = request.Timestamp ?? DateTime.UtcNow,
                Action = request.Action,
                IdempotencyKey = request.IdempotencyKey
            };

            await _usageRepo.CreateAsync(record);
            response.SetSuccess(MapUsageRecord(record));
            return response;
        }

        public async Task<GatewayResponseWrapper<List<UsageRecordResponseDto>>> BatchReportUsageAsync(BatchUsageRecordDto request)
        {
            var response = new GatewayResponseWrapper<List<UsageRecordResponseDto>>();
            var tenantId = CurrentTenantContext.TenantId;

            if (request.Records.Count > 100)
            {
                response.SetError("Maximum 100 records per batch.");
                return response;
            }

            var records = request.Records.Select(r => new UsageRecord
            {
                TenantId = tenantId,
                SubscriptionId = r.SubscriptionId,
                Quantity = r.Quantity,
                Timestamp = r.Timestamp ?? DateTime.UtcNow,
                Action = r.Action,
                IdempotencyKey = r.IdempotencyKey
            }).ToList();

            await _usageRepo.CreateRangeAsync(records);
            response.SetSuccess(records.Select(MapUsageRecord).ToList());
            return response;
        }

        public async Task<GatewayPaginatedListResponseWrapper<UsageRecordResponseDto>> GetUsageAsync(UsageFilterDto filter)
        {
            var response = new GatewayPaginatedListResponseWrapper<UsageRecordResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var query = _usageRepo.Query(tenantId);

            if (filter.SubscriptionId.HasValue)
                query = query.Where(u => u.SubscriptionId == filter.SubscriptionId.Value);
            if (filter.CustomerId.HasValue)
                query = query.Where(u => u.Subscription != null && u.Subscription.CustomerId == filter.CustomerId.Value);
            if (filter.FromDate.HasValue)
                query = query.Where(u => u.Timestamp >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(u => u.Timestamp <= filter.ToDate.Value);

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(u => u.Timestamp)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize).ToListAsync();

            response.SetSuccessWithPagination(items.Select(MapUsageRecord).ToList(), totalCount, filter.Page, filter.PageSize);
            return response;
        }

        public async Task<GatewayResponseWrapper<UsageSummaryDto>> GetUsageSummaryAsync(Guid subscriptionId)
        {
            var response = new GatewayResponseWrapper<UsageSummaryDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var now = DateTime.UtcNow;
            var periodStart = new DateTime(now.Year, now.Month, 1);
            var prevPeriodStart = periodStart.AddMonths(-1);

            var currentUsage = await _usageRepo.SumUsageAsync(tenantId, subscriptionId, periodStart, now);
            var previousUsage = await _usageRepo.SumUsageAsync(tenantId, subscriptionId, prevPeriodStart, periodStart);

            var changePercent = previousUsage > 0 ? ((decimal)(currentUsage - previousUsage) / previousUsage) * 100 : 0;

            var dailyRecords = await _usageRepo.Query(tenantId)
                .Where(u => u.SubscriptionId == subscriptionId && u.Timestamp >= periodStart)
                .GroupBy(u => u.Timestamp.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(x => x.Quantity) })
                .ToListAsync();

            var summary = new UsageSummaryDto
            {
                SubscriptionId = subscriptionId,
                CurrentPeriodUsage = currentUsage,
                PreviousPeriodUsage = previousUsage,
                UsageChangePercent = Math.Round(changePercent, 2),
                DailyUsage = dailyRecords.ToDictionary(d => d.Date.ToString("yyyy-MM-dd"), d => d.Total)
            };

            response.SetSuccess(summary);
            return response;
        }

        public async Task<GatewayResponseWrapper<MeterEventResponseDto>> CreateMeterEventAsync(CreateMeterEventDto request)
        {
            var response = new GatewayResponseWrapper<MeterEventResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var meterEvent = new MeterEvent
            {
                TenantId = tenantId,
                CustomerId = request.CustomerId,
                EventName = request.EventName,
                Value = request.Value,
                Timestamp = request.Timestamp ?? DateTime.UtcNow,
                Properties = request.Properties
            };

            await _usageRepo.CreateMeterEventAsync(meterEvent);
            response.SetSuccess(MapMeterEvent(meterEvent));
            return response;
        }

        public async Task<GatewayPaginatedListResponseWrapper<MeterEventResponseDto>> GetMeterEventsAsync(Guid? customerId, string? eventName, int page, int pageSize)
        {
            var response = new GatewayPaginatedListResponseWrapper<MeterEventResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var query = _usageRepo.QueryMeterEvents(tenantId);

            if (customerId.HasValue)
                query = query.Where(m => m.CustomerId == customerId.Value);
            if (!string.IsNullOrEmpty(eventName))
                query = query.Where(m => m.EventName == eventName);

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(m => m.Timestamp)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            response.SetSuccessWithPagination(items.Select(MapMeterEvent).ToList(), totalCount, page, pageSize);
            return response;
        }

        public async Task<GatewayResponseWrapper<UsageDashboardDto>> GetUsageDashboardAsync()
        {
            var response = new GatewayResponseWrapper<UsageDashboardDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var now = DateTime.UtcNow;
            var periodStart = new DateTime(now.Year, now.Month, 1);

            var currentPeriodRecords = _usageRepo.Query(tenantId)
                .Where(u => u.Timestamp >= periodStart);

            // Total usage for current period
            var totalUsage = await currentPeriodRecords.SumAsync(u => u.Quantity);

            // Active metered subscriptions (distinct subscriptions with usage this period)
            var activeMetered = await currentPeriodRecords
                .Select(u => u.SubscriptionId)
                .Distinct()
                .CountAsync();

            // Estimated revenue: sum usage * plan amount per unit for each subscription
            var subscriptionUsages = await currentPeriodRecords
                .GroupBy(u => u.SubscriptionId)
                .Select(g => new { SubscriptionId = g.Key, TotalUsage = g.Sum(x => x.Quantity) })
                .ToListAsync();

            var subscriptionIds = subscriptionUsages.Select(s => s.SubscriptionId).ToList();
            var subscriptions = await _subscriptionRepo.Query(tenantId)
                .Where(s => subscriptionIds.Contains(s.Id))
                .ToListAsync();

            var estimatedRevenue = 0m;
            foreach (var su in subscriptionUsages)
            {
                var sub = subscriptions.FirstOrDefault(s => s.Id == su.SubscriptionId);
                if (sub?.Plan != null)
                {
                    estimatedRevenue += su.TotalUsage * sub.Plan.Amount;
                }
            }

            // Top consumers: group by customer, sum usage, take top 5
            var topConsumerData = await currentPeriodRecords
                .Where(u => u.Subscription != null)
                .GroupBy(u => u.Subscription!.CustomerId)
                .Select(g => new { CustomerId = g.Key, TotalUsage = g.Sum(x => x.Quantity) })
                .OrderByDescending(c => c.TotalUsage)
                .Take(5)
                .ToListAsync();

            // Look up customer names and calculate estimated charges from in-memory data
            var topConsumers = topConsumerData.Select(tc =>
            {
                var consumerSubs = subscriptions.Where(s => s.CustomerId == tc.CustomerId).ToList();
                var customerName = consumerSubs.FirstOrDefault()?.Customer?.Name;
                var estimatedCharge = 0m;
                if (consumerSubs.Any(s => s.Plan != null))
                {
                    var avgRate = consumerSubs.Where(s => s.Plan != null).Average(s => s.Plan.Amount);
                    estimatedCharge = tc.TotalUsage * avgRate;
                }
                return new TopConsumerDto
                {
                    CustomerId = tc.CustomerId,
                    CustomerName = customerName,
                    TotalUsage = tc.TotalUsage,
                    EstimatedCharge = Math.Round(estimatedCharge, 2),
                };
            }).ToList();

            // Usage trend: daily totals for the current period
            var dailyUsage = await currentPeriodRecords
                .GroupBy(u => u.Timestamp.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(x => x.Quantity) })
                .OrderBy(d => d.Date)
                .ToListAsync();

            var dashboard = new UsageDashboardDto
            {
                TotalUsageCurrentPeriod = totalUsage,
                ActiveMeteredSubscriptions = activeMetered,
                EstimatedRevenue = Math.Round(estimatedRevenue, 2),
                TopConsumers = topConsumers,
                UsageTrend = dailyUsage.ToDictionary(d => d.Date.ToString("yyyy-MM-dd"), d => d.Total),
            };

            response.SetSuccess(dashboard);
            return response;
        }

        private UsageRecordResponseDto MapUsageRecord(UsageRecord u) => new()
        {
            Id = u.Id,
            SubscriptionId = u.SubscriptionId,
            StripeSubscriptionItemId = u.StripeSubscriptionItemId,
            Quantity = u.Quantity,
            Timestamp = u.Timestamp,
            Action = u.Action,
            IdempotencyKey = u.IdempotencyKey,
            CreatedAt = u.CreatedAt
        };

        private MeterEventResponseDto MapMeterEvent(MeterEvent m) => new()
        {
            Id = m.Id,
            CustomerId = m.CustomerId,
            EventName = m.EventName,
            Value = m.Value,
            Timestamp = m.Timestamp,
            Properties = m.Properties,
            CreatedAt = m.CreatedAt
        };
    }
}
