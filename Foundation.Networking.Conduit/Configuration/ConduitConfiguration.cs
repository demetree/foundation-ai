// ============================================================================
//
// ConduitConfiguration.cs — Configuration for the Conduit WebSocket gateway.
//
// AI-Developed | Gemini
//
// ============================================================================

namespace Foundation.Networking.Conduit.Configuration
{
    /// <summary>
    ///
    /// Configuration for the Conduit WebSocket gateway.
    ///
    /// </summary>
    public class ConduitConfiguration
    {
        //
        // ── Connection Settings ──────────────────────────────────────────
        //

        /// <summary>
        /// Maximum concurrent connections.
        /// </summary>
        public int MaxConnections { get; set; } = 10000;

        /// <summary>
        /// Idle timeout in seconds before a connection is closed.
        /// </summary>
        public int IdleTimeoutSeconds { get; set; } = 300;

        /// <summary>
        /// Heartbeat / ping interval in seconds.
        /// </summary>
        public int PingIntervalSeconds { get; set; } = 30;

        /// <summary>
        /// Maximum message size in bytes.
        /// </summary>
        public int MaxMessageSizeBytes { get; set; } = 65536;

        //
        // ── Channel Settings ─────────────────────────────────────────────
        //

        /// <summary>
        /// Maximum subscribers per channel.
        /// </summary>
        public int MaxSubscribersPerChannel { get; set; } = 1000;

        /// <summary>
        /// Maximum channels a single connection can join.
        /// </summary>
        public int MaxChannelsPerConnection { get; set; } = 50;

        /// <summary>
        /// Message history depth per channel.
        /// </summary>
        public int ChannelHistoryDepth { get; set; } = 100;
    }
}
