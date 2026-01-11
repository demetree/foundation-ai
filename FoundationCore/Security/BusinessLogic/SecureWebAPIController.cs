using Foundation.Auditor;
using Foundation.Concurrent;
using Foundation.Security.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Foundation.Configuration;
using static Foundation.Security.SecurityFramework;

namespace Foundation.Security
{
    //
    // All controllers deriving from this require authorization in ASP.Net
    //
    [Authorize]
    public class SecureWebAPIController : ControllerBase
    {
        protected string moduleName;
        protected string entityName;

        private SecurityContext securityDb = new SecurityContext();
        private List<SecurityFramework.RoleAndPrivilege> rolesAndPrivilegesForThisModule;
        private string environmentName = null;
        private DateTime auditEventStartTime = DateTime.MinValue;

        private const string SECURITY_MODULE_NAME = "Security";


        //
        // 5 seconds to suppress repeated audit messages for - This means the same audit log message won't be created more than once every 5 seconds
        //
        private static ExpiringCache<string, bool> _auditFloodCheckerCache = new ExpiringCache<string, bool>(5);


        protected SecureWebAPIController(string moduleName, string entityName)
        {
            this.moduleName = moduleName;
            this.entityName = entityName;

            //
            // First, get all of the roles and privileges for this module.
            //
            rolesAndPrivilegesForThisModule = SecurityFramework.GetRolesAndPrivilegesForModule(this.moduleName, securityDb);
        }


        protected ISession GetSession()
        {
            return HttpContext.Session;
        }


