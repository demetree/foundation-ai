using Foundation.Auditor;
using Foundation.Controllers;
using Foundation.Security.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


namespace Foundation.Security.Controllers.WebAPI
{
    public partial class SecurityUsersController : SecureWebAPIController
    {
        private const int MAXIMUM_IMAGEFILE_SIZE = 2000000;       // 2,000,000 bytes - about 2 MB


        /// <summary>
        /// Request model for user image upload
        /// </summary>
        public class UserImageUploadRequest
        {
            public string ImageData { get; set; }
        }

        //
        // This overridden version nulls out the passwords, and removes the password field as a filter param.
        //
        // GET: api/Users
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/SecurityUsers")]
        public async Task<IActionResult> GetSecurityUsers(
            string accountName = null,
            int? activeDirectoryAccount = null,
            int? canLogin = null,
            int? mustChangePassword = null,
            string firstName = null,
            string middleName = null,
            string lastName = null,
            DateTime? dateOfBirth = null,
            string emailAddress = null,
            string cellPhoneNumber = null,
            string phoneNumber = null,
            string phoneExtension = null,
            string description = null,
            int? reportsToSecurityUserId = null,
            string authenticationDomain = null,
            int? failedLoginCount = null,
            DateTime? lastLoginAttempt = null,
            DateTime? mostRecentActivity = null,
            string alternateIdentifier = null,
            string settings = null,
            string authenticationToken = null,
            DateTime? authenticationTokenExpiry = null,
            string twoFactorToken = null,
            DateTime? twoFactorTokenExpiry = null,
            int? twoFactorSendByEmail = null,
            int? twoFactorSendBySMS = null,
            Guid? objectGuid = null,
            int? active = null,
            int? deleted = null,
            int? securityTenantId = null,
            int? securityOrganizationId = null,
            int? securityDepartmentId = null,
            int? securityTeamId = null,
            int? pageSize = null,
            int? pageNumber = null,
            bool includeRelations = true,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Unauthorized();
            }


            SecurityUser user = await GetSecurityUserAsync(cancellationToken);

            bool userIsWriter = await UserCanWriteAsync(user, 0, cancellationToken);
            bool userIsAdmin = await UserCanAdministerAsync(user, cancellationToken);

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
            if (dateOfBirth.HasValue == true && dateOfBirth.Value.Kind != DateTimeKind.Utc)
            {
                dateOfBirth = dateOfBirth.Value.ToUniversalTime();
            }

            if (lastLoginAttempt.HasValue == true && lastLoginAttempt.Value.Kind != DateTimeKind.Utc)
            {
                lastLoginAttempt = lastLoginAttempt.Value.ToUniversalTime();
            }

            if (mostRecentActivity.HasValue == true && mostRecentActivity.Value.Kind != DateTimeKind.Utc)
            {
                mostRecentActivity = mostRecentActivity.Value.ToUniversalTime();
            }

            if (authenticationTokenExpiry.HasValue == true && authenticationTokenExpiry.Value.Kind != DateTimeKind.Utc)
            {
                authenticationTokenExpiry = authenticationTokenExpiry.Value.ToUniversalTime();
            }

            if (twoFactorTokenExpiry.HasValue == true && twoFactorTokenExpiry.Value.Kind != DateTimeKind.Utc)
            {
                twoFactorTokenExpiry = twoFactorTokenExpiry.Value.ToUniversalTime();
            }


            var query = (from u in _context.SecurityUsers select u);
            if (accountName != null)
            {
                query = query.Where(u => u.accountName == accountName);
            }
            if (activeDirectoryAccount.HasValue == true)
            {
                query = query.Where(u => u.activeDirectoryAccount == false);
            }
            if (canLogin.HasValue == true)
            {
                query = query.Where(u => u.canLogin == (canLogin.Value == 1 ? true : false));
            }
            if (mustChangePassword.HasValue == true)
            {
                query = query.Where(u => u.mustChangePassword == (mustChangePassword.Value == 1 ? true : false));
            }
            if (firstName != null)
            {
                query = query.Where(u => u.firstName == firstName);
            }
            if (middleName != null)
            {
                query = query.Where(u => u.middleName == middleName);
            }
            if (lastName != null)
            {
                query = query.Where(u => u.lastName == lastName);
            }
            if (dateOfBirth.HasValue == true)
            {
                query = query.Where(u => u.dateOfBirth == dateOfBirth.Value);
            }
            if (emailAddress != null)
            {
                query = query.Where(u => u.emailAddress == emailAddress);
            }
            if (cellPhoneNumber != null)
            {
                query = query.Where(u => u.cellPhoneNumber == cellPhoneNumber);
            }
            if (phoneNumber != null)
            {
                query = query.Where(u => u.phoneNumber == phoneNumber);
            }
            if (phoneExtension != null)
            {
                query = query.Where(u => u.phoneExtension == phoneExtension);
            }
            if (description != null)
            {
                query = query.Where(u => u.description == description);
            }
            if (reportsToSecurityUserId.HasValue == true)
            {
                query = query.Where(u => u.reportsToSecurityUserId == reportsToSecurityUserId.Value);
            }
            if (authenticationDomain != null)
            {
                query = query.Where(u => u.authenticationDomain == authenticationDomain);
            }
            if (failedLoginCount.HasValue == true)
            {
                query = query.Where(u => u.failedLoginCount == failedLoginCount.Value);
            }
            if (lastLoginAttempt.HasValue == true)
            {
                query = query.Where(u => u.lastLoginAttempt == lastLoginAttempt.Value);
            }
            if (mostRecentActivity.HasValue == true)
            {
                query = query.Where(u => u.mostRecentActivity == mostRecentActivity.Value);
            }
            if (alternateIdentifier != null)
            {
                query = query.Where(u => u.alternateIdentifier == alternateIdentifier);
            }
            if (settings != null)
            {
                query = query.Where(u => u.settings == settings);
            }
            if (authenticationToken != null)
            {
                query = query.Where(u => u.authenticationToken == authenticationToken);
            }
            if (authenticationTokenExpiry.HasValue == true)
            {
                query = query.Where(u => u.authenticationTokenExpiry == authenticationTokenExpiry.Value);
            }
            if (twoFactorToken != null)
            {
                query = query.Where(u => u.twoFactorToken == twoFactorToken);
            }
            if (twoFactorTokenExpiry.HasValue == true)
            {
                query = query.Where(u => u.twoFactorTokenExpiry == twoFactorTokenExpiry.Value);
            }
            if (twoFactorSendByEmail.HasValue == true)
            {
                query = query.Where(u => u.twoFactorSendByEmail == false);
            }
            if (twoFactorSendBySMS.HasValue == true)
            {
                query = query.Where(u => u.twoFactorSendBySMS == false);
            }
            if (objectGuid != null)
            {
                query = query.Where(u => u.objectGuid == objectGuid);
            }
            if (userIsWriter == true)
            {
                if (active.HasValue == true)
                {
                    query = query.Where(u => u.active == (active.Value == 1 ? true : false));
                }

                if (userIsAdmin == true)
                {
                    if (deleted.HasValue == true)
                    {
                        query = query.Where(u => u.deleted == (deleted.Value == 1 ? true : false));
                    }
                }
                else
                {
                    query = query.Where(u => u.deleted == false);
                }
            }
            else
            {
                query = query.Where(u => u.active == true);
                query = query.Where(u => u.deleted == false);
            }
            if (securityTenantId.HasValue == true)
            {
                query = query.Where(u => u.securityTenantId == securityTenantId.Value);
            }
            if (securityOrganizationId.HasValue == true)
            {
                query = query.Where(u => u.securityOrganizationId == securityOrganizationId.Value);
            }
            if (securityDepartmentId.HasValue == true)
            {
                query = query.Where(u => u.securityDepartmentId == securityDepartmentId.Value);
            }
            if (securityTeamId.HasValue == true)
            {
                query = query.Where(u => u.securityTeamId == securityTeamId.Value);
            }

            query = query.OrderBy(u => u.id);

