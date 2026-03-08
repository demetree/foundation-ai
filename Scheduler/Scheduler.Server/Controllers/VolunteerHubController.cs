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
    }


    // ─────────────────────────────────────────────────────────────
    // REQUEST MODELS
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
}