        protected async Task<SecurityUser> GetSecurityUserAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (HttpContext.User != null && HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated == true)
                {
                    /* This doesn't work with the access token.  Switching to use the sub claim which stores the user's object guid.
                    string userName = HttpContext.User.Identity.Name;

                    if (userName != null)
                    {
                        return await SecurityLogic.GetUserRecordAsync(userName);
                    }
                    else
                    {
                        return null;
                    }
                    */

                    string userObjectGuidString = (from x in HttpContext.User.Claims where x.Type == "sub" select x.Value).FirstOrDefault();

                    if (userObjectGuidString != null)
                    {
                        if (Guid.TryParse(userObjectGuidString, out Guid userObjectGuid) == true)
                        {
                            return await SecurityLogic.GetUserRecordAsync(userObjectGuid, cancellationToken);
                        }
                    }

                    return null;
                }
                else
                {
                    //
                    // Cheater method to help API Test programs resolve a user when allowing anonymous connections AND are running in the debugger.  
                    //
                    // Intended for API test projects more than anything else.
                    //
                    string anonymousUserSubstitute = Foundation.Configuration.GetStringConfigurationSetting("AnonymousUserSubstitute", null);

                    if (string.IsNullOrEmpty(anonymousUserSubstitute) == false)
                    {

                        if (System.Diagnostics.Debugger.IsAttached == true)
                        {
                            string logMessage = "FOUNDATION SECURITY BYPASS IS ENABLED. THIS WILL ONLY WORK WHEN DEBUGGING.  FIX THIS BY FINDING A BETTER SOLUTION!!!";
                            System.Diagnostics.Debug.WriteLine(logMessage);
                            Console.WriteLine(logMessage);

                            return await SecurityLogic.GetUserRecordAsync(anonymousUserSubstitute, cancellationToken);
                        }
                        else
                        {
                            throw new Exception("Anonymous User Substitution is Not Support When Not Debugging");
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }


        protected SecurityUser GetSecurityUser()
        {
            try
            {
                if (HttpContext.User != null && HttpContext.User.Identity.IsAuthenticated == true)
                {
                    string userName = HttpContext.User.Identity.Name;

                    return SecurityLogic.GetUserRecord(userName);
                }
                else
                {
                    //
                    // Cheater method to help API Test programs resolve a user when allowing anonymous connections AND are running in the debugger.  
                    //
                    // Intended for API test projects more than anything else.
                    //
                    string anonymousUserSubstitute = Foundation.Configuration.GetStringConfigurationSetting("AnonymousUserSubstitute", null);

                    if (string.IsNullOrEmpty(anonymousUserSubstitute) == false)
                    {
                        if (System.Diagnostics.Debugger.IsAttached == true)
                        {
                            return SecurityLogic.GetUserRecord(anonymousUserSubstitute);
                        }
                        else
                        {
                            throw new Exception("Anonymous User Substitution is Not Support When Not Debugging");
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }

        protected SecurityFramework.SecurityProfile GetCurrentUserSecurityProfileFromSecurityFramework()
        {
            return SecurityFramework.GetUserSecurityProfile(GetSecurityUser());
        }

        protected async Task<SecurityFramework.SecurityProfile> GetCurrentUserSecurityProfileFromSecurityFrameworkAsync(CancellationToken cancellationToken = default)
        {
            return await SecurityFramework.GetUserSecurityProfileAsync(await GetSecurityUserAsync(cancellationToken), cancellationToken);
        }

        protected void DestroySessionAndAuthentication()
        {
            ISession session = GetSession();
            session.Clear();
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }


        protected string GetEntityDataToken()
        {
            string entityDataToken = null;

            if (Request.Headers.ContainsKey("Entity-Data-Token") == true)
            {
                Microsoft.Extensions.Primitives.StringValues values;

                if (Request.Headers.TryGetValue("Entity-Data-Token", out values) == true)   // IEnumerable<string> headerValues = Rquest.Headers.GetCommaSeparatedValues("Entity-Data-Token");
                {
                    entityDataToken = values.FirstOrDefault();
                }
            }

            return entityDataToken;
        }


        protected async Task<bool> IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel trustLevel, string comments = null, string alternateModule = null, CancellationToken cancellationToken = default)
        {
            //
            // The alternate module parameter helps custom screens belonging to sub modules that reside under a main module.  
            //
            // The main module is whatever module the HomeController declares as it's module.  
            // Any CUSTOM SCREEN in a sub module must report the main module as their alternate module when checking the EDT if using shared web api controller methods with templatized screens
            // and that is why the moduleOverride is here.
            //
            if (Foundation.Configuration.GetBooleanConfigurationSetting(TokenLogic.DISABLE_CROSS_SITE_FORGERY_CONFIGURATION_SETTING_NAME, false) == true)
            {
                return true;
            }


            return true;

            /* Disabling the entity data token concept in FoundationCore for now.

            SecurityUser securityUser = GetSecurityUser();

            ISession session = GetSession();

            string authenticationToken = GetAuthenticationTokenFromAuthenticationCookie();

            if (string.IsNullOrEmpty(authenticationToken) == true)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UnauthorizedAccessAttempt, "User attempted to access a resource requiring an authentication token, which could not be found.  User is: " + (securityUser != null ? securityUser.accountName : "null"), false);
                return false;
            }


            string commentsToSaveOnEvent = this.Request.Method.ToString() + " - " + Request.Path.ToString() + HttpContext.Request.QueryString.ToString() + (comments != null ? " - " + comments : "");

            string entityDataToken = GetEntityDataToken();

            if (string.IsNullOrWhiteSpace(entityDataToken) == true)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UnauthorizedAccessAttempt, "User attempted to access a resource requiring a token which was not provided.  User is: " + (securityUser != null ? securityUser.accountName : "null"), false);
                return false;
            }

            //
            // try the entity data token to see if it works.  If it doesn't, there is a scenario when it can still be approved at this level.
            //
            // Cross module requests where an alternate module is provided will retry with the alternate module name.
            //
            if (await TokenLogic.ValidateEntityDataTokenAsync(trustLevel, entityDataToken, securityUser, this.moduleName, this.entityName, authenticationToken, commentsToSaveOnEvent) == false)
            {
                //
                // First try doesn't work, but is the circumstance such that it is an expected failure and it can be allowed?
                //

                //
                // Do we have an alternate module to try?  If not, fail out.
                //
                // If so, try that module too before failing.  This is for cross module screen calling support
                //
                if (alternateModule != null)
                {
                    //
                    // Try again with the alternate module being the primary module
                    //
                    if (await TokenLogic.ValidateEntityDataTokenAsync(trustLevel, entityDataToken, securityUser, alternateModule, this.entityName, authenticationToken, commentsToSaveOnEvent) == false)
                    {
                        await CreateAuditEventAsync(AuditEngine.AuditType.UnauthorizedAccessAttempt, "User attempted to access a resource requiring a token, but an invalid token value of '" + entityDataToken + "' was provided.  Both main and alternate modules were tried.  User is: '" + (securityUser != null ? securityUser.accountName : "null") + "' Module is: '" + this.moduleName + "' Alternate module is: '" + alternateModule + "' Entity is:  '" + this.entityName + "' Authentication Token is: '" + authenticationToken + "' Session is: '" + session.Id + "'", false);
                        return false;
                    }
                }
                else
                {
                    //
                    // no alternate module provided.  Fail out.
                    //
                    await CreateAuditEventAsync(AuditEngine.AuditType.UnauthorizedAccessAttempt, "User attempted to access a resource requiring a token, but an invalid entity data token value of '" + entityDataToken + "' was provided.  User is: '" + (securityUser != null ? securityUser.accountName : "null") + "' Module is: '" + this.moduleName + "' Entity is: '" + this.entityName + "' Authentication Token is: '" + authenticationToken + "' Session is: '" + session.Id + "'", false); 
                    return false;
                }
            }

            //
            // If we get here, it is OK To Proceed
            //
            return true;

            */
        }



        //
        // Call this in each handler in a deriving Web API class to allow a simple way to throw an error when the user doesn't have the privilege to rear or write to the data based on the entity data token they provide.  Also checks against the forms authentication ticket, (as opposed to session, because the session can change in long user interactions, particulary with periods of inactivity  )
        //
        protected bool IsEntityDataTokenValid(TokenLogic.EntityDataTokenTrustLevel trustLevel, string comments = null, string alternateModule = null)
        {
            //
            // The alternate module parameter helps custom screens belonging to sub modules that reside under a main module.  
            //
            // The main module is whatever module the HomeController declares as it's module.  
            // Any CUSTOM SCREEN in a sub module must report the main module as their alternate module when checking the EDT if using shared web api controller methods with templatized screens
            // and that is why the moduleOverride is here.
            //
            if (Foundation.Configuration.GetBooleanConfigurationSetting(TokenLogic.DISABLE_CROSS_SITE_FORGERY_CONFIGURATION_SETTING_NAME, false) == true)
            {
                return true;
            }

            return true;

            /* disabling the entity data token concept from FoundationCore for now.

            SecurityUser securityUser = GetSecurityUser();


            ISession session = GetSession();

            string authenticationToken = GetAuthenticationTokenFromAuthenticationCookie();

            if (string.IsNullOrEmpty(authenticationToken) == true)
            {
                CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "User attempted to access a resource requiring an authentication token, which could not be found.  User is: " + (securityUser != null ? securityUser.accountName : "null"), false);
                return false;
            }


            string commentsToSaveOnEvent = this.Request.Method.ToString() + " - " + Request.Path.ToString() + Request.QueryString.ToString() + (comments != null ? " - " + comments : "");

            string entityDataToken = GetEntityDataToken();

            if (string.IsNullOrWhiteSpace(entityDataToken) == true)
            {
                CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "User attempted to access a resource requiring a token which was not provided.  User is: " + (securityUser != null ? securityUser.accountName : "null"), false);
                return false;
            }

            //
            // try the entity data token to see if it works.  If it doesn't, there is a scenario when it can still be approved at this level.
            //
            // Cross module requests where an alternate module is provided will retry with the alternate module name.
            //
            if (TokenLogic.ValidateEntityDataToken(trustLevel, entityDataToken, securityUser, this.moduleName, this.entityName, authenticationToken, commentsToSaveOnEvent) == false)
            {
                //
                // First try doesn't work, but is the circumstance such that it is an expected failure and it can be allowed?
                //

                //
                // Do we have an alternate module to try?  If not, fail out.
                //
                // If so, try that module too before failing.  This is for cross module screen calling support
                //
                if (alternateModule != null)
                {
                    //
                    // Try again with the alternate module being the primary module
                    //
                    if (TokenLogic.ValidateEntityDataToken(trustLevel, entityDataToken, securityUser, alternateModule, this.entityName, authenticationToken, commentsToSaveOnEvent) == false)
                    {
                        CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "User attempted to access a resource requiring a token, but an invalid token value of '" + entityDataToken + "' was provided.  Both main and alternate modules were tried.  User is: '" + (securityUser != null ? securityUser.accountName : "null") + "' Module is: '" + this.moduleName + "' Alternate module is: '" + alternateModule + "' Entity is:  '" + this.entityName + "' Authentication Token is: '" + authenticationToken + "' Session is: '" + session.Id + "'", false);
                        return false;
                    }
                }
                else
                {
                    //
                    // no alternate module provided.  Fail out.
                    //
                    CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "User attempted to access a resource requiring a token, but an invalid entity data token value of '" + entityDataToken + "' was provided.  User is: '" + (securityUser != null ? securityUser.accountName : "null") + "' Module is: '" + this.moduleName + "' Entity is: '" + this.entityName + "' Authentication Token is: '" + authenticationToken + "' Session is: '" + session.Id + "'", false);
                    return false;               
                }
            }

            //
            // If we get here, it is OK To Proceed
            //
            return true;
            */
        }


        #region Security Check Methods.  These signout the user if they don't pass.

        protected async Task<bool> DoesUserHaveReadPrivilegeSecurityCheckAsync(int readPrivilegeRequired = 0, CancellationToken cancellationToken = default)
        {
            //
            // Call this in each Get handler in a deriving Web API class to allow a simple way to throw an error when the user doesn't have the privilege to read.
            //
            if (await this.UserHasNoAccessAsync(cancellationToken) == true || await this.UserCanReadAsync(readPrivilegeRequired, cancellationToken) == false)
            {
                SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

                await CreateAuditEventAsync(AuditEngine.AuditType.UnauthorizedAccessAttempt, "User attempted to access a resource requiring reader privilege to which they are not entitled.  User is: " + (securityUser != null ? securityUser.accountName : "null"));
                DestroySessionAndAuthentication();
                return false;
            }

            return true;
        }

        protected bool DoesUserHaveReadPrivilegeSecurityCheck(int readPrivilegeRequired = 0)
        {
            //
            // Call this in each Get handler in a deriving Web API class to allow a simple way to throw an error when the user doesn't have the privilege to read.
            //
            if (this.UserHasNoAccess() == true || this.UserCanRead(readPrivilegeRequired) == false)
            {
                SecurityUser securityUser = GetSecurityUser();

                CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "User attempted to access a resource requiring reader privilege to which they are not entitled.  User is: " + (securityUser != null ? securityUser.accountName : "null"));
                DestroySessionAndAuthentication();
                return false;
            }

            return true;
        }


        protected async Task<bool> DoesUserHaveWritePrivilegeSecurityCheckAsync(int writePrivilegeRequired = 0, CancellationToken cancellationToken = default)
        {
            //
            // Call this in each PUT/POST handler in a deriving Web API class to allow a simple way to throw an error when the user doesn't have the privilege to write.
            //
            if (await this.UserHasNoAccessAsync(cancellationToken) == true || await this.UserCanWriteAsync(writePrivilegeRequired, cancellationToken) == false)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UnauthorizedAccessAttempt, "User attempted to access a resource requiring writer privilege to which they are not entitled", false);

                DestroySessionAndAuthentication();

                return false;
            }

            return true;
        }


        protected bool DoesUserHaveWritePrivilegeSecurityCheck(int writePrivilegeRequired = 0)
        {
            //
            // Call this in each PUT/POST handler in a deriving Web API class to allow a simple way to throw an error when the user doesn't have the privilege to write.
            //
            if (this.UserHasNoAccess() == true || this.UserCanWrite(writePrivilegeRequired) == false)
            {
                CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "User attempted to access a resource requiring writer privilege to which they are not entitled", false);
                DestroySessionAndAuthentication();
                return false;
            }

            return true;
        }


        protected async Task<bool> DoesUserHaveAdminPrivilegeSecurityCheckAsync(CancellationToken cancellationToken = default)
        {
            //
            // Call this in each Delete handler in a deriving Web API class to allow a simple way to throw an error when the user doesn't have the privilege to administer.
            //
            if (await this.UserHasNoAccessAsync(cancellationToken) == true || await this.UserCanAdministerAsync(cancellationToken) == false)
            {
                CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "User attempted to access a resource requiring administrator privilege to which they are not entitled", false);

                DestroySessionAndAuthentication();

                return false;
            }

            return true;
        }


