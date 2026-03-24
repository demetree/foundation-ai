// AI-Developed — This file was significantly developed with AI assistance.
//
// FeatureConfigController.cs
//
// Unified endpoint that returns all system-level feature toggle states.
// The Angular client calls this once on startup to determine which
// modules and navigation items should be visible.
//
// Toggle Resolution Order:
//   1. Check tenant settings JSON for a boolean override (if user is authenticated)
//   2. Fall back to appsettings.json Settings:XxxEnabled value
//

using System;
using System.Threading.Tasks;
using Foundation.Security;
using Foundation.Security.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace Foundation.Scheduler.Controllers.WebAPI
{
    //
    /// <summary>
    ///
    /// Returns all system-level feature toggle states in a single payload.
    /// Used by the client to gate module visibility in the sidebar and routing.
    ///
    /// When the caller is authenticated, tenant-level overrides are checked
    /// first (stored in SecurityTenant.settings JSON). If no tenant override
    /// exists, the appsettings.json default is used.
    ///
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FeatureConfigController : ControllerBase
    {
        private readonly ILogger<FeatureConfigController> _logger;
        private readonly IConfiguration _configuration;
        private readonly SecurityContext _securityContext;


        public FeatureConfigController(
            ILogger<FeatureConfigController> logger,
            IConfiguration configuration,
            SecurityContext securityContext)
        {
            _logger = logger;
            _configuration = configuration;
            _securityContext = securityContext;
        }


        //
        /// <summary>
        ///
        /// Returns all feature toggle states.
        /// This is a public endpoint (no auth required) so the client can gate
        /// the UI before the user navigates to gated sections.
        ///
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            //
            // Try to resolve the current user's tenant for per-tenant overrides
            //
            SecurityTenant currentTenant = null;

            try
            {
                string userObjectGuid = User?.FindFirst("sub")?.Value;

                if (string.IsNullOrWhiteSpace(userObjectGuid) == false && Guid.TryParse(userObjectGuid, out Guid parsedGuid))
                {
                    SecurityUser securityUser = await _securityContext.SecurityUsers
                        .AsNoTracking()
                        .Include(u => u.securityTenant)
                        .FirstOrDefaultAsync(u => u.objectGuid == parsedGuid && u.deleted == false);

                    currentTenant = securityUser?.securityTenant;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "FeatureConfigController: Unable to resolve tenant for feature overrides, using appsettings defaults.");
            }


            return Ok(new
            {
                volunteerManagementEnabled = ResolveToggle("VolunteerManagementEnabled", currentTenant),
                fundraisingEnabled = ResolveToggle("FundraisingEnabled", currentTenant),
                financialManagementEnabled = ResolveToggle("FinancialManagementEnabled", currentTenant),
                crewManagementEnabled = ResolveToggle("CrewManagementEnabled", currentTenant),
                messagingEnabled = ResolveToggle("MessagingEnabled", currentTenant)
            });
        }


        /// <summary>
        ///
        /// Resolves a feature toggle value using tenant override → appsettings fallback.
        ///
        /// </summary>
        private bool ResolveToggle(string settingName, SecurityTenant tenant)
        {
            //
            // 1. Check tenant-level override
            //
            if (tenant != null)
            {
                bool? tenantOverride = TenantSettings.GetBoolSetting(settingName, tenant);

                if (tenantOverride.HasValue)
                {
                    return tenantOverride.Value;
                }
            }

            //
            // 2. Fall back to appsettings.json
            //
            return _configuration.GetValue<bool>($"Settings:{settingName}", false);
        }
    }
}
