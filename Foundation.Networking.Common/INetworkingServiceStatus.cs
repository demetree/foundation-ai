// ============================================================================
//
// INetworkingServiceStatus.cs — Standard contract for networking service status.
//
// All Foundation.Networking services implement this interface so the
// Networking Dashboard can query them uniformly.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.Networking.Common
{
    /// <summary>
    ///
    /// Standard interface for any Foundation.Networking service to report
    /// its operational status.  Consumed by the Networking Dashboard.
    ///
    /// </summary>
    public interface INetworkingServiceStatus
    {
        /// <summary>
        /// Display name of the service (e.g., "Deep Space", "Coturn", "Skynet").
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// Whether the service is currently running and accepting requests.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// UTC timestamp when the service was started.  Null if not started.
        /// </summary>
        DateTime? StartedAtUtc { get; }

        /// <summary>
        /// Gets detailed status information about the service.
        /// </summary>
        Task<NetworkingServiceInfo> GetStatusAsync(CancellationToken cancellationToken = default);
    }
}
