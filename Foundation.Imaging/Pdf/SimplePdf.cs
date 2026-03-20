using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using Foundation.Imaging.Pdf.TrueType;

namespace Foundation.Imaging.Pdf
{
    /// <summary>
    /// Minimal PDF 1.4 document writer — zero external dependencies.
    ///
    /// Supports:
    ///   - Multi-page documents with custom page sizes (with presets: A4, Letter, A3, Legal)
    ///   - Text rendering with built-in PDF fonts (Helvetica, Helvetica-Bold)
    ///   - Unicode text rendering with embedded TrueType fonts
    ///   - PNG image embedding (types 0/2/3/4/6) via FlateDecode + SMask for alpha
    ///   - JPEG image embedding via DCTDecode passthrough
    ///   - Filled rectangles, rounded rectangles, ellipses, lines
    ///   - Solid fills with alpha transparency (via ExtGState)
    ///   - Native axial gradient fills (PDF Type 2 shading)
    ///   - Clipping regions (rectangular and rounded-rectangular)
    ///   - Hyperlink annotations
    ///   - Centre/left/right text alignment with word-wrapping
    ///   - Text measurement via character-width tables
    ///   - Table rendering with configurable borders and alignment
    ///
    /// Usage:
    ///   var doc = new SimplePdfDocument("Title", "Author");
    ///   var page = doc.AddPage(SimplePdfPageSize.A4);
    ///   page.FillRect(0, 0, 595, 841, 26, 26, 46);
    ///   page.DrawText("Hello", SimplePdfFont.Bold, 32, 100, 100, 255, 255, 255);
    ///   page.DrawImage(pngBytes, 50, 200, 200, 150);
    ///   byte[] pdf = doc.Save();
    ///
    /// Relocated from BMC.LDraw.Render to Foundation.Imaging for cross-system reuse.
    /// </summary>
    public class SimplePdfDocument
    {
        private readonly string _title;
        private readonly string _author;
        private readonly List<SimplePdfPage> _pages = new List<SimplePdfPage>();

        // Object registry for PDF cross-reference table
        internal readonly List<PdfObject> Objects = new List<PdfObject>();
        internal int NextObjectId => Objects.Count + 1;

        // Font support
        private readonly FontRegistry _fontRegistry = new FontRegistry();
        private readonly List<FontDescriptor> _fontDescriptors = new List<FontDescriptor>();
        internal IReadOnlyList<FontDescriptor> FontDescriptors => _fontDescriptors;

        public SimplePdfDocument(string title, string author)
        {
            _title = title ?? "";
            _author = author ?? "";
        }

        /// <summary>Add a page with dimensions in points (72 points = 1 inch).</summary>
        public SimplePdfPage AddPage(double widthPt, double heightPt)
        {
            var page = new SimplePdfPage(this, widthPt, heightPt);
            _pages.Add(page);
            return page;
        }

        /// <summary>Add a page using a predefined page size.</summary>
        public SimplePdfPage AddPage(SimplePdfPageSize size)
        {
            var (w, h) = SimplePdfPageSizes.GetDimensions(size);
            return AddPage(w, h);
        }

        /// <summary>
        /// Register a TrueType font for use with DrawTextUnicode.
        /// Call this before drawing any text with the font.
        /// </summary>
        /// <param name="name">Friendly name to refer to the font (e.g. "MyFont").</param>
        /// <param name="fontData">Raw bytes of the .ttf or .otf file.</param>
        /// <param name="isBold">Whether to use the bold weight variant.</param>
        public void RegisterFont(string name, byte[] fontData, bool isBold = false)
        {
            var embedded = _fontRegistry.Register(name, fontData, isBold);
            _fontDescriptors.Add(new FontDescriptor(embedded));
        }

        /// <summary>
        /// Register a TrueType font from a file path.
        /// </summary>
        public void RegisterFont(string name, string filePath, bool isBold = false)
        {
            RegisterFont(name, File.ReadAllBytes(filePath), isBold);
        }

        internal FontRegistry FontRegistry => _fontRegistry;
        internal FontDescriptor GetFontDescriptor(string name)
        {
            foreach (var fd in _fontDescriptors)
            {
                if (fd.EmbeddedFont.Name == name) return fd;
            }
            return null;
        }

        internal int GetPageIndex(SimplePdfPage page) => _pages.IndexOf(page);
        internal int GetTotalPages() => _pages.Count;

        internal void GetFontObjectIds(string fontName, out int fontId, out int fontDescId, out int toUnicodeId)
        {
            foreach (var fd in _fontDescriptors)
            {
                if (fd.EmbeddedFont.Name == fontName)
                {
                    fontId = fd.FontObjectId;
                    fontDescId = fd.FontDescriptorId;
                    toUnicodeId = fd.ToUnicodeId;
                    return;
                }
            }
            fontId = fontDescId = toUnicodeId = 0;
        }

        internal void BuildFontSubsets()
        {
            foreach (var fd in _fontDescriptors)
                fd.BuildSubset();
        }

        internal void WriteFontObjects(PdfStreamWriter writer)
        {
            foreach (var fd in _fontDescriptors)
            {
                var touniMap = BuildToUnicodeMap(fd.EmbeddedFont.Name);
                byte[] touniBytes = Encoding.ASCII.GetBytes(touniMap);
                byte[] touniCompressed;
                using (var ms = new MemoryStream())
                {
                    using (var zs = new ZLibStream(ms, CompressionLevel.Fastest, leaveOpen: true))
                        zs.Write(touniBytes, 0, touniBytes.Length);
                    touniCompressed = ms.ToArray();
                }

                var src = fd.EmbeddedFont.SourceFont;
                int uw = src.UnitsPerEm;
                double f = 1000.0 / uw;

                WriteObj(writer, fd.FontDescriptorId,
                    $"<< /Type /FontDescriptor /FontName /{fd.BaseFont} " +
                    $"/Flags 4 /FontBBox [{(int)(src.XMin * f)} {(int)(src.YMin * f)} {(int)(src.XMax * f)} {(int)(src.YMax * f)}] " +
                    $"/ItalicAngle 0 /Ascent {(int)(src.Ascender * f)} " +
                    $"/Descent {(int)(src.Descender * f)} " +
                    $"/CapHeight {(int)(src.Ascender * f)} " +
                    $"/StemV {(fd.IsBold ? 120 : 80)} " +
                    $"/FontFile{(fd.IsCFF ? "3" : "2")} {fd.FontStreamId} 0 R >>");

                string subtype = fd.IsCFF ? "/Type1" : "/TrueType";
                string encoding = fd.IsCFF ? "/WinAnsiEncoding" : "/Identity-H";
                WriteObj(writer, fd.FontObjectId,
                    $"<< /Type /Font /Subtype {subtype} /BaseFont /{fd.BaseFont} " +
                    $"/FirstChar 0 /LastChar 255 " +
                    $"/FontDescriptor {fd.FontDescriptorId} 0 R " +
                    $"/Encoding {encoding} /ToUnicode {fd.ToUnicodeId} 0 R >>");

                WriteStreamObj(writer, fd.ToUnicodeId,
                    $"<< /Length {touniCompressed.Length} /Filter /FlateDecode >>",
                    touniCompressed, touniBytes.Length);

                byte[] subset = fd.GetSubsetData();
                if (subset != null && subset.Length > 0)
                {
                    WriteStreamObj(writer, fd.FontStreamId,
                        $"<< /Length {subset.Length} /Filter /FlateDecode >>",
                        subset, subset.Length);
                }
            }
        }

        private string BuildToUnicodeMap(string fontName)
        {
            var seen = new HashSet<(int unicode, int glyphIdx)>();
            var uniqueGlyphs = new List<(int unicode, int glyphIdx)>();
            foreach (var page in _pages)
            {
                if (page.TryGetGlyphTracker(fontName, out var tracker))
                {
                    foreach (var g in tracker.UsedGlyphs)
                    {
                        if (seen.Add(g))
                            uniqueGlyphs.Add(g);
                    }
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine("/CIDInit /ProcSet findresource begin");
            sb.AppendLine("12 dict begin");
            sb.AppendLine("begincmap");
            sb.AppendLine("/CIDSystemInfo << /Registry (Adobe) /Ordering (UCS) /Supplement 0 >> def");
            sb.AppendLine("/CMapName /Adobe-Identity-UCS def");
            sb.AppendLine("/CMapType 2 def");
            sb.AppendLine("1 begincodespacerange");
            sb.AppendLine("<0000> <FFFF>");
            sb.AppendLine("endcodespacerange");

            if (uniqueGlyphs.Count > 0)
            {
                // bfchar entries must be emitted in blocks of up to 100
                int remaining = uniqueGlyphs.Count;
                int idx = 0;
                while (remaining > 0)
                {
                    int blockSize = Math.Min(remaining, 100);
                    sb.AppendLine($"{blockSize} beginbfchar");
                    for (int i = 0; i < blockSize; i++)
                    {
                        var g = uniqueGlyphs[idx + i];
                        string unicodeHex = g.unicode > 0xFFFF
                            ? $"{char.ConvertToUtf32((char)0, (char)0):X4}" // BMP fallback
                            : $"{g.unicode:X4}";
                        sb.AppendLine($"<{idx + i + 1:X4}> <{unicodeHex}>");
                    }
                    sb.AppendLine("endbfchar");
                    idx += blockSize;
                    remaining -= blockSize;
                }
            }

            sb.AppendLine("endcmap");
            sb.AppendLine("CMapName currentdict /CMap defineresource pop");
            sb.AppendLine("end");
            sb.AppendLine("end");
            return sb.ToString();
        }


        /// <summary>Serialize the entire PDF document to bytes.</summary>
        public byte[] Save()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Write(ms);
                return ms.ToArray();
            }
        }


