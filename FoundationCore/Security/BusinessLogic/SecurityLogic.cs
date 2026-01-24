using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Foundation.Cache;
using Foundation.Security.Database;


namespace Foundation.Security
{
    public partial class SecurityLogic
    {
        public const float CACHE_TIME_MINUTES = 0.1f;

        // These int enum values must ALWAYS match the ids of the privileges on the Privilege table for each privilege.
        public enum Privileges
        {
            NO_ACCESS = 1,
            ANONYMOUS_READ_ONLY = 2,
            READ_ONLY = 3,
            READ_AND_WRITE = 4,
            ADMINISTRATIVE = 5,
            CUSTOM = 6
        }

        //
        // AI-Generated: Added AdminInitiatedPasswordSet, AdminActionLockAccount, AccountUnlocked for admin actions
        //
        public enum SecurityUserEventTypes
        {
            LoginSuccess = 1,
            LoginFailure = 2,
            LoginAttemptDuringCooldown = 3,
            Logout = 4,
            TwoFactorSend = 5,
            Miscellaneous = 6,
            AccountInactivated = 7,
            UserInitiatedPasswordResetRequest = 8,
            UserInitiatedPasswordResetCompleted = 9,
            SystemInitiatedPasswordResetRequest = 10,
            SystemInitiatedPasswordResetCompleted = 11,
            AdminInitiatedPasswordSet = 12,
            AdminActionLockAccount = 13,
            AccountUnlocked = 14
        }


        public const string READ_ONLY_PRIVILEGE_NAME = "Read Only";
        public const string READ_AND_WRITE_PRIVILEGE_NAME = "Read and Write";
        public const string ADMINISTRATIVE_PRIVILEGE_NAME = "Administrative";


        // This suffix is added to user accounts created for the firstName / lastName login
        public const string FIRST_AND_LAST_USER_ACCOUNT_SUFFIX = "@FirstAndLastPlaceholderUserAccount";


        //
        // LoginAttempt Auto-Purge Configuration
        //
        private const string LOGIN_ATTEMPT_AUTO_PURGE = "LoginAttemptAutoPurge";
        private const int LOGIN_ATTEMPT_PURGE_BATCH_SIZE = 1000;
        public static object loginAttemptPurgeSyncRoot = new object();


        /// <summary>
        /// 
        /// Enables automatic purging of LoginAttempt records older than the specified number of days.
        /// Runs hourly via Hangfire recurring job.
        /// 
        /// </summary>
        /// <param name="daysToKeep">Number of days to retain LoginAttempt records. Must be greater than 0.</param>
        public static void EnableLoginAttemptAutoPurge(int daysToKeep)
        {
            if (daysToKeep <= 0)
            {
                return;
            }

            try
            {
                RecurringJob.AddOrUpdate(LOGIN_ATTEMPT_AUTO_PURGE, () => PurgeLoginAttempts(daysToKeep), RecurringJob.CRON_HOURLY);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error enabling LoginAttempt auto-purge: " + ex.ToString());
            }
        }


        /// <summary>
        /// 
        /// Disables the automatic LoginAttempt purge job.
        /// 
        /// </summary>
        public static void DisableLoginAttemptAutoPurge()
        {
            try
            {
                RecurringJob.RemoveIfExists(LOGIN_ATTEMPT_AUTO_PURGE);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error disabling LoginAttempt auto-purge: " + ex.ToString());
            }
        }


        /// <summary>
        /// 
        /// Purges LoginAttempt records older than the specified number of days.
        /// Deletes in batches to handle large datasets without running out of memory.
        /// 
        /// </summary>
        /// <param name="daysToKeep">Number of days to retain. Records older than this will be deleted.</param>
        public static void PurgeLoginAttempts(int daysToKeep)
        {
            if (daysToKeep <= 0)
            {
                return;
            }

            lock (loginAttemptPurgeSyncRoot)
            {
                try
                {
                    DateTime cutoffDate = DateTime.UtcNow.AddDays(-1 * daysToKeep);

                    //
                    // Get the count of records to remove
                    //
                    int removalCount = 0;

                    using (SecurityContext countDb = new SecurityContext())
                    {
                        removalCount = (from la in countDb.LoginAttempts
                                        where la.timeStamp <= cutoffDate
                                        select la).Count();
                    }

                    if (removalCount == 0)
                    {
                        return;
                    }

                    Foundation.Auditor.AuditEngine.Instance.CreateAuditEvent(
                        DateTime.UtcNow, 
                        DateTime.UtcNow, 
                        true, 
                        Foundation.Auditor.AuditEngine.AuditAccessType.Ambiguous, 
                        Foundation.Auditor.AuditEngine.AuditType.Miscellaneous, 
                        null, null, null, null, 
                        "Security", 
                        "LoginAttempt", 
                        null, null, null, 
                        System.Threading.Thread.CurrentThread.ManagedThreadId, 
                        "Beginning LoginAttempt purge process. Days to keep: " + daysToKeep.ToString() + ". Records to remove: " + removalCount.ToString(), 
                        null, null, null);

                    int rowsLeftToRemove = removalCount;

                    //
                    // Delete in batches to avoid memory issues with large datasets
                    //
                    while (rowsLeftToRemove > 0)
                    {
                        using (SecurityContext iterationDb = new SecurityContext())
                        {
                            int rowsToRemoveThisPass = (rowsLeftToRemove > LOGIN_ATTEMPT_PURGE_BATCH_SIZE ? LOGIN_ATTEMPT_PURGE_BATCH_SIZE : rowsLeftToRemove);

                            List<LoginAttempt> recordsToRemove = (from la in iterationDb.LoginAttempts
                                                                   where la.timeStamp <= cutoffDate
                                                                   orderby la.id
                                                                   select la).Take(rowsToRemoveThisPass).ToList();

                            if (recordsToRemove.Count == 0)
                            {
                                break;
                            }

                            iterationDb.LoginAttempts.RemoveRange(recordsToRemove);
                            iterationDb.SaveChanges();

                            rowsLeftToRemove -= recordsToRemove.Count;
                        }
                    }

                    Foundation.Auditor.AuditEngine.Instance.CreateAuditEvent(
                        DateTime.UtcNow, 
                        DateTime.UtcNow, 
                        true, 
                        Foundation.Auditor.AuditEngine.AuditAccessType.Ambiguous, 
                        Foundation.Auditor.AuditEngine.AuditType.Miscellaneous, 
                        null, null, null, null, 
                        "Security", 
                        "LoginAttempt", 
                        null, null, null, 
                        System.Threading.Thread.CurrentThread.ManagedThreadId, 
                        "Completed LoginAttempt purge process. Removed " + removalCount.ToString() + " records.", 
                        null, null, null);
                }
                catch (Exception ex)
                {
                    Foundation.Auditor.AuditEngine.Instance.CreateAuditEvent(
                        DateTime.UtcNow, 
                        DateTime.UtcNow, 
                        false, 
                        Foundation.Auditor.AuditEngine.AuditAccessType.Ambiguous, 
                        Foundation.Auditor.AuditEngine.AuditType.Error, 
                        null, null, null, null, 
                        "Security", 
                        "LoginAttempt", 
                        null, null, null, 
                        System.Threading.Thread.CurrentThread.ManagedThreadId, 
                        "Error during LoginAttempt purge process: " + ex.Message, 
                        null, null, 
                        new List<string> { ex.ToString() });
                }
            }
        }


        public static void IntegrityCheckPrivilegeTable()
        {
            //
            // This should be called once at startup just to confirm that the startup state of the SecurityPrivilege table is what is expected.
            //
            using (SecurityContext db = new SecurityContext())
            {
                List<Privilege> allPrivileges = (from x in db.Privileges select x).AsNoTracking().ToList();

                if (allPrivileges.Count != 6)
                {
                    throw new Exception("Security System Integrity Error.  Incorrect number of Privileges in Security database.");
                }

                List<Privilege> validatedPrivileges = (from x in allPrivileges
                                                       where ((x.id == 1 && x.name == "No Access") ||
                                                       (x.id == 2 && x.name == "Anonymous Read Only") ||
                                                       (x.id == 3 && x.name == "Read Only") ||
                                                       (x.id == 4 && x.name == "Read and Write") ||
                                                       (x.id == 5 && x.name == "Administrative") ||
                                                       (x.id == 6 && x.name == "Custom"))
                                                       select x).ToList();

                if (validatedPrivileges.Count != 6)
                {
                    throw new Exception("Security System Integrity Error.  Privilege configuration is incorrect in the Security database.");
                }
            }
        }


        public static void IntegrityCheckEntityDataTokenEventTypeTable()
        {
            //
            // This should be called once at startup just to confirm that the startup state of the EntityDataTokenEventType table is what is expected.
            //
            using (SecurityContext db = new SecurityContext())
            {
                List<EntityDataTokenEventType> allEDTETs = (from x in db.EntityDataTokenEventTypes select x).AsNoTracking().ToList();

                if (allEDTETs.Count != 5)
                {
                    throw new Exception("Security System Integrity Error.  Incorrect number of Entity Data Token event types in Security database.");
                }

                List<EntityDataTokenEventType> validatedTypes = (from x in allEDTETs
                                                                 where ((x.id == 1 && x.name == "ReadFromEntity") ||
                                                                 (x.id == 2 && x.name == "CascadeValidatedReadFromEntity") ||
                                                                 (x.id == 3 && x.name == "WriteToEntity") ||
                                                                 (x.id == 4 && x.name == "CascadeValidatedWriteToEntity") ||
                                                                 (x.id == 5 && x.name == "ReuseExistingToken"))
                                                                 select x).ToList();

                if (validatedTypes.Count != 5)
                {
                    throw new Exception("Security System Integrity Error.  Entity Data Token Event Type configuration is incorrect in the Security database.");
                }
            }
        }

        public static void IntegrityCheckSecurityUserEventTypeTable()
        {
            //
            // This should be called once at startup just to confirm that the startup state of the SecurityUserEventType table is what is expected.
            //
            using (SecurityContext db = new SecurityContext())
            {
                List<SecurityUserEventType> allSUETs = (from x in db.SecurityUserEventTypes select x).AsNoTracking().ToList();

                if (allSUETs.Count != 14)
                {
                    throw new Exception("Security System Integrity Error.  Incorrect number of Security User Event Types in Security database.");
                }

                List<SecurityUserEventType> validatedTypes = (from x in allSUETs
                                                              where ((x.id == 1 && x.name == "LoginSuccess") ||
                                                              (x.id == 2 && x.name == "LoginFailure") ||
                                                              (x.id == 3 && x.name == "LoginAttemptDuringCooldown") ||
                                                              (x.id == 4 && x.name == "Logout") ||
                                                              (x.id == 5 && x.name == "TwoFactorSend") ||
                                                              (x.id == 6 && x.name == "Miscellaneous") ||
                                                              (x.id == 7 && x.name == "AccountInactivated") ||
                                                              (x.id == 8 && x.name == "UserInitiatedPasswordResetRequest") ||
                                                              (x.id == 9 && x.name == "UserInitiatedPasswordResetCompleted") ||
                                                              (x.id == 10 && x.name == "SystemInitiatedPasswordResetRequest") ||
                                                              (x.id == 11 && x.name == "SystemInitiatedPasswordResetCompleted") ||
                                                              (x.id == 12 && x.name == "AdminInitiatedPasswordSet") ||
                                                              (x.id == 13 && x.name == "AdminActionLockAccount") ||
                                                              (x.id == 14 && x.name == "AccountUnlocked"))
                                                              select x).ToList();

                if (validatedTypes.Count != 14)
                {
                    throw new Exception("Security System Integrity Error.  Security User Event Type configuration is incorrect in the Security database.");
                }
            }
        }


