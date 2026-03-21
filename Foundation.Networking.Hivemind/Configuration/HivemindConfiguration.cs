// ============================================================================
//
// HivemindConfiguration.cs — Configuration for the Hivemind cache/state system.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Collections.Generic;

namespace Foundation.Networking.Hivemind.Configuration
{
    /// <summary>
    ///
    /// Configuration for the Hivemind distributed cache and state system.
    ///
    /// </summary>
    public class HivemindConfiguration
    {
        //
        // ── Cache Settings ───────────────────────────────────────────────
        //

        /// <summary>
        /// Default cache entry TTL in seconds.
        /// </summary>
        public int DefaultTtlSeconds { get; set; } = 300;

        /// <summary>
        /// Maximum number of cache entries before eviction kicks in.
        /// </summary>
        public int MaxCacheEntries { get; set; } = 10000;

        /// <summary>
        /// Eviction strategy: "LRU" or "LFU".
        /// </summary>
        public string EvictionStrategy { get; set; } = "LRU";

        //
        // ── Session Settings ─────────────────────────────────────────────
        //

        /// <summary>
        /// Default session TTL in seconds.
        /// </summary>
        public int SessionTtlSeconds { get; set; } = 1800;

        /// <summary>
        /// Whether to extend session TTL on each access.
        /// </summary>
        public bool SessionSlidingExpiry { get; set; } = true;

        //
        // ── Pub/Sub Settings ─────────────────────────────────────────────
        //

        /// <summary>
        /// Maximum message queue depth per channel.
        /// </summary>
        public int PubSubMaxQueueDepth { get; set; } = 1000;

        //
        // ── Cleanup ──────────────────────────────────────────────────────
        //

        /// <summary>
        /// How often to run the expired-entry cleanup, in seconds.
        /// </summary>
        public int CleanupIntervalSeconds { get; set; } = 60;
    }
}
