//using Foundation.Alerting.Database;
//using Foundation.Security.Database;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Alerting.Server.Services
//{
//    /// <summary>
//    /// 
//    /// Provides access methods for users and teams for the Alerting module.  
//    /// 
//    /// It sources its data from the Security database, and constrains on users that belong to the same tenant as the user
//    /// 
//    /// </summary>
//    public class UserService : IUserService
//    {
//        private readonly AlertingContext _alertingContext;
//        private readonly SecurityContext _securityContext;
//        private readonly ILogger<UserService> _logger;


//        public UserService(AlertingContext context, SecurityContext securityContext, ILogger<UserService> logger)
//        {
//            _alertingContext = context;
//            _securityContext = securityContext;
//            _logger = logger;
//        }

//        /// <summary>
//        /// 
//        /// Gets all applicable users from the security module
//        /// 
//        /// - Users must belong to the tenant guid provided
//        /// - Users must have a security role that belongs to the Alerting module
//        /// - Users must be active and not deleted
//        /// 
//        /// </summary>
//        public async Task<List<User>> GetUsersAsync(Guid tenantGuid, CancellationToken cancellationToken = default)
//        {
//            return await (from stu in _securityContext.SecurityTenantUsers
//                          join su in _securityContext.SecurityUsers on stu.securityTenantId equals su.securityTenantId
//                          join st in _securityContext.SecurityTenants on stu.securityTenantId equals st.id
//                          join sur in _securityContext.SecurityUserSecurityRoles on su.id equals sur.securityUserId
//                          join sr in _securityContext.SecurityRoles on sur.securityRoleId equals sr.id
//                          join smr in _securityContext.ModuleSecurityRoles on sr.id equals smr.securityRoleId
//                          join m in _securityContext.Modules on smr.moduleId equals m.id
//                          where st.objectGuid == tenantGuid && st.active == true && st.deleted == false &&
//                          stu.active == true && stu.deleted == false &&
//                          su.active == true && su.deleted == false &&
//                          sur.active == true && sur.deleted == false &&
//                          sr.active == true && sr.deleted == false &&
//                          smr.active == true && smr.deleted == false &&
//                          m.active == true && m.deleted == false &&
//                          m.name == "Alerting"
//                          select new User()
//                          {
//                              accountName = su.accountName,
//                              firstName = su.firstName,
//                              middleName = su.middleName,
//                              lastName = su.lastName,
//                              dateOfBirth = su.dateOfBirth,
//                              emailAddress = su.emailAddress,
//                              cellPhoneNumber = su.cellPhoneNumber,
//                              phoneNumber = su.phoneNumber,
//                              phoneExtension = su.phoneExtension,
//                              description = su.description,
//                              image = su.image,
//                              securityOrganizationGuid = su.securityOrganization.objectGuid,
//                              securityDepartmentGuid = su.securityDepartment.objectGuid,
//                              securityTeamGuid = su.securityTeam.objectGuid,
//                              objectGuid = su.objectGuid,
//                          })
//                          .DistinctBy(u => u.objectGuid)
//                          .ToListAsync(cancellationToken).ConfigureAwait(false);
//        }

//        public async Task<User> GetUserAsync(Guid tenantGuid, Guid userGuid, CancellationToken cancellationToken = default)
//        {
//            return await (from stu in _securityContext.SecurityTenantUsers
//                          join su in _securityContext.SecurityUsers on stu.securityTenantId equals su.securityTenantId
//                          join st in _securityContext.SecurityTenants on stu.securityTenantId equals st.id
//                          join sur in _securityContext.SecurityUserSecurityRoles on su.id equals sur.securityUserId
//                          join sr in _securityContext.SecurityRoles on sur.securityRoleId equals sr.id
//                          join smr in _securityContext.ModuleSecurityRoles on sr.id equals smr.securityRoleId
//                          join m in _securityContext.Modules on smr.moduleId equals m.id
//                          where st.objectGuid == tenantGuid &&
//                          stu.active == true && stu.deleted == false &&
//                          su.objectGuid == userGuid && su.active == true && su.deleted == false &&
//                          sur.active == true && sur.deleted == false &&
//                          sr.active == true && sr.deleted == false &&
//                          smr.active == true && smr.deleted == false &&
//                          m.active == true && m.deleted == false &&
//                          m.name == "Alerting"

//                          select new User()
//                          {
//                              accountName = su.accountName,
//                              firstName = su.firstName,
//                              middleName = su.middleName,
//                              lastName = su.lastName,
//                              dateOfBirth = su.dateOfBirth,
//                              emailAddress = su.emailAddress,
//                              cellPhoneNumber = su.cellPhoneNumber,
//                              phoneNumber = su.phoneNumber,
//                              phoneExtension = su.phoneExtension,
//                              description = su.description,
//                              image = su.image,
//                              securityOrganizationGuid = su.securityOrganization.objectGuid,
//                              securityDepartmentGuid = su.securityDepartment.objectGuid,
//                              securityTeamGuid = su.securityTeam.objectGuid,
//                              objectGuid = su.objectGuid,
//                          }).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

