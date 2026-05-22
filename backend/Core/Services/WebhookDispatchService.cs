using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.ServiceContracts;
using Core.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Core.Services
{
    public class WebhookDispatchService : IWebhookDispatchService
    {
        private readonly IWebhookSubscriptionRepository _webhookSubRepo;
        private readonly IWebhookDeliveryRepository _deliveryRepo;

        public WebhookDispatchService(IWebhookSubscriptionRepository webhookSubRepo, IWebhookDeliveryRepository deliveryRepo)
        {
            _webhookSubRepo = webhookSubRepo;
            _deliveryRepo = deliveryRepo;
        }

        public async Task EnqueueAsync(Guid tenantId, string eventType, object eventData)
        {
            try
            {
                var subscriptions = await _webhookSubRepo.GetActiveByTenantAndEventAsync(tenantId, eventType);
                if (!subscriptions.Any()) return;
                var eventDataJson = JsonConvert.SerializeObject(eventData);
                var timestamp = DateTime.UtcNow;
                var deliveries = subscriptions.Select(subscription => new WebhookDelivery
                {
                    WebhookSubscriptionId = subscription.Id, EventType = eventType, EventData = eventDataJson,
                    TargetUrl = subscription.TargetUrl, Status = "pending", RetryCount = 0, MaxRetries = 5,
                    NextRetryAt = timestamp, CreatedAt = timestamp
                }).ToList();
                await _deliveryRepo.CreateRangeAsync(deliveries);
            }
            catch (Exception ex) { /* Log error but don't throw */ }
        }

        public async Task<List<WebhookDelivery>> GetPendingDeliveriesAsync(int maxCount = 100)
        {
            return await _deliveryRepo.GetPendingAsync(maxCount);
        }

        public async Task<GatewayResponseWrapper<bool>> MarkAsDeliveredAsync(Guid deliveryId)
        {
            var response = new GatewayResponseWrapper<bool>();
            var delivery = await _deliveryRepo.GetByIdAsync(deliveryId);
            if (delivery == null) { response.SetError("Delivery not found."); return response; }
            delivery.Status = "delivered"; delivery.DeliveredAt = DateTime.UtcNow;
            await _deliveryRepo.UpdateAsync(delivery);
            response.SetSuccess(true);
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> MarkAsFailedAsync(Guid deliveryId, string errorMessage)
        {
            var response = new GatewayResponseWrapper<bool>();
            var delivery = await _deliveryRepo.GetByIdAsync(deliveryId);
            if (delivery == null) { response.SetError("Delivery not found."); return response; }
            delivery.RetryCount++; delivery.LastError = errorMessage;
            if (delivery.RetryCount >= delivery.MaxRetries) { delivery.Status = "failed"; delivery.FailedAt = DateTime.UtcNow; }
            else { var minutesDelay = (int)Math.Pow(2, delivery.RetryCount - 1); delivery.NextRetryAt = DateTime.UtcNow.AddMinutes(minutesDelay); }
            await _deliveryRepo.UpdateAsync(delivery);
            response.SetSuccess(true);
            return response;
        }

        public async Task<GatewayPaginatedListResponseWrapper<WebhookDeliveryResponseDto>> ListDeliveriesAsync(Guid subscriptionId, WebhookDeliveryFilterDto filter)
        {
            var response = new GatewayPaginatedListResponseWrapper<WebhookDeliveryResponseDto>();
            var query = _deliveryRepo.Query().Where(wd => wd.WebhookSubscriptionId == subscriptionId);
            if (!string.IsNullOrEmpty(filter.Status)) query = query.Where(wd => wd.Status == filter.Status);
            if (!string.IsNullOrEmpty(filter.EventType)) query = query.Where(wd => wd.EventType == filter.EventType);
            if (filter.DateFrom.HasValue) query = query.Where(wd => wd.CreatedAt >= filter.DateFrom.Value);
            if (filter.DateTo.HasValue) query = query.Where(wd => wd.CreatedAt <= filter.DateTo.Value);
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(wd => wd.CreatedAt).Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
            response.SetSuccessWithPagination(items.Select(MapDelivery).ToList(), total, filter.Page, filter.PageSize);
            return response;
        }

        public async Task<GatewayResponseWrapper<WebhookDeliveryDetailResponseDto>> GetDeliveryAsync(Guid deliveryId)
        {
            var response = new GatewayResponseWrapper<WebhookDeliveryDetailResponseDto>();
            var delivery = await _deliveryRepo.GetByIdAsync(deliveryId);
            if (delivery == null) { response.SetError("Delivery not found."); return response; }
            response.SetSuccess(new WebhookDeliveryDetailResponseDto
            {
                Id = delivery.Id, WebhookSubscriptionId = delivery.WebhookSubscriptionId, EventType = delivery.EventType,
                EventData = !string.IsNullOrEmpty(delivery.EventData) ? JsonConvert.DeserializeObject<dynamic>(delivery.EventData) : null,
                TargetUrl = delivery.TargetUrl, Status = delivery.Status, StatusCode = delivery.StatusCode,
                ResponseBody = delivery.ResponseBody, RetryCount = delivery.RetryCount, MaxRetries = delivery.MaxRetries,
                NextRetryAt = delivery.NextRetryAt, LastError = delivery.LastError, CreatedAt = delivery.CreatedAt,
                DeliveredAt = delivery.DeliveredAt, FailedAt = delivery.FailedAt
            });
            return response;
        }

        public async Task<GatewayResponseWrapper<WebhookDeliveryStatsDto>> GetDeliveryStatsAsync(Guid tenantId)
        {
            var response = new GatewayResponseWrapper<WebhookDeliveryStatsDto>();
            var deliveries = await _deliveryRepo.QueryByTenant(tenantId).ToListAsync();
            response.SetSuccess(new WebhookDeliveryStatsDto
            {
                TotalDeliveries = deliveries.Count, SuccessCount = deliveries.Count(wd => wd.Status == "delivered"),
                FailedCount = deliveries.Count(wd => wd.Status == "failed"), PendingCount = deliveries.Count(wd => wd.Status == "pending"),
                SuccessRate = deliveries.Count > 0 ? Math.Round((decimal)deliveries.Count(wd => wd.Status == "delivered") / deliveries.Count * 100, 1) : 0,
                AverageRetries = deliveries.Count > 0 ? Math.Round(deliveries.Average(wd => wd.RetryCount), 1) : 0,
                ByEventType = deliveries.GroupBy(wd => wd.EventType).ToDictionary(g => g.Key, g => g.Count()),
                ByStatus = deliveries.GroupBy(wd => wd.Status).ToDictionary(g => g.Key, g => g.Count()),
                Last24HoursDeliveries = deliveries.Where(wd => wd.CreatedAt >= DateTime.UtcNow.AddDays(-1)).Count(),
                Last24HoursSuccessRate = CalculateSuccess24h(deliveries)
            });
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> RetryDeliveryAsync(Guid deliveryId)
        {
            var response = new GatewayResponseWrapper<bool>();
            var delivery = await _deliveryRepo.GetByIdAsync(deliveryId);
            if (delivery == null) { response.SetError("Delivery not found."); return response; }
            if (delivery.Status == "delivered") { response.SetError("Cannot retry a successfully delivered webhook."); return response; }
            if (delivery.Status == "failed" && delivery.RetryCount >= delivery.MaxRetries)
            { delivery.RetryCount = 0; delivery.Status = "pending"; delivery.NextRetryAt = DateTime.UtcNow; delivery.LastError = null; delivery.FailedAt = null; }
            else { delivery.NextRetryAt = DateTime.UtcNow; }
            await _deliveryRepo.UpdateAsync(delivery);
            response.SetSuccess(true, "Delivery queued for retry.");
            return response;
        }

        private static WebhookDeliveryResponseDto MapDelivery(WebhookDelivery wd) => new() { Id = wd.Id, EventType = wd.EventType, TargetUrl = wd.TargetUrl, Status = wd.Status, StatusCode = wd.StatusCode, RetryCount = wd.RetryCount, CreatedAt = wd.CreatedAt, DeliveredAt = wd.DeliveredAt };
        private static decimal CalculateSuccess24h(List<WebhookDelivery> deliveries) { var last24h = deliveries.Where(wd => wd.CreatedAt >= DateTime.UtcNow.AddDays(-1)).ToList(); if (last24h.Count == 0) return 0; return Math.Round((decimal)last24h.Count(wd => wd.Status == "delivered") / last24h.Count * 100, 1); }
    }
}
