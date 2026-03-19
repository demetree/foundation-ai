using System;
using System.IO;

namespace Foundation.Imaging.Encoders
{
    /// <summary>
    /// Baseline JPEG encoder for RGB pixel data.
    ///
    /// Implements sequential DCT-based JPEG (SOI, APP0/JFIF, DQT, SOF0,
    /// DHT, SOS with Huffman entropy coding, EOI).  No chroma subsampling
    /// (4:4:4).
    ///
    /// No external dependencies.
    /// </summary>
    public static class JpegEncoder
    {
        public static byte[] Encode(byte[] rgbPixels, int width, int height, int quality = 85)
        {
            if (quality < 1) quality = 1;
            if (quality > 100) quality = 100;

            int[] qtLum = BuildQuantizationTable(JpegConstants.QuantLum, quality);
            int[] qtChr = BuildQuantizationTable(JpegConstants.QuantChr, quality);

            // Build Huffman code tables from the standard bit/val arrays
            var dcLumCodes = BuildHuffmanTable(JpegConstants.DcLumBits, JpegConstants.DcLumVal);
            var dcChrCodes = BuildHuffmanTable(JpegConstants.DcChrBits, JpegConstants.DcChrVal);
            var acLumCodes = BuildHuffmanTable(JpegConstants.AcLumBits, JpegConstants.AcLumVal);
            var acChrCodes = BuildHuffmanTable(JpegConstants.AcChrBits, JpegConstants.AcChrVal);

            // Write markers into the header stream
            using var output = new MemoryStream();

            WriteSOI(output);
            WriteAPP0(output);
            WriteDQT(output, 0, qtLum);
            WriteDQT(output, 1, qtChr);
            WriteSOF0(output, width, height);
            WriteDHT(output, 0x00, JpegConstants.DcLumBits, JpegConstants.DcLumVal);
            WriteDHT(output, 0x10, JpegConstants.AcLumBits, JpegConstants.AcLumVal);
            WriteDHT(output, 0x01, JpegConstants.DcChrBits, JpegConstants.DcChrVal);
            WriteDHT(output, 0x11, JpegConstants.AcChrBits, JpegConstants.AcChrVal);
            WriteSOS(output, rgbPixels, width, height, qtLum, qtChr,
                     dcLumCodes, acLumCodes, dcChrCodes, acChrCodes);
            WriteEOI(output);

            return output.ToArray();
        }

        // ─── Quantization table ─────────────────────────────────────

        private static int[] BuildQuantizationTable(int[] baseTable, int quality)
        {
            int scale = quality < 50 ? 5000 / quality : 200 - quality * 2;
            var table = new int[64];
            for (int i = 0; i < 64; i++)
            {
                int val = (baseTable[i] * scale + 50) / 100;
                table[i] = Math.Clamp(val, 1, 255);
            }
            return table;
        }

        // ─── Huffman code table builder ─────────────────────────────

        /// <summary>
        /// Build a lookup from symbol → (code, length) from the JPEG
        /// standard bits/vals arrays.
        /// </summary>
        private static (int code, int length)[] BuildHuffmanTable(byte[] bits, byte[] vals)
        {
            // Maximum symbol value in JPEG is 0xFF (AC run/size byte)
            var table = new (int code, int length)[256];
            for (int i = 0; i < 256; i++)
                table[i] = (0, 0);

            int code = 0;
            int vi = 0;
            for (int len = 1; len <= 16; len++)
            {
                for (int j = 0; j < bits[len - 1]; j++)
                {
                    byte symbol = vals[vi++];
                    table[symbol] = (code, len);
                    code++;
                }
                code <<= 1;
            }
            return table;
        }

        // ─── Marker writers ─────────────────────────────────────────

        private static void WriteSOI(Stream s)
        {
            s.WriteByte(0xFF); s.WriteByte(0xD8);
        }

        private static void WriteEOI(Stream s)
        {
            s.WriteByte(0xFF); s.WriteByte(0xD9);
        }

        private static void WriteAPP0(Stream s)
        {
            WriteMarker(s, 0xFFE0);
            WriteU16(s, 16);          // length
            s.Write(System.Text.Encoding.ASCII.GetBytes("JFIF"), 0, 4);
            s.WriteByte(0);           // null terminator
            s.WriteByte(1);           // major version
            s.WriteByte(1);           // minor version
            s.WriteByte(0);           // units = no units
            WriteU16(s, 1);           // X density
            WriteU16(s, 1);           // Y density
            s.WriteByte(0);           // thumbnail width
            s.WriteByte(0);           // thumbnail height
        }

