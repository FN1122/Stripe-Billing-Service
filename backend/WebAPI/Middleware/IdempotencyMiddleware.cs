using Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace StripeBilling.API.Middleware
{
    public class IdempotencyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<IdempotencyMiddleware> _logger;

        public IdempotencyMiddleware(RequestDelegate next, ILogger<IdempotencyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method != "POST" && context.Request.Method != "PUT")
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey) || string.IsNullOrEmpty(idempotencyKey))
            {
                await _next(context);
                return;
            }

            var key = idempotencyKey.ToString();
            var dbContext = context.RequestServices.GetRequiredService<BillingDbContext>();

            var existing = await dbContext.Set<IdempotencyKey>().FirstOrDefaultAsync(k => k.Key == key && k.ExpiresAt > DateTime.UtcNow);
            if (existing != null)
            {
                _logger.LogInformation("Idempotency key {Key} found, returning cached response", key);
                context.Response.StatusCode = existing.ResponseStatusCode;
                context.Response.ContentType = "application/json";
                if (!string.IsNullOrEmpty(existing.ResponseBody))
                    await context.Response.WriteAsync(existing.ResponseBody);
                return;
            }

            var originalBody = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            memoryStream.Position = 0;
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

            Guid tenantId = Guid.Empty;
            if (context.Items.TryGetValue("TenantId", out var tenantIdObj) && tenantIdObj != null)
                Guid.TryParse(tenantIdObj.ToString(), out tenantId);

            try
            {
                var idempotencyEntry = new IdempotencyKey
                {
                    Key = key,
                    TenantId = tenantId,
                    HttpMethod = context.Request.Method,
                    Endpoint = context.Request.Path,
                    ResponseStatusCode = context.Response.StatusCode,
                    ResponseBody = responseBody
                };

                dbContext.Set<IdempotencyKey>().Add(idempotencyEntry);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save idempotency key {Key}", key);
            }

            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
        }
    }
}
