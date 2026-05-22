using Core.Infrastructure;
using System.Diagnostics;
using System.Text;

namespace StripeBilling.API.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";
            if (!path.StartsWith("/api/") || path.Contains("/swagger") || path.Contains("/health"))
            {
                await _next(context);
                return;
            }

            var sw = Stopwatch.StartNew();

            // Read request body
            context.Request.EnableBuffering();
            string requestBody = "";
            if (context.Request.ContentLength > 0)
            {
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            // Capture response
            var originalBody = context.Response.Body;
            using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            await _next(context);

            sw.Stop();
            memStream.Position = 0;
            var responseBody = await new StreamReader(memStream).ReadToEndAsync();
            memStream.Position = 0;
            await memStream.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            // Log to database
            try
            {
                var db = context.RequestServices.GetRequiredService<BillingDbContext>();
                var tenantId = context.Items.ContainsKey("TenantId")
                    ? Guid.Parse(context.Items["TenantId"].ToString()!)
                    : Guid.Empty;

                if (tenantId != Guid.Empty)
                {
                    var log = new ApiCallLog
                    {
                        TenantId = tenantId,
                        ApiKeyId = context.Items.ContainsKey("ApiKeyId") ? Guid.Parse(context.Items["ApiKeyId"].ToString()!) : null,
                        ServiceType = DetectServiceType(path),
                        Endpoint = path,
                        Method = context.Request.Method,
                        RequestBody = Truncate(RedactSensitive(requestBody), 5000),
                        ResponseStatusCode = context.Response.StatusCode,
                        ResponseBody = Truncate(responseBody, 5000),
                        DurationMs = (int)sw.ElapsedMilliseconds,
                        Status = context.Response.StatusCode < 400 ? "Success" : "Error",
                        IpAddress = context.Connection.RemoteIpAddress?.ToString()
                    };

                    db.ApiCallLogs.Add(log);
                    await db.SaveChangesAsync();
                }
            }
            catch { /* Don't fail request on logging error */ }
        }

        private static string DetectServiceType(string path) =>
            path switch
            {
                var p when p.Contains("/payments") || p.Contains("/checkout") => "Stripe",
                var p when p.Contains("/subscriptions") => "Stripe",
                var p when p.Contains("/customers") => "Stripe",
                var p when p.Contains("/invoices") => "Stripe",
                var p when p.Contains("/refunds") => "Stripe",
                var p when p.Contains("/plans") => "Stripe",
                var p when p.Contains("/webhooks") => "Webhook",
                var p when p.Contains("/auth") => "Auth",
                _ => "Internal"
            };

        private static string RedactSensitive(string body)
        {
            if (string.IsNullOrEmpty(body)) return body;
            return body.Replace("password", "***").Replace("secret", "***");
        }

        private static string Truncate(string value, int maxLength) =>
            string.IsNullOrEmpty(value) ? value : value.Length <= maxLength ? value : value[..maxLength];
    }
}
