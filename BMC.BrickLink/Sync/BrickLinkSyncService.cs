using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BMC.BrickLink.Api;
using Foundation.BMC.Database;


namespace BMC.BrickLink.Sync
{
    /// <summary>
    /// Server-side middleware for BrickLink integration.
    ///
    /// Design principles (mirroring BrickSetSyncService):
    ///  1. Non-blocking — API failures don't break BMC, errors are logged
    ///  2. Tenant-scoped — all operations scoped to the caller's tenant
    ///  3. Encryption at rest — OAuth tokens encrypted via Data Protection API
    ///  4. Quota-aware — 500ms throttle baked into the API client
    ///
    /// BrickLink uses OAuth 1.0 authentication:
    ///  - Consumer key/secret are app-level (stored in appsettings.json)
    ///  - Token value/secret are per-user (stored encrypted in BrickLinkUserLink)
    ///
    /// AI-Developed — This file was significantly developed with AI assistance.
    /// </summary>
    public class BrickLinkSyncService
    {
        private readonly BMCContext _context;
        private readonly ILogger<BrickLinkSyncService> _logger;
        private readonly IDataProtector _protector;
        private readonly IMemoryCache _cache;
        private readonly string _consumerKey;
        private readonly string _consumerSecret;


        // ───────────────────────── Sync direction constants ─────────────────────────

        public const string DIRECTION_PULL = "Pull";
        public const string DIRECTION_PUSH = "Push";
        public const string DIRECTION_BOTH = "Both";


        // ───────────────────────── Cache constants ─────────────────────────

        private const string CACHE_PREFIX = "BrickLinkSync_";
        private static readonly TimeSpan TOKEN_CACHE_DURATION = TimeSpan.FromHours(8);


        public BrickLinkSyncService(
            BMCContext context,
            ILogger<BrickLinkSyncService> logger,
            IDataProtectionProvider dataProtectionProvider,
            IMemoryCache cache,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _protector = dataProtectionProvider.CreateProtector("BMC.BrickLink.TokenProtection");
            _cache = cache;
            _consumerKey = configuration.GetValue<string>("DataImport:BrickLink:ConsumerKey") ?? string.Empty;
            _consumerSecret = configuration.GetValue<string>("DataImport:BrickLink:ConsumerSecret") ?? string.Empty;
        }


        // ───────────────────────── Encryption helpers ─────────────────────────

        private string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;
            return _protector.Protect(plainText);
        }

        private string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;
            try
            {
                return _protector.Unprotect(cipherText);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to decrypt stored BrickLink credentials — may need re-authentication");
                return string.Empty;
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  CONNECTION & STATUS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Get the user's BrickLink link configuration. Returns null if no link exists.
        /// </summary>
        public async Task<BrickLinkUserLink> GetUserLinkAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            return await _context.BrickLinkUserLinks
                .FirstOrDefaultAsync(l => l.tenantGuid == tenantGuid && l.active == true && l.deleted == false, ct);
        }


        /// <summary>
        /// Get the current BrickLink sync status for display in the UI.
        /// </summary>
        public async Task<BrickLinkSyncStatus> GetSyncStatusAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var link = await GetUserLinkAsync(tenantGuid, ct);

            if (link == null)
            {
                return new BrickLinkSyncStatus { IsConnected = false };
            }

            bool hasCredentials = !string.IsNullOrEmpty(link.encryptedTokenValue)
                               && !string.IsNullOrEmpty(link.encryptedTokenSecret);

