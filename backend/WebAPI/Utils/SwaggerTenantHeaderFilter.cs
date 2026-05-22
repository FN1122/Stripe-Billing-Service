using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace StripeBilling.API.Utils;

/// <summary>
/// Adds the X-Tenant-Id header parameter to Swagger UI for endpoints that require it.
/// Skips auth, health, and public chat endpoints that don't need tenant context.
/// </summary>
public class SwaggerTenantHeaderFilter : IOperationFilter
{
    private static readonly HashSet<string> SkipControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "Auth", "Health"
        };

    // Public chat endpoints that get tenantId from route/body instead of header
    private static readonly HashSet<string> SkipActions = new(StringComparer.OrdinalIgnoreCase)
        {
            "GetWidgetConfig", "GetEmbedScript"
        };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var controller = context.MethodInfo.DeclaringType?.Name.Replace("Controller", "");
        var action = context.MethodInfo.Name;

        // Skip endpoints that don't use X-Tenant-Id
        if (controller != null && SkipControllers.Contains(controller))
            return;

        if (SkipActions.Contains(action))
            return;

        // Don't add duplicate if already declared via [FromHeader]
        var alreadyHasTenantHeader = operation.Parameters?.Any(p =>
            p.Name.Equals("X-Tenant-Id", StringComparison.OrdinalIgnoreCase)) ?? false;

        if (alreadyHasTenantHeader)
        {
            // Update existing one with better description and default value
            var existing = operation.Parameters.First(p =>
                p.Name.Equals("X-Tenant-Id", StringComparison.OrdinalIgnoreCase));
            existing.Description = "Tenant identifier (GUID)";
            existing.Example = new OpenApiString("8265f78a-6736-487d-af48-ef6c6c590e70");
            return;
        }

        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Tenant-Id",
            In = ParameterLocation.Header,
            Description = "Tenant identifier (GUID)",
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Format = "uuid",
                Default = new OpenApiString("8265f78a-6736-487d-af48-ef6c6c590e70")
            },
            Example = new OpenApiString("8265f78a-6736-487d-af48-ef6c6c590e70")
        });
    }
}

