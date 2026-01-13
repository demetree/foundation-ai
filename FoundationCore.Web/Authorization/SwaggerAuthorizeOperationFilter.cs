using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace Foundation.Security.Authorization
{
    /// <summary>
    /// Swagger IOperationFilter that adds 401 response + security requirement 
    /// for endpoints (or controllers) decorated with [Authorize]
    /// </summary>
    public class SwaggerAuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check for AuthorizeAttribute on controller OR action
            var hasAuthorize = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<AuthorizeAttribute>()
                .Any() ?? false;

            if (!hasAuthorize)
            {
                return;
            }

            // Add common unauthorized response
            operation.Responses.TryAdd("401", new OpenApiResponse
            {
                Description = "Unauthorized"
            });

            // ───────────────────────────────────────────────────────────────
            // IMPORTANT: New pattern for .NET 10 / Swashbuckle v10 / Microsoft.OpenApi 2.x
            // We no longer set .Reference directly on the scheme
            // ───────────────────────────────────────────────────────────────

            var securitySchemeRef = new OpenApiSecuritySchemeReference("oauth2");

            var requirement = new OpenApiSecurityRequirement
            {
                [securitySchemeRef] = new List<string>() // ← empty list = no scopes (common for JWT/Bearer)
            };

            // Assign the requirement(s) – usually just one in most projects
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                requirement
            };
        }
    }
}