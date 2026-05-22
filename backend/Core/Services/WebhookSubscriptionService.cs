using Core.ContextProviders;
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
    public class WebhookSubscriptionService : BaseService, IWebhookSubscriptionService
    {
        private readonly IWebhookSubscriptionRepository _webhookSubRepo;
        private readonly IWebhookDeliveryRepository _deliveryRepo;

        public WebhookSubscriptionService(ITenantContextProvider tcp, IWebhookSubscriptionRepository webhookSubRepo, IWebhookDeliveryRepository deliveryRepo) : base(tcp)
        {
            _webhookSubRepo = webhookSubRepo;
            _deliveryRepo = deliveryRepo;
        }

        public async Task<GatewayResponseWrapper<WebhookSubscriptionResponseDto>> CreateAsync(CreateWebhookSubscriptionDto request)
        {
            var response = new GatewayResponseWrapper<WebhookSubscriptionResponseDto>();
            if (string.IsNullOrEmpty(request.TargetUrl) || !Uri.TryCreate(request.TargetUrl, UriKind.Absolute, out _)) { response.SetError("Invalid target URL."); return response; }
            if (request.Events == null || !request.Events.Any()) { response.SetError("At least one event must be specified."); return response; }
            var secret = GenerateWebhookSecret();
            var subscription = new WebhookSubscription
            {
                TenantId = CurrentTenantContext.TenantId, TargetUrl = request.TargetUrl, Events = string.Join(",", request.Events),
                Secret = secret, IsActive = true, Description = request.Description,
                Metadata = request.Metadata != null ? JsonConvert.SerializeObject(request.Metadata) : null,
                RetryPolicy = request.RetryPolicy ?? "exponential", MaxRetries = request.MaxRetries ?? 5, Timeout = request.Timeout ?? 30
            };
            await _webhookSubRepo.CreateAsync(subscription);
            response.SetSuccess(new WebhookSubscriptionResponseDto { Id = subscription.Id, TargetUrl = subscription.TargetUrl, Events = subscription.Events.Split(',').ToList(), Secret = secret, IsActive = subscription.IsActive, CreatedAt = subscription.CreatedAt });
            return response;
        }

        public async Task<GatewayResponseWrapper<WebhookSubscriptionResponseDto>> GetAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<WebhookSubscriptionResponseDto>();
            var subscription = await _webhookSubRepo.GetByIdAndTenantAsync(CurrentTenantContext.TenantId, id);
            if (subscription == null) { response.SetError("Webhook subscription not found."); return response; }
            response.SetSuccess(MapWebhookSubscription(subscription));
            return response;
        }

        public async Task<GatewayPaginatedListResponseWrapper<WebhookSubscriptionResponseDto>> ListAsync(WebhookSubscriptionFilterDto filter)
        {
            var response = new GatewayPaginatedListResponseWrapper<WebhookSubscriptionResponseDto>();
            var query = _webhookSubRepo.Query(CurrentTenantContext.TenantId);
            if (!string.IsNullOrEmpty(filter.TargetUrl)) query = query.Where(ws => ws.TargetUrl.Contains(filter.TargetUrl));
            if (filter.IsActive.HasValue) query = query.Where(ws => ws.IsActive == filter.IsActive.Value);
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(ws => ws.CreatedAt).Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
            response.SetSuccessWithPagination(items.Select(MapWebhookSubscription).ToList(), total, filter.Page, filter.PageSize);
            return response;
        }

        public async Task<GatewayResponseWrapper<WebhookSubscriptionResponseDto>> UpdateAsync(Guid id, UpdateWebhookSubscriptionDto request)
        {
            var response = new GatewayResponseWrapper<WebhookSubscriptionResponseDto>();
            var subscription = await _webhookSubRepo.GetByIdAndTenantAsync(CurrentTenantContext.TenantId, id);
            if (subscription == null) { response.SetError("Webhook subscription not found."); return response; }
            if (request.TargetUrl != null) { if (!Uri.TryCreate(request.TargetUrl, UriKind.Absolute, out _)) { response.SetError("Invalid target URL."); return response; } subscription.TargetUrl = request.TargetUrl; }
            if (request.Events != null && request.Events.Any()) subscription.Events = string.Join(",", request.Events);
            if (request.Description != null) subscription.Description = request.Description;
            if (request.MaxRetries.HasValue) subscription.MaxRetries = request.MaxRetries.Value;
            if (request.Timeout.HasValue) subscription.Timeout = request.Timeout.Value;
            if (request.Metadata != null) subscription.Metadata = JsonConvert.SerializeObject(request.Metadata);
            subscription.UpdatedAt = DateTime.UtcNow;
            await _webhookSubRepo.UpdateAsync(subscription);
            response.SetSuccess(MapWebhookSubscription(subscription));
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> DeleteAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var subscription = await _webhookSubRepo.GetByIdAndTenantAsync(CurrentTenantContext.TenantId, id);
            if (subscription == null) { response.SetError("Webhook subscription not found."); return response; }
            await _webhookSubRepo.DeleteAsync(subscription);
            response.SetSuccess(true, "Webhook subscription deleted.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> DisableAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var subscription = await _webhookSubRepo.GetByIdAndTenantAsync(CurrentTenantContext.TenantId, id);
            if (subscription == null) { response.SetError("Webhook subscription not found."); return response; }
            subscription.IsActive = false; subscription.DisabledAt = DateTime.UtcNow;
            await _webhookSubRepo.UpdateAsync(subscription);
            response.SetSuccess(true, "Webhook subscription disabled.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> EnableAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var subscription = await _webhookSubRepo.GetByIdAndTenantAsync(CurrentTenantContext.TenantId, id);
            if (subscription == null) { response.SetError("Webhook subscription not found."); return response; }
            subscription.IsActive = true; subscription.DisabledAt = null;
            await _webhookSubRepo.UpdateAsync(subscription);
            response.SetSuccess(true, "Webhook subscription enabled.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> TestAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            var subscription = await _webhookSubRepo.GetByIdAndTenantAsync(CurrentTenantContext.TenantId, id);
            if (subscription == null) { response.SetError("Webhook subscription not found."); return response; }
            var testDelivery = new WebhookDelivery
            {
                WebhookSubscriptionId = subscription.Id, EventType = "test.event",
                EventData = JsonConvert.SerializeObject(new { message = "Test webhook event", timestamp = DateTime.UtcNow }),
                TargetUrl = subscription.TargetUrl, Status = "pending", RetryCount = 0, MaxRetries = subscription.MaxRetries, NextRetryAt = DateTime.UtcNow
            };
            await _deliveryRepo.CreateAsync(testDelivery);
            response.SetSuccess(true, "Test webhook queued for delivery.");
            return response;
        }

        public async Task<GatewayResponseWrapper<WebhookSubscriptionStatsDto>> GetStatsAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<WebhookSubscriptionStatsDto>();
            var subscription = await _webhookSubRepo.GetByIdWithDeliveriesAsync(CurrentTenantContext.TenantId, id);
            if (subscription == null) { response.SetError("Webhook subscription not found."); return response; }
            var deliveries = subscription.WebhookDeliveries?.ToList() ?? new List<WebhookDelivery>();
            var last7d = DateTime.UtcNow.AddDays(-7);
            response.SetSuccess(new WebhookSubscriptionStatsDto
            {
                TotalDeliveries = deliveries.Count, SuccessfulDeliveries = deliveries.Count(d => d.Status == "delivered"),
                FailedDeliveries = deliveries.Count(d => d.Status == "failed"), PendingDeliveries = deliveries.Count(d => d.Status == "pending"),
                SuccessRate = deliveries.Count > 0 ? Math.Round((decimal)deliveries.Count(d => d.Status == "delivered") / deliveries.Count * 100, 1) : 0,
                AverageRetries = deliveries.Count > 0 ? Math.Round(deliveries.Average(d => d.RetryCount), 1) : 0,
                Deliveries7d = deliveries.Count(d => d.CreatedAt >= last7d),
                SuccessRate7d = CalculateSuccessRate7d(deliveries),
                MostRecentDelivery = deliveries.OrderByDescending(d => d.CreatedAt).FirstOrDefault()?.CreatedAt,
                EstimatedMonthlyWebhooks = EstimateMonthlyWebhooks(deliveries)
            });
            return response;
        }

        public async Task<GatewayResponseWrapper<string>> RotateSecretAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<string>();
            var subscription = await _webhookSubRepo.GetByIdAndTenantAsync(CurrentTenantContext.TenantId, id);
            if (subscription == null) { response.SetError("Webhook subscription not found."); return response; }
            var newSecret = GenerateWebhookSecret();
            subscription.Secret = newSecret; subscription.UpdatedAt = DateTime.UtcNow;
            await _webhookSubRepo.UpdateAsync(subscription);
            response.SetSuccess(newSecret, "Webhook secret rotated. Update your verification logic with the new secret.");
            return response;
        }

        private static string GenerateWebhookSecret() { var randomBytes = new byte[32]; using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider()) { rng.GetBytes(randomBytes); } return "whsec_" + Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('='); }
        private static WebhookSubscriptionResponseDto MapWebhookSubscription(WebhookSubscription ws) => new() { Id = ws.Id, TargetUrl = ws.TargetUrl, Events = ws.Events.Split(',').ToList(), IsActive = ws.IsActive, Description = ws.Description, CreatedAt = ws.CreatedAt, UpdatedAt = ws.UpdatedAt };
        private static decimal CalculateSuccessRate7d(List<WebhookDelivery> deliveries) { var last7d = DateTime.UtcNow.AddDays(-7); var recent = deliveries.Where(d => d.CreatedAt >= last7d).ToList(); if (recent.Count == 0) return 0; return Math.Round((decimal)recent.Count(d => d.Status == "delivered") / recent.Count * 100, 1); }
        private static int EstimateMonthlyWebhooks(List<WebhookDelivery> deliveries) { if (deliveries.Count == 0) return 0; var last7d = DateTime.UtcNow.AddDays(-7); var last7dCount = deliveries.Count(d => d.CreatedAt >= last7d); return last7dCount > 0 ? (int)(last7dCount * 4.29) : 0; }
    }
}
