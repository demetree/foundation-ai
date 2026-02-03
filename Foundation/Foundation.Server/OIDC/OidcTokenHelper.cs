//
// OIDC Token Helper
//
// Utility for obtaining access tokens for service-to-service communication.
//
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Foundation.Server.OIDC
{
    /// <summary>
    /// Helper class for obtaining OIDC access tokens for service-to-service communication.
    /// </summary>
    public static class OidcTokenHelper
    {
        /// <summary>
        /// Get an access token using the configured service account credentials.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="httpClient">HTTP client to use for the token request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Access token string.</returns>
        public static async Task<string> GetServiceAccountTokenAsync(
            IConfiguration configuration,
            HttpClient httpClient,
            CancellationToken cancellationToken = default)
        {
            var username = configuration["ServiceAccount:Username"];
            var password = configuration["ServiceAccount:Password"];
            var selfUrl = configuration["Alerting:ServiceUrl"] ?? "https://localhost:9101";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException(
                    "ServiceAccount:Username and ServiceAccount:Password must be configured for Alerting integration.");
            }

            // Build token request - call our own /connect/token endpoint
            var tokenUrl = $"{selfUrl.TrimEnd('/')}/connect/token";

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = username,
                ["password"] = password,
                ["scope"] = "openid profile email roles",
                ["client_id"] = "swagger"  // Use swagger client for service account auth
            });

            var response = await httpClient.PostAsync(tokenUrl, content, cancellationToken).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Failed to obtain service account token: {response.StatusCode} - {responseBody}");
            }

            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody);
            
            if (string.IsNullOrEmpty(tokenResponse?.AccessToken))
            {
                throw new InvalidOperationException("Token response did not contain an access_token");
            }

            return tokenResponse.AccessToken;
        }

        private class TokenResponse
        {
            public string AccessToken { get; set; }
            public string TokenType { get; set; }
            public int ExpiresIn { get; set; }
            public string Scope { get; set; }

            // Constructor for JSON deserialization with snake_case
            public TokenResponse() { }
        }
    }
}