        protected bool DoesUserHaveAdminPrivilegeSecurityCheck()
        {
            //
            // Call this in each Delete handler in a deriving Web API class to allow a simple way to throw an error when the user doesn't have the privilege to administer.
            //
            if (this.UserHasNoAccess() == true || this.UserCanAdminister() == false)
            {
                CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "User attempted to access a resource requiring administrator privilege to which they are not entitled", false);
                DestroySessionAndAuthentication();
                return false;
            }

            return true;
        }



        protected bool DoesUserHaveCustomRoleSecurityCheck(string roleName)
        {
            //
            // Call this in each Get handler in a deriving Web API class to allow a simple way to throw an error when the user doesn't have the privilege to administer.
            //
            if (this.UserHasCustomRole(roleName) == false)
            {
                DestroySessionAndAuthentication();
                return false;
            }

            return true;
        }

        #endregion

        protected bool UserHasCustomRole(SecurityUser securityUser, string roleName)
        {
            return SecurityFramework.UserHasCustomRole(this.moduleName, rolesAndPrivilegesForThisModule, roleName, securityUser);
        }

        protected bool UserHasNoAccess(SecurityUser securityUser)
        {
            return SecurityFramework.UserHasNoAccess(this.moduleName, rolesAndPrivilegesForThisModule, securityUser);
        }

