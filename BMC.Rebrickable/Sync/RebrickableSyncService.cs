using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
    ///
    /// Responsibilities:
    ///  - PUSH: After BMC writes, conditionally call the Rebrickable equivalent
    ///  - PULL: Import Rebrickable data into BMC tables
    ///  - AUDIT: Log every API call to the RebrickableTransaction table
    /// </summary>
    public class RebrickableSyncService
    {
        private readonly BMCContext _context;
        private readonly ILogger<RebrickableSyncService> _logger;


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

        public const string AUTH_API_TOKEN = "ApiToken";
        public const string AUTH_ENCRYPTED_CREDENTIALS = "EncryptedCredentials";
        public const string AUTH_SESSION_ONLY = "SessionOnly";


        // ───────────────────────── Transaction trigger constants ─────────────────────────

        public const string TRIGGER_USER_ACTION = "UserAction";
        public const string TRIGGER_PERIODIC_SYNC = "PeriodicSync";
        public const string TRIGGER_MANUAL_PULL = "ManualPull";
        public const string TRIGGER_SESSION_LOGIN = "SessionLogin";


        public RebrickableSyncService(BMCContext context, ILogger<RebrickableSyncService> logger)
        {
            _context = context;
            _logger = logger;
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
                IsConnected = !string.IsNullOrEmpty(link.encryptedApiToken),
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
        /// Connect to Rebrickable with an API token. Validates by calling the profile endpoint.
        /// </summary>
        public async Task<(bool success, string error)> ConnectWithTokenAsync(
            Guid tenantGuid, string apiToken, string integrationMode, CancellationToken ct = default)
        {
            // Validate the token by attempting to get the user profile
            var client = new RebrickableApiClient(apiToken);
            RebrickableUserProfile profile;

            try
            {
                profile = await client.GetUserProfileAsync(apiToken);
            }
            catch (RebrickableApiException ex)
            {
                await LogTransactionAsync(tenantGuid, "Push", "GET",
                    "api/v3/users/_token/profile/",
                    "Validate API token",
                    (int)ex.StatusCode, ex.Message, false, ex.Message, TRIGGER_USER_ACTION, ct);

                return (false, $"Invalid token: {ex.Message}");
            }

            // Create or update the user link
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

            link.rebrickableUsername = profile.Username;
            link.encryptedApiToken = apiToken; // TODO: encrypt at rest
            link.authMode = AUTH_API_TOKEN;
            link.syncEnabled = integrationMode != MODE_NONE;
            link.syncDirectionFlags = integrationMode;
            link.lastSyncError = null;

            await _context.SaveChangesAsync(ct);

            await LogTransactionAsync(tenantGuid, "Push", "GET",
                "api/v3/users/_token/profile/",
                $"Connected as {profile.Username}",
                200, null, true, null, TRIGGER_USER_ACTION, ct);

            _logger.LogInformation("Rebrickable connected for tenant {TenantGuid} as {Username}", tenantGuid, profile.Username);

            return (true, null);
        }


        /// <summary>
        /// Disconnect from Rebrickable — clear stored credentials and set mode to None.
        /// </summary>
        public async Task DisconnectAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            var link = await GetUserLinkAsync(tenantGuid, ct);
            if (link == null) return;

            link.encryptedApiToken = null;
            link.encryptedPassword = null;
            link.authMode = null;
            link.syncEnabled = false;
            link.syncDirectionFlags = MODE_NONE;
            link.lastSyncError = null;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Rebrickable disconnected for tenant {TenantGuid}", tenantGuid);
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
                () => client.GetAllUserSetsAsync(token));

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
                () => client.GetAllUserSetListsAsync(token));

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
                    () => client.GetAllUserSetListSetsAsync(token, remoteList.Id));

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
                () => client.GetAllUserPartListsAsync(token));

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
                    () => client.GetAllUserPartListPartsAsync(token, remoteList.Id));

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
                    var brickColour = await _context.BrickColours
                        .FirstOrDefaultAsync(c => c.rebrickableColorId == (remotePart.Color?.Id ?? 0)
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
                () => client.GetAllUserLostPartsAsync(token));

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

                var brickColour = await _context.BrickColours
                    .FirstOrDefaultAsync(c => c.rebrickableColorId == (remoteLp.Color?.Id ?? 0)
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
        /// Returns (null, null) if not connected.
        /// </summary>
        private async Task<(RebrickableApiClient client, string token)> GetClientAndTokenAsync(Guid tenantGuid)
        {
            var link = await GetUserLinkAsync(tenantGuid);
            if (link == null || string.IsNullOrEmpty(link.encryptedApiToken))
            {
                _logger.LogWarning("No Rebrickable link/token for tenant {TenantGuid}", tenantGuid);
                return (null, null);
            }

            string apiToken = link.encryptedApiToken; // TODO: decrypt
            var client = new RebrickableApiClient(apiToken);
            return (client, apiToken);
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
            Func<Task<T>> apiCall)
        {
            try
            {
                T result = await apiCall();

                await LogTransactionAsync(tenantGuid, direction, httpMethod,
                    endpoint, summary, 200, null, true, null, triggeredBy);

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
            CancellationToken ct = default)
        {
            try
            {
                var transaction = new RebrickableTransaction
                {
                    tenantGuid = tenantGuid,
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
