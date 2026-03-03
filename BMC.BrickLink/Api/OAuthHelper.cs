using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace BMC.BrickLink.Api
{
    //
    // OAuth 1.0 Helper for BrickLink Store API
    //
    // BrickLink requires OAuth 1.0 signatures on every request. This helper generates
    // the Authorization header value using HMAC-SHA1 signatures.
    //
    // OAuth 1.0 flow for BrickLink:
    //   1. Consumer key + secret are app-level credentials (from appsettings.json)
    //   2. Token key + secret are per-user credentials (stored encrypted in DB)
    //   3. Every request is signed with all four values
    //   4. The signature goes in the Authorization header
    //
    // AI-Developed — This file was significantly developed with AI assistance.
    //

    /// <summary>
    /// Static helper for generating OAuth 1.0 Authorization headers for the BrickLink API.
    /// </summary>
    public static class OAuthHelper
    {
        private const string OAuthVersion = "1.0";
        private const string SignatureMethod = "HMAC-SHA1";
        private static readonly Random _random = new Random();


        /// <summary>
        /// Generates a complete OAuth 1.0 Authorization header for a BrickLink API request.
        /// </summary>
        /// <param name="httpMethod">HTTP method (GET, POST, PUT, DELETE).</param>
        /// <param name="url">Full request URL (without query parameters).</param>
        /// <param name="consumerKey">App-level consumer key.</param>
        /// <param name="consumerSecret">App-level consumer secret.</param>
        /// <param name="tokenValue">Per-user token value.</param>
        /// <param name="tokenSecret">Per-user token secret.</param>
        /// <returns>The value to set on the Authorization header.</returns>
        public static string GenerateAuthorizationHeader(string httpMethod,
                                                         string url,
                                                         string consumerKey,
                                                         string consumerSecret,
                                                         string tokenValue,
                                                         string tokenSecret)
        {
            string nonce = GenerateNonce();
            string timestamp = GenerateTimestamp();

            //
            // Build the parameter string — all OAuth parameters sorted alphabetically
            //
            var oauthParameters = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", consumerKey },
                { "oauth_nonce", nonce },
                { "oauth_signature_method", SignatureMethod },
                { "oauth_timestamp", timestamp },
                { "oauth_token", tokenValue },
                { "oauth_version", OAuthVersion }
            };

            //
            // Build the parameter string by joining all key=value pairs with &
            //
            string parameterString = string.Join("&",
                oauthParameters.Select(kvp =>
                    $"{PercentEncode(kvp.Key)}={PercentEncode(kvp.Value)}"));

            //
            // Build the signature base string:
            //   HTTP_METHOD&url_encoded_base_url&url_encoded_parameters
            //
            string signatureBaseString = $"{httpMethod.ToUpperInvariant()}&{PercentEncode(url)}&{PercentEncode(parameterString)}";

            //
            // Build the signing key from the consumer secret and token secret
            //
            string signingKey = $"{PercentEncode(consumerSecret)}&{PercentEncode(tokenSecret)}";

            //
            // Compute the HMAC-SHA1 signature
            //
            string signature = ComputeHmacSha1(signingKey, signatureBaseString);

            //
            // Build the Authorization header value
            //
            string authorizationHeader = "OAuth " +
                $"oauth_consumer_key=\"{PercentEncode(consumerKey)}\", " +
                $"oauth_nonce=\"{PercentEncode(nonce)}\", " +
                $"oauth_signature=\"{PercentEncode(signature)}\", " +
                $"oauth_signature_method=\"{PercentEncode(SignatureMethod)}\", " +
                $"oauth_timestamp=\"{PercentEncode(timestamp)}\", " +
                $"oauth_token=\"{PercentEncode(tokenValue)}\", " +
                $"oauth_version=\"{PercentEncode(OAuthVersion)}\"";

            return authorizationHeader;
        }


        /// <summary>
        /// Generates a random nonce string for OAuth 1.0.
        /// </summary>
        private static string GenerateNonce()
        {
            byte[] nonceBytes = new byte[16];

            lock (_random)
            {
                _random.NextBytes(nonceBytes);
            }

            return Convert.ToBase64String(nonceBytes)
                .Replace("=", "")
                .Replace("+", "")
                .Replace("/", "");
        }


        /// <summary>
        /// Generates a Unix timestamp string for OAuth 1.0.
        /// </summary>
        private static string GenerateTimestamp()
        {
            long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return unixTimestamp.ToString();
        }


        /// <summary>
        /// Computes an HMAC-SHA1 hash and returns it as a Base64 string.
        /// </summary>
        private static string ComputeHmacSha1(string key, string data)
        {
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            byte[] dataBytes = Encoding.ASCII.GetBytes(data);

            using (HMACSHA1 hmac = new HMACSHA1(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(dataBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }


        /// <summary>
        /// RFC 3986 percent-encoding — required by OAuth 1.0 signature spec.
        /// 
        /// This differs from Uri.EscapeDataString in that it encodes every character
        /// except unreserved characters (A-Z, a-z, 0-9, '-', '.', '_', '~').
        /// </summary>
        private static string PercentEncode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            //
            // Uri.EscapeDataString handles most of it correctly for RFC 3986,
            // but we need to ensure the specific characters are handled.
            //
            string encoded = Uri.EscapeDataString(value);

            //
            // Uri.EscapeDataString doesn't encode '!' or '*' or '\'' or '(' or ')'
            // which RFC 3986 requires for OAuth
            //
            encoded = encoded.Replace("!", "%21");
            encoded = encoded.Replace("*", "%2A");
            encoded = encoded.Replace("'", "%27");
            encoded = encoded.Replace("(", "%28");
            encoded = encoded.Replace(")", "%29");

            return encoded;
        }
    }
}
