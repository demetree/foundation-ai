//
// ThumbnailGenerator.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Generates thumbnail images from PNG source files using the Foundation.Imaging
// pipeline: PngDecoder → PostProcess.Downsample → PngEncoder.
//
// Supports PNG input only (colour types 2 and 6).  JPEG/WebP/BMP support can
// be added later by introducing additional decoders.
//
using System;
using Foundation.Imaging.Decoders;
using Foundation.Imaging.Encoders;
using Foundation.Imaging.Processing;

namespace Foundation.Imaging
{
    /// <summary>
    /// Generates thumbnail PNG images from source image bytes.
    /// </summary>
    public static class ThumbnailGenerator
    {
        /// <summary>
        /// Generate a thumbnail PNG from source image bytes.
        /// </summary>
        /// <param name="imageBytes">Source image bytes (PNG format).</param>
        /// <param name="mimeType">MIME type of the source image.</param>
        /// <param name="maxDimension">Maximum width or height of the thumbnail (default 80).</param>
        /// <returns>PNG thumbnail bytes, or null if the format is not supported.</returns>
        public static byte[] GenerateThumbnail(byte[] imageBytes, string mimeType, int maxDimension = 80,
            ResampleFilter filter = ResampleFilter.Lanczos3)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return null;

            if (!IsSupportedFormat(mimeType))
                return null;

            try
            {
                var (rgbaPixels, width, height) = PngDecoder.Decode(imageBytes);

                if (width <= 0 || height <= 0)
                    return null;

                int thumbW, thumbH;
                if (width >= height)
                {
                    thumbW = Math.Min(maxDimension, width);
                    thumbH = (int)Math.Round((double)height / width * thumbW);
                }
                else
                {
                    thumbH = Math.Min(maxDimension, height);
                    thumbW = (int)Math.Round((double)width / height * thumbH);
                }

                thumbW = Math.Max(1, thumbW);
                thumbH = Math.Max(1, thumbH);

                if (thumbW == width && thumbH == height)
                    return PngEncoder.Encode(rgbaPixels, width, height);

                byte[] thumbPixels = Resampler.ResizeBgra(rgbaPixels, width, height, thumbW, thumbH, filter);
                return PngEncoder.Encode(thumbPixels, thumbW, thumbH);
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Returns true if the given MIME type can be thumbnailed.
        /// </summary>
        public static bool IsSupportedFormat(string mimeType)
        {
            if (string.IsNullOrEmpty(mimeType))
                return false;

            return mimeType.Equals("image/png", StringComparison.OrdinalIgnoreCase);
            // Future: add "image/bmp", "image/jpeg" when decoders are available
        }
    }
}
