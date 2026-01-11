using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.Security.OIDC.TokenValidators
{
    public abstract class TokenValidator
    {
        private readonly ILogger<TokenValidator> _logger;

        public TokenValidator(ILogger<TokenValidator> logger)
        {
            _logger = logger;
        }

        public abstract Task<TokenValidationResult> ValidateTokenAsync(string token);

        protected async Task<TokenValidationResult> ValidateOpenIdConnectTokenAsync(
            string jwtToken, string clientId, string issuer, bool validateIssuer)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(jwtToken);

            ArgumentException.ThrowIfNullOrWhiteSpace(clientId);

            ArgumentException.ThrowIfNullOrWhiteSpace(issuer);

            try
            {
                var metadataDocument = GetOpenIdConnectMetadataDocumentUrl(issuer);
                var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(metadataDocument,
                    new OpenIdConnectConfigurationRetriever());

                var openIdConfig = await configurationManager.GetConfigurationAsync(CancellationToken.None);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudiences = new[] { clientId },
                    ValidateIssuer = validateIssuer,
                    ValidIssuer = issuer,
                    ValidateLifetime = true,
                    IssuerSigningKeys = openIdConfig.SigningKeys
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var _ = tokenHandler.ValidateToken(jwtToken, validationParameters, out var validatedToken);

                var claims = ((JwtSecurityToken)validatedToken).Claims;

                return new TokenValidationResult
                {
                    IsValid = true,
                    Subject = claims.FirstOrDefault(c => c.Type == "sub")?.Value,
                    Email = claims.FirstOrDefault(c => c.Type == "email")?.Value,
                    Claims = claims
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Oidc JWT token validation failed");

                return new TokenValidationResult
                {
                    IsValid = false,
                    ErrorDescription = ex.Message
                };
            }
        }

        public static string GetOpenIdConnectMetadataDocumentUrl(string issuerUrl)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(issuerUrl);

            return $"{issuerUrl.TrimEnd('/')}/.well-known/openid-configuration";
        }
    }
}
