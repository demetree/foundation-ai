using System;
using System.Buffers;
using System.Threading.Tasks;

namespace Foundation.Imaging.Processing
{
    public enum ResampleFilter
    {
        Nearest,
        Bilinear,
        CatmullRom,
        MitchellNetravali,
        Lanczos3,
    }

    public static class Resampler
    {
        // ─── Kernel functions ────────────────────────────────────────

        public static float CatmullRomKernel(float x)
        {
            if (x < 0f) x = -x;
            if (x < 1f)
                return 1.5f * x * x * x - 2.5f * x * x + 1f;
            if (x < 2f)
                return -0.5f * x * x * x + 2.5f * x * x - 4f * x + 2f;
            return 0f;
        }

        public static float MitchellNetravaliKernel(float x)
        {
            if (x < 0f) x = -x;
            float B = 1f / 3f, C = 1f / 3f;
            if (x < 1f)
                return ((12f - 9f * B - 6f * C) * x * x * x +
                        (-18f + 12f * B + 6f * C) * x * x +
                        (6f - 2f * B)) / 6f;
            if (x < 2f)
                return ((-B - 6f * C) * x * x * x +
                        (6f * B + 30f * C) * x * x +
                        (-12f * B - 48f * C) * x +
                        (8f * B + 24f * C)) / 6f;
            return 0f;
        }

        public static float Lanczos3Kernel(float x)
        {
            if (x < 0f) x = -x;
            if (x < 3f)
            {
                if (x < 1e-6f) return 1f;
                float pix = MathF.PI * x;
                return (MathF.Sin(pix) / pix) * (MathF.Sin(pix / 3f) / (pix / 3f));
            }
            return 0f;
        }

        private static int KernelRadius(ResampleFilter filter) => filter switch
        {
            ResampleFilter.CatmullRom => 2,
            ResampleFilter.MitchellNetravali => 2,
            ResampleFilter.Lanczos3 => 3,
            _ => 3,
        };

        private static Func<float, float> KernelFunc(ResampleFilter filter) => filter switch
        {
            ResampleFilter.CatmullRom => CatmullRomKernel,
            ResampleFilter.MitchellNetravali => MitchellNetravaliKernel,
            ResampleFilter.Lanczos3 => Lanczos3Kernel,
            _ => Lanczos3Kernel,
        };

        // ─── BGRA (4-channel) resizing ──────────────────────────────

        public static byte[] ResizeBgra(
            byte[] src, int srcW, int srcH, int dstW, int dstH,
            ResampleFilter filter = ResampleFilter.Lanczos3)
        {
            if (srcW == dstW && srcH == dstH) return src;

            switch (filter)
            {
                case ResampleFilter.Nearest:
                    return ResizeNearest(src, srcW, srcH, dstW, dstH);
                case ResampleFilter.Bilinear:
                    return ResizeBilinear(src, srcW, srcH, dstW, dstH);
                default:
                    return ResizeWithKernel(src, srcW, srcH, dstW, dstH,
                        KernelFunc(filter), KernelRadius(filter));
            }
        }

        // ─── RGB (3-channel) resizing ───────────────────────────────

        public static byte[] ResizeRgb(
            byte[] src, int srcW, int srcH, int dstW, int dstH,
            ResampleFilter filter = ResampleFilter.Lanczos3)
        {
            if (srcW == dstW && srcH == dstH) return src;

            switch (filter)
            {
                case ResampleFilter.Nearest:
                    return ResizeRgbNearest(src, srcW, srcH, dstW, dstH);
                case ResampleFilter.Bilinear:
                    return ResizeRgbBilinear(src, srcW, srcH, dstW, dstH);
                default:
                    return ResizeRgbWithKernel(src, srcW, srcH, dstW, dstH,
                        KernelFunc(filter), KernelRadius(filter));
            }
        }

        // ─── Nearest-neighbour ──────────────────────────────────────

