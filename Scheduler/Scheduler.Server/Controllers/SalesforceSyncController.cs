//
// SalesforceSyncController.cs
//
// API controller for manual Salesforce sync triggers, connectivity testing,
// tenant link management, and queue monitoring.
//
// AI Assisted Development:  This file was created with AI assistance for the
// Scheduler Salesforce integration.
//

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Foundation.Controllers;
using Foundation.Security;
using Foundation.Scheduler.Database;
using Scheduler.Salesforce.Auth;
using Scheduler.Salesforce.Sync;


namespace Foundation.Scheduler.Controllers.WebAPI
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesforceSyncController : SecureWebAPIController
    {
        private readonly ILogger<SalesforceSyncController> _logger;
        private readonly SalesforceSyncService _syncService;
        private readonly SalesforceCredentialProtector _credentialProtector;

        public const int PERMISSION_LEVEL_REQUIRED = 100;


        public SalesforceSyncController(
            ILogger<SalesforceSyncController> logger,
            SalesforceSyncService syncService,
            SalesforceCredentialProtector credentialProtector)
            : base("Scheduler", "SalesforceSyncController")
        {
            _logger = logger;
            _syncService = syncService;
            _credentialProtector = credentialProtector;
        }


        #region Config Management


        /// <summary>
        ///
        /// Returns the current tenant's Salesforce configuration with secrets masked.
        ///
        /// </summary>
        [HttpGet("config")]
        public async Task<IActionResult> GetConfig(CancellationToken ct = default)
        {
            if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(ct) == false)
            {
                return Forbid();
            }

            Foundation.Security.Database.SecurityUser securityUser = await GetSecurityUserAsync(ct);
            Guid tenantGuid = await UserTenantGuidAsync(securityUser, ct);

            SchedulerContext context = GetSchedulerContext();

            SalesforceTenantLink link = await context.SalesforceTenantLinks
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.tenantGuid == tenantGuid && l.deleted == false, ct);

            if (link == null)
            {
                return Ok(new
                {
                    configured = false,
                    syncEnabled = false,
                    syncDirectionFlags = "None",
                    pullIntervalMinutes = 5,
                    loginUrl = "https://login.salesforce.com/services/oauth2/token",
                    sfClientId = "",
                    sfUsername = "",
                    apiVersion = "v56.0"
                });
            }

            return Ok(new
            {
                configured = true,
                id = link.id,
                syncEnabled = link.syncEnabled,
                syncDirectionFlags = link.syncDirectionFlags ?? "None",
                pullIntervalMinutes = link.pullIntervalMinutes ?? 5,
                lastPullDate = link.lastPullDate,
                loginUrl = link.loginUrl ?? "",
                sfClientId = link.sfClientId ?? "",
                sfUsername = link.sfUsername ?? "",
                apiVersion = link.apiVersion ?? "v56.0",
                instanceUrl = link.instanceUrl ?? "",
                //
                // Secrets are masked — the UI knows they're set but can't read them
                //
                hasClientSecret = string.IsNullOrEmpty(link.sfClientSecret) == false,
                hasPassword = string.IsNullOrEmpty(link.sfPassword) == false,
                hasSecurityToken = string.IsNullOrEmpty(link.sfSecurityToken) == false,
                versionNumber = link.versionNumber
            });
        }


        /// <summary>
        ///
        /// Creates or updates the tenant's Salesforce configuration.
        /// Encrypts secret fields before persisting.
        ///
        /// </summary>
        [HttpPut("config")]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        public async Task<IActionResult> SaveConfig([FromBody] SalesforceConfigDTO dto, CancellationToken ct = default)
        {
            if (dto == null)
            {
                return BadRequest();
            }

            if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(ct) == false)
            {
                return Forbid();
            }

            Foundation.Security.Database.SecurityUser securityUser = await GetSecurityUserAsync(ct);
            Guid tenantGuid = await UserTenantGuidAsync(securityUser, ct);

            SchedulerContext context = GetSchedulerContext();

            SalesforceTenantLink link = await context.SalesforceTenantLinks
                .FirstOrDefaultAsync(l => l.tenantGuid == tenantGuid && l.deleted == false, ct);

            bool isNew = link == null;

            if (isNew == true)
            {
                link = new SalesforceTenantLink
                {
                    tenantGuid = tenantGuid,
                    objectGuid = Guid.NewGuid(),
                    versionNumber = 1,
                    active = true,
                    deleted = false
                };
                context.SalesforceTenantLinks.Add(link);
            }
            else
            {
                link.versionNumber++;
            }

            //
            // Non-secret fields — always update
            //
            link.syncEnabled = dto.syncEnabled;
            link.syncDirectionFlags = dto.syncDirectionFlags ?? "None";
            link.pullIntervalMinutes = dto.pullIntervalMinutes > 0 ? dto.pullIntervalMinutes : 5;
            link.loginUrl = dto.loginUrl ?? "https://login.salesforce.com/services/oauth2/token";
            link.sfClientId = dto.sfClientId;
            link.sfUsername = dto.sfUsername;
            link.apiVersion = dto.apiVersion ?? "v56.0";

            //
            // Secret fields — only update if the UI sent a non-empty value
            // (empty means "keep existing encrypted value")
            //
            if (string.IsNullOrEmpty(dto.sfClientSecret) == false)
            {
                link.sfClientSecret = _credentialProtector.Encrypt(dto.sfClientSecret);
            }

            if (string.IsNullOrEmpty(dto.sfPassword) == false)
            {
                link.sfPassword = _credentialProtector.Encrypt(dto.sfPassword);
            }

            if (string.IsNullOrEmpty(dto.sfSecurityToken) == false)
            {
                link.sfSecurityToken = _credentialProtector.Encrypt(dto.sfSecurityToken);
            }

            try
            {
                await context.SaveChangesAsync(ct);

                _logger.LogInformation("Salesforce config {Action} for tenant {TenantGuid}", isNew ? "created" : "updated", tenantGuid);

                return Ok(new { success = true, id = link.id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save Salesforce config for tenant {TenantGuid}", tenantGuid);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }


        #endregion


        #region Connection Testing


        /// <summary>
        ///
        /// Tests the Salesforce connection by attempting an OAuth login.
        ///
        /// </summary>
        [HttpPost("testConnection")]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        public async Task<IActionResult> TestConnection(CancellationToken ct = default)
        {
            if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(ct) == false)
            {
                return Forbid();
            }

            Foundation.Security.Database.SecurityUser securityUser = await GetSecurityUserAsync(ct);
            Guid tenantGuid = await UserTenantGuidAsync(securityUser, ct);

            try
            {
                var (success, instanceUrl, error) = await _syncService.TestConnectionAsync(tenantGuid, ct);

                if (success == true)
                {
                    return Ok(new
                    {
                        success = true,
                        instanceUrl = instanceUrl,
                        message = "Successfully connected to Salesforce."
                    });
                }

                return Ok(new
                {
                    success = false,
                    error = error
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Salesforce connection test failed for tenant {TenantGuid}", tenantGuid);
                return Ok(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }


        #endregion


        #region Queue Monitoring


        /// <summary>
        ///
        /// Returns sync queue status counts grouped by status.
        ///
        /// </summary>
        [HttpGet("queueStatus")]
        public async Task<IActionResult> GetQueueStatus(CancellationToken ct = default)
        {
            if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(ct) == false)
            {
                return Forbid();
            }

            Foundation.Security.Database.SecurityUser securityUser = await GetSecurityUserAsync(ct);
            Guid tenantGuid = await UserTenantGuidAsync(securityUser, ct);

            SchedulerContext context = GetSchedulerContext();

            var statusCounts = await context.SalesforceSyncQueues
                .Where(q => q.tenantGuid == tenantGuid && q.deleted == false)
                .GroupBy(q => q.status)
                .Select(g => new { status = g.Key, count = g.Count() })
                .ToListAsync(ct);

            int totalPending = statusCounts.FirstOrDefault(s => s.status == "Pending")?.count ?? 0;
            int totalInProgress = statusCounts.FirstOrDefault(s => s.status == "InProgress")?.count ?? 0;
            int totalCompleted = statusCounts.FirstOrDefault(s => s.status == "Completed")?.count ?? 0;
            int totalFailed = statusCounts.FirstOrDefault(s => s.status == "Failed")?.count ?? 0;
            int totalAbandoned = statusCounts.FirstOrDefault(s => s.status == "Abandoned")?.count ?? 0;

            //
            // Get the most recent failed items for display
            //
            var recentFailed = await context.SalesforceSyncQueues
                .Where(q => q.tenantGuid == tenantGuid && q.status == "Failed" && q.deleted == false)
                .OrderByDescending(q => q.lastAttemptDate)
                .Take(10)
                .Select(q => new
                {
                    q.id,
                    q.entityType,
                    q.operationType,
                    q.entityId,
                    q.attemptCount,
                    q.maxAttempts,
                    q.lastAttemptDate,
                    q.errorMessage
                })
                .ToListAsync(ct);

            return Ok(new
            {
                pending = totalPending,
                inProgress = totalInProgress,
                completed = totalCompleted,
                failed = totalFailed,
                abandoned = totalAbandoned,
                total = totalPending + totalInProgress + totalCompleted + totalFailed + totalAbandoned,
                recentFailed = recentFailed
            });
        }


        /// <summary>
        ///
        /// Resets all Failed queue items back to Pending for retry.
        ///
        /// </summary>
        [HttpPost("retryFailed")]
        [RateLimit(RateLimitOption.OnePerMinute, Scope = RateLimitScope.PerUser)]
        public async Task<IActionResult> RetryFailed(CancellationToken ct = default)
        {
            if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(ct) == false)
            {
                return Forbid();
            }

            Foundation.Security.Database.SecurityUser securityUser = await GetSecurityUserAsync(ct);
            Guid tenantGuid = await UserTenantGuidAsync(securityUser, ct);

            SchedulerContext context = GetSchedulerContext();

            var failedItems = await context.SalesforceSyncQueues
                .Where(q => q.tenantGuid == tenantGuid && q.status == "Failed" && q.deleted == false)
                .ToListAsync(ct);

            foreach (var item in failedItems)
            {
                item.status = "Pending";
                item.attemptCount = 0;
                item.errorMessage = null;
                item.lastAttemptDate = null;
            }

            await context.SaveChangesAsync(ct);

            _logger.LogInformation("Reset {Count} failed Salesforce sync items to Pending for tenant {TenantGuid}", failedItems.Count, tenantGuid);

            return Ok(new { success = true, resetCount = failedItems.Count });
        }


        #endregion


        #region Sync Triggers


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
            Guid tenantGuid = await UserTenantGuidAsync(securityUser, ct);

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
            Guid tenantGuid = await UserTenantGuidAsync(securityUser, ct);

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
            Guid tenantGuid = await UserTenantGuidAsync(securityUser, ct);

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


        #endregion


        #region Helpers


        private SchedulerContext GetSchedulerContext()
        {
            return (SchedulerContext)HttpContext.RequestServices.GetService(typeof(SchedulerContext));
        }


        #endregion


        #region DTOs


        /// <summary>
        ///
        /// DTO for receiving Salesforce config from the admin UI.
        ///
        /// </summary>
        public class SalesforceConfigDTO
        {
            public bool syncEnabled { get; set; }
            public string syncDirectionFlags { get; set; }
            public int pullIntervalMinutes { get; set; }
            public string loginUrl { get; set; }
            public string sfClientId { get; set; }
            public string sfClientSecret { get; set; }
            public string sfUsername { get; set; }
            public string sfPassword { get; set; }
            public string sfSecurityToken { get; set; }
            public string apiVersion { get; set; }
        }


        #endregion
    }
}
