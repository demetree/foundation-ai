using System;
using System.Buffers;

namespace Foundation.Imaging.Processing
{
    public enum FlipMode
    {
        Horizontal,
        Vertical,
        Both,
    }

    public static class ImageTransforms
    {
        public static byte[] CropBgra(byte[] src, int srcW, int srcH,
            int x, int y, int w, int h)
        {
            x = Math.Clamp(x, 0, srcW - 1);
            y = Math.Clamp(y, 0, srcH - 1);
            w = Math.Clamp(w, 1, srcW - x);
            h = Math.Clamp(h, 1, srcH - y);

            var dst = new byte[w * h * 4];
            for (int dy = 0; dy < h; dy++)
            {
                int srcRow = (y + dy) * srcW * 4;
                int dstRow = dy * w * 4;
                Buffer.BlockCopy(src, srcRow + x * 4, dst, dstRow, w * 4);
            }
            return dst;
        }

        public static byte[] CropRgb(byte[] src, int srcW, int srcH,
            int x, int y, int w, int h)
        {
            x = Math.Clamp(x, 0, srcW - 1);
            y = Math.Clamp(y, 0, srcH - 1);
            w = Math.Clamp(w, 1, srcW - x);
            h = Math.Clamp(h, 1, srcH - y);

            var dst = new byte[w * h * 3];
            for (int dy = 0; dy < h; dy++)
            {
                int srcRow = (y + dy) * srcW * 3;
                int dstRow = dy * w * 3;
                Buffer.BlockCopy(src, srcRow + x * 3, dst, dstRow, w * 3);
            }
            return dst;
        }

        public static byte[] FlipBgra(byte[] src, int w, int h, FlipMode mode)
        {
            var dst = new byte[w * h * 4];
            int stride = w * 4;

            if (mode == FlipMode.Horizontal)
            {
                for (int y = 0; y < h; y++)
                {
                    int row = y * stride;
                    for (int x = 0; x < w; x++)
                    {
                        int di = row + x * 4;
                        int si = row + (w - 1 - x) * 4;
                        dst[di]     = src[si];
                        dst[di + 1] = src[si + 1];
                        dst[di + 2] = src[si + 2];
                        dst[di + 3] = src[si + 3];
                    }
                }
            }
            else if (mode == FlipMode.Vertical)
            {
                for (int y = 0; y < h; y++)
                {
                    int di = y * stride;
                    int si = (h - 1 - y) * stride;
                    Buffer.BlockCopy(src, si, dst, di, stride);
                }
            }
            else
            {
                for (int i = 0; i < dst.Length; i++)
                    dst[i] = src[dst.Length - 1 - i];
            }
            return dst;
        }

        public static byte[] FlipRgb(byte[] src, int w, int h, FlipMode mode)
        {
            var dst = new byte[w * h * 3];
            int stride = w * 3;

            if (mode == FlipMode.Horizontal)
            {
                for (int y = 0; y < h; y++)
                {
                    int row = y * stride;
                    for (int x = 0; x < w; x++)
                    {
                        int di = row + x * 3;
                        int si = row + (w - 1 - x) * 3;
                        dst[di]     = src[si];
                        dst[di + 1] = src[si + 1];
                        dst[di + 2] = src[si + 2];
                    }
                }
            }
            else if (mode == FlipMode.Vertical)
            {
                for (int y = 0; y < h; y++)
                {
                    int di = y * stride;
                    int si = (h - 1 - y) * stride;
                    Buffer.BlockCopy(src, si, dst, di, stride);
                }
            }
            else
            {
                for (int i = 0; i < dst.Length; i++)
                    dst[i] = src[dst.Length - 1 - i];
            }
            return dst;
        }

        public static byte[] Rotate90Bgra(byte[] src, int w, int h)
        {
            var dst = new byte[w * h * 4];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int si = (y * w + x) * 4;
                    int di = ((w - 1 - x) * h + y) * 4;
                    dst[di]     = src[si];
                    dst[di + 1] = src[si + 1];
                    dst[di + 2] = src[si + 2];
                    dst[di + 3] = src[si + 3];
                }
            return dst;
        }

        public static byte[] Rotate180Bgra(byte[] src, int w, int h)
        {
            var dst = new byte[w * h * 4];
            int n = w * h;
            for (int i = 0; i < n; i++)
            {
                int si = i * 4;
                int di = (n - 1 - i) * 4;
                dst[di]     = src[si];
                dst[di + 1] = src[si + 1];
                dst[di + 2] = src[si + 2];
                dst[di + 3] = src[si + 3];
            }
            return dst;
        }

