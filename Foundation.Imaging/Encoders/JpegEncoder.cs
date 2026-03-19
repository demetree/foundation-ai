using System;
using System.IO;
using System.IO.Compression;

namespace Foundation.Imaging.Encoders
{
    public static class JpegEncoder
    {
        public static byte[] Encode(byte[] rgbPixels, int width, int height, int quality = 85)
        {
            if (quality < 1) quality = 1;
            if (quality > 100) quality = 100;

            int[] qtLum = BuildQuantizationTable(JpegConstants.QuantLum, quality);
            int[] qtChr = BuildQuantizationTable(JpegConstants.QuantChr, quality);

            var writer = new BitWriter();

            WriteSOI(writer);
            WriteAPP0(writer);
            WriteDQT(writer, qtLum, qtChr);
            WriteSOF0(writer, width, height);
            WriteDHT(writer);
            WriteSOS(writer, rgbPixels, width, height, qtLum, qtChr, writer);
            WriteEOI(writer);

            return writer.ToArray();
        }

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

        private static void WriteSOI(BitWriter w) => w.WriteU16(0xFFD8);

        private static void WriteEOI(BitWriter w) => w.WriteU16(0xFFD9);

        private static void WriteAPP0(BitWriter w)
        {
            int len = 16;
            w.WriteU16(0xFFE0);
            w.WriteU16((ushort)len);
            w.WriteBytes(System.Text.Encoding.ASCII.GetBytes("JFIF"));
            w.WriteByte(0);
            w.WriteByte(1);
            w.WriteByte(1);
            w.WriteByte(0);
            w.WriteByte(1);
            w.WriteByte(1);
            w.WriteByte(1);
            w.WriteByte(0);
        }

        private static void WriteDQT(BitWriter w, int[] qtLum, int[] qtChr)
        {
            for (int tq = 0; tq < 2; tq++)
            {
                int[] qt = tq == 0 ? qtLum : qtChr;
                w.WriteU16(0xFFDB);
                w.WriteU16((ushort)(67 + tq * 2));
                w.WriteByte((byte)(tq << 4));
                for (int i = 0; i < 64; i++)
                    w.WriteByte((byte)qt[JpegConstants.Zigzag[i]]);
            }
        }

        private static void WriteSOF0(BitWriter w, int width, int height)
        {
            w.WriteU16(0xFFC0);
            w.WriteU16(17);
            w.WriteByte(8);
            w.WriteU16((ushort)height);
            w.WriteU16((ushort)width);
            w.WriteByte(3);
            w.WriteByte(1);
            w.WriteByte(0x22);
            w.WriteByte(0);
            w.WriteByte(2);
            w.WriteByte(0x11);
            w.WriteByte(1);
            w.WriteByte(3);
            w.WriteByte(0x11);
            w.WriteByte(1);
            w.WriteByte(4);
            w.WriteByte(0x11);
            w.WriteByte(1);
            w.WriteByte(5);
            w.WriteByte(0x11);
            w.WriteByte(1);
        }

        private static void WriteDHT(BitWriter w)
        {
            WriteDHTSegment(w, 0xFFC4, 0, JpegConstants.DcLumBits, JpegConstants.DcLumVal);
            WriteDHTSegment(w, 0xFFC4, 1, JpegConstants.DcChrBits, JpegConstants.DcChrVal);
            WriteDHTSegment(w, 0xFFC4, 0x10, JpegConstants.AcLumBits, JpegConstants.AcLumVal);
            WriteDHTSegment(w, 0xFFC4, 0x11, JpegConstants.AcChrBits, JpegConstants.AcChrVal);
        }

        private static void WriteDHTSegment(BitWriter w, ushort marker, byte tableId,
            byte[] bits, byte[] vals)
        {
            int total = 0;
            for (int i = 1; i <= 16; i++) total += bits[i - 1];

            int len = 19 + total;
            for (int i = 0; i < total; i++) len++;

            w.WriteU16(marker);
            w.WriteU16((ushort)len);
            w.WriteByte(tableId);
            for (int i = 0; i < 16; i++) w.WriteByte(bits[i]);
            for (int i = 0; i < total; i++) w.WriteByte(vals[i]);
        }