        protected bool UserCanRead(SecurityUser securityUser, int entityMinimumWriteReadSecurityLevel = 0)
        {
            return SecurityFramework.UserCanRead(this.moduleName, rolesAndPrivilegesForThisModule, securityUser, entityMinimumWriteReadSecurityLevel);
        }

        protected bool UserMustReadAnonymously(SecurityUser securityUser)
        {
            return SecurityFramework.UserMustReadAnonymously(this.moduleName, rolesAndPrivilegesForThisModule, securityUser);
        }

        protected async Task<bool> UserCanWriteAsync(SecurityUser securityUser, int entityMinimumWriteWriteSecurityLevel = 0, CancellationToken cancellationToken = default)
        {
            return await SecurityFramework.UserCanWriteAsync(this.moduleName, rolesAndPrivilegesForThisModule, securityUser, entityMinimumWriteWriteSecurityLevel, cancellationToken);
        }

        protected bool UserCanWrite(SecurityUser securityUser, int entityMinimumWriteWriteSecurityLevel = 0)
        {
            return SecurityFramework.UserCanWrite(this.moduleName, rolesAndPrivilegesForThisModule, securityUser, entityMinimumWriteWriteSecurityLevel);
        }

        protected async Task<bool> UserCanAdministerAsync(SecurityUser securityUser, CancellationToken cancellationToken = default)
        {
            return await SecurityFramework.UserCanAdministerAsync(this.moduleName, rolesAndPrivilegesForThisModule, securityUser, cancellationToken);
        }

        protected bool UserCanAdminister(SecurityUser securityUser)
        {
            return SecurityFramework.UserCanAdminister(this.moduleName, rolesAndPrivilegesForThisModule, securityUser);
        }

        protected bool UserHasCustomRole(string roleName)
        {
            return SecurityFramework.UserHasCustomRole(this.moduleName, rolesAndPrivilegesForThisModule, roleName, GetSecurityUser());
        }

        protected async Task<bool> UserHasNoAccessAsync(CancellationToken cancellationToken = default)
        {
            return await SecurityFramework.UserHasNoAccessAsync(this.moduleName, rolesAndPrivilegesForThisModule, await GetSecurityUserAsync(cancellationToken), cancellationToken);
        }

        protected bool UserHasNoAccess()
        {
            return SecurityFramework.UserHasNoAccess(this.moduleName, rolesAndPrivilegesForThisModule, GetSecurityUser());
        }

        protected async Task<bool> UserCanReadAsync(int entityMinimumReadSecurityLevel = 0, CancellationToken cancellationToken = default)
        {
            return await SecurityFramework.UserCanReadAsync(this.moduleName, rolesAndPrivilegesForThisModule, await GetSecurityUserAsync(cancellationToken), entityMinimumReadSecurityLevel, cancellationToken);
        }

        protected bool UserCanRead(int entityMinimumReadSecurityLevel = 0)
        {
            return SecurityFramework.UserCanRead(this.moduleName, rolesAndPrivilegesForThisModule, GetSecurityUser(), entityMinimumReadSecurityLevel);
        }

