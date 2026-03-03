using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using BMC.BrickEconomy.Api;
using Foundation.BMC.Database;


namespace BMC.BrickEconomy.Sync
{
    /// <summary>
    /// Server-side middleware for BrickEconomy integration.
    ///
    /// Design principles (mirroring BrickSetSyncService):
    ///  1. Non-blocking — API failures don't break BMC, errors are logged
    ///  2. Tenant-scoped — all operations scoped to the caller's tenant
    ///  3. Encryption at rest — API key encrypted via Data Protection API
    ///  4. Quota-aware — tracks daily quota usage (100 req/day limit)
    ///
    /// BrickEconomy uses API key authentication (x-apikey header).
    /// Requires a Premium membership for API access.
    ///
    /// AI-Developed — This file was significantly developed with AI assistance.
    /// </summary>
    public class BrickEconomySyncService
    {
        private readonly BMCContext _context;
        private readonly ILogger<BrickEconomySyncService> _logger;
        private readonly IDataProtector _protector;
        private readonly IMemoryCache _cache;


        /// <summary>Maximum daily API quota for BrickEconomy.</summary>
        public const int DAILY_QUOTA_LIMIT = 100;


        // ───────────────────────── Cache constants ─────────────────────────

        private const string CACHE_PREFIX = "BrickEconomySync_";
        private static readonly TimeSpan KEY_CACHE_DURATION = TimeSpan.FromHours(8);


        public BrickEconomySyncService(
            BMCContext context,
            ILogger<BrickEconomySyncService> logger,
            IDataProtectionProvider dataProtectionProvider,
            IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _protector = dataProtectionProvider.CreateProtector("BMC.BrickEconomy.TokenProtection");
            _cache = cache;
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
                _logger.LogWarning(ex, "Failed to decrypt stored BrickEconomy credentials — may need re-authentication");
                return string.Empty;
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  CONNECTION & STATUS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Get the user's BrickEconomy link configuration. Returns null if no link exists.
        /// </summary>
        public async Task<BrickEconomyUserLink> GetUserLinkAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            return await _context.BrickEconomyUserLinks
                .FirstOrDefaultAsync(l => l.tenantGuid == tenantGuid && l.active == true && l.deleted == false, ct);
        }


        /// <summary>
        /// Get the current BrickEconomy sync status for display in the UI.
        /// </summary>
        public async Task<BrickEconomySyncStatus> GetSyncStatusAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var link = await GetUserLinkAsync(tenantGuid, ct);

            if (link == null)
            {
                return new BrickEconomySyncStatus { IsConnected = false };
            }

            bool hasCredentials = !string.IsNullOrEmpty(link.encryptedApiKey);

            // Check if quota needs reset (resets at 00:00 UTC daily)
            int quotaUsed = link.dailyQuotaUsed ?? 0;
            if (link.quotaResetDate.HasValue && link.quotaResetDate.Value.Date < DateTime.UtcNow.Date)
            {
                quotaUsed = 0;
            }

            return new BrickEconomySyncStatus
            {
                IsConnected = hasCredentials && link.syncEnabled,
                LastSyncDate = link.lastSyncDate,
                LastSyncError = link.lastSyncError,
                DailyQuotaUsed = quotaUsed,
                DailyQuotaLimit = DAILY_QUOTA_LIMIT,
                DailyQuotaRemaining = DAILY_QUOTA_LIMIT - quotaUsed
            };
        }


        /// <summary>
        /// Connect to BrickEconomy — validate API key and store encrypted.
        /// </summary>
        public async Task<(bool success, string error)> ConnectAsync(
            Guid tenantGuid, string apiKey, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return (false, "API key is required.");
            }

            // Validate by making a test API call
            try
            {
                using (var client = new BrickEconomyApiClient(apiKey,
                    msg => _logger.LogInformation(msg)))
                {
                    // Look up a well-known set (UCS Millennium Falcon)
                    var set = await client.GetSetAsync("75192-1");

                    _logger.LogInformation(
                        "BrickEconomy API key validation succeeded for tenant {TenantGuid}",
                        tenantGuid);
                }
            }
            catch (BrickEconomyApiException ex)
            {
                _logger.LogWarning(ex, "BrickEconomy API key validation failed for tenant {TenantGuid}", tenantGuid);
                return (false, $"BrickEconomy API key validation failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during BrickEconomy API key validation for tenant {TenantGuid}", tenantGuid);
                return (false, "Failed to connect to BrickEconomy. Please check your API key.");
            }

