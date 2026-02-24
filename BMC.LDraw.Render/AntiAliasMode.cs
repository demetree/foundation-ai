namespace BMC.LDraw.Render
{
    /// <summary>
    /// Anti-aliasing mode for the rendering pipeline.
    ///
    /// AI-generated — Feb 2026 (Phase 2.2).
    /// </summary>
    public enum AntiAliasMode
    {
        /// <summary>No anti-aliasing.  Fastest, jagged edges on diagonals.</summary>
        None,

        /// <summary>2× Super-Sample Anti-Aliasing.  Render at double resolution, downsample with box filter.</summary>
        SSAA2x,

        /// <summary>4× Super-Sample Anti-Aliasing.  Render at quadruple resolution, downsample.  High quality, 16× slower.</summary>
        SSAA4x
    }
}
