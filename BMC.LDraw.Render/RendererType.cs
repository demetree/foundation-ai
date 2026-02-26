namespace BMC.LDraw.Render
{
    /// <summary>
    /// Selects which rendering backend to use.
    /// </summary>
    public enum RendererType
    {
        /// <summary>CPU rasterizer with Z-buffer (fast, instruction-manual style).</summary>
        Rasterizer,

        /// <summary>CPU ray tracer with BVH acceleration (shadows, reflections).</summary>
        RayTracer
    }
}