            if (pageNumber.HasValue == true &&
                pageSize.HasValue == true)
            {
                query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            if (includeRelations == true)
            {
                query = query.Include(x => x.reportsToSecurityUser);
                query = query.Include(x => x.securityTenant);
                query = query.Include(x => x.securityDepartment);
                query = query.Include(x => x.securityOrganization);
                query = query.Include(x => x.securityTeam);
            }


            query = query.AsNoTracking();

            var materialized = await query.ToListAsync(cancellationToken);

            /* this is the start of the custom bit */

            //
            // Null out the passwords for the user, and any reports to user that is linked for the list getter for the users.
            // 
            for (int i = 0; i < materialized.Count; i++)
            {
                materialized[i].password = null;

                if (materialized[i].reportsToSecurityUser != null)
                {
                    materialized[i].reportsToSecurityUser.password = null;
                }

                materialized[i].authenticationToken = null;
                materialized[i].authenticationTokenExpiry = null;
                materialized[i].twoFactorToken = null;

            }

            /* this is the end of the custom bit */


            // Convert all the date properties to be of kind UTC.
            bool databaseStoresDateWithTimeZone = DoesDatabaseStoreDateWithTimeZone(_context);
            foreach (Database.SecurityUser securityUser in materialized)
            {
                Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(securityUser, databaseStoresDateWithTimeZone);
            }

            await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Security.User Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Security.User Entity list was read.  Returning " + materialized.Count + " rows of data.");

            // Create a new output object that only includes the relations if necessary, and doesn't include the emmpty list objects, so that we can reduce the amount of data being transferred.
            if (includeRelations == true)
            {
                var reducedFieldOutput = (from materializedData in materialized
                                          select SecurityUser.CreateAnonymousWithFirstLevelSubObjects(materializedData)
                                          ).ToList();

                return Ok(reducedFieldOutput);
            }
            else
            {
                var reducedFieldOutput = (from materializedData in materialized
                                          select SecurityUser.CreateAnonymous(materializedData)
                                          ).ToList();

                return Ok(reducedFieldOutput);
            }
        }


        //
        // Just adds password clearing.  Otherwise it's the same as auto generated.
        //
        [HttpGet]
        [Route("api/SecurityUser/{id}")]
        [Route("api/SecurityUser")]
        public async Task<IActionResult> GetSecurityUser(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Unauthorized();
            }


            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

            bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
            bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

            try
            {
                var query = (from u in _context.SecurityUsers
                             where
                            (u.id == id) &&
                            (userIsAdmin == true || u.deleted == false) &&
                            (userIsWriter == true || u.active == true)
                             select u);

                if (includeRelations == true)
                {
                    query = query.Include(x => x.reportsToSecurityUser);
                    query = query.Include(x => x.securityTenant);
                    query = query.Include(x => x.securityDepartment);
                    query = query.Include(x => x.securityOrganization);
                    query = query.Include(x => x.securityTeam);
                }

                SecurityUser materialized = await query.FirstOrDefaultAsync(cancellationToken);

                if (materialized != null)
                {
                    //
                    // Dates are stored as UTC in the database, but EF reads them as being of kind 'Unspecified'.  The problem with this is that they seralize to JSON without a 'Z' UTC indicator.
                    // To fix this, change the kind of all of the the date objects to be UTC.
                    //
                    if (materialized.dateOfBirth.HasValue == true && materialized.dateOfBirth.Value.Kind != DateTimeKind.Utc)
                    {
                        materialized.dateOfBirth = DateTime.SpecifyKind(materialized.dateOfBirth.Value, DateTimeKind.Utc);
                    }

                    if (materialized.lastLoginAttempt.HasValue == true && materialized.lastLoginAttempt.Value.Kind != DateTimeKind.Utc)
                    {
                        materialized.lastLoginAttempt = DateTime.SpecifyKind(materialized.lastLoginAttempt.Value, DateTimeKind.Utc);
                    }

                    if (materialized.mostRecentActivity.HasValue == true && materialized.mostRecentActivity.Value.Kind != DateTimeKind.Utc)
                    {
                        materialized.mostRecentActivity = DateTime.SpecifyKind(materialized.mostRecentActivity.Value, DateTimeKind.Utc);
                    }

                    if (materialized.authenticationTokenExpiry.HasValue == true && materialized.authenticationTokenExpiry.Value.Kind != DateTimeKind.Utc)
                    {
                        materialized.authenticationTokenExpiry = DateTime.SpecifyKind(materialized.authenticationTokenExpiry.Value, DateTimeKind.Utc);
                    }

                    if (materialized.twoFactorTokenExpiry.HasValue == true && materialized.twoFactorTokenExpiry.Value.Kind != DateTimeKind.Utc)
                    {
                        materialized.twoFactorTokenExpiry = DateTime.SpecifyKind(materialized.twoFactorTokenExpiry.Value, DateTimeKind.Utc);
                    }



                    /* start of custom bit*/
                    // null the password for the user and the person that they report to before before sending out the response.
                    materialized.password = null;

                    if (materialized.reportsToSecurityUser != null)
                    {
                        materialized.reportsToSecurityUser.password = null;
                    }
                    /* end of custom bit */


                    // Convert all the date properties to be of kind UTC.
                    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, DoesDatabaseStoreDateWithTimeZone(_context));

                    await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Security.User Entity was read with Admin privilege." : "Security.User Entity was read.");

                    if (includeRelations == true)
                    {
                        return Ok(SecurityUser.CreateAnonymousWithFirstLevelSubObjects(materialized));
                    }
                    else
                    {
                        return Ok(SecurityUser.CreateAnonymous(materialized));
                    }
                }
                else
                {
                    await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.Error, "Attempt to read a Security.User entity that does not exist.", id.ToString());
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(Auditor.AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Security.User.   Entity was read with Admin privilege." : "Exception caught during entity read of Security.User.", id.ToString(), ex);
                return Problem(ex.ToString());
            }
        }


        //
        // This custom version does some password checking, hashes it, and manages the most recent activity field.
        //
        // PUT: api/Users/1
        [HttpPut]
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/SecurityUser/{id}")]
        public async Task<IActionResult> PutSecurityUser(int id, [FromBody] Database.SecurityUser.SecurityUserDTO securityUserDTO, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Unauthorized();
            }

            if (id != securityUserDTO.id)
            {
                return BadRequest();
            }


            SecurityUser user = await GetSecurityUserAsync(cancellationToken);

            bool userIsWriter = await UserCanWriteAsync(user, 0, cancellationToken);
            bool userIsAdmin = await UserCanAdministerAsync(user, cancellationToken);

            SecurityUser existingSecurityUser = await (from x in _context.SecurityUsers where x.id == id select x).FirstOrDefaultAsync(cancellationToken);


            //
            // Custom declaration
            //
            SecurityUser securityUserToReturn = null;
            //end custom

