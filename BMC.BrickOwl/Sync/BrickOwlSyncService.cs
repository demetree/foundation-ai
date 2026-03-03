using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using BMC.BrickOwl.Api;
using Foundation.BMC.Database;


namespace BMC.BrickOwl.Sync
{
    /// <summary>
    /// Server-side middleware for Brick Owl integration.
    ///
    /// Design principles (mirroring BrickSetSyncService):
    ///  1. Non-blocking — API failures don't break BMC, errors are logged
    ///  2. Tenant-scoped — all operations scoped to the caller's tenant
    ///  3. Encryption at rest — API key encrypted via Data Protection API
    ///  4. Rate-limit aware — 600 req/min; 200ms throttle in API client
    ///
    /// Brick Owl uses API key authentication via the "key" query parameter.
    /// Brick Owl is the second-largest LEGO marketplace with cross-platform
    /// ID mapping capabilities (BrickLink ↔ Brick Owl ↔ LEGO design IDs).
    ///
    /// AI-Developed — This file was significantly developed with AI assistance.
    /// </summary>
    public class BrickOwlSyncService
    {
        private readonly BMCContext _context;
        private readonly ILogger<BrickOwlSyncService> _logger;
        private readonly IDataProtector _protector;
        private readonly IMemoryCache _cache;


        // ───────────────────────── Sync direction constants ─────────────────────────

        public const string DIRECTION_PULL = "Pull";
        public const string DIRECTION_PUSH = "Push";
        public const string DIRECTION_BOTH = "Both";


        // ───────────────────────── Cache constants ─────────────────────────

        private const string CACHE_PREFIX = "BrickOwlSync_";
        private static readonly TimeSpan KEY_CACHE_DURATION = TimeSpan.FromHours(8);


        public BrickOwlSyncService(
            BMCContext context,
            ILogger<BrickOwlSyncService> logger,
            IDataProtectionProvider dataProtectionProvider,
            IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _protector = dataProtectionProvider.CreateProtector("BMC.BrickOwl.TokenProtection");
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
                _logger.LogWarning(ex, "Failed to decrypt stored Brick Owl credentials — may need re-authentication");
                return string.Empty;
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  CONNECTION & STATUS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Get the user's Brick Owl link configuration. Returns null if no link exists.
        /// </summary>
        public async Task<BrickOwlUserLink> GetUserLinkAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            return await _context.BrickOwlUserLinks
                .FirstOrDefaultAsync(l => l.tenantGuid == tenantGuid && l.active == true && l.deleted == false, ct);
        }


        /// <summary>
        /// Get the current Brick Owl sync status for display in the UI.
        /// </summary>
        public async Task<BrickOwlSyncStatus> GetSyncStatusAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var link = await GetUserLinkAsync(tenantGuid, ct);

            if (link == null)
            {
                return new BrickOwlSyncStatus { IsConnected = false };
            }

            bool hasCredentials = !string.IsNullOrEmpty(link.encryptedApiKey);

            return new BrickOwlSyncStatus
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
        /// Connect to Brick Owl — validate API key and store encrypted.
        /// </summary>
        public async Task<(bool success, string error)> ConnectAsync(
            Guid tenantGuid, string apiKey,
            string syncDirection = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return (false, "API key is required.");
            }

            // Validate by making a test catalog lookup
            try
            {
                using (var client = new BrickOwlApiClient(apiKey,
                    msg => _logger.LogInformation(msg)))
                {
                    var result = await client.CatalogIdLookupAsync("75192-1", "Set", "set_number");

                    _logger.LogInformation(
                        "Brick Owl API key validation succeeded for tenant {TenantGuid}",
                        tenantGuid);
                }
            }
            catch (BrickOwlApiException ex)
            {
                _logger.LogWarning(ex, "Brick Owl API key validation failed for tenant {TenantGuid}", tenantGuid);
                return (false, $"Brick Owl API key validation failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Brick Owl API key validation for tenant {TenantGuid}", tenantGuid);
                return (false, "Failed to connect to Brick Owl. Please check your API key.");
            }

            // Store encrypted API key
            var link = await GetOrCreateLinkAsync(tenantGuid, ct);

            link.encryptedApiKey = Encrypt(apiKey);
            link.syncEnabled = true;
            link.syncDirection = syncDirection ?? DIRECTION_PULL;
            link.lastSyncError = null;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Brick Owl connected for tenant {TenantGuid}", tenantGuid);

            return (true, null);
        }


        /// <summary>
        /// Disconnect from Brick Owl — clear stored API key.
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

            _logger.LogInformation("Brick Owl disconnected for tenant {TenantGuid}", tenantGuid);
        }


        /// <summary>
        /// Create a BrickOwlApiClient with the decrypted API key.
        /// Returns null if no key is stored or decryption fails.
        /// </summary>
        public async Task<BrickOwlApiClient> CreateClientAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var link = await GetUserLinkAsync(tenantGuid, ct);
            if (link == null || string.IsNullOrEmpty(link.encryptedApiKey))
                return null;

            string apiKey = Decrypt(link.encryptedApiKey);
            if (string.IsNullOrEmpty(apiKey))
                return null;

            return new BrickOwlApiClient(apiKey, msg => _logger.LogDebug(msg));
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  INTERNAL HELPERS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Get or create a BrickOwlUserLink for the given tenant.
        /// </summary>
        private async Task<BrickOwlUserLink> GetOrCreateLinkAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var link = await GetUserLinkAsync(tenantGuid, ct);

            if (link == null)
            {
                link = new BrickOwlUserLink
                {
                    tenantGuid = tenantGuid,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };
                _context.BrickOwlUserLinks.Add(link);
            }

            return link;
        }
    }


    /// <summary>
    /// Status DTO for Brick Owl integration.
    /// </summary>
    public class BrickOwlSyncStatus
    {
        public bool IsConnected { get; set; }
        public string SyncDirection { get; set; }
        public DateTime? LastSyncDate { get; set; }
        public DateTime? LastPullDate { get; set; }
        public DateTime? LastPushDate { get; set; }
        public string LastSyncError { get; set; }
    }
}
