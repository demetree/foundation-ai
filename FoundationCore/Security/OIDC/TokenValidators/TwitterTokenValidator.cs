using Foundation.Security.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Foundation.Security.OIDC.TokenValidators
{
    public class TwitterTokenValidator : TokenValidator
    {
        private const string userInfoEndpoint =
            "https://api.twitter.com/1.1/account/verify_credentials.json?include_email=true&include_entities=false&skip_status=true";

        private readonly HttpClient _client;
        private readonly OAuth1Helper _oauthHelper;
        private readonly TwitterAuthConfig _providerConfig;

        public TwitterTokenValidator(HttpClient client, OAuth1Helper oauthHelper, IOptions<AppSettings> options,
            ILogger<TwitterTokenValidator> logger) : base(logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _oauthHelper = oauthHelper ?? throw new ArgumentNullException(nameof(oauthHelper));
            _providerConfig = options.Value.ExternalLogin?.Twitter ??
                throw new InvalidOperationException("Configuration for \"Twitter\" External Login was not found.");
        }

        public override async Task<TokenValidationResult> ValidateTokenAsync(string token)
        {
            var endpoint = new Uri(userInfoEndpoint);
            var tokenValues = OAuth1Helper.GetResponseValues(token);

            tokenValues.TryGetValue("oauth_token", out var oauthToken);
            tokenValues.TryGetValue("oauth_token_secret", out var oauthTokenSecret);

            var authorizationHeader = _oauthHelper.GetAuthorizationHeader(endpoint, "GET",
                _providerConfig.ConsumerAPIKey, _providerConfig.ConsumerSecret, oauthToken, oauthTokenSecret, null);

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(OAuth1Helper.GetMediaTypeHeader());
            _client.DefaultRequestHeaders.Add("Authorization", authorizationHeader);

            var response = await _client.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                var userInfo = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

                var claims = userInfo.RootElement.EnumerateObject()
                    .Select(item => new Claim(item.Name, item.Value.ToString()));

                return new TokenValidationResult
                {
                    IsValid = true,
                    Subject = claims.FirstOrDefault(c => c.Type == "id")?.Value,
                    Email = claims.FirstOrDefault(c => c.Type == "email")?.Value,
                    Claims = claims
                };
            }

            return new TokenValidationResult
            {
                IsValid = false,
                ErrorDescription = response.ReasonPhrase
            };
        }
    }
}
