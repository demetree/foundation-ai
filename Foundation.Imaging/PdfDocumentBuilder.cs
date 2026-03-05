using System;
using System.Collections.Generic;
using Foundation.Imaging.Pdf;

namespace Foundation.Imaging
{
    /// <summary>
    /// Generic PDF document builder implementing IDocumentBuilder.
    ///
    /// Produces a multi-page PDF using SimplePdfDocument with:
    ///   - Themed cover/closing pages with gradient backgrounds
    ///   - Content pages with images, items callouts, and thumbnails
    ///   - Section grouping with visual callout indicator
    ///   - Summary pages with 4-column grid layout
    ///   - Page numbering via deferred footer pass
    ///   - A4/Letter page size support
    ///
    /// Theme colours are pulled from DocumentTheme; defaults to a
    /// professional dark navy/blue palette if no theme is supplied.
    /// </summary>
    public class PdfDocumentBuilder : IDocumentBuilder
    {
        private SimplePdfDocument _doc;
        private string _title;
        private DocumentOptions _options;
        private DocumentTheme _theme;

        private double _pageWidth;
        private double _pageHeight;
        private const double Margin = 40;

        // Font sizes
        private const double TitleSize = 32;
        private const double HeaderSize = 20;
        private const double BodySize = 10;
        private const double SmallSize = 8;
        private const double TinySize = 7;

        // Page tracking for footer pass
        private readonly List<(SimplePdfPage page, bool isDarkBg)> _allPages
            = new List<(SimplePdfPage, bool)>();

        // Section state
        private string _currentSectionName;

        // Summary grid
        private const int SummaryItemsPerPage = 15;


        // ═══════════════════════════════════════════════════════════
        //  IDocumentBuilder implementation
        // ═══════════════════════════════════════════════════════════

        public void BeginDocument(string title, DocumentOptions options)
        {
            _title = title ?? "";
            _options = options ?? new DocumentOptions();
            _theme = _options.Theme ?? new DocumentTheme();

            _doc = new SimplePdfDocument(_title, "Foundation.Imaging");

            bool letter = (_options.PageSize ?? "a4").ToLowerInvariant() == "letter";
            _pageWidth = letter ? 612 : 595.28;   // 8.5" or 210mm
            _pageHeight = letter ? 792 : 841.89;  // 11" or 297mm
        }


        public void AddCoverPage(byte[] titleImage)
        {
            var page = NewPage(isDarkBg: true);
            ParseHex(_theme.GradientStartHex, out byte gs_r, out byte gs_g, out byte gs_b);
            ParseHex(_theme.GradientEndHex, out byte ge_r, out byte ge_g, out byte ge_b);
            ParseHex(_theme.CoverTextHex, out byte ct_r, out byte ct_g, out byte ct_b);

            // Gradient background
            page.FillGradientRect(0, 0, _pageWidth, _pageHeight,
                gs_r, gs_g, gs_b, ge_r, ge_g, ge_b, diagonal: true);

            double y = _pageHeight * 0.06;

            // Title
            page.DrawTextCentered(_title, SimplePdfFont.Bold, TitleSize,
                Margin, y, _pageWidth - Margin * 2,
                ct_r, ct_g, ct_b);
            y += 50;

            // Cover image
            if (titleImage != null && titleImage.Length > 0)
            {
                var (imgW, imgH) = SimplePdfPage.GetPngDimensions(titleImage);
                if (imgW > 0 && imgH > 0)
                {
                    double maxW = _pageWidth - Margin * 2;
                    double maxH = _pageHeight - y - 60;
                    double scale = Math.Min(maxW / imgW, maxH / imgH);
                    double drawW = imgW * scale;
                    double drawH = imgH * scale;
                    double cx = _pageWidth / 2;

                    page.DrawImage(titleImage, cx - drawW / 2, y, drawW, drawH);
                }
            }
        }


        public void BeginSection(string sectionName)
        {
            _currentSectionName = sectionName;
        }


