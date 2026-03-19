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
        private static readonly float[] CatmullRomWeights = PrecomputeWeights(CatmullRomKernel, 2);
        private static readonly float[] MitchellNetravaliWeights = PrecomputeWeights(MitchellNetravaliKernel, 2);
        private static readonly float[] Lanczos3Weights = PrecomputeWeights(Lanczos3Kernel, 3);

        public static float CatmullRomKernel(float x)
        {
            if (x < 0f) x = -x;
            if (x < 1f)
                return 1f - 2.5f * x * x + 1.5f * x * x * x;
            if (x < 2f)
                return 2f - 4f * x + 2.5f * x * x - 0.5f * x * x * x;
            return 0f;
        }

        public static float MitchellNetravaliKernel(float x)
        {
            if (x < 0f) x = -x;
            float B = 1f / 3f, C = 1f / 3f;
            if (x < 1f)
                return (6f - 2f * B + (-18f + 6f * B + 30f * C) * x * x +
                        (12f - 9f * B - 6f * C) * x * x * x) / 6f;
            if (x < 2f)
                return ((8f * B + 24f * C) + x * (-12f * B - 48f * C) +
                        x * (10.5f * B + 18f * C) + x * (-1.5f * B - 3f * C)) / 6f;
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

        private static float[] PrecomputeWeights(Func<float, float> kernel, int radius)
        {
            int count = radius * 4 + 1;
            var weights = new float[count];
            for (int i = 0; i < count; i++)
            {
                weights[i] = kernel(i / 2f);
            }
            NormalizeWeights(weights);
            return weights;
        }

        private static void NormalizeWeights(float[] weights)
        {
            float sum = 0f;
            for (int i = 0; i < weights.Length; i++) sum += weights[i];
            if (sum > 0f)
                for (int i = 0; i < weights.Length; i++) weights[i] /= sum;
        }

        public static byte[] ResizeBgra(
            byte[] src, int srcW, int srcH, int dstW, int dstH,
            ResampleFilter filter = ResampleFilter.Lanczos3)
        {
            if (srcW == dstW && srcH == dstH) return src;

            float[] weights;
            int radius;
            switch (filter)
            {
                case ResampleFilter.Nearest:
                    return ResizeNearest(src, srcW, srcH, dstW, dstH);
                case ResampleFilter.Bilinear:
                    return ResizeBilinear(src, srcW, srcH, dstW, dstH);
                case ResampleFilter.CatmullRom:
                    weights = CatmullRomWeights; radius = 2; break;
                case ResampleFilter.MitchellNetravali:
                    weights = MitchellNetravaliWeights; radius = 2; break;
                case ResampleFilter.Lanczos3:
                default:
                    weights = Lanczos3Weights; radius = 3; break;
            }

            return ResizeWithKernel(src, srcW, srcH, dstW, dstH, weights, radius);
        }

        private static byte[] ResizeNearest(byte[] src, int srcW, int srcH, int dstW, int dstH)
        {
            var dst = new byte[dstW * dstH * 4];
            float scaleX = (float)srcW / dstW;
            float scaleY = (float)srcH / dstH;

            Parallel.For(0, dstH, y =>
            {
                int srcRow = (int)(y * scaleY) * srcW * 4;
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

        private static byte[] ResizeWithKernel(
            byte[] src, int srcW, int srcH, int dstW, int dstH,
            float[] weights, int radius)
        {
            var work = ArrayPool<float>.Shared.Rent(dstW * srcH * 4);

            try
            {
                float scaleX = (float)srcW / dstW;

                Parallel.For(0, srcH, sy =>
                {
                    int srcRow = sy * srcW * 4;
                    for (int dx = 0; dx < dstW; dx++)
                    {
                        float fx = dx * scaleX;
                        float wxf = fx - (int)fx;
                        int wi = (int)(wxf * 2);
                        float sumR = 0f, sumG = 0f, sumB = 0f, sumA = 0f;

                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int sx = Math.Clamp((int)fx + kx, 0, srcW - 1);
                            float w = weights[wi + radius - kx];
                            int si = srcRow + sx * 4;
                            sumB += src[si]     * w;
                            sumG += src[si + 1] * w;
                            sumR += src[si + 2] * w;
                            sumA += src[si + 3] * w;
                        }

                        int ti = (sy * dstW + dx) * 4;
                        work[dx * 4]     = sumB;
                        work[dx * 4 + 1] = sumG;
                        work[dx * 4 + 2] = sumR;
                        work[dx * 4 + 3] = sumA;
                    }
                });

                var dst = new byte[dstW * dstH * 4];
                float scaleY = (float)srcH / dstH;

                Parallel.For(0, dstH, dy =>
                {
                    float fy = dy * scaleY;
                    float wyf = fy - (int)fy;
                    int wi = (int)(wyf * 2);
                    for (int dx = 0; dx < dstW; dx++)
                    {
                        float sumR = 0f, sumG = 0f, sumB = 0f, sumA = 0f;

                        for (int ky = -radius; ky <= radius; ky++)
                        {
                            int sy = Math.Clamp((int)fy + ky, 0, srcH - 1);
                            float w = weights[wi + radius - ky];
                            int ti = (sy * dstW + dx) * 4;
                            sumB += work[ti]     * w;
                            sumG += work[ti + 1] * w;
                            sumR += work[ti + 2] * w;
                            sumA += work[ti + 3] * w;
                        }

                        int di = (dy * dstW + dx) * 4;
                        dst[di]     = (byte)Math.Clamp(sumB, 0f, 255f);
                        dst[di + 1] = (byte)Math.Clamp(sumG, 0f, 255f);
                        dst[di + 2] = (byte)Math.Clamp(sumR, 0f, 255f);
                        dst[di + 3] = (byte)Math.Clamp(sumA, 0f, 255f);
                    }
                });

                return dst;
            }
            finally
            {
                ArrayPool<float>.Shared.Return(work);
            }
        }

        public static byte[] ResizeRgb(
            byte[] src, int srcW, int srcH, int dstW, int dstH,
            ResampleFilter filter = ResampleFilter.Lanczos3)
        {
            if (srcW == dstW && srcH == dstH) return src;

            float[] weights;
            int radius;
            switch (filter)
            {
                case ResampleFilter.Nearest:
                    return ResizeRgbNearest(src, srcW, srcH, dstW, dstH);
                case ResampleFilter.Bilinear:
                    return ResizeRgbBilinear(src, srcW, srcH, dstW, dstH);
                case ResampleFilter.CatmullRom:
                    weights = CatmullRomWeights; radius = 2; break;
                case ResampleFilter.MitchellNetravali:
                    weights = MitchellNetravaliWeights; radius = 2; break;
                case ResampleFilter.Lanczos3:
                default:
                    weights = Lanczos3Weights; radius = 3; break;
            }

            return ResizeRgbWithKernel(src, srcW, srcH, dstW, dstH, weights, radius);
        }

        private static byte[] ResizeRgbNearest(byte[] src, int srcW, int srcH, int dstW, int dstH)
        {
            var dst = new byte[dstW * dstH * 3];
            float scaleX = (float)srcW / dstW;
            float scaleY = (float)srcH / dstH;

            Parallel.For(0, dstH, y =>
            {
                int srcRow = (int)(y * scaleY) * srcW * 3;
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

        private static byte[] ResizeRgbWithKernel(
            byte[] src, int srcW, int srcH, int dstW, int dstH,
            float[] weights, int radius)
        {
            var work = ArrayPool<float>.Shared.Rent(dstW * srcH * 3);

            try
            {
                float scaleX = (float)srcW / dstW;

                Parallel.For(0, srcH, sy =>
                {
                    int srcRow = sy * srcW * 3;
                    for (int dx = 0; dx < dstW; dx++)
                    {
                        float fx = dx * scaleX;
                        float wxf = fx - (int)fx;
                        int wi = (int)(wxf * 2);
                        float sumR = 0f, sumG = 0f, sumB = 0f;

                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int sx = Math.Clamp((int)fx + kx, 0, srcW - 1);
                            float w = weights[wi + radius - kx];
                            int si = srcRow + sx * 3;
                            sumB += src[si]     * w;
                            sumG += src[si + 1] * w;
                            sumR += src[si + 2] * w;
                        }

                        int ti = (sy * dstW + dx) * 3;
                        work[ti]     = sumB;
                        work[ti + 1] = sumG;
                        work[ti + 2] = sumR;
                    }
                });

                var dst = new byte[dstW * dstH * 3];
                float scaleY = (float)srcH / dstH;

                Parallel.For(0, dstH, dy =>
                {
                    float fy = dy * scaleY;
                    float wyf = fy - (int)fy;
                    int wi = (int)(wyf * 2);
                    for (int dx = 0; dx < dstW; dx++)
                    {
                        float sumR = 0f, sumG = 0f, sumB = 0f;

                        for (int ky = -radius; ky <= radius; ky++)
                        {
                            int sy = Math.Clamp((int)fy + ky, 0, srcH - 1);
                            float w = weights[wi + radius - ky];
                            int ti = (sy * dstW + dx) * 3;
                            sumB += work[ti]     * w;
                            sumG += work[ti + 1] * w;
                            sumR += work[ti + 2] * w;
                        }

                        int di = (dy * dstW + dx) * 3;
                        dst[di]     = (byte)Math.Clamp(sumB, 0f, 255f);
                        dst[di + 1] = (byte)Math.Clamp(sumG, 0f, 255f);
                        dst[di + 2] = (byte)Math.Clamp(sumR, 0f, 255f);
                    }
                });

                return dst;
            }
            finally
            {
                ArrayPool<float>.Shared.Return(work);
            }
        }
    }
}
