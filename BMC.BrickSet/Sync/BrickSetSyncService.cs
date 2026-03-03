using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BMC.BrickSet.Api;
using BMC.BrickSet.Api.Models.Responses;
using Foundation.BMC.Database;


namespace BMC.BrickSet.Sync
{
    /// <summary>
    /// Server-side middleware for BrickSet integration.
    ///
    /// Design principles (mirroring RebrickableSyncService):
    ///  1. Trust through transparency — every API call is logged to BrickSetTransaction
    ///  2. Non-blocking — enrichment failures don't break BMC, errors are logged
    ///  3. Tenant-scoped — all operations scoped to the caller's tenant
    ///  4. Encryption at rest — userHash and password encrypted via Data Protection API
    ///  5. Quota-aware — tracks daily API call budget before making calls
    ///
    /// Phase 1 scope: connect/disconnect, set enrichment (pricing, reviews, instructions),
    /// and transaction auditing.  Full push/pull collection sync is a follow-up.
    /// </summary>
    public class BrickSetSyncService
    {
        private readonly BMCContext _context;
        private readonly ILogger<BrickSetSyncService> _logger;
        private readonly IDataProtector _protector;
        private readonly IMemoryCache _cache;
        private readonly string _apiKey;


        // ───────────────────────── Sync direction constants ─────────────────────────

        /// <summary>No BrickSet integration.</summary>
        public const string DIRECTION_NONE = "None";

        /// <summary>Enrich-only — pull set data from BrickSet, no collection sync.</summary>
        public const string DIRECTION_ENRICH_ONLY = "EnrichOnly";

        /// <summary>Full sync — enrichment + bidirectional collection sync (Phase 2).</summary>
        public const string DIRECTION_FULL = "Full";


        // ───────────────────────── Transaction trigger constants ─────────────────────────

        public const string TRIGGER_USER_ACTION = "UserAction";
        public const string TRIGGER_ENRICHMENT = "Enrichment";
        public const string TRIGGER_MANUAL_PULL = "ManualPull";


        // ───────────────────────── Cache constants ─────────────────────────

        private const string CACHE_PREFIX = "BrickSetSync_";
        private static readonly TimeSpan HASH_CACHE_DURATION = TimeSpan.FromHours(8);


        public BrickSetSyncService(
            BMCContext context,
            ILogger<BrickSetSyncService> logger,
            IDataProtectionProvider dataProtectionProvider,
            IMemoryCache cache,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _protector = dataProtectionProvider.CreateProtector("BMC.BrickSet.TokenProtection");
            _cache = cache;
            _apiKey = configuration.GetValue<string>("DataImport:BrickSet:ApiKey") ?? string.Empty;
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
                _logger.LogWarning(ex, "Failed to decrypt stored BrickSet credentials — may need re-authentication");
                return string.Empty;
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  CONNECTION & STATUS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Get the user's BrickSet link configuration. Returns null if no link exists.
        /// </summary>
        public async Task<BrickSetUserLink> GetUserLinkAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            return await _context.BrickSetUserLinks
                .FirstOrDefaultAsync(l => l.tenantGuid == tenantGuid && l.active == true && l.deleted == false, ct);
        }


        /// <summary>
        /// Get the current BrickSet sync status for display in the UI.
        /// </summary>
        public async Task<BrickSetSyncStatus> GetSyncStatusAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var link = await GetUserLinkAsync(tenantGuid, ct);

            if (link == null)
            {
                return new BrickSetSyncStatus { IsConnected = false };
            }

            bool hasCredentials = !string.IsNullOrEmpty(link.encryptedUserHash);

            int totalTx = await _context.BrickSetTransactions
                .CountAsync(t => t.tenantGuid == tenantGuid && t.active == true && t.deleted == false, ct);

            int recentErrors = await _context.BrickSetTransactions
                .CountAsync(t => t.tenantGuid == tenantGuid
                    && t.success == false
                    && t.active == true && t.deleted == false
                    && t.transactionDate > DateTime.UtcNow.AddHours(-24), ct);

