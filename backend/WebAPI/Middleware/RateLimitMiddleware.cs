using Core.Utils;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace StripeBilling.API.Middleware
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, RateLimitEntry> _store = new();

        public RateLimitMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";
            if (path.Contains("/swagger") || path.Contains("/health") || path.Contains("/hubs/"))
            {
                await _next(context);
                return;
            }

            string key;
            int limit;

            if (context.Items.ContainsKey("ApiKeyId"))
            {
                key = $"apikey:{context.Items["ApiKeyId"]}";
                limit = context.Items.ContainsKey("RateLimitPerMinute")
                    ? (int)context.Items["RateLimitPerMinute"]
                    : 60;
            }
            else
            {
                key = $"ip:{context.Connection.RemoteIpAddress}";
                limit = 60;
            }

            var now = DateTime.UtcNow;
            var entry = _store.GetOrAdd(key, _ => new RateLimitEntry());

            lock (entry)
            {
                entry.Requests.RemoveAll(t => (now - t).TotalMinutes > 1);
                if (entry.Requests.Count >= limit)
                {
                    context.Response.StatusCode = 429;
                    context.Response.Headers["Retry-After"] = "60";
                    context.Response.ContentType = "application/json";
                    var error = new GatewayResponseWrapper<object>();
                    error.SetError("Rate limit exceeded. Try again later.", 429);
                    context.Response.WriteAsync(JsonConvert.SerializeObject(error)).Wait();
                    return;
                }
                entry.Requests.Add(now);
            }

            await _next(context);
        }

        private class RateLimitEntry
        {
            public List<DateTime> Requests { get; set; } = new();
        }
    }
}