        protected bool UserMustReadAnonymously()
        {
            return SecurityFramework.UserMustReadAnonymously(this.moduleName, rolesAndPrivilegesForThisModule, GetSecurityUser());
        }

        protected async Task<bool> UserCanWriteAsync(int entityMinimumWriteSecurityLevel = 0, CancellationToken cancellationToken = default)
        {
            return await SecurityFramework.UserCanWriteAsync(this.moduleName, rolesAndPrivilegesForThisModule, await GetSecurityUserAsync(cancellationToken), entityMinimumWriteSecurityLevel, cancellationToken);
        }


        protected bool UserCanWrite(int entityMinimumWriteSecurityLevel = 0)
        {
            return SecurityFramework.UserCanWrite(this.moduleName, rolesAndPrivilegesForThisModule, GetSecurityUser(), entityMinimumWriteSecurityLevel);
        }

        protected async Task<bool> UserCanAdministerAsync(CancellationToken cancellationToken = default)
        {
            return await SecurityFramework.UserCanAdministerAsync(this.moduleName, rolesAndPrivilegesForThisModule, await GetSecurityUserAsync(cancellationToken), cancellationToken);
        }

        protected bool UserCanAdminister()
        {
            return SecurityFramework.UserCanAdminister(this.moduleName, rolesAndPrivilegesForThisModule, GetSecurityUser());
        }


        protected async Task<bool> UserCanAdministerSecurityModuleAsync(CancellationToken cancellationToken = default)
        {
            List<RoleAndPrivilege> rolesAndPrivilegesForSecurityModule = await SecurityFramework.GetRolesAndPrivilegesForModuleAsync(SECURITY_MODULE_NAME, securityDb, cancellationToken);

            return await SecurityFramework.UserCanAdministerAsync(SECURITY_MODULE_NAME, rolesAndPrivilegesForSecurityModule, await GetSecurityUserAsync(cancellationToken), cancellationToken);
        }


        protected bool UserCanAdministerSecurityModule()
        {
            List<RoleAndPrivilege> rolesAndPrivilegesForSecurityModule = SecurityFramework.GetRolesAndPrivilegesForModule(SECURITY_MODULE_NAME, securityDb);

            return SecurityFramework.UserCanAdminister(SECURITY_MODULE_NAME, rolesAndPrivilegesForSecurityModule, GetSecurityUser());
        }



        protected async Task<bool> UserCanAdministerSecurityModuleAsync(SecurityUser securityUser, CancellationToken cancellationToken = default)
        {
            List<RoleAndPrivilege> rolesAndPrivilegesForSecurityModule = await SecurityFramework.GetRolesAndPrivilegesForModuleAsync(SECURITY_MODULE_NAME, securityDb, cancellationToken);

            return await SecurityFramework.UserCanAdministerAsync(SECURITY_MODULE_NAME, rolesAndPrivilegesForSecurityModule, securityUser, cancellationToken);
        }


        protected bool UserCanAdministerSecurityModule(SecurityUser securityUser)
        {
            List<RoleAndPrivilege> rolesAndPrivilegesForSecurityModule = SecurityFramework.GetRolesAndPrivilegesForModule(SECURITY_MODULE_NAME, securityDb);

            return SecurityFramework.UserCanAdminister(SECURITY_MODULE_NAME, rolesAndPrivilegesForSecurityModule, securityUser);
        }


