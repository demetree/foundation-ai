using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Exports an RGBA pixel buffer to PNG files using ImageSharp.
    /// </summary>
    public static class PngExporter
    {
        /// <summary>
        /// Save an RGBA byte array to a PNG file.
        /// </summary>
        public static void SaveToPng(byte[] rgbaPixels, int width, int height, string outputPath)
        {
            using (Image<Rgba32> image = Image.LoadPixelData<Rgba32>(rgbaPixels, width, height))
            {
                string dir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
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
    }
}
