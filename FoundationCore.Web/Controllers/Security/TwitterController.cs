using Foundation.Security.Configuration;
using Foundation.Security.OIDC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Foundation.Controllers.WebAPI.Controllers
{
    [Route("oauth/[controller]")]
    public class TwitterController : ControllerBase
    {
        private readonly HttpClient _client;
        private readonly OAuth1Helper _oauthHelper;
        private readonly TwitterAuthConfig _twitterAuthKeys;

        private const string requestTokenEndpoint = "https://api.twitter.com/oauth/request_token";
        private const string accessTokenEndpoint = "https://api.twitter.com/oauth/access_token";

        public TwitterController(IHttpClientFactory httpClientFactory, OAuth1Helper oauthHelper,
            IOptions<AppSettings> appSettings)
        {
            _client = httpClientFactory.CreateClient(nameof(TwitterController));
            _oauthHelper = oauthHelper;
            _twitterAuthKeys = appSettings.Value.ExternalLogin?.Twitter ??
                throw new InvalidOperationException("Configuration for \"Twitter\" External Login was not found.");
        }

        [HttpPost("request_token")]
        public async Task<IActionResult> RequestToken([FromBody] dynamic oauthValue)
        {
            var endpoint = new Uri(requestTokenEndpoint);
            var tokens = (JsonElement)oauthValue;

            tokens.TryGetProperty("oauth_callback", out var oauthCallback);

            var authorizationHeader = _oauthHelper.GetAuthorizationHeader(endpoint, "POST",
                _twitterAuthKeys.ConsumerAPIKey, _twitterAuthKeys.ConsumerSecret, null, null, oauthCallback.ToString());

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(OAuth1Helper.GetMediaTypeHeader());
            _client.DefaultRequestHeaders.Add("Authorization", authorizationHeader);

            var response = await _client.PostAsync(endpoint, null);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return Ok(responseContent);
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                return BadRequest(errorMsg);
            }
        }

        [HttpPost("access_token")]
        public async Task<IActionResult> AccessToken([FromBody] dynamic oauthValues)
        {
            var endpoint = new Uri(accessTokenEndpoint);
            var tokens = (JsonElement)oauthValues;

            tokens.TryGetProperty("oauth_token", out var oauthToken);
            tokens.TryGetProperty("oauth_token_secret", out var oauthTokenSecret);
            tokens.TryGetProperty("oauth_verifier", out var oauthVerifier);

            var authorizationHeaderParams = _oauthHelper.GetAuthorizationHeader(endpoint, "POST",
                _twitterAuthKeys.ConsumerAPIKey, _twitterAuthKeys.ConsumerSecret, oauthToken.ToString(),
                oauthTokenSecret.ToString(), null);

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(OAuth1Helper.GetMediaTypeHeader());
            _client.DefaultRequestHeaders.Add("Authorization", authorizationHeaderParams);

            var httpContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("oauth_verifier", oauthVerifier.ToString())
            });
            var response = await _client.PostAsync(endpoint, httpContent);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return Ok(responseContent);
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                return BadRequest(errorMsg);
            }
        }
    }
}
