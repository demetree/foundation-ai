using System.Collections.Generic;
using System.Security.Claims;

namespace Foundation.Security.OIDC
{
    public class TokenValidationResult
    {
        public bool IsValid { get; set; }

        public string Subject { get; set; }

        public string Email { get; set; }

        public string ErrorDescription { get; set; }

        public IEnumerable<Claim> Claims { get; set; }
    }
}