        /// <summary>Serialize the PDF and write it to a file path.</summary>
        public void SaveToFile(string filePath)
        {
            byte[] pdf = Save();
            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");
            File.WriteAllBytes(filePath, pdf);
        }


        private void Write(Stream output)
        {
            Objects.Clear();
            var writer = new PdfStreamWriter(output);

            // ── Header ──
            writer.WriteRaw("%PDF-1.4\n%\xE2\xE3\xCF\xD3\n");

            // ── Catalog (obj 1) ──
            int catalogId = AllocId();
            int pagesId = AllocId();

            // ── Info (obj 3) ──
            int infoId = AllocId();

            // ── Built-in Fonts ──
            int fontHelveticaId = AllocId();
            int fontHelveticaBoldId = AllocId();

            // ── Embedded font object IDs ──
            foreach (var fd in _fontDescriptors)
            {
                fd.FontObjectId = AllocId();
                fd.FontDescriptorId = AllocId();
                fd.ToUnicodeId = AllocId();
                fd.FontStreamId = AllocId();
            }

            // ── Finalize pages (allocate image objects, build content streams) ──
            foreach (var page in _pages)
            {
                page.Prepare(pagesId, fontHelveticaId, fontHelveticaBoldId);
            }

            // ── Build font subsets now that all glyph usage is known ──
            BuildFontSubsets();

            // ── Write objects ──

            // Catalog
            WriteObj(writer, catalogId,
                $"<< /Type /Catalog /Pages {pagesId} 0 R >>");

            // Pages
            StringBuilder kids = new StringBuilder();
            foreach (var page in _pages)
            {
                if (kids.Length > 0) kids.Append(' ');
                kids.Append($"{page.PageObjectId} 0 R");
            }
            WriteObj(writer, pagesId,
                $"<< /Type /Pages /Kids [{kids}] /Count {_pages.Count} >>");

            // Info
            WriteObj(writer, infoId,
                $"<< /Title ({PdfEscape(_title)}) /Author ({PdfEscape(_author)}) " +
                $"/Creator (Foundation.Imaging) /Producer (SimplePdf) >>");

            // Built-in Fonts
            WriteObj(writer, fontHelveticaId,
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica " +
                "/Encoding /WinAnsiEncoding >>");
            WriteObj(writer, fontHelveticaBoldId,
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold " +
                "/Encoding /WinAnsiEncoding >>");

            // Embedded fonts
            WriteFontObjects(writer);

            // Page objects + content streams + images
            foreach (var page in _pages)
            {
                page.WriteObjects(writer);
            }

            // ── Cross-reference table ──
            long xrefOffset = writer.Position;
            writer.WriteRaw($"xref\n0 {Objects.Count + 1}\n");
            writer.WriteRaw("0000000000 65535 f \n");
            foreach (var obj in Objects)
            {
                writer.WriteRaw(string.Format("{0:D10} 00000 n \n", obj.Offset));
            }

            // ── Trailer ──
            writer.WriteRaw($"trailer\n<< /Size {Objects.Count + 1} " +
                $"/Root {catalogId} 0 R /Info {infoId} 0 R >>\n");
            writer.WriteRaw($"startxref\n{xrefOffset}\n%%EOF\n");
        }


        internal int AllocId()
        {
            int id = NextObjectId;
            Objects.Add(new PdfObject { Id = id });
            return id;
        }

        internal void WriteObj(PdfStreamWriter writer, int id, string content)
        {
            Objects[id - 1].Offset = writer.Position;
            writer.WriteRaw($"{id} 0 obj\n{content}\nendobj\n");
        }

        internal void WriteStreamObj(PdfStreamWriter writer, int id,
            string dict, byte[] streamData, int uncompressedLength = 0)
        {
            Objects[id - 1].Offset = writer.Position;
            string lengthPart = uncompressedLength > 0
                ? $"/LengthKey {uncompressedLength} "
                : "";
            writer.WriteRaw($"{id} 0 obj\n{dict}\nstream\n");
            writer.WriteBytes(streamData);
            writer.WriteRaw("\nendstream\nendobj\n");
        }

