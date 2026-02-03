//
// Push Token Controller
//
// Manages push notification token registration for web and mobile clients.
//
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Foundation.Alerting.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Alerting.Server.Controllers
{
    /// <summary>
    /// API controller for managing push notification tokens.
    /// </summary>
    [Route("api/push-tokens")]
    [ApiController]
    [Authorize]
    public class PushTokenController : ControllerBase
    {
        private readonly AlertingContext _context;
        private readonly ILogger<PushTokenController> _logger;

        public PushTokenController(
            AlertingContext context,
            ILogger<PushTokenController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Register or update a push token for the current user.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterToken([FromBody] RegisterTokenRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserObjectGuid();
            var tenantGuid = GetTenantGuid();

            try
            {
                // Look for existing token entry for this user
                var existingToken = await _context.UserPushTokens
                    .FirstOrDefaultAsync(t => t.tenantGuid == tenantGuid && 
                                              t.userObjectGuid == userId &&
                                              t.deviceFingerprint == request.DeviceFingerprint);

                if (existingToken != null)
                {
                    // Update existing token
                    existingToken.fcmToken = request.Token;
                    existingToken.lastUpdatedAt = DateTime.UtcNow;
                    existingToken.userAgent = request.UserAgent;
                }
                else
                {
                    // Create new token entry
                    var newToken = new UserPushToken
                    {
                        tenantGuid = tenantGuid,
                        userObjectGuid = userId,
                        fcmToken = request.Token,
                        deviceFingerprint = request.DeviceFingerprint,
                        platform = request.Platform ?? "web",
                        userAgent = request.UserAgent,
                        registeredAt = DateTime.UtcNow,
                        lastUpdatedAt = DateTime.UtcNow,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };

                    _context.UserPushTokens.Add(newToken);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Push token registered for user {UserId} on {Platform}",
                    userId, request.Platform ?? "web");

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register push token for user {UserId}", userId);
                return StatusCode(500, new { error = "Failed to register push token" });
            }
        }

        /// <summary>
        /// Unregister a push token (user disabled notifications or logged out).
        /// </summary>
        [HttpPost("unregister")]
        public async Task<IActionResult> UnregisterToken([FromBody] UnregisterTokenRequest request)
        {
            var userId = GetUserObjectGuid();
            var tenantGuid = GetTenantGuid();

            try
            {
                var token = await _context.UserPushTokens
                    .FirstOrDefaultAsync(t => t.tenantGuid == tenantGuid &&
                                              t.userObjectGuid == userId &&
                                              t.fcmToken == request.Token);

                if (token != null)
                {
                    token.deleted = true;
                    token.active = false;
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Push token unregistered for user {UserId}", userId);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unregister push token for user {UserId}", userId);
                return StatusCode(500, new { error = "Failed to unregister push token" });
            }
        }

        #region Helpers

        private Guid GetTenantGuid()
        {
            var claim = User.FindFirst("tenant_guid");
            return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
        }

        private Guid GetUserObjectGuid()
        {
            var claim = User.FindFirst("sub") ?? User.FindFirst("object_guid");
            return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
        }

        #endregion
    }

    public class RegisterTokenRequest
    {
        [Required]
        [StringLength(500)]
        public string Token { get; set; }

        [Required]
        [StringLength(100)]
        public string DeviceFingerprint { get; set; }

        [StringLength(20)]
        public string Platform { get; set; } // "web", "ios", "android"

        [StringLength(500)]
        public string UserAgent { get; set; }
    }

    public class UnregisterTokenRequest
    {
        [Required]
        public string Token { get; set; }
    }
}