        public void AddContentPage(string heading, byte[] contentImage,
            List<ContentItem> items, Dictionary<string, byte[]> thumbnails)
        {
            var page = NewPage();
            ParseHex(_theme.AccentHex, out byte ac_r, out byte ac_g, out byte ac_b);
            ParseHex(_theme.BodyTextHex, out byte bt_r, out byte bt_g, out byte bt_b);
            ParseHex(_theme.MutedTextHex, out byte mt_r, out byte mt_g, out byte mt_b);
            ParseHex(_theme.SeparatorHex, out byte sp_r, out byte sp_g, out byte sp_b);

            double contentW = _pageWidth - Margin * 2;
            double y = Margin;

            // Section indicator
            if (_currentSectionName != null)
            {
                ParseHex(_theme.SectionBorderHex, out byte sb_r, out byte sb_g, out byte sb_b);
                page.DrawText("\u203A " + _currentSectionName, SimplePdfFont.Regular, SmallSize,
                    Margin, y + 10, sb_r, sb_g, sb_b);
                y += 18;
            }

            // Heading
            page.DrawText(heading ?? "", SimplePdfFont.Bold, HeaderSize,
                Margin, y + 22, ac_r, ac_g, ac_b);
            y += 32;

            // Separator line
            page.DrawLine(Margin, y, _pageWidth - Margin, y, sp_r, sp_g, sp_b);
            y += 12;

            // Content image
            if (contentImage != null && contentImage.Length > 0)
            {
                var (imgW, imgH) = SimplePdfPage.GetPngDimensions(contentImage);
                if (imgW > 0 && imgH > 0)
                {
                    double maxImgW = contentW;
                    double maxImgH = _pageHeight * 0.5;
                    double scale = Math.Min(maxImgW / imgW, maxImgH / imgH);
                    double drawW = imgW * scale;
                    double drawH = imgH * scale;
                    double imgX = Margin + (contentW - drawW) / 2;

                    page.DrawImage(contentImage, imgX, y, drawW, drawH);
                    y += drawH + 16;
                }
            }

            // Items callout
            if (items != null && items.Count > 0)
            {
                ParseHex(_theme.CalloutBackgroundHex, out byte cb_r, out byte cb_g, out byte cb_b);

                double calloutH = 6 + items.Count * 22;
                page.FillRoundedRect(Margin, y, contentW, calloutH, 4,
                    cb_r, cb_g, cb_b, 255,
                    sp_r, sp_g, sp_b, true);

                y += 4;
                page.DrawText("ITEMS", SimplePdfFont.Regular, TinySize,
                    Margin + 8, y + 8, mt_r, mt_g, mt_b);
                y += 14;

                foreach (var item in items)
                {
                    double x = Margin + 8;

                    // Thumbnail
                    byte[] thumbPng = null;
                    if (thumbnails != null && item.Key != null)
                        thumbnails.TryGetValue(item.Key, out thumbPng);

                    if (thumbPng != null && thumbPng.Length > 0)
                    {
                        page.DrawImage(thumbPng, x, y - 2, 16, 16);
                        x += 20;
                    }

                    // Quantity
                    page.DrawText($"{item.Quantity}×", SimplePdfFont.Bold, BodySize,
                        x, y + 10, ac_r, ac_g, ac_b);
                    x += 28;

                    // Label
                    string label = item.Label ?? "";
                    page.DrawText(label, SimplePdfFont.Regular, BodySize,
                        x, y + 10, bt_r, bt_g, bt_b);
                    x += page.MeasureText(label, SimplePdfFont.Regular, BodySize) + 8;

                    // Colour swatch
                    if (!string.IsNullOrEmpty(item.ColourHex))
                    {
                        try
                        {
                            ParseHex(item.ColourHex, out byte sw_r, out byte sw_g, out byte sw_b);
                            page.FillEllipse(x, y + 2, 8, 8, sw_r, sw_g, sw_b);
                            x += 12;
                        }
                        catch { /* skip on parse error */ }
                    }

                    // Subtitle
                    if (!string.IsNullOrEmpty(item.Subtitle))
                    {
                        page.DrawText(item.Subtitle, SimplePdfFont.Regular, SmallSize,
                            x, y + 9, mt_r, mt_g, mt_b);
                    }

                    y += 20;
                }
            }

            // Footer
            page.DrawTextCentered(
                $"{_title} · {heading}",
                SimplePdfFont.Regular, TinySize,
                Margin, _pageHeight - Margin, contentW,
                mt_r, mt_g, mt_b);
        }


