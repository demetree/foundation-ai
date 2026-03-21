// ============================================================================
//
// FirewallEngineTests.cs — Unit tests for FirewallEngine.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Collections.Generic;
using Xunit;

using Foundation.Networking.Skynet.Configuration;
using Foundation.Networking.Skynet.Firewall;

namespace Foundation.Networking.Skynet.Tests.Firewall
{
    public class FirewallEngineTests
    {
        // ── Default Allow ────────────────────────────────────────────────


        [Fact]
        public void Evaluate_NoRules_AllowsByDefault()
        {
            SkynetConfiguration config = new SkynetConfiguration
            {
                FirewallEnabled = true,
                RateLimitEnabled = false,
                Rules = new List<FirewallRuleConfig>()
            };

            FirewallEngine engine = new FirewallEngine(config);

            FirewallDecision decision = engine.Evaluate("192.168.1.1");

            Assert.True(decision.Allowed);
        }


        // ── IP-Based Rules ───────────────────────────────────────────────


        [Fact]
        public void Evaluate_DenyIp_BlocksMatchingIp()
        {
            SkynetConfiguration config = new SkynetConfiguration
            {
                FirewallEnabled = true,
                RateLimitEnabled = false,
                Rules = new List<FirewallRuleConfig>
                {
                    new FirewallRuleConfig
                    {
                        Name = "Block Bad IP",
                        Action = "Deny",
                        IpRanges = new List<string> { "10.0.0.100" },
                        Enabled = true,
                        Priority = 1
                    }
                }
            };

            FirewallEngine engine = new FirewallEngine(config);

            Assert.False(engine.Evaluate("10.0.0.100").Allowed);
            Assert.True(engine.Evaluate("10.0.0.101").Allowed);
        }


        [Fact]
        public void Evaluate_DenyCidr_BlocksSubnet()
        {
            SkynetConfiguration config = new SkynetConfiguration
            {
                FirewallEnabled = true,
                RateLimitEnabled = false,
                Rules = new List<FirewallRuleConfig>
                {
                    new FirewallRuleConfig
                    {
                        Name = "Block Subnet",
                        Action = "Deny",
                        IpRanges = new List<string> { "172.16.0.0/16" },
                        Enabled = true,
                        Priority = 1
                    }
                }
            };

            FirewallEngine engine = new FirewallEngine(config);

            Assert.False(engine.Evaluate("172.16.0.1").Allowed);
            Assert.False(engine.Evaluate("172.16.255.100").Allowed);
            Assert.True(engine.Evaluate("172.17.0.1").Allowed);
        }


        [Fact]
        public void Evaluate_AllowIp_AllowsExplicitly()
        {
            SkynetConfiguration config = new SkynetConfiguration
            {
                FirewallEnabled = true,
                RateLimitEnabled = false,
                Rules = new List<FirewallRuleConfig>
                {
                    new FirewallRuleConfig
                    {
                        Name = "Allow Internal",
                        Action = "Allow",
                        IpRanges = new List<string> { "192.168.1.0/24" },
                        Enabled = true,
                        Priority = 1
                    }
                }
            };

            FirewallEngine engine = new FirewallEngine(config);

            FirewallDecision decision = engine.Evaluate("192.168.1.50");

            Assert.True(decision.Allowed);
            Assert.Equal("Allow Internal", decision.MatchedRule);
        }


        // ── Country-Based Rules ──────────────────────────────────────────


        [Fact]
        public void Evaluate_DenyCountry_BlocksMatchingCountry()
        {
            SkynetConfiguration config = new SkynetConfiguration
            {
                FirewallEnabled = true,
                RateLimitEnabled = false,
                Rules = new List<FirewallRuleConfig>
                {
                    new FirewallRuleConfig
                    {
                        Name = "Block Region",
                        Action = "Deny",
                        CountryCodes = new List<string> { "CN", "RU" },
                        Enabled = true,
                        Priority = 1
                    }
                }
            };

            FirewallEngine engine = new FirewallEngine(config);

            Assert.False(engine.Evaluate("10.0.0.1", "/", "CN").Allowed);
            Assert.False(engine.Evaluate("10.0.0.1", "/", "RU").Allowed);
            Assert.True(engine.Evaluate("10.0.0.1", "/", "US").Allowed);
        }


        [Fact]
        public void Evaluate_CountryRule_NullCountryCode_DoesNotMatch()
        {
            SkynetConfiguration config = new SkynetConfiguration
            {
                FirewallEnabled = true,
                RateLimitEnabled = false,
                Rules = new List<FirewallRuleConfig>
                {
                    new FirewallRuleConfig
                    {
                        Name = "Block Region",
                        Action = "Deny",
                        CountryCodes = new List<string> { "CN" },
                        Enabled = true,
                        Priority = 1
                    }
                }
            };

            FirewallEngine engine = new FirewallEngine(config);

            Assert.True(engine.Evaluate("10.0.0.1", "/", null).Allowed);
        }


        // ── Path-Based Rules ─────────────────────────────────────────────


