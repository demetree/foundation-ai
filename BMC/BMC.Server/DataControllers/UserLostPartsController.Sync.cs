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
    /// Partial class for UserLostPartsController with Rebrickable push hooks.
    /// Uses rebrickableInvPartId to call PushLostPartAddedAsync/PushLostPartRemovedAsync.
    /// </summary>
    public partial class UserLostPartsController
    {
        private RebrickableSyncService SyncService =>
            HttpContext.RequestServices.GetService<RebrickableSyncService>();


        [Route("api/UserLostPart/{id}")]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [HttpPost]
        [HttpPut]
        public async Task<IActionResult> PutUserLostPart(int id, [FromBody]Database.UserLostPart.UserLostPartDTO userLostPartDTO, CancellationToken cancellationToken = default)
        {
            if (userLostPartDTO == null) return BadRequest();
            StartAuditEventClock();

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
                return Forbid();

            if (id != userLostPartDTO.id) return BadRequest();

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

            Database.UserLostPart existing = await _context.UserLostParts
                .Where(x => x.id == id && x.tenantGuid == userTenantGuid)
                .FirstOrDefaultAsync(cancellationToken);

            if (existing == null)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserLostPart PUT", id.ToString(), new Exception("No BMC.UserLostPart entity could be found with the primary key provided."));
                return NotFound();
            }

            if (userLostPartDTO.objectGuid == Guid.Empty) { userLostPartDTO.objectGuid = existing.objectGuid; }
            else if (userLostPartDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a UserLostPart record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }

            Database.UserLostPart cloneOfExisting = (Database.UserLostPart)_context.Entry(existing).GetDatabaseValues().ToObject();
            Database.UserLostPart userLostPart = (Database.UserLostPart)_context.Entry(existing).GetDatabaseValues().ToObject();
            userLostPart.ApplyDTO(userLostPartDTO);

            if (existing.tenantGuid != userTenantGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
                return Problem("Data integrity violation detected while attempting to save.");
            }
            else { userLostPart.tenantGuid = existing.tenantGuid; }

            if (userIsAdmin == false && (userLostPart.deleted == true || existing.deleted == true))
            {
                CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.UserLostPart record.", id.ToString());
                DestroySessionAndAuthentication();
                return Forbid();
            }

            EntityEntry<Database.UserLostPart> attached = _context.Entry(existing);
            attached.CurrentValues.SetValues(userLostPart);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "BMC.UserLostPart entity successfully updated.", true, id.ToString(),
                    JsonSerializer.Serialize(Database.UserLostPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                    JsonSerializer.Serialize(Database.UserLostPart.CreateAnonymousWithFirstLevelSubObjects(userLostPart)), null);

                // Fire-and-forget push — lost part updates are re-pushes (add with new quantity)
                if (userLostPart.rebrickableInvPartId.HasValue)
                {
                    var syncService = SyncService;
                    if (syncService != null)
                    {
                        var tenantGuid = userTenantGuid;
                        var invPartId = userLostPart.rebrickableInvPartId.Value;
                        var qty = userLostPart.lostQuantity;
                        _ = Task.Run(async () =>
                        {
                            try { await syncService.PushLostPartAddedAsync(tenantGuid, invPartId, qty); }
                            catch (Exception ex) { _logger.LogWarning(ex, "Rebrickable push failed for UserLostPart update {Id}", id); }
                        });
                    }
                }

                return Ok(Database.UserLostPart.CreateAnonymous(userLostPart));
            }
            catch (Exception ex)
            {
                CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "BMC.UserLostPart entity update failed", false, id.ToString(),
                    JsonSerializer.Serialize(Database.UserLostPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                    JsonSerializer.Serialize(Database.UserLostPart.CreateAnonymousWithFirstLevelSubObjects(userLostPart)), ex);
                return Problem(ex.Message);
            }
        }


        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/UserLostPart", Name = "UserLostPart")]
        public async Task<IActionResult> PostUserLostPart([FromBody]Database.UserLostPart.UserLostPartDTO userLostPartDTO, CancellationToken cancellationToken = default)
        {
            if (userLostPartDTO == null) return BadRequest();
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

            Database.UserLostPart userLostPart = Database.UserLostPart.FromDTO(userLostPartDTO);

            try
            {
                userLostPart.tenantGuid = userTenantGuid;
                userLostPart.objectGuid = Guid.NewGuid();
                _context.UserLostParts.Add(userLostPart);
                await _context.SaveChangesAsync(cancellationToken);

                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserLostPart entity successfully created.", true, userLostPart.id.ToString(), "",
                    JsonSerializer.Serialize(Database.UserLostPart.CreateAnonymousWithFirstLevelSubObjects(userLostPart)), null);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.UserLostPart entity creation failed.", false, userLostPart.id.ToString(), "",
                    JsonSerializer.Serialize(userLostPart), ex);
                return Problem(ex.Message);
            }

            BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "UserLostPart", userLostPart.id, userLostPart.id.ToString()));

            // Fire-and-forget push
            if (userLostPart.rebrickableInvPartId.HasValue)
            {
                var syncService = SyncService;
                if (syncService != null)
                {
                    var tenantGuid = userTenantGuid;
                    var invPartId = userLostPart.rebrickableInvPartId.Value;
                    var qty = userLostPart.lostQuantity;
                    _ = Task.Run(async () =>
                    {
                        try { await syncService.PushLostPartAddedAsync(tenantGuid, invPartId, qty); }
                        catch (Exception ex) { _logger.LogWarning(ex, "Rebrickable push failed for UserLostPart creation {Id}", userLostPart.id); }
                    });
                }
            }

            return CreatedAtRoute("UserLostPart", new { id = userLostPart.id }, Database.UserLostPart.CreateAnonymousWithFirstLevelSubObjects(userLostPart));
        }


        [HttpDelete]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/UserLostPart/{id}")]
        [Route("api/UserLostPart")]
        public async Task<IActionResult> DeleteUserLostPart(int id, CancellationToken cancellationToken = default)
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

            Database.UserLostPart userLostPart = await _context.UserLostParts
                .Where(x => x.id == id && x.tenantGuid == userTenantGuid)
                .FirstOrDefaultAsync(cancellationToken);

            if (userLostPart == null)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.UserLostPart DELETE", id.ToString(), new Exception("No BMC.UserLostPart entity could be find with the primary key provided."));
                return NotFound();
            }
            Database.UserLostPart cloneOfExisting = (Database.UserLostPart)_context.Entry(userLostPart).GetDatabaseValues().ToObject();

            int? rebrickableInvPartId = userLostPart.rebrickableInvPartId;

            try
            {
                userLostPart.deleted = true;
                await _context.SaveChangesAsync(cancellationToken);

                await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity, "BMC.UserLostPart entity successfully deleted.", true, id.ToString(),
                    JsonSerializer.Serialize(Database.UserLostPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                    JsonSerializer.Serialize(Database.UserLostPart.CreateAnonymousWithFirstLevelSubObjects(userLostPart)), null);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity, "BMC.UserLostPart entity delete failed.", false, id.ToString(),
                    JsonSerializer.Serialize(Database.UserLostPart.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                    JsonSerializer.Serialize(Database.UserLostPart.CreateAnonymousWithFirstLevelSubObjects(userLostPart)), ex);
                return Problem(ex.Message);
            }

            // Fire-and-forget push
            if (rebrickableInvPartId.HasValue)
            {
                var syncService = SyncService;
                if (syncService != null)
                {
                    var tenantGuid = userTenantGuid;
                    var invPartId = rebrickableInvPartId.Value;
                    _ = Task.Run(async () =>
                    {
                        try { await syncService.PushLostPartRemovedAsync(tenantGuid, invPartId); }
                        catch (Exception ex) { _logger.LogWarning(ex, "Rebrickable push failed for UserLostPart delete {Id}", id); }
                    });
                }
            }

            return Ok();
        }
    }
}
