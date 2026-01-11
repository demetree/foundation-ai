using Foundation.Security;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Foundation.Auditor.Controllers.WebAPI
{
    public class AuditEventPurgeController : SecureWebAPIController
    {
        public AuditEventPurgeController() : base("Auditor", "AuditEventPurge")
        {
            return;
        }


        [Route("api/AuditEventPurge/PurgeAuditEvents")]
        [HttpGet]                                               // make this a get so we can easily invoke it from a browser.
        public async Task<IActionResult> PurgeAuditEvents(int? daysToKeep = 0)
        {
            StartAuditEventClock();

            if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync() == false)
            {
                return Unauthorized();
            }

            IActionResult output;

            if (daysToKeep > 0)
            {
                try
                {
                    AuditEngine.PurgeAuditEvents(daysToKeep.Value);

                    CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "Audit Events Successfully Purged.  Kept " + daysToKeep.Value.ToString() + " days worth of audit logs", true, null, null, null, null);

                    output = Ok();
                }
                catch (Exception ex)
                {
                    CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, "Audit Events Purge Encountered error.  Days to keep was " + daysToKeep.Value.ToString(), false, null, null, null, ex);

                    output = Problem(ex.ToString());
                }
            }
            else
            {
                output = BadRequest("No days to keep provided.");
            }

            return output;
        }
    }
}