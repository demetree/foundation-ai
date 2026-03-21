// AI-Developed — This file was significantly developed with AI assistance.
//
// FeatureConfigController.cs
//
// Unified endpoint that returns all system-level feature toggle states.
// The Angular client calls this once on startup to determine which
// modules and navigation items should be visible.
//

using Microsoft.AspNetCore.Mvc;
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
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FeatureConfigController : ControllerBase
    {
        private readonly ILogger<FeatureConfigController> _logger;
        private readonly IConfiguration _configuration;


        public FeatureConfigController(
            ILogger<FeatureConfigController> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
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
        public IActionResult Get()
        {
            return Ok(new
            {
                volunteerManagementEnabled = _configuration.GetValue<bool>("Settings:VolunteerManagementEnabled", false),
                fundraisingEnabled = _configuration.GetValue<bool>("Settings:FundraisingEnabled", false),
                financialManagementEnabled = _configuration.GetValue<bool>("Settings:FinancialManagementEnabled", false),
                crewManagementEnabled = _configuration.GetValue<bool>("Settings:CrewManagementEnabled", false)
            });
        }
    }
}
