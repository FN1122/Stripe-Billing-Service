namespace StripeBilling.API.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // TenantId may already be set by ApiKeyAuthMiddleware
            if (!context.Items.ContainsKey("TenantId"))
            {
                if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader))
                {
                    context.Items["TenantId"] = tenantIdHeader.ToString();
                }
            }

            await _next(context);
        }
    }
}
