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

                    _logger.LogInformation("VolunteerHub OTP sent for user {User}. Code: {Code} (remove in production)",
                        user.accountName, token);

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
            var (user, errorResult) = await ResolveSessionUserAsync();
            if (user == null) return errorResult!;

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
            var (user, errorResult) = await ResolveSessionUserAsync();
            if (user == null) return errorResult!;

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
            var (user, errorResult) = await ResolveSessionUserAsync();
            if (user == null) return errorResult!;

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
        // HELPERS
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Resolves the session token from the request header and returns the SecurityUser.
        /// </summary>
        private async Task<(SecurityUser?, IActionResult?)> ResolveSessionUserAsync()
        {
            string? token = Request.Headers["X-Volunteer-Session"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(token))
            {
                return (null, Unauthorized(new { message = "Session token required." }));
            }

            using (SecurityContext securityDb = new SecurityContext())
            {
                SecurityUser? user = await securityDb.SecurityUsers
                    .Where(u => u.authenticationToken == token)
                    .Where(u => u.active == true && u.deleted == false)
                    .FirstOrDefaultAsync();

                if (user == null || user.authenticationTokenExpiry == null || user.authenticationTokenExpiry < DateTime.UtcNow)
                {
                    return (null, Unauthorized(new { message = "Session expired or invalid." }));
                }

                return (user, null);
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

    public class OtpRequestModel
    {
        public string Identifier { get; set; } = "";
    }

    public class OtpVerifyModel
    {
        public string Identifier { get; set; } = "";
        public string Code { get; set; } = "";
    }
}