            // Get latest API calls remaining from most recent transaction
            int? apiCallsRemaining = await _context.BrickSetTransactions
                .Where(t => t.tenantGuid == tenantGuid && t.active == true && t.deleted == false && t.apiCallsRemaining.HasValue)
                .OrderByDescending(t => t.transactionDate)
                .Select(t => t.apiCallsRemaining)
                .FirstOrDefaultAsync(ct);

            return new BrickSetSyncStatus
            {
                IsConnected = hasCredentials && link.syncEnabled,
                BrickSetUsername = link.brickSetUsername,
                SyncDirection = link.syncDirection,
                LastSyncDate = link.lastSyncDate,
                LastEnrichmentDate = link.lastPullDate,
                LastSyncError = link.lastSyncError,
                TotalTransactions = totalTx,
                RecentErrorCount = recentErrors,
                ApiCallsRemainingToday = apiCallsRemaining
            };
        }


        /// <summary>
        /// Connect to BrickSet — login with username/password, get userHash, encrypt and store.
        /// </summary>
        public async Task<(bool success, string error)> ConnectAsync(
            Guid tenantGuid, string username, string password,
            string syncDirection, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                return (false, "BrickSet API key not configured in appsettings.json.");
            }

            var client = new BrickSetApiClient(_apiKey);

            // Step 1: Validate the API key
            try
            {
                bool keyValid = await client.CheckKeyAsync();
                if (!keyValid)
                {
                    await LogTransactionAsync(tenantGuid, "Pull", "checkKey",
                        "Validate API key", false, "API key invalid", TRIGGER_USER_ACTION, ct: ct);

                    return (false, "Invalid BrickSet API key.");
                }
            }
            catch (BrickSetApiException ex)
            {
                await LogTransactionAsync(tenantGuid, "Pull", "checkKey",
                    "Validate API key", false, ex.Message, TRIGGER_USER_ACTION, ct: ct);

                return (false, $"Could not validate API key: {ex.Message}");
            }

            // Step 2: Login to get userHash
            string userHash;
            try
            {
                userHash = await client.LoginAsync(username, password);
            }
            catch (BrickSetApiException ex)
            {
                await LogTransactionAsync(tenantGuid, "Pull", "login",
                    "Login to BrickSet", false, ex.Message, TRIGGER_USER_ACTION, ct: ct);

                return (false, $"BrickSet login failed: {ex.Message}");
            }

            // Step 3: Store encrypted credentials
            var link = await GetOrCreateLinkAsync(tenantGuid, ct);

            link.brickSetUsername = username;
            link.encryptedUserHash = Encrypt(userHash);
            link.encryptedPassword = Encrypt(password);
            link.syncEnabled = syncDirection != DIRECTION_NONE;
            link.syncDirection = syncDirection;
            link.lastSyncError = null;
            link.userHashStoredDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            await LogTransactionAsync(tenantGuid, "Pull", "login",
                $"Connected as {username}", true, null, TRIGGER_USER_ACTION, ct: ct);

            _logger.LogInformation("BrickSet connected for tenant {TenantGuid} as {Username}",
                tenantGuid, username);

