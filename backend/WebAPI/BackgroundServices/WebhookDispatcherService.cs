using Core.Infrastructure;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;

namespace StripeBilling.API.BackgroundServices
{
    public class WebhookDispatcherService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WebhookDispatcherService> _logger;

        public WebhookDispatcherService(IServiceProvider serviceProvider, ILogger<WebhookDispatcherService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
                    var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

                    var pendingDeliveries = await db.WebhookDeliveries
                        .Include(d => d.WebhookSubscription)
                        .Where(d => d.Status == "Pending")
                        .Take(50)
                        .ToListAsync(stoppingToken);

                    foreach (var delivery in pendingDeliveries)
                    {
                        try
                        {
                            var client = httpClientFactory.CreateClient("Webhook");
                            client.Timeout = TimeSpan.FromSeconds(30);

                            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                            var signature = WebhookSignatureService.Sign(
                                delivery.Payload + timestamp,
                                delivery.WebhookSubscription.HmacSecret ?? "");

                            var request = new HttpRequestMessage(HttpMethod.Post, delivery.WebhookSubscription.WebhookUrl)
                            {
                                Content = new StringContent(delivery.Payload, Encoding.UTF8, "application/json")
                            };

                            request.Headers.Add("X-Webhook-Signature", signature);
                            request.Headers.Add("X-Webhook-Timestamp", timestamp);
                            request.Headers.Add("X-Webhook-ID", delivery.Id.ToString());
                            request.Headers.Add("X-Webhook-Retry", delivery.RetryCount.ToString());

                            var sw = System.Diagnostics.Stopwatch.StartNew();
                            var response = await client.SendAsync(request, stoppingToken);
                            sw.Stop();

                            delivery.HttpStatusCode = (int)response.StatusCode;
                            delivery.DurationMs = (int)sw.ElapsedMilliseconds;
                            delivery.ResponseBody = await response.Content.ReadAsStringAsync(stoppingToken);

                            if (response.IsSuccessStatusCode)
                            {
                                delivery.Status = "Delivered";
                                delivery.DeliveredAt = DateTime.UtcNow;
                            }
                            else
                            {
                                delivery.Status = "Failed";
                                delivery.FailureReason = $"HTTP {(int)response.StatusCode}";
                                delivery.RetryCount++;
                                ScheduleRetry(delivery);
                            }
                        }
                        catch (Exception ex)
                        {
                            delivery.Status = "Failed";
                            delivery.FailureReason = ex.Message;
                            delivery.RetryCount++;
                            ScheduleRetry(delivery);
                        }

                        delivery.UpdatedAt = DateTime.UtcNow;
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in WebhookDispatcherService");
                }

                try { await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        private static void ScheduleRetry(WebhookDelivery delivery)
        {
            if (delivery.RetryCount >= delivery.MaxAttempts)
            {
                delivery.Status = "PermanentlyFailed";
                return;
            }

            var delays = new[] { 60, 300, 1800, 7200, 86400 }; // 1m, 5m, 30m, 2h, 24h
            var delayIndex = Math.Min(delivery.RetryCount - 1, delays.Length - 1);
            delivery.NextRetryAt = DateTime.UtcNow.AddSeconds(delays[delayIndex]);
            delivery.Status = "Pending";
        }
    }
}
