using System;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Foundation.Auditor;
using Foundation.Security.Database;



namespace Foundation.Security.Controllers.WebAPI
{
    public partial class SecurityUserSecurityRolesController : SecureWebAPIController
    {
        //
        // These are just like the auto generated, but add a cache clear command of  
        //
        // SecurityFramework.ClearSecurityCaches();
        // SecurityLogic.ClearSecurityCaches();
        //
        // upon success
        //


        /* same as auto generated, but clears caches on save complete */
        // PUT: api/UserSecurityRoles/1
        [HttpPut]
        [HttpPost]
        [Route("api/SecurityUserSecurityRole/{id}")]
        public async Task<IActionResult> PutSecurityUserSecurityRole(int id, Database.SecurityUserSecurityRole.SecurityUserSecurityRoleDTO securityUserSecurityRoleDTO)
        {

            StartAuditEventClock();

            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Unauthorized();
            }


            if (await IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel.Write) == false)
            {
                return Unauthorized();
            }


            if (id != securityUserSecurityRoleDTO.id)
            {
                return BadRequest();
            }

            SecurityUser securityUser = await GetSecurityUserAsync();

            bool userIsWriter = await UserCanWriteAsync(securityUser, 0);
            bool userIsAdmin = await UserCanAdministerAsync(securityUser);
            IQueryable<Database.SecurityUserSecurityRole> query = (from x in _context.SecurityUserSecurityRoles
                                                                   where
                                                                   (x.id == id)
                                                                   select x);


            Database.SecurityUserSecurityRole existing = await query.FirstOrDefaultAsync();

