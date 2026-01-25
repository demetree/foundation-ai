//
// Session Validation Middleware
//
// Validates that the user's session is still active (not revoked) on each request.
// This ensures that revoked sessions take immediate effect without waiting for token expiry.
//
using Foundation.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Foundation.Middleware
{
    /// <summary>
    /// Middleware that validates user sessions are still active on each request.
    /// If a session has been revoked, the request is rejected with 401 Unauthorized.
    /// </summary>
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionValidationMiddleware> _logger;

        public SessionValidationMiddleware(RequestDelegate next, ILogger<SessionValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //
            // Only validate if the user is authenticated
            //
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                //
                // Try to get session ID from claims (added during token issuance)
                //
                var sessionIdClaim = context.User.FindFirst("session_id");
                if (sessionIdClaim != null && int.TryParse(sessionIdClaim.Value, out int sessionId))
                {
                    var sessionTracking = context.RequestServices.GetService<ISessionTrackingService>();
                    if (sessionTracking != null)
                    {
                        bool isValid = await sessionTracking.IsSessionValidAsync(sessionId);
                        if (!isValid)
                        {
                            var username = context.User.Identity.Name ?? "Unknown";
                            _logger.LogWarning(
                                "SECURITY: Rejected request from user '{Username}' - session {SessionId} is no longer valid (revoked or expired)",
                                username, sessionId);

                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync("{\"error\":\"Session has been revoked or expired. Please log in again.\"}");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
