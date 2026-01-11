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


        public async static Task<SecurityProfile> GetUserSecurityProfileAsync(SecurityUser user, CancellationToken cancellationToken = default)
        {
            SecurityProfile output = new SecurityProfile();

            if (user == null)
            {
                return output;
            }

            output.accountName = user.accountName;
            output.id = user.id;


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

                    //return UserIsInRole(roleName, user);
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

        public static void ClearSecurityCaches()
        {
            MemoryCacheManager mcm = new MemoryCacheManager();

            mcm.RemoveByPattern("^SF_");        // Clear any item starting with SF
        }
    }
}