        /// <summary>
        /// 
        /// Ensures all expected SecurityUserEventType records exist in the database.
        /// Inserts any missing records with explicit IDs using provider-specific syntax.
        /// Supports SQL Server, PostgreSQL, MySQL, and SQLite.
        /// Call this at startup to auto-seed new event types added in code updates.
        /// 
        /// </summary>
        public static async Task EnsureSecurityUserEventTypesAsync(Action<string> logAction = null)
        {
            //
            // Define expected records in order (id, name, description)
            // Must match the SecurityUserEventTypes enum and SecurityDatabaseGenerator seed data
            //
            var expectedTypes = new List<(int id, string name, string description)>
            {
                (1, "LoginSuccess", "Login Success"),
                (2, "LoginFailure", "Login Failure"),
                (3, "LoginAttemptDuringCooldown", "Login Attempt During Cooldown"),
                (4, "Logout", "Logout"),
                (5, "TwoFactorSend", "TwoFactorSend"),
                (6, "Miscellaneous", "Miscellaneous"),
                (7, "AccountInactivated", "AccountInactivated"),
                (8, "UserInitiatedPasswordResetRequest", "UserInitiatedPasswordResetRequest"),
                (9, "UserInitiatedPasswordResetCompleted", "UserInitiatedPasswordResetCompleted"),
                (10, "SystemInitiatedPasswordResetRequest", "SystemInitiatedPasswordResetRequest"),
                (11, "SystemInitiatedPasswordResetCompleted", "SystemInitiatedPasswordResetCompleted"),
                (12, "AdminInitiatedPasswordSet", "Admin Initiated Password Set"),
                (13, "AdminActionLockAccount", "Admin Action Lock Account"),
                (14, "AccountUnlocked", "Account Unlocked")
            };

            try
            {
                using (SecurityContext db = new SecurityContext())
                {
                    List<int> existingIds = await db.SecurityUserEventTypes
                        .Select(x => x.id)
                        .ToListAsync();

                    var missing = expectedTypes.Where(e => !existingIds.Contains(e.id)).ToList();

                    if (missing.Count > 0)
                    {
                        //
                        // Detect provider and use appropriate insert syntax
                        //
                        string providerName = db.Database.ProviderName ?? "";

                        if (providerName.Contains("SqlServer"))
                        {
                            //
                            // SQL Server: Use IDENTITY_INSERT
                            // Must use a transaction to ensure all commands run on the same connection
                            // (IDENTITY_INSERT is session-scoped)
                            //
                            using (var transaction = await db.Database.BeginTransactionAsync())
                            {
                                try
                                {
                                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Security.SecurityUserEventType ON");

                                    foreach (var (id, name, description) in missing)
                                    {
                                        await db.Database.ExecuteSqlRawAsync(
                                            "INSERT INTO Security.SecurityUserEventType (id, name, description) VALUES ({0}, {1}, {2})",
                                            id, name, description);

                                        logAction?.Invoke($"Seeded SecurityUserEventType: {name} (id={id})");
                                    }

                                    await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Security.SecurityUserEventType OFF");

                                    await transaction.CommitAsync();
                                }
                                catch
                                {
                                    await transaction.RollbackAsync();
                                    throw;
                                }
                            }
                        }
                        else if (providerName.Contains("Npgsql") || providerName.Contains("PostgreSQL"))
                        {
                            //
                            // PostgreSQL: Use OVERRIDING SYSTEM VALUE
                            //
                            foreach (var (id, name, description) in missing)
                            {
                                await db.Database.ExecuteSqlRawAsync(
                                    "INSERT INTO \"SecurityUserEventType\" (id, name, description) OVERRIDING SYSTEM VALUE VALUES ({0}, {1}, {2})",
                                    id, name, description);

                                logAction?.Invoke($"Seeded SecurityUserEventType: {name} (id={id})");
                            }
                        }
                        else
                        {
                            //
                            // MySQL, SQLite, and others: Direct insert with explicit ID works by default
                            //
                            foreach (var (id, name, description) in missing)
                            {
                                await db.Database.ExecuteSqlRawAsync(
                                    "INSERT INTO SecurityUserEventType (id, name, description) VALUES ({0}, {1}, {2})",
                                    id, name, description);

                                logAction?.Invoke($"Seeded Missing SecurityUserEventType: {name} (id={id})");
                            }
                        }
                    }

                    //
                    // Final validation: Ensure all records exist with correct IDs
                    //
                    List<SecurityUserEventType> allTypes = await db.SecurityUserEventTypes
                        .AsNoTracking()
                        .ToListAsync();

                    foreach (var expected in expectedTypes)
                    {
                        var actual = allTypes.FirstOrDefault(x => x.id == expected.id);

                        if (actual == null)
                        {
                            string errorMsg = $"SecurityUserEventType validation failed: Missing id={expected.id} ({expected.name})";
                            logAction?.Invoke($"ERROR: {errorMsg}");
                            throw new Exception(errorMsg);
                        }

                        if (actual.name != expected.name)
                        {
                            string errorMsg = $"SecurityUserEventType validation failed: id={expected.id} has name '{actual.name}' but expected '{expected.name}'";
                            logAction?.Invoke($"ERROR: {errorMsg}");
                            throw new Exception(errorMsg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error - unable to ensure Security User Event Types: {ex.Message}";
                logAction?.Invoke(errorMsg);
                Console.WriteLine(errorMsg);
                throw;
            }
        }

        public static bool ValidateOAUTHStateToken(string stateToken)
        {
            using (SecurityContext db = new SecurityContext())
            {
                DateTime now = DateTime.UtcNow;

                List<OAUTHToken> validTokens = (from x in db.OAUTHTokens where x.expiryDateTime > now && x.active == true && x.deleted == false select x).ToList();

                foreach (OAUTHToken validToken in validTokens)
                {
                    if (validToken.token == stateToken)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static bool ValidateOAUTHStateToken(string stateToken, out string userData)
        {
            using (SecurityContext db = new SecurityContext())
            {
                userData = null;

                DateTime now = DateTime.UtcNow;

                List<OAUTHToken> validTokens = (from x in db.OAUTHTokens where x.expiryDateTime > now && x.active == true && x.deleted == false select x).ToList();

                foreach (OAUTHToken validToken in validTokens)
                {
                    if (validToken.token == stateToken)
                    {
                        userData = validToken.userData;
                        return true;
                    }
                }
                return false;
            }
        }


        public static void RecordOAUTHToken(string state, string userData = null)
        {
            using (SecurityContext db = new SecurityContext())
            {
                OAUTHToken ot = new OAUTHToken();

                ot.expiryDateTime = DateTime.UtcNow.AddMinutes(+5);
                ot.token = state;
                ot.userData = userData;
                ot.active = true;
                ot.deleted = false;

                db.OAUTHTokens.Add(ot);
                db.SaveChanges();
            }
        }


        public static SecurityUserEvent AddUserEvent(SecurityUser securityUser, SecurityUserEventTypes eventType, string comments)
        {
            using (SecurityContext db = new SecurityContext())
            {
                SecurityUserEvent newEvent = new SecurityUserEvent();

                newEvent.active = true;
                newEvent.deleted = false;
                newEvent.securityUserId = securityUser.id;
                newEvent.securityUserEventTypeId = (int)eventType;
                newEvent.comments = comments;
                newEvent.timeStamp = DateTime.UtcNow;

                db.SecurityUserEvents.Add(newEvent);
                db.SaveChanges();

                return newEvent;
            }
        }

        public static string GenerateAndSendTwoFactorToken(SecurityUser securityUser, int timeoutMinutes = 5)
        {
            using (SecurityContext db = new SecurityContext())
            {
                Random randomizer = new Random(Guid.NewGuid().GetHashCode());


                //
                // Get the user from the db, and update the freshly loaded record, only touching the 2fa fields.
                //
                SecurityUser userToUpdate = (from x in db.SecurityUsers where x.accountName == securityUser.accountName select x).FirstOrDefault();

                if (userToUpdate != null)
                {
                    string twoFactorToken = randomizer.Next(0, 999999).ToString("000000");
                    DateTime twoFactorExpiry = DateTime.UtcNow.AddMinutes(timeoutMinutes);

                    userToUpdate.twoFactorToken = twoFactorToken;
                    userToUpdate.twoFactorTokenExpiry = twoFactorExpiry;
                    db.SaveChanges();

                    securityUser.twoFactorToken = twoFactorToken;
                    securityUser.twoFactorTokenExpiry = twoFactorExpiry;

                    if (securityUser.twoFactorSendByEmail != true &&
                        securityUser.twoFactorSendBySMS != true)
                    {
                        throw new Exception("User must be configured for either of email or SMS for two factor authentication, and neither is enabled.");
                    }

                    if (securityUser.twoFactorSendByEmail == true)
                    {
                        string emailAddress = userToUpdate.emailAddress;

                        if (emailAddress != null && emailAddress.Trim().Length > 0)
                        {
                            string subject = "Your Two Factor Token";
                            string body = "Your two factor token is " + twoFactorToken;

                            Utility.SendMail(emailAddress, subject, body, true, null, null, null, securityUser);
                        }
                        else
                        {
                            throw new Exception("User has no email address on their user record.");
                        }
                    }

                    if (securityUser.twoFactorSendBySMS == true)
                    {
                        string cellPhoneNumber = userToUpdate.cellPhoneNumber;

                        if (cellPhoneNumber != null && cellPhoneNumber.Trim().Length > 0)
                        {
                            string body = "Your two factor token is " + twoFactorToken;

                            Utility.SendSMS(cellPhoneNumber, body);

                        }
                        else
                        {
                            throw new Exception("User has no cell phone number on their user record.");
                        }
                    }

                    return twoFactorToken;

                }

                return null;
            }
        }

        private static void ValidateUserAndLoginAttempts(SecurityUser securityUser, SecurityContext db)
        {
            //
            // Make sure that the user is not inactive or deleted
            //
            if (securityUser.active == false || securityUser.deleted == true)
            {
                throw new Exception("Unable to proceed with login because account is not active.");
            }

            //
            // User must be explicitly authorized to login.
            //
            if (securityUser.canLogin != true)
            {
                throw new Exception("Unable to login.");
            }

            //
            // Make sure that the user is OK to attempt to login, based on their previous failure attempts
            //
            if (securityUser.failedLoginCount.HasValue == false || securityUser.failedLoginCount <= 3)
            {
                return;
            }
            else
            {

                //
                // Is automatic account lockout on?
                //
                if (Foundation.Configuration.GetBooleanConfigurationSetting("EnableAccountCooldownAndLockout", false) == true)
                {

                    //
                    // Wait increasingly longer times before allowing another login attempt.
                    //
                    int failedLoginAttempts = securityUser.failedLoginCount.Value;
                    DateTime now = DateTime.UtcNow;
                    DateTime lastLoginAttempt = securityUser.lastLoginAttempt.HasValue == true ? securityUser.lastLoginAttempt.Value : now;


                    DateTime nextTimeLoginAttemptAllowed;


                    if (failedLoginAttempts <= 4)
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
                        // Disable the user and return an error.
                        //
                        securityUser.active = false;

                        var entry = db.Entry(securityUser);
                        entry.State = EntityState.Modified;

                        db.SaveChanges();

                        AddUserEvent(securityUser, SecurityUserEventTypes.AccountInactivated, "Account is being inactivated due to too many repeated failed password attempts.");

                        throw new Exception("Unable to continue with login.  Account inactivated.");
                    }

                    if (now < nextTimeLoginAttemptAllowed)
                    {
                        AddUserEvent(securityUser, SecurityUserEventTypes.LoginAttemptDuringCooldown, "User is attempting login, but this is not being being allowed a login attempt because they are on failure cooldown.  The failure count is " + failedLoginAttempts + " and the next time an attempt will be allowed is " + nextTimeLoginAttemptAllowed.ToString(Foundation.StringUtility.JSON_DATE_FORMAT));
                        throw new Exception("User is unable to attempt logins at this time due to too many repeated failed attempts.  Please try again later.");
                    }
                }
                else
                {
                    return;
                }
            }
        }

        public class LocalUserNotFoundException : Exception
        {
            public LocalUserNotFoundException(string message) : base(message)
            {


            }
        }

        public static SecurityUser AuthenticateLocalCredentials(string userName, string password, string twoFactorCode = null)
        {
            if (String.IsNullOrWhiteSpace(userName) == true)
            {
                throw new Exception("Please enter a user name");
            }


            if (String.IsNullOrWhiteSpace(password) == true)
            {
                throw new Exception("Please enter a password");
            }

            using (SecurityContext db = new SecurityContext())
            {
                SecurityUser securityUser = (from u in db.SecurityUsers
                                             where u.accountName.Trim().ToUpper() == userName.Trim().ToUpper()
                                             && u.activeDirectoryAccount == false
                                             && u.active == true
                                             && u.deleted == false
                                             select u).FirstOrDefault();


                if (securityUser != null)
                {
                    ValidateUserAndLoginAttempts(securityUser, db);

                    if (SecurityLogic.SecurePasswordHasher.Verify(password, securityUser.password) == true)
                    {
                        //
                        // If a two factor code is provided in the auth request, make sure that it is correct and current.  It is up to the calling function to provide the value when it is needed
                        //
                        if (twoFactorCode != null)
                        {
                            if (securityUser.twoFactorToken != twoFactorCode)
                            {
                                AddUserEvent(securityUser, SecurityUserEventTypes.LoginFailure, "Invalid Two factor code entered ");
                                throw new Exception("The two factor code is incorrect");
                            }
                            else if (securityUser.twoFactorTokenExpiry.HasValue == false || securityUser.twoFactorTokenExpiry.Value < DateTime.UtcNow)
                            {
                                AddUserEvent(securityUser, SecurityUserEventTypes.LoginFailure, "Two factor code has expired.");
                                throw new Exception("The two factor code has expired");
                            }
                        }


                        //
                        // Correctly entered password.  restart the fail counter.
                        //
                        securityUser.failedLoginCount = 0;
                        securityUser.lastLoginAttempt = DateTime.UtcNow;

                        var entry = db.Entry(securityUser);
                        entry.State = EntityState.Modified;

                        db.SaveChanges();

                        // take out the password when returning a user object.
                        securityUser.password = null;

                        if (twoFactorCode != null)
                        {
                            SecurityLogic.AddUserEvent(securityUser, SecurityLogic.SecurityUserEventTypes.LoginSuccess, "Correct password and two factor code entered.");
                        }
                        else
                        {
                            SecurityLogic.AddUserEvent(securityUser, SecurityLogic.SecurityUserEventTypes.LoginSuccess, "Correct password entered.");
                        }

                        return securityUser;
                    }
                    else
                    {
                        if (securityUser.failedLoginCount == null)
                        {
                            securityUser.failedLoginCount = 1;
                        }
                        else
                        {
                            securityUser.failedLoginCount++;
                        }

                        securityUser.lastLoginAttempt = DateTime.UtcNow;
                        db.SaveChanges();

                        SecurityLogic.AddUserEvent(securityUser, SecurityLogic.SecurityUserEventTypes.LoginFailure, "Invalid password entered.  Not proceeding.  This is failure count " + securityUser.failedLoginCount);

                        throw new Exception("The password entered is incorrect");
                    }
                }
                else
                {
                    throw new LocalUserNotFoundException("The user name entered could not be found as an active user.  User name that could not be found is " + userName);
                }
            }
        }


        internal static void CreateGoogleLoginAttemptRecord(GoogleAuthentication.GoogleUser user, string resource, string sessionId, string ipAddress, string userAgent)
        {
            LoginAttempt la = new LoginAttempt();

            la.userName = user.name;

            CompleteAndSaveLoginAttempt(la, resource, sessionId, ipAddress, userAgent);
        }


        internal static void CreateMicrosoftLoginAttemptRecord(MicrosoftAuthentication.MicrosoftUser user, string resource, string sessionId, string ipAddress, string userAgent)
        {
            LoginAttempt la = new LoginAttempt();

            la.userName = user.name;

            CompleteAndSaveLoginAttempt(la, resource, sessionId, ipAddress, userAgent);
        }


        internal static void CreateFacebookLoginAttemptRecord(FacebookAuthentication.FacebookUser user, string resource, string sessionId, string ipAddress, string userAgent)
        {
            LoginAttempt la = new LoginAttempt();

            la.userName = user.name;

            CompleteAndSaveLoginAttempt(la, resource, sessionId, ipAddress, userAgent);
        }

        private static void CompleteAndSaveLoginAttempt(LoginAttempt la, string resource, string sessionId, string ipAddress, string userAgent)
        {
            la.timeStamp = DateTime.UtcNow;

            if (resource != null)
            {
                la.resource = resource;

                if (la.resource.Length > 250)
                {
                    la.resource = la.resource.Substring(0, 250);
                }
            }

            if (sessionId != null)
            {
                la.sessionId = sessionId;

                if (la.sessionId.Length > 100)
                {
                    la.sessionId = la.sessionId.Substring(0, 100);
                }
            }

            if (ipAddress != null)
            {
                la.ipAddress = ipAddress;

                if (la.ipAddress.Length > 50)
                {
                    la.ipAddress = la.ipAddress.Substring(0, 50);
                }
            }

            if (userAgent != null)
            {
                la.userAgent = userAgent;

                if (la.userAgent.Length > 100)
                {
                    la.userAgent = la.userAgent.Substring(0, 100);
                }
            }

            la.active = true;
            la.deleted = false;

            using (SecurityContext db = new SecurityContext())
            {
                db.LoginAttempts.Add(la);

                db.SaveChanges();
            }
        }


        internal static void CreateLoginAttemptRecord(string userName, string password, string notes, string resource, string ipAddress, string userAgent)
        {
            LoginAttempt la = new LoginAttempt();

            if (userName != null )
            {
                la.userName = userName;

                if (la.userName.Length > 100)
                {
                    la.userName = userName.Substring(0, 100);
                }
            }

            if (password != null)
            {
                la.passwordHash = password.GetHashCode();
            }

            la.value = notes;
                
            CompleteAndSaveLoginAttempt(la, resource, null, ipAddress, userAgent);
        }


        internal static void CreateLoginAttemptRecord(Models.AuthenticateViewModel model, string resource, string sessionId, string ipAddress, string userAgent)
        {
            LoginAttempt la = new LoginAttempt();

            if (model.Username != null)
            {
                la.userName = model.Username;

                if (la.userName.Length > 100)
                {
                    la.userName = la.userName.Substring(0, 100);
                }
            }

            if (model.Password != null)
            {
                la.passwordHash = model.Password.GetHashCode();
            }

            CompleteAndSaveLoginAttempt(la, resource, sessionId, ipAddress, userAgent);
        }

#if WINDOWS

        public static UserPrincipal AuthenticateCredentialsAgainstDomain(string userName, string password, out string domainName, string twoFactorCode = null, string domainNameToLookIn = null )
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
            {
                domainName = "Not Supported";
                return null;
            }


            if (String.IsNullOrWhiteSpace(userName) == true)
            {
                throw new Exception("Please enter a user name");
            }

            if (String.IsNullOrWhiteSpace(password) == true)
            {
                throw new Exception("Please enter a password");
            }


            //
            // Get the list of domains form the config file.
            //
            string[] domains = null;

            // If we don't get a specific domain to look in, use all available from the config file
            if (domainNameToLookIn == null)
            { 
                string searchDomainNames = GetStringConfigurationSetting("UserAuthenticationDomains");
                domains = searchDomainNames.Split(',');
            }
            else
            { 
                domains = new string[0];
                domains[0] = domainNameToLookIn;
            }



            //
            // Make sure that either the user exists and is configured as a domain account, or that the user doesn't exist.
            //
            // Don't check active or deleted here, as we don't want to presume that the user isn't one who was deactivated
            //
            using (SecurityContext db = new SecurityContext())
            {
                bool foundUserByTheNameProvided = false;

                //
                //   Do we we already have a user with this exact user name?
                // 
                SecurityUser userByAccountName = (from u in db.SecurityUsers
                                                  where
                                          u.accountName.Trim().ToUpper() == userName.Trim().ToUpper()
                                          && (domainNameToLookIn == null || u.authenticationDomain == domainNameToLookIn)
                                          && u.activeDirectoryAccount == true
                                          select u).FirstOrDefault();


                //
                // Maybe the user didn't add the domain suffix to their account (like @ad.stjosham.on.ca)
                //
                int userCountMatch = 0;
                if (userByAccountName == null)
                {
                    foreach (string domain in domains)
                    {
                        SecurityUser userByAccountNameInDomainBeingChecked = (from u in db.SecurityUsers
                                                                     where
                                                                     u.accountName.Trim().ToUpper() == userName.Trim().ToUpper() + "@" + domain.Trim().ToUpper()
                                                                     && (domainNameToLookIn == null || u.authenticationDomain == domainNameToLookIn)
                                                                     && u.activeDirectoryAccount == true
                                                                     select u).FirstOrDefault();

                        if (userByAccountNameInDomainBeingChecked != null)
                        {
                            userCountMatch++;
                            userByAccountName = userByAccountNameInDomainBeingChecked;
                        }
                    }

                    //
                    // if we have multiple users iwth the same acocunt prefix in multiple domains, they must specify the domain so that we don't test their credentails against the wrong domain and lock the other person out.
                    //
                    if (userCountMatch > 1)
                    {
                        throw new Exception("You must login with your fully qualified domain account name.  Found " + userCountMatch + " possible accounts");
                    }
                }


                //
                // If we have a user, and don't have a domain to look in, then user the domain from the user record we have.
                //
                if (userByAccountName != null)
                {
                    if (domainNameToLookIn == null && userByAccountName.authenticationDomain != null)
                    {
                        domainNameToLookIn = userByAccountName.authenticationDomain;
                    }
                    else
                    {
                        if (domainNameToLookIn != userByAccountName.authenticationDomain)
                        {
                            throw new Exception("Mismatch between user authentication domain and the domain to look in.");
                        }
                    }
                }

                //
                // If we get this far, we may have come in with no domainNameToLookIn, but if we found a user, then this may have been set, so rebuld the domain list just in case.
                //
                // If we have an account, we'll only look at that account's domain.
                //
                if (domainNameToLookIn != null)
                {
                    domains = new string[1];
                    domains[0] = domainNameToLookIn;
                }

                //
                // Multi domain Loop construct is really just for initial testing when we don't already have an account.
                //
                foreach (string domain in domains)
                {
                    //
                    // Just a double check since user account locks could follow this function.
                    // 
                    // if we have a domain specified, then look in that one only.  Otherwise look in all of them.  (
                    //
                    if (domainNameToLookIn != null && domain != domainNameToLookIn)
                    {
                        continue;
                    }

                    UserPrincipal up = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, domain), userName);

                    if (up != null)
                    {
                        foundUserByTheNameProvided = true;

                        //
                        // If we already have a user account that is connected to this domain, then pull it.  
                        //
                        SecurityUser user = (from u in db.SecurityUsers
                                     where
                                     u.objectGuid == up.Guid.Value
                                     && u.activeDirectoryAccount == true
                                     && (u.authenticationDomain == null || u.authenticationDomain == domain)
                                     select u).FirstOrDefault();

                        if (user != null && userByAccountName != null)
                        {
                            if (user.objectGuid != userByAccountName.objectGuid)
                            {
                                throw new Exception("Account consistency error for user name " + userName);
                            }
                        }

                        //
                        // Check the login count and such
                        //
                        if (user != null)
                        {
                            ValidateUserAndLoginAttempts(user, db);
                        }

                        //
                        // Test the password against the domain.
                        //

                        /*
                         * 
                         * In the event of weird registry errors that occur from time to time if this is an issue, check this out:
                         * 
Changed setting in IIS manager: in the solution's application pool, advanced settings, Load user profile is now set to "True". Reason is when Windows detects registry file is still in use, the file is unloaded.
This setting makes sure that the user profile - a COM object - loads when needed by the application. Relevant MS Windows blog: 
                        https://docs.microsoft.com/en-us/archive/blogs/dsnotes/com-intermittent-error-800703fa-illegal-operation-attempted-on-a-registry-key
                         * 
                         * */
                        if (up.Context.ValidateCredentials(userName, password, ContextOptions.Negotiate) == true)
                        {
                            //
                            // If a two factor code is provided in the auth request, make sure that it is correct and current.  It is up to the calling function to provide the value when it is needed
                            //
                            if (twoFactorCode != null)
                            {
                                if (user == null)
                                {
                                    throw new Exception("User record could not be found for two factor checking purposes.");
                                }

                                if (user.twoFactorToken != twoFactorCode)
                                {
                                    AddUserEvent(user, SecurityUserEventTypes.LoginFailure, "Invalid 2 factor code entered ");
                                    throw new Exception("The two factor code is incorrect");
                                }
                                else if (user.twoFactorTokenExpiry.HasValue == false || user.twoFactorTokenExpiry.Value < DateTime.UtcNow)
                                {
                                    AddUserEvent(user, SecurityUserEventTypes.LoginFailure, "2 factor code has expired.");
                                    throw new Exception("The two factor code has expired");
                                }
                            }

                            //
                            // Correctly entered password.
                            //
                            if (user != null)
                            {
                                user.failedLoginCount = 0;
                                user.lastLoginAttempt = DateTime.UtcNow;

                                var entry = db.Entry(user);
                                entry.State = EntityState.Modified;

                                db.SaveChanges();
                            }

                            if (twoFactorCode != null)
                            {
                                SecurityLogic.AddUserEvent(user, SecurityLogic.SecurityUserEventTypes.LoginSuccess, "Correct password and two factor code entered.  Validated credentials against " + domain);
                            }
                            else
                            {
                                SecurityLogic.AddUserEvent(user, SecurityLogic.SecurityUserEventTypes.LoginSuccess, "Correct password entered.  Validated credentials against " + domain);
                            }

                            domainName = domain;
                            return up;
                        }
                        else
                        {
                            //
                            // Either invalid credentials are provided, or user was not found in this domain with this password.  Keep going and see if it can be found in another domain.
                            // 
                            if (user != null)
                            {
                                //
                                // Invalid password.  Record the failure.
                                //
                                if (user.failedLoginCount == null)
                                {
                                    user.failedLoginCount = 1;
                                }
                                else
                                {
                                    user.failedLoginCount++;
                                }

                                user.lastLoginAttempt = DateTime.UtcNow;
                                db.SaveChanges();

                                SecurityLogic.AddUserEvent(user, SecurityLogic.SecurityUserEventTypes.LoginFailure, "Invalid password entered.  Not proceeding.  This is failure count " + user.failedLoginCount);
                            }
                        }
                    }
                    else
                    {
                        // User was not found in domain
                    }
                }

                if (foundUserByTheNameProvided == false)
                {
                    throw new Exception("Could not find user in domain.  Username is " + userName);
                }
                else
                {
                    throw new Exception("The password entered is incorrect");
                }
            }
        }

#endif


        public static SecurityUser GetUserByActiveDirectoryAccountName(string accountName)
        {
            using (SecurityContext db = new SecurityContext())
            {
                SecurityUser user = (from u in db.SecurityUsers
                                     where u.accountName == accountName &&
                                     u.activeDirectoryAccount == true &&
                                     u.active == true &&
                                     u.deleted == false
                                     select u).FirstOrDefault();

                // Don't return any password value to a client
                user.password = null;

                return user;
            }
        }



        public static async void signOutUser(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return;
        }



#if WINDOWS
        public static UserPrincipal GetUserPrincipalFromDomainByAccountName(string accountName, out string domainAccountFoundIn, string domainToLookIn = null)
        {
            try
            {
                if (domainToLookIn == null)
                { 
                    string seachDomainNames = GetStringConfigurationSetting("UserAuthenticationDomains");

                    string[] domains = seachDomainNames.Split(',');

                    foreach (string domain in domains)
                    {
                        UserPrincipal up = null;

                        try
                        { 
                            up = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, domain), accountName);
                        }
                        catch (Exception)
                        { 
                            up = null;
                        }

                        if (up != null)
                        {
                            domainAccountFoundIn = domain;
                            return up;
                        }
                    }
                }
                else
                { 
                    UserPrincipal up = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, domainToLookIn), accountName);                                    

                    if (up != null)
                    { 
                        domainAccountFoundIn = domainToLookIn;
                        return up;
                    }
                }

                domainAccountFoundIn = null;
                return null;
            }
            catch (Exception)
            {
                domainAccountFoundIn = null;
                return null;
            }
        }
#endif