        [Fact]
        public void Evaluate_DenyPath_BlocksMatchingPath()
        {
            SkynetConfiguration config = new SkynetConfiguration
            {
                FirewallEnabled = true,
                RateLimitEnabled = false,
                Rules = new List<FirewallRuleConfig>
                {
                    new FirewallRuleConfig
                    {
                        Name = "Block Admin",
                        Action = "Deny",
                        Paths = new List<string> { "/admin/*" },
                        Enabled = true,
                        Priority = 1
                    }
                }
            };

            FirewallEngine engine = new FirewallEngine(config);

            Assert.False(engine.Evaluate("10.0.0.1", "/admin/panel").Allowed);
            Assert.False(engine.Evaluate("10.0.0.1", "/admin/settings").Allowed);
            Assert.True(engine.Evaluate("10.0.0.1", "/api/data").Allowed);
        }


        [Fact]
        public void Evaluate_DenyFileExtension_BlocksPattern()
        {
            SkynetConfiguration config = new SkynetConfiguration
            {
                FirewallEnabled = true,
                RateLimitEnabled = false,
                Rules = new List<FirewallRuleConfig>
                {
                    new FirewallRuleConfig
                    {
                        Name = "Block PHP",
                        Action = "Deny",
                        Paths = new List<string> { "*.php" },
                        Enabled = true,
                        Priority = 1
                    }
                }
            };

            FirewallEngine engine = new FirewallEngine(config);

            Assert.False(engine.Evaluate("10.0.0.1", "/backdoor.php").Allowed);
            Assert.True(engine.Evaluate("10.0.0.1", "/index.html").Allowed);
        }


        // ── Priority ─────────────────────────────────────────────────────


        [Fact]
        public void Evaluate_HigherPriorityRuleWins()
        {
            SkynetConfiguration config = new SkynetConfiguration
            {
                FirewallEnabled = true,
                RateLimitEnabled = false,
                Rules = new List<FirewallRuleConfig>
                {
                    new FirewallRuleConfig
                    {
                        Name = "Deny All 10.x",
                        Action = "Deny",
                        IpRanges = new List<string> { "10.0.0.0/8" },
                        Enabled = true,
                        Priority = 10
                    },
                    new FirewallRuleConfig
                    {
                        Name = "Allow Trusted",
                        Action = "Allow",
                        IpRanges = new List<string> { "10.0.0.0/8" },
                        Enabled = true,
                        Priority = 1
                    }
                }
            };

            FirewallEngine engine = new FirewallEngine(config);

            //
            // Priority 1 (Allow) should win over Priority 10 (Deny)
            //
            FirewallDecision decision = engine.Evaluate("10.0.0.50");

            Assert.True(decision.Allowed);
            Assert.Equal("Allow Trusted", decision.MatchedRule);
        }


        // ── Disabled Rules ───────────────────────────────────────────────


        [Fact]
        public void Evaluate_DisabledRule_IsIgnored()
        {
            SkynetConfiguration config = new SkynetConfiguration
            {
                FirewallEnabled = true,
                RateLimitEnabled = false,
                Rules = new List<FirewallRuleConfig>
                {
                    new FirewallRuleConfig
                    {
                        Name = "Disabled Block",
                        Action = "Deny",
                        IpRanges = new List<string> { "10.0.0.0/8" },
                        Enabled = false,
                        Priority = 1
                    }
                }
            };

            FirewallEngine engine = new FirewallEngine(config);

            Assert.True(engine.Evaluate("10.0.0.1").Allowed);
            Assert.Equal(0, engine.RuleCount);
        }


        // ── Rate Limiting ────────────────────────────────────────────────


        [Fact]
        public void Evaluate_RateLimitExceeded_Denies()
        {
            SkynetConfiguration config = new SkynetConfiguration
            {
                FirewallEnabled = false,
                RateLimitEnabled = true,
                DefaultRateLimit = 3,
                RateLimitWindowSeconds = 60
            };

            FirewallEngine engine = new FirewallEngine(config);

            Assert.True(engine.Evaluate("192.168.1.1").Allowed);
            Assert.True(engine.Evaluate("192.168.1.1").Allowed);
            Assert.True(engine.Evaluate("192.168.1.1").Allowed);

            FirewallDecision denied = engine.Evaluate("192.168.1.1");

            Assert.False(denied.Allowed);
            Assert.Equal("RateLimit", denied.MatchedRule);
        }


        // ── Firewall Disabled ────────────────────────────────────────────


        [Fact]
        public void Evaluate_FirewallDisabled_SkipsRules()
        {
            SkynetConfiguration config = new SkynetConfiguration
            {
                FirewallEnabled = false,
                RateLimitEnabled = false,
                Rules = new List<FirewallRuleConfig>
                {
                    new FirewallRuleConfig
                    {
                        Name = "Block All",
                        Action = "Deny",
                        IpRanges = new List<string> { "0.0.0.0/0" },
                        Enabled = true,
                        Priority = 1
                    }
                }
            };

            FirewallEngine engine = new FirewallEngine(config);

            Assert.True(engine.Evaluate("10.0.0.1").Allowed);
        }
    }
}