        private static byte[] ResizeNearest(byte[] src, int srcW, int srcH, int dstW, int dstH)
        {
            var dst = new byte[dstW * dstH * 4];
            float scaleX = (float)srcW / dstW;
            float scaleY = (float)srcH / dstH;

            Parallel.For(0, dstH, y =>
            {
                int sy = Math.Min((int)(y * scaleY), srcH - 1);
                int srcRow = sy * srcW * 4;
                int dstRow = y * dstW * 4;
                for (int x = 0; x < dstW; x++)
                {
                    int sx = Math.Min((int)(x * scaleX), srcW - 1) * 4;
                    int di = dstRow + x * 4;
                    dst[di]     = src[srcRow + sx];
                    dst[di + 1] = src[srcRow + sx + 1];
                    dst[di + 2] = src[srcRow + sx + 2];
                    dst[di + 3] = src[srcRow + sx + 3];
                }
            });
            return dst;
        }

        private static byte[] ResizeRgbNearest(byte[] src, int srcW, int srcH, int dstW, int dstH)
        {
            var dst = new byte[dstW * dstH * 3];
            float scaleX = (float)srcW / dstW;
            float scaleY = (float)srcH / dstH;

            Parallel.For(0, dstH, y =>
            {
                int sy = Math.Min((int)(y * scaleY), srcH - 1);
                int srcRow = sy * srcW * 3;
                int dstRow = y * dstW * 3;
                for (int x = 0; x < dstW; x++)
                {
                    int sx = Math.Min((int)(x * scaleX), srcW - 1) * 3;
                    int di = dstRow + x * 3;
                    dst[di]     = src[srcRow + sx];
                    dst[di + 1] = src[srcRow + sx + 1];
                    dst[di + 2] = src[srcRow + sx + 2];
                }
            });
            return dst;
        }

        // ─── Bilinear ───────────────────────────────────────────────

        private static byte[] ResizeBilinear(byte[] src, int srcW, int srcH, int dstW, int dstH)
        {
            var dst = new byte[dstW * dstH * 4];
            float scaleX = (float)srcW / dstW;
            float scaleY = (float)srcH / dstH;

            Parallel.For(0, dstH, y =>
            {
                float fy = y * scaleY;
                int sy0 = Math.Min((int)fy, srcH - 1);
                int sy1 = Math.Min(sy0 + 1, srcH - 1);
                float wy1 = fy - sy0;
                float wy0 = 1f - wy1;

                for (int x = 0; x < dstW; x++)
                {
                    float fx = x * scaleX;
                    int sx0 = Math.Min((int)fx, srcW - 1);
                    int sx1 = Math.Min(sx0 + 1, srcW - 1);
                    float wx1 = fx - sx0;
                    float wx0 = 1f - wx1;

                    int i00 = (sy0 * srcW + sx0) * 4;
                    int i10 = (sy0 * srcW + sx1) * 4;
                    int i01 = (sy1 * srcW + sx0) * 4;
                    int i11 = (sy1 * srcW + sx1) * 4;
                    int di = (y * dstW + x) * 4;

                    dst[di]     = (byte)(wy0 * (wx0 * src[i00]     + wx1 * src[i10])     + wy1 * (wx0 * src[i01]     + wx1 * src[i11]));
                    dst[di + 1] = (byte)(wy0 * (wx0 * src[i00 + 1] + wx1 * src[i10 + 1]) + wy1 * (wx0 * src[i01 + 1] + wx1 * src[i11 + 1]));
                    dst[di + 2] = (byte)(wy0 * (wx0 * src[i00 + 2] + wx1 * src[i10 + 2]) + wy1 * (wx0 * src[i01 + 2] + wx1 * src[i11 + 2]));
                    dst[di + 3] = (byte)(wy0 * (wx0 * src[i00 + 3] + wx1 * src[i10 + 3]) + wy1 * (wx0 * src[i01 + 3] + wx1 * src[i11 + 3]));
                }
            });
            return dst;
        }

