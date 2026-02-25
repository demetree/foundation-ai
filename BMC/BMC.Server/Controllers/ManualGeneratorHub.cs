using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BMC.LDraw.Render;
using BMC.LDraw.Models;
using Foundation.BMC.Database;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// SignalR hub for streaming manual generation progress step-by-step.
    ///
    /// Phases:
    /// 1. Recursive step analysis (submodel-aware)
    /// 2. Database enrichment (part descriptions + colour names)
    /// 3. Pre-resolve and pre-smooth meshes
    /// 4. Premium cover render (gradient + SSAA4x)
    /// 5. Parallel PLI thumbnail pre-rendering
    /// 6. Per-step rendering with smart page fill
    /// 7. Bill of Materials aggregation
    /// </summary>
    [Authorize]
    public class ManualGeneratorHub : Hub
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ManualGeneratorHub> _logger;
        private readonly BMCContext _db;

        public ManualGeneratorHub(
            IConfiguration configuration,
            ILogger<ManualGeneratorHub> logger,
            BMCContext db)
        {
            _configuration = configuration;
            _logger = logger;
            _db = db;
        }


        /// <summary>
        /// Client invokes this to start manual generation.
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

                // ── Phase 1: Recursive step analysis ──
                var plan = renderService.AnalyseStepsRecursive(lines, fileName);
                int totalSteps = plan.TotalSteps;

                if (totalSteps == 0)
                {
                    await Clients.Caller.SendAsync("GenerationError", "No build steps found in file.");
                    return;
                }

                int imageSize = Math.Clamp(options.ImageSize, 256, 1024);
                float elevation = Math.Clamp(options.Elevation, -90f, 90f);
                float azimuth = Math.Clamp(options.Azimuth, -360f, 360f);


                // ── Phase 2: Enrich parts with DB lookups ──
                await EnrichPartsFromDatabase(plan);


                // ── Phase 3: Pre-resolve and pre-smooth meshes ──
                int[] rootTriBounds = null;
                int[] rootEdgeBounds = null;
                LDrawMesh rootMesh = null;

                var submodelMeshes = new Dictionary<string, (LDrawMesh mesh, int[] triBounds, int[] edgeBounds)>(
                    StringComparer.OrdinalIgnoreCase);

                await Task.Run(() =>
                {
                    rootMesh = renderService.ResolveFullModelWithStepBoundaries(
                        lines, fileName, out rootTriBounds, out rootEdgeBounds);

                    if (options.SmoothShading)
                    {
                        NormalSmoother.Smooth(rootMesh);
                    }

                    foreach (string subName in plan.SubmodelsWithSteps)
                    {
                        var (mesh, triBounds, edgeBounds) =
                            renderService.ResolveSubmodelWithStepBoundaries(
                                lines, fileName, subName);

                        if (mesh.Triangles.Count > 0)
                        {
                            if (options.SmoothShading)
                            {
                                NormalSmoother.Smooth(mesh);
                            }
                            submodelMeshes[subName] = (mesh, triBounds, edgeBounds);
                        }
                    }
                }, Context.ConnectionAborted);


                // ── Phase 4: Premium cover render ──
                // Render the full model (no step slicing) with gradient + SSAA4x
                Context.ConnectionAborted.ThrowIfCancellationRequested();
                byte[] coverImage = await Task.Run(() =>
                    renderService.RenderToPng(
                        lines: lines,
                        fileName: fileName,
                        width: Math.Max(imageSize, 768),       // Higher res for cover
                        height: Math.Max(imageSize, 768),
                        elevation: elevation,
                        azimuth: azimuth,
                        renderEdges: true,
                        smoothShading: true,
                        antiAliasMode: AntiAliasMode.SSAA4x,   // Premium quality
                        gradientTopHex: "#1A1A2E",             // Deep navy
                        gradientBottomHex: "#0F3460"),         // Rich dark blue
                    Context.ConnectionAborted);


                // ── Phase 5: Parallel PLI thumbnail pre-rendering ──
                Context.ConnectionAborted.ThrowIfCancellationRequested();
                var pliCache = new PartImageCache();
                await Task.Run(() =>
                    pliCache.PreRenderAll(renderService, plan, Context.ConnectionAborted),
                    Context.ConnectionAborted);

                var partImages = pliCache.GetAll();

                _logger.LogInformation(
                    "PLI pre-render complete: {Count} unique part thumbnails (parallel)",
                    partImages.Count);


                // ── Phase 5.5: Pre-compute auto-rotation for root steps ──
                // Analyse where new parts are added relative to model centre
                // and derive an optimal camera azimuth per step.
                for (int i = 0; i < totalSteps; i++)
                {
                    var step = plan.Steps[i];
                    if (step.IsSubmodelStep) continue;             // submodels use their own mesh
                    if (step.RotStep != null) continue;            // author-specified rotation wins
                    if (step.LocalStepIndex >= rootTriBounds.Length) continue;

                    step.AutoAzimuth = RenderService.ComputeAutoAzimuth(
                        rootMesh, rootTriBounds, step.LocalStepIndex, azimuth, 0.6f);
                }


                // ── Phase 6: Render each step and build document ──
                bool isPdf = string.Equals(options.OutputFormat, "pdf",
                    StringComparison.OrdinalIgnoreCase);

                IManualDocumentBuilder builder = isPdf
                    ? (IManualDocumentBuilder)new PdfManualBuilder()
                    : new HtmlManualBuilder();

                builder.BeginDocument(
                    Path.GetFileNameWithoutExtension(fileName),
                    plan, options);

                builder.AddCoverPage(coverImage);

                // Track callout state
                string currentCalloutModel = null;

                for (int i = 0; i < totalSteps; i++)
                {
                    Context.ConnectionAborted.ThrowIfCancellationRequested();

                    var step = plan.Steps[i];

                    // Handle callout transitions
                    if (step.IsSubmodelStep && currentCalloutModel != step.ModelName)
                    {
                        if (currentCalloutModel != null)
                        {
                            builder.EndSubmodelCallout();
                        }
                        int subStepCount = plan.Steps.Count(s => s.ModelName == step.ModelName);
                        builder.BeginSubmodelCallout(step.ModelName, subStepCount);
                        currentCalloutModel = step.ModelName;
                    }
                    else if (!step.IsSubmodelStep && currentCalloutModel != null)
                    {
                        builder.EndSubmodelCallout();
                        currentCalloutModel = null;
                    }

                    // Render the step image
                    byte[] stepImageBytes = null;

                    if (step.IsSubmodelStep
                        && submodelMeshes.TryGetValue(step.ModelName, out var subData))
                    {
                        float stepElevation = elevation;
                        float stepAzimuth = azimuth;
                        ApplyRotStep(step.RotStep, ref stepElevation, ref stepAzimuth,
                            elevation, azimuth);

                        var result = await Task.Run(() =>
                            renderService.RenderStepFromPreSmoothedMesh(
                                subData.mesh, step.LocalStepIndex,
                                subData.triBounds, subData.edgeBounds,
                                imageSize, imageSize,
                                stepElevation, stepAzimuth,
                                options.RenderEdges, options.SmoothShading),
                            Context.ConnectionAborted);
                        stepImageBytes = result.PngBytes;
                    }
                    else if (!step.IsSubmodelStep
                             && step.LocalStepIndex < rootTriBounds.Length)
                    {
                        float stepElevation = elevation;
                        // Use auto-computed azimuth if available, otherwise default
                        float stepAzimuth = step.AutoAzimuth ?? azimuth;
                        ApplyRotStep(step.RotStep, ref stepElevation, ref stepAzimuth,
                            elevation, stepAzimuth);

                        var result = await Task.Run(() =>
                            renderService.RenderStepFromPreSmoothedMesh(
                                rootMesh, step.LocalStepIndex,
                                rootTriBounds, rootEdgeBounds,
                                imageSize, imageSize,
                                stepElevation, stepAzimuth,
                                options.RenderEdges, options.SmoothShading),
                            Context.ConnectionAborted);
                        stepImageBytes = result.PngBytes;
                    }

                    builder.AddStep(step, stepImageBytes, partImages);

                    // Stream progress to client
                    string previewB64 = stepImageBytes != null
                        ? Convert.ToBase64String(stepImageBytes) : null;

                    await Clients.Caller.SendAsync("StepProgress", new
                    {
                        step = i + 1,
                        total = totalSteps,
                        previewBase64 = previewB64
                    }, Context.ConnectionAborted);
                }

                // Close any remaining callout
                if (currentCalloutModel != null)
                {
                    builder.EndSubmodelCallout();
                }

                // ── Phase 7: Bill of Materials ──
                var bomParts = AggregateBom(plan);
                builder.AddBillOfMaterials(bomParts, partImages);

                // Finalize document
                var finalResult = builder.Build();

                sw.Stop();

                if (isPdf)
                {
                    // Send PDF as base64
                    string pdfBase64 = finalResult.DocumentBytes != null
                        ? Convert.ToBase64String(finalResult.DocumentBytes) : null;

                    await Clients.Caller.SendAsync("GenerationComplete", new
                    {
                        format = "pdf",
                        pdfBase64 = pdfBase64,
                        totalSteps = finalResult.TotalSteps,
                        totalParts = finalResult.TotalParts,
                        renderTimeMs = (int)sw.ElapsedMilliseconds
                    }, Context.ConnectionAborted);
                }
                else
                {
                    await Clients.Caller.SendAsync("GenerationComplete", new
                    {
                        format = "html",
                        html = finalResult.Html,
                        totalSteps = finalResult.TotalSteps,
                        totalParts = finalResult.TotalParts,
                        renderTimeMs = (int)sw.ElapsedMilliseconds
                    }, Context.ConnectionAborted);
                }

                _logger.LogInformation(
                    "Manual generation complete for {FileName}: {Format}, {Steps} steps, {Submodels} submodels, {PliCount} PLI in {Ms}ms",
                    fileName, isPdf ? "PDF" : "HTML", totalSteps,
                    plan.SubmodelsWithSteps.Count, partImages.Count,
                    sw.ElapsedMilliseconds);
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
        /// Enrich all StepPartInfo entries with part descriptions,
        /// colour names, and hex values from the BMC database.
        /// </summary>
        private async Task EnrichPartsFromDatabase(ManualBuildPlan plan)
        {
            var allPartIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var allColourCodes = new HashSet<int>();

            foreach (var step in plan.Steps)
            {
                if (step.NewParts == null) continue;
                foreach (var p in step.NewParts)
                {
                    allPartIds.Add(Path.GetFileNameWithoutExtension(p.FileName));
                    allColourCodes.Add(p.ColourCode);
                }
            }

            // Batch query: part descriptions
            var partLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (allPartIds.Count > 0)
            {
                try
                {
                    var dbParts = await _db.BrickParts
                        .AsNoTracking()
                        .Where(bp => bp.active && !bp.deleted
                            && bp.ldrawPartId != null
                            && allPartIds.Contains(bp.ldrawPartId))
                        .Select(bp => new { bp.ldrawPartId, bp.name, bp.ldrawTitle })
                        .ToListAsync();

                    foreach (var bp in dbParts)
                    {
                        // Prefer ldrawTitle (descriptive), fall back to name
                        string desc = !string.IsNullOrEmpty(bp.ldrawTitle) ? bp.ldrawTitle : bp.name;
                        if (!string.IsNullOrEmpty(desc) && !partLookup.ContainsKey(bp.ldrawPartId))
                        {
                            partLookup[bp.ldrawPartId] = desc;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to lookup part descriptions from database");
                }
            }

            // Batch query: colour names + hex values
            var colourLookup = new Dictionary<int, (string name, string hex)>();
            if (allColourCodes.Count > 0)
            {
                try
                {
                    var dbColours = await _db.BrickColours
                        .AsNoTracking()
                        .Where(bc => bc.active && !bc.deleted
                            && allColourCodes.Contains(bc.ldrawColourCode))
                        .Select(bc => new { bc.ldrawColourCode, bc.name, bc.hexRgb })
                        .ToListAsync();

                    foreach (var bc in dbColours)
                    {
                        if (!colourLookup.ContainsKey(bc.ldrawColourCode))
                        {
                            colourLookup[bc.ldrawColourCode] = (bc.name, bc.hexRgb);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to lookup colours from database");
                }
            }

            // Apply enrichment to all steps
            foreach (var step in plan.Steps)
            {
                if (step.NewParts == null) continue;
                foreach (var p in step.NewParts)
                {
                    string partId = Path.GetFileNameWithoutExtension(p.FileName);

                    if (partLookup.TryGetValue(partId, out string desc))
                        p.PartDescription = desc;

                    if (colourLookup.TryGetValue(p.ColourCode, out var colourInfo))
                    {
                        p.ColourName = colourInfo.name;
                        p.ColourHex = colourInfo.hex;
                    }
                }
            }
        }


        /// <summary>
        /// Aggregate all unique part × colour combinations across the entire plan
        /// into a single BOM list with total quantities.
        /// </summary>
        private static List<StepPartInfo> AggregateBom(ManualBuildPlan plan)
        {
            var bomDict = new Dictionary<string, StepPartInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var step in plan.Steps)
            {
                if (step.NewParts == null) continue;
                foreach (var p in step.NewParts)
                {
                    string key = PartImageCache.MakeKey(p.FileName, p.ColourCode);
                    if (bomDict.TryGetValue(key, out StepPartInfo existing))
                    {
                        existing.Quantity += p.Quantity;
                    }
                    else
                    {
                        bomDict[key] = new StepPartInfo
                        {
                            FileName = p.FileName,
                            ColourCode = p.ColourCode,
                            Quantity = p.Quantity,
                            PartDescription = p.PartDescription,
                            ColourName = p.ColourName,
                            ColourHex = p.ColourHex
                        };
                    }
                }
            }

            var result = new List<StepPartInfo>(bomDict.Values);
            result.Sort((a, b) =>
            {
                int cmp = string.Compare(a.ColourName ?? "", b.ColourName ?? "",
                    StringComparison.OrdinalIgnoreCase);
                if (cmp != 0) return cmp;
                return string.Compare(
                    a.PartDescription ?? a.FileName,
                    b.PartDescription ?? b.FileName,
                    StringComparison.OrdinalIgnoreCase);
            });

            return result;
        }


        /// <summary>
        /// Apply a ROTSTEP camera rotation override.
        /// </summary>
        private static void ApplyRotStep(RotStepData rotStep,
            ref float elevation, ref float azimuth,
            float defaultElevation, float defaultAzimuth)
        {
            if (rotStep == null) return;

            switch (rotStep.Type)
            {
                case "ABS":
                    elevation = rotStep.X;
                    azimuth = rotStep.Y;
                    break;
                case "REL":
                    elevation = defaultElevation + rotStep.X;
                    azimuth = defaultAzimuth + rotStep.Y;
                    break;
                case "ADD":
                    elevation += rotStep.X;
                    azimuth += rotStep.Y;
                    break;
            }
        }
    }
}