        private static void WriteSOS(BitWriter w, byte[] rgb, int imgW, int imgH,
            int[] qtLum, int[] qtChr, BitWriter data)
        {
            int blocksY = (imgH + 7) / 8;
            int blocksX = (imgW + 7) / 8;
            int blocks = blocksX * blocksY;

            int stride = imgW * 3;
            int maxBlocks = Math.Max(blocksX, blocksY);

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
                    lastDCY = EncodeBlock(data, qBlock, 0, lastDCY,
                        JpegConstants.DcLumBits, JpegConstants.DcLumVal,
                        JpegConstants.AcLumBits, JpegConstants.AcLumVal);

                    ForwardDct(cbBlock, dctBlock);
                    qBlock = Quantize(dctBlock, qtChr);
                    lastDCCb = EncodeBlock(data, qBlock, 1, lastDCCb,
                        JpegConstants.DcChrBits, JpegConstants.DcChrVal,
                        JpegConstants.AcChrBits, JpegConstants.AcChrVal);

                    ForwardDct(crBlock, dctBlock);
                    qBlock = Quantize(dctBlock, qtChr);
                    lastDCCr = EncodeBlock(data, qBlock, 1, lastDCCr,
                        JpegConstants.DcChrBits, JpegConstants.DcChrVal,
                        JpegConstants.AcChrBits, JpegConstants.AcChrVal);
                }
            }

