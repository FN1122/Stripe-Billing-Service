using Core.ContextProviders;
using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.ServiceContracts;
using Core.Utils;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
    public class WebhookEventLogService : BaseService, IWebhookEventLogService
    {
        private readonly BillingDbContext _dbContext;

        public WebhookEventLogService(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
        }

        public async Task<GatewayPaginatedListResponseWrapper<WebhookEventResponseDto>> GetInboundEventsAsync(string? eventType, string? status, int page, int pageSize)
        {
            var response = new GatewayPaginatedListResponseWrapper<WebhookEventResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var query = _dbContext.WebhookEventsInbound.Where(e => e.TenantId == tenantId);

            if (!string.IsNullOrEmpty(eventType)) query = query.Where(e => e.EventType == eventType);
            if (!string.IsNullOrEmpty(status)) query = query.Where(e => e.Status == status);

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(e => e.ReceivedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            response.SetSuccessWithPagination(items.Select(e => new WebhookEventResponseDto
            {
                Id = e.Id, StripeEventId = e.StripeEventId, EventType = e.EventType,
                Status = e.Status, ProcessedAt = e.ProcessedAt, CreatedAt = e.ReceivedAt
            }).ToList(), totalCount, page, pageSize);
            return response;
        }

        public async Task<GatewayResponseWrapper<WebhookEventDetailDto>> GetInboundEventAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<WebhookEventDetailDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var evt = await _dbContext.WebhookEventsInbound.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id);
            if (evt == null) { response.SetError("Event not found.", 404); return response; }

            response.SetSuccess(new WebhookEventDetailDto
            {
                Id = evt.Id, StripeEventId = evt.StripeEventId, EventType = evt.EventType,
                PayloadJson = evt.Payload, Status = evt.Status, ProcessedAt = evt.ProcessedAt, CreatedAt = evt.ReceivedAt
            });
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> ReplayEventAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;
            var evt = await _dbContext.WebhookEventsInbound.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id);
            if (evt == null) { response.SetError("Event not found.", 404); return response; }

            evt.Status = "pending";
            evt.ProcessedAt = null;
            await _dbContext.SaveChangesAsync();
            response.SetSuccess(true, "Event queued for replay.");
            return response;
        }

        public async Task<GatewayPaginatedListResponseWrapper<WebhookDeliveryResponseDto>> GetDeliveryLogAsync(string? status, int page, int pageSize)
        {
            var response = new GatewayPaginatedListResponseWrapper<WebhookDeliveryResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var query = _dbContext.WebhookDeliveries
                .Include(d => d.WebhookSubscription)
                .Where(d => d.WebhookSubscription != null && d.WebhookSubscription.TenantId == tenantId);

            if (!string.IsNullOrEmpty(status)) query = query.Where(d => d.Status == status);

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(d => d.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            response.SetSuccessWithPagination(items.Select(d => new WebhookDeliveryResponseDto
            {
                Id = d.Id, WebhookSubscriptionId = d.WebhookSubscriptionId, EventType = d.EventType,
                TargetUrl = d.TargetUrl, Payload = d.Payload, Status = d.Status,
                StatusCode = d.StatusCode, HttpStatusCode = d.HttpStatusCode,
                ResponseBody = d.ResponseBody, DurationMs = d.DurationMs,
                RetryCount = d.RetryCount, FailureReason = d.FailureReason,
                DeliveredAt = d.DeliveredAt, CreatedAt = d.CreatedAt
            }).ToList(), totalCount, page, pageSize);
            return response;
        }

        public async Task<GatewayResponseWrapper<WebhookDeliveryDetailDto>> GetDeliveryDetailAsync(Guid deliveryId)
        {
            var response = new GatewayResponseWrapper<WebhookDeliveryDetailDto>();
            var delivery = await _dbContext.WebhookDeliveries.FirstOrDefaultAsync(d => d.Id == deliveryId);
            if (delivery == null) { response.SetError("Delivery not found.", 404); return response; }

            response.SetSuccess(new WebhookDeliveryDetailDto
            {
                Id = delivery.Id, WebhookSubscriptionId = delivery.WebhookSubscriptionId,
                EventType = delivery.EventType, PayloadJson = delivery.Payload,
                Status = delivery.Status, HttpStatusCode = delivery.HttpStatusCode,
                ResponseBody = delivery.ResponseBody, Attempts = delivery.RetryCount,
                LastAttemptedAt = delivery.UpdatedAt, NextRetryAt = delivery.NextRetryAt
            });
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> RetryDeliveryAsync(Guid deliveryId)
        {
            var response = new GatewayResponseWrapper<bool>();
            var delivery = await _dbContext.WebhookDeliveries.FirstOrDefaultAsync(d => d.Id == deliveryId);
            if (delivery == null) { response.SetError("Delivery not found.", 404); return response; }

            delivery.Status = "pending";
            delivery.NextRetryAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            response.SetSuccess(true, "Delivery queued for retry.");
            return response;
        }

        public async Task<GatewayResponseWrapper<WebhookEventStatsDto>> GetEventStatsAsync()
        {
            var response = new GatewayResponseWrapper<WebhookEventStatsDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var stats = new WebhookEventStatsDto
            {
                TotalEventsReceived = await _dbContext.WebhookEventsInbound.CountAsync(e => e.TenantId == tenantId),
                TotalEventsProcessed = await _dbContext.WebhookEventsInbound.CountAsync(e => e.TenantId == tenantId && e.Status == "processed"),
                TotalEventsFailed = await _dbContext.WebhookEventsInbound.CountAsync(e => e.TenantId == tenantId && e.Status == "failed"),
            };

            response.SetSuccess(stats);
            return response;
        }
    }
}