        private static void WriteDQT(Stream s, int tableId, int[] qt)
        {
            WriteMarker(s, 0xFFDB);
            WriteU16(s, 67);          // length: 2 + 1 + 64 = 67
            s.WriteByte((byte)tableId);
            for (int i = 0; i < 64; i++)
                s.WriteByte((byte)qt[JpegConstants.Zigzag[i]]);
        }

        private static void WriteSOF0(Stream s, int width, int height)
        {
            WriteMarker(s, 0xFFC0);
            // Length = 2(len) + 1(prec) + 2(h) + 2(w) + 1(nComp) + 3*3 = 17
            WriteU16(s, 17);

            s.WriteByte(8);           // precision: 8 bits
            WriteU16(s, (ushort)height);
            WriteU16(s, (ushort)width);
            s.WriteByte(3);           // 3 components

            // Component 1: Y, sampling 1×1, quant table 0
            s.WriteByte(1);           // component id
            s.WriteByte(0x11);        // sampling factors: H=1, V=1
            s.WriteByte(0);           // quant table id

            // Component 2: Cb, sampling 1×1, quant table 1
            s.WriteByte(2);
            s.WriteByte(0x11);
            s.WriteByte(1);

            // Component 3: Cr, sampling 1×1, quant table 1
            s.WriteByte(3);
            s.WriteByte(0x11);
            s.WriteByte(1);
        }

        private static void WriteDHT(Stream s, byte tableId, byte[] bits, byte[] vals)
        {
            int total = 0;
            for (int i = 0; i < 16; i++) total += bits[i];

            int length = 2 + 1 + 16 + total;  // length field + tableId + 16 bits counts + values
            WriteMarker(s, 0xFFC4);
            WriteU16(s, (ushort)length);
            s.WriteByte(tableId);
            for (int i = 0; i < 16; i++) s.WriteByte(bits[i]);
            for (int i = 0; i < total; i++) s.WriteByte(vals[i]);
        }

        private static void WriteSOS(Stream s, byte[] rgb, int imgW, int imgH,
            int[] qtLum, int[] qtChr,
            (int code, int length)[] dcLumCodes, (int code, int length)[] acLumCodes,
            (int code, int length)[] dcChrCodes, (int code, int length)[] acChrCodes)
        {
            // Write SOS marker header
            WriteMarker(s, 0xFFDA);
            WriteU16(s, 12);          // length: 2 + 1 + 3*(1+1) + 3 = 12
            s.WriteByte(3);           // 3 components

            s.WriteByte(1); s.WriteByte(0x00);  // Y:  DC table 0, AC table 0
            s.WriteByte(2); s.WriteByte(0x11);  // Cb: DC table 1, AC table 1
            s.WriteByte(3); s.WriteByte(0x11);  // Cr: DC table 1, AC table 1

            s.WriteByte(0);   // Ss (start of spectral selection)
            s.WriteByte(63);  // Se (end of spectral selection)
            s.WriteByte(0);   // Ah/Al

            // Encode scan data
            var bitWriter = new JpegBitWriter(s);

            int blocksY = (imgH + 7) / 8;
            int blocksX = (imgW + 7) / 8;
            int stride = imgW * 3;

            var yBlock = new float[64];
            var cbBlock = new float[64];
            var crBlock = new float[64];
            var dctBlock = new float[64];

            int lastDCY = 0, lastDCCb = 0, lastDCCr = 0;

            for (int by = 0; by < blocksY; by++)
            {
                for (int bx = 0; bx < blocksX; bx++)
                {
                    CollectBlock(rgb, imgW, imgH, bx, by, stride, yBlock, cbBlock, crBlock);

                    ForwardDct(yBlock, dctBlock);
                    int[] qBlock = Quantize(dctBlock, qtLum);
                    lastDCY = EncodeBlock(bitWriter, qBlock, lastDCY, dcLumCodes, acLumCodes);

                    ForwardDct(cbBlock, dctBlock);
                    qBlock = Quantize(dctBlock, qtChr);
                    lastDCCb = EncodeBlock(bitWriter, qBlock, lastDCCb, dcChrCodes, acChrCodes);

                    ForwardDct(crBlock, dctBlock);
                    qBlock = Quantize(dctBlock, qtChr);
                    lastDCCr = EncodeBlock(bitWriter, qBlock, lastDCCr, dcChrCodes, acChrCodes);
                }
            }

            bitWriter.Flush();
        }