            return new BrickLinkSyncStatus
            {
                IsConnected = hasCredentials && link.syncEnabled,
                SyncDirection = link.syncDirection,
                LastSyncDate = link.lastSyncDate,
                LastPullDate = link.lastPullDate,
                LastPushDate = link.lastPushDate,
                LastSyncError = link.lastSyncError
            };
        }


        /// <summary>
        /// Connect to BrickLink — validate OAuth tokens and store encrypted.
        /// </summary>
        public async Task<(bool success, string error)> ConnectAsync(
            Guid tenantGuid, string tokenValue, string tokenSecret,
            string syncDirection = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_consumerKey) || string.IsNullOrWhiteSpace(_consumerSecret))
            {
                return (false, "BrickLink consumer key/secret not configured in appsettings.json.");
            }

            if (string.IsNullOrWhiteSpace(tokenValue) || string.IsNullOrWhiteSpace(tokenSecret))
            {
                return (false, "Token value and secret are required.");
            }

            // Validate by making a test API call
            try
            {
                using (var client = new BrickLinkApiClient(
                    _consumerKey, _consumerSecret, tokenValue, tokenSecret,
                    msg => _logger.LogInformation(msg)))
                {
                    var colors = await client.GetColorListAsync();

                    _logger.LogInformation(
                        "BrickLink OAuth token validation succeeded for tenant {TenantGuid} — retrieved {Count} colors",
                        tenantGuid, colors?.Count ?? 0);
                }
            }
            catch (BrickLinkApiException ex)
            {
                _logger.LogWarning(ex, "BrickLink token validation failed for tenant {TenantGuid}", tenantGuid);
                return (false, $"BrickLink token validation failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during BrickLink token validation for tenant {TenantGuid}", tenantGuid);
                return (false, "Failed to connect to BrickLink. Please check your tokens.");
            }

            // Store encrypted tokens
            var link = await GetOrCreateLinkAsync(tenantGuid, ct);

            link.encryptedTokenValue = Encrypt(tokenValue);
            link.encryptedTokenSecret = Encrypt(tokenSecret);
            link.syncEnabled = true;
            link.syncDirection = syncDirection ?? DIRECTION_PULL;
            link.lastSyncError = null;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("BrickLink connected for tenant {TenantGuid}", tenantGuid);

            return (true, null);
        }


        /// <summary>
        /// Disconnect from BrickLink — clear stored tokens.
        /// </summary>
        public async Task DisconnectAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var link = await GetUserLinkAsync(tenantGuid, ct);
            if (link == null) return;

            link.encryptedTokenValue = string.Empty;
            link.encryptedTokenSecret = string.Empty;
            link.syncEnabled = false;
            link.lastSyncError = null;

            await _context.SaveChangesAsync(ct);

            _cache.Remove($"{CACHE_PREFIX}{tenantGuid}_tokens");

            _logger.LogInformation("BrickLink disconnected for tenant {TenantGuid}", tenantGuid);
        }


        /// <summary>
        /// Create a BrickLinkApiClient with decrypted tokens.
        /// Returns null if no tokens are stored or decryption fails.
        /// </summary>
        public async Task<BrickLinkApiClient> CreateClientAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_consumerKey) || string.IsNullOrWhiteSpace(_consumerSecret))
                return null;

            var link = await GetUserLinkAsync(tenantGuid, ct);
            if (link == null || string.IsNullOrEmpty(link.encryptedTokenValue))
                return null;

            string tokenValue = Decrypt(link.encryptedTokenValue);
            string tokenSecret = Decrypt(link.encryptedTokenSecret);

            if (string.IsNullOrEmpty(tokenValue) || string.IsNullOrEmpty(tokenSecret))
                return null;

            return new BrickLinkApiClient(
                _consumerKey, _consumerSecret, tokenValue, tokenSecret,
                msg => _logger.LogDebug(msg));
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  INTERNAL HELPERS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Get or create a BrickLinkUserLink for the given tenant.
        /// </summary>
        private async Task<BrickLinkUserLink> GetOrCreateLinkAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var link = await GetUserLinkAsync(tenantGuid, ct);

            if (link == null)
            {
                link = new BrickLinkUserLink
                {
                    tenantGuid = tenantGuid,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };
                _context.BrickLinkUserLinks.Add(link);
            }

            return link;
        }
    }


    /// <summary>
    /// Status DTO for BrickLink integration.
    /// </summary>
    public class BrickLinkSyncStatus
    {
        public bool IsConnected { get; set; }
        public string SyncDirection { get; set; }
        public DateTime? LastSyncDate { get; set; }
        public DateTime? LastPullDate { get; set; }
        public DateTime? LastPushDate { get; set; }
        public string LastSyncError { get; set; }
    }
}
