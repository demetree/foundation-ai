using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Foundation.Security.Database;
using Foundation.Security.OIDC;
using Foundation.Security.OIDC.TokenValidators;
using Foundation.Security.Services;
using Foundation.Services;


namespace Foundation.Security.Controllers.WebAPI
{
    public class AuthorizationController : Controller
    {
        private readonly IServiceProvider _services;
        private readonly IUserService _userService;
        private readonly ICredentialCacheService _credentialCache;
        private readonly ISessionTrackingService _sessionTracking;

        private readonly TokenValidatorOptions _tokenValidatorOptions;

        public AuthorizationController(IServiceProvider services,
                                       IUserService userService,
                                       ICredentialCacheService credentialCache,
                                       IOptions<TokenValidatorOptions> tokenValidatorOptions,
                                       ISessionTrackingService sessionTracking = null)
        {
            if (tokenValidatorOptions == null)
            {
                throw new ArgumentNullException(nameof(tokenValidatorOptions),
                    "Assertion Token Validators is not configured");
            }

            _services = services;
            _userService = userService;
            _credentialCache = credentialCache ?? throw new ArgumentNullException(nameof(credentialCache));
            _tokenValidatorOptions = tokenValidatorOptions.Value;
            _sessionTracking = sessionTracking;
        }


