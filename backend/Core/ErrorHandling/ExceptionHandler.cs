using Core.ErrorHandling.Exceptions;
using Core.Utils;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Authentication;

namespace Core.ErrorHandling
{
    public class ExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ExceptionHandler> _logger;

        public ExceptionHandler(ILogger<ExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

            var response = new GatewayResponseWrapper<object>();
            int statusCode;

            switch (exception)
            {
                case AuthenticationException:
                    statusCode = 401;
                    response.SetError("Authentication failed.", 401);
                    break;
                case UnauthorizedAccessException:
                case FeatureNotAvailableException:
                    statusCode = 403;
                    response.SetError(exception.Message, 403);
                    break;
                case RateLimitExceededException rle:
                    statusCode = 429;
                    response.SetError($"Rate limit exceeded. Retry after {rle.RetryAfterSeconds} seconds.", 429);
                    httpContext.Response.Headers["Retry-After"] = rle.RetryAfterSeconds.ToString();
                    break;
                case FluentValidation.ValidationException ve:
                    statusCode = 400;
                    response.SetError("Validation failed.", 400);
                    response.Errors = ve.Errors.Select(e => e.ErrorMessage).ToList();
                    break;
                case ArgumentException:
                    statusCode = 400;
                    response.SetError(exception.Message, 400);
                    break;
                default:
                    statusCode = 500;
                    response.SetError("An unexpected error occurred.", 500);
                    break;
            }

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true;
        }
    }
}
