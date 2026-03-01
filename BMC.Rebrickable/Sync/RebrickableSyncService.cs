using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using BMC.Rebrickable.Api;
using BMC.Rebrickable.Api.Models.Responses;
using Foundation.BMC.Database;


namespace BMC.Rebrickable.Sync
{
    /// <summary>
    /// Server-side middleware for bidirectional Rebrickable sync.
    ///
    /// Design principles:
    ///  1. Trust through transparency — every API call is logged to RebrickableTransaction
    ///  2. Mode-aware — checks user's integration mode before firing API calls
    ///  3. Non-blocking — push failures don't block BMC operations; errors are logged
    ///  4. Tenant-scoped — all operations are scoped to the caller's tenant
    ///  5. Encryption at rest — API key and user_token are encrypted via Data Protection API
    ///  6. Three auth modes — LoginOnce (encrypted DB), TokenOnly (no password), SessionOnly (memory only)
    /// </summary>
    public class RebrickableSyncService
    {
        private readonly BMCContext _context;
        private readonly ILogger<RebrickableSyncService> _logger;
        private readonly IDataProtector _protector;
        private readonly IMemoryCache _cache;


        // ───────────────────────── Integration mode constants ─────────────────────────

        /// <summary>No Rebrickable integration — BMC-only mode.</summary>
        public const string MODE_NONE = "None";

        /// <summary>Real-time push on write + periodic pull.</summary>
        public const string MODE_REALTIME = "RealTime";

        /// <summary>Push to Rebrickable on BMC write, no automatic pull.</summary>
        public const string MODE_PUSH_ONLY = "PushOnly";

        /// <summary>Import-only — manual pull, no push.</summary>
        public const string MODE_IMPORT_ONLY = "ImportOnly";


        // ───────────────────────── Auth mode constants ─────────────────────────

        /// <summary>Login once — API key + user_token encrypted in database.</summary>
        public const string AUTH_API_TOKEN = "ApiToken";

        /// <summary>Token only — user pastes API key + user_token directly, no password ever sent.</summary>
        public const string AUTH_TOKEN_ONLY = "TokenOnly";

        /// <summary>Session only — credentials stored in memory only, cleared on server restart.</summary>
        public const string AUTH_SESSION_ONLY = "SessionOnly";


        // ───────────────────────── Transaction trigger constants ─────────────────────────

        public const string TRIGGER_USER_ACTION = "UserAction";
        public const string TRIGGER_PERIODIC_SYNC = "PeriodicSync";
        public const string TRIGGER_MANUAL_PULL = "ManualPull";
        public const string TRIGGER_SESSION_LOGIN = "SessionLogin";


        // ───────────────────────── Session cache constants ─────────────────────────

        private const string SESSION_CACHE_PREFIX = "RebrickableSession_";
        private static readonly TimeSpan SESSION_CACHE_DURATION = TimeSpan.FromHours(8);


        public RebrickableSyncService(
            BMCContext context,
            ILogger<RebrickableSyncService> logger,
            IDataProtectionProvider dataProtectionProvider,
            IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _protector = dataProtectionProvider.CreateProtector("BMC.Rebrickable.TokenProtection");
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
                _logger.LogWarning(ex, "Failed to decrypt stored token — may need re-authentication");
                return string.Empty;
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  CONNECTION & STATUS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Get the user's Rebrickable link configuration. Returns null if no link exists.
        /// </summary>
        public async Task<RebrickableUserLink> GetUserLinkAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            return await _context.RebrickableUserLinks
                .FirstOrDefaultAsync(l => l.tenantGuid == tenantGuid && l.active == true && l.deleted == false, ct);
        }


        /// <summary>
        /// Get the current sync status for display in the UI.
        /// </summary>
        public async Task<SyncStatus> GetSyncStatusAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var link = await GetUserLinkAsync(tenantGuid, ct);

            if (link == null)
            {
                return new SyncStatus { IsConnected = false };
            }

            // Count total transactions and recent errors
            int totalTx = await _context.RebrickableTransactions
                .CountAsync(t => t.tenantGuid == tenantGuid && t.active == true && t.deleted == false, ct);

            int recentErrors = await _context.RebrickableTransactions
                .CountAsync(t => t.tenantGuid == tenantGuid
                    && t.success == false
                    && t.active == true && t.deleted == false
                    && t.transactionDate > DateTime.UtcNow.AddHours(-24), ct);

            return new SyncStatus
            {
                IsConnected = !string.IsNullOrEmpty(link.authMode),
                AuthMode = link.authMode,
                IntegrationMode = link.syncDirectionFlags,
                RebrickableUsername = link.rebrickableUsername,
                LastPullDate = link.lastPullDate,
                LastPushDate = link.lastPushDate,
                LastSyncError = link.lastSyncError,
                PullIntervalMinutes = link.pullIntervalMinutes,
                TotalTransactions = totalTx,
                RecentErrorCount = recentErrors
            };
        }