            return (true, null);
        }


        /// <summary>
        /// Disconnect from BrickSet — clear stored credentials.
        /// </summary>
        public async Task DisconnectAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var link = await GetUserLinkAsync(tenantGuid, ct);
            if (link == null) return;

            link.encryptedUserHash = string.Empty;
            link.encryptedPassword = string.Empty;
            link.syncEnabled = false;
            link.syncDirection = DIRECTION_NONE;
            link.lastSyncError = null;
            link.userHashStoredDate = null;

            await _context.SaveChangesAsync(ct);

            // Clear cached userHash
            _cache.Remove($"{CACHE_PREFIX}{tenantGuid}_hash");

            _logger.LogInformation("BrickSet disconnected for tenant {TenantGuid}", tenantGuid);
        }


        /// <summary>
        /// Validate the stored userHash by calling checkUserHash.
        /// If invalid, attempt to re-authenticate using stored encrypted password.
        /// </summary>
        public async Task<(bool valid, string error)> ValidateAndRefreshHashAsync(
            Guid tenantGuid, CancellationToken ct = default)
        {
            var link = await GetUserLinkAsync(tenantGuid, ct);
            if (link == null || string.IsNullOrEmpty(link.encryptedUserHash))
            {
                return (false, "No stored credentials — please reconnect.");
            }

            string userHash = Decrypt(link.encryptedUserHash);
            if (string.IsNullOrEmpty(userHash))
            {
                return (false, "Could not decrypt stored credentials — please reconnect.");
            }

            var client = new BrickSetApiClient(_apiKey);

            // Check if current hash is still valid
            try
            {
                bool valid = await client.CheckUserHashAsync(userHash);
                if (valid)
                {
                    await LogTransactionAsync(tenantGuid, "Pull", "checkUserHash",
                        "Hash validation — valid", true, null, TRIGGER_USER_ACTION, ct: ct);
                    return (true, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "BrickSet hash check failed for tenant {TenantGuid}", tenantGuid);
            }

            // Hash is invalid — try to re-authenticate
            string password = Decrypt(link.encryptedPassword);
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(link.brickSetUsername))
            {
                link.lastSyncError = "UserHash expired and cannot re-authenticate — no stored credentials.";
                await _context.SaveChangesAsync(ct);

                return (false, "UserHash expired — please reconnect with your BrickSet credentials.");
            }

            try
            {
                string newHash = await client.LoginAsync(link.brickSetUsername, password);
                link.encryptedUserHash = Encrypt(newHash);
                link.userHashStoredDate = DateTime.UtcNow;
                link.lastSyncError = null;
                await _context.SaveChangesAsync(ct);

                await LogTransactionAsync(tenantGuid, "Pull", "login",
                    $"Re-authenticated as {link.brickSetUsername}", true, null, TRIGGER_USER_ACTION, ct: ct);

                _logger.LogInformation("BrickSet hash refreshed for tenant {TenantGuid}", tenantGuid);
                return (true, null);
            }
            catch (BrickSetApiException ex)
            {
                link.lastSyncError = $"Re-authentication failed: {ex.Message}";
                await _context.SaveChangesAsync(ct);

                await LogTransactionAsync(tenantGuid, "Pull", "login",
                    "Re-authentication failed", false, ex.Message, TRIGGER_USER_ACTION, ct: ct);

                return (false, $"Re-authentication failed: {ex.Message}");
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  SET ENRICHMENT — Phase 1 Core
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Enrich a single LegoSet with BrickSet data — pricing, subtheme, availability,
        /// ratings, instructions URL, and minifig count.
        ///
        /// Returns true if the set was found and enriched.
        /// </summary>
        public async Task<bool> EnrichSetAsync(
            Guid tenantGuid, string setNumber, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogWarning("BrickSet API key not configured — skipping enrichment");
                return false;
            }

            var client = new BrickSetApiClient(_apiKey, msg => _logger.LogDebug(msg));

            BrickSetSet bsSet;
            try
            {
                bsSet = await client.GetSetByNumberAsync(setNumber);
            }
            catch (BrickSetApiException ex)
            {
                await LogTransactionAsync(tenantGuid, "Pull", "getSets",
                    $"Enrich set {setNumber}", false, ex.Message, TRIGGER_ENRICHMENT, ct: ct);
                return false;
            }

            if (bsSet == null)
            {
                await LogTransactionAsync(tenantGuid, "Pull", "getSets",
                    $"Enrich set {setNumber} — not found on BrickSet", true, null, TRIGGER_ENRICHMENT,
                    recordCount: 0, ct: ct);
                return false;
            }

            // Find the matching LegoSet in BMC
            var legoSet = await _context.LegoSets
                .FirstOrDefaultAsync(s => s.setNumber == setNumber && s.active == true && s.deleted == false, ct);

            if (legoSet == null)
            {
                _logger.LogWarning("LegoSet {SetNumber} not found in BMC database", setNumber);
                return false;
            }

            // Enrich the fields
            legoSet.brickSetId = bsSet.SetID;
            legoSet.brickSetUrl = bsSet.BricksetURL;
            legoSet.subtheme = bsSet.Subtheme;
            legoSet.availability = bsSet.Availability;
            legoSet.minifigCount = bsSet.Minifigs;
            legoSet.brickSetRating = (float?)bsSet.Rating;
            legoSet.brickSetReviewCount = bsSet.ReviewCount;

            // Pricing
            if (bsSet.LEGOCom != null)
            {
                if (bsSet.LEGOCom.US != null)
                    legoSet.retailPriceUS = bsSet.LEGOCom.US.RetailPrice;
                if (bsSet.LEGOCom.UK != null)
                    legoSet.retailPriceUK = bsSet.LEGOCom.UK.RetailPrice;
                if (bsSet.LEGOCom.CA != null)
                    legoSet.retailPriceCA = bsSet.LEGOCom.CA.RetailPrice;
                if (bsSet.LEGOCom.DE != null)
                    legoSet.retailPriceEU = bsSet.LEGOCom.DE.RetailPrice;
            }

            // Instructions URL — fetch from getInstructions2 if available
            if (bsSet.InstructionsCount > 0)
            {
                try
                {
                    var instructions = await client.GetInstructions2Async(setNumber);
                    if (instructions != null && instructions.Count > 0)
                    {
                        legoSet.instructionsUrl = instructions[0].URL;
                    }
                }
                catch (BrickSetApiException ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch instructions for set {SetNumber}", setNumber);
                }
            }

            legoSet.brickSetLastEnrichedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            await LogTransactionAsync(tenantGuid, "Pull", "getSets",
                $"Enriched set {setNumber} — ID:{bsSet.SetID}",
                true, null, TRIGGER_ENRICHMENT, recordCount: 1,
                apiCallsRemaining: client.TodayApiCallCount, ct: ct);

            _logger.LogInformation("Enriched LegoSet {SetNumber} with BrickSet data (ID={SetId})",
                setNumber, bsSet.SetID);

            return true;
        }


        /// <summary>
        /// Fetch and cache BrickSet reviews for a set.
        /// Upserts reviews into the BrickSetSetReview table.
        /// </summary>
        public async Task<int> EnrichSetReviewsAsync(
            Guid tenantGuid, int legoSetId, int brickSetId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_apiKey)) return 0;

            var client = new BrickSetApiClient(_apiKey, msg => _logger.LogDebug(msg));

            List<BrickSetReview> reviews;
            try
            {
                reviews = await client.GetReviewsAsync(brickSetId);
            }
            catch (BrickSetApiException ex)
            {
                await LogTransactionAsync(tenantGuid, "Pull", "getReviews",
                    $"Fetch reviews for BrickSet ID {brickSetId}", false, ex.Message, TRIGGER_ENRICHMENT, ct: ct);
                return 0;
            }

            if (reviews == null || reviews.Count == 0)
            {
                await LogTransactionAsync(tenantGuid, "Pull", "getReviews",
                    $"No reviews for BrickSet ID {brickSetId}", true, null, TRIGGER_ENRICHMENT,
                    recordCount: 0, ct: ct);
                return 0;
            }

            // Get existing reviews for this set to avoid duplicates
            var existingAuthors = await _context.BrickSetSetReviews
                .Where(r => r.legoSetId == legoSetId && r.active == true && r.deleted == false)
                .Select(r => r.reviewAuthor)
                .ToListAsync(ct);

            var existingAuthorSet = new HashSet<string>(existingAuthors, StringComparer.OrdinalIgnoreCase);
            int added = 0;

            foreach (var review in reviews)
            {
                // Skip duplicates by author
                if (existingAuthorSet.Contains(review.Author ?? string.Empty))
                    continue;

                DateTime? reviewDate = null;
                if (DateTime.TryParse(review.DatePosted, out DateTime parsed))
                    reviewDate = parsed;

                var entity = new BrickSetSetReview
                {
                    legoSetId = legoSetId,
                    reviewAuthor = review.Author,
                    reviewDate = reviewDate,
                    reviewTitle = review.Title,
                    reviewBody = review.ReviewText,
                    overallRating = review.OverallRating,
                    buildingExperienceRating = review.BuildingExperience,
                    valueForMoneyRating = review.ValueForMoney,
                    partsRating = review.Parts,
                    playabilityRating = review.Playability,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.BrickSetSetReviews.Add(entity);
                existingAuthorSet.Add(review.Author ?? string.Empty);
                added++;
            }

            if (added > 0)
            {
                await _context.SaveChangesAsync(ct);
            }

            await LogTransactionAsync(tenantGuid, "Pull", "getReviews",
                $"Cached {added} reviews for BrickSet ID {brickSetId}",
                true, null, TRIGGER_ENRICHMENT, recordCount: added, ct: ct);

            _logger.LogInformation("Cached {Count} reviews for LegoSet {LegoSetId} from BrickSet",
                added, legoSetId);

            return added;
        }


        /// <summary>
        /// Get the current daily API quota status.
        /// </summary>
        public async Task<int?> GetQuotaRemainingAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_apiKey)) return null;

            var client = new BrickSetApiClient(_apiKey, msg => _logger.LogDebug(msg));

            try
            {
                var stats = await client.GetKeyUsageStatsAsync();

                await LogTransactionAsync(tenantGuid, "Pull", "getKeyUsageStats",
                    $"Quota check — today: {client.TodayApiCallCount ?? 0} calls",
                    true, null, TRIGGER_USER_ACTION,
                    apiCallsRemaining: client.TodayApiCallCount, ct: ct);

                return client.TodayApiCallCount;
            }
            catch (BrickSetApiException ex)
            {
                _logger.LogWarning(ex, "Failed to check BrickSet API quota");
                return null;
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  INTERNAL HELPERS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Get or create a BrickSetUserLink for the given tenant.
        /// </summary>
        private async Task<BrickSetUserLink> GetOrCreateLinkAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var link = await GetUserLinkAsync(tenantGuid, ct);

            if (link == null)
            {
                link = new BrickSetUserLink
                {
                    tenantGuid = tenantGuid,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };
                _context.BrickSetUserLinks.Add(link);
            }

            return link;
        }


        /// <summary>
        /// Write a transaction record for audit and rate limit tracking.
        /// Mirrors the Rebrickable LogTransactionAsync pattern.
        /// </summary>
        private async Task LogTransactionAsync(
            Guid tenantGuid, string direction, string methodName,
            string requestSummary, bool success, string errorMessage,
            string triggeredBy, int? recordCount = null,
            int? apiCallsRemaining = null, CancellationToken ct = default)
        {
            try
            {
                var transaction = new BrickSetTransaction
                {
                    tenantGuid = tenantGuid,
                    transactionDate = DateTime.UtcNow,
                    direction = direction,
                    methodName = methodName,
                    requestSummary = requestSummary,
                    success = success,
                    errorMessage = errorMessage,
                    triggeredBy = triggeredBy,
                    recordCount = recordCount,
                    apiCallsRemaining = apiCallsRemaining,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.BrickSetTransactions.Add(transaction);
                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                // Never let transaction logging break actual operations
                _logger.LogError(ex, "Failed to log BrickSet transaction");
            }
        }
    }
}
