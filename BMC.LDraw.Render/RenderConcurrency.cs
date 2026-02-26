using System;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Shared concurrency settings for render-related parallelism.
    ///
    /// Rendering operations (rasterization, ray tracing, normal smoothing) are
    /// CPU-intensive and will happily saturate all available cores.  On a
    /// web server that also handles HTTP requests, SignalR connections, and
    /// database queries, allowing a single render to consume every core
    /// starves the rest of the application and degrades responsiveness for
    /// all users.
    ///
    /// Capping at half the logical processors keeps render throughput high
    /// while reserving headroom for ASP.NET request handling, GC, and other
    /// background work.  This is especially important during manual generation,
    /// where dozens of sequential renders queue up back-to-back.
    /// </summary>
    public static class RenderConcurrency
    {
        /// <summary>
        /// Maximum number of threads any single render operation should use.
        /// Set to half the logical processor count (minimum 1).
        /// </summary>
        public static readonly int MaxThreads = Math.Max(1, Environment.ProcessorCount / 2);
    }
}
