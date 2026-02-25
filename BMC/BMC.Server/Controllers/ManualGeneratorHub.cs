using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BMC.LDraw.Render;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// SignalR hub for streaming manual generation progress step-by-step.
    ///
    /// The client uploads a file via the REST endpoint (which returns a generationId),
    /// then connects here and invokes <see cref="GenerateManual"/> to start generation.
    /// Progress is streamed via StepProgress events; the final HTML is sent via GenerationComplete.
    ///
    /// Uses bearer token authentication via the SignalR accessTokenFactory.
    /// </summary>
    [Authorize]
    public class ManualGeneratorHub : Hub
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ManualGeneratorHub> _logger;

        public ManualGeneratorHub(IConfiguration configuration, ILogger<ManualGeneratorHub> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }


        /// <summary>
        /// Client invokes this to start manual generation.
        /// Progress is pushed back via "StepProgress" as each step renders.
        /// "GenerationComplete" is sent when the HTML manual is fully assembled.
        /// "GenerationError" is sent if something goes wrong.
        /// </summary>
        public async Task GenerateManual(string generationId, ManualOptions options)
        {
            if (string.IsNullOrWhiteSpace(generationId))
            {
                await Clients.Caller.SendAsync("GenerationError", "No generation ID provided.");
                return;
            }

            if (!ManualGeneratorController.PendingFiles.TryRemove(generationId, out var entry))
            {
                await Clients.Caller.SendAsync("GenerationError", "Upload not found or expired. Please re-upload.");
                return;
            }

            string[] lines = entry.Lines;
            string fileName = entry.FileName;
            options ??= new ManualOptions();

            try
            {
                _logger.LogInformation("Manual generation started for {FileName}, connection {ConnectionId}",
                    fileName, Context.ConnectionId);

                var sw = Stopwatch.StartNew();

                string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                var renderService = new RenderService(dataPath);

                // Fast metadata-only analysis (no geometry resolution)
                var analysis = renderService.AnalyseSteps(lines, fileName);
                int totalSteps = analysis.Count;

                if (totalSteps == 0)
                {
                    await Clients.Caller.SendAsync("GenerationError", "No build steps found in file.");
                    return;
                }

                int imageSize = Math.Clamp(options.ImageSize, 256, 1024);
                float elevation = Math.Clamp(options.Elevation, -90f, 90f);
                float azimuth = Math.Clamp(options.Azimuth, -360f, 360f);

                // Collect rendered step images
                var stepImages = new List<string>(totalSteps);

                // ── Resolve the full model ONCE with per-step triangle boundaries ──
                // This also caches all part files, so no repeated file I/O.
                int[] stepTriBounds = null;
                int[] stepEdgeBounds = null;
                global::BMC.LDraw.Models.LDrawMesh fullMesh = null;

                await Task.Run(() =>
                {
                    fullMesh = renderService.ResolveFullModelWithStepBoundaries(
                        lines, fileName, out stepTriBounds, out stepEdgeBounds);

                    // Smooth the full mesh ONCE (all normals computed here)
                    if (options.SmoothShading)
                    {
                        global::BMC.LDraw.Render.NormalSmoother.Smooth(fullMesh);
                    }
                }, Context.ConnectionAborted);

                for (int i = 0; i < totalSteps; i++)
                {
                    Context.ConnectionAborted.ThrowIfCancellationRequested();

                    // Render step from pre-smoothed mesh — NO per-step resolution or smoothing
                    var result = await Task.Run(() =>
                        renderService.RenderStepFromPreSmoothedMesh(
                            fullMesh, i, stepTriBounds, stepEdgeBounds,
                            imageSize, imageSize, elevation, azimuth,
                            options.RenderEdges, options.SmoothShading),
                        Context.ConnectionAborted);

                    string base64 = Convert.ToBase64String(result.PngBytes);
                    stepImages.Add(base64);

                    // Stream progress to client in real time
                    await Clients.Caller.SendAsync("StepProgress", new
                    {
                        step = i + 1,
                        total = totalSteps,
                        previewBase64 = base64
                    }, Context.ConnectionAborted);
                }


                // Assemble the HTML manual
                string html = AssembleHtml(fileName, analysis, stepImages, options);

                sw.Stop();

                await Clients.Caller.SendAsync("GenerationComplete", new
                {
                    html = html,
                    totalSteps = totalSteps,
                    totalParts = analysis.LastOrDefault()?.CumulativePartCount ?? 0,
                    renderTimeMs = (int)sw.ElapsedMilliseconds
                }, Context.ConnectionAborted);

                _logger.LogInformation("Manual generation complete for {FileName}: {Steps} steps in {Ms}ms",
                    fileName, totalSteps, sw.ElapsedMilliseconds);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Manual generation cancelled for {FileName}, connection {ConnectionId}",
                    fileName, Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manual generation error for {FileName}", fileName);
                await Clients.Caller.SendAsync("GenerationError", "Generation failed: " + ex.Message);
            }
        }


        /// <summary>
        /// Assemble a self-contained HTML document with embedded step images.
        /// Uses CSS @page rules for print-ready paper formatting.
        /// </summary>
        private static string AssembleHtml(string fileName,
                                            List<ManualStepAnalysis> analysis,
                                            List<string> stepImages,
                                            ManualOptions options)
        {
            string modelName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            string pageSize = (options.PageSize ?? "a4").ToLowerInvariant() == "letter" ? "letter" : "A4";

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"UTF-8\">");
            sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendFormat("<title>{0} — Build Manual</title>\n", System.Net.WebUtility.HtmlEncode(modelName));
            sb.AppendLine("<style>");
            sb.AppendLine(GetManualCss(pageSize));
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Cover page
            sb.AppendLine("<div class=\"page cover-page\">");
            sb.AppendLine("<div class=\"cover-content\">");
            sb.AppendFormat("<h1 class=\"cover-title\">{0}</h1>\n", System.Net.WebUtility.HtmlEncode(modelName));
            sb.AppendFormat("<div class=\"cover-meta\">Build Manual · {0} Steps · {1} Parts</div>\n",
                analysis.Count, analysis.LastOrDefault()?.CumulativePartCount ?? 0);
            if (stepImages.Count > 0)
            {
                // Show the final completed model on the cover
                sb.AppendFormat("<img class=\"cover-image\" src=\"data:image/png;base64,{0}\" alt=\"Completed model\" />\n",
                    stepImages[stepImages.Count - 1]);
            }
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            // Step pages
            for (int i = 0; i < analysis.Count; i++)
            {
                var step = analysis[i];
                sb.AppendLine("<div class=\"page step-page\">");

                // Header
                sb.AppendFormat("<div class=\"step-header\"><span class=\"step-number\">Step {0}</span>",
                    step.StepIndex + 1);
                sb.AppendFormat("<span class=\"step-of\">of {0}</span></div>\n", analysis.Count);

                // Main build image
                if (i < stepImages.Count)
                {
                    sb.AppendFormat("<div class=\"step-image\"><img src=\"data:image/png;base64,{0}\" alt=\"Step {1}\" /></div>\n",
                        stepImages[i], step.StepIndex + 1);
                }

                // New parts callout
                if (step.NewParts.Count > 0)
                {
                    sb.AppendLine("<div class=\"parts-callout\">");
                    sb.AppendLine("<div class=\"parts-callout-title\">New Parts</div>");
                    sb.AppendLine("<div class=\"parts-list\">");
                    foreach (var part in step.NewParts)
                    {
                        sb.AppendFormat("<div class=\"part-item\"><span class=\"part-qty\">{0}×</span>",
                            part.Quantity);
                        sb.AppendFormat("<span class=\"part-name\">{0}</span>",
                            System.Net.WebUtility.HtmlEncode(part.FileName));
                        sb.AppendFormat("<span class=\"part-colour\">Colour {0}</span></div>\n",
                            part.ColourCode);
                    }
                    sb.AppendLine("</div>");
                    sb.AppendLine("</div>");
                }

                // Footer
                sb.AppendFormat("<div class=\"step-footer\">{0} · Step {1}/{2}</div>\n",
                    System.Net.WebUtility.HtmlEncode(modelName), step.StepIndex + 1, analysis.Count);

                sb.AppendLine("</div>");
            }

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }


        /// <summary>
        /// CSS for the self-contained HTML manual.
        /// Includes @page rules for print-friendly paper formatting.
        /// </summary>
        private static string GetManualCss(string pageSize)
        {
            return @"
*, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

@page {
    size: " + pageSize + @";
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
}
";
        }
    }
}