        private static byte[] ResizeRgbBilinear(byte[] src, int srcW, int srcH, int dstW, int dstH)
        {
            var dst = new byte[dstW * dstH * 3];
            float scaleX = (float)srcW / dstW;
            float scaleY = (float)srcH / dstH;

            Parallel.For(0, dstH, y =>
            {
                float fy = y * scaleY;
                int sy0 = Math.Min((int)fy, srcH - 1);
                int sy1 = Math.Min(sy0 + 1, srcH - 1);
                float wy1 = fy - sy0;
                float wy0 = 1f - wy1;

                for (int x = 0; x < dstW; x++)
                {
                    float fx = x * scaleX;
                    int sx0 = Math.Min((int)fx, srcW - 1);
                    int sx1 = Math.Min(sx0 + 1, srcW - 1);
                    float wx1 = fx - sx0;
                    float wx0 = 1f - wx1;

                    int i00 = (sy0 * srcW + sx0) * 3;
                    int i10 = (sy0 * srcW + sx1) * 3;
                    int i01 = (sy1 * srcW + sx0) * 3;
                    int i11 = (sy1 * srcW + sx1) * 3;
                    int di = (y * dstW + x) * 3;

                    dst[di]     = (byte)(wy0 * (wx0 * src[i00]     + wx1 * src[i10])     + wy1 * (wx0 * src[i01]     + wx1 * src[i11]));
                    dst[di + 1] = (byte)(wy0 * (wx0 * src[i00 + 1] + wx1 * src[i10 + 1]) + wy1 * (wx0 * src[i01 + 1] + wx1 * src[i11 + 1]));
                    dst[di + 2] = (byte)(wy0 * (wx0 * src[i00 + 2] + wx1 * src[i10 + 2]) + wy1 * (wx0 * src[i01 + 2] + wx1 * src[i11 + 2]));
                }
            });
            return dst;
        }

        // ─── Separable kernel resize (4-channel) ────────────────────
        //
        // Two-pass separable convolution:
        //   Pass 1: resample horizontally  (srcW×srcH → dstW×srcH)
        //   Pass 2: resample vertically    (dstW×srcH → dstW×dstH)
        //
        // When downsampling, the kernel is widened by the scale factor
        // (prefilter) so that every source pixel contributes to the
        // destination.  Without this, large reductions (e.g. 1000→80)
        // skip most source pixels and produce blocky/pixelated output.
        //

        private static byte[] ResizeWithKernel(
            byte[] src, int srcW, int srcH, int dstW, int dstH,
            Func<float, float> kernel, int baseRadius)
        {
            var work = new float[dstW * srcH * 4];

            float scaleX = (float)srcW / dstW;
            float filterScaleX = Math.Max(1f, scaleX);          // widen kernel when downsampling
            int radiusX = (int)Math.Ceiling(baseRadius * filterScaleX);

            // ── Horizontal pass ──────────────────────────────────
            Parallel.For(0, srcH, sy =>
            {
                int srcRow = sy * srcW * 4;
                for (int dx = 0; dx < dstW; dx++)
                {
                    float center = (dx + 0.5f) * scaleX - 0.5f;
                    float sum0 = 0f, sum1 = 0f, sum2 = 0f, sum3 = 0f, wSum = 0f;

                    for (int kx = -radiusX; kx <= radiusX; kx++)
                    {
                        int sx = Math.Clamp((int)MathF.Floor(center) + kx, 0, srcW - 1);
                        float dist = (sx - center) / filterScaleX;
                        float w = kernel(dist);
                        wSum += w;
                        int si = srcRow + sx * 4;
                        sum0 += src[si]     * w;
                        sum1 += src[si + 1] * w;
                        sum2 += src[si + 2] * w;
                        sum3 += src[si + 3] * w;
                    }

                    if (wSum > 0f)
                    {
                        float inv = 1f / wSum;
                        sum0 *= inv; sum1 *= inv; sum2 *= inv; sum3 *= inv;
                    }

                    int ti = (sy * dstW + dx) * 4;
                    work[ti]     = sum0;
                    work[ti + 1] = sum1;
                    work[ti + 2] = sum2;
                    work[ti + 3] = sum3;
                }
            });

            // ── Vertical pass ────────────────────────────────────
            var dst = new byte[dstW * dstH * 4];
            float scaleY = (float)srcH / dstH;
            float filterScaleY = Math.Max(1f, scaleY);
            int radiusY = (int)Math.Ceiling(baseRadius * filterScaleY);

            Parallel.For(0, dstH, dy =>
            {
                float center = (dy + 0.5f) * scaleY - 0.5f;
                for (int dx = 0; dx < dstW; dx++)
                {
                    float sum0 = 0f, sum1 = 0f, sum2 = 0f, sum3 = 0f, wSum = 0f;

                    for (int ky = -radiusY; ky <= radiusY; ky++)
                    {
                        int sy = Math.Clamp((int)MathF.Floor(center) + ky, 0, srcH - 1);
                        float dist = (sy - center) / filterScaleY;
                        float w = kernel(dist);
                        wSum += w;
                        int ti = (sy * dstW + dx) * 4;
                        sum0 += work[ti]     * w;
                        sum1 += work[ti + 1] * w;
                        sum2 += work[ti + 2] * w;
                        sum3 += work[ti + 3] * w;
                    }

                    if (wSum > 0f)
                    {
                        float inv = 1f / wSum;
                        sum0 *= inv; sum1 *= inv; sum2 *= inv; sum3 *= inv;
                    }

                    int di = (dy * dstW + dx) * 4;
                    dst[di]     = (byte)Math.Clamp(sum0, 0f, 255f);
                    dst[di + 1] = (byte)Math.Clamp(sum1, 0f, 255f);
                    dst[di + 2] = (byte)Math.Clamp(sum2, 0f, 255f);
                    dst[di + 3] = (byte)Math.Clamp(sum3, 0f, 255f);
                }
            });

            return dst;
        }

