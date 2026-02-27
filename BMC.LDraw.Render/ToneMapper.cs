using System;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// HDR-to-LDR tone mapping and gamma correction utilities.
    ///
    /// Applied as a post-process after the ray tracer accumulates linear HDR
    /// colour values.  ACES filmic tone mapping compresses the dynamic range
    /// so that bright highlights roll off naturally instead of hard-clamping,
    /// and sRGB gamma makes mid-tones look correct on standard displays.
    /// </summary>
    public static class ToneMapper
    {
        /// <summary>
        /// Apply ACES filmic tone mapping to a single linear HDR value.
        ///
        /// Uses the fitted curve from Krzysztof Narkowicz:
        ///   f(x) = (x * (2.51x + 0.03)) / (x * (2.43x + 0.59) + 0.14)
        /// </summary>
        public static float AcesFilmic(float x)
        {
            if (x < 0f) x = 0f;

            float a = 2.51f;
            float b = 0.03f;
            float c = 2.43f;
            float d = 0.59f;
            float e = 0.14f;

            float mapped = (x * (a * x + b)) / (x * (c * x + d) + e);

            return mapped > 1f ? 1f : mapped;
        }


        /// <summary>
        /// Convert a linear-space value to sRGB gamma space.
        /// Uses the standard sRGB transfer function.
        /// </summary>
        public static float LinearToSrgb(float linear)
        {
            if (linear <= 0f) return 0f;
            if (linear >= 1f) return 1f;

            if (linear <= 0.0031308f)
            {
                return linear * 12.92f;
            }

            return 1.055f * MathF.Pow(linear, 1f / 2.4f) - 0.055f;
        }


        /// <summary>
        /// Apply ACES tone mapping + sRGB gamma to an entire HDR float buffer
        /// and write the result into an RGBA byte buffer.
        ///
        /// The HDR buffer stores 3 floats per pixel (R, G, B) in linear space.
        /// The output buffer stores 4 bytes per pixel (R, G, B, A).
        /// Alpha values are passed through from <paramref name="alphaBuffer"/>.
        /// </summary>
        /// <param name="hdrBuffer">Linear HDR colour buffer (width × height × 3 floats).</param>
        /// <param name="alphaBuffer">Alpha values per pixel (width × height bytes).</param>
        /// <param name="outputBuffer">Destination RGBA byte buffer (width × height × 4 bytes).</param>
        /// <param name="pixelCount">Total number of pixels to process.</param>
        /// <param name="exposure">Exposure multiplier applied before tone mapping (default 1.0).</param>
        public static void Apply(float[] hdrBuffer, byte[] alphaBuffer,
                                 byte[] outputBuffer, int pixelCount,
                                 float exposure = 1f)
        {
            for (int i = 0; i < pixelCount; i++)
            {
                int hdrIdx = i * 3;
                int outIdx = i * 4;

                //
                // Apply exposure
                //
                float r = hdrBuffer[hdrIdx] * exposure;
                float g = hdrBuffer[hdrIdx + 1] * exposure;
                float b = hdrBuffer[hdrIdx + 2] * exposure;

                //
                // Tone map (HDR → [0, 1])
                //
                r = AcesFilmic(r);
                g = AcesFilmic(g);
                b = AcesFilmic(b);

                //
                // Gamma correct (linear → sRGB)
                //
                r = LinearToSrgb(r);
                g = LinearToSrgb(g);
                b = LinearToSrgb(b);

                //
                // Quantize to byte
                //
                outputBuffer[outIdx]     = (byte)(r * 255f + 0.5f);
                outputBuffer[outIdx + 1] = (byte)(g * 255f + 0.5f);
                outputBuffer[outIdx + 2] = (byte)(b * 255f + 0.5f);
                outputBuffer[outIdx + 3] = alphaBuffer[i];
            }
        }
    }
}
