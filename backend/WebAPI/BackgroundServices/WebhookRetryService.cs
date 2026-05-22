using Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StripeBilling.API.BackgroundServices
{
    public class WebhookRetryService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WebhookRetryService> _logger;

        public WebhookRetryService(IServiceProvider serviceProvider, ILogger<WebhookRetryService> logger)
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

                    var retryDeliveries = await db.WebhookDeliveries
                        .Where(d => d.Status == "Failed"
                            && d.NextRetryAt != null
                            && d.NextRetryAt <= DateTime.UtcNow
                            && d.RetryCount < d.MaxAttempts)
                        .Take(20)
                        .ToListAsync(stoppingToken);

                    foreach (var delivery in retryDeliveries)
                    {
                        delivery.Status = "Pending";
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
                    _logger.LogError(ex, "Error in WebhookRetryService");
                }

                try { await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); }
                catch (OperationCanceledException ex) {
                    break;
                }
            }
        }
    }
}
