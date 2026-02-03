//
// Integration Self-Registration Controller
//
// Allows Foundation systems to self-register with Alerting using OIDC authentication.
// This enables automated integration setup without manual API key configuration.
//
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Foundation.Alerting.Database;
using Foundation.Auditor;
using Foundation.Security;
using Foundation.Security.Database;


namespace Alerting.Server.Controllers
{
    /// <summary>
    /// Enables Foundation systems to self-register with Alerting using OIDC authentication.
    /// After registration, the system receives an API key for subsequent calls.
    /// </summary>
    [ApiController]
    [Route("api/integrations")]
    [Authorize]
    public class IntegrationRegistrationController : SecureWebAPIController
    {
        private readonly AlertingContext _context;
        private readonly ILogger<IntegrationRegistrationController> _logger;

        #region Request/Response DTOs

        public class SelfRegisterRequest
        {
            /// <summary>
            /// Name of the registering system (e.g., "Foundation", "Scheduler").
            /// </summary>
            public string ServiceName { get; set; }

            /// <summary>
            /// Optional description of the system.
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Base URL of the registering system (e.g., "https://foundation.mycompany.com").
            /// </summary>
            public string ServiceUrl { get; set; }

            /// <summary>
            /// URL where Alerting should send webhook callbacks for incident events.
            /// </summary>
            public string CallbackUrl { get; set; }
        }

        public class SelfRegisterResponse
        {
            /// <summary>
            /// The integration ID in Alerting.
            /// </summary>
            public int IntegrationId { get; set; }

            /// <summary>
            /// The integration's unique identifier.
            /// </summary>
            public Guid IntegrationGuid { get; set; }

            /// <summary>
            /// The service ID in Alerting.
            /// </summary>
            public int ServiceId { get; set; }

            /// <summary>
            /// Name of the registered service.
            /// </summary>
            public string ServiceName { get; set; }

            /// <summary>
            /// The API key for this integration.
            /// Store this securely - it cannot be retrieved again.
            /// </summary>
            public string ApiKey { get; set; }

            /// <summary>
            /// Message for the caller.
            /// </summary>
            public string Message { get; set; }
        }

        #endregion

        public IntegrationRegistrationController(
            AlertingContext context,
            ILogger<IntegrationRegistrationController> logger) : base("Alerting", "IntegrationRegistration")
        {
            _context = context;
            _logger = logger;
        }


        /// <summary>
        /// Register a Foundation system with Alerting using OIDC authentication.
        /// Creates both a Service and Integration record, returning an API key.
        /// </summary>
        /// <remarks>
        /// This endpoint is designed for automated integration setup. The calling system
        /// authenticates with its OIDC token, and Alerting creates the necessary records
        /// and returns an API key for subsequent unauthenticated calls.
        /// 
        /// If a service with the same name already exists, it will be reused.
        /// If an integration already exists for that service, a new API key will be generated.
        /// </remarks>
        /// <param name="request">Registration request.</param>
        /// <returns>Registration response with API key.</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(SelfRegisterResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SelfRegister([FromBody] SelfRegisterRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request?.ServiceName))
            {
                return BadRequest(new { error = "ServiceName is required" });
            }

            StartAuditEventClock();

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, 
                    "Self-registration attempted by user without tenant: " + securityUser?.accountName, 
                    securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant.");
            }

            // Find or create the Service
            var serviceName = request.ServiceName.Trim();
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.name == serviceName && 
                                         s.tenantGuid == userTenantGuid && 
                                         s.deleted == false, cancellationToken);

            if (service == null)
            {
                // Create new service
                service = new Service
                {
                    tenantGuid = userTenantGuid,
                    name = serviceName,
                    description = request.Description?.Trim() ?? $"Auto-registered service from {serviceName}",
                    versionNumber = 1,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                var serviceChts = Service.GetChangeHistoryToolsetForWriting(_context, securityUser, false, cancellationToken);
                await serviceChts.AddEntityAsync(service);

                _logger.LogInformation("Created service {ServiceName} (ID: {ServiceId}) via self-registration",
                    serviceName, service.id);
            }

            // Find existing integration or create new one
            var integrationName = $"{serviceName} Auto-Integration";
            var integration = await _context.Integrations
                .FirstOrDefaultAsync(i => i.serviceId == service.id &&
                                         i.tenantGuid == userTenantGuid &&
                                         i.deleted == false, cancellationToken);

            // Generate API key
            string plainApiKey = GenerateApiKey();
            string apiKeyHash = HashApiKey(plainApiKey);

            if (integration == null)
            {
                // Create new integration
                integration = new Integration
                {
                    tenantGuid = userTenantGuid,
                    serviceId = service.id,
                    name = integrationName,
                    description = $"Auto-registered integration for {serviceName}",
                    apiKeyHash = apiKeyHash,
                    callbackWebhookUrl = request.CallbackUrl?.Trim(),
                    maxRetryAttempts = 3,
                    retryBackoffSeconds = 30,
                    versionNumber = 1,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                var integrationChts = Integration.GetChangeHistoryToolsetForWriting(_context, securityUser, false, cancellationToken);
                await integrationChts.AddEntityAsync(integration);

                _logger.LogInformation("Created integration {IntegrationName} (ID: {IntegrationId}) via self-registration",
                    integrationName, integration.id);
            }
            else
            {
                // Update existing integration with new API key
                integration.apiKeyHash = apiKeyHash;
                integration.callbackWebhookUrl = request.CallbackUrl?.Trim() ?? integration.callbackWebhookUrl;
                integration.active = true;

                var integrationChts = Integration.GetChangeHistoryToolsetForWriting(_context, securityUser, false, cancellationToken);
                await integrationChts.UpdateEntityAsync(integration);

                _logger.LogInformation("Regenerated API key for existing integration {IntegrationId} via self-registration",
                    integration.id);
            }

            await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                $"Self-registration completed for service '{serviceName}'",
                true,
                integration.id.ToString(),
                "",
                JsonSerializer.Serialize(new { ServiceId = service.id, IntegrationId = integration.id }),
                null);

            return Ok(new SelfRegisterResponse
            {
                IntegrationId = integration.id,
                IntegrationGuid = integration.objectGuid,
                ServiceId = service.id,
                ServiceName = service.name,
                ApiKey = plainApiKey,
                Message = "Registration successful. Store the API key securely - it cannot be retrieved again."
            });
        }


        #region Private Helpers

        private static string GenerateApiKey()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private static string HashApiKey(string apiKey)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
            return Convert.ToBase64String(bytes);
        }

        #endregion
    }
}