            if (existingSecurityUser == null)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.User PUT", id.ToString(), new Exception("No Security.User entity could be find with the primary key provided."));
                return NotFound();
            }

            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (securityUserDTO.objectGuid == Guid.Empty)
            {
                securityUserDTO.objectGuid = existingSecurityUser.objectGuid;
            }
            else if (securityUserDTO.objectGuid != existingSecurityUser.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a SecurityUser record.  This is not allowed.  The User is " + user.accountName, existingSecurityUser.id.ToString());
                return Problem("Invalid Operation.");
            }



            /* start of custom bit */
            //
            // if a non null password is provided, then use it.  Otherwise, if null is provided, then we assume that the current password should be kept.
            //
            // Note that some of the the Angular DTOs for security have no password field, so this field coming in as null is expected and by design,
            // and we can't clobber the password value with null because it comes in that way..
            //
            if (securityUserDTO.password != null && securityUserDTO.password.Trim().Length > 0)
            {
                //
                // A password was provided.  Secure it and update the object.
                //
                try
                {
                    ValidatePasswordStrength(securityUserDTO.password);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }

                //
                // Has he password and put it into the DTO
                //
                securityUserDTO.password = SecurityLogic.SecurePasswordHasher.Hash(securityUserDTO.password);
            }
            else
            {
                //
                // No password provided, so keep the current one  (note this is a hashed value)
                //
                securityUserDTO.password = existingSecurityUser.password;
            }


            SecurityUser cloneOfExisting = (SecurityUser)_context.Entry(existingSecurityUser).GetDatabaseValues().ToObject();

            //
            // Create a new SecurityUser object using the data from the existing record, updated with what is in the DTO.
            //
            Database.SecurityUser securityUser = (Database.SecurityUser)_context.Entry(existingSecurityUser).GetDatabaseValues().ToObject();
            securityUser.ApplyDTO(securityUserDTO);


            // Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
            if (userIsAdmin == false && (securityUser.deleted == true || existingSecurityUser.deleted == true))
            {
                // we're not recording state here because it is not being changed.
                CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Security.SecurityUser record.", id.ToString());
                DestroySessionAndAuthentication();
                return Unauthorized();
            }


            //
            // Limit the size of the images
            //
            if (securityUser.image != null && securityUser.image.Length > MAXIMUM_IMAGEFILE_SIZE)
            {
                return BadRequest("Image size exceeds maximum size allowed value of " + MAXIMUM_IMAGEFILE_SIZE);
            }

            //
            // Confirm that the reports to hierarchy makes sense
            //
            ValidateReportsTo(securityUser);


            /* end of custom bit */

            if (securityUser.accountName != null && securityUser.accountName.Length > 250)
            {
                securityUser.accountName = securityUser.accountName.Substring(0, 250);
            }

            if (securityUser.password != null && securityUser.password.Length > 250)
            {
                securityUser.password = securityUser.password.Substring(0, 250);
            }

            if (securityUser.firstName != null && securityUser.firstName.Length > 100)
            {
                securityUser.firstName = securityUser.firstName.Substring(0, 100);
            }

            if (securityUser.middleName != null && securityUser.middleName.Length > 100)
            {
                securityUser.middleName = securityUser.middleName.Substring(0, 100);
            }

            if (securityUser.lastName != null && securityUser.lastName.Length > 100)
            {
                securityUser.lastName = securityUser.lastName.Substring(0, 100);
            }

            if (securityUser.dateOfBirth.HasValue == true && securityUser.dateOfBirth.Value.Kind != DateTimeKind.Utc)
            {
                securityUser.dateOfBirth = securityUser.dateOfBirth.Value.ToUniversalTime();
            }

            if (securityUser.emailAddress != null && securityUser.emailAddress.Length > 100)
            {
                securityUser.emailAddress = securityUser.emailAddress.Substring(0, 100);
            }

            if (securityUser.cellPhoneNumber != null && securityUser.cellPhoneNumber.Length > 100)
            {
                securityUser.cellPhoneNumber = securityUser.cellPhoneNumber.Substring(0, 100);
            }

            if (securityUser.phoneNumber != null && securityUser.phoneNumber.Length > 50)
            {
                securityUser.phoneNumber = securityUser.phoneNumber.Substring(0, 50);
            }

            if (securityUser.phoneExtension != null && securityUser.phoneExtension.Length > 50)
            {
                securityUser.phoneExtension = securityUser.phoneExtension.Substring(0, 50);
            }

            if (securityUser.description != null && securityUser.description.Length > 500)
            {
                securityUser.description = securityUser.description.Substring(0, 500);
            }

            if (securityUser.authenticationDomain != null && securityUser.authenticationDomain.Length > 100)
            {
                securityUser.authenticationDomain = securityUser.authenticationDomain.Substring(0, 100);
            }

            if (securityUser.lastLoginAttempt.HasValue == true && securityUser.lastLoginAttempt.Value.Kind != DateTimeKind.Utc)
            {
                securityUser.lastLoginAttempt = securityUser.lastLoginAttempt.Value.ToUniversalTime();
            }

            if (securityUser.mostRecentActivity.HasValue == true && securityUser.mostRecentActivity.Value.Kind != DateTimeKind.Utc)
            {
                securityUser.mostRecentActivity = securityUser.mostRecentActivity.Value.ToUniversalTime();
            }

            if (securityUser.alternateIdentifier != null && securityUser.alternateIdentifier.Length > 100)
            {
                securityUser.alternateIdentifier = securityUser.alternateIdentifier.Substring(0, 100);
            }

            if (securityUser.authenticationToken != null && securityUser.authenticationToken.Length > 100)
            {
                securityUser.authenticationToken = securityUser.authenticationToken.Substring(0, 100);
            }

            if (securityUser.authenticationTokenExpiry.HasValue == true && securityUser.authenticationTokenExpiry.Value.Kind != DateTimeKind.Utc)
            {
                securityUser.authenticationTokenExpiry = securityUser.authenticationTokenExpiry.Value.ToUniversalTime();
            }

            if (securityUser.twoFactorToken != null && securityUser.twoFactorToken.Length > 10)
            {
                securityUser.twoFactorToken = securityUser.twoFactorToken.Substring(0, 10);
            }

            if (securityUser.twoFactorTokenExpiry.HasValue == true && securityUser.twoFactorTokenExpiry.Value.Kind != DateTimeKind.Utc)
            {
                securityUser.twoFactorTokenExpiry = securityUser.twoFactorTokenExpiry.Value.ToUniversalTime();
            }


            //
            // Custom bit here to validate the data visibility defaults for this user.  Make sure that they make sense in terms of hierarchy integrity.
            //
            try
            {
                await ValidateDataVisibilityDefaultsForUserAsync(securityUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            //
            // End of data visibility checking custom bit.
            //


            var attached = _context.Entry(existingSecurityUser);
            attached.CurrentValues.SetValues(securityUser);

            try
            {

                //
                // Write the changes to the security user 
                //
                await _context.SaveChangesAsync(cancellationToken);

                //
                // More custom here - nullify the image field so that it doesn't get serialized in the AuditEventEntityState table because large images exceed the serializer max size, and it's not absolutely necessary to save the images.
                //
                cloneOfExisting.image = null;
                securityUser.image = null;


                //
                // Blow away the related fields for serialization, especially the reports to user which could make a circular reference bad data is in the database.
                //
                securityUser.reportsToSecurityUser = null;
                securityUser.securityTeam = null;
                securityUser.securityDepartment = null;
                securityUser.securityOrganization = null;

                //
                // Custom bit ends here
                //

                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                    "Security.SecurityUser entity successfully updated.",
                    true,
                    id.ToString(),
                    JsonSerializer.Serialize(Database.SecurityUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                    JsonSerializer.Serialize(Database.SecurityUser.CreateAnonymousWithFirstLevelSubObjects(securityUser)),
                    null);


                // start of custom bit
                //
                // Clear the security caches after modifying users
                //
                SecurityFramework.ClearSecurityCaches();
                SecurityLogic.ClearSecurityCaches();

                //
                // If user has any values in any of the tenant, organization, department or team fields, make sure that there are matching counterparts in the associated user mapping tables
                //
                await CreateOrUpdateUserDataVisibilityTablesAsync(securityUser, cancellationToken);


                //
                // Because we removed some data for the serializing, load the record again and clobber the password for the return object
                //
                securityUserToReturn = await (from x in _context.SecurityUsers where x.id == id select x)
                                            .AsNoTracking()         // This is key!  we do not want any changes made to this to be saved - specifically the password nullification
                                            .FirstOrDefaultAsync(cancellationToken);
                securityUserToReturn.password = null;


                //
                // Create a SecurityUserEvent to track this user modification
                //
                string changeSummary = GenerateUserChangeSummary(cloneOfExisting, securityUser);
                await CreateSecurityUserEventAsync(
                    securityUser.id,
                    "User Modified",
                    $"User '{securityUser.accountName}' modified by '{user?.accountName ?? "Unknown"}'. Changes: {changeSummary}",
                    cancellationToken);


                // end of custom bit 
                return Ok(Database.SecurityUser.CreateAnonymous(securityUserToReturn));

            }
            catch (Exception ex)
            {
                CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
                    "Security.SecurityUser entity update failed",
                    false,
                    id.ToString(),
                    JsonSerializer.Serialize(Database.SecurityUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                    JsonSerializer.Serialize(Database.SecurityUser.CreateAnonymousWithFirstLevelSubObjects(securityUser)),
                    ex);

                return Problem(ex.Message);
            }

        }


        private async Task<bool> ValidateDataVisibilityDefaultsForUserAsync(SecurityUser securityUser)
        {
            //
            // Starting at the user's team work upwards to make sure that the hierarchy in place makes sense.  Complain by throwing an error if it does not.
            //
            if (securityUser == null)
            {
                throw new Exception("No user provided to validate data visibility settings for.");
            }


            //
            // If user has a team, make sure that it belongs to the department default that they have.  If there is no department default, then set it based on the team.
            // 
            if (securityUser.securityTeamId.HasValue == true)
            {
                SecurityTeam st = await (from x in _context.SecurityTeams where x.id == securityUser.securityTeamId.Value select x).FirstAsync();

                if (securityUser.securityDepartmentId.HasValue == false)
                {
                    securityUser.securityDepartmentId = st.securityDepartmentId;
                }
                else if (securityUser.securityDepartmentId.Value != st.securityDepartmentId)
                {
                    throw new Exception("Data visibility defaults mismatch.  Team does not belong to department.  User team is " + st.name);
                }
            }


            //
            // If user has a default department, make sure that it belongs to the organization default that they have.  If there is organization default, then set it based on the department.
            //
            if (securityUser.securityDepartmentId.HasValue == true)
            {
                SecurityDepartment sd = await (from x in _context.SecurityDepartments where x.id == securityUser.securityDepartmentId.Value select x).FirstAsync();

                if (securityUser.securityOrganizationId.HasValue == false)
                {
                    securityUser.securityOrganizationId = sd.securityOrganizationId;
                }
                else if (securityUser.securityOrganizationId.Value != sd.securityOrganizationId)
                {
                    throw new Exception("Data visibility defaults mismatch.  Department does not belong to organization.  User department is " + sd.name);
                }
            }

            return true;
        }

        private void ValidateDataVisibilityDefaultsForUser(SecurityUser securityUser)
        {
            //
            // Starting at the user's team work upwards to make sure that the hierarchy in place makes sense.  Complain by throwing an error if it does not.
            //
            if (securityUser == null)
            {
                throw new Exception("No user provided to validate data visibility settings for.");
            }


            //
            // If user has a team, make sure that it belongs to the department default that they have.  If there is no department default, then set it based on the team.
            // 
            if (securityUser.securityTeamId.HasValue == true)
            {
                SecurityTeam st = (from x in _context.SecurityTeams where x.id == securityUser.securityTeamId.Value select x).First();

                if (securityUser.securityDepartmentId.HasValue == false)
                {
                    securityUser.securityDepartmentId = st.securityDepartmentId;
                }
                else if (securityUser.securityDepartmentId.Value != st.securityDepartmentId)
                {
                    throw new Exception("Data visibility defaults mismatch.  Team does not belong to department.  User team is " + st.name);
                }
            }


            //
            // If user has a default department, make sure that it belongs to the organization default that they have.  If there is organization default, then set it based on the department.
            //
            if (securityUser.securityDepartmentId.HasValue == true)
            {
                SecurityDepartment sd = (from x in _context.SecurityDepartments where x.id == securityUser.securityDepartmentId.Value select x).First();

                if (securityUser.securityOrganizationId.HasValue == false)
                {
                    securityUser.securityOrganizationId = sd.securityOrganizationId;
                }
                else if (securityUser.securityOrganizationId.Value != sd.securityOrganizationId)
                {
                    throw new Exception("Data visibility defaults mismatch.  Department does not belong to organization.  User department is " + sd.name);
                }
            }

            return;
        }

        private async Task<bool> CreateOrUpdateUserDataVisibilityTablesAsync(SecurityUser securityUser, CancellationToken cancellationToken = default)
        {
            if (securityUser.securityTenantId.HasValue == true)
            {
                SecurityTenantUser stu = await (from x in _context.SecurityTenantUsers
                                                where
                                                x.securityUserId == securityUser.id &&
                                                x.securityTenantId == securityUser.securityTenantId.Value
                                                select x).FirstOrDefaultAsync(cancellationToken);

                if (stu == null)
                {
                    stu = new SecurityTenantUser();

                    stu.securityUserId = securityUser.id;
                    stu.securityTenantId = securityUser.securityTenantId.Value;
                    stu.objectGuid = Guid.NewGuid();

                    stu.active = true;
                    stu.deleted = false;
                    _context.SecurityTenantUsers.Add(stu);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    if (stu.active == false || stu.deleted == true)
                    {
                        stu.active = true;
                        stu.deleted = false;

                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }
            }


            if (securityUser.securityOrganizationId.HasValue == true)
            {
                SecurityOrganizationUser sou = await (from x in _context.SecurityOrganizationUsers
                                                      where
                                                      x.securityUserId == securityUser.id &&
                                                      x.securityOrganizationId == securityUser.securityOrganizationId.Value
                                                      select x).FirstOrDefaultAsync(cancellationToken);

                if (sou == null)
                {
                    sou = new SecurityOrganizationUser();

                    sou.securityUserId = securityUser.id;
                    sou.securityOrganizationId = securityUser.securityOrganizationId.Value;
                    sou.objectGuid = Guid.NewGuid();

                    //
                    // Default to being able to read and write
                    //
                    sou.canRead = true;
                    sou.canWrite = true;

                    sou.active = true;
                    sou.deleted = false;
                    _context.SecurityOrganizationUsers.Add(sou);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    if (sou.active == false || sou.deleted == true)
                    {
                        sou.active = true;
                        sou.deleted = false;

                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }
            }


            if (securityUser.securityDepartmentId.HasValue == true)
            {
                SecurityDepartmentUser sdu = await (from x in _context.SecurityDepartmentUsers
                                                    where
                                                    x.securityUserId == securityUser.id &&
                                                    x.securityDepartmentId == securityUser.securityDepartmentId.Value
                                                    select x).FirstOrDefaultAsync(cancellationToken);

                if (sdu == null)
                {
                    sdu = new SecurityDepartmentUser();

                    sdu.securityUserId = securityUser.id;
                    sdu.securityDepartmentId = securityUser.securityDepartmentId.Value;
                    sdu.objectGuid = Guid.NewGuid();

                    //
                    // Default to being able to read and write
                    //
                    sdu.canRead = true;
                    sdu.canWrite = true;


                    sdu.active = true;
                    sdu.deleted = false;
                    _context.SecurityDepartmentUsers.Add(sdu);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    if (sdu.active == false || sdu.deleted == true)
                    {
                        sdu.active = true;
                        sdu.deleted = false;

                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }
            }

            if (securityUser.securityTeamId.HasValue == true)
            {
                SecurityTeamUser stu = await (from x in _context.SecurityTeamUsers
                                              where
                                              x.securityUserId == securityUser.id &&
                                              x.securityTeamId == securityUser.securityTeamId.Value
                                              select x)
                                        .FirstOrDefaultAsync(cancellationToken);

                if (stu == null)
                {
                    stu = new SecurityTeamUser();

                    stu.securityUserId = securityUser.id;
                    stu.securityTeamId = securityUser.securityTeamId.Value;
                    stu.objectGuid = Guid.NewGuid();

                    //
                    // Default to being able to read and write
                    //
                    stu.canRead = true;
                    stu.canWrite = true;

                    stu.active = true;
                    stu.deleted = false;
                    _context.SecurityTeamUsers.Add(stu);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    if (stu.active == false || stu.deleted == true)
                    {
                        stu.active = true;
                        stu.deleted = false;

                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }
            }

            return true;
        }




        private void CreateOrUpdateUserDataVisibilityTables(SecurityUser securityUser)
        {
            if (securityUser.securityTenantId.HasValue == true)
            {
                SecurityTenantUser stu = (from x in _context.SecurityTenantUsers
                                          where
                                          x.securityUserId == securityUser.id &&
                                          x.securityTenantId == securityUser.securityTenantId.Value
                                          select x).FirstOrDefault();

                if (stu == null)
                {
                    stu = new SecurityTenantUser();

                    stu.securityUserId = securityUser.id;
                    stu.securityTenantId = securityUser.securityTenantId.Value;
                    stu.objectGuid = Guid.NewGuid();

                    stu.active = true;
                    stu.deleted = false;
                    _context.SecurityTenantUsers.Add(stu);
                    _context.SaveChanges();
                }
                else
                {
                    if (stu.active == false || stu.deleted == true)
                    {
                        stu.active = true;
                        stu.deleted = false;

                        _context.SaveChanges();
                    }
                }
            }


            if (securityUser.securityOrganizationId.HasValue == true)
            {
                SecurityOrganizationUser sou = (from x in _context.SecurityOrganizationUsers
                                                where
                                                x.securityUserId == securityUser.id &&
                                                x.securityOrganizationId == securityUser.securityOrganizationId.Value
                                                select x).FirstOrDefault();

                if (sou == null)
                {
                    sou = new SecurityOrganizationUser();

                    sou.securityUserId = securityUser.id;
                    sou.securityOrganizationId = securityUser.securityOrganizationId.Value;
                    sou.objectGuid = Guid.NewGuid();

                    //
                    // Default to being able to read and write
                    //
                    sou.canRead = true;
                    sou.canWrite = true;

                    sou.active = true;
                    sou.deleted = false;
                    _context.SecurityOrganizationUsers.Add(sou);
                    _context.SaveChanges();
                }
                else
                {
                    if (sou.active == false || sou.deleted == true)
                    {
                        sou.active = true;
                        sou.deleted = false;

                        _context.SaveChanges();
                    }
                }
            }


            if (securityUser.securityDepartmentId.HasValue == true)
            {
                SecurityDepartmentUser sdu = (from x in _context.SecurityDepartmentUsers
                                              where
                                              x.securityUserId == securityUser.id &&
                                              x.securityDepartmentId == securityUser.securityDepartmentId.Value
                                              select x).FirstOrDefault();

                if (sdu == null)
                {
                    sdu = new SecurityDepartmentUser();

                    sdu.securityUserId = securityUser.id;
                    sdu.securityDepartmentId = securityUser.securityDepartmentId.Value;
                    sdu.objectGuid = Guid.NewGuid();

                    //
                    // Default to being able to read and write
                    //
                    sdu.canRead = true;
                    sdu.canWrite = true;


                    sdu.active = true;
                    sdu.deleted = false;
                    _context.SecurityDepartmentUsers.Add(sdu);
                    _context.SaveChanges();
                }
                else
                {
                    if (sdu.active == false || sdu.deleted == true)
                    {
                        sdu.active = true;
                        sdu.deleted = false;

                        _context.SaveChanges();
                    }
                }
            }

            if (securityUser.securityTeamId.HasValue == true)
            {
                SecurityTeamUser stu = (from x in _context.SecurityTeamUsers
                                        where
                                        x.securityUserId == securityUser.id &&
                                        x.securityTeamId == securityUser.securityTeamId.Value
                                        select x).FirstOrDefault();

                if (stu == null)
                {
                    stu = new SecurityTeamUser();

                    stu.securityUserId = securityUser.id;
                    stu.securityTeamId = securityUser.securityTeamId.Value;
                    stu.objectGuid = Guid.NewGuid();

                    //
                    // Default to being able to read and write
                    //
                    stu.canRead = true;
                    stu.canWrite = true;

                    stu.active = true;
                    stu.deleted = false;
                    _context.SecurityTeamUsers.Add(stu);
                    _context.SaveChanges();
                }
                else
                {
                    if (stu.active == false || stu.deleted == true)
                    {
                        stu.active = true;
                        stu.deleted = false;

                        _context.SaveChanges();
                    }
                }
            }
        }


        //
        // This custom version does some password checking and hashing and manages the most recent activity field
        //
        // POST: api/User
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/SecurityUser", Name = "SecurityUser")]
        public async Task<IActionResult> PostSecurityUser([FromBody] Database.SecurityUser.SecurityUserDTO securityUserDTO, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Unauthorized();
            }


            //
            // Create a new SecurityUser object using the data from the DTO
            //
            Database.SecurityUser securityUser = Database.SecurityUser.FromDTO(securityUserDTO);


            //
            // Custom declaration
            //
            SecurityUser securityUserToReturn = null;
            //end custom

            try
            {
                if (securityUser.activeDirectoryAccount == false && (securityUser.password == null || securityUser.password.Trim().Length == 0))
                {
                    throw new Exception("Local user cannot be created without a password.");
                }

                /* custom bit starts here */
                //
                // if a non null password is provided, then use it.  Otherwise, if null is provided, then we assume that the current password should be kept.
                //
                if (securityUser.password != null && securityUser.password.Trim().Length > 0)
                {
                    try
                    {
                        ValidatePasswordStrength(securityUser.password);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex.Message);
                    }

                    //
                    // A password was provided.  Secure it and update the object.
                    //
                    securityUser.password = SecurityLogic.SecurePasswordHasher.Hash(securityUser.password);
                }


                //
                // Limit the size of the images
                //
                if (securityUser.image != null && securityUser.image.Length > MAXIMUM_IMAGEFILE_SIZE)
                {
                    return BadRequest("Image size exceeds maximum size allowed value of " + MAXIMUM_IMAGEFILE_SIZE);
                }


                /* custom bit ends here */


                if (securityUser.accountName != null && securityUser.accountName.Length > 250)
                {
                    securityUser.accountName = securityUser.accountName.Substring(0, 250);
                }

                if (securityUser.password != null && securityUser.password.Length > 250)
                {
                    securityUser.password = securityUser.password.Substring(0, 250);
                }

                if (securityUser.firstName != null && securityUser.firstName.Length > 100)
                {
                    securityUser.firstName = securityUser.firstName.Substring(0, 100);
                }

                if (securityUser.middleName != null && securityUser.middleName.Length > 100)
                {
                    securityUser.middleName = securityUser.middleName.Substring(0, 100);
                }

                if (securityUser.lastName != null && securityUser.lastName.Length > 100)
                {
                    securityUser.lastName = securityUser.lastName.Substring(0, 100);
                }

                if (securityUser.dateOfBirth.HasValue == true && securityUser.dateOfBirth.Value.Kind != DateTimeKind.Utc)
                {
                    securityUser.dateOfBirth = securityUser.dateOfBirth.Value.ToUniversalTime();
                }

                if (securityUser.emailAddress != null && securityUser.emailAddress.Length > 100)
                {
                    securityUser.emailAddress = securityUser.emailAddress.Substring(0, 100);
                }

                if (securityUser.cellPhoneNumber != null && securityUser.cellPhoneNumber.Length > 100)
                {
                    securityUser.cellPhoneNumber = securityUser.cellPhoneNumber.Substring(0, 100);
                }

                if (securityUser.phoneNumber != null && securityUser.phoneNumber.Length > 50)
                {
                    securityUser.phoneNumber = securityUser.phoneNumber.Substring(0, 50);
                }

                if (securityUser.phoneExtension != null && securityUser.phoneExtension.Length > 50)
                {
                    securityUser.phoneExtension = securityUser.phoneExtension.Substring(0, 50);
                }

                if (securityUser.description != null && securityUser.description.Length > 500)
                {
                    securityUser.description = securityUser.description.Substring(0, 500);
                }

                if (securityUser.authenticationDomain != null && securityUser.authenticationDomain.Length > 100)
                {
                    securityUser.authenticationDomain = securityUser.authenticationDomain.Substring(0, 100);
                }

                if (securityUser.lastLoginAttempt.HasValue == true && securityUser.lastLoginAttempt.Value.Kind != DateTimeKind.Utc)
                {
                    securityUser.lastLoginAttempt = securityUser.lastLoginAttempt.Value.ToUniversalTime();
                }

                if (securityUser.mostRecentActivity.HasValue == true && securityUser.mostRecentActivity.Value.Kind != DateTimeKind.Utc)
                {
                    securityUser.mostRecentActivity = securityUser.mostRecentActivity.Value.ToUniversalTime();
                }

                if (securityUser.alternateIdentifier != null && securityUser.alternateIdentifier.Length > 100)
                {
                    securityUser.alternateIdentifier = securityUser.alternateIdentifier.Substring(0, 100);
                }

                if (securityUser.authenticationToken != null && securityUser.authenticationToken.Length > 100)
                {
                    securityUser.authenticationToken = securityUser.authenticationToken.Substring(0, 100);
                }

                if (securityUser.authenticationTokenExpiry.HasValue == true && securityUser.authenticationTokenExpiry.Value.Kind != DateTimeKind.Utc)
                {
                    securityUser.authenticationTokenExpiry = securityUser.authenticationTokenExpiry.Value.ToUniversalTime();
                }

                if (securityUser.twoFactorToken != null && securityUser.twoFactorToken.Length > 10)
                {
                    securityUser.twoFactorToken = securityUser.twoFactorToken.Substring(0, 10);
                }

                if (securityUser.twoFactorTokenExpiry.HasValue == true && securityUser.twoFactorTokenExpiry.Value.Kind != DateTimeKind.Utc)
                {
                    securityUser.twoFactorTokenExpiry = securityUser.twoFactorTokenExpiry.Value.ToUniversalTime();
                }

                securityUser.objectGuid = Guid.NewGuid();
                _context.SecurityUsers.Add(securityUser);


                //
                // Custom bit here to validate the data visibility defaults for this user.  Make sure that they make sense in terms of hierarchy integrity.
                //
                await ValidateDataVisibilityDefaultsForUserAsync(securityUser);

                //
                // End of data visibility checking custom bit.
                //
                await _context.SaveChangesAsync(cancellationToken);

                //
                // Custom bit here
                //
                // If user has any values in any of the tenant, organization, department or team fields, make sure that there are matching counterparts in the associated user mapping tables
                //
                await CreateOrUpdateUserDataVisibilityTablesAsync(securityUser, cancellationToken);

                //
                // Confirm that the reports to hierarchy makes sense
                //
                ValidateReportsTo(securityUser);

                securityUser.image = null;


                //
                // Blow away the related fields for serialization, especially the reports to user which could make a circular reference bad data is in the database.
                //
                securityUser.reportsToSecurityUser = null;
                securityUser.securityTeam = null;
                securityUser.securityDepartment = null;
                securityUser.securityOrganization = null;


                //
                // Because we removed some data for the serializing, load the record again and clobber the password for the return object
                //
                securityUserToReturn = await (from x in _context.SecurityUsers where x.id == securityUser.id select x).FirstOrDefaultAsync(cancellationToken);
                securityUserToReturn.password = null;


                //
                // custom bit ends here
                //
                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                    "Security.SecurityUser entity successfully created.",
                    true,
                    securityUser.id.ToString(),
                    "",
                    JsonSerializer.Serialize(Database.SecurityUser.CreateAnonymousWithFirstLevelSubObjects(securityUser)),
                    null);

                //
                // Create a SecurityUserEvent to track user creation
                //
                string displayName = $"{securityUser.firstName} {securityUser.lastName}".Trim();
                await CreateSecurityUserEventAsync(
                    securityUser.id,
                    "User Created",
                    $"User account created for {displayName} ({securityUser.accountName})",
                    cancellationToken);

            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Security.User entity creation failed.", false, securityUser.id.ToString(), "", JsonSerializer.Serialize(securityUser), ex);

                return Problem(ex.Message);
            }

            BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "SecurityUser", securityUser.id, securityUser.accountName));


            // custom return
            return CreatedAtRoute("SecurityUser", new { id = securityUser.id }, SecurityUser.CreateAnonymousWithFirstLevelSubObjects(securityUserToReturn));
        }



        //
        // Fixes the name clash for the term securityUser
        //
        [HttpDelete]
        [Route("api/SecurityUser/{id}")]
        [Route("api/SecurityUser")]
        [Route("api/Surface/SecurityUser/{id}")]
        [Route("api/Surface/SecurityUser")]
        public async Task<IActionResult> DeleteSecurityUser(int id, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync() == false)
            {
                return Unauthorized();
            }

            if (await IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel.Write) == false)
            {
                return Unauthorized();
            }

            SecurityUser securityUserForCurrentUser = GetSecurityUser();

            IQueryable<SecurityUser> query = (from x in _context.SecurityUsers
                                              where
                                              (x.id == id)
                                              select x);


            SecurityUser securityUserObject = query.FirstOrDefault();

            if (securityUserObject == null)
            {
                CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Security.SecurityUser DELETE", id.ToString(), new Exception("No Security.SecurityUser entity could be find with the primary key provided."));
                return NotFound();
            }
            SecurityUser cloneOfExisting = (SecurityUser)_context.Entry(securityUserObject).GetDatabaseValues().ToObject();

            try
            {
                securityUserObject.deleted = true;
                await _context.SaveChangesAsync(cancellationToken);

                await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
                    "Security.SecurityUser entity successfully deleted.",
                    true,
                    id.ToString(),
                    JsonSerializer.Serialize(Database.SecurityUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                    JsonSerializer.Serialize(Database.SecurityUser.CreateAnonymousWithFirstLevelSubObjects(securityUserObject)),
                    null);

                //
                // Create a SecurityUserEvent to track user deletion
                //
                string displayName = $"{cloneOfExisting.firstName} {cloneOfExisting.lastName}".Trim();
                await CreateSecurityUserEventAsync(
                    id,
                    "User Deleted",
                    $"User account deleted for {displayName} ({cloneOfExisting.accountName}) by '{securityUserForCurrentUser?.accountName ?? "Unknown"}'",
                    cancellationToken);

            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
                    "Security.SecurityUser entity delete failed.",
                    false,
                    id.ToString(),
                    JsonSerializer.Serialize(Database.SecurityUser.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                    JsonSerializer.Serialize(Database.SecurityUser.CreateAnonymousWithFirstLevelSubObjects(securityUserObject)),
                    ex);

                return Problem(ex.Message);
            }

            return Ok();
        }


        /// <summary>
        /// 
        /// Upload a profile image for a user.
        /// Accepts base64-encoded image data.
        /// 
        /// </summary>
        [HttpPut("api/SecurityUsers/{id}/image")]
        public async Task<IActionResult> PutUserImage(int id, [FromBody] UserImageUploadRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Unauthorized();
            }

            SecurityUser user = await _context.SecurityUsers.FindAsync(new object[] { id }, cancellationToken);
            if (user == null)
            {
                return NotFound();
            }

            try
            {
                //
                // Decode base64 image data
                //
                if (string.IsNullOrWhiteSpace(request.ImageData))
                {
                    return BadRequest("Image data is required");
                }

                //
                // Remove data URL prefix if present (e.g., "data:image/png;base64,")
                //
                string base64Data = request.ImageData;
                if (base64Data.Contains(","))
                {
                    base64Data = base64Data.Split(',')[1];
                }

                byte[] imageBytes = Convert.FromBase64String(base64Data);

                //
                // Validate image size (max 5MB)
                //
                const int maxSizeBytes = 5 * 1024 * 1024;
                if (imageBytes.Length > maxSizeBytes)
                {
                    return BadRequest("Image size exceeds 5MB limit");
                }

                user.image = imageBytes;
                await _context.SaveChangesAsync(cancellationToken);

                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, $"User image updated for: {user.accountName}");


                //
                // Create a SecurityUserEvent to track user image changing
                //
                string displayName = $"{user.firstName} {user.lastName}".Trim();
                await CreateSecurityUserEventAsync(
                    id,
                    "User Deleted",
                    $"User image updated for {displayName} ({user.accountName}) by '{user?.accountName ?? "Unknown"}'",
                    cancellationToken);

                return Ok(new { message = "Image uploaded successfully", imageSize = imageBytes.Length });
            }
            catch (FormatException)
            {
                return BadRequest("Invalid base64 image data");
            }
        }


        /// <summary>
        /// 
        /// Delete a user's profile image.
        /// 
        /// </summary>
        [HttpDelete("api/SecurityUsers/{id}/image")]
        public async Task<IActionResult> DeleteUserImage(int id, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Unauthorized();
            }

            var user = await _context.SecurityUsers.FindAsync(new object[] { id }, cancellationToken);
            if (user == null)
            {
                return NotFound();
            }

            user.image = null;
            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, $"User image removed for: {user.accountName}");


            //
            // Create a SecurityUserEvent to track user image changing
            //
            string displayName = $"{user.firstName} {user.lastName}".Trim();
            await CreateSecurityUserEventAsync(
                id,
                "User Deleted",
                $"User image deleted for {displayName} ({user.accountName}) by '{user?.accountName ?? "Unknown"}'",
                cancellationToken);

            return Ok(new { message = "Image removed successfully" });
        }


        /* adds in support for new routes.  Otherwise same as auto generated */
        [Route("api/SecurityUsers/ListData")]
        [Route("api/ReportsToSecurityUsers/ListData")]
        [HttpGet]
        public async Task<IActionResult> GetListData(
            string accountName = null,
            int? activeDirectoryAccount = null,
            int? canLogin = null,
            int? mustChangePassword = null,
            string firstName = null,
            string middleName = null,
            string lastName = null,
            DateTime? dateOfBirth = null,
            string emailAddress = null,
            string cellPhoneNumber = null,
            string phoneNumber = null,
            string phoneExtension = null,
            string description = null,
            int? securityUserTitleId = null,
            int? reportsToSecurityUserId = null,
            string authenticationDomain = null,
            int? failedLoginCount = null,
            DateTime? lastLoginAttempt = null,
            DateTime? mostRecentActivity = null,
            string alternateIdentifier = null,
            string settings = null,
            int? securityTenantId = null,
            int? readPermissionLevel = null,
            int? writePermissionLevel = null,
            int? securityOrganizationId = null,
            int? securityDepartmentId = null,
            int? securityTeamId = null,
            string authenticationToken = null,
            DateTime? authenticationTokenExpiry = null,
            string twoFactorToken = null,
            DateTime? twoFactorTokenExpiry = null,
            int? twoFactorSendByEmail = null,
            int? twoFactorSendBySMS = null,
            Guid? objectGuid = null,
            int? active = null,
            int? deleted = null,
            int? pageSize = null,
            int? pageNumber = null,
            string anyStringContains = null,
            CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Unauthorized();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

            bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
            bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);

            bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

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
            // Turn any local time kinded parameters to UTC.
            //
            if (dateOfBirth.HasValue == true && dateOfBirth.Value.Kind != DateTimeKind.Utc)
            {
                dateOfBirth = dateOfBirth.Value.ToUniversalTime();
            }

            if (lastLoginAttempt.HasValue == true && lastLoginAttempt.Value.Kind != DateTimeKind.Utc)
            {
                lastLoginAttempt = lastLoginAttempt.Value.ToUniversalTime();
            }

            if (mostRecentActivity.HasValue == true && mostRecentActivity.Value.Kind != DateTimeKind.Utc)
            {
                mostRecentActivity = mostRecentActivity.Value.ToUniversalTime();
            }

            if (authenticationTokenExpiry.HasValue == true && authenticationTokenExpiry.Value.Kind != DateTimeKind.Utc)
            {
                authenticationTokenExpiry = authenticationTokenExpiry.Value.ToUniversalTime();
            }

            if (twoFactorTokenExpiry.HasValue == true && twoFactorTokenExpiry.Value.Kind != DateTimeKind.Utc)
            {
                twoFactorTokenExpiry = twoFactorTokenExpiry.Value.ToUniversalTime();
            }

            var query = (from su in _context.SecurityUsers select su);
            if (string.IsNullOrEmpty(accountName) == false)
            {
                query = query.Where(su => su.accountName == accountName);
            }
            if (activeDirectoryAccount.HasValue == true)
            {
                query = query.Where(su => su.activeDirectoryAccount == (activeDirectoryAccount.Value == 1 ? true : false));
            }
            if (canLogin.HasValue == true)
            {
                query = query.Where(su => su.canLogin == (canLogin.Value == 1 ? true : false));
            }
            if (mustChangePassword.HasValue == true)
            {
                query = query.Where(su => su.mustChangePassword == (mustChangePassword.Value == 1 ? true : false));
            }
            if (string.IsNullOrEmpty(firstName) == false)
            {
                query = query.Where(su => su.firstName == firstName);
            }
            if (string.IsNullOrEmpty(middleName) == false)
            {
                query = query.Where(su => su.middleName == middleName);
            }
            if (string.IsNullOrEmpty(lastName) == false)
            {
                query = query.Where(su => su.lastName == lastName);
            }
            if (dateOfBirth.HasValue == true)
            {
                query = query.Where(su => su.dateOfBirth == dateOfBirth.Value);
            }
            if (string.IsNullOrEmpty(emailAddress) == false)
            {
                query = query.Where(su => su.emailAddress == emailAddress);
            }
            if (string.IsNullOrEmpty(cellPhoneNumber) == false)
            {
                query = query.Where(su => su.cellPhoneNumber == cellPhoneNumber);
            }
            if (string.IsNullOrEmpty(phoneNumber) == false)
            {
                query = query.Where(su => su.phoneNumber == phoneNumber);
            }
            if (string.IsNullOrEmpty(phoneExtension) == false)
            {
                query = query.Where(su => su.phoneExtension == phoneExtension);
            }
            if (string.IsNullOrEmpty(description) == false)
            {
                query = query.Where(su => su.description == description);
            }
            if (securityUserTitleId.HasValue == true)
            {
                query = query.Where(su => su.securityUserTitleId == securityUserTitleId.Value);
            }
            if (reportsToSecurityUserId.HasValue == true)
            {
                query = query.Where(su => su.reportsToSecurityUserId == reportsToSecurityUserId.Value);
            }
            if (string.IsNullOrEmpty(authenticationDomain) == false)
            {
                query = query.Where(su => su.authenticationDomain == authenticationDomain);
            }
            if (failedLoginCount.HasValue == true)
            {
                query = query.Where(su => su.failedLoginCount == failedLoginCount.Value);
            }
            if (lastLoginAttempt.HasValue == true)
            {
                query = query.Where(su => su.lastLoginAttempt == lastLoginAttempt.Value);
            }
            if (mostRecentActivity.HasValue == true)
            {
                query = query.Where(su => su.mostRecentActivity == mostRecentActivity.Value);
            }
            if (string.IsNullOrEmpty(alternateIdentifier) == false)
            {
                query = query.Where(su => su.alternateIdentifier == alternateIdentifier);
            }
            if (string.IsNullOrEmpty(settings) == false)
            {
                query = query.Where(su => su.settings == settings);
            }
            if (securityTenantId.HasValue == true)
            {
                query = query.Where(su => su.securityTenantId == securityTenantId.Value);
            }
            if (readPermissionLevel.HasValue == true)
            {
                query = query.Where(su => su.readPermissionLevel == readPermissionLevel.Value);
            }
            if (writePermissionLevel.HasValue == true)
            {
                query = query.Where(su => su.writePermissionLevel == writePermissionLevel.Value);
            }
            if (securityOrganizationId.HasValue == true)
            {
                query = query.Where(su => su.securityOrganizationId == securityOrganizationId.Value);
            }
            if (securityDepartmentId.HasValue == true)
            {
                query = query.Where(su => su.securityDepartmentId == securityDepartmentId.Value);
            }
            if (securityTeamId.HasValue == true)
            {
                query = query.Where(su => su.securityTeamId == securityTeamId.Value);
            }
            if (string.IsNullOrEmpty(authenticationToken) == false)
            {
                query = query.Where(su => su.authenticationToken == authenticationToken);
            }
            if (authenticationTokenExpiry.HasValue == true)
            {
                query = query.Where(su => su.authenticationTokenExpiry == authenticationTokenExpiry.Value);
            }
            if (string.IsNullOrEmpty(twoFactorToken) == false)
            {
                query = query.Where(su => su.twoFactorToken == twoFactorToken);
            }
            if (twoFactorTokenExpiry.HasValue == true)
            {
                query = query.Where(su => su.twoFactorTokenExpiry == twoFactorTokenExpiry.Value);
            }
            if (twoFactorSendByEmail.HasValue == true)
            {
                query = query.Where(su => su.twoFactorSendByEmail == (twoFactorSendByEmail.Value == 1 ? true : false));
            }
            if (twoFactorSendBySMS.HasValue == true)
            {
                query = query.Where(su => su.twoFactorSendBySMS == (twoFactorSendBySMS.Value == 1 ? true : false));
            }
            if (objectGuid.HasValue == true)
            {
                query = query.Where(su => su.objectGuid == objectGuid);
            }
            if (userIsWriter == true)
            {
                if (active.HasValue == true)
                {
                    query = query.Where(su => su.active == (active.Value == 1 ? true : false));
                }

                if (userIsAdmin == true)
                {
                    if (deleted.HasValue == true)
                    {
                        query = query.Where(su => su.deleted == (deleted.Value == 1 ? true : false));
                    }
                }
                else
                {
                    query = query.Where(su => su.deleted == false);
                }
            }
            else
            {
                query = query.Where(su => su.active == true);
                query = query.Where(su => su.deleted == false);
            }


            //
            // Add the any string contains parameter to span all the string fields on the Security User, or on an any of the string fields on its immediate relations
            //
            // Note that this will be a time intensive parameter to apply, so use it with that understanding.
            //
            if (!string.IsNullOrEmpty(anyStringContains))
            {
                query = query.Where(x =>
                    x.accountName.Contains(anyStringContains)
                    || x.password.Contains(anyStringContains)
                    || x.firstName.Contains(anyStringContains)
                    || x.middleName.Contains(anyStringContains)
                    || x.lastName.Contains(anyStringContains)
                    || x.emailAddress.Contains(anyStringContains)
                    || x.cellPhoneNumber.Contains(anyStringContains)
                    || x.phoneNumber.Contains(anyStringContains)
                    || x.phoneExtension.Contains(anyStringContains)
                    || x.description.Contains(anyStringContains)
                    || x.authenticationDomain.Contains(anyStringContains)
                    || x.alternateIdentifier.Contains(anyStringContains)
                    || x.settings.Contains(anyStringContains)
                    || x.authenticationToken.Contains(anyStringContains)
                    || x.twoFactorToken.Contains(anyStringContains)
                    || x.reportsToSecurityUser.accountName.Contains(anyStringContains)
                    || x.reportsToSecurityUser.password.Contains(anyStringContains)
                    || x.reportsToSecurityUser.firstName.Contains(anyStringContains)
                    || x.reportsToSecurityUser.middleName.Contains(anyStringContains)
                    || x.reportsToSecurityUser.lastName.Contains(anyStringContains)
                    || x.reportsToSecurityUser.emailAddress.Contains(anyStringContains)
                    || x.reportsToSecurityUser.cellPhoneNumber.Contains(anyStringContains)
                    || x.reportsToSecurityUser.phoneNumber.Contains(anyStringContains)
                    || x.reportsToSecurityUser.phoneExtension.Contains(anyStringContains)
                    || x.reportsToSecurityUser.description.Contains(anyStringContains)
                    || x.reportsToSecurityUser.authenticationDomain.Contains(anyStringContains)
                    || x.reportsToSecurityUser.alternateIdentifier.Contains(anyStringContains)
                    || x.reportsToSecurityUser.settings.Contains(anyStringContains)
                    || x.reportsToSecurityUser.authenticationToken.Contains(anyStringContains)
                    || x.reportsToSecurityUser.twoFactorToken.Contains(anyStringContains)
                    || x.securityDepartment.name.Contains(anyStringContains)
                    || x.securityDepartment.description.Contains(anyStringContains)
                    || x.securityOrganization.name.Contains(anyStringContains)
                    || x.securityOrganization.description.Contains(anyStringContains)
                    || x.securityTeam.name.Contains(anyStringContains)
                    || x.securityTeam.description.Contains(anyStringContains)
                    || x.securityTenant.name.Contains(anyStringContains)
                    || x.securityTenant.description.Contains(anyStringContains)
                    || x.securityUserTitle.name.Contains(anyStringContains)
                    || x.securityUserTitle.description.Contains(anyStringContains)
                );
            }



            query = query.OrderBy(x => x.accountName);

            if (pageNumber.HasValue == true &&
                pageSize.HasValue == true)
            {
                query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            return Ok(await (from queryData in query select Database.SecurityUser.CreateMinimalAnonymous(queryData)).ToListAsync());
        }


