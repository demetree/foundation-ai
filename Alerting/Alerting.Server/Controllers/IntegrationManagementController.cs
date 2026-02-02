//
// Integration Management Controller
//
// Custom REST API for managing integrations with secure API key generation.
// Handles key generation and hashing on the server side - clients never see the hash algorithm.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Foundation.Alerting.Database;
using Foundation.Auditor;
using Foundation.Security;
using Foundation.Security.Database;


namespace Alerting.Server.Controllers
{
    /// <summary>
    /// Custom API for managing integrations with secure API key generation.
    /// Keys are generated and hashed server-side - clients receive the plain key once at creation.
    /// </summary>
    [ApiController]
    [Route("api/integration-management")]
    [Authorize]
    public class IntegrationManagementController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
        public const int WRITE_PERMISSION_LEVEL_REQUIRED = 150;

        private readonly AlertingContext _context;
        private readonly ILogger<IntegrationManagementController> _logger;


        #region Request/Response DTOs

        public class CreateIntegrationRequest
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public int ServiceId { get; set; }
            public string WebhookUrl { get; set; }
            public int? MaxRetryAttempts { get; set; }
            public int? RetryBackoffSeconds { get; set; }
            /// <summary>
            /// IDs of IncidentEventTypes that should trigger callbacks for this integration.
            /// If empty or null, no automatic callbacks will be triggered.
            /// </summary>
            public List<int> CallbackEventTypeIds { get; set; }
        }

        public class UpdateIntegrationRequest
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string WebhookUrl { get; set; }
            public bool? Active { get; set; }
            public int? MaxRetryAttempts { get; set; }
            public int? RetryBackoffSeconds { get; set; }
            /// <summary>
            /// IDs of IncidentEventTypes that should trigger callbacks. Pass null to leave unchanged.
            /// </summary>
            public List<int> CallbackEventTypeIds { get; set; }
        }

        public class IntegrationDto
        {
            public int Id { get; set; }
            public Guid ObjectGuid { get; set; }
            public int ServiceId { get; set; }
            public string ServiceName { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string WebhookUrl { get; set; }
            public bool Active { get; set; }
            public int VersionNumber { get; set; }
            // Retry settings
            public int? MaxRetryAttempts { get; set; }
            public int? RetryBackoffSeconds { get; set; }
            // Callback status (read-only)
            public DateTime? LastCallbackSuccessAt { get; set; }
            public int? ConsecutiveCallbackFailures { get; set; }
            // Selected event types for callbacks
            public List<CallbackEventTypeDto> CallbackEventTypes { get; set; }
        }

        public class CallbackEventTypeDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class IntegrationCreatedResponse : IntegrationDto
        {
            /// <summary>
            /// The plain API key. This is returned ONLY on creation.
            /// Store it securely - it cannot be retrieved again.
            /// </summary>
            public string ApiKey { get; set; }
        }

        public class RegenerateKeyResponse
        {
            public int IntegrationId { get; set; }
            public string IntegrationName { get; set; }
            /// <summary>
            /// The new plain API key. Store it securely - it cannot be retrieved again.
            /// </summary>
            public string ApiKey { get; set; }
            public string Message { get; set; }
        }

        #endregion

        public IntegrationManagementController(
            AlertingContext context,
            ILogger<IntegrationManagementController> logger) : base("Alerting", "IntegrationManagment")
        {
            _context = context;
            _logger = logger;
        }


        /// <summary>
        /// Create a new integration with auto-generated API key.
        /// </summary>
        /// <remarks>
        /// The API key is returned ONLY in this response - it cannot be retrieved later.
        /// Store the key securely; only the hash is persisted in the database.
        /// </remarks>
        /// <param name="request">Integration creation request.</param>
        /// <returns>Created integration with one-time API key.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(IntegrationCreatedResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateIntegration([FromBody] CreateIntegrationRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
            {
                return BadRequest(new { error = "Name is required" });
            }

            if (request.ServiceId <= 0)
            {
                return BadRequest(new { error = "Valid ServiceId is required" });
            }


            //
            // Alerting Master Config Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
            //
            if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting Master Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
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
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }


            // Validate that the service exists and belongs to this tenant
            Service service = await _context.Services.AsNoTracking()
                                                     .FirstOrDefaultAsync(s => s.id == request.ServiceId &&
                                                                               s.tenantGuid == userTenantGuid &&
                                                                               s.active == true && s.deleted == false);
                                                     

            if (service == null)
            {
                return BadRequest(new { error = "Service not found" });
            }

            // Generate API key and hash
            string plainApiKey = GenerateApiKey();
            string apiKeyHash = HashApiKey(plainApiKey);

            // Create the integration
            Integration integration = new Integration
            {
                tenantGuid = userTenantGuid,
                serviceId = request.ServiceId,
                name = request.Name.Trim(),
                description = request.Description?.Trim(),
                apiKeyHash = apiKeyHash,
                callbackWebhookUrl = request.WebhookUrl?.Trim(),
                maxRetryAttempts = request.MaxRetryAttempts,
                retryBackoffSeconds = request.RetryBackoffSeconds,
                versionNumber = 1,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            var chts = Integration.GetChangeHistoryToolsetForWriting(_context, securityUser, false, cancellationToken);

            await chts.AddEntityAsync(integration);

            // Create callback event type mappings if specified
            if (request.CallbackEventTypeIds?.Any() == true)
            {
                var eventTypeChts = IntegrationCallbackIncidentEventType.GetChangeHistoryToolsetForWriting(_context, securityUser, false, cancellationToken);
                
                foreach (var eventTypeId in request.CallbackEventTypeIds.Distinct())
                {
                    var callbackEventType = new IntegrationCallbackIncidentEventType
                    {
                        tenantGuid = userTenantGuid,
                        integrationId = integration.id,
                        incidentEventTypeId = eventTypeId,
                        versionNumber = 1,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };
                    await eventTypeChts.AddEntityAsync(callbackEventType);
                }
            }


            await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                                        "Alerting.Integration entity successfully created and pass key hash calculated.",
                                        true,
                                        integration.id.ToString(),
                                        "",
                                        JsonSerializer.Serialize(Integration.CreateAnonymousWithFirstLevelSubObjects(integration)),
                                        null);


            _logger.LogInformation("Integration {IntegrationId} created for service {ServiceId} by user", integration.id, request.ServiceId);


            return CreatedAtAction(nameof(GetIntegration), new { id = integration.id }, new IntegrationCreatedResponse
            {
                Id = integration.id,
                ObjectGuid = integration.objectGuid,
                ServiceId = integration.serviceId,
                ServiceName = service.name,
                Name = integration.name,
                Description = integration.description,
                WebhookUrl = integration.callbackWebhookUrl,
                Active = integration.active,
                VersionNumber = integration.versionNumber,
                // This is the ONLY time the plain API key is returned
                ApiKey = plainApiKey
            });
        }


        /// <summary>
        /// Get an integration by ID.
        /// </summary>
        /// <param name="id">Integration ID.</param>
        /// <returns>Integration details (without API key).</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IntegrationDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetIntegration(int id, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }


            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

            bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }

            Integration integration = await _context.Integrations
                .Include(i => i.service)
                .Include(i => i.IntegrationCallbackIncidentEventTypes.Where(m => m.active && !m.deleted))
                    .ThenInclude(m => m.incidentEventType)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.id == id && i.tenantGuid == userTenantGuid && i.deleted == false);

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Alerting.Integration Entity was read with Admin privilege." : "Alerting.Integration Entity was read.");


            if (integration == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(integration));
        }


        /// <summary>
        /// List integrations for the current tenant.
        /// </summary>
        /// <param name="serviceId">Optional filter by service.</param>
        /// <param name="includeInactive">Include inactive integrations.</param>
        /// <returns>List of integrations.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<IntegrationDto>), 200)]
        public async Task<IActionResult> GetIntegrations([FromQuery] int? serviceId = null,
                                                         [FromQuery] bool includeInactive = false,
                                                         CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }


            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

            bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
            bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

            bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }

            var query = _context.Integrations
                .Include(i => i.service)
                .Where(i => i.tenantGuid == userTenantGuid && i.deleted == false);

            if (!includeInactive)
            {
                query = query.Where(i => i.active);
            }

            if (serviceId.HasValue)
            {
                query = query.Where(i => i.serviceId == serviceId.Value);
            }

            var integrations = await query
                .OrderBy(i => i.name)
                .Take(100)
                .Select(i => new IntegrationDto
                {
                    Id = i.id,
                    ObjectGuid = i.objectGuid,
                    ServiceId = i.serviceId,
                    ServiceName = i.service.name,
                    Name = i.name,
                    Description = i.description,
                    WebhookUrl = i.callbackWebhookUrl,
                    Active = i.active,
                    VersionNumber = i.versionNumber,
                    MaxRetryAttempts = i.maxRetryAttempts,
                    RetryBackoffSeconds = i.retryBackoffSeconds,
                    LastCallbackSuccessAt = i.lastCallbackSuccessAt,
                    ConsecutiveCallbackFailures = i.consecutiveCallbackFailures,
                    CallbackEventTypes = i.IntegrationCallbackIncidentEventTypes
                        .Where(m => m.active && !m.deleted)
                        .Select(m => new CallbackEventTypeDto
                        {
                            Id = m.incidentEventTypeId,
                            Name = m.incidentEventType.name
                        })
                        .ToList()
                })
                .ToListAsync();

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Alerting.Integration Entity list was read with Admin privilege.  Returning " + integrations.Count + " rows of data." : "Alerting.Integration Entity list was read.  Returning " + integrations.Count + " rows of data.");


            return Ok(integrations);
        }


        /// <summary>
        /// Regenerate the API key for an integration.
        /// </summary>
        /// <remarks>
        /// The new API key is returned ONLY in this response.
        /// The old key is immediately invalidated.
        /// </remarks>
        /// <param name="id">Integration ID.</param>
        /// <returns>New API key.</returns>
        [HttpPost("{id}/regenerate-key")]
        [ProducesResponseType(typeof(RegenerateKeyResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RegenerateApiKey(int id, CancellationToken cancellationToken = default)
        {
            //
            // Alerting Master Config Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
            //
            if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting Master Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
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
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }


            Integration integration = await _context.Integrations
                .FirstOrDefaultAsync(i => i.id == id && i.tenantGuid == userTenantGuid && i.deleted == false)
                .ConfigureAwait(false);

            if (integration == null)
            {
                return NotFound();
            }

            // Generate new key and hash
            string plainApiKey = GenerateApiKey();
            string apiKeyHash = HashApiKey(plainApiKey);

            integration.apiKeyHash = apiKeyHash;


            var chts = Integration.GetChangeHistoryToolsetForWriting(_context, securityUser, false, cancellationToken);

            await chts.UpdateEntityAsync(integration);


            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                                        $"API key regenerated for integration {id}",
                                        true,
                                        integration.id.ToString(),
                                        "",
                                        JsonSerializer.Serialize(Integration.CreateAnonymousWithFirstLevelSubObjects(integration)),
                                        null);


            _logger.LogInformation($"API key regenerated for integration {id}");

            return Ok(new RegenerateKeyResponse
            {
                IntegrationId = integration.id,
                IntegrationName = integration.name,
                // This is the ONLY time the new plain API key is returned
                ApiKey = plainApiKey,
                Message = "API key regenerated successfully. Store this key securely - it cannot be retrieved again."
            });
        }


        /// <summary>
        /// Update an integration (name, description, webhook, active status).
        /// </summary>
        /// <param name="id">Integration ID.</param>
        /// <param name="request">Update request.</param>
        /// <returns>Updated integration.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IntegrationDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateIntegration(int id, [FromBody] UpdateIntegrationRequest request, CancellationToken cancellationToken = default)
        {
            //
            // Alerting Master Config Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
            //
            if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting Master Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
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
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }

            var integration = await _context.Integrations
                .Include(i => i.service)
                .FirstOrDefaultAsync(i => i.id == id && i.tenantGuid == userTenantGuid && i.deleted == false)
                .ConfigureAwait(false);

            if (integration == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                integration.name = request.Name.Trim();
            }

            integration.description = request.Description?.Trim();
            integration.callbackWebhookUrl = request.WebhookUrl?.Trim();

            if (request.Active.HasValue)
            {
                integration.active = request.Active.Value;
            }

            // Update retry settings (allow explicit null to clear)
            if (request.MaxRetryAttempts.HasValue)
            {
                integration.maxRetryAttempts = request.MaxRetryAttempts;
            }
            if (request.RetryBackoffSeconds.HasValue)
            {
                integration.retryBackoffSeconds = request.RetryBackoffSeconds;
            }

            var chts = Integration.GetChangeHistoryToolsetForWriting(_context, securityUser, false, cancellationToken);

            await chts.UpdateEntityAsync(integration);

            // Sync callback event type mappings if specified (null = no change)
            if (request.CallbackEventTypeIds != null)
            {
                var eventTypeChts = IntegrationCallbackIncidentEventType.GetChangeHistoryToolsetForWriting(_context, securityUser, false, cancellationToken);
                
                // Get existing active mappings
                var existingMappings = await _context.IntegrationCallbackIncidentEventTypes
                    .Where(m => m.integrationId == id && m.active == true && m.deleted == false)
                    .ToListAsync(cancellationToken);

                var existingEventTypeIds = existingMappings.Select(m => m.incidentEventTypeId).ToHashSet();
                var requestedEventTypeIds = request.CallbackEventTypeIds.Distinct().ToHashSet();

                // Soft-delete removed mappings
                foreach (var mapping in existingMappings.Where(m => !requestedEventTypeIds.Contains(m.incidentEventTypeId)))
                {
                    mapping.deleted = true;
                    await eventTypeChts.UpdateEntityAsync(mapping);
                }

                // Add new mappings
                foreach (var eventTypeId in requestedEventTypeIds.Where(id => !existingEventTypeIds.Contains(id)))
                {
                    var newMapping = new IntegrationCallbackIncidentEventType
                    {
                        tenantGuid = userTenantGuid,
                        integrationId = integration.id,
                        incidentEventTypeId = eventTypeId,
                        versionNumber = 1,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };
                    await eventTypeChts.AddEntityAsync(newMapping);
                }
            }


            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                                        $"Integration {id} has been updated",
                                        true,
                                        integration.id.ToString(),
                                        "",
                                        JsonSerializer.Serialize(Integration.CreateAnonymousWithFirstLevelSubObjects(integration)),
                                        null);

            _logger.LogInformation($"Integration {id} has been updated");

            return Ok(MapToDto(integration));
        }


        /// <summary>
        /// Delete an integration (soft delete).
        /// </summary>
        /// <param name="id">Integration ID.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteIntegration(int id, CancellationToken cancellationToken = default)
        {
            //
            // Alerting Master Config Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
            //
            if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting Master Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
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
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }


            Integration integration = await _context.Integrations
                .FirstOrDefaultAsync(i => i.id == id && i.tenantGuid == userTenantGuid && i.deleted == false);

            if (integration == null)
            {
                return NotFound();
            }

            integration.deleted = true;
            integration.active = false;

            var chts = Integration.GetChangeHistoryToolsetForWriting(_context, securityUser, false, cancellationToken);

            await chts.UpdateEntityAsync(integration);


            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                                        $"Integration {id} deleted",
                                        true,
                                        integration.id.ToString(),
                                        "",
                                        JsonSerializer.Serialize(Integration.CreateAnonymousWithFirstLevelSubObjects(integration)),
                                        null);



            _logger.LogInformation($"Integration {id} deleted");

            return NoContent();
        }


        #region Private Helpers

        /// <summary>
        /// Generate a cryptographically secure random API key.
        /// </summary>
        private static string GenerateApiKey()
        {
            // Generate 32 bytes (256 bits) of random data
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            // Convert to hex string (64 characters)
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        /// <summary>
        /// Hash an API key using SHA-256 and encode as Base64.
        /// This must match the algorithm used in AlertingService.HashApiKey().
        /// </summary>
        private static string HashApiKey(string apiKey)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
            return Convert.ToBase64String(bytes);
        }

        private static IntegrationDto MapToDto(Integration integration)
        {
            return new IntegrationDto
            {
                Id = integration.id,
                ObjectGuid = integration.objectGuid,
                ServiceId = integration.serviceId,
                ServiceName = integration.service?.name,
                Name = integration.name,
                Description = integration.description,
                WebhookUrl = integration.callbackWebhookUrl,
                Active = integration.active,
                VersionNumber = integration.versionNumber,
                MaxRetryAttempts = integration.maxRetryAttempts,
                RetryBackoffSeconds = integration.retryBackoffSeconds,
                LastCallbackSuccessAt = integration.lastCallbackSuccessAt,
                ConsecutiveCallbackFailures = integration.consecutiveCallbackFailures,
                CallbackEventTypes = integration.IntegrationCallbackIncidentEventTypes?
                    .Where(m => m.active && !m.deleted)
                    .Select(m => new CallbackEventTypeDto
                    {
                        Id = m.incidentEventTypeId,
                        Name = m.incidentEventType?.name
                    })
                    .ToList() ?? new List<CallbackEventTypeDto>()
            };
        }

        #endregion
    }
}
