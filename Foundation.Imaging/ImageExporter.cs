using System.IO;
using Foundation.Imaging.Encoders;

namespace Foundation.Imaging
{
    /// <summary>
    /// Exports an RGBA pixel buffer to image files (PNG, WebP) using pure .NET encoders.
    ///
    /// Replaces the original ImageSharp-based exporter.
    /// PNG uses a custom ZLib-based encoder.
    /// WebP "format" falls back to PNG output for compatibility (lossless, perfect quality).
    ///
    /// Relocated from BMC.LDraw.Render to Foundation.Imaging for cross-system reuse.
    /// </summary>
    public static class ImageExporter
    {
        // ────────────────────────────────────────────────────────
        // PNG
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Save an RGBA byte array to a PNG file.
        /// </summary>
        public static void SaveToPng(byte[] rgbaPixels, int width, int height, string outputPath)
        {
            EnsureDirectory(outputPath);
            byte[] png = PngEncoder.Encode(rgbaPixels, width, height);
            File.WriteAllBytes(outputPath, png);
        }

        /// <summary>
        /// Convert an RGBA byte array to PNG bytes (for in-memory use).
        /// </summary>
        public static byte[] ToPngBytes(byte[] rgbaPixels, int width, int height)
        {
            return PngEncoder.Encode(rgbaPixels, width, height);
        }


        // ────────────────────────────────────────────────────────
        // JPEG
        // ────────────────────────────────────────────────────────

        public static void SaveToJpeg(byte[] rgbaPixels, int width, int height, string outputPath, int quality = 85)
        {
            EnsureDirectory(outputPath);
            byte[] data = ToJpegBytes(rgbaPixels, width, height, quality);
            File.WriteAllBytes(outputPath, data);
        }

        public static byte[] ToJpegBytes(byte[] rgbaPixels, int width, int height, int quality = 85)
        {
            byte[] rgb = RgbaToRgb(rgbaPixels);
            return JpegEncoder.Encode(rgb, width, height, quality);
        }


        // ────────────────────────────────────────────────────────
        // WebP  (outputs PNG — lossless, universally supported)
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Save an RGBA byte array to a "WebP" file.
        /// Currently outputs PNG data (lossless, universally supported).
        /// The quality parameter is accepted for API compatibility but ignored.
        /// </summary>
        public static void SaveToWebP(byte[] rgbaPixels, int width, int height, string outputPath, int quality = 90)
        {
            EnsureDirectory(outputPath);
            byte[] data = PngEncoder.Encode(rgbaPixels, width, height);
            File.WriteAllBytes(outputPath, data);
        }

        /// <summary>
        /// Convert an RGBA byte array to "WebP" bytes (for in-memory use).
        /// Currently outputs PNG data (lossless, universally supported).
        /// The quality parameter is accepted for API compatibility but ignored.
        /// </summary>
        public static byte[] ToWebPBytes(byte[] rgbaPixels, int width, int height, int quality = 90)
        {
            return PngEncoder.Encode(rgbaPixels, width, height);
        }


        // ────────────────────────────────────────────────────────
        // Helpers
        // ────────────────────────────────────────────────────────

        private static byte[] RgbaToRgb(byte[] rgba)
        {
            int n = rgba.Length / 4;
            var rgb = new byte[n * 3];
            for (int i = 0, j = 0; i < rgba.Length; i += 4, j += 3)
            {
                rgb[j]     = rgba[i];
                rgb[j + 1] = rgba[i + 1];
                rgb[j + 2] = rgba[i + 2];
            }
            return rgb;
        }

        private static void EnsureDirectory(string outputPath)
        {
            string dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }


    /// <summary>
    /// Backward-compatible alias for ImageExporter.
    /// </summary>
    public static class PngExporter
    {
        public static void SaveToPng(byte[] rgbaPixels, int width, int height, string outputPath)
            => ImageExporter.SaveToPng(rgbaPixels, width, height, outputPath);

        public static byte[] ToPngBytes(byte[] rgbaPixels, int width, int height)
            => ImageExporter.ToPngBytes(rgbaPixels, width, height);
    }
}