//        }

//        public async Task<List<Team>> GetTeamsAsync(Guid tenantGuid, CancellationToken cancellationToken = default)
//        {
//            return await (from st in _securityContext.SecurityTenants 
//                          join sto in _securityContext.SecurityOrganizations on st.id equals sto.securityTenantId
//                          join std in _securityContext.SecurityDepartments on sto.id equals std.securityOrganizationId
//                          join stt in _securityContext.SecurityTeams on std.id equals stt.securityDepartmentId
//                          where st.objectGuid == tenantGuid && st.active == true && st.deleted == false &&
//                          stt.active == true && stt.deleted == false &&
//                          sto.active == true && sto.deleted == false &&
//                          std.active == true && std.deleted == false
//                          select new Team()
//                          {
//                              name = stt.name,
//                              description = stt.description,
//                              objectGuid = stt.objectGuid,
//                              Users = stt.SecurityUsers.Select(su => new User() {
//                                  accountName = su.accountName,
//                                  firstName = su.firstName,
//                                  middleName = su.middleName,
//                                  lastName = su.lastName,
//                                  dateOfBirth = su.dateOfBirth,
//                                  emailAddress = su.emailAddress,
//                                  cellPhoneNumber = su.cellPhoneNumber,
//                                  phoneNumber = su.phoneNumber,
//                                  phoneExtension = su.phoneExtension,
//                                  description = su.description,
//                                  image = su.image,
//                                  securityOrganizationGuid = su.securityOrganization.objectGuid,
//                                  securityDepartmentGuid = su.securityDepartment.objectGuid,
//                                  securityTeamGuid = su.securityTeam.objectGuid,
//                                  objectGuid = su.objectGuid,
//                              }).ToList(),
//                          }).ToListAsync(cancellationToken).ConfigureAwait(false);

//        }

//        public async Task<Team> GetTeamAsync(Guid tenantGuid, Guid teamGuid, CancellationToken cancellationToken = default)
//        {
//            return await (from st in _securityContext.SecurityTenants
//                          join sto in _securityContext.SecurityOrganizations on st.id equals sto.securityTenantId
//                          join std in _securityContext.SecurityDepartments on sto.id equals std.securityOrganizationId
//                          join stt in _securityContext.SecurityTeams on std.id equals stt.securityDepartmentId
//                          where st.objectGuid == tenantGuid && st.active == true && st.deleted == false &&
//                          stt.objectGuid == teamGuid && stt.active == true && stt.deleted == false &&
//                          sto.active == true && sto.deleted == false &&
//                          std.active == true && std.deleted == false
//                          select new Team()
//                          {
//                              name = stt.name,
//                              description = stt.description,
//                              objectGuid = stt.objectGuid,
//                              Users = stt.SecurityUsers.Select(su => new User()
//                              {
//                                  accountName = su.accountName,
//                                  firstName = su.firstName,
//                                  middleName = su.middleName,
//                                  lastName = su.lastName,
//                                  dateOfBirth = su.dateOfBirth,
//                                  emailAddress = su.emailAddress,
//                                  cellPhoneNumber = su.cellPhoneNumber,
//                                  phoneNumber = su.phoneNumber,
//                                  phoneExtension = su.phoneExtension,
//                                  description = su.description,
//                                  image = su.image,
//                                  securityOrganizationGuid = su.securityOrganization.objectGuid,
//                                  securityDepartmentGuid = su.securityDepartment.objectGuid,
//                                  securityTeamGuid = su.securityTeam.objectGuid,
//                                  objectGuid = su.objectGuid,
//                              }).ToList(),
//                          }).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

//        }

//    }
//}

