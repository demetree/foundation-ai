namespace BMC.LDraw.Render.RayTracing
{
    /// <summary>
    /// Interface for environment lighting sources.
    ///
    /// Implementations provide the colour seen when a ray misses all geometry
    /// (sky dome, HDR image, studio backdrop, etc.).  The same sample is used
    /// for both the visible background and reflection/IBL rays.
    /// </summary>
    public interface IEnvironmentMap
    {
        /// <summary>
        /// Sample the environment in the given world-space direction.
        /// Returns linear HDR colour (values may exceed 1.0 for bright sources).
        /// </summary>
        /// <param name="dirX">Normalised ray direction X.</param>
        /// <param name="dirY">Normalised ray direction Y.</param>
        /// <param name="dirZ">Normalised ray direction Z.</param>
        /// <param name="r">Red channel (linear HDR).</param>
        /// <param name="g">Green channel (linear HDR).</param>
        /// <param name="b">Blue channel (linear HDR).</param>
        void Sample(float dirX, float dirY, float dirZ,
                    out float r, out float g, out float b);
    }
}
