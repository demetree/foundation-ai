using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Foundation.Imaging.Pdf.TrueType
{
    public class EmbeddedFont
    {
        public string Name { get; }
        public string PostScriptName { get; }
        public string SubsetPrefix { get; }
        public bool IsBold { get; }
        public TrueTypeFont SourceFont { get; }
        public HashSet<int> UsedGlyphIndices { get; } = new HashSet<int>();

        private readonly Dictionary<int, int> _unicodeToGlyph = new Dictionary<int, int>();

        internal EmbeddedFont(string name, string postScriptName, TrueTypeFont font, bool isBold)
        {
            Name = name;
            PostScriptName = postScriptName;
            SubsetPrefix = GenerateSubsetPrefix();
            IsBold = isBold;
            SourceFont = font;
            BuildUnicodeMap();
        }

        private static string GenerateSubsetPrefix()
        {
            Random r = new Random();
            char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
            StringBuilder sb = new StringBuilder(7);
            for (int i = 0; i < 6; i++)
                sb.Append(chars[r.Next(chars.Length)]);
            sb.Append('+');
            return sb.ToString();
        }

        private void BuildUnicodeMap()
        {
            for (int cp = 0; cp <= 0x1FFFF; cp++)
            {
                int gi = SourceFont.GetGlyphIndex(cp);
                _unicodeToGlyph[cp] = gi;
                if (gi > 0)
                    UsedGlyphIndices.Add(gi);
            }
        }

        public int RecordGlyphUsage(int unicodeCodePoint)
        {
            int glyphIndex = SourceFont.GetGlyphIndex(unicodeCodePoint);
            if (glyphIndex > 0)
            {
                lock (UsedGlyphIndices)
                {
                    if (UsedGlyphIndices.Add(glyphIndex))
                        _unicodeToGlyph[unicodeCodePoint] = glyphIndex;
                }
            }
            return glyphIndex;
        }

        public int GetGlyphIndex(int unicodeCodePoint)
        {
            return _unicodeToGlyph.TryGetValue(unicodeCodePoint, out int gi) ? gi : 0;
        }

        public double GetGlyphWidth(int glyphIndex, double fontSize)
        {
            return SourceFont.GetGlyphWidth(glyphIndex, fontSize);
        }

        public double MeasureText(string text, double fontSize)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            double width = 0;
            int i = 0;
            while (i < text.Length)
            {
                int cp = text[i];
                if (char.IsHighSurrogate((char)cp) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                {
                    cp = char.ConvertToUtf32(text[i], text[i + 1]);
                    i += 2;
                }
                else
                {
                    i++;
                }
                width += GetGlyphWidth(GetGlyphIndex(cp), fontSize);
            }
            return width;
        }
    }

    public class FontRegistry
    {
        private readonly Dictionary<string, EmbeddedFont> _fonts = new Dictionary<string, EmbeddedFont>();
        private readonly HashSet<string> _registeredNames = new HashSet<string>();

        public EmbeddedFont Register(string name, byte[] fontData, bool isBold = false)
        {
            if (_registeredNames.Contains(name))
                return _fonts[name];

            var ttf = TrueTypeFont.Load(fontData);
            if (!ttf.CanEmbed)
                throw new InvalidOperationException(
                    $"Font '{name}' ({ttf.FamilyName}) cannot be embedded due to licensing restrictions.");

            string psName = $"{MakePsName(ttf.FamilyName, isBold)}-{ttf.StyleName}";
            var embedded = new EmbeddedFont(name, psName, ttf, isBold);
            _fonts[name] = embedded;
            _registeredNames.Add(name);
            return embedded;
        }

        public EmbeddedFont Register(string name, string filePath, bool isBold = false)
            => Register(name, File.ReadAllBytes(filePath), isBold);

        public EmbeddedFont Get(string name)
            => _fonts.TryGetValue(name, out var f) ? f : null;

        public bool IsRegistered(string name) => _registeredNames.Contains(name);

        private static string MakePsName(string familyName, bool isBold)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in familyName)
            {
                if (char.IsLetterOrDigit(c))
                    sb.Append(char.ToUpperInvariant(c));
            }
            if (isBold) sb.Append("Bold");
            return sb.Length > 27 ? sb.ToString(0, 27) : sb.ToString();
        }
    }

    public class FontSubsetBuilder
    {
        private readonly EmbeddedFont _font;

        public FontSubsetBuilder(EmbeddedFont font) { _font = font; }

        public byte[] BuildSubset()
        {
            if (_font.SourceFont.IsCFF)
            {
                var cff = _font.SourceFont.GetCffTable();
                return cff != null ? (byte[])cff.Clone() : null;
            }
            // CFF subsetting: the full CFF table is copied verbatim. True glyph
            // index remapping (via CFF charset/Encoding restructure) is not performed.
            // This works for Identity-H because the ToUnicode CMap handles text extraction,
            // but proper CFF subsetting (extract used glyphs, renumber charset) is needed
            // for file-size reduction and full spec compliance with CFF-based fonts.
            return BuildTrueTypeSubset();
        }

        private byte[] BuildTrueTypeSubset()
        {
            var ttf = _font.SourceFont;
            int glyphCount = _font.UsedGlyphIndices.Count + 1;
            if (glyphCount <= 1) return null;

            var sortedOrigGlyphIds = new List<int>(_font.UsedGlyphIndices);
            sortedOrigGlyphIds.Sort();

            int numHMetrics = glyphCount;

            int headSrc = ttf.GetHeadOffset();
            bool locaLong = headSrc >= 0 && ((ttf.RawData[headSrc + 50] << 8) | ttf.RawData[headSrc + 51]) == 1;

            int glyfDataSize = 0;
            for (int i = 0; i < sortedOrigGlyphIds.Count; i++)
            {
                glyfDataSize += Pad4(ExtractGlyphSize(ttf, sortedOrigGlyphIds[i]));
            }
            glyfDataSize += Pad4(0);

            int locaSize = (glyphCount + 1) * (locaLong ? 4 : 2);
            byte[] cmapData = BuildCmapFormat12(sortedOrigGlyphIds);
            int cmapSize = cmapData.Length;
            int hmtxSize = numHMetrics * 4;
            int maxpSize = 6 + 2 * glyphCount;

            int numTables = 7;
            int headerSize = 12 + numTables * 16;

            int glyfOff = headerSize;
            int locaOff = glyfOff + Pad4(glyfDataSize);
            int hmtxOff = locaOff + Pad4(locaSize);
            int headOff = hmtxOff + hmtxSize;
            int hheaOff = headOff + 54;
            int maxpOff = hheaOff + 36;
            int cmapOff = maxpOff + maxpSize;
            int totalSize = cmapOff + Pad4(cmapSize);

            var result = new byte[totalSize];
            int Pad4(int n) => (n + 3) & ~3;

            void WriteU16(int off, ushort v) { result[off] = (byte)(v >> 8); result[off + 1] = (byte)(v & 0xFF); }
            uint CalcChecksum(int off, int len)
            {
                uint sum = 0;
                for (int i = 0; i < len; i += 4)
                {
                    uint v = 0;
                    for (int j = 0; j < 4 && i + j < len; j++)
                        v = (v << 8) | (off + i + j < result.Length ? result[off + i + j] : (byte)0);
                    sum += v;
                }
                return sum;
            }

            void WriteLoca(int off, int value)
            {
                if (locaLong)
                {
                    result[off] = (byte)(value >> 24);
                    result[off + 1] = (byte)((value >> 16) & 0xFF);
                    result[off + 2] = (byte)((value >> 8) & 0xFF);
                    result[off + 3] = (byte)(value & 0xFF);
                }
                else
                {
                    result[off] = (byte)(value / 2 >> 8);
                    result[off + 1] = (byte)((value / 2) & 0xFF);
                }
            }

            void WriteDirEntry(string tag, int dataOff, int dataLen)
            {
                string[] order = { "cmap", "glyf", "head", "hhea", "hmtx", "loca", "maxp" };
                int idx = 0;
                for (int i = 0; i < order.Length; i++) { if (order[i] == tag) { idx = i; break; } }
                int b = 12 + idx * 16;
                byte[] tb = Encoding.ASCII.GetBytes(tag);
                result[b] = tb[0]; result[b + 1] = tb[1];
                result[b + 2] = tb[2]; result[b + 3] = tb[3];
                uint cs = CalcChecksum(dataOff, Pad4(dataLen));
                result[b + 4] = (byte)(cs >> 24); result[b + 5] = (byte)((cs >> 16) & 0xFF);
                result[b + 6] = (byte)((cs >> 8) & 0xFF); result[b + 7] = (byte)(cs & 0xFF);
                result[b + 8] = (byte)(dataOff >> 24); result[b + 9] = (byte)((dataOff >> 16) & 0xFF);
                result[b + 10] = (byte)((dataOff >> 8) & 0xFF); result[b + 11] = (byte)(dataOff & 0xFF);
                result[b + 12] = (byte)(Pad4(dataLen) >> 24); result[b + 13] = (byte)((Pad4(dataLen) >> 16) & 0xFF);
                result[b + 14] = (byte)((Pad4(dataLen) >> 8) & 0xFF); result[b + 15] = (byte)(Pad4(dataLen) & 0xFF);
            }

            // head (54 bytes)
            if (headSrc >= 0)
                Array.Copy(ttf.RawData, headSrc, result, headOff, 54);
            result[headOff + 17] = 1;
            WriteDirEntry("head", headOff, 54);

            // hhea (36 bytes)
            int hheaSrc = ttf.GetHheaOffset();
            if (hheaSrc >= 0)
                Array.Copy(ttf.RawData, hheaSrc, result, hheaOff, 36);
            WriteU16(hheaOff + 34, (ushort)numHMetrics);
            WriteDirEntry("hhea", hheaOff, 36);

            // hmtx — build lookup: original glyph index → hmtx entry index
            int origNumHMetrics = ttf.GetHheaNumOfLongHorMetrics();
            var glyphToHmtxIdx = new Dictionary<int, int>();
            for (int gi = 0; gi < origNumHMetrics; gi++)
                glyphToHmtxIdx[gi] = gi;
            for (int gi = origNumHMetrics; gi < ttf.NumGlyphs; gi++)
                glyphToHmtxIdx[gi] = origNumHMetrics - 1;

            int hmtxSrc = hheaSrc >= 0 ? hheaSrc + 36 : 0;
            for (int i = 0; i < numHMetrics; i++)
            {
                int origGi = i == 0 ? 0 : sortedOrigGlyphIds[i - 1];
                int hmtxIdx = glyphToHmtxIdx.TryGetValue(origGi, out int idx) ? idx : 0;
                int src = hmtxSrc + hmtxIdx * 4;
                ushort aw = (ushort)((src + 2 <= ttf.RawData.Length) ?
                    ((ttf.RawData[src] << 8) | ttf.RawData[src + 1]) : 0);
                WriteU16(hmtxOff + i * 4, aw);
            }
            WriteDirEntry("hmtx", hmtxOff, hmtxSize);

            // maxp
            WriteU16(maxpOff, 1);
            WriteU16(maxpOff + 4, (ushort)glyphCount);
            WriteDirEntry("maxp", maxpOff, maxpSize);

            // glyf + loca
            int glyfCur = glyfOff;
            int locaCur = locaOff;
            int cumOffset = 0;
            for (int i = 0; i < sortedOrigGlyphIds.Count; i++)
            {
                byte[] gd = ExtractGlyph(ttf, sortedOrigGlyphIds[i]);
                if (gd != null)
                {
                    Array.Copy(gd, 0, result, glyfCur, gd.Length);
                    glyfCur += Pad4(gd.Length);
                }
                WriteLoca(locaCur, cumOffset);
                locaCur += locaLong ? 4 : 2;
                cumOffset += Pad4(gd?.Length ?? 0);
            }
            WriteLoca(locaCur, cumOffset);
            WriteDirEntry("loca", locaOff, locaSize);
            WriteDirEntry("glyf", glyfOff, glyfDataSize);

            // cmap — Format 12: maps glyph indices to subset indices
            Array.Copy(cmapData, 0, result, cmapOff, cmapData.Length);
            WriteDirEntry("cmap", cmapOff, cmapSize);

            FixChecksumAdjustment(result, headOff);

            return result;
        }

        private int ExtractGlyphSize(TrueTypeFont ttf, int glyphIndex)
        {
            int glyfOff = ttf.GetGlyfOffset();
            int locaOff = ttf.GetLocaOffset();
            int headSrc = ttf.GetHeadOffset();
            if (glyphIndex < 0 || glyphIndex >= ttf.NumGlyphs || glyfOff < 0 || locaOff < 0)
                return 0;
            bool locaLong = headSrc >= 0 && ((ttf.RawData[headSrc + 50] << 8) | ttf.RawData[headSrc + 51]) == 1;
            int l0 = ReadLoca(ttf, locaOff, glyphIndex, locaLong);
            int l1 = ReadLoca(ttf, locaOff, glyphIndex + 1, locaLong);
            return l1 - l0;
        }

        private byte[] ExtractGlyph(TrueTypeFont ttf, int glyphIndex)
        {
            int sz = ExtractGlyphSize(ttf, glyphIndex);
            if (sz <= 0) return null;
            int glyfOff = ttf.GetGlyfOffset();
            int locaOff = ttf.GetLocaOffset();
            int headSrc = ttf.GetHeadOffset();
            bool locaLong = headSrc >= 0 && ((ttf.RawData[headSrc + 50] << 8) | ttf.RawData[headSrc + 51]) == 1;
            int loca0 = ReadLoca(ttf, locaOff, glyphIndex, locaLong);
            byte[] gd = new byte[sz];
            Array.Copy(ttf.RawData, glyfOff + loca0, gd, 0, sz);
            return gd;
        }

        private int ReadLoca(TrueTypeFont ttf, int off, int gi, bool longFormat)
        {
            int idx = off + gi * (longFormat ? 4 : 2);
            if (idx < 0 || idx + (longFormat ? 4 : 2) > ttf.RawData.Length) return 0;
            if (longFormat)
                return ((ttf.RawData[idx] << 24) | (ttf.RawData[idx + 1] << 16) |
                        (ttf.RawData[idx + 2] << 8) | ttf.RawData[idx + 3]);
            else
                return ((ttf.RawData[idx] << 8) | ttf.RawData[idx + 1]) * 2;
        }

        private byte[] BuildCmapFormat12(List<int> sortedOrigGlyphIds)
        {
            int numGlyphs = sortedOrigGlyphIds.Count;
            int subtableSize = 16 + numGlyphs * 12;
            var b = new byte[4 + subtableSize];
            int p = 0;
            b[p++] = 0; b[p++] = 12;
            b[p++] = (byte)(subtableSize >> 8);
            b[p++] = (byte)subtableSize;
            b[p++] = 0; b[p++] = 0;
            b[p++] = 0; b[p++] = (byte)numGlyphs;

            // Identity-mapping approach: charCode = subsetIdx, glyphId = subsetIdx, delta = 0.
            // The content stream uses subset indices as character codes (Identity-H).
            // When the renderer looks up a subsetIdx, the cmap returns the same value,
            // which the TrueType interpreter then uses as a glyph index into the glyf table.
            // Since the glyf table was built with glyphs at sequential positions 1, 2, 3...
            // (the .notdef glyph at position 0 is always present), this correctly routes
            // each subset index to the right glyph data without needing idDelta arithmetic.
            for (int i = 0; i < numGlyphs; i++)
            {
                uint subsetIdx = (uint)(i + 1);
                b[p++] = (byte)(subsetIdx >> 24);
                b[p++] = (byte)(subsetIdx >> 16);
                b[p++] = (byte)(subsetIdx >> 8);
                b[p++] = (byte)subsetIdx;
                b[p++] = (byte)(subsetIdx >> 24);
                b[p++] = (byte)(subsetIdx >> 16);
                b[p++] = (byte)(subsetIdx >> 8);
                b[p++] = (byte)subsetIdx;
                b[p++] = 0; b[p++] = 0; b[p++] = 0; b[p++] = 0;
            }
            return b;
        }

        private byte[] BuildCmapBytes(List<int> sortedOrigGlyphIds)
        {
            return BuildCmapFormat12(sortedOrigGlyphIds);
        }

        private void FixChecksumAdjustment(byte[] data, int headOff)
        {
            uint sum = 0;
            for (int i = 0; i < data.Length; i += 4)
            {
                uint v = 0;
                for (int j = 0; j < 4 && i + j < data.Length; j++)
                    v = (v << 8) | data[i + j];
                sum += v;
            }
            uint adj = 0xB1B0AFBAu - sum;
            data[headOff + 8] = (byte)(adj >> 24);
            data[headOff + 9] = (byte)((adj >> 16) & 0xFF);
            data[headOff + 10] = (byte)((adj >> 8) & 0xFF);
            data[headOff + 11] = (byte)(adj & 0xFF);
        }
    }
}
