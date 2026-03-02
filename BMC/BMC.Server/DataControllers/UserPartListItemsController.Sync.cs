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
    /// Partial class for UserPartListItemsController with Rebrickable push hooks.
    /// </summary>
    public partial class UserPartListItemsController
    {
        private RebrickableSyncService SyncService =>
            HttpContext.RequestServices.GetService<RebrickableSyncService>();

        /// <summary>
        /// Resolves the Rebrickable list ID and part identifiers for a UserPartListItem push.
        /// </summary>
        private async Task<(int? rebrickableListId, string partNum, int? colorId)> ResolvePartListItemPushDataAsync(
            int userPartListId, int brickPartId, int brickColourId, CancellationToken ct)
        {
            var parentList = await _context.UserPartLists
                .Where(l => l.id == userPartListId && l.deleted == false)
                .Select(l => l.rebrickableListId)
                .FirstOrDefaultAsync(ct);

            if (!parentList.HasValue) return (null, null, null);

            var partNum = await _context.BrickParts
                .Where(p => p.id == brickPartId && p.active == true && p.deleted == false)
                .Select(p => p.rebrickablePartNum)
                .FirstOrDefaultAsync(ct);

            var colorId = await _context.BrickColours
                .Where(c => c.id == brickColourId && c.active == true && c.deleted == false)
                .Select(c => c.rebrickableColorId)
                .FirstOrDefaultAsync(ct);

            return (parentList, partNum, colorId);
        }


        [Route("api/UserPartListItem/{id}")]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [HttpPost]
        [HttpPut]
        public async Task<IActionResult> PutUserPartListItem(int id, [FromBody]Database.UserPartListItem.UserPartListItemDTO userPartListItemDTO, CancellationToken cancellationToken = default)
        {
            if (userPartListItemDTO == null) return BadRequest();
            StartAuditEventClock();

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
                return Forbid();

            if (id != userPartListItemDTO.id) return BadRequest();

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

            Database.UserPartListItem existing = await _context.UserPartListItems
                .Where(x => x.id == id && x.tenantGuid == userTenantGuid)
                .FirstOrDefaultAsync(cancellationToken);

            if (existing == null)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserPartListItem PUT", id.ToString(), new Exception("No BMC.UserPartListItem entity could be found with the primary key provided."));
                return NotFound();
            }

            if (userPartListItemDTO.objectGuid == Guid.Empty) { userPartListItemDTO.objectGuid = existing.objectGuid; }
            else if (userPartListItemDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserPartListItem record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }

            Database.UserPartListItem cloneOfExisting = (Database.UserPartListItem)_context.Entry(existing).GetDatabaseValues().ToObject();
            Database.UserPartListItem userPartListItem = (Database.UserPartListItem)_context.Entry(existing).GetDatabaseValues().ToObject();
            userPartListItem.ApplyDTO(userPartListItemDTO);

            if (existing.tenantGuid != userTenantGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
                return Problem("Data integrity violation detected while attempting to save.");
            }
            else { userPartListItem.tenantGuid = existing.tenantGuid; }

            if (userIsAdmin == false && (userPartListItem.deleted == true || existing.deleted == true))
            {
                CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.UserPartListItem record.", id.ToString());
                DestroySessionAndAuthentication();
                return Forbid();
            }

            EntityEntry<Database.UserPartListItem> attached = _context.Entry(existing);
            attached.CurrentValues.SetValues(userPartListItem);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "BMC.UserPartListItem entity successfully updated.", true, id.ToString(),
                    JsonSerializer.Serialize(Database.UserPartListItem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                    JsonSerializer.Serialize(Database.UserPartListItem.CreateAnonymousWithFirstLevelSubObjects(userPartListItem)), null);

                // Fire-and-forget push
                var syncService = SyncService;
                if (syncService != null)
                {
                    var tenantGuid = userTenantGuid;
                    var listId = userPartListItem.userPartListId;
                    var partId = userPartListItem.brickPartId;
                    var colourId = userPartListItem.brickColourId;
                    var qty = userPartListItem.quantity;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var (rbListId, partNum, rbColorId) = await ResolvePartListItemPushDataAsync(listId, partId, colourId, CancellationToken.None);
                            if (rbListId.HasValue && partNum != null && rbColorId.HasValue)
                                await syncService.PushPartListPartUpdatedAsync(tenantGuid, rbListId.Value, partNum, rbColorId.Value, qty);
                        }
                        catch (Exception ex) { _logger.LogWarning(ex, "Rebrickable push failed for UserPartListItem update {Id}", id); }
                    });
                }

                return Ok(Database.UserPartListItem.CreateAnonymous(userPartListItem));
            }
            catch (Exception ex)
            {
                CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "BMC.UserPartListItem entity update failed", false, id.ToString(),
                    JsonSerializer.Serialize(Database.UserPartListItem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                    JsonSerializer.Serialize(Database.UserPartListItem.CreateAnonymousWithFirstLevelSubObjects(userPartListItem)), ex);
                return Problem(ex.Message);
            }
        }


        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/UserPartListItem", Name = "UserPartListItem")]
        public async Task<IActionResult> PostUserPartListItem([FromBody]Database.UserPartListItem.UserPartListItemDTO userPartListItemDTO, CancellationToken cancellationToken = default)
        {
            if (userPartListItemDTO == null) return BadRequest();
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

            Database.UserPartListItem userPartListItem = Database.UserPartListItem.FromDTO(userPartListItemDTO);

            try
            {
                userPartListItem.tenantGuid = userTenantGuid;
                userPartListItem.objectGuid = Guid.NewGuid();
                _context.UserPartListItems.Add(userPartListItem);
                await _context.SaveChangesAsync(cancellationToken);

                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserPartListItem entity successfully created.", true, userPartListItem.id.ToString(), "",
                    JsonSerializer.Serialize(Database.UserPartListItem.CreateAnonymousWithFirstLevelSubObjects(userPartListItem)), null);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserPartListItem entity creation failed.", false, userPartListItem.id.ToString(), "",
                    JsonSerializer.Serialize(userPartListItem), ex);
                return Problem(ex.Message);
            }

            BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserPartListItem", userPartListItem.id, userPartListItem.id.ToString()));

            // Fire-and-forget push
            var syncService = SyncService;
            if (syncService != null)
            {
                var tenantGuid = userTenantGuid;
                var listId = userPartListItem.userPartListId;
                var partId = userPartListItem.brickPartId;
                var colourId = userPartListItem.brickColourId;
                var qty = userPartListItem.quantity;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var (rbListId, partNum, rbColorId) = await ResolvePartListItemPushDataAsync(listId, partId, colourId, CancellationToken.None);
                        if (rbListId.HasValue && partNum != null && rbColorId.HasValue)
                            await syncService.PushPartListPartAddedAsync(tenantGuid, rbListId.Value, partNum, rbColorId.Value, qty);
                    }
                    catch (Exception ex) { _logger.LogWarning(ex, "Rebrickable push failed for UserPartListItem creation {Id}", userPartListItem.id); }
                });
            }

            return CreatedAtRoute("UserPartListItem", new { id = userPartListItem.id }, Database.UserPartListItem.CreateAnonymousWithFirstLevelSubObjects(userPartListItem));
        }


        [HttpDelete]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/UserPartListItem/{id}")]
        [Route("api/UserPartListItem")]
        public async Task<IActionResult> DeleteUserPartListItem(int id, CancellationToken cancellationToken = default)
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

            Database.UserPartListItem userPartListItem = await _context.UserPartListItems
                .Where(x => x.id == id && x.tenantGuid == userTenantGuid)
                .FirstOrDefaultAsync(cancellationToken);

            if (userPartListItem == null)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserPartListItem DELETE", id.ToString(), new Exception("No BMC.UserPartListItem entity could be find with the primary key provided."));
                return NotFound();
            }
            Database.UserPartListItem cloneOfExisting = (Database.UserPartListItem)_context.Entry(userPartListItem).GetDatabaseValues().ToObject();

            int listId = userPartListItem.userPartListId;
            int partId = userPartListItem.brickPartId;
            int colourId = userPartListItem.brickColourId;

            try
            {
                userPartListItem.deleted = true;
                await _context.SaveChangesAsync(cancellationToken);

                await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity, "BMC.UserPartListItem entity successfully deleted.", true, id.ToString(),
                    JsonSerializer.Serialize(Database.UserPartListItem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                    JsonSerializer.Serialize(Database.UserPartListItem.CreateAnonymousWithFirstLevelSubObjects(userPartListItem)), null);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity, "BMC.UserPartListItem entity delete failed.", false, id.ToString(),
                    JsonSerializer.Serialize(Database.UserPartListItem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                    JsonSerializer.Serialize(Database.UserPartListItem.CreateAnonymousWithFirstLevelSubObjects(userPartListItem)), ex);
                return Problem(ex.Message);
            }

            // Fire-and-forget push
            var syncService = SyncService;
            if (syncService != null)
            {
                var tenantGuid = userTenantGuid;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var (rbListId, partNum, rbColorId) = await ResolvePartListItemPushDataAsync(listId, partId, colourId, CancellationToken.None);
                        if (rbListId.HasValue && partNum != null && rbColorId.HasValue)
                            await syncService.PushPartListPartRemovedAsync(tenantGuid, rbListId.Value, partNum, rbColorId.Value);
                    }
                    catch (Exception ex) { _logger.LogWarning(ex, "Rebrickable push failed for UserPartListItem delete {Id}", id); }
                });
            }

            return Ok();
        }
    }
}