            // Store encrypted API key
            var link = await GetOrCreateLinkAsync(tenantGuid, ct);

            link.encryptedApiKey = Encrypt(apiKey);
            link.syncEnabled = true;
            link.lastSyncError = null;
            link.dailyQuotaUsed = 1; // We just used 1 call for validation
            link.quotaResetDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("BrickEconomy connected for tenant {TenantGuid}", tenantGuid);

            return (true, null);
        }


        /// <summary>
        /// Disconnect from BrickEconomy — clear stored API key.
        /// </summary>
        public async Task DisconnectAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var link = await GetUserLinkAsync(tenantGuid, ct);
            if (link == null) return;

            link.encryptedApiKey = string.Empty;
            link.syncEnabled = false;
            link.lastSyncError = null;

            await _context.SaveChangesAsync(ct);

            _cache.Remove($"{CACHE_PREFIX}{tenantGuid}_key");

            _logger.LogInformation("BrickEconomy disconnected for tenant {TenantGuid}", tenantGuid);
        }


        /// <summary>
        /// Create a BrickEconomyApiClient with the decrypted API key.
        /// Returns null if no key is stored or decryption fails.
        /// Also increments the daily quota counter.
        /// </summary>
        public async Task<BrickEconomyApiClient> CreateClientAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var link = await GetUserLinkAsync(tenantGuid, ct);
            if (link == null || string.IsNullOrEmpty(link.encryptedApiKey))
                return null;

            // Check quota
            int quotaUsed = link.dailyQuotaUsed ?? 0;
            if (link.quotaResetDate.HasValue && link.quotaResetDate.Value.Date < DateTime.UtcNow.Date)
            {
                quotaUsed = 0;
                link.dailyQuotaUsed = 0;
                link.quotaResetDate = DateTime.UtcNow;
            }

            if (quotaUsed >= DAILY_QUOTA_LIMIT)
            {
                _logger.LogWarning("BrickEconomy daily quota exceeded for tenant {TenantGuid} ({Used}/{Limit})",
                    tenantGuid, quotaUsed, DAILY_QUOTA_LIMIT);
                return null;
            }

            string apiKey = Decrypt(link.encryptedApiKey);
            if (string.IsNullOrEmpty(apiKey))
                return null;

            return new BrickEconomyApiClient(apiKey, msg => _logger.LogDebug(msg));
        }


        /// <summary>
        /// Increment the daily quota counter after a successful API call.
        /// </summary>
        public async Task IncrementQuotaAsync(Guid tenantGuid, int calls = 1, CancellationToken ct = default)
        {
            var link = await GetUserLinkAsync(tenantGuid, ct);
            if (link == null) return;

            // Reset counter if new day
            if (link.quotaResetDate.HasValue && link.quotaResetDate.Value.Date < DateTime.UtcNow.Date)
            {
                link.dailyQuotaUsed = 0;
                link.quotaResetDate = DateTime.UtcNow;
            }

            link.dailyQuotaUsed = (link.dailyQuotaUsed ?? 0) + calls;
            await _context.SaveChangesAsync(ct);
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  INTERNAL HELPERS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Get or create a BrickEconomyUserLink for the given tenant.
        /// </summary>
        private async Task<BrickEconomyUserLink> GetOrCreateLinkAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var link = await GetUserLinkAsync(tenantGuid, ct);

            if (link == null)
            {
                link = new BrickEconomyUserLink
                {
                    tenantGuid = tenantGuid,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };
                _context.BrickEconomyUserLinks.Add(link);
            }

            return link;
        }
    }


    /// <summary>
    /// Status DTO for BrickEconomy integration.
    /// </summary>
    public class BrickEconomySyncStatus
    {
        public bool IsConnected { get; set; }
        public DateTime? LastSyncDate { get; set; }
        public string LastSyncError { get; set; }
        public int DailyQuotaUsed { get; set; }
        public int DailyQuotaLimit { get; set; }
        public int DailyQuotaRemaining { get; set; }
    }
}