        internal static string PdfEscape(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            StringBuilder sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                if (c == '\\' || c == '(' || c == ')')
                {
                    sb.Append('\\').Append(c);
                }
                else if (c < 32 || (c > 127 && c < 160))
                {
                    sb.Append('?'); // Replace problematic control / Win-1252 special chars
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        internal static string F(double v)
        {
            return v.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }


    // ═══════════════════════════════════════════════════════════════
    //  Page
    // ═══════════════════════════════════════════════════════════════

    public class SimplePdfPage
    {
        private readonly SimplePdfDocument _doc;
        public readonly double Width;
        public readonly double Height;

        internal int PageObjectId;
        private int _contentsId;
        private int _fontHelveticaId;
        private int _fontBoldId;

        // Content stream built up by drawing commands
        private readonly StringBuilder _stream = new StringBuilder();

        // Images embedded in this page
        private readonly List<PageImage> _images = new List<PageImage>();

        // ExtGState for alpha transparency
        private readonly Dictionary<byte, string> _alphaStates = new Dictionary<byte, string>();
        private int _nextAlphaStateIdx;
        private bool _alphaActive; // tracks if we're in a q/Q block for alpha

        // Link annotations
        private readonly List<PdfLinkAnnotation> _linkAnnotations = new List<PdfLinkAnnotation>();

        // Clipping depth tracking
        private int _clipDepth;

        // Shading objects for native gradients
        private readonly List<PdfShading> _shadings = new List<PdfShading>();

        // Page numbering
        private bool _addPageNumber;
        private SimplePdfFont _pageNumberFont;
        private double _pageNumberFontSize;
        private byte _pageNumberR, _pageNumberG, _pageNumberB;
        private double _pageNumberMargin;
        private bool _pageNumberRight;

        // Embedded font glyph tracking: fontName → GlyphTracker
        private readonly Dictionary<string, GlyphTracker> _glyphTrackers = new Dictionary<string, GlyphTracker>();

        internal SimplePdfPage(SimplePdfDocument doc, double width, double height)
        {
            _doc = doc;
            Width = width;
            Height = height;
        }


        // ─── Drawing API ─────────────────────────────────────────

        /// <summary>Fill a rectangle with a solid colour.</summary>
        public void FillRect(double x, double y, double w, double h,
            byte r, byte g, byte b, byte a = 255)
        {
            double py = Height - y - h;  // PDF y-axis is bottom-up
            SetFillColour(r, g, b, a);
            _stream.AppendLine($"{F(x)} {F(py)} {F(w)} {F(h)} re f");
        }


        /// <summary>Fill a rounded rectangle (approximated with arcs).</summary>
        public void FillRoundedRect(double x, double y, double w, double h,
            double radius, byte r, byte g, byte b, byte a = 255,
            byte strokeR = 0, byte strokeG = 0, byte strokeB = 0, bool stroke = false)
        {
            double py = Height - y - h;
            SetFillColour(r, g, b, a);

            if (stroke)
            {
                _stream.AppendLine($"{F(strokeR / 255.0)} {F(strokeG / 255.0)} {F(strokeB / 255.0)} RG");
                _stream.AppendLine("0.5 w");
            }

            // Rounded rect path using Bézier curves
            double k = 0.5523; // Bézier approximation of quarter circle
            double cr = Math.Min(radius, Math.Min(w / 2, h / 2));
            double ck = cr * k;

            _stream.Append($"{F(x + cr)} {F(py)} m ");
            _stream.Append($"{F(x + w - cr)} {F(py)} l ");
            _stream.Append($"{F(x + w - cr + ck)} {F(py)} {F(x + w)} {F(py + cr - ck)} {F(x + w)} {F(py + cr)} c ");
            _stream.Append($"{F(x + w)} {F(py + h - cr)} l ");
            _stream.Append($"{F(x + w)} {F(py + h - cr + ck)} {F(x + w - cr + ck)} {F(py + h)} {F(x + w - cr)} {F(py + h)} c ");
            _stream.Append($"{F(x + cr)} {F(py + h)} l ");
            _stream.Append($"{F(x + cr - ck)} {F(py + h)} {F(x)} {F(py + h - cr + ck)} {F(x)} {F(py + h - cr)} c ");
            _stream.Append($"{F(x)} {F(py + cr)} l ");
            _stream.AppendLine($"{F(x)} {F(py + cr - ck)} {F(x + cr - ck)} {F(py)} {F(x + cr)} {F(py)} c ");
            _stream.AppendLine(stroke ? "B" : "f");
        }


        /// <summary>Fill an ellipse with a solid colour.</summary>
        public void FillEllipse(double x, double y, double w, double h,
            byte r, byte g, byte b)
        {
            double cx = x + w / 2;
            double cy = Height - (y + h / 2);
            double rx = w / 2;
            double ry = h / 2;
            double k = 0.5523;

            SetFillColour(r, g, b, 255);

            _stream.Append($"{F(cx)} {F(cy + ry)} m ");
            _stream.Append($"{F(cx + rx * k)} {F(cy + ry)} {F(cx + rx)} {F(cy + ry * k)} {F(cx + rx)} {F(cy)} c ");
            _stream.Append($"{F(cx + rx)} {F(cy - ry * k)} {F(cx + rx * k)} {F(cy - ry)} {F(cx)} {F(cy - ry)} c ");
            _stream.Append($"{F(cx - rx * k)} {F(cy - ry)} {F(cx - rx)} {F(cy - ry * k)} {F(cx - rx)} {F(cy)} c ");
            _stream.AppendLine($"{F(cx - rx)} {F(cy + ry * k)} {F(cx - rx * k)} {F(cy + ry)} {F(cx)} {F(cy + ry)} c f");
        }


        /// <summary>Draw a line.</summary>
        public void DrawLine(double x1, double y1, double x2, double y2,
            byte r, byte g, byte b, double lineWidth = 1)
        {
            double py1 = Height - y1;
            double py2 = Height - y2;
            _stream.AppendLine($"{F(r / 255.0)} {F(g / 255.0)} {F(b / 255.0)} RG");
            _stream.AppendLine($"{F(lineWidth)} w");
            _stream.AppendLine($"{F(x1)} {F(py1)} m {F(x2)} {F(py2)} l S");
        }


        /// <summary>
        /// Fill a vertical gradient rectangle (top colour → bottom colour)
        /// using a native PDF Type 2 (axial) shading pattern.
        /// </summary>
        public void FillGradientRect(double x, double y, double w, double h,
            byte r1, byte g1, byte b1, byte r2, byte g2, byte b2,
            bool diagonal = false)
        {
            double py = Height - y - h;
            string shadingName = $"Sh{_shadings.Count}";

            // Axial shading: top colour at top of rect, bottom colour at bottom
            double x0 = diagonal ? x : x + w / 2;
            double y0 = py + h; // top in PDF coords
            double x1 = diagonal ? x + w : x + w / 2;
            double y1 = py;     // bottom in PDF coords

            _shadings.Add(new PdfShading
            {
                Name = shadingName,
                X0 = x0, Y0 = y0, X1 = x1, Y1 = y1,
                R0 = r1 / 255.0, G0 = g1 / 255.0, B0 = b1 / 255.0,
                R1 = r2 / 255.0, G1 = g2 / 255.0, B1 = b2 / 255.0
            });

            // Clip to the rectangle, then paint the shading
            _stream.AppendLine("q");
            _stream.AppendLine($"{F(x)} {F(py)} {F(w)} {F(h)} re W n");
            _stream.AppendLine($"/{shadingName} sh");
            _stream.AppendLine("Q");
        }


        /// <summary>
        /// Draw text at a position. Coordinates are from top-left (converted to PDF bottom-up).
        /// </summary>
        /// <param name="text">Text to draw.</param>
        /// <param name="font">Font selection.</param>
        /// <param name="fontSize">Font size in points.</param>
        /// <param name="x">X position from left edge.</param>
        /// <param name="y">Y position from top edge (baseline position).</param>
        /// <param name="r">Red (0-255).</param>
        /// <param name="g">Green (0-255).</param>
        /// <param name="b">Blue (0-255).</param>
        /// <param name="a">Alpha (0-255). Currently ignored (PDF doesn't support text alpha natively).</param>
        public void DrawText(string text, SimplePdfFont font, double fontSize,
            double x, double y, byte r, byte g, byte b, byte a = 255)
        {
            if (string.IsNullOrEmpty(text)) return;

            double py = Height - y;
            string fontRef = font == SimplePdfFont.Bold ? "/F2" : "/F1";

            _stream.AppendLine("BT");
            _stream.AppendLine($"{fontRef} {F(fontSize)} Tf");
            _stream.AppendLine($"{F(r / 255.0)} {F(g / 255.0)} {F(b / 255.0)} rg");
            _stream.AppendLine($"{F(x)} {F(py)} Td");
            _stream.AppendLine($"({SimplePdfDocument.PdfEscape(text)}) Tj");
            _stream.AppendLine("ET");
        }


        /// <summary>
        /// Draw text centred within a horizontal rectangle.
        /// </summary>
        public void DrawTextCentered(string text, SimplePdfFont font, double fontSize,
            double rectX, double rectY, double rectW,
            byte r, byte g, byte b, byte a = 255)
        {
            if (string.IsNullOrEmpty(text)) return;

            double textW = MeasureText(text, font, fontSize);
            double x = rectX + (rectW - textW) / 2;
            DrawText(text, font, fontSize, x, rectY, r, g, b, a);
        }


        /// <summary>
        /// Draw text right-aligned within a horizontal rectangle.
        /// </summary>
        public void DrawTextRight(string text, SimplePdfFont font, double fontSize,
            double rectX, double rectY, double rectW,
            byte r, byte g, byte b, byte a = 255)
        {
            if (string.IsNullOrEmpty(text)) return;

            double textW = MeasureText(text, font, fontSize);
            double x = rectX + rectW - textW;
            DrawText(text, font, fontSize, x, rectY, r, g, b, a);
        }


        /// <summary>Measure text width in points.</summary>
        public double MeasureText(string text, SimplePdfFont font, double fontSize)
        {
            if (string.IsNullOrEmpty(text)) return 0;

            double[] widths = font == SimplePdfFont.Bold
                ? HelveticaBoldWidths : HelveticaWidths;

            double total = 0;
            foreach (char c in text)
            {
                int idx = (int)c;
                if (idx >= 0 && idx < widths.Length)
                    total += widths[idx];
                else
                    total += widths[' ']; // fallback to space width
            }

            return total * fontSize / 1000.0;
        }


        /// <summary>
        /// Draw text that wraps within a bounding box. Breaks at word boundaries first,
        /// then at character boundaries if a single word is too wide.
        /// Returns the y position after the last line drawn.
        /// </summary>
        /// <param name="text">Text to draw.</param>
        /// <param name="font">Font selection.</param>
        /// <param name="fontSize">Font size in points.</param>
        /// <param name="x">Left edge of bounding box.</param>
        /// <param name="y">Top edge of bounding box (baseline of first line).</param>
        /// <param name="maxWidth">Width of the bounding box.</param>
        /// <param name="maxHeight">Height of the bounding box. Drawing stops if this is exceeded.</param>
        /// <param name="r">Red (0-255).</param>
        /// <param name="g">Green (0-255).</param>
        /// <param name="b">Blue (0-255).</param>
        /// <param name="a">Alpha (0-255).</param>
        /// <param name="lineSpacing">Multiplier for line height. Default 1.2.</param>
        /// <returns>The y position after the last line drawn (may be used to continue text below).</returns>
        public double DrawTextWrapped(string text, SimplePdfFont font, double fontSize,
            double x, double y, double maxWidth, double maxHeight,
            byte r, byte g, byte b, byte a = 255, double lineSpacing = 1.2)
        {
            if (string.IsNullOrEmpty(text) || maxWidth <= 0 || maxHeight <= 0)
                return y;

            double lineHeight = fontSize * lineSpacing;
            double currentY = y;

            string[] paragraphs = text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            foreach (string paragraph in paragraphs)
            {
                if (paragraph.Length == 0)
                {
                    currentY += lineHeight;
                    if (currentY - y >= maxHeight) break;
                    continue;
                }

                string[] words = paragraph.Split(' ');
                string currentLine = "";

                foreach (string word in words)
                {
                    string testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
                    double testWidth = MeasureText(testLine, font, fontSize);

                    if (testWidth > maxWidth)
                    {
                        if (currentLine.Length > 0)
                        {
                            DrawText(currentLine, font, fontSize, x, currentY, r, g, b, a);
                            currentY += lineHeight;
                            if (currentY - y >= maxHeight) return currentY;
                            currentLine = word;
                        }
                        else
                        {
                            string remaining = word;
                            while (remaining.Length > 0)
                            {
                                int charsFitting = 0;
                                double widthSoFar = 0;
                                double[] charWidths = font == SimplePdfFont.Bold ? HelveticaBoldWidths : HelveticaWidths;
                                for (int i = 0; i < remaining.Length; i++)
                                {
                                    int idx = (int)remaining[i];
                                    double cw = (idx >= 0 && idx < charWidths.Length) ? charWidths[idx] : charWidths[' '];
                                    double nextWidth = widthSoFar + cw * fontSize / 1000.0;
                                    if (nextWidth > maxWidth)
                                    {
                                        break;
                                    }
                                    widthSoFar = nextWidth;
                                    charsFitting = i + 1;
                                }

                                if (charsFitting == 0) break;

                                DrawText(remaining.Substring(0, charsFitting), font, fontSize, x, currentY, r, g, b, a);
                                currentY += lineHeight;
                                if (currentY - y >= maxHeight) return currentY;
                                remaining = remaining.Substring(charsFitting);
                            }
                            currentLine = "";
                            continue;
                        }
                    }
                    else
                    {
                        currentLine = testLine;
                    }
                }

                if (currentLine.Length > 0)
                {
                    DrawText(currentLine, font, fontSize, x, currentY, r, g, b, a);
                    currentY += lineHeight;
                    if (currentY - y >= maxHeight) return currentY;
                }
            }

            return currentY;
        }


        /// <summary>
        /// Draw Unicode text using an embedded TrueType font registered via RegisterFont().
        /// Uses Identity-H encoding — glyph indices are assigned sequentially per page.
        /// </summary>
        public double DrawTextUnicode(string text, string fontName, double fontSize,
            double x, double y, byte r, byte g, byte b, byte a = 255,
            double maxWidth = double.MaxValue)
        {
            if (string.IsNullOrEmpty(text)) return y;

            if (!_glyphTrackers.TryGetValue(fontName, out var tracker))
            {
                var fd = _doc.GetFontDescriptor(fontName);
                if (fd == null)
                    throw new InvalidOperationException($"Font '{fontName}' is not registered. Call RegisterFont() first.");
                tracker = new GlyphTracker(fontName, fd.EmbeddedFont);
                _glyphTrackers[fontName] = tracker;
            }

            double py = Height - y;
            double lineH = fontSize * 1.2;
            double curY = y;

            _stream.AppendLine("BT");
            int fdIdx = 3;
            for (int fi = 0; fi < _doc.FontDescriptors.Count; fi++)
            {
                if (_doc.FontDescriptors[fi].EmbeddedFont.Name == fontName)
                {
                    fdIdx = fi + 3;
                    break;
                }
            }
            _stream.AppendLine($"/F{fdIdx} {F(fontSize)} Tf");
            _stream.AppendLine($"{F(r / 255.0)} {F(g / 255.0)} {F(b / 255.0)} rg");

            string[] paras = text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            foreach (string para in paras)
            {
                if (para.Length == 0)
                {
                    curY += lineH;
                    py = Height - curY;
                    continue;
                }

                string[] words = para.Split(' ');
                string line = "";
                foreach (string word in words)
                {
                    string test = line.Length == 0 ? word : line + " " + word;
                    if (tracker.Font.MeasureText(test, fontSize) > maxWidth)
                    {
                        if (line.Length > 0)
                        {
                            WriteUnicodeLine(line, tracker, x, py);
                            curY += lineH;
                            py = Height - curY;
                            line = word;
                        }
                        else
                        {
                            WriteUnicodeLine(word, tracker, x, py);
                            curY += lineH;
                            py = Height - curY;
                            continue;
                        }
                    }
                    else
                    {
                        line = test;
                    }
                }
                if (line.Length > 0)
                {
                    WriteUnicodeLine(line, tracker, x, py);
                    curY += lineH;
                    py = Height - curY;
                }
            }
            _stream.AppendLine("ET");
            return curY;
        }

        private void WriteUnicodeLine(string text, GlyphTracker tracker, double x, double y)
        {
            tracker.RecordTextRun(text);

            _stream.AppendLine($"{F(x)} {F(y)} Td");
            _stream.Append("[");

            int i = 0;
            bool first = true;
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
                int gi = tracker.Font.GetGlyphIndex(cp);
                int subsetIdx = tracker.RecordGlyphUsage(gi);
                if (!first) _stream.Append(" ");
                first = false;
                if (subsetIdx > 0)
                    _stream.Append(subsetIdx.ToString());
                else
                    _stream.Append("0");
            }
            _stream.Append("] TJ");
        }

        /// <summary>
        /// Measure the width of Unicode text in a registered embedded font.
        /// </summary>
        public double MeasureTextUnicode(string text, string fontName, double fontSize)
        {
            var fd = _doc.GetFontDescriptor(fontName);
            return fd?.EmbeddedFont.MeasureText(text, fontSize) ?? 0;
        }

        /// <summary>
        /// Expose glyph tracker to the document for building ToUnicode maps.
        /// </summary>
        internal bool TryGetGlyphTracker(string fontName, out GlyphTracker tracker)
            => _glyphTrackers.TryGetValue(fontName, out tracker);


        /// <summary>
        /// Draw a table starting at the given position. Returns the y position after the table.
        /// </summary>
        /// <param name="columns">Column widths from left to right. Must sum to the intended table width.</param>
        /// <param name="data">2D array of cell text. First row is the header row.</param>
        /// <param name="x">Left edge of the table.</param>
        /// <param name="y">Top edge of the table (first row baseline).</param>
        /// <param name="rowHeight">Fixed row height in points. Default 18.</param>
        /// <param name="fontSize">Font size for cell text. Default 9.</param>
        /// <param name="border">Border style. Default Outer.</param>
        /// <param name="headerBgR">Header background red (0-255). Default 200.</param>
        /// <param name="headerBgG">Header background green.</param>
        /// <param name="headerBgB">Header background blue.</param>
        /// <param name="textR">Cell text red. Default 0 (black).</param>
        /// <param name="textG">Cell text green.</param>
        /// <param name="textB">Cell text blue.</param>
        /// <param name="alternateRowBg">If true, odd rows get a light gray background. Default true.</param>
        /// <param name="altBgR">Alternate row background red. Default 245.</param>
        /// <param name="altBgG">Alternate row background green.</param>
        /// <param name="altBgB">Alternate row background blue.</param>
        /// <param name="headerTextR">Header text red. Default 255 (white).</param>
        /// <param name="headerTextG">Header text green.</param>
        /// <param name="headerTextB">Header text blue.</param>
        /// <param name="cellPadding">Horizontal padding inside each cell. Default 4.</param>
        /// <param name="cellAlign">Default alignment for data cells. Default Left.</param>
        /// <param name="headerAlign">Alignment for header cells. Default Left.</param>
        /// <returns>The y position after the last row of the table.</returns>
        public double DrawTable(double[] columns, string[][] data,
            double x, double y,
            double rowHeight = 18, double fontSize = 9,
            SimplePdfTableBorderStyle border = SimplePdfTableBorderStyle.Outer,
            byte headerBgR = 200, byte headerBgG = 200, byte headerBgB = 200,
            byte textR = 0, byte textG = 0, byte textB = 0,
            bool alternateRowBg = true,
            byte altBgR = 245, byte altBgG = 245, byte altBgB = 245,
            byte headerTextR = 255, byte headerTextG = 255, byte headerTextB = 255,
            double cellPadding = 4,
            SimplePdfCellAlign cellAlign = SimplePdfCellAlign.Left,
            SimplePdfCellAlign headerAlign = SimplePdfCellAlign.Left)
        {
            if (columns == null || columns.Length == 0 || data == null || data.Length == 0)
                return y;

            int rowCount = data.Length;
            int colCount = columns.Length;
            double tableWidth = 0;
            foreach (double c in columns) tableWidth += c;
            double currentY = y;
            byte lineGray = 180;

            for (int row = 0; row < rowCount; row++)
            {
                bool isHeader = row == 0;
                bool isAlternate = !isHeader && alternateRowBg && (row % 2 == 0);

                double rowTop = currentY + rowHeight - fontSize * 0.85;
                double rowBottom = currentY;

                // ── Background fill ──
                if (isHeader)
                {
                    FillRect(x, currentY, tableWidth, rowHeight, headerBgR, headerBgG, headerBgB);
                }
                else if (isAlternate)
                {
                    FillRect(x, currentY, tableWidth, rowHeight, altBgR, altBgG, altBgB);
                }

                // ── Cell text ──
                double colX = x;
                for (int col = 0; col < colCount; col++)
                {
                    double colW = columns[col];
                    string cellText = data[row].Length > col ? (data[row][col] ?? "") : "";

                    byte tR = isHeader ? headerTextR : textR;
                    byte tG = isHeader ? headerTextG : textG;
                    byte tB = isHeader ? headerTextB : textB;
                    SimplePdfFont cellFont = isHeader ? SimplePdfFont.Bold : SimplePdfFont.Regular;
                    SimplePdfCellAlign align = isHeader ? headerAlign : cellAlign;

                    double textX = colX + cellPadding;
                    double availW = colW - cellPadding * 2;

                    if (align == SimplePdfCellAlign.Right)
                    {
                        double tw = MeasureText(cellText, cellFont, fontSize);
                        textX = colX + colW - cellPadding - tw;
                    }
                    else if (align == SimplePdfCellAlign.Center)
                    {
                        double tw = MeasureText(cellText, cellFont, fontSize);
                        textX = colX + (colW - tw) / 2;
                    }

                    if (availW > 0)
                    {
                        DrawTextWrapped(cellText, cellFont, fontSize,
                            textX, rowTop, availW, rowHeight,
                            tR, tG, tB);
                    }

                    colX += colW;
                }

                // ── Borders ──
                if (border == SimplePdfTableBorderStyle.All)
                {
                    // Draw horizontal separator after every row
                    DrawLine(x, rowBottom, x + tableWidth, rowBottom, lineGray, lineGray, lineGray, 0.3);
                }
                else if (border == SimplePdfTableBorderStyle.HeaderOnly && isHeader)
                {
                    // Draw separator only below the header row
                    DrawLine(x, currentY + rowHeight, x + tableWidth, currentY + rowHeight, lineGray, lineGray, lineGray, 0.5);
                }
                // Outer: no internal row separators (outer border drawn separately below)

                // Vertical borders (All style only)
                if (border == SimplePdfTableBorderStyle.All)
                {
                    colX = x;
                    for (int col = 0; col <= colCount; col++)
                    {
                        DrawLine(colX, rowBottom, colX, rowBottom + rowHeight, lineGray, lineGray, lineGray, 0.3);
                        if (col < colCount) colX += columns[col];
                    }
                }

                currentY += rowHeight;
            }

            // ── Outer border ──
            if (border == SimplePdfTableBorderStyle.Outer || border == SimplePdfTableBorderStyle.All)
            {
                double tableTop = y;
                double tableBottom = currentY;

                DrawLine(x, tableTop, x + tableWidth, tableTop, lineGray, lineGray, lineGray, 0.5);
                DrawLine(x, tableBottom, x + tableWidth, tableBottom, lineGray, lineGray, lineGray, 0.5);
                DrawLine(x, tableTop, x, tableBottom, lineGray, lineGray, lineGray, 0.5);
                DrawLine(x + tableWidth, tableTop, x + tableWidth, tableBottom, lineGray, lineGray, lineGray, 0.5);
            }

            return currentY;
        }
        public void DrawImage(byte[] pngBytes, double x, double y, double w, double h)
        {
            if (pngBytes == null || pngBytes.Length == 0) return;

            string imgName = $"Im{_images.Count}";
            _images.Add(new PageImage
            {
                Name = imgName,
                PngData = pngBytes
            });

            double py = Height - y - h;
            _stream.AppendLine("q");
            _stream.AppendLine($"{F(w)} 0 0 {F(h)} {F(x)} {F(py)} cm");
            _stream.AppendLine($"/{imgName} Do");
            _stream.AppendLine("Q");
        }


        /// <summary>
        /// Embed a JPEG image directly using DCTDecode (no re-encoding).
        /// </summary>
        public void DrawJpegImage(byte[] jpegBytes, double x, double y, double w, double h)
        {
            if (jpegBytes == null || jpegBytes.Length == 0) return;

            string imgName = $"Im{_images.Count}";
            _images.Add(new PageImage
            {
                Name = imgName,
                JpegData = jpegBytes
            });

            double py = Height - y - h;
            _stream.AppendLine("q");
            _stream.AppendLine($"{F(w)} 0 0 {F(h)} {F(x)} {F(py)} cm");
            _stream.AppendLine($"/{imgName} Do");
            _stream.AppendLine("Q");
        }


        // ─── Clipping API ─────────────────────────────────────

        /// <summary>
        /// Push a rectangular clipping region. Must be paired with PopClip().
        /// </summary>
        public void PushClipRect(double x, double y, double w, double h)
        {
            double py = Height - y - h;
            _stream.AppendLine("q");
            _stream.AppendLine($"{F(x)} {F(py)} {F(w)} {F(h)} re W n");
            _clipDepth++;
        }

        /// <summary>
        /// Push a rounded-rectangle clipping region. Must be paired with PopClip().
        /// </summary>
        public void PushClipRoundedRect(double x, double y, double w, double h, double radius)
        {
            double py = Height - y - h;
            double k = 0.5523;
            double cr = Math.Min(radius, Math.Min(w / 2, h / 2));
            double ck = cr * k;

            _stream.AppendLine("q");
            _stream.Append($"{F(x + cr)} {F(py)} m ");
            _stream.Append($"{F(x + w - cr)} {F(py)} l ");
            _stream.Append($"{F(x + w - cr + ck)} {F(py)} {F(x + w)} {F(py + cr - ck)} {F(x + w)} {F(py + cr)} c ");
            _stream.Append($"{F(x + w)} {F(py + h - cr)} l ");
            _stream.Append($"{F(x + w)} {F(py + h - cr + ck)} {F(x + w - cr + ck)} {F(py + h)} {F(x + w - cr)} {F(py + h)} c ");
            _stream.Append($"{F(x + cr)} {F(py + h)} l ");
            _stream.Append($"{F(x + cr - ck)} {F(py + h)} {F(x)} {F(py + h - cr + ck)} {F(x)} {F(py + h - cr)} c ");
            _stream.Append($"{F(x)} {F(py + cr)} l ");
            _stream.AppendLine($"{F(x)} {F(py + cr - ck)} {F(x + cr - ck)} {F(py)} {F(x + cr)} {F(py)} c ");
            _stream.AppendLine("W n");
            _clipDepth++;
        }

        /// <summary>
        /// Pop the most recent clipping region.
        /// </summary>
        public void PopClip()
        {
            if (_clipDepth <= 0)
                throw new InvalidOperationException("No clipping region to pop. Mismatched PushClip/PopClip calls.");
            _stream.AppendLine("Q");
            _clipDepth--;
        }


        // ─── Hyperlink Annotations ───────────────────────────

        /// <summary>
        /// Add a clickable URL link annotation over a rectangular area.
        /// Coordinates use the same top-left origin as other drawing methods.
        /// </summary>
        public void AddLinkAnnotation(double x, double y, double w, double h, string url)
        {
            double py = Height - y - h;
            _linkAnnotations.Add(new PdfLinkAnnotation
            {
                X = x, Y = py, W = w, H = h, Url = url
            });
        }


        /// <summary>
        /// Add page numbering to this page's footer. Call after all other drawing on the page.
        /// Format: "1 of N" right-aligned, or "N" centred.
        /// </summary>
        /// <param name="font">Font for the page number.</param>
        /// <param name="fontSize">Font size in points.</param>
        /// <param name="margin">Distance from the bottom edge. Default 20pt.</param>
        /// <param name="r">Red (0-255).</param>
        /// <param name="g">Green (0-255).</param>
        /// <param name="b">Blue (0-255).</param>
        /// <param name="rightAlign">If true, show "Page N of Total". If false, show just "N".</param>
        public void AddPageNumber(SimplePdfFont font = SimplePdfFont.Regular, double fontSize = 8,
            double margin = 20, byte r = 100, byte g = 100, byte b = 100, bool rightAlign = true)
        {
            _addPageNumber = true;
            _pageNumberFont = font;
            _pageNumberFontSize = fontSize;
            _pageNumberR = r;
            _pageNumberG = g;
            _pageNumberB = b;
            _pageNumberMargin = margin;
            _pageNumberRight = rightAlign;
        }


        /// <summary>
        /// Get the pixel dimensions of a PNG image from its header.
        /// Returns (width, height).
        /// </summary>
        public static (int width, int height) GetPngDimensions(byte[] pngBytes)
        {
            if (pngBytes == null || pngBytes.Length < 24)
                return (0, 0);

            // PNG IHDR chunk starts at byte 16: width (4 bytes BE), height (4 bytes BE)
            int w = (pngBytes[16] << 24) | (pngBytes[17] << 16) | (pngBytes[18] << 8) | pngBytes[19];
            int h = (pngBytes[20] << 24) | (pngBytes[21] << 16) | (pngBytes[22] << 8) | pngBytes[23];
            return (w, h);
        }


        // ─── Internal ────────────────────────────────────────────

        private void AppendPageNumber()
        {
            if (!_addPageNumber) return;

            int pageIndex = _doc.GetPageIndex(this) + 1;
            int totalPages = _doc.GetTotalPages();

            double baselineY = _pageNumberMargin;
            string fontRef = _pageNumberFont == SimplePdfFont.Bold ? "/F2" : "/F1";

            if (_pageNumberRight)
            {
                string label = $"Page {pageIndex} of {totalPages}";
                double labelWidth = MeasureText(label, _pageNumberFont, _pageNumberFontSize);
                double x = Width - labelWidth - 20;

                _stream.AppendLine("BT");
                _stream.AppendLine($"{fontRef} {_pageNumberFontSize} Tf");
                _stream.AppendLine($"{F(_pageNumberR / 255.0)} {F(_pageNumberG / 255.0)} {F(_pageNumberB / 255.0)} rg");
                _stream.AppendLine($"{F(x)} {F(baselineY)} Td");
                _stream.AppendLine($"({SimplePdfDocument.PdfEscape(label)}) Tj");
                _stream.AppendLine("ET");
            }
            else
            {
                string label = pageIndex.ToString();
                double labelWidth = MeasureText(label, _pageNumberFont, _pageNumberFontSize);
                double x = (Width - labelWidth) / 2;

                _stream.AppendLine("BT");
                _stream.AppendLine($"{fontRef} {_pageNumberFontSize} Tf");
                _stream.AppendLine($"{F(_pageNumberR / 255.0)} {F(_pageNumberG / 255.0)} {F(_pageNumberB / 255.0)} rg");
                _stream.AppendLine($"{F(x)} {F(baselineY)} Td");
                _stream.AppendLine($"({SimplePdfDocument.PdfEscape(label)}) Tj");
                _stream.AppendLine("ET");
            }
        }

        internal void Prepare(int pagesId, int fontHelveticaId, int fontBoldId)
        {
            _fontHelveticaId = fontHelveticaId;
            _fontBoldId = fontBoldId;

            PageObjectId = _doc.AllocId();
            _contentsId = _doc.AllocId();

            // Allocate image object IDs
            foreach (var img in _images)
            {
                img.ImageObjectId = _doc.AllocId();
                if (img.PngData != null)
                    img.SmaskObjectId = _doc.AllocId();
            }

            // Allocate shading object IDs
            foreach (var shading in _shadings)
            {
                shading.ObjectId = _doc.AllocId();
                shading.FunctionId = _doc.AllocId();
            }

            // Allocate annotation object IDs
            foreach (var annot in _linkAnnotations)
            {
                annot.ObjectId = _doc.AllocId();
            }

            // Inject page numbering into the content stream
            AppendPageNumber();
        }


        internal void WriteObjects(PdfStreamWriter writer)
        {
            // ── Content stream ──
            byte[] contentBytes = Encoding.GetEncoding(1252).GetBytes(_stream.ToString());
            byte[] compressed = Deflate(contentBytes);

            _doc.WriteStreamObj(writer, _contentsId,
                $"<< /Length {compressed.Length} /Filter /FlateDecode >>",
                compressed, contentBytes.Length);

            // ── Image XObjects ──
            StringBuilder xobjectEntries = new StringBuilder();
            foreach (var img in _images)
            {
                if (img.JpegData != null)
                    WriteJpegImageObject(writer, img);
                else
                    WriteImageObject(writer, img);
                if (xobjectEntries.Length > 0) xobjectEntries.Append(' ');
                xobjectEntries.Append($"/{img.Name} {img.ImageObjectId} 0 R");
            }

            // ── Shading objects ──
            StringBuilder shadingEntries = new StringBuilder();
            foreach (var sh in _shadings)
            {
                // Function (Type 2 exponential interpolation)
                _doc.WriteObj(writer, sh.FunctionId,
                    $"<< /FunctionType 2 /Domain [0 1] /N 1 " +
                    $"/C0 [{F(sh.R0)} {F(sh.G0)} {F(sh.B0)}] " +
                    $"/C1 [{F(sh.R1)} {F(sh.G1)} {F(sh.B1)}] >>");

                // Shading dictionary
                _doc.WriteObj(writer, sh.ObjectId,
                    $"<< /ShadingType 2 /ColorSpace /DeviceRGB " +
                    $"/Coords [{F(sh.X0)} {F(sh.Y0)} {F(sh.X1)} {F(sh.Y1)}] " +
                    $"/Function {sh.FunctionId} 0 R /Extend [true true] >>");

                if (shadingEntries.Length > 0) shadingEntries.Append(' ');
                shadingEntries.Append($"/{sh.Name} {sh.ObjectId} 0 R");
            }

            // ── Annotation objects ──
            StringBuilder annotRefs = new StringBuilder();
            foreach (var annot in _linkAnnotations)
            {
                _doc.WriteObj(writer, annot.ObjectId,
                    $"<< /Type /Annot /Subtype /Link " +
                    $"/Rect [{F(annot.X)} {F(annot.Y)} {F(annot.X + annot.W)} {F(annot.Y + annot.H)}] " +
                    $"/Border [0 0 0] " +
                    $"/A << /Type /Action /S /URI /URI ({SimplePdfDocument.PdfEscape(annot.Url)}) >> >>");
                if (annotRefs.Length > 0) annotRefs.Append(' ');
                annotRefs.Append($"{annot.ObjectId} 0 R");
            }

            // ── Page object ──
            StringBuilder fontEntries = new StringBuilder();
            fontEntries.Append($"/F1 {_fontHelveticaId} 0 R /F2 {_fontBoldId} 0 R");
            int fdIdx = 3;
            foreach (var fd in _doc.FontDescriptors)
            {
                if (_glyphTrackers.ContainsKey(fd.EmbeddedFont.Name))
                {
                    fontEntries.Append($" /F{fdIdx} {fd.FontObjectId} 0 R");
                }
                fdIdx++;
            }

            // Build ExtGState entries for alpha transparency
            StringBuilder gsEntries = new StringBuilder();
            foreach (var kvp in _alphaStates)
            {
                double alpha = kvp.Key / 255.0;
                gsEntries.Append($" /{kvp.Value} << /ca {F(alpha)} /CA {F(alpha)} >>");
            }

            string resources = $"<< /Font << {fontEntries} >>";
            if (xobjectEntries.Length > 0)
                resources += $" /XObject << {xobjectEntries} >>";
            if (gsEntries.Length > 0)
                resources += $" /ExtGState <<{gsEntries} >>";
            if (shadingEntries.Length > 0)
                resources += $" /Shading << {shadingEntries} >>";
            resources += " >>";

            string annotsStr = annotRefs.Length > 0
                ? $" /Annots [{annotRefs}]"
                : "";

            _doc.WriteObj(writer, PageObjectId,
                $"<< /Type /Page /Parent {_doc.Objects[1].Id} 0 R " +
                $"/MediaBox [0 0 {F(Width)} {F(Height)}] " +
                $"/Contents {_contentsId} 0 R " +
                $"/Resources {resources}{annotsStr} >>");
        }


        /// <summary>
        /// Parse a PNG, extract raw RGB and alpha data, and write as
        /// a FlateDecode image XObject with an SMask for transparency.
        /// </summary>
        private void WriteImageObject(PdfStreamWriter writer, PageImage img)
        {
            // Parse PNG to get raw RGBA pixels
            int imgW, imgH;
            byte[] rgbaPixels;
            DecodePng(img.PngData, out imgW, out imgH, out rgbaPixels);

            // Split into RGB and alpha channels
            int pixelCount = imgW * imgH;
            byte[] rgb = new byte[pixelCount * 3];
            byte[] alpha = new byte[pixelCount];
            bool hasAlpha = false;

            for (int i = 0; i < pixelCount; i++)
            {
                rgb[i * 3 + 0] = rgbaPixels[i * 4 + 0];
                rgb[i * 3 + 1] = rgbaPixels[i * 4 + 1];
                rgb[i * 3 + 2] = rgbaPixels[i * 4 + 2];
                alpha[i] = rgbaPixels[i * 4 + 3];
                if (alpha[i] != 255) hasAlpha = true;
            }

            // SMask (alpha channel)
            byte[] compressedAlpha = Deflate(alpha);
            _doc.WriteStreamObj(writer, img.SmaskObjectId,
                $"<< /Type /XObject /Subtype /Image /Width {imgW} /Height {imgH} " +
                $"/ColorSpace /DeviceGray /BitsPerComponent 8 " +
                $"/Length {compressedAlpha.Length} /Filter /FlateDecode >>",
                compressedAlpha, alpha.Length);

            // Image
            byte[] compressedRgb = Deflate(rgb);
            string smaskRef = hasAlpha ? $" /SMask {img.SmaskObjectId} 0 R" : "";
            _doc.WriteStreamObj(writer, img.ImageObjectId,
                $"<< /Type /XObject /Subtype /Image /Width {imgW} /Height {imgH} " +
                $"/ColorSpace /DeviceRGB /BitsPerComponent 8 " +
                $"/Length {compressedRgb.Length} /Filter /FlateDecode{smaskRef} >>",
                compressedRgb, rgb.Length);
        }


        /// <summary>
        /// Write a JPEG image as a DCTDecode XObject (raw passthrough — no re-encoding).
        /// </summary>
        private void WriteJpegImageObject(PdfStreamWriter writer, PageImage img)
        {
            var (imgW, imgH, components) = ParseJpegDimensions(img.JpegData);
            string colorSpace = components == 1 ? "/DeviceGray" :
                                components == 4 ? "/DeviceCMYK" : "/DeviceRGB";

            _doc.WriteStreamObj(writer, img.ImageObjectId,
                $"<< /Type /XObject /Subtype /Image /Width {imgW} /Height {imgH} " +
                $"/ColorSpace {colorSpace} /BitsPerComponent 8 " +
                $"/Length {img.JpegData.Length} /Filter /DCTDecode >>",
                img.JpegData, img.JpegData.Length);
        }

        /// <summary>
        /// Parse JPEG SOF0/SOF2 marker to extract width, height, and component count.
        /// </summary>
        private static (int width, int height, int components) ParseJpegDimensions(byte[] jpeg)
        {
            int pos = 2; // skip SOI marker (FF D8)
            while (pos < jpeg.Length - 1)
            {
                if (jpeg[pos] != 0xFF) break;
                byte marker = jpeg[pos + 1];
                pos += 2;

                // SOF0 (baseline), SOF1 (extended), SOF2 (progressive)
                if (marker == 0xC0 || marker == 0xC1 || marker == 0xC2)
                {
                    int height = (jpeg[pos + 3] << 8) | jpeg[pos + 4];
                    int width = (jpeg[pos + 5] << 8) | jpeg[pos + 6];
                    int components = jpeg[pos + 7];
                    return (width, height, components);
                }

                // Skip this marker segment
                int segLen = (jpeg[pos] << 8) | jpeg[pos + 1];
                pos += segLen;
            }
            return (0, 0, 3); // fallback
        }


        // ─── PNG Decoder (minimal, for re-extracting pixels) ─────

        /// <summary>
        /// Decode a PNG file into raw RGBA pixels.
        /// Handles colour types: 0 (grayscale), 2 (RGB), 3 (indexed/palette),
        /// 4 (grayscale+alpha), 6 (RGBA). Supports 8-bit depth.
        /// Supports filter types 0 (None), 1 (Sub), 2 (Up), 3 (Average), 4 (Paeth).
        /// </summary>
        private static void DecodePng(byte[] png, out int width, out int height, out byte[] rgba)
        {
            // IHDR at offset 16
            width = ReadBE32(png, 16);
            height = ReadBE32(png, 20);
            int bitDepth = png[24];
            int colourType = png[25];

            // Determine channels and bytes-per-pixel
            int channels;
            switch (colourType)
            {
                case 0: channels = 1; break; // grayscale
                case 2: channels = 3; break; // RGB
                case 3: channels = 1; break; // indexed (1 byte per pixel = palette index)
                case 4: channels = 2; break; // grayscale + alpha
                case 6: channels = 4; break; // RGBA
                default: channels = 4; break;
            }
            int bpp = channels * (bitDepth / 8);

            // Collect PLTE chunk for indexed colour
            byte[] plteData = null;
            if (colourType == 3)
                plteData = ExtractChunkData(png, "PLTE");

            // Collect tRNS chunk
            byte[] trnsData = ExtractChunkData(png, "tRNS");

            // Collect all IDAT chunks
            using (MemoryStream idatStream = new MemoryStream())
            {
                int pos = 8;
                while (pos < png.Length)
                {
                    int chunkLen = ReadBE32(png, pos);
                    string chunkType = Encoding.ASCII.GetString(png, pos + 4, 4);
                    if (chunkType == "IDAT")
                    {
                        idatStream.Write(png, pos + 8, chunkLen);
                    }
                    pos += 12 + chunkLen;
                }

                // Decompress
                idatStream.Position = 0;
                byte[] rawScanlines;
                using (ZLibStream zlib = new ZLibStream(idatStream, CompressionMode.Decompress))
                using (MemoryStream decompressed = new MemoryStream())
                {
                    zlib.CopyTo(decompressed);
                    rawScanlines = decompressed.ToArray();
                }

                // Unfilter scanlines
                int stride = width * bpp;
                rgba = new byte[width * height * 4];

                byte[] prevRow = new byte[stride];
                int srcPos = 0;

                for (int y = 0; y < height; y++)
                {
                    byte filterType = rawScanlines[srcPos++];
                    byte[] currRow = new byte[stride];
                    Array.Copy(rawScanlines, srcPos, currRow, 0, stride);

                    // Apply filter
                    for (int x = 0; x < stride; x++)
                    {
                        byte a = x >= bpp ? currRow[x - bpp] : (byte)0;
                        byte b = prevRow[x];
                        byte c = x >= bpp ? prevRow[x - bpp] : (byte)0;

                        switch (filterType)
                        {
                            case 1: currRow[x] = (byte)(currRow[x] + a); break;
                            case 2: currRow[x] = (byte)(currRow[x] + b); break;
                            case 3: currRow[x] = (byte)(currRow[x] + (a + b) / 2); break;
                            case 4: currRow[x] = (byte)(currRow[x] + PaethPredictor(a, b, c)); break;
                        }
                    }

                    // Copy to RGBA output based on colour type
                    for (int x = 0; x < width; x++)
                    {
                        int dstOff = (y * width + x) * 4;
                        switch (colourType)
                        {
                            case 0: // Grayscale
                            {
                                byte g = currRow[x];
                                rgba[dstOff + 0] = g;
                                rgba[dstOff + 1] = g;
                                rgba[dstOff + 2] = g;
                                // tRNS for type 0: 2-byte gray value
                                if (trnsData != null && trnsData.Length >= 2)
                                {
                                    int trnsGray = (trnsData[0] << 8) | trnsData[1];
                                    rgba[dstOff + 3] = (g == trnsGray) ? (byte)0 : (byte)255;
                                }
                                else
                                    rgba[dstOff + 3] = 255;
                                break;
                            }
                            case 2: // RGB
                            {
                                byte pr = currRow[x * 3 + 0];
                                byte pg = currRow[x * 3 + 1];
                                byte pb = currRow[x * 3 + 2];
                                rgba[dstOff + 0] = pr;
                                rgba[dstOff + 1] = pg;
                                rgba[dstOff + 2] = pb;
                                if (trnsData != null && trnsData.Length >= 6)
                                {
                                    int trnsR = (trnsData[0] << 8) | trnsData[1];
                                    int trnsG = (trnsData[2] << 8) | trnsData[3];
                                    int trnsB = (trnsData[4] << 8) | trnsData[5];
                                    rgba[dstOff + 3] = (pr == trnsR && pg == trnsG && pb == trnsB)
                                        ? (byte)0 : (byte)255;
                                }
                                else
                                    rgba[dstOff + 3] = 255;
                                break;
                            }
                            case 3: // Indexed (palette)
                            {
                                int palIdx = currRow[x];
                                if (plteData != null && palIdx * 3 + 2 < plteData.Length)
                                {
                                    rgba[dstOff + 0] = plteData[palIdx * 3 + 0];
                                    rgba[dstOff + 1] = plteData[palIdx * 3 + 1];
                                    rgba[dstOff + 2] = plteData[palIdx * 3 + 2];
                                }
                                // tRNS for type 3: one alpha byte per palette entry
                                rgba[dstOff + 3] = (trnsData != null && palIdx < trnsData.Length)
                                    ? trnsData[palIdx] : (byte)255;
                                break;
                            }
                            case 4: // Grayscale + Alpha
                            {
                                byte g = currRow[x * 2 + 0];
                                rgba[dstOff + 0] = g;
                                rgba[dstOff + 1] = g;
                                rgba[dstOff + 2] = g;
                                rgba[dstOff + 3] = currRow[x * 2 + 1];
                                break;
                            }
                            case 6: // RGBA
                            default:
                            {
                                rgba[dstOff + 0] = currRow[x * 4 + 0];
                                rgba[dstOff + 1] = currRow[x * 4 + 1];
                                rgba[dstOff + 2] = currRow[x * 4 + 2];
                                rgba[dstOff + 3] = currRow[x * 4 + 3];
                                break;
                            }
                        }
                    }

                    prevRow = currRow;
                    srcPos += stride;
                }
            }
        }

        private static byte[] ExtractChunkData(byte[] png, string chunkName)
        {
            int pos = 8;
            while (pos < png.Length)
            {
                int chunkLen = ReadBE32(png, pos);
                string chunkType = Encoding.ASCII.GetString(png, pos + 4, 4);
                if (chunkType == chunkName)
                {
                    byte[] data = new byte[chunkLen];
                    Array.Copy(png, pos + 8, data, 0, chunkLen);
                    return data;
                }
                pos += 12 + chunkLen;
            }
            return null;
        }

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

        private static int ReadBE32(byte[] data, int offset)
        {
            return (data[offset] << 24) | (data[offset + 1] << 16) |
                   (data[offset + 2] << 8) | data[offset + 3];
        }


        // ─── Helpers ─────────────────────────────────────────────

        private void SetFillColour(byte r, byte g, byte b, byte a)
        {
            if (a < 255)
            {
                if (!_alphaActive)
                {
                    _stream.AppendLine("q"); // save graphics state before alpha change
                    _alphaActive = true;
                }
                if (!_alphaStates.TryGetValue(a, out string gsName))
                {
                    gsName = $"GS{_nextAlphaStateIdx++}";
                    _alphaStates[a] = gsName;
                }
                _stream.AppendLine($"/{gsName} gs");
            }
            else if (_alphaActive)
            {
                _stream.AppendLine("Q"); // restore graphics state to full opacity
                _alphaActive = false;
            }
            _stream.AppendLine($"{F(r / 255.0)} {F(g / 255.0)} {F(b / 255.0)} rg");
        }

        private static string F(double v)
        {
            return SimplePdfDocument.F(v);
        }

        private static byte[] Deflate(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (ZLibStream zlib = new ZLibStream(ms, CompressionLevel.Fastest, leaveOpen: true))
                {
                    zlib.Write(data, 0, data.Length);
                }
                return ms.ToArray();
            }
        }


        // ─── Helvetica character widths (AFM standard) ───────────

        // Standard widths for Helvetica (per 1000 units)
        // Only ASCII 32..126 needed; rest default to 500
        private static readonly double[] HelveticaWidths = BuildHelveticaWidths(false);
        private static readonly double[] HelveticaBoldWidths = BuildHelveticaWidths(true);

        private static double[] BuildHelveticaWidths(bool bold)
        {
            double[] w = new double[256];
            double defaultWidth = bold ? 556 : 500;
            for (int i = 0; i < 256; i++) w[i] = defaultWidth;

            if (!bold)
            {
                // Helvetica (standard widths from AFM)
                w[32] = 278; w[33] = 278; w[34] = 355; w[35] = 556; w[36] = 556;
                w[37] = 889; w[38] = 667; w[39] = 191; w[40] = 333; w[41] = 333;
                w[42] = 389; w[43] = 584; w[44] = 278; w[45] = 333; w[46] = 278;
                w[47] = 278; w[48] = 556; w[49] = 556; w[50] = 556; w[51] = 556;
                w[52] = 556; w[53] = 556; w[54] = 556; w[55] = 556; w[56] = 556;
                w[57] = 556; w[58] = 278; w[59] = 278; w[60] = 584; w[61] = 584;
                w[62] = 584; w[63] = 556; w[64] = 1015; w[65] = 667; w[66] = 667;
                w[67] = 722; w[68] = 722; w[69] = 667; w[70] = 611; w[71] = 778;
                w[72] = 722; w[73] = 278; w[74] = 500; w[75] = 667; w[76] = 556;
                w[77] = 833; w[78] = 722; w[79] = 778; w[80] = 667; w[81] = 778;
                w[82] = 722; w[83] = 667; w[84] = 611; w[85] = 722; w[86] = 667;
                w[87] = 944; w[88] = 667; w[89] = 667; w[90] = 611; w[91] = 278;
                w[92] = 278; w[93] = 278; w[94] = 469; w[95] = 556; w[96] = 333;
                w[97] = 556; w[98] = 556; w[99] = 500; w[100] = 556; w[101] = 556;
                w[102] = 278; w[103] = 556; w[104] = 556; w[105] = 222; w[106] = 222;
                w[107] = 500; w[108] = 222; w[109] = 833; w[110] = 556; w[111] = 556;
                w[112] = 556; w[113] = 556; w[114] = 333; w[115] = 500; w[116] = 278;
                w[117] = 556; w[118] = 500; w[119] = 722; w[120] = 500; w[121] = 500;
                w[122] = 500; w[123] = 334; w[124] = 260; w[125] = 334; w[126] = 584;
                // Special chars
                w[183] = 350; // middle dot ·
                w[215] = 584; // multiplication sign ×
            }
            else
            {
                // Helvetica-Bold (standard widths from AFM)
                w[32] = 278; w[33] = 333; w[34] = 474; w[35] = 556; w[36] = 556;
                w[37] = 889; w[38] = 722; w[39] = 238; w[40] = 333; w[41] = 333;
                w[42] = 389; w[43] = 584; w[44] = 278; w[45] = 333; w[46] = 278;
                w[47] = 278; w[48] = 556; w[49] = 556; w[50] = 556; w[51] = 556;
                w[52] = 556; w[53] = 556; w[54] = 556; w[55] = 556; w[56] = 556;
                w[57] = 556; w[58] = 333; w[59] = 333; w[60] = 584; w[61] = 584;
                w[62] = 584; w[63] = 611; w[64] = 975; w[65] = 722; w[66] = 722;
                w[67] = 722; w[68] = 722; w[69] = 667; w[70] = 611; w[71] = 778;
                w[72] = 722; w[73] = 278; w[74] = 556; w[75] = 722; w[76] = 611;
                w[77] = 833; w[78] = 722; w[79] = 778; w[80] = 667; w[81] = 778;
                w[82] = 722; w[83] = 667; w[84] = 611; w[85] = 722; w[86] = 667;
                w[87] = 944; w[88] = 667; w[89] = 667; w[90] = 611; w[91] = 333;
                w[92] = 278; w[93] = 333; w[94] = 584; w[95] = 556; w[96] = 333;
                w[97] = 556; w[98] = 611; w[99] = 556; w[100] = 611; w[101] = 556;
                w[102] = 333; w[103] = 611; w[104] = 611; w[105] = 278; w[106] = 278;
                w[107] = 556; w[108] = 278; w[109] = 889; w[110] = 611; w[111] = 611;
                w[112] = 611; w[113] = 611; w[114] = 389; w[115] = 556; w[116] = 333;
                w[117] = 611; w[118] = 556; w[119] = 778; w[120] = 556; w[121] = 556;
                w[122] = 500; w[123] = 389; w[124] = 280; w[125] = 389; w[126] = 584;
                w[183] = 350;
                w[215] = 584;
            }

            return w;
        }
    }


