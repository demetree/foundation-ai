using System;
using static CodeGenerationCommon.Utility;

namespace Foundation.CodeGeneration
{
    public partial class WebAPICodeGeneratorCore : CodeGenerationBase
    {

        public static string BuildWebAPIBaseClassImplementationFromEntityFrameworkContext(string moduleName, int defaultPageSize, Type contextType, DatabaseGenerator.Database database)
        {
            //
            // This base class is very standard between projects, so we can use one big string and replace a few tokens.  
            //
            // Because it is a very large and reasonably complex file, it is easier to have one master template for it that works in .Net Framework and .Net Core, so this
            // does that.  It uses a few extension methods to reach this goal to make the Context class compatible between the platforms.
            //
            // I have considered making this a template class, but it becomes tough because there will need to be a base class or an interface that contains the 
            // standard module side data visibility tables, which complicates all the linq expressions, as well as context class setup so this approach is likely the best.
            //
            // Pattern to update this generator is this:
            //
            // 1.) Make updates to the <Module>BaseWebApiController.cs in code in real module to add or fix features.
            // 2.) Copy that work into a temp file, and find and replace the actual module name with  %MODULENAME%
            // 3.) Paste that into here.
            // 4.) Ensure that the code works in .Net Framework and .Net Core
            //
            //
            string webApiBaseClassCode = @"using System;
using System.Collections.Generic;
using System.Threading;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
" +

((IsNetCore() == true) ?
@"using Microsoft.EntityFrameworkCore;" : "") +

((IsNetFramework48() == true) ? "" +
@"using System.Data.Entity;" : "") +

@"using Foundation.Security;
using Foundation.%MODULENAME%.Database;
using Foundation.Security.Database;
using static Foundation.DatabaseUtility.DatabaseExtensions;     // Bridge extension utility to support cross platform Contexts.

namespace Foundation.%MODULENAME%.Controllers.WebAPI
{
    public abstract class %MODULENAME%BaseWebAPIController : SecureWebAPIController
    {
        public %MODULENAME%BaseWebAPIController(string moduleName, string entityName) : base(moduleName, entityName)
        {

        }
" + ((IsNetFramework48() == true) ? @"
        protected %MODULENAME%Context _context = new %MODULENAME%Context();
" : @"
        protected %MODULENAME%Context _context;       // Assigned through Dependency injection in implementations
") +
@"
        
        protected const int DEFAULT_ALL_DATA_LIST_PAGE_SIZE = %DEFAULTPAGESIZE%;
        protected const int DEFAULT_NAME_VALUE_PAIR_LIST_PAGE_SIZE = %DEFAULTPAGESIZE%;

        private static object fullSynchronizationSyncRoot = new object();
        private static object userSynchronizationSyncRoot = new object();

        private const string LAST_SYNC_SETTING = ""%MODULENAME%_DataVisibility_LastSync"";
        private const int SYNC_PAGE_SIZE = 10000;
        private const int SYNC_COMMAND_TIMEOUT = 60000;


        protected User GetUser(Guid tenantGuid, SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            User user = (from u in _context.Users
                         where
                         u.active == true &&
                         u.deleted == false &&
                         u.objectGuid == securityUser.objectGuid &&
                         u.tenantGuid == tenantGuid
                         select u)
                            .FirstOrDefault();

            return user;
        }


        protected async Task<User> GetUserAsync(SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync(cancellationToken);
            }

            Guid tenantGuid = await UserTenantGuidAsync(securityUser);

            User user = await (from u in _context.Users
                               where
                               u.active == true &&
                               u.deleted == false &&
                               u.objectGuid == securityUser.objectGuid &&
                               u.tenantGuid == tenantGuid
                               select u)
                             .FirstOrDefaultAsync(cancellationToken);

            return user;
        }


        protected async Task<User> GetUserAsync(Guid tenantGuid, SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync(cancellationToken);
            }

            User user = await (from u in _context.Users
                         where
                         u.active == true &&
                         u.deleted == false &&
                         u.objectGuid == securityUser.objectGuid &&
                         u.tenantGuid == tenantGuid
                         select u)
                         .FirstOrDefaultAsync(cancellationToken);

            return user;
        }


        protected async Task<List<int>> GetListOfUserIdsForUserAndPeopleThatReportToThemAsync(SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync(cancellationToken);
            }

            List<User> userAndTheirReports = await GetListOfUsersForUserAndPeopleThatReportToThemAsync(securityUser, cancellationToken);

            if (userAndTheirReports != null && userAndTheirReports.Count > 0)
            {
                return (from x in userAndTheirReports select x.id).ToList();
            }
            else
            {
                return new List<int>();
            }
        }


        protected List<int> GetListOfUserIdsForUserAndPeopleThatReportToThem(SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<User> userAndTheirReports = GetListOfUsersForUserAndPeopleThatReportToThem(securityUser);

            if (userAndTheirReports != null && userAndTheirReports.Count > 0)
            {
                return (from x in userAndTheirReports select x.id).ToList();
            }
            else
            {
                return new List<int>();
            }
        }
    

        protected async Task<List<User>> GetListOfUsersForUserAndPeopleThatReportToThemAsync(SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync(cancellationToken);
            }

            User user = await (from u in _context.Users
                         where
                         u.active == true &&
                         u.deleted == false &&
                         u.objectGuid == securityUser.objectGuid
                         select u)
                        .FirstOrDefaultAsync(cancellationToken);

            if (user != null)
            {
                return await GetUserReportsAsync(user, null, cancellationToken);
            }
            else
            {
                return null;
            }
        }


        protected List<User> GetListOfUsersForUserAndPeopleThatReportToThem(SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            User user = (from u in _context.Users
                         where
                         u.active == true &&
                         u.deleted == false &&
                         u.objectGuid == securityUser.objectGuid
                         select u)
                        .FirstOrDefault();

            if (user != null)
            {
                return GetUserReports(user);
            }
            else
            {
                return null;
            }
        }


        protected async Task<List<User>> GetUserReportsAsync(User user, List<int> reportingStructureMembers = null, CancellationToken cancellationToken = default)
        {
            //
            // If hierarchy is null, then we're at the first request.
            //
            if (reportingStructureMembers == null)
            {
                reportingStructureMembers = new List<int>();

                reportingStructureMembers.Add(user.id);
            }

            List<User> output = new List<User>();

            //
            // Put the provided user into the output list
            //
            output.Add(user);


            //
            // See if the user has any reports.  If they do, then add them, and find their reports.
            //
            List<User> directReports = await (from x in _context.Users
                                        where
                                        x.reportsToUserId == user.id &&
                                        x.active == true &&
                                        x.deleted == false
                                        select x)
                                        .ToListAsync(cancellationToken);

            foreach (User directReport in directReports)
            {
                //
                // This is to stop circular references that could theoretically exist in the table if some body does something dumb.
                //
                if (reportingStructureMembers.Contains(directReport.id) == false)
                {
                    reportingStructureMembers.Add(directReport.id);

                    List<User> peopleThatReportToThisReport = await GetUserReportsAsync(directReport, reportingStructureMembers);

                    if (peopleThatReportToThisReport != null && peopleThatReportToThisReport.Count > 0)
                    {
                        output.AddRange(peopleThatReportToThisReport);
                    }
                }
            }

            return output;
        }


        protected List<User> GetUserReports(User user, List<int> reportingStructureMembers = null)
        {
            //
            // If hierarchy is null, then we're at the first request.
            //
            if (reportingStructureMembers == null)
            {
                reportingStructureMembers = new List<int>();

                reportingStructureMembers.Add(user.id);
            }

            List<User> output = new List<User>();

            //
            // Put the provided user into the output list
            //
            output.Add(user);


            //
            // See if the user has any reports.  If they do, then add them, and find their reports.
            //
            List<User> directReports = (from x in _context.Users
                                        where
                                        x.reportsToUserId == user.id &&
                                        x.active == true &&
                                        x.deleted == false
                                        select x).ToList();

            foreach (User directReport in directReports)
            {
                //
                // This is to stop circular references that could theoretically exist in the table if some body does something dumb.
                //
                if (reportingStructureMembers.Contains(directReport.id) == false)
                {
                    reportingStructureMembers.Add(directReport.id);

                    List<User> peopleThatReportToThisReport = GetUserReports(directReport, reportingStructureMembers);

                    if (peopleThatReportToThisReport != null && peopleThatReportToThisReport.Count > 0)
                    {
                        output.AddRange(peopleThatReportToThisReport);
                    }
                }
            }

            return output;
        }


        protected List<Organization> GetOrganizationsUserIsEntitledToReadAnythingAtAllFrom(SecurityUser securityUser = null, bool userIsAdmin = false)
        {
            //
            // The purpose of this is to get all organizations that a user has any level of readability from, either directly at the org level,
            // or implicitly/partially through a dep or a team that the user can read from, to populate drop down lists with.
            //
            // Organizations with explicit read refusal are excluded, as are implicit team links via departments that have explicit read refusal
            //
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<Organization> output = null;

            if (userIsAdmin == true)
            {
                //
                // Admins can read all
                // 
                output = (from
                          o in _context.Organizations
                          where
                          o.active == true &&
                          o.deleted == false
                          select o)
                        .ToList();
            }
            else
            {
                //
                // Non-admins will have explicitly assigned privilege
                //
                output = new List<Organization>();

                //
                // Read all data, then split into readable and unreadable lists
                // 
                List<OrganizationUser> ouList = (from
                        o in _context.Organizations
                          join ou in _context.OrganizationUsers on o.id equals ou.organizationId
                          join u in _context.Users on ou.userId equals u.id
                          where
                          o.active == true &&
                          o.deleted == false &&
                          ou.active == true &&
                          ou.deleted == false &&
                          u.active == true &&
                          u.deleted == false &&
                          u.objectGuid == securityUser.objectGuid
                          select ou)
                        .ToList();

                List<int> explicitlyReadableOrgIds = (from ou in ouList where ou.canRead == true select ou.organizationId).ToList();
                List<int> explicitlyUnreadableOrgIds = (from ou in ouList where ou.canRead != true select ou.organizationId).ToList();

                // Start the output with the explicitly readable organizations.
                output = (from o in _context.Organizations where explicitlyReadableOrgIds.Contains(o.id) select o).ToList();

                //
                // Get the organizations that have implicit readership via an organization's department, when there is no clear rule on this user's access to the organization
                //
                // Exclude any organization with an explicit rule from consideration
                //
                List<Organization> organizationsReadableViaDepartmentReadership = (from
                                                                                   o in _context.Organizations
                                                                                      join d in _context.Departments on o.id equals d.organizationId
                                                                                      join du in _context.DepartmentUsers on d.id equals du.departmentId
                                                                                      join u in _context.Users on du.userId equals u.id
                                                                                      where
                                                                                      explicitlyReadableOrgIds.Contains(o.id) == false &&           // Can't be one we already have
                                                                                      explicitlyUnreadableOrgIds.Contains(o.id) == false &&         // Can't be one we aren't allowed to read from
                                                                                      o.active == true &&
                                                                                      o.deleted == false &&
                                                                                      d.active == true &&
                                                                                      d.deleted == false &&
                                                                                      du.active == true &&
                                                                                      du.deleted == false &&
                                                                                      u.active == true &&
                                                                                      u.deleted == false &&
                                                                                      u.objectGuid == securityUser.objectGuid &&
                                                                                      du.canRead == true
                                                                                      select o)
                                                                                   .ToList();

                if (organizationsReadableViaDepartmentReadership != null && organizationsReadableViaDepartmentReadership.Count > 0)
                {
                    output.AddRange(organizationsReadableViaDepartmentReadership);

                    explicitlyReadableOrgIds = (from x in output select x.id).ToList();
                }

                //
                // Get the organizations that have implicit readership via a department's team, when there is no clear rule on this user's access to the organization
                //
                // Exclude any organization with an explicit rule from consideration.  Remove links via explicitly unreadable departments
                //
                List<int> explicitlyUnreadableDepartmentIds = (from du in _context.DepartmentUsers
                                                             join u in _context.Users on du.userId equals u.id
                                                             where u.objectGuid == securityUser.objectGuid &&
                                                             du.canRead != true
                                                             select du.departmentId).ToList();


                // Add organizations that are implicitly readable via teams readership, except through unreadable departments
                List<Organization> organizationsReadableViaTeamReadership = (from
                                                                             o in _context.Organizations
                                                                                join d in _context.Departments on o.id equals d.organizationId
                                                                                join t in _context.Teams on d.id equals t.departmentId
                                                                                join tu in _context.TeamUsers on t.id equals tu.teamId
                                                                                join u in _context.Users on tu.userId equals u.id
                                                                                where
                                                                                explicitlyReadableOrgIds.Contains(o.id) == false &&             // Can't be one we already have
                                                                                explicitlyUnreadableOrgIds.Contains(o.id) == false &&           // Can't be one we aren't allowed to read from
                                                                                explicitlyUnreadableDepartmentIds.Contains(d.id) == false &&    // Can't get implicit access via a department that we aren't allowed to read from
                                                                                o.active == true &&
                                                                                o.deleted == false &&
                                                                                d.active == true &&
                                                                                d.deleted == false &&
                                                                                t.active == true &&
                                                                                t.deleted == false &&
                                                                                tu.active == true &&
                                                                                tu.deleted == false &&
                                                                                u.active == true &&
                                                                                u.deleted == false &&
                                                                                u.objectGuid == securityUser.objectGuid &&
                                                                                tu.canRead == true
                                                                                select o)
                                                                             .ToList();


                if (organizationsReadableViaTeamReadership != null && organizationsReadableViaTeamReadership.Count > 0)
                {
                    output.AddRange(organizationsReadableViaTeamReadership);
                }
            }

            if (output == null)
            {
                output = new List<Organization>();
            }

            return (from x in output orderby x.name select x).ToList();
        }


        protected List<Department> GetDepartmentsUserIsEntitledToReadAnythingAtAllFrom(SecurityUser securityUser = null, bool userIsAdmin = false)
        {
            //
            // The purpose of this is to get all departments that a user has any level of readability from, either directly at the dep level,
            // or implicitly/partially through an org or a team that the user can read from, to populate drop down lists with.
            //
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<Department> output = null;

            if (userIsAdmin == true)
            {
                //
                // Admins can read all
                // 
                output = (from
                          d in _context.Departments
                          where
                          d.active == true &&
                          d.deleted == false
                          select d)
                        .ToList();
            }
            else
            {
                //
                // Non-admins will have explicitly assigned privilege
                //
                output = new List<Department>();

                //
                // Read all data, then split into readable and unreadable lists
                // 
                List<DepartmentUser> duList = (from
                          d in _context.Departments
                          join du in _context.DepartmentUsers on d.id equals du.departmentId
                          join u in _context.Users on du.userId equals u.id
                          where
                          d.active == true &&
                          d.deleted == false &&
                          du.active == true &&
                          du.deleted == false &&
                          u.active == true &&
                          u.deleted == false &&
                          u.objectGuid == securityUser.objectGuid
                          select du)
                        .ToList();


                List<int> explicitlyReadableDepIds = (from du in duList where du.canRead == true select du.departmentId).ToList();
                List<int> explicitlyUnreadableDepIds = (from du in duList where du.canRead != true select du.departmentId).ToList();

                // Start the output with the explicitly readable departments.
                output = (from d in _context.Departments where explicitlyReadableDepIds.Contains(d.id) select d).ToList();

                // Add departments that are implicitly readable via organization readership
                List<Department> departmentsReadableViaOrganizationReadership = (from
                                                                                 d in _context.Departments
                                                                                 join o in _context.Organizations on d.organizationId equals o.id
                                                                                 join ou in _context.OrganizationUsers on o.id equals ou.organizationId
                                                                                 join u in _context.Users on ou.userId equals u.id
                                                                                 where
                                                                                 explicitlyReadableDepIds.Contains(d.id) == false &&        // Can't be one we already have
                                                                                 explicitlyUnreadableDepIds.Contains(d.id) == false &&      // Can't be one we aren't allowed to read from
                                                                                 o.active == true &&
                                                                                 o.deleted == false &&
                                                                                 d.active == true &&
                                                                                 d.deleted == false &&
                                                                                 ou.active == true &&
                                                                                 ou.deleted == false &&
                                                                                 u.active == true &&
                                                                                 u.deleted == false &&
                                                                                 u.objectGuid == securityUser.objectGuid &&
                                                                                 ou.canRead == true
                                                                                 select d)
                                                                                 .ToList();

                if (departmentsReadableViaOrganizationReadership != null && departmentsReadableViaOrganizationReadership.Count > 0)
                {
                    output.AddRange(departmentsReadableViaOrganizationReadership);

                    explicitlyReadableDepIds = (from x in output select x.id).ToList();
                }


                // Add departments that are implicitly readable via teams readership
                List<Department> departmentsReadableViaTeamReadership = (from
                                                                         d in _context.Departments
                                                                         join t in _context.Teams on d.id equals t.departmentId
                                                                         join tu in _context.TeamUsers on t.id equals tu.teamId
                                                                         join u in _context.Users on tu.userId equals u.id
                                                                         where
                                                                         explicitlyReadableDepIds.Contains(d.id) == false &&            // Can't be one we already have
                                                                         explicitlyUnreadableDepIds.Contains(d.id) == false &&          // Can't be one we aren't allowed to read from
                                                                         d.active == true &&
                                                                         d.deleted == false &&
                                                                         t.active == true &&
                                                                         t.deleted == false &&
                                                                         tu.active == true &&
                                                                         tu.deleted == false &&
                                                                         u.active == true &&
                                                                         u.deleted == false &&
                                                                         u.objectGuid == securityUser.objectGuid &&
                                                                         tu.canRead == true
                                                                         select d)
                                                                         .ToList();


                if (departmentsReadableViaTeamReadership != null && departmentsReadableViaTeamReadership.Count > 0)
                {
                    output.AddRange(departmentsReadableViaTeamReadership);
                }
            }

            if (output == null)
            {
                output = new List<Department>();
            }

            return (from x in output orderby x.name select x).ToList();
        }


