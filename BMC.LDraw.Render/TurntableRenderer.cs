using System;
using System.IO;
using BMC.LDraw.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Renders an LDraw mesh as a 360° turntable animation assembled into an animated GIF.
    ///
    /// Each frame is rendered at an evenly-spaced azimuth angle using SoftwareRenderer,
    /// then composed into a looping GIF via ImageSharp's GifEncoder.
    ///
    /// AI-generated — Phase 3.2, Feb 2026.
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
            // Create the output GIF image with the first frame
            //
            using (Image<Rgba32> gif = new Image<Rgba32>(width, height))
            {
                gif.Frames.RemoveFrame(0); // Remove the default empty frame

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
                    // Create an ImageSharp frame from the pixel data
                    //
                    using (Image<Rgba32> frame = Image.LoadPixelData<Rgba32>(pixels, width, height))
                    {
                        //
                        // Add frame to the GIF
                        //
                        ImageFrame<Rgba32> addedFrame = gif.Frames.AddFrame(frame.Frames.RootFrame);

                        //
                        // Set frame delay (in hundredths of a second, GIF spec)
                        //
                        GifFrameMetadata frameMeta = addedFrame.Metadata.GetGifMetadata();
                        frameMeta.FrameDelay = frameDelayMs / 10;
                    }
                }

                //
                // Configure GIF metadata for looping
                //
                GifMetadata gifMeta = gif.Metadata.GetGifMetadata();
                gifMeta.RepeatCount = 0; // 0 = loop forever

                //
                // Encode to byte array
                //
                using (MemoryStream ms = new MemoryStream())
                {
                    GifEncoder encoder = new GifEncoder
                    {
                        ColorTableMode = GifColorTableMode.Local
                    };
                    gif.SaveAsGif(ms, encoder);
                    return ms.ToArray();
                }
            }
        }
    }
}
