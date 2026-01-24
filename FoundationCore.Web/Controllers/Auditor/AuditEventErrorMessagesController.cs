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
    public partial class AuditEventErrorMessagesController : SecureWebAPIController
    {
        //
        // Added 2 new join parameters
        //
        // GET: api/AuditEventErrorMessages
        [HttpGet]
        [Route("api/AuditEventErrorMessages")]
        public async Task<IActionResult> GetAuditEventErrorMessages(
            int? auditEventId = null,
            string errorMessage = null,
            DateTime? startTime = null,
            DateTime? stopTime = null,
            string user = null,
            string message = null,
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
            // Fix any UTC dates that come in.
            //
            if (startTime.HasValue == true && startTime.Value.Kind == DateTimeKind.Utc)
            {
                startTime = startTime.Value.ToLocalTime();
            }

            if (stopTime.HasValue == true && stopTime.Value.Kind == DateTimeKind.Utc)
            {
                stopTime = stopTime.Value.ToLocalTime();
            }

            //
            // If there is no start date, then only go back 180 days
            //
            if (startTime == null || startTime.HasValue == false)
            {
                if (stopTime != null && stopTime.HasValue == true)
                {
                    startTime = stopTime.Value.AddDays(-180);
                }
                else
                {
                    startTime = DateTime.UtcNow.AddDays(-180);
                }
            }
            

            var query = (from aeem in _context.AuditEventErrorMessages
                         join ae in _context.AuditEvents on aeem.auditEventId equals ae.id
                         join aeu in _context.AuditUsers on ae.auditUserId equals aeu.id
                         select aeem);

            if (auditEventId.HasValue == true)
            {
                query = query.Where(aeem => aeem.auditEventId == auditEventId.Value);
            }
            if (errorMessage != null)
            {
                query = query.Where(aeem => aeem.errorMessage == errorMessage);
            }

            query = query.OrderBy(aeem => aeem.id);

            if (pageNumber.HasValue == true &&
                pageSize.HasValue == true)
            {
                query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            if (includeRelations == true)
            {
                query = query.Include(x => x.auditEvent);
            }
            query = query.AsNoTracking();

            var materialized = await query.ToListAsync(cancellationToken);

            // Convert all the date properties to be of kind UTC.
            bool databaseStoresDateWithTimeZone = DoesDatabaseStoreDateWithTimeZone(_context);
            foreach (Database.AuditEventErrorMessage auditEventErrorMessage in materialized)
            {
                Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(auditEventErrorMessage, databaseStoresDateWithTimeZone);
            }

            await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Auditor.AuditEventErrorMessage Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Auditor.AuditEventErrorMessage Entity list was read.  Returning " + materialized.Count + " rows of data.");

            // Create a new output object that only includes the relations if necessary, and doesn't include the emmpty list objects, so that we can reduce the amount of data being transferred.
            if (includeRelations == true)
            {
                var reducedFieldOutput = (from materializedData in materialized
                                          select Database.AuditEventErrorMessage.CreateAnonymousWithFirstLevelSubObjects(materializedData)).ToList();

                return Ok(reducedFieldOutput);
            }
            else
            {
                var reducedFieldOutput = (from materializedData in materialized
                                          select Database.AuditEventErrorMessage.CreateAnonymous(materializedData)).ToList();

                return Ok(reducedFieldOutput);
            }
        }

    }
}