        protected List<Team> GetTeamsUserIsEntitledToReadAnythingAtAllFrom(SecurityUser securityUser = null, bool userIsAdmin = false)
        {
            //
            // The purpose of this is to get all teams that a user has any level of readability from, either directly at the team level, or implicitly/partially through an org or a dep that the user can read from, to populate drop down lists with.
            //
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<Team> output = null;

            if (userIsAdmin == true)
            {
                //
                // Admins can read all
                // 
                output = (from
                          t in _context.Teams
                          where
                          t.active == true &&
                          t.deleted == false
                          select t)
                        .ToList();
            }
            else
            {
                //
                // Non-admins will have explicitly assigned privilege
                //
                output = new List<Team>();

                //
                // Read all data, then split into readable and unreadable lists
                // 
                List<TeamUser> tuList = (from
                          t in _context.Teams
                          join tu in _context.TeamUsers on t.id equals tu.teamId
                          join u in _context.Users on tu.userId equals u.id
                          where
                          t.active == true &&
                          t.deleted == false &&
                          tu.active == true &&
                          tu.deleted == false &&
                          u.active == true &&
                          u.deleted == false &&
                          u.objectGuid == securityUser.objectGuid
                          select tu)
                        .ToList();

                List<int> explicitlyReadableTeamIds = (from tu in tuList where tu.canRead == true select tu.teamId).ToList();
                List<int> explicitlyUnreadableTeamIds = (from tu in tuList where tu.canRead != true select tu.teamId).ToList();

                // Start the output with the explicitly readable teams.
                output = (from t in _context.Teams where explicitlyReadableTeamIds.Contains(t.id) select t).ToList();

              
                //
                // Add teams with implied access via explicitly readable departments
                //
                List<Team> teamsReadableViaDepartmentReadership = (from
                                                                   t in _context.Teams
                                                                   join d in _context.Departments on t.departmentId equals d.id
                                                                   join du in _context.DepartmentUsers on d.id equals du.departmentId
                                                                   join u in _context.Users on du.userId equals u.id
                                                                   where
                                                                   explicitlyReadableTeamIds.Contains(t.id) == false &&         // Can't be one we already have
                                                                   explicitlyUnreadableTeamIds.Contains(t.id) == false &&       // Can't be one we aren't allowed to read from
                                                                   d.active == true &&
                                                                   d.deleted == false &&
                                                                   t.active == true &&
                                                                   t.deleted == false &&
                                                                   du.active == true &&
                                                                   du.deleted == false &&
                                                                   u.active == true &&
                                                                   u.deleted == false &&
                                                                   u.objectGuid == securityUser.objectGuid &&
                                                                   du.canRead == true
                                                                   select t)
                                                                    .ToList();


                if (teamsReadableViaDepartmentReadership != null && teamsReadableViaDepartmentReadership.Count > 0)
                {
                    output.AddRange(teamsReadableViaDepartmentReadership);
                }


                //
                // Get the teams implicitly visible through organization readership
                //
                // Remove links via explicitly unreadable departments
                //
                List<int> explicitlyUnreadableDepartmentIds = (from du in _context.DepartmentUsers
                                                               join u in _context.Users on du.userId equals u.id
                                                               where u.objectGuid == securityUser.objectGuid &&
                                                               du.canRead != true
                                                               select du.departmentId).ToList();

                List<Team> teamsReadableViaOrganizationReadership = (from
                                                                  t in _context.Teams
                                                                     join d in _context.Departments on t.departmentId equals d.id
                                                                     join o in _context.Organizations on d.organizationId equals o.id
                                                                     join ou in _context.OrganizationUsers on o.id equals ou.organizationId
                                                                     join u in _context.Users on ou.userId equals u.id
                                                                     where
                                                                     explicitlyReadableTeamIds.Contains(t.id) == false &&           // Can't be one we already have
                                                                     explicitlyUnreadableTeamIds.Contains(t.id) == false &&         // Can't be one we aren't allowed to read from
                                                                     explicitlyUnreadableDepartmentIds.Contains(d.id) == false &&   // Can't get implicit access via a department that we aren't allowed to read from
                                                                     o.active == true &&
                                                                     o.deleted == false &&
                                                                     d.active == true &&
                                                                     d.deleted == false &&
                                                                     t.active == true &&
                                                                     t.deleted == false &&
                                                                     ou.active == true &&
                                                                     ou.deleted == false &&
                                                                     u.active == true &&
                                                                     u.deleted == false &&
                                                                     u.objectGuid == securityUser.objectGuid &&
                                                                     ou.canRead == true
                                                                     select t)
                                                                  .ToList();

                if (teamsReadableViaOrganizationReadership != null && teamsReadableViaOrganizationReadership.Count > 0)
                {
                    output.AddRange(teamsReadableViaOrganizationReadership);

                    explicitlyReadableTeamIds = (from x in output select x.id).ToList();
                }
            }

            if (output == null)
            {
                output = new List<Team>();
            }

            return (from x in output orderby x.name select x).ToList();
        }


        protected List<Organization> GetOrganizationsUserIsEntitledToReadFrom(SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<Organization> output = (from
                                o in _context.Organizations
                                         join ou in _context.OrganizationUsers on o.id equals ou.organizationId
                                         join u in _context.Users on ou.userId equals u.id
                                         where
                                         o.active == true &&
                                         o.deleted == false &&
                                         ou.active == true &&
                                         ou.deleted == false &&
                                         u.active == true &&
                                         u.deleted == false &&
                                         u.objectGuid == securityUser.objectGuid &&
                                         ou.canRead == true
                                         orderby o.name
                                         select o)
                                .ToList();

            if (output == null)
            {
                output = new List<Organization>();
            }

            return output;
        }


        protected List<Department> GetDepartmentsUserIsEntitledToReadFrom(SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<Department> output = (from
                                d in _context.Departments
                                       join du in _context.DepartmentUsers on d.id equals du.departmentId
                                       join u in _context.Users on du.userId equals u.id
                                       where
                                       d.active == true &&
                                       d.deleted == false &&
                                       du.active == true &&
                                       du.deleted == false &&
                                       u.active == true &&
                                       u.deleted == false &&
                                       u.objectGuid == securityUser.objectGuid &&
                                       du.canRead == true
                                       orderby d.name
                                       select d)
                                .ToList();

            if (output == null)
            {
                output = new List<Department>();
            }

            return output;
        }


        protected List<Team> GetTeamsUserIsEntitledToReadFrom(SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<Team> output = (from
                                t in _context.Teams
                                 join tu in _context.TeamUsers on t.id equals tu.teamId
                                 join u in _context.Users on tu.userId equals u.id
                                 where
                                 t.active == true &&
                                 t.deleted == false &&
                                 tu.active == true &&
                                 tu.deleted == false &&
                                 u.active == true &&
                                 u.deleted == false &&
                                 u.objectGuid == securityUser.objectGuid &&
                                 tu.canRead == true
                                 orderby t.name
                                 select t)
                                .ToList();

            if (output == null)
            {
                output = new List<Team>();
            }

            return output;
        }


        protected async Task<List<int>> GetOrganizationIdsUserIsEntitledToReadFromAsync(SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync(cancellationToken);
            }