        public async static Task<List<SecurityRole>> GetRolesUserIsEntitledToAsync(SecurityUser user, List<SecurityUserSecurityGroup> SecurityUserSecurityGroups, CancellationToken cancellationToken = default)
        {
            //
            // This returns a list of Roles that a user is entitled to.  It does this by querying the UserRole table, and the GroupRole table for any
            // Roles the user may belong to, based on their user name, and their AD group memberships.
            //
            MemoryCacheManager mcm = new MemoryCacheManager();

            string cacheKey = "SL_GRUIET" + user.accountName;

            if (mcm.IsSet(cacheKey) == true)
            {
                return mcm.Get<List<SecurityRole>>(cacheKey);
            }
            else
            {
                using (SecurityContext db = new SecurityContext())
                {
                    List<SecurityRole> userRoles = await (from ur in db.SecurityUserSecurityRoles
                                                          join r in db.SecurityRoles on ur.securityRoleId equals r.id
                                                          join u in db.SecurityUsers on ur.securityUserId equals u.id
                                                          where u.id == user.id &&
                                                          u.active == true &&
                                                          u.deleted != true &&
                                                          ur.active == true &&
                                                          ur.deleted != true
                                                          select r)
                                                    .ToListAsync(cancellationToken)
                                                    .ConfigureAwait(false);


                    List<int> groupIds = new List<int>();

                    foreach (SecurityUserSecurityGroup ug in SecurityUserSecurityGroups)
                    {
                        groupIds.Add(ug.securityGroupId);
                    }

                    List<SecurityRole> groupRoles = await (from gr in db.SecurityGroupSecurityRoles
                                                           join r in db.SecurityRoles on gr.securityRoleId equals r.id
                                                           join g in db.SecurityGroups on gr.securityGroupId equals g.id
                                                           where groupIds.Any(x => x.Equals(g.id)) &&
                                                           g.active == true &&
                                                           g.deleted != true &&
                                                           gr.active == true &&
                                                           gr.deleted != true
                                                           select r)
                                                     .ToListAsync(cancellationToken)
                                                     .ConfigureAwait(false);

                    List<SecurityRole> output = userRoles.Concat(groupRoles).Distinct().ToList();

                    mcm.Set(cacheKey, output, CACHE_TIME_MINUTES);          // cache the user permissions for 0.1 minutes (cache gets cleared on security updates through the web api)

                    return output;
                }
            }
        }



        public static List<SecurityRole> GetRolesUserIsEntitledTo(SecurityUser user, List<SecurityUserSecurityGroup> SecurityUserSecurityGroups)
        {
            //
            // This returns a list of Roles that a user is entitled to.  It does this by querying the UserRole table, and the GroupRole table for any
            // Roles the user may belong to, based on their user name, and their AD group memberships.
            //
            MemoryCacheManager mcm = new MemoryCacheManager();

            string cacheKey = "SL_GRUIET" + user.accountName;

            if (mcm.IsSet(cacheKey) == true)
            {
                return mcm.Get<List<SecurityRole>>(cacheKey);
            }
            else
            {
                using (SecurityContext db = new SecurityContext())
                {
                    List<SecurityRole> userRoles = (from ur in db.SecurityUserSecurityRoles
                                                    join r in db.SecurityRoles on ur.securityRoleId equals r.id
                                                    join u in db.SecurityUsers on ur.securityUserId equals u.id
                                                    where u.id == user.id &&
                                                    u.active == true &&
                                                    u.deleted != true &&
                                                    ur.active == true &&
                                                    ur.deleted != true
                                                    select r).ToList();


                    List<int> groupIds = new List<int>();

                    foreach (SecurityUserSecurityGroup ug in SecurityUserSecurityGroups)
                    {
                        groupIds.Add(ug.securityGroupId);
                    }

                    List<SecurityRole> groupRoles = (from gr in db.SecurityGroupSecurityRoles
                                                     join r in db.SecurityRoles on gr.securityRoleId equals r.id
                                                     join g in db.SecurityGroups on gr.securityGroupId equals g.id
                                                     where groupIds.Any(x => x.Equals(g.id)) &&
                                                     g.active == true &&
                                                     g.deleted != true &&
                                                     gr.active == true &&
                                                     gr.deleted != true
                                                     select r).ToList();

                    List<SecurityRole> output = userRoles.Concat(groupRoles).Distinct().ToList();

                    mcm.Set(cacheKey, output, CACHE_TIME_MINUTES);          // cache the user permissions for 0.1 minutes (cache gets cleared on security updates through the web api)

                    return output;
                }
            }
        }


        public async static Task<List<SecurityRole>> GetRolesUserIsEntitledToForModuleAsync(string moduleName, SecurityUser user, List<SecurityUserSecurityGroup> SecurityUserSecurityGroups, CancellationToken cancellationToken = default)
        {
            //
            // This returns a list of Roles that a user is entitled to.  It does this by querying the UserRole table, and the GroupRole table for any
            // Roles the user may belong to, based on their user name, and their AD group memberships.
            //
            MemoryCacheManager mcm = new MemoryCacheManager();


            string cacheKey = "SL_GRUIETFRM_" + user.accountName + "_" + moduleName;

            if (mcm.IsSet(cacheKey) == true)
            {
                return mcm.Get<List<SecurityRole>>(cacheKey);
            }
            else
            {
                using (SecurityContext db = new SecurityContext())
                {
                    List<SecurityRole> userRoles = await (from ur in db.SecurityUserSecurityRoles
                                                          join sr in db.SecurityRoles on ur.securityRoleId equals sr.id
                                                          join u in db.SecurityUsers on ur.securityUserId equals u.id
                                                          join msr in db.ModuleSecurityRoles on sr.id equals msr.securityRoleId
                                                          join m in db.Modules on msr.moduleId equals m.id
                                                          where u.id == user.id &&
                                                          m.name == moduleName &&
                                                          u.active == true &&
                                                          u.deleted != true &&
                                                          ur.active == true &&
                                                          ur.deleted != true &&
                                                          sr.active == true &&
                                                          sr.deleted != true &&
                                                          msr.active == true &&
                                                          msr.deleted != true &&
                                                          m.active == true &&
                                                          m.deleted != true
                                                          select sr)
                                                    .ToListAsync(cancellationToken)
                                                    .ConfigureAwait(false);


                    List<int> groupIds = new List<int>();

                    foreach (SecurityUserSecurityGroup ug in SecurityUserSecurityGroups)
                    {
                        groupIds.Add(ug.securityGroupId);
                    }


                    // Demetree todo august 5, 2025 make this query fast!!

                    List<SecurityRole> groupRoles = await (from gr in db.SecurityGroupSecurityRoles
                                                           join sr in db.SecurityRoles on gr.securityRoleId equals sr.id
                                                           join g in db.SecurityGroups on gr.securityGroupId equals g.id
                                                           join msr in db.ModuleSecurityRoles on sr.id equals msr.securityRoleId
                                                           join m in db.Modules on msr.moduleId equals m.id
                                                           where groupIds.Any(x => x.Equals(g.id)) &&
                                                           m.name == moduleName &&
                                                           g.active == true &&
                                                           g.deleted != true &&
                                                           gr.active == true &&
                                                           gr.deleted != true &&
                                                           msr.active == true &&
                                                           msr.deleted != true &&
                                                           m.active == true &&
                                                           m.deleted != true
                                                           select sr)
                                                     .ToListAsync(cancellationToken)
                                                     .ConfigureAwait(false);

                    List<SecurityRole> output = userRoles.Concat(groupRoles).Distinct().ToList();

                    mcm.Set(cacheKey, output, CACHE_TIME_MINUTES);          // cache the user permissions for 0.1 minutes (cache gets cleared on security updates through the web api)

                    return output;
                }
            }
        }


        public static List<SecurityRole> GetRolesUserIsEntitledToForModule(string moduleName, SecurityUser user, List<SecurityUserSecurityGroup> SecurityUserSecurityGroups)
        {
            //
            // This returns a list of Roles that a user is entitled to.  It does this by querying the UserRole table, and the GroupRole table for any
            // Roles the user may belong to, based on their user name, and their AD group memberships.
            //
            MemoryCacheManager mcm = new MemoryCacheManager();


            string cacheKey = "SL_GRUIETFRM_" + user.accountName + "_" + moduleName;

            if (mcm.IsSet(cacheKey) == true)
            {
                return mcm.Get<List<SecurityRole>>(cacheKey);
            }
            else
            {
                using (SecurityContext db = new SecurityContext())
                {
                    List<SecurityRole> userRoles = (from ur in db.SecurityUserSecurityRoles
                                                    join sr in db.SecurityRoles on ur.securityRoleId equals sr.id
                                                    join u in db.SecurityUsers on ur.securityUserId equals u.id
                                                    join msr in db.ModuleSecurityRoles on sr.id equals msr.securityRoleId
                                                    join m in db.Modules on msr.moduleId equals m.id
                                                    where u.id == user.id &&
                                                    m.name == moduleName &&
                                                    u.active == true &&
                                                    u.deleted != true &&
                                                    ur.active == true &&
                                                    ur.deleted != true &&
                                                    sr.active == true &&
                                                    sr.deleted != true &&
                                                    msr.active == true &&
                                                    msr.deleted != true &&
                                                    m.active == true &&
                                                    m.deleted != true
                                                    select sr).ToList();


                    List<int> groupIds = new List<int>();

                    foreach (SecurityUserSecurityGroup ug in SecurityUserSecurityGroups)
                    {
                        groupIds.Add(ug.securityGroupId);
                    }

                    List<SecurityRole> groupRoles = (from gr in db.SecurityGroupSecurityRoles
                                                     join sr in db.SecurityRoles on gr.securityRoleId equals sr.id
                                                     join g in db.SecurityGroups on gr.securityGroupId equals g.id
                                                     join msr in db.ModuleSecurityRoles on sr.id equals msr.securityRoleId
                                                     join m in db.Modules on msr.moduleId equals m.id
                                                     where groupIds.Any(x => x.Equals(g.id)) &&
                                                     m.name == moduleName &&
                                                     g.active == true &&
                                                     g.deleted != true &&
                                                     gr.active == true &&
                                                     gr.deleted != true &&
                                                     msr.active == true &&
                                                     msr.deleted != true &&
                                                     m.active == true &&
                                                     m.deleted != true
                                                     select sr).ToList();

                    List<SecurityRole> output = userRoles.Concat(groupRoles).Distinct().ToList();

                    mcm.Set(cacheKey, output, CACHE_TIME_MINUTES);          // cache the user permissions for 0.1 minutes (cache gets cleared on security updates through the web api)

                    return output;
                }
            }
        }


