using System;
using System.Data;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Foundation.Security;
using Foundation.Auditor;
using Foundation;
using Foundation.Security.Database;
using Foundation.BMC.Database;
using Foundation.ChangeHistory;
using Foundation.Controllers;
using BMC.Rebrickable.Sync;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// Partial class for UserPartListsController with Rebrickable push hooks.
    /// Mirrors UserSetListsController.Sync.cs but for part lists.
    /// </summary>
    public partial class UserPartListsController
    {
        private RebrickableSyncService SyncService =>
            HttpContext.RequestServices.GetService<RebrickableSyncService>();


        [Route("api/UserPartList/{id}")]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [HttpPost]
        [HttpPut]
        public async Task<IActionResult> PutUserPartList(int id, [FromBody]Database.UserPartList.UserPartListDTO userPartListDTO, CancellationToken cancellationToken = default)
        {
            if (userPartListDTO == null) return BadRequest();

            StartAuditEventClock();

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
                return Forbid();

            if (id != userPartListDTO.id) return BadRequest();

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
            bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
            Guid userTenantGuid;

            try { userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken); }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }

            var query = _context.UserPartLists.Where(x => x.id == id && x.tenantGuid == userTenantGuid);
            Database.UserPartList existing = await query.FirstOrDefaultAsync(cancellationToken);

            if (existing == null)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserPartList PUT", id.ToString(), new Exception("No BMC.UserPartList entity could be found with the primary key provided."));
                return NotFound();
            }

            if (userPartListDTO.objectGuid == Guid.Empty) { userPartListDTO.objectGuid = existing.objectGuid; }
            else if (userPartListDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserPartList record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }

            Database.UserPartList cloneOfExisting = (Database.UserPartList)_context.Entry(existing).GetDatabaseValues().ToObject();
            Database.UserPartList userPartList = (Database.UserPartList)_context.Entry(existing).GetDatabaseValues().ToObject();
            userPartList.ApplyDTO(userPartListDTO);

            if (existing.tenantGuid != userTenantGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
                return Problem("Data integrity violation detected while attempting to save.");
            }
            else { userPartList.tenantGuid = existing.tenantGuid; }

            lock (userPartListPutSyncRoot)
            {
                if (existing.versionNumber != userPartList.versionNumber)
                {
                    CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "UserPartList save attempt was made but save request was with version " + userPartList.versionNumber + " and the current version number is " + existing.versionNumber, false);
                    return Problem("The UserPartList you are trying to update has already changed.  Please try your save again after reloading the UserPartList.");
                }
                else { userPartList.versionNumber++; }

                if (userIsAdmin == false && (userPartList.deleted == true || existing.deleted == true))
                {
                    CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.UserPartList record.", id.ToString());
                    DestroySessionAndAuthentication();
                    return Forbid();
                }

                if (userPartList.name != null && userPartList.name.Length > 100) { userPartList.name = userPartList.name.Substring(0, 100); }

                try
                {
                    EntityEntry<Database.UserPartList> attached = _context.Entry(existing);
                    attached.CurrentValues.SetValues(userPartList);

                    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                    {
                        _context.SaveChanges();

                        UserPartListChangeHistory h = new UserPartListChangeHistory();
                        h.userPartListId = userPartList.id;
                        h.versionNumber = userPartList.versionNumber;
                        h.timeStamp = DateTime.UtcNow;
                        h.userId = securityUser.id;
                        h.tenantGuid = userTenantGuid;
                        h.data = JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList));
                        _context.UserPartListChangeHistories.Add(h);

                        _context.SaveChanges();
                        transaction.Commit();
                    }

                    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "BMC.UserPartList entity successfully updated.", true, id.ToString(),
                        JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                        JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList)), null);

                    // Fire-and-forget push to Rebrickable
                    if (userPartList.rebrickableListId.HasValue)
                    {
                        var syncService = SyncService;
                        if (syncService != null)
                        {
                            var tenantGuid = userTenantGuid;
                            var rbListId = userPartList.rebrickableListId.Value;
                            var listName = userPartList.name;
                            var isBuildable = userPartList.isBuildable;
                            _ = Task.Run(async () =>
                            {
                                try { await syncService.PushPartListUpdatedAsync(tenantGuid, rbListId, listName, isBuildable); }
                                catch (Exception ex) { _logger.LogWarning(ex, "Rebrickable push failed for UserPartList update {ListId}", rbListId); }
                            });
                        }
                    }

                    return Ok(Database.UserPartList.CreateAnonymous(userPartList));
                }
                catch (Exception ex)
                {
                    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "BMC.UserPartList entity update failed", false, id.ToString(),
                        JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                        JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList)), ex);
                    return Problem(ex.Message);
                }
            }
        }


        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/UserPartList", Name = "UserPartList")]
        public async Task<IActionResult> PostUserPartList([FromBody]Database.UserPartList.UserPartListDTO userPartListDTO, CancellationToken cancellationToken = default)
        {
            if (userPartListDTO == null) return BadRequest();

            StartAuditEventClock();

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
                return Forbid();

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
            Guid userTenantGuid;

            try { userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken); }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }

            Database.UserPartList userPartList = Database.UserPartList.FromDTO(userPartListDTO);

            try
            {
                userPartList.tenantGuid = userTenantGuid;
                if (userPartList.name != null && userPartList.name.Length > 100) { userPartList.name = userPartList.name.Substring(0, 100); }
                userPartList.objectGuid = Guid.NewGuid();
                userPartList.versionNumber = 1;
                _context.UserPartLists.Add(userPartList);

                await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
                {
                    await _context.SaveChangesAsync(cancellationToken);
                    _context.Entry(userPartList).State = EntityState.Detached;
                    userPartList.UserPartListChangeHistories = null;
                    userPartList.UserPartListItems = null;

                    UserPartListChangeHistory h = new UserPartListChangeHistory();
                    h.userPartListId = userPartList.id;
                    h.versionNumber = userPartList.versionNumber;
                    h.timeStamp = DateTime.UtcNow;
                    h.userId = securityUser.id;
                    h.tenantGuid = userTenantGuid;
                    h.data = JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList));
                    _context.UserPartListChangeHistories.Add(h);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserPartList entity successfully created.", true, userPartList.id.ToString(), "",
                        JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList)), null);
                }
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserPartList entity creation failed.", false, userPartList.id.ToString(), "",
                    JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList)), ex);
                return Problem(ex.Message);
            }

            BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserPartList", userPartList.id, userPartList.name));

            // Fire-and-forget push to Rebrickable
            {
                var syncService = SyncService;
                if (syncService != null)
                {
                    var tenantGuid = userTenantGuid;
                    var listName = userPartList.name;
                    var isBuildable = userPartList.isBuildable;
                    var localId = userPartList.id;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            int? rbId = await syncService.PushPartListCreatedAsync(tenantGuid, listName, isBuildable);
                            if (rbId.HasValue)
                                _logger.LogInformation("Rebrickable part list created with ID {RebrickableListId} for local ID {LocalId}", rbId.Value, localId);
                        }
                        catch (Exception ex) { _logger.LogWarning(ex, "Rebrickable push failed for UserPartList creation {LocalId}", localId); }
                    });
                }
            }

            return CreatedAtRoute("UserPartList", new { id = userPartList.id }, Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList));
        }


        [HttpDelete]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/UserPartList/{id}")]
        [Route("api/UserPartList")]
        public async Task<IActionResult> DeleteUserPartList(int id, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
                return Forbid();

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;

            try { userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken); }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }

            var query = _context.UserPartLists.Where(x => x.id == id && x.tenantGuid == userTenantGuid);
            Database.UserPartList userPartList = await query.FirstOrDefaultAsync(cancellationToken);

            if (userPartList == null)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserPartList DELETE", id.ToString(), new Exception("No BMC.UserPartList entity could be find with the primary key provided."));
                return NotFound();
            }
            Database.UserPartList cloneOfExisting = (Database.UserPartList)_context.Entry(userPartList).GetDatabaseValues().ToObject();
            int? rebrickableListId = userPartList.rebrickableListId;

            lock (userPartListDeleteSyncRoot)
            {
                try
                {
                    userPartList.deleted = true;
                    userPartList.versionNumber++;
                    _context.SaveChanges();

                    UserPartListChangeHistory h = new UserPartListChangeHistory();
                    h.userPartListId = userPartList.id;
                    h.versionNumber = userPartList.versionNumber;
                    h.timeStamp = DateTime.UtcNow;
                    h.userId = securityUser.id;
                    h.tenantGuid = userTenantGuid;
                    h.data = JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList));
                    _context.UserPartListChangeHistories.Add(h);
                    _context.SaveChanges();

                    CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "BMC.UserPartList entity successfully deleted.", true, id.ToString(),
                        JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                        JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList)), null);
                }
                catch (Exception ex)
                {
                    CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "BMC.UserPartList entity delete failed", false, id.ToString(),
                        JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                        JsonSerializer.Serialize(Database.UserPartList.CreateAnonymousWithFirstLevelSubObjects(userPartList)), ex);
                    return Problem(ex.Message);
                }

                // Fire-and-forget push to Rebrickable
                if (rebrickableListId.HasValue)
                {
                    var syncService = SyncService;
                    if (syncService != null)
                    {
                        var tenantGuid = userTenantGuid;
                        var rbListId = rebrickableListId.Value;
                        _ = Task.Run(async () =>
                        {
                            try { await syncService.PushPartListDeletedAsync(tenantGuid, rbListId); }
                            catch (Exception ex) { _logger.LogWarning(ex, "Rebrickable push failed for UserPartList delete {ListId}", rbListId); }
                        });
                    }
                }

                return Ok();
            }
        }
    }
}