    // ═══════════════════════════════════════════════════════════════
    //  Supporting types
    // ═══════════════════════════════════════════════════════════════

    public enum SimplePdfFont
    {
        Regular,
        Bold
    }

    public enum SimplePdfTableBorderStyle
    {
        None,
        All,
        Outer,
        HeaderOnly
    }

    public enum SimplePdfCellAlign
    {
        Left,
        Center,
        Right
    }


    internal class PdfObject
    {
        public int Id;
        public long Offset;
    }


    internal class PageImage
    {
        public string Name;
        public byte[] PngData;
        public byte[] JpegData;
        public int ImageObjectId;
        public int SmaskObjectId;
    }


    internal class PdfShading
    {
        public string Name;
        public int ObjectId;
        public int FunctionId;
        public double X0, Y0, X1, Y1;
        public double R0, G0, B0;
        public double R1, G1, B1;
    }


    internal class PdfLinkAnnotation
    {
        public int ObjectId;
        public double X, Y, W, H;
        public string Url;
    }


    /// <summary>Predefined page sizes in points.</summary>
    public enum SimplePdfPageSize
    {
        A4,
        Letter,
        A3,
        Legal
    }

    /// <summary>Page size dimension lookup.</summary>
    public static class SimplePdfPageSizes
    {
        public static (double Width, double Height) GetDimensions(SimplePdfPageSize size)
        {
            switch (size)
            {
                case SimplePdfPageSize.A4: return (595.28, 841.89);
                case SimplePdfPageSize.Letter: return (612, 792);
                case SimplePdfPageSize.A3: return (841.89, 1190.55);
                case SimplePdfPageSize.Legal: return (612, 1008);
                default: return (595.28, 841.89);
            }
        }
    }


