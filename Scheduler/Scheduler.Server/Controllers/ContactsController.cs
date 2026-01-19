using Foundation.Auditor;
using Foundation.Scheduler.Database;
using Foundation.ChangeHistory;
using Foundation.Security;
using Foundation.Security.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Foundation.Controllers;
using System.Threading;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    public partial class ContactsController : SecureWebAPIController
    {

        /// <summary>
        /// 
        /// The purpose of this method is to return the contact for the current user.
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet]
        [RateLimit(RateLimitOption.TenPerMinute, Scope = RateLimitScope.PerUser)]
        [Route("api/Contacts/GetCurrentUserContact")]
        public async Task<IActionResult> GetCurrentUserContact()
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync();

            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser.accountName, securityUser.accountName, ex);
                throw new Exception("Your user account is not configured with a tenant, so this operation is not allowed.");
            }

            try
            {

                //
                // Step 1 - Get Contact for current user.  If none exists, then create one.  Match must be on contact.externalId == securityUser.objectGuid for a particular tenant.
                //
                Contact contactForCurrentUser = await LoadContactForUser(securityUser, userTenantGuid);

                if (contactForCurrentUser == null)
                {
                    //
                    // We could not find a contact for the current user.  Create one.
                    //

                    //
                    // First make sure that we have contact types.  If there are none, then the tenant has not yet been setup.
                    //
                    ContactType staffContactType = await (from ct in _context.ContactTypes
                                                          where ct.name == "Staff"            // Improve this later.  Probably better to have a 'Company Staff' flag on the contact type table or something
                                                          select ct).FirstOrDefaultAsync();

                    if (staffContactType == null)
                    {
                        return Problem("Database has no 'Staff' contact type.");
                    }

                    contactForCurrentUser = new Contact();

                    contactForCurrentUser.firstName = securityUser.firstName;
                    contactForCurrentUser.middleName = securityUser.middleName;
                    contactForCurrentUser.lastName = securityUser.lastName;
                    contactForCurrentUser.lastName = securityUser.lastName;
                    contactForCurrentUser.email = securityUser.emailAddress;
                    contactForCurrentUser.phone = securityUser.phoneNumber;
                    contactForCurrentUser.externalId = securityUser.objectGuid.ToString();       // User object guid becomes the contact's external id to link the contact to the tenant user.

                    // Use the 'Staff' contact type
                    contactForCurrentUser.contactTypeId = staffContactType.id;

                    // setup admin fields - use the same object guid as the security user has.
                    contactForCurrentUser.tenantGuid = userTenantGuid;
                    contactForCurrentUser.active = true;
                    contactForCurrentUser.deleted = false;

                    contactForCurrentUser.objectGuid = Guid.NewGuid();

                    //
                    // Use the change history toolset to manage the saving of the Contact
                    //
                    ChangeHistoryToolset<Contact, ContactChangeHistory> cch = Contact.GetChangeHistoryToolsetForWriting(_context, securityUser);

                    await cch.AddEntityAsync(contactForCurrentUser);

                    await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                        "Scheduler.Contact entity successfully created during new contact creation for current user process.",
                        true,
                        contactForCurrentUser.id.ToString(),
                        "",
                        JsonSerializer.Serialize(Database.Contact.CreateAnonymousWithFirstLevelSubObjects(contactForCurrentUser)),
                        null);

                }

                //
                // Step 2 - Update the contact we have found to follow the current security user's configuration.  This to help simplify the process
                // of synchronizing current user to current user contact so it never needs to be done manually, and it'll never be out of sync.
                //
                if (contactForCurrentUser.firstName != securityUser.firstName ||
                    contactForCurrentUser.middleName != securityUser.middleName ||
                    contactForCurrentUser.lastName != securityUser.lastName ||
                    contactForCurrentUser.email != securityUser.emailAddress ||
                    contactForCurrentUser.phone != securityUser.phoneNumber)
                {
                    //
                    // Sync the relevant fields
                    //
                    contactForCurrentUser.firstName = securityUser.firstName;
                    contactForCurrentUser.middleName = securityUser.middleName;
                    contactForCurrentUser.lastName = securityUser.lastName;
                    contactForCurrentUser.email = securityUser.emailAddress;
                    contactForCurrentUser.phone = securityUser.phoneNumber;


                    //
                    // Kick off the controller PUT to save the contact.  We don't need to do anything with the result.
                    //
                    await PutContact(contactForCurrentUser.id, contactForCurrentUser.ToDTO());

                    //
                    // Reload the contact after the change
                    //
                    contactForCurrentUser = await LoadContactForUser(securityUser, userTenantGuid);
                }

                //
                // Convert to DTO object before serializing out.
                //
                return Ok(contactForCurrentUser.ToOutputDTO());

            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Error getting current user contacts.", securityUser.accountName, ex);

                return Problem("Could not get current user contacts.");
            }
        }

        /// <summary>
        /// 
        /// This loads the contact record for a security user.  It matches on the securityUser.objectGuid == contact.externalId
        /// 
        /// It loads all navigation properties for the contact.
        /// 
        /// </summary>
        /// <param name="securityUser"></param>
        /// <param name="userTenantGuid"></param>
        /// <returns></returns>
        private async Task<Contact> LoadContactForUser(SecurityUser securityUser, Guid userTenantGuid)
        {
            Contact contactForCurrentUser = await (from c in _context.Contacts
                                                   where
                                                   c.tenantGuid == userTenantGuid &&
                                                   c.externalId == securityUser.objectGuid.ToString()         // Match user to contact by it having it's externalId match match the user obejct guid.  This will allow the same user to have contacts across tenants if they move companies (public emails, or test accounts at risk for this)
                                                   select c)
                                                   .Include(x => x.contactType)
                                                   .Include(x => x.salutation)
                                                   .Include(x => x.timeZone)
                                                   .Include(x => x.contactMethod)
                                                   .Include(x => x.icon)
                                                   .FirstOrDefaultAsync();
            return contactForCurrentUser;
        }

        /// <summary>
        /// 
        /// Gets the change metadata (audit details) for a specific version of a Contact.
        /// 
        /// </summary>
        /// <param name="id">The contact ID</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>AuditEntry containing timestamp and user ID</returns>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Contact/{id}/ChangeMetadata")]
        public async Task<IActionResult> GetContactChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            // Load the contact to verify access and get tenant/context
            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);

            Database.Contact contact = await _context.Contacts.Where(c => c.id == id && c.tenantGuid == userTenantGuid).FirstOrDefaultAsync(cancellationToken);
            if (contact == null)
            {
                return NotFound();
            }

            try
            {
                contact.SetupVersionInquiry(_context, userTenantGuid);
                ChangeHistoryToolset<Database.Contact, ContactChangeHistory> toolset = Database.Contact.GetChangeHistoryToolsetForReading(_context, cancellationToken);
                
                AuditEntry audit = await toolset.GetAuditForVersion(contact, versionNumber).ConfigureAwait(false);
                
                if (audit == null)
                {
                    return NotFound($"Version {versionNumber} not found.");
                }

                return Ok(audit);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }


        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Contact/{id}/ChangeMetadataBetter")]
        public async Task<IActionResult> GetContactChangeMetadataBetter(int id, int versionNumber, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            // Load the contact to verify access and get tenant/context
            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);

            Database.Contact contact = await _context.Contacts.Where(c => c.id == id && c.tenantGuid == userTenantGuid).FirstOrDefaultAsync(cancellationToken);
            if (contact == null)
            {
                return NotFound();
            }

            try
            {
                contact.SetupVersionInquiry(_context, userTenantGuid);
                ChangeHistoryToolset<Database.Contact, ContactChangeHistory> toolset = Database.Contact.GetChangeHistoryToolsetForReading(_context, cancellationToken);


                VersionInformation<Contact> thisVersionMetaData = await contact.GetVersionAsync(versionNumber, false, cancellationToken);

                if (thisVersionMetaData == null)
                {
                    return NotFound($"Version {versionNumber} not found.");
                }

                //AuditEntry audit = await toolset.GetAuditForVersion(contact, versionNumber).ConfigureAwait(false);

                //if (audit == null)
                //{
                //    return NotFound($"Version {versionNumber} not found.");
                //}

                return Ok(thisVersionMetaData);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// Gets the full audit history for a Contact.
        /// 
        /// </summary>
        /// <param name="id">The contact ID</param>
        /// <param name="includeJson">Whether to include the full JSON data of the history records (Can be large)</param>
        /// <returns>List of AuditEntry items</returns>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Contact/{id}/AuditHistory")]
        public async Task<IActionResult> GetContactAuditHistory(int id, bool includeJson = false, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

             SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);

            Database.Contact contact = await _context.Contacts.Where(c => c.id == id && c.tenantGuid == userTenantGuid).FirstOrDefaultAsync(cancellationToken);
            if (contact == null)
            {
                return NotFound();
            }

            try
            {
                contact.SetupVersionInquiry(_context, userTenantGuid);
                
                if (includeJson)
                {
                    List<VersionInformation<Database.Contact>> versions = await contact.GetAllVersionsAsync(includeData: true, cancellationToken).ConfigureAwait(false);
                    return Ok(versions);
                }
                else
                {
                    //ChangeHistoryToolset<Database.Contact, ContactChangeHistory> toolset = Database.Contact.GetChangeHistoryToolsetForReading(_context, cancellationToken);
                    //List<AuditEntry> audits = await toolset.GetAuditTrailAsync(contact).ConfigureAwait(false);
                    //return Ok(audits);

                    List<VersionInformation<Database.Contact>> versions = await contact.GetAllVersionsAsync(includeData: false, cancellationToken).ConfigureAwait(false);
                    return Ok(versions);

                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// Gets a specific version of a Contact.
        /// 
        /// </summary>
        /// <param name="id">The contact ID</param>
        /// <param name="version">The version number</param>
        /// <returns>The Contact object at that version</returns>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Contact/{id}/Version/{version}")]
        public async Task<IActionResult> GetContactVersion(int id, int version, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);

            Database.Contact contact = await _context.Contacts.Where(c => c.id == id && c.tenantGuid == userTenantGuid).FirstOrDefaultAsync(cancellationToken);
            if (contact == null)
            {
                return NotFound();
            }

            try
            {
                contact.SetupVersionInquiry(_context, userTenantGuid);
                VersionInformation<Database.Contact> versionInfo = await contact.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);
                
                if (versionInfo == null || versionInfo.data == null)
                {
                    return NotFound();
                }

                return Ok(versionInfo.data.ToOutputDTO());
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// Gets the state of a Contact at a specific point in time.
        /// 
        /// </summary>
        /// <param name="id">The contact ID</param>
        /// <param name="time">The point in time (ISO format)</param>
        /// <returns>The Contact object at that time</returns>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Contact/{id}/StateAtTime")]
        public async Task<IActionResult> GetContactStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);

            Database.Contact contact = await _context.Contacts.Where(c => c.id == id && c.tenantGuid == userTenantGuid).FirstOrDefaultAsync(cancellationToken);
            if (contact == null)
            {
                return NotFound();
            }

            try
            {
                contact.SetupVersionInquiry(_context, userTenantGuid);
                VersionInformation<Database.Contact> versionInfo = await contact.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

                if (versionInfo == null || versionInfo.data == null)
                {
                     return NotFound("No state found at specified time.");
                }

                return Ok(versionInfo.data.ToOutputDTO());
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
