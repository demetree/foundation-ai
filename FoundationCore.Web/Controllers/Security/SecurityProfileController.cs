using Foundation.Security.Database;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Foundation.Security.Controllers.WebAPI
{
    //[Authorize] We don't want the authorize attribute on SecureWebAPIControllers because the Security System manages its own security and the attribute stops anonymous access from working.  
    // The Security System Security will handle the rejection of invalid requests.
    public class SecurityProfileController : SecureWebAPIController
    {
        //
        // The purpose of this class is to allow a means for the client to get a list of modules, and security levels to each module that the logged in user
        // has so that it can decide what buttons to activate, etc..
        //
        public SecurityProfileController() : base("Security", "SecurityProfile")
        {
            return;
        }

        private SecurityContext db = new SecurityContext();

        [HttpGet]
        [Route("api/SecurityProfile")]
        public async Task<IActionResult> GetSecurityProfile()
        {
            //
            // This is a utility method that is called on each page load, usually more than once.  Auditing this read doesn't add value, and clutters the auditEvent table.
            //
            SecurityFramework.SecurityProfile profile = await GetCurrentUserSecurityProfileFromSecurityFrameworkAsync();
            return Ok(profile);
        }


        [HttpGet]
        [Route("api/SecurityProfiles")]
        public async Task<IActionResult> GetSecurityProfiles()
        {
            StartAuditEventClock();

            if (await IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel.Read) == false)
            {
                return Unauthorized();
            }
            //
            // This returns a list.  - for now, it will just contain the current user.  Possibly can extend this to show all users to people who can read the security module.
            //
            SecurityFramework.SecurityProfile profile = await GetCurrentUserSecurityProfileFromSecurityFrameworkAsync();

            List<SecurityFramework.SecurityProfile> output = new List<SecurityFramework.SecurityProfile>();
            output.Add(profile);

            await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.ReadList, "Security Profile List was read for current user");

            return Ok(output);
        }

        [HttpGet]
        [Route("api/SecurityProfile/{userId:int}")]
        public async Task<IActionResult> GetSecurityProfile(int userId)
        {
            StartAuditEventClock();

            if (await IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel.Read) == false)
            {
                return Unauthorized();
            }

            //
            // This returns a list.  - for now, it will just contain the current user.  Possibly can extend this to show all users to people who can read the security module.
            //

            SecurityFramework.SecurityProfile profile = await GetCurrentUserSecurityProfileFromSecurityFrameworkAsync();

            await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.ReadEntity, "Security Profile was read");

            return Ok(profile);
        }



        [HttpGet]
        [Route("api/SecurityProfile/ListDataForUsersWithModulePermission")]
        public async Task<IActionResult> GetListDataForUsersWithModulePermission(string moduleName, string privilegeName, int? active = 1)
        {
            //
            // Get a list of the users that have the specified privilege to the specified  module
            //
            // User making call must have read access to the module before this will return anything.
            //
            SecurityLogic.Privileges privilege;


            if (Enum.TryParse<SecurityLogic.Privileges>(privilegeName, out privilege) == false)
            {
                throw new Exception("Invalid privilege name.");
            }

            SecurityFramework.SecurityProfile currentUserProfile = await GetCurrentUserSecurityProfileFromSecurityFrameworkAsync();

            bool userCanQueryForListOfOtherUsersWithPrivilege = false;

            if (currentUserProfile.roles != null)
            {
                foreach (SecurityFramework.ModuleAndRoleAndPrivilege mr in currentUserProfile.roles)
                {
                    if (mr.moduleName == moduleName)
                    {
                        if (privilege == SecurityLogic.Privileges.CUSTOM)
                        {
                            // have to have custom privilege to look for cusom privilege
                            if (mr.privilege == SecurityLogic.Privileges.NO_ACCESS)
                            {
                                userCanQueryForListOfOtherUsersWithPrivilege = false;
                                break;          // we have found the explicit no acess.  Reset flag to false and stop searching here,
                            }
                            else if (mr.privilege == SecurityLogic.Privileges.ANONYMOUS_READ_ONLY ||
                                mr.privilege == SecurityLogic.Privileges.READ_ONLY ||
                                mr.privilege == SecurityLogic.Privileges.READ_AND_WRITE ||
                                mr.privilege == SecurityLogic.Privileges.ADMINISTRATIVE)
                            {
                                // don't break beacuse we might find a no access later in the list
                                userCanQueryForListOfOtherUsersWithPrivilege = true;
                            }
                        }
                        else
                        {
                            if (mr.privilege == SecurityLogic.Privileges.NO_ACCESS)
                            {
                                userCanQueryForListOfOtherUsersWithPrivilege = false;
                                break;          // we have found the explicit no acess.  Reset flag to false and stop searching here,
                            }
                            else if (mr.privilege == SecurityLogic.Privileges.ANONYMOUS_READ_ONLY ||
                                mr.privilege == SecurityLogic.Privileges.READ_ONLY ||
                                mr.privilege == SecurityLogic.Privileges.READ_AND_WRITE ||
                                mr.privilege == SecurityLogic.Privileges.ADMINISTRATIVE)
                            {
                                // don't break beacuse we might find a no access later in the list
                                userCanQueryForListOfOtherUsersWithPrivilege = true;
                            }
                        }
                    }
                }
            }

            if (userCanQueryForListOfOtherUsersWithPrivilege == true)
            {
                return Ok(await SecurityFramework.GetListDataForUsersWithModulePermissionAsync(db, moduleName, privilege, active));
            }
            else
            {
                throw new UnauthorizedAccessException("User does not have privilege to access the list of users for the provided module and privilege.");
            }
        }

    }
}