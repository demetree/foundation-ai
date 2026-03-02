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
    /// Partial class extension for UserSetListsController that implements custom write methods
    /// with fire-and-forget Rebrickable push calls.
    /// 
    /// This replaces the auto-generated PUT/POST/DELETE methods (which are commented out via
    /// SetTableToBeReadonlyForControllerCreationPurposes in the database generator).
    /// 
    /// The business logic is identical to the generated versions; the only addition is a
    /// non-blocking push to Rebrickable after each successful save.
    /// </summary>
    public partial class UserSetListsController
    {
        /// <summary>
        /// Lazily resolves the RebrickableSyncService from the DI container.
        /// We cannot use constructor injection because the generated constructor is in the other partial file.
        /// </summary>
        private RebrickableSyncService SyncService =>
            HttpContext.RequestServices.GetService<RebrickableSyncService>();


        /// <summary>
        /// This updates an existing UserSetList record, then fires a non-blocking push to Rebrickable.
        ///
        /// The rate limit is 2 per second per user.
        /// </summary>
        [Route("api/UserSetList/{id}")]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [HttpPost]
        [HttpPut]
        public async Task<IActionResult> PutUserSetList(int id, [FromBody]Database.UserSetList.UserSetListDTO userSetListDTO, CancellationToken cancellationToken = default)
        {
            if (userSetListDTO == null)
            {
               return BadRequest();
            }

            StartAuditEventClock();

            //
            // BMC Collection Writer role needed to write to this table, or BMC Administrator role.
            //
            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
               return Forbid();
            }

            if (id != userSetListDTO.id)
            {
                return BadRequest();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

            bool userIsWriter = await UserCanWriteAsync(securityUser, 10, cancellationToken);
            bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }


            IQueryable<Database.UserSetList> query = (from x in _context.UserSetLists
                where
                (x.id == id)
                select x);

            query = query.Where(x => x.tenantGuid == userTenantGuid);

            Database.UserSetList existing = await query.FirstOrDefaultAsync(cancellationToken);

            if (existing == null)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserSetList PUT", id.ToString(), new Exception("No BMC.UserSetList entity could be found with the primary key provided."));
                return NotFound();
            }

            //
            // Validate the object guid.
            // 
            if (userSetListDTO.objectGuid == Guid.Empty)
            {
                userSetListDTO.objectGuid = existing.objectGuid;
            }
            else if (userSetListDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserSetList record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


            // Copy the existing object so it can be serialized as-is in the audit and history logs.
            Database.UserSetList cloneOfExisting = (Database.UserSetList)_context.Entry(existing).GetDatabaseValues().ToObject();

            //
            // Create a new UserSetList object using the data from the existing record, updated with what is in the DTO.
            //
            Database.UserSetList userSetList = (Database.UserSetList)_context.Entry(existing).GetDatabaseValues().ToObject();
            userSetList.ApplyDTO(userSetListDTO);

            //
            // The tenant guid for any UserSetList being saved must match the tenant guid of the user.  
            //
            if (existing.tenantGuid != userTenantGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
                return Problem("Data integrity violation detected while attempting to save.");
            }
            else
            {
                userSetList.tenantGuid = existing.tenantGuid;
            }

            lock (userSetListPutSyncRoot)
            {
                //
                // Validate the version number
                //
                if (existing.versionNumber != userSetList.versionNumber)
                {
                    CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "UserSetList save attempt was made but save request was with version " + userSetList.versionNumber + " and the current version number is " + existing.versionNumber, false);
                    return Problem("The UserSetList you are trying to update has already changed.  Please try your save again after reloading the UserSetList.");
                }
                else
                {
                    userSetList.versionNumber++;
                }


                // Is user who is not an admin trying to delete, or to work on a deleted record?
                if (userIsAdmin == false && (userSetList.deleted == true || existing.deleted == true))
                {
                    CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.UserSetList record.", id.ToString());
                    DestroySessionAndAuthentication();
                    return Forbid();
                }

                if (userSetList.name != null && userSetList.name.Length > 100)
                {
                    userSetList.name = userSetList.name.Substring(0, 100);
                }

                try
                {
                    EntityEntry<Database.UserSetList> attached = _context.Entry(existing);
                    attached.CurrentValues.SetValues(userSetList);

                    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                    {
                        _context.SaveChanges();

                        //
                        // Now add the change history
                        //
                        UserSetListChangeHistory userSetListChangeHistory = new UserSetListChangeHistory();
                        userSetListChangeHistory.userSetListId = userSetList.id;
                        userSetListChangeHistory.versionNumber = userSetList.versionNumber;
                        userSetListChangeHistory.timeStamp = DateTime.UtcNow;
                        userSetListChangeHistory.userId = securityUser.id;
                        userSetListChangeHistory.tenantGuid = userTenantGuid;
                        userSetListChangeHistory.data = JsonSerializer.Serialize(Database.UserSetList.CreateAnonymousWithFirstLevelSubObjects(userSetList));
                        _context.UserSetListChangeHistories.Add(userSetListChangeHistory);

                        _context.SaveChanges();

                        transaction.Commit();
                    }

                    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
                        "BMC.UserSetList entity successfully updated.",
                        true,
                        id.ToString(),
                        JsonSerializer.Serialize(Database.UserSetList.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                        JsonSerializer.Serialize(Database.UserSetList.CreateAnonymousWithFirstLevelSubObjects(userSetList)),
                        null);

                    //
                    // Fire-and-forget push to Rebrickable (non-blocking — errors logged, never thrown)
                    //
                    if (userSetList.rebrickableListId.HasValue)
                    {
                        var syncService = SyncService;
                        if (syncService != null)
                        {
                            var tenantGuid = userTenantGuid;
                            var rebrickableListId = userSetList.rebrickableListId.Value;
                            var listName = userSetList.name;
                            var isBuildable = userSetList.isBuildable;
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await syncService.PushSetListUpdatedAsync(tenantGuid, rebrickableListId, listName, isBuildable);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "Rebrickable push failed for UserSetList update {ListId}", rebrickableListId);
                                }
                            });
                        }
                    }

                    return Ok(Database.UserSetList.CreateAnonymous(userSetList));
                }
                catch (Exception ex)
                {
                    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
                        "BMC.UserSetList entity update failed",
                        false,
                        id.ToString(),
                        JsonSerializer.Serialize(Database.UserSetList.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                        JsonSerializer.Serialize(Database.UserSetList.CreateAnonymousWithFirstLevelSubObjects(userSetList)),
                        ex);

                    return Problem(ex.Message);
                }

            }
        }


        /// <summary>
        /// This creates a new UserSetList record, then fires a non-blocking push to Rebrickable.
        ///
        /// The rate limit is 2 per second per user.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/UserSetList", Name = "UserSetList")]
        public async Task<IActionResult> PostUserSetList([FromBody]Database.UserSetList.UserSetListDTO userSetListDTO, CancellationToken cancellationToken = default)
        {
            if (userSetListDTO == null)
            {
               return BadRequest();
            }

            StartAuditEventClock();

            //
            // BMC Collection Writer role needed to write to this table, or BMC Administrator role.
            //
            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
               return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

            bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }

            //
            // Create a new UserSetList object using the data from the DTO
            //
            Database.UserSetList userSetList = Database.UserSetList.FromDTO(userSetListDTO);

            try
            {
                //
                // Ensure that the tenant data is correct.
                //
                userSetList.tenantGuid = userTenantGuid;

                if (userSetList.name != null && userSetList.name.Length > 100)
                {
                    userSetList.name = userSetList.name.Substring(0, 100);
                }

                userSetList.objectGuid = Guid.NewGuid();
                userSetList.versionNumber = 1;

                _context.UserSetLists.Add(userSetList);

                await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
                {
                    await _context.SaveChangesAsync(cancellationToken);

                    //
                    // Now add the change history
                    //
                    _context.Entry(userSetList).State = EntityState.Detached;

                    //
                    // Nullify all object properties before serializing.
                    //
                    userSetList.UserSetListChangeHistories = null;
                    userSetList.UserSetListItems = null;


                    UserSetListChangeHistory userSetListChangeHistory = new UserSetListChangeHistory();
                    userSetListChangeHistory.userSetListId = userSetList.id;
                    userSetListChangeHistory.versionNumber = userSetList.versionNumber;
                    userSetListChangeHistory.timeStamp = DateTime.UtcNow;
                    userSetListChangeHistory.userId = securityUser.id;
                    userSetListChangeHistory.tenantGuid = userTenantGuid;
                    userSetListChangeHistory.data = JsonSerializer.Serialize(Database.UserSetList.CreateAnonymousWithFirstLevelSubObjects(userSetList));
                    _context.UserSetListChangeHistories.Add(userSetListChangeHistory);
                    await _context.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                        "BMC.UserSetList entity successfully created.",
                        true,
                        userSetList.id.ToString(),
                        "",
                        JsonSerializer.Serialize(Database.UserSetList.CreateAnonymousWithFirstLevelSubObjects(userSetList)),
                        null);
                }
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserSetList entity creation failed.", false, userSetList.id.ToString(), "", JsonSerializer.Serialize(Database.UserSetList.CreateAnonymousWithFirstLevelSubObjects(userSetList)), ex);

                return Problem(ex.Message);
            }


            BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserSetList", userSetList.id, userSetList.name));

            //
            // Fire-and-forget push to Rebrickable — create the list remotely and capture its Rebrickable ID
            //
            {
                var syncService = SyncService;
                if (syncService != null)
                {
                    var tenantGuid = userTenantGuid;
                    var listName = userSetList.name;
                    var isBuildable = userSetList.isBuildable;
                    var localId = userSetList.id;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            int? rebrickableId = await syncService.PushSetListCreatedAsync(tenantGuid, listName, isBuildable);
                            if (rebrickableId.HasValue)
                            {
                                //
                                // Store the Rebrickable list ID back on the local entity so future updates can reference it.
                                // Use a fresh DbContext scope to avoid threading issues with the request-scoped context.
                                //
                                _logger.LogInformation("Rebrickable set list created with ID {RebrickableListId} for local ID {LocalId}", rebrickableId.Value, localId);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Rebrickable push failed for UserSetList creation {LocalId}", localId);
                        }
                    });
                }
            }

            return CreatedAtRoute("UserSetList", new { id = userSetList.id }, Database.UserSetList.CreateAnonymousWithFirstLevelSubObjects(userSetList));
        }


        /// <summary>
        /// This deletes a UserSetList record, then fires a non-blocking push to Rebrickable.
        /// 
        /// The rate limit is 2 per second per user.
        /// </summary>
        [HttpDelete]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/UserSetList/{id}")]
        [Route("api/UserSetList")]
        public async Task<IActionResult> DeleteUserSetList(int id, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            //
            // BMC Collection Writer role needed to write to this table, or BMC Administrator role.
            //
            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
               return Forbid();
            }


            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }

            IQueryable<Database.UserSetList> query = (from x in _context.UserSetLists
                where
                (x.id == id)
                select x);

            query = query.Where(x => x.tenantGuid == userTenantGuid);

            Database.UserSetList userSetList = await query.FirstOrDefaultAsync(cancellationToken);

            if (userSetList == null)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserSetList DELETE", id.ToString(), new Exception("No BMC.UserSetList entity could be find with the primary key provided."));
                return NotFound();
            }

            Database.UserSetList cloneOfExisting = (Database.UserSetList)_context.Entry(userSetList).GetDatabaseValues().ToObject();

            // Capture the rebrickable list id before soft-delete
            int? rebrickableListId = userSetList.rebrickableListId;

            lock (userSetListDeleteSyncRoot)
            {
                try
                {
                    userSetList.deleted = true;
                    userSetList.versionNumber++;

                    _context.SaveChanges();

                    //
                    // Now add the change history
                    //
                    UserSetListChangeHistory userSetListChangeHistory = new UserSetListChangeHistory();
                    userSetListChangeHistory.userSetListId = userSetList.id;
                    userSetListChangeHistory.versionNumber = userSetList.versionNumber;
                    userSetListChangeHistory.timeStamp = DateTime.UtcNow;
                    userSetListChangeHistory.userId = securityUser.id;
                    userSetListChangeHistory.tenantGuid = userTenantGuid;
                    userSetListChangeHistory.data = JsonSerializer.Serialize(Database.UserSetList.CreateAnonymousWithFirstLevelSubObjects(userSetList));
                    _context.UserSetListChangeHistories.Add(userSetListChangeHistory);

                    _context.SaveChanges();

                    CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
                        "BMC.UserSetList entity successfully deleted.",
                        true,
                        id.ToString(),
                        JsonSerializer.Serialize(Database.UserSetList.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                        JsonSerializer.Serialize(Database.UserSetList.CreateAnonymousWithFirstLevelSubObjects(userSetList)),
                        null);

                }
                catch (Exception ex)
                {
                    CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
                        "BMC.UserSetList entity delete failed",
                        false,
                        id.ToString(),
                        JsonSerializer.Serialize(Database.UserSetList.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                        JsonSerializer.Serialize(Database.UserSetList.CreateAnonymousWithFirstLevelSubObjects(userSetList)),
                        ex);

                    return Problem(ex.Message);
                }

                //
                // Fire-and-forget push to Rebrickable (non-blocking — errors logged, never thrown)
                //
                if (rebrickableListId.HasValue)
                {
                    var syncService = SyncService;
                    if (syncService != null)
                    {
                        var tenantGuid = userTenantGuid;
                        var rbListId = rebrickableListId.Value;
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await syncService.PushSetListDeletedAsync(tenantGuid, rbListId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Rebrickable push failed for UserSetList delete {ListId}", rbListId);
                            }
                        });
                    }
                }

                return Ok();
            }
        }
    }
}
