//
// Alerting Integration Service Interface
//
// Defines the contract for Foundation systems to interact with Alerting.
//
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.Web.Services.Alerting
{
    /// <summary>
    /// Client service for integrating with the Alerting system.
    /// Provides methods for raising incidents, querying status, and self-registration.
    /// </summary>
    public interface IAlertingIntegrationService
    {
        /// <summary>
        /// Gets whether the service is configured and ready to use.
        /// </summary>
        bool IsConfigured { get; }

        /// <summary>
        /// Register this system with Alerting using OIDC authentication.
        /// Returns the API key which should be stored securely.
        /// </summary>
        /// <param name="accessToken">OIDC access token for authentication.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Registration result with API key.</returns>
        Task<RegistrationResponse> RegisterAsync(string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Raise a new incident or update an existing one (if deduplication key matches).
        /// </summary>
        /// <param name="request">Incident details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Incident response with key and status.</returns>
        Task<IncidentResponse> RaiseIncidentAsync(RaiseIncidentRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the current status of an incident by its key.
        /// </summary>
        /// <param name="incidentKey">The incident key (e.g., "INC-00001").</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Current incident status.</returns>
        Task<IncidentStatusResponse> GetIncidentStatusAsync(string incidentKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a list of incidents raised by this integration.
        /// </summary>
        /// <param name="filter">Optional filter criteria.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of incident summaries.</returns>
        Task<List<IncidentSummary>> GetMyIncidentsAsync(IncidentFilter filter = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolve an incident programmatically.
        /// </summary>
        /// <param name="incidentKey">The incident key.</param>
        /// <param name="resolution">Optional resolution note.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Updated incident status.</returns>
        Task<IncidentStatusResponse> ResolveIncidentAsync(string incidentKey, string resolution = null, CancellationToken cancellationToken = default);
    }
}
