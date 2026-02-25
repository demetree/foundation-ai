using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BMC.LDraw.Render;
using BMC.LDraw.Models;

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

                // ── Phase 1: Recursive step analysis (submodel-aware) ──
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


                // ── Phase 2: Pre-resolve and pre-smooth meshes ──
                // Root model mesh (with per-step boundaries)
                int[] rootTriBounds = null;
                int[] rootEdgeBounds = null;
                LDrawMesh rootMesh = null;

                // Per-submodel meshes (with per-step boundaries)
                var submodelMeshes = new Dictionary<string, (LDrawMesh mesh, int[] triBounds, int[] edgeBounds)>(
                    StringComparer.OrdinalIgnoreCase);

                await Task.Run(() =>
                {
                    // Resolve root model
                    rootMesh = renderService.ResolveFullModelWithStepBoundaries(
                        lines, fileName, out rootTriBounds, out rootEdgeBounds);

                    if (options.SmoothShading)
                    {
                        NormalSmoother.Smooth(rootMesh);
                    }

                    // Resolve each submodel that has its own steps
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


                // ── Phase 3: Render each step and build document ──
                IManualDocumentBuilder builder = new HtmlManualBuilder();
                builder.BeginDocument(
                    System.IO.Path.GetFileNameWithoutExtension(fileName),
                    plan, options);

                // Render the final model image for the cover
                Context.ConnectionAborted.ThrowIfCancellationRequested();
                byte[] coverImage = null;
                if (rootMesh.Triangles.Count > 0)
                {
                    var lastRootStep = plan.Steps.LastOrDefault(s => !s.IsSubmodelStep);
                    int coverStepIdx = lastRootStep != null ? lastRootStep.LocalStepIndex : rootTriBounds.Length - 1;
                    if (coverStepIdx < rootTriBounds.Length)
                    {
                        var coverResult = await Task.Run(() =>
                            renderService.RenderStepFromPreSmoothedMesh(
                                rootMesh, coverStepIdx, rootTriBounds, rootEdgeBounds,
                                imageSize, imageSize, elevation, azimuth,
                                options.RenderEdges, options.SmoothShading),
                            Context.ConnectionAborted);
                        coverImage = coverResult.PngBytes;
                    }
                }
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
                        // Close previous callout if switching submodels
                        if (currentCalloutModel != null)
                        {
                            builder.EndSubmodelCallout();
                        }

                        // Count steps in this submodel
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
                        && submodelMeshes.TryGetValue(step.ModelName,
                            out var subData))
                    {
                        // Render from submodel's pre-smoothed mesh
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
                        // Render from root model's pre-smoothed mesh
                        float stepElevation = elevation;
                        float stepAzimuth = azimuth;
                        ApplyRotStep(step.RotStep, ref stepElevation, ref stepAzimuth,
                            elevation, azimuth);

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

                    builder.AddStep(step, stepImageBytes);

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

                // Finalize document
                var finalResult = builder.Build();

                sw.Stop();

                await Clients.Caller.SendAsync("GenerationComplete", new
                {
                    html = finalResult.Html,
                    totalSteps = finalResult.TotalSteps,
                    totalParts = finalResult.TotalParts,
                    renderTimeMs = (int)sw.ElapsedMilliseconds
                }, Context.ConnectionAborted);

                _logger.LogInformation(
                    "Manual generation complete for {FileName}: {Steps} steps ({SubmodelCount} submodels) in {Ms}ms",
                    fileName, totalSteps, plan.SubmodelsWithSteps.Count, sw.ElapsedMilliseconds);
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