        // ─── Block collection (RGB → YCbCr) ────────────────────────

        private static void CollectBlock(byte[] rgb, int imgW, int imgH,
            int bx, int by, int stride,
            float[] yBlock, float[] cbBlock, float[] crBlock)
        {
            int startX = bx * 8, startY = by * 8;

            for (int row = 0; row < 8; row++)
            {
                int py = Math.Min(startY + row, imgH - 1);
                int srcRow = py * stride;

                for (int col = 0; col < 8; col++)
                {
                    int px = Math.Min(startX + col, imgW - 1);
                    int si = srcRow + px * 3;

                    float r = rgb[si];
                    float g = rgb[si + 1];
                    float b = rgb[si + 2];

                    int idx = row * 8 + col;
                    yBlock[idx]  =  0.299f * r + 0.587f * g + 0.114f * b - 128f;
                    cbBlock[idx] = -0.168736f * r - 0.331264f * g + 0.5f * b;
                    crBlock[idx] =  0.5f * r - 0.418688f * g - 0.081312f * b;
                }
            }
        }

        // ─── Forward DCT (AAN-style, separable) ────────────────────

        private static void ForwardDct(float[] input, float[] output)
        {
            // Separable 2D DCT: row-transform then column-transform
            float[] tmp = new float[64];

            // Row transform
            for (int row = 0; row < 8; row++)
            {
                int off = row * 8;
                for (int u = 0; u < 8; u++)
                {
                    float sum = 0f;
                    for (int x = 0; x < 8; x++)
                        sum += input[off + x] * CosTable.Cos[(x << 3) | u];
                    float cu = (u == 0) ? 0.70710678f : 1f;
                    tmp[off + u] = sum * cu * 0.5f;
                }
            }

            // Column transform
            for (int col = 0; col < 8; col++)
            {
                for (int v = 0; v < 8; v++)
                {
                    float sum = 0f;
                    for (int y = 0; y < 8; y++)
                        sum += tmp[y * 8 + col] * CosTable.Cos[(y << 3) | v];
                    float cv = (v == 0) ? 0.70710678f : 1f;
                    output[v * 8 + col] = sum * cv * 0.5f;
                }
            }
        }

        // ─── Quantization ───────────────────────────────────────────

        private static int[] Quantize(float[] dct, int[] qt)
        {
            var result = new int[64];
            for (int i = 0; i < 64; i++)
            {
                int zi = JpegConstants.Zigzag[i];
                float v = dct[i] / qt[i];
                result[zi] = v > 0 ? (int)Math.Floor(v + 0.5f) : (int)Math.Ceiling(v - 0.5f);
            }
            return result;
        }

        // ─── Huffman encode one 8×8 block ───────────────────────────

        private static int EncodeBlock(JpegBitWriter w, int[] block, int lastDC,
            (int code, int length)[] dcCodes, (int code, int length)[] acCodes)
        {
            // DC coefficient (differential)
            int dc = block[0] - lastDC;
            int dcCat = BitCategory(dc);
            var (dcCode, dcLen) = dcCodes[dcCat];
            w.WriteBits(dcCode, dcLen);
            if (dcCat > 0) w.WriteBits(MakeSignedBits(dc, dcCat), dcCat);

            // AC coefficients (zigzag order, block[] is already zigzagged)
            int lastNonZero = 0;
            for (int i = 63; i > 0; i--)
            {
                if (block[i] != 0) { lastNonZero = i; break; }
            }

            int pos = 1;
            while (pos <= lastNonZero)
            {
                int zeroRun = 0;
                while (pos <= lastNonZero && block[pos] == 0)
                {
                    zeroRun++;
                    pos++;
                }
                if (pos > lastNonZero) break;

                while (zeroRun >= 16)
                {
                    var (zrlCode, zrlLen) = acCodes[0xF0];
                    w.WriteBits(zrlCode, zrlLen);
                    zeroRun -= 16;
                }

                int val = block[pos];
                int acCat = BitCategory(val);
                int symbol = (zeroRun << 4) | acCat;
                var (acCode, acLen) = acCodes[symbol];
                w.WriteBits(acCode, acLen);
                w.WriteBits(MakeSignedBits(val, acCat), acCat);
                pos++;
            }

            // EOB if we ended before position 63
            if (lastNonZero < 63)
            {
                var (eobCode, eobLen) = acCodes[0x00];
                w.WriteBits(eobCode, eobLen);
            }

            return block[0];
        }