        public void EndSection()
        {
            _currentSectionName = null;
        }


        public void AddSummaryPage(string heading, List<ContentItem> items,
            Dictionary<string, byte[]> thumbnails)
        {
            if (items == null || items.Count == 0) return;

            ParseHex(_theme.BodyTextHex, out byte bt_r, out byte bt_g, out byte bt_b);
            ParseHex(_theme.MutedTextHex, out byte mt_r, out byte mt_g, out byte mt_b);
            ParseHex(_theme.AccentHex, out byte ac_r, out byte ac_g, out byte ac_b);
            ParseHex(_theme.SeparatorHex, out byte sp_r, out byte sp_g, out byte sp_b);

            int totalItems = items.Count;
            int totalQty = 0;
            for (int i = 0; i < items.Count; i++) totalQty += items[i].Quantity;

            int pageCount = (int)Math.Ceiling((double)totalItems / SummaryItemsPerPage);

            for (int pg = 0; pg < pageCount; pg++)
            {
                var page = NewPage();
                double contentW = _pageWidth - Margin * 2;
                double y = Margin;

                // Header
                string titleText = pageCount > 1
                    ? $"{heading} ({pg + 1}/{pageCount})"
                    : heading;

                page.DrawText(titleText, SimplePdfFont.Bold, HeaderSize,
                    Margin, y + 22, bt_r, bt_g, bt_b);
                y += 30;

                page.DrawText($"{totalItems} items · {totalQty} total",
                    SimplePdfFont.Regular, BodySize,
                    Margin, y + 10, mt_r, mt_g, mt_b);
                y += 22;

                page.DrawLine(Margin, y, _pageWidth - Margin, y, sp_r, sp_g, sp_b);
                y += 16;

                // Grid layout: 4 columns
                int cols = 4;
                double cellW = contentW / cols;
                double cellH = 100;
                int col = 0;

                int start = pg * SummaryItemsPerPage;
                int end = Math.Min(start + SummaryItemsPerPage, totalItems);

                for (int idx = start; idx < end; idx++)
                {
                    if (y + cellH > _pageHeight - Margin - 20)
                    {
                        page = NewPage();
                        y = Margin;
                        col = 0;
                    }

                    var item = items[idx];
                    double cellX = Margin + col * cellW;

                    // Thumbnail
                    byte[] thumbPng = null;
                    if (thumbnails != null && item.Key != null)
                        thumbnails.TryGetValue(item.Key, out thumbPng);

                    if (thumbPng != null && thumbPng.Length > 0)
                    {
                        page.DrawImage(thumbPng, cellX + (cellW - 40) / 2, y, 40, 40);
                    }

                    // Quantity
                    page.DrawTextCentered($"{item.Quantity}×", SimplePdfFont.Bold, BodySize,
                        cellX, y + 44, cellW,
                        ac_r, ac_g, ac_b);

                    // Label
                    string label = item.Label ?? "";
                    if (label.Length > 20) label = label.Substring(0, 18) + "...";
                    page.DrawTextCentered(label, SimplePdfFont.Regular, SmallSize,
                        cellX, y + 60, cellW,
                        bt_r, bt_g, bt_b);

                    // Colour swatch
                    if (!string.IsNullOrEmpty(item.ColourHex))
                    {
                        try
                        {
                            ParseHex(item.ColourHex, out byte sw_r, out byte sw_g, out byte sw_b);
                            page.FillEllipse(cellX + cellW / 2 - 20, y + 74, 8, 8, sw_r, sw_g, sw_b);
                        }
                        catch { /* skip */ }
                    }

                    // Subtitle
                    string subtitle = item.Subtitle ?? "";
                    if (subtitle.Length > 18) subtitle = subtitle.Substring(0, 16) + "...";
                    page.DrawTextCentered(subtitle, SimplePdfFont.Regular, TinySize,
                        cellX + 10, y + 73, cellW - 10,
                        mt_r, mt_g, mt_b);

                    col++;
                    if (col >= cols)
                    {
                        col = 0;
                        y += cellH;
                    }
                }
            }
        }


