using Core.Infrastructure;
using Core.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace StripeBilling.API.Middleware
{
    public class HmacAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly string[] HmacPaths = { "/api/v1/payments", "/api/v1/subscriptions", "/api/v1/customers" };

        public HmacAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";

            if (!HmacPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            // Only for API key authenticated requests (not JWT)
            if (!context.Items.ContainsKey("TenantId") || context.Request.Headers.ContainsKey("Authorization"))
            {
                await _next(context);
                return;
            }

            // GET requests don't need HMAC
            if (context.Request.Method == "GET")
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue("X-Signature", out var signature) ||
                !context.Request.Headers.TryGetValue("X-Timestamp", out var timestamp))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var error = new GatewayResponseWrapper<object>();
                error.SetError("HMAC authentication required. Provide X-Signature and X-Timestamp headers.", 401);
                await context.Response.WriteAsync(JsonConvert.SerializeObject(error));
                return;
            }

            // Validate timestamp (within 5 minutes)
            if (!long.TryParse(timestamp.ToString(), out var ts))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var error = new GatewayResponseWrapper<object>();
                error.SetError("Invalid timestamp format.", 401);
                await context.Response.WriteAsync(JsonConvert.SerializeObject(error));
                return;
            }

            var requestTime = DateTimeOffset.FromUnixTimeSeconds(ts).UtcDateTime;
            if (Math.Abs((DateTime.UtcNow - requestTime).TotalMinutes) > 5)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var error = new GatewayResponseWrapper<object>();
                error.SetError("Request timestamp expired. Must be within 5 minutes.", 401);
                await context.Response.WriteAsync(JsonConvert.SerializeObject(error));
                return;
            }

            // Check idempotency key
            var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
            if (context.Request.Headers.TryGetValue("X-Idempotency-Key", out var idempotencyKey))
            {
                if (cache.TryGetValue($"idempotency:{idempotencyKey}", out _))
                {
                    context.Response.StatusCode = 409;
                    context.Response.ContentType = "application/json";
                    var error = new GatewayResponseWrapper<object>();
                    error.SetError("Duplicate request. Idempotency key already used.", 409);
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(error));
                    return;
                }
            }

            // Read request body
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            // Look up tenant's secret key hash
            var tenantId = Guid.Parse(context.Items["TenantId"].ToString()!);
            var db = context.RequestServices.GetRequiredService<BillingDbContext>();
            var tenant = await db.Tenants.FindAsync(tenantId);

            if (tenant == null || string.IsNullOrEmpty(tenant.SecretApiKeyHash))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var error = new GatewayResponseWrapper<object>();
                error.SetError("Tenant configuration error.", 401);
                await context.Response.WriteAsync(JsonConvert.SerializeObject(error));
                return;
            }

            // Compute expected signature: HMAC-SHA256(body|timestamp, secretKeyHash)
            var payload = $"{body}|{timestamp}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(tenant.SecretApiKeyHash));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var expectedSignature = Convert.ToHexString(hash).ToLower();

            if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedSignature),
                Encoding.UTF8.GetBytes(signature.ToString())))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var error = new GatewayResponseWrapper<object>();
                error.SetError("Invalid HMAC signature.", 401);
                await context.Response.WriteAsync(JsonConvert.SerializeObject(error));
                return;
            }

            // Store idempotency key with 24h TTL
            if (!string.IsNullOrEmpty(idempotencyKey))
            {
                cache.Set($"idempotency:{idempotencyKey}", true, TimeSpan.FromHours(24));
            }

            await _next(context);
        }
    }
}
