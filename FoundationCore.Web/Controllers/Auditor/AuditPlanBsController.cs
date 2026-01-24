using Foundation.Security;
using Foundation.Security.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.Auditor.Controllers.WebAPI
{
    public partial class AuditPlanBsController : SecureWebAPIController
    {
        //
        // This custom version adds some date range security, and changes the == to be contains for all the strings
        //
        // GET: api/AuditPlanBs
        [HttpGet]
        [Route("api/AuditPlanBs")]
        public async Task<IActionResult> GetAuditPlanBs(
            int? id = null,
            DateTime? startTime = null,
            DateTime? stopTime = null,
            int? completedSuccessfully = null,
            string user = null,
            string session = null,
            string type = null,
            string accessType = null,
            string source = null,
            string userAgent = null,
            string module = null,
            string moduleEntity = null,
            string resource = null,
            string hostSystem = null,
            string primaryKey = null,
            string message = null,
            string beforeState = null,
            string afterState = null,
            string errorMessage = null,
            string exceptionText = null,
            int? pageSize = null,
            int? pageNumber = null,
            bool includeRelations = true,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Unauthorized();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

            bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
            bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

            if (pageNumber.HasValue == true &&
                pageNumber < 1)
            {
                pageNumber = null;
            }

            if (pageSize.HasValue == true &&
                pageSize <= 0)
            {
                pageSize = null;
            }


            //
            // Turn any local time parameters to UTC.
            //
            if (startTime.HasValue == true && startTime.Value.Kind != DateTimeKind.Utc)
            {
                startTime = startTime.Value.ToUniversalTime();
            }

            if (stopTime.HasValue == true && stopTime.Value.Kind != DateTimeKind.Utc)
            {
                stopTime = stopTime.Value.ToUniversalTime();
            }


            //
            // If there is no start date, then only go back 90 days
            //
            if (startTime == null || startTime.HasValue == false)
            {
                if (stopTime != null && stopTime.HasValue == true)
                {
                    startTime = stopTime.Value.AddDays(-90);
                }
                else
                {
                    startTime = DateTime.UtcNow.AddDays(-90);
                }
            }

            DateTime stopTimeToCheckFor;
            if (stopTime.HasValue == true)
            {
                stopTimeToCheckFor = stopTime.Value;
            }
            else
            {
                stopTimeToCheckFor = DateTime.UtcNow;
            }

            // unless other params are added, don't allow more than 365 days worth of logs to be retrieved at once
            if (user == null &&
                ((stopTimeToCheckFor - startTime.Value).TotalDays > 365))
            {
                startTime = stopTimeToCheckFor.AddDays(-365);
            }


            var query = (from apb in _context.AuditPlanBs select apb);
            if (startTime.HasValue == true)
            {
                query = query.Where(apb => apb.startTime >= startTime.Value);
            }
            if (stopTime.HasValue == true)
            {
                query = query.Where(apb => apb.stopTime <= stopTime.Value);
            }
            if (completedSuccessfully.HasValue == true)
            {
                query = query.Where(apb => apb.completedSuccessfully == false);
            }
            if (user != null)
            {
                query = query.Where(apb => apb.user.Contains(user));
            }
            if (session != null)
            {
                query = query.Where(apb => apb.session.Contains(session));
            }
            if (type != null)
            {
                query = query.Where(apb => apb.type.Contains(type));
            }
            if (accessType != null)
            {
                query = query.Where(apb => apb.accessType.Contains(accessType));
            }
            if (source != null)
            {
                query = query.Where(apb => apb.source.Contains(source));
            }
            if (userAgent != null)
            {
                query = query.Where(apb => apb.userAgent.Contains(userAgent));
            }
            if (module != null)
            {
                query = query.Where(apb => apb.module.Contains(module));
            }
            if (moduleEntity != null)
            {
                query = query.Where(apb => apb.moduleEntity.Contains(moduleEntity));
            }
            if (resource != null)
            {
                query = query.Where(apb => apb.resource.Contains(resource));
            }
            if (hostSystem != null)
            {
                query = query.Where(apb => apb.hostSystem.Contains(hostSystem));
            }
            if (primaryKey != null)
            {
                query = query.Where(apb => apb.primaryKey == primaryKey);
            }
            if (message != null)
            {
                query = query.Where(apb => apb.message.Contains(message));
            }
            if (beforeState != null)
            {
                query = query.Where(apb => apb.beforeState.Contains(beforeState));
            }
            if (afterState != null)
            {
                query = query.Where(apb => apb.afterState.Contains(afterState));
            }
            if (errorMessage != null)
            {
                query = query.Where(apb => apb.errorMessage.Contains(errorMessage));
            }
            if (exceptionText != null)
            {
                query = query.Where(apb => apb.exceptionText.Contains(exceptionText));
            }

            query = query.OrderByDescending(apb => apb.id);

            if (pageNumber.HasValue == true &&
                            pageSize.HasValue == true)
            {
                query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            if (includeRelations == true)
            {
            }
            query = query.AsNoTracking();

            var materialized = await query.ToListAsync(cancellationToken);


            // Convert all the date properties to be of kind UTC.
            bool databaseStoresDateWithTimeZone = DoesDatabaseStoreDateWithTimeZone(_context);
            foreach (Database.AuditPlanB auditPlanB in materialized)
            {
                Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(auditPlanB, databaseStoresDateWithTimeZone);
            }


            await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Auditor.AuditPlanB Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Auditor.AuditPlanB Entity list was read.  Returning " + materialized.Count + " rows of data.");

            // Create a new output object that only includes the relations if necessary, and doesn't include the emmpty list objects, so that we can reduce the amount of data being transferred.
            if (includeRelations == true)
            {
                var reducedFieldOutput = (from materializedData in materialized
                                          select Database.AuditPlanB.CreateAnonymousWithFirstLevelSubObjects(materializedData)).ToList();

                return Ok(reducedFieldOutput);
            }
            else
            {
                var reducedFieldOutput = (from materializedData in materialized
                                          select Database.AuditPlanB.CreateAnonymous(materializedData)).ToList();

                return Ok(reducedFieldOutput);
            }
        }
    }
}