#if WINDOWS

        [HttpGet]
        [Route("api/SecurityUsers/CreateADUser")]
        public async Task<IActionResult> CreateADUser(string accountName, string domainToLookIn = null)
        {
            StartAuditEventClock();


            if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync() == false)
            {
                return Unauthorized();
            }

            CreateAuditEvent(Auditor.AuditEngine.AuditType.Miscellaneous, "Request received to create user for AD account with name of " + accountName);

            try
            {
                string domainName;
                System.DirectoryServices.AccountManagement.UserPrincipal principal = SecurityLogic.GetUserPrincipalFromDomainByAccountName(accountName, out domainName, domainToLookIn);

                if (principal != null)
                {
                    var user = SecurityLogic.CreateOrUpdateUserRecordFromUserPrincipal(principal, domainName);

                    CreateAuditEvent(Auditor.AuditEngine.AuditType.Miscellaneous, "Created or updated user for AD account with name of " + accountName + " from domain: " + domainName);

                    return Ok(user);
                }
                else
                {
                    return BadRequest("Could not find user with account name of: " + accountName);
                }

            }
            catch (Exception ex)
            {
                CreateAuditEvent(Auditor.AuditEngine.AuditType.Error, "Caught error trying to create user for AD account with name of: " + accountName, accountName, ex);

				throw;
            }
        }