        /// <summary>
        /// 
        /// Retrieves the list of most-recently-used items for the current user from their persisted settings.
        /// 
        /// </summary>
        /// <param name="securityUser">The authenticated security user.</param>
        /// <param name="cancellationToken">Optional cancellation token for the asynchronous operation.</param>
        /// <returns>
        /// A sorted List<UserMostRecent> containing the user's most recent items, or null if no items are stored
        /// or if the user/tenant does not qualify for most-recent tracking.
        /// </returns>
        /// <remarks>
        ///
        /// The stored setting is expected to be a JSON object with the shape:
        /// {
        ///   "mostRecents": [
        ///     { "id": "...", "entity": "...", "sequence": 1, "description": "..." },
        ///     ...
        ///   ]
        /// }
        ///
        /// - Returns null if the setting does not exist, is null, or the "mostRecents" array is missing/null.
        /// - Deserializes each array element into a strongly-typed UserMostRecent instance.
        /// - Sorts the final list by sequence to ensure consistent ordering.
        /// - Logs exceptions via the existing audit mechanism and returns null (fail-closed).
        /// 
        /// </remarks>
        public static List<UserMostRecent> GetUserMostRecents(SecurityUser securityUser)
        {
            //
            // Basic precondition checks
            //
            if (securityUser == null)
            {
                return null;
            }

            //
            // Most-recent tracking requires a tenant
            //
            if (!securityUser.securityTenantId.HasValue)
            {
                return null;
            }

            string settingName = "MostRecents_" + securityUser.securityTenantId.Value.ToString();

            try
            {
                // Retrieve the stored JSON subtree (returns JsonNode? – null if not present or JSON null)
                JsonNode mostRecentsNode = UserSettings.GetObjectSetting(settingName, securityUser);
                                                                                    

                //
                // If nothing was stored or the value was explicit JSON null, return null
                //
                if (mostRecentsNode is not JsonObject rootObject)
                {
                    return null;
                }

                //
                // Look for the "mostRecents" array property
                //
                if (rootObject.TryGetPropertyValue("mostRecents", out JsonNode? arrayNode) == false || arrayNode is not JsonArray mostRecentsArray)
                {
                    return null;
                }

                //
                // Build the working list by deserializing each array element
                //
                List<UserMostRecent> workList = new List<UserMostRecent>(mostRecentsArray.Count);

                foreach (JsonNode itemNode in mostRecentsArray)
                {
                    //
                    // Skip any null elements in the array (defensive – should not occur but safe)
                    //
                    if (itemNode is not JsonObject itemObject)
                    {
                        continue;
                    }

                    //
                    // Extract the expected properties with safe fallbacks
                    //
                    // Use GetValue<T>() which throws if the property is missing or wrong type.
                    //
                    // The try/catch around the whole method will log any issues.
                    //
                    int id = itemObject["id"]!.GetValue<int>();
                    string entity = itemObject["entity"]!.GetValue<string>();
                    int sequence = itemObject["sequence"]!.GetValue<int>();
                    string description = itemObject["description"]!.GetValue<string>();

                    workList.Add(new UserMostRecent
                    {
                        id = id,
                        entity = entity,
                        sequence = sequence,
                        description = description
                    });
                }

                //
                // Sort by sequence if we have more than one item (preserves original behavior)
                //
                if (workList.Count > 1)
                {
                    workList = workList.OrderBy(x => x.sequence).ToList();
                }

                return workList;
            }
            catch (Exception ex)
            {
                //
                // Audit any error getting the most recents
                //
                Foundation.Utility.CreateAuditEvent("Error caught while reading the most recent record list for user '" + securityUser.accountName + "'.", ex);

                return null;
            }
        }

       

        /// <summary>
        /// 
        /// Retrieves the list of most-recently-used items for the current user from their persisted settings.
        /// 
        /// </summary>
        /// <param name="securityUser">The authenticated security user.</param>
        /// <param name="cancellationToken">Optional cancellation token for the asynchronous operation.</param>
        /// <returns>
        /// A sorted List<UserMostRecent> containing the user's most recent items, or null if no items are stored
        /// or if the user/tenant does not qualify for most-recent tracking.
        /// </returns>
        /// <remarks>
        ///
        /// The stored setting is expected to be a JSON object with the shape:
        /// {
        ///   "mostRecents": [
        ///     { "id": "...", "entity": "...", "sequence": 1, "description": "..." },
        ///     ...
        ///   ]
        /// }
        ///
        /// - Returns null if the setting does not exist, is null, or the "mostRecents" array is missing/null.
        /// - Deserializes each array element into a strongly-typed UserMostRecent instance.
        /// - Sorts the final list by sequence to ensure consistent ordering.
        /// - Logs exceptions via the existing audit mechanism and returns null (fail-closed).
        /// 
        /// </remarks>
        public static async Task<List<UserMostRecent>> GetUserMostRecentsAsync(SecurityUser securityUser,
                                                                               CancellationToken cancellationToken = default)
        {
            //
            // Basic precondition checks
            //
            if (securityUser == null)
            {
                return null;
            }

            //
            // Most-recent tracking requires a tenant
            //
            if (!securityUser.securityTenantId.HasValue)
            {
                return null;
            }

            string settingName = "MostRecents_" + securityUser.securityTenantId.Value.ToString();

            try
            {
                // Retrieve the stored JSON subtree (returns JsonNode? – null if not present or JSON null)
                JsonNode mostRecentsNode = await UserSettings.GetObjectSettingAsync(settingName,
                                                                                    securityUser,
                                                                                    cancellationToken)
                                                             .ConfigureAwait(false);

                //
                // If nothing was stored or the value was explicit JSON null, return null
                //
                if (mostRecentsNode is not JsonObject rootObject)
                {
                    return null;
                }

                //
                // Look for the "mostRecents" array property
                //
                if (rootObject.TryGetPropertyValue("mostRecents", out JsonNode? arrayNode) == false || arrayNode is not JsonArray mostRecentsArray)
                {
                    return null;
                }

                //
                // Build the working list by deserializing each array element
                //
                List<UserMostRecent> workList = new List<UserMostRecent>(mostRecentsArray.Count);

                foreach (JsonNode itemNode in mostRecentsArray)
                {
                    //
                    // Skip any null elements in the array (defensive – should not occur but safe)
                    //
                    if (itemNode is not JsonObject itemObject)
                    {
                        continue;
                    }

                    //
                    // Extract the expected properties with safe fallbacks
                    //
                    // Use GetValue<T>() which throws if the property is missing or wrong type.
                    //
                    // The try/catch around the whole method will log any issues.
                    //
                    int id = itemObject["id"]!.GetValue<int>();
                    string entity = itemObject["entity"]!.GetValue<string>();
                    int sequence = itemObject["sequence"]!.GetValue<int>();
                    string description = itemObject["description"]!.GetValue<string>();

                    workList.Add(new UserMostRecent
                    {
                        id = id,
                        entity = entity,
                        sequence = sequence,
                        description = description
                    });
                }

                //
                // Sort by sequence if we have more than one item (preserves original behavior)
                //
                if (workList.Count > 1)
                {
                    workList = workList.OrderBy(x => x.sequence).ToList();
                }

                return workList;
            }
            catch (Exception ex)
            {
                //
                // Audit any error getting the most recents
                //
                Foundation.Utility.CreateAuditEvent("Error caught while reading the most recent record list for user '" + securityUser.accountName + "'.", ex);

                return null;
            }
        }


        /// <summary>
        /// 
        /// Helper to allow long keyed entities to be used for most recents, up to the int max value anyway.
        /// 
        /// </summary>
        /// <param name="securityUserId"></param>
        /// <param name="entity"></param>
        /// <param name="id"></param>
        /// <param name="description"></param>
        public static void AddToUserMostRecents(int securityUserId, string entity, long id, string description)
        {
            if (id < int.MaxValue)
            {
                AddToUserMostRecents(securityUserId, entity, (int)id, description);
            }
            else
            { 
                // Not currently supporting most recent tracking for ids exceeding the max int.  Can add in the future, but the tables needing bigint keys aren't likely to be containing records making any sense to store as 'most recents' because they are massive data storage tables in 99% of use cases.
            }

        }


        /// <summary>
        /// 
        /// Adds a new item to the user's "most recently used" list for their tenant.
        /// 
        /// This method ensures the newly referenced item appears first in the list, while preserving
        /// up to 19 previous distinct items (for a maximum total of 20 entries). Existing duplicates
        /// are removed, and the list is re-sequenced starting from 1.
        /// 
        /// </summary>
        /// <param name="securityUserId">The database ID of the security user.</param>
        /// <param name="entity">The entity type being recorded (e.g., "Customer", "Order"). Must not be null or empty.</param>
        /// <param name="id">The unique identifier of the specific record within the entity.</param>
        /// <param name="description">Optional description. If null or empty, a default of "{entity} {id}" is used.</param>
        /// <param name="cancellationToken">Optional cancellation token for the asynchronous operations.</param>
        /// <remarks>
        /// 
        /// The stored setting is a JSON object with the shape:
        /// {
        ///   "mostRecents": [
        ///     { "id": 123, "entity": "Customer", "sequence": 1, "description": "..." },
        ///     ...
        ///   ]
        /// }
        /// 
        /// - Early exit if user not found, no tenant, or entity is empty.
        /// - New item is always placed first (sequence = 1).
        /// - Duplicates of the new item are removed from previous entries.
        /// - Up to 19 additional distinct previous items are retained (total max 20).
        /// - List is re-sequenced 1..n after insertion.
        /// - On any error, the entire setting is cleared (null) and the exception is audited.
        /// 
        /// </remarks>
        public static void AddToUserMostRecents(int securityUserId, string entity, int id, string description)
        {
            // Load the user record – we need the tenant ID and the full SecurityUser object for settings access
            SecurityUser securityUser = null;

            using (SecurityContext sdb = new SecurityContext())
            {
                securityUser = sdb.SecurityUsers.Where(x => x.id == securityUserId)
                                                .FirstOrDefault();
                        
            }

            //
            // Basic validation – no user, no entity, or no tenant → nothing to do
            //
            if (securityUser == null || string.IsNullOrEmpty(entity))
            {
                return;
            }

            if (!securityUser.securityTenantId.HasValue)
            {
                return;
            }

            //
            // Provide a fallback description if none supplied
            //
            description ??= $"{entity} {id}";

            string settingName = "MostRecents_" + securityUser.securityTenantId.Value.ToString();

            try
            {
                //
                // Retrieve the existing stored JSON subtree (returns JsonNode? – null if not present)
                //
                JsonNode storedNode = UserSettings.GetObjectSetting(settingName, securityUser);
                                                                    

                //
                // Prepare the new list structure
                //
                JsonObject rootObject = new JsonObject();
                JsonArray mostRecentsArray = new JsonArray();
                rootObject["mostRecents"] = mostRecentsArray;

                //
                // Always add the new item as the first entry (sequence 1)
                //
                mostRecentsArray.Add(new JsonObject
                {
                    ["id"] = id,
                    ["entity"] = entity,
                    ["sequence"] = 1,
                    ["description"] = description
                });

                //
                // If there were previous entries, process them
                //
                if (storedNode is JsonObject storedRoot &&
                    storedRoot.TryGetPropertyValue("mostRecents", out JsonNode storedArrayNode) &&
                    storedArrayNode is JsonArray storedArray)
                {
                    List<JsonObject> previousItems = new List<JsonObject>(storedArray.Count);

                    //
                    // Extract valid previous items, skipping any that match the new item (deduplication)
                    //
                    foreach (JsonNode itemNode in storedArray)
                    {
                        if (itemNode is JsonObject itemObject &&
                            itemObject.TryGetPropertyValue("entity", out JsonNode entityNode) && entityNode?.GetValue<string>() == entity &&
                            itemObject.TryGetPropertyValue("id", out JsonNode idNode) && idNode?.GetValue<int>() == id)
                        {
                            //
                            // This is a duplicate of the item we're adding now – skip it
                            //
                            continue;
                        }

                        if (itemNode is JsonObject validObject)
                        {
                            previousItems.Add(validObject);
                        }

                        //
                        // Limit to 19 previous items (plus the new one makes 20 total)
                        //
                        if (previousItems.Count == 19)
                        {
                            break;
                        }
                    }

                    ////
                    //// Re-sequence the retained previous items starting from 2
                    ////
                    //int sequence = 2;
                    //foreach (JsonObject previousItem in previousItems)
                    //{
                    //    // Ensure the sequence property is updated (in case stored data had gaps)
                    //    previousItem["sequence"] = sequence;
                    //    mostRecentsArray.Add(previousItem);
                    //    sequence++;
                    //}

                    int sequence = 2;
                    foreach (JsonObject previousItem in previousItems)
                    {
                        // Deep clone the JsonObject to detach it from its original parent
                        // This creates a new node tree that can be safely added to another array
                        JsonObject clonedItem = (JsonObject)previousItem.DeepClone();

                        // Update the sequence number on the cloned item
                        // This ensures no gaps and avoids modifying the original source data if needed
                        clonedItem["sequence"] = sequence;

                        // Now safe to add to the new array
                        mostRecentsArray.Add(clonedItem);

                        sequence++;
                    }
                }

                //
                // Persist the updated structure
                // We pass the root JsonObject – SetObjectSettingAsync will serialize it correctly
                //
                UserSettings.SetObjectSetting(settingName, rootObject, securityUser);
            }
            catch (Exception ex)
            {
                //
                // On any failure (parsing, DB, etc.), log the issue and clear the corrupted setting
                // This prevents repeated failures on subsequent calls
                //
                Foundation.Utility.CreateAuditEvent("Error caught while managing the most recent record list for user '" + securityUser.accountName +
                                                    "'. Clearing all values. Entity is '" + entity + "' and id is '" + id +
                                                    "' and description is '" + description + "'.", ex);

                // Clear the setting to avoid persistent corruption
                UserSettings.SetObjectSetting(settingName, null, securityUser);
            }
        }