            if (existing == null)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityUserSecurityRole PUT", id.ToString(), new Exception("No Security.SecurityUserSecurityRole entity could be found with the primary key provided."));
                return NotFound();
            }

            // Copy the existing object so it can be serialized as-is in the audit and history logs.
            Database.SecurityUserSecurityRole cloneOfExisting = (Database.SecurityUserSecurityRole)_context.Entry(existing).GetDatabaseValues().ToObject();

            //
            // Create a new account object using the data from the existing record, updated with what is in the DTO.
            //
            Database.SecurityUserSecurityRole securityUserSecurityRole = (Database.SecurityUserSecurityRole)_context.Entry(existing).GetDatabaseValues().ToObject();
            securityUserSecurityRole.ApplyDTO(securityUserSecurityRoleDTO);

            // Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
            if (userIsAdmin == false && (securityUserSecurityRole.deleted == true || existing.deleted == true))
            {
                // we're not recording state here because it is not being changed.
                CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.SecurityUserSecurityRole record.", id.ToString());
                DestroySessionAndAuthentication();
                return Unauthorized();
            }

            if (securityUserSecurityRole.comments != null && securityUserSecurityRole.comments.Length > 1000)
            {
                securityUserSecurityRole.comments = securityUserSecurityRole.comments.Substring(0, 1000);
            }

            var attached = _context.Entry(existing);
            attached.CurrentValues.SetValues(securityUserSecurityRole);

            try
            {
                await _context.SaveChangesAsync();

                /* start of custom bit */
                //
                // Clear the security caches after modifying users
                //
                SecurityFramework.ClearSecurityCaches();
                SecurityLogic.ClearSecurityCaches();
                /* end of custom bit */

                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                    "Security.SecurityUserSecurityRole entity successfully updated.",
                    true,
                    id.ToString(),
                    JsonSerializer.Serialize(Database.SecurityUserSecurityRole.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                    JsonSerializer.Serialize(Database.SecurityUserSecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityUserSecurityRole)),
                    null);


                return Ok(Database.SecurityUserSecurityRole.CreateAnonymous(securityUserSecurityRole));
            }
            catch (Exception ex)
            {
                CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
                    "Security.SecurityUserSecurityRole entity update failed",
                    false,
                    id.ToString(),
                    JsonSerializer.Serialize(Database.SecurityUserSecurityRole.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                    JsonSerializer.Serialize(Database.SecurityUserSecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityUserSecurityRole)),
                    ex);
                throw;
            }

        }

        /* same as auto generated, but clears caches on save complete */
        // POST: api/UserSecurityRole
        [HttpPost]
        [Route("api/SecurityUserSecurityRole", Name = "SecurityUserSecurityRole")]
        [Route("api/Surface/SecurityUserSecurityRole")]
        public async Task<IActionResult> PostSecurityUserSecurityRole([FromBody ]Database.SecurityUserSecurityRole.SecurityUserSecurityRoleDTO securityUserSecurityRoleDTO)
        {
            StartAuditEventClock();

            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Unauthorized();
            }


            if (await IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel.Write) == false)
            {
                return Unauthorized();
            }


            SecurityUser securityUser = await GetSecurityUserAsync();

            //
            // Create a new SecurityUserSecurityRole object using the data from the DTO
            //
            Database.SecurityUserSecurityRole securityUserSecurityRole = Database.SecurityUserSecurityRole.FromDTO(securityUserSecurityRoleDTO);

            try
            {
                if (securityUserSecurityRole.comments != null && securityUserSecurityRole.comments.Length > 1000)
                {
                    securityUserSecurityRole.comments = securityUserSecurityRole.comments.Substring(0, 1000);
                }

                _context.SecurityUserSecurityRoles.Add(securityUserSecurityRole);
                await _context.SaveChangesAsync();

                /* start of custom bit */
                //
                // Clear the security caches after modifying users
                //
                SecurityFramework.ClearSecurityCaches();
                SecurityLogic.ClearSecurityCaches();
                /* end of custom bit */

                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                    "Security.SecurityUserSecurityRole entity successfully created.",
                    true,
                    securityUserSecurityRole.id.ToString(),
                    "",
                    JsonSerializer.Serialize(Database.SecurityUserSecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityUserSecurityRole)),
                    null);

            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.SecurityUserSecurityRole entity creation failed.", false, securityUserSecurityRole.id.ToString(), "", JsonSerializer.Serialize(securityUserSecurityRole), ex);
                throw;
            }


            BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityUserSecurityRole", securityUserSecurityRole.id, securityUserSecurityRole.comments));

            return CreatedAtRoute("SecurityUserSecurityRole", new { id = securityUserSecurityRole.id }, SecurityUserSecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityUserSecurityRole));
        }


        [HttpDelete]
        [Route("api/SecurityUserSecurityRole/{id}")]
        [Route("api/SecurityUserSecurityRole")]
        public async Task<IActionResult> DeleteSecurityUserSecurityRole(int id)
        {
            StartAuditEventClock();

            if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync() == false)
            {
                return Unauthorized();
            }

            if (await IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel.Write) == false)
            {
                return Unauthorized();
            }

            SecurityUser securityUser = await GetSecurityUserAsync();

            IQueryable<Database.SecurityUserSecurityRole> query = (from x in _context.SecurityUserSecurityRoles
                                                                   where
                                                                   (x.id == id)
                                                                   select x);


            Database.SecurityUserSecurityRole securityUserSecurityRole = await query.FirstOrDefaultAsync();

            if (securityUserSecurityRole == null)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityUserSecurityRole DELETE", id.ToString(), new Exception("No Security.SecurityUserSecurityRole entity could be find with the primary key provided."));
                return NotFound();
            }
            Database.SecurityUserSecurityRole cloneOfExisting = (Database.SecurityUserSecurityRole)_context.Entry(securityUserSecurityRole).GetDatabaseValues().ToObject();


            try
            {
                securityUserSecurityRole.deleted = true;
                await _context.SaveChangesAsync();

                /* start of custom bit */
                //
                // Clear the security caches after modifying users
                //
                SecurityFramework.ClearSecurityCaches();
                SecurityLogic.ClearSecurityCaches();
                /* end of custom bit */


                await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
                    "Security.SecurityUserSecurityRole entity successfully deleted.",
                    true,
                    id.ToString(),
                    JsonSerializer.Serialize(Database.SecurityUserSecurityRole.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                    JsonSerializer.Serialize(Database.SecurityUserSecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityUserSecurityRole)),
                    null);

            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
                    "Security.SecurityUserSecurityRole entity delete failed.",
                    false,
                    id.ToString(),
                    JsonSerializer.Serialize(Database.SecurityUserSecurityRole.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                    JsonSerializer.Serialize(Database.SecurityUserSecurityRole.CreateAnonymousWithFirstLevelSubObjects(securityUserSecurityRole)),
                    ex);
                throw;
            }
            return Ok();
        }

    }
}
