using Foundation.Auditor.Database;
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
    public partial class AuditEventsController : SecureWebAPIController
    {

        // GET: api/AuditEvents
        //
        // This is the custom version with higher performance and custom filters to suit the nature of the audit event data better than the automatically generated version.
        //
        // Also has custom log flush logic to handle one of the audit logging background modes
        //
        [HttpGet]
        [Route("api/AuditEvents")]
        public async Task<IActionResult> GetAuditEvents(string userName = null,
                                                string session = null,
                                                string source = null,
                                                string resource = null,
                                                string userAgent = null,
                                                DateTime? startTime = null,
                                                DateTime? stopTime = null,
                                                bool? completedSuccessfully = null,
                                                int? auditUserId = null,
                                                int? auditSessionId = null,
                                                int? auditTypeId = null,
                                                int? auditAccessTypeId = null,
                                                int? auditSourceId = null,
                                                int? auditUserAgentId = null,
                                                int? auditModuleId = null,
                                                int? auditModuleEntityId = null,
                                                int? auditResourceId = null,
                                                int? auditHostSystemId = null,
                                                string primaryKey = null,
                                                int? threadId = null,
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


            //
            // If the Auditor Mode is memory queue, then force the flush so that we can see the data right away
            //
            if (AuditEngine.Instance.GetAuditorMode() == AuditEngine.AuditorMode.MemoryQueueWithOneMinuteFlush)
            {
                // this will flush the queue and wait till it is done before continuing
                AuditEngine.FlushMemoryQueueToDatabase();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

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

            //
            // Consider -1 to be a null.
            //
            if (auditUserId.HasValue == true && auditUserId.Value == -1)
            {
                auditUserId = null;
            }

            if (auditSessionId.HasValue == true && auditSessionId.Value == -1)
            {
                auditSessionId = null;
            }

            if (auditTypeId.HasValue == true && auditTypeId.Value == -1)
            {
                auditTypeId = null;
            }

            if (auditAccessTypeId.HasValue == true && auditAccessTypeId.Value == -1)
            {
                auditAccessTypeId = null;
            }

            if (auditSourceId.HasValue == true && auditSourceId.Value == -1)
            {
                auditSourceId = null;
            }

            if (auditUserAgentId.HasValue == true && auditUserAgentId.Value == -1)
            {
                auditUserAgentId = null;
            }

            if (auditModuleId.HasValue == true && auditModuleId.Value == -1)
            {
                auditModuleId = null;
            }

            if (auditModuleEntityId.HasValue == true && auditModuleEntityId.Value == -1)
            {
                auditModuleEntityId = null;
            }

            if (auditHostSystemId.HasValue == true && auditHostSystemId.Value == -1)
            {
                auditHostSystemId = null;
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

            //
            // Unless a user is being checked for, only get a year's worth of data to try to avoid massive data pulls
            //
            if ((userName == null) &&
                ((stopTimeToCheckFor - startTime.Value).TotalDays > 365))
            {
                startTime = stopTimeToCheckFor.AddDays(-365);
            }

            //List<AuditEvent> output = (from ae in _context.AuditEvents
            //                           join src in _context.AuditSources on ae.auditSourceId equals src.id
            //                           join agent in _context.AuditUserAgents on ae.auditUserAgentId equals agent.id
            //                           join user in _context.AuditUsers on ae.auditUserId equals user.id
            //                           join _session in _context.AuditSessions on ae.auditSessionId equals _session.id
            //                           join rsrc in _context.AuditResources on ae.auditResourceId equals rsrc.id
            //                           where
            //                            (id.HasValue == false || ae.id == id.Value) &&
            //                            (userName == null || user.name.ToUpper().Contains(userName.ToUpper())) &&
            //                            (session == null || _session.name.ToUpper().Contains(session.ToUpper())) &&
            //                            (message == null || ae.message.ToUpper().Contains(message.ToUpper())) &&
            //                            (source == null || src.name.ToUpper().Contains(source.ToUpper())) &&
            //                            (resource == null || rsrc.name.ToUpper().Contains(resource.ToUpper())) &&
            //                            (userAgent == null || agent.name.ToUpper().Contains(userAgent.ToUpper())) &&
            //                            (startTime.HasValue == false || ae.startTime >= startTime) &&
            //                            (stopTime.HasValue == false || ae.startTime <= stopTime) &&
            //                            (primaryKey == null || ae.primaryKey == primaryKey) &&
            //                            (completedSuccessfully.HasValue == false || ae.completedSuccessfully == (completedSuccessfully.Value == 1 ? true : false)) &&
            //                            (auditUserId.HasValue == false || ae.auditUserId == auditUserId.Value) &&
            //                            (auditSessionId.HasValue == false || ae.auditSessionId == auditSessionId.Value) &&
            //                            (auditTypeId.HasValue == false || ae.auditTypeId == auditTypeId.Value) &&
            //                            (auditAccessTypeId.HasValue == false || ae.auditAccessTypeId == auditAccessTypeId.Value) &&
            //                            (auditSourceId.HasValue == false || ae.auditSourceId == auditSourceId.Value) &&
            //                            (auditUserAgentId.HasValue == false || ae.auditUserAgentId == auditUserAgentId.Value) &&
            //                            (auditModuleId.HasValue == false || ae.auditModuleId == auditModuleId.Value) &&
            //                            (auditModuleEntityId.HasValue == false || ae.auditModuleEntityId == auditModuleEntityId.Value) &&
            //                            (auditResourceId.HasValue == false || ae.auditResourceId == auditResourceId.Value) &&
            //                            (auditHostSystemId.HasValue == false || ae.auditHostSystemId == auditHostSystemId.Value) 
            //                           orderby ae.id descending         // so the newest ones show up first
            //                           select ae)
            //                            .AsNoTracking() // The AsNoTracking really speeds this up as it doesn't bring in the child relationships (that I am surprised were actually brought in with no includes ) // https://stackoverflow.com/questions/58035040/why-ef-core-loads-child-entities-when-i-query-it-without-include
            //                            .ToList();    

            //
            // To put in the string contains filters that reach into linked tables, I need to build it into the main query syntax because I can't selectively add where clauses against fields not selected (can't select because they're on other tables )
            // 
            IQueryable<AuditEvent> query = null;

            //
            // Unless we get specific strings for the sub tables as filters, keep the query as simple as possible
            //
            if (userName == null && session == null && message == null && source == null && resource == null && userAgent == null)
            {
                //
                // No special params.  Use the simplest query possible.
                //
                query = (from ae in _context.AuditEvents select ae);
            }
            else
            {
                // 
                // Any one of the 'sub table string contains' parameters will use the same conditional query with the joins to the link tables
                //
                query = (from ae in _context.AuditEvents
                         join src in _context.AuditSources on ae.auditSourceId equals src.id
                         join agent in _context.AuditUserAgents on ae.auditUserAgentId equals agent.id
                         join user in _context.AuditUsers on ae.auditUserId equals user.id
                         join _session in _context.AuditSessions on ae.auditSessionId equals _session.id
                         join rsrc in _context.AuditResources on ae.auditResourceId equals rsrc.id
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
                query = query.Where(ae => ae.completedSuccessfully == completedSuccessfully.Value);
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
            if (threadId.HasValue == true)
            {
                query = query.Where(ae => ae.threadId == threadId.Value);
            }

            query = query.OrderByDescending(ae => ae.id);

            if (pageNumber.HasValue == true &&
                pageSize.HasValue == true)
            {
                query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            query = query.AsNoTracking();

            var materialized = await query.ToListAsync(cancellationToken);

            if (includeRelations == true)
            {
                //
                // Rebuild the data that the includes didn't get because the performance was terrible when using them
                //
                Dictionary<int, AuditAccessType> auditAccessTypes = await (from x in _context.AuditAccessTypes select x).AsNoTracking().ToDictionaryAsync(x => x.id, x => x, cancellationToken);
                Dictionary<int, AuditModule> auditModules = await (from x in _context.AuditModules select x).AsNoTracking().ToDictionaryAsync(x => x.id, x => x, cancellationToken);
                Dictionary<int, AuditModuleEntity> auditModuleEntities = await (from x in _context.AuditModuleEntities select x).AsNoTracking().ToDictionaryAsync(x => x.id, x => x, cancellationToken);
                Dictionary<int, AuditResource> auditResources = await (from x in _context.AuditResources select x).AsNoTracking().ToDictionaryAsync(x => x.id, x => x, cancellationToken);
                Dictionary<int, AuditUser> auditUsers = await (from x in _context.AuditUsers select x).AsNoTracking().ToDictionaryAsync(x => x.id, x => x, cancellationToken);
                Dictionary<int, AuditSession> auditSessions = await (from x in _context.AuditSessions select x).AsNoTracking().ToDictionaryAsync(x => x.id, x => x, cancellationToken);
                Dictionary<int, AuditSource> auditSources = await (from x in _context.AuditSources select x).AsNoTracking().ToDictionaryAsync(x => x.id, x => x, cancellationToken);
                Dictionary<int, AuditType> auditTypes = await (from x in _context.AuditTypes select x).AsNoTracking().ToDictionaryAsync(x => x.id, x => x, cancellationToken);
                Dictionary<int, AuditHostSystem> auditHostSystems = await (from x in _context.AuditHostSystems select x).AsNoTracking().ToDictionaryAsync(x => x.id, x => x, cancellationToken);
                Dictionary<int, AuditUserAgent> auditUserAgents = await (from x in _context.AuditUserAgents select x).AsNoTracking().ToDictionaryAsync(x => x.id, x => x, cancellationToken);

                foreach (AuditEvent ae in materialized)
                {
                    if (auditAccessTypes.ContainsKey(ae.auditAccessTypeId))
                    {
                        ae.auditAccessType = auditAccessTypes[ae.auditAccessTypeId];
                    }

                    if (auditModules.ContainsKey(ae.auditModuleId))
                    {
                        ae.auditModule = auditModules[ae.auditModuleId];
                    }

                    if (auditModuleEntities.ContainsKey(ae.auditModuleEntityId))
                    {
                        ae.auditModuleEntity = auditModuleEntities[ae.auditModuleEntityId];
                    }


                    if (auditResources.ContainsKey(ae.auditResourceId))
                    {
                        ae.auditResource = auditResources[ae.auditResourceId];
                    }

                    if (auditUsers.ContainsKey(ae.auditUserId))
                    {
                        ae.auditUser = auditUsers[ae.auditUserId];
                    }

                    if (auditSessions.ContainsKey(ae.auditSessionId))
                    {
                        ae.auditSession = auditSessions[ae.auditSessionId];
                    }

                    if (auditSources.ContainsKey(ae.auditSourceId))
                    {
                        ae.auditSource = auditSources[ae.auditSourceId];
                    }

                    if (auditTypes.ContainsKey(ae.auditTypeId))
                    {
                        ae.auditType = auditTypes[ae.auditTypeId];
                    }

                    if (auditHostSystems.ContainsKey(ae.auditHostSystemId))
                    {
                        ae.auditHostSystem = auditHostSystems[ae.auditHostSystemId];
                    }

                    if (auditUserAgents.ContainsKey(ae.auditUserAgentId))
                    {
                        ae.auditUserAgent = auditUserAgents[ae.auditUserAgentId];
                    }
                }
            }


            //
            // SQL Server DateTime fields do not store the UTC flag on the date, but SQLite does.
            // This affects the way that the resultant DateTime objects are 'kinded', and that varies between reads from SQL Server and SQLite.
            // This means that SQL Server DATETIME fields don't store thir original time zone nor the UTC identifier, and come back with a local date kind.
            // SQLite stores UTC dates as ISO date strings, including the Z suffix, so they are read into local time, after converting from the UTC stored time if there is a Z suffix..
            // 
            // We want all date/time fields to serialize out as UTC dates.
            // 
            bool databaseStoresDateWithTimeZone = DoesDatabaseStoreDateWithTimeZone(_context);

            foreach (AuditEvent auditEvent in materialized)
            {
                Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(auditEvent, databaseStoresDateWithTimeZone);
            }

            await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Entity list was read with Admin privilege" : "Entity list was read");

            // Create a new output object that only includes the relations if necessary, and doesn't include the emmpty list objects, so that we can reduce the amount of data being transferred.
            if (includeRelations == true)
            {
                var reducedFieldOutput = (from materializedData in materialized
                                          select Database.AuditEvent.CreateAnonymousWithFirstLevelSubObjects(materializedData)).ToList();

                return Ok(reducedFieldOutput);
            }
            else
            {
                var reducedFieldOutput = (from materializedData in materialized
                                          select Database.AuditEvent.CreateAnonymous(materializedData)).ToList();

                return Ok(reducedFieldOutput);
            }
        }


        //
        // This is to be used to create arbitrary audit events from client side code.  Only requirement is that there is a user logged in.
        //
        [Route("api/AuditEvents/CreateEvent")]
        [HttpPost]
        public IActionResult CreateEvent(string message, bool success = true)
        {
            Security.Database.SecurityUser user = GetSecurityUser();

            if (user == null)
            {
                return BadRequest();
            }

            CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, message, success);

            return Ok();
        }

    }
}