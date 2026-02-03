using System;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Auditor;
using Foundation.Controllers;
using Foundation.Security.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foundation.Security.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// Web API controller for managing tenant-level settings.
    /// 
    /// Settings are stored as a JSON object in the SecurityTenant.settings field.
    /// This controller provides endpoints to get and set individual settings by key,
    /// as well as retrieve all settings for a tenant.
    /// 
    /// </summary>
    /// <remarks>
    /// 
    /// Authorization: Requires admin-level access to read or modify tenant settings.
    /// The caller must provide a tenantId query parameter to specify the target tenant.
    /// 
    /// </remarks>
    [ApiController]
    public class TenantSettingsController : SecureWebAPIController
    {
        //
        // Admin level required to read or write tenant settings
        //
        private const int READ_PERMISSION_LEVEL_REQUIRED = 3;
        private const int WRITE_PERMISSION_LEVEL_REQUIRED = 3;

        private readonly SecurityContext _context;


        public TenantSettingsController(SecurityContext context) : base("Security", "TenantSettings")
        {
            _context = context;
        }


        /// <summary>
        /// 
        /// Retrieves a specific setting value for a tenant by key.
        /// 
        /// </summary>
        /// <param name="key">The setting key to retrieve.</param>
        /// <param name="tenantId">The ID of the tenant whose setting is being retrieved.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The setting key and value.</returns>
        [HttpGet]
        [RateLimit(RateLimitOption.OneHundredPerMinute, Scope = RateLimitScope.PerUser)]
        [Route("api/TenantSettings/{key}")]
        public async Task<IActionResult> GetStringSetting(string key, 
                                                           [FromQuery] int tenantId, 
                                                           CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                return BadRequest("Setting key cannot be empty.");
            }

            if (tenantId <= 0)
            {
                return BadRequest("A valid tenantId must be provided.");
            }

            try
            {
                SecurityTenant securityTenant = await _context.SecurityTenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.id == tenantId && t.deleted == false, cancellationToken);

                if (securityTenant == null)
                {
                    return NotFound("Tenant not found.");
                }

                string value = await TenantSettings.GetStringSettingAsync(key, securityTenant, cancellationToken);
                return Ok(new { key = key, value = value });
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(
                    AuditEngine.AuditType.Error,
                    $"Error getting tenant setting '{key}' for tenant {tenantId}.",
                    ex.Message,
                    ex);
                return Problem($"Could not retrieve setting '{key}'.");
            }
        }


        /// <summary>
        /// 
        /// Sets (or updates) a specific setting value for a tenant by key.
        /// 
        /// </summary>
        /// <param name="key">The setting key to set.</param>
        /// <param name="tenantId">The ID of the tenant whose setting is being modified.</param>
        /// <param name="request">The request body containing the new value.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The updated setting key, value, and success status.</returns>
        [HttpPut]
        [RateLimit(RateLimitOption.OneHundredPerMinute, Scope = RateLimitScope.PerUser)]
        [Route("api/TenantSettings/{key}")]
        public async Task<IActionResult> SetStringSetting(string key, 
                                                           [FromQuery] int tenantId, 
                                                           [FromBody] SetSettingRequest request, 
                                                           CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                return BadRequest("Setting key cannot be empty.");
            }

            if (tenantId <= 0)
            {
                return BadRequest("A valid tenantId must be provided.");
            }

            try
            {
                SecurityTenant securityTenant = await _context.SecurityTenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.id == tenantId && t.deleted == false, cancellationToken);

                if (securityTenant == null)
                {
                    return NotFound("Tenant not found.");
                }

                bool success = await TenantSettings.SetStringSettingAsync(key, request?.Value, securityTenant, cancellationToken);

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
                    $"Error setting tenant setting '{key}' for tenant {tenantId}.",
                    ex.Message,
                    ex);
                return Problem($"Could not save setting '{key}'.");
            }
        }


        /// <summary>
        /// 
        /// Retrieves all settings for a tenant as a JSON object.
        /// 
        /// </summary>
        /// <param name="tenantId">The ID of the tenant whose settings are being retrieved.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A JSON object containing all tenant settings.</returns>
        [HttpGet]
        [RateLimit(RateLimitOption.TenPerMinute, Scope = RateLimitScope.PerUser)]
        [Route("api/TenantSettings")]
        public async Task<IActionResult> GetAllSettings([FromQuery] int tenantId, 
                                                         CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Forbid();
            }

            if (tenantId <= 0)
            {
                return BadRequest("A valid tenantId must be provided.");
            }

            try
            {
                SecurityTenant securityTenant = await _context.SecurityTenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.id == tenantId && t.deleted == false, cancellationToken);

                if (securityTenant == null)
                {
                    return NotFound("Tenant not found.");
                }

                string settingsJson = await TenantSettings.GetTenantSettingsAsync(securityTenant, cancellationToken);

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
                    $"Error getting all tenant settings for tenant {tenantId}.",
                    ex.Message,
                    ex);
                return Problem("Could not retrieve tenant settings.");
            }
        }


        /// <summary>
        /// 
        /// Request model for setting a tenant setting value.
        /// 
        /// </summary>
        public class SetSettingRequest
        {
            public string Value { get; set; }
        }
    }
}
