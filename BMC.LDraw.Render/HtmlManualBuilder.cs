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
    /// Supports submodel callouts rendered as bordered inset sections,
    /// step numbering, parts lists, cover page, and print-ready @page CSS.
    /// </summary>
    public class HtmlManualBuilder : IManualDocumentBuilder
    {
        private StringBuilder _sb;
        private ManualBuildPlan _plan;
        private ManualOptions _options;
        private string _modelName;
        private string _pageSize;
        private int _totalParts;


        /// <summary>
        /// Start a new HTML document.
        /// </summary>
        public void BeginDocument(string modelName, ManualBuildPlan plan, ManualOptions options)
        {
            _modelName = modelName;
            _plan = plan;
            _options = options;
            _pageSize = (options.PageSize ?? "a4").ToLowerInvariant() == "letter" ? "letter" : "A4";

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
        /// Add the cover page showing the completed model.
        /// </summary>
        public void AddCoverPage(byte[] finalModelImage)
        {
            _sb.AppendLine("<div class=\"page cover-page\">");
            _sb.AppendLine("<div class=\"cover-content\">");
            _sb.AppendFormat("<h1 class=\"cover-title\">{0}</h1>\n",
                System.Net.WebUtility.HtmlEncode(_modelName));
            _sb.AppendFormat("<div class=\"cover-meta\">Build Manual · {0} Steps · {1} Parts</div>\n",
                _plan.TotalSteps,
                _plan.Steps.Count > 0
                    ? _plan.Steps[_plan.Steps.Count - 1].CumulativePartCount
                    : 0);

            if (finalModelImage != null && finalModelImage.Length > 0)
            {
                string b64 = Convert.ToBase64String(finalModelImage);
                _sb.AppendFormat("<img class=\"cover-image\" src=\"data:image/png;base64,{0}\" alt=\"Completed model\" />\n", b64);
            }
            _sb.AppendLine("</div>");
            _sb.AppendLine("</div>");
        }


        /// <summary>
        /// Begin a submodel callout section — a bordered inset area in the HTML.
        /// </summary>
        public void BeginSubmodelCallout(string submodelName, int totalSubmodelSteps)
        {
            _sb.AppendLine("<div class=\"page step-page\">");
            _sb.AppendLine("<div class=\"submodel-callout\">");
            _sb.AppendFormat("<div class=\"submodel-callout-header\">{0}</div>\n",
                System.Net.WebUtility.HtmlEncode(
                    System.IO.Path.GetFileNameWithoutExtension(submodelName)));
            _sb.AppendFormat("<div class=\"submodel-callout-meta\">{0} step{1}</div>\n",
                totalSubmodelSteps, totalSubmodelSteps == 1 ? "" : "s");
        }


        /// <summary>
        /// Add a build step page or submodel step to the document.
        /// </summary>
        public void AddStep(ManualBuildStep step, byte[] stepImage)
        {
            _totalParts = Math.Max(_totalParts, step.CumulativePartCount);

            if (step.IsSubmodelStep)
            {
                // Submodel steps render inside the callout as compact entries
                _sb.AppendLine("<div class=\"submodel-step\">");
                _sb.AppendFormat("<div class=\"submodel-step-num\">Step {0}</div>\n",
                    step.GlobalStepIndex);

                if (stepImage != null && stepImage.Length > 0)
                {
                    string b64 = Convert.ToBase64String(stepImage);
                    _sb.AppendFormat("<div class=\"submodel-step-image\"><img src=\"data:image/png;base64,{0}\" alt=\"Step {1}\" /></div>\n",
                        b64, step.GlobalStepIndex);
                }

                AddPartsCallout(step.NewParts);
                _sb.AppendLine("</div>");
            }
            else
            {
                // Root steps get their own full page
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

                AddPartsCallout(step.NewParts);

                _sb.AppendFormat("<div class=\"step-footer\">{0} · Step {1}/{2}</div>\n",
                    System.Net.WebUtility.HtmlEncode(_modelName),
                    step.GlobalStepIndex, _plan.TotalSteps);
                _sb.AppendLine("</div>");
            }
        }


        /// <summary>
        /// End the current submodel callout section.
        /// </summary>
        public void EndSubmodelCallout()
        {
            _sb.AppendLine("</div>"); // close .submodel-callout
            _sb.AppendLine("</div>"); // close .page
        }


        /// <summary>
        /// Finalize the HTML document and return the result.
        /// </summary>
        public ManualGenerationResult Build()
        {
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
        /// Render a parts callout section for new parts in a step.
        /// </summary>
        private void AddPartsCallout(List<StepPartInfo> parts)
        {
            if (parts == null || parts.Count == 0) return;

            _sb.AppendLine("<div class=\"parts-callout\">");
            _sb.AppendLine("<div class=\"parts-callout-title\">New Parts</div>");
            _sb.AppendLine("<div class=\"parts-list\">");
            foreach (var part in parts)
            {
                _sb.AppendFormat("<div class=\"part-item\"><span class=\"part-qty\">{0}×</span>",
                    part.Quantity);
                _sb.AppendFormat("<span class=\"part-name\">{0}</span>",
                    System.Net.WebUtility.HtmlEncode(part.FileName));
                _sb.AppendFormat("<span class=\"part-colour\">Colour {0}</span></div>\n",
                    part.ColourCode);
            }
            _sb.AppendLine("</div>");
            _sb.AppendLine("</div>");
        }


        /// <summary>
        /// CSS for the self-contained HTML manual.
        /// Includes @page rules for print-friendly paper formatting
        /// and submodel callout styling.
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

/* Cover Page */
.cover-page {
    justify-content: center;
    align-items: center;
    text-align: center;
}
.cover-content { max-width: 80%; }
.cover-title {
    font-size: 2.4em;
    font-weight: 700;
    color: #16213e;
    margin-bottom: 12px;
    letter-spacing: -0.02em;
}
.cover-meta {
    font-size: 1.1em;
    color: #64748b;
    margin-bottom: 32px;
}
.cover-image {
    max-width: 100%;
    max-height: 400px;
    border-radius: 8px;
    box-shadow: 0 4px 20px rgba(0,0,0,0.12);
}

/* Step Pages */
.step-header {
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

.step-image {
    flex: 1;
    display: flex;
    justify-content: center;
    align-items: center;
    margin: 12px 0;
}
.step-image img {
    max-width: 100%;
    max-height: 500px;
    border-radius: 6px;
}

/* Submodel Callout */
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
    max-height: 300px;
    border-radius: 4px;
}

/* Parts Callout */
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
    gap: 8px;
}
.part-item {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    background: #ffffff;
    border: 1px solid #e2e8f0;
    border-radius: 6px;
    padding: 4px 10px;
    font-size: 0.85em;
}
.part-qty {
    font-weight: 700;
    color: #e53e3e;
}
.part-name {
    font-weight: 500;
    color: #1a1a2e;
}
.part-colour {
    color: #94a3b8;
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

/* Print optimizations */
@media print {
    body { background: none; }
    .page {
        box-shadow: none;
        border-radius: 0;
        margin: 0;
        max-width: none;
        min-height: auto;
    }
    .submodel-callout {
        break-inside: avoid;
    }
}
";
        }
    }
}
