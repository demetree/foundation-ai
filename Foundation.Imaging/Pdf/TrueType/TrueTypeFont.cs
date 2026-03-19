using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Foundation.Imaging.Pdf.TrueType
{
    public class TrueTypeFont
    {
        private readonly byte[] _data;
        private readonly Dictionary<string, TableRecord> _tableDirectory = new Dictionary<string, TableRecord>();

        public string FamilyName { get; private set; }
        public string StyleName { get; private set; }
        public int NumGlyphs { get; private set; }
        public int UnitsPerEm { get; private set; }
        public ushort Ascender { get; private set; }
        public ushort Descender { get; private set; }
        public ushort XMin { get; private set; }
        public ushort YMin { get; private set; }
        public ushort XMax { get; private set; }
        public ushort YMax { get; private set; }
        public bool IsCFF { get; private set; }
        public bool CanEmbed { get; private set; }
        public byte[] RawData => _data;

        private ushort[] _glyphWidths;
        private readonly Dictionary<int, List<int>> _cmapCache = new Dictionary<int, List<int>>();
        private byte[] _cmapData;

        public static TrueTypeFont Load(byte[] fontData)
        {
            if (fontData == null || fontData.Length < 12)
                throw new ArgumentException("Invalid font data");

            var font = new TrueTypeFont(fontData);
            font.Parse();
            return font;
        }

        public static TrueTypeFont Load(string filePath)
        {
            return Load(File.ReadAllBytes(filePath));
        }

        private TrueTypeFont(byte[] data)
        {
            _data = data;
        }

        private void Parse()
        {
            ReadTableDirectory();

            ParseHead();
            ParseHhea();
            ParseMaxp();
            ParseOs2();
            ParseName();
            ParseCmap();
            ParseHmtx();
        }

        private void ReadTableDirectory()
        {
            uint sfntVersion = ReadUInt32(0);
            if (sfntVersion == 0x00010000)
            {
                IsCFF = false;
            }
            else if (sfntVersion == 0x4F54544F) // "OTTO"
            {
                IsCFF = true;
            }
            else
            {
                throw new ArgumentException($"Unknown sfnt version: 0x{sfntVersion:X}");
            }

            ushort numTables = ReadUInt16(4);

            for (int i = 0; i < numTables; i++)
            {
                int offset = 12 + i * 16;
                string tag = Encoding.ASCII.GetString(_data, offset, 4);
                uint checksum = ReadUInt32(offset + 4);
                uint tableOffset = ReadUInt32(offset + 8);
                uint tableLength = ReadUInt32(offset + 12);

                _tableDirectory[tag] = new TableRecord
                {
                    Tag = tag,
                    Checksum = checksum,
                    Offset = (int)tableOffset,
                    Length = (int)tableLength
                };
            }
        }

        private void ParseHead()
        {
            var head = GetTable("head");
            if (head == null) throw new InvalidOperationException("Missing 'head' table");

            UnitsPerEm = ReadUInt16(head.Offset + 18);
            XMin = ReadUInt16(head.Offset + 36);
            YMin = ReadUInt16(head.Offset + 38);
            XMax = ReadUInt16(head.Offset + 40);
            YMax = ReadUInt16(head.Offset + 42);
        }

        private void ParseHhea()
        {
            var hhea = GetTable("hhea");
            if (hhea == null) throw new InvalidOperationException("Missing 'hhea' table");

            Ascender = ReadUInt16(hhea.Offset + 4);
            Descender = ReadUInt16(hhea.Offset + 6);
        }

        private void ParseMaxp()
        {
            var maxp = GetTable("maxp");
            if (maxp == null) throw new InvalidOperationException("Missing 'maxp' table");

            if (IsCFF)
            {
                NumGlyphs = ReadUInt16(maxp.Offset + 12);
            }
            else
            {
                NumGlyphs = ReadUInt16(maxp.Offset + 4);
            }
        }

        private void ParseOs2()
        {
            var os2 = GetTable("OS/2");
            CanEmbed = true;

            if (os2 != null && os2.Length >= 8)
            {
                ushort fsType = ReadUInt16(os2.Offset + 8);
                if ((fsType & 0x0001) != 0)
                {
                    CanEmbed = false;
                }
            }
        }

        private void ParseName()
        {
            var name = GetTable("name");
            if (name == null)
            {
                FamilyName = "Unknown";
                StyleName = "Regular";
                return;
            }

            FamilyName = ReadNameTable(name.Offset, 1);
            StyleName = ReadNameTable(name.Offset, 2);

            if (string.IsNullOrEmpty(FamilyName))
                FamilyName = "Unknown";
            if (string.IsNullOrEmpty(StyleName))
                StyleName = "Regular";
        }

        private string ReadNameTable(int offset, ushort nameId)
        {
            ushort format = ReadUInt16(offset);
            ushort count = ReadUInt16(offset + 2);
            ushort stringOffset = ReadUInt16(offset + 4);

            for (int i = 0; i < count; i++)
            {
                int recordOffset = offset + 6 + i * 12;
                ushort platformId = ReadUInt16(recordOffset);
                ushort encodingId = ReadUInt16(recordOffset + 2);
                ushort languageId = ReadUInt16(recordOffset + 4);
                ushort name = ReadUInt16(recordOffset + 6);
                ushort length = ReadUInt16(recordOffset + 8);
                ushort strOffset = ReadUInt16(recordOffset + 10);

                if (name != nameId) continue;

                int stringStart = offset + stringOffset + strOffset;
                byte[] stringBytes = new byte[length];
                Array.Copy(_data, stringStart, stringBytes, 0, length);

                if (platformId == 3 && encodingId == 1)
                {
                    return Encoding.BigEndianUnicode.GetString(stringBytes);
                }
                else if (platformId == 1 && encodingId == 0)
                {
                    return Encoding.ASCII.GetString(stringBytes);
                }
                else if (platformId == 0)
                {
                    return Encoding.BigEndianUnicode.GetString(stringBytes);
                }
            }

            return null;
        }

        private void ParseCmap()
        {
            var cmap = GetTable("cmap");
            if (cmap == null)
            {
                _cmapData = Array.Empty<byte>();
                return;
            }

            int len = cmap.Length;
            byte[] data = new byte[len];
            Array.Copy(_data, cmap.Offset, data, 0, len);
            _cmapData = data;
        }

        private void ParseHmtx()
        {
            var hhea = GetTable("hhea");
            var hmtx = GetTable("hmtx");
            if (hhea == null || hmtx == null)
                throw new InvalidOperationException("Missing 'hmtx' or 'hhea' table");

            ushort numHMetrics = ReadUInt16(hhea.Offset + 34);
            _glyphWidths = new ushort[NumGlyphs];

            int offset = hmtx.Offset;
            for (int i = 0; i < NumGlyphs; i++)
            {
                if (i < numHMetrics)
                {
                    _glyphWidths[i] = ReadUInt16(offset);
                    offset += 4;
                }
                else
                {
                    offset += 2;
                }
            }
        }

        public int GetGlyphIndex(int unicodeCodePoint)
        {
            if (_cmapData == null || _cmapData.Length == 0)
                return 0;

            int offset = 0;

            ushort version = ReadUInt16(offset);
            ushort numTables = ReadUInt16(offset + 2);

            int bestPlatform = -1;
            int bestEncoding = -1;
            int bestMappingOffset = -1;
            int bestPriority = int.MaxValue;

            for (int i = 0; i < numTables; i++)
            {
                int recordOffset = offset + 4 + i * 8;
                ushort platformId = ReadUInt16(recordOffset);
                ushort encodingId = ReadUInt16(recordOffset + 2);
                uint mappingOffset = ReadUInt32(recordOffset + 4);

                int priority = GetCmapPriority(platformId, encodingId, unicodeCodePoint);
                if (priority < bestPriority)
                {
                    bestPriority = priority;
                    bestPlatform = platformId;
                    bestEncoding = encodingId;
                    bestMappingOffset = (int)mappingOffset;
                }
            }

            if (bestMappingOffset < 0 || bestMappingOffset >= _cmapData.Length)
                return 0;

            return ReadCmapSubtable(bestMappingOffset, unicodeCodePoint);
        }

        private int GetCmapPriority(ushort platformId, ushort encodingId, int unicodeCodePoint)
        {
            if (platformId == 3 && encodingId == 1)
            {
                if (unicodeCodePoint <= 0xFFFF) return 1;
            }
            if (platformId == 0 && encodingId == 4)
            {
                if (unicodeCodePoint <= 0x10FFFF) return 2;
            }
            if (platformId == 0 && encodingId == 3)
            {
                if (unicodeCodePoint <= 0xFFFF) return 3;
            }
            if (platformId == 1 && encodingId == 0)
            {
                return 10;
            }
            return 100;
        }

        private int ReadCmapSubtable(int offset, int unicodeCodePoint)
        {
            ushort format = ReadUInt16(offset);

            switch (format)
            {
                case 0:
                    return ReadCmapFormat0(offset, (byte)unicodeCodePoint);
                case 4:
                    return ReadCmapFormat4(offset, unicodeCodePoint);
                case 6:
                    return ReadCmapFormat6(offset, unicodeCodePoint);
                case 12:
                    return ReadCmapFormat12(offset, unicodeCodePoint);
                default:
                    return 0;
            }
        }

        private int ReadCmapFormat0(int offset, byte charCode)
        {
            int glyphIdArrayOffset = offset + 6;
            if (glyphIdArrayOffset >= _cmapData.Length)
                return 0;
            return _cmapData[glyphIdArrayOffset + charCode];
        }

        private int ReadCmapFormat4(int offset, int unicodeCodePoint)
        {
            ushort segCountX2 = ReadUInt16(offset + 6);
            int segCount = segCountX2 / 2;
            int endCodesOffset = offset + 14;
            int startCodesOffset = endCodesOffset + segCountX2 + 2;
            int idDeltaOffset = startCodesOffset + segCountX2;
            int idRangeOffset = idDeltaOffset + segCountX2;

            for (int i = 0; i < segCount; i++)
            {
                ushort endCode = ReadUInt16(endCodesOffset + i * 2);
                if (unicodeCodePoint > endCode) continue;

                ushort startCode = ReadUInt16(startCodesOffset + i * 2);
                if (unicodeCodePoint < startCode) return 0;

                ushort idRangeOffsetVal = ReadUInt16(idRangeOffset + i * 2);
                if (idRangeOffsetVal == 0)
                {
                    short idDelta = ReadInt16(idDeltaOffset + i * 2);
                    return (unicodeCodePoint + idDelta) & 0xFFFF;
                }
                else
                {
                    int glyphIndexOffset = idRangeOffset + i * 2 + idRangeOffsetVal + (unicodeCodePoint - startCode) * 2;
                    if (glyphIndexOffset + 2 > _cmapData.Length)
                        return 0;
                    ushort glyphIndex = ReadUInt16(glyphIndexOffset);
                    if (glyphIndex == 0) return 0;
                    short idDelta = ReadInt16(idDeltaOffset + i * 2);
                    return (glyphIndex + idDelta) & 0xFFFF;
                }
            }

            return 0;
        }

        private int ReadCmapFormat6(int offset, int unicodeCodePoint)
        {
            ushort firstCode = ReadUInt16(offset + 6);
            ushort entryCount = ReadUInt16(offset + 8);

            int index = unicodeCodePoint - firstCode;
            if (index < 0 || index >= entryCount)
                return 0;

            return ReadUInt16(offset + 10 + index * 2);
        }

        private int ReadCmapFormat12(int offset, int unicodeCodePoint)
        {
            int numGroups = (int)ReadUInt32(offset + 12);

            for (int i = 0; i < numGroups; i++)
            {
                int groupOffset = offset + 16 + i * 12;
                uint startCharCode = ReadUInt32(groupOffset);
                uint endCharCode = ReadUInt32(groupOffset + 4);
                uint startGlyphId = ReadUInt32(groupOffset + 8);

                if (unicodeCodePoint >= startCharCode && unicodeCodePoint <= endCharCode)
                {
                    return (int)(startGlyphId + unicodeCodePoint - startCharCode);
                }
            }

            return 0;
        }

        public double GetGlyphWidth(int glyphIndex, double fontSize)
        {
            if (glyphIndex < 0 || glyphIndex >= _glyphWidths.Length)
                glyphIndex = 0;

            return (_glyphWidths[glyphIndex] / (double)UnitsPerEm) * fontSize;
        }

        public double GetGlyphWidthByUnicode(int unicodeCodePoint, double fontSize)
        {
            int glyphIndex = GetGlyphIndex(unicodeCodePoint);
            return GetGlyphWidth(glyphIndex, fontSize);
        }

        public double MeasureText(string text, double fontSize)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            double width = 0;
            int i = 0;
            while (i < text.Length)
            {
                int codePoint = text[i];
                if (char.IsHighSurrogate((char)codePoint) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                {
                    codePoint = char.ConvertToUtf32(text[i], text[i + 1]);
                    i += 2;
                }
                else
                {
                    i++;
                }
                width += GetGlyphWidthByUnicode(codePoint, fontSize);
            }
            return width;
        }

        public byte[] GetCffTable()
        {
            if (!IsCFF) return null;
            var cff = GetTable("CFF");
            if (cff == null) return null;
            byte[] result = new byte[cff.Length];
            Array.Copy(_data, cff.Offset, result, 0, cff.Length);
            return result;
        }

        public byte[] GetGlyfAndLoca(out bool locaLongFormat)
        {
            locaLongFormat = false;
            var head = GetTable("head");
            if (head == null) return null;
            locaLongFormat = ReadUInt16(head.Offset + 50) == 1;

            var glyf = GetTable("glyf");
            var loca = GetTable("loca");
            if (glyf == null || loca == null) return null;

            int totalSize = loca.Length + glyf.Length;
            byte[] result = new byte[totalSize];
            Array.Copy(_data, glyf.Offset, result, 0, glyf.Length);
            Array.Copy(_data, loca.Offset, result, glyf.Length, loca.Length);
            return result;
        }

        public int GetLocaOffset() => GetTable("loca")?.Offset ?? -1;
        public int GetLocaLength() => GetTable("loca")?.Length ?? 0;
        public int GetGlyfOffset() => GetTable("glyf")?.Offset ?? -1;
        public int GetGlyfLength() => GetTable("glyf")?.Length ?? 0;
        public int GetCmapOffset() => GetTable("cmap")?.Offset ?? -1;
        public int GetCmapLength() => GetTable("cmap")?.Length ?? 0;
        public int GetHheaOffset() => GetTable("hhea")?.Offset ?? -1;
        public int GetHmtxLength() => GetTable("hmtx")?.Length ?? 0;
        public int GetHeadOffset() => GetTable("head")?.Offset ?? -1;
        public int GetHheaNumOfLongHorMetrics() => ReadUInt16(GetHheaOffset() + 34);

        private TableRecord GetTable(string tag)
        {
            return _tableDirectory.TryGetValue(tag, out var record) ? record : null;
        }

        private ushort ReadUInt16(int offset)
        {
            if (offset + 2 > _data.Length) return 0;
            return (ushort)((_data[offset] << 8) | _data[offset + 1]);
        }

        private short ReadInt16(int offset)
        {
            if (offset + 2 > _data.Length) return 0;
            return (short)((_data[offset] << 8) | _data[offset + 1]);
        }

        private uint ReadUInt32(int offset)
        {
            if (offset + 4 > _data.Length) return 0;
            return ((uint)_data[offset] << 24) |
                   ((uint)_data[offset + 1] << 16) |
                   ((uint)_data[offset + 2] << 8) |
                   _data[offset + 3];
        }

        private class TableRecord
        {
            public string Tag;
            public uint Checksum;
            public int Offset;
            public int Length;
        }
    }
}