        /// <summary>
        /// 
        /// This is the handler for OpenID authentication requests.  
        /// 
        /// This handles grant types of password, refresh token, and extension
        /// 
        /// User password verification and such is done through a call to the User service, which calls down to the SecurityLogic class.
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [HttpPost("~/connect/token")]
        [Produces("application/json")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Exchange()
        {
            OpenIddictRequest request = HttpContext.GetOpenIddictServerRequest();

            if (request == null)
            {
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
            }

            //
            // Note that when we get here, the Authentication Handler has already approved the user.
            //
            // Doing some sanity/safety checks on the user state anyway, but there's no need to retest the password (mostly due to the additional audit logging it would create)
            //
            if (request.IsPasswordGrantType() == true)
            {
                string username = request.Username;
                string password = request.Password;

                SecurityLogic.CreateLoginAttemptRecord(request.Username, request.Password, "Request For OIDC Password Grant", "/connect/token", GetClientIP(), Request.Headers.UserAgent);

                //
                // Validate user credentials
                //
                UserValidationResult userValidationResult = await _userService.ValidateUserCredentialsAsync(username, password);

                if (userValidationResult.validationSucceeded == false)
                {
                    return GetForbidResult(userValidationResult.errorMessage);
                }

                //
                // Retrieve the user details
                //
                SecurityUser securityUser = await _userService.GetUserByUserNameAsync(username);
                if (securityUser == null)
                {
                    return GetForbidResult("The user does not exist.");
                }

                if (securityUser.active == false || securityUser.deleted == true)
                {
                    return GetForbidResult("The user cannot login.");
                }

                if (securityUser.canLogin == false)
                {
                    return GetForbidResult("The user account is not permitted to login.");
                }

                //
                // Get the user's roles to add to as claims
                //
                IEnumerable<string> userRoles = await _userService.GetUserRolesByUserNameAsync(securityUser.accountName);

                ClaimsPrincipal principal = CreateClaimsPrincipal(securityUser, userRoles, request.GetScopes(), request.ClientId);

                //
                // Cache the user's credentials for cross-app authentication
                // This allows us to obtain tokens from remote servers when needed
                // Key by objectGuid since that's what's in the sub claim for reliable identity resolution
                //
                _credentialCache.CacheCredentials(securityUser.objectGuid.ToString(), securityUser.accountName, password);

                //
                // Record session for compliance tracking
                //
                int sessionId = await RecordSessionAsync(securityUser, "Password", request.ClientId);
                if (sessionId > 0)
                {
                    AddSessionIdClaim(principal, sessionId);
                }

                //
                // This completes the sign in process
                //
                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
            else if (request.IsRefreshTokenGrantType())
            {
                //
                // Note: Refresh token grants are NOT logged to LoginAttempt table.
                // These are automatic token renewals, not actual login attempts, and logging them
                // was generating millions of unnecessary records (observed ~3M in production).
                // Actual login attempts (password grants, extension grants) are still logged.
                //

                AuthenticateResult result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                string userObjectGuidString = result?.Principal?.GetClaim(Claims.Subject);

                if (string.IsNullOrEmpty(userObjectGuidString) == false && Guid.TryParse(userObjectGuidString, out Guid userObjectGuid) == true)
                {
                    SecurityUser securityUser = await _userService.GetUserByObjectGuidAsync(userObjectGuid);

                    if (securityUser == null)
                    {
                        return GetForbidResult("The refresh token is no longer valid.");
                    }

                    if (securityUser.active == false || securityUser.deleted == true)
                    {
                        return GetForbidResult("The specified user account is disabled.");
                    }

                    if (securityUser.canLogin == false)
                    {
                        return GetForbidResult("The user account is not permitted to login.");
                    }

                    var scopes = request.GetScopes();
                    if (scopes.Length == 0 && result?.Principal != null)
                    {
                        scopes = result.Principal.GetScopes();
                    }

                    // Get the user's roles
                    IEnumerable<string> userRoles = await _userService.GetUserRolesByObjectGuidAsync(userObjectGuid);

                    // Recreate the claims principal in case they changed since the refresh token was issued.
                    ClaimsPrincipal principal = CreateClaimsPrincipal(securityUser, userRoles, scopes, request.ClientId);

                    //
                    // Record session for compliance tracking (refresh token)
                    //
                    int sessionId = await RecordSessionAsync(securityUser, "RefreshToken", request.ClientId);
                    if (sessionId > 0)
                    {
                        AddSessionIdClaim(principal, sessionId);
                    }

                    //
                    // This completes the sign in process
                    // 
                    return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }
                else
                {
                    return GetForbidResult("The specified user account is not found.");
                }
            }
            else if (request.GrantType == Constants.ExtensionGrantType)
            {
                SecurityLogic.CreateLoginAttemptRecord(request.Username, request.Password, "Request For OIDC Extension Grant", "/connect/token", GetClientIP(), Request.Headers.UserAgent);


                var provider = request["provider"].ToString()?.Trim().ToLower();
                var password = request["password"].ToString();

                if (string.IsNullOrEmpty(request.Assertion))
                {
                    return GetForbidResult("The mandatory 'assertion' parameter is missing.", Errors.InvalidRequest);
                }

                if (string.IsNullOrWhiteSpace(provider))
                {
                    return GetForbidResult("The mandatory 'provider' parameter is missing.", Errors.InvalidRequest);
                }

                Type validatorType = _tokenValidatorOptions.GetValidator(provider);

                if (validatorType == null || _services.GetService(validatorType) is not TokenValidator tokenValidator)
                {
                    return GetForbidResult($"Unsupported provider \"{provider}\".", Errors.InvalidRequest);
                }

                var tokenValidationResult = await tokenValidator.ValidateTokenAsync(request.Assertion);

                if (tokenValidationResult.IsValid == false)
                {
                    return GetForbidResult($"The user token failed validation. {tokenValidationResult.ErrorDescription}");
                }

                string providerSubject = tokenValidationResult.Subject!;
                string email = tokenValidationResult.Email!;
                IEnumerable<Claim> claims = tokenValidationResult.Claims!;


                SecurityUser securityUser = await _userService.GetUserByUserNameAsync(providerSubject);

                if (securityUser == null)
                {
                    return GetForbidResult("The refresh token is no longer valid.");
                }

                if (securityUser.active == false || securityUser.deleted == true)
                {
                    return GetForbidResult("The specified user account is disabled.");
                }

                if (securityUser.canLogin == false)
                {
                    return GetForbidResult("The user account is not permitted to login.");
                }


                // Get the user's roles
                IEnumerable<string> userRoles = await _userService.GetUserRolesByUserNameAsync(securityUser.accountName);


                ClaimsPrincipal principal = CreateClaimsPrincipal(securityUser, userRoles, request.GetScopes(), request.ClientId);

                //
                // Record session for compliance tracking (SSO provider)
                //
                int sessionId = await RecordSessionAsync(securityUser, provider ?? "SSO", request.ClientId);
                if (sessionId > 0)
                {
                    AddSessionIdClaim(principal, sessionId);
                }

                //
                // This completes the sign in process
                // 
                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
            else
            {
                SecurityLogic.CreateLoginAttemptRecord(request.Username, request.Password, $"Request For unsupported grant type of {request.GrantType}", "/connect/token", GetClientIP(), Request.Headers.UserAgent);

                throw new InvalidOperationException($"The specified grant type \"{request.GrantType}\" is not supported.");
            }
        }

        private string GetClientIP()
        {
            // Get the client IP address
            string ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();

            // Optionally, handle headers for scenarios like load balancers or proxies
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ipAddress = Request.Headers["X-Forwarded-For"].ToString();
            }

            return ipAddress;
        }


        /// <summary>
        /// Records session metadata for compliance tracking
        /// </summary>
        /// <returns>The session ID, or 0 if recording failed</returns>
        private async Task<int> RecordSessionAsync(SecurityUser user, string loginMethod, string clientApplication)
        {
            if (_sessionTracking == null)
            {
                return 0; // Session tracking not configured
            }

            try
            {
                var sessionInfo = new SessionInfo
                {
                    SecurityUserId = user.id,
                    ObjectGuid = user.objectGuid,
                    SessionStart = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(1), // Will be updated when token is actually issued
                    IpAddress = GetClientIP(),
                    UserAgent = Request.Headers.UserAgent.ToString(),
                    LoginMethod = loginMethod,
                    ClientApplication = clientApplication
                };

                return await _sessionTracking.RecordSessionAsync(sessionInfo);
            }
            catch
            {
                // Don't fail the login if session tracking fails
                return 0;
            }
        }


        /// <summary>
        /// Adds session_id claim to the principal for session validation middleware.
        /// Must set destination to AccessToken so it's included in the JWT token.
        /// </summary>
        private void AddSessionIdClaim(ClaimsPrincipal principal, int sessionId)
        {
            var identity = principal.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var claim = new Claim("session_id", sessionId.ToString());
                claim.SetDestinations(Destinations.AccessToken);
                identity.AddClaim(claim);
            }
        }


        private ForbidResult GetForbidResult(string errorDescription, string error = Errors.InvalidGrant, Dictionary<string, string> errorData = null)
        {
            var properties = new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = errorDescription
            });

