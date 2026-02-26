namespace BMC.LDraw.Render
{
    /// <summary>
    /// Common interface for LDraw renderers.
    ///
    /// Both the rasterizer (SoftwareRenderer) and ray tracer (RayTraceRenderer)
    /// implement this interface, allowing RenderService to select between them.
    /// </summary>
    public interface IRenderer
    {
        /// <summary>Width of the render target in pixels.</summary>
        int Width { get; }

        /// <summary>Height of the render target in pixels.</summary>
        int Height { get; }

        /// <summary>The lighting configuration for this renderer.</summary>
        LightingModel Lighting { get; set; }

        /// <summary>Set a solid background colour.</summary>
        void SetBackground(byte r, byte g, byte b, byte a);

        /// <summary>Set a vertical gradient background.</summary>
        void SetGradientBackground(byte topR, byte topG, byte topB,
                                   byte bottomR, byte bottomG, byte bottomB);

        /// <summary>
        /// Render a mesh using the given camera.  Returns the RGBA pixel buffer
        /// (width × height × 4 bytes).
        /// </summary>
        byte[] Render(Models.LDrawMesh mesh, Camera camera);
    }
}
