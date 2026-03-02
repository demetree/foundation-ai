using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Foundation.Security;
using Foundation.Auditor;
using Foundation;
using Foundation.Security.Database;
using Foundation.BMC.Database;
using Foundation.Controllers;
using BMC.Rebrickable.Sync;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// Partial class extension for UserSetListItemsController that implements custom write methods
    /// with fire-and-forget Rebrickable push calls.
    /// 
    /// Replaces the auto-generated PUT/POST/DELETE methods (commented out via
    /// SetTableToBeReadonlyForControllerCreationPurposes).
    /// </summary>
    public partial class UserSetListItemsController
    {
        private RebrickableSyncService SyncService =>
            HttpContext.RequestServices.GetService<RebrickableSyncService>();


        /// <summary>
        /// Resolves the Rebrickable set number and list ID for a given UserSetListItem.
        /// Returns (rebrickableListId, setNum) or (null, null) if not resolvable.
        /// </summary>
        private async Task<(int? rebrickableListId, string setNum)> ResolveSetListItemPushDataAsync(
            int userSetListId, int legoSetId, CancellationToken ct)
        {
            var parentList = await _context.UserSetLists
                .Where(l => l.id == userSetListId && l.deleted == false)
                .Select(l => l.rebrickableListId)
                .FirstOrDefaultAsync(ct);

            if (!parentList.HasValue) return (null, null);

            var setNum = await _context.LegoSets
                .Where(s => s.id == legoSetId && s.active == true && s.deleted == false)
                .Select(s => s.setNumber)
                .FirstOrDefaultAsync(ct);

            return (parentList, setNum);
        }


        /// <summary>
        /// Updates an existing UserSetListItem, then fires a non-blocking push to Rebrickable.
        /// </summary>
        [Route("api/UserSetListItem/{id}")]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [HttpPost]
        [HttpPut]
        public async Task<IActionResult> PutUserSetListItem(int id, [FromBody]Database.UserSetListItem.UserSetListItemDTO userSetListItemDTO, CancellationToken cancellationToken = default)
        {
            if (userSetListItemDTO == null)
            {
               return BadRequest();
            }

            StartAuditEventClock();

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
               return Forbid();
            }

            if (id != userSetListItemDTO.id)
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


            IQueryable<Database.UserSetListItem> query = (from x in _context.UserSetListItems
                where
                (x.id == id)
                select x);

            query = query.Where(x => x.tenantGuid == userTenantGuid);

            Database.UserSetListItem existing = await query.FirstOrDefaultAsync(cancellationToken);

            if (existing == null)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserSetListItem PUT", id.ToString(), new Exception("No BMC.UserSetListItem entity could be found with the primary key provided."));
                return NotFound();
            }

            if (userSetListItemDTO.objectGuid == Guid.Empty)
            {
                userSetListItemDTO.objectGuid = existing.objectGuid;
            }
            else if (userSetListItemDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserSetListItem record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


            Database.UserSetListItem cloneOfExisting = (Database.UserSetListItem)_context.Entry(existing).GetDatabaseValues().ToObject();

            Database.UserSetListItem userSetListItem = (Database.UserSetListItem)_context.Entry(existing).GetDatabaseValues().ToObject();
            userSetListItem.ApplyDTO(userSetListItemDTO);

            if (existing.tenantGuid != userTenantGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
                return Problem("Data integrity violation detected while attempting to save.");
            }
            else
            {
                userSetListItem.tenantGuid = existing.tenantGuid;
            }

            if (userIsAdmin == false && (userSetListItem.deleted == true || existing.deleted == true))
            {
                CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.UserSetListItem record.", id.ToString());
                DestroySessionAndAuthentication();
                return Forbid();
            }

            EntityEntry<Database.UserSetListItem> attached = _context.Entry(existing);
            attached.CurrentValues.SetValues(userSetListItem);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);

                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                    "BMC.UserSetListItem entity successfully updated.",
                    true,
                    id.ToString(),
                    JsonSerializer.Serialize(Database.UserSetListItem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                    JsonSerializer.Serialize(Database.UserSetListItem.CreateAnonymousWithFirstLevelSubObjects(userSetListItem)),
                    null);

                // Fire-and-forget push to Rebrickable
                {
                    var syncService = SyncService;
                    if (syncService != null)
                    {
                        var tenantGuid = userTenantGuid;
                        var listId = userSetListItem.userSetListId;
                        var setId = userSetListItem.legoSetId;
                        var qty = userSetListItem.quantity;
                        var spares = userSetListItem.includeSpares;
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                var (rbListId, setNum) = await ResolveSetListItemPushDataAsync(listId, setId, CancellationToken.None);
                                if (rbListId.HasValue && setNum != null)
                                {
                                    await syncService.PushSetListSetUpdatedAsync(tenantGuid, rbListId.Value, setNum, qty, spares);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Rebrickable push failed for UserSetListItem update {Id}", id);
                            }
                        });
                    }
                }

                return Ok(Database.UserSetListItem.CreateAnonymous(userSetListItem));
            }
            catch (Exception ex)
            {
                CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
                    "BMC.UserSetListItem entity update failed",
                    false,
                    id.ToString(),
                    JsonSerializer.Serialize(Database.UserSetListItem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                    JsonSerializer.Serialize(Database.UserSetListItem.CreateAnonymousWithFirstLevelSubObjects(userSetListItem)),
                    ex);

                return Problem(ex.Message);
            }
        }


        /// <summary>
        /// Creates a new UserSetListItem, then fires a non-blocking push to Rebrickable.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/UserSetListItem", Name = "UserSetListItem")]
        public async Task<IActionResult> PostUserSetListItem([FromBody]Database.UserSetListItem.UserSetListItemDTO userSetListItemDTO, CancellationToken cancellationToken = default)
        {
            if (userSetListItemDTO == null)
            {
               return BadRequest();
            }

            StartAuditEventClock();

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

            Database.UserSetListItem userSetListItem = Database.UserSetListItem.FromDTO(userSetListItemDTO);

            try
            {
                userSetListItem.tenantGuid = userTenantGuid;
                userSetListItem.objectGuid = Guid.NewGuid();
                _context.UserSetListItems.Add(userSetListItem);
                await _context.SaveChangesAsync(cancellationToken);

                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                    "BMC.UserSetListItem entity successfully created.",
                    true,
                    userSetListItem.id.ToString(),
                    "",
                    JsonSerializer.Serialize(Database.UserSetListItem.CreateAnonymousWithFirstLevelSubObjects(userSetListItem)),
                    null);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserSetListItem entity creation failed.", false, userSetListItem.id.ToString(), "", JsonSerializer.Serialize(userSetListItem), ex);
                return Problem(ex.Message);
            }


            BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserSetListItem", userSetListItem.id, userSetListItem.id.ToString()));

            // Fire-and-forget push to Rebrickable
            {
                var syncService = SyncService;
                if (syncService != null)
                {
                    var tenantGuid = userTenantGuid;
                    var listId = userSetListItem.userSetListId;
                    var setId = userSetListItem.legoSetId;
                    var qty = userSetListItem.quantity;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var (rbListId, setNum) = await ResolveSetListItemPushDataAsync(listId, setId, CancellationToken.None);
                            if (rbListId.HasValue && setNum != null)
                            {
                                await syncService.PushSetListSetAddedAsync(tenantGuid, rbListId.Value, setNum, qty);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Rebrickable push failed for UserSetListItem creation {Id}", userSetListItem.id);
                        }
                    });
                }
            }

            return CreatedAtRoute("UserSetListItem", new { id = userSetListItem.id }, Database.UserSetListItem.CreateAnonymousWithFirstLevelSubObjects(userSetListItem));
        }


        /// <summary>
        /// Deletes a UserSetListItem, then fires a non-blocking push to Rebrickable.
        /// </summary>
        [HttpDelete]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/UserSetListItem/{id}")]
        [Route("api/UserSetListItem")]
        public async Task<IActionResult> DeleteUserSetListItem(int id, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

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

            IQueryable<Database.UserSetListItem> query = (from x in _context.UserSetListItems
                where
                (x.id == id)
                select x);

            query = query.Where(x => x.tenantGuid == userTenantGuid);

            Database.UserSetListItem userSetListItem = await query.FirstOrDefaultAsync(cancellationToken);

            if (userSetListItem == null)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserSetListItem DELETE", id.ToString(), new Exception("No BMC.UserSetListItem entity could be find with the primary key provided."));
                return NotFound();
            }
            Database.UserSetListItem cloneOfExisting = (Database.UserSetListItem)_context.Entry(userSetListItem).GetDatabaseValues().ToObject();

            // Capture push data before soft-delete
            int listId = userSetListItem.userSetListId;
            int setId = userSetListItem.legoSetId;

            try
            {
                userSetListItem.deleted = true;
                await _context.SaveChangesAsync(cancellationToken);

                await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
                    "BMC.UserSetListItem entity successfully deleted.",
                    true,
                    id.ToString(),
                    JsonSerializer.Serialize(Database.UserSetListItem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                    JsonSerializer.Serialize(Database.UserSetListItem.CreateAnonymousWithFirstLevelSubObjects(userSetListItem)),
                    null);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
                    "BMC.UserSetListItem entity delete failed.",
                    false,
                    id.ToString(),
                    JsonSerializer.Serialize(Database.UserSetListItem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                    JsonSerializer.Serialize(Database.UserSetListItem.CreateAnonymousWithFirstLevelSubObjects(userSetListItem)),
                    ex);

                return Problem(ex.Message);
            }

            // Fire-and-forget push to Rebrickable
            {
                var syncService = SyncService;
                if (syncService != null)
                {
                    var tenantGuid = userTenantGuid;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var (rbListId, setNum) = await ResolveSetListItemPushDataAsync(listId, setId, CancellationToken.None);
                            if (rbListId.HasValue && setNum != null)
                            {
                                await syncService.PushSetListSetRemovedAsync(tenantGuid, rbListId.Value, setNum);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Rebrickable push failed for UserSetListItem delete {Id}", id);
                        }
                    });
                }
            }

            return Ok();
        }
    }
}
