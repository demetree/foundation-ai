using Foundation.Security.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Foundation.Security.Services
{
    public class UserValidationResult
    {
        public bool validationSucceeded;
        public string errorMessage;
    }

    public interface IUserService
    {
        Task<UserValidationResult> ValidateUserCredentialsAsync(string userName, string password);

        // Get user by username
        Task<SecurityUser> GetUserByUserNameAsync(string userName);
        Task<IEnumerable<string>> GetUserRolesByUserNameAsync(string userName);
        Task<IEnumerable<Claim>> GetUserClaimsByUserNameAsync(string userName);


        // Get user by object guid
        Task<SecurityUser> GetUserByObjectGuidAsync(Guid userObjectGuid);
        Task<IEnumerable<string>> GetUserRolesByObjectGuidAsync(Guid userObjectGuid);
        Task<IEnumerable<Claim>> GetUserClaimsByObjectGuidAsync(Guid userObjectGuid);
    }

    public class UserService : IUserService
    {
        private readonly SecurityContext _securityContext;

        private const string GENERIC_ERROR_MESSAGE = "Check Username and Password.";
        private const string TENANT_DISABLED_MESSAGE = "Your organization is Unable To login at this time.";

        public UserService(SecurityContext securityContext)
        {
            _securityContext = securityContext;
        }


        /// <summary>
        /// 
        /// This function verifies the user name and password, and returns the security user. 
        /// 
        /// It will not allow inactive or deleted users to sign in, and will record events in the security access logs
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<UserValidationResult> ValidateUserCredentialsAsync(string userName, string password)
        {
            UserValidationResult output = new UserValidationResult();

            //
            // Set default output values to failure.
            //
            output.validationSucceeded = false;
            output.errorMessage = "";

            //
            // Read the user from the database.  Don't filter the query in any way other than the user name.
            //
            SecurityUser user = await (from su in _securityContext.SecurityUsers
                                       where su.accountName == userName
                                       select su)
                                       .Include(su => su.securityTenant)
                                       .FirstOrDefaultAsync(u => u.accountName == userName);


            if (user == null)
            {
                output.validationSucceeded = false;
                output.errorMessage = GENERIC_ERROR_MESSAGE;

                return output;
            }


            //
            // Tenant enabled check
            //
            if (user.securityTenant != null)
            {
                if (user.securityTenant.active == false ||
                    user.securityTenant.deleted == true) 
                {
                    output.validationSucceeded = false;
                    output.errorMessage = TENANT_DISABLED_MESSAGE;

                    return output;
                }
            }

            //
            // Update the most recent activity time on the user, and save right away.
            //
            user.mostRecentActivity = DateTime.UtcNow;


            if (user.active == false)
            {
                SecurityLogic.AddUserEvent(user, SecurityLogic.SecurityUserEventTypes.LoginFailure, "Inactive account attempted login.  Not proceeding.");

                if (user.failedLoginCount.HasValue == false)
                {
                    user.failedLoginCount = 1;
                }
                else
                {
                    user.failedLoginCount++;
                }

                user.lastLoginAttempt = DateTime.UtcNow;

                await _securityContext.SaveChangesAsync();

                output.validationSucceeded = false;
                output.errorMessage = "User is not active.";

                return output;
            }

            if (user.deleted == true)
            {
                SecurityLogic.AddUserEvent(user, SecurityLogic.SecurityUserEventTypes.LoginFailure, "Deleted account attempted login.  Not proceeding.");

                if (user.failedLoginCount.HasValue == false)
                {
                    user.failedLoginCount = 1;
                }
                else
                {
                    user.failedLoginCount++;
                }

                user.lastLoginAttempt = DateTime.UtcNow;

                await _securityContext.SaveChangesAsync();

                output.validationSucceeded = false;
                output.errorMessage = "User is deleted.";

                return output;
            }


            if (user.canLogin == false)
            {
                SecurityLogic.AddUserEvent(user, SecurityLogic.SecurityUserEventTypes.LoginFailure, "User without ability to login attempted login.  Not proceeding.");

                if (user.failedLoginCount.HasValue == false)
                {
                    user.failedLoginCount = 1;
                }
                else
                {
                    user.failedLoginCount++;
                }

                user.lastLoginAttempt = DateTime.UtcNow;

                await _securityContext.SaveChangesAsync();

                output.validationSucceeded = false;
                output.errorMessage = "User is unable to login.";

                return output;
            }


            //
            // When there are existing failed attempts, Wait increasingly longer times before allowing another login attempt.
            //
            int failedLoginAttempts = 0;

            if (user.failedLoginCount.HasValue == true)
            {
                failedLoginAttempts = user.failedLoginCount.Value;
            }
            else
            {
                user.failedLoginCount = 0;
            }

            DateTime utcNow = DateTime.UtcNow;
            DateTime lastLoginAttempt = user.lastLoginAttempt.HasValue == true ? user.lastLoginAttempt.Value : utcNow;


            DateTime nextTimeLoginAttemptAllowed;

            // first 3 attempts can happen right away
            if (failedLoginAttempts <= 2)
            {
                nextTimeLoginAttemptAllowed = utcNow;
            }
            else if (failedLoginAttempts <= 4)
            {
                nextTimeLoginAttemptAllowed = lastLoginAttempt.AddMinutes(1);
            }
            else if (failedLoginAttempts <= 5)
            {
                nextTimeLoginAttemptAllowed = lastLoginAttempt.AddMinutes(5);
            }
            else if (failedLoginAttempts <= 6)
            {
                nextTimeLoginAttemptAllowed = lastLoginAttempt.AddMinutes(10);
            }
            else if (failedLoginAttempts <= 7)
            {
                nextTimeLoginAttemptAllowed = lastLoginAttempt.AddMinutes(60);
            }
            else if (failedLoginAttempts <= 8)
            {
                nextTimeLoginAttemptAllowed = lastLoginAttempt.AddMinutes(60 * 4);
            }
            else if (failedLoginAttempts <= 9)
            {
                nextTimeLoginAttemptAllowed = lastLoginAttempt.AddMinutes(60 * 24);
            }
            else
            {
                //
                // Too many failure attempts.  Disable the user and return an error.
                //
                user.active = false;
                user.lastLoginAttempt = DateTime.UtcNow;

                await _securityContext.SaveChangesAsync();

                SecurityLogic.AddUserEvent(user, SecurityLogic.SecurityUserEventTypes.AccountInactivated, "Account is being inactivated due to too many repeated failed password attempts.");

                output.validationSucceeded = false;
                output.errorMessage = "Unable to continue with login.  Account inactivated.";

                return output;
            }

            //
            // Stop user from even attempting to login if the time they can login hasn't come yet.  Don't update the last login attempt because that's what the cool down timer is based on.
            // and we're not even letting them try here.
            //
            if (utcNow.ToUniversalTime() < nextTimeLoginAttemptAllowed.ToUniversalTime())
            {
                SecurityLogic.AddUserEvent(user, SecurityLogic.SecurityUserEventTypes.LoginAttemptDuringCooldown, "User is attempting login, but this is not being being allowed a login attempt because they are on failure cooldown.  The failure count is " + failedLoginAttempts + " and the next time an attempt will be allowed is " + nextTimeLoginAttemptAllowed.ToString(Foundation.StringUtility.JSON_DATE_FORMAT));

                output.validationSucceeded = false;
                output.errorMessage = "User is unable to attempt logins at this time due to too many repeated failed attempts.  Please try again later.";

                return output;
            }


            //
            // Verify the password
            //
            bool passwordVerified = SecurityLogic.SecurePasswordHasher.Verify(password, user.password);


            //
            // Record the time of the login attempt
            //
            user.lastLoginAttempt = DateTime.UtcNow;

            //
            // If the password is verified, and the user has a failed login count, reset it to 0.
            //
            if (passwordVerified == true)
            {
                SecurityLogic.AddUserEvent(user, SecurityLogic.SecurityUserEventTypes.LoginSuccess, "User authenticated by name and password.");

                user.failedLoginCount = 0;

                output.validationSucceeded = true;
                output.errorMessage = "";
            }
            else
            {
                SecurityLogic.AddUserEvent(user, SecurityLogic.SecurityUserEventTypes.LoginFailure, "User password was not verified.  Failure count is now " + user.failedLoginCount);

                user.failedLoginCount++;

                output.validationSucceeded = false;
                output.errorMessage = GENERIC_ERROR_MESSAGE;
            }

            await _securityContext.SaveChangesAsync();

            return output;
        }


        /// <summary>
        /// 
        /// This returns a Security user by username
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<SecurityUser> GetUserByUserNameAsync(string username)
        {
            bool multiTenancyModeOn = Foundation.Configuration.GetMultiTenancyMode();
            bool dataVisibilityModeOn = Foundation.Configuration.GetDataVisibilityMode();

            //
            // Selectively load the sub entities based on the operating mode
            //
            if (multiTenancyModeOn == true && dataVisibilityModeOn == true)
            {
                return await (from users in _securityContext.SecurityUsers
                              where users.accountName == username &&
                              users.active == true &&
                              users.deleted == false
                              select users)
                                .Include(x => x.securityTenant)
                                .Include(x => x.securityOrganization)
                                .Include(x => x.securityDepartment)
                                .Include(x => x.securityTeam)
                                .AsNoTracking()
                                .FirstOrDefaultAsync();
            }
            else if (multiTenancyModeOn == true)
            {
                return await (from users in _securityContext.SecurityUsers
                              where users.accountName == username &&
                              users.active == true &&
                              users.deleted == false
                              select users)
                                .Include(x => x.securityTenant)
                                .AsNoTracking()
                                .FirstOrDefaultAsync();
            }
            else
            {
                return await (from users in _securityContext.SecurityUsers
                              where users.accountName == username &&
                              users.active == true &&
                              users.deleted == false
                              select users)
                        .Include(x => x.securityTenant)    // Note We always need the navigation property for securityTenant to be loaded, because the auth controller verification expects it if the user has a securityTenantId value.   This will allow for user sharing between Multi Tenanted and non multi tenanted systems.  
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
            }
        }


        /// <summary>
        /// This returns a Security user's roles by username
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetUserRolesByUserNameAsync(string userName)
        {
            //
            // ToDo - add to this roles that the user inherits from group membership.
            //
            return await (from su in _securityContext.SecurityUsers
                          join sur in _securityContext.SecurityUserSecurityRoles on su.id equals sur.securityUserId
                          join sr in _securityContext.SecurityRoles on sur.securityRoleId equals sr.id
                          where
                          su.accountName == userName &&
                          su.active == true &&
                          su.deleted == false &&
                          sur.active == true &&
                          sur.deleted == false &&
                          sr.active == true &&
                          sr.deleted == false
                          select sr.name)
                          .AsNoTracking()
                          .ToListAsync();
        }


        /// <summary>
        /// This returns an enumerable of claims based on the user's roles
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Claim>> GetUserClaimsByUserNameAsync(string username)
        {
            var roles = await GetUserRolesByUserNameAsync(username);
            return roles.Select(r => new Claim(Claims.Role, r));
        }

        /// <summary>
        /// 
        /// This returns a Security user by object guid
        /// 
        /// </summary>
        /// <param name="userObjectGuid"></param>
        /// <returns></returns>
        public async Task<SecurityUser> GetUserByObjectGuidAsync(Guid userObjectGuid)
        {
            bool multiTenancyModeOn = Foundation.Configuration.GetMultiTenancyMode();
            bool dataVisibilityModeOn = Foundation.Configuration.GetDataVisibilityMode();

            //
            // Selectively load the sub entities based on the operating mode
            //
            if (multiTenancyModeOn == true && dataVisibilityModeOn == true)
            {
                return await (from users in _securityContext.SecurityUsers
                              where users.objectGuid == userObjectGuid &&
                              users.active == true &&
                              users.deleted == false
                              select users)
                                .Include(x => x.securityTenant)
                                .Include(x => x.securityOrganization)
                                .Include(x => x.securityDepartment)
                                .Include(x => x.securityTeam)
                                .AsNoTracking()
                                .FirstOrDefaultAsync();
            }
            else if (multiTenancyModeOn == true)
            {
                return await (from users in _securityContext.SecurityUsers
                              where users.objectGuid == userObjectGuid &&
                              users.active == true &&
                              users.deleted == false
                              select users)
                                .Include(x => x.securityTenant)
                                .AsNoTracking()
                                .FirstOrDefaultAsync();
            }
            else
            {
                return await (from users in _securityContext.SecurityUsers
                              where users.objectGuid == userObjectGuid &&
                              users.active == true &&
                              users.deleted == false
                              select users)
                         .Include(x => x.securityTenant)    // Note We always need the navigation property for securityTenant to be loaded, because the auth controller verification expects it if the user has a securityTenantId value.   This will allow for user sharing between Multi Tenanted and non multi tenanted systems.  
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
            }
        }


        /// <summary>
        /// This returns a Security user's roles by object guid
        /// </summary>
        /// <param name="userObjectGuid"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetUserRolesByObjectGuidAsync(Guid userObjectGuid)
        {
            //
            // ToDo - add to this roles that the user inherits from group membership.
            //
            return await (from su in _securityContext.SecurityUsers
                          join sur in _securityContext.SecurityUserSecurityRoles on su.id equals sur.securityUserId
                          join sr in _securityContext.SecurityRoles on sur.securityRoleId equals sr.id
                          where
                          su.objectGuid == userObjectGuid &&
                          su.active == true &&
                          su.deleted == false &&
                          sur.active == true &&
                          sur.deleted == false &&
                          sr.active == true &&
                          sr.deleted == false
                          select sr.name)
                          .AsNoTracking()
                          .ToListAsync();
        }


        /// <summary>
        /// This returns an enumerable of claims based on the user's roles by object guid
        /// </summary>
        /// <param name="userObjectGuid"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Claim>> GetUserClaimsByObjectGuidAsync(Guid userObjectGuid)
        {
            var roles = await GetUserRolesByObjectGuidAsync(userObjectGuid);
            return roles.Select(r => new Claim(Claims.Role, r));
        }
    }
}
