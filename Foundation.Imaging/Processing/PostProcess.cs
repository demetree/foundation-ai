using System;

namespace Foundation.Imaging.Processing
{
    /// <summary>
    /// Post-processing utilities for RGBA pixel buffers.
    ///
    /// Currently provides SSAA (Super-Sample Anti-Aliasing) downsampling using a box filter.
    ///
    /// Relocated from BMC.LDraw.Render to Foundation.Imaging for cross-system reuse.
    /// AI-generated — Feb 2026 (Phase 2.2).
    /// </summary>
    public static class PostProcess
    {
        /// <summary>
        /// Downsample a high-resolution RGBA pixel buffer to a lower resolution using a box filter.
        /// Each destination pixel is the average of a block of source pixels.
        /// </summary>
        /// <param name="srcPixels">Source RGBA pixel buffer (srcW × srcH × 4 bytes).</param>
        /// <param name="srcW">Source width in pixels.</param>
        /// <param name="srcH">Source height in pixels.</param>
        /// <param name="dstW">Destination width in pixels.</param>
        /// <param name="dstH">Destination height in pixels.</param>
        /// <returns>Downsampled RGBA pixel buffer (dstW × dstH × 4 bytes).</returns>
        public static byte[] Downsample(byte[] srcPixels, int srcW, int srcH, int dstW, int dstH)
        {
            if (srcW == dstW && srcH == dstH)
            {
                return srcPixels;
            }

            byte[] dstPixels = new byte[dstW * dstH * 4];

            //
            // Scale factors (how many source pixels per destination pixel)
            //
            float scaleX = (float)srcW / dstW;
            float scaleY = (float)srcH / dstH;

            for (int dy = 0; dy < dstH; dy++)
            {
                //
                // Source Y range for this destination row
                //
                int srcY0 = (int)(dy * scaleY);
                int srcY1 = Math.Min((int)((dy + 1) * scaleY), srcH);

                for (int dx = 0; dx < dstW; dx++)
                {
                    //
                    // Source X range for this destination column
                    //
                    int srcX0 = (int)(dx * scaleX);
                    int srcX1 = Math.Min((int)((dx + 1) * scaleX), srcW);

                    //
                    // Accumulate all source pixels in the block
                    //
                    int sumR = 0, sumG = 0, sumB = 0, sumA = 0;
                    int count = 0;

                    for (int sy = srcY0; sy < srcY1; sy++)
                    {
                        for (int sx = srcX0; sx < srcX1; sx++)
                        {
                            int srcIdx = (sy * srcW + sx) * 4;
                            sumR += srcPixels[srcIdx];
                            sumG += srcPixels[srcIdx + 1];
                            sumB += srcPixels[srcIdx + 2];
                            sumA += srcPixels[srcIdx + 3];
                            count++;
                        }
                    }

                    //
                    // Average and write to destination
                    //
                    if (count > 0)
                    {
                        int dstIdx = (dy * dstW + dx) * 4;
                        dstPixels[dstIdx]     = (byte)(sumR / count);
                        dstPixels[dstIdx + 1] = (byte)(sumG / count);
                        dstPixels[dstIdx + 2] = (byte)(sumB / count);
                        dstPixels[dstIdx + 3] = (byte)(sumA / count);
                    }
                }
            }

            return dstPixels;
        }
    }
}