            w.WriteU16(0xFFDA);
            w.WriteU16((ushort)(12 + 3 * 2));
            w.WriteByte(3);
            w.WriteByte(1); w.WriteByte(0x00);
            w.WriteByte(2); w.WriteByte(0x11);
            w.WriteByte(3); w.WriteByte(0x11);
            w.WriteByte(0);
            w.WriteBytes(data.ToArray());
        }

        private static void CollectBlock(byte[] rgb, int imgW, int imgH,
            int bx, int by, int stride,
            float[] yBlock, float[] cbBlock, float[] crBlock)
        {
            int startX = bx * 8, startY = by * 8;

            for (int row = 0; row < 8; row++)
            {
                int py = Math.Min(startY + row, imgH - 1);
                int srcRow = py * stride;
                int pSrcRow = Math.Max(0, py) * stride;

                for (int col = 0; col < 8; col++)
                {
                    int px = Math.Min(startX + col, imgW - 1);
                    int si = pSrcRow + px * 3;

                    float r = rgb[si];
                    float g = rgb[si + 1];
                    float b = rgb[si + 2];

                    yBlock[row * 8 + col]  =  0.299f * r + 0.587f * g + 0.114f * b - 128f;
                    cbBlock[row * 8 + col] = -0.168736f * r - 0.331264f * g + 0.5f * b;
                    crBlock[row * 8 + col] =  0.5f * r - 0.418688f * g - 0.081312f * b;
                }
            }
        }

        private static void ForwardDct(float[] input, float[] output)
        {
            float[] tmp = new float[64];

            for (int u = 0; u < 8; u++)
            {
                for (int v = 0; v < 8; v++)
                {
                    float sum = 0f;
                    for (int x = 0; x < 8; x++)
                    {
                        float rx = input[x * 8 + u];
                        for (int y = 0; y < 8; y++)
                            sum += rx * input[y * 8 + x] * CosTable.Cos[(x << 3) | u] * CosTable.Cos[(y << 3) | v];
                    }
                    tmp[v * 8 + u] = sum * 0.25f;
                }
            }

            for (int i = 0; i < 64; i++)
            {
                float cu = (i & 7) == 0 ? 0.70710678f : 1f;
                float cv = (i >= 56) ? 0.70710678f : 1f;
                output[i] = cu * cv * tmp[i];
            }
        }

        private static int[] Quantize(float[] dct, int[] qt)
        {
            var result = new int[64];
            for (int i = 0; i < 64; i++)
            {
                float v = dct[i] / qt[i];
                result[JpegConstants.Zigzag[i]] = v > 0 ? (int)Math.Floor(v + 0.5f) : (int)Math.Ceiling(v - 0.5f);
            }
            return result;
        }

        private static int EncodeBlock(BitWriter w, int[] block, int tableType,
            int lastDC,
            byte[] dcBits, byte[] dcVals,
            byte[] acBits, byte[] acVals)
        {
            int dc = block[0] - lastDC;
            int dcSize = BitSize(dc);
            EncodeHuffman(w, dcSize, dcBits, dcVals);
            if (dcSize > 0) w.WriteBits(dc, dcSize);

            int nz = 0;
            for (int i = 63; i > 0; i--)
            {
                if (block[JpegConstants.Zigzag[i]] != 0) { nz = i; break; }
            }

            int pos = 1;
            while (pos <= nz)
            {
                int zeroCount = 0;
                while (pos <= nz && block[JpegConstants.Zigzag[pos]] == 0)
                {
                    zeroCount++;
                    pos++;
                }
                if (pos > nz) break;

                int val = block[JpegConstants.Zigzag[pos]];
                int valSize = BitSize(val);

                while (zeroCount >= 16)
                {
                    EncodeHuffman(w, 0xF0, acBits, acVals);
                    zeroCount -= 16;
                }

                EncodeHuffman(w, (zeroCount << 4) | valSize, acBits, acVals);
                w.WriteBits(val, valSize);
                pos++;
            }

            if (nz < 63)
                EncodeHuffman(w, 0, acBits, acVals);

            return block[0];
        }

        private static int BitSize(int value)
        {
            if (value == 0) return 0;
            if (value < 0) value = ~value;
            int bits = 1;
            while ((value >>= 1) != 0) bits++;
            return bits;
        }

        private static void EncodeHuffman(BitWriter w, int code,
            byte[] bits, byte[] vals)
        {
            int count = bits[0];
            int i = 0;
            while (count <= code)
            {
                i++;
                count += bits[i];
            }
            w.WriteBits(vals[i], bits[i]);
        }
    }

    internal class BitWriter
    {
        private readonly MemoryStream _ms = new MemoryStream();
        private int _buffer;
        private int _bitsInBuffer;

        public void WriteByte(byte b) => _ms.WriteByte(b);

        public void WriteBits(int value, int count)
        {
            while (count > 0)
            {
                int take = Math.Min(count, 8 - _bitsInBuffer);
                _buffer = (_buffer << take) | ((value >> (count - take)) & ((1 << take) - 1));
                _bitsInBuffer += take;
                count -= take;
                if (_bitsInBuffer == 8)
                {
                    _ms.WriteByte((byte)_buffer);
                    if (_buffer == 0xFF)
                        _ms.WriteByte(0);
                    _buffer = 0;
                    _bitsInBuffer = 0;
                }
            }
        }

        public void WriteU16(ushort v)
        {
            _ms.WriteByte((byte)(v >> 8));
            _ms.WriteByte((byte)v);
        }

        public void WriteBytes(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                _ms.WriteByte(data[i]);
                if (data[i] == 0xFF)
                    _ms.WriteByte(0);
            }
        }

        public byte[] ToArray()
        {
            if (_bitsInBuffer > 0)
            {
                int pad = 8 - _bitsInBuffer;
                _buffer <<= pad;
                _ms.WriteByte((byte)_buffer);
                if (_buffer == 0xFF)
                    _ms.WriteByte(0);
            }
            return _ms.ToArray();
        }
    }

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

        public static readonly byte[] DcLumBits = { 0, 1, 5, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 };
        public static readonly byte[] DcLumVal  = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,10,11 };

        public static readonly byte[] DcChrBits = { 0, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 };
        public static readonly byte[] DcChrVal  = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,10,11 };

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

        public static readonly byte[] AcChrBits = { 0, 2, 1, 2, 4, 4, 3, 4, 7, 5, 4, 0, 1, 125 };
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
