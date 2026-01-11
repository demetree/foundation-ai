using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CsvHelper;
using MessagePack;
using Foundation.Auditor;
using Foundation.Scheduler.Database;
using Foundation.Security.Database;

using System.Threading;
using Foundation.Security;
using Foundation.Controllers;


namespace Foundation.Scheduler.Controllers.WebAPI
{
    [ApiController]
    public class DataController : SecureWebAPIController
    {
        private readonly SchedulerContext _db;           // Used for tenant data export to Excel




        public DataController(SchedulerContext db,
                              ILogger<DataController> logger,
                              IConfiguration config)
                              : base("Scheduler", "DataController")
        {
            _db = db;
        }


        /// <summary>
        /// 
        /// This produces an Excel document with a sheet for most tables in the database that contain all the table's rows.
        /// 
        /// This gets all of the data for the whole tenant, including all projects.
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Data/ExportDatabaseToExcel")]
        [RateLimit(RateLimitOption.OnePerMinute, Scope = RateLimitScope.PerUser)]
        public async Task<IActionResult> ExportDatabaseToExcel(CancellationToken cancellationToken = default)
        {
            SecurityUser securityUser = await GetSecurityUserAsync();

            bool userIsReader = await UserCanReadAsync(255);          // 255 is for the Foundation super admin user level.   Until we finish this feature, we're hiding it from regular users.

            if (userIsReader == false)
            {
                return Unauthorized();
            }

            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser.accountName, securityUser.accountName, ex);
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }

            try
            {
                //
                // Add in the names of the entities that we want fro the context.  This much mach the DBSet name.
                //

                //
                // List of all tenant-specific entities to export
                //
                // Order: masters to core to linking to children to history
                List<string> entitySetNames = new List<string>(){
                    
                    // Tenant-specific masters
                    "TenantProfiles",
                    "TimeZones",
                    "Currencies",
                    "ResourceTypes",
                    "Countries",
                    "StateProvinces",
                    "Qualifications",
                    "RateTypes",
                    "AssignmentRoles",
                    "AssignmentRoleQualificationRequirements",
                    "Priorities",

                    "RateSheets",

                    // Core operational entities
                    "Offices",
                    "SchedulingTargetTypes",
                    "SchedulingTargets",
                    "SchedulingTargetAddresses",
                    "SchedulingTargetQualificationRequirements",

                    "ShiftPatterns",
                    "ShiftPatternDays",


                    "Resources",
                    "ResourceQualifications",
                    "ResourceShifts",
                    "ResourceAvailabilities",
                    "NotificationSubscriptions",


                    //
                    // CRM
                    //
                    "ClientTypes",
                    "Clients",

                    "Contacts",
                    "ResourceContacts",
                    "ClientContacts",
                    "SchedulingTargetContacts",
                    "ContactContacts",
                    "ContactInteractions",
                    "Tags",

                    "Crews",
                    "CrewMembers",

                    "ScheduledEventTemplates",
                    "ScheduledEventTemplateQualificationRequirements",

                    "ScheduledEvents",
                    "ScheduledEventDependecies",
                    "RecurrenceExceptions",

                    "EventResourceAssignments",

                    "Calendars",
                    "EventCalendars",

                    //
                    // Foundation masters
                    //
                    "OfficeTypes",
                    "ContactTypes",
                    "RelationshipTypes",
                    "RecurrenceFrequencies",
                    "RecurrenceRules",

                    "ContactMethods",
                    "InteractionTypes",
                    "ContactTag",
                    "Salutations",
                    "Icon",
                    "BookingSourceTypes",
                    "DependencyTypes",
                    

                    
                    //
                    // Note this list is not comprehensive..  Need to add more in if this this to be enabled.
                    //
                    // History / Audit tables (optional — include if you want full audit trail)
                    //"TenantProfileChangeHistories",
                    //"ScheduledEventChangeHistories",
                    //"EventResourceAssignmentChangeHistories",
                    //"CrewChangeHistories",
                    //"CrewMemberChangeHistories",
                    //"ResourceChangeHistories",
                    //"ResourceQualificationChangeHistories",
                    //"ResourceAvailabilityChangeHistories",
                    //"ResourceShiftChangeHistories",
                    //"ShiftPatternChangeHistories",
                    //"ShiftPatternDayChangeHistories",
                    //"SchedulingTargetChangeHistories",
                    //"SchedulingTargetAddressChangeHistories",
                    //"SchedulingTargetQualificationRequirementChangeHistories",
                    //"ScheduledEventTemplateChangeHistories",
                    //"ScheduledEventTemplateQualificationRequirementChangeHistories",
                    //"AssignmentRoleQualificationRequirementChangeHistories",
                    //"CalendarChangeHistories"
                };



                //
                // Export to Excel, writing directly to the response body
                //
                Response.StatusCode = 200; // Explicitly set 200 OK

                // 
                // Set mime type used for Excel spreadsheets
                //
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";


                string fileName = $"SchedulerExport_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
                Response.Headers.TryAdd("Content-Disposition", $"attachment; filename=\"{fileName}\"");

                //
                // Use the foundation DB Context to Excel exporter to write the database to the response stream, for the provided entities.
                //
                // We need to use the database here rather than the lot processor, because we are writing multiple projects to the Excel sheet, because the sheet models the DB,
                // and handles all projects in the tenant.
                //
                // It will filter by tenant guid.
                //
                await Foundation.DbContextExcelExporter.ExportToExcelAsync(Response.Body, _db, userTenantGuid, entitySetNames, true, "tenantGuid", cancellationToken);

                await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, $"Tenant database exported to Excel");

                return new EmptyResult();

            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Failed to export database to Excel for tenant: {userTenantGuid}", securityUser.accountName, ex);

                return Problem($"An error occurred while exporting the database to Excel.  {ex.Message}");
            }
        }
    }
}
