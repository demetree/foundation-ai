using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// PDF manual builder using SimplePdf (zero external dependencies).
    /// Produces a multi-page PDF with cover, step pages, parts callouts,
    /// and a Bill of Materials summary.
    /// </summary>
    public class PdfManualBuilder : IManualDocumentBuilder
    {
        private SimplePdfDocument _doc;
        private ManualBuildPlan _plan;
        private ManualOptions _options;
        private string _modelName;
        private int _totalParts;

        // Page tracking for footers (page numbering)
        private readonly List<(SimplePdfPage page, bool isDarkBg)> _allPages
            = new List<(SimplePdfPage, bool)>();

        private double _pageWidth;
        private double _pageHeight;
        private const double Margin = 40;

        // Font sizes
        private const double TitleSize = 32;
        private const double SubtitleSize = 14;
        private const double HeaderSize = 20;
        private const double BodySize = 10;
        private const double SmallSize = 8;
        private const double TinySize = 7;

        // Colours (r, g, b)
        private static readonly (byte r, byte g, byte b) DarkNavy = (26, 26, 46);
        private static readonly (byte r, byte g, byte b) MidNavy = (22, 33, 62);
        private static readonly (byte r, byte g, byte b) Red = (229, 62, 62);
        private static readonly (byte r, byte g, byte b) Grey = (148, 163, 184);
        private static readonly (byte r, byte g, byte b) LightGrey = (226, 232, 240);
        private static readonly (byte r, byte g, byte b) BodyText = (26, 26, 46);
        private static readonly (byte r, byte g, byte b) White = (255, 255, 255);
        private static readonly (byte r, byte g, byte b) CalloutBg = (248, 250, 252);


        public void BeginDocument(string modelName, ManualBuildPlan plan, ManualOptions options)
        {
            _modelName = modelName;
            _plan = plan;
            _options = options;
            _doc = new SimplePdfDocument(
                modelName + " — Build Manual",
                "BMC Manual Generator");

            // Page dimensions (points)
            bool letter = (options.PageSize ?? "a4").ToLowerInvariant() == "letter";
            _pageWidth = letter ? 612 : 595.28;   // 8.5" or 210mm
            _pageHeight = letter ? 792 : 841.89;  // 11" or 297mm
        }


        public void AddCoverPage(byte[] finalModelImage)
        {
            var page = AddPage(isDarkBackground: true);

            // Gradient background
            page.FillGradientRect(0, 0, _pageWidth, _pageHeight,
                DarkNavy.r, DarkNavy.g, DarkNavy.b,
                MidNavy.r, MidNavy.g, MidNavy.b, diagonal: true);

            double y = _pageHeight * 0.06;

            // Title
            page.DrawTextCentered(_modelName, SimplePdfFont.Bold, TitleSize,
                Margin, y, _pageWidth - Margin * 2,
                White.r, White.g, White.b);
            y += 42;

            // Subtitle
            page.DrawTextCentered("BUILD MANUAL", SimplePdfFont.Regular, SubtitleSize,
                Margin, y, _pageWidth - Margin * 2,
                White.r, White.g, White.b, 150);
            y += 30;

            // Cover image — fill almost the entire remaining page
            if (finalModelImage != null && finalModelImage.Length > 0)
            {
                var (imgW, imgH) = SimplePdfPage.GetPngDimensions(finalModelImage);
                if (imgW > 0 && imgH > 0)
                {
                    double maxW = _pageWidth - Margin * 2;
                    double maxH = _pageHeight - y - 60; // Leave room for stats line at bottom
                    double scale = Math.Min(maxW / imgW, maxH / imgH);
                    double drawW = imgW * scale;
                    double drawH = imgH * scale;
                    double cx = _pageWidth / 2;

                    page.DrawImage(finalModelImage, cx - drawW / 2, y, drawW, drawH);
                    y += drawH + 16;
                }
            }

            // Meta info — sum NewParts across ALL steps (submodel-safe)
            int totalParts = 0;
            for (int s = 0; s < _plan.Steps.Count; s++)
                foreach (var p in _plan.Steps[s].NewParts) totalParts += p.Quantity;
            page.DrawTextCentered($"{_plan.TotalSteps} Steps · {totalParts} Parts",
                SimplePdfFont.Regular, BodySize,
                Margin, y, _pageWidth - Margin * 2,
                White.r, White.g, White.b, 130);
        }


        public void BeginSubmodelCallout(string submodelName, int totalSubmodelSteps)
        {
            // Submodel steps are rendered as individual step pages with a callout header
        }


        public void AddStep(ManualBuildStep step, byte[] stepImage,
            Dictionary<string, byte[]> partImages)
        {
            foreach (var p in step.NewParts) _totalParts += p.Quantity;

            var page = AddPage();
            double contentW = _pageWidth - Margin * 2;
            double y = Margin;

            // Submodel callout indicator
            if (step.IsSubmodelStep)
            {
                string subName = Path.GetFileNameWithoutExtension(step.ModelName);
                page.DrawText("\u203A " + subName, SimplePdfFont.Regular, SmallSize,
                    Margin, y + 10, 43, 108, 176);
                y += 18;
            }

            // Step header
            page.DrawText($"Step {step.GlobalStepIndex}", SimplePdfFont.Bold, HeaderSize,
                Margin, y + 22, Red.r, Red.g, Red.b);
            page.DrawText($"of {_plan.TotalSteps}", SimplePdfFont.Regular, BodySize,
                Margin + 120, y + 22, Grey.r, Grey.g, Grey.b);
            y += 32;

            // Separator line
            page.DrawLine(Margin, y, _pageWidth - Margin, y,
                LightGrey.r, LightGrey.g, LightGrey.b);
            y += 12;

            // Step image
            if (stepImage != null && stepImage.Length > 0)
            {
                var (imgW, imgH) = SimplePdfPage.GetPngDimensions(stepImage);
                if (imgW > 0 && imgH > 0)
                {
                    double maxImgW = contentW;
                    double maxImgH = _pageHeight * 0.5;
                    double scale = Math.Min(maxImgW / imgW, maxImgH / imgH);
                    double drawW = imgW * scale;
                    double drawH = imgH * scale;
                    double imgX = Margin + (contentW - drawW) / 2;

                    page.DrawImage(stepImage, imgX, y, drawW, drawH);
                    y += drawH + 16;
                }
            }

            // Parts callout
            if (step.NewParts != null && step.NewParts.Count > 0)
            {
                y = DrawPartsCallout(page, step.NewParts, partImages, y, contentW);
            }

            // Footer
            page.DrawTextCentered(
                $"{_modelName} · Step {step.GlobalStepIndex}/{_plan.TotalSteps}",
                SimplePdfFont.Regular, TinySize,
                Margin, _pageHeight - Margin, contentW,
                Grey.r, Grey.g, Grey.b);
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
            double contentW = _pageWidth - Margin * 2;
            double y = Margin;

            // BOM header
            page.DrawText("Bill of Materials", SimplePdfFont.Bold, HeaderSize,
                Margin, y + 22, DarkNavy.r, DarkNavy.g, DarkNavy.b);
            y += 30;

            int totalPieces = allUniqueParts.Sum(p => p.Quantity);
            page.DrawText($"{allUniqueParts.Count} unique parts · {totalPieces} total pieces",
                SimplePdfFont.Regular, BodySize,
                Margin, y + 10, Grey.r, Grey.g, Grey.b);
            y += 22;

            page.DrawLine(Margin, y, _pageWidth - Margin, y,
                LightGrey.r, LightGrey.g, LightGrey.b);
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
                    page = AddPage();
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
                    page.DrawImage(thumbPng, cellX + (cellW - 40) / 2, y, 40, 40);
                }

                // Quantity
                page.DrawTextCentered($"{part.Quantity}×", SimplePdfFont.Bold, BodySize,
                    cellX, y + 44, cellW,
                    Red.r, Red.g, Red.b);

                // Part name
                string displayName = !string.IsNullOrEmpty(part.PartDescription)
                    ? part.PartDescription
                    : Path.GetFileNameWithoutExtension(part.FileName);
                if (displayName.Length > 20)
                    displayName = displayName.Substring(0, 18) + "...";
                page.DrawTextCentered(displayName, SimplePdfFont.Regular, SmallSize,
                    cellX, y + 60, cellW,
                    BodyText.r, BodyText.g, BodyText.b);

                // Colour swatch
                if (!string.IsNullOrEmpty(part.ColourHex))
                {
                    try
                    {
                        string hex = part.ColourHex.TrimStart('#');
                        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                        byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                        page.FillEllipse(cellX + cellW / 2 - 20, y + 74, 8, 8, r, g, b);
                    }
                    catch { /* skip swatch on parse error */ }
                }

                // Colour name
                string colourLabel = !string.IsNullOrEmpty(part.ColourName)
                    ? part.ColourName : "Colour " + part.ColourCode;
                if (colourLabel.Length > 18)
                    colourLabel = colourLabel.Substring(0, 16) + "...";
                page.DrawTextCentered(colourLabel, SimplePdfFont.Regular, TinySize,
                    cellX + 10, y + 73, cellW - 10,
                    Grey.r, Grey.g, Grey.b);

                // Part ID
                page.DrawTextCentered(
                    Path.GetFileNameWithoutExtension(part.FileName),
                    SimplePdfFont.Regular, TinySize,
                    cellX, y + 86, cellW,
                    Grey.r, Grey.g, Grey.b);

                col++;
                if (col >= cols)
                {
                    col = 0;
                    y += cellH;
                }
            }
        }


        public void AddCompletionPage(byte[] completedModelImage)
        {
            var page = AddPage(isDarkBackground: true);

            // Forest green gradient background
            page.FillGradientRect(0, 0, _pageWidth, _pageHeight,
                10, 47, 31, 15, 76, 58);

            double y = 40;

            // Title
            page.DrawTextCentered(_modelName ?? "Model", SimplePdfFont.Bold, 28,
                0, y, _pageWidth,
                White.r, White.g, White.b);
            y += 38;

            // Subtitle
            page.DrawTextCentered("Build Complete!", SimplePdfFont.Regular, 16,
                0, y, _pageWidth,
                White.r, White.g, White.b, 180);
            y += 30;

            // Model image
            if (completedModelImage != null && completedModelImage.Length > 0)
            {
                var (imgW, imgH) = SimplePdfPage.GetPngDimensions(completedModelImage);
                if (imgW > 0 && imgH > 0)
                {
                    double maxW = _pageWidth - 80;
                    double maxH = _pageHeight - y - 80;
                    double scale = Math.Min(maxW / imgW, maxH / imgH);
                    double drawW = imgW * scale;
                    double drawH = imgH * scale;
                    double x = (_pageWidth - drawW) / 2;

                    page.DrawImage(completedModelImage, x, y, drawW, drawH);
                    y += drawH + 20;
                }
            }

            // Stats — sum NewParts across ALL steps (submodel-safe)
            int totalParts = 0;
            if (_plan != null)
                for (int s = 0; s < _plan.Steps.Count; s++)
                    foreach (var p in _plan.Steps[s].NewParts) totalParts += p.Quantity;
            page.DrawTextCentered(
                $"{_plan?.TotalSteps ?? 0} Steps · {totalParts} Parts",
                SimplePdfFont.Regular, 11,
                0, y, _pageWidth,
                White.r, White.g, White.b, 128);
        }


        public ManualGenerationResult Build()
        {
            // Stamp page number footers now that total count is known
            int totalPages = _allPages.Count;
            for (int i = 0; i < totalPages; i++)
            {
                var (page, isDarkBg) = _allPages[i];
                double contentW = _pageWidth - Margin * 2;

                // "Page X of Y" — white on dark backgrounds, grey on light
                byte cr = isDarkBg ? White.r : Grey.r;
                byte cg = isDarkBg ? White.g : Grey.g;
                byte cb = isDarkBg ? White.b : Grey.b;
                byte ca = isDarkBg ? (byte)130 : (byte)255;

                page.DrawTextCentered(
                    $"Page {i + 1} of {totalPages}",
                    SimplePdfFont.Regular, TinySize,
                    Margin, _pageHeight - 20, contentW,
                    cr, cg, cb, ca);
            }

            byte[] pdfBytes = _doc.Save();
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

        private SimplePdfPage AddPage(bool isDarkBackground = false)
        {
            var page = _doc.AddPage(_pageWidth, _pageHeight);
            _allPages.Add((page, isDarkBackground));
            return page;
        }


        /// <summary>
        /// Draw the parts callout at position y. Returns the new y position.
        /// </summary>
        private double DrawPartsCallout(SimplePdfPage page, List<StepPartInfo> parts,
            Dictionary<string, byte[]> partImages, double y, double contentW)
        {
            double calloutH = 6 + parts.Count * 22;

            // Background
            page.FillRoundedRect(Margin, y, contentW, calloutH, 4,
                CalloutBg.r, CalloutBg.g, CalloutBg.b, 255,
                LightGrey.r, LightGrey.g, LightGrey.b, true);

            y += 4;

            // "New Parts" label
            page.DrawText("NEW PARTS", SimplePdfFont.Regular, TinySize,
                Margin + 8, y + 8, Grey.r, Grey.g, Grey.b);
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
                    page.DrawImage(thumbPng, x, y - 2, 16, 16);
                    x += 20;
                }

                // Quantity
                page.DrawText($"{part.Quantity}×", SimplePdfFont.Bold, BodySize,
                    x, y + 10, Red.r, Red.g, Red.b);
                x += 28;

                // Part name
                string displayName = !string.IsNullOrEmpty(part.PartDescription)
                    ? part.PartDescription
                    : Path.GetFileNameWithoutExtension(part.FileName);
                page.DrawText(displayName, SimplePdfFont.Regular, BodySize,
                    x, y + 10, BodyText.r, BodyText.g, BodyText.b);
                x += page.MeasureText(displayName, SimplePdfFont.Regular, BodySize) + 8;

                // Colour swatch
                if (!string.IsNullOrEmpty(part.ColourHex))
                {
                    try
                    {
                        string hex = part.ColourHex.TrimStart('#');
                        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                        byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                        page.FillEllipse(x, y + 2, 8, 8, r, g, b);
                        x += 12;
                    }
                    catch { /* skip */ }
                }

                // Colour name
                string colourLabel = !string.IsNullOrEmpty(part.ColourName)
                    ? part.ColourName : "Colour " + part.ColourCode;
                page.DrawText(colourLabel, SimplePdfFont.Regular, SmallSize,
                    x, y + 9, Grey.r, Grey.g, Grey.b);

                y += 20;
            }

            return y + 8;
        }
    }
}
