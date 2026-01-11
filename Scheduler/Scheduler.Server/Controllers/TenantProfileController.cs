using Foundation.Controllers;
using Foundation.Auditor;
using Foundation.Scheduler.Database;
using Foundation.Security;
using Foundation.Security.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    public partial class TenantProfileController : SecureWebAPIController
    {

        public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
        public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

        private SchedulerContext db;

        public TenantProfileController(SchedulerContext db) : base("Scheduler", "TenantProfile")
        {
            this.db = db;
        }

        [Route("api/Tenant/GetProfile")]
        [RateLimit(RateLimitOption.TenPerMinute, Scope = RateLimitScope.PerUser)]
        [HttpGet]
        public async Task<IActionResult> GetTenantProfile()
        {
            StartAuditEventClock();

            if (!await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED))
            {
                return Forbid();
            }
            
            if (await IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel.Read) == false)
            {
                return Forbid();
            }
            
            
            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();

                Guid userTenantGuid;

                try
                {
                    userTenantGuid = await UserTenantGuidAsync(securityUser);
                }
                catch (Exception ex)
                {
                    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Tenant config issue.", securityUser.accountName, ex);
                    return StatusCode(500, "Tenant not configured.");
                }


                TenantProfile profile = await db.TenantProfiles
                    .Where(tp => tp.tenantGuid == userTenantGuid && tp.deleted == false)
                    .FirstOrDefaultAsync();


                //
                // Contingency : If we do not have a tenant profile record, then it is a tenant provisioning issue, and we will failsafe
                // it by creating one here.  Not ideal, but it is better than having no profile at all. 
                //
                if (profile == null)
                {
                    await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Tenant has no profile record.  Will attempt to auto create.", securityUser.accountName);

                    try
                    {
                        //
                        // If we do not have a tenant profile record, then it is a tenant provisioning issue, and we will failsafe it by 
                        // creating one here.
                        //
                        profile = new TenantProfile();

                        profile.name = "Edit in Company Profile";

                        profile.active = true;
                        profile.deleted = false;
                        profile.tenantGuid = userTenantGuid;
                        profile.objectGuid = Guid.NewGuid();

                        var chts = TenantProfile.GetChangeHistoryToolsetForWriting(db, securityUser);

                        await chts.SaveEntityAsync(profile);

                        return Ok(profile.ToDTO());
                    }
                    catch (Exception ex)
                    {
                        await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Could not automatically create tenant profile.", securityUser.accountName, ex);

                        return NotFound();
                    }
                }
                    
                return Ok(profile.ToDTO());
            }

            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error,
                    "Failed to fetch full tenant profile for user",
                    ex.ToString());
                return StatusCode(500, "Internal server error while retrieving tenant profile.");
            }
        }
    }
}