            List<int> output = await (from ou in _context.OrganizationUsers
                                join u in _context.Users on ou.userId equals u.id
                                where
                                ou.active == true &&
                                ou.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                ou.canRead == true
                                select ou.organizationId)
                                .ToListAsync(cancellationToken);

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }



        protected List<int> GetOrganizationIdsUserIsEntitledToReadFrom(SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<int> output = (from ou in _context.OrganizationUsers
                                join u in _context.Users on ou.userId equals u.id
                                where
                                ou.active == true &&
                                ou.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                ou.canRead == true
                                select ou.organizationId)
                                .ToList();

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected async Task<List<int>> GetOrganizationIdsUserIsEntitledToReadImplicitlyFromForNullDepartmentAndTeamValuesAsync(SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            //
            // The purpose of this is to get all organizations that a user gains implicit readability for through a department or a team that the user can read from.
            //
            // This is to be applied to records that have an organization set but no department or team.  - i.e. the record is intended for the entirety of the organization
            //
            // If a user is explicitly denied read to an organization, they will not get access.
            //
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync(cancellationToken);
            }


            List<int> explicitlyUnreadableOrgIds = await (from ou in _context.OrganizationUsers
                                                    join u in _context.Users on ou.userId equals u.id
                                                    where
                                                    ou.active == true &&
                                                    ou.deleted == false &&
                                                    u.active == true &&
                                                    u.deleted == false &&
                                                    u.objectGuid == securityUser.objectGuid &&
                                                    ou.canRead != true                      // get organizations we know that we can't read from.
                                                    select ou.organizationId)
                                                    .ToListAsync(cancellationToken);


            List<int> output = null;

            List<int> organizationsReadableViaDepartmentReadership = await (from
                                                                      o in _context.Organizations
                                                                      join d in _context.Departments on o.id equals d.organizationId
                                                                      join du in _context.DepartmentUsers on d.id equals du.departmentId
                                                                      join u in _context.Users on du.userId equals u.id
                                                                      where
                                                                      explicitlyUnreadableOrgIds.Contains(o.id) == false && 
                                                                      o.active == true &&
                                                                      o.deleted == false &&
                                                                      d.active == true &&
                                                                      d.deleted == false &&
                                                                      du.active == true &&
                                                                      du.deleted == false &&
                                                                      u.active == true &&
                                                                      u.deleted == false &&
                                                                      u.objectGuid == securityUser.objectGuid &&
                                                                      du.canRead == true
                                                                      select o.id)
                                                                      .ToListAsync(cancellationToken);

            List<int> alreadyReadableOrgIds = null;
            if (organizationsReadableViaDepartmentReadership != null && organizationsReadableViaDepartmentReadership.Count > 0)
            {
                output = organizationsReadableViaDepartmentReadership;

                alreadyReadableOrgIds = output;
            }
            else
            {
                alreadyReadableOrgIds = new List<int>();
            }



            List<int> explicitlyUnreadableDepIds = await (from du in _context.DepartmentUsers
                                                    join u in _context.Users on du.userId equals u.id
                                                    where
                                                    du.active == true &&
                                                    du.deleted == false &&
                                                    u.active == true &&
                                                    u.deleted == false &&
                                                    u.objectGuid == securityUser.objectGuid &&
                                                    du.canRead != true                      // get departments we know that we can't read from.
                                                    select du.departmentId)
                                                    .ToListAsync(cancellationToken);


            List<int> organizationsReadableViaTeamReadership = await (from
                                                                o in _context.Organizations
                                                                join d in _context.Departments on o.id equals d.organizationId
                                                                join t in _context.Teams on d.id equals t.departmentId
                                                                join tu in _context.TeamUsers on t.id equals tu.teamId
                                                                join u in _context.Users on tu.userId equals u.id
                                                                where
                                                                alreadyReadableOrgIds.Contains(o.id) == false &&
                                                                explicitlyUnreadableOrgIds.Contains(o.id) == false &&
                                                                explicitlyUnreadableDepIds.Contains(d.id) == false &&
                                                                o.active == true &&
                                                                o.deleted == false &&
                                                                d.active == true &&
                                                                d.deleted == false &&
                                                                t.active == true &&
                                                                t.deleted == false &&
                                                                tu.active == true &&
                                                                tu.deleted == false &&
                                                                u.active == true &&
                                                                u.deleted == false &&
                                                                u.objectGuid == securityUser.objectGuid &&
                                                                tu.canRead == true
                                                                select o.id)
                                                                .ToListAsync(cancellationToken);


            if (organizationsReadableViaTeamReadership != null && organizationsReadableViaTeamReadership.Count > 0)
            {
                if (output != null)
                {
                    output.AddRange(organizationsReadableViaTeamReadership);
                }
                else
                {
                    output = organizationsReadableViaTeamReadership;
                }
            }

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected List<int> GetOrganizationIdsUserIsEntitledToReadImplicitlyFromForNullDepartmentAndTeamValues(SecurityUser securityUser = null)
        {
            //
            // The purpose of this is to get all organizations that a user gains implicit readability for through a department or a team that the user can read from.
            //
            // This is to be applied to records that have an organization set but no department or team.  - i.e. the record is intended for the entirety of the organization
            //
            // If a user is explicitly denied read to an organization, they will not get access.
            //
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<int> explicitlyUnreadableOrgIds = (from ou in _context.OrganizationUsers
                                                    join u in _context.Users on ou.userId equals u.id
                                                    where
                                                    ou.active == true &&
                                                    ou.deleted == false &&
                                                    u.active == true &&
                                                    u.deleted == false &&
                                                    u.objectGuid == securityUser.objectGuid &&
                                                    ou.canRead != true                      // get organizations we know that we can't read from.
                                                    select ou.organizationId)
                                                    .ToList();


            List<int> output = null;

            List<int> organizationsReadableViaDepartmentReadership = (from
                                                                      o in _context.Organizations
                                                                      join d in _context.Departments on o.id equals d.organizationId
                                                                      join du in _context.DepartmentUsers on d.id equals du.departmentId
                                                                      join u in _context.Users on du.userId equals u.id
                                                                      where
                                                                      explicitlyUnreadableOrgIds.Contains(o.id) == false &&
                                                                      o.active == true &&
                                                                      o.deleted == false &&
                                                                      d.active == true &&
                                                                      d.deleted == false &&
                                                                      du.active == true &&
                                                                      du.deleted == false &&
                                                                      u.active == true &&
                                                                      u.deleted == false &&
                                                                      u.objectGuid == securityUser.objectGuid &&
                                                                      du.canRead == true
                                                                      select o.id)
                                                                      .ToList();

            List<int> alreadyReadableOrgIds = null;
            if (organizationsReadableViaDepartmentReadership != null && organizationsReadableViaDepartmentReadership.Count > 0)
            {
                output = organizationsReadableViaDepartmentReadership;

                alreadyReadableOrgIds = output;
            }
            else
            {
                alreadyReadableOrgIds = new List<int>();
            }


            List<int> explicitlyUnreadableDepIds = (from du in _context.DepartmentUsers
                                                    join u in _context.Users on du.userId equals u.id
                                                    where
                                                    du.active == true &&
                                                    du.deleted == false &&
                                                    u.active == true &&
                                                    u.deleted == false &&
                                                    u.objectGuid == securityUser.objectGuid &&
                                                    du.canRead != true                      // get departments we know that we can't read from.
                                                    select du.departmentId)
                                                    .ToList();


            List<int> organizationsReadableViaTeamReadership = (from
                                                                o in _context.Organizations
                                                                join d in _context.Departments on o.id equals d.organizationId
                                                                join t in _context.Teams on d.id equals t.departmentId
                                                                join tu in _context.TeamUsers on t.id equals tu.teamId
                                                                join u in _context.Users on tu.userId equals u.id
                                                                where
                                                                alreadyReadableOrgIds.Contains(o.id) == false &&
                                                                explicitlyUnreadableOrgIds.Contains(o.id) == false &&
                                                                explicitlyUnreadableDepIds.Contains(d.id) == false &&
                                                                o.active == true &&
                                                                o.deleted == false &&
                                                                d.active == true &&
                                                                d.deleted == false &&
                                                                t.active == true &&
                                                                t.deleted == false &&
                                                                tu.active == true &&
                                                                tu.deleted == false &&
                                                                u.active == true &&
                                                                u.deleted == false &&
                                                                u.objectGuid == securityUser.objectGuid &&
                                                                tu.canRead == true
                                                                select o.id)
                                                                .ToList();


            if (organizationsReadableViaTeamReadership != null && organizationsReadableViaTeamReadership.Count > 0)
            {
                if (output != null)
                {
                    output.AddRange(organizationsReadableViaTeamReadership);
                }
                else
                {
                    output = organizationsReadableViaTeamReadership;
                }
            }

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected List<int> GetOrganizationIdsUserIsEntitledToChangeOwnerFor(SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<int> output = (from ou in _context.OrganizationUsers
                                join u in _context.Users on ou.userId equals u.id
                                where
                                ou.active == true &&
                                ou.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                ou.canChangeOwner == true
                                select ou.organizationId)
                                .ToList();

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected async Task<List<int>> GetOrganizationIdsUserIsEntitledToChangeOwnerForAsync(SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync(cancellationToken);
            }

            List<int> output = await (from ou in _context.OrganizationUsers
                                join u in _context.Users on ou.userId equals u.id
                                where
                                ou.active == true &&
                                ou.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                ou.canChangeOwner == true
                                select ou.organizationId)
                                .ToListAsync(cancellationToken);

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected List<int> GetOrganizationIdsUserIsEntitledToChangeHierarchyFor(SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<int> output = (from ou in _context.OrganizationUsers
                                join u in _context.Users on ou.userId equals u.id
                                where
                                ou.active == true &&
                                ou.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                ou.canChangeHierarchy == true
                                select ou.organizationId)
                                .ToList();

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected async Task<List<int>> GetOrganizationIdsUserIsEntitledToChangeHierarchyForAsync(SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync(cancellationToken);
            }

            List<int> output = await (from ou in _context.OrganizationUsers
                                join u in _context.Users on ou.userId equals u.id
                                where
                                ou.active == true &&
                                ou.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                ou.canChangeHierarchy == true
                                select ou.organizationId)
                                .ToListAsync(cancellationToken);

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }



        protected List<int> GetOrganizationIdsUserIsEntitledToWriteTo(SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<int> output = (from ou in _context.OrganizationUsers
                                join u in _context.Users on ou.userId equals u.id
                                where
                                ou.active == true &&
                                ou.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                ou.canWrite == true
                                select ou.organizationId)
                                .ToList();

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected async Task<List<int>> GetOrganizationIdsUserIsEntitledToWriteToAsync(SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync(cancellationToken);
            }

            List<int> output = await (from ou in _context.OrganizationUsers
                                join u in _context.Users on ou.userId equals u.id
                                where
                                ou.active == true &&
                                ou.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                ou.canWrite == true
                                select ou.organizationId)
                                .ToListAsync(cancellationToken);

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected List<Organization> GetOrganizationsUserIsEntitledToWriteTo(SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<Organization> output = (from
                                o in _context.Organizations
                                         join
                   ou in _context.OrganizationUsers on o.id equals ou.organizationId
                                         join u in _context.Users on ou.userId equals u.id
                                         where
                                         o.active == true &&
                                         o.deleted == false &&
                                         ou.active == true &&
                                         ou.deleted == false &&
                                         u.active == true &&
                                         u.deleted == false &&
                                         u.objectGuid == securityUser.objectGuid &&
                                         ou.canWrite == true
                                         orderby o.name
                                         select o)
                                .ToList();

            if (output == null)
            {
                output = new List<Organization>();
            }

            return output;
        }


        protected async Task<List<Organization>> GetOrganizationsUserIsEntitledToWriteToAsync(SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync(cancellationToken);
            }

            List<Organization> output = await (from
                                o in _context.Organizations
                                join
                                ou in _context.OrganizationUsers on o.id equals ou.organizationId
                                join u in _context.Users on ou.userId equals u.id
                                where
                                o.active == true &&
                                o.deleted == false &&
                                ou.active == true &&
                                ou.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                ou.canWrite == true
                                orderby o.name
                                select o)
                                .ToListAsync(cancellationToken);

            if (output == null)
            {
                output = new List<Organization>();
            }

            return output;
        }


        protected List<Department> GetDepartmentsUserIsEntitledToWriteTo(SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<Department> output = (from
                                d in _context.Departments
                                       join du in _context.DepartmentUsers on d.id equals du.departmentId
                                       join u in _context.Users on du.userId equals u.id
                                       where
                                       du.active == true &&
                                       du.deleted == false &&
                                       d.active == true &&
                                       d.deleted == false &&
                                       u.active == true &&
                                       u.deleted == false &&
                                       u.objectGuid == securityUser.objectGuid &&
                                       du.canWrite == true
                                       orderby d.name
                                       select d)
                                .ToList();

            if (output == null)
            {
                output = new List<Department>();
            }

            return output;
        }


        protected async Task<List<Department>> GetDepartmentsUserIsEntitledToWriteToAsync(SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync(cancellationToken);
            }

            List<Department> output = await (from
                                d in _context.Departments
                                       join du in _context.DepartmentUsers on d.id equals du.departmentId
                                       join u in _context.Users on du.userId equals u.id
                                       where
                                       du.active == true &&
                                       du.deleted == false &&
                                       d.active == true &&
                                       d.deleted == false &&
                                       u.active == true &&
                                       u.deleted == false &&
                                       u.objectGuid == securityUser.objectGuid &&
                                       du.canWrite == true
                                       orderby d.name
                                       select d)
                                .ToListAsync(cancellationToken);

            if (output == null)
            {
                output = new List<Department>();
            }

            return output;
        }


        protected List<Team> GetTeamsUserIsEntitledToWriteTo(SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<Team> output = (from
                                t in _context.Teams
                                 join tu in _context.TeamUsers on t.id equals tu.teamId
                                 join u in _context.Users on tu.userId equals u.id
                                 where
                                 tu.active == true &&
                                 tu.deleted == false &&
                                 t.active == true &&
                                 t.deleted == false &&
                                 u.active == true &&
                                 u.deleted == false &&
                                 u.objectGuid == securityUser.objectGuid &&
                                 tu.canWrite == true
                                 orderby t.name
                                 select t)
                                .ToList();

            if (output == null)
            {
                output = new List<Team>();
            }

            return output;
        }


        protected async Task<List<Team>> GetTeamsUserIsEntitledToWriteToAsync(SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync(cancellationToken);
            }

            List<Team> output = await (from
                                t in _context.Teams
                                 join tu in _context.TeamUsers on t.id equals tu.teamId
                                 join u in _context.Users on tu.userId equals u.id
                                 where
                                 tu.active == true &&
                                 tu.deleted == false &&
                                 t.active == true &&
                                 t.deleted == false &&
                                 u.active == true &&
                                 u.deleted == false &&
                                 u.objectGuid == securityUser.objectGuid &&
                                 tu.canWrite == true
                                 orderby t.name
                                 select t)
                                .ToListAsync(cancellationToken);

            if (output == null)
            {
                output = new List<Team>();
            }

            return output;
        }

        protected async Task<List<int>> GetDepartmentIdsUserIsEntitledToReadFromAsync(SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync(cancellationToken);
            }

            List<int> output = await (from du in _context.DepartmentUsers
                                join u in _context.Users on du.userId equals u.id
                                where
                                du.active == true &&
                                du.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                du.canRead == true
                                select du.departmentId)
                                .ToListAsync(cancellationToken);

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected List<int> GetDepartmentIdsUserIsEntitledToReadFrom(SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<int> output = (from du in _context.DepartmentUsers
                                join u in _context.Users on du.userId equals u.id
                                where
                                du.active == true &&
                                du.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                du.canRead == true
                                select du.departmentId)
                                .ToList();

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }



        protected async Task<List<int>> GetDepartmentsIdsUserIsEntitledToReadImplicitlyFromForNullTeamValuesAsync(SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            //
            // The purpose of this is to get all departments that a user gains implicit readability for through a team that the user can read from.
            //
            // This is to be applied to records that have an department set but no team.  - i.e. the record is intended for the entirety of the department
            //
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync(cancellationToken);
            }


            List<int> explicitlyUnreadableDepIds = await (from du in _context.DepartmentUsers
                                                    join u in _context.Users on du.userId equals u.id
                                                    where
                                                    du.active == true &&
                                                    du.deleted == false &&
                                                    u.active == true &&
                                                    u.deleted == false &&
                                                    u.objectGuid == securityUser.objectGuid &&
                                                    du.canRead != true                      // get departments that we know that we can't read from.
                                                    select du.departmentId)
                                                    .ToListAsync(cancellationToken);


            List<int> departmentsReadableViaTeamReadership = await (from d in _context.Departments
                                                                join t in _context.Teams on d.id equals t.departmentId
                                                                join tu in _context.TeamUsers on t.id equals tu.teamId
                                                                join u in _context.Users on tu.userId equals u.id
                                                                where
                                                                explicitlyUnreadableDepIds.Contains(d.id) == false &&
                                                                d.active == true &&
                                                                d.deleted == false &&
                                                                t.active == true &&
                                                                t.deleted == false &&
                                                                tu.active == true &&
                                                                tu.deleted == false &&
                                                                u.active == true &&
                                                                u.deleted == false &&
                                                                u.objectGuid == securityUser.objectGuid &&
                                                                tu.canRead == true
                                                                select d.id)
                                                                .ToListAsync(cancellationToken);

            if (departmentsReadableViaTeamReadership == null)
            {
                departmentsReadableViaTeamReadership = new List<int>();
            }

            return departmentsReadableViaTeamReadership;
        }

        protected List<int> GetDepartmentsIdsUserIsEntitledToReadImplicitlyFromForNullTeamValues(SecurityUser securityUser = null)
        {
            //
            // The purpose of this is to get all departments that a user gains implicit readability for through a team that the user can read from.
            //
            // This is to be applied to records that have an department set but no team.  - i.e. the record is intended for the entirety of the department
            //
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }


            List<int> explicitlyUnreadableDepIds = (from du in _context.DepartmentUsers
                                                    join u in _context.Users on du.userId equals u.id
                                                    where
                                                    du.active == true &&
                                                    du.deleted == false &&
                                                    u.active == true &&
                                                    u.deleted == false &&
                                                    u.objectGuid == securityUser.objectGuid &&
                                                    du.canRead != true                    // get departments that we know that we can't read from.
                                                    select du.departmentId)
                                                    .ToList();

            List<int> departmentsReadableViaTeamReadership = (from d in _context.Departments
                                                                join t in _context.Teams on d.id equals t.departmentId
                                                                join tu in _context.TeamUsers on t.id equals tu.teamId
                                                                join u in _context.Users on tu.userId equals u.id
                                                                where
                                                                explicitlyUnreadableDepIds.Contains(d.id) == false &&
                                                                d.active == true &&
                                                                d.deleted == false &&
                                                                t.active == true &&
                                                                t.deleted == false &&
                                                                tu.active == true &&
                                                                tu.deleted == false &&
                                                                u.active == true &&
                                                                u.deleted == false &&
                                                                u.objectGuid == securityUser.objectGuid &&
                                                                tu.canRead == true
                                                                select d.id)
                                                                .ToList();

            if (departmentsReadableViaTeamReadership == null)
            {
                departmentsReadableViaTeamReadership = new List<int>();
            }

            return departmentsReadableViaTeamReadership;
        }


        protected List<int> GetDepartmentIdsUserIsEntitledToWriteTo(SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<int> output = (from du in _context.DepartmentUsers
                                join u in _context.Users on du.userId equals u.id
                                where
                                du.active == true &&
                                du.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                du.canWrite == true
                                select du.departmentId)
                                .ToList();

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }

        protected async Task<List<int>> GetDepartmentIdsUserIsEntitledToWriteToAsync(SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync(cancellationToken);
            }

            List<int> output = await (from du in _context.DepartmentUsers
                                join u in _context.Users on du.userId equals u.id
                                where
                                du.active == true &&
                                du.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                du.canWrite == true
                                select du.departmentId)
                                .ToListAsync(cancellationToken);

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }



        protected List<int> GetDepartmentIdsUserIsEntitledToChangeOwnerFor(SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<int> output = (from du in _context.DepartmentUsers
                                join u in _context.Users on du.userId equals u.id
                                where
                                du.active == true &&
                                du.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                du.canChangeOwner == true
                                select du.departmentId)
                                .ToList();

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected async Task<List<int>> GetDepartmentIdsUserIsEntitledToChangeOwnerForAsync(SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync();
            }

            List<int> output = await (from du in _context.DepartmentUsers
                                join u in _context.Users on du.userId equals u.id
                                where
                                du.active == true &&
                                du.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                du.canChangeOwner == true
                                select du.departmentId)
                                .ToListAsync(cancellationToken);

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected List<int> GetDepartmentIdsUserIsEntitledToChangeHierarchyFor(SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<int> output = (from du in _context.DepartmentUsers
                                join u in _context.Users on du.userId equals u.id
                                where
                                du.active == true &&
                                du.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                du.canChangeHierarchy == true
                                select du.departmentId)
                                .ToList();

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected async Task<List<int>> GetDepartmentIdsUserIsEntitledToChangeHierarchyForAsync(SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync(cancellationToken);
            }

            List<int> output = await (from du in _context.DepartmentUsers
                                join u in _context.Users on du.userId equals u.id
                                where
                                du.active == true &&
                                du.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                du.canChangeHierarchy == true
                                select du.departmentId)
                                .ToListAsync(cancellationToken);

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected async Task<List<int>> GetTeamIdsUserIsEntitledToReadFromAsync(SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync(cancellationToken);
            }

            List<int> output = await (from tu in _context.TeamUsers
                                join u in _context.Users on tu.userId equals u.id
                                where
                                tu.active == true &&
                                tu.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                tu.canRead == true
                                select tu.teamId)
                                .ToListAsync(cancellationToken);

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected List<int> GetTeamIdsUserIsEntitledToReadFrom(SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<int> output = (from tu in _context.TeamUsers
                                join u in _context.Users on tu.userId equals u.id
                                where
                                tu.active == true &&
                                tu.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                tu.canRead == true
                                select tu.teamId)
                                .ToList();

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected List<int> GetTeamIdsUserIsEntitledToWriteTo(SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<int> output = (from tu in _context.TeamUsers
                                join u in _context.Users on tu.userId equals u.id
                                where
                                tu.active == true &&
                                tu.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                tu.canWrite == true
                                select tu.teamId)
                                .ToList();

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected async Task<List<int>> GetTeamIdsUserIsEntitledToWriteToAsync(SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync(cancellationToken);
            }

            List<int> output = await (from tu in _context.TeamUsers
                                join u in _context.Users on tu.userId equals u.id
                                where
                                tu.active == true &&
                                tu.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                tu.canWrite == true
                                select tu.teamId)
                                .ToListAsync(cancellationToken);

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected List<int> GetTeamIdsUserIsEntitledToChangeOwnerFor(SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<int> output = (from tu in _context.TeamUsers
                                      join u in _context.Users on tu.userId equals u.id
                                      where
                                      tu.active == true &&
                                      tu.deleted == false &&
                                      u.active == true &&
                                      u.deleted == false &&
                                      u.objectGuid == securityUser.objectGuid &&
                                      tu.canChangeOwner == true
                                      select tu.teamId)
                                .ToList();

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected async Task<List<int>> GetTeamIdsUserIsEntitledToChangeOwnerForAsync(SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync(cancellationToken);
            }

            List<int> output = await (from tu in _context.TeamUsers
                                join u in _context.Users on tu.userId equals u.id
                                where
                                tu.active == true &&
                                tu.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                tu.canChangeOwner == true
                                select tu.teamId)
                                .ToListAsync(cancellationToken);

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected List<int> GetTeamIdsUserIsEntitledToChangeHierarchyFor(SecurityUser securityUser = null)
        {
            if (securityUser == null)
            {
                securityUser = GetSecurityUser();
            }

            List<int> output = (from tu in _context.TeamUsers
                                join u in _context.Users on tu.userId equals u.id
                                where
                                tu.active == true &&
                                tu.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                tu.canChangeHierarchy == true
                                select tu.teamId)
                                .ToList();

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected async Task<List<int>> GetTeamIdsUserIsEntitledToChangeHierarchyForAsync(SecurityUser securityUser = null, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                securityUser = await GetSecurityUserAsync(cancellationToken);
            }

            List<int> output = await (from tu in _context.TeamUsers
                                join u in _context.Users on tu.userId equals u.id
                                where
                                tu.active == true &&
                                tu.deleted == false &&
                                u.active == true &&
                                u.deleted == false &&
                                u.objectGuid == securityUser.objectGuid &&
                                tu.canChangeHierarchy == true
                                select tu.teamId)
                                .ToListAsync(cancellationToken);

            if (output == null)
            {
                output = new List<int>();
            }

            return output;
        }


        protected void VerifyIntegrityOfDataVisibilityEntities(Team team, Department department, Organization organization)
        {
            //
            // This will throw an error if there is an integrity issue with the data visibility configuration provided.
            //

            //
            //
            // check the team/dep relationship.  If there is a team and a department, the team must belong to the department
            //
            if (team != null && department != null)
            {
                if (team.departmentId != department.id)
                {
                    throw new Exception(""Configuration is incorrect.  Team does not belong to department."");
                }
            }

            //
            // check the department/organization relationship.  If there is a department and an organization, the department must belong to the organization
            //
            if (department != null && organization != null)
            {
                if (department.organizationId != organization.id)
                {
                    throw new Exception(""Data Visibility Configuration is incorrect.  Department does not belong to organization."");
                }
            }

            // have team, but couldn't load department or organization
            if (team != null && (department == null || organization == null))
            {
                throw new Exception(""Data Visibility Configuration is incorrect.  Inconsistency in department or organization."");
            }

            // have department but no organization
            if (department != null && organization == null)
            {
                throw new Exception(""Data Visibility Configuration is incorrect.  Inconsistency in organization."");
            }
        }


        protected void VerifyDataWritePrivilegeForAdd(Team team, Department department, Organization organization, List<int> teamsUserIsEntitledToWriteTo, List<int> departmentsUserIsEntitledToWriteTo, List<int> organizationsUserIsEntitledToWriteTo)
        {
            //
            // This will throw an error if the user can't write to the data visibility settings provided
            //
            // owner param is the User from the userId from the data record, NOT THE CURRENT USER
            // team param is the Team from the teamId from the data record
            // department param is the Department from the departmentId from the data record
            // organization param is the Organization from the organizationId from the data record
            //
            // The list of int params are the ids of the items to which the current user is entitled to write to.
            //
            // Work from the team level and work upwards to dep and org.  A User only needs to be able to write to the most specific level of visibility for this to be verify successfully.
            //

            if (team != null)       // check if the data being updated has a team.  If it does, then test for writeability at all levels in the team's hierarchy, starting at the org.
            {
                if (department == null || organization == null)     // Sanity check.
                {
                    throw new Exception(""VerifyDataWritePrivilegeForAdd requires department and organization to be provided when a team is provided."");     
                }

                if (organizationsUserIsEntitledToWriteTo != null && organizationsUserIsEntitledToWriteTo.Contains(organization.id) == true)
                {
                    // write ability verified via organization writership.  We can get out.
                    return;
                }

                if (departmentsUserIsEntitledToWriteTo != null && departmentsUserIsEntitledToWriteTo.Contains(department.id) == true)
                {
                    // write ability verified via department writership.  We can get out.
                    return;
                }

                if (teamsUserIsEntitledToWriteTo != null && teamsUserIsEntitledToWriteTo.Contains(team.id) == true)
                {
                    // write ability verified via team writership.  We can get out.
                    return;
                }

                throw new Exception(""Unable to write to the team named "" + team.name + "" with Id of "" + team.id);
            }
            else if (department != null)    // check if the data being updated has a department.  If it does, then test for writeability at all levels in the department's hierarchy, starting at the org.
            {
                if (organization == null)     // Sanity check.
                {
                    throw new Exception(""VerifyDataWritePrivilegeForAdd requires organization to be provided when a department is provided."");
                }

                if (organizationsUserIsEntitledToWriteTo != null && organizationsUserIsEntitledToWriteTo.Contains(organization.id) == true)
                {
                    // write ability verified via organization writership.  We can get out.
                    return;
                }

                if (departmentsUserIsEntitledToWriteTo != null && departmentsUserIsEntitledToWriteTo.Contains(department.id))
                {
                    // write ability verified via department writership.  We can get out.
                    return;
                }

                throw new Exception(""Unable to write to the department named "" + department.name + "" with Id of "" + department.id);
            }
            else if (organization != null)  // check if the data being updated has an organization.  
            {
                if (organizationsUserIsEntitledToWriteTo != null && organizationsUserIsEntitledToWriteTo.Contains(organization.id))
                {
                    // write ability verified via organization writership.  We can get out.
                    return;
                }

                throw new Exception(""Unable to write to the organization named "" + organization.name + "" with Id of "" + organization.id);
            }
            else
            {
                throw new Exception(""Unable to find reason to grant ability to write."");
            }
        }


        protected void VerifyDataWritePrivilegeForUpdate(User owner, Team team, Department department, Organization organization, List<int> userAndTheirReports, List<int> teamsUserIsEntitledToWriteTo, List<int> departmentsUserIsEntitledToWriteTo, List<int> organizationsUserIsEntitledToWriteTo)
        {
            //
            // This will throw an error if the user can't write to the data visibility settings provided
            //
            // owner param is the User from the userId from the data record, NOT THE CURRENT USER
            // team param is the Team from the teamId from the data record
            // department param is the Department from the departmentId from the data record
            // organization param is the Organization from the organizationId from the data record
            //
            // The list of int params are the ids of the items to which the current user is entitled to write to.
            //
            // First check the owner of the record, and see if the current user's write permission set at the user level includes it.  If not, then work from the team level and work upwards to dep and org.  A User only needs to be able to write to the most specific level of visibility for this to be verify successfully.
            //
            // Any record that a user is the owner of, by way of their user ID being referenced in the userId field of the record, implies that the user can always read and write the 'Data' attributes on that record.
            // That means that record ownership grants a user permission to read and change data fields but not the ownership or hierarchy fields which are (userid, organizationId, departmentId, and teamId).  Changing those requires hierarchy granted permissions.
            // The granting of this privilege by way of ownership will be done in spite of the record's position in the hierarchy, and the user's relation to the hierarchy position.
            //


            //
            // Step 1 is only for updates, and that is to check the owner of the record, and to see if that user is in the list of the current user and their reports.  If it is, then return normally to indicate the record is writeable.  However, if it is not, then do not throw an error, but continue checking the hierarchy configuration for writeability.
            //
            if (owner != null)
            {
                if (userAndTheirReports.Contains(owner.id) == true)
                {
                    // write ability verified.  We can get out.
                    return;
                }
            }

            //
            // Step 2 is the same as for adds, to check the hierarchy permissions, so call that
            //
            VerifyDataWritePrivilegeForAdd(team, department, organization, teamsUserIsEntitledToWriteTo, departmentsUserIsEntitledToWriteTo, organizationsUserIsEntitledToWriteTo);

            return;
        }


        protected void VerifyDataChangeOwnerPrivilege(Team team, Department department, Organization organization, List<int> teamsUserIsEntitledToChangeOwnerFor, List<int> departmentsUserIsEntitledToChangeOwnerFor, List<int> organizationsUserIsEntitledToChangeOwnerFor)
        {
            //
            // This will throw an error if the user can't can't change ownership with  the data visibility settings provided
            //
            // team param is the Team from the teamId from the data record
            // department param is the Department from the departmentId from the data record
            // organization param is the Organization from the organizationId from the data record
            //
            // The list of int params are the ids of the items to which the current user is entitled to change ownership for
            //
            // Start at team level and work upwards to dep and org.  A User only needs to be able to change at to the most specific level of visibility for this to be verify successfully.
            //
            if (team != null)
            {
                if (teamsUserIsEntitledToChangeOwnerFor != null && teamsUserIsEntitledToChangeOwnerFor.Contains(team.id) == true)
                {
                    // change owner ability verified.  We can get out.
                    return;
                }

                throw new Exception(""Unable to change owner or the the team named "" + team.name + "" with Id of "" + team.id);
            }
            else if (department != null)
            {
                if (departmentsUserIsEntitledToChangeOwnerFor != null && departmentsUserIsEntitledToChangeOwnerFor.Contains(department.id))
                {
                    // change owner ability verified.  We can get out.
                    return;
                }

                throw new Exception(""Unable to change owner for the department named "" + department.name + "" with Id of "" + department.id);
            }
            else if (organization != null)
            {
                if (organizationsUserIsEntitledToChangeOwnerFor != null && organizationsUserIsEntitledToChangeOwnerFor.Contains(organization.id))
                {
                    // change owner ability verified.  We can get out.
                    return;
                }

                throw new Exception(""Unable to change owner for the organization named "" + organization.name + "" with Id of "" + organization.id);
            }

            return;
        }


        protected void VerifyDataChangeHierarchyPrivilege(Team team, Department department, Organization organization, List<int> teamsUserIsEntitledToChangeHierarchyFor, List<int> departmentsUserIsEntitledToChangeHierarchyFor, List<int> organizationsUserIsEntitledToChangeHierarchyFor)
        {
            //
            // This will throw an error if the user can't can't change the hierarchy with  the data visibility settings provided
            //
            // team param is the Team from the teamId from the data record
            // department param is the Department from the departmentId from the data record
            // organization param is the Organization from the organizationId from the data record
            //
            // The list of int params are the ids of the items to which the current user is entitled to change hierarchy for
            //
            // Start at team level and work upwards to dep and org.  A User only needs to be able to change at to the most specific level of visibility for this to be verify successfully.
            //
            if (team != null)
            {
                if (teamsUserIsEntitledToChangeHierarchyFor != null && teamsUserIsEntitledToChangeHierarchyFor.Contains(team.id) == true)
                {
                    // change hierarchy ability verified.  We can get out.
                    return;
                }

                throw new Exception(""Unable to change hierarchy or the the team named "" + team.name + "" with Id of "" + team.id);
            }
            else if (department != null)
            {
                if (departmentsUserIsEntitledToChangeHierarchyFor != null && departmentsUserIsEntitledToChangeHierarchyFor.Contains(department.id))
                {
                    // change hierarchy ability verified.  We can get out.
                    return;
                }

                throw new Exception(""Unable to change hierarchy for the department named "" + department.name + "" with Id of "" + department.id);
            }
            else if (organization != null)
            {
                if (organizationsUserIsEntitledToChangeHierarchyFor != null && organizationsUserIsEntitledToChangeHierarchyFor.Contains(organization.id))
                {
                    // change hierarchy ability verified.  We can get out.
                    return;
                }

                throw new Exception(""Unable to change hierarchy for the organization named "" + organization.name + "" with Id of "" + organization.id);
            }

            return;
        }


        protected Organization GetDefaultOrganization(List<Organization> organizationsUserIsEntitledToWriteTo, SecurityUser securityUser, Department defaultDepartment)
        {
            Organization defaultOrganization = null;
            //
            // Find the organization
            // 
            if (organizationsUserIsEntitledToWriteTo.Count > 0 && securityUser.securityOrganization != null)
            {
                foreach (Organization organization in organizationsUserIsEntitledToWriteTo)
                {
                    if (organization.objectGuid == securityUser.securityOrganization.objectGuid)
                    {
                        defaultOrganization = organization;
                        break;
                    }
                }
            }


            //
            // If the security user doesn't explicitly have a default organization set, but they have a default department, then use the department's organization to infer the user' default organization
            //
            if (defaultDepartment != null && defaultOrganization == null)
            {
                defaultOrganization = (from x in organizationsUserIsEntitledToWriteTo
                                       where x.id == defaultDepartment.organizationId
                                       select x).FirstOrDefault();
            }
            else if (defaultDepartment != null && defaultOrganization != null)
            {
                //
                // Sanity check.
                //
                if (defaultDepartment.organizationId != defaultOrganization.id)
                {
                    throw new Exception(""Configuration inconsistency on user record.  Default department does not belong to default organization"");
                }
            }

            return defaultOrganization;
        }


        protected Department GetDefaultDepartment(List<Department> departmentsUserIsEntitledToWriteTo, SecurityUser securityUser, Team defaultTeam)
        {
            Department defaultDepartment = null;

            //
            // Find the department
            //
            if (departmentsUserIsEntitledToWriteTo.Count > 0 && securityUser.securityDepartment != null)
            {
                foreach (Department department in departmentsUserIsEntitledToWriteTo)
                {
                    if (department.objectGuid == securityUser.securityDepartment.objectGuid)
                    {
                        defaultDepartment = department;
                        break;
                    }
                }
            }

            //
            // If the security user doesn't explicitly have a default department set, but they have a default team, then use the team's department to infer the user' default department
            //
            if (defaultTeam != null && defaultDepartment == null)
            {
                defaultDepartment = (from x in departmentsUserIsEntitledToWriteTo
                                     where x.id == defaultTeam.departmentId
                                     select x).FirstOrDefault();
            }
            else if (defaultTeam != null && defaultDepartment != null)
            {
                //
                // Sanity check.
                //
                if (defaultTeam.departmentId != defaultDepartment.id)
                {
                    throw new Exception(""Configuration inconsistency on user record.  Default team does not belong to default department"");
                }
            }

            return defaultDepartment;
        }


        protected Team GetDefaultTeam(List<Team> teamsUserIsEntitledToWriteTo, SecurityUser securityUser)
        {
            //
            // It is expected that if a particular organization, department, or team is assigned as a default, then the user will also have explicit write permission to that same entity.
            //
            // Find the team
            //
            if (teamsUserIsEntitledToWriteTo.Count > 0 && securityUser.securityTeam != null)
            {
                foreach (Team team in teamsUserIsEntitledToWriteTo)
                {
                    if (team.objectGuid == securityUser.securityTeam.objectGuid)
                    {
                        return team;
                    }
                }
            }

            return null;
        }


        protected User GetUser(int id)
        {
            return (from x in _context.Users
                    where x.id == id
                    select x).FirstOrDefault();
        }


        protected Team GetTeam(int id)
        {
            return (from x in _context.Teams
                    where x.id == id
                    select x).FirstOrDefault();
        }


        protected Department GetDepartment(int id)
        {
            return (from x in _context.Departments
                    where x.id == id
                    select x).FirstOrDefault();
        }


        protected Organization GetOrganization(int id)
        {
            return (from x in _context.Organizations
                    where x.id == id
                    select x).FirstOrDefault();
        }


        protected async Task<User> GetUserAsync(int id, CancellationToken cancellationToken = default)
        {
            return await (from x in _context.Users
                    where x.id == id
                    select x).FirstOrDefaultAsync(cancellationToken);
        }


        protected async Task<Team> GetTeamAsync(int id, CancellationToken cancellationToken = default)
        {
            return await (from x in _context.Teams
                    where x.id == id
                    select x).FirstOrDefaultAsync(cancellationToken);
        }


        protected async Task<Department> GetDepartmentAsync(int id, CancellationToken cancellationToken = default)
        {
            return await (from x in _context.Departments
                    where x.id == id
                    select x).FirstOrDefaultAsync(cancellationToken);
        }


        protected async Task<Organization> GetOrganizationAsync(int id, CancellationToken cancellationToken = default)
        {
            return await (from x in _context.Organizations
                    where x.id == id
                    select x).FirstOrDefaultAsync(cancellationToken);
        }


        public static void SynchronizeDataVisibilityEntitiesForSingleUser(Guid securityUserObjectGuid)
        {
            lock (userSynchronizationSyncRoot)
            {
                try
                {

                    // 
                    // Synchronize the data visibility entries significant to the Security User provided
                    //
                    using (SecurityContext securityContext = new SecurityContext())
                    {
                        //
                        // If using SQLite as the security database, and this query by object guid doesn't work (ie. it doesn't find the user by object guid), then make sure that the connection
                        // string has BinaryGuid=False in its options.  For example, <add name=""SecurityContext"" connectionString=""data source=|DataDirectory|\Security\Security._context;BinaryGUID=false;"" providerName=""System.Data.SQLite"" />
                        //
                        SecurityUser securityUser = (from x in securityContext.SecurityUsers where x.objectGuid == securityUserObjectGuid select x).FirstOrDefault();

                        if (securityUser == null)
                        {
                            return;
                        }

                        Foundation.Utility.CreateAuditEvent(""Starting SynchronizeDataVisibilityEntitiesForSingleUser() process.  Security User object guid is: "" + securityUserObjectGuid + "" and user account name is: "" + securityUser.accountName);

                        SecurityTenant securityTenant;

                        securityTenant = (from x in securityContext.SecurityTenants
                                          where x.id == securityUser.securityTenantId
                                          select x)
                                        .AsNoTracking()
                                        .FirstOrDefault();

                        if (securityTenant == null)
                        {
                            return;
                        }

                        if (securityTenant.active == true && securityTenant.deleted == false)
                        {
                            SecurityTenantUser securityTenantUser;

                            securityTenantUser = (from x in securityContext.SecurityTenantUsers
                                                  where x.securityTenantId == securityTenant.id
                                                  && x.securityUserId == securityUser.id
                                                  select x)
                                                .Include(""securityUser.reportsToSecurityUser"")
                                                .Include(""securityUser.securityUserTitle"")
                                                .AsNoTracking()
                                                .FirstOrDefault();

                            if (securityTenantUser != null)
                            {
                                User user = null;

                                //
                                // First, synchronize this tenant's users from the security database into the application database.
                                //
                                using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                                {
                                    auditorContext.Configuration.AutoDetectChangesEnabled = true;
                                    auditorContext.Configuration.ValidateOnSaveEnabled = false;

                                    //
                                    // Get the application user by matching the user by object guid
                                    //
                                    bool userChangesMade = false;

                                    //
                                    // We may be adjusting this record, so don't use .AsNoTracking() here
                                    //
                                    // Note that in the application database side, the keys are user object guid, and the user's tenant guid.  As such, a single user in the security database will have one or more user records in the application database, depending
                                    // entirely upon how many tenants that they have moved around between
                                    //
                                    user = (from x in auditorContext.Users
                                            where
                                            x.objectGuid == securityTenantUser.securityUser.objectGuid &&
                                            x.tenantGuid == securityTenant.objectGuid
                                            select x)
                                            .FirstOrDefault();

                                    //
                                    // If we don't have the user yet, then add them.  If the user exists, then make sure the fields are the same.
                                    //
                                    if (user == null)
                                    {
                                        user = new User();

                                        //
                                        // Set the object guids here, the rest of the data fields will follow later.
                                        //
                                        user.objectGuid = securityTenantUser.securityUser.objectGuid;
                                        user.tenantGuid = securityTenant.objectGuid;

                                        auditorContext.Users.Add(user);

                                        userChangesMade = true;
                                    }


                                    //
                                    // Copy the property state from the security organization
                                    //
                                    if (user.accountName != securityUser.accountName)
                                    {
                                        user.accountName = securityUser.accountName;
                                        userChangesMade = true;
                                    }


                                    if (user.firstName != securityUser.firstName)
                                    {
                                        user.firstName = securityUser.firstName;
                                        userChangesMade = true;
                                    }


                                    if (user.middleName != securityUser.middleName)
                                    {
                                        user.middleName = securityUser.middleName;
                                        userChangesMade = true;
                                    }


                                    if (user.lastName != securityUser.lastName)
                                    {
                                        user.lastName = securityUser.lastName;
                                        userChangesMade = true;
                                    }


                                    string displayName = securityUser.firstName;

                                    if (string.IsNullOrEmpty(securityUser.middleName) == false)
                                    {
                                        displayName += "" "";
                                        displayName += securityUser.middleName;
                                    }

                                    displayName += "" "";
                                    displayName += securityUser.lastName;

                                    if (user.displayName != displayName)
                                    {
                                        user.displayName = displayName;
                                        userChangesMade = true;
                                    }

                                    if (user.active != securityTenantUser.active || user.deleted != securityTenantUser.deleted)
                                    {
                                        user.active = securityTenantUser.active;
                                        user.deleted = securityTenantUser.deleted;
                                        userChangesMade = true;
                                    }

                                    //
                                    // Handle the reports to structure here.  This code assumes that the user that this user reports to already exists in the application's User database.  If it does not, then the reports to for this user
                                    // will not be assigned this pass.  It is expected that the missing user will be processed later in this loop, and therefore the next data sync pass will find it, and allow this reports to relationship to be set.
                                    //
                                    // This is good enough for this problem because the data sync process happens very frequently, and mass user creations happen very infrequently.
                                    //
                                    if (securityUser.reportsToSecurityUserId.HasValue == true)
                                    {
                                        User managerUser = (from x in auditorContext.Users where x.objectGuid == securityTenantUser.securityUser.reportsToSecurityUser.objectGuid select x).FirstOrDefault();

                                        if (managerUser != null)
                                        {
                                            if (user.reportsToUserId != managerUser.id)
                                            {
                                                user.reportsToUserId = managerUser.id;
                                                userChangesMade = true;
                                            }
                                        }
                                    }

                                    if (securityUser.securityUserTitleId.HasValue == true)
                                    {
                                        UserTitle userTitle = (from x in auditorContext.UserTitles where x.objectGuid == securityTenantUser.securityUser.securityUserTitle.objectGuid select x).FirstOrDefault();

                                        if (userTitle == null)
                                        {
                                            userTitle = new UserTitle();

                                            userTitle.objectGuid = securityTenantUser.securityUser.securityUserTitle.objectGuid;
                                            userTitle.active = true;
                                            userTitle.deleted = false;

                                            userTitle.name = securityTenantUser.securityUser.securityUserTitle.name;
                                            userTitle.description = securityTenantUser.securityUser.securityUserTitle.description;

                                            using (%MODULENAME%Context userTitleContext = new %MODULENAME%Context())
                                            {
                                                userTitleContext.Configuration.AutoDetectChangesEnabled = true;
                                                userTitleContext.Configuration.ValidateOnSaveEnabled = false;

                                                userTitleContext.UserTitles.Add(userTitle);

                                                userTitleContext.SaveChanges();
                                            }
                                        }

                                        if (userTitle != null)
                                        {
                                            if (user.userTitleId != userTitle.id)
                                            {
                                                user.userTitleId = userTitle.id;
                                                userChangesMade = true;
                                            }
                                        }
                                    }

                                    if (userChangesMade == true)
                                    {
                                        auditorContext.SaveChanges();
                                    }
                                }
                            }
                        }


                        //
                        // Next, go through all of the organizations that are defined for this tenant in the security system that this user can read form, and synchronize them
                        //
                        List<SecurityOrganization> securityOrganizations = null;


                        List<Security.SecurityFramework.ModuleAndRoleAndPrivilege> accessibleModules = Security.SecurityFramework.GetUserModuleRoles(securityUser);

                        bool userIsModuleAdministrator = Security.SecurityFramework.UserCanAdministerModule(securityUser, ""%MODULENAME%"", accessibleModules);

                        //
                        // First get the organizations the user has explicit read permission to, or all of them if the user is a module administrator.
                        //
                        if (userIsModuleAdministrator == true)
                        {
                            //
                            // User is admin.  They see them all.
                            //
                            securityOrganizations = (from so in securityContext.SecurityOrganizations
                                                     where
                                                     so.securityTenantId == securityTenant.id
                                                     orderby so.id
                                                     select so)
                                                     .Distinct()
                                                     .AsNoTracking()
                                                     .ToList();

                        }
                        else
                        {
                            //
                            // get the organizations that the user can explicitly read from
                            //
                            securityOrganizations = (from so in securityContext.SecurityOrganizations
                                                     join sou in securityContext.SecurityOrganizationUsers on so.id equals sou.securityOrganizationId
                                                     where
                                                     so.securityTenantId == securityTenant.id &&
                                                     sou.securityUserId == securityUser.id && 
                                                     sou.canRead == true
                                                     orderby so.id
                                                     select so)
                                                     .Distinct()
                                                     .AsNoTracking()
                                                     .ToList();

                            //
                            // Now get any organizations that the user can partially access via department readership, that are not yet in the list of organizations
                            //
                            List<int> currentOrganizationIds = (from x in securityOrganizations select x.id).ToList();
                            List<SecurityOrganization> securityOrganizationsViaDepartment = (from so in securityContext.SecurityOrganizations
                                                                                             join sd in securityContext.SecurityDepartments on so.id equals sd.securityOrganizationId
                                                                                             join sdu in securityContext.SecurityDepartmentUsers on sd.id equals sdu.securityDepartmentId
                                                                                             where
                                                                                             currentOrganizationIds.Contains(so.id) == false &&
                                                                                             so.securityTenantId == securityTenant.id &&
                                                                                             sdu.securityUserId == securityUser.id &&
                                                                                             sdu.canRead == true
                                                                                             orderby so.id
                                                                                             select so)
                                                                                             .Distinct()
                                                                                             .AsNoTracking()
                                                                                             .ToList();

                            //
                            // Add in the new ones.
                            //
                            if (securityOrganizationsViaDepartment != null && securityOrganizationsViaDepartment.Count > 0)
                            {
                                securityOrganizations.AddRange(securityOrganizationsViaDepartment);
                                currentOrganizationIds = (from x in securityOrganizations select x.id).ToList();
                            }


                            //
                            // Now get the organizations partially available view team readership, that are not yet in the list of organizations
                            //
                            List<SecurityOrganization> securityOrganizationsViaTeam = (from so in securityContext.SecurityOrganizations
                                                                                       join sd in securityContext.SecurityDepartments on so.id equals sd.securityOrganizationId
                                                                                       join st in securityContext.SecurityTeams on sd.id equals st.securityDepartmentId
                                                                                       join stu in securityContext.SecurityTeamUsers on st.id equals stu.securityTeamId
                                                                                       where
                                                                                       currentOrganizationIds.Contains(so.id) == false &&
                                                                                       so.securityTenantId == securityTenant.id &&
                                                                                       stu.securityUserId == securityUser.id &&
                                                                                       stu.canRead == true
                                                                                       orderby so.id
                                                                                       select so)
                                                     .Distinct()
                                                     .AsNoTracking()
                                                     .ToList();


                            //
                            // Add in any new ones.
                            //
                            if (securityOrganizationsViaTeam != null && securityOrganizationsViaTeam.Count > 0)
                            {
                                securityOrganizations.AddRange(securityOrganizationsViaTeam);
                            }
                        }


                        foreach (SecurityOrganization securityOrganization in securityOrganizations)
                        {
                            Organization organization = null;

                            //
                            // First, synchronize this organization from the security database into the application database.
                            //
                            using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                            {
                                auditorContext.Configuration.AutoDetectChangesEnabled = true;
                                auditorContext.Configuration.ValidateOnSaveEnabled = false;

                                //
                                // Get the application organization by matching by object guid
                                //
                                bool organizationChangesMade = false;
                                organization = (from x in auditorContext.Organizations where x.objectGuid == securityOrganization.objectGuid select x).FirstOrDefault();

                                //
                                // If we don't have it yet, then add it.  If we have it, then make sure the fields are the same.
                                //
                                if (organization == null)
                                {
                                    organization = new Organization();

                                    //
                                    // Set the object guids here, the rest of the data fields will follow later.
                                    //
                                    organization.objectGuid = securityOrganization.objectGuid;
                                    organization.tenantGuid = securityTenant.objectGuid;

                                    auditorContext.Organizations.Add(organization);

                                    organizationChangesMade = true;
                                }


                                //
                                // Copy the property state from the security organization
                                //
                                if (organization.name != securityOrganization.name)
                                {
                                    organization.name = securityOrganization.name;
                                    organizationChangesMade = true;
                                }

                                if (organization.description != securityOrganization.description)
                                {
                                    organization.description = securityOrganization.description;
                                    organizationChangesMade = true;
                                }

                                if (organization.active != securityOrganization.active || organization.deleted != securityOrganization.deleted)
                                {
                                    organization.active = securityOrganization.active;
                                    organization.deleted = securityOrganization.deleted;
                                    organizationChangesMade = true;
                                }

                                if (organizationChangesMade == true)
                                {
                                    auditorContext.SaveChanges();
                                }

                                //
                                // Now synchronize the users associated to this security organization into the application database.
                                //
                                SynchronizeOrganizationUsers(securityOrganization, securityTenant.objectGuid, securityUser);
                            }
                        }


                        //
                        // First get the departments with explicit read access, or all of them if the user is a module administrator
                        //
                        List<SecurityDepartment> securityDepartments = null;
                        if (userIsModuleAdministrator == true)
                        {
                            //
                            // User is module administrator so they see all of the departments
                            //
                            securityDepartments = (from sd in securityContext.SecurityDepartments
                                                   join so in securityContext.SecurityOrganizations on sd.securityOrganizationId equals so.id
                                                   where
                                                   so.securityTenantId == securityTenant.id 
                                                   orderby sd.id
                                                   select sd)
                                                   .Distinct()
                                                   .Include(""securityOrganization"")
                                                   .AsNoTracking()
                                                   .ToList();
                        }
                        else 
                        {
                            //
                            // First get the departments that the user has explicit read
                            //
                            securityDepartments = (from sd in securityContext.SecurityDepartments
                                                   join sdu in securityContext.SecurityDepartmentUsers on sd.id equals sdu.securityDepartmentId
                                                   join so in securityContext.SecurityOrganizations on sd.securityOrganizationId equals so.id
                                                   where
                                                   so.securityTenantId == securityTenant.id &&
                                                   sdu.securityUserId == securityUser.id &&
                                                   sdu.canRead == true
                                                   orderby sd.id
                                                   select sd)
                                                   .Distinct()
                                                   .Include(""securityOrganization"")
                                                   .AsNoTracking()
                                                   .ToList();

                            //
                            // Now get the departments that are partially readable via team readership
                            //
                            List<int> currentDepartmentIds = (from x in securityDepartments select x.id).ToList();
                            List<SecurityDepartment> securityDepartmentsViaTeam = (from sd in securityContext.SecurityDepartments
                                                                                   join so in securityContext.SecurityOrganizations on sd.securityOrganizationId equals so.id
                                                                                   join st in securityContext.SecurityTeams on sd.id equals st.securityDepartmentId
                                                                                   join stu in securityContext.SecurityTeamUsers on st.id equals stu.securityTeamId
                                                                                   where
                                                                                   currentDepartmentIds.Contains(sd.id) == false &&
                                                                                   stu.securityUserId == securityUser.id &&
                                                                                   stu.canRead == true &&
                                                                                   so.securityTenantId == securityTenant.id
                                                                                   orderby sd.id
                                                                                   select sd)
                                                                            .Distinct()
                                                                            .Include(""securityOrganization"")
                                                                            .AsNoTracking()
                                                                            .ToList();


                            //
                            // Add in the new ones.
                            //
                            if (securityDepartmentsViaTeam != null && securityDepartmentsViaTeam.Count > 0)
                            {
                                securityDepartments.AddRange(securityDepartmentsViaTeam);
                            }


                            //
                            // Now add any departments that belong to organizations that the user can read from.
                            //
                            currentDepartmentIds = (from x in securityDepartments select x.id).ToList();
                            List<SecurityDepartment> securityDepartmentsViaOrg = (from sd in securityContext.SecurityDepartments
                                                                                  join so in securityContext.SecurityOrganizations on sd.securityOrganizationId equals so.id
                                                                                  join sou in securityContext.SecurityOrganizationUsers on so.id equals sou.securityOrganizationId
                                                                                  where
                                                                                  currentDepartmentIds.Contains(sd.id) == false &&
                                                                                  so.securityTenantId == securityTenant.id &&
                                                                                  sou.securityUserId == securityUser.id &&
                                                                                  sou.canRead == true
                                                                                  orderby sd.id
                                                                                  select sd)
                                                                                  .Distinct()
                                                                                  .Include(""securityOrganization"")
                                                                                  .AsNoTracking()
                                                                                  .ToList();


                            //
                            // Add in the new ones.
                            //
                            if (securityDepartmentsViaOrg != null && securityDepartmentsViaOrg.Count > 0)
                            {
                                securityDepartments.AddRange(securityDepartmentsViaOrg);
                            }
                        }


                        foreach (SecurityDepartment securityDepartment in securityDepartments)
                        {

                            //
                            // Synchronize this department from the security database into the application database.
                            //
                            Department department = null;
                            using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                            {
                                auditorContext.Configuration.AutoDetectChangesEnabled = true;
                                auditorContext.Configuration.ValidateOnSaveEnabled = false;

                                //
                                // Get the department from the application database by joining by object guid
                                //
                                bool departmentChangesMade = false;
                                department = (from x in auditorContext.Departments where x.objectGuid == securityDepartment.objectGuid select x).FirstOrDefault();


                                // If we don't have it yet, then add it.  If we have it, then make sure the fields are the same.
                                if (department == null)
                                {
                                    department = new Department();

                                    //
                                    // Set the object guid here, the rest of the data fields will follow later.
                                    //
                                    department.objectGuid = securityDepartment.objectGuid;
                                    department.tenantGuid = securityTenant.objectGuid;

                                    auditorContext.Departments.Add(department);

                                    departmentChangesMade = true;
                                }


                                //
                                // Copy the property state from the security department
                                //
                                Organization organization = (from o in auditorContext.Organizations
                                                             where o.objectGuid == securityDepartment.securityOrganization.objectGuid &&
                                                             o.tenantGuid == securityTenant.objectGuid
                                                             select o).FirstOrDefault();

                                if (department.organizationId != organization.id)
                                {
                                    department.organizationId = organization.id;
                                    departmentChangesMade = true;
                                }

                                if (department.name != securityDepartment.name)
                                {
                                    department.name = securityDepartment.name;
                                    departmentChangesMade = true;
                                }

                                if (department.description != securityDepartment.description)
                                {
                                    department.description = securityDepartment.description;
                                    departmentChangesMade = true;
                                }

                                //
                                // If the security organization is inactive, or deleted, then that state is used for this department.  Otherwise, use the security department value
                                // 
                                if (department.active != securityDepartment.active || department.deleted != securityDepartment.deleted)
                                {
                                    department.active = securityDepartment.active;
                                    department.deleted = securityDepartment.deleted;
                                    departmentChangesMade = true;
                                }

                                if (departmentChangesMade == true)
                                {
                                    auditorContext.SaveChanges();
                                }

                                //
                                // Now synchronize the users associated to this security department into the application database.
                                //
                                SynchronizeDepartmentUsers(securityDepartment, securityTenant.objectGuid, securityUser);
                            }
                        }


                        //
                        // Next synchronize the Teams
                        //
                        List<SecurityTeam> securityTeams = null;


                        if (userIsModuleAdministrator == true)
                        {
                            //
                            // User is administrator, so they see all the teams.
                            //
                            securityTeams = (from st in securityContext.SecurityTeams
                                             join sd in securityContext.SecurityDepartments on st.securityDepartmentId equals sd.id
                                             join so in securityContext.SecurityOrganizations on sd.securityOrganizationId equals so.id
                                             where
                                             so.securityTenantId == securityTenant.id 
                                             orderby st.id
                                             select st)
                                             .Include(""securityDepartment"")
                                             .AsNoTracking()
                                             .ToList();
                        }
                        else
                        {
                            //
                            // First, get the teams that the user has explicit readership to.
                            //
                            securityTeams = (from st in securityContext.SecurityTeams
                                             join stu in securityContext.SecurityTeamUsers on st.id equals stu.securityTeamId
                                             join sd in securityContext.SecurityDepartments on st.securityDepartmentId equals sd.id
                                             join so in securityContext.SecurityOrganizations on sd.securityOrganizationId equals so.id
                                             where
                                             so.securityTenantId == securityTenant.id &&                            
                                             stu.securityUserId == securityUser.id && 
                                             stu.canRead == true
                                             orderby st.id
                                             select st)
                                             .Include(""securityDepartment"")
                                             .AsNoTracking()
                                             .ToList();

                            //
                            // Now get the teams that are readable via department readership
                            //
                            List<int> currentTeamIds = (from x in securityTeams select x.id).ToList();
                            List<SecurityTeam> securityTeamsViaDepartment = (from st in securityContext.SecurityTeams
                                                                             join sd in securityContext.SecurityDepartments on st.securityDepartmentId equals sd.id
                                                                             join so in securityContext.SecurityOrganizations on sd.securityOrganizationId equals so.id
                                                                             join sdu in securityContext.SecurityDepartmentUsers on sd.id equals sdu.securityDepartmentId
                                                                             where
                                                                             currentTeamIds.Contains(st.id) == false &&
                                                                             sdu.securityUserId == securityUser.id &&
                                                                             sdu.canRead == true &&
                                                                             so.securityTenantId == securityTenant.id
                                                                             orderby st.id
                                                                             select st)
                                                                            .Distinct()
                                                                            .Include(""securityDepartment"")
                                                                            .AsNoTracking()
                                                                            .ToList();


                            //
                            // Add in the new ones.
                            //
                            if (securityTeamsViaDepartment != null && securityTeamsViaDepartment.Count > 0)
                            {
                                securityTeams.AddRange(securityTeamsViaDepartment);
                            }

                            //
                            // Now add any teams that belong to organizations that the user can read from.
                            //
                            currentTeamIds = (from x in securityTeams select x.id).ToList();
                            List<SecurityTeam> securityTeamsViaOrg = (from st in securityContext.SecurityTeams
                                                                      join sd in securityContext.SecurityDepartments on st.securityDepartmentId equals sd.id
                                                                      join so in securityContext.SecurityOrganizations on sd.securityOrganizationId equals so.id
                                                                      join sou in securityContext.SecurityOrganizationUsers on so.id equals sou.securityOrganizationId
                                                                      where
                                                                      currentTeamIds.Contains(st.id) == false &&
                                                                      so.securityTenantId == securityTenant.id &&
                                                                      sou.securityUserId == securityUser.id &&
                                                                      sou.canRead == true
                                                                      orderby st.id
                                                                      select st)
                                                                      .Distinct()
                                                                      .Include(""securityDepartment"")
                                                                      .AsNoTracking()
                                                                      .ToList();


                            //
                            // Add in the new ones.
                            //
                            if (securityTeamsViaOrg != null && securityTeamsViaOrg.Count > 0)
                            {
                                securityTeams.AddRange(securityTeamsViaOrg);
                            }
                        }


                        foreach (SecurityTeam securityTeam in securityTeams)
                        {

                            //
                            // Synchronize this team from the security database into the application database.
                            //
                            using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                            {
                                auditorContext.Configuration.AutoDetectChangesEnabled = true;
                                auditorContext.Configuration.ValidateOnSaveEnabled = false;

                                //
                                // Get the team from the application database by matching by object guid
                                //
                                bool teamChangesMade = false;
                                Team team = (from x in auditorContext.Teams where x.objectGuid == securityTeam.objectGuid select x).FirstOrDefault();


                                // If we don't have it yet, then add it.  If we have it, then make sure the fields are the same.
                                if (team == null)
                                {
                                    team = new Team();

                                    //
                                    // Set the object guid here, the rest of the data fields will follow later.
                                    //
                                    team.objectGuid = securityTeam.objectGuid;
                                    team.tenantGuid = securityTenant.objectGuid;

                                    auditorContext.Teams.Add(team);

                                    teamChangesMade = true;
                                }

                                //
                                // Copy the property state from the security department
                                //
                                Department department = (from d in auditorContext.Departments
                                                         where d.objectGuid == securityTeam.securityDepartment.objectGuid &&
                                                         d.tenantGuid == securityTenant.objectGuid
                                                         select d).FirstOrDefault();

                                if (team.departmentId != department.id)
                                {
                                    team.departmentId = department.id;
                                    teamChangesMade = true;
                                }

                                if (team.name != securityTeam.name)
                                {
                                    team.name = securityTeam.name;
                                    teamChangesMade = true;
                                }

                                if (team.description != securityTeam.description)
                                {
                                    team.description = securityTeam.description;
                                    teamChangesMade = true;
                                }

                                if (team.active != securityTeam.active || team.deleted != securityTeam.deleted)
                                {
                                    team.active = securityTeam.active;
                                    team.deleted = securityTeam.deleted;
                                    teamChangesMade = true;
                                }

                                if (teamChangesMade == true)
                                {
                                    auditorContext.SaveChanges();
                                }

                                //
                                // Now synchronize the users associated to this security department into the application database.
                                //
                                SynchronizeTeamUsers(securityTeam, securityTenant.objectGuid, securityUser);
                            }
                        }

                        Foundation.Utility.CreateAuditEvent(""Completed SynchronizeDataVisibilityEntitiesForSingleUser() process.  Security User object guid is: "" + securityUserObjectGuid + "" and user account name is: "" + (securityUser != null ? securityUser.accountName : ""null""));
                    }
                }
                catch (Exception ex)
                {
                    Foundation.Utility.CreateAuditEvent(""Caught error in SynchronizeDataVisibilityEntitiesForSingleUser() process.  Security User object guid is: "" + securityUserObjectGuid + "".  Error is "" + ex.Message, ex);
                }
            }
        }


        //
        // Optional tenant constraint
        //
        public static void SynchronizeDataVisibilityEntities(Guid? tenantGuid, bool forceSyncInSpiteOfLastRunDate = false)
        {
            lock (fullSynchronizationSyncRoot)
            {
                try
                {
                    DateTime? lastRunDate = Foundation.Security.SystemSettings.GetDateTimeSystemSetting(LAST_SYNC_SETTING);
                    int minimumMinutesBetweenCompleteDataVisibilitySyncs = Foundation.Configuration.GetIntegerConfigurationSetting(""MinimumMinutesBetweenCompleteDataVisibilitySyncs"", 240);

                    Foundation.Utility.CreateAuditEvent(""Starting SynchronizeDataVisibilityEntities() process.  Last run date is "" + (lastRunDate.HasValue == true ? (lastRunDate.Value.ToString(""s"")) : "" No last run date"") + "" minimum minutes between syncs is "" + minimumMinutesBetweenCompleteDataVisibilitySyncs.ToString());

                    if (lastRunDate.HasValue == false ||
                        lastRunDate < DateTime.UtcNow.AddMinutes(-1 * minimumMinutesBetweenCompleteDataVisibilitySyncs) ||
                        forceSyncInSpiteOfLastRunDate == true)
                    {
                        Foundation.Security.SystemSettings.SetDateTimeSystemSetting(LAST_SYNC_SETTING, DateTime.UtcNow);

                        Foundation.Utility.CreateAuditEvent(""Beginning synchronization process because sufficient time has elapsed since the last synchronization."");

                        //
                        // First sync the User Title table. It should have a low row count of 100 or less so it doesn't need fancy paging logic.
                        //
                        Dictionary<int, UserTitle> userTitlesAccountedFor = new Dictionary<int, UserTitle>();
                        using (SecurityContext securityContext = new SecurityContext())
                        {
                            securityContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                            Foundation.Utility.CreateAuditEvent(""User Title synchronization is starting."");

                            List<SecurityUserTitle> securityUserTitles = (from x in securityContext.SecurityUserTitles select x).AsNoTracking().ToList();

                            foreach (SecurityUserTitle securityUserTitle in securityUserTitles)
                            {
                                using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                                {
                                    auditorContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                                    auditorContext.Configuration.AutoDetectChangesEnabled = true;
                                    auditorContext.Configuration.ValidateOnSaveEnabled = false;

                                    UserTitle userTitle = (from x in auditorContext.UserTitles where x.objectGuid == securityUserTitle.objectGuid select x).FirstOrDefault();

                                    if (userTitle == null)
                                    {
                                        userTitle = new UserTitle();

                                        userTitle.objectGuid = securityUserTitle.objectGuid;
                                        userTitle.active = securityUserTitle.active;
                                        userTitle.deleted = securityUserTitle.deleted;

                                        userTitle.name = securityUserTitle.name;
                                        userTitle.description = securityUserTitle.description;

                                        auditorContext.UserTitles.Add(userTitle);

                                        auditorContext.SaveChanges();

                                    }
                                    else
                                    {
                                        bool changesMade = false;

                                        if (securityUserTitle.name != userTitle.name ||
                                            securityUserTitle.description != userTitle.description ||
                                            securityUserTitle.active != userTitle.active ||
                                            securityUserTitle.deleted != userTitle.deleted)
                                        {
                                            userTitle.name = securityUserTitle.name;
                                            userTitle.description = securityUserTitle.description;
                                            userTitle.active = securityUserTitle.active;
                                            userTitle.deleted = securityUserTitle.deleted;

                                            changesMade = true;
                                        }

                                        if (changesMade == true)
                                        {
                                            auditorContext.SaveChanges();
                                        }
                                    }

                                    userTitlesAccountedFor.Add(userTitle.id, null);
                                }
                            }

                            //
                            // Delete and inactivate any user titles found only in the application database.
                            //
                            using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                            {
                                auditorContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                                auditorContext.Configuration.AutoDetectChangesEnabled = true;
                                auditorContext.Configuration.ValidateOnSaveEnabled = false;


                                List<int> userTitlesOnFile = (from x in auditorContext.UserTitles select x.id).ToList();

                                foreach (int userTitleId in userTitlesOnFile)
                                {
                                    if (userTitlesAccountedFor.ContainsKey(userTitleId) == false)
                                    {
                                        UserTitle ut = (from x in auditorContext.UserTitles where x.id == userTitleId select x).FirstOrDefault();

                                        if (ut != null)
                                        {
                                            ut.active = false;
                                            ut.deleted = false;

                                            auditorContext.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }

                        Foundation.Utility.CreateAuditEvent(""User Title synchronization is complete."");


                        // 
                        // Synchronize the complete User, Organization, Department, and Team tables from the security database into the application database
                        //
                        int securityTenantCount = 0;

                        using (SecurityContext securityContext = new SecurityContext())
                        {
                            securityTenantCount = (from x in securityContext.SecurityTenants
                                                   where 
                                                   (tenantGuid == null || x.objectGuid == tenantGuid)
                                                   select x).Count();
                        }


                        Foundation.Utility.CreateAuditEvent(""Tenant synchronization starting.  There are "" + securityTenantCount.ToString() + "" tenants to synchronize."");

                        int remainingTenantRecords = securityTenantCount;
                        int tenantPageNumber = 0;

                        while (remainingTenantRecords > 0)
                        {
                            List<SecurityTenant> securityTenants;

                            using (SecurityContext securityContext = new SecurityContext())
                            {
                                securityTenants = (from x in securityContext.SecurityTenants
                                                   where 
                                                   (tenantGuid == null || x.objectGuid == tenantGuid)
                                                   orderby x.id
                                                   select x)
                                                    .Skip(tenantPageNumber * SYNC_PAGE_SIZE)
                                                    .Take(SYNC_PAGE_SIZE)
                                                    .AsNoTracking()
                                                    .ToList();

                            }

                            tenantPageNumber++;
                            remainingTenantRecords -= securityTenants.Count;

                            Foundation.Utility.CreateAuditEvent(""Processing security tenant page "" + tenantPageNumber + "" of "" + securityTenants.Count + "" records and there are "" + remainingTenantRecords + "" to process after this set."");

                            foreach (SecurityTenant securityTenant in securityTenants)
                            {
                                if (securityTenant.active == true && securityTenant.deleted == false)
                                {
                                    //
                                    // First, go throgh all the users that are defined for this tenant in the security system, and make sure that they are in the same in the application database.
                                    //
                                    Dictionary<int, User> applicationDatabaseUsersIdsThatHaveBeenReconciled = new Dictionary<int, User>();

                                    int securityTenantUserCount = 0;

                                    using (SecurityContext securityContext = new SecurityContext())
                                    {
                                        securityTenantUserCount = (from x in securityContext.SecurityTenantUsers where x.securityTenantId == securityTenant.id select x).Count();
                                    }

                                    int remainingTenantUserRecords = securityTenantUserCount;
                                    int tenantUserPageNumber = 0;

                                    while (remainingTenantUserRecords > 0)
                                    {
                                        List<SecurityTenantUser> securityTenantUsers;

                                        using (SecurityContext securityContext = new SecurityContext())
                                        {
                                            securityTenantUsers = (from x in securityContext.SecurityTenantUsers
                                                                   where x.securityTenantId == securityTenant.id
                                                                   orderby x.id
                                                                   select x)
                                                                    .Include(""securityUser.reportsToSecurityUser"")
                                                                    .Include(""securityUser.securityUserTitle"")
                                                                    .Skip(tenantUserPageNumber * SYNC_PAGE_SIZE)
                                                                    .Take(SYNC_PAGE_SIZE)
                                                                    .AsNoTracking()
                                                                    .ToList();
                                        }

                                        tenantUserPageNumber++;
                                        remainingTenantUserRecords -= securityTenantUsers.Count;

                                        //
                                        // Garbage collect between every chunk of data - Not absolutely necessary
                                        //
                                        //GC.Collect();
                                        //GC.WaitForPendingFinalizers();

                                        Foundation.Utility.CreateAuditEvent(""Processing security tenant user page "" + tenantUserPageNumber + "" of "" + securityTenantUsers.Count + "" records and there are "" + remainingTenantUserRecords + "" to process after this set."");

                                        foreach (SecurityTenantUser securityTenantUser in securityTenantUsers)
                                        {
                                            User user = null;

                                            //
                                            // First, synchronize this tenant's users from the security database into the application database.
                                            //
                                            using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                                            {
                                                auditorContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                                                auditorContext.Configuration.AutoDetectChangesEnabled = true;
                                                auditorContext.Configuration.ValidateOnSaveEnabled = false;

                                                //
                                                // Get the application user by matching the user by object guid
                                                //
                                                bool userChangesMade = false;

                                                //
                                                // We may be adjusting this record, so don't use .AsNoTracking() here
                                                //
                                                user = (from x in auditorContext.Users
                                                        where
                                                        x.objectGuid == securityTenantUser.securityUser.objectGuid &&
                                                        x.tenantGuid == securityTenant.objectGuid
                                                        select x).FirstOrDefault();


                                                //
                                                // If we don't have the user yet, then add them.  If the user exists, then make sure the fields are the same.
                                                //
                                                if (user == null)
                                                {
                                                    user = new User();

                                                    //
                                                    // Set the object guids here, the rest of the data fields will follow later.
                                                    //
                                                    user.objectGuid = securityTenantUser.securityUser.objectGuid;
                                                    user.tenantGuid = securityTenant.objectGuid;

                                                    auditorContext.Users.Add(user);

                                                    userChangesMade = true;
                                                }


                                                SecurityUser securityUser = securityTenantUser.securityUser;

                                                //
                                                // Copy the property state from the security organization
                                                //
                                                if (user.accountName != securityUser.accountName)
                                                {
                                                    user.accountName = securityUser.accountName;
                                                    userChangesMade = true;
                                                }


                                                if (user.firstName != securityUser.firstName)
                                                {
                                                    user.firstName = securityUser.firstName;
                                                    userChangesMade = true;
                                                }


                                                if (user.middleName != securityUser.middleName)
                                                {
                                                    user.middleName = securityUser.middleName;
                                                    userChangesMade = true;
                                                }


                                                if (user.lastName != securityUser.lastName)
                                                {
                                                    user.lastName = securityUser.lastName;
                                                    userChangesMade = true;
                                                }

                                                string displayName = securityUser.firstName;

                                                if (string.IsNullOrEmpty(securityUser.middleName) == false)
                                                {
                                                    displayName += "" "";
                                                    displayName += securityUser.middleName;
                                                }

                                                displayName += "" "";
                                                displayName += securityUser.lastName;

                                                if (user.displayName != displayName)
                                                {
                                                    user.displayName = displayName;
                                                    userChangesMade = true;
                                                }


                                                if (user.active != securityTenantUser.active || user.deleted != securityTenantUser.deleted)
                                                {
                                                    user.active = securityTenantUser.active;
                                                    user.deleted = securityTenantUser.deleted;
                                                    userChangesMade = true;
                                                }

                                                //
                                                // Handle the reports to structure here.  This code assumes that the user that this user reports to already exists in the application's User database.  If it does not, then the reports to for this user
                                                // will not be assigned this pass.  It is expected that the missing user will be processed later in this loop, and therefore the next data sync pass will find it, and allow this reports to relationship to be set.
                                                //
                                                // This is good enough for this problem because the data sync process happens very frequently, and mass user creations happen very infrequently.
                                                //
                                                if (securityUser.reportsToSecurityUserId.HasValue == true)
                                                {
                                                    User managerUser = (from x in auditorContext.Users where x.objectGuid == securityUser.reportsToSecurityUser.objectGuid select x).FirstOrDefault();

                                                    if (managerUser != null)
                                                    {
                                                        if (user.reportsToUserId != managerUser.id)
                                                        {
                                                            user.reportsToUserId = managerUser.id;
                                                            userChangesMade = true;
                                                        }
                                                    }
                                                }

                                                if (securityUser.securityUserTitleId.HasValue == true)
                                                {
                                                    UserTitle userTitle = (from x in auditorContext.UserTitles where x.objectGuid == securityUser.securityUserTitle.objectGuid select x).FirstOrDefault();

                                                    if (userTitle == null)
                                                    {
                                                        userTitle = new UserTitle();

                                                        userTitle.objectGuid = securityUser.securityUserTitle.objectGuid;
                                                        userTitle.active = true;
                                                        userTitle.deleted = false;

                                                        userTitle.name = securityUser.securityUserTitle.name;
                                                        userTitle.description = securityUser.securityUserTitle.description;

                                                        using (%MODULENAME%Context userTitleContext = new %MODULENAME%Context())
                                                        {
                                                            userTitleContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                                                            userTitleContext.Configuration.AutoDetectChangesEnabled = true;
                                                            userTitleContext.Configuration.ValidateOnSaveEnabled = false;

                                                            userTitleContext.UserTitles.Add(userTitle);

                                                            userTitleContext.SaveChanges();
                                                        }
                                                    }

                                                    if (userTitle != null)
                                                    {
                                                        if (user.userTitleId != userTitle.id)
                                                        {
                                                            user.userTitleId = userTitle.id;
                                                            userChangesMade = true;
                                                        }
                                                    }
                                                }



                                                if (userChangesMade == true)
                                                {
                                                    auditorContext.SaveChanges();
                                                }

                                                // giving this null or we will run out of memory on large sets.
                                                applicationDatabaseUsersIdsThatHaveBeenReconciled.Add(user.id, null);
                                            }
                                        }
                                    }


                                    //
                                    // Now, check the application database and see if it has any users related to this security tenant that exist, and that were not part of the reconciliation above.  
                                    //
                                    // If any are found, then deactivate and delete them, as they are bad/orphan data and shouldn't be there.
                                    //
                                    using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                                    {
                                        auditorContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                                        //
                                        // This list of integers is highly unlikely to ever throw an out of memory error, so we're not going to page here.
                                        //
                                        List<int> allUsersInTenant = (from x in auditorContext.Users where x.tenantGuid == securityTenant.objectGuid select x.id).ToList();

                                        foreach (int userToCheckId in allUsersInTenant)
                                        {
                                            if (applicationDatabaseUsersIdsThatHaveBeenReconciled.ContainsKey(userToCheckId) == false)
                                            {
                                                //
                                                // Turn off this orphaned user.
                                                //
                                                SetUserActiveDeletedState(userToCheckId, false, false);
                                            }
                                        }
                                    }


                                    //
                                    // Next, go through all of the organizations that are defined for this tenant in the security system, and make sure that they are the same in the application database.
                                    //
                                    int securityOrganizationCount;
                                    using (SecurityContext securityContext = new SecurityContext())
                                    {
                                        securityContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                                        securityOrganizationCount = (from x in securityContext.SecurityOrganizations where x.securityTenantId == securityTenant.id select x).Count();
                                    }


                                    //
                                    // Null is going to be passed into each of these as a data parameter otherwise we will run out of memory on large syncs.  
                                    //
                                    Dictionary<int, Organization> applicationDatabaseOrganizationIdsThatHaveBeenReconciled = new Dictionary<int, Organization>();
                                    Dictionary<int, Department> applicationDatabaseDepartmentIdsThatHaveBeenReconciled = new Dictionary<int, Department>();
                                    Dictionary<int, Team> applicationDatabaseTeamIdsThatHaveBeenReconciled = new Dictionary<int, Team>();

                                    int remainingOrganizationRecords = securityOrganizationCount;
                                    int organizationPageNumber = 0;

                                    while (remainingOrganizationRecords > 0)
                                    {
                                        List<SecurityOrganization> securityOrganizations;

                                        using (SecurityContext securityContext = new SecurityContext())
                                        {
                                            securityContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                                            securityOrganizations = (from o in securityContext.SecurityOrganizations
                                                                     where o.securityTenantId == securityTenant.id
                                                                     orderby o.id
                                                                     select o)
                                                                    .Skip(organizationPageNumber * SYNC_PAGE_SIZE)
                                                                    .Take(SYNC_PAGE_SIZE)
                                                                    .AsNoTracking()
                                                                    .ToList();
                                        }

                                        organizationPageNumber++;
                                        remainingOrganizationRecords -= securityOrganizations.Count;

                                        Foundation.Utility.CreateAuditEvent(""Processing security organization page "" + organizationPageNumber + "" of "" + securityOrganizations.Count + "" records and there are "" + remainingTenantUserRecords + "" to process after this set."");

                                        foreach (SecurityOrganization securityOrganization in securityOrganizations)
                                        {
                                            Organization organization = null;

                                            //
                                            // First, synchronize this organization from the security database into the application database.
                                            //
                                            using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                                            {
                                                auditorContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                                                auditorContext.Configuration.AutoDetectChangesEnabled = true;
                                                auditorContext.Configuration.ValidateOnSaveEnabled = false;

                                                //
                                                // Get the application organization by matching by object guid
                                                //
                                                bool organizationChangesMade = false;
                                                organization = (from x in auditorContext.Organizations where x.objectGuid == securityOrganization.objectGuid select x).FirstOrDefault();

                                                //
                                                // If we don't have it yet, then add it.  If we have it, then make sure the fields are the same.
                                                //
                                                if (organization == null)
                                                {
                                                    organization = new Organization();

                                                    //
                                                    // Set the object guids here, the rest of the data fields will follow later.
                                                    //
                                                    organization.objectGuid = securityOrganization.objectGuid;
                                                    organization.tenantGuid = securityTenant.objectGuid;

                                                    auditorContext.Organizations.Add(organization);

                                                    organizationChangesMade = true;
                                                }


                                                //
                                                // Copy the property state from the security organization
                                                //
                                                if (organization.name != securityOrganization.name)
                                                {
                                                    organization.name = securityOrganization.name;
                                                    organizationChangesMade = true;
                                                }

                                                if (organization.description != securityOrganization.description)
                                                {
                                                    organization.description = securityOrganization.description;
                                                    organizationChangesMade = true;
                                                }

                                                if (organization.active != securityOrganization.active || organization.deleted != securityOrganization.deleted)
                                                {
                                                    organization.active = securityOrganization.active;
                                                    organization.deleted = securityOrganization.deleted;
                                                    organizationChangesMade = true;
                                                }

                                                if (organizationChangesMade == true)
                                                {
                                                    auditorContext.SaveChanges();
                                                }

                                                // giving this null or we will run out of memory on large sets.
                                                applicationDatabaseOrganizationIdsThatHaveBeenReconciled.Add(organization.id, null);

                                                //
                                                // Now synchronize the users associated to this security organization into the application database.
                                                //
                                                SynchronizeOrganizationUsers(securityOrganization, securityTenant.objectGuid);
                                            }

                                            //
                                            // Next synchronize the departments 
                                            //
                                            int securityDepartmentCount = 0;

                                            using (SecurityContext securityContext = new SecurityContext())
                                            {
                                                securityContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                                                securityDepartmentCount = (from x in securityContext.SecurityDepartments where x.securityOrganizationId == securityOrganization.id select x).Count();
                                            }

                                            int remainingDepartmentRecords = securityDepartmentCount;
                                            int departmentPageNumber = 0;

                                            while (remainingDepartmentRecords > 0)
                                            {
                                                List<SecurityDepartment> securityDepartments;

                                                using (SecurityContext securityContext = new SecurityContext())
                                                {
                                                    securityContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                                                    securityDepartments = (from d in securityContext.SecurityDepartments
                                                                           where d.securityOrganizationId == securityOrganization.id
                                                                           orderby d.id
                                                                           select d)
                                                                           .Skip(departmentPageNumber * SYNC_PAGE_SIZE)
                                                                           .Take(SYNC_PAGE_SIZE)
                                                                           .AsNoTracking()
                                                                           .ToList();

                                                }

                                                departmentPageNumber++;
                                                remainingDepartmentRecords -= securityDepartments.Count;

                                                Foundation.Utility.CreateAuditEvent(""Processing security department page "" + departmentPageNumber + "" of "" + securityDepartments.Count + "" records and there are "" + remainingDepartmentRecords + "" to process after this set."");

                                                foreach (SecurityDepartment securityDepartment in securityDepartments)
                                                {
                                                    //
                                                    // First, in case the organization is inactive or deleted, then assign that state into this department.  No need to persist it to the db, but use it for this loop to drive the state in the application db.
                                                    //
                                                    if (securityOrganization.active == false)
                                                    {
                                                        securityDepartment.active = false;
                                                    }
                                                    if (securityOrganization.deleted == true)
                                                    {
                                                        securityDepartment.deleted = true;
                                                    }

                                                    //
                                                    // Synchronize this department from the security database into the application database.
                                                    //
                                                    Department department = null;
                                                    using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                                                    {
                                                        auditorContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                                                        auditorContext.Configuration.AutoDetectChangesEnabled = true;
                                                        auditorContext.Configuration.ValidateOnSaveEnabled = false;

                                                        //
                                                        // Get the department from the application database by joining by object guid
                                                        //
                                                        bool departmentChangesMade = false;
                                                        department = (from x in auditorContext.Departments where x.objectGuid == securityDepartment.objectGuid select x).FirstOrDefault();


                                                        // If we don't have it yet, then add it.  If we have it, then make sure the fields are the same.
                                                        if (department == null)
                                                        {
                                                            department = new Department();

                                                            //
                                                            // Set the object guid here, the rest of the data fields will follow later.
                                                            //
                                                            department.objectGuid = securityDepartment.objectGuid;
                                                            department.tenantGuid = securityTenant.objectGuid;

                                                            auditorContext.Departments.Add(department);

                                                            departmentChangesMade = true;
                                                        }

                                                        //
                                                        // Copy the property state from the security department
                                                        //
                                                        if (department.organizationId != organization.id)
                                                        {
                                                            department.organizationId = organization.id;
                                                            departmentChangesMade = true;
                                                        }

                                                        if (department.name != securityDepartment.name)
                                                        {
                                                            department.name = securityDepartment.name;
                                                            departmentChangesMade = true;
                                                        }

                                                        if (department.description != securityDepartment.description)
                                                        {
                                                            department.description = securityDepartment.description;
                                                            departmentChangesMade = true;
                                                        }

                                                        //
                                                        // If the security organization is inactive, or deleted, then that state is used for this department.  Otherwise, use the security department value
                                                        // 
                                                        if (department.active != securityDepartment.active || department.deleted != securityDepartment.deleted)
                                                        {
                                                            department.active = securityDepartment.active;
                                                            department.deleted = securityDepartment.deleted;
                                                            departmentChangesMade = true;
                                                        }

                                                        if (departmentChangesMade == true)
                                                        {
                                                            auditorContext.SaveChanges();
                                                        }


                                                        // giving this null or we will run out of memory on large sets.
                                                        applicationDatabaseDepartmentIdsThatHaveBeenReconciled.Add(department.id, null);

                                                        //
                                                        // Now synchronize the users associated to this security department into the application database.
                                                        //
                                                        SynchronizeDepartmentUsers(securityDepartment, securityTenant.objectGuid, null);
                                                    }

                                                    //
                                                    // Next synchronize the Teams
                                                    //
                                                    int securityTeamCount = 0;

                                                    using (SecurityContext securityContext = new SecurityContext())
                                                    {
                                                        securityContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                                                        securityTeamCount = (from x in securityContext.SecurityTeams where x.securityDepartmentId == securityDepartment.id select x).Count();
                                                    }

                                                    int remainingTeamRecords = securityTeamCount;
                                                    int teamPageNumber = 0;

                                                    while (remainingTeamRecords > 0)
                                                    {
                                                        List<SecurityTeam> securityTeams;

                                                        using (SecurityContext securityContext = new SecurityContext())
                                                        {
                                                            securityContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                                                            securityTeams = (from t in securityContext.SecurityTeams
                                                                             where t.securityDepartmentId == securityDepartment.id
                                                                             orderby t.id
                                                                             select t)
                                                                            .Skip(teamPageNumber * SYNC_PAGE_SIZE)
                                                                            .Take(SYNC_PAGE_SIZE)
                                                                            .AsNoTracking()
                                                                            .ToList();

                                                        }

                                                        teamPageNumber++;
                                                        remainingTeamRecords -= securityTeams.Count;

                                                        //Foundation.Utility.CreateAuditEvent(""User Title synchronization is complete."");(""Processing security team page "" + teamPageNumber + "" of "" + securityTeams.Count + "" records and there are "" + remainingTeamRecords + "" to process after this set."");

                                                        foreach (SecurityTeam securityTeam in securityTeams)
                                                        {
                                                            //
                                                            // First, in case the department is inactive or deleted, then assign that state into this team.  No need to persist it to the db, but use it for this loop to drive the state in the application db.
                                                            //
                                                            if (securityDepartment.active == false)
                                                            {
                                                                securityTeam.active = false;
                                                            }
                                                            if (securityDepartment.deleted == true)
                                                            {
                                                                securityTeam.deleted = true;
                                                            }


                                                            //
                                                            // Synchronize this team from the security database into the application database.
                                                            //
                                                            using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                                                            {
                                                                auditorContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                                                                auditorContext.Configuration.AutoDetectChangesEnabled = true;
                                                                auditorContext.Configuration.ValidateOnSaveEnabled = false;

                                                                //
                                                                // Get the team from the application database by matching by object guid
                                                                //
                                                                bool teamChangesMade = false;
                                                                Team team = (from x in auditorContext.Teams where x.objectGuid == securityTeam.objectGuid select x).FirstOrDefault();


                                                                // If we don't have it yet, then add it.  If we have it, then make sure the fields are the same.
                                                                if (team == null)
                                                                {
                                                                    team = new Team();

                                                                    //
                                                                    // Set the object guid here, the rest of the data fields will follow later.
                                                                    //
                                                                    team.objectGuid = securityTeam.objectGuid;
                                                                    team.tenantGuid = securityTenant.objectGuid;

                                                                    auditorContext.Teams.Add(team);

                                                                    teamChangesMade = true;
                                                                }

                                                                //
                                                                // Copy the property state from the security department
                                                                //
                                                                if (team.departmentId != department.id)
                                                                {
                                                                    team.departmentId = department.id;
                                                                    teamChangesMade = true;
                                                                }

                                                                if (team.name != securityTeam.name)
                                                                {
                                                                    team.name = securityTeam.name;
                                                                    teamChangesMade = true;
                                                                }

                                                                if (team.description != securityTeam.description)
                                                                {
                                                                    team.description = securityTeam.description;
                                                                    teamChangesMade = true;
                                                                }

                                                                if (team.active != securityTeam.active || team.deleted != securityTeam.deleted)
                                                                {
                                                                    team.active = securityTeam.active;
                                                                    team.deleted = securityTeam.deleted;
                                                                    teamChangesMade = true;
                                                                }

                                                                if (teamChangesMade == true)
                                                                {
                                                                    auditorContext.SaveChanges();
                                                                }


                                                                // giving this null or we will run out of memory on large sets.
                                                                applicationDatabaseTeamIdsThatHaveBeenReconciled.Add(team.id, null);

                                                                //
                                                                // Now synchronize the users associated to this security department into the application database.
                                                                //
                                                                SynchronizeTeamUsers(securityTeam, securityTenant.objectGuid);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }


                                    //
                                    // Now, check the application database and see if it has any organizations related to this security tenant that exist, and that were not part of the reconciliation above.  
                                    //
                                    // If any are found, then deactivate and delete them, as they are bad/orphan data and shouldn't be there.
                                    //
                                    using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                                    {
                                        auditorContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                                        List<int> allOrganizationsInTenant = (from x in auditorContext.Organizations where x.tenantGuid == securityTenant.objectGuid select x.id).ToList();

                                        foreach (int organizationToCheckId in allOrganizationsInTenant)
                                        {
                                            if (applicationDatabaseOrganizationIdsThatHaveBeenReconciled.ContainsKey(organizationToCheckId) == false)
                                            {
                                                //
                                                // Turn off this orphaned organization.
                                                //
                                                SetOrganizationActiveDeletedState(organizationToCheckId, false, false);
                                            }

                                            List<int> allDepartmentsInOrganization = (from x in auditorContext.Departments where x.organizationId == organizationToCheckId select x.id).ToList();

                                            foreach (int departmentToCheckId in allDepartmentsInOrganization)
                                            {
                                                if (applicationDatabaseDepartmentIdsThatHaveBeenReconciled.ContainsKey(departmentToCheckId) == false)
                                                {
                                                    //
                                                    // Turn off this orphaned department.
                                                    //
                                                    SetDepartmentActiveDeletedState(departmentToCheckId, false, false);
                                                }

                                                List<int> allTeamsInDepartment = (from x in auditorContext.Teams where x.departmentId == departmentToCheckId select x.id).ToList();

                                                foreach (int teamToCheckId in allTeamsInDepartment)
                                                {
                                                    if (applicationDatabaseTeamIdsThatHaveBeenReconciled.ContainsKey(teamToCheckId) == false)
                                                    {
                                                        //
                                                        // Turn off this orphaned team.
                                                        //
                                                        SetTeamActiveDeletedState(teamToCheckId, false, false);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //
                                    // This tenant is not active, or they are deleted.  
                                    //
                                    // Cascade this down to the application database applying the same active/deleted state to all of this tenants children in this application database.
                                    //
                                    // This makes it really easy to turn off tenants, by setting the state of all of it's orgs under the active/deleted state of the tenant record.
                                    //
                                    using (%MODULENAME%Context organizationContext = new %MODULENAME%Context())
                                    {
                                        organizationContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                                        //
                                        // To-Do: Add paging here just in case large tenants get de-activated.
                                        // 
                                        //
                                        // Get all the organizations in the application database that are linked to this tenant.
                                        //
                                        List<Organization> organizations = (from x in organizationContext.Organizations where x.tenantGuid == securityTenant.objectGuid select x).ToList();

                                        foreach (Organization org in organizations)
                                        {
                                            SetOrganizationActiveDeletedState(org.id, securityTenant.active, securityTenant.deleted);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Foundation.Utility.CreateAuditEvent(""Not executing synchronization process because insufficient time has elapsed since the last synchronization."");
                    }

                    Foundation.Utility.CreateAuditEvent(""Completed SynchronizeDataVisibilityEntities() process."");
                }
                catch (Exception ex)
                {
                    Foundation.Utility.CreateAuditEvent(""Caught error in SynchronizeDataVisibilityEntities() process.  Error is "" + ex.Message, ex);
                }
            }
        }


        private static void SynchronizeOrganizationUsers(SecurityOrganization securityOrganization, Guid tenantGuid, SecurityUser securityUser = null)
        {
            bool reconcileOrphans = true;

            if (securityUser != null)
            {
                // if we only process one user, we can't do an overall cleanup or we'll blow away all other users.
                reconcileOrphans = false;
            }

            Organization applicationOrganization = null;

            using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
            {
                auditorContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                applicationOrganization = (from x in auditorContext.Organizations where x.objectGuid == securityOrganization.objectGuid select x).FirstOrDefault();
            }

            if (applicationOrganization == null)
            {
                throw new Exception(""Unable to find Organization with guid of "" + securityOrganization.objectGuid + "" in application database."");
            }

            // null is being put into this to stop out of memory errors on large sets of data
            Dictionary<int, OrganizationUser> applicationDatabaseOrganizationUserIdsThatHaveBeenReconciled = new Dictionary<int, OrganizationUser>();

            int securityOrganizationUserCount = 0;

            using (SecurityContext securityContext = new SecurityContext())
            {
                securityContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                if (securityUser != null)
                {
                    securityOrganizationUserCount = (from x in securityContext.SecurityOrganizationUsers
                                                     where x.securityOrganizationId == securityOrganization.id &&
                                                     x.securityUserId == securityUser.id
                                                     select x).Count();
                }
                else
                {
                    securityOrganizationUserCount = (from x in securityContext.SecurityOrganizationUsers
                                                     where x.securityOrganizationId == securityOrganization.id
                                                     select x).Count();
                }
                

            }

            int remainingOrganizationUserRecords = securityOrganizationUserCount;
            int organizationUserPageNumber = 0;

            while (remainingOrganizationUserRecords > 0)
            {
                List<SecurityOrganizationUser> securityOrganizationUsers;

                using (SecurityContext securityContext = new SecurityContext())
                {
                    securityContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                    if (securityUser != null)
                    {
                        securityOrganizationUsers = (from x in securityContext.SecurityOrganizationUsers
                                                     where x.securityOrganizationId == securityOrganization.id
                                                     && x.securityUserId == securityUser.id
                                                     orderby x.id
                                                     select x)
                                                    .Include(""securityUser"")
                                                    .Skip(organizationUserPageNumber * SYNC_PAGE_SIZE)
                                                    .Take(SYNC_PAGE_SIZE)
                                                    .AsNoTracking()
                                                    .ToList();
                    }
                    else
                    {
                        securityOrganizationUsers = (from x in securityContext.SecurityOrganizationUsers
                                                     where x.securityOrganizationId == securityOrganization.id
                                                     orderby x.id
                                                     select x)
                                                    .Include(""securityUser"")
                                                    .Skip(organizationUserPageNumber * SYNC_PAGE_SIZE)
                                                    .Take(SYNC_PAGE_SIZE)
                                                    .AsNoTracking()
                                                    .ToList();
                    }
                }

                organizationUserPageNumber++;
                remainingOrganizationUserRecords -= securityOrganizationUsers.Count;

                Foundation.Utility.CreateAuditEvent(""Processing security organization user page "" + organizationUserPageNumber + "" of "" + securityOrganizationUsers.Count + "" records and there are "" + remainingOrganizationUserRecords + "" to process after this set."");


                foreach (SecurityOrganizationUser securityOrganizationUser in securityOrganizationUsers)
                {
                    //
                    // First, in case the organization is inactive or deleted, then assign that state into this organization User.  No need to persist it to the db, but use it for this loop to drive the state in the application db.
                    //
                    if (securityOrganization.active == false)
                    {
                        securityOrganizationUser.active = false;
                    }
                    if (securityOrganization.deleted == true)
                    {
                        securityOrganizationUser.deleted = true;
                    }

                    //
                    // Synchronize the organization users from the security database into the application database.
                    //
                    using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                    {
                        auditorContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                        auditorContext.Configuration.AutoDetectChangesEnabled = true;
                        auditorContext.Configuration.ValidateOnSaveEnabled = false;

                        OrganizationUser organizationUser = (from x in auditorContext.OrganizationUsers where x.objectGuid == securityOrganizationUser.objectGuid select x).FirstOrDefault();
                        bool organizationUserChangesMade = false;

                        // If we don't have it yet, then add it.  If we have it, then make sure the fields are the same.
                        if (organizationUser == null)
                        {
                            organizationUser = new OrganizationUser();

                            //
                            // Set the object guids here, the rest of the data fields will follow later.
                            //
                            organizationUser.objectGuid = securityOrganizationUser.objectGuid;

                            auditorContext.OrganizationUsers.Add(organizationUser);

                            organizationUserChangesMade = true;
                        }

                        //
                        // Copy the property state from the security organization
                        //

                        //
                        // Get the user that this security organization user links to, and then the matching user in the application db
                        //
                        User user = (from x in auditorContext.Users 
                                     where 
                                     x.objectGuid == securityOrganizationUser.securityUser.objectGuid && 
                                     x.tenantGuid == tenantGuid 
                                     select x).FirstOrDefault();

                        if (user != null)
                        {
                            if (organizationUser.tenantGuid != tenantGuid)
                            {
                                organizationUser.tenantGuid = tenantGuid;
                                organizationUserChangesMade = true;
                            }

                            if (organizationUser.organizationId != applicationOrganization.id)
                            {
                                organizationUser.organizationId = applicationOrganization.id;
                                organizationUserChangesMade = true;
                            }

                            if (organizationUser.userId != user.id)
                            {
                                organizationUser.userId = user.id;
                                organizationUserChangesMade = true;
                            }

                            if (organizationUser.canRead != securityOrganizationUser.canRead)
                            {
                                organizationUser.canRead = securityOrganizationUser.canRead;
                                organizationUserChangesMade = true;
                            }

                            if (organizationUser.canWrite != securityOrganizationUser.canWrite)
                            {
                                organizationUser.canWrite = securityOrganizationUser.canWrite;
                                organizationUserChangesMade = true;
                            }

                            if (organizationUser.canChangeHierarchy != securityOrganizationUser.canChangeHierarchy)
                            {
                                organizationUser.canChangeHierarchy = securityOrganizationUser.canChangeHierarchy;
                                organizationUserChangesMade = true;
                            }

                            if (organizationUser.canChangeOwner != securityOrganizationUser.canChangeOwner)
                            {
                                organizationUser.canChangeOwner = securityOrganizationUser.canChangeOwner;
                                organizationUserChangesMade = true;
                            }

                            if (organizationUser.active != securityOrganizationUser.active || organizationUser.deleted != securityOrganizationUser.deleted)
                            {
                                organizationUser.active = securityOrganizationUser.active;
                                organizationUser.deleted = securityOrganizationUser.deleted;
                                organizationUserChangesMade = true;
                            }

                            if (organizationUserChangesMade == true)
                            {
                                auditorContext.SaveChanges();
                            }

                            applicationDatabaseOrganizationUserIdsThatHaveBeenReconciled.Add(organizationUser.id, null);
                        }
                    }
                }


                if (reconcileOrphans == true)
                {
                    //
                    // Now remove any orphan organization users in the application database. 
                    //
                    List<int> allOrganizationUsersInOrganization;
                    using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                    {
                        auditorContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                        allOrganizationUsersInOrganization = (from x in auditorContext.OrganizationUsers where x.organizationId == applicationOrganization.id select x.id).ToList();
                    }

                    foreach (int organizationUserId in allOrganizationUsersInOrganization)
                    {
                        if (applicationDatabaseOrganizationUserIdsThatHaveBeenReconciled.ContainsKey(organizationUserId) == false)
                        {
                            using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                            {
                                auditorContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                                auditorContext.Configuration.AutoDetectChangesEnabled = true;
                                auditorContext.Configuration.ValidateOnSaveEnabled = false;


                                OrganizationUser organizationUser = (from x in auditorContext.OrganizationUsers where x.id == organizationUserId select x).FirstOrDefault();

                                organizationUser.active = false;
                                organizationUser.deleted = true;

                                auditorContext.SaveChanges();
                            }
                        }
                    }
                }
            }
        }


        private static void SynchronizeDepartmentUsers(SecurityDepartment securityDepartment, Guid tenantGuid, SecurityUser securityUser)
        {
            bool reconcileOrphans = true;

            if (securityUser != null)
            {
                // if we only process one user, we can't do an overall cleanup or we'll blow away all other users.
                reconcileOrphans = false;
            }

            Department applicationDepartment = null;

            using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
            {
                auditorContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                applicationDepartment = (from x in auditorContext.Departments where x.objectGuid == securityDepartment.objectGuid select x).FirstOrDefault();
            }

            if (applicationDepartment == null)
            {
                throw new Exception(""Unable to find Department with guid of "" + securityDepartment.objectGuid + "" in application database."");
            }

            // null is being put into this to stop out of memory errors on large sets of data
            Dictionary<int, DepartmentUser> applicationDatabaseDepartmentUserIdsThatHaveBeenReconciled = new Dictionary<int, DepartmentUser>();

            int securityDepartmentUserCount = 0;

            using (SecurityContext securityContext = new SecurityContext())
            {
                securityContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                if (securityUser != null)
                {
                    securityDepartmentUserCount = (from x in securityContext.SecurityDepartmentUsers
                                                   where x.securityDepartmentId == securityDepartment.id &&
                                                   x.securityUserId == securityUser.id
                                                   select x).Count();

                }
                else
                {
                    securityDepartmentUserCount = (from x in securityContext.SecurityDepartmentUsers
                                                   where x.securityDepartmentId == securityDepartment.id
                                                   select x).Count();
                }
            }

            int remainingDepartmentUserRecords = securityDepartmentUserCount;
            int departmentUserPageNumber = 0;

            while (remainingDepartmentUserRecords > 0)
            {
                List<SecurityDepartmentUser> securityDepartmentUsers;

                using (SecurityContext securityContext = new SecurityContext())
                {
                    securityContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                    if (securityUser != null)
                    {
                        securityDepartmentUsers = (from x in securityContext.SecurityDepartmentUsers
                                                   where x.securityDepartmentId == securityDepartment.id &&
                                                   x.securityUserId == securityUser.id
                                                   orderby x.id
                                                   select x)
                                                    .Include(""securityUser"")
                                                    .Skip(departmentUserPageNumber * SYNC_PAGE_SIZE)
                                                    .Take(SYNC_PAGE_SIZE)
                                                    .AsNoTracking()
                                                    .ToList();
                    }
                    else
                    {
                        securityDepartmentUsers = (from x in securityContext.SecurityDepartmentUsers
                                                   where x.securityDepartmentId == securityDepartment.id
                                                   orderby x.id
                                                   select x)
                                                    .Include(""securityUser"")
                                                    .Skip(departmentUserPageNumber * SYNC_PAGE_SIZE)
                                                    .Take(SYNC_PAGE_SIZE)
                                                    .AsNoTracking()
                                                    .ToList();
                    }
                }

                departmentUserPageNumber++;
                remainingDepartmentUserRecords -= securityDepartmentUsers.Count;

                Foundation.Utility.CreateAuditEvent(""Processing security department user page "" + departmentUserPageNumber + "" of "" + securityDepartmentUsers.Count + "" records and there are "" + remainingDepartmentUserRecords + "" to process after this set.  Department name is "" + securityDepartment.name + "" - "" + securityDepartment.description);


                foreach (SecurityDepartmentUser securityDepartmentUser in securityDepartmentUsers)
                {
                    //
                    // First, in case the department is inactive or deleted, then assign that state into this department User.  No need to persist it to the db, but use it for this loop to drive the state in the application db.
                    //
                    if (securityDepartment.active == false)
                    {
                        securityDepartmentUser.active = false;
                    }
                    if (securityDepartment.deleted == true)
                    {
                        securityDepartmentUser.deleted = true;
                    }

                    //
                    // Synchronize the department users from the security database into the application database.
                    //
                    using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                    {
                        auditorContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                        auditorContext.Configuration.AutoDetectChangesEnabled = true;
                        auditorContext.Configuration.ValidateOnSaveEnabled = false;

                        DepartmentUser departmentUser = (from x in auditorContext.DepartmentUsers where x.objectGuid == securityDepartmentUser.objectGuid select x).FirstOrDefault();
                        bool departmentUserChangesMade = false;

                        // If we don't have it yet, then add it.  If we have it, then make sure the fields are the same.
                        if (departmentUser == null)
                        {
                            departmentUser = new DepartmentUser();

                            //
                            // Set the object guids here, the rest of the data fields will follow later.
                            //
                            departmentUser.objectGuid = securityDepartmentUser.objectGuid;

                            auditorContext.DepartmentUsers.Add(departmentUser);

                            departmentUserChangesMade = true;
                        }

                        //
                        // Copy the property state from the security department
                        //

                        //
                        // Get the user that this security department user links to, and then the matching user in the application db
                        //
                        User user = (from x in auditorContext.Users
                                     where x.objectGuid == securityDepartmentUser.securityUser.objectGuid &&
                                     x.tenantGuid == tenantGuid
                                     select x).FirstOrDefault();

                        if (user != null)
                        {
                            if (departmentUser.tenantGuid != tenantGuid)
                            {
                                departmentUser.tenantGuid = tenantGuid;
                                departmentUserChangesMade = true;
                            }

                            if (departmentUser.departmentId != applicationDepartment.id)
                            {
                                departmentUser.departmentId = applicationDepartment.id;
                                departmentUserChangesMade = true;
                            }

                            if (departmentUser.userId != user.id)
                            {
                                departmentUser.userId = user.id;
                                departmentUserChangesMade = true;
                            }

                            if (departmentUser.canRead != securityDepartmentUser.canRead)
                            {
                                departmentUser.canRead = securityDepartmentUser.canRead;
                                departmentUserChangesMade = true;
                            }

                            if (departmentUser.canWrite != securityDepartmentUser.canWrite)
                            {
                                departmentUser.canWrite = securityDepartmentUser.canWrite;
                                departmentUserChangesMade = true;
                            }

                            if (departmentUser.canChangeHierarchy != securityDepartmentUser.canChangeHierarchy)
                            {
                                departmentUser.canChangeHierarchy = securityDepartmentUser.canChangeHierarchy;
                                departmentUserChangesMade = true;
                            }

                            if (departmentUser.canChangeOwner != securityDepartmentUser.canChangeOwner)
                            {
                                departmentUser.canChangeOwner = securityDepartmentUser.canChangeOwner;
                                departmentUserChangesMade = true;
                            }

                            if (departmentUser.active != securityDepartmentUser.active || departmentUser.deleted != securityDepartmentUser.deleted)
                            {
                                departmentUser.active = securityDepartmentUser.active;
                                departmentUser.deleted = securityDepartmentUser.deleted;
                                departmentUserChangesMade = true;
                            }

                            if (departmentUserChangesMade == true)
                            {
                                auditorContext.SaveChanges();
                            }


                            applicationDatabaseDepartmentUserIdsThatHaveBeenReconciled.Add(departmentUser.id, null);
                        }
                    }
                }

                if (reconcileOrphans == true)
                {
                    //
                    // Now remove any orphan department users in the application database. 
                    //
                    List<int> allDepartmentUsersInDepartment;
                    using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                    {
                        auditorContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                        allDepartmentUsersInDepartment = (from x in auditorContext.DepartmentUsers where x.departmentId == applicationDepartment.id select x.id).ToList();
                    }

                    foreach (int departmentUserId in allDepartmentUsersInDepartment)
                    {
                        if (applicationDatabaseDepartmentUserIdsThatHaveBeenReconciled.ContainsKey(departmentUserId) == false)
                        {
                            using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                            {
                                auditorContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                                auditorContext.Configuration.AutoDetectChangesEnabled = true;
                                auditorContext.Configuration.ValidateOnSaveEnabled = false;


                                DepartmentUser departmentUser = (from x in auditorContext.DepartmentUsers where x.id == departmentUserId select x).FirstOrDefault();

                                departmentUser.active = false;
                                departmentUser.deleted = true;

                                auditorContext.SaveChanges();
                            }
                        }
                    }
                }
            }
        }


        private static void SynchronizeTeamUsers(SecurityTeam securityTeam, Guid tenantGuid, SecurityUser securityUser = null)
        {
            bool reconcileOrphans = true;

            if (securityUser != null)
            {
                // if we only process one user, we can't do an overall cleanup or we'll blow away all other users.
                reconcileOrphans = false;
            }

            Team applicationTeam = null;

            using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
            {
                auditorContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                applicationTeam = (from x in auditorContext.Teams where x.objectGuid == securityTeam.objectGuid select x).FirstOrDefault();
            }

            if (applicationTeam == null)
            {
                throw new Exception(""Unable to find Team with guid of "" + securityTeam.objectGuid + "" in application database."");
            }

            // null is being put into this to stop out of memory errors on large sets of data
            Dictionary<int, TeamUser> applicationDatabaseTeamUserIdsThatHaveBeenReconciled = new Dictionary<int, TeamUser>();

            int securityTeamUserCount = 0;

            using (SecurityContext securityContext = new SecurityContext())
            {
                securityContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                if (securityUser != null)
                {
                    securityTeamUserCount = (from x in securityContext.SecurityTeamUsers 
                                             where x.securityTeamId == securityTeam.id &&
                                             x.securityUserId == securityUser.id
                                             select x).Count();
                }
                else
                {
                    securityTeamUserCount = (from x in securityContext.SecurityTeamUsers
                                             where x.securityTeamId == securityTeam.id
                                             select x).Count();
                }
            }

            int remainingTeamUserRecords = securityTeamUserCount;
            int teamUserPageNumber = 0;

            while (remainingTeamUserRecords > 0)
            {
                List<SecurityTeamUser> securityTeamUsers;

                using (SecurityContext securityContext = new SecurityContext())
                {
                    securityContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                    if (securityUser != null)
                    {
                        securityTeamUsers = (from x in securityContext.SecurityTeamUsers
                                             where x.securityTeamId == securityTeam.id &&
                                             x.securityUserId == securityUser.id
                                             orderby x.id
                                             select x)
                                            .Include(""securityUser"")
                                            .Skip(teamUserPageNumber * SYNC_PAGE_SIZE)
                                            .Take(SYNC_PAGE_SIZE)
                                            .AsNoTracking()
                                            .ToList();
                    }
                    else
                    {
                        securityTeamUsers = (from x in securityContext.SecurityTeamUsers
                                             where x.securityTeamId == securityTeam.id 
                                             orderby x.id
                                             select x)
                                            .Include(""securityUser"")
                                            .Skip(teamUserPageNumber * SYNC_PAGE_SIZE)
                                            .Take(SYNC_PAGE_SIZE)
                                            .AsNoTracking()
                                            .ToList();
                    }
                }

                teamUserPageNumber++;
                remainingTeamUserRecords -= securityTeamUsers.Count;

                //Foundation.Utility.CreateAuditEvent(""User Title synchronization is complete."");(""Processing security team user page "" + teamUserPageNumber + "" of "" + securityTeamUsers.Count + "" records and there are "" + remainingTeamUserRecords + "" to process after this set."");

                foreach (SecurityTeamUser securityTeamUser in securityTeamUsers)
                {
                    //
                    // First, in case the team is inactive or deleted, then assign that state into this team User.  No need to persist it to the db, but use it for this loop to drive the state in the application db.
                    //
                    if (securityTeam.active == false)
                    {
                        securityTeamUser.active = false;
                    }
                    if (securityTeam.deleted == true)
                    {
                        securityTeamUser.deleted = true;
                    }

                    //
                    // Synchronize the team users from the security database into the application database.
                    //
                    using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                    {
                        auditorContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                        auditorContext.Configuration.AutoDetectChangesEnabled = true;
                        auditorContext.Configuration.ValidateOnSaveEnabled = false;

                        TeamUser teamUser = (from x in auditorContext.TeamUsers where x.objectGuid == securityTeamUser.objectGuid select x).FirstOrDefault();
                        bool teamUserChangesMade = false;

                        // If we don't have it yet, then add it.  If we have it, then make sure the fields are the same.
                        if (teamUser == null)
                        {
                            teamUser = new TeamUser();

                            //
                            // Set the object guids here, the rest of the data fields will follow later.
                            //
                            teamUser.objectGuid = securityTeamUser.objectGuid;

                            auditorContext.TeamUsers.Add(teamUser);

                            teamUserChangesMade = true;
                        }

                        //
                        // Copy the property state from the security team
                        //

                        //
                        // Get the user that this security team user links to, and then the matching user in the application db
                        //
                        User user = (from x in auditorContext.Users 
                                     where 
                                     x.objectGuid == securityTeamUser.securityUser.objectGuid &&
                                     x.tenantGuid == tenantGuid
                                     select x).FirstOrDefault();

                        if (user != null)
                        {
                            if (teamUser.tenantGuid != tenantGuid)
                            {
                                teamUser.tenantGuid = tenantGuid;
                                teamUserChangesMade = true;
                            }

                            if (teamUser.teamId != applicationTeam.id)
                            {
                                teamUser.teamId = applicationTeam.id;
                                teamUserChangesMade = true;
                            }

                            if (teamUser.userId != user.id)
                            {
                                teamUser.userId = user.id;
                                teamUserChangesMade = true;
                            }

                            if (teamUser.canRead != securityTeamUser.canRead)
                            {
                                teamUser.canRead = securityTeamUser.canRead;
                                teamUserChangesMade = true;
                            }

                            if (teamUser.canWrite != securityTeamUser.canWrite)
                            {
                                teamUser.canWrite = securityTeamUser.canWrite;
                                teamUserChangesMade = true;
                            }

                            if (teamUser.canChangeHierarchy != securityTeamUser.canChangeHierarchy)
                            {
                                teamUser.canChangeHierarchy = securityTeamUser.canChangeHierarchy;
                                teamUserChangesMade = true;
                            }

                            if (teamUser.canChangeOwner != securityTeamUser.canChangeOwner)
                            {
                                teamUser.canChangeOwner = securityTeamUser.canChangeOwner;
                                teamUserChangesMade = true;
                            }

                            if (teamUser.active != securityTeamUser.active || teamUser.deleted != securityTeamUser.deleted)
                            {
                                teamUser.active = securityTeamUser.active;
                                teamUser.deleted = securityTeamUser.deleted;
                                teamUserChangesMade = true;
                            }

                            if (teamUserChangesMade == true)
                            {
                                auditorContext.SaveChanges();
                            }

                            applicationDatabaseTeamUserIdsThatHaveBeenReconciled.Add(teamUser.id, null);
                        }
                    }
                }

                if (reconcileOrphans == true)
                {
                    //
                    // Now remove any orphan team users in the application database. 
                    //
                    List<int> allTeamUsersInTeam;
                    using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                    {
                        auditorContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                        allTeamUsersInTeam = (from x in auditorContext.TeamUsers where x.teamId == applicationTeam.id select x.id).ToList();
                    }

                    foreach (int teamUserId in allTeamUsersInTeam)
                    {
                        if (applicationDatabaseTeamUserIdsThatHaveBeenReconciled.ContainsKey(teamUserId) == false)
                        {
                            using (%MODULENAME%Context auditorContext = new %MODULENAME%Context())
                            {
                                auditorContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                                auditorContext.Configuration.AutoDetectChangesEnabled = true;
                                auditorContext.Configuration.ValidateOnSaveEnabled = false;

                                TeamUser teamUser = (from x in auditorContext.TeamUsers where x.id == teamUserId select x).FirstOrDefault();

                                teamUser.active = false;
                                teamUser.deleted = true;

                                auditorContext.SaveChanges();
                            }
                        }
                    }
                }
            }
        }


        private static void SetUserActiveDeletedState(int userId, bool active, bool deleted)
        {
            //
            // This is designed to apply an active/deleted state across all elements of an organization and its children
            //
            using (%MODULENAME%Context userContext = new %MODULENAME%Context())
            {
                userContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                userContext.Configuration.AutoDetectChangesEnabled = true;
                userContext.Configuration.ValidateOnSaveEnabled = false;

                //
                User user = (from x in userContext.Users where x.id == userId select x).FirstOrDefault();

                //
                // Copy the active/deleted property state from the security tenant
                //
                user.active = active;
                user.deleted = deleted;

                userContext.SaveChanges();
            }
        }


        private static void SetOrganizationActiveDeletedState(int organizationId, bool active, bool deleted)
        {
            //
            // This is designed to apply an active/deleted state across all elements of an organization and its children
            //
            using (%MODULENAME%Context organizationContext = new %MODULENAME%Context())
            {
                organizationContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                organizationContext.Configuration.AutoDetectChangesEnabled = true;
                organizationContext.Configuration.ValidateOnSaveEnabled = false;

                //
                // Get the orgnization sent in as a param
                //
                Organization org = (from x in organizationContext.Organizations where x.id == organizationId select x).FirstOrDefault();

                //
                // Copy the active/deleted property state from the security tenant
                //
                org.active = active;
                org.deleted = deleted;

                organizationContext.SaveChanges();

                //
                // Now update this same state for all of the child tables under this organization.
                //
                using (%MODULENAME%Context organizationUserContext = new %MODULENAME%Context())
                {
                    organizationUserContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                    organizationUserContext.Configuration.AutoDetectChangesEnabled = true;
                    organizationUserContext.Configuration.ValidateOnSaveEnabled = false;

                    string sql = ""UPDATE [%MODULENAME%].[OrganizationUser] SET active = "" + (active == true ? ""1"" : ""0"") + "", deleted = "" + (deleted == true ? ""1"" : ""0"") + "" WHERE organizationId = "" + org.id.ToString();
                    organizationUserContext.Database.ExecuteSqlCommand(sql);

                    organizationUserContext.SaveChanges();
                }

                using (%MODULENAME%Context departmentContext = new %MODULENAME%Context())
                {
                    departmentContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                    // To-Do: make this loop a SQL update
                    List<int> organizationDepartmentIds = (from x in departmentContext.Departments where x.organizationId == org.id select x.id).ToList();

                    foreach (int departmentId in organizationDepartmentIds)
                    {
                        SetDepartmentActiveDeletedState(departmentId, active, deleted);
                    }
                }
            }
        }


        private static void SetDepartmentActiveDeletedState(int departmentId, bool active, bool deleted)
        {
            //
            // This is designed to apply an active/deleted state across all elements of a department and its children
            //
            using (%MODULENAME%Context departmentContext = new %MODULENAME%Context())
            {
                departmentContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                departmentContext.Configuration.AutoDetectChangesEnabled = true;
                departmentContext.Configuration.ValidateOnSaveEnabled = false;

                Department department = (from x in departmentContext.Departments where x.id == departmentId select x).FirstOrDefault();

                department.active = active;
                department.deleted = deleted;

                departmentContext.SaveChanges();

                using (%MODULENAME%Context departmentUserContext = new %MODULENAME%Context())
                {
                    departmentUserContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                    departmentUserContext.Configuration.AutoDetectChangesEnabled = true;
                    departmentUserContext.Configuration.ValidateOnSaveEnabled = false;

                    string sql = ""UPDATE [%MODULENAME%].[DepartmentUser] SET active = "" + (active == true ? ""1"" : ""0"") + "", deleted = "" + (deleted == true ? ""1"" : ""0"") + "" WHERE departmentId = "" + department.id.ToString();
                    departmentUserContext.Database.ExecuteSqlCommand(sql);

                    departmentUserContext.SaveChanges();
                }


                using (%MODULENAME%Context teamContext = new %MODULENAME%Context())
                {
                    teamContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                    // To-Do: make this loop a SQL update
                    List<int> departmentTeamIds = (from x in teamContext.Teams where x.departmentId == department.id select x.id).ToList();

                    foreach (int teamId in departmentTeamIds)
                    {
                        SetTeamActiveDeletedState(teamId, active, deleted);
                    }
                }
            }
        }


        private static void SetTeamActiveDeletedState(int teamId, bool active, bool deleted)
        {
            //
            // This is designed to apply an active/deleted state across all elements of a team and its children
            //
            using (%MODULENAME%Context teamContext = new %MODULENAME%Context())
            {
                teamContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                teamContext.Configuration.AutoDetectChangesEnabled = true;
                teamContext.Configuration.ValidateOnSaveEnabled = false;

                Team team = (from x in teamContext.Teams where x.id == teamId select x).FirstOrDefault();

                team.active = active;
                team.deleted = deleted;

                using (%MODULENAME%Context teamUserContext = new %MODULENAME%Context())
                {
                    teamUserContext.Database.SetCommandTimeout(SYNC_COMMAND_TIMEOUT);

                    teamUserContext.Configuration.AutoDetectChangesEnabled = true;
                    teamUserContext.Configuration.ValidateOnSaveEnabled = false;

                    string sql = ""UPDATE [%MODULENAME%].[TeamUser] SET active = "" + (active == true ? ""1"" : ""0"") + "", deleted = "" + (deleted == true ? ""1"" : ""0"") + "" WHERE teamId = "" + team.id.ToString();
                    teamUserContext.Database.ExecuteSqlCommand(sql);

                    teamUserContext.SaveChanges();
                }

                teamContext.SaveChanges();
            }
        }
    }
}";

            return webApiBaseClassCode.Replace("%MODULENAME%", moduleName).Replace("%DEFAULTPAGESIZE%", defaultPageSize.ToString());
        }
    }
}
