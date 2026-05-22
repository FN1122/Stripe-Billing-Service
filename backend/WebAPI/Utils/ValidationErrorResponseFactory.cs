using Core.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace StripeBilling.API.Utils
{
    public static class ValidationErrorResponseFactory
    {
        public static IActionResult CreateResponse(ActionContext context)
        {
            var errors = context.ModelState
                .Where(ms => ms.Value.Errors.Any())
                .Select(ms => new
                {
                    Field = ms.Key,
                    Errors = ms.Value.Errors.Select(e => e.ErrorMessage).ToList()
                })
                .ToList();

            // Check for specific unauthorized error
            var hasUnauthorizedError = errors.Any(e => e.Field == "Unauthorized");

            // Determine status code
            var statusCode = hasUnauthorizedError
                ? HttpStatusCode.Unauthorized
                : HttpStatusCode.BadRequest;

            var errorMessages = errors.SelectMany(x => x.Errors).ToList();
            var response = new GatewayResponseWrapper<object>();
            response.SetError(string.Join("; ", errorMessages), (int)statusCode);
            response.Errors = errorMessages;

            return new ObjectResult(response)
            {
                StatusCode = (int)statusCode
            };
        }
    }
}
