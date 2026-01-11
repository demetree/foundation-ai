using Foundation.Security.Database;
using System.Collections.Generic;

namespace Foundation.Security.OIDC
{
    public class UserResolutionResult
    {
        public UserResolutionResult(SecurityUser user)
        {
            User = user;
        }

        public UserResolutionResult(string errorMessage, Dictionary<string, string> errorData = null)
        {
            ErrorMessage = errorMessage;
            ErrorData = errorData;
        }

        public bool IsError => !string.IsNullOrWhiteSpace(ErrorMessage) || ErrorData?.Count > 0;

        public SecurityUser User { get; set; }

        public string ErrorMessage { get; set; }

        public Dictionary<string, string> ErrorData { get; set; }
    }
}