        /// <summary>
        /// Returns the number of bits needed to represent |value| (JPEG category).
        /// </summary>
        private static int BitCategory(int value)
        {
            if (value == 0) return 0;
            if (value < 0) value = -value;
            int bits = 0;
            while (value > 0)
            {
                bits++;
                value >>= 1;
            }
            return bits;
        }

        /// <summary>
        /// Encode a signed value for JPEG: positive values are emitted directly,
        /// negative values are ones'-complement (value - 1).
        /// </summary>
        private static int MakeSignedBits(int value, int category)
        {
            if (value < 0)
                return value + (1 << category) - 1;
            return value;
        }

        // ─── Helpers ────────────────────────────────────────────────

        private static void WriteMarker(Stream s, int marker)
        {
            s.WriteByte((byte)(marker >> 8));
            s.WriteByte((byte)marker);
        }

        private static void WriteU16(Stream s, int value)
        {
            s.WriteByte((byte)(value >> 8));
            s.WriteByte((byte)value);
        }

        private static void WriteU16(Stream s, ushort value)
        {
            s.WriteByte((byte)(value >> 8));
            s.WriteByte((byte)value);
        }
    }

    // ─── Bit-stream writer for JPEG entropy coding ──────────────

    internal class JpegBitWriter
    {
        private readonly Stream _stream;
        private int _buffer;
        private int _bitsInBuffer;

        public JpegBitWriter(Stream stream)
        {
            _stream = stream;
        }

        public void WriteBits(int value, int count)
        {
            for (int i = count - 1; i >= 0; i--)
            {
                _buffer = (_buffer << 1) | ((value >> i) & 1);
                _bitsInBuffer++;
                if (_bitsInBuffer == 8)
                {
                    _stream.WriteByte((byte)_buffer);
                    if (_buffer == 0xFF)
                        _stream.WriteByte(0x00);  // byte-stuff
                    _buffer = 0;
                    _bitsInBuffer = 0;
                }
            }
        }

        public void Flush()
        {
            if (_bitsInBuffer > 0)
            {
                _buffer <<= (8 - _bitsInBuffer);
                _stream.WriteByte((byte)_buffer);
                if (_buffer == 0xFF)
                    _stream.WriteByte(0x00);
                _buffer = 0;
                _bitsInBuffer = 0;
            }
        }
    }

    // ─── Cosine lookup table ────────────────────────────────────

    internal static class CosTable
    {
        public static readonly float[] Cos = Build();

        private static float[] Build()
        {
            var t = new float[64];
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    t[(i << 3) | j] = MathF.Cos((2 * i + 1) * j * MathF.PI / 16f);
            return t;
        }
    }

    // ─── JPEG constants ─────────────────────────────────────────

    internal static class JpegConstants
    {
        public static readonly int[] Zigzag = {
             0,  1,  8, 16,  9,  2,  3, 10,
            17, 24, 32, 25, 18, 11,  4,  5,
            12, 19, 26, 33, 40, 48, 41, 34,
            27, 20, 13,  6,  7, 14, 21, 28,
            35, 42, 49, 56, 57, 50, 43, 36,
            29, 22, 15, 23, 30, 37, 44, 51,
            58, 59, 52, 45, 38, 31, 39, 46,
            53, 60, 61, 54, 47, 55, 62, 63,
        };

        public static readonly int[] QuantLum = {
            16, 11, 10, 16,  24,  40,  51,  61,
            12, 12, 14, 19,  26,  58,  60,  55,
            14, 13, 16, 24,  40,  57,  69,  56,
            14, 17, 22, 29,  51,  87,  80,  62,
            18, 22, 37, 56,  68, 109, 103,  77,
            24, 35, 55, 64,  81, 104, 113,  92,
            49, 64, 78, 87, 103, 121, 120, 101,
            72, 92, 95, 98, 112, 100, 103,  99,
        };

        public static readonly int[] QuantChr = {
            17, 18, 24, 47, 99, 99, 99, 99,
            18, 21, 26, 66, 99, 99, 99, 99,
            24, 26, 56, 99, 99, 99, 99, 99,
            47, 66, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
        };