using Foundation.Alerting.Database;
using Foundation.Security.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Alerting.Server.Services
{
    /// <summary>
    /// 
    /// Provides access methods for users and teams for the Alerting module.
    /// 
    /// It sources its data from the Security database, and constrains on users that belong to the same tenant.
    /// 
    /// </summary>
    public class UserService : IUserService
    {
        private readonly AlertingContext _alertingContext;
        private readonly SecurityContext _securityContext;
        private readonly ILogger<UserService> _logger;

        public UserService(AlertingContext context, SecurityContext securityContext, ILogger<UserService> logger)
        {
            _alertingContext = context;
            _securityContext = securityContext;
            _logger = logger;
        }

        private static User ProjectToUser(SecurityUser su) => new()
        {
            accountName = su.accountName,
            firstName = su.firstName,
            middleName = su.middleName,
            lastName = su.lastName,
            dateOfBirth = su.dateOfBirth,
            emailAddress = su.emailAddress,
            cellPhoneNumber = su.cellPhoneNumber,
            phoneNumber = su.phoneNumber,
            phoneExtension = su.phoneExtension,
            description = su.description,
            image = su.image,
            securityOrganizationGuid = su.securityOrganization?.objectGuid,
            securityDepartmentGuid = su.securityDepartment?.objectGuid,
            securityTeamGuid = su.securityTeam?.objectGuid,
            objectGuid = su.objectGuid,
        };

        private async Task<List<User>> GetApplicableUsersAsync(
            Guid tenantGuid,
            Guid? teamGuid = null,
            Guid? userGuid = null,
            CancellationToken cancellationToken = default)
        {
            var query = from stu in _securityContext.SecurityTenantUsers
                        join su in _securityContext.SecurityUsers on stu.securityTenantId equals su.securityTenantId
                        join st in _securityContext.SecurityTenants on stu.securityTenantId equals st.id
                        join sur in _securityContext.SecurityUserSecurityRoles on su.id equals sur.securityUserId
                        join sr in _securityContext.SecurityRoles on sur.securityRoleId equals sr.id
                        join smr in _securityContext.ModuleSecurityRoles on sr.id equals smr.securityRoleId
                        join m in _securityContext.Modules on smr.moduleId equals m.id
                        where st.objectGuid == tenantGuid &&
                              st.active && !st.deleted &&
                              stu.active && !stu.deleted &&
                              su.active && !su.deleted &&
                              sur.active && !sur.deleted &&
                              sr.active && !sr.deleted &&
                              smr.active && !smr.deleted &&
                              m.active && !m.deleted &&
                              m.name == "Alerting"
                        select su;

            if (teamGuid.HasValue)
            {
                query = query.Where(su => su.securityTeam != null &&
                                          su.securityTeam.objectGuid == teamGuid.Value &&
                                          su.securityTeam.active && !su.securityTeam.deleted &&
                                          su.securityTeam.securityDepartment != null &&
                                          su.securityTeam.securityDepartment.active && !su.securityTeam.securityDepartment.deleted &&
                                          su.securityTeam.securityDepartment.securityOrganization != null &&
                                          su.securityTeam.securityDepartment.securityOrganization.active && !su.securityTeam.securityDepartment.securityOrganization.deleted);
            }

            if (userGuid.HasValue)
            {
                query = query.Where(su => su.objectGuid == userGuid.Value);
            }

            var securityUsers = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

            return securityUsers
                .DistinctBy(su => su.objectGuid)
                .Select(ProjectToUser)
                .ToList();
        }

        /// <summary>
        /// Gets all applicable users from the security module
        /// - Users must belong to the tenant guid provided
        /// - Users must have a security role that belongs to the Alerting module
        /// - Users must be active and not deleted
        /// </summary>
        public async Task<List<User>> GetUsersAsync(Guid tenantGuid, CancellationToken cancellationToken = default)
        {
            return await GetApplicableUsersAsync(tenantGuid, cancellationToken: cancellationToken);
        }

        public async Task<User> GetUserAsync(Guid tenantGuid, Guid userGuid, CancellationToken cancellationToken = default)
        {
            List<User> users = await GetApplicableUsersAsync(tenantGuid, userGuid: userGuid, cancellationToken: cancellationToken);

            return users.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// Gets all applicable users that belong to the specified team
        /// 
        /// - Same constraints as GetUsersAsync, plus membership in the team
        /// - Returns empty list if team does not exist or has no applicable members
        /// 
        /// </summary>
        public async Task<List<User>> GetTeamUsersAsync(Guid tenantGuid, Guid teamGuid, CancellationToken cancellationToken = default)
        {
            return await GetApplicableUsersAsync(tenantGuid, teamGuid, cancellationToken: cancellationToken);
        }

        public async Task<List<Team>> GetTeamsAsync(Guid tenantGuid, CancellationToken cancellationToken = default)
        {
            return await (from st in _securityContext.SecurityTenants
                          join sto in _securityContext.SecurityOrganizations on st.id equals sto.securityTenantId
                          join std in _securityContext.SecurityDepartments on sto.id equals std.securityOrganizationId
                          join stt in _securityContext.SecurityTeams on std.id equals stt.securityDepartmentId
                          where st.objectGuid == tenantGuid &&
                                st.active && !st.deleted &&
                                sto.active && !sto.deleted &&
                                std.active && !std.deleted &&
                                stt.active && !stt.deleted
                          select new Team
                          {
                              name = stt.name,
                              description = stt.description,
                              objectGuid = stt.objectGuid
                              // Users intentionally omitted — use GetTeamUsersAsync for filtered members
                          }).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<Team> GetTeamAsync(Guid tenantGuid, Guid teamGuid, CancellationToken cancellationToken = default)
        {
            return await (from st in _securityContext.SecurityTenants
                          join sto in _securityContext.SecurityOrganizations on st.id equals sto.securityTenantId
                          join std in _securityContext.SecurityDepartments on sto.id equals std.securityOrganizationId
                          join stt in _securityContext.SecurityTeams on std.id equals stt.securityDepartmentId
                          where st.objectGuid == tenantGuid &&
                                st.active && !st.deleted &&
                                sto.active && !sto.deleted &&
                                std.active && !std.deleted &&
                                stt.active && !stt.deleted &&
                                stt.objectGuid == teamGuid
                          select new Team
                          {
                              name = stt.name,
                              description = stt.description,
                              objectGuid = stt.objectGuid
                              // Users intentionally omitted — use GetTeamUsersAsync for filtered members
                          }).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}