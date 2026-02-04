//
// Monitored Applications Controller
//
// API endpoints for managing and querying monitored Foundation applications.
//
using Foundation.Security;
using Foundation.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Foundation.Controllers.WebAPI
{

    /// <summary>
    /// 
    /// API for querying health status of monitored Foundation applications
    /// 
    /// </summary>
    [Route("api/[controller]")]
    public class MonitoredApplicationsController : SecureWebAPIController
    {
        private readonly IMonitoredApplicationService _monitoredAppService;
        private readonly ILogger<MonitoredApplicationsController> _logger;


        public MonitoredApplicationsController(IMonitoredApplicationService monitoredAppService,
                                               ILogger<MonitoredApplicationsController> logger) : base("Auditor", "MonitoredApplications")
        {
            _monitoredAppService = monitoredAppService;
            _logger = logger;
        }


        //
        // GET: api/MonitoredApplications
        //
        // Returns list of configured monitored applications
        //
        [HttpGet]
        public IActionResult GetApplications()
        {
            try
            {
                List<MonitoredApplicationConfig> apps = _monitoredAppService.GetConfiguredApplications();
                
                return Ok(apps);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configured applications");
                return Problem("Failed to retrieve configured applications");
            }
        }


        //
        // GET: api/MonitoredApplications/status
        //
        // Returns health status for all configured applications
        //
        [HttpGet("status")]
        public async Task<IActionResult> GetAllStatus()
        {
            try
            {
                //
                // Get current user's objectGuid for authenticated health checks
                //
                string userObjectGuid = User.FindFirst("sub")?.Value;

                List<MonitoredApplicationStatus> statuses = await _monitoredAppService.GetAllApplicationStatusesAsync(userObjectGuid);

                return Ok(statuses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all application statuses");
                return Problem("Failed to retrieve application statuses");
            }
        }


        //
        // GET: api/MonitoredApplications/{name}/status
        //
        // Returns health status for a specific application
        //
        [HttpGet("{name}/status")]
        public async Task<IActionResult> GetApplicationStatus(string name)
        {
            try
            {
                //
                // Get current user's objectGuid for authenticated health checks
                //
                string userObjectGuid = User.FindFirst("sub")?.Value;

                MonitoredApplicationStatus status = await _monitoredAppService.GetApplicationStatusAsync(name, userObjectGuid);

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status for application {Name}", name);
                return Problem($"Failed to retrieve status for application '{name}'");
            }
        }


        //
        // GET: api/MonitoredApplications/metrics
        //
        // Returns aggregated application metrics from all monitored applications
        //
        [HttpGet("metrics")]
        public async Task<IActionResult> GetAllApplicationMetrics()
        {
            try
            {
                //
                // Get current user's objectGuid for authenticated metrics calls
                //
                string userObjectGuid = User.FindFirst("sub")?.Value;

                ApplicationMetricsResponse metrics = await _monitoredAppService.GetAllApplicationMetricsAsync(userObjectGuid);

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application metrics");
                return Problem("Failed to retrieve application metrics");
            }
        }
    }
}