        /// <summary>
        /// Connect via Login Once mode — API key + Rebrickable username/password.
        /// Obtains user_token, encrypts both tokens, stores in database.
        /// Password is used once and never stored.
        /// </summary>
        public async Task<(bool success, string error)> ConnectWithTokenAsync(
            Guid tenantGuid, string apiToken, string username, string password,
            string integrationMode, CancellationToken ct = default)
        {
            var client = new RebrickableApiClient(apiToken);

            // Step 1: Validate the API key with a lightweight catalog call
            try
            {
                await client.GetColorsAsync(1, 1);
            }
            catch (RebrickableApiException ex)
            {
                await LogTransactionAsync(tenantGuid, "Push", "GET",
                    "api/v3/lego/colors/",
                    "Validate API key",
                    (int)ex.StatusCode, ex.Message, false, ex.Message, TRIGGER_USER_ACTION, ct);

                return (false, $"Invalid API key: {ex.Message}");
            }

            // Step 2: Obtain a user_token via username/password
            RebrickableUserToken tokenResult;
            try
            {
                tokenResult = await client.GetUserTokenAsync(username, password);
            }
            catch (RebrickableApiException ex)
            {
                await LogTransactionAsync(tenantGuid, "Push", "POST",
                    "api/v3/users/_token/",
                    "Obtain user token",
                    (int)ex.StatusCode, ex.Message, false, ex.Message, TRIGGER_USER_ACTION, ct);

                return (false, $"Invalid Rebrickable credentials: {ex.Message}");
            }

            string userToken = tokenResult.UserToken;

            // Step 3: Validate the user_token by getting the user profile
            RebrickableUserProfile profile;
            try
            {
                profile = await client.GetUserProfileAsync(userToken);
            }
            catch (RebrickableApiException ex)
            {
                await LogTransactionAsync(tenantGuid, "Push", "GET",
                    $"api/v3/users/{{token}}/profile/",
                    "Validate user profile",
                    (int)ex.StatusCode, ex.Message, false, ex.Message, TRIGGER_USER_ACTION, ct);

                return (false, $"Could not retrieve user profile: {ex.Message}");
            }

            // Create or update the user link — encrypt tokens before storing
            var link = await GetOrCreateLinkAsync(tenantGuid, ct);

            link.rebrickableUsername = profile.Username;
            link.encryptedApiToken = Encrypt(apiToken);
            link.encryptedPassword = Encrypt(userToken);
            link.authMode = AUTH_API_TOKEN;
            link.syncEnabled = integrationMode != MODE_NONE;
            link.syncDirectionFlags = integrationMode;
            link.lastSyncError = null;
            link.tokenStoredDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            await LogTransactionAsync(tenantGuid, "Push", "POST",
                "api/v3/users/_token/",
                $"Connected as {profile.Username}",
                200, null, true, null, TRIGGER_USER_ACTION, ct);

            _logger.LogInformation("Rebrickable connected (LoginOnce) for tenant {TenantGuid} as {Username}",
                tenantGuid, profile.Username);

            return (true, null);
        }


        /// <summary>
        /// Connect via Token Only mode — user pastes API key + user_token directly.
        /// No username or password ever reaches the server.
        /// Validates the user_token by calling the profile endpoint.
        /// </summary>
        public async Task<(bool success, string error)> ConnectWithDirectTokenAsync(
            Guid tenantGuid, string apiToken, string userToken,
            string integrationMode, CancellationToken ct = default)
        {
            var client = new RebrickableApiClient(apiToken);

            // Validate the API key
            try
            {
                await client.GetColorsAsync(1, 1);
            }
            catch (RebrickableApiException ex)
            {
                await LogTransactionAsync(tenantGuid, "Push", "GET",
                    "api/v3/lego/colors/",
                    "Validate API key (TokenOnly)",
                    (int)ex.StatusCode, ex.Message, false, ex.Message, TRIGGER_USER_ACTION, ct);

                return (false, $"Invalid API key: {ex.Message}");
            }

            // Validate the user_token by getting the profile
            RebrickableUserProfile profile;
            try
            {
                profile = await client.GetUserProfileAsync(userToken);
            }
            catch (RebrickableApiException ex)
            {
                await LogTransactionAsync(tenantGuid, "Push", "GET",
                    $"api/v3/users/{{token}}/profile/",
                    "Validate user token (TokenOnly)",
                    (int)ex.StatusCode, ex.Message, false, ex.Message, TRIGGER_USER_ACTION, ct);

                return (false, $"Invalid user token: {ex.Message}");
            }

            // Encrypt and store
            var link = await GetOrCreateLinkAsync(tenantGuid, ct);

            link.rebrickableUsername = profile.Username;
            link.encryptedApiToken = Encrypt(apiToken);
            link.encryptedPassword = Encrypt(userToken);
            link.authMode = AUTH_TOKEN_ONLY;
            link.syncEnabled = integrationMode != MODE_NONE;
            link.syncDirectionFlags = integrationMode;
            link.lastSyncError = null;
            link.tokenStoredDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            await LogTransactionAsync(tenantGuid, "Push", "GET",
                $"api/v3/users/{{token}}/profile/",
                $"Connected as {profile.Username} (TokenOnly)",
                200, null, true, null, TRIGGER_USER_ACTION, ct);

            _logger.LogInformation("Rebrickable connected (TokenOnly) for tenant {TenantGuid} as {Username}",
                tenantGuid, profile.Username);

            return (true, null);
        }