        // ─── Separable kernel resize (3-channel) ────────────────────

        private static byte[] ResizeRgbWithKernel(
            byte[] src, int srcW, int srcH, int dstW, int dstH,
            Func<float, float> kernel, int baseRadius)
        {
            var work = new float[dstW * srcH * 3];

            float scaleX = (float)srcW / dstW;
            float filterScaleX = Math.Max(1f, scaleX);
            int radiusX = (int)Math.Ceiling(baseRadius * filterScaleX);

            // ── Horizontal pass ──────────────────────────────────
            Parallel.For(0, srcH, sy =>
            {
                int srcRow = sy * srcW * 3;
                for (int dx = 0; dx < dstW; dx++)
                {
                    float center = (dx + 0.5f) * scaleX - 0.5f;
                    float sum0 = 0f, sum1 = 0f, sum2 = 0f, wSum = 0f;

                    for (int kx = -radiusX; kx <= radiusX; kx++)
                    {
                        int sx = Math.Clamp((int)MathF.Floor(center) + kx, 0, srcW - 1);
                        float dist = (sx - center) / filterScaleX;
                        float w = kernel(dist);
                        wSum += w;
                        int si = srcRow + sx * 3;
                        sum0 += src[si]     * w;
                        sum1 += src[si + 1] * w;
                        sum2 += src[si + 2] * w;
                    }

                    if (wSum > 0f)
                    {
                        float inv = 1f / wSum;
                        sum0 *= inv; sum1 *= inv; sum2 *= inv;
                    }

                    int ti = (sy * dstW + dx) * 3;
                    work[ti]     = sum0;
                    work[ti + 1] = sum1;
                    work[ti + 2] = sum2;
                }
            });

            // ── Vertical pass ────────────────────────────────────
            var dst = new byte[dstW * dstH * 3];
            float scaleY = (float)srcH / dstH;
            float filterScaleY = Math.Max(1f, scaleY);
            int radiusY = (int)Math.Ceiling(baseRadius * filterScaleY);

            Parallel.For(0, dstH, dy =>
            {
                float center = (dy + 0.5f) * scaleY - 0.5f;
                for (int dx = 0; dx < dstW; dx++)
                {
                    float sum0 = 0f, sum1 = 0f, sum2 = 0f, wSum = 0f;

                    for (int ky = -radiusY; ky <= radiusY; ky++)
                    {
                        int sy = Math.Clamp((int)MathF.Floor(center) + ky, 0, srcH - 1);
                        float dist = (sy - center) / filterScaleY;
                        float w = kernel(dist);
                        wSum += w;
                        int ti = (sy * dstW + dx) * 3;
                        sum0 += work[ti]     * w;
                        sum1 += work[ti + 1] * w;
                        sum2 += work[ti + 2] * w;
                    }

                    if (wSum > 0f)
                    {
                        float inv = 1f / wSum;
                        sum0 *= inv; sum1 *= inv; sum2 *= inv;
                    }

                    int di = (dy * dstW + dx) * 3;
                    dst[di]     = (byte)Math.Clamp(sum0, 0f, 255f);
                    dst[di + 1] = (byte)Math.Clamp(sum1, 0f, 255f);
                    dst[di + 2] = (byte)Math.Clamp(sum2, 0f, 255f);
                }
            });

            return dst;
        }
    }
}