        public void AddClosingPage(byte[] closingImage)
        {
            var page = NewPage(isDarkBg: true);
            ParseHex(_theme.ClosingGradientStartHex, out byte gs_r, out byte gs_g, out byte gs_b);
            ParseHex(_theme.ClosingGradientEndHex, out byte ge_r, out byte ge_g, out byte ge_b);
            ParseHex(_theme.CoverTextHex, out byte ct_r, out byte ct_g, out byte ct_b);

            // Green gradient background
            page.FillGradientRect(0, 0, _pageWidth, _pageHeight,
                gs_r, gs_g, gs_b, ge_r, ge_g, ge_b);

            double y = 40;

            // Title
            page.DrawTextCentered(_title, SimplePdfFont.Bold, 28,
                0, y, _pageWidth, ct_r, ct_g, ct_b);
            y += 40;

            // Closing image
            if (closingImage != null && closingImage.Length > 0)
            {
                var (imgW, imgH) = SimplePdfPage.GetPngDimensions(closingImage);
                if (imgW > 0 && imgH > 0)
                {
                    double maxW = _pageWidth - 80;
                    double maxH = _pageHeight - y - 80;
                    double scale = Math.Min(maxW / imgW, maxH / imgH);
                    double drawW = imgW * scale;
                    double drawH = imgH * scale;
                    double x = (_pageWidth - drawW) / 2;

                    page.DrawImage(closingImage, x, y, drawW, drawH);
                }
            }
        }


        public DocumentResult Build()
        {
            ParseHex(_theme.CoverTextHex, out byte wt_r, out byte wt_g, out byte wt_b);
            ParseHex(_theme.MutedTextHex, out byte mt_r, out byte mt_g, out byte mt_b);

            // Stamp page number footers
            int totalPages = _allPages.Count;
            for (int i = 0; i < totalPages; i++)
            {
                var (page, isDarkBg) = _allPages[i];
                double contentW = _pageWidth - Margin * 2;

                byte cr = isDarkBg ? wt_r : mt_r;
                byte cg = isDarkBg ? wt_g : mt_g;
                byte cb = isDarkBg ? wt_b : mt_b;

                page.DrawTextCentered(
                    $"Page {i + 1} of {totalPages}",
                    SimplePdfFont.Regular, TinySize,
                    Margin, _pageHeight - 20, contentW,
                    cr, cg, cb, isDarkBg ? (byte)130 : (byte)255);
            }

            byte[] pdfBytes = _doc.Save();
            _doc.Dispose();

            return new DocumentResult
            {
                DocumentBytes = pdfBytes,
                ContentType = "application/pdf"
            };
        }


        // ═══════════════════════════════════════════════════════════
        //  Private helpers
        // ═══════════════════════════════════════════════════════════

        private SimplePdfPage NewPage(bool isDarkBg = false)
        {
            var page = _doc.AddPage(_pageWidth, _pageHeight);
            _allPages.Add((page, isDarkBg));
            return page;
        }


        /// <summary>
        /// Parse a hex colour string (e.g. "#1A1A2E" or "1A1A2E") into RGB bytes.
        /// </summary>
        private static void ParseHex(string hex, out byte r, out byte g, out byte b)
        {
            if (string.IsNullOrEmpty(hex))
            {
                r = 0; g = 0; b = 0;
                return;
            }

            hex = hex.TrimStart('#');
            if (hex.Length < 6)
            {
                r = 0; g = 0; b = 0;
                return;
            }

            r = Convert.ToByte(hex.Substring(0, 2), 16);
            g = Convert.ToByte(hex.Substring(2, 2), 16);
            b = Convert.ToByte(hex.Substring(4, 2), 16);
        }
    }
}
