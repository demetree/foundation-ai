using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Exports an RGBA pixel buffer to image files (PNG, WebP) using ImageSharp.
    ///
    /// Replaces the original PngExporter — all PNG methods preserved for backward compat.
    ///
    /// AI-generated — Phase 3.1 (WebP) added Feb 2026.
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
            using (Image<Rgba32> image = Image.LoadPixelData<Rgba32>(rgbaPixels, width, height))
            {
                EnsureDirectory(outputPath);
                image.SaveAsPng(outputPath);
            }
        }

        /// <summary>
        /// Convert an RGBA byte array to PNG bytes (for in-memory use).
        /// </summary>
        public static byte[] ToPngBytes(byte[] rgbaPixels, int width, int height)
        {
            using (Image<Rgba32> image = Image.LoadPixelData<Rgba32>(rgbaPixels, width, height))
            using (MemoryStream ms = new MemoryStream())
            {
                image.SaveAsPng(ms);
                return ms.ToArray();
            }
        }

        // ────────────────────────────────────────────────────────
        // WebP
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Save an RGBA byte array to a WebP file.
        /// </summary>
        /// <param name="quality">WebP quality (1–100).  Higher = better quality, larger file.</param>
        public static void SaveToWebP(byte[] rgbaPixels, int width, int height, string outputPath, int quality = 90)
        {
            using (Image<Rgba32> image = Image.LoadPixelData<Rgba32>(rgbaPixels, width, height))
            {
                EnsureDirectory(outputPath);
                WebpEncoder encoder = new WebpEncoder
                {
                    Quality = quality,
                    FileFormat = WebpFileFormatType.Lossy
                };
                image.SaveAsWebp(outputPath, encoder);
            }
        }

        /// <summary>
        /// Convert an RGBA byte array to WebP bytes (for in-memory use).
        /// </summary>
        /// <param name="quality">WebP quality (1–100).  Higher = better quality, larger file.</param>
        public static byte[] ToWebPBytes(byte[] rgbaPixels, int width, int height, int quality = 90)
        {
            using (Image<Rgba32> image = Image.LoadPixelData<Rgba32>(rgbaPixels, width, height))
            using (MemoryStream ms = new MemoryStream())
            {
                WebpEncoder encoder = new WebpEncoder
                {
                    Quality = quality,
                    FileFormat = WebpFileFormatType.Lossy
                };
                image.SaveAsWebp(ms, encoder);
                return ms.ToArray();
            }
        }


        // ────────────────────────────────────────────────────────
        // Helpers
        // ────────────────────────────────────────────────────────

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