        protected static async Task<Guid> UserTenantGuidAsync(SecurityUser securityUser, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                throw new Exception("No security user provided.");
            }
            else
            {
                return await SecurityFramework.UserTenantGuidAsync(securityUser, cancellationToken);
            }
        }

        protected static Guid UserTenantGuid(SecurityUser securityUser)
        {
            if (securityUser == null)
            {
                throw new Exception("No security user provided.");
            }
            else
            {
                return SecurityFramework.UserTenantGuid(securityUser);
            }
        }

        protected Guid UserTenantGuid()
        {
            return SecurityFramework.UserTenantGuid(GetSecurityUser());
        }

        protected static string UserTenantGuidString(SecurityUser securityUser)
        {
            if (securityUser == null)
            {
                return null;
            }
            else
            {
                return SecurityFramework.UserTenantGuidString(securityUser);
            }
        }

        protected string UserTenantGuidString()
        {
            return SecurityFramework.UserTenantGuidString(GetSecurityUser());
        }



        protected void StartAuditEventClock()
        {
            auditEventStartTime = DateTime.UtcNow;
        }


        protected async Task<bool> CreateAuditEventAsync(AuditEngine.AuditType auditType, string message)
        {
            return await CreateAuditEventAsync(auditType, message, true, null, null, null, null);
        }


        protected void CreateAuditEvent(AuditEngine.AuditType auditType, string message)
        {
            CreateAuditEvent(auditType, message, true, null, null, null, null);
        }



        protected async Task<bool> CreateAuditEventAsync(AuditEngine.AuditType auditType, string message, string primaryKey)
        {
            return await CreateAuditEventAsync(auditType, message, true, primaryKey, null, null, null);
        }

        protected void CreateAuditEvent(AuditEngine.AuditType auditType, string message, string primaryKey)
        {
            CreateAuditEvent(auditType, message, true, primaryKey, null, null, null);
        }


        protected async Task<bool> CreateAuditEventAsync(AuditEngine.AuditType auditType, string message, Boolean success)
        {
            return await CreateAuditEventAsync(auditType, message, success, null, null, null, null);
        }

        protected void CreateAuditEvent(AuditEngine.AuditType auditType, string message, Boolean success)
        {
            CreateAuditEvent(auditType, message, success, null, null, null, null);
        }

        protected async Task<bool> CreateAuditEventAsync(AuditEngine.AuditType auditType, string message, string primaryKey, Exception ex)
        {
            return await CreateAuditEventAsync(auditType, message, false, primaryKey, null, null, ex);
        }

        protected void CreateAuditEvent(AuditEngine.AuditType auditType, string message, string primaryKey, Exception ex)
        {
            CreateAuditEvent(auditType, message, false, primaryKey, null, null, ex);
        }


        protected async Task<bool> CreateAuditEventAsync(AuditEngine.AuditType auditType, string message, Boolean success, string primaryKey, string beforeState, string afterState, Exception ex)
        {
            if (environmentName == null || environmentName == "")
            {
                environmentName = GetStringConfigurationSetting("EnvironmentName");
            }

            if (environmentName == null)
            {
                environmentName = "Unknown";
            }

            string userName = "";


            SecurityUser securityUser = await GetSecurityUserAsync();

            if (securityUser == null)
            {
                userName = "Unknown";
            }
            else
            {
                if (securityUser.accountName != null && securityUser.accountName.Length > 0)
                {
                    userName = securityUser.accountName;
                }
                else
                {
                    userName = "Unknown";
                }
            }

            // Audit flood protection.  Return true to the caller when we have suppressed a message because it doesn't affect them.
            // 
            string floodCacheKey = $"{userName}_{auditType}_{message}_{success}_{primaryKey}_{beforeState}_{afterState}";
            if (_auditFloodCheckerCache.ContainsKey(floodCacheKey) == true)
            {
                return true;
            }
            else
            {
                _auditFloodCheckerCache.Add(floodCacheKey, true);
            }

            List<string> errors = null;

            if (ex != null)
            {
                errors = new List<string>();

                Exception subEx = ex;

                //
                // First put in the entire error written as a string
                //
                errors.Add(ex.ToString());

                while (subEx != null)
                {
                    errors.Add(subEx.Message + " - " + subEx.ToString());
                    subEx = subEx.InnerException;
                }
            }

            DateTime startTime;

            if (auditEventStartTime != DateTime.MinValue)
            {
                startTime = auditEventStartTime;
            }
            else
            {
                startTime = DateTime.UtcNow;
            }

            DateTime stopTime = DateTime.UtcNow;


            string sessionId = null;

            var session = GetSession();

            if (session != null)
            {
                sessionId = session.Id;
            }

            AuditEngine a = Auditor.AuditEngine.Instance;
            await a.CreateAuditEventAsync(startTime,
                stopTime,
                success,
                AuditEngine.AuditAccessType.WebBrowser,
                auditType,
                userName,
                sessionId,
                HttpContext.Request.Headers["UserHostAddress"],
                HttpContext.Request.Headers["UserAgent"],
                moduleName,
                entityName,
                HttpContext.Request.Path.ToString() + HttpContext.Request.QueryString.ToString(),
                environmentName,
                primaryKey,
                System.Threading.Thread.CurrentThread.ManagedThreadId,
                message,
                beforeState,
                afterState,
                errors);

            auditEventStartTime = DateTime.MinValue;

            return true;
        }



        protected void CreateAuditEvent(AuditEngine.AuditType auditType, string message, Boolean success, string primaryKey, string beforeState, string afterState, Exception ex)
        {
            if (environmentName == null || environmentName == "")
            {
                environmentName = GetStringConfigurationSetting("EnvironmentName");
            }

            if (environmentName == null)
            {
                environmentName = "Unknown";
            }

            string userName = "";


            SecurityUser securityUser = GetSecurityUser();

            if (securityUser == null)
            {
                userName = "Unknown";
            }
            else
            {
                if (securityUser.accountName != null && securityUser.accountName.Length > 0)
                {
                    userName = securityUser.accountName;
                }
                else
                {
                    userName = "Unknown";
                }
            }

            // Audit flood protection.  Return true to the caller when we have suppressed a message because it doesn't affect them.
            // 
            string floodCacheKey = $"{userName}_{auditType}_{message}_{success}_{primaryKey}_{beforeState}_{afterState}";
            if (_auditFloodCheckerCache.ContainsKey(floodCacheKey) == true)
            {
                return;
            }
            else
            {
                _auditFloodCheckerCache.Add(floodCacheKey, true);
            }

            List<string> errors = null;

            if (ex != null)
            {
                errors = new List<string>();

                Exception subEx = ex;

                //
                // First put in the entire error written as a string
                //
                errors.Add(ex.ToString());

                while (subEx != null)
                {
                    errors.Add(subEx.Message + " - " + subEx.ToString());
                    subEx = subEx.InnerException;
                }
            }

            DateTime startTime;

            if (auditEventStartTime != DateTime.MinValue)
            {
                startTime = auditEventStartTime;
            }
            else
            {
                startTime = DateTime.UtcNow;
            }

            DateTime stopTime = DateTime.UtcNow;


            string sessionId = null;

            var session = GetSession();

            if (session != null)
            {
                sessionId = session.Id;
            }

            AuditEngine a = Auditor.AuditEngine.Instance;
            a.CreateAuditEvent(startTime,
                stopTime,
                success,
                AuditEngine.AuditAccessType.WebBrowser,
                auditType,
                userName,
                sessionId,
                HttpContext.Request.Headers["UserHostAddress"],
                HttpContext.Request.Headers["UserAgent"],
                moduleName,
                entityName,
                HttpContext.Request.Path.ToString() + HttpContext.Request.QueryString.ToString(),
                environmentName,
                primaryKey,
                System.Threading.Thread.CurrentThread.ManagedThreadId,
                message,
                beforeState,
                afterState,
                errors);

            auditEventStartTime = DateTime.MinValue;
        }


        protected void DeleteDataFromDisk(Guid objectGuid, int versionNumber, string extension)
        {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string folderNameToBecreated = System.IO.Path.Combine(this.moduleName, StringUtility.Pluralize(this.entityName));
            folderNameToBecreated = System.IO.Path.Combine(folderNameToBecreated, objectGuid.ToString());
            string finalDirectoryPath = System.IO.Path.Combine(rootDirectory, "App_Data", folderNameToBecreated);

            string filename = objectGuid.ToString() + "_" + versionNumber.ToString() + "." + extension;

            var fullFilePath = System.IO.Path.Combine(finalDirectoryPath, filename);

            System.IO.File.Delete(fullFilePath);

            return;
        }


        protected async Task<bool> WriteDataToDiskAsync(Guid objectGuid, int versionNumber, byte[] data, string extension, CancellationToken cancellationToken = default)
        {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string folderNameToBecreated = System.IO.Path.Combine(this.moduleName, StringUtility.Pluralize(this.entityName));
            folderNameToBecreated = System.IO.Path.Combine(folderNameToBecreated, objectGuid.ToString());
            string finalDirectoryPath = System.IO.Path.Combine(rootDirectory, "App_Data", folderNameToBecreated);


            string filename = objectGuid.ToString() + "_" + versionNumber.ToString() + "." + extension;

            //Create Directory if need be
            if (System.IO.Directory.Exists(finalDirectoryPath) == false)
            {
                System.IO.Directory.CreateDirectory(finalDirectoryPath);
            }

            string fullFilePath = System.IO.Path.Combine(finalDirectoryPath, filename);

            using (System.IO.FileStream sw = new System.IO.FileStream(fullFilePath, (FileMode)FileAccess.Write))
            {
                await sw.WriteAsync(data, 0, data.Length, cancellationToken);
            }

            return true;
        }


        protected bool WriteDataToDisk(Guid objectGuid, int versionNumber, byte[] data, string extension)
        {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string folderNameToBecreated = System.IO.Path.Combine(this.moduleName, StringUtility.Pluralize(this.entityName));
            folderNameToBecreated = System.IO.Path.Combine(folderNameToBecreated, objectGuid.ToString());
            string finalDirectoryPath = System.IO.Path.Combine(rootDirectory, "App_Data", folderNameToBecreated);


            string filename = objectGuid.ToString() + "_" + versionNumber.ToString() + "." + extension;

            //Create Directory if need tbe
            if (System.IO.Directory.Exists(finalDirectoryPath) == false)
            {
                System.IO.Directory.CreateDirectory(finalDirectoryPath);
            }

            var fullFilePath = System.IO.Path.Combine(finalDirectoryPath, filename);

            using (System.IO.FileStream sw = new System.IO.FileStream(fullFilePath, (FileMode)FileAccess.Write))
            {
                sw.Write(data, 0, data.Length);
            }

            return true;
        }


        protected bool WriteDataToDisk(Guid objectGuid, int versionNumber, Stream dataStream, string extension)
        {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string folderNameToBecreated = System.IO.Path.Combine(this.moduleName, StringUtility.Pluralize(this.entityName));
            folderNameToBecreated = System.IO.Path.Combine(folderNameToBecreated, objectGuid.ToString());
            string finalDirectoryPath = System.IO.Path.Combine(rootDirectory, "App_Data", folderNameToBecreated);


            string filename = objectGuid.ToString() + "_" + versionNumber.ToString() + "." + extension;

            //Create Directory if need be
            if (System.IO.Directory.Exists(finalDirectoryPath) == false)
            {
                System.IO.Directory.CreateDirectory(finalDirectoryPath);
            }

            var fullFilePath = System.IO.Path.Combine(finalDirectoryPath, filename);

            using (System.IO.FileStream sw = new System.IO.FileStream(fullFilePath, (FileMode)FileAccess.Write))
            {
                dataStream.CopyTo(sw);
            }

            return true;
        }


        protected async Task<bool> WriteDataToDiskAsync(Guid objectGuid, int versionNumber, Stream dataStream, string extension, CancellationToken cancellationToken = default)
        {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string folderNameToBecreated = System.IO.Path.Combine(this.moduleName, StringUtility.Pluralize(this.entityName));
            folderNameToBecreated = System.IO.Path.Combine(folderNameToBecreated, objectGuid.ToString());
            string finalDirectoryPath = System.IO.Path.Combine(rootDirectory, "App_Data", folderNameToBecreated);


            string filename = objectGuid.ToString() + "_" + versionNumber.ToString() + "." + extension;

            //Create Directory if need be
            if (System.IO.Directory.Exists(finalDirectoryPath) == false)
            {
                System.IO.Directory.CreateDirectory(finalDirectoryPath);
            }

            var fullFilePath = System.IO.Path.Combine(finalDirectoryPath, filename);

            using (System.IO.FileStream sw = new System.IO.FileStream(fullFilePath, (FileMode)FileAccess.Write))
            {
                await dataStream.CopyToAsync(sw, cancellationToken);
            }

            return true;
        }



        protected async Task<byte[]> LoadDataFromDiskAsync(Guid objectGuid, int versionNumber, string extension, CancellationToken cancellationToken = default)
        {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string folderNameToBecreated = System.IO.Path.Combine(this.moduleName, StringUtility.Pluralize(this.entityName));
            folderNameToBecreated = System.IO.Path.Combine(folderNameToBecreated, objectGuid.ToString());
            string finalDirectoryPath = System.IO.Path.Combine(rootDirectory, "App_Data", folderNameToBecreated);


            string filename = objectGuid.ToString() + "_" + versionNumber.ToString() + "." + extension;

            //Create Directory if need tbe
            if (System.IO.Directory.Exists(finalDirectoryPath) == false)
            {
                System.IO.Directory.CreateDirectory(finalDirectoryPath);
            }

            string fullFilePath = System.IO.Path.Combine(finalDirectoryPath, filename);

            using (System.IO.FileStream fsSource = new System.IO.FileStream(fullFilePath, FileMode.Open, FileAccess.Read))
            {
                // Read the source file into a byte array.
                byte[] bytes = new byte[fsSource.Length];
                int numBytesToRead = (int)fsSource.Length;
                int numBytesRead = 0;
                while (numBytesToRead > 0)
                {
                    // Read may return anything from 0 to numBytesToRead.
                    int n = await fsSource.ReadAsync(bytes, numBytesRead, numBytesToRead, cancellationToken);

                    // Break when the end of the file is reached.
                    if (n == 0)
                    {
                        break;
                    }

                    numBytesRead += n;
                    numBytesToRead -= n;
                }
                numBytesToRead = bytes.Length;

                return bytes;
            }
        }


        protected byte[] LoadDataFromDisk(Guid objectGuid, int versionNumber, string extension)
        {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string folderNameToBecreated = System.IO.Path.Combine(this.moduleName, StringUtility.Pluralize(this.entityName));
            folderNameToBecreated = System.IO.Path.Combine(folderNameToBecreated, objectGuid.ToString());
            string finalDirectoryPath = System.IO.Path.Combine(rootDirectory, "App_Data", folderNameToBecreated);


            string filename = objectGuid.ToString() + "_" + versionNumber.ToString() + "." + extension;

            //Create Directory if need tbe
            if (System.IO.Directory.Exists(finalDirectoryPath) == false)
            {
                System.IO.Directory.CreateDirectory(finalDirectoryPath);
            }

            var fullFilePath = System.IO.Path.Combine(finalDirectoryPath, filename);

            //testing your written file
            using (System.IO.FileStream fsSource = new System.IO.FileStream(fullFilePath, FileMode.Open, FileAccess.Read))
            {
                // Read the source file into a byte array.
                byte[] bytes = new byte[fsSource.Length];
                int numBytesToRead = (int)fsSource.Length;
                int numBytesRead = 0;
                while (numBytesToRead > 0)
                {
                    // Read may return anything from 0 to numBytesToRead.
                    int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);

                    // Break when the end of the file is reached.
                    if (n == 0)
                    {
                        break;
                    }

                    numBytesRead += n;
                    numBytesToRead -= n;
                }
                numBytesToRead = bytes.Length;

                return bytes;
            }
        }

        protected static bool DoesDatabaseStoreDateWithTimeZone(DbContext db)
        {
            //
            // SQL Server DateTime fields do not store the UTC flag on the date, but SQLite does.
            // This affects the way that the resultant DateTime objects are 'kinded', and that varies between reads from SQL Server and SQLite.
            // This means that SQL Server DATETIME fields don't store their original time zone nor the UTC identifier, and come back with a local date kind.
            // SQLite stores UTC dates as ISO date strings, including the Z suffix, so they are read into local time, after converting from the UTC stored time if there is a Z suffix.
            // 
            // We want all date/time fields to serialize out as UTC dates, so we need to adjust them to be that, and this function helps determine what to do for the serialization.
            //

            if (db.Database.GetDbConnection().GetType().ToString().Contains("SQLite") == true)
            {
                //
                // For SQLite, and any other databases that store dates with the time zone (ie. in ISO 8601 string format, like SQLite), the date that comes back is of 'kind' local, and the time is already ajusted from the UTC raw storage format.
                // For these, do a direct conversion back into Universal time so that it serializes out with the UTC time and Z suffix.
                //
                return true;
            }
            else
            {
                //
                // This is for SQL Server, or other databases that don't retain the 'Z' UTC identifier (or any other time zone code) in the date, meaning that the date always come back in a format that is understood by the EF to be a local date time.  In this case, change the date to be of kind 'UTC' because that is what we want it to be.
                //
                return false;
            }
        }
    }
}