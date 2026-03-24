using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace Foundation.Security.Services
{
    public interface ITenantIdAccessor
    {
        string GetTenantId();
    }

    public class TenantIdAccessor : ITenantIdAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantIdAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public string GetTenantId()
        {
            // Check if HttpContext is available
            var context = _httpContextAccessor.HttpContext;
            if (context == null || context.User == null)
            {
                return null;
            }

            // Fetch the tenant ID from claims
            // This assumes we store tenant ID in a claim during authentication
            return context.User.FindFirst("tenant_id")?.Value ??
                   context.User.FindFirst(ClaimTypes.GroupSid)?.Value;
        }
    }
}