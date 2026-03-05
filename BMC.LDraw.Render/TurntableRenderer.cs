using BMC.LDraw.Models;
using Foundation.Imaging.Encoders;
using System.IO;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Renders a turntable animation of an LDraw model as an animated GIF.
    ///
    /// Rotates the camera 360° around the model, rendering each frame with
    /// the software renderer, then encodes all frames into a looping GIF.
    ///
    /// Uses the built-in GifEncoder (no external dependencies).
    /// </summary>
    public static class TurntableRenderer
    {
        /// <summary>
        /// Render a turntable animation and return the GIF bytes.
        /// </summary>
        /// <param name="mesh">Pre-resolved LDraw mesh (call NormalSmoother.Smooth before passing if desired).</param>
        /// <param name="width">Frame width in pixels.</param>
        /// <param name="height">Frame height in pixels.</param>
        /// <param name="frameCount">Number of frames in the animation (e.g. 36 for 10° per frame).</param>
        /// <param name="elevation">Camera elevation angle in degrees.</param>
        /// <param name="frameDelayMs">Delay between frames in milliseconds (default 80ms ≈ 12.5 fps).</param>
        /// <param name="renderEdges">Whether to render edge lines.</param>
        /// <param name="smoothShading">Whether to use smooth shading.</param>
        /// <param name="lighting">Optional lighting model.  Null uses the default.</param>
        /// <returns>Animated GIF as a byte array.</returns>
        public static byte[] RenderToGif(
            LDrawMesh mesh,
            int width,
            int height,
            int frameCount = 36,
            float elevation = 30f,
            int frameDelayMs = 80,
            bool renderEdges = true,
            bool smoothShading = true,
            LightingModel lighting = null)
        {
            if (frameCount < 1) frameCount = 1;

            //
            // Create the GIF encoder
            //
            GifEncoder gif = new GifEncoder(width, height, frameDelayMs);

            for (int i = 0; i < frameCount; i++)
            {
                //
                // Compute azimuth: evenly distribute across 360°
                //
                float azimuth = -180f + (360f * i / frameCount);

                //
                // Set up camera for this frame
                //
                Camera camera = new Camera();
                camera.AutoFrame(mesh, elevation, azimuth);

                //
                // Render the frame
                //
                SoftwareRenderer renderer = new SoftwareRenderer(width, height);
                renderer.RenderEdges = renderEdges;
                renderer.SmoothShading = smoothShading;
                if (lighting != null)
                {
                    renderer.Lighting = lighting;
                }

                byte[] pixels = renderer.Render(mesh, camera);

                //
                // Add frame to the GIF
                //
                gif.AddFrame(pixels);
            }

            //
            // Encode to byte array
            //
            return gif.Encode();
        }
    }
}
