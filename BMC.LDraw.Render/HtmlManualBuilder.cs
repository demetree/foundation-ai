using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Builds a self-contained HTML document from a sequence of build steps and images.
    /// Implements the IManualDocumentBuilder contract.
    ///
    /// Features:
    /// - Smart multi-step pages (small steps share a page, complex steps go solo)
    /// - Submodel callouts rendered as bordered inset sections
    /// - PLI (Parts List Images) with rendered thumbnails and colour swatches
    /// - Bill of Materials summary page
    /// - Gradient cover page
    /// - Step numbering and print-ready @page CSS
    /// </summary>
    public class HtmlManualBuilder : IManualDocumentBuilder
    {
        private StringBuilder _sb;
        private ManualBuildPlan _plan;
        private ManualOptions _options;
        private string _modelName;
        private string _pageSize;
        private int _totalParts;
        private int _pageCount;

        // Multi-step page state
        private bool _pageOpen;
        private int _stepsOnCurrentPage;
        private const int MaxStepsPerPage = 2;
        private const int SmallStepPartThreshold = 3;


        /// <summary>
        /// Start a new HTML document.
        /// </summary>
        public void BeginDocument(string modelName, ManualBuildPlan plan, ManualOptions options)
        {
            _modelName = modelName;
            _plan = plan;
            _options = options;
            _pageSize = (options.PageSize ?? "a4").ToLowerInvariant() == "letter" ? "letter" : "A4";
            _pageOpen = false;
            _stepsOnCurrentPage = 0;

            _sb = new StringBuilder();
            _sb.AppendLine("<!DOCTYPE html>");
            _sb.AppendLine("<html lang=\"en\">");
            _sb.AppendLine("<head>");
            _sb.AppendLine("<meta charset=\"UTF-8\">");
            _sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            _sb.AppendFormat("<title>{0} — Build Manual</title>\n",
                System.Net.WebUtility.HtmlEncode(modelName));
            _sb.AppendLine("<style>");
            _sb.AppendLine(GetManualCss());
            _sb.AppendLine("</style>");
            _sb.AppendLine("</head>");
            _sb.AppendLine("<body>");
        }


        /// <summary>
        /// Add the cover page showing the completed model with gradient background.
        /// </summary>
        public void AddCoverPage(byte[] finalModelImage)
        {
            _pageCount++;
            _sb.AppendLine("<div class=\"page cover-page\">");
            _sb.AppendLine("<div class=\"cover-content\">");
            _sb.AppendFormat("<h1 class=\"cover-title\">{0}</h1>\n",
                System.Net.WebUtility.HtmlEncode(_modelName));
            _sb.AppendFormat("<div class=\"cover-subtitle\">Build Manual</div>\n");

            if (finalModelImage != null && finalModelImage.Length > 0)
            {
                string b64 = Convert.ToBase64String(finalModelImage);
                _sb.AppendFormat("<img class=\"cover-image\" src=\"data:image/png;base64,{0}\" alt=\"Completed model\" />\n", b64);
            }

            _sb.AppendFormat("<div class=\"cover-meta\">{0} Steps · {1} Parts</div>\n",
                _plan.TotalSteps,
                _plan.Steps.Count > 0
                    ? _plan.Steps[_plan.Steps.Count - 1].CumulativePartCount
                    : 0);
            _sb.AppendLine("</div>");
            _sb.AppendFormat("<div class=\"page-number page-number-light\">Page {0} of {{TOTAL_PAGES}}</div>\n", _pageCount);
            _sb.AppendLine("</div>");
        }

        /// <summary>
        /// Add a completion page showing the finished model as a full-page beauty shot.
        /// Always added after the last step.
        /// </summary>
        public void AddCompletionPage(byte[] completedModelImage)
        {
            CloseCurrentPage();
            _pageCount++;
            _sb.AppendLine("<div class=\"page completion-page\">");
            _sb.AppendLine("<div class=\"completion-content\">");
            _sb.AppendFormat("<h1 class=\"completion-title\">{0}</h1>\n",
                System.Net.WebUtility.HtmlEncode(_modelName));
            _sb.AppendLine("<div class=\"completion-subtitle\">Build Complete!</div>");

            if (completedModelImage != null && completedModelImage.Length > 0)
            {
                string b64 = Convert.ToBase64String(completedModelImage);
                _sb.AppendFormat("<img class=\"completion-image\" src=\"data:image/png;base64,{0}\" alt=\"Completed model\" />\n", b64);
            }

            int totalParts = _plan.Steps.Count > 0
                ? _plan.Steps[_plan.Steps.Count - 1].CumulativePartCount
                : 0;
            _sb.AppendFormat("<div class=\"completion-meta\">{0} Steps · {1} Parts</div>\n",
                _plan.TotalSteps, totalParts);
            _sb.AppendLine("</div>");
            _sb.AppendFormat("<div class=\"page-number page-number-light\">Page {0} of {{TOTAL_PAGES}}</div>\n", _pageCount);
            _sb.AppendLine("</div>");
        }


        // ── Submodel callout state ──
        private string _currentSubmodelName;
        private int _totalSubmodelSteps;
        private int _submodelStepsOnPage;
        private const int MaxSubStepsPerPage = 2;

        /// <summary>
        /// Begin a submodel callout section.
        /// Sub-steps will be batched into pages of MaxSubStepsPerPage.
        /// </summary>
        public void BeginSubmodelCallout(string submodelName, int totalSubmodelSteps)
        {
            CloseCurrentPage();
            _currentSubmodelName = submodelName;
            _totalSubmodelSteps = totalSubmodelSteps;
            _submodelStepsOnPage = 0;
            OpenSubmodelPage();
        }

        private void OpenSubmodelPage()
        {
            _pageCount++;
            _sb.AppendLine("<div class=\"page step-page\">");
            _sb.AppendLine("<div class=\"submodel-callout\">");
            _sb.AppendFormat("<div class=\"submodel-callout-header\">{0}</div>\n",
                System.Net.WebUtility.HtmlEncode(
                    System.IO.Path.GetFileNameWithoutExtension(_currentSubmodelName)));
            _sb.AppendFormat("<div class=\"submodel-callout-meta\">{0} step{1}</div>\n",
                _totalSubmodelSteps, _totalSubmodelSteps == 1 ? "" : "s");
        }


        /// <summary>
        /// Add a build step page or submodel step to the document.
        /// Small root steps (≤3 parts) can share a page in a side-by-side layout.
        /// </summary>
        public void AddStep(ManualBuildStep step, byte[] stepImage,
            Dictionary<string, byte[]> partImages)
        {
            _totalParts = Math.Max(_totalParts, step.CumulativePartCount);

            if (step.IsSubmodelStep)
            {
                AddSubmodelStep(step, stepImage, partImages);
            }
            else
            {
                AddRootStep(step, stepImage, partImages);
            }
        }


        /// <summary>
        /// End the current submodel callout section.
        /// </summary>
        public void EndSubmodelCallout()
        {
            _sb.AppendLine("</div>"); // close .submodel-callout
            _sb.AppendFormat("<div class=\"page-number\">Page {0} of {{TOTAL_PAGES}}</div>\n", _pageCount);
            _sb.AppendLine("</div>"); // close .page
            _currentSubmodelName = null;
            _submodelStepsOnPage = 0;
        }


        /// <summary>
        /// Add paginated Bill of Materials pages.
        /// Items are chunked into pages of BomItemsPerPage.
        /// </summary>
        private const int BomItemsPerPage = 15;

        public void AddBillOfMaterials(List<StepPartInfo> allUniqueParts,
            Dictionary<string, byte[]> partImages)
        {
            if (allUniqueParts == null || allUniqueParts.Count == 0) return;

            CloseCurrentPage();

            int totalUnique = allUniqueParts.Count;
            int totalPieces = allUniqueParts.Sum(p => p.Quantity);
            int pageCount = (int)Math.Ceiling((double)totalUnique / BomItemsPerPage);

            for (int pg = 0; pg < pageCount; pg++)
            {
                _pageCount++;
                _sb.AppendLine("<div class=\"page bom-page\">");
                _sb.AppendLine("<div class=\"bom-header\">");

                if (pageCount > 1)
                {
                    _sb.AppendFormat("<h2 class=\"bom-title\">Bill of Materials ({0}/{1})</h2>\n",
                        pg + 1, pageCount);
                }
                else
                {
                    _sb.AppendLine("<h2 class=\"bom-title\">Bill of Materials</h2>");
                }

                _sb.AppendFormat("<div class=\"bom-meta\">{0} unique parts · {1} total pieces</div>\n",
                    totalUnique, totalPieces);
                _sb.AppendLine("</div>");

                _sb.AppendLine("<div class=\"bom-grid\">");

                int start = pg * BomItemsPerPage;
                int end = Math.Min(start + BomItemsPerPage, totalUnique);

                for (int idx = start; idx < end; idx++)
                {
                    var part = allUniqueParts[idx];
                    _sb.AppendLine("<div class=\"bom-item\">");

                    // Thumbnail
                    string imgKey = PartImageCache.MakeKey(part.FileName, part.ColourCode);
                    byte[] thumbPng = null;
                    if (partImages != null)
                        partImages.TryGetValue(imgKey, out thumbPng);

                    if (thumbPng != null && thumbPng.Length > 0)
                    {
                        string b64 = Convert.ToBase64String(thumbPng);
                        _sb.AppendFormat("<img class=\"bom-thumb\" src=\"data:image/png;base64,{0}\" alt=\"{1}\" />\n",
                            b64, System.Net.WebUtility.HtmlEncode(part.FileName));
                    }
                    else
                    {
                        _sb.AppendLine("<div class=\"bom-thumb bom-thumb-placeholder\"></div>");
                    }

                    // Quantity badge
                    _sb.AppendFormat("<div class=\"bom-qty\">{0}×</div>\n", part.Quantity);

                    // Part info
                    _sb.AppendLine("<div class=\"bom-info\">");
                    string displayName = !string.IsNullOrEmpty(part.PartDescription)
                        ? part.PartDescription
                        : System.IO.Path.GetFileNameWithoutExtension(part.FileName);
                    _sb.AppendFormat("<div class=\"bom-part-name\">{0}</div>\n",
                        System.Net.WebUtility.HtmlEncode(displayName));

                    // Colour swatch + name
                    _sb.AppendLine("<div class=\"bom-colour-row\">");
                    if (!string.IsNullOrEmpty(part.ColourHex))
                    {
                        _sb.AppendFormat("<span class=\"colour-swatch\" style=\"background:{0}\"></span>\n",
                            part.ColourHex);
                    }
                    string colourLabel = !string.IsNullOrEmpty(part.ColourName)
                        ? part.ColourName
                        : "Colour " + part.ColourCode;
                    _sb.AppendFormat("<span class=\"bom-colour-name\">{0}</span>\n",
                        System.Net.WebUtility.HtmlEncode(colourLabel));
                    _sb.AppendLine("</div>");

                    // Part ID
                    _sb.AppendFormat("<div class=\"bom-part-id\">{0}</div>\n",
                        System.Net.WebUtility.HtmlEncode(
                            System.IO.Path.GetFileNameWithoutExtension(part.FileName)));

                    _sb.AppendLine("</div>"); // bom-info
                    _sb.AppendLine("</div>"); // bom-item
                }

                _sb.AppendLine("</div>"); // bom-grid
                _sb.AppendFormat("<div class=\"page-number\">Page {0} of {{TOTAL_PAGES}}</div>\n", _pageCount);
                _sb.AppendLine("</div>"); // bom-page
            }
        }


        /// <summary>
        /// Finalize the HTML document and return the result.
        /// </summary>
        public ManualGenerationResult Build()
        {
            CloseCurrentPage();

            // Replace page-total placeholder now that all pages are emitted
            _sb.Replace("{TOTAL_PAGES}", _pageCount.ToString());

            _sb.AppendLine("</body>");
            _sb.AppendLine("</html>");

            return new ManualGenerationResult
            {
                Html = _sb.ToString(),
                TotalSteps = _plan.TotalSteps,
                TotalParts = _totalParts
            };
        }


        // ═══════════════════════════════════════════════════════════════
        //  Private helpers
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Determine whether a step is "small" enough to share a page.
        /// </summary>
        private static bool IsSmallStep(ManualBuildStep step)
        {
            int partCount = step.NewParts != null ? step.NewParts.Count : 0;
            return partCount <= SmallStepPartThreshold && !step.IsSubmodelStep;
        }

        /// <summary>
        /// Add a root-level step. Small steps may share a page in
        /// a side-by-side layout; complex steps get a full page.
        /// </summary>
        private void AddRootStep(ManualBuildStep step, byte[] stepImage,
            Dictionary<string, byte[]> partImages)
        {
            bool small = IsSmallStep(step);

            if (small && _pageOpen && _stepsOnCurrentPage < MaxStepsPerPage)
            {
                // Append to the current multi-step row
                AddStepCell(step, stepImage, partImages);
                _stepsOnCurrentPage++;
            }
            else
            {
                // Close previous page
                CloseCurrentPage();

                if (small)
                {
                    // Start a multi-step row page
                    _pageCount++;
                    _sb.AppendLine("<div class=\"page step-page\">");
                    _sb.AppendFormat("<div class=\"page-header\"><span class=\"page-header-model\">{0}</span></div>\n",
                        System.Net.WebUtility.HtmlEncode(_modelName));
                    _sb.AppendLine("<div class=\"multi-step-row\">");
                    AddStepCell(step, stepImage, partImages);
                    _pageOpen = true;
                    _stepsOnCurrentPage = 1;
                }
                else
                {
                    // Full page for this step
                    _pageCount++;
                    _sb.AppendLine("<div class=\"page step-page\">");
                    _sb.AppendFormat("<div class=\"step-header\"><span class=\"step-number\">Step {0}</span>",
                        step.GlobalStepIndex);
                    _sb.AppendFormat("<span class=\"step-of\">of {0}</span></div>\n", _plan.TotalSteps);

                    if (stepImage != null && stepImage.Length > 0)
                    {
                        string b64 = Convert.ToBase64String(stepImage);
                        _sb.AppendFormat("<div class=\"step-image\"><img src=\"data:image/png;base64,{0}\" alt=\"Step {1}\" /></div>\n",
                            b64, step.GlobalStepIndex);
                    }

                    AddPartsCallout(step.NewParts, partImages);

                    _sb.AppendFormat("<div class=\"step-footer\">{0} · Step {1}/{2}</div>\n",
                        System.Net.WebUtility.HtmlEncode(_modelName),
                        step.GlobalStepIndex, _plan.TotalSteps);
                    _sb.AppendFormat("<div class=\"page-number\">Page {0} of {{TOTAL_PAGES}}</div>\n", _pageCount);
                    _sb.AppendLine("</div>");
                }
            }
        }

        /// <summary>
        /// Add a step "cell" within a multi-step row.
        /// </summary>
        private void AddStepCell(ManualBuildStep step, byte[] stepImage,
            Dictionary<string, byte[]> partImages)
        {
            _sb.AppendLine("<div class=\"step-cell\">");
            _sb.AppendFormat("<div class=\"step-cell-header\"><span class=\"step-number\">Step {0}</span>",
                step.GlobalStepIndex);
            _sb.AppendFormat("<span class=\"step-of\">of {0}</span></div>\n", _plan.TotalSteps);

            if (stepImage != null && stepImage.Length > 0)
            {
                string b64 = Convert.ToBase64String(stepImage);
                _sb.AppendFormat("<div class=\"step-cell-image\"><img src=\"data:image/png;base64,{0}\" alt=\"Step {1}\" /></div>\n",
                    b64, step.GlobalStepIndex);
            }

            AddPartsCallout(step.NewParts, partImages);
            _sb.AppendLine("</div>");
        }

        /// <summary>
        /// Add a submodel step inside a callout section.
        /// Pages are split when MaxSubStepsPerPage is reached.
        /// </summary>
        private void AddSubmodelStep(ManualBuildStep step, byte[] stepImage,
            Dictionary<string, byte[]> partImages)
        {
            // Start a new callout page if we've hit the limit
            if (_submodelStepsOnPage >= MaxSubStepsPerPage)
            {
                _sb.AppendLine("</div>"); // close .submodel-callout
                _sb.AppendFormat("<div class=\"page-number\">Page {0} of {{TOTAL_PAGES}}</div>\n", _pageCount);
                _sb.AppendLine("</div>"); // close .page
                OpenSubmodelPage();
                _submodelStepsOnPage = 0;
            }

            _sb.AppendFormat("<div class=\"submodel-step\">");
            _sb.AppendFormat("<div class=\"submodel-step-num\">Sub-step {0}</div>\n",
                step.LocalStepIndex + 1);

            if (stepImage != null && stepImage.Length > 0)
            {
                string b64 = Convert.ToBase64String(stepImage);
                _sb.AppendFormat("<div class=\"submodel-step-image\"><img src=\"data:image/png;base64,{0}\" alt=\"Sub-step {1}\" /></div>\n",
                    b64, step.LocalStepIndex + 1);
            }

            // Compact inline parts list for submodel steps (saves vertical space)
            if (step.NewParts != null && step.NewParts.Count > 0)
            {
                _sb.AppendLine("<div class=\"submodel-parts-inline\">");
                for (int p = 0; p < step.NewParts.Count; p++)
                {
                    var part = step.NewParts[p];
                    string displayName = !string.IsNullOrEmpty(part.PartDescription)
                        ? part.PartDescription
                        : System.IO.Path.GetFileNameWithoutExtension(part.FileName);

                    _sb.Append("<span class=\"sub-part-chip\">");

                    // Small PLI thumbnail
                    string imgKey = PartImageCache.MakeKey(part.FileName, part.ColourCode);
                    byte[] thumbPng = null;
                    if (partImages != null)
                        partImages.TryGetValue(imgKey, out thumbPng);
                    if (thumbPng != null && thumbPng.Length > 0)
                    {
                        string b64 = Convert.ToBase64String(thumbPng);
                        _sb.AppendFormat("<img class=\"sub-part-thumb\" src=\"data:image/png;base64,{0}\" alt=\"\" />",
                            b64);
                    }
                    else if (!string.IsNullOrEmpty(part.ColourHex))
                    {
                        _sb.AppendFormat("<span class=\"colour-swatch\" style=\"background:{0}\"></span>",
                            part.ColourHex);
                    }

                    _sb.AppendFormat("<span class=\"sub-part-qty\">{0}×</span> ", part.Quantity);
                    _sb.AppendFormat("{0}</span>\n",
                        System.Net.WebUtility.HtmlEncode(displayName));
                }
                _sb.AppendLine("</div>");
            }

            _sb.AppendLine("</div>");
            _submodelStepsOnPage++;
        }

        /// <summary>
        /// Close the currently open multi-step page, if any.
        /// </summary>
        private void CloseCurrentPage()
        {
            if (_pageOpen)
            {
                _sb.AppendLine("</div>"); // close .multi-step-row
                _sb.AppendFormat("<div class=\"page-number\">Page {0} of {{TOTAL_PAGES}}</div>\n", _pageCount);
                _sb.AppendLine("</div>"); // close .page
                _pageOpen = false;
                _stepsOnCurrentPage = 0;
            }
        }


        /// <summary>
        /// Render a parts callout section with PLI thumbnails and colour swatches.
        /// </summary>
        private void AddPartsCallout(List<StepPartInfo> parts,
            Dictionary<string, byte[]> partImages)
        {
            if (parts == null || parts.Count == 0) return;

            _sb.AppendLine("<div class=\"parts-callout\">");
            _sb.AppendLine("<div class=\"parts-callout-title\">New Parts</div>");
            _sb.AppendLine("<div class=\"parts-list\">");
            foreach (var part in parts)
            {
                _sb.AppendLine("<div class=\"part-item\">");

                // PLI thumbnail
                string imgKey = PartImageCache.MakeKey(part.FileName, part.ColourCode);
                byte[] thumbPng = null;
                if (partImages != null)
                    partImages.TryGetValue(imgKey, out thumbPng);

                if (thumbPng != null && thumbPng.Length > 0)
                {
                    string b64 = Convert.ToBase64String(thumbPng);
                    _sb.AppendFormat("<img class=\"pli-thumbnail\" src=\"data:image/png;base64,{0}\" alt=\"{1}\" />\n",
                        b64, System.Net.WebUtility.HtmlEncode(part.FileName));
                }

                // Quantity badge
                _sb.AppendFormat("<span class=\"part-qty\">{0}×</span>", part.Quantity);

                // Part description (friendly name or filename fallback)
                string displayName = !string.IsNullOrEmpty(part.PartDescription)
                    ? part.PartDescription
                    : System.IO.Path.GetFileNameWithoutExtension(part.FileName);
                _sb.AppendFormat("<span class=\"part-name\">{0}</span>",
                    System.Net.WebUtility.HtmlEncode(displayName));

                // Colour swatch + name
                if (!string.IsNullOrEmpty(part.ColourHex))
                {
                    _sb.AppendFormat("<span class=\"colour-swatch\" style=\"background:{0}\"></span>",
                        part.ColourHex);
                }
                string colourLabel = !string.IsNullOrEmpty(part.ColourName)
                    ? part.ColourName
                    : "Colour " + part.ColourCode;
                _sb.AppendFormat("<span class=\"part-colour\">{0}</span>",
                    System.Net.WebUtility.HtmlEncode(colourLabel));

                _sb.AppendLine("</div>");
            }
            _sb.AppendLine("</div>");
            _sb.AppendLine("</div>");
        }


        /// <summary>
        /// CSS for the self-contained HTML manual.
        /// </summary>
        private string GetManualCss()
        {
            return @"
*, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

@page {
    size: " + _pageSize + @";
    margin: 12mm;
}

body {
    font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
    background: #f0f2f5;
    color: #1a1a2e;
}

.page {
    width: 100%;
    max-width: 210mm;
    margin: 20px auto;
    background: #ffffff;
    box-shadow: 0 2px 12px rgba(0,0,0,0.08);
    border-radius: 8px;
    padding: 24px;
    page-break-after: always;
    min-height: 280mm;
    display: flex;
    flex-direction: column;
}

/* ═══ Cover Page ═══ */
.cover-page {
    background: linear-gradient(160deg, #1A1A2E 0%, #16213E 40%, #0F3460 100%);
    justify-content: center;
    align-items: center;
    text-align: center;
    color: #ffffff;
    position: relative;
    overflow: hidden;
}
.cover-page::before {
    content: '';
    position: absolute;
    top: -50%;
    left: -50%;
    width: 200%;
    height: 200%;
    background: radial-gradient(circle at 30% 40%, rgba(255,255,255,0.03) 0%, transparent 60%);
    pointer-events: none;
}
.cover-content {
    max-width: 85%;
    position: relative;
    z-index: 1;
}
.cover-title {
    font-size: 2.8em;
    font-weight: 800;
    letter-spacing: -0.03em;
    margin-bottom: 4px;
    text-shadow: 0 2px 20px rgba(0,0,0,0.3);
}
.cover-subtitle {
    font-size: 1.2em;
    font-weight: 300;
    letter-spacing: 0.15em;
    text-transform: uppercase;
    color: rgba(255,255,255,0.6);
    margin-bottom: 28px;
}
.cover-image {
    max-width: 100%;
    max-height: 420px;
    border-radius: 12px;
    box-shadow: 0 8px 40px rgba(0,0,0,0.4);
    margin-bottom: 24px;
}
.cover-meta {
    font-size: 1em;
    color: rgba(255,255,255,0.5);
    letter-spacing: 0.05em;
}

/* ═══ Completion Page ═══ */
.completion-page {
    background: linear-gradient(135deg, #0f766e 0%, #065f46 40%, #064e3b 100%);
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    text-align: center;
    color: #ffffff;
    position: relative;
    overflow: hidden;
}
.completion-page::before {
    content: '';
    position: absolute;
    top: -50%;
    left: -50%;
    width: 200%;
    height: 200%;
    background: radial-gradient(circle at 70% 60%, rgba(255,255,255,0.04) 0%, transparent 60%);
    pointer-events: none;
}
.completion-content {
    max-width: 90%;
    position: relative;
    z-index: 1;
}
.completion-title {
    font-size: 2.2em;
    font-weight: 800;
    letter-spacing: -0.02em;
    margin-bottom: 2px;
    text-shadow: 0 2px 16px rgba(0,0,0,0.3);
}
.completion-subtitle {
    font-size: 1.4em;
    font-weight: 300;
    letter-spacing: 0.12em;
    text-transform: uppercase;
    color: rgba(255,255,255,0.7);
    margin-bottom: 24px;
}
.completion-image {
    max-width: 100%;
    max-height: 520px;
    border-radius: 12px;
    box-shadow: 0 8px 40px rgba(0,0,0,0.4);
    margin-bottom: 20px;
}
.completion-meta {
    font-size: 1em;
    color: rgba(255,255,255,0.5);
    letter-spacing: 0.05em;
}

/* ═══ Step Pages ═══ */
.step-header, .step-cell-header {
    display: flex;
    align-items: baseline;
    gap: 8px;
    margin-bottom: 16px;
    padding-bottom: 8px;
    border-bottom: 2px solid #e2e8f0;
}
.step-number {
    font-size: 1.6em;
    font-weight: 700;
    color: #e53e3e;
}
.step-of {
    font-size: 1em;
    color: #94a3b8;
}

.page-header {
    display: flex;
    align-items: center;
    padding-bottom: 8px;
    margin-bottom: 12px;
    border-bottom: 1px solid #e2e8f0;
}
.page-header-model {
    font-size: 0.85em;
    color: #94a3b8;
}

.step-image {
    flex: 1;
    display: flex;
    justify-content: center;
    align-items: center;
    margin: 12px 0;
}
.step-image img {
    max-width: 100%;
    max-height: 650px;
    border-radius: 6px;
}

/* ═══ Multi-Step Row ═══ */
.multi-step-row {
    display: flex;
    gap: 20px;
    flex: 1;
}
.step-cell {
    flex: 1;
    display: flex;
    flex-direction: column;
    border: 1px solid #e2e8f0;
    border-radius: 10px;
    padding: 16px;
    background: #fafbfc;
}
.step-cell-header {
    margin-bottom: 10px;
    padding-bottom: 6px;
}
.step-cell-image {
    flex: 1;
    display: flex;
    justify-content: center;
    align-items: center;
    margin-bottom: 10px;
}
.step-cell-image img {
    max-width: 100%;
    max-height: 300px;
    border-radius: 4px;
}

/* ═══ Submodel Callout ═══ */
.submodel-callout {
    border: 2px solid #3182ce;
    border-radius: 12px;
    background: #ebf8ff;
    padding: 16px 20px;
    margin: 12px 0;
}
.submodel-callout-header {
    font-size: 1.1em;
    font-weight: 700;
    color: #2b6cb0;
    margin-bottom: 4px;
}
.submodel-callout-meta {
    font-size: 0.8em;
    color: #4a90d9;
    margin-bottom: 12px;
}
.submodel-step {
    background: #ffffff;
    border: 1px solid #bee3f8;
    border-radius: 8px;
    padding: 12px;
    margin-bottom: 10px;
}
.submodel-step-num {
    font-weight: 600;
    font-size: 0.9em;
    color: #2b6cb0;
    margin-bottom: 8px;
}
.submodel-step-image {
    display: flex;
    justify-content: center;
    margin-bottom: 8px;
}
.submodel-step-image img {
    max-width: 100%;
    max-height: 200px;
    border-radius: 4px;
}

/* Compact inline parts list for submodel steps */
.submodel-parts-inline {
    display: flex;
    flex-wrap: wrap;
    gap: 4px 8px;
    padding: 6px 10px;
    background: #f1f5f9;
    border-radius: 6px;
    font-size: 0.78em;
    color: #475569;
    line-height: 1.6;
}
.sub-part-chip {
    display: inline-flex;
    align-items: center;
    gap: 3px;
    white-space: nowrap;
}
.sub-part-qty {
    font-weight: 700;
    color: #e53e3e;
}
.sub-part-thumb {
    width: 28px;
    height: 28px;
    object-fit: contain;
    border-radius: 3px;
    background: #ffffff;
    flex-shrink: 0;
}

/* ═══ Parts Callout (PLI) ═══ */
.parts-callout {
    background: #f8fafc;
    border: 1px solid #e2e8f0;
    border-radius: 8px;
    padding: 12px 16px;
    margin-top: auto;
}
.parts-callout-title {
    font-weight: 600;
    font-size: 0.85em;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: #64748b;
    margin-bottom: 8px;
}
.parts-list {
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
}
.part-item {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    background: #ffffff;
    border: 1px solid #e2e8f0;
    border-radius: 8px;
    padding: 6px 12px;
    font-size: 0.85em;
}
.pli-thumbnail {
    width: 48px;
    height: 48px;
    object-fit: contain;
    border-radius: 4px;
    background: #f1f5f9;
    flex-shrink: 0;
}
.part-qty {
    font-weight: 700;
    color: #e53e3e;
    font-size: 1.1em;
    min-width: 24px;
}
.part-name {
    font-weight: 500;
    color: #1a1a2e;
}
.colour-swatch {
    display: inline-block;
    width: 12px;
    height: 12px;
    border-radius: 50%;
    border: 1px solid rgba(0,0,0,0.15);
    flex-shrink: 0;
}
.part-colour {
    color: #64748b;
    font-size: 0.85em;
}

.step-footer {
    margin-top: 12px;
    padding-top: 8px;
    border-top: 1px solid #e2e8f0;
    font-size: 0.75em;
    color: #94a3b8;
    text-align: center;
}

.page-number {
    margin-top: auto;
    padding-top: 8px;
    font-size: 0.7em;
    color: #94a3b8;
    text-align: center;
    letter-spacing: 0.03em;
}
.page-number-light {
    color: rgba(255,255,255,0.4);
}

/* ═══ Bill of Materials ═══ */
.bom-page {
    page-break-before: always;
}
.bom-header {
    margin-bottom: 20px;
    padding-bottom: 12px;
    border-bottom: 2px solid #e2e8f0;
}
.bom-title {
    font-size: 1.8em;
    font-weight: 700;
    color: #16213e;
    margin-bottom: 4px;
}
.bom-meta {
    font-size: 0.95em;
    color: #64748b;
}
.bom-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
    gap: 12px;
}
.bom-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    text-align: center;
    background: #f8fafc;
    border: 1px solid #e2e8f0;
    border-radius: 10px;
    padding: 12px;
    gap: 4px;
}
.bom-thumb {
    width: 80px;
    height: 80px;
    object-fit: contain;
    border-radius: 6px;
    background: #f1f5f9;
}
.bom-thumb-placeholder {
    border: 1px dashed #cbd5e1;
}
.bom-qty {
    font-weight: 700;
    font-size: 1.2em;
    color: #e53e3e;
}
.bom-info {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 2px;
}
.bom-part-name {
    font-weight: 600;
    font-size: 0.85em;
    color: #1a1a2e;
}
.bom-colour-row {
    display: flex;
    align-items: center;
    gap: 4px;
}
.bom-colour-name {
    font-size: 0.8em;
    color: #64748b;
}
.bom-part-id {
    font-size: 0.75em;
    color: #94a3b8;
    font-family: monospace;
}

/* ═══ Print ═══ */
@media print {
    body { background: none; }
    .page {
        box-shadow: none;
        border-radius: 0;
        margin: 0;
        max-width: none;
        min-height: auto;
    }
    .cover-page {
        -webkit-print-color-adjust: exact;
        print-color-adjust: exact;
    }
    .submodel-step { break-inside: avoid; }
    .parts-callout { break-inside: avoid; }
    .bom-item { break-inside: avoid; }
    .step-cell { break-inside: avoid; }
}
";
        }
    }
}