    /// <summary>Simple stream writer that tracks byte position.</summary>
    internal class PdfStreamWriter
    {
        private readonly Stream _stream;

        public PdfStreamWriter(Stream stream) { _stream = stream; }

        public long Position => _stream.Position;

        public void WriteRaw(string text)
        {
            byte[] bytes = Encoding.GetEncoding(1252).GetBytes(text);
            _stream.Write(bytes, 0, bytes.Length);
        }

        public void WriteBytes(byte[] data)
        {
            _stream.Write(data, 0, data.Length);
        }
    }


    // ═══════════════════════════════════════════════════════════════
    //  GlyphTracker — tracks per-page glyph usage for embedded fonts
    // ═══════════════════════════════════════════════════════════════

    internal class GlyphTracker
    {
        private readonly EmbeddedFont _embeddedFont;
        private readonly List<(int unicode, int glyphIdx)> _usedGlyphs = new List<(int, int)>();
        private readonly Dictionary<int, int> _glyphIdxToSubsetIdx = new Dictionary<int, int>();

        public GlyphTracker(string fontName, EmbeddedFont embeddedFont)
        {
            _embeddedFont = embeddedFont;
        }

        public List<(int unicode, int glyphIdx)> UsedGlyphs => _usedGlyphs;
        public EmbeddedFont Font => _embeddedFont;

        public int RecordGlyphUsage(int glyphIndex)
        {
            return glyphIndex != 0 && _glyphIdxToSubsetIdx.TryGetValue(glyphIndex, out int idx) ? idx : 0;
        }

        public void RecordTextRun(string text)
        {
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
                int gi = _embeddedFont.SourceFont.GetGlyphIndex(cp);
                if (gi > 0 && !_glyphIdxToSubsetIdx.ContainsKey(gi))
                {
                    _glyphIdxToSubsetIdx[gi] = _usedGlyphs.Count + 1;
                    _usedGlyphs.Add((cp, gi));
                    _embeddedFont.RecordGlyphUsage(cp);
                }
            }
        }
    }
}