        /// <summary>
        /// Connect via Session Only mode — same login flow as LoginOnce, but
        /// tokens are stored in memory cache only, never persisted to the database.
        /// The database link records the connection state and username, but no secrets.
        /// </summary>
        public async Task<(bool success, string error)> ConnectSessionOnlyAsync(
            Guid tenantGuid, string apiToken, string username, string password,
            string integrationMode, CancellationToken ct = default)
        {
            var client = new RebrickableApiClient(apiToken);

            // Validate API key
            try
            {
                await client.GetColorsAsync(1, 1);
            }
            catch (RebrickableApiException ex)
            {
                await LogTransactionAsync(tenantGuid, "Push", "GET",
                    "api/v3/lego/colors/",
                    "Validate API key (SessionOnly)",
                    (int)ex.StatusCode, ex.Message, false, ex.Message, TRIGGER_USER_ACTION, ct);

                return (false, $"Invalid API key: {ex.Message}");
            }

            // Obtain user_token
            RebrickableUserToken tokenResult;
            try
            {
                tokenResult = await client.GetUserTokenAsync(username, password);
            }
            catch (RebrickableApiException ex)
            {
                await LogTransactionAsync(tenantGuid, "Push", "POST",
                    "api/v3/users/_token/",
                    "Obtain user token (SessionOnly)",
                    (int)ex.StatusCode, ex.Message, false, ex.Message, TRIGGER_USER_ACTION, ct);

                return (false, $"Invalid Rebrickable credentials: {ex.Message}");
            }

            string userToken = tokenResult.UserToken;

            // Validate user_token
            RebrickableUserProfile profile;
            try
            {
                profile = await client.GetUserProfileAsync(userToken);
            }
            catch (RebrickableApiException ex)
            {
                await LogTransactionAsync(tenantGuid, "Push", "GET",
                    $"api/v3/users/{{token}}/profile/",
                    "Validate user profile (SessionOnly)",
                    (int)ex.StatusCode, ex.Message, false, ex.Message, TRIGGER_USER_ACTION, ct);

                return (false, $"Could not retrieve user profile: {ex.Message}");
            }

            // Store tokens in memory cache ONLY — never in the database
            string cacheKey = $"{SESSION_CACHE_PREFIX}{tenantGuid}";
            var sessionData = new SessionOnlyCredentials
            {
                ApiKey = apiToken,
                UserToken = userToken
            };
            _cache.Set(cacheKey, sessionData, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = SESSION_CACHE_DURATION,
                Priority = CacheItemPriority.High
            });

            // Update the link record — store auth mode and username but NO tokens
            var link = await GetOrCreateLinkAsync(tenantGuid, ct);

            link.rebrickableUsername = profile.Username;
            link.encryptedApiToken = string.Empty;   // No tokens in DB for SessionOnly
            link.encryptedPassword = string.Empty;
            link.authMode = AUTH_SESSION_ONLY;
            link.syncEnabled = integrationMode != MODE_NONE;
            link.syncDirectionFlags = integrationMode;
            link.lastSyncError = null;
            link.tokenStoredDate = null;  // Not persisted

            await _context.SaveChangesAsync(ct);

            await LogTransactionAsync(tenantGuid, "Push", "POST",
                "api/v3/users/_token/",
                $"Connected as {profile.Username} (SessionOnly — not persisted)",
                200, null, true, null, TRIGGER_USER_ACTION, ct);

            _logger.LogInformation("Rebrickable connected (SessionOnly) for tenant {TenantGuid} as {Username}",
                tenantGuid, profile.Username);

