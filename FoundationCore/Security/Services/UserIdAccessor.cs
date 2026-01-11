using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace Foundation.Security.Services
{
    public interface IUserIdAccessor
    {
        string GetUserId();
    }

    public class UserIdAccessor : IUserIdAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserIdAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public string GetUserId()
        {
            // Check if HttpContext is available
            var context = _httpContextAccessor.HttpContext;
            if (context == null || context.User == null)
            {
                return null;
            }

            // Fetch the 'sub' claim or another claim that represents the user ID
            return context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                   context.User.FindFirst("sub")?.Value; // 'sub' is commonly used in OpenID Connect
        }
    }
}