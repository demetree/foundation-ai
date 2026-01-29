using Foundation.Auditor.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Foundation.Security.Controllers.WebAPI
{
    public class SecurityAuditEventsController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
        public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

        //
        // This is a minimal copy implementation of the Auditor.AuditEvent WebAPI class.  It's here in the security module to allow the users screen to read the audit events for a user.
        //
        private const int DEFAULT_ALL_DATA_LIST_PAGE_SIZE = 10000;

        public SecurityAuditEventsController() : base("Security", "SecurityAuditEvent")
        {
            return;
        }

        //
        // This class is basically wrapping a read only list.  No changes to it allowed, so I've deleted the update and delete and add methods.
        //
        private readonly AuditorContext db = new AuditorContext();

        // GET: api/SecurityAuditEvents
        public async Task<IActionResult> GetSecurityAuditEvents(DateTime? startTime = null,
                                               DateTime? stopTime = null,
                                               string userName = null,
                                               int? completedSuccessfully = null,
                                               int? auditUserId = null,
                                               int? auditSessionId = null,
                                               int? auditAccessTypeId = null,
                                               int? auditHostSystemId = null,
                                               int? auditTypeId = null,
                                               int? auditModuleId = null,
                                               int? auditModuleEntityId = null,
                                               int? auditResourceId = null,
                                               int? auditSourceId = null,
                                               int? auditUserAgentId = null,
                                               string primaryKey = null,
                                               string message = null,
                                               string source = null,
                                               string userAgent = null,
                                               string session = null,
                                               string resource = null,
                                               int pageSize = DEFAULT_ALL_DATA_LIST_PAGE_SIZE,
                                               int pageNumber = 1,
                                               bool includeRelations = true,
                                               CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Unauthorized();
            }

            bool userIsAdmin = await UserCanAdministerAsync(cancellationToken);


            //
            // This little pass through directly the auditevens table must be constrained by a user name, as it's only expected to be used wihtin the User detail screen in the context of one user.
            //
            if (string.IsNullOrEmpty(userName) == true)
            {
                throw new Exception("SecurityAuditEvent requests must provide a user name");
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
            // If there is no start date, then only go back 30 days
            //
            if (startTime == null || startTime.HasValue == false)
            {
                if (stopTime != null && stopTime.HasValue == true)
                {
                    startTime = stopTime.Value.AddDays(-30);
                }
                else
                {
                    startTime = DateTime.UtcNow.AddDays(-30);
                }
            }

            //
            // Consider -1 to be a null.
            //
            if (auditTypeId.HasValue == true && auditTypeId.Value == -1)
            {
                auditTypeId = null;
            }

            if (auditModuleId.HasValue == true && auditModuleId.Value == -1)
            {
                auditModuleId = null;
            }


            if (auditModuleEntityId.HasValue == true && auditModuleEntityId.Value == -1)
            {
                auditModuleEntityId = null;
            }

            // unless other params are added, don't allow more than 90 days worth of logs to be retrieved at once

            DateTime endDateToCheckFor;
            if (stopTime.HasValue == true)
            {
                endDateToCheckFor = stopTime.Value;
            }
            else
            {
                endDateToCheckFor = DateTime.UtcNow;
            }

            //
            // Unless a user is being checked for, only get a year's worth of data to try to avoid massive data pulls
            //
            if ((userName == null) &&
                ((endDateToCheckFor - startTime.Value).TotalDays > 365))
            {
                startTime = endDateToCheckFor.AddDays(-365);
            }

            //List<AuditEvent> output = (from ae in db.AuditEvents
            //                           join src in db.AuditSources on ae.auditSourceId equals src.id
            //                           join agent in db.AuditUserAgents on ae.auditUserAgentId equals agent.id
            //                           join user in db.AuditUsers on ae.auditUserId equals user.id
            //                           where (startDate.HasValue == false || ae.startTime >= startDate) &&
            //                            (endDate.HasValue == false || ae.startTime <= endDate) &&
            //                            (userId.HasValue == false || ae.auditUserId == userId.Value) &&
            //                            (userName == null || user.name.ToUpper().Contains(userName.ToUpper())) &&
            //                            (primaryKey == null || ae.primaryKey == primaryKey) &&
            //                            (accessTypeId.HasValue == false || ae.auditAccessTypeId == accessTypeId.Value) &&
            //                            (hostSystemId.HasValue == false || ae.auditHostSystemId == hostSystemId.Value) &&
            //                            (typeId.HasValue == false || ae.auditTypeId == typeId.Value) &&
            //                            (moduleId.HasValue == false || ae.auditModuleId == moduleId.Value) &&
            //                            (moduleEntityId.HasValue == false || ae.auditModuleEntityId == moduleEntityId.Value) &&
            //                            (resourceId.HasValue == false || ae.auditResourceId == resourceId.Value) &&
            //                            (sourceId.HasValue == false || ae.auditSourceId == sourceId.Value) &&
            //                            (message == null || ae.message.ToUpper().Contains(message.ToUpper())) &&
            //                            (source == null || src.name.ToUpper().Contains(source.ToUpper())) &&
            //                            (userAgent == null || agent.name.ToUpper().Contains(userAgent.ToUpper()))
            //                           orderby ae.id descending         // so the newest ones show up first
            //                           select ae)
            //                            .AsNoTracking()
            //                            .ToList();    // The AsNoTracking really speeds this up as it doesn't bring in the child relationships (that I am surprised were actually brought in with no includes ) // https://stackoverflow.com/questions/58035040/why-ef-core-loads-child-entities-when-i-query-it-without-include



            //
            // To put in the string contains filters that reach into linked tables, I need to build it into the main query syntax because I can't selectively add where clauses against fields not selected (can't select because they're on other tables )
            // 
            IQueryable<AuditEvent> query = null;

            //
            // Unless we get specific strings for the subtables as filters, keep the query as simple as possible
            //
            if (userName == null && session == null && message == null && source == null && resource == null && userAgent == null)
            {
                //
                // No special params.  Use the simplest query possible.
                //
                query = (from ae in db.AuditEvents select ae);
            }
            else if (userName != null && session == null && message == null && source == null && resource == null && userAgent == null)
            {
                //
                // No special params.  Use the simplest query possible.
                //
                query = (from ae in db.AuditEvents
                         join user in db.AuditUsers on ae.auditUserId equals user.id
                         where user.name.ToUpper().Contains(userName.ToUpper())
                         select ae);
            }
            else
            {
                query = (from ae in db.AuditEvents
                         join src in db.AuditSources on ae.auditSourceId equals src.id
                         join agent in db.AuditUserAgents on ae.auditUserAgentId equals agent.id
                         join user in db.AuditUsers on ae.auditUserId equals user.id
                         join _session in db.AuditSessions on ae.auditSessionId equals _session.id
                         join rsrc in db.AuditResources on ae.auditResourceId equals rsrc.id
                         where
                        (userName == null || user.name.ToUpper().Contains(userName.ToUpper())) &&
                        (session == null || _session.name.ToUpper().Contains(session.ToUpper())) &&
                        (message == null || ae.message.ToUpper().Contains(message.ToUpper())) &&
                        (source == null || src.name.ToUpper().Contains(source.ToUpper())) &&
                        (resource == null || rsrc.name.ToUpper().Contains(resource.ToUpper())) &&
                        (userAgent == null || agent.name.ToUpper().Contains(userAgent.ToUpper()))
                         select ae);
            }

            if (startTime.HasValue == true)
            {
                query = query.Where(ae => ae.startTime >= startTime.Value);
            }
            if (stopTime.HasValue == true)
            {
                query = query.Where(ae => ae.stopTime <= stopTime.Value);
            }
            if (completedSuccessfully.HasValue == true)
            {
                query = query.Where(ae => ae.completedSuccessfully == false);
            }
            if (auditUserId.HasValue == true)
            {
                query = query.Where(ae => ae.auditUserId == auditUserId.Value);
            }
            if (auditSessionId.HasValue == true)
            {
                query = query.Where(ae => ae.auditSessionId == auditSessionId.Value);
            }
            if (auditTypeId.HasValue == true)
            {
                query = query.Where(ae => ae.auditTypeId == auditTypeId.Value);
            }
            if (auditAccessTypeId.HasValue == true)
            {
                query = query.Where(ae => ae.auditAccessTypeId == auditAccessTypeId.Value);
            }
            if (auditSourceId.HasValue == true)
            {
                query = query.Where(ae => ae.auditSourceId == auditSourceId.Value);
            }
            if (auditUserAgentId.HasValue == true)
            {
                query = query.Where(ae => ae.auditUserAgentId == auditUserAgentId.Value);
            }
            if (auditModuleId.HasValue == true)
            {
                query = query.Where(ae => ae.auditModuleId == auditModuleId.Value);
            }
            if (auditModuleEntityId.HasValue == true)
            {
                query = query.Where(ae => ae.auditModuleEntityId == auditModuleEntityId.Value);
            }
            if (auditResourceId.HasValue == true)
            {
                query = query.Where(ae => ae.auditResourceId == auditResourceId.Value);
            }
            if (auditHostSystemId.HasValue == true)
            {
                query = query.Where(ae => ae.auditHostSystemId == auditHostSystemId.Value);
            }
            if (primaryKey != null)
            {
                query = query.Where(ae => ae.primaryKey == primaryKey);
            }
            if (message != null)
            {
                query = query.Where(ae => ae.message == message);
            }

            query = query.OrderByDescending(ae => ae.id);

            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            query = query.AsNoTracking();

            var materialized = await query.ToListAsync(cancellationToken);

            if (includeRelations == true)
            {

                //
                // Rebuild the data that the includes didn't get because the performance was terrible when using them
                //
                Dictionary<int, AuditAccessType> auditAccessTypes = await (from x in db.AuditAccessTypes select x).AsNoTracking().ToDictionaryAsync(x => x.id, x => x);
                Dictionary<int, AuditModule> auditModules = await (from x in db.AuditModules select x).AsNoTracking().ToDictionaryAsync(x => x.id, x => x);
                Dictionary<int, AuditModuleEntity> auditModuleEntities = await (from x in db.AuditModuleEntities select x).AsNoTracking().ToDictionaryAsync(x => x.id, x => x);
                Dictionary<int, AuditResource> auditResources = await (from x in db.AuditResources select x).AsNoTracking().ToDictionaryAsync(x => x.id, x => x);
                Dictionary<int, AuditUser> auditUsers = await (from x in db.AuditUsers select x).AsNoTracking().ToDictionaryAsync(x => x.id, x => x);
                Dictionary<int, AuditSession> auditSessions = await (from x in db.AuditSessions select x).AsNoTracking().ToDictionaryAsync(x => x.id, x => x);
                Dictionary<int, AuditSource> auditSources = await (from x in db.AuditSources select x).AsNoTracking().ToDictionaryAsync(x => x.id, x => x);
                Dictionary<int, AuditType> auditTypes = await (from x in db.AuditTypes select x).AsNoTracking().ToDictionaryAsync(x => x.id, x => x);
                Dictionary<int, AuditHostSystem> auditHostSystems = await (from x in db.AuditHostSystems select x).AsNoTracking().ToDictionaryAsync(x => x.id, x => x);
                Dictionary<int, AuditUserAgent> auditUserAgents = await (from x in db.AuditUserAgents select x).AsNoTracking().ToDictionaryAsync(x => x.id, x => x);

                foreach (AuditEvent ae in materialized)
                {
                    ae.auditAccessType = auditAccessTypes[ae.auditAccessTypeId];
                    ae.auditModule = auditModules[ae.auditModuleId];
                    ae.auditModuleEntity = auditModuleEntities[ae.auditModuleEntityId];
                    ae.auditResource = auditResources[ae.auditResourceId];
                    ae.auditUser = auditUsers[ae.auditUserId];
                    ae.auditSession = auditSessions[ae.auditSessionId];
                    ae.auditSource = auditSources[ae.auditSourceId];
                    ae.auditType = auditTypes[ae.auditTypeId];
                    ae.auditHostSystem = auditHostSystems[ae.auditHostSystemId];
                    ae.auditUserAgent = auditUserAgents[ae.auditUserAgentId];
                }
            }


            // Convert all the date properties to be of kind UTC.
            bool databaseStoresDateWithTimeZone = DoesDatabaseStoreDateWithTimeZone(db);
            foreach (AuditEvent auditEvent in materialized)
            {
                Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(auditEvent, databaseStoresDateWithTimeZone);
            }

            await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Entity list was read with Admin privilege" : "Entity list was read");

            return Ok(materialized);
        }

    }
}