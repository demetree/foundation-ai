// ============================================================================
//
// FirewallEngine.cs — Firewall rule evaluation engine.
//
// Evaluates incoming requests against configured rules (IP ranges, country
// codes, path patterns) and returns allow/deny decisions.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

using Foundation.Networking.Skynet.Configuration;

namespace Foundation.Networking.Skynet.Firewall
{
    /// <summary>
    /// Result of a firewall evaluation.
    /// </summary>
    public class FirewallDecision
    {
        /// <summary>
        /// Whether the request is allowed.
        /// </summary>
        public bool Allowed { get; set; } = true;

        /// <summary>
        /// The name of the rule that matched.
        /// </summary>
        public string MatchedRule { get; set; } = string.Empty;

        /// <summary>
        /// The reason for the decision.
        /// </summary>
        public string Reason { get; set; } = string.Empty;
    }


    /// <summary>
    ///
    /// Evaluates requests against configured firewall rules.
    ///
    /// Rules are evaluated in priority order (lowest number = highest priority).
    /// The first matching rule determines the action.  If no rule matches,
    /// the request is allowed (default-allow).
    ///
    /// </summary>
    public class FirewallEngine
    {
        private readonly SkynetConfiguration _config;
        private readonly RateLimiter _rateLimiter;
        private readonly List<FirewallRule> _compiledRules;


        public FirewallEngine(SkynetConfiguration config)
        {
            _config = config;

            //
            // Set up rate limiter
            //
            _rateLimiter = new RateLimiter(
                config.DefaultRateLimit,
                config.RateLimitWindowSeconds);

            //
            // Compile the rules (pre-process IP ranges)
            //
            _compiledRules = new List<FirewallRule>();

            if (config.Rules != null)
            {
                foreach (FirewallRuleConfig ruleConfig in config.Rules.OrderBy(r => r.Priority))
                {
                    if (ruleConfig.Enabled == true)
                    {
                        _compiledRules.Add(new FirewallRule(ruleConfig));
                    }
                }
            }
        }


        /// <summary>
        /// The rate limiter instance (exposed for query/management).
        /// </summary>
        public RateLimiter RateLimiter => _rateLimiter;


        /// <summary>
        /// Number of compiled firewall rules.
        /// </summary>
        public int RuleCount => _compiledRules.Count;


        /// <summary>
        /// Evaluates whether a request should be allowed.
        /// </summary>
        /// <param name="ipAddress">Source IP address.</param>
        /// <param name="path">Request URL path.</param>
        /// <param name="countryCode">Two-letter country code (from GeoIP). Null if unknown.</param>
        public FirewallDecision Evaluate(string ipAddress, string path = "/", string countryCode = null)
        {
            //
            // ── Check firewall rules first ───────────────────────────────
            //
            if (_config.FirewallEnabled == true)
            {
                foreach (FirewallRule rule in _compiledRules)
                {
                    if (rule.Matches(ipAddress, path, countryCode) == true)
                    {
                        bool isAllow = string.Equals(rule.Action, "Allow", StringComparison.OrdinalIgnoreCase);

                        return new FirewallDecision
                        {
                            Allowed = isAllow,
                            MatchedRule = rule.Name,
                            Reason = isAllow
                                ? "Allowed by rule: " + rule.Name
                                : "Denied by rule: " + rule.Name
                        };
                    }
                }
            }

            //
            // ── Check rate limiting ──────────────────────────────────────
            //
            if (_config.RateLimitEnabled == true)
            {
                if (_rateLimiter.IsAllowed(ipAddress) == false)
                {
                    return new FirewallDecision
                    {
                        Allowed = false,
                        MatchedRule = "RateLimit",
                        Reason = "Rate limit exceeded for IP: " + ipAddress
                    };
                }
            }

            //
            // ── Default: allow ───────────────────────────────────────────
            //
            return new FirewallDecision
            {
                Allowed = true,
                MatchedRule = string.Empty,
                Reason = "Default allow"
            };
        }


        // ── Internal ──────────────────────────────────────────────────────


        /// <summary>
        /// Compiled firewall rule with pre-parsed IP ranges.
        /// </summary>
        private class FirewallRule
        {
            private readonly IpBlockList _ipBlockList;
            private readonly HashSet<string> _countryCodes;
            private readonly List<string> _pathPatterns;

            public string Name { get; }
            public string Action { get; }


            public FirewallRule(FirewallRuleConfig config)
            {
                Name = config.Name;
                Action = config.Action;

                //
                // Compile IP ranges
                //
                _ipBlockList = new IpBlockList(config.IpRanges ?? new List<string>());

                //
                // Compile country codes (uppercased)
                //
                _countryCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                if (config.CountryCodes != null)
                {
                    foreach (string code in config.CountryCodes)
                    {
                        _countryCodes.Add(code.ToUpperInvariant());
                    }
                }

                //
                // Path patterns
                //
                _pathPatterns = config.Paths ?? new List<string>();
            }


            /// <summary>
            /// Checks whether this rule matches the given request properties.
            /// </summary>
            public bool Matches(string ipAddress, string path, string countryCode)
            {
                bool ipMatches = true;
                bool countryMatches = true;
                bool pathMatches = true;

                //
                // IP matching (if ranges are defined)
                //
                if (_ipBlockList.Count > 0)
                {
                    ipMatches = _ipBlockList.Contains(ipAddress);
                }

                //
                // Country code matching (if codes are defined)
                //
                if (_countryCodes.Count > 0)
                {
                    if (string.IsNullOrEmpty(countryCode))
                    {
                        countryMatches = false;
                    }
                    else
                    {
                        countryMatches = _countryCodes.Contains(countryCode);
                    }
                }

                //
                // Path matching (if patterns are defined)
                //
                if (_pathPatterns.Count > 0)
                {
                    pathMatches = false;

                    foreach (string pattern in _pathPatterns)
                    {
                        if (PathMatchesPattern(path, pattern) == true)
                        {
                            pathMatches = true;
                            break;
                        }
                    }
                }

                return ipMatches && countryMatches && pathMatches;
            }


            /// <summary>
            /// Simple path wildcard matching.
            ///   "/api/*"  matches "/api/anything"
            ///   "*.php"   matches "/any/path.php"
            ///   "/exact"  matches "/exact" only
            /// </summary>
            private static bool PathMatchesPattern(string path, string pattern)
            {
                if (string.IsNullOrEmpty(pattern))
                {
                    return true;
                }

                if (pattern == "*")
                {
                    return true;
                }

                //
                // Starts-with wildcard: "/api/*"
                //
                if (pattern.EndsWith("*"))
                {
                    string prefix = pattern.Substring(0, pattern.Length - 1);
                    return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
                }

                //
                // Ends-with wildcard: "*.php"
                //
                if (pattern.StartsWith("*"))
                {
                    string suffix = pattern.Substring(1);
                    return path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
                }

                //
                // Exact match
                //
                return string.Equals(path, pattern, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
