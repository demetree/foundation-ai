using System;
using System.Collections.Generic;
using System.IO;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Minimal GIF89a encoder for animated GIFs from RGBA frames.
    ///
    /// Supports multiple frames with per-frame delay and looping.
    /// Uses median-cut colour quantization to reduce RGBA to 256-colour palette,
    /// then LZW-compresses each frame per the GIF spec.
    ///
    /// No external dependencies.
    /// </summary>
    public class GifEncoder
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _frameDelayCs;   // delay in centiseconds (1/100 s)
        private readonly List<(byte[] rgbaPixels, int delayCs)> _frames = new List<(byte[], int)>();

        /// <summary>
        /// Create a GIF encoder for fixed-size frames.
        /// </summary>
        /// <param name="width">Frame width in pixels.</param>
        /// <param name="height">Frame height in pixels.</param>
        /// <param name="defaultDelayMs">Default delay between frames in milliseconds.</param>
        public GifEncoder(int width, int height, int defaultDelayMs = 80)
        {
            _width = width;
            _height = height;
            _frameDelayCs = defaultDelayMs / 10;
        }

        /// <summary>Add a frame with the default delay.</summary>
        public void AddFrame(byte[] rgbaPixels)
        {
            _frames.Add((rgbaPixels, _frameDelayCs));
        }

        /// <summary>Add a frame with a specific delay in milliseconds.</summary>
        public void AddFrame(byte[] rgbaPixels, int delayMs)
        {
            _frames.Add((rgbaPixels, delayMs / 10));
        }

        /// <summary>
        /// Encode all frames to a looping animated GIF.
        /// </summary>
        public byte[] Encode()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter w = new BinaryWriter(ms))
            {
                // ── Header ──
                w.Write(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }); // "GIF89a"

                // ── Logical Screen Descriptor ──
                w.Write((ushort)_width);
                w.Write((ushort)_height);
                w.Write((byte)0x00);  // no global colour table
                w.Write((byte)0x00);  // background colour index
                w.Write((byte)0x00);  // pixel aspect ratio

                // ── Netscape Looping Extension (loop forever) ──
                w.Write((byte)0x21);  // extension introducer
                w.Write((byte)0xFF);  // application extension
                w.Write((byte)0x0B);  // block size
                w.Write(new byte[] { 0x4E, 0x45, 0x54, 0x53, 0x43, 0x41, 0x50, 0x45,
                                     0x32, 0x2E, 0x30 }); // "NETSCAPE2.0"
                w.Write((byte)0x03);  // sub-block size
                w.Write((byte)0x01);  // loop sub-block id
                w.Write((ushort)0);   // loop count (0 = infinite)
                w.Write((byte)0x00);  // block terminator

                // ── Frames ──
                foreach (var (pixels, delayCs) in _frames)
                {
                    WriteFrame(w, pixels, delayCs);
                }

                // ── Trailer ──
                w.Write((byte)0x3B);

                return ms.ToArray();
            }
        }


        private void WriteFrame(BinaryWriter w, byte[] rgbaPixels, int delayCs)
        {
            // Quantize RGBA to 256-colour palette
            byte[] palette;
            byte[] indices;
            Quantize(rgbaPixels, _width, _height, out palette, out indices);

            int colourCount = palette.Length / 3;
            int tableBits = ColorTableBits(colourCount);
            int tableSize = 1 << tableBits;

            // ── Graphic Control Extension ──
            w.Write((byte)0x21);  // extension introducer
            w.Write((byte)0xF9);  // graphic control label
            w.Write((byte)0x04);  // block size

            // Find transparent index (if any pixel was transparent)
            int transparentIndex = -1;
            for (int i = 0; i < rgbaPixels.Length; i += 4)
            {
                if (rgbaPixels[i + 3] < 128)
                {
                    transparentIndex = indices[i / 4];
                    break;
                }
            }

            byte packed = (byte)(
                (2 << 2) |                              // disposal = restore to background
                (transparentIndex >= 0 ? 1 : 0));       // transparent flag
            w.Write(packed);
            w.Write((ushort)delayCs);
            w.Write((byte)(transparentIndex >= 0 ? transparentIndex : 0));
            w.Write((byte)0x00);  // block terminator

            // ── Image Descriptor ──
            w.Write((byte)0x2C);    // image separator
            w.Write((ushort)0);     // left
            w.Write((ushort)0);     // top
            w.Write((ushort)_width);
            w.Write((ushort)_height);
            w.Write((byte)(0x80 | (tableBits - 1)));  // local colour table flag + size

            // ── Local Colour Table ──
            byte[] fullTable = new byte[tableSize * 3];
            Array.Copy(palette, 0, fullTable, 0, Math.Min(palette.Length, fullTable.Length));
            w.Write(fullTable);

            // ── LZW Image Data ──
            int minCodeSize = Math.Max(2, tableBits);
            w.Write((byte)minCodeSize);

            byte[] lzwData = LzwCompress(indices, minCodeSize);
            WriteSubBlocks(w, lzwData);

            w.Write((byte)0x00);  // block terminator
        }


        // ─── Colour Quantization ─────────────────────────────────────

        /// <summary>
        /// Simple popularity-based colour quantization.
        /// Takes the 255 most popular colours + 1 slot for transparency.
        /// </summary>
        private static void Quantize(byte[] rgbaPixels, int width, int height,
            out byte[] palette, out byte[] indices)
        {
            int pixelCount = width * height;
            Dictionary<int, int> colourCounts = new Dictionary<int, int>();

            // Count colour frequencies (ignore fully transparent pixels)
            for (int i = 0; i < pixelCount; i++)
            {
                int off = i * 4;
                byte a = rgbaPixels[off + 3];
                if (a < 128) continue;  // transparent

                int key = (rgbaPixels[off] << 16) | (rgbaPixels[off + 1] << 8) | rgbaPixels[off + 2];
                if (colourCounts.ContainsKey(key))
                    colourCounts[key]++;
                else
                    colourCounts[key] = 1;
            }

            // Sort by frequency, take top 255
            var sorted = new List<KeyValuePair<int, int>>(colourCounts);
            sorted.Sort((a, b) => b.Value.CompareTo(a.Value));

            int maxColours = 255;  // reserve index 0 for transparent
            int paletteCount = Math.Min(sorted.Count, maxColours);

            palette = new byte[(paletteCount + 1) * 3];

            // Index 0 = transparent (black)
            palette[0] = 0; palette[1] = 0; palette[2] = 0;

            // Build palette and lookup
            Dictionary<int, byte> colourToIndex = new Dictionary<int, byte>();
            for (int i = 0; i < paletteCount; i++)
            {
                int rgb = sorted[i].Key;
                int idx = (i + 1);
                palette[idx * 3 + 0] = (byte)((rgb >> 16) & 0xFF);
                palette[idx * 3 + 1] = (byte)((rgb >> 8) & 0xFF);
                palette[idx * 3 + 2] = (byte)(rgb & 0xFF);
                colourToIndex[rgb] = (byte)idx;
            }

            // Map pixels to palette indices
            indices = new byte[pixelCount];
            for (int i = 0; i < pixelCount; i++)
            {
                int off = i * 4;
                byte a = rgbaPixels[off + 3];
                if (a < 128)
                {
                    indices[i] = 0;  // transparent
                    continue;
                }

                int key = (rgbaPixels[off] << 16) | (rgbaPixels[off + 1] << 8) | rgbaPixels[off + 2];
                if (colourToIndex.TryGetValue(key, out byte idx))
                {
                    indices[i] = idx;
                }
                else
                {
                    // Colour not in palette — find nearest
                    indices[i] = FindNearest(palette, rgbaPixels[off], rgbaPixels[off + 1], rgbaPixels[off + 2]);
                }
            }
        }


        private static byte FindNearest(byte[] palette, byte r, byte g, byte b)
        {
            int best = 1;
            int bestDist = int.MaxValue;
            int count = palette.Length / 3;

            for (int i = 1; i < count; i++)
            {
                int dr = r - palette[i * 3];
                int dg = g - palette[i * 3 + 1];
                int db = b - palette[i * 3 + 2];
                int dist = dr * dr + dg * dg + db * db;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = i;
                }
            }

            return (byte)best;
        }


        private static int ColorTableBits(int colourCount)
        {
            // Must be 1..8, representing table sizes 2..256
            if (colourCount <= 2) return 1;
            if (colourCount <= 4) return 2;
            if (colourCount <= 8) return 3;
            if (colourCount <= 16) return 4;
            if (colourCount <= 32) return 5;
            if (colourCount <= 64) return 6;
            if (colourCount <= 128) return 7;
            return 8;
        }


        // ─── LZW Compression ─────────────────────────────────────────

        private static byte[] LzwCompress(byte[] indices, int minCodeSize)
        {
            int clearCode = 1 << minCodeSize;
            int endCode = clearCode + 1;
            int nextCode = endCode + 1;
            int codeBits = minCodeSize + 1;

            LzwBitPacker packer = new LzwBitPacker();
            Dictionary<long, int> table = new Dictionary<long, int>();

            packer.WriteBits(clearCode, codeBits);

            if (indices.Length == 0)
            {
                packer.WriteBits(endCode, codeBits);
                return packer.ToArray();
            }

            int current = indices[0];
            for (int i = 1; i < indices.Length; i++)
            {
                int next = indices[i];
                long key = ((long)current << 16) | (long)next;

                if (table.ContainsKey(key))
                {
                    current = table[key];
                }
                else
                {
                    packer.WriteBits(current, codeBits);

                    if (nextCode < 4096)
                    {
                        table[key] = nextCode++;
                        if (nextCode > (1 << codeBits) && codeBits < 12)
                            codeBits++;
                    }
                    else
                    {
                        // Table full — reset
                        packer.WriteBits(clearCode, codeBits);
                        table.Clear();
                        nextCode = endCode + 1;
                        codeBits = minCodeSize + 1;
                    }

                    current = next;
                }
            }

            packer.WriteBits(current, codeBits);
            packer.WriteBits(endCode, codeBits);

            return packer.ToArray();
        }


        /// <summary>
        /// Write LZW data as GIF sub-blocks (max 255 bytes each).
        /// </summary>
        private static void WriteSubBlocks(BinaryWriter w, byte[] data)
        {
            int offset = 0;
            while (offset < data.Length)
            {
                int blockSize = Math.Min(255, data.Length - offset);
                w.Write((byte)blockSize);
                w.Write(data, offset, blockSize);
                offset += blockSize;
            }
        }


        /// <summary>
        /// LSB-first bit packer for GIF LZW.
        /// </summary>
        private class LzwBitPacker
        {
            private readonly List<byte> _bytes = new List<byte>();
            private int _currentByte;
            private int _bitPos;

            public void WriteBits(int code, int numBits)
            {
                for (int i = 0; i < numBits; i++)
                {
                    if ((code & (1 << i)) != 0)
                        _currentByte |= (1 << _bitPos);

                    _bitPos++;
                    if (_bitPos == 8)
                    {
                        _bytes.Add((byte)_currentByte);
                        _currentByte = 0;
                        _bitPos = 0;
                    }
                }
            }

            public byte[] ToArray()
            {
                if (_bitPos > 0)
                    _bytes.Add((byte)_currentByte);
                return _bytes.ToArray();
            }
        }
    }
}
