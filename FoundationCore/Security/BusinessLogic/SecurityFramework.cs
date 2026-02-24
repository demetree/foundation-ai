using Foundation.Cache;
using Foundation.Security.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Foundation.Security
{
    public class SecurityFramework
    {
        public const float CACHE_TIME_MINUTES = 0.1f;

        public class RoleAndPrivilege
        {
            public int roleId { get; set; }
            public string roleName { get; set; }
            public string privilegeName { get; set; }
            public SecurityLogic.Privileges privilege { get; set; }
        }


        public class ModuleAndRoleAndPrivilege : RoleAndPrivilege
        {
            public int moduleId { get; set; }
            public string moduleName { get; set; }
        }


        public class ModuleAccess
        {
            public int moduleId { get; set; }
            public string moduleName { get; set; }
            public bool userCanAccess { get; set; }
            public bool userCanRead { get; set; }
            public bool userMustReadAnonymous { get; set; }
            public bool userCanWrite { get; set; }
            public bool userCanAdminister { get; set; }
        }


        public class SecurityProfile
        {
            public int id { get; set; }
            public string accountName { get; set; }
            public List<ModuleAccess> moduleAccess { get; set; }
            public List<ModuleAndRoleAndPrivilege> roles { get; set; }
        }


        // =============================================================================================================
        //
        //  TENANT GUARD INFRASTRUCTURE
        //
        //  The SecurityTenantUser table carries three broad-stroke entitlement flags that govern a user's
        //  relationship with their tenant.  These are intentionally much less granular than the per-module
        //  role / privilege system:
        //
        //    isOwner  – The user is the tenant owner and has full administrative rights to every module
        //               in the tenant, regardless of what roles are assigned to them.
        //
        //    canRead  – The user is allowed to read data from this tenant.  When false, the user is
        //               effectively locked out of all read operations across all modules.
        //
        //    canWrite – The user is allowed to write data in this tenant.  When false, the user can
        //               still read (if canRead is true) but cannot modify any data in any module.
        //
        //  These guards are evaluated BEFORE the per-module role/privilege checks.  If a guard denies
        //  the operation, the more granular checks are never reached.
        //
        //  The guard system is ONLY active when multi-tenancy mode is enabled.  In single-tenant
        //  deployments, these checks are bypassed entirely to preserve backward compatibility.
        //
        //  The SecurityTenantUser record is cached using the same MemoryCacheManager pattern as
        //  other security data, with a key prefix of "SF_TUG_" (Tenant User Guard).  This cache
        //  is automatically cleared by ClearSecurityCaches() because it matches the "^SF_" pattern.
        //
        // =============================================================================================================


        /// <summary>
        /// 
        /// Retrieves the SecurityTenantUser record for the given user and their assigned tenant.
        /// This record contains the tenant-level guard fields (isOwner, canRead, canWrite).
        ///
        /// Returns null if:
        ///   - Multi-tenancy mode is not enabled
        ///   - The user is null or has no tenant assigned
        ///   - No SecurityTenantUser record exists for this user/tenant combination
        ///   - The SecurityTenantUser record is inactive or deleted
        ///
        /// Results are cached using the standard SF_ cache key pattern.
        /// 
        /// </summary>
        private static async Task<SecurityTenantUser> GetTenantUserGuardAsync(SecurityUser user, CancellationToken cancellationToken = default)
        {
            //
            // Guard checks only apply in multi-tenant deployments.
            // In single-tenant mode, return null to indicate "no guard applies."
            //
            if (Foundation.Configuration.GetMultiTenancyMode() == false)
            {
                return null;
            }

            //
            // If we don't have a user or the user isn't assigned to a tenant, there's nothing to guard against.
            //
            if (user == null || user.securityTenantId.HasValue == false)
            {
                return null;
            }

            MemoryCacheManager mcm = new MemoryCacheManager();

            string cacheKey = "SF_TUG_" + user.id + "_" + user.securityTenantId.Value;

            if (mcm.IsSet(cacheKey) == true)
            {
                return mcm.Get<SecurityTenantUser>(cacheKey);
            }

            using (SecurityContext db = new SecurityContext())
            {
                //
                // Look up the SecurityTenantUser join record that ties this user to their tenant.
                // This is the record that carries the isOwner, canRead, and canWrite flags.
                //
                SecurityTenantUser tenantUser = await (from stu in db.SecurityTenantUsers
                                                       where stu.securityUserId == user.id
                                                       && stu.securityTenantId == user.securityTenantId.Value
                                                       && stu.active == true
                                                       && stu.deleted == false
                                                       select stu)
                                                       .AsNoTracking()
                                                       .FirstOrDefaultAsync(cancellationToken)
                                                       .ConfigureAwait(false);

                mcm.Set(cacheKey, tenantUser, CACHE_TIME_MINUTES);

                return tenantUser;
            }
        }


        /// <summary>
        /// 
        /// Synchronous version of GetTenantUserGuardAsync.
        /// See the async version for full documentation.
        /// 
        /// </summary>
        private static SecurityTenantUser GetTenantUserGuard(SecurityUser user)
        {
            //
            // Guard checks only apply in multi-tenant deployments.
            //
            if (Foundation.Configuration.GetMultiTenancyMode() == false)
            {
                return null;
            }

            if (user == null || user.securityTenantId.HasValue == false)
            {
                return null;
            }

            MemoryCacheManager mcm = new MemoryCacheManager();

            string cacheKey = "SF_TUG_" + user.id + "_" + user.securityTenantId.Value;

            if (mcm.IsSet(cacheKey) == true)
            {
                return mcm.Get<SecurityTenantUser>(cacheKey);
            }

            using (SecurityContext db = new SecurityContext())
            {
                SecurityTenantUser tenantUser = (from stu in db.SecurityTenantUsers
                                                 where stu.securityUserId == user.id
                                                 && stu.securityTenantId == user.securityTenantId.Value
                                                 && stu.active == true
                                                 && stu.deleted == false
                                                 select stu)
                                                 .AsNoTracking()
                                                 .FirstOrDefault();

                mcm.Set(cacheKey, tenantUser, CACHE_TIME_MINUTES);

                return tenantUser;
            }
        }



        public async static Task<SecurityProfile> GetUserSecurityProfileAsync(SecurityUser user, CancellationToken cancellationToken = default)
        {
            SecurityProfile output = new SecurityProfile();

            if (user == null)
            {
                return output;
            }

            output.accountName = user.accountName;
            output.id = user.id;

            //
            // TENANT GUARD CHECK
            //
            // Before building the per-module access profile, check if the user's tenant-level
            // entitlements restrict their capabilities.  The tenantUser record (if present) will
            // be used after the per-module role resolution to override access flags.
            //
            SecurityTenantUser tenantUser = await GetTenantUserGuardAsync(user, cancellationToken);

            output.roles = await GetUserModuleRolesAsync(user, cancellationToken);
            output.moduleAccess = new List<ModuleAccess>();

            //
            // Note that the modulesAndRoles are expected to be ordered by name
            //
            string moduleName = null;
            ModuleAccess access = null;
            for (int i = 0; i < output.roles.Count; i++)
            {
                ModuleAndRoleAndPrivilege mr = output.roles.ElementAt(i);

                // is this a new module?
                if (mr.moduleName != moduleName || moduleName == null)
                {
                    moduleName = mr.moduleName;

                    access = new ModuleAccess();

                    access.moduleId = mr.moduleId;
                    access.moduleName = moduleName;
                    access.userCanAccess = true;            // until found to be false
                    access.userCanRead = false;             // until found to be true
                    access.userMustReadAnonymous = true;   // until found to be false
                    access.userCanWrite = false;            // until found to be true            
                    access.userCanWrite = false;            // until found to be true            
                    access.userCanAdminister = false;       // until found to be true            

                    output.moduleAccess.Add(access);
                }


                //
                // Stop everything if there is a No Access role found.
                //
                if (access.userCanAccess == true)
                {
                    if (mr.privilege == SecurityLogic.Privileges.NO_ACCESS)
                    {
                        // shut the door.
                        access.userCanAccess = false;
                        access.userCanAdminister = false;
                        access.userCanRead = false;
                        access.userMustReadAnonymous = false;
                        access.userCanWrite = false;
                    }
                    else if (mr.privilege == SecurityLogic.Privileges.ANONYMOUS_READ_ONLY)
                    {
                        // Do nothing there.  The anonymous read starts at true, and must be flipped to false by a higher privilege, like read, write, or admin.
                    }
                    else if (mr.privilege == SecurityLogic.Privileges.READ_ONLY)
                    {
                        access.userMustReadAnonymous = false;
                        access.userCanRead = true;
                    }
                    else if (mr.privilege == SecurityLogic.Privileges.READ_AND_WRITE)
                    {
                        access.userMustReadAnonymous = false;
                        access.userCanRead = true;
                        access.userCanWrite = true;
                    }
                    else if (mr.privilege == SecurityLogic.Privileges.ADMINISTRATIVE)
                    {
                        access.userMustReadAnonymous = false;
                        access.userCanRead = true;
                        access.userCanWrite = true;
                        access.userCanAdminister = true;
                    }
                }
            }


            //
            // TENANT GUARD OVERRIDES
            //
            // After the per-module role/privilege logic has run, apply the tenant-level guard overrides.
            // These are broad-stroke overrides that can revoke access or grant full admin rights
            // regardless of what the per-module roles say.
            //
            if (tenantUser != null)
            {
                for (int m = 0; m < output.moduleAccess.Count; m++)
                {
                    ModuleAccess ma = output.moduleAccess[m];

                    //
                    // If the user cannot read at the tenant level, shut the door on all modules.
                    //
                    if (tenantUser.canRead == false)
                    {
                        ma.userCanAccess = false;
                        ma.userCanRead = false;
                        ma.userCanWrite = false;
                        ma.userCanAdminister = false;
                        ma.userMustReadAnonymous = false;
                    }
                    //
                    // If the user cannot write at the tenant level, revoke write and admin on all modules.
                    // Read access is preserved.
                    //
                    else if (tenantUser.canWrite == false)
                    {
                        ma.userCanWrite = false;
                        ma.userCanAdminister = false;
                    }

                    //
                    // If the user is the tenant owner, grant full administrative access to every module.
                    // This overrides any role-based restrictions — tenant owners can do everything.
                    //
                    if (tenantUser.isOwner == true)
                    {
                        ma.userCanAccess = true;
                        ma.userCanRead = true;
                        ma.userCanWrite = true;
                        ma.userCanAdminister = true;
                        ma.userMustReadAnonymous = false;
                    }
                }
            }


            return output;
        }


        public static SecurityProfile GetUserSecurityProfile(SecurityUser user)
        {
            SecurityProfile output = new SecurityProfile();

            if (user == null)
            {
                return output;
            }

            output.accountName = user.accountName;
            output.id = user.id;

            //
            // TENANT GUARD CHECK — see async version for full documentation.
            //
            SecurityTenantUser tenantUser = GetTenantUserGuard(user);

            output.roles = GetUserModuleRoles(user);
            output.moduleAccess = new List<ModuleAccess>();

            //
            // Note that the modulesAndRoles are expected to be ordered by name
            //
            string moduleName = null;
            ModuleAccess access = null;
            for (int i = 0; i < output.roles.Count; i++)
            {
                ModuleAndRoleAndPrivilege mr = output.roles.ElementAt(i);

                // is this a new module?
                if (mr.moduleName != moduleName || moduleName == null)
                {
                    moduleName = mr.moduleName;

                    access = new ModuleAccess();

                    access.moduleId = mr.moduleId;
                    access.moduleName = moduleName;
                    access.userCanAccess = true;            // until found to be false
                    access.userCanRead = false;             // until found to be true
                    access.userMustReadAnonymous = true;   // until found to be false
                    access.userCanWrite = false;            // until found to be true            
                    access.userCanWrite = false;            // until found to be true            
                    access.userCanAdminister = false;       // until found to be true            

                    output.moduleAccess.Add(access);
                }


                //
                // Stop everything if there is a No Access role found.
                //
                if (access.userCanAccess == true)
                {
                    if (mr.privilege == SecurityLogic.Privileges.NO_ACCESS)
                    {
                        // shut the door.
                        access.userCanAccess = false;
                        access.userCanAdminister = false;
                        access.userCanRead = false;
                        access.userMustReadAnonymous = false;
                        access.userCanWrite = false;
                    }
                    else if (mr.privilege == SecurityLogic.Privileges.ANONYMOUS_READ_ONLY)
                    {
                        // Do nothing there.  The anonymous read starts at true, and must be flipped to false by a higher privilege, like read, write, or admin.
                    }
                    else if (mr.privilege == SecurityLogic.Privileges.READ_ONLY)
                    {
                        access.userMustReadAnonymous = false;
                        access.userCanRead = true;
                    }
                    else if (mr.privilege == SecurityLogic.Privileges.READ_AND_WRITE)
                    {
                        access.userMustReadAnonymous = false;
                        access.userCanRead = true;
                        access.userCanWrite = true;
                    }
                    else if (mr.privilege == SecurityLogic.Privileges.ADMINISTRATIVE)
                    {
                        access.userMustReadAnonymous = false;
                        access.userCanRead = true;
                        access.userCanWrite = true;
                        access.userCanAdminister = true;
                    }
                }
            }


            //
            // TENANT GUARD OVERRIDES — see async version for full documentation.
            //
            if (tenantUser != null)
            {
                for (int m = 0; m < output.moduleAccess.Count; m++)
                {
                    ModuleAccess ma = output.moduleAccess[m];

                    if (tenantUser.canRead == false)
                    {
                        ma.userCanAccess = false;
                        ma.userCanRead = false;
                        ma.userCanWrite = false;
                        ma.userCanAdminister = false;
                        ma.userMustReadAnonymous = false;
                    }
                    else if (tenantUser.canWrite == false)
                    {
                        ma.userCanWrite = false;
                        ma.userCanAdminister = false;
                    }

                    if (tenantUser.isOwner == true)
                    {
                        ma.userCanAccess = true;
                        ma.userCanRead = true;
                        ma.userCanWrite = true;
                        ma.userCanAdminister = true;
                        ma.userMustReadAnonymous = false;
                    }
                }
            }


            return output;
        }


        public static void GrantRoleToUser(SecurityUser user, string roleName)
        {
            using (SecurityContext db = new SecurityContext())
            {

                SecurityRole role = db.SecurityRoles.Where(i => i.name == roleName)
                  .Where(x => x.deleted != true)
                  .FirstOrDefault();


                if (role != null)
                {
                    SecurityUserSecurityRole existing = (from x in db.SecurityUserSecurityRoles
                                                         where x.securityUserId == user.id && x.securityRoleId == role.id
                                                         select x).FirstOrDefault();

                    if (existing == null && user.id > 0)
                    {
                        SecurityUserSecurityRole userRole = new SecurityUserSecurityRole();

                        userRole.active = true;
                        userRole.deleted = false;
                        userRole.securityUserId = user.id;
                        userRole.securityRoleId = role.id;

                        db.SecurityUserSecurityRoles.Add(userRole);
                        db.SaveChanges();
                    }
                }
                else
                {
                    throw new Exception("Could not find role named " + roleName);
                }
            }
        }

        public async static Task<List<ModuleAndRoleAndPrivilege>> GetUserModuleRolesAsync(SecurityUser user, CancellationToken cancellationToken = default)
        {
            MemoryCacheManager mcm = new MemoryCacheManager();

            string cacheKey = "SF_UMR_" + user.accountName;

            List<ModuleAndRoleAndPrivilege> modulesAndRoles;

            if (mcm.IsSet(cacheKey) == true)
            {
                modulesAndRoles = mcm.Get<List<ModuleAndRoleAndPrivilege>>(cacheKey);
            }
            else
            {
                using (SecurityContext db = new SecurityContext())
                {
                    List<SecurityRole> userRoles = await GetUserSecurityRolesAsync(user, cancellationToken);

                    List<int> roleIds = (from ur in userRoles
                                         select ur.id)
                                        .ToList();

                    modulesAndRoles = await (from mr in db.ModuleSecurityRoles
                                             join module in db.Modules on mr.moduleId equals module.id
                                             join role in db.SecurityRoles on mr.securityRoleId equals role.id
                                             join privilege in db.Privileges on role.privilegeId equals privilege.id
                                             where mr.deleted != true &&
                                             mr.active == true &&
                                             roleIds.Any(x => x == role.id)
                                             orderby module.name           // DO NOT CHANGE THIS -> SEQUENCE IS USED ABOVE, AND CODE WILL BREAK IF NOT ORDERED BY THIS.
                                             select new ModuleAndRoleAndPrivilege
                                             {
                                                 roleId = role.id,
                                                 moduleId = module.id,
                                                 privilege = (SecurityLogic.Privileges)role.privilegeId,
                                                 moduleName = module.name,
                                                 roleName = role.name,
                                                 privilegeName = privilege.name
                                             }
                             )
                             .ToListAsync(cancellationToken)
                             .ConfigureAwait(false);


                    mcm.Set(cacheKey, modulesAndRoles, CACHE_TIME_MINUTES);
                }
            }

            return modulesAndRoles;
        }


        public static List<ModuleAndRoleAndPrivilege> GetUserModuleRoles(SecurityUser user)
        {
            MemoryCacheManager mcm = new MemoryCacheManager();

            string cacheKey = "SF_UMR_" + user.accountName;

            List<ModuleAndRoleAndPrivilege> modulesAndRoles;

            if (mcm.IsSet(cacheKey) == true)
            {
                modulesAndRoles = mcm.Get<List<ModuleAndRoleAndPrivilege>>(cacheKey);
            }
            else
            {
                using (SecurityContext db = new SecurityContext())
                {
                    List<SecurityRole> userRoles = GetUserSecurityRoles(user);

                    List<int> roleIds = (from ur in userRoles
                                         select ur.id).ToList();

                    modulesAndRoles = (from mr in db.ModuleSecurityRoles
                                       join module in db.Modules on mr.moduleId equals module.id
                                       join role in db.SecurityRoles on mr.securityRoleId equals role.id
                                       join privilege in db.Privileges on role.privilegeId equals privilege.id
                                       where mr.deleted != true &&
                                       mr.active == true &&
                                       roleIds.Any(x => x == role.id)
                                       orderby module.name           // DO NOT CHANGE THIS -> SEQUENCE IS USED ABOVE, AND CODE WILL BREAK IF NOT ORDERED BY THIS.
                                       select new ModuleAndRoleAndPrivilege
                                       {
                                           roleId = role.id,
                                           moduleId = module.id,
                                           privilege = (SecurityLogic.Privileges)role.privilegeId,
                                           moduleName = module.name,
                                           roleName = role.name,
                                           privilegeName = privilege.name
                                       }
                             ).ToList();


                    mcm.Set(cacheKey, modulesAndRoles, CACHE_TIME_MINUTES);
                }
            }

            return modulesAndRoles;
        }


        public static Boolean UserCanAccessModule(SecurityUser user, string moduleName, List<ModuleAndRoleAndPrivilege> modulesAndRoles = null)
        {
            // If no list provided, then get it.
            if (modulesAndRoles == null)
            {
                modulesAndRoles = GetUserModuleRoles(user);
            }


            bool userCanAccessModule = false;

            for (int i = 0; i < modulesAndRoles.Count; i++)
            {
                ModuleAndRoleAndPrivilege mr = modulesAndRoles.ElementAt(i);

                if (mr.moduleName == moduleName)
                {
                    //
                    // Stop everything if there is a No Access role found.
                    //
                    if (mr.privilege == SecurityLogic.Privileges.NO_ACCESS)
                    {
                        return false;
                    }
                    else if (mr.privilege == SecurityLogic.Privileges.ANONYMOUS_READ_ONLY ||
                        mr.privilege == SecurityLogic.Privileges.READ_ONLY ||
                        mr.privilege == SecurityLogic.Privileges.READ_AND_WRITE ||
                        mr.privilege == SecurityLogic.Privileges.ADMINISTRATIVE)
                    {
                        // we don't want to exit yet because there may be a NO_ACCESS permission later in the set..
                        userCanAccessModule = true;
                    }
                }
            }

            return userCanAccessModule;
        }


        public static Boolean UserCanAdministerModule(SecurityUser user, string moduleName, List<ModuleAndRoleAndPrivilege> modulesAndRoles = null)
        {
            // If no list provided, then get it.
            if (modulesAndRoles == null)
            {
                modulesAndRoles = GetUserModuleRoles(user);
            }


            bool userCanAdministerModule = false;


            for (int i = 0; i < modulesAndRoles.Count; i++)
            {
                ModuleAndRoleAndPrivilege mr = modulesAndRoles.ElementAt(i);

                if (mr.moduleName == moduleName)
                {
                    //
                    // Stop everything if there is a No Access role found.
                    //
                    if (mr.privilege == SecurityLogic.Privileges.NO_ACCESS)
                    {
                        return false;
                    }
                    else if (mr.privilege == SecurityLogic.Privileges.ADMINISTRATIVE)
                    {
                        // we don't want to exit yet because there may be a NO_ACCESS permission later in the set..
                        userCanAdministerModule = true;
                    }
                }
            }

            return userCanAdministerModule;
        }


        public async static Task<List<SecurityUserSecurityGroup>> GetUserGroupsAsync(int userId, CancellationToken cancellationToken = default)
        {
            MemoryCacheManager mcm = new MemoryCacheManager();

            List<SecurityUserSecurityGroup> output = null;

            string cacheKey = "SF_GUG_BY_ID";

            if (mcm.IsSet(cacheKey) == true)
            {
                output = mcm.Get<List<SecurityUserSecurityGroup>>(cacheKey);
            }
            else
            {
                using (SecurityContext db = new SecurityContext())
                {
                    output = await (from ug in db.SecurityUserSecurityGroups
                                    where ug.securityUserId == userId &&
                                    ug.active == true &&
                                    ug.deleted == false
                                    select ug)
                                    .ToListAsync(cancellationToken)
                                    .ConfigureAwait(false);

                    mcm.Set(cacheKey, output, CACHE_TIME_MINUTES);
                }
            }

            return output;
        }

        public static List<SecurityUserSecurityGroup> GetUserGroups(int userId)
        {
            MemoryCacheManager mcm = new MemoryCacheManager();

            List<SecurityUserSecurityGroup> output = null;

            string cacheKey = "SF_GUG_BY_ID";

            if (mcm.IsSet(cacheKey) == true)
            {
                output = mcm.Get<List<SecurityUserSecurityGroup>>(cacheKey);
            }
            else
            {
                using (SecurityContext db = new SecurityContext())
                {
                    output = (from ug in db.SecurityUserSecurityGroups
                              where ug.securityUserId == userId &&
                              ug.active == true &&
                              ug.deleted == false
                              select ug).ToList();

                    mcm.Set(cacheKey, output, CACHE_TIME_MINUTES);
                }
            }

            return output;
        }

        public static List<SecurityUserSecurityGroup> GetUserGroups(string userName)
        {
            MemoryCacheManager mcm = new MemoryCacheManager();

            List<SecurityUserSecurityGroup> output = null;

            string cacheKey = "SF_GUG_BY_NAME";

            if (mcm.IsSet(cacheKey) == true)
            {
                output = mcm.Get<List<SecurityUserSecurityGroup>>(cacheKey);

            }
            else
            {
                using (SecurityContext db = new SecurityContext())
                {
                    output = (from ug in db.SecurityUserSecurityGroups
                              join u in db.SecurityUsers on ug.securityUserId equals u.id
                              where u.accountName == userName &&
                              ug.active == true &&
                              ug.deleted == false &&
                              u.active == true &&
                              u.deleted == false
                              select ug).ToList();
                }

                mcm.Set(cacheKey, output, CACHE_TIME_MINUTES);

            }

            return output;
        }


        public static Boolean UserCanWriteToModule(SecurityUser user, string moduleName, List<ModuleAndRoleAndPrivilege> modulesAndRoles = null)
        {
            // If no list provided, then get it.
            if (modulesAndRoles == null)
            {
                modulesAndRoles = GetUserModuleRoles(user);
            }


            bool userCanWriteToModule = false;


            for (int i = 0; i < modulesAndRoles.Count; i++)
            {
                ModuleAndRoleAndPrivilege mr = modulesAndRoles.ElementAt(i);

                if (mr.moduleName == moduleName)
                {
                    //
                    // Stop everything if there is a No Access role found.
                    //
                    if (mr.privilege == SecurityLogic.Privileges.NO_ACCESS)
                    {
                        return false;
                    }
                    else if (mr.privilege == SecurityLogic.Privileges.READ_AND_WRITE ||
                             mr.privilege == SecurityLogic.Privileges.ADMINISTRATIVE)
                    {
                        // we don't want to exit yet because there may be a NO_ACCESS permission later in the set..
                        userCanWriteToModule = true;
                    }
                }
            }

            return userCanWriteToModule;
        }

        internal static string UserTenantGuidString(SecurityUser user)
        {
            //
            // Nothing is allowed if we don't know who this is.
            //
            if (user == null)
            {
                return null;
            }

            //
            // If we already have the tenant, then use it.  Otherwise, get it.
            //
            if (user.securityTenant != null)
            {
                if (user.securityTenant.active == false || user.securityTenant.deleted == true)
                {
                    return null;
                }
                else
                {
                    return user.securityTenant.objectGuid.ToString();
                }

            }
            else if (user.securityTenantId.HasValue == true && user.securityTenant == null)
            {
                using (SecurityContext db = new SecurityContext())
                {
                    user.securityTenant = (from x in db.SecurityTenants where x.id == user.securityTenantId select x).FirstOrDefault();

                    if (user.securityTenant != null)
                    {
                        if (user.securityTenant.active == false || user.securityTenant.deleted == true)
                        {
                            return null;
                        }
                        else
                        {
                            return user.securityTenant.objectGuid.ToString();
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }
        }


        public static async Task<Guid> UserTenantGuidAsync(SecurityUser user, CancellationToken cancellationToken = default)
        {
            //
            // Nothing is allowed if we don't know who this is.
            //
            if (user == null)
            {
                throw new Exception("no user provided to find tenant guid for.");
            }

            //
            // If we already have the tenant, then use it.  Otherwise, get it.
            //
            if (user.securityTenant != null)
            {
                if (user.securityTenant.active == false || user.securityTenant.deleted == true)
                {
                    throw new Exception("User has tenant but the tenant is inactive or deleted.");
                }
                else
                {
                    return user.securityTenant.objectGuid;
                }
            }
            else if (user.securityTenantId.HasValue == true && user.securityTenant == null)
            {
                using (SecurityContext db = new SecurityContext())
                {
                    user.securityTenant = await (from x in db.SecurityTenants
                                                 where x.id == user.securityTenantId
                                                 select x)
                                                 .FirstOrDefaultAsync(cancellationToken)
                                                 .ConfigureAwait(false);

                    if (user.securityTenant != null)
                    {
                        if (user.securityTenant.active == false || user.securityTenant.deleted == true)
                        {
                            throw new Exception("User has tenant but the tenant is inactive or deleted.");
                        }
                        else
                        {
                            return user.securityTenant.objectGuid;
                        }
                    }
                    else
                    {
                        throw new Exception("User has tenant but the tenant could not be loaded.");
                    }
                }
            }
            else
            {
                throw new Exception("User has no tenant.");
            }
        }


        public static Guid UserTenantGuid(SecurityUser user)
        {
            //
            // Nothing is allowed if we don't know who this is.
            //
            if (user == null)
            {
                throw new Exception("no user provided to find tenant guid for.");
            }

            //
            // If we already have the tenant, then use it.  Otherwise, get it.
            //
            if (user.securityTenant != null)
            {
                if (user.securityTenant.active == false || user.securityTenant.deleted == true)
                {
                    throw new Exception("User has tenant but the tenant is inactive or deleted.");
                }
                else
                {
                    return user.securityTenant.objectGuid;
                }

            }
            else if (user.securityTenantId.HasValue == true && user.securityTenant == null)
            {
                using (SecurityContext db = new SecurityContext())
                {
                    user.securityTenant = (from x in db.SecurityTenants 
                                           where x.id == user.securityTenantId 
                                           select x).FirstOrDefault();

                    if (user.securityTenant != null)
                    {
                        if (user.securityTenant.active == false || user.securityTenant.deleted == true)
                        {
                            throw new Exception("User has tenant but the tenant is inactive or deleted.");
                        }
                        else
                        {
                            return user.securityTenant.objectGuid;
                        }
                    }
                    else
                    {
                        throw new Exception("User has tenant but the tenant could not be loaded.");
                    }
                }
            }
            else
            {
                throw new Exception("User has no tenant.");
            }
        }

        public static Boolean UserCanReadFromModule(SecurityUser user, string moduleName, List<ModuleAndRoleAndPrivilege> modulesAndRoles = null)
        {
            // If no list provided, then get it.
            if (modulesAndRoles == null)
            {
                modulesAndRoles = GetUserModuleRoles(user);
            }


            bool userCanReadFromModule = false;


            for (int i = 0; i < modulesAndRoles.Count; i++)
            {
                ModuleAndRoleAndPrivilege mr = modulesAndRoles.ElementAt(i);

                if (mr.moduleName == moduleName)
                {
                    //
                    // Stop everything if there is a No Access role found.
                    //
                    if (mr.privilege == SecurityLogic.Privileges.NO_ACCESS)
                    {
                        return false;
                    }
                    else if (
                            mr.privilege == SecurityLogic.Privileges.READ_ONLY ||
                            mr.privilege == SecurityLogic.Privileges.READ_AND_WRITE ||
                            mr.privilege == SecurityLogic.Privileges.ADMINISTRATIVE)
                    {
                        // we don't want to exit yet because there may be a NO_ACCESS permission later in the set..
                        userCanReadFromModule = true;
                    }
                }
            }

            return userCanReadFromModule;
        }

        public static Boolean UserCanReadAnonymouslyFromModule(SecurityUser user, string moduleName, List<ModuleAndRoleAndPrivilege> modulesAndRoles = null)
        {
            // If no list provided, then get it.
            if (modulesAndRoles == null)
            {
                modulesAndRoles = GetUserModuleRoles(user);
            }


            bool userCanReadAnonymousFromModule = false;


            for (int i = 0; i < modulesAndRoles.Count; i++)
            {
                ModuleAndRoleAndPrivilege mr = modulesAndRoles.ElementAt(i);

                if (mr.moduleName == moduleName)
                {
                    //
                    // Stop everything if there is a No Access role found.
                    //
                    if (mr.privilege == SecurityLogic.Privileges.NO_ACCESS)
                    {
                        return false;
                    }
                    else if (
                            mr.privilege == SecurityLogic.Privileges.ANONYMOUS_READ_ONLY)
                    {
                        // we don't want to exit yet because there may be a NO_ACCESS permission later in the set..
                        userCanReadAnonymousFromModule = true;
                    }
                }
            }

            return userCanReadAnonymousFromModule;
        }


        public static async Task<List<RoleAndPrivilege>> GetRolesAndPrivilegesForModuleAsync(string moduleName, SecurityContext db, CancellationToken cancellationToken = default)
        {
            MemoryCacheManager mcm = new MemoryCacheManager();

            string cacheKey = "SF_RP_" + moduleName;

            List<RoleAndPrivilege> rolesAndPrivileges;

            if (mcm.IsSet(cacheKey) == true)
            {
                rolesAndPrivileges = mcm.Get<List<RoleAndPrivilege>>(cacheKey);
            }
            else
            {
                //
                // Get all of the roles and privileges for this module.
                //
                rolesAndPrivileges = await (from mr in db.ModuleSecurityRoles
                                            join module in db.Modules on mr.moduleId equals module.id
                                            join role in db.SecurityRoles on mr.securityRoleId equals role.id
                                            join privilege in db.Privileges on role.privilegeId equals privilege.id

                                            where mr.deleted != true &&
                                            mr.active == true &&
                                            module.name == moduleName

                                            //
                                            // The purpose of this sort is to pre-build the role list in a way that will reduce the number of loop iterations needed when testing for the most common permissions in the controller loads.
                                            //
                                            orderby ((SecurityLogic.Privileges)role.privilegeId == SecurityLogic.Privileges.ADMINISTRATIVE ? 1 :
                                                      ((SecurityLogic.Privileges)role.privilegeId == SecurityLogic.Privileges.READ_AND_WRITE ? 2 :
                                                      ((SecurityLogic.Privileges)role.privilegeId == SecurityLogic.Privileges.READ_ONLY ? 3 :
                                                      ((SecurityLogic.Privileges)role.privilegeId == SecurityLogic.Privileges.CUSTOM ? 4 :
                                                      ((SecurityLogic.Privileges)role.privilegeId == SecurityLogic.Privileges.ANONYMOUS_READ_ONLY ? 5 :
                                                      ((SecurityLogic.Privileges)role.privilegeId == SecurityLogic.Privileges.NO_ACCESS ? 6 : 7)
                                                      )))))

                                            select new RoleAndPrivilege
                                            {
                                                roleId = role.id,
                                                privilege = (SecurityLogic.Privileges)role.privilegeId,
                                                roleName = role.name,
                                                privilegeName = privilege.name
                                            }
                                     )
                                     .ToListAsync(cancellationToken)
                                     .ConfigureAwait(false);

                mcm.Set(cacheKey, rolesAndPrivileges, 1440);      // this data is static for all intents and purposes.  There is no use case for this to change during run time.
            }

            return rolesAndPrivileges;
        }



        public static List<RoleAndPrivilege> GetRolesAndPrivilegesForModule(string moduleName, SecurityContext db)
        {
            MemoryCacheManager mcm = new MemoryCacheManager();

            string cacheKey = "SF_RP_" + moduleName;

            List<RoleAndPrivilege> rolesAndPrivileges;

            if (mcm.IsSet(cacheKey) == true)
            {
                rolesAndPrivileges = mcm.Get<List<RoleAndPrivilege>>(cacheKey);
            }
            else
            {


                //
                // Get all of the roles and privileges for this module.
                //
                rolesAndPrivileges = (from mr in db.ModuleSecurityRoles
                                      join module in db.Modules on mr.moduleId equals module.id
                                      join role in db.SecurityRoles on mr.securityRoleId equals role.id
                                      join privilege in db.Privileges on role.privilegeId equals privilege.id

                                      where mr.deleted != true &&
                                      mr.active == true &&
                                      module.name == moduleName

                                      //
                                      // The purpose of this sort is to pre-build the role list in a way that will reduce the number of loop iterations needed when testing for the most common permissions in the controller loads.
                                      //
                                      orderby ((SecurityLogic.Privileges)role.privilegeId == SecurityLogic.Privileges.ADMINISTRATIVE ? 1 :
                                                ((SecurityLogic.Privileges)role.privilegeId == SecurityLogic.Privileges.READ_AND_WRITE ? 2 :
                                                ((SecurityLogic.Privileges)role.privilegeId == SecurityLogic.Privileges.READ_ONLY ? 3 :
                                                ((SecurityLogic.Privileges)role.privilegeId == SecurityLogic.Privileges.CUSTOM ? 4 :
                                                ((SecurityLogic.Privileges)role.privilegeId == SecurityLogic.Privileges.ANONYMOUS_READ_ONLY ? 5 :
                                                ((SecurityLogic.Privileges)role.privilegeId == SecurityLogic.Privileges.NO_ACCESS ? 6 : 7)
                                                )))))

                                      select new RoleAndPrivilege
                                      {
                                          roleId = role.id,
                                          privilege = (SecurityLogic.Privileges)role.privilegeId,
                                          roleName = role.name,
                                          privilegeName = privilege.name
                                      }
                                     ).ToList();

                mcm.Set(cacheKey, rolesAndPrivileges, 1440);      // this data is static for all intents and purposes.
            }

            return rolesAndPrivileges;
        }



        public async static Task<bool> UserHasNoAccessAsync(string moduleName, List<RoleAndPrivilege> rolesForModule, SecurityUser user, CancellationToken cancellationToken = default)
        {
            //
            // Will return true if any role the user has is indicated to provide 'No Access'
            //
            //

            // Nothing is allowed if we don't know who this is.
            if (user == null)
            {
                return true;
            }


            //
            // TENANT GUARD CHECK
            //
            // If the user's SecurityTenantUser record indicates they cannot read from this tenant,
            // then they effectively have no access to anything.  This is checked before the per-module
            // role resolution because it is a broader, tenant-level restriction.
            //
            // If the user is the tenant owner (isOwner == true), they always have access, so we
            // return false immediately — no need to check per-module roles.
            //
            SecurityTenantUser tenantUser = await GetTenantUserGuardAsync(user, cancellationToken);

            if (tenantUser != null)
            {
                // Tenant owners always have access — bypass all further checks.
                if (tenantUser.isOwner == true)
                {
                    return false;
                }

                // If the user cannot read at the tenant level, they have no access.
                if (tenantUser.canRead == false)
                {
                    return true;
                }
            }


            //
            // Get the roles the user has access to in the provided module, if one is provided
            //
            List<SecurityRole> userRoles = null;

            if (string.IsNullOrEmpty(moduleName) == false)
            {
                userRoles = await GetUserSecurityRolesInModuleAsync(user, moduleName, cancellationToken);
            }
            else
            {
                userRoles = await GetUserSecurityRolesAsync(user, cancellationToken);
            }

            if (userRoles != null)
            {
                for (int i = 0; i < userRoles.Count; i++)
                {
                    SecurityRole userRole = userRoles.ElementAt(i);

                    for (int j = 0; j < rolesForModule.Count; j++)
                    {
                        RoleAndPrivilege moduleRole = rolesForModule.ElementAt(j);

                        // See if there's a privilege that denies access to this role.  If so, then return immediately.
                        if (moduleRole.roleName == userRole.name && moduleRole.privilege == SecurityLogic.Privileges.NO_ACCESS)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }



        public static bool UserHasNoAccess(string moduleName, List<RoleAndPrivilege> rolesForModule, SecurityUser user)
        {
            //
            // Will return true if any role the user has is indicated to provide 'No Access'
            //
            //

            // Nothing is allowed if we don't know who this is.
            if (user == null)
            {
                return true;
            }


            //
            // TENANT GUARD CHECK — see async version for full documentation.
            //
            SecurityTenantUser tenantUser = GetTenantUserGuard(user);

            if (tenantUser != null)
            {
                if (tenantUser.isOwner == true)
                {
                    return false;
                }

                if (tenantUser.canRead == false)
                {
                    return true;
                }
            }


            //
            // Get the roles the user has access to in the provided module, if one is provided
            //
            List<SecurityRole> userRoles = null;

            if (string.IsNullOrEmpty(moduleName) == false)
            {
                userRoles = GetUserSecurityRolesInModule(user, moduleName);
            }
            else
            {
                userRoles = GetUserSecurityRoles(user);
            }

            for (int i = 0; i < userRoles.Count; i++)
            {
                SecurityRole userRole = userRoles.ElementAt(i);

                for (int j = 0; j < rolesForModule.Count; j++)
                {
                    RoleAndPrivilege moduleRole = rolesForModule.ElementAt(j);

                    // See if theres a privilege that denies access to this role.  If so, then return immediately.
                    if (moduleRole.roleName == userRole.name && moduleRole.privilege == SecurityLogic.Privileges.NO_ACCESS)
                    {
                        return true;
                    }
                }
            }

            return false;
        }



        public static async Task<bool> UserHasCustomRoleAsync(string moduleName, List<RoleAndPrivilege> rolesForModule, string roleName, SecurityUser user, CancellationToken cancellationToken = default)
        {
            //
            // Get the roles the user has access to in the provided module, if one is provided
            //
            List<SecurityRole> userRoles = null;

            if (string.IsNullOrEmpty(moduleName) == false)
            {
                userRoles = await GetUserSecurityRolesInModuleAsync(user, moduleName, cancellationToken);
            }
            else
            {
                userRoles = await GetUserSecurityRolesAsync(user);
            }

            for (int i = 0; i < rolesForModule.Count; i++)
            {
                RoleAndPrivilege moduleRole = rolesForModule.ElementAt(i);

                //
                // Does this module have this role linked with a 'Custom' privilege?
                //
                if (moduleRole.roleName == roleName &&
                    moduleRole.privilege == SecurityLogic.Privileges.CUSTOM)
                {
                    //
                    // Does user have this role?
                    //
                    foreach (SecurityRole sr in userRoles)
                    {
                        if (sr.name == moduleRole.roleName)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool UserHasCustomRole(string moduleName, List<RoleAndPrivilege> rolesForModule, string roleName, SecurityUser user)
        {
            //
            // Get the roles the user has access to in the provided module, if one is provided
            //
            List<SecurityRole> userRoles = null;

            if (string.IsNullOrEmpty(moduleName) == false)
            {
                userRoles = GetUserSecurityRolesInModule(user, moduleName);
            }
            else
            {
                userRoles = GetUserSecurityRoles(user);
            }

            for (int i = 0; i < rolesForModule.Count; i++)
            {
                RoleAndPrivilege moduleRole = rolesForModule.ElementAt(i);

                //
                // Does this module have this role linked with a 'Custom' privilege?
                //
                if (moduleRole.roleName == roleName &&
                    moduleRole.privilege == SecurityLogic.Privileges.CUSTOM)
                {
                    //
                    // Does user have this role?
                    //
                    foreach (SecurityRole sr in userRoles)
                    {
                        if (sr.name == moduleRole.roleName)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }


        public async static Task<bool> UserCanReadAsync(string moduleName, List<RoleAndPrivilege> rolesForModule, SecurityUser user, int entityMinimumReadSecurityLevel = 0, CancellationToken cancellationToken = default)
        {
            // Nothing is allowed if we don't know who this is.
            if (user == null)
            {
                return false;
            }


            //
            // TENANT GUARD CHECK
            //
            // Before evaluating per-module roles, check the tenant-level entitlement flags.
            //
            // If the user's SecurityTenantUser record says canRead == false, they cannot read from
            // any module in this tenant, regardless of what roles they hold.  Return false immediately.
            //
            // If the user is the tenant owner (isOwner == true), they can always read everything.
            // Return true immediately — no need to evaluate roles.
            //
            SecurityTenantUser tenantUser = await GetTenantUserGuardAsync(user, cancellationToken);

            if (tenantUser != null)
            {
                // Tenant owners can always read.
                if (tenantUser.isOwner == true)
                {
                    return true;
                }

                // If the user cannot read at the tenant level, deny immediately.
                if (tenantUser.canRead == false)
                {
                    return false;
                }
            }


            bool adminFound = false;
            bool readerFound = false;

            //
            // Get the roles the user has access to in the provided module, if one is provided
            //
            List<SecurityRole> userRoles = null;

            if (string.IsNullOrEmpty(moduleName) == false)
            {
                userRoles = await GetUserSecurityRolesInModuleAsync(user, moduleName, cancellationToken);
            }
            else
            {
                userRoles = await GetUserSecurityRolesAsync(user, cancellationToken);
            }


            for (int i = 0; i < userRoles.Count; i++)
            {
                SecurityRole roleForUser = userRoles.ElementAt(i);

                //
                // Compare what the user has against the roles and privileges needed to access the resource that is invoking this check.
                //
                if (rolesForModule != null)
                {
                    for (int j = 0; j < rolesForModule.Count; j++)
                    {
                        RoleAndPrivilege moduleRole = rolesForModule.ElementAt(j);

                        if (moduleRole.roleName == roleForUser.name && moduleRole.privilege == SecurityLogic.Privileges.ADMINISTRATIVE)
                        {
                            adminFound = true;
                            break;  // don't need to look for anything further as we have the top privilege.
                        }
                        //
                        // Test for any non-Admin privilege that grants the ability to read, but be careful with this for entities that implement an anonymized presentation of data, a subsequent call to 'UserMustReadAnonymously' must be made to decide whether or not to anonymize the data prior to returnign it.
                        //
                        else if (moduleRole.roleName == roleForUser.name && (moduleRole.privilege == SecurityLogic.Privileges.READ_AND_WRITE || moduleRole.privilege == SecurityLogic.Privileges.READ_ONLY || moduleRole.privilege == SecurityLogic.Privileges.ANONYMOUS_READ_ONLY))
                        {
                            readerFound = true;     // don't stop looking here because admin could be later in the list.
                        }
                    }
                }
            }


            //
            // Based on the admin / reader state for this user, decide what to do.
            //
            if (adminFound == true)
            {
                // admins can always write, regardless of their or the entity writer security level
                return true;
            }
            else if (readerFound == true)
            {
                //
                // Is this user privileged enough to write to this entity, based on a security level comparison
                //
                if (user.readPermissionLevel >= entityMinimumReadSecurityLevel)
                {
                    return true;
                }
                else
                {
                    //
                    // User is reader, but does not have sufficient reader privilege level on their user record to read from this entity. 
                    //
                    return false;
                }
            }

            return false;
        }



        public static bool UserCanRead(string moduleName, List<RoleAndPrivilege> rolesForModule, SecurityUser user, int entityMinimumReadSecurityLevel = 0)
        {
            // Nothing is allowed if we don't know who this is.
            if (user == null)
            {
                return false;
            }


            //
            // TENANT GUARD CHECK — see async version for full documentation.
            //
            SecurityTenantUser tenantUser = GetTenantUserGuard(user);

            if (tenantUser != null)
            {
                if (tenantUser.isOwner == true)
                {
                    return true;
                }

                if (tenantUser.canRead == false)
                {
                    return false;
                }
            }


            bool adminFound = false;
            bool readerFound = false;

            //
            // Get the roles the user has access to in the provided module, if one is provided
            //
            List<SecurityRole> userRoles = null;

            if (string.IsNullOrEmpty(moduleName) == false)
            {
                userRoles = GetUserSecurityRolesInModule(user, moduleName);
            }
            else
            {
                userRoles = GetUserSecurityRoles(user);
            }


            for (int i = 0; i < userRoles.Count; i++)
            {
                SecurityRole roleForUser = userRoles.ElementAt(i);

                //
                // Compare what the user has against the roles and privileges needed to access the resource that is invoking this check.
                //
                if (rolesForModule != null)
                {
                    for (int j = 0; j < rolesForModule.Count; j++)
                    {
                        RoleAndPrivilege moduleRole = rolesForModule.ElementAt(j);

                        if (moduleRole.roleName == roleForUser.name && moduleRole.privilege == SecurityLogic.Privileges.ADMINISTRATIVE)
                        {
                            adminFound = true;
                            break;  // don't need to look for anything further as we have the top privilege.
                        }
                        //
                        // Test for any non-Admin privilege that grants the ability to read, but be careful with this for entities that implement an anonymized presentation of data, a subsequent call to 'UserMustReadAnonymously' must be made to decide whether or not to anonymize the data prior to returnign it.
                        //
                        else if (moduleRole.roleName == roleForUser.name && (moduleRole.privilege == SecurityLogic.Privileges.READ_AND_WRITE || moduleRole.privilege == SecurityLogic.Privileges.READ_ONLY || moduleRole.privilege == SecurityLogic.Privileges.ANONYMOUS_READ_ONLY))
                        {
                            readerFound = true;     // don't stop looking here because admin could be later in the list.
                        }
                    }
                }
            }


            //
            // Based on the admin / reader state for this user, decide what to do.
            //
            if (adminFound == true)
            {
                // admins can always write, regardless of their or the entity writer security level
                return true;
            }
            else if (readerFound == true)
            {
                //
                // Is this user privileged enough to write to this entity, based on a security level comparison
                //
                if (user.readPermissionLevel >= entityMinimumReadSecurityLevel)
                {
                    return true;
                }
                else
                {
                    //
                    // User is reader, but does not have sufficient reader privilege level on their user record to read from this entity. 
                    //
                    return false;
                }
            }

            return false;
        }

        public static bool UserMustReadAnonymously(string moduleName, List<RoleAndPrivilege> rolesForModule, SecurityUser user)
        {
            //
            // Will return true if the only privilege a user has is one that forces them to read anonymously.
            //
            // This is useful because there will likely be situations where a user belongs to more than one role, via Groups, etc..  So it is likely that they
            // will belong to one group with anonymous read, and another with real read.  Therefore, this will considre the read read as an override to the anonymous read, if such a 
            // real read permission exists.
            //

            //
            // Get the roles the user has access to in the provided module, if one is provided
            //
            List<SecurityRole> userRoles = null;

            if (string.IsNullOrEmpty(moduleName) == false)
            {
                userRoles = GetUserSecurityRolesInModule(user, moduleName);
            }
            else
            {
                userRoles = GetUserSecurityRoles(user);
            }

            bool regularReadPrivilegeFound = false;
            bool anonymousReadPrivilegeFound = false;

            for (int i = 0; i < userRoles.Count; i++)
            {
                SecurityRole role = userRoles.ElementAt(i);

                for (int j = 0; j < rolesForModule.Count; j++)
                {
                    RoleAndPrivilege moduleRole = rolesForModule.ElementAt(j);

                    if (moduleRole.roleName == role.name &&
                        (
                            moduleRole.privilege == SecurityLogic.Privileges.ADMINISTRATIVE ||
                            moduleRole.privilege == SecurityLogic.Privileges.READ_AND_WRITE ||
                            moduleRole.privilege == SecurityLogic.Privileges.READ_ONLY
                        ))
                    {
                        regularReadPrivilegeFound = true;
                    }
                    else if (moduleRole.roleName == role.name && moduleRole.privilege == SecurityLogic.Privileges.ANONYMOUS_READ_ONLY)
                    {
                        anonymousReadPrivilegeFound = true;
                    }
                }
            }


            if (regularReadPrivilegeFound == false && anonymousReadPrivilegeFound == true)
            {
                // caller will know they must anonymize
                return true;
            }
            else
            {
                // caller will need to check if there is regular read permission, if they have not done so yet. 
                return false;
            }
        }

        public async static Task<bool> UserCanWriteAsync(string moduleName, List<RoleAndPrivilege> rolesForModule, SecurityUser user, int entityMinimumWriteSecurityLevel = 0, CancellationToken cancellationToken = default)
        {
            // Nothing is allowed if we don't know who this is.
            if (user == null)
            {
                return false;
            }


            //
            // TENANT GUARD CHECK
            //
            // Before evaluating per-module roles, check the tenant-level entitlement flags.
            //
            // If the user is the tenant owner, they can always write — return true immediately.
            //
            // If canRead is false, the user can't even access the tenant, so writing is also denied.
            //
            // If canWrite is false (but canRead is true), the user is in a read-only state at the
            // tenant level.  They cannot write to any module regardless of their roles.
            //
            SecurityTenantUser tenantUser = await GetTenantUserGuardAsync(user, cancellationToken);

            if (tenantUser != null)
            {
                // Tenant owners can always write.
                if (tenantUser.isOwner == true)
                {
                    return true;
                }

                // Cannot write if the user can't even read at the tenant level.
                if (tenantUser.canRead == false)
                {
                    return false;
                }

                // Cannot write if the tenant-level write flag is explicitly off.
                if (tenantUser.canWrite == false)
                {
                    return false;
                }
            }


            //
            // Get the roles the user has access to in the provided module, if one is provided
            //
            List<SecurityRole> userRoles = null;

            if (string.IsNullOrEmpty(moduleName) == false)
            {
                userRoles = await GetUserSecurityRolesInModuleAsync(user, moduleName, cancellationToken);
            }
            else
            {
                userRoles = await GetUserSecurityRolesAsync(user, cancellationToken);
            }

            bool adminFound = false;
            bool writerFound = false;

            //
            // Figure out if the user can either administer or write to module who's roles and privileges have been provided
            //
            for (int i = 0; i < userRoles.Count; i++)
            {
                SecurityRole roleForUser = userRoles.ElementAt(i);

                for (int j = 0; j < rolesForModule.Count; j++)
                {
                    RoleAndPrivilege moduleRole = rolesForModule.ElementAt(j);

                    if (moduleRole.roleName == roleForUser.name && moduleRole.privilege == SecurityLogic.Privileges.ADMINISTRATIVE)
                    {
                        adminFound = true;
                        break;  // don't need to look for anything further as we have the top privilege.
                    }
                    else if (moduleRole.roleName == roleForUser.name && moduleRole.privilege == SecurityLogic.Privileges.READ_AND_WRITE)
                    {
                        writerFound = true;     // don't stop looking here because admin could be later in the list.
                    }
                }
            }


            //
            // Based on the admin / writer state for this user, decide what to do.
            //
            if (adminFound == true)
            {
                // admins can always write, regardless of their or the entity writer security level
                return true;
            }
            else if (writerFound == true)
            {
                //
                // Is this user privlieged enough to write to this entity, based on a security level comparison
                //
                if (user.writePermissionLevel >= entityMinimumWriteSecurityLevel)
                {
                    return true;
                }
                else
                {
                    //
                    // User is writer, but does not have sufficient writer privilege level on their user record to write to this entity. 
                    //
                    return false;
                }
            }

            return false;
        }


        public static bool UserCanWrite(string moduleName, List<RoleAndPrivilege> rolesForModule, SecurityUser user, int entityMinimumWriteSecurityLevel = 0)
        {
            // Nothing is allowed if we don't know who this is.
            if (user == null)
            {
                return false;
            }


            //
            // TENANT GUARD CHECK — see async version for full documentation.
            //
            SecurityTenantUser tenantUser = GetTenantUserGuard(user);

            if (tenantUser != null)
            {
                if (tenantUser.isOwner == true)
                {
                    return true;
                }

                if (tenantUser.canRead == false)
                {
                    return false;
                }

                if (tenantUser.canWrite == false)
                {
                    return false;
                }
            }


            //
            // Get the roles the user has access to in the provided module, if one is provided
            //
            List<SecurityRole> userRoles = null;

            if (string.IsNullOrEmpty(moduleName) == false)
            {
                userRoles = GetUserSecurityRolesInModule(user, moduleName);
            }
            else
            {
                userRoles = GetUserSecurityRoles(user);
            }

            bool adminFound = false;
            bool writerFound = false;

            //
            // Figure out if the user can either administer or write to module who's roles and privileges have been provided
            //
            for (int i = 0; i < userRoles.Count; i++)
            {
                SecurityRole roleForUser = userRoles.ElementAt(i);

                for (int j = 0; j < rolesForModule.Count; j++)
                {
                    RoleAndPrivilege moduleRole = rolesForModule.ElementAt(j);

                    if (moduleRole.roleName == roleForUser.name && moduleRole.privilege == SecurityLogic.Privileges.ADMINISTRATIVE)
                    {
                        adminFound = true;
                        break;  // don't need to look for anything further as we have the top privilege.
                    }
                    else if (moduleRole.roleName == roleForUser.name && moduleRole.privilege == SecurityLogic.Privileges.READ_AND_WRITE)
                    {
                        writerFound = true;     // don't stop looking here because admin could be later in the list.
                    }
                }
            }


            //
            // Based on the admin / writer state for this user, decide what to do.
            //
            if (adminFound == true)
            {
                // admins can always write, regardless of their or the entity writer security level
                return true;
            }
            else if (writerFound == true)
            {
                //
                // Is this user privleged enough to write to this entity, based on a security level comparison
                //
                if (user.writePermissionLevel >= entityMinimumWriteSecurityLevel)
                {
                    return true;
                }
                else
                {
                    //
                    // User is writer, but does not have sufficient writer privilege level on their user record to write to this entity. 
                    //
                    return false;
                }
            }

            return false;
        }

        public async static Task<bool> UserCanAdministerAsync(string moduleName, List<RoleAndPrivilege> rolesForModule, SecurityUser user, CancellationToken cancellationToken = default)
        {
            // Nothing is allowed if we don't know who this is.
            if (user == null)
            {
                return false;
            }


            //
            // TENANT GUARD CHECK
            //
            // Tenant owners have full administrative rights to every module — return true immediately.
            //
            // If canRead is false, the user has no access at all — deny administration.
            //
            // If canWrite is false, the user is read-only at the tenant level — administration requires
            // write capability, so deny.
            //
            SecurityTenantUser tenantUser = await GetTenantUserGuardAsync(user, cancellationToken);

            if (tenantUser != null)
            {
                // Tenant owners are always administrators.
                if (tenantUser.isOwner == true)
                {
                    return true;
                }

                // No read access means no access at all.
                if (tenantUser.canRead == false)
                {
                    return false;
                }

                // Read-only users cannot administer.
                if (tenantUser.canWrite == false)
                {
                    return false;
                }
            }



            //
            // Get the roles the user has access to in the provided module, if one is provided
            //
            List<SecurityRole> userRoles = null;

            if (string.IsNullOrEmpty(moduleName) == false)
            {
                userRoles = await GetUserSecurityRolesInModuleAsync(user, moduleName, cancellationToken);
            }
            else
            {
                userRoles = await GetUserSecurityRolesAsync(user, cancellationToken);
            }

            for (int i = 0; i < userRoles.Count; i++)
            {
                SecurityRole userRole = userRoles.ElementAt(i);

                for (int j = 0; j < rolesForModule.Count; j++)
                {
                    RoleAndPrivilege moduleRole = rolesForModule.ElementAt(j);

                    if (moduleRole.roleName == userRole.name && moduleRole.privilege == SecurityLogic.Privileges.ADMINISTRATIVE)
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        public static bool UserCanAdminister(string moduleName, List<RoleAndPrivilege> rolesForModule, SecurityUser user)
        {
            // Nothing is allowed if we don't know who this is.
            if (user == null)
            {
                return false;
            }


            //
            // TENANT GUARD CHECK — see async version for full documentation.
            //
            SecurityTenantUser tenantUser = GetTenantUserGuard(user);

            if (tenantUser != null)
            {
                if (tenantUser.isOwner == true)
                {
                    return true;
                }

                if (tenantUser.canRead == false)
                {
                    return false;
                }

                if (tenantUser.canWrite == false)
                {
                    return false;
                }
            }



            //
            // Get the roles the user has access to in the provided module, if one is provided
            //
            List<SecurityRole> userRoles = null;

            if (string.IsNullOrEmpty(moduleName) == false)
            {
                userRoles = GetUserSecurityRolesInModule(user, moduleName);
            }
            else
            {
                userRoles = GetUserSecurityRoles(user);
            }

            for (int i = 0; i < userRoles.Count; i++)
            {
                SecurityRole userRole = userRoles.ElementAt(i);

                for (int j = 0; j < rolesForModule.Count; j++)
                {
                    RoleAndPrivilege moduleRole = rolesForModule.ElementAt(j);

                    if (moduleRole.roleName == userRole.name && moduleRole.privilege == SecurityLogic.Privileges.ADMINISTRATIVE)
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        public static bool UserIsInRole(string roleName, SecurityUser user)
        {
            // Nothing is allowed if we don't know who this is.
            if (user == null)
            {
                return false;
            }

            List<SecurityRole> userRoles = GetUserSecurityRoles(user);

            for (int i = 0; i < userRoles.Count; i++)
            {
                if (userRoles.ElementAt(i).name == roleName)
                {
                    return true;
                }
            }

            return false;
        }


        public static async Task<List<SecurityRole>> GetUserSecurityRolesAsync(SecurityUser user, CancellationToken cancellationToken = default)
        {
            List<SecurityUserSecurityGroup> groups = null;

            //
            // Both of these sub functions are memory cached
            //
            groups = await GetUserGroupsAsync(user.id, cancellationToken);

            List<SecurityRole> userRoles = await SecurityLogic.GetRolesUserIsEntitledToAsync(user, groups, cancellationToken);

            return userRoles;
        }


        public static List<SecurityRole> GetUserSecurityRoles(SecurityUser user)
        {
            List<SecurityUserSecurityGroup> groups = null;

            //
            // Both of these sub functions are memory cached
            //
            groups = GetUserGroups(user.id);

            List<SecurityRole> userRoles = SecurityLogic.GetRolesUserIsEntitledTo(user, groups);

            return userRoles;
        }


        public async static Task<List<SecurityRole>> GetUserSecurityRolesInModuleAsync(SecurityUser user, string moduleName, CancellationToken cancellationToken = default)
        {
            List<SecurityUserSecurityGroup> groups = null;

            //
            // Both of these sub functions are memory cached
            //
            groups = await GetUserGroupsAsync(user.id, cancellationToken);

            List<SecurityRole> userRoles = await SecurityLogic.GetRolesUserIsEntitledToForModuleAsync(moduleName, user, groups, cancellationToken);

            return userRoles;
        }


        public static List<SecurityRole> GetUserSecurityRolesInModule(SecurityUser user, string moduleName)
        {
            List<SecurityUserSecurityGroup> groups = null;

            //
            // Both of these sub functions are memory cached
            //
            groups = GetUserGroups(user.id);

            List<SecurityRole> userRoles = SecurityLogic.GetRolesUserIsEntitledToForModule(moduleName, user, groups);

            return userRoles;
        }


        public static async Task<List<ComplexListItem>> GetListDataForUsersWithModulePermissionAsync(SecurityContext db, string moduleName, SecurityLogic.Privileges privilege, int? active = null, CancellationToken cancellationToken = default)
        {
            //
            // This returns a list of the users that have roles that entitle them to the specified module with the specified privilege or better.
            //
            // Note, that this does not consider group membership because that is done in the AD, so we don't know what groups contain what users so we can't work backwards
            // and produce a list of users who gain a  privilege via a group membership because we don't know ( or at least don't know unless we query AD ) what users belong to what groups..
            //
            // For this reason, at this point in time, it is better to assign important functions to a user directly rather than to an AD group, or this function may not return a complete
            // list of users that can access a particular function.
            //
            List<ComplexListItem> output = await (from ur in db.SecurityUserSecurityRoles
                                                  join u in db.SecurityUsers on ur.securityUserId equals u.id
                                                  join sr in db.SecurityRoles on ur.securityRoleId equals sr.id
                                                  join mr in db.ModuleSecurityRoles on sr.id equals mr.securityRoleId
                                                  join m in db.Modules on mr.moduleId equals m.id
                                                  where
                                                  (ur.deleted == false && u.deleted == false && sr.deleted == false && mr.deleted == false && m.deleted == false) &&
                                                  (active.HasValue == false || ur.active == (active.Value == 1 ? true : false)) &&
                                                  (active.HasValue == false || u.active == (active.Value == 1 ? true : false)) &&
                                                  (active.HasValue == false || sr.active == (active.Value == 1 ? true : false)) &&
                                                  (active.HasValue == false || mr.active == (active.Value == 1 ? true : false)) &&
                                                  (active.HasValue == false || m.active == (active.Value == 1 ? true : false)) &&
                                                  (active.HasValue == false || ur.active == (active.Value == 1 ? true : false)) &&
                                                  ((privilege == SecurityLogic.Privileges.CUSTOM && sr.privilegeId == (int)SecurityLogic.Privileges.CUSTOM) || ((int)privilege < (int)SecurityLogic.Privileges.CUSTOM && sr.privilegeId >= (int)privilege)) &&       // 6 is for custom, so that must be targeted exactly.  values 1-5 are increasing levels of permission so read/write gets read etc...
                                                  (m.name == moduleName)
                                                  select new ComplexListItem
                                                  {
                                                      id = u.id,
                                                      name = u.accountName,
                                                      description = u.lastName + ", " + u.firstName + (u.middleName != null ? " " + u.middleName : "") + " (" + u.accountName + ")"
                                                  })
                                                .Distinct()
                                                .OrderBy(x => x.description)
                                                .ToListAsync(cancellationToken)
                                                .ConfigureAwait(false);

            if (output == null)
            {
                output = new List<ComplexListItem>();
            }


            return output;
        }

        public static List<ComplexListItem> GetListDataForUsersWithModulePermission(SecurityContext db, string moduleName, SecurityLogic.Privileges privilege, int? active = null)
        {
            //
            // This returns a list of the users that have roles that entitle them to the specified module with the specified privilege or better.
            //
            // Note, that this does not consider group membership because that is done in the AD, so we don't know what groups contain what users so we can't work backwards
            // and produce a list of users who gain a  privilege via a group membership because we don't know ( or at least don't know unless we query AD ) what users belong to what groups..
            //
            // For this reason, at this point in time, it is better to assign important functions to a user directly rather than to an AD group, or this function may not return a complete
            // list of users that can access a particular function.
            //
            List<ComplexListItem> output = (from ur in db.SecurityUserSecurityRoles
                                            join u in db.SecurityUsers on ur.securityUserId equals u.id
                                            join sr in db.SecurityRoles on ur.securityRoleId equals sr.id
                                            join mr in db.ModuleSecurityRoles on sr.id equals mr.securityRoleId
                                            join m in db.Modules on mr.moduleId equals m.id
                                            where
                                            (ur.deleted == false && u.deleted == false && sr.deleted == false && mr.deleted == false && m.deleted == false) &&
                                            (active.HasValue == false || ur.active == (active.Value == 1 ? true : false)) &&
                                            (active.HasValue == false || u.active == (active.Value == 1 ? true : false)) &&
                                            (active.HasValue == false || sr.active == (active.Value == 1 ? true : false)) &&
                                            (active.HasValue == false || mr.active == (active.Value == 1 ? true : false)) &&
                                            (active.HasValue == false || m.active == (active.Value == 1 ? true : false)) &&
                                            (active.HasValue == false || ur.active == (active.Value == 1 ? true : false)) &&
                                            ((privilege == SecurityLogic.Privileges.CUSTOM && sr.privilegeId == (int)SecurityLogic.Privileges.CUSTOM) || ((int)privilege < (int)SecurityLogic.Privileges.CUSTOM && sr.privilegeId >= (int)privilege)) &&       // 6 is for custom, so that must be targeted exactly.  values 1-5 are increasing levels of permission so read/write gets read etc...
                                            (m.name == moduleName)
                                            select new ComplexListItem
                                            {
                                                id = u.id,
                                                name = u.accountName,
                                                description = u.lastName + ", " + u.firstName + (u.middleName != null ? " " + u.middleName : "") + " (" + u.accountName + ")"
                                            })
                                                .Distinct()
                                                .OrderBy(x => x.description)
                                                .ToList();

            if (output == null)
            {
                output = new List<ComplexListItem>();
            }


            return output;
        }

        /// <summary>
        /// 
        /// Returns a list of SecurityUser objects that have been directly assigned a specific named role
        /// that carries the CUSTOM privilege.  Password fields are nulled out for safety.
        ///
        /// When securityTenantId is provided, the results are filtered to only include users that are
        /// linked to that tenant via the SecurityTenantUser table.  When null, all users system-wide
        /// with the specified custom role are returned.
        ///
        /// Note: This does not consider group-based role membership (same caveat as
        /// GetListDataForUsersWithModulePermission).  Only directly-assigned roles are evaluated.
        ///
        /// AI Developed - Feb 2026
        /// 
        /// </summary>
        /// <param name="db">An existing SecurityContext to query against.</param>
        /// <param name="roleName">The name of the custom security role to search for.</param>
        /// <param name="securityTenantId">Optional tenant ID to restrict results to users linked to that tenant.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A list of SecurityUser objects with the password field nulled out.</returns>
        public static async Task<List<SecurityUser>> GetUsersWithCustomRoleAsync(SecurityContext db,
                                                                                 string roleName,
                                                                                 int? securityTenantId = null,
                                                                                 CancellationToken cancellationToken = default)
        {
            List<SecurityUser> output = null;

            //
            // Build the base query that joins user-role assignments to security roles,
            // filtering for the specified role name with the CUSTOM privilege level.
            //
            // When a securityTenantId is provided, an additional join to SecurityTenantUsers
            // restricts the results to users that belong to that tenant.
            //
            if (securityTenantId.HasValue == true)
            {
                //
                // Tenant-scoped query: only return users linked to the specified tenant
                //
                output = await (from ur in db.SecurityUserSecurityRoles
                                join u in db.SecurityUsers on ur.securityUserId equals u.id
                                join sr in db.SecurityRoles on ur.securityRoleId equals sr.id
                                join stu in db.SecurityTenantUsers on u.id equals stu.securityUserId
                                where ur.deleted == false &&
                                      ur.active == true &&
                                      u.deleted == false &&
                                      u.active == true &&
                                      sr.deleted == false &&
                                      sr.active == true &&
                                      sr.name == roleName &&
                                      sr.privilegeId == (int)SecurityLogic.Privileges.CUSTOM &&
                                      stu.securityTenantId == securityTenantId.Value &&
                                      stu.deleted == false &&
                                      stu.active == true
                                select u)
                                .Distinct()
                                .OrderBy(u => u.lastName)
                                .ThenBy(u => u.firstName)
                                .AsNoTracking()
                                .ToListAsync(cancellationToken)
                                .ConfigureAwait(false);
            }
            else
            {
                //
                // System-wide query: return all users with the specified custom role regardless of tenant
                //
                output = await (from ur in db.SecurityUserSecurityRoles
                                join u in db.SecurityUsers on ur.securityUserId equals u.id
                                join sr in db.SecurityRoles on ur.securityRoleId equals sr.id
                                where ur.deleted == false &&
                                      ur.active == true &&
                                      u.deleted == false &&
                                      u.active == true &&
                                      sr.deleted == false &&
                                      sr.active == true &&
                                      sr.name == roleName &&
                                      sr.privilegeId == (int)SecurityLogic.Privileges.CUSTOM
                                select u)
                                .Distinct()
                                .OrderBy(u => u.lastName)
                                .ThenBy(u => u.firstName)
                                .AsNoTracking()
                                .ToListAsync(cancellationToken)
                                .ConfigureAwait(false);
            }

            if (output == null)
            {
                output = new List<SecurityUser>();
            }

            //
            // Null out the password field on each user for safety before returning
            //
            for (int i = 0; i < output.Count; i++)
            {
                output[i].password = null;
            }

            return output;
        }


        /// <summary>
        /// 
        /// Synchronous version of GetUsersWithCustomRoleAsync.
        /// See the async version for full documentation.
        ///
        /// AI Developed - Feb 2026
        /// 
        /// </summary>
        public static List<SecurityUser> GetUsersWithCustomRole(SecurityContext db,
                                                                 string roleName,
                                                                 int? securityTenantId = null)
        {
            List<SecurityUser> output = null;

            //
            // Build the base query that joins user-role assignments to security roles,
            // filtering for the specified role name with the CUSTOM privilege level.
            //
            if (securityTenantId.HasValue == true)
            {
                //
                // Tenant-scoped query: only return users linked to the specified tenant
                //
                output = (from ur in db.SecurityUserSecurityRoles
                          join u in db.SecurityUsers on ur.securityUserId equals u.id
                          join sr in db.SecurityRoles on ur.securityRoleId equals sr.id
                          join stu in db.SecurityTenantUsers on u.id equals stu.securityUserId
                          where ur.deleted == false &&
                                ur.active == true &&
                                u.deleted == false &&
                                u.active == true &&
                                sr.deleted == false &&
                                sr.active == true &&
                                sr.name == roleName &&
                                sr.privilegeId == (int)SecurityLogic.Privileges.CUSTOM &&
                                stu.securityTenantId == securityTenantId.Value &&
                                stu.deleted == false &&
                                stu.active == true
                          select u)
                          .Distinct()
                          .OrderBy(u => u.lastName)
                          .ThenBy(u => u.firstName)
                          .AsNoTracking()
                          .ToList();
            }
            else
            {
                //
                // System-wide query: return all users with the specified custom role regardless of tenant
                //
                output = (from ur in db.SecurityUserSecurityRoles
                          join u in db.SecurityUsers on ur.securityUserId equals u.id
                          join sr in db.SecurityRoles on ur.securityRoleId equals sr.id
                          where ur.deleted == false &&
                                ur.active == true &&
                                u.deleted == false &&
                                u.active == true &&
                                sr.deleted == false &&
                                sr.active == true &&
                                sr.name == roleName &&
                                sr.privilegeId == (int)SecurityLogic.Privileges.CUSTOM
                          select u)
                          .Distinct()
                          .OrderBy(u => u.lastName)
                          .ThenBy(u => u.firstName)
                          .AsNoTracking()
                          .ToList();
            }

            if (output == null)
            {
                output = new List<SecurityUser>();
            }

            //
            // Null out the password field on each user for safety before returning
            //
            for (int i = 0; i < output.Count; i++)
            {
                output[i].password = null;
            }

            return output;
        }


        public static void ClearSecurityCaches()
        {
            MemoryCacheManager mcm = new MemoryCacheManager();

            mcm.RemoveByPattern("^SF_");        // Clear any item starting with SF
        }
    }
}