#endif


        private void ValidateReportsTo(SecurityUser user, List<SecurityUser> reportingHierarchy = null)
        {
            if (user.reportsToSecurityUserId.HasValue == false)
            {
                //
                // This user has no report as read directly from the database, but this could be the same record that being updated, but in a circular reference that is not yet written yet.  As such,   Check the hierarchy to see if the it already has any isntance of this user.  One is too many.
                // 
                if (reportingHierarchy != null)
                {
                    foreach (SecurityUser userInHierarchy in reportingHierarchy)
                    {
                        if (userInHierarchy.id == user.id)
                        {
                            string message = "Invalid reports to structure found for user with id of " + user.id + " and account name of " + user.accountName + " because this write would createa a circular reference of a user reporting to themselves.";

                            CreateAuditEvent(AuditEngine.AuditType.Error, message);

                            throw new Exception(message);
                        }
                    }
                }
                return;
            }
            else
            {
                if (reportingHierarchy == null)
                {
                    reportingHierarchy = new List<SecurityUser>();
                }

                //
                // Add this person to the hierarchy
                //
                reportingHierarchy.Add(user);

                //
                // Look through the existing hierarchy for this user's id to make sure that it is not there more than once.  We expect each manager to be be in the list one time, as we start traversing upwards.
                //
                int foundCount = 0;
                foreach (SecurityUser userInHierarchy in reportingHierarchy)
                {
                    if (userInHierarchy.id == user.id)
                    {
                        foundCount++;

                        if (foundCount > 1)
                        {
                            //
                            // Update the record to break the circular reference moving forward
                            //
                            using (SecurityContext transactionDb = new SecurityContext())
                            {
                                SecurityUser userToUpdate = (from x in transactionDb.SecurityUsers where x.id == user.id select x).FirstOrDefault();

                                if (userToUpdate != null)
                                {
                                    userToUpdate.reportsToSecurityUserId = null;
                                    _context.SaveChanges();
                                }
                            }


                            string message = "Invalid reports to structure found for user with id of " + user.id + " and account name of " + user.accountName + " because of a circular reference of user reporting to themselves.  User " + user.accountName + " has had its reports to cleared.";

                            CreateAuditEvent(AuditEngine.AuditType.Error, message);

                            throw new Exception(message);
                        }
                    }
                }



                //
                // Get the user's manager, and validate upwards till we find somebody with no manager.
                //
                SecurityUser userReportedTo = (from x in _context.SecurityUsers where x.id == user.reportsToSecurityUserId select x).AsNoTracking().FirstOrDefault();

                if (userReportedTo != null)
                {
                    ValidateReportsTo(userReportedTo, reportingHierarchy);
                }
            }


            return;
        }


        //
        // Security User Event Logging Helpers
        //
        // These methods create human-readable audit trail entries in the SecurityUserEvent table
        // when user accounts are created, modified, or deleted.
        //

        /// <summary>
        /// 
        /// Generates a human-readable summary of changes between two SecurityUser states.
        /// 
        /// </summary>
        private string GenerateUserChangeSummary(SecurityUser before, SecurityUser after)
        {
            var changes = new List<string>();

            // Name changes
            if (before.firstName != after.firstName)
            {
                changes.Add($"First name: '{before.firstName ?? "(empty)"}' → '{after.firstName ?? "(empty)"}'");
            }


            if (before.middleName != after.middleName)
            {
                changes.Add($"Middle name: '{before.middleName ?? "(empty)"}' → '{after.middleName ?? "(empty)"}'");
            }


            if (before.lastName != after.lastName)
            {
                changes.Add($"Last name: '{before.lastName ?? "(empty)"}' → '{after.lastName ?? "(empty)"}'");
            }

            // Contact info
            if (before.emailAddress != after.emailAddress)
            {
                changes.Add($"Email: '{before.emailAddress ?? "(empty)"}' → '{after.emailAddress ?? "(empty)"}'");
            }


            if (before.cellPhoneNumber != after.cellPhoneNumber)
            {
                changes.Add($"Cell phone changed");
            }

            if (before.phoneNumber != after.phoneNumber)
            {
                changes.Add($"Phone number changed");
            }

            // Account status
            if (before.active != after.active)
            {
                changes.Add(after.active ? "Account activated" : "Account deactivated");
            }

            if (before.deleted != after.deleted)
            {
                changes.Add(after.deleted ? "Account deleted" : "Account restored");
            }

            if (before.accountName != after.accountName)
            {
                changes.Add($"Account name: '{before.accountName}' → '{after.accountName}'");
            }

            // Authentication
            if (before.activeDirectoryAccount != after.activeDirectoryAccount)
            {
                changes.Add(after.activeDirectoryAccount ? "Switched to AD authentication" : "Switched to local authentication");
            }


            if (before.authenticationDomain != after.authenticationDomain)
            {
                changes.Add($"Auth domain: '{before.authenticationDomain ?? "(none)"}' → '{after.authenticationDomain ?? "(none)"}'");
            }


            // Organization hierarchy
            if (before.securityTenantId != after.securityTenantId)
            {
                changes.Add("Tenant assignment changed");
            }

            if (before.securityOrganizationId != after.securityOrganizationId)
            {
                changes.Add("Organization assignment changed");
            }


            if (before.securityDepartmentId != after.securityDepartmentId)
            {
                changes.Add("Department assignment changed");
            }

            if (before.securityTeamId != after.securityTeamId)
            {
                changes.Add("Team assignment changed");
            }

            if (before.reportsToSecurityUserId != after.reportsToSecurityUserId)
            {
                changes.Add("Manager (reports to) changed");
            }

            // Password change (we can't see the actual password, but we know if it changed)
            if (before.password != after.password && !string.IsNullOrEmpty(after.password))
            {
                changes.Add("Password updated");
            }

            // Description
            if (before.description != after.description)
            {
                changes.Add("Description updated");
            }

            // Image
            bool beforeHasImage = before.image != null && before.image.Length > 0;
            bool afterHasImage = after.image != null && after.image.Length > 0;

            if (beforeHasImage != afterHasImage)
            {
                changes.Add(afterHasImage ? "Profile image added" : "Profile image removed");
            }
            else if (beforeHasImage && afterHasImage && !before.image.SequenceEqual(after.image))
            {
                changes.Add("Profile image updated");
            }

            return changes.Count > 0
                ? string.Join("; ", changes)
                : "No significant changes detected";
        }


        /// <summary>
        /// 
        /// Creates a SecurityUserEvent record to track user account changes.
        /// 
        /// Event Types (assumed IDs - will use names to look up):
        /// - "User Created", "User Modified", "User Deleted"
        /// 
        /// </summary>
        private async Task CreateSecurityUserEventAsync(int targetUserId,
                                                        string eventTypeName,
                                                        string comments,
                                                        CancellationToken cancellationToken = default)
        {
            try
            {
                // Look up the event type by name, or create it if it doesn't exist
                var eventType = await _context.SecurityUserEventTypes.FirstOrDefaultAsync(t => t.name == eventTypeName, cancellationToken);

                if (eventType == null)
                {
                    // Event type doesn't exist, create it
                    eventType = new SecurityUserEventType
                    {
                        name = eventTypeName,
                        description = $"Auto-created event type for {eventTypeName}"
                    };

                    _context.SecurityUserEventTypes.Add(eventType);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                SecurityUserEvent userEvent = new SecurityUserEvent
                {
                    securityUserId = targetUserId,
                    securityUserEventTypeId = eventType.id,
                    timeStamp = DateTime.UtcNow,
                    comments = comments?.Length > 4000 ? comments.Substring(0, 4000) : comments, // Truncate if too long
                    active = true,
                    deleted = false
                };

                _context.SecurityUserEvents.Add(userEvent);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Don't let event logging failures break the main operation
                Logger.GetCommonLogger().LogWarning($"Failed to create SecurityUserEvent: {ex.Message}");
            }
        }


        private void ValidatePasswordStrength(string password)
        {
            if (Foundation.Configuration.GetBooleanConfigurationSetting("RequireComplexLocalPasswords", true) == true)
            {
                PasswordStrength passwordStrength = Foundation.Security.PasswordCheck.GetPasswordStrength(password);

                switch (passwordStrength)
                {
                    case PasswordStrength.Blank:
                    case PasswordStrength.VeryWeak:
                    case PasswordStrength.Weak:
                        {
                            CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Password provided to User Post was not complex enough.");
                            throw new Exception("Password is not strong enough to be used.");
                        }


                    case PasswordStrength.Medium:
                    case PasswordStrength.Strong:
                    case PasswordStrength.VeryStrong:
                        // Password deemed strong enough, allow user to be added to database etc
                        break;
                }
            }
        }
    }
}
