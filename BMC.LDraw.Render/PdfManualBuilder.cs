using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Pdf;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// PDF manual builder using PdfSharpCore.
    /// Produces a multi-page PDF with cover, step pages, parts callouts,
    /// and a Bill of Materials summary.
    /// </summary>
    public class PdfManualBuilder : IManualDocumentBuilder
    {
        private PdfDocument _doc;
        private ManualBuildPlan _plan;
        private ManualOptions _options;
        private string _modelName;
        private int _totalParts;

        private double _pageWidth;
        private double _pageHeight;
        private const double Margin = 40;

        // Font cache
        private XFont _titleFont;
        private XFont _subtitleFont;
        private XFont _headerFont;
        private XFont _bodyFont;
        private XFont _smallFont;
        private XFont _tinyFont;
        private XFont _boldFont;

        // Colours
        private static readonly XColor DarkNavy = XColor.FromArgb(255, 26, 26, 46);
        private static readonly XColor MidNavy = XColor.FromArgb(255, 22, 33, 62);
        private static readonly XColor Red = XColor.FromArgb(255, 229, 62, 62);
        private static readonly XColor Grey = XColor.FromArgb(255, 148, 163, 184);
        private static readonly XColor LightGrey = XColor.FromArgb(255, 226, 232, 240);
        private static readonly XColor BodyText = XColor.FromArgb(255, 26, 26, 46);


        public void BeginDocument(string modelName, ManualBuildPlan plan, ManualOptions options)
        {
            _modelName = modelName;
            _plan = plan;
            _options = options;
            _doc = new PdfDocument();
            _doc.Info.Title = modelName + " — Build Manual";
            _doc.Info.Author = "BMC Manual Generator";

            // Page dimensions
            bool letter = (options.PageSize ?? "a4").ToLowerInvariant() == "letter";
            _pageWidth = letter ? XUnit.FromInch(8.5).Point : XUnit.FromMillimeter(210).Point;
            _pageHeight = letter ? XUnit.FromInch(11).Point : XUnit.FromMillimeter(297).Point;

            // Fonts
            _titleFont = new XFont("Segoe UI", 32, XFontStyle.Bold);
            _subtitleFont = new XFont("Segoe UI", 14, XFontStyle.Regular);
            _headerFont = new XFont("Segoe UI", 20, XFontStyle.Bold);
            _bodyFont = new XFont("Segoe UI", 10, XFontStyle.Regular);
            _boldFont = new XFont("Segoe UI", 10, XFontStyle.Bold);
            _smallFont = new XFont("Segoe UI", 8, XFontStyle.Regular);
            _tinyFont = new XFont("Segoe UI", 7, XFontStyle.Regular);
        }


        public void AddCoverPage(byte[] finalModelImage)
        {
            var page = AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            double w = _pageWidth;
            double h = _pageHeight;

            // Gradient background
            var rect = new XRect(0, 0, w, h);
            var brush = new XLinearGradientBrush(rect, DarkNavy, MidNavy,
                XLinearGradientMode.ForwardDiagonal);
            gfx.DrawRectangle(brush, rect);

            double cx = w / 2;
            double y = h * 0.12;

            // Title
            gfx.DrawString(_modelName, _titleFont, XBrushes.White,
                new XRect(Margin, y, w - Margin * 2, 50),
                XStringFormats.TopCenter);
            y += 50;

            // Subtitle
            gfx.DrawString("BUILD MANUAL", _subtitleFont,
                new XSolidBrush(XColor.FromArgb(150, 255, 255, 255)),
                new XRect(Margin, y, w - Margin * 2, 25),
                XStringFormats.TopCenter);
            y += 50;

            // Cover image
            if (finalModelImage != null && finalModelImage.Length > 0)
            {
                using (var ms = new MemoryStream(finalModelImage))
                {
                    var img = XImage.FromStream(() => new MemoryStream(finalModelImage));
                    double maxW = w - Margin * 4;
                    double maxH = h * 0.5;
                    double scale = Math.Min(maxW / img.PixelWidth, maxH / img.PixelHeight);
                    double imgW = img.PixelWidth * scale;
                    double imgH = img.PixelHeight * scale;

                    gfx.DrawImage(img, cx - imgW / 2, y, imgW, imgH);
                    y += imgH + 30;
                }
            }

            // Meta info
            int totalParts = _plan.Steps.Count > 0
                ? _plan.Steps[_plan.Steps.Count - 1].CumulativePartCount : 0;
            gfx.DrawString($"{_plan.TotalSteps} Steps · {totalParts} Parts",
                _bodyFont, new XSolidBrush(XColor.FromArgb(130, 255, 255, 255)),
                new XRect(Margin, y, w - Margin * 2, 20),
                XStringFormats.TopCenter);

            gfx.Dispose();
        }


        public void BeginSubmodelCallout(string submodelName, int totalSubmodelSteps)
        {
            // Submodel steps are rendered as individual step pages with a callout header
        }


        public void AddStep(ManualBuildStep step, byte[] stepImage,
            Dictionary<string, byte[]> partImages)
        {
            _totalParts = Math.Max(_totalParts, step.CumulativePartCount);

            var page = AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            double contentW = _pageWidth - Margin * 2;
            double y = Margin;

            // Submodel callout indicator
            if (step.IsSubmodelStep)
            {
                string subName = System.IO.Path.GetFileNameWithoutExtension(step.ModelName);
                gfx.DrawString("▸ " + subName, _smallFont,
                    new XSolidBrush(XColor.FromArgb(255, 43, 108, 176)),
                    new XPoint(Margin, y + 10));
                y += 18;
            }

            // Step header
            gfx.DrawString($"Step {step.GlobalStepIndex}",
                _headerFont, new XSolidBrush(Red),
                new XPoint(Margin, y + 22));
            gfx.DrawString($"of {_plan.TotalSteps}",
                _bodyFont, new XSolidBrush(Grey),
                new XPoint(Margin + 120, y + 22));
            y += 32;

            // Separator line
            gfx.DrawLine(new XPen(LightGrey, 1), Margin, y, _pageWidth - Margin, y);
            y += 12;

            // Step image
            if (stepImage != null && stepImage.Length > 0)
            {
                var img = XImage.FromStream(() => new MemoryStream(stepImage));
                double maxImgW = contentW;
                double maxImgH = _pageHeight * 0.5;
                double scale = Math.Min(maxImgW / img.PixelWidth, maxImgH / img.PixelHeight);
                double imgW = img.PixelWidth * scale;
                double imgH = img.PixelHeight * scale;
                double imgX = Margin + (contentW - imgW) / 2;

                gfx.DrawImage(img, imgX, y, imgW, imgH);
                y += imgH + 16;
            }

            // Parts callout
            if (step.NewParts != null && step.NewParts.Count > 0)
            {
                y = DrawPartsCallout(gfx, step.NewParts, partImages, y, contentW);
            }

            // Footer
            gfx.DrawString($"{_modelName} · Step {step.GlobalStepIndex}/{_plan.TotalSteps}",
                _tinyFont, new XSolidBrush(Grey),
                new XRect(Margin, _pageHeight - Margin, contentW, 12),
                XStringFormats.BottomCenter);

            gfx.Dispose();
        }


        public void EndSubmodelCallout()
        {
            // Each submodel step is its own page; no special close needed
        }


        public void AddBillOfMaterials(List<StepPartInfo> allUniqueParts,
            Dictionary<string, byte[]> partImages)
        {
            if (allUniqueParts == null || allUniqueParts.Count == 0) return;

            var page = AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            double contentW = _pageWidth - Margin * 2;
            double y = Margin;

            // BOM header
            gfx.DrawString("Bill of Materials", _headerFont,
                new XSolidBrush(DarkNavy), new XPoint(Margin, y + 22));
            y += 30;

            int totalPieces = allUniqueParts.Sum(p => p.Quantity);
            gfx.DrawString($"{allUniqueParts.Count} unique parts · {totalPieces} total pieces",
                _bodyFont, new XSolidBrush(Grey), new XPoint(Margin, y + 10));
            y += 22;

            gfx.DrawLine(new XPen(LightGrey, 1), Margin, y, _pageWidth - Margin, y);
            y += 16;

            // Grid layout: 4 columns
            int cols = 4;
            double cellW = contentW / cols;
            double cellH = 100;
            int col = 0;

            foreach (var part in allUniqueParts)
            {
                if (y + cellH > _pageHeight - Margin - 20)
                {
                    gfx.Dispose();
                    page = AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = Margin;
                    col = 0;
                }

                double cellX = Margin + col * cellW;

                // Thumbnail
                string imgKey = PartImageCache.MakeKey(part.FileName, part.ColourCode);
                byte[] thumbPng = null;
                if (partImages != null)
                    partImages.TryGetValue(imgKey, out thumbPng);

                if (thumbPng != null && thumbPng.Length > 0)
                {
                    var img = XImage.FromStream(() => new MemoryStream(thumbPng));
                    gfx.DrawImage(img, cellX + (cellW - 40) / 2, y, 40, 40);
                }

                // Quantity
                gfx.DrawString($"{part.Quantity}×", _boldFont,
                    new XSolidBrush(Red),
                    new XRect(cellX, y + 44, cellW, 14),
                    XStringFormats.TopCenter);

                // Part name
                string displayName = !string.IsNullOrEmpty(part.PartDescription)
                    ? part.PartDescription
                    : System.IO.Path.GetFileNameWithoutExtension(part.FileName);
                // Truncate long names
                if (displayName.Length > 20)
                    displayName = displayName.Substring(0, 18) + "…";
                gfx.DrawString(displayName, _smallFont,
                    new XSolidBrush(BodyText),
                    new XRect(cellX, y + 60, cellW, 12),
                    XStringFormats.TopCenter);

                // Colour swatch + name
                if (!string.IsNullOrEmpty(part.ColourHex))
                {
                    try
                    {
                        string hex = part.ColourHex.TrimStart('#');
                        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                        byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                        var swatchColor = XColor.FromArgb(255, r, g, b);
                        gfx.DrawEllipse(new XSolidBrush(swatchColor),
                            cellX + cellW / 2 - 20, y + 74, 8, 8);
                    }
                    catch { /* skip swatch on parse error */ }
                }

                string colourLabel = !string.IsNullOrEmpty(part.ColourName)
                    ? part.ColourName : "Colour " + part.ColourCode;
                if (colourLabel.Length > 18)
                    colourLabel = colourLabel.Substring(0, 16) + "…";
                gfx.DrawString(colourLabel, _tinyFont,
                    new XSolidBrush(Grey),
                    new XRect(cellX + 10, y + 73, cellW - 10, 12),
                    XStringFormats.TopCenter);

                // Part ID
                gfx.DrawString(
                    System.IO.Path.GetFileNameWithoutExtension(part.FileName),
                    _tinyFont, new XSolidBrush(Grey),
                    new XRect(cellX, y + 86, cellW, 10),
                    XStringFormats.TopCenter);

                col++;
                if (col >= cols)
                {
                    col = 0;
                    y += cellH;
                }
            }

            gfx.Dispose();
        }

        public void AddCompletionPage(byte[] completedModelImage)
        {
            // Re-use cover page rendering logic with a different colour scheme
            var page = AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            // Forest green gradient background
            var topColour = XColor.FromArgb(255, 10, 47, 31);
            var bottomColour = XColor.FromArgb(255, 15, 76, 58);
            var bgBrush = new XLinearGradientBrush(
                new XPoint(0, 0), new XPoint(0, page.Height),
                topColour, bottomColour);
            gfx.DrawRectangle(bgBrush, 0, 0, page.Width, page.Height);

            double y = 60;

            // Title
            var titleFont = new XFont("Arial", 28, XFontStyle.Bold);
            gfx.DrawString(_modelName ?? "Model",
                titleFont, XBrushes.White,
                new XRect(0, y, page.Width, 40),
                XStringFormats.TopCenter);
            y += 45;

            // Subtitle
            var subtitleFont = new XFont("Arial", 16, XFontStyle.Regular);
            gfx.DrawString("Build Complete!",
                subtitleFont, new XSolidBrush(XColor.FromArgb(180, 255, 255, 255)),
                new XRect(0, y, page.Width, 30),
                XStringFormats.TopCenter);
            y += 50;

            // Model image
            if (completedModelImage != null && completedModelImage.Length > 0)
            {
                using (var ms = new MemoryStream(completedModelImage))
                {
                    var img = XImage.FromStream(() => new MemoryStream(completedModelImage));
                    double maxW = page.Width - 80;
                    double maxH = page.Height - y - 80;
                    double scale = Math.Min(maxW / img.PixelWidth, maxH / img.PixelHeight);
                    double drawW = img.PixelWidth * scale;
                    double drawH = img.PixelHeight * scale;
                    double x = (page.Width - drawW) / 2;
                    gfx.DrawImage(img, x, y, drawW, drawH);
                    y += drawH + 20;
                }
            }

            // Stats
            int totalParts = _plan != null && _plan.Steps.Count > 0
                ? _plan.Steps[_plan.Steps.Count - 1].CumulativePartCount
                : 0;
            var metaFont = new XFont("Arial", 11, XFontStyle.Regular);
            string metaText = $"{_plan?.TotalSteps ?? 0} Steps · {totalParts} Parts";
            gfx.DrawString(metaText,
                metaFont, new XSolidBrush(XColor.FromArgb(128, 255, 255, 255)),
                new XRect(0, y, page.Width, 20),
                XStringFormats.TopCenter);

            gfx.Dispose();
        }


        public ManualGenerationResult Build()
        {
            byte[] pdfBytes;
            using (var ms = new MemoryStream())
            {
                _doc.Save(ms, false);
                pdfBytes = ms.ToArray();
            }
            _doc.Dispose();

            return new ManualGenerationResult
            {
                Html = null,
                DocumentBytes = pdfBytes,
                TotalSteps = _plan?.TotalSteps ?? 0,
                TotalParts = _totalParts
            };
        }


        // ═══ Private helpers ═══

        private PdfPage AddPage()
        {
            var page = _doc.AddPage();
            page.Width = XUnit.FromPoint(_pageWidth);
            page.Height = XUnit.FromPoint(_pageHeight);
            return page;
        }


        /// <summary>
        /// Draw the parts callout at position y. Returns the new y position.
        /// </summary>
        private double DrawPartsCallout(XGraphics gfx, List<StepPartInfo> parts,
            Dictionary<string, byte[]> partImages, double y, double contentW)
        {
            // Background
            gfx.DrawRoundedRectangle(
                new XPen(LightGrey, 0.5),
                new XSolidBrush(XColor.FromArgb(255, 248, 250, 252)),
                Margin, y, contentW, 6 + parts.Count * 22,
                4, 4);

            y += 4;

            // "New Parts" label
            gfx.DrawString("NEW PARTS", _tinyFont,
                new XSolidBrush(Grey),
                new XPoint(Margin + 8, y + 8));
            y += 14;

            foreach (var part in parts)
            {
                double x = Margin + 8;

                // PLI thumbnail (small inline)
                string imgKey = PartImageCache.MakeKey(part.FileName, part.ColourCode);
                byte[] thumbPng = null;
                if (partImages != null)
                    partImages.TryGetValue(imgKey, out thumbPng);

                if (thumbPng != null && thumbPng.Length > 0)
                {
                    var img = XImage.FromStream(() => new MemoryStream(thumbPng));
                    gfx.DrawImage(img, x, y - 2, 16, 16);
                    x += 20;
                }

                // Quantity
                gfx.DrawString($"{part.Quantity}×", _boldFont,
                    new XSolidBrush(Red),
                    new XPoint(x, y + 10));
                x += 28;

                // Part name
                string displayName = !string.IsNullOrEmpty(part.PartDescription)
                    ? part.PartDescription
                    : System.IO.Path.GetFileNameWithoutExtension(part.FileName);
                gfx.DrawString(displayName, _bodyFont,
                    new XSolidBrush(BodyText),
                    new XPoint(x, y + 10));
                x += gfx.MeasureString(displayName, _bodyFont).Width + 8;

                // Colour swatch
                if (!string.IsNullOrEmpty(part.ColourHex))
                {
                    try
                    {
                        string hex = part.ColourHex.TrimStart('#');
                        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                        byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                        var swatchColor = XColor.FromArgb(255, r, g, b);
                        gfx.DrawEllipse(new XSolidBrush(swatchColor), x, y + 2, 8, 8);
                        x += 12;
                    }
                    catch { /* skip */ }
                }

                // Colour name
                string colourLabel = !string.IsNullOrEmpty(part.ColourName)
                    ? part.ColourName : "Colour " + part.ColourCode;
                gfx.DrawString(colourLabel, _smallFont,
                    new XSolidBrush(Grey),
                    new XPoint(x, y + 9));

                y += 20;
            }

            return y + 8;
        }
    }
}
