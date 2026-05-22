using Core.Infrastructure;
using Core.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace StripeBilling.API.Middleware
{
    public class ApiKeyAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly HashSet<string> SkipPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/health", "/swagger", "/api/v1/auth", "/api/v1/webhooks/stripe"
        };

        public ApiKeyAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";

            if (SkipPaths.Any(sp => path.StartsWith(sp, StringComparison.OrdinalIgnoreCase))
                || path.Contains("/swagger")
                || path.Contains("/hubs/"))
            {
                await _next(context);
                return;
            }

            // If JWT Authorization header present, skip API key auth
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeader))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var error = new GatewayResponseWrapper<object>();
                error.SetError("API key is required. Provide X-Api-Key header.", 401);
                await context.Response.WriteAsync(JsonConvert.SerializeObject(error));
                return;
            }

            var apiKey = apiKeyHeader.ToString();
            using var sha256 = SHA256.Create();
            var keyHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey)));

            var db = context.RequestServices.GetRequiredService<BillingDbContext>();
            var key = await db.ApiKeys.Include(k => k.Tenant).FirstOrDefaultAsync(k => k.KeyHash == keyHash);

            if (key == null)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var error = new GatewayResponseWrapper<object>();
                error.SetError("Invalid API key.", 401);
                await context.Response.WriteAsync(JsonConvert.SerializeObject(error));
                return;
            }

            if (!key.IsActive || !key.Tenant.IsActive)
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                var error = new GatewayResponseWrapper<object>();
                error.SetError("API key or tenant is deactivated.", 403);
                await context.Response.WriteAsync(JsonConvert.SerializeObject(error));
                return;
            }

            if (key.ExpiresAt.HasValue && key.ExpiresAt.Value < DateTime.UtcNow)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var error = new GatewayResponseWrapper<object>();
                error.SetError("API key has expired.", 401);
                await context.Response.WriteAsync(JsonConvert.SerializeObject(error));
                return;
            }

            // Set context items
            context.Items["TenantId"] = key.TenantId.ToString();
            context.Items["ApiKeyId"] = key.Id.ToString();
            context.Items["RateLimitPerMinute"] = key.RateLimitPerMinute;

            if (!string.IsNullOrEmpty(key.Permissions))
            {
                context.Items["ApiKeyPermissions"] = JsonConvert.DeserializeObject<List<string>>(key.Permissions);
            }

            // Update usage
            key.LastUsedAt = DateTime.UtcNow;
            key.TotalRequests++;
            await db.SaveChangesAsync();

            await _next(context);
        }
    }
}
