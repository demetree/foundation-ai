// AI-Developed — This file was significantly developed with AI assistance.
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Foundation.Security;
using Foundation.Security.Database;
using Foundation.Controllers;
using static Foundation.Auditor.AuditEngine;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// Custom partial class extension for FiscalPeriodsController.
    /// Adds year-generation endpoint that delegates to FinancialManagementService.
    /// </summary>
    public partial class FiscalPeriodsController
    {
        /// <summary>
        /// Generates 12 monthly fiscal periods for the specified year.
        /// Idempotent — returns existing count if periods already exist.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/FiscalPeriods/GenerateYear")]
        public async Task<IActionResult> GenerateYear(
            [FromQuery] int year,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditType.Error,
                    "Attempt to generate fiscal year by user with no tenant. User: " + securityUser?.accountName,
                    securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant.");
            }

            // Resolve FinancialManagementService from DI
            var financialService = HttpContext.RequestServices
                .GetService(typeof(Foundation.Scheduler.Services.FinancialManagementService))
                as Foundation.Scheduler.Services.FinancialManagementService;

            if (financialService == null)
            {
                return Problem("Financial management service is not available.");
            }

            var result = await financialService.GenerateFiscalYearAsync(
                userTenantGuid,
                year,
                cancellationToken);

            if (!result.Success)
            {
                return BadRequest(new { error = result.ErrorMessage });
            }

            await CreateAuditEventAsync(AuditType.Miscellaneous,
                $"Fiscal year {year} generated via FinancialManagementService. Created: {result.Data["created"]}");

            return Ok(result.Data);
        }
    }
}
