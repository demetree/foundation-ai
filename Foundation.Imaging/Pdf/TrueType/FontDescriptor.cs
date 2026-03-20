using System;

namespace Foundation.Imaging.Pdf.TrueType
{
    public class FontDescriptor
    {
        public int FontObjectId { get; set; }
        public int FontDescriptorId { get; set; }
        public int ToUnicodeId { get; set; }
        public int FontStreamId { get; set; }
        public string PostScriptName { get; }
        public string BaseFont { get; }
        public bool IsCFF { get; }
        public bool IsBold { get; }
        public EmbeddedFont EmbeddedFont { get; }

        private byte[] _subsetData;
        private int _uncompressedLength;

        public FontDescriptor(EmbeddedFont font)
        {
            EmbeddedFont = font;
            PostScriptName = font.PostScriptName;
            BaseFont = font.SubsetPrefix + font.PostScriptName;
            IsCFF = font.SourceFont.IsCFF;
            IsBold = font.IsBold;
        }

        public void BuildSubset()
        {
            if (_subsetData != null) return;
            var builder = new FontSubsetBuilder(EmbeddedFont);
            _subsetData = builder.BuildSubset();
            _uncompressedLength = _subsetData?.Length ?? 0;
        }

        public string GetPdfFontDict(int fontObjectId, int fontDescId, int toUnicodeId)
        {
            string subtype = IsCFF ? "/Type1" : "/TrueType";
            string encoding = IsCFF ? "/WinAnsiEncoding" : "/Identity-H";
            return $"<< /Type /Font /Subtype {subtype} /BaseFont /{BaseFont} " +
                   $"/FirstChar 0 /LastChar 255 " +
                   $"/FontDescriptor {fontDescId} 0 R " +
                   $"/Encoding {encoding} " +
                   $"/ToUnicode {toUnicodeId} 0 R >>";
        }

        public string GetPdfFontDescriptor(int fontDescId, int fontObjectId)
        {
            var src = EmbeddedFont.SourceFont;
            int uw = src.UnitsPerEm;
            double f = 1000.0 / uw;
            int xMin = (int)(src.XMin * f);
            int yMin = (int)(src.YMin * f);
            int xMax = (int)(src.XMax * f);
            int yMax = (int)(src.YMax * f);

            return $"<< /Type /FontDescriptor /FontName /{BaseFont} " +
                   $"/Flags 4 " +
                   $"/FontBBox [{xMin} {yMin} {xMax} {yMax}] " +
                   $"/ItalicAngle 0 /Ascent {(int)(src.Ascender * f)} " +
                   $"/Descent {(int)(src.Descender * f)} " +
                   $"/CapHeight {(int)(src.Ascender * f)} " +
                   $"/StemV {(IsBold ? 120 : 80)} " +
                   $"/FontFile{(IsCFF ? "3" : "2")} {fontObjectId} 0 R >>";
        }

        public byte[] GetSubsetData() => _subsetData;
        public int GetUncompressedLength() => _uncompressedLength;
    }
}