        public static byte[] Rotate270Bgra(byte[] src, int w, int h)
        {
            var dst = new byte[w * h * 4];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int si = (y * w + x) * 4;
                    int di = (x * h + (h - 1 - y)) * 4;
                    dst[di]     = src[si];
                    dst[di + 1] = src[si + 1];
                    dst[di + 2] = src[si + 2];
                    dst[di + 3] = src[si + 3];
                }
            return dst;
        }

        public static byte[] RotateArbitraryBgra(byte[] src, int srcW, int srcH, float angleDeg,
            out int dstW, out int dstH, byte backgroundR = 255, byte backgroundG = 255, byte backgroundB = 255, byte backgroundA = 255)
        {
            double a = angleDeg * Math.PI / 180.0;
            double cos = Math.Cos(a);
            double sin = Math.Sin(a);

            double cx = srcW / 2.0, cy = srcH / 2.0;
            double dx0 = -cx, dy0 = -cy;
            double dx1 =  srcW - cx, dy1 = -cy;
            double dx2 =  srcW - cx, dy2 =  srcH - cy;
            double dx3 = -cx, dy3 =  srcH - cy;

            double rx0 = dx0 * cos - dy0 * sin + cx;
            double ry0 = dx0 * sin + dy0 * cos + cy;
            double rx1 = dx1 * cos - dy1 * sin + cx;
            double ry1 = dx1 * sin + dy1 * cos + cy;
            double rx2 = dx2 * cos - dy2 * sin + cx;
            double ry2 = dx2 * sin + dy2 * cos + cy;
            double rx3 = dx3 * cos - dy3 * sin + cx;
            double ry3 = dx3 * sin + dy3 * cos + cy;

            double minX = Math.Min(Math.Min(rx0, rx1), Math.Min(rx2, rx3));
            double maxX = Math.Max(Math.Max(rx0, rx1), Math.Max(rx2, rx3));
            double minY = Math.Min(Math.Min(ry0, ry1), Math.Min(ry2, ry3));
            double maxY = Math.Max(Math.Max(ry0, ry1), Math.Max(ry2, ry3));

            dstW = Math.Max(1, (int)Math.Ceiling(maxX - minX));
            dstH = Math.Max(1, (int)Math.Ceiling(maxY - minY));
            var dst = new byte[dstW * dstH * 4];

            double tx = (minX + maxX) / 2.0;
            double ty = (minY + maxY) / 2.0;

            byte bgR = backgroundR, bgG = backgroundG, bgB = backgroundB, bgA = backgroundA;

            for (int dy = 0; dy < dstH; dy++)
            {
                for (int dx = 0; dx < dstW; dx++)
                {
                    double sx = (dx - tx) * cos + (dy - ty) * sin + cx;
                    double sy = -(dx - tx) * sin + (dy - ty) * cos + cy;
                    int di = (dy * dstW + dx) * 4;

                    if (sx < 0 || sx >= srcW - 1 || sy < 0 || sy >= srcH - 1)
                    {
                        dst[di]     = bgB;
                        dst[di + 1] = bgG;
                        dst[di + 2] = bgR;
                        dst[di + 3] = bgA;
                        continue;
                    }

                    int x0 = (int)sx, y0 = (int)sy;
                    float fx = (float)(sx - x0), fy = (float)(sy - y0);
                    float wx1 = fx, wx0 = 1f - fx;
                    float wy1 = fy, wy0 = 1f - fy;

                    int i00 = (y0 * srcW + x0) * 4;
                    int i10 = (y0 * srcW + x0 + 1) * 4;
                    int i01 = ((y0 + 1) * srcW + x0) * 4;
                    int i11 = ((y0 + 1) * srcW + x0 + 1) * 4;

                    float w00 = wx0 * wy0, w10 = wx1 * wy0;
                    float w01 = wx0 * wy1, w11 = wx1 * wy1;

                    dst[di]     = (byte)Math.Clamp(w00 * src[i00]     + w10 * src[i10]     + w01 * src[i01]     + w11 * src[i11],     0, 255);
                    dst[di + 1] = (byte)Math.Clamp(w00 * src[i00 + 1] + w10 * src[i10 + 1] + w01 * src[i01 + 1] + w11 * src[i11 + 1], 0, 255);
                    dst[di + 2] = (byte)Math.Clamp(w00 * src[i00 + 2] + w10 * src[i10 + 2] + w01 * src[i01 + 2] + w11 * src[i11 + 2], 0, 255);
                    dst[di + 3] = (byte)Math.Clamp(w00 * src[i00 + 3] + w10 * src[i10 + 3] + w01 * src[i01 + 3] + w11 * src[i11 + 3], 0, 255);
                }
            }
            return dst;
        }

        public static byte[] RotateArbitraryRgb(byte[] src, int srcW, int srcH, float angleDeg,
            out int dstW, out int dstH, byte backgroundR = 255, byte backgroundG = 255, byte backgroundB = 255)
        {
            double a = angleDeg * Math.PI / 180.0;
            double cos = Math.Cos(a);
            double sin = Math.Sin(a);

            double cx = srcW / 2.0, cy = srcH / 2.0;
            double dx0 = -cx, dy0 = -cy;
            double dx1 =  srcW - cx, dy1 = -cy;
            double dx2 =  srcW - cx, dy2 =  srcH - cy;
            double dx3 = -cx, dy3 =  srcH - cy;

            double rx0 = dx0 * cos - dy0 * sin + cx;
            double ry0 = dx0 * sin + dy0 * cos + cy;
            double rx1 = dx1 * cos - dy1 * sin + cx;
            double ry1 = dx1 * sin + dy1 * cos + cy;
            double rx2 = dx2 * cos - dy2 * sin + cx;
            double ry2 = dx2 * sin + dy2 * cos + cy;
            double rx3 = dx3 * cos - dy3 * sin + cx;
            double ry3 = dx3 * sin + dy3 * cos + cy;

            double minX = Math.Min(Math.Min(rx0, rx1), Math.Min(rx2, rx3));
            double maxX = Math.Max(Math.Max(rx0, rx1), Math.Max(rx2, rx3));
            double minY = Math.Min(Math.Min(ry0, ry1), Math.Min(ry2, ry3));
            double maxY = Math.Max(Math.Max(ry0, ry1), Math.Max(ry2, ry3));

            dstW = Math.Max(1, (int)Math.Ceiling(maxX - minX));
            dstH = Math.Max(1, (int)Math.Ceiling(maxY - minY));
            var dst = new byte[dstW * dstH * 3];

            double tx = (minX + maxX) / 2.0;
            double ty = (minY + maxY) / 2.0;

            byte bgR = backgroundR, bgG = backgroundG, bgB = backgroundB;

            for (int dy = 0; dy < dstH; dy++)
            {
                for (int dx = 0; dx < dstW; dx++)
                {
                    double sx = (dx - tx) * cos + (dy - ty) * sin + cx;
                    double sy = -(dx - tx) * sin + (dy - ty) * cos + cy;
                    int di = (dy * dstW + dx) * 3;

                    if (sx < 0 || sx >= srcW - 1 || sy < 0 || sy >= srcH - 1)
                    {
                        dst[di]     = bgB;
                        dst[di + 1] = bgG;
                        dst[di + 2] = bgR;
                        continue;
                    }

                    int x0 = (int)sx, y0 = (int)sy;
                    float fx = (float)(sx - x0), fy = (float)(sy - y0);
                    float wx1 = fx, wx0 = 1f - fx;
                    float wy1 = fy, wy0 = 1f - fy;

                    int i00 = (y0 * srcW + x0) * 3;
                    int i10 = (y0 * srcW + x0 + 1) * 3;
                    int i01 = ((y0 + 1) * srcW + x0) * 3;
                    int i11 = ((y0 + 1) * srcW + x0 + 1) * 3;

                    float w00 = wx0 * wy0, w10 = wx1 * wy0;
                    float w01 = wx0 * wy1, w11 = wx1 * wy1;

                    dst[di]     = (byte)Math.Clamp(w00 * src[i00]     + w10 * src[i10]     + w01 * src[i01]     + w11 * src[i11],     0, 255);
                    dst[di + 1] = (byte)Math.Clamp(w00 * src[i00 + 1] + w10 * src[i10 + 1] + w01 * src[i01 + 1] + w11 * src[i11 + 1], 0, 255);
                    dst[di + 2] = (byte)Math.Clamp(w00 * src[i00 + 2] + w10 * src[i10 + 2] + w01 * src[i01 + 2] + w11 * src[i11 + 2], 0, 255);
                }
            }
            return dst;
        }

        public static (int w, int h) RotateSize(int w, int h, float angleDeg)
        {
            double a = angleDeg * Math.PI / 180.0;
            double cos = Math.Abs(Math.Cos(a));
            double sin = Math.Abs(Math.Sin(a));
            int rw = (int)Math.Ceiling(h * sin + w * cos);
            int rh = (int)Math.Ceiling(w * sin + h * cos);
            return (Math.Max(1, rw), Math.Max(1, rh));
        }
    }
}
