using Foundation.Auditor;
using Foundation.Controllers;
using Foundation.Security;
using Foundation.Security.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace Foundation.Security.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// Controller for managing user-specific settings.
    /// 
    /// Provides endpoints for getting and setting string/object settings that are persisted
    /// per-user in the SecurityUser.settings JSON field via the Foundation UserSettings API.
    /// 
    /// </summary>
    [ApiController]
    public class UserSettingsController : SecureWebAPIController
    {

        //
        // Permission level required to read settings (user's own settings)
        //
        private const int READ_PERMISSION_LEVEL_REQUIRED = 1;

        //
        // Permission level required to write settings (user's own settings)
        //
        private const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

        //
        // Admin permission level required to read/write other users' settings
        //
        private const int ADMIN_READ_PERMISSION_LEVEL_REQUIRED = 3;
        private const int ADMIN_WRITE_PERMISSION_LEVEL_REQUIRED = 3;

        private readonly SecurityContext _context;


        public UserSettingsController(SecurityContext context) : base("Security", "UserSettings")
        { 
            _context = context;
        }


        /// <summary>
        /// 
        /// Gets a string setting value for the current user.
        /// 
        /// </summary>
        /// <param name="key">The setting key/name</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>The setting value as a string, or null if not set</returns>
        [HttpGet]
        [RateLimit(RateLimitOption.OneHundredPerMinute, Scope = RateLimitScope.PerUser)]
        [Route("api/UserSettings/{key}")]
        public async Task<IActionResult> GetStringSetting(string key, CancellationToken cancellationToken = default)
        {
            //
            // Permission check - This is a cross module function for a user to read their settings.  We do not enforce the security module readership role here.
            //

            //
            // Validate input
            //
            if (string.IsNullOrWhiteSpace(key))
            {
                return BadRequest("Setting key cannot be empty.");
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();

                if (securityUser == null)
                {
                    return Unauthorized("User not found.");
                }

                //
                // Must have read permission level of at least one to read this
                //
                if (securityUser.readPermissionLevel < 0)
                {
                    return Forbid();
                }


                //
                // Retrieve the setting using Foundation's UserSettings API
                //
                string value = await UserSettings.GetStringSettingAsync(key, securityUser, cancellationToken);

                //
                // Return the value (can be null if not set)
                //
                return Ok(new { key = key, value = value });
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.Error,
                    $"Error getting user setting '{key}'.",
                    ex.Message,
                    ex);

                return Problem($"Could not retrieve setting '{key}'.");
            }
        }


        /// <summary>
        /// 
        /// Sets a string setting value for the current user.
        /// 
        /// </summary>
        /// <param name="key">The setting key/name</param>
        /// <param name="request">The request body containing the value</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Success indicator</returns>
        [HttpPut]
        [RateLimit(RateLimitOption.OneHundredPerMinute, Scope = RateLimitScope.PerUser)]
        [Route("api/UserSettings/{key}")]
        public async Task<IActionResult> SetStringSetting(string key, [FromBody] SetSettingRequest request, CancellationToken cancellationToken = default)
        {
            //
            // Permission check - This is a cross module function for a user to read their settings.  We do not enforce the security module readership role here.
            //

            //
            // Validate input
            //
            if (string.IsNullOrWhiteSpace(key))
            {
                return BadRequest("Setting key cannot be empty.");
            }

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();

                if (securityUser == null)
                {
                    return Unauthorized("User not found.");
                }

                //
                // Must have read permission level of at least one to read this
                //
                if (securityUser.readPermissionLevel < 0)
                {
                    return Forbid();
                }

                //
                // Set the setting using Foundation's UserSettings API
                //
                bool success = await UserSettings.SetStringSettingAsync(key, request?.Value, securityUser, cancellationToken);

                if (success == true)
                {
                    return Ok(new { key = key, value = request?.Value, success = true });
                }
                else
                {
                    return Problem($"Could not save setting '{key}'.");
                }
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.Error,
                    $"Error setting user setting '{key}'.",
                    ex.Message,
                    ex);

                return Problem($"Could not save setting '{key}'.");
            }
        }


        /// <summary>
        /// 
        /// Gets all user settings as a JSON object.
        /// 
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>All user settings as JSON</returns>
        [HttpGet]
        [RateLimit(RateLimitOption.TenPerMinute, Scope = RateLimitScope.PerUser)]
        [Route("api/UserSettings")]
        public async Task<IActionResult> GetAllSettings(CancellationToken cancellationToken = default)
        {
            //
            // Permission check - This is a cross module function for a user to read their settings.  We do not enforce the security module readership role here.
            //

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();

                if (securityUser == null)
                {
                    return Unauthorized("User not found.");
                }

                //
                // Must have read permission level of at least one to read this
                //
                if (securityUser.readPermissionLevel < 0)
                {
                    return Forbid();
                }


                //
                // Get the raw settings JSON string
                //
                string settingsJson = await UserSettings.GetUserSettingsAsync(securityUser, cancellationToken);

                //
                // Return empty object if no settings exist
                //
                if (string.IsNullOrWhiteSpace(settingsJson))
                {
                    return Ok(new { });
                }

                //
                // Return the raw JSON (let ASP.NET handle serialization)
                //
                return Content(settingsJson, "application/json");
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.Error,
                    "Error getting all user settings.",
                    ex.Message,
                    ex);

                return Problem("Could not retrieve user settings.");
            }
        }


        // ============================================================================
        // ADMIN ENDPOINTS - For managing other users' settings
        // ============================================================================


        /// <summary>
        /// 
        /// Gets all settings for a specific user (admin only).
        /// 
        /// </summary>
        /// <param name="userId">The ID of the user whose settings are being retrieved.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A JSON object containing all user settings.</returns>
        [HttpGet]
        [RateLimit(RateLimitOption.TenPerMinute, Scope = RateLimitScope.PerUser)]
        [Route("api/UserSettings/Admin")]
        public async Task<IActionResult> GetAllSettingsAdmin([FromQuery] long userId,
                                                             CancellationToken cancellationToken = default)
        {
            // 
            // Security module reader role required for this.
            //
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(ADMIN_READ_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Forbid();
            }

            if (userId <= 0)
            {
                return BadRequest("A valid userId must be provided.");
            }

            try
            {
                SecurityUser securityUser = await _context.SecurityUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.id == userId && u.deleted == false, cancellationToken);

                if (securityUser == null)
                {
                    return NotFound("User not found.");
                }

                string settingsJson = await UserSettings.GetUserSettingsAsync(securityUser, cancellationToken);

                if (string.IsNullOrWhiteSpace(settingsJson))
                {
                    return Ok(new { });
                }

                return Content(settingsJson, "application/json");
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.Error,
                    $"Error getting all user settings for user {userId}.",
                    ex.Message,
                    ex);

                return Problem("Could not retrieve user settings.");
            }
        }


        /// <summary>
        /// 
        /// Gets a specific setting value for a user by key (admin only).
        /// 
        /// </summary>
        /// <param name="key">The setting key to retrieve.</param>
        /// <param name="userId">The ID of the user whose setting is being retrieved.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The setting key and value.</returns>
        [HttpGet]
        [RateLimit(RateLimitOption.OneHundredPerMinute, Scope = RateLimitScope.PerUser)]
        [Route("api/UserSettings/Admin/{key}")]
        public async Task<IActionResult> GetStringSettingAdmin(string key,
                                                               [FromQuery] long userId,
                                                               CancellationToken cancellationToken = default)
        {
            //
            // This settings getter is cross user, so a security module reader role is required
            //
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(ADMIN_READ_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                return BadRequest("Setting key cannot be empty.");
            }

            if (userId <= 0)
            {
                return BadRequest("A valid userId must be provided.");
            }

            try
            {
                SecurityUser securityUser = await _context.SecurityUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.id == userId && u.deleted == false, cancellationToken);

                if (securityUser == null)
                {
                    return NotFound("User not found.");
                }

                string value = await UserSettings.GetStringSettingAsync(key, securityUser, cancellationToken);

                return Ok(new { key = key, value = value });
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.Error,
                    $"Error getting user setting '{key}' for user {userId}.",
                    ex.Message,
                    ex);

                return Problem($"Could not retrieve setting '{key}'.");
            }
        }


        /// <summary>
        /// 
        /// Sets (or updates) a specific setting value for a user by key (admin only).
        /// 
        /// </summary>
        /// <param name="key">The setting key to set.</param>
        /// <param name="userId">The ID of the user whose setting is being modified.</param>
        /// <param name="request">The request body containing the new value.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The updated setting key, value, and success status.</returns>
        [HttpPut]
        [RateLimit(RateLimitOption.OneHundredPerMinute, Scope = RateLimitScope.PerUser)]
        [Route("api/UserSettings/Admin/{key}")]
        public async Task<IActionResult> SetStringSettingAdmin(string key,
                                                               [FromQuery] long userId,
                                                               [FromBody] SetSettingRequest request,
                                                               CancellationToken cancellationToken = default)
        {
            //
            // This cross user setting save function requires a security module write role
            //
            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(ADMIN_WRITE_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                return BadRequest("Setting key cannot be empty.");
            }

            if (userId <= 0)
            {
                return BadRequest("A valid userId must be provided.");
            }

            try
            {
                SecurityUser securityUser = await _context.SecurityUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.id == userId && u.deleted == false, cancellationToken);

                if (securityUser == null)
                {
                    return NotFound("User not found.");
                }

                bool success = await UserSettings.SetStringSettingAsync(key, request?.Value, securityUser, cancellationToken);

                if (success == true)
                {
                    return Ok(new { key = key, value = request?.Value, success = true });
                }
                else
                {
                    return Problem($"Could not save setting '{key}'.");
                }
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.Error,
                    $"Error setting user setting '{key}' for user {userId}.",
                    ex.Message,
                    ex);

                return Problem($"Could not save setting '{key}'.");
            }
        }


        /// <summary>
        /// 
        /// Gets the user's favourites list.
        /// 
        /// </summary>
        /// <param name="entityType">Optional entity type filter (e.g., "Contact", "ScheduledEvent")</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>List of favourited items</returns>
        [HttpGet]
        [RateLimit(RateLimitOption.OneHundredPerMinute, Scope = RateLimitScope.PerUser)]
        [Route("api/UserSettings/Favourites")]
        public async Task<IActionResult> GetFavourites(string entityType = null, CancellationToken cancellationToken = default)
        {
            //
            // NOTE - Not doing a security check here for a role because this controller is in the security module, which most user's won't have a read role in.
            //
            // We will enforce read security here stricly by read permission being > 0.
            //

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();

                if (securityUser == null)
                {
                    return Unauthorized("User not found.");
                }


                //
                // Security check for user to have at least a read permission level of 1 to read their favourites.  - Note no Security module role check done here.
                //
                if (securityUser.readPermissionLevel < 1)
                {
                    return Forbid();
                }



                //
                // Get favourites using SecurityLogic
                //
                var favourites = await SecurityLogic.GetUserFavouritesAsync(securityUser, entityType, cancellationToken);

                return Ok(favourites);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.Error,
                    "Error getting user favourites.",
                    ex.Message,
                    ex);

                return Problem("Could not retrieve user favourites.");
            }
        }


        /// <summary>
        /// 
        /// Gets the user's most recently accessed items list.
        /// 
        /// </summary>
        /// <param name="entityType">Optional entity type filter</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>List of recently accessed items</returns>
        [HttpGet]
        [RateLimit(RateLimitOption.OneHundredPerMinute, Scope = RateLimitScope.PerUser)]
        [Route("api/UserSettings/MostRecents")]
        public async Task<IActionResult> GetMostRecents(string entityType = null, CancellationToken cancellationToken = default)
        {
            //
            // NOTE - Not doing a security check here for a role because this controller is in the security module, which most user's won't have a read role in.
            //
            // We will enforce read security here stricly by read permission being > 0.
            //

            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync();

                if (securityUser == null)
                {
                    return Unauthorized("User not found.");
                }


                //
                // Security check for user to have at least a read permission level of 1 to read their favourites.  - Note no Security module role check done here.
                //
                if (securityUser.readPermissionLevel < 1)
                {
                    return Forbid();
                }



                //
                // Get most recents using SecurityLogic
                //
                var mostRecents = await SecurityLogic.GetUserMostRecentsAsync(securityUser, cancellationToken);

                return Ok(mostRecents);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.Error,
                    "Error getting user most recents.",
                    ex.Message,
                    ex);

                return Problem("Could not retrieve user most recents.");
            }
        }
    }


    /// <summary>
    /// Request model for setting a user setting value.
    /// </summary>
    public class SetSettingRequest
    {
        /// <summary>
        /// The value to set (can be null to clear the setting).
        /// </summary>
        public string Value { get; set; }
    }
}