            return (true, null);
        }


        /// <summary>
        /// Re-authenticate — refresh the stored token without losing sync settings.
        /// Works for LoginOnce and TokenOnly modes.
        /// </summary>
        public async Task<(bool success, string error)> ReauthenticateAsync(
            Guid tenantGuid, string apiToken, string username, string password,
            string userToken, string authMode, CancellationToken ct = default)
        {
            var link = await GetUserLinkAsync(tenantGuid, ct);
            if (link == null)
                return (false, "No existing Rebrickable connection found.");

            // Preserve current integration mode
            string currentMode = link.syncDirectionFlags ?? MODE_NONE;

            if (authMode == AUTH_TOKEN_ONLY)
            {
                return await ConnectWithDirectTokenAsync(tenantGuid, apiToken, userToken, currentMode, ct);
            }
            else if (authMode == AUTH_SESSION_ONLY)
            {
                return await ConnectSessionOnlyAsync(tenantGuid, apiToken, username, password, currentMode, ct);
            }
            else
            {
                return await ConnectWithTokenAsync(tenantGuid, apiToken, username, password, currentMode, ct);
            }
        }


        /// <summary>
        /// Validate the currently stored token by calling the Rebrickable profile endpoint.
        /// Returns whether the token is still valid.
        /// </summary>
        public async Task<(bool valid, string error)> ValidateStoredTokenAsync(
            Guid tenantGuid, CancellationToken ct = default)
        {
            var (client, token) = await GetClientAndTokenAsync(tenantGuid);
            if (client == null || string.IsNullOrEmpty(token))
            {
                return (false, "No stored credentials — please reconnect.");
            }

            try
            {
                var profile = await client.GetUserProfileAsync(token);

                await LogTransactionAsync(tenantGuid, "Pull", "GET",
                    $"api/v3/users/{{token}}/profile/",
                    $"Token health check — valid ({profile.Username})",
                    200, null, true, null, TRIGGER_USER_ACTION, ct);

                return (true, null);
            }
            catch (RebrickableApiException ex)
            {
                // Mark the link as having an error
                var link = await GetUserLinkAsync(tenantGuid, ct);
                if (link != null)
                {
                    link.lastSyncError = $"Token health check failed: {ex.Message}";
                    await _context.SaveChangesAsync(ct);
                }

                await LogTransactionAsync(tenantGuid, "Pull", "GET",
                    $"api/v3/users/{{token}}/profile/",
                    "Token health check — FAILED",
                    (int)ex.StatusCode, ex.Message, false, ex.Message, TRIGGER_USER_ACTION, ct);

                return (false, $"Stored token is no longer valid: {ex.Message}");
            }
        }


        /// <summary>
        /// Disconnect from Rebrickable — clear stored credentials and session cache.
        /// </summary>
        public async Task DisconnectAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var link = await GetUserLinkAsync(tenantGuid, ct);
            if (link == null) return;

            link.encryptedApiToken = string.Empty;
            link.encryptedPassword = string.Empty;
            link.authMode = string.Empty;
            link.syncEnabled = false;
            link.syncDirectionFlags = MODE_NONE;
            link.lastSyncError = null;
            link.tokenStoredDate = null;

            await _context.SaveChangesAsync(ct);

            // Also clear any session-only cache
            _cache.Remove($"{SESSION_CACHE_PREFIX}{tenantGuid}");

            _logger.LogInformation("Rebrickable disconnected for tenant {TenantGuid}", tenantGuid);
        }


        /// <summary>
        /// Helper class to hold session-only credentials in memory cache.
        /// </summary>
        private class SessionOnlyCredentials
        {
            public string ApiKey { get; set; }
            public string UserToken { get; set; }
        }


        /// <summary>
        /// Get or create a RebrickableUserLink for the given tenant.
        /// </summary>
        private async Task<RebrickableUserLink> GetOrCreateLinkAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var link = await GetUserLinkAsync(tenantGuid, ct);

            if (link == null)
            {
                link = new RebrickableUserLink
                {
                    tenantGuid = tenantGuid,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };
                _context.RebrickableUserLinks.Add(link);
            }

            return link;
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  PUSH METHODS — called after BMC writes
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Push a set addition to Rebrickable.
        /// </summary>
        public async Task PushSetAddedAsync(Guid tenantGuid, string setNum, int quantity, bool includeSpares = true)
        {
            if (!await ShouldPushAsync(tenantGuid)) return;

            var (client, token) = await GetClientAndTokenAsync(tenantGuid);
            if (client == null) return;

            await ExecuteWithAuditAsync(tenantGuid, "Push", "POST",
                $"api/v3/users/_token/sets/",
                $"Add set {setNum} x{quantity}",
                TRIGGER_USER_ACTION,
                async () =>
                {
                    await client.AddUserSetAsync(token, setNum, quantity, includeSpares);
                    return true;
                });
        }


        /// <summary>
        /// Push a set removal to Rebrickable.
        /// </summary>
        public async Task PushSetRemovedAsync(Guid tenantGuid, string setNum)
        {
            if (!await ShouldPushAsync(tenantGuid)) return;

            var (client, token) = await GetClientAndTokenAsync(tenantGuid);
            if (client == null) return;

            await ExecuteWithAuditAsync(tenantGuid, "Push", "DELETE",
                $"api/v3/users/_token/sets/{setNum}/",
                $"Remove set {setNum}",
                TRIGGER_USER_ACTION,
                async () =>
                {
                    await client.DeleteUserSetAsync(token, setNum);
                    return true;
                });
        }


        /// <summary>
        /// Push a set list creation to Rebrickable.
        /// Returns the Rebrickable list ID for storage in BMC, or null on failure.
        /// </summary>
        public async Task<int?> PushSetListCreatedAsync(Guid tenantGuid, string name, bool isBuildable)
        {
            if (!await ShouldPushAsync(tenantGuid)) return null;

            var (client, token) = await GetClientAndTokenAsync(tenantGuid);
            if (client == null) return null;

            return await ExecuteWithAuditAsync(tenantGuid, "Push", "POST",
                $"api/v3/users/_token/setlists/",
                $"Create set list '{name}'",
                TRIGGER_USER_ACTION,
                async () =>
                {
                    var result = await client.CreateUserSetListAsync(token, name, isBuildable);
                    return result.Id;
                });
        }


        /// <summary>
        /// Push a set list deletion to Rebrickable.
        /// </summary>
        public async Task PushSetListDeletedAsync(Guid tenantGuid, int rebrickableListId)
        {
            if (!await ShouldPushAsync(tenantGuid)) return;

            var (client, token) = await GetClientAndTokenAsync(tenantGuid);
            if (client == null) return;

            await ExecuteWithAuditAsync(tenantGuid, "Push", "DELETE",
                $"api/v3/users/_token/setlists/{rebrickableListId}/",
                $"Delete set list {rebrickableListId}",
                TRIGGER_USER_ACTION,
                async () =>
                {
                    await client.DeleteUserSetListAsync(token, rebrickableListId);
                    return true;
                });
        }


        /// <summary>
        /// Push adding a set to a set list.
        /// </summary>
        public async Task PushSetListSetAddedAsync(Guid tenantGuid, int rebrickableListId, string setNum, int quantity)
        {
            if (!await ShouldPushAsync(tenantGuid)) return;

            var (client, token) = await GetClientAndTokenAsync(tenantGuid);
            if (client == null) return;

            await ExecuteWithAuditAsync(tenantGuid, "Push", "POST",
                $"api/v3/users/_token/setlists/{rebrickableListId}/sets/",
                $"Add set {setNum} x{quantity} to list {rebrickableListId}",
                TRIGGER_USER_ACTION,
                async () =>
                {
                    await client.AddUserSetListSetAsync(token, rebrickableListId, setNum, quantity);
                    return true;
                });
        }


        /// <summary>
        /// Push a part list creation to Rebrickable.
        /// Returns the Rebrickable list ID, or null on failure.
        /// </summary>
        public async Task<int?> PushPartListCreatedAsync(Guid tenantGuid, string name, bool isBuildable)
        {
            if (!await ShouldPushAsync(tenantGuid)) return null;

            var (client, token) = await GetClientAndTokenAsync(tenantGuid);
            if (client == null) return null;

            return await ExecuteWithAuditAsync(tenantGuid, "Push", "POST",
                $"api/v3/users/_token/partlists/",
                $"Create part list '{name}'",
                TRIGGER_USER_ACTION,
                async () =>
                {
                    var result = await client.CreateUserPartListAsync(token, name, isBuildable);
                    return result.Id;
                });
        }


        /// <summary>
        /// Push adding a part to a part list.
        /// </summary>
        public async Task PushPartListPartAddedAsync(
            Guid tenantGuid, int rebrickableListId, string partNum, int colorId, int quantity)
        {
            if (!await ShouldPushAsync(tenantGuid)) return;

            var (client, token) = await GetClientAndTokenAsync(tenantGuid);
            if (client == null) return;

            await ExecuteWithAuditAsync(tenantGuid, "Push", "POST",
                $"api/v3/users/_token/partlists/{rebrickableListId}/parts/",
                $"Add part {partNum} (color {colorId}) x{quantity} to list {rebrickableListId}",
                TRIGGER_USER_ACTION,
                async () =>
                {
                    await client.AddUserPartListPartAsync(token, rebrickableListId, partNum, colorId, quantity);
                    return true;
                });
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  PULL METHODS — import from Rebrickable into BMC
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Pull the user's complete Rebrickable collection into BMC.
        /// This is the master import method.
        /// </summary>
        public async Task<SyncImportResult> PullFullCollectionAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var result = new SyncImportResult { StartedAt = DateTime.UtcNow };

            var (client, token) = await GetClientAndTokenAsync(tenantGuid);
            if (client == null)
            {
                result.ErrorCount = 1;
                result.LastError = "Not connected to Rebrickable";
                result.CompletedAt = DateTime.UtcNow;
                return result;
            }

            try
            {
                // Pull sets
                await PullSetsInternalAsync(tenantGuid, client, token, result, ct);

                // Pull set lists
                await PullSetListsInternalAsync(tenantGuid, client, token, result, ct);

                // Pull part lists
                await PullPartListsInternalAsync(tenantGuid, client, token, result, ct);

                // Pull lost parts
                await PullLostPartsInternalAsync(tenantGuid, client, token, result, ct);

                // Update link with last pull date
                var link = await GetUserLinkAsync(tenantGuid, ct);
                if (link != null)
                {
                    link.lastPullDate = DateTime.UtcNow;
                    link.lastSyncError = result.ErrorCount > 0 ? result.LastError : null;
                    await _context.SaveChangesAsync(ct);
                }
            }
            catch (Exception ex)
            {
                result.ErrorCount++;
                result.LastError = ex.Message;

                var link = await GetUserLinkAsync(tenantGuid, ct);
                if (link != null)
                {
                    link.lastSyncError = ex.Message;
                    await _context.SaveChangesAsync(ct);
                }

                _logger.LogError(ex, "Full collection pull failed for tenant {TenantGuid}", tenantGuid);
            }

            result.CompletedAt = DateTime.UtcNow;
            return result;
        }


        /// <summary>
        /// Pull only sets from Rebrickable.
        /// </summary>
        public async Task<SyncImportResult> PullSetsAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var result = new SyncImportResult { StartedAt = DateTime.UtcNow };

            var (client, token) = await GetClientAndTokenAsync(tenantGuid);
            if (client == null)
            {
                result.ErrorCount = 1;
                result.LastError = "Not connected to Rebrickable";
                result.CompletedAt = DateTime.UtcNow;
                return result;
            }

            try
            {
                await PullSetsInternalAsync(tenantGuid, client, token, result, ct);
            }
            catch (Exception ex)
            {
                result.ErrorCount++;
                result.LastError = ex.Message;
                _logger.LogError(ex, "Set pull failed for tenant {TenantGuid}", tenantGuid);
            }

            result.CompletedAt = DateTime.UtcNow;
            return result;
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  INTERNAL PULL HELPERS
        // ═══════════════════════════════════════════════════════════════════════

        private async Task PullSetsInternalAsync(
            Guid tenantGuid, RebrickableApiClient client, string token,
            SyncImportResult result, CancellationToken ct)
        {
            var remoteSets = await ExecuteWithAuditAsync(tenantGuid, "Pull", "GET",
                "api/v3/users/_token/sets/",
                "Pull all user sets",
                TRIGGER_MANUAL_PULL,
                () => client.GetAllUserSetsAsync(token),
                r => r?.Count ?? 0);

            if (remoteSets == null) return;

            // Load existing BMC set imports to compare
            var existingImports = await _context.UserCollectionSetImports
                .Include(i => i.legoSet)
                .Where(i => i.tenantGuid == tenantGuid && i.active == true && i.deleted == false)
                .ToListAsync(ct);

            // Get the user's default collection (create if needed)
            var collection = await EnsureDefaultCollectionAsync(tenantGuid, ct);

            foreach (var remoteSet in remoteSets)
            {
                // Find the matching LEGO set in BMC by set number
                var legoSet = await _context.LegoSets
                    .FirstOrDefaultAsync(s => s.setNumber == remoteSet.SetNum
                        && s.active == true && s.deleted == false, ct);

                if (legoSet == null)
                {
                    _logger.LogWarning("Set {SetNum} from Rebrickable not found in BMC catalog", remoteSet.SetNum);
                    continue;
                }

                var existing = existingImports.FirstOrDefault(
                    i => i.legoSetId == legoSet.id && i.userCollectionId == collection.id);

                if (existing != null)
                {
                    // Update quantity if changed
                    if (existing.quantity != remoteSet.Quantity)
                    {
                        existing.quantity = remoteSet.Quantity;
                        result.SetsUpdated++;
                    }
                }
                else
                {
                    // Create new import record
                    _context.UserCollectionSetImports.Add(new UserCollectionSetImport
                    {
                        userCollectionId = collection.id,
                        legoSetId = legoSet.id,
                        quantity = remoteSet.Quantity,
                        importedDate = DateTime.UtcNow,
                        tenantGuid = tenantGuid,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    });
                    result.SetsCreated++;
                }
            }

            await _context.SaveChangesAsync(ct);
        }


        private async Task PullSetListsInternalAsync(
            Guid tenantGuid, RebrickableApiClient client, string token,
            SyncImportResult result, CancellationToken ct)
        {
            var remoteLists = await ExecuteWithAuditAsync(tenantGuid, "Pull", "GET",
                "api/v3/users/_token/setlists/",
                "Pull all user set lists",
                TRIGGER_MANUAL_PULL,
                () => client.GetAllUserSetListsAsync(token),
                r => r?.Count ?? 0);

            if (remoteLists == null) return;

            var existingLists = await _context.UserSetLists
                .Where(l => l.tenantGuid == tenantGuid && l.active == true && l.deleted == false)
                .ToListAsync(ct);

            foreach (var remoteList in remoteLists)
            {
                var existing = existingLists.FirstOrDefault(l => l.rebrickableListId == remoteList.Id);

                UserSetList bmcList;
                if (existing != null)
                {
                    existing.name = remoteList.Name;
                    existing.isBuildable = remoteList.IsBuildable;
                    bmcList = existing;
                    result.SetListsUpdated++;
                }
                else
                {
                    bmcList = new UserSetList
                    {
                        name = remoteList.Name,
                        isBuildable = remoteList.IsBuildable,
                        rebrickableListId = remoteList.Id,
                        tenantGuid = tenantGuid,
                        objectGuid = Guid.NewGuid(),
                        versionNumber = 1,
                        active = true,
                        deleted = false
                    };
                    _context.UserSetLists.Add(bmcList);
                    result.SetListsCreated++;
                }

                await _context.SaveChangesAsync(ct);

                // Pull sets within this list
                var remoteSets = await ExecuteWithAuditAsync(tenantGuid, "Pull", "GET",
                    $"api/v3/users/_token/setlists/{remoteList.Id}/sets/",
                    $"Pull sets in list '{remoteList.Name}'",
                    TRIGGER_MANUAL_PULL,
                    () => client.GetAllUserSetListSetsAsync(token, remoteList.Id),
                r => r?.Count ?? 0);

                if (remoteSets == null) continue;

                var existingItems = await _context.UserSetListItems
                    .Where(i => i.userSetListId == bmcList.id && i.active == true && i.deleted == false)
                    .ToListAsync(ct);

                foreach (var remoteSet in remoteSets)
                {
                    var legoSet = await _context.LegoSets
                        .FirstOrDefaultAsync(s => s.setNumber == remoteSet.SetNum
                            && s.active == true && s.deleted == false, ct);

                    if (legoSet == null) continue;

                    var existingItem = existingItems.FirstOrDefault(i => i.legoSetId == legoSet.id);

                    if (existingItem != null)
                    {
                        if (existingItem.quantity != remoteSet.Quantity)
                        {
                            existingItem.quantity = remoteSet.Quantity;
                            result.SetListItemsUpdated++;
                        }
                    }
                    else
                    {
                        _context.UserSetListItems.Add(new UserSetListItem
                        {
                            userSetListId = bmcList.id,
                            legoSetId = legoSet.id,
                            quantity = remoteSet.Quantity,
                            includeSpares = true,
                            tenantGuid = tenantGuid,
                            objectGuid = Guid.NewGuid(),
                            active = true,
                            deleted = false
                        });
                        result.SetListItemsCreated++;
                    }
                }

                await _context.SaveChangesAsync(ct);
            }
        }


        private async Task PullPartListsInternalAsync(
            Guid tenantGuid, RebrickableApiClient client, string token,
            SyncImportResult result, CancellationToken ct)
        {
            var remoteLists = await ExecuteWithAuditAsync(tenantGuid, "Pull", "GET",
                "api/v3/users/_token/partlists/",
                "Pull all user part lists",
                TRIGGER_MANUAL_PULL,
                () => client.GetAllUserPartListsAsync(token),
                r => r?.Count ?? 0);

            if (remoteLists == null) return;

            var existingLists = await _context.UserPartLists
                .Where(l => l.tenantGuid == tenantGuid && l.active == true && l.deleted == false)
                .ToListAsync(ct);

            foreach (var remoteList in remoteLists)
            {
                var existing = existingLists.FirstOrDefault(l => l.rebrickableListId == remoteList.Id);

                UserPartList bmcList;
                if (existing != null)
                {
                    existing.name = remoteList.Name;
                    existing.isBuildable = remoteList.IsBuildable;
                    bmcList = existing;
                    result.PartListsUpdated++;
                }
                else
                {
                    bmcList = new UserPartList
                    {
                        name = remoteList.Name,
                        isBuildable = remoteList.IsBuildable,
                        rebrickableListId = remoteList.Id,
                        tenantGuid = tenantGuid,
                        objectGuid = Guid.NewGuid(),
                        versionNumber = 1,
                        active = true,
                        deleted = false
                    };
                    _context.UserPartLists.Add(bmcList);
                    result.PartListsCreated++;
                }

                await _context.SaveChangesAsync(ct);

                // Pull parts within this list
                var remoteParts = await ExecuteWithAuditAsync(tenantGuid, "Pull", "GET",
                    $"api/v3/users/_token/partlists/{remoteList.Id}/parts/",
                    $"Pull parts in list '{remoteList.Name}'",
                    TRIGGER_MANUAL_PULL,
                    () => client.GetAllUserPartListPartsAsync(token, remoteList.Id),
                r => r?.Count ?? 0);

                if (remoteParts == null) continue;

                var existingItems = await _context.UserPartListItems
                    .Where(i => i.userPartListId == bmcList.id && i.active == true && i.deleted == false)
                    .ToListAsync(ct);

                foreach (var remotePart in remoteParts)
                {
                    if (remotePart.Part == null) continue;

                    // Look up the BMC brick part by part number
                    var brickPart = await _context.BrickParts
                        .FirstOrDefaultAsync(p => p.rebrickablePartNum == remotePart.Part.PartNum
                            && p.active == true && p.deleted == false, ct);

                    if (brickPart == null) continue;

                    // Look up the BMC colour by Rebrickable colour ID
                    int remoteColorId = remotePart.Color?.Id ?? 0;
                    var brickColour = await _context.BrickColours
                        .FirstOrDefaultAsync(c => c.rebrickableColorId == remoteColorId
                            && c.active == true && c.deleted == false, ct);

                    if (brickColour == null) continue;

                    var existingItem = existingItems.FirstOrDefault(
                        i => i.brickPartId == brickPart.id && i.brickColourId == brickColour.id);

                    if (existingItem != null)
                    {
                        if (existingItem.quantity != remotePart.Quantity)
                        {
                            existingItem.quantity = remotePart.Quantity;
                            result.PartListItemsUpdated++;
                        }
                    }
                    else
                    {
                        _context.UserPartListItems.Add(new UserPartListItem
                        {
                            userPartListId = bmcList.id,
                            brickPartId = brickPart.id,
                            brickColourId = brickColour.id,
                            quantity = remotePart.Quantity,
                            tenantGuid = tenantGuid,
                            objectGuid = Guid.NewGuid(),
                            active = true,
                            deleted = false
                        });
                        result.PartListItemsCreated++;
                    }
                }

                await _context.SaveChangesAsync(ct);
            }
        }


        private async Task PullLostPartsInternalAsync(
            Guid tenantGuid, RebrickableApiClient client, string token,
            SyncImportResult result, CancellationToken ct)
        {
            var remoteLostParts = await ExecuteWithAuditAsync(tenantGuid, "Pull", "GET",
                "api/v3/users/_token/lost_parts/",
                "Pull all lost parts",
                TRIGGER_MANUAL_PULL,
                () => client.GetAllUserLostPartsAsync(token),
                r => r?.Count ?? 0);

            if (remoteLostParts == null) return;

            var existingLostParts = await _context.UserLostParts
                .Where(l => l.tenantGuid == tenantGuid && l.active == true && l.deleted == false)
                .ToListAsync(ct);

            foreach (var remoteLp in remoteLostParts)
            {
                if (remoteLp.Part == null) continue;

                var brickPart = await _context.BrickParts
                    .FirstOrDefaultAsync(p => p.rebrickablePartNum == remoteLp.Part.PartNum
                        && p.active == true && p.deleted == false, ct);

                if (brickPart == null) continue;

                int lostColorId = remoteLp.Color?.Id ?? 0;
                var brickColour = await _context.BrickColours
                    .FirstOrDefaultAsync(c => c.rebrickableColorId == lostColorId
                        && c.active == true && c.deleted == false, ct);

                if (brickColour == null) continue;

                var existing = existingLostParts.FirstOrDefault(
                    l => l.brickPartId == brickPart.id && l.brickColourId == brickColour.id);

                if (existing != null)
                {
                    if (existing.lostQuantity != remoteLp.LostQuantity)
                    {
                        existing.lostQuantity = remoteLp.LostQuantity;
                        result.LostPartsUpdated++;
                    }
                }
                else
                {
                    _context.UserLostParts.Add(new UserLostPart
                    {
                        brickPartId = brickPart.id,
                        brickColourId = brickColour.id,
                        lostQuantity = remoteLp.LostQuantity,
                        rebrickableInvPartId = remoteLp.InvPartId,
                        tenantGuid = tenantGuid,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    });
                    result.LostPartsCreated++;
                }
            }

            await _context.SaveChangesAsync(ct);
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Determine whether push operations should fire based on the user's integration mode.
        /// </summary>
        private async Task<bool> ShouldPushAsync(Guid tenantGuid)
        {
            var link = await GetUserLinkAsync(tenantGuid);
            if (link == null) return false;

            return link.syncDirectionFlags == MODE_REALTIME || link.syncDirectionFlags == MODE_PUSH_ONLY;
        }


        /// <summary>
        /// Get a RebrickableApiClient and user token for the given tenant.
        /// Handles all three auth modes: reads from encrypted DB or from session cache.
        /// Returns (null, null) if not connected or if tokens are expired/missing.
        /// </summary>
        private async Task<(RebrickableApiClient client, string token)> GetClientAndTokenAsync(Guid tenantGuid)
        {
            var link = await GetUserLinkAsync(tenantGuid);
            if (link == null)
            {
                _logger.LogWarning("No Rebrickable link for tenant {TenantGuid}", tenantGuid);
                return (null, null);
            }

            string apiKey;
            string userToken;

            // Session-only mode: read from memory cache
            if (link.authMode == AUTH_SESSION_ONLY)
            {
                string cacheKey = $"{SESSION_CACHE_PREFIX}{tenantGuid}";
                if (_cache.TryGetValue(cacheKey, out SessionOnlyCredentials sessionCreds) && sessionCreds != null)
                {
                    apiKey = sessionCreds.ApiKey;
                    userToken = sessionCreds.UserToken;
                }
                else
                {
                    _logger.LogWarning("Session-only credentials expired for tenant {TenantGuid} — reconnect required", tenantGuid);
                    link.lastSyncError = "Session expired — please reconnect.";
                    await _context.SaveChangesAsync();
                    return (null, null);
                }
            }
            else
            {
                // DB-backed modes (ApiToken / TokenOnly): decrypt from stored values
                if (string.IsNullOrEmpty(link.encryptedApiToken))
                {
                    _logger.LogWarning("No Rebrickable API key stored for tenant {TenantGuid}", tenantGuid);
                    return (null, null);
                }

                if (string.IsNullOrEmpty(link.encryptedPassword))
                {
                    _logger.LogWarning("No Rebrickable user_token stored for tenant {TenantGuid} — reconnect required", tenantGuid);
                    return (null, null);
                }

                // Check token expiry if configured
                if (link.tokenExpiryDays.HasValue && link.tokenStoredDate.HasValue)
                {
                    var expiresAt = link.tokenStoredDate.Value.AddDays(link.tokenExpiryDays.Value);
                    if (DateTime.UtcNow > expiresAt)
                    {
                        _logger.LogWarning("Rebrickable token expired for tenant {TenantGuid} — stored {StoredDate}, expires after {ExpiryDays} days",
                            tenantGuid, link.tokenStoredDate.Value, link.tokenExpiryDays.Value);
                        link.lastSyncError = "Token expired — please reconnect or re-authenticate.";
                        await _context.SaveChangesAsync();
                        return (null, null);
                    }
                }

                apiKey = Decrypt(link.encryptedApiToken);
                userToken = Decrypt(link.encryptedPassword);

                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(userToken))
                {
                    _logger.LogWarning("Failed to decrypt Rebrickable tokens for tenant {TenantGuid} — reconnect required", tenantGuid);
                    return (null, null);
                }
            }

            var client = new RebrickableApiClient(apiKey);
            return (client, userToken);
        }


        /// <summary>
        /// Ensure the user has a default collection. Creates one if needed.
        /// </summary>
        private async Task<UserCollection> EnsureDefaultCollectionAsync(Guid tenantGuid, CancellationToken ct)
        {
            var collection = await _context.UserCollections
                .FirstOrDefaultAsync(c => c.tenantGuid == tenantGuid
                    && c.isDefault == true
                    && c.active == true && c.deleted == false, ct);

            if (collection == null)
            {
                collection = new UserCollection
                {
                    name = "My Collection",
                    description = "Default brick collection",
                    isDefault = true,
                    tenantGuid = tenantGuid,
                    objectGuid = Guid.NewGuid(),
                    versionNumber = 1,
                    active = true,
                    deleted = false
                };
                _context.UserCollections.Add(collection);
                await _context.SaveChangesAsync(ct);
            }

            return collection;
        }


        /// <summary>
        /// Execute an API call wrapped in transaction logging.
        /// Captures success/failure and logs to RebrickableTransaction.
        /// </summary>
        private async Task<T> ExecuteWithAuditAsync<T>(
            Guid tenantGuid, string direction, string httpMethod,
            string endpoint, string summary, string triggeredBy,
            Func<Task<T>> apiCall,
            Func<T, int> countExtractor = null)
        {
            try
            {
                T result = await apiCall();

                int? rowCount = countExtractor != null ? countExtractor(result) : null;
                string logSummary = rowCount.HasValue
                    ? $"{summary} — {rowCount.Value} rows"
                    : summary;

                await LogTransactionAsync(tenantGuid, direction, httpMethod,
                    endpoint, logSummary, 200, null, true, null, triggeredBy, recordCount: rowCount);

                // Update last push/pull date
                var link = await GetUserLinkAsync(tenantGuid);
                if (link != null)
                {
                    if (direction == "Push") link.lastPushDate = DateTime.UtcNow;
                    else link.lastPullDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return result;
            }
            catch (RebrickableApiException ex)
            {
                await LogTransactionAsync(tenantGuid, direction, httpMethod,
                    endpoint, summary, (int)ex.StatusCode, ex.ResponseBody, false, ex.Message, triggeredBy);

                _logger.LogWarning(ex, "Rebrickable API call failed: {Method} {Endpoint}", httpMethod, endpoint);
                return default;
            }
            catch (Exception ex)
            {
                await LogTransactionAsync(tenantGuid, direction, httpMethod,
                    endpoint, summary, 0, null, false, ex.Message, triggeredBy);

                _logger.LogError(ex, "Unexpected error in Rebrickable sync: {Method} {Endpoint}", httpMethod, endpoint);
                return default;
            }
        }


        /// <summary>
        /// Log a transaction to the RebrickableTransaction audit table.
        /// </summary>
        private async Task LogTransactionAsync(
            Guid tenantGuid, string direction, string httpMethod,
            string endpoint, string requestSummary,
            int responseStatusCode, string responseBody,
            bool success, string errorMessage,
            string triggeredBy,
            CancellationToken ct = default,
            int? recordCount = null)
        {
            try
            {
                var transaction = new RebrickableTransaction
                {
                    tenantGuid = tenantGuid,
                    objectGuid = Guid.NewGuid(),
                    transactionDate = DateTime.UtcNow,
                    direction = direction,
                    httpMethod = httpMethod,
                    endpoint = endpoint,
                    requestSummary = requestSummary,
                    responseStatusCode = responseStatusCode,
                    responseBody = responseBody?.Length > 4000 ? responseBody.Substring(0, 4000) : responseBody,
                    success = success,
                    errorMessage = errorMessage,
                    triggeredBy = triggeredBy,
                    recordCount = recordCount,
                    active = true,
                    deleted = false
                };

                _context.RebrickableTransactions.Add(transaction);
                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                // Never let transaction logging prevent the main operation
                _logger.LogError(ex, "Failed to log Rebrickable transaction");
            }
        }
    }
}