            if (errorData != null)
            {
                foreach (var item in errorData)
                {
                    properties.Parameters.Add(item.Key, item.Value);
                }
            }

            return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }


        private ClaimsPrincipal CreateClaimsPrincipal(SecurityUser user, IEnumerable<string> roles, IEnumerable<string> scopes, string clientId = null)
        {
            // Create a list of standard claims for the user
            var claims = new List<Claim>
            {
                new Claim(OpenIddictConstants.Claims.Subject, user.objectGuid.ToString()),                  // User's object guid.  This one is necessary for OpenID.
                new Claim(OpenIddictConstants.Claims.Name, user.accountName),                               // User's account name
                new Claim(OpenIddictConstants.Claims.Email, user.emailAddress ?? string.Empty),             // User's email address
                new Claim(OpenIddictConstants.Claims.PhoneNumber, user.cellPhoneNumber ?? string.Empty),    // User's cell phone number
            };


            //
            // Custom claims
            //
            string fullName = string.Join(", ", new[] { user.firstName, user.middleName, user.lastName }.Where(s => !string.IsNullOrWhiteSpace(s)));

            if (string.IsNullOrEmpty(fullName) == false)
            {
                claims.Add(new Claim(CustomClaims.FullName, fullName));
            }

            if (string.IsNullOrEmpty(user.settings) == false)
            {
                claims.Add(new Claim(CustomClaims.Settings, user.settings));
            }

            if (user.securityTenantId.HasValue == true)
            {
                if (user.securityTenant == null)
                {
                    throw new ApplicationException("Creating of claims principal requires a security tenant object be provided on the security user object when a securityTenantId is present.");
                }

                claims.Add(new Claim(CustomClaims.TenantName, user.securityTenant?.name));
            }


            claims.Add(new Claim(CustomClaims.ReadPermission, user.readPermissionLevel.ToString()));
            claims.Add(new Claim(CustomClaims.WritePermission, user.writePermissionLevel.ToString()));

            //
            // Add claims for each of the user roles
            //
            foreach (var role in roles)
            {
                claims.Add(new Claim(OpenIddictConstants.Claims.Role, role));
            }

            // Add claims for the scopes
            foreach (var scope in scopes)
            {
                claims.Add(new Claim(OpenIddictConstants.Claims.Scope, scope));
            }

            // Create a ClaimsIdentity and associate it with the claims
            //var identity = new ClaimsIdentity(claims, "FoundationAuth"); // The authentication type can be any string
            var identity = new ClaimsIdentity(claims, "Bearer"); // Per ChatGPT suggestion


            // Create the ClaimsPrincipal and add the identity
            var principal = new ClaimsPrincipal(identity);

            // Set additional claims-based configurations
            principal.SetScopes(scopes);

            //
            // Set the resources (audience) for the token
            // This is required when audience validation is enabled via AddAudiences()
            //
            if (!string.IsNullOrEmpty(clientId))
            {
                principal.SetResources(clientId);
            }

            principal.SetDestinations(GetDestinations);

            return principal;
        }


        /// <summary>
        /// 
        /// Route the claims to the appropriate destination token
        /// 
        /// </summary>
        /// <param name="claim"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static IEnumerable<string> GetDestinations(Claim claim)
        {
            if (claim.Subject == null)
            {
                throw new InvalidOperationException("The Claim's Subject is null.");
            }


            switch (claim.Type)
            {
                case Claims.Name:
                    if (claim.Subject.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Email:
                    if (claim.Subject.HasScope(Scopes.Email))
                        yield return Destinations.IdentityToken;

                    yield break;

                case CustomClaims.ReadPermission:
                    if (claim.Subject.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;

                    yield break;

                case CustomClaims.WritePermission:
                    if (claim.Subject.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;

                    yield break;


                case CustomClaims.FullName:
                    if (claim.Subject.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;

                    yield break;

                case CustomClaims.Settings:
                    if (claim.Subject.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;

                    yield break;


                case CustomClaims.TenantName:
                    if (claim.Subject.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Role:
                    yield return Destinations.AccessToken;

                    if (claim.Subject.HasScope(Scopes.Roles))
                        yield return Destinations.IdentityToken;

                    yield break;


                // IdentityOptions.ClaimsIdentity.SecurityStampClaimType
                case "AspNet.Identity.SecurityStamp":
                    // Never include the security stamp in the access and identity tokens, as it's a secret value.
                    yield break;

                default:
                    yield return Destinations.AccessToken;
                    yield break;
            }
        }
    }
}
