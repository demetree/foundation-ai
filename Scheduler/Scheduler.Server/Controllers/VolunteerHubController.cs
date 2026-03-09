//
// VolunteerHubController.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Standalone controller for the Volunteer Self-Service Hub.
// Uses OTP (2FA) via the existing SecurityUser infrastructure for passwordless auth.
// All endpoints are volunteer-scoped — a volunteer can only access their own data.
//
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Foundation.Security;
using Foundation.Security.Database;
using Foundation.Scheduler.Database;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    [ApiController]
    [Route("api/volunteerhub")]
    public class VolunteerHubController : ControllerBase
    {
        private readonly SchedulerContext _schedulerDb;
        private readonly ILogger<VolunteerHubController> _logger;

        public VolunteerHubController(
            SchedulerContext schedulerDb,
            ILogger<VolunteerHubController> logger)
        {
            _schedulerDb = schedulerDb;
            _logger = logger;
        }


        // ─────────────────────────────────────────────────────────────
        // AUTH: Request OTP Code
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Accepts an email or phone number, finds the matching SecurityUser,
        /// and sends a 6-digit OTP code via the existing 2FA infrastructure.
        /// </summary>
        [HttpPost("auth/request-code")]
        public async Task<IActionResult> RequestCode([FromBody] OtpRequestModel model)
        {
            if (string.IsNullOrWhiteSpace(model?.Identifier))
            {
                return BadRequest(new { message = "Email or phone number is required." });
            }

            string identifier = model.Identifier.Trim().ToLowerInvariant();

            try
            {
                using (SecurityContext securityDb = new SecurityContext())
                {
                    // Look up user by email or cell phone
                    SecurityUser user = await securityDb.SecurityUsers
                        .Where(u => u.active == true && u.deleted == false && u.canLogin == true)
                        .Where(u =>
                            u.emailAddress.ToLower() == identifier ||
                            u.cellPhoneNumber == model.Identifier.Trim())
                        .FirstOrDefaultAsync();

                    if (user == null)
                    {
                        // Don't reveal whether the account exists
                        _logger.LogWarning("VolunteerHub OTP request for unknown identifier: {Identifier}", identifier);
                        return Ok(new { message = "If an account exists with that email or phone, a code has been sent." });
                    }

                    // Verify this user is actually linked to a volunteer profile
                    bool isLinkedVolunteer = await _schedulerDb.VolunteerProfiles
                        .AnyAsync(vp => vp.linkedUserGuid == user.objectGuid
                                     && vp.active == true
                                     && vp.deleted == false);

                    if (!isLinkedVolunteer)
                    {
                        _logger.LogWarning("VolunteerHub OTP request for user {User} who is not linked to a volunteer profile.", user.accountName);
                        return Ok(new { message = "If an account exists with that email or phone, a code has been sent." });
                    }

                    // Ensure 2FA delivery is configured
                    if (user.twoFactorSendByEmail != true && user.twoFactorSendBySMS != true)
                    {
                        // Auto-configure based on what identifier they used
                        if (identifier.Contains('@'))
                        {
                            user.twoFactorSendByEmail = true;
                        }
                        else
                        {
                            user.twoFactorSendBySMS = true;
                        }
                        await securityDb.SaveChangesAsync();
                    }

                    // Generate and send OTP using existing infrastructure
                    string token = SecurityLogic.GenerateAndSendTwoFactorToken(user, timeoutMinutes: 10);

                    _logger.LogInformation("VolunteerHub OTP sent for user {User}", user.accountName);

                    return Ok(new { message = "If an account exists with that email or phone, a code has been sent." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OTP request for VolunteerHub");
                return StatusCode(500, new { message = "An error occurred. Please try again." });
            }
        }


        // ─────────────────────────────────────────────────────────────
        // AUTH: Verify OTP Code
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Verifies the 6-digit OTP code and returns a session token.
        /// </summary>
        [HttpPost("auth/verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] OtpVerifyModel model)
        {
            if (string.IsNullOrWhiteSpace(model?.Identifier) || string.IsNullOrWhiteSpace(model?.Code))
            {
                return BadRequest(new { message = "Email/phone and code are required." });
            }

            string identifier = model.Identifier.Trim().ToLowerInvariant();

            try
            {
                using (SecurityContext securityDb = new SecurityContext())
                {
                    SecurityUser user = await securityDb.SecurityUsers
                        .Where(u => u.active == true && u.deleted == false && u.canLogin == true)
                        .Where(u =>
                            u.emailAddress.ToLower() == identifier ||
                            u.cellPhoneNumber == model.Identifier.Trim())
                        .FirstOrDefaultAsync();

                    if (user == null)
                    {
                        return Unauthorized(new { message = "Invalid credentials." });
                    }

                    // Verify OTP code and expiry
                    if (user.twoFactorToken != model.Code.Trim())
                    {
                        _logger.LogWarning("VolunteerHub invalid OTP code for user {User}", user.accountName);
                        return Unauthorized(new { message = "Invalid or expired code." });
                    }

                    if (user.twoFactorTokenExpiry == null || user.twoFactorTokenExpiry < DateTime.UtcNow)
                    {
                        _logger.LogWarning("VolunteerHub expired OTP code for user {User}", user.accountName);
                        return Unauthorized(new { message = "Invalid or expired code." });
                    }

                    // OTP valid — generate session token
                    string sessionToken = Guid.NewGuid().ToString("N");
                    DateTime sessionExpiry = DateTime.UtcNow.AddDays(7);

                    user.authenticationToken = sessionToken;
                    user.authenticationTokenExpiry = sessionExpiry;

                    // Clear the OTP so it can't be reused
                    user.twoFactorToken = null;
                    user.twoFactorTokenExpiry = null;

                    await securityDb.SaveChangesAsync();

                    _logger.LogInformation("VolunteerHub session created for user {User}, expires {Expiry}",
                        user.accountName, sessionExpiry);

                    return Ok(new
                    {
                        sessionToken = sessionToken,
                        expiresAt = sessionExpiry,
                        userName = user.firstName ?? user.accountName
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for VolunteerHub");
                return StatusCode(500, new { message = "An error occurred. Please try again." });
            }
        }


        // ─────────────────────────────────────────────────────────────
        // AUTH: Validate Session
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Validates the session token and returns the volunteer's profile.
        /// </summary>
        [HttpGet("auth/session")]
        public async Task<IActionResult> GetSession()
        {
            SessionResolution session = await ResolveSessionUserAsync();
            if (session.User == null) return session.ErrorResult!;
            SecurityUser user = session.User;

            var profile = await GetVolunteerProfileForUserAsync(user);
            if (profile == null) return Unauthorized(new { message = "No volunteer profile linked." });

            return Ok(new
            {
                userName = user.firstName ?? user.accountName,
                expiresAt = user.authenticationTokenExpiry,
                volunteer = profile.ToOutputDTO()
            });
        }


        // ─────────────────────────────────────────────────────────────
        // DATA: My Profile
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the current volunteer's full profile.
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            SessionResolution session = await ResolveSessionUserAsync();
            if (session.User == null) return session.ErrorResult!;
            SecurityUser user = session.User;

            var profile = await GetVolunteerProfileForUserAsync(user);
            if (profile == null) return NotFound(new { message = "Volunteer profile not found." });

            return Ok(profile.ToOutputDTO());
        }


        // ─────────────────────────────────────────────────────────────
        // DATA: My Assignments
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the volunteer's event resource assignments.
        /// </summary>
        [HttpGet("me/assignments")]
        public async Task<IActionResult> GetMyAssignments(
            DateTime? from = null,
            DateTime? to = null)
        {
            SessionResolution session = await ResolveSessionUserAsync();
            if (session.User == null) return session.ErrorResult!;
            SecurityUser user = session.User;

            var profile = await GetVolunteerProfileForUserAsync(user);
            if (profile == null) return NotFound(new { message = "Volunteer profile not found." });

            var query = _schedulerDb.EventResourceAssignments
                .Where(a => a.resourceId == profile.resourceId)
                .Where(a => a.active == true && a.deleted == false)
                .Include(a => a.scheduledEvent)
                .Include(a => a.assignmentRole)
                .Include(a => a.assignmentStatus)
                .AsNoTracking();

            if (from.HasValue)
            {
                query = query.Where(a =>
                    a.assignmentEndDateTime >= from.Value ||
                    (a.assignmentEndDateTime == null && a.scheduledEvent.endDateTime >= from.Value));
            }

            if (to.HasValue)
            {
                query = query.Where(a =>
                    a.assignmentStartDateTime <= to.Value ||
                    (a.assignmentStartDateTime == null && a.scheduledEvent.startDateTime <= to.Value));
            }

            var assignments = await query
                .OrderByDescending(a => a.assignmentStartDateTime ?? a.scheduledEvent.startDateTime)
                .ToListAsync();

            var result = assignments.Select(a => new
            {
                id = a.id,
                eventName = a.scheduledEvent?.name,
                eventDescription = a.scheduledEvent?.description,
                startDateTime = a.assignmentStartDateTime ?? a.scheduledEvent?.startDateTime,
                endDateTime = a.assignmentEndDateTime ?? a.scheduledEvent?.endDateTime,
                role = a.assignmentRole?.name,
                status = a.assignmentStatus?.name,
                reportedHours = a.reportedVolunteerHours,
                approvedHours = a.approvedVolunteerHours,
                isVolunteer = a.isVolunteer,
                notes = a.volunteerNotes
            });

            return Ok(result);
        }


        // ─────────────────────────────────────────────────────────────
        // DATA: Report Hours
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Allows a volunteer to report hours for a specific assignment.
        /// </summary>
        [HttpPost("me/assignments/{assignmentId}/report-hours")]
        public async Task<IActionResult> ReportHours(int assignmentId, [FromBody] ReportHoursModel model)
        {
            SessionResolution session = await ResolveSessionUserAsync();
            if (session.User == null) return session.ErrorResult!;
            SecurityUser user = session.User;

            var profile = await GetVolunteerProfileForUserAsync(user);
            if (profile == null) return NotFound(new { message = "Volunteer profile not found." });

            var assignment = await _schedulerDb.EventResourceAssignments
                .Where(a => a.id == assignmentId)
                .Where(a => a.resourceId == profile.resourceId)
                .Where(a => a.active == true && a.deleted == false)
                .FirstOrDefaultAsync();

            if (assignment == null)
            {
                return NotFound(new { message = "Assignment not found." });
            }

            assignment.reportedVolunteerHours = model.Hours;
            assignment.volunteerNotes = model.Notes;
            await _schedulerDb.SaveChangesAsync();

            _logger.LogInformation(
                "VolunteerHub: User {User} reported {Hours} hours for assignment {AssignmentId}",
                user.accountName, model.Hours, assignmentId);

            return Ok(new { message = "Hours reported successfully.", reportedHours = model.Hours });
        }


        // ─────────────────────────────────────────────────────────────
        // DATA: Respond to Assignment (Accept/Decline)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Allows a volunteer to accept or decline an assignment.
        /// </summary>
        [HttpPost("me/assignments/{assignmentId}/respond")]
        public async Task<IActionResult> RespondToAssignment(int assignmentId, [FromBody] RespondToAssignmentModel model)
        {
            SessionResolution session = await ResolveSessionUserAsync();
            if (session.User == null) return session.ErrorResult!;
            SecurityUser user = session.User;

            var profile = await GetVolunteerProfileForUserAsync(user);
            if (profile == null) return NotFound(new { message = "Volunteer profile not found." });

            var assignment = await _schedulerDb.EventResourceAssignments
                .Where(a => a.id == assignmentId)
                .Where(a => a.resourceId == profile.resourceId)
                .Where(a => a.active == true && a.deleted == false)
                .Include(a => a.assignmentStatus)
                .FirstOrDefaultAsync();

            if (assignment == null)
            {
                return NotFound(new { message = "Assignment not found." });
            }

            // Look up the target status by name
            string targetStatusName = model.Accepted ? "Confirmed" : "Declined";
            var targetStatus = await _schedulerDb.AssignmentStatuses
                .Where(s => s.name == targetStatusName && s.active == true && s.deleted == false)
                .FirstOrDefaultAsync();

            if (targetStatus != null)
            {
                assignment.assignmentStatusId = targetStatus.id;
            }

            await _schedulerDb.SaveChangesAsync();

            _logger.LogInformation(
                "VolunteerHub: User {User} {Response} assignment {AssignmentId}",
                user.accountName, model.Accepted ? "accepted" : "declined", assignmentId);

            return Ok(new { message = $"Assignment {targetStatusName.ToLower()} successfully.", status = targetStatusName });
        }


        // ─────────────────────────────────────────────────────────────
        // DATA: Update Profile
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Allows a volunteer to update their own profile fields.
        /// </summary>
        [HttpPut("me/profile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileModel model)
        {
            SessionResolution session = await ResolveSessionUserAsync();
            if (session.User == null) return session.ErrorResult!;
            SecurityUser user = session.User;

            var profile = await _schedulerDb.VolunteerProfiles
                .Where(vp => vp.linkedUserGuid == user.objectGuid)
                .Where(vp => vp.active == true && vp.deleted == false)
                .FirstOrDefaultAsync();

            if (profile == null) return NotFound(new { message = "Volunteer profile not found." });

            if (model.AvailabilityPreferences != null)
                profile.availabilityPreferences = model.AvailabilityPreferences;

            if (model.InterestsAndSkillsNotes != null)
                profile.interestsAndSkillsNotes = model.InterestsAndSkillsNotes;

            if (model.EmergencyContactNotes != null)
                profile.emergencyContactNotes = model.EmergencyContactNotes;

            await _schedulerDb.SaveChangesAsync();

            _logger.LogInformation("VolunteerHub: User {User} updated their profile", user.accountName);

            return Ok(new { message = "Profile updated successfully." });
        }


        // ─────────────────────────────────────────────────────────────
        // AUTH: Logout
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Invalidates the volunteer's hub session.
        /// </summary>
        [HttpPost("auth/logout")]
        public async Task<IActionResult> Logout()
        {
            SessionResolution session = await ResolveSessionUserAsync();
            if (session.User == null) return session.ErrorResult!;
            SecurityUser user = session.User;

            using (SecurityContext securityDb = new SecurityContext())
            {
                // Re-fetch the user from a writable context
                SecurityUser? dbUser = await securityDb.SecurityUsers
                    .Where(u => u.objectGuid == user.objectGuid)
                    .FirstOrDefaultAsync();

                if (dbUser != null)
                {
                    dbUser.authenticationToken = null;
                    dbUser.authenticationTokenExpiry = null;
                    await securityDb.SaveChangesAsync();
                }
            }

            _logger.LogInformation("VolunteerHub: User {User} logged out", user.accountName);

            return Ok(new { message = "Logged out successfully." });
        }


        // ─────────────────────────────────────────────────────────────
        // ADMIN: Provision Hub Access for a Volunteer
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates (or reuses) a SecurityUser record for a volunteer, links it to
        /// the VolunteerProfile, and configures 2FA delivery for OTP login.
        /// Called from the admin volunteer add/edit form.
        /// </summary>
        [Authorize]
        [HttpPost("admin/provision-access")]
        public async Task<IActionResult> ProvisionAccess([FromBody] ProvisionAccessModel model)
        {
            if (string.IsNullOrWhiteSpace(model?.Email))
            {
                return BadRequest(new { message = "Email is required." });
            }

            if (model.VolunteerProfileId <= 0)
            {
                return BadRequest(new { message = "Volunteer profile ID is required." });
            }

            try
            {
                // Verify the volunteer profile exists
                VolunteerProfile? volunteerProfile = await _schedulerDb.VolunteerProfiles
                    .Include(vp => vp.resource)
                    .Where(vp => vp.id == model.VolunteerProfileId)
                    .FirstOrDefaultAsync();

                if (volunteerProfile == null)
                {
                    return NotFound(new { message = "Volunteer profile not found." });
                }

                string email = model.Email.Trim().ToLowerInvariant();
                string? phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
                string firstName = model.FirstName?.Trim() ?? "";
                string lastName = model.LastName?.Trim() ?? "";

                Guid linkedUserGuid;
                string accountName;

                using (SecurityContext securityDb = new SecurityContext())
                {
                    // Check if a SecurityUser already exists with this email
                    SecurityUser? existingUser = await securityDb.SecurityUsers
                        .Where(u => u.accountName.ToLower() == email || u.emailAddress.ToLower() == email)
                        .FirstOrDefaultAsync();

                    if (existingUser != null)
                    {
                        // Refuse if this is a real system account (AD user or non-volunteer-hub account)
                        bool isSystemAccount = existingUser.activeDirectoryAccount == true
                            || (existingUser.description != null && !existingUser.description.StartsWith("Volunteer Hub access provisioned"));

                        if (isSystemAccount)
                        {
                            return Conflict(new { message = "This email belongs to an existing system user account. Please use a different email address for Volunteer Hub access." });
                        }

                        // Check if another volunteer profile is already linked to this SecurityUser
                        bool alreadyLinked = await _schedulerDb.VolunteerProfiles
                            .AnyAsync(vp => vp.linkedUserGuid == existingUser.objectGuid
                                         && vp.id != model.VolunteerProfileId
                                         && vp.active == true
                                         && vp.deleted == false);

                        if (alreadyLinked)
                        {
                            return Conflict(new { message = "This email is already linked to another volunteer profile." });
                        }

                        // Reuse existing volunteer hub user — ensure 2FA delivery is configured
                        if (existingUser.twoFactorSendByEmail != true)
                        {
                            existingUser.twoFactorSendByEmail = true;
                        }
                        if (phone != null && existingUser.twoFactorSendBySMS != true)
                        {
                            existingUser.cellPhoneNumber = phone;
                            existingUser.twoFactorSendBySMS = true;
                        }

                        existingUser.canLogin = true;
                        await securityDb.SaveChangesAsync();

                        linkedUserGuid = existingUser.objectGuid;
                        accountName = existingUser.accountName;

                        _logger.LogInformation(
                            "VolunteerHub: Reused existing SecurityUser '{AccountName}' for volunteer profile {ProfileId}",
                            accountName, model.VolunteerProfileId);
                    }
                    else
                    {
                        // Create a new SecurityUser for this volunteer
                        SecurityUser newUser = new SecurityUser();

                        newUser.objectGuid = Guid.NewGuid();
                        newUser.accountName = email;
                        newUser.emailAddress = email;

                        // Random GUID password — prevents login via admin credentials
                        newUser.password = SecurityLogic.SecurePasswordHasher.Hash(Guid.NewGuid().ToString("N"));

                        newUser.firstName = firstName;
                        newUser.lastName = lastName;
                        newUser.description = $"Volunteer Hub access provisioned for {firstName} {lastName}".Trim();

                        newUser.cellPhoneNumber = phone;

                        newUser.twoFactorSendByEmail = true;
                        newUser.twoFactorSendBySMS = phone != null;

                        newUser.activeDirectoryAccount = false;
                        newUser.canLogin = true;
                        newUser.failedLoginCount = 0;
                        newUser.mostRecentActivity = DateTime.UtcNow;

                        newUser.active = true;
                        newUser.deleted = false;

                        securityDb.SecurityUsers.Add(newUser);
                        await securityDb.SaveChangesAsync();

                        linkedUserGuid = newUser.objectGuid;
                        accountName = newUser.accountName;

                        _logger.LogInformation(
                            "VolunteerHub: Created new SecurityUser '{AccountName}' (objectGuid: {Guid}) for volunteer profile {ProfileId}",
                            accountName, linkedUserGuid, model.VolunteerProfileId);
                    }
                }

                // Link the volunteer profile to the SecurityUser
                volunteerProfile.linkedUserGuid = linkedUserGuid;
                await _schedulerDb.SaveChangesAsync();

                return Ok(new
                {
                    linkedUserGuid = linkedUserGuid,
                    accountName = accountName,
                    message = "Hub access provisioned successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error provisioning hub access for volunteer profile {ProfileId}", model.VolunteerProfileId);
                return StatusCode(500, new { message = "An error occurred while provisioning access." });
            }
        }


        // ─────────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Resolves the session token from the request header and returns the SecurityUser.
        /// </summary>
        private async Task<SessionResolution> ResolveSessionUserAsync()
        {
            string? token = Request.Headers["X-Volunteer-Session"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(token))
            {
                return new SessionResolution(null, Unauthorized(new { message = "Session token required." }));
            }

            using (SecurityContext securityDb = new SecurityContext())
            {
                SecurityUser? user = await securityDb.SecurityUsers
                    .Where(u => u.authenticationToken == token)
                    .Where(u => u.active == true && u.deleted == false)
                    .FirstOrDefaultAsync();

                if (user == null || user.authenticationTokenExpiry == null || user.authenticationTokenExpiry < DateTime.UtcNow)
                {
                    return new SessionResolution(null, Unauthorized(new { message = "Session expired or invalid." }));
                }

                return new SessionResolution(user, null);
            }
        }

        /// <summary>
        /// Gets the VolunteerProfile linked to the given SecurityUser.
        /// </summary>
        private async Task<VolunteerProfile?> GetVolunteerProfileForUserAsync(SecurityUser user)
        {
            return await _schedulerDb.VolunteerProfiles
                .Where(vp => vp.linkedUserGuid == user.objectGuid)
                .Where(vp => vp.active == true && vp.deleted == false)
                .Include(vp => vp.resource)
                .Include(vp => vp.volunteerStatus)
                .FirstOrDefaultAsync();
        }


        // ─────────────────────────────────────────────────────────────
        // PUBLIC: Volunteer Self-Registration
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Public endpoint — no auth required.
        /// Accepts a volunteer registration form and creates a VolunteerProfile
        /// with "Pending" status for admin review.
        /// </summary>
        [HttpPost("public/register")]
        public async Task<IActionResult> Register([FromBody] VolunteerRegistrationModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email))
                return BadRequest(new { error = "Email is required." });

            if (string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.LastName))
                return BadRequest(new { error = "First name and last name are required." });

            try
            {
                // Check for duplicate registration by email
                var securityContext = new SecurityContext();
                var existingUser = await securityContext.SecurityUsers
                    .FirstOrDefaultAsync(u => u.emailAddress == model.Email.Trim().ToLower());

                if (existingUser != null)
                {
                    return Conflict(new { error = "An account with this email already exists. Please log in to the Volunteer Hub." });
                }

                // Resolve the "Pending" volunteer status (convention: status name "Pending")
                var pendingStatus = await _schedulerDb.VolunteerStatuses
                    .FirstOrDefaultAsync(s => s.name == "Pending");

                if (pendingStatus == null)
                {
                    // Fallback: use the first status
                    pendingStatus = await _schedulerDb.VolunteerStatuses.FirstOrDefaultAsync();
                }

                if (pendingStatus == null)
                    return StatusCode(500, new { error = "No volunteer statuses configured." });

                // Get default tenant
                var tenantGuid = await GetDefaultTenantGuidAsync();

                // Create a minimal Resource for this volunteer
                var resource = new Resource
                {
                    tenantGuid = tenantGuid,
                    name = $"{model.FirstName.Trim()} {model.LastName.Trim()}",
                    active = true,
                    deleted = false,
                    objectGuid = Guid.NewGuid(),
                    versionNumber = 1
                };
                _schedulerDb.Resources.Add(resource);
                await _schedulerDb.SaveChangesAsync();

                // Create the VolunteerProfile in "Pending" status
                var profile = new VolunteerProfile
                {
                    tenantGuid = tenantGuid,
                    resourceId = resource.id,
                    volunteerStatusId = pendingStatus.id,
                    availabilityPreferences = model.AvailabilityPreferences,
                    interestsAndSkillsNotes = model.InterestsAndSkillsNotes,
                    emergencyContactNotes = model.EmergencyContactNotes,
                    // Store email in attributes JSON for later retrieval during approval
                    attributes = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        registrationEmail = model.Email.Trim().ToLower(),
                        registrationPhone = model.Phone,
                        registrationFirstName = model.FirstName.Trim(),
                        registrationLastName = model.LastName.Trim(),
                        registrationDate = DateTime.UtcNow
                    }),
                    active = true,
                    deleted = false,
                    objectGuid = Guid.NewGuid(),
                    versionNumber = 1
                };
                _schedulerDb.VolunteerProfiles.Add(profile);
                await _schedulerDb.SaveChangesAsync();

                _logger.LogInformation("Volunteer self-registration created: profile {Id} for {Email}",
                    profile.id, model.Email);

                return Ok(new
                {
                    message = "Registration submitted! An administrator will review your application.",
                    profileId = profile.id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing volunteer self-registration for {Email}", model.Email);
                return StatusCode(500, new { error = "Failed to process registration." });
            }
        }


        // ─────────────────────────────────────────────────────────────
        // ADMIN: Pending Registrations Management
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Admin: Get pending volunteer registrations (profiles with "Pending" status).
        /// </summary>
        [Authorize]
        [HttpGet("admin/registrations")]
        public async Task<IActionResult> GetPendingRegistrations()
        {
            var pendingStatus = await _schedulerDb.VolunteerStatuses
                .FirstOrDefaultAsync(s => s.name == "Pending");

            if (pendingStatus == null)
                return Ok(Array.Empty<object>());

            var pending = await _schedulerDb.VolunteerProfiles
                .Include(vp => vp.resource)
                .Include(vp => vp.volunteerStatus)
                .Where(vp => vp.volunteerStatusId == pendingStatus.id && vp.active && !vp.deleted)
                .OrderByDescending(vp => vp.id)
                .Select(vp => new
                {
                    vp.id,
                    vp.resource.name,
                    statusName = vp.volunteerStatus.name,
                    vp.availabilityPreferences,
                    vp.interestsAndSkillsNotes,
                    vp.emergencyContactNotes,
                    vp.attributes // Contains registration email, phone, date
                })
                .ToListAsync();

            return Ok(pending);
        }

        /// <summary>
        /// Admin: Approve a pending registration — creates SecurityUser and provisions Hub access.
        /// </summary>
        [Authorize]
        [HttpPost("admin/registrations/{profileId}/approve")]
        public async Task<IActionResult> ApproveRegistration(int profileId)
        {
            var profile = await _schedulerDb.VolunteerProfiles
                .Include(vp => vp.resource)
                .FirstOrDefaultAsync(vp => vp.id == profileId);

            if (profile == null)
                return NotFound(new { error = "Profile not found." });

            try
            {
                // Parse registration data from attributes
                var regData = System.Text.Json.JsonDocument.Parse(profile.attributes ?? "{}");
                var email = regData.RootElement.TryGetProperty("registrationEmail", out var emailEl) ? emailEl.GetString() : null;
                var phone = regData.RootElement.TryGetProperty("registrationPhone", out var phoneEl) ? phoneEl.GetString() : null;
                var firstName = regData.RootElement.TryGetProperty("registrationFirstName", out var fnEl) ? fnEl.GetString() : null;
                var lastName = regData.RootElement.TryGetProperty("registrationLastName", out var lnEl) ? lnEl.GetString() : null;

                if (string.IsNullOrWhiteSpace(email))
                    return BadRequest(new { error = "Registration email not found in profile attributes." });

                // Move to "Active" status
                var activeStatus = await _schedulerDb.VolunteerStatuses
                    .FirstOrDefaultAsync(s => s.name == "Active");

                if (activeStatus != null)
                {
                    profile.volunteerStatusId = activeStatus.id;
                    profile.onboardedDate = DateOnly.FromDateTime(DateTime.UtcNow);
                }

                await _schedulerDb.SaveChangesAsync();

                // Provision Hub access (reuse existing provisioning logic pattern)
                var securityContext = new SecurityContext();

                var secUser = await securityContext.SecurityUsers
                    .FirstOrDefaultAsync(u => u.emailAddress == email);

                if (secUser == null)
                {
                    // Create new SecurityUser for this volunteer
                    secUser = new SecurityUser
                    {
                        accountName = email,
                        emailAddress = email,
                        cellPhoneNumber = phone,
                        firstName = firstName,
                        lastName = lastName,
                        canLogin = true,
                        activeDirectoryAccount = false,
                        active = true,
                        deleted = false,
                        objectGuid = Guid.NewGuid(),
                        twoFactorSendByEmail = true,
                        twoFactorSendBySMS = !string.IsNullOrWhiteSpace(phone)
                    };
                    securityContext.SecurityUsers.Add(secUser);
                    await securityContext.SaveChangesAsync();
                }

                // Link profile to SecurityUser
                profile.linkedUserGuid = secUser.objectGuid;
                await _schedulerDb.SaveChangesAsync();

                _logger.LogInformation("Volunteer registration approved: profile {Id}, email {Email}", profileId, email);

                return Ok(new
                {
                    message = "Registration approved. Volunteer can now log in to the Hub.",
                    profileId = profile.id,
                    securityUserId = secUser.id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving registration for profile {Id}", profileId);
                return StatusCode(500, new { error = "Failed to approve registration." });
            }
        }

        /// <summary>
        /// Admin: Reject a pending registration.
        /// </summary>
        [Authorize]
        [HttpPost("admin/registrations/{profileId}/reject")]
        public async Task<IActionResult> RejectRegistration(int profileId, [FromBody] RejectRegistrationModel model)
        {
            var profile = await _schedulerDb.VolunteerProfiles
                .FirstOrDefaultAsync(vp => vp.id == profileId);

            if (profile == null)
                return NotFound(new { error = "Profile not found." });

            // Mark as inactive/rejected
            var inactiveStatus = await _schedulerDb.VolunteerStatuses
                .FirstOrDefaultAsync(s => s.name == "Inactive");

            if (inactiveStatus != null)
            {
                profile.volunteerStatusId = inactiveStatus.id;
            }

            profile.active = false;
            profile.inactiveSince = DateOnly.FromDateTime(DateTime.UtcNow);

            // Store rejection notes in attributes
            if (!string.IsNullOrWhiteSpace(model?.Notes))
            {
                try
                {
                    var existing = System.Text.Json.JsonDocument.Parse(profile.attributes ?? "{}");
                    var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(profile.attributes ?? "{}");
                    dict["rejectionNotes"] = model.Notes;
                    dict["rejectionDate"] = DateTime.UtcNow.ToString("o");
                    profile.attributes = System.Text.Json.JsonSerializer.Serialize(dict);
                }
                catch
                {
                    // If attributes parsing fails, just append
                }
            }

            await _schedulerDb.SaveChangesAsync();

            _logger.LogInformation("Volunteer registration rejected: profile {Id}", profileId);

            return Ok(new { message = "Registration rejected." });
        }


        // ─────────────────────────────────────────────────────────────
        // OPPORTUNITIES: Browse & Sign-Up
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Authenticated volunteer: Browse upcoming events that are open for volunteer sign-up.
        /// Returns events with available slot counts.
        /// </summary>
        [HttpGet("opportunities")]
        public async Task<IActionResult> GetOpportunities()
        {
            var session = await ResolveSessionAsync();
            if (session == null) return Unauthorized(new { error = "Invalid session." });

            var now = DateTime.UtcNow;

            // Get upcoming events that are open for volunteers
            // Using convention: events with 'volunteer' in name/description or events that
            // already have volunteer assignments are considered volunteer-eligible.
            // TODO: Once isOpenForVolunteers column is added to ScheduledEvent, use that instead.
            var events = await _schedulerDb.ScheduledEvents
                .Include(e => e.EventResourceAssignments)
                .Where(e =>
                    e.active &&
                    !e.deleted &&
                    e.startDateTime > now &&
                    e.tenantGuid == session.TenantGuid)
                .OrderBy(e => e.startDateTime)
                .Take(50)
                .Select(e => new
                {
                    e.id,
                    e.name,
                    e.description,
                    e.location,
                    e.startDateTime,
                    e.endDateTime,
                    totalVolunteerSlots = (int?)null, // TODO: use e.maxVolunteerSlots once column added
                    currentVolunteers = e.EventResourceAssignments
                        .Count(a => a.isVolunteer && a.active && !a.deleted),
                    isAlreadySignedUp = e.EventResourceAssignments
                        .Any(a => a.isVolunteer && a.active && !a.deleted &&
                                  a.resourceId == session.ResourceId)
                })
                .ToListAsync();

            return Ok(events);
        }

        /// <summary>
        /// Authenticated volunteer: Sign up for an opportunity.
        /// Creates a new volunteer EventResourceAssignment.
        /// </summary>
        [HttpPost("opportunities/{eventId}/sign-up")]
        public async Task<IActionResult> SignUpForOpportunity(int eventId)
        {
            var session = await ResolveSessionAsync();
            if (session == null) return Unauthorized(new { error = "Invalid session." });

            var scheduledEvent = await _schedulerDb.ScheduledEvents
                .Include(e => e.EventResourceAssignments)
                .FirstOrDefaultAsync(e => e.id == eventId && e.active && !e.deleted);

            if (scheduledEvent == null)
                return NotFound(new { error = "Event not found." });

            if (scheduledEvent.startDateTime <= DateTime.UtcNow)
                return BadRequest(new { error = "Cannot sign up for past events." });

            // Check for duplicate sign-up
            var existing = scheduledEvent.EventResourceAssignments
                .FirstOrDefault(a => a.isVolunteer && a.resourceId == session.ResourceId && a.active && !a.deleted);

            if (existing != null)
                return Conflict(new { error = "You are already signed up for this event." });

            // TODO: Check slot availability once maxVolunteerSlots column is added

            // Get default "Planned" assignment status
            var plannedStatus = await _schedulerDb.AssignmentStatuses
                .FirstOrDefaultAsync(s => s.name == "Planned");

            var assignment = new EventResourceAssignment
            {
                tenantGuid = session.TenantGuid,
                scheduledEventId = eventId,
                resourceId = session.ResourceId,
                assignmentStatusId = plannedStatus?.id ?? 1,
                isVolunteer = true,
                active = true,
                deleted = false,
                objectGuid = Guid.NewGuid(),
                versionNumber = 1
            };

            _schedulerDb.EventResourceAssignments.Add(assignment);
            await _schedulerDb.SaveChangesAsync();

            _logger.LogInformation("Volunteer {ResourceId} signed up for event {EventId}",
                session.ResourceId, eventId);

            return Ok(new
            {
                message = "Successfully signed up!",
                assignmentId = assignment.id,
                eventName = scheduledEvent.name,
                eventDate = scheduledEvent.startDateTime
            });
        }


        // ─────────────────────────────────────────────────────────────
        // Internal: Session resolution helper for opportunity endpoints
        // ─────────────────────────────────────────────────────────────

        private async Task<VolunteerSession> ResolveSessionAsync()
        {
            var token = HttpContext.Request.Headers["X-Volunteer-Session"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(token)) return null;

            var securityContext = new SecurityContext();
            var secUser = await securityContext.SecurityUsers
                .FirstOrDefaultAsync(u => u.authenticationToken == token &&
                                          u.authenticationTokenExpiry > DateTime.UtcNow);

            if (secUser == null) return null;

            // Resolve volunteer profile
            var profile = await _schedulerDb.VolunteerProfiles
                .FirstOrDefaultAsync(vp => vp.linkedUserGuid == secUser.objectGuid && vp.active && !vp.deleted);

            if (profile == null) return null;

            // Get tenant from a tenant user mapping
            var tenantUser = await securityContext.SecurityTenantUsers
                .FirstOrDefaultAsync(tu => tu.securityUserId == secUser.id);

            return new VolunteerSession
            {
                SecurityUserId = secUser.id,
                UserGuid = secUser.objectGuid,
                ResourceId = profile.resourceId,
                ProfileId = profile.id,
                TenantGuid = tenantUser?.securityTenant?.objectGuid ?? Guid.Empty
            };
        }

        private async Task<Guid> GetDefaultTenantGuidAsync()
        {
            var securityContext = new SecurityContext();
            var tenant = await securityContext.SecurityTenants.FirstOrDefaultAsync();
            return tenant?.objectGuid ?? Guid.Empty;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Request / Response Models
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Result of resolving a volunteer hub session from the request header.
    /// </summary>
    public record SessionResolution(SecurityUser? User, IActionResult? ErrorResult);

    public class OtpRequestModel
    {
        public string Identifier { get; set; } = "";
    }

    public class OtpVerifyModel
    {
        public string Identifier { get; set; } = "";
        public string Code { get; set; } = "";
    }

    public class ProvisionAccessModel
    {
        public int VolunteerProfileId { get; set; }
        public string Email { get; set; } = "";
        public string? Phone { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public class ReportHoursModel
    {
        public float Hours { get; set; }
        public string? Notes { get; set; }
    }

    public class RespondToAssignmentModel
    {
        public bool Accepted { get; set; }
    }

    public class UpdateProfileModel
    {
        public string? AvailabilityPreferences { get; set; }
        public string? InterestsAndSkillsNotes { get; set; }
        public string? EmergencyContactNotes { get; set; }
    }

    public class VolunteerRegistrationModel
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Phone { get; set; }
        public string? AvailabilityPreferences { get; set; }
        public string? InterestsAndSkillsNotes { get; set; }
        public string? EmergencyContactNotes { get; set; }
    }

    public class RejectRegistrationModel
    {
        public string? Notes { get; set; }
    }

    public class VolunteerSession
    {
        public int SecurityUserId { get; set; }
        public Guid UserGuid { get; set; }
        public int ResourceId { get; set; }
        public int ProfileId { get; set; }
        public Guid TenantGuid { get; set; }
    }
}
