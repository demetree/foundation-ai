//
// SalesforceSyncController.cs
//
// API controller for manual Salesforce sync triggers, connectivity testing,
// and tenant link management.
//
// AI Assisted Development:  This file was created with AI assistance for the
// Scheduler Salesforce integration.
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Foundation.Controllers;
using Foundation.Security;
using Scheduler.Salesforce.Sync;


namespace Foundation.Scheduler.Controllers.WebAPI
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesforceSyncController : SecureWebAPIController
    {
        private readonly ILogger<SalesforceSyncController> _logger;
        private readonly SalesforceSyncService _syncService;

        public const int PERMISSION_LEVEL_REQUIRED = 100;


        public SalesforceSyncController(
            ILogger<SalesforceSyncController> logger,
            SalesforceSyncService syncService)
            : base("Scheduler", "SalesforceSyncController")
        {
            _logger = logger;
            _syncService = syncService;
        }


        /// <summary>
        ///
        /// Triggers a full pull from Salesforce for the current user's tenant.
        ///
        /// </summary>
        [HttpPost("pullAll")]
        [RateLimit(RateLimitOption.OnePerMinute, Scope = RateLimitScope.PerUser)]
        public async Task<IActionResult> PullAll(CancellationToken ct = default)
        {
            if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(ct) == false)
            {
                return Forbid();
            }

            Foundation.Security.Database.SecurityUser securityUser = await GetSecurityUserAsync(ct);
            Guid tenantGuid;

            try
            {
                tenantGuid = await UserTenantGuidAsync(securityUser, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User is not configured with a tenant.");
                return Problem("Your user account is not configured with a tenant.");
            }

            try
            {
                var config = await _syncService.LoadConfigForTenantAsync(tenantGuid, ct);

                if (config == null)
                {
                    return BadRequest(new { success = false, error = "No Salesforce integration configured for this tenant." });
                }

                _logger.LogInformation("Manual Salesforce pullAll triggered for tenant {TenantGuid}", tenantGuid);

                var result = await _syncService.PullAllAsync(config, null, ct);

                return Ok(new
                {
                    success = true,
                    created = result.TotalCreated,
                    updated = result.TotalUpdated,
                    errors = result.ErrorCount,
                    errorDetails = result.Errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual Salesforce pull for tenant {TenantGuid}", tenantGuid);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }


        /// <summary>
        ///
        /// Triggers a pull of only Accounts (-> Clients) from Salesforce.
        ///
        /// </summary>
        [HttpPost("pullAccounts")]
        [RateLimit(RateLimitOption.OnePerMinute, Scope = RateLimitScope.PerUser)]
        public async Task<IActionResult> PullAccounts(CancellationToken ct = default)
        {
            if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(ct) == false)
            {
                return Forbid();
            }

            Foundation.Security.Database.SecurityUser securityUser = await GetSecurityUserAsync(ct);
            Guid tenantGuid;

            try
            {
                tenantGuid = await UserTenantGuidAsync(securityUser, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User is not configured with a tenant.");
                return Problem("Your user account is not configured with a tenant.");
            }

            try
            {
                var config = await _syncService.LoadConfigForTenantAsync(tenantGuid, ct);
                if (config == null)
                {
                    return BadRequest(new { success = false, error = "No Salesforce integration configured for this tenant." });
                }

                var result = await _syncService.PullAccountsAsync(config, null, ct);

                return Ok(new { success = true, created = result.TotalCreated, updated = result.TotalUpdated, errors = result.ErrorCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pulling Salesforce accounts for tenant {TenantGuid}", tenantGuid);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }


        /// <summary>
        ///
        /// Triggers a pull of only Contacts from Salesforce.
        ///
        /// </summary>
        [HttpPost("pullContacts")]
        [RateLimit(RateLimitOption.OnePerMinute, Scope = RateLimitScope.PerUser)]
        public async Task<IActionResult> PullContacts(CancellationToken ct = default)
        {
            if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(ct) == false)
            {
                return Forbid();
            }

            Foundation.Security.Database.SecurityUser securityUser = await GetSecurityUserAsync(ct);
            Guid tenantGuid;

            try
            {
                tenantGuid = await UserTenantGuidAsync(securityUser, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User is not configured with a tenant.");
                return Problem("Your user account is not configured with a tenant.");
            }

            try
            {
                var config = await _syncService.LoadConfigForTenantAsync(tenantGuid, ct);
                if (config == null)
                {
                    return BadRequest(new { success = false, error = "No Salesforce integration configured for this tenant." });
                }

                var result = await _syncService.PullContactsAsync(config, null, ct);

                return Ok(new { success = true, created = result.TotalCreated, updated = result.TotalUpdated, errors = result.ErrorCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pulling Salesforce contacts for tenant {TenantGuid}", tenantGuid);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }


        /// <summary>
        ///
        /// Triggers a pull of only Events (-> ScheduledEvents) from Salesforce.
        ///
        /// </summary>
        [HttpPost("pullEvents")]
        [RateLimit(RateLimitOption.OnePerMinute, Scope = RateLimitScope.PerUser)]
        public async Task<IActionResult> PullEvents(CancellationToken ct = default)
        {
            if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(ct) == false)
            {
                return Forbid();
            }

            Foundation.Security.Database.SecurityUser securityUser = await GetSecurityUserAsync(ct);
            Guid tenantGuid;

            try
            {
                tenantGuid = await UserTenantGuidAsync(securityUser, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User is not configured with a tenant.");
                return Problem("Your user account is not configured with a tenant.");
            }

            try
            {
                var config = await _syncService.LoadConfigForTenantAsync(tenantGuid, ct);
                if (config == null)
                {
                    return BadRequest(new { success = false, error = "No Salesforce integration configured for this tenant." });
                }

                var result = await _syncService.PullEventsAsync(config, null, ct);

                return Ok(new { success = true, created = result.TotalCreated, updated = result.TotalUpdated, errors = result.ErrorCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pulling Salesforce events for tenant {TenantGuid}", tenantGuid);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