        // Standard Huffman tables (JPEG Annex K)
        public static readonly byte[] DcLumBits = { 0, 1, 5, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 };
        public static readonly byte[] DcLumVal  = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

        public static readonly byte[] DcChrBits = { 0, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 };
        public static readonly byte[] DcChrVal  = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

        public static readonly byte[] AcLumBits = { 0, 2, 1, 3, 3, 2, 4, 3, 5, 5, 4, 4, 0, 0, 1, 125 };
        public static readonly byte[] AcLumVal  = {
            0x01,0x02,0x03,0x00,0x04,0x11,0x05,0x12,0x21,0x31,0x41,0x06,0x13,0x51,0x61,0x07,
            0x22,0x71,0x14,0x32,0x81,0x91,0xA1,0x08,0x23,0x42,0xB1,0xC1,0x15,0x52,0xD1,0xF0,
            0x24,0x33,0x62,0x72,0x82,0x09,0x0A,0x16,0x17,0x18,0x19,0x1A,0x25,0x26,0x27,0x28,
            0x29,0x2A,0x34,0x35,0x36,0x37,0x38,0x39,0x3A,0x43,0x44,0x45,0x46,0x47,0x48,0x49,
            0x4A,0x53,0x54,0x55,0x56,0x57,0x58,0x59,0x5A,0x63,0x64,0x65,0x66,0x67,0x68,0x69,
            0x6A,0x73,0x74,0x75,0x76,0x77,0x78,0x79,0x7A,0x83,0x84,0x85,0x86,0x87,0x88,0x89,
            0x8A,0x92,0x93,0x94,0x95,0x96,0x97,0x98,0x99,0x9A,0xA2,0xA3,0xA4,0xA5,0xA6,0xA7,
            0xA8,0xA9,0xAA,0xB2,0xB3,0xB4,0xB5,0xB6,0xB7,0xB8,0xB9,0xBA,0xC2,0xC3,0xC4,0xC5,
            0xC6,0xC7,0xC8,0xC9,0xCA,0xD2,0xD3,0xD4,0xD5,0xD6,0xD7,0xD8,0xD9,0xDA,0xE1,0xE2,
            0xE3,0xE4,0xE5,0xE6,0xE7,0xE8,0xE9,0xEA,0xF1,0xF2,0xF3,0xF4,0xF5,0xF6,0xF7,0xF8,
            0xF9,0xFA,
        };

        public static readonly byte[] AcChrBits = { 0, 2, 1, 2, 4, 4, 3, 4, 7, 5, 4, 4, 0, 1, 2, 119 };
        public static readonly byte[] AcChrVal = {
            0x00,0x01,0x02,0x03,0x11,0x04,0x05,0x21,0x31,0x06,0x12,0x41,0x51,0x07,0x61,0x71,
            0x13,0x22,0x32,0x81,0x08,0x14,0x42,0x91,0xA1,0xB1,0xC1,0x09,0x23,0x33,0x52,0xF0,
            0x15,0x62,0x72,0xD1,0x0A,0x16,0x24,0x34,0xE1,0x25,0xF1,0x17,0x18,0x19,0x1A,0x26,
            0x27,0x28,0x29,0x2A,0x35,0x36,0x37,0x38,0x39,0x3A,0x43,0x44,0x45,0x46,0x47,0x48,
            0x49,0x4A,0x53,0x54,0x55,0x56,0x57,0x58,0x59,0x5A,0x63,0x64,0x65,0x66,0x67,0x68,
            0x69,0x6A,0x73,0x74,0x75,0x76,0x77,0x78,0x79,0x7A,0x82,0x83,0x84,0x85,0x86,0x87,
            0x88,0x89,0x8A,0x92,0x93,0x94,0x95,0x96,0x97,0x98,0x99,0x9A,0xA2,0xA3,0xA4,0xA5,
            0xA6,0xA7,0xA8,0xA9,0xAA,0xB2,0xB3,0xB4,0xB5,0xB6,0xB7,0xB8,0xB9,0xBA,0xC2,0xC3,
            0xC4,0xC5,0xC6,0xC7,0xC8,0xC9,0xCA,0xD2,0xD3,0xD4,0xD5,0xD6,0xD7,0xD8,0xD9,0xDA,
            0xE2,0xE3,0xE4,0xE5,0xE6,0xE7,0xE8,0xE9,0xEA,0xF2,0xF3,0xF4,0xF5,0xF6,0xF7,0xF8,
            0xF9,0xFA,
        };
    }
}
