using System;
using System.IO;
using System.IO.Compression;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Minimal PNG encoder for RGBA pixel data.
    ///
    /// Writes a valid PNG file from raw RGBA byte arrays without any
    /// external dependencies — uses .NET's built-in ZLibStream for
    /// DEFLATE compression and manual CRC32 for chunk checksums.
    /// </summary>
    public static class PngEncoder
    {
        private static readonly byte[] Signature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        /// <summary>
        /// Encode RGBA pixels to PNG bytes.
        /// </summary>
        public static byte[] Encode(byte[] rgbaPixels, int width, int height)
        {
            using (MemoryStream output = new MemoryStream())
            {
                // PNG signature
                output.Write(Signature, 0, Signature.Length);

                // IHDR chunk
                WriteChunk(output, "IHDR", writer =>
                {
                    WriteBigEndian(writer, width);
                    WriteBigEndian(writer, height);
                    writer.Write((byte)8);   // bit depth
                    writer.Write((byte)6);   // colour type: RGBA
                    writer.Write((byte)0);   // compression method
                    writer.Write((byte)0);   // filter method
                    writer.Write((byte)0);   // interlace method
                });

                // IDAT chunk — filtered scanlines compressed with ZLib
                byte[] compressedData = CompressScanlines(rgbaPixels, width, height);
                WriteChunk(output, "IDAT", writer =>
                {
                    writer.Write(compressedData);
                });

                // IEND chunk
                WriteChunk(output, "IEND", _ => { });

                return output.ToArray();
            }
        }


        /// <summary>
        /// Compress RGBA scanlines with filter byte 0 (None) per row.
        /// </summary>
        private static byte[] CompressScanlines(byte[] rgbaPixels, int width, int height)
        {
            int stride = width * 4;

            using (MemoryStream compressed = new MemoryStream())
            {
                // ZLibStream writes the ZLib header + DEFLATE data + checksum
                using (ZLibStream zlib = new ZLibStream(compressed, CompressionLevel.Fastest, leaveOpen: true))
                {
                    byte filterNone = 0;
                    for (int y = 0; y < height; y++)
                    {
                        zlib.WriteByte(filterNone);
                        zlib.Write(rgbaPixels, y * stride, stride);
                    }
                }

                return compressed.ToArray();
            }
        }


        /// <summary>
        /// Write a PNG chunk: length (4 bytes) + type (4 bytes) + data + CRC32 (4 bytes).
        /// </summary>
        private static void WriteChunk(Stream output, string chunkType, Action<BinaryWriter> writeData)
        {
            byte[] typeBytes = new byte[4];
            for (int i = 0; i < 4; i++)
                typeBytes[i] = (byte)chunkType[i];

            // Write data to a temp buffer to get the length
            byte[] data;
            using (MemoryStream dataStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(dataStream))
                {
                    writeData(writer);
                    writer.Flush();
                }
                data = dataStream.ToArray();
            }

            // Length (big-endian)
            byte[] lengthBytes = new byte[4];
            lengthBytes[0] = (byte)(data.Length >> 24);
            lengthBytes[1] = (byte)(data.Length >> 16);
            lengthBytes[2] = (byte)(data.Length >> 8);
            lengthBytes[3] = (byte)(data.Length);
            output.Write(lengthBytes, 0, 4);

            // Type
            output.Write(typeBytes, 0, 4);

            // Data
            if (data.Length > 0)
                output.Write(data, 0, data.Length);

            // CRC32 over type + data
            uint crc = Crc32(typeBytes, data);
            byte[] crcBytes = new byte[4];
            crcBytes[0] = (byte)(crc >> 24);
            crcBytes[1] = (byte)(crc >> 16);
            crcBytes[2] = (byte)(crc >> 8);
            crcBytes[3] = (byte)(crc);
            output.Write(crcBytes, 0, 4);
        }


        private static void WriteBigEndian(BinaryWriter writer, int value)
        {
            writer.Write((byte)(value >> 24));
            writer.Write((byte)(value >> 16));
            writer.Write((byte)(value >> 8));
            writer.Write((byte)(value));
        }


        // ─── CRC32 (PNG spec polynomial) ─────────────────────────────

        private static readonly uint[] CrcTable = BuildCrcTable();

        private static uint[] BuildCrcTable()
        {
            uint[] table = new uint[256];
            for (uint n = 0; n < 256; n++)
            {
                uint c = n;
                for (int k = 0; k < 8; k++)
                {
                    if ((c & 1) != 0)
                        c = 0xEDB88320u ^ (c >> 1);
                    else
                        c >>= 1;
                }
                table[n] = c;
            }
            return table;
        }

        private static uint Crc32(byte[] typeBytes, byte[] data)
        {
            uint crc = 0xFFFFFFFF;

            for (int i = 0; i < typeBytes.Length; i++)
                crc = CrcTable[(crc ^ typeBytes[i]) & 0xFF] ^ (crc >> 8);

            for (int i = 0; i < data.Length; i++)
                crc = CrcTable[(crc ^ data[i]) & 0xFF] ^ (crc >> 8);

            return crc ^ 0xFFFFFFFF;
        }
    }
}
