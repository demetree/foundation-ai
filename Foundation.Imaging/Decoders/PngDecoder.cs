//
// PngDecoder.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Pure .NET PNG decoder — reads a PNG file and returns raw RGBA pixel data.
// Inverse of PngEncoder.  Uses .NET's built-in ZLibStream for DEFLATE
// decompression.  Supports 8-bit RGBA (colour type 6) and 8-bit RGB
// (colour type 2) with automatic alpha expansion.
//
// Part of Foundation.Imaging for cross-system reuse.
//
using System;
using System.IO;
using System.IO.Compression;

namespace Foundation.Imaging.Decoders
{
    /// <summary>
    /// Decodes a PNG file to raw RGBA pixel data.
    /// </summary>
    public static class PngDecoder
    {
        private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };


        /// <summary>
        /// Decode PNG bytes to an RGBA pixel buffer.
        /// </summary>
        /// <param name="pngBytes">Raw PNG file bytes.</param>
        /// <returns>Tuple of (rgbaPixels, width, height).</returns>
        public static (byte[] RgbaPixels, int Width, int Height) Decode(byte[] pngBytes)
        {
            if (pngBytes == null || pngBytes.Length < 8)
                throw new ArgumentException("Invalid PNG data.");

            // Verify signature
            for (int i = 0; i < 8; i++)
            {
                if (pngBytes[i] != PngSignature[i])
                    throw new ArgumentException("Not a valid PNG file (bad signature).");
            }

            int offset = 8;
            int width = 0, height = 0;
            int bitDepth = 0, colourType = 0;
            using MemoryStream idatStream = new MemoryStream();

            while (offset + 8 <= pngBytes.Length)
            {
                int chunkLength = ReadBigEndianInt(pngBytes, offset);
                string chunkType = "" + (char)pngBytes[offset + 4]
                                     + (char)pngBytes[offset + 5]
                                     + (char)pngBytes[offset + 6]
                                     + (char)pngBytes[offset + 7];

                int dataStart = offset + 8;

                if (chunkType == "IHDR")
                {
                    width      = ReadBigEndianInt(pngBytes, dataStart);
                    height     = ReadBigEndianInt(pngBytes, dataStart + 4);
                    bitDepth   = pngBytes[dataStart + 8];
                    colourType = pngBytes[dataStart + 9];

                    if (bitDepth != 8)
                        throw new NotSupportedException($"PNG bit depth {bitDepth} is not supported (only 8-bit).");
                    if (colourType != 6 && colourType != 2)
                        throw new NotSupportedException($"PNG colour type {colourType} is not supported (only RGB=2 and RGBA=6).");
                }
                else if (chunkType == "IDAT")
                {
                    idatStream.Write(pngBytes, dataStart, chunkLength);
                }
                else if (chunkType == "IEND")
                {
                    break;
                }

                // Skip to next chunk: 4 (length) + 4 (type) + data + 4 (CRC)
                offset = dataStart + chunkLength + 4;
            }

            if (width == 0 || height == 0)
                throw new ArgumentException("PNG file has no valid IHDR chunk.");

            // Decompress the concatenated IDAT data
            idatStream.Position = 0;
            byte[] rawScanlines;
            int bytesPerPixel = (colourType == 6) ? 4 : 3;
            int stride = width * bytesPerPixel + 1; // +1 for filter byte per row

            using (MemoryStream decompressed = new MemoryStream())
            {
                using (ZLibStream zlib = new ZLibStream(idatStream, CompressionMode.Decompress, leaveOpen: true))
                {
                    zlib.CopyTo(decompressed);
                }
                rawScanlines = decompressed.ToArray();
            }

            // Unfilter scanlines and produce RGBA output
            byte[] rgbaPixels = new byte[width * height * 4];
            int rawStride = width * bytesPerPixel;

            for (int y = 0; y < height; y++)
            {
                int rowStart = y * stride;
                byte filterType = rawScanlines[rowStart];
                int dataOffset = rowStart + 1;

                // Apply PNG row filter (None=0, Sub=1, Up=2, Average=3, Paeth=4)
                UnfilterRow(rawScanlines, dataOffset, rawStride, bytesPerPixel, filterType, y);

                // Copy to RGBA output
                for (int x = 0; x < width; x++)
                {
                    int srcIdx = dataOffset + x * bytesPerPixel;
                    int dstIdx = (y * width + x) * 4;

                    rgbaPixels[dstIdx]     = rawScanlines[srcIdx];       // R
                    rgbaPixels[dstIdx + 1] = rawScanlines[srcIdx + 1];   // G
                    rgbaPixels[dstIdx + 2] = rawScanlines[srcIdx + 2];   // B
                    rgbaPixels[dstIdx + 3] = (colourType == 6)
                        ? rawScanlines[srcIdx + 3]   // A from file
                        : (byte)255;                  // fully opaque for RGB
                }
            }

            return (rgbaPixels, width, height);
        }


        /// <summary>
        /// Undo PNG row filtering in-place.
        /// </summary>
        private static void UnfilterRow(byte[] data, int offset, int stride, int bpp, byte filterType, int y)
        {
            switch (filterType)
            {
                case 0: // None
                    break;

                case 1: // Sub
                    for (int i = bpp; i < stride; i++)
                    {
                        data[offset + i] = (byte)(data[offset + i] + data[offset + i - bpp]);
                    }
                    break;

                case 2: // Up
                    if (y > 0)
                    {
                        int prevRowOffset = offset - stride - 1; // previous row's data start
                        for (int i = 0; i < stride; i++)
                        {
                            data[offset + i] = (byte)(data[offset + i] + data[prevRowOffset + i]);
                        }
                    }
                    break;

                case 3: // Average
                    for (int i = 0; i < stride; i++)
                    {
                        byte a = (i >= bpp) ? data[offset + i - bpp] : (byte)0;
                        byte b = (y > 0) ? data[offset - stride - 1 + i] : (byte)0;
                        data[offset + i] = (byte)(data[offset + i] + (byte)((a + b) / 2));
                    }
                    break;

                case 4: // Paeth
                    for (int i = 0; i < stride; i++)
                    {
                        byte a = (i >= bpp) ? data[offset + i - bpp] : (byte)0;
                        byte b = (y > 0) ? data[offset - stride - 1 + i] : (byte)0;
                        byte c = (i >= bpp && y > 0) ? data[offset - stride - 1 + i - bpp] : (byte)0;
                        data[offset + i] = (byte)(data[offset + i] + PaethPredictor(a, b, c));
                    }
                    break;

                default:
                    throw new NotSupportedException($"PNG filter type {filterType} is not supported.");
            }
        }


        /// <summary>
        /// Paeth predictor function (PNG spec).
        /// </summary>
        private static byte PaethPredictor(byte a, byte b, byte c)
        {
            int p = a + b - c;
            int pa = Math.Abs(p - a);
            int pb = Math.Abs(p - b);
            int pc = Math.Abs(p - c);

            if (pa <= pb && pa <= pc) return a;
            if (pb <= pc) return b;
            return c;
        }


        private static int ReadBigEndianInt(byte[] data, int offset)
        {
            return (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];
        }
    }
}