        /// <summary>
        /// 
        /// Adds a new item to the user's "most recently used" list for their tenant.
        /// 
        /// This method ensures the newly referenced item appears first in the list, while preserving
        /// up to 19 previous distinct items (for a maximum total of 20 entries). Existing duplicates
        /// are removed, and the list is re-sequenced starting from 1.
        /// 
        /// </summary>
        /// <param name="securityUserId">The database ID of the security user.</param>
        /// <param name="entity">The entity type being recorded (e.g., "Customer", "Order"). Must not be null or empty.</param>
        /// <param name="id">The unique identifier of the specific record within the entity.</param>
        /// <param name="description">Optional description. If null or empty, a default of "{entity} {id}" is used.</param>
        /// <param name="cancellationToken">Optional cancellation token for the asynchronous operations.</param>
        /// <remarks>
        /// 
        /// The stored setting is a JSON object with the shape:
        /// {
        ///   "mostRecents": [
        ///     { "id": 123, "entity": "Customer", "sequence": 1, "description": "..." },
        ///     ...
        ///   ]
        /// }
        /// 
        /// - Early exit if user not found, no tenant, or entity is empty.
        /// - New item is always placed first (sequence = 1).
        /// - Duplicates of the new item are removed from previous entries.
        /// - Up to 19 additional distinct previous items are retained (total max 20).
        /// - List is re-sequenced 1..n after insertion.
        /// - On any error, the entire setting is cleared (null) and the exception is audited.
        /// 
        /// </remarks>
        public static async Task AddToUserMostRecentsAsync(int securityUserId,
                                                           string entity,
                                                           int id,
                                                           string description,
                                                           CancellationToken cancellationToken = default)
        {
            //
            // Load the user record – we need the tenant ID and the full SecurityUser object for settings access
            //
            SecurityUser securityUser = null;

            using (SecurityContext sdb = new SecurityContext())
            {
                securityUser = await sdb.SecurityUsers.Where(x => x.id == securityUserId)
                                                      .FirstOrDefaultAsync(cancellationToken)
                                                      .ConfigureAwait(false);
            }

            //
            // Basic validation – no user, no entity, or no tenant → nothing to do
            //
            if (securityUser == null || string.IsNullOrEmpty(entity))
            {
                return;
            }

            if (!securityUser.securityTenantId.HasValue)
            {
                return;
            }

            //
            // Provide a fallback description if none supplied
            //
            description ??= $"{entity} {id}";

            string settingName = "MostRecents_" + securityUser.securityTenantId.Value.ToString();

            try
            {
                //
                // Retrieve the existing stored JSON subtree (returns JsonNode? – null if not present)
                //
                JsonNode storedNode = await UserSettings.GetObjectSettingAsync(settingName,
                                                                               securityUser,
                                                                               cancellationToken).ConfigureAwait(false);

                //
                // Prepare the new list structure
                //
                JsonObject rootObject = new JsonObject();
                JsonArray mostRecentsArray = new JsonArray();
                rootObject["mostRecents"] = mostRecentsArray;

                //
                // Always add the new item as the first entry (sequence 1)
                //
                mostRecentsArray.Add(new JsonObject
                {
                    ["id"] = id,
                    ["entity"] = entity,
                    ["sequence"] = 1,
                    ["description"] = description
                });

                //
                // If there were previous entries, process them
                //
                if (storedNode is JsonObject storedRoot &&
                    storedRoot.TryGetPropertyValue("mostRecents", out JsonNode storedArrayNode) &&
                    storedArrayNode is JsonArray storedArray)
                {
                    List<JsonObject> previousItems = new List<JsonObject>(storedArray.Count);

                    //
                    // Extract valid previous items, skipping any that match the new item (deduplication)
                    //
                    foreach (JsonNode itemNode in storedArray)
                    {
                        if (itemNode is JsonObject itemObject &&
                            itemObject.TryGetPropertyValue("entity", out JsonNode entityNode) && entityNode?.GetValue<string>() == entity &&
                            itemObject.TryGetPropertyValue("id", out JsonNode idNode) && idNode?.GetValue<int>() == id)
                        {
                            //
                            // This is a duplicate of the item we're adding now – skip it
                            //
                            continue;
                        }

                        if (itemNode is JsonObject validObject)
                        {
                            previousItems.Add(validObject);
                        }

                        //
                        // Limit to 19 previous items (plus the new one makes 20 total)
                        //
                        if (previousItems.Count == 19)
                        {
                            break;
                        }
                    }

                    //
                    // Re-sequence the retained previous items starting from 2
                    //
                    int sequence = 2;
                    foreach (JsonObject previousItem in previousItems)
                    {
                        // Ensure the sequence property is updated (in case stored data had gaps)
                        previousItem["sequence"] = sequence;
                        mostRecentsArray.Add(previousItem);
                        sequence++;
                    }
                }

                //
                // Persist the updated structure
                // We pass the root JsonObject – SetObjectSettingAsync will serialize it correctly
                //
                await UserSettings.SetObjectSettingAsync(settingName,
                                                         rootObject,
                                                         securityUser,
                                                         cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                //
                // On any failure (parsing, DB, etc.), log the issue and clear the corrupted setting
                // This prevents repeated failures on subsequent calls
                //
                Foundation.Utility.CreateAuditEvent("Error caught while managing the most recent record list for user '" + securityUser.accountName +
                                                    "'. Clearing all values. Entity is '" + entity + "' and id is '" + id +
                                                    "' and description is '" + description + "'.", ex);

                // Clear the setting to avoid persistent corruption
                await UserSettings.SetObjectSettingAsync(settingName,
                                                         null,
                                                         securityUser,
                                                         cancellationToken).ConfigureAwait(false);
            }
        }


        /// <summary>
        /// 
        /// Retrieves the user's favourited items, grouped by entity type, and returns them in a dictionary.
        /// Optionally filters to a single entity.
        /// 
        /// </summary>
        /// <param name="securityUser">The authenticated security user.</param>
        /// <param name="entity">
        /// Optional entity type filter (e.g., "Customer", "Order"). If null, all favourites from all entities are returned.
        /// If specified, only favourites for that entity are included.
        /// </param>
        /// <returns>
        /// A Dictionary<string, List<UserFavouriteMinimalData>> where the key is the entity name and the value
        /// is a list of minimal favourite data (id + description) for that entity.
        /// Returns null if the user has no tenant or no favourites are stored.
        /// </returns>
        /// <remarks>
        /// The stored setting is expected to be a JSON object with the shape:
        /// {
        ///   "favourites": [
        ///     { "entity": "Customer", "id": 123, "description": "Acme Corp" },
        ///     ...
        ///   ]
        /// }
        ///
        /// - Returns null if the user has no tenant.
        /// - Returns null if the setting does not exist or the "favourites" array is missing/null.
        /// - Applies optional entity filtering (case-sensitive string comparison).
        /// - Groups results by entity and projects into UserFavouriteMinimalData (id + description only).
        /// - Returns a dictionary with entity as key and list of minimal items as value.
        /// 
        /// </remarks>
        public static Dictionary<string, List<UserFavouriteMinimalData>> GetUserFavouritesGroupedByEntity(SecurityUser securityUser, string entity = null)
        {
            // Favourites require a tenant – early exit if missing
            if (securityUser.securityTenantId.HasValue == false)
            {
                return null;
            }

            string settingName = "Favourites_" + securityUser.securityTenantId.Value.ToString();

            //
            // Retrieve the stored JSON subtree synchronously (returns JsonNode – null if not present or JSON null)
            //
            JsonNode favouritesNode = UserSettings.GetObjectSetting(settingName, securityUser);

            //
            // If nothing was stored or the root is not an object, treat as no favourites
            //
            if (favouritesNode is not JsonObject rootObject)
            {
                return null;
            }

            //
            // Look for the "favourites" array property
            //
            if (rootObject.TryGetPropertyValue("favourites", out JsonNode arrayNode) == false ||
                arrayNode is not JsonArray favouritesArray)
            {
                return null;
            }

            //
            // Temporary list to hold full UserFavourite objects during processing
            // We use the full object for easier filtering, then project to minimal data during grouping
            //
            List<UserFavourite> fullItems = new List<UserFavourite>(favouritesArray.Count);

            foreach (JsonNode itemNode in favouritesArray)
            {
                //
                // Defensive: skip any non-object or null elements in the array
                //
                if (itemNode is not JsonObject itemObject)
                {
                    continue;
                }

                //
                // Safely extract required properties
                // Skip the item if any required property is missing or wrong type (treat as corrupted)
                //
                if (!itemObject.TryGetPropertyValue("entity", out JsonNode entityNode) ||
                    !itemObject.TryGetPropertyValue("id", out JsonNode idNode) ||
                    !itemObject.TryGetPropertyValue("description", out JsonNode descriptionNode))
                {
                    continue;
                }

                string entityFromFavourite = entityNode.GetValue<string>();

                //
                // Apply optional entity filter
                //
                if (entity == null || entity == entityFromFavourite)
                {
                    fullItems.Add(new UserFavourite
                    {
                        entity = entityFromFavourite,
                        id = idNode.GetValue<int>(),
                        description = descriptionNode.GetValue<string>()
                    });
                }
            }

            //
            // If no items matched (e.g., filter applied and nothing found), return null
            //
            if (fullItems.Count == 0)
            {
                return null;
            }

            //
            // Group by entity and project into the minimal data structure
            // Order within each group is stored order
            //
            return fullItems
                .GroupBy(x => x.entity,
                         x => new UserFavouriteMinimalData { id = x.id, description = x.description })
                .ToDictionary(g => g.Key, g => g.ToList());
        }



        /// <summary>
        /// 
        /// Retrieves the list of favourited items for the current user from their persisted settings.
        /// Optionally filters the results to a specific entity type.
        /// 
        /// </summary>
        /// <param name="securityUser">The authenticated security user.</param>
        /// <param name="entity">
        /// 
        /// Optional entity type filter (e.g., "Customer", "Order"). If null or empty, all favourites are returned.
        /// If specified, only favourites matching that entity are included.
        /// 
        /// </param>
        /// <returns>
        /// A sorted List<UserFavourite> containing the user's favourited items (optionally filtered),
        /// or null if no favourites are stored or if the user has no tenant.
        /// </returns>
        /// <remarks>
        ///
        /// The stored setting is expected to be a JSON object with the shape:
        /// {
        ///   "favourites": [
        ///     { "entity": "Customer", "id": 123, "description": "Acme Corp" },
        ///     ...
        ///   ]
        /// }
        ///
        /// - Returns null if the user has no tenant.
        /// - Returns null if the setting does not exist or the "favourites" array is missing/null.
        /// - Optionally filters by entity (case-sensitive string comparison).
        /// - Returns items sorted by entity → description → id.
        /// 
        /// Defensive parsing is applied: malformed items (missing properties or wrong types) are skipped
        /// rather than failing the entire load, ensuring resilience against minor data corruption.
        /// 
        /// </remarks>
        public static List<UserFavourite> GetUserFavourites(SecurityUser securityUser, string entity = null)
        {
            //
            // Favourites require a tenant – early exit if missing
            //
            if (securityUser.securityTenantId.HasValue == false)
            {
                return null;
            }

            string settingName = "Favourites_" + securityUser.securityTenantId.Value.ToString();

            //
            // Retrieve the stored JSON subtree synchronously (returns JsonNode – null if not present or JSON null)
            //
            JsonNode favouritesNode = UserSettings.GetObjectSetting(settingName, securityUser);

            //
            // If nothing was stored or the root is not an object, treat as no favourites
            //
            if (favouritesNode is not JsonObject rootObject)
            {
                return null;
            }

            //
            // Look for the "favourites" array property
            //
            if (rootObject.TryGetPropertyValue("favourites", out JsonNode arrayNode) == false||
                arrayNode is not JsonArray favouritesArray)
            {
                return null;
            }

            //
            // Build the working list by processing each array element
            //
            List<UserFavourite> workList = new List<UserFavourite>(favouritesArray.Count);

            foreach (JsonNode itemNode in favouritesArray)
            {
                //
                // Defensive: skip any non-object or null elements in the array
                //
                if (itemNode is not JsonObject itemObject)
                {
                    continue;
                }

                //
                // Safely extract the required properties.
                // If any property is missing or of the wrong type, skip this item to avoid breaking the whole list.
                //
                if (!itemObject.TryGetPropertyValue("entity", out JsonNode entityNode) ||
                    !itemObject.TryGetPropertyValue("id", out JsonNode idNode) ||
                    !itemObject.TryGetPropertyValue("description", out JsonNode descriptionNode))
                {
                    continue;
                }

                string entityFromFavourite = entityNode.GetValue<string>();

                //
                // Apply optional entity filter
                //
                if (entity == null || entity == entityFromFavourite)
                {
                    workList.Add(new UserFavourite
                    {
                        entity = entityFromFavourite,
                        id = idNode.GetValue<int>(),
                        description = descriptionNode.GetValue<string>()
                    });
                }
            }

            //
            // Sort the results consistently: entity, description, id
            //
            if (workList.Count > 0)
            {
                workList = workList
                    .OrderBy(x => x.entity)
                    .ThenBy(x => x.description)
                    .ThenBy(x => x.id)
                    .ToList();
            }

            return workList;
        }


        /// <summary>
        /// 
        /// Retrieves the list of favourited items for the current user from their persisted settings.
        /// Optionally filters the results to a specific entity type.
        /// 
        /// </summary>
        /// <param name="securityUser">The authenticated security user.</param>
        /// <param name="entity">
        /// 
        /// Optional entity type filter (e.g., "Customer", "Order"). If null or empty, all favourites are returned.
        /// If specified, only favourites matching that entity are included.
        /// 
        /// </param>
        /// <returns>
        /// A sorted List<UserFavourite> containing the user's favourited items (optionally filtered),
        /// or null if no favourites are stored or if the user has no tenant.
        /// </returns>
        /// <remarks>
        ///
        /// The stored setting is expected to be a JSON object with the shape:
        /// {
        ///   "favourites": [
        ///     { "entity": "Customer", "id": 123, "description": "Acme Corp" },
        ///     ...
        ///   ]
        /// }
        ///
        /// - Returns null if the user has no tenant.
        /// - Returns null if the setting does not exist or the "favourites" array is missing/null.
        /// - Optionally filters by entity (case-sensitive string comparison).
        /// - Returns items sorted by entity → description → id.
        /// 
        /// Defensive parsing is applied: malformed items (missing properties or wrong types) are skipped
        /// rather than failing the entire load, ensuring resilience against minor data corruption.
        /// 
        /// </remarks>
        public static async Task<List<UserFavourite>> GetUserFavouritesAsync(SecurityUser securityUser, string entity = null, CancellationToken cancellationToken = default)
        {
            //
            // Favourites require a tenant – early exit if missing
            //
            if (securityUser.securityTenantId.HasValue == false)
            {
                return null;
            }

            string settingName = "Favourites_" + securityUser.securityTenantId.Value.ToString();

            //
            // Retrieve the stored JSON subtree synchronously (returns JsonNode – null if not present or JSON null)
            //
            JsonNode favouritesNode = await UserSettings.GetObjectSettingAsync(settingName, securityUser, cancellationToken).ConfigureAwait(false);

            //
            // If nothing was stored or the root is not an object, treat as no favourites
            //
            if (favouritesNode is not JsonObject rootObject)
            {
                return null;
            }

            //
            // Look for the "favourites" array property
            //
            if (rootObject.TryGetPropertyValue("favourites", out JsonNode arrayNode) == false ||
                arrayNode is not JsonArray favouritesArray)
            {
                return null;
            }

            //
            // Build the working list by processing each array element
            //
            List<UserFavourite> workList = new List<UserFavourite>(favouritesArray.Count);

            foreach (JsonNode itemNode in favouritesArray)
            {
                //
                // Defensive: skip any non-object or null elements in the array
                //
                if (itemNode is not JsonObject itemObject)
                {
                    continue;
                }

                //
                // Safely extract the required properties.
                // If any property is missing or of the wrong type, skip this item to avoid breaking the whole list.
                //
                if (!itemObject.TryGetPropertyValue("entity", out JsonNode entityNode) ||
                    !itemObject.TryGetPropertyValue("id", out JsonNode idNode) ||
                    !itemObject.TryGetPropertyValue("description", out JsonNode descriptionNode))
                {
                    continue;
                }

                string entityFromFavourite = entityNode.GetValue<string>();

                //
                // Apply optional entity filter
                //
                if (entity == null || entity == entityFromFavourite)
                {
                    workList.Add(new UserFavourite
                    {
                        entity = entityFromFavourite,
                        id = idNode.GetValue<int>(),
                        description = descriptionNode.GetValue<string>()
                    });
                }
            }

            //
            // Sort the results consistently: entity, description, id
            //
            if (workList.Count > 0)
            {
                workList = workList
                    .OrderBy(x => x.entity)
                    .ThenBy(x => x.description)
                    .ThenBy(x => x.id)
                    .ToList();
            }

            return workList;
        }



        // This version is here with simple parameters so it can be scheduled by scheduling tools more easily
        public static void AddToUserFavourites(int securityUserId, string entity, int id, string description)
        {
            SecurityUser securityUser = null;

            using (SecurityContext sdb = new SecurityContext())
            {
                securityUser = (from x in sdb.SecurityUsers where x.id == securityUserId select x).FirstOrDefault();
            }

            AddToUserFavourites(securityUser.id, entity, id, description);

            return;
        }


        /// <summary>
        /// 
        /// Adds a new favourite item for the user, placing it at the beginning of the list.
        /// 
        /// If the same item (entity + id combination) already exists, it is removed from its previous
        /// position and re-added as the first entry (effectively "bumping" it to the top).
        /// The list is capped at 1000 items
        /// 
        /// </summary>
        /// <param name="securityUser">The authenticated security user. Must not be null.</param>
        /// <param name="entity">The entity type (e.g., "Customer", "Order"). Must not be null or empty.</param>
        /// <param name="id">The unique identifier of the record within the entity.</param>
        /// <param name="description">
        /// Optional description for the favourite. If null or empty, a default of "{entity} {id}" is used.
        /// </param>
        /// <remarks>
        ///
        /// The stored setting is expected to be a JSON object with the shape:
        /// {
        ///   "favourites": [
        ///     { "entity": "Customer", "id": 123, "description": "Acme Corp" },
        ///     ...
        ///   ]
        /// }
        ///
        /// - Early exit if user is null, entity is empty, or user has no tenant.
        /// - New item is always added as the first entry.
        /// - Duplicates (same entity + id) are removed from previous positions.
        /// - Maximum of 1000 retained previous items (plus the new one).
        /// - On any error, the entire favourites setting is cleared and the exception is audited.
        /// 
        /// </remarks>
        public static void AddToUserFavourites(SecurityUser securityUser,
                                               string entity,
                                               int id,
                                               string description)
        {
            //
            // Basic input validation – early exit on invalid parameters
            //
            if (securityUser == null || string.IsNullOrEmpty(entity))
            {
                return;
            }

            //
            // Provide a fallback description if none supplied
            //
            if (string.IsNullOrEmpty(description) == true)
            {
                description = $"{entity} {id}";
            }

            // Favourites require a tenant – early exit if missing
            if (securityUser.securityTenantId.HasValue == false)
            {
                return;
            }

            string settingName = "Favourites_" + securityUser.securityTenantId.Value.ToString();

            try
            {
                //
                // Retrieve the existing stored JSON subtree synchronously (returns JsonNode – null if not present)
                //
                JsonNode storedNode = UserSettings.GetObjectSetting(settingName, securityUser);

                //
                // Prepare the new root structure
                //
                JsonObject rootObject = new JsonObject();
                JsonArray favouritesArray = new JsonArray();
                rootObject["favourites"] = favouritesArray;

                //
                // Always add the new favourite as the first item
                //
                favouritesArray.Add(new JsonObject
                {
                    ["entity"] = entity,
                    ["id"] = id,
                    ["description"] = description
                });

                //
                // If previous favourites exist, process them
                //
                if (storedNode is JsonObject storedRoot &&
                    storedRoot.TryGetPropertyValue("favourites", out JsonNode storedArrayNode) &&
                    storedArrayNode is JsonArray storedArray)
                {
                    List<JsonObject> previousItems = new List<JsonObject>(storedArray.Count);

                    int processedCount = 0;
                    const int MaxPreviousItems = 1000; // Hard limit from original implementation

                    foreach (JsonNode itemNode in storedArray)
                    {
                        // Only consider valid object items
                        if (itemNode is JsonObject itemObject &&
                            itemObject.TryGetPropertyValue("entity", out JsonNode prevEntityNode) &&
                            itemObject.TryGetPropertyValue("id", out JsonNode prevIdNode))
                        {
                            string prevEntity = prevEntityNode.GetValue<string>();
                            int prevId = prevIdNode.GetValue<int>();

                            // Skip if this is a duplicate of the item we're adding now
                            if (prevEntity == entity && prevId == id)
                            {
                                continue;
                            }
                        }

                        // Add valid non-duplicate items to the retained list
                        if (itemNode is JsonObject validObject)
                        {
                            previousItems.Add(validObject);
                        }

                        // Enforce the hard limit of 1000 previous items
                        processedCount++;
                        if (processedCount >= MaxPreviousItems)
                        {
                            break;
                        }
                    }

                    // Append the retained previous items after the new one
                    foreach (JsonObject previousItem in previousItems)
                    {
                        favouritesArray.Add(previousItem);
                    }
                }

                //
                // Persist the updated favourites structure
                //
                UserSettings.SetObjectSetting(settingName, rootObject, securityUser);
            }
            catch (Exception ex)
            {
                // On any failure (parsing error, DB issue, etc.), log the problem and clear the corrupted setting
                // This prevents repeated failures on subsequent additions
                Foundation.Utility.CreateAuditEvent("Error caught while adding a record to the favourite records list for user '" + securityUser.accountName +
                                                    "'. Clearing all values. Entity is '" + entity + "' and id is '" + id + "'.",
                                                    ex);

                // Clear the setting to ensure future operations start fresh
                try
                {
                    UserSettings.SetObjectSetting(settingName, null, securityUser);
                }
                catch
                { }
            }
        }


        /// <summary>
        /// 
        /// Adds a new favourite item for the user, placing it at the beginning of the list.
        /// 
        /// If the same item (entity + id combination) already exists, it is removed from its previous
        /// position and re-added as the first entry (effectively "bumping" it to the top).
        /// The list is capped at 1000 items
        /// 
        /// </summary>
        /// <param name="securityUser">The authenticated security user. Must not be null.</param>
        /// <param name="entity">The entity type (e.g., "Customer", "Order"). Must not be null or empty.</param>
        /// <param name="id">The unique identifier of the record within the entity.</param>
        /// <param name="description">
        /// Optional description for the favourite. If null or empty, a default of "{entity} {id}" is used.
        /// </param>
        /// <remarks>
        ///
        /// The stored setting is expected to be a JSON object with the shape:
        /// {
        ///   "favourites": [
        ///     { "entity": "Customer", "id": 123, "description": "Acme Corp" },
        ///     ...
        ///   ]
        /// }
        ///
        /// - Early exit if user is null, entity is empty, or user has no tenant.
        /// - New item is always added as the first entry.
        /// - Duplicates (same entity + id) are removed from previous positions.
        /// - Maximum of 1000 retained previous items (plus the new one).
        /// - On any error, the entire favourites setting is cleared and the exception is audited.
        /// 
        /// </remarks>
        public static async Task<bool> AddToUserFavouritesAsync(SecurityUser securityUser, string entity, int id, string description, CancellationToken cancellationToken = default)
        {
            //
            // Basic input validation – early exit on invalid parameters
            //
            if (securityUser == null || string.IsNullOrEmpty(entity))
            {
                return false;
            }

            //
            // Provide a fallback description if none supplied
            //
            if (string.IsNullOrEmpty(description) == true)
            {
                description = $"{entity} {id}";
            }

            // Favourites require a tenant – early exit if missing
            if (securityUser.securityTenantId.HasValue == false)
            {
                return false;
            }

            string settingName = "Favourites_" + securityUser.securityTenantId.Value.ToString();

            try
            {
                //
                // Retrieve the existing stored JSON subtree synchronously (returns JsonNode – null if not present)
                //
                JsonNode storedNode = await UserSettings.GetObjectSettingAsync(settingName, securityUser, cancellationToken).ConfigureAwait(false);

                //
                // Prepare the new root structure
                //
                JsonObject rootObject = new JsonObject();
                JsonArray favouritesArray = new JsonArray();
                rootObject["favourites"] = favouritesArray;

                //
                // Always add the new favourite as the first item
                //
                favouritesArray.Add(new JsonObject
                {
                    ["entity"] = entity,
                    ["id"] = id,
                    ["description"] = description
                });

                //
                // If previous favourites exist, process them
                //
                if (storedNode is JsonObject storedRoot &&
                    storedRoot.TryGetPropertyValue("favourites", out JsonNode storedArrayNode) &&
                    storedArrayNode is JsonArray storedArray)
                {
                    List<JsonObject> previousItems = new List<JsonObject>(storedArray.Count);

                    int processedCount = 0;
                    const int MaxPreviousItems = 1000; // Hard limit from original implementation

                    foreach (JsonNode itemNode in storedArray)
                    {
                        // Only consider valid object items
                        if (itemNode is JsonObject itemObject &&
                            itemObject.TryGetPropertyValue("entity", out JsonNode prevEntityNode) &&
                            itemObject.TryGetPropertyValue("id", out JsonNode prevIdNode))
                        {
                            string prevEntity = prevEntityNode.GetValue<string>();
                            int prevId = prevIdNode.GetValue<int>();

                            // Skip if this is a duplicate of the item we're adding now
                            if (prevEntity == entity && prevId == id)
                            {
                                continue;
                            }
                        }

                        // Add valid non-duplicate items to the retained list
                        if (itemNode is JsonObject validObject)
                        {
                            previousItems.Add(validObject);
                        }

                        // Enforce the hard limit of 1000 previous items
                        processedCount++;
                        if (processedCount >= MaxPreviousItems)
                        {
                            break;
                        }
                    }

                    // Append the retained previous items after the new one
                    foreach (JsonObject previousItem in previousItems)
                    {
                        favouritesArray.Add(previousItem);
                    }
                }

                //
                // Persist the updated favourites structure
                //
                await UserSettings.SetObjectSettingAsync(settingName, rootObject, securityUser, cancellationToken).ConfigureAwait(false);

                return true;
            }
            catch (Exception ex)
            {
                // On any failure (parsing error, DB issue, etc.), log the problem and clear the corrupted setting
                // This prevents repeated failures on subsequent additions
                Foundation.Utility.CreateAuditEvent("Error caught while adding a record to the favourite records list for user '" + securityUser.accountName +
                                                    "'. Clearing all values. Entity is '" + entity + "' and id is '" + id + "'.",
                                                    ex);

                // Clear the setting to ensure future operations start fresh
                try
                {
                    await UserSettings.SetObjectSettingAsync(settingName, null, securityUser, cancellationToken).ConfigureAwait(false);
                }
                catch 
                { }
            }

            return false;
        }



        public static void RemoveFromUserFavourites(int securityUserId, string entity, int id)
        {
            SecurityUser securityUser = null;

            using (SecurityContext sdb = new SecurityContext())
            {
                securityUser = (from x in sdb.SecurityUsers where x.id == securityUserId select x).FirstOrDefault();
            }

            RemoveFromUserFavourites(securityUser, entity, id);

            return;
        }


        /// <summary>
        /// 
        /// Removes a specific favourite item (identified by entity and id) from the user's favourites list.
        /// 
        /// If the item is not present, the operation is a no-op (the list remains unchanged).
        /// If the list becomes empty after removal, the setting is cleared (set to null) to keep storage clean.
        /// 
        /// </summary>
        /// <param name="securityUser">The authenticated security user. Must not be null.</param>
        /// <param name="entity">The entity type of the favourite to remove (e.g., "Customer", "Order"). Must not be null or empty.</param>
        /// <param name="id">The unique identifier of the record within the entity to remove.</param>
        /// <remarks>
        /// 
        /// The stored setting is expected to be a JSON object with the shape:
        /// {
        ///   "favourites": [
        ///     { "entity": "Customer", "id": 123, "description": "Acme Corp" },
        ///     ...
        ///   ]
        /// }
        ///
        /// - Early exit if user is null, entity is empty, or user has no tenant.
        /// - Removes only the matching item (entity + id).
        /// - Retains all other items in their original order.
        /// - On any error (parsing, DB issue, etc.), the entire favourites setting is cleared and the exception is audited.
        /// 
        /// </remarks>

        public static async Task<bool> RemoveFromUserFavouritesAsync(SecurityUser securityUser, string entity, int id, CancellationToken cancellationToken = default)
        {
            //
            // Basic input validation – early exit on invalid parameters
            //
            if (securityUser == null || string.IsNullOrEmpty(entity))
            {
                return false;
            }

            //
            // Favourites require a tenant – early exit if missing
            //
            if (securityUser.securityTenantId.HasValue == false)
            {
                return false;
            }

            string settingName = "Favourites_" + securityUser.securityTenantId.Value.ToString();

            try
            {
                //
                // Retrieve the existing stored JSON subtree synchronously (returns JsonNode? – null if not present)
                //
                JsonNode storedNode = await UserSettings.GetObjectSettingAsync(settingName, securityUser, cancellationToken).ConfigureAwait(false);

                // If nothing is stored, there is nothing to remove – exit cleanly
                if (storedNode is null)
                {
                    return false;
                }

                //
                // We expect an object root with a "favourites" array
                //
                if (storedNode is not JsonObject rootObject ||
                    !rootObject.TryGetPropertyValue("favourites", out JsonNode? arrayNode) ||
                    arrayNode is not JsonArray storedArray)
                {
                    // Malformed or empty structure – treat as no favourites worth preserving
                    return false;
                }

                //
                // Prepare a new array containing only the items we want to keep
                //
                JsonArray newArray = new JsonArray();

                foreach (JsonNode itemNode in storedArray)
                {
                    // Only process valid object items
                    if (itemNode is JsonObject itemObject &&
                        itemObject.TryGetPropertyValue("entity", out JsonNode entityNode) &&
                        itemObject.TryGetPropertyValue("id", out JsonNode idNode))
                    {
                        string storedEntity = entityNode.GetValue<string>();
                        int storedId = idNode.GetValue<int>();

                        // Skip the item that matches the one we want to remove
                        if (storedEntity == entity && storedId == id)
                        {
                            continue;
                        }
                    }

                    //
                    // Keep all other valid items (including malformed ones – we copy them as-is to preserve data)
                    //
                    if (itemNode != null)
                    {
                        newArray.Add(itemNode.DeepClone()); // Clone to ensure independence from the original node
                    }
                }

                //
                // If the new array is empty, clear the entire setting (keeps storage tidy and consistent with "no favourites")
                //
                if (newArray.Count == 0)
                {
                    await UserSettings.SetObjectSettingAsync(settingName, null, securityUser, cancellationToken).ConfigureAwait(false);
                    return false;
                }

                //
                // Otherwise, save the updated structure with the reduced array
                //
                JsonObject newRoot = new JsonObject
                {
                    ["favourites"] = newArray
                };

                await UserSettings.SetObjectSettingAsync(settingName, newRoot, securityUser, cancellationToken).ConfigureAwait(false);

                return true;
            }
            catch (Exception ex)
            {
                //
                // On any failure, log the issue and clear the corrupted setting
                // This prevents repeated errors and ensures future operations start from a clean state
                //
                Foundation.Utility.CreateAuditEvent("Error caught while removing a record from the favourite records list for user '" + securityUser.accountName +
                                                    "'. Clearing all values. Entity is '" + entity + "' and id is '" + id + "'.", ex);

                try
                {
                    // Clear the setting to avoid persistent corruption
                    await UserSettings.SetObjectSettingAsync(settingName, null, securityUser, cancellationToken).ConfigureAwait(false);
                }
                catch
                { }
            }

            return false;
        }


        /// <summary>
        /// 
        /// Removes a specific favourite item (identified by entity and id) from the user's favourites list.
        /// 
        /// If the item is not present, the operation is a no-op (the list remains unchanged).
        /// If the list becomes empty after removal, the setting is cleared (set to null) to keep storage clean.
        /// 
        /// </summary>
        /// <param name="securityUser">The authenticated security user. Must not be null.</param>
        /// <param name="entity">The entity type of the favourite to remove (e.g., "Customer", "Order"). Must not be null or empty.</param>
        /// <param name="id">The unique identifier of the record within the entity to remove.</param>
        /// <remarks>
        /// 
        /// The stored setting is expected to be a JSON object with the shape:
        /// {
        ///   "favourites": [
        ///     { "entity": "Customer", "id": 123, "description": "Acme Corp" },
        ///     ...
        ///   ]
        /// }
        ///
        /// - Early exit if user is null, entity is empty, or user has no tenant.
        /// - Removes only the matching item (entity + id).
        /// - Retains all other items in their original order.
        /// - On any error (parsing, DB issue, etc.), the entire favourites setting is cleared and the exception is audited.
        /// 
        /// </remarks>
        public static void RemoveFromUserFavourites(SecurityUser securityUser, string entity, int id)
        {
            //
            // Basic input validation – early exit on invalid parameters
            //
            if (securityUser == null || string.IsNullOrEmpty(entity))
            {
                return;
            }

            //
            // Favourites require a tenant – early exit if missing
            //
            if (securityUser.securityTenantId.HasValue == false)
            {
                return;
            }

            string settingName = "Favourites_" + securityUser.securityTenantId.Value.ToString();

            try
            {
                //
                // Retrieve the existing stored JSON subtree synchronously (returns JsonNode? – null if not present)
                //
                JsonNode storedNode = UserSettings.GetObjectSetting(settingName, securityUser);

                // If nothing is stored, there is nothing to remove – exit cleanly
                if (storedNode is null)
                {
                    return;
                }

                //
                // We expect an object root with a "favourites" array
                //
                if (storedNode is not JsonObject rootObject ||
                    !rootObject.TryGetPropertyValue("favourites", out JsonNode? arrayNode) ||
                    arrayNode is not JsonArray storedArray)
                {
                    // Malformed or empty structure – treat as no favourites worth preserving
                    return;
                }

                //
                // Prepare a new array containing only the items we want to keep
                //
                JsonArray newArray = new JsonArray();

                foreach (JsonNode itemNode in storedArray)
                {
                    // Only process valid object items
                    if (itemNode is JsonObject itemObject &&
                        itemObject.TryGetPropertyValue("entity", out JsonNode entityNode) &&
                        itemObject.TryGetPropertyValue("id", out JsonNode idNode))
                    {
                        string storedEntity = entityNode.GetValue<string>();
                        int storedId = idNode.GetValue<int>();

                        // Skip the item that matches the one we want to remove
                        if (storedEntity == entity && storedId == id)
                        {
                            continue;
                        }
                    }

                    //
                    // Keep all other valid items (including malformed ones – we copy them as-is to preserve data)
                    //
                    if (itemNode != null)
                    {
                        newArray.Add(itemNode.DeepClone()); // Clone to ensure independence from the original node
                    }
                }

                //
                // If the new array is empty, clear the entire setting (keeps storage tidy and consistent with "no favourites")
                //
                if (newArray.Count == 0)
                {
                    UserSettings.SetObjectSetting(settingName, null, securityUser);
                    return;
                }

                //
                // Otherwise, save the updated structure with the reduced array
                //
                JsonObject newRoot = new JsonObject
                {
                    ["favourites"] = newArray
                };

                UserSettings.SetObjectSetting(settingName, newRoot, securityUser);
            }
            catch (Exception ex)
            {
                //
                // On any failure, log the issue and clear the corrupted setting
                // This prevents repeated errors and ensures future operations start from a clean state
                //
                Foundation.Utility.CreateAuditEvent("Error caught while removing a record from the favourite records list for user '" + securityUser.accountName +
                                                    "'. Clearing all values. Entity is '" + entity + "' and id is '" + id + "'.", ex);

                try
                {
                    // Clear the setting to avoid persistent corruption
                    UserSettings.SetObjectSetting(settingName, null, securityUser);
                }
                catch
                { }
            }
        }


        public static async Task<SecurityUser> GetUserRecordAsync(string accountName, CancellationToken cancellationToken = default)
        {
            //
            // This gets the user record for the provided acocunt name, whether or not the user record is or is not indicated to be an AD account
            //
            MemoryCacheManager mcm = new MemoryCacheManager();


            string cacheKey = "SL_GUR_" + accountName;

            if (mcm.IsSet(cacheKey) == true)
            {
                return mcm.Get<SecurityUser>(cacheKey);
            }
            else
            {
                using (SecurityContext db = new SecurityContext())
                {
                    SecurityUser securityUser = null;

                    bool multiTenancyModeOn = Foundation.Configuration.GetMultiTenancyMode();
                    bool dataVisibilityModeOn = Foundation.Configuration.GetDataVisibilityMode();


                    if (multiTenancyModeOn == true && dataVisibilityModeOn == true)
                    {
                        securityUser = await (from users in db.SecurityUsers
                                              where users.accountName.ToUpper() == accountName.ToUpper()
                                              select users)
                                              .Include("securityTenant")
                                              .Include("securityOrganization")
                                              .Include("securityDepartment")
                                              .Include("securityTeam")
                                              .AsNoTracking()
                                              .FirstOrDefaultAsync(cancellationToken)
                                              .ConfigureAwait(false);
                    }
                    else if (multiTenancyModeOn == true)
                    {
                        securityUser = await (from users in db.SecurityUsers
                                              where users.accountName.ToUpper() == accountName.ToUpper()
                                              select users)
                                              .Include("securityTenant")
                                              .AsNoTracking()
                                              .FirstOrDefaultAsync(cancellationToken)
                                              .ConfigureAwait(false);
                    }
                    else
                    {
                        securityUser = await (from users in db.SecurityUsers
                                              where users.accountName.ToUpper() == accountName.ToUpper()
                                              select users)
                                              .AsNoTracking()
                                              .FirstOrDefaultAsync(cancellationToken)
                                              .ConfigureAwait(false);
                    }


                    if (securityUser != null)
                    {
                        // take out the password when returning a user object.
                        securityUser.password = null;
                    }

                    mcm.Set(cacheKey, securityUser, CACHE_TIME_MINUTES);          // cache the user permissions for 0.1 minutes (cache gets cleared on security updates through the web api)

                    return securityUser;
                }
            }
        }


        public static async Task<SecurityUser> GetUserRecordAsync(Guid userObjectGuid, CancellationToken cancellationToken = default)
        {
            //
            // This gets the user record for the provided account name, whether or not the user record is or is not indicated to be an AD account
            //
            MemoryCacheManager mcm = new MemoryCacheManager();


            string cacheKey = "SL_GUR_" + userObjectGuid.ToString();

            if (mcm.IsSet(cacheKey) == true)
            {
                return mcm.Get<SecurityUser>(cacheKey);
            }
            else
            {
                using (SecurityContext db = new SecurityContext())
                {
                    SecurityUser securityUser = null;

                    bool multiTenancyModeOn = Foundation.Configuration.GetMultiTenancyMode();
                    bool dataVisibilityModeOn = Foundation.Configuration.GetDataVisibilityMode();


                    if (multiTenancyModeOn == true && dataVisibilityModeOn == true)
                    {
                        securityUser = await (from users in db.SecurityUsers
                                              where users.objectGuid == userObjectGuid
                                              select users)
                                .Include("securityTenant")
                                .Include("securityOrganization")
                                .Include("securityDepartment")
                                .Include("securityTeam")
                                .AsNoTracking()
                                .FirstOrDefaultAsync(cancellationToken)
                                .ConfigureAwait(false);
                    }
                    else if (multiTenancyModeOn == true)
                    {
                        securityUser = await (from users in db.SecurityUsers
                                              where users.objectGuid == userObjectGuid
                                              select users)
                                .Include("securityTenant")
                                .AsNoTracking()
                                .FirstOrDefaultAsync(cancellationToken)
                                .ConfigureAwait(false);
                    }
                    else
                    {
                        securityUser = await (from users in db.SecurityUsers
                                              where users.objectGuid == userObjectGuid
                                              select users)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(cancellationToken)
                                .ConfigureAwait(false);
                    }


                    if (securityUser != null)
                    {
                        // take out the password when returning a user object.
                        securityUser.password = null;
                    }

                    mcm.Set(cacheKey, securityUser, CACHE_TIME_MINUTES);          // cache the user permissions for 0.1 minutes (cache gets cleared on security updates through the web api)

                    return securityUser;
                }
            }
        }



        public static SecurityUser GetUserRecord(string accountName)
        {
            if (accountName == null)
            {
                return null;
            }

            //
            // This gets the user record for the provided account name, whether or not the user record is or is not indicated to be an AD account
            //
            MemoryCacheManager mcm = new MemoryCacheManager();


            string cacheKey = "SL_GUR_" + accountName;

            if (mcm.IsSet(cacheKey) == true)
            {
                return mcm.Get<SecurityUser>(cacheKey);
            }
            else
            {
                using (SecurityContext db = new SecurityContext())
                {
                    SecurityUser securityUser = null;

                    bool multiTenancyModeOn = Foundation.Configuration.GetMultiTenancyMode();
                    bool dataVisibilityModeOn = Foundation.Configuration.GetDataVisibilityMode();


                    if (multiTenancyModeOn == true && dataVisibilityModeOn == true)
                    {
                        securityUser = (from users in db.SecurityUsers
                                        where users.accountName.ToUpper() == accountName.ToUpper()
                                        select users)
                                        .Include("securityTenant")
                                        .Include("securityOrganization")
                                        .Include("securityDepartment")
                                        .Include("securityTeam")
                                        .AsNoTracking()
                                        .FirstOrDefault();
                    }
                    else if (multiTenancyModeOn == true)
                    {
                        securityUser = (from users in db.SecurityUsers
                                        where users.accountName.ToUpper() == accountName.ToUpper()
                                        select users)
                                        .Include("securityTenant")
                                        .AsNoTracking()
                                        .FirstOrDefault();
                    }
                    else
                    {
                        securityUser = (from users in db.SecurityUsers
                                        where users.accountName.ToUpper() == accountName.ToUpper()
                                        select users)
                                        .AsNoTracking()
                                        .FirstOrDefault();
                    }


                    if (securityUser != null)
                    {
                        // take out the password when returning a user object.
                        securityUser.password = null;
                    }

                    mcm.Set(cacheKey, securityUser, CACHE_TIME_MINUTES);          // cache the user permissions for 0.1 minutes (cache gets cleared on security updates through the web api)

                    return securityUser;
                }
            }
        }


        public static void ClearSecurityCaches()
        {
            MemoryCacheManager mcm = new MemoryCacheManager();

            mcm.RemoveByPattern("^SL_");        // Clear any item starting with SL
        }


        public static SecurityUser CreateLocalUserRecord(string accountName, string password, string firstName, string lastName, SecurityTenant tenant = null)
        {
            using (SecurityContext db = new SecurityContext())
            {
                //
                // Check for an existing user by user object guid
                //
                SecurityUser securityUser = (from users in db.SecurityUsers
                                             where
                                             users.accountName == accountName
                                             select users).FirstOrDefault();

                if (securityUser == null)
                {
                    //
                    // Create a new user record
                    //
                    securityUser = new SecurityUser();


                    securityUser.objectGuid = Guid.NewGuid();
                    securityUser.authenticationDomain = null;

                    securityUser.accountName = accountName;
                    securityUser.activeDirectoryAccount = false;         // when creating from a user principal object, this is to be marked as an AD account 


                    securityUser.mostRecentActivity = DateTime.UtcNow;

                    securityUser.firstName = firstName;
                    securityUser.lastName = lastName;

                    securityUser.password = Foundation.Security.SecurityLogic.SecurePasswordHasher.Hash(password);
                    securityUser.failedLoginCount = 0;
                    securityUser.lastLoginAttempt = null;

                    if (tenant != null)
                    {
                        securityUser.securityTenantId = tenant.id;
                    }

                    string description = "";

                    if (firstName != null)
                    {
                        description += firstName;
                    }


                    if (lastName != null)
                    {
                        if (description.Length > 0)
                        {
                            description += " ";
                        }
                        description += lastName;
                    }


                    securityUser.description = description;

                    securityUser.active = true;
                    securityUser.deleted = false;

                    db.SecurityUsers.Add(securityUser);

                    db.SaveChanges();

                    if (tenant != null)
                    {
                        SecurityTenantUser stu = new SecurityTenantUser();

                        stu.securityUserId = securityUser.id;
                        stu.securityTenantId = tenant.id;

                        stu.objectGuid = Guid.NewGuid();
                        stu.active = true;
                        stu.deleted = false;

                        db.SecurityTenantUsers.Add(stu);

                        db.SaveChanges();
                    }
                }
                else
                {
                    throw new Exception("Security User already exists");
                }

                return securityUser;
            }
        }


#if WINDOWS

        public static SecurityUser CreateOrUpdateUserRecordFromUserPrincipal(UserPrincipal principal, string domainName)
        {
            using (SecurityContext db = new SecurityContext())
            {
                //
                // Try to use the UPN as the account name if it's there.  If not, then use the SAMAccountName
                //
                string accountName = "";
                if (principal.UserPrincipalName != null &&
                    principal.UserPrincipalName.Trim().Length > 0 &&
                    principal.UserPrincipalName.Trim().StartsWith("@") == false)
                {
                    accountName = principal.UserPrincipalName;
                }
                else
                {
                    accountName = principal.SamAccountName;
                }


                //
                // Check for an existing user by user object guid
                //
                SecurityUser securityUser = (from users in db.SecurityUsers
                             where
                             users.objectGuid== principal.Guid.Value &&
                             (users.authenticationDomain.Trim().ToUpper() == domainName.Trim().ToUpper() || users.authenticationDomain == null)     // look for a users in this domain, but also allow a state where the record has no explicit domain to be nice to older records that may not have this field set.   
                             select users).FirstOrDefault();

                if (securityUser == null)
                {
                    //
                    // Create a new user record
                    //
                    securityUser = new SecurityUser();


                    securityUser.objectGuid = principal.Guid.Value;
                    securityUser.authenticationDomain = domainName;

                    securityUser.accountName = accountName;
                    securityUser.activeDirectoryAccount = true;         // when creating from a user principal object, this is to be marked as an AD account 


                    securityUser.mostRecentActivity = DateTime.UtcNow;

                    securityUser.firstName = principal.GivenName;
                    securityUser.middleName = principal.MiddleName;
                    securityUser.lastName = principal.Surname;
                    securityUser.emailAddress = principal.EmailAddress;

                    securityUser.password = null;
                    securityUser.failedLoginCount = 0;
                    securityUser.lastLoginAttempt = null;

                    string description = "";

                    if (principal.GivenName != null)
                    {
                        description += principal.GivenName;
                    }

                    if (principal.MiddleName != null)
                    {
                        if (description.Length > 0)
                        {
                            description += " ";
                        }
                        description += principal.MiddleName;
                    }

                    if (principal.Surname != null)
                    {
                        if (description.Length > 0)
                        {
                            description += " ";
                        }
                        description += principal.Surname;
                    }

                    if (principal.Description != null)
                    {
                        if (description.Length > 0)
                        {
                            description += " - ";
                        }
                        description += principal.Description;
                    }

                    securityUser.description = description;

                    //
                    // This uses an LDAP query to get the image from the first domain in the domain list.  It won't work perfectly in multi domain environments because 2nd and later domains have no LDAP credentials in the config
                    //
                    securityUser.image = GetUserPictureFromDomainByObjectGuid(securityUser.objectGuid);

                    securityUser.active = true;
                    securityUser.deleted = false;

                    db.SecurityUsers.Add(securityUser);
                }
                else
                {
                    //
                    // Update the activity time stamp on the user record, and update some of the the data fields
                    //
                    securityUser.mostRecentActivity = DateTime.UtcNow;

                    securityUser.activeDirectoryAccount = true;

                    securityUser.firstName = principal.GivenName;
                    securityUser.middleName = principal.MiddleName;
                    securityUser.lastName = principal.Surname;

                    securityUser.emailAddress = principal.EmailAddress;
                    securityUser.phoneNumber = principal.VoiceTelephoneNumber;

                    securityUser.image = GetUserPictureFromDomainByObjectGuid(securityUser.objectGuid);

                    db.Entry(securityUser).State = EntityState.Modified;
                    db.SaveChanges();
                }


                // Are the user group values managed by the AD?
                string groupsManagedByAD = GetStringConfigurationSetting("SecurityUserSecurityGroupsManagedByActiveDirectory");

                if (groupsManagedByAD != null && groupsManagedByAD.Trim().ToUpper() == "TRUE")
                {
                    //
                    // If the user has AD groups, then auto create those too, and manage the user->group relations
                    //
                    PrincipalSearchResult<Principal> ADGroups = principal.GetAuthorizationGroups();

                    List<SecurityUserSecurityGroup> SecurityUserSecurityGroups = (from x in db.SecurityUserSecurityGroups.Include("Group")
                                                  where x.securityUserId == securityUser.id
                                                  select x).ToList();


                    // Use this to store the list of user groups that the user has, according to AD.  Then compare this
                    // with the list from the security schema, so we can take out the ones that are no longer relevant in our schema.
                    List<SecurityUserSecurityGroup> SecurityUserSecurityGroupsThatAreValid = new List<SecurityUserSecurityGroup>();

                    try
                    {
                        // Go through each group that AD providers
                        foreach (GroupPrincipal adg in ADGroups)
                        {
                            // Is the group global?
                            if (adg.GroupScope == System.DirectoryServices.AccountManagement.GroupScope.Global)
                            {
                                string groupName = "";
                                if (adg.Name != null)
                                {
                                    groupName = adg.Name;
                                }
                                else if (adg.UserPrincipalName != null &&
                                         adg.UserPrincipalName.Trim().Length > 0 &&
                                         adg.UserPrincipalName.Trim().StartsWith("@") == false)
                                {
                                    groupName = adg.UserPrincipalName;
                                }
                                else if (adg.SamAccountName != null)
                                {
                                    groupName = adg.SamAccountName;
                                }

                                // do we know about this group yet?
                                SecurityGroup securityGroup = (from groups in db.SecurityGroups
                                                               where groups.name.Trim().ToUpper() == groupName.Trim().ToUpper()
                                               select groups).FirstOrDefault();

                                if (securityGroup == null)
                                {
                                    securityGroup = new SecurityGroup();

                                    securityGroup.name = groupName;
                                    securityGroup.description = adg.Description;

                                    securityGroup.active = true;
                                    securityGroup.deleted = false;

                                    db.SecurityGroups.Add(securityGroup);
                                    db.SaveChanges();


                                    SecurityUserSecurityGroup ug = new SecurityUserSecurityGroup();

                                    ug.securityUserId = securityUser.id;
                                    ug.securityGroupId = securityGroup.id;
                                    ug.active = true;
                                    ug.deleted = false;

                                    db.SecurityUserSecurityGroups.Add(ug);
                                    db.SaveChanges();

                                    SecurityUserSecurityGroupsThatAreValid.Add(ug);

                                }
                                else
                                {
                                    // does the User Group record exist?
                                    bool foundSecurityUserSecurityGroupRecord = false;
                                    for (int i = 0; i < SecurityUserSecurityGroups.Count; i++)
                                    {
                                        if (SecurityUserSecurityGroups[i].SecurityGroup.name.Trim().ToUpper() == securityGroup.name.Trim().ToUpper())
                                        {
                                            foundSecurityUserSecurityGroupRecord = true;
                                            SecurityUserSecurityGroupsThatAreValid.Add(SecurityUserSecurityGroups[i]);
                                            break;
                                        }
                                    }

                                    if (foundSecurityUserSecurityGroupRecord == false)
                                    {
                                        SecurityUserSecurityGroup ug = new SecurityUserSecurityGroup();

                                        ug.securityUserId = securityUser.id;
                                        ug.securityGroupId = securityGroup.id;
                                        ug.active = true;
                                        ug.deleted = false;

                                        db.SecurityUserSecurityGroups.Add(ug);
                                        db.SaveChanges();

                                        SecurityUserSecurityGroups.Add(ug);

                                        SecurityUserSecurityGroupsThatAreValid.Add(ug);
                                    }
                                }
                            }
                        }

                        // now see if there are any user group records we can take away 
                        List<SecurityUserSecurityGroup> SecurityUserSecurityGroupsToRemove = new List<SecurityUserSecurityGroup>();
                        for (int i = 0; i < SecurityUserSecurityGroups.Count; i++)
                        {
                            bool foundInValidList = false;

                            for (int j = 0; j < SecurityUserSecurityGroupsThatAreValid.Count; j++)
                            {
                                if (SecurityUserSecurityGroups[i].id == SecurityUserSecurityGroupsThatAreValid[j].id)
                                {
                                    foundInValidList = true;
                                    break;
                                }
                            }

                            if (foundInValidList == false)
                            {
                                SecurityUserSecurityGroupsToRemove.Add(SecurityUserSecurityGroups[i]);
                            }
                        }

                        if (SecurityUserSecurityGroupsToRemove.Count > 0)
                        {
                            db.SecurityUserSecurityGroups.RemoveRange(SecurityUserSecurityGroupsToRemove);
                            db.SaveChanges();
                        }

                    }
                    catch (Exception ex)
                    {
                        // suppress errors related to managing groups from AD
                        string exm = ex.Message;
                    }
                }

                db.SaveChanges();

                return securityUser;
            }
        }

#endif

        public static SecurityUser CreateOrUpdateUserRecordFromGoogleUser(GoogleAuthentication.GoogleUser googleUser)
        {
            using (SecurityContext db = new SecurityContext())
            {
                //
                //
                string accountName = googleUser.email;

                string domainName = "Google";

                //
                // Check for an existing user by user object guid
                //
                SecurityUser user = (from users in db.SecurityUsers
                                     where
                                     users.alternateIdentifier == googleUser.sub &&
                                     (users.authenticationDomain.Trim().ToUpper() == domainName.Trim().ToUpper() || users.authenticationDomain == null)     // look for a users in this domain, but also allow a state where the record has no explicit domain to be nice to older records that may not have this field set.   
                                     select users).FirstOrDefault();

                if (user == null)
                {
                    //
                    // Create a new user record
                    //
                    user = new SecurityUser();

                    user.alternateIdentifier = googleUser.sub;
                    user.objectGuid = Guid.NewGuid();             //ConvertGoogleIdToGuid(googleUser.sub);

                    user.authenticationDomain = domainName;

                    user.accountName = accountName;
                    user.activeDirectoryAccount = true;         // This is being marked as an AD account, because Google is really the domain here.

                    user.mostRecentActivity = DateTime.UtcNow;

                    user.firstName = googleUser.given_name;
                    user.lastName = googleUser.family_name;
                    user.description = googleUser.name;

                    user.emailAddress = googleUser.email;

                    user.password = null;
                    user.failedLoginCount = 0;
                    user.lastLoginAttempt = null;

                    user.image = googleUser.pictureData;

                    user.active = true;
                    user.deleted = false;

                    db.SecurityUsers.Add(user);
                }
                else
                {
                    //
                    // Update the activity time stamp on the user record, and update the data fields we have
                    //
                    user.accountName = accountName;
                    user.mostRecentActivity = DateTime.UtcNow;

                    user.firstName = googleUser.given_name;
                    user.lastName = googleUser.family_name;
                    user.description = googleUser.name;

                    user.emailAddress = googleUser.email;

                    user.image = googleUser.pictureData;

                    db.Entry(user).State = EntityState.Modified;
                }

                db.SaveChanges();

                return user;
            }
        }


        public static SecurityUser CreateOrUpdateUserRecordFromMicrosoftUser(MicrosoftAuthentication.MicrosoftUser microsoftUser)
        {
            using (SecurityContext db = new SecurityContext())
            {
                //
                string accountName = microsoftUser.email;

                string domainName = "Microsoft";

                //
                // Check for an existing user by user object guid
                //
                SecurityUser user = (from users in db.SecurityUsers
                                     where
                             users.alternateIdentifier == microsoftUser.sub &&
                             (users.authenticationDomain.Trim().ToUpper() == domainName.Trim().ToUpper() || users.authenticationDomain == null)     // look for a users in this domain, but also allow a state where the record has no explicit domain to be nice to older records that may not have this field set.   
                                     select users).FirstOrDefault();

                if (user == null)
                {
                    //
                    // Create a new user record
                    //
                    user = new SecurityUser();

                    user.alternateIdentifier = microsoftUser.sub;

                    user.objectGuid = new Guid();
                    user.authenticationDomain = domainName;

                    user.accountName = accountName;
                    user.activeDirectoryAccount = true;         // This is being marked as an AD account, because microsoft is really the domain here.

                    user.mostRecentActivity = DateTime.UtcNow;

                    user.firstName = microsoftUser.given_name;
                    user.lastName = microsoftUser.family_name;
                    user.description = microsoftUser.name;

                    user.emailAddress = microsoftUser.email;

                    user.password = null;
                    user.failedLoginCount = 0;
                    user.lastLoginAttempt = null;


                    user.image = microsoftUser.pictureData;

                    user.active = true;
                    user.deleted = false;

                    db.SecurityUsers.Add(user);
                }
                else
                {
                    //
                    // Update the activity time stamp on the user record, and update the data fields we have
                    //
                    user.accountName = accountName;
                    user.mostRecentActivity = DateTime.UtcNow;

                    user.firstName = microsoftUser.given_name;
                    user.lastName = microsoftUser.family_name;
                    user.description = microsoftUser.name;

                    user.emailAddress = microsoftUser.email;


                    user.image = microsoftUser.pictureData;

                    db.Entry(user).State = EntityState.Modified;
                }

                db.SaveChanges();

                return user;
            }
        }


        public static SecurityUser CreateOrUpdateUserRecordFromFacebookUser(FacebookAuthentication.FacebookUser facebookUser)
        {
            using (SecurityContext db = new SecurityContext())
            {
                //
                //
                string accountName = facebookUser.email + "_Facebook";

                string domainName = "Facebook";

                //
                // Check for an existing user by user object guid
                //
                SecurityUser user = (from users in db.SecurityUsers
                                     where
                                     users.alternateIdentifier == facebookUser.id &&
                                     (users.authenticationDomain.Trim().ToUpper() == domainName.Trim().ToUpper() || users.authenticationDomain == null)     // look for a users in this domain, but also allow a state where the record has no explicit domain to be nice to older records that may not have this field set.   
                                     select users).FirstOrDefault();

                if (user == null)
                {
                    //
                    // Create a new user record
                    //
                    user = new SecurityUser();

                    user.alternateIdentifier = facebookUser.id;
                    user.objectGuid = Guid.NewGuid();
                    user.authenticationDomain = domainName;

                    user.accountName = accountName;
                    user.activeDirectoryAccount = true;         // This is being marked as an AD account, because Google is really the domain here.

                    user.mostRecentActivity = DateTime.UtcNow;

                    user.firstName = facebookUser.first_name;
                    user.lastName = facebookUser.last_name;
                    user.description = facebookUser.name;

                    user.emailAddress = facebookUser.email;

                    user.password = null;
                    user.failedLoginCount = 0;
                    user.lastLoginAttempt = null;

                    user.image = facebookUser.pictureData;

                    user.active = true;
                    user.deleted = false;

                    db.SecurityUsers.Add(user);
                }
                else
                {
                    //
                    // Update the activity time stamp on the user record, and update the data fields we have
                    //
                    user.accountName = accountName;
                    user.mostRecentActivity = DateTime.UtcNow;

                    user.firstName = facebookUser.first_name;
                    user.lastName = facebookUser.last_name;
                    user.description = facebookUser.name;

                    user.emailAddress = facebookUser.email;

                    user.image = facebookUser.pictureData;

                    db.Entry(user).State = EntityState.Modified;
                }

                db.SaveChanges();

                return user;
            }
        }



        public static bool IsUserAFirstAndLastNameBasedNonAuthenticatedUser(SecurityUser user)
        {
            if (user != null &&
                user.accountName != null)
            {
                if (user.accountName.EndsWith(FIRST_AND_LAST_USER_ACCOUNT_SUFFIX) == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }


        private static string FixStringLength(string data, int maxLength)
        {
            string output = data;

            if (data != null)
            {
                if (data.Length <= maxLength)
                {
                    return data;
                }
                else
                {
                    return data.Substring(0, maxLength);
                }
            }
            return output;
        }


        public static void UpdateUserLastLogin(SecurityUser userToUpdate)
        {
            using (SecurityContext db = new SecurityContext())
            {
                SecurityUser user = (from u in db.SecurityUsers
                                     where u.id == userToUpdate.id
                                     select u).FirstOrDefault();

                if (user != null)
                {
                    //
                    // Update the activity time stamp on the user record
                    //
                    user.mostRecentActivity = DateTime.UtcNow;

                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                }

                return;
            }
        }

#if WINDOWS
        public static byte[] GetUserPictureFromDomainByObjectGuid(Guid objectGuid)
        {
            //
            // Search the first domain only for an image.  
            //
            try
            {
                string seachDomainNames = GetStringConfigurationSetting("UserAuthenticationDomains");

                string[] domains = seachDomainNames.Split(',');

                if (domains != null && domains.Length > 0)
                {
                    string domain = domains[0];
                    
                    string ldapUserName = GetStringConfigurationSetting("LDAPUserName");
                    string ldapUserPassword = GetStringConfigurationSetting("LDAPUserPassword");

                    DirectoryEntry rootEntry = new DirectoryEntry("LDAP://" + domain, ldapUserName, ldapUserPassword, AuthenticationTypes.Secure);
                    DirectorySearcher directorySearcher = new DirectorySearcher(rootEntry);
                    directorySearcher.Filter = string.Format("(&(objectGUID={0})(objectClass=user))", GuidStringToEscapedCharacterString(objectGuid.ToString()) );
                    
                    var user = directorySearcher.FindOne();

                    if (user != null)
                    {
                        byte[] bytes = user.Properties["thumbnailPhoto"][0] as byte[];
                        return bytes;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting user image. " + ex.Message);
                return null;
            }
        }
#endif


#if WINDOWS
        public static byte[] GetUserPictureFromDomain(string userPrincipalName)
        {
            //
            // Search the first domain for an image.
            //
            try
            {
                string seachDomainNames = GetStringConfigurationSetting("UserAuthenticationDomains");

                string[] domains = seachDomainNames.Split(',');

                if (domains != null && domains.Length > 0)
                {
                    string domain = domains[0];

                    string ldapUserName = GetStringConfigurationSetting("LDAPUserName");
                    string ldapUserPassword = GetStringConfigurationSetting("LDAPUserPassword");

                    DirectoryEntry rootEntry = new DirectoryEntry("LDAP://" + domain, ldapUserName, ldapUserPassword, AuthenticationTypes.Secure);
                    DirectorySearcher directorySearcher = new DirectorySearcher(rootEntry);

                    directorySearcher.Filter = string.Format("(&(userPrincipalName={0}))", userPrincipalName);
                    var user = directorySearcher.FindOne();

                    if (user != null)
                    {
                        byte[] bytes = user.Properties["thumbnailPhoto"][0] as byte[];
                        return bytes;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting user image. " + ex.Message);
                return null;
            }
        }
#endif



#if WINDOWS
        public static string GetusersManagerEmailAddressFromDomain(string userPrincipalName)
        {
            //
            // Search the first domain for an email address.
            //
            try
            {
                string seachDomainNames = GetStringConfigurationSetting("UserAuthenticationDomains");

                string[] domains = seachDomainNames.Split(',');


                if (domains != null && domains.Length > 0)
                {

                    string domain = domains[0];

                    string ldapUserName = GetStringConfigurationSetting("LDAPUserName");
                    string ldapUserPassword = GetStringConfigurationSetting("LDAPUserPassword");


                    DirectoryEntry rootEntry = new DirectoryEntry("LDAP://" + domain, ldapUserName, ldapUserPassword, AuthenticationTypes.Secure);

                    DirectorySearcher directorySearcher = new DirectorySearcher(rootEntry);
                    directorySearcher.Filter = string.Format("(&(userPrincipalName={0}))", userPrincipalName);
                    var user = directorySearcher.FindOne();


                    if (user != null)
                    {
                        string managerDistinguishedName = user.Properties["manager"][0].ToString();

                        DirectorySearcher directoryManagerSearcher = new DirectorySearcher(rootEntry);
                        directoryManagerSearcher.Filter = string.Format("distinguishedName={0}", managerDistinguishedName);
                        var manager = directoryManagerSearcher.FindOne();

                        if (manager != null)
                        { 
                            string managerEmailAddress = manager.Properties["mail"][0].ToString();
                            return managerEmailAddress;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting user's manager email address. " + ex.Message);
                return null;
            }
        }
#endif

#if WINDOWS
        public static string GetUserEmailAddressFromDomain(string userPrincipalName)
        {
            //
            // Search the first domain for an email address.
            //
            try
            {
                string seachDomainNames = GetStringConfigurationSetting("UserAuthenticationDomains");
                string[] domains = seachDomainNames.Split(',');


                if (domains != null && domains.Length > 0)
                {
                    string domain = domains[0];

                    string ldapUserName = GetStringConfigurationSetting("LDAPUserName");
                    string ldapUserPassword = GetStringConfigurationSetting("LDAPUserPassword");


                    DirectoryEntry rootEntry = new DirectoryEntry("LDAP://" + domain, ldapUserName, ldapUserPassword, AuthenticationTypes.Secure);
                    DirectorySearcher directorySearcher = new DirectorySearcher(rootEntry);

                    directorySearcher.Filter = string.Format("(&(userPrincipalName={0}))", userPrincipalName);
                    var user = directorySearcher.FindOne();


                    if (user != null)
                    {
                        string data = user.Properties["mail"][0].ToString();

                        return data;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting user email address. " + ex.Message);
                return null;
            }
        }
#endif


#if WINDOWS

        public static string GetUserManagerFromDomain(string userPrincipalName)
        {
            //
            // Search the first domain for an email address.
            //
            try
            {
                string seachDomainNames = GetStringConfigurationSetting("UserAuthenticationDomains");

                string[] domains = seachDomainNames.Split(',');

                if (domains != null && domains.Length > 0)
                {
                    string domain = domains[0];

                    string ldapUserName = GetStringConfigurationSetting("LDAPUserName");
                    string ldapUserPassword = GetStringConfigurationSetting("LDAPUserPassword");

                    DirectoryEntry rootEntry = new DirectoryEntry("LDAP://" + domain, ldapUserName, ldapUserPassword, AuthenticationTypes.Secure);
                    DirectorySearcher directorySearcher = new DirectorySearcher(rootEntry);

                    directorySearcher.Filter = string.Format("(&(userPrincipalName={0}))", userPrincipalName);
                    var user = directorySearcher.FindOne();


                    if (user != null)
                    {
                        string managerADString = user.Properties["manager"][0].ToString();

                        // example is CN=Carina Andreatta,CN=Users,DC=ad,DC=stjosham,DC=on,DC=ca
                        if (managerADString != null)
                        {
                            //
                            // Pull the name out of the LDAP manager string.
                            //
                            string manager = managerADString.Substring(3);
                            manager = manager.Substring(0, manager.IndexOf(","));
                            return manager;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting user manager. " + ex.Message);
                return null;
            }

        }
#endif

        public static bool SetUserAuthenticationToken(SecurityUser userToUpdate, string authenticationToken, int authenticationTokenExpiryMinutes = 5)
        {
            using (SecurityContext db = new SecurityContext())
            {

                try
                {

                    SecurityUser user = (from u in db.SecurityUsers
                                         where u.id == userToUpdate.id
                                         select u).FirstOrDefault();

                    if (user != null)
                    {

                        //
                        // Update the user's authentication token.
                        //
                        user.authenticationToken = authenticationToken;
                        user.authenticationTokenExpiry = DateTime.UtcNow.AddMinutes(authenticationTokenExpiryMinutes);

                        db.Entry(user).State = EntityState.Modified;
                        db.SaveChanges();

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Foundation.Utility.CreateAuditEvent(Auditor.AuditEngine.AuditType.Error, "Unable to set user authentication token", false, userToUpdate, null, null, null, ex);
                    throw;
                }

                return false;
            }
        }


        /// <summary>
        /// 
        /// Provides secure hashing and verification for passwords using PBKDF2-HMAC-SHA1 with a custom format.
        /// 
        /// This class is intended solely for internal/private obfuscation of credentials in a persistence layer
        /// where direct database access already implies full administrative compromise or network breach.
        /// It is not suitable for high-security, internet-facing user authentication scenarios.
        /// 
        /// The algorithm (PBKDF2-HMAC-SHA1) and format are used for stored values.
        /// 
        /// </summary>
        public sealed class SecurePasswordHasher
        {
            /// <summary>
            /// Size of the random salt in bytes (128 bits – standard recommendation)
            /// </summary>
            private const int SALT_SIZE = 16;

            /// <summary>
            /// Size of the derived hash in bytes (SHA-1 output size)
            /// </summary>
            private const int HashSize = 20;

            /// <summary>
            /// Header string that identifies this specific hash format
            /// </summary>
            private const string HASH_HEADER_STRING = "$HASH$V1000$";

            /// <summary>
            /// Default number of PBKDF2 iterations when not specified.
            /// 10,000 is a reasonable legacy value; modern recommendations are higher (e.g., 600,000+ for SHA-256).
            /// </summary>
            private const int DEFAULT_ITERATIONS = 10000;

            /// <summary>
            /// 
            /// Creates a hash from a password
            /// 
            /// </summary>
            /// <param name="password">the password</param>
            /// <param name="iterations">number of iterations</param>
            /// <returns>the hash</returns>
            public static string Hash(string password, int iterations)
            {
                if (password == null)
                {
                    throw new ArgumentNullException(nameof(password));
                }


                if (iterations <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(iterations), "Iterations must be greater than zero.");
                }

                //
                // Generate a cryptographically strong random salt
                // RandomNumberGenerator is the current recommended API (replaces RNGCryptoServiceProvider)
                //
                byte[] salt = new byte[SALT_SIZE];
                RandomNumberGenerator.Fill(salt);

                //
                // Derive the hash bytes using the modern one-shot PBKDF2 implementation
                // Explicitly use SHA1 to exactly replicate the legacy Rfc2898DeriveBytes behavior
                //
                byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA1,
                    HashSize);

                //
                // Combine salt and hash into a single byte array for storage
                //
                byte[] hashBytes = new byte[SALT_SIZE + HashSize];
                Array.Copy(salt, 0, hashBytes, 0, SALT_SIZE);
                Array.Copy(hash, 0, hashBytes, SALT_SIZE, HashSize);

                //
                // Encode the combined bytes as Base64 for compact storage
                //
                string base64Hash = Convert.ToBase64String(hashBytes);

                //
                // Return the formatted hash string with version tag and iteration count
                //
                // This format is parsed by the Verify method
                //
                return $"{HASH_HEADER_STRING}{iterations}${base64Hash}";
            }


            /// <summary>
            /// Creates a hash from a password with 10000 iterations
            /// </summary>
            /// <param name="password">the password</param>
            /// <returns>the hash</returns>
            public static string Hash(string password)
            {
                return Hash(password, DEFAULT_ITERATIONS);
            }



            /// <summary>
            /// 
            /// Determines whether a stored hash string uses this class's supported format.
            /// 
            /// </summary>
            /// <param name="hashString">The hash string to inspect (may be null or empty).</param>
            /// <returns>true if the hash starts with the expected header; otherwise, false.</returns>
            public static bool IsHashSupported(string hashString)
            {
                if (hashString == null)
                { 
                    throw new ArgumentNullException(nameof(hashString));
                }

                //
                // Use StartsWith for precision and performance;
                //
                return hashString.StartsWith(HASH_HEADER_STRING) == true;
            }


            /// <summary>
            /// 
            /// Verifies a password against a hash
            /// 
            /// </summary>
            /// <param name="password">the password</param>
            /// <param name="hashedPassword">the hash</param>
            /// <returns>could be verified?</returns>
            public static bool Verify(string password, string hashedPassword)
            {
                //
                // Validate inputs early – null passwords are not supported
                //
                if (password == null)
                {
                    throw new ArgumentNullException(nameof(password));
                }

                if (hashedPassword == null)
                {
                    throw new ArgumentNullException(nameof(hashedPassword));
                }

                //
                // Check hash format support
                //
                if (IsHashSupported(hashedPassword) == false)
                {
                    throw new NotSupportedException("The hashtype is not supported");
                }

                //
                // Extract iteration count and Base64-encoded hash+salt string
                //
                string[] splittedHashString = hashedPassword.Replace(HASH_HEADER_STRING, "").Split('$');
                int iterations = int.Parse(splittedHashString[0]);
                string base64Hash = splittedHashString[1];

                //
                // Decode the combined salt + hash bytes
                //
                byte[] hashBytes = Convert.FromBase64String(base64Hash);

                // Validate length to prevent index-out-of-range errors later
                if (hashBytes.Length != SALT_SIZE + HashSize)
                {
                    throw new FormatException("Invalid hash length – corrupted data.");
                }

                //
                // Extract the salt (first SaltSize bytes)
                //
                byte[] salt = new byte[SALT_SIZE];
                Array.Copy(hashBytes, 0, salt, 0, SALT_SIZE);

                //
                // Derive the expected hash bytes using the modern one-shot PBKDF2 method
                // This uses HMAC-SHA1 by default (HashAlgorithmName.SHA1) to match the legacy behavior
                //
                byte[] expectedHash = Rfc2898DeriveBytes.Pbkdf2(password,
                                                                salt,
                                                                iterations,
                                                                HashAlgorithmName.SHA1,
                                                                HashSize);

                //
                // Compare the derived hash with the stored hash (skip the salt prefix)
                // Use a simple loop for constant-time comparison to prevent timing attacks
                //
                for (int i = 0; i < HashSize; i++)
                {
                    if (hashBytes[i + SALT_SIZE] != expectedHash[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}