using Foundation.Security;
using Foundation.Security.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.Auditor.Controllers.WebAPI
{
    //
    // Added 4 new join parameters
    //
    public partial class AuditEventEntityStatesController : SecureWebAPIController
    {

        [HttpGet]
        [Route("api/AuditEventEntityStates")]
        public async Task<IActionResult> GetAuditEventEntityStates(
            int? auditEventId = null,
            string beforeState = null,
            string afterState = null,
            DateTime? startTime = null,
            DateTime? stopTime = null,
            string userId = null,
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

            // New parameters are added here.
            var query = (from aees in _context.AuditEventEntityStates
                         join ae in _context.AuditEvents on aees.auditEventId equals ae.id
                         join aeu in _context.AuditUsers on ae.auditUserId equals aeu.id
                         where
                         (startTime.HasValue == false || ae.startTime >= startTime) &&
                         (stopTime.HasValue == false || ae.startTime <= stopTime) &&
                         (userId == null || ae.auditUser.name.ToUpper().Contains(userId.ToUpper())) &&
                         (userId == null || aeu.name.ToUpper().Contains(userId.ToUpper())) &&
                         (message == null || ae.message.ToUpper().Contains(message.ToUpper()))
                         select aees);

            if (auditEventId.HasValue == true)
            {
                query = query.Where(aees => aees.auditEventId == auditEventId.Value);
            }
            if (string.IsNullOrEmpty(beforeState) == false)
            {
                query = query.Where(aees => aees.beforeState == beforeState);
            }
            if (string.IsNullOrEmpty(afterState) == false)
            {
                query = query.Where(aees => aees.afterState == afterState);
            }

            query = query.OrderBy(aees => aees.id);

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

            List<Database.AuditEventEntityState> materialized = await query.ToListAsync(cancellationToken);


            // Convert all the date properties to be of kind UTC.
            bool databaseStoresDateWithTimeZone = DoesDatabaseStoreDateWithTimeZone(_context);
            foreach (Database.AuditEventEntityState auditEventEntityState in materialized)
            {
                Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(auditEventEntityState, databaseStoresDateWithTimeZone);
            }


            await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Auditor.AuditEventEntityState Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Auditor.AuditEventEntityState Entity list was read.  Returning " + materialized.Count + " rows of data.");

            // Create a new output object that only includes the relations if necessary, and doesn't include the emmpty list objects, so that we can reduce the amount of data being transferred.
            if (includeRelations == true)
            {
                var reducedFieldOutput = (from materializedData in materialized
                                          select Database.AuditEventEntityState.CreateAnonymousWithFirstLevelSubObjects(materializedData)).ToList();

                return Ok(reducedFieldOutput);
            }
            else
            {
                var reducedFieldOutput = (from materializedData in materialized
                                          select Database.AuditEventEntityState.CreateAnonymous(materializedData)).ToList();

                return Ok(reducedFieldOutput);
            }
        }

    }
}
