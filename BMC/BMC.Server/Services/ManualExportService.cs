using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BMC.LDraw.Render;
using Foundation.BMC.Database;

namespace Foundation.BMC.Services
{
    /// <summary>
    /// Exports a BuildManual from the database as PDF or HTML
    /// by feeding stored step/PLI images through the existing
    /// HtmlManualBuilder / PdfManualBuilder pipeline.
    ///
    /// No re-rendering is needed — images are already stored
    /// as base64 data URIs in BuildManualStep.renderImagePath / pliImagePath.
    /// </summary>
    public class ManualExportService
    {
        private readonly BMCContext _db;
        private readonly ILogger _logger;

        public ManualExportService(BMCContext db, ILogger logger)
        {
            _db = db;
            _logger = logger;
        }


        /// <summary>
        /// Export a manual as HTML or PDF.
        /// </summary>
        /// <param name="manualId">BuildManual.id</param>
        /// <param name="format">"html" or "pdf"</param>
        /// <returns>The generated document result.</returns>
        public async Task<ManualGenerationResult> ExportAsync(int manualId, string format)
        {
            //
            // Step 1: Load all data from DB in minimal queries
            //
            var manual = await _db.BuildManuals
                .AsNoTracking()
                .Include(m => m.project)
                .FirstOrDefaultAsync(m => m.id == manualId && !m.deleted);

            if (manual == null)
                throw new InvalidOperationException($"Manual {manualId} not found.");

            var pages = await _db.BuildManualPages
                .AsNoTracking()
                .Where(p => p.buildManualId == manualId && !p.deleted && p.active)
                .OrderBy(p => p.pageNum)
                .ToListAsync();

            var pageIds = pages.Select(p => p.id).ToList();

            var steps = await _db.BuildManualSteps
                .AsNoTracking()
                .Include(s => s.BuildStepParts)
                    .ThenInclude(bsp => bsp.placedBrick)
                        .ThenInclude(pb => pb.brickPart)
                .Include(s => s.BuildStepParts)
                    .ThenInclude(bsp => bsp.placedBrick)
                        .ThenInclude(pb => pb.brickColour)
                .Where(s => pageIds.Contains(s.buildManualPageId) && !s.deleted && s.active)
                .ToListAsync();

            string modelName = manual.project?.name ?? manual.name ?? "Manual";

            _logger.LogInformation(
                "Exporting manual {ManualId} '{Name}' as {Format}: {Pages} pages, {Steps} steps",
                manualId, modelName, format, pages.Count, steps.Count);


            //
            // Step 2: Build a ManualBuildPlan from DB data
            //
            var plan = new ManualBuildPlan { ModelName = modelName };
            var allPartImages = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

            // Flatten steps in page order, then step order within page
            int globalIndex = 1;
            foreach (var page in pages)
            {
                var pageSteps = steps
                    .Where(s => s.buildManualPageId == page.id)
                    .OrderBy(s => s.stepNumber)
                    .ToList();

                foreach (var dbStep in pageSteps)
                {
                    var buildStep = new ManualBuildStep
                    {
                        GlobalStepIndex = globalIndex++,
                        LocalStepIndex = dbStep.stepNumber ?? 0,
                        ModelName = dbStep.calloutModelName ?? modelName,
                        IsSubmodelStep = dbStep.isCallout,
                        SubmodelDepth = dbStep.calloutNestingDepth ?? 0,
                        NewParts = new List<StepPartInfo>()
                    };

                    //
                    // For each BuildStepPart junction, extract the part info
                    // for PLI display and BOM aggregation.
                    //
                    if (dbStep.BuildStepParts != null)
                    {
                        var partGroups = dbStep.BuildStepParts
                            .Where(bsp => bsp.placedBrick != null && !bsp.deleted && bsp.active)
                            .GroupBy(bsp => new
                            {
                                FileName = bsp.placedBrick.brickPart?.ldrawPartId ?? "unknown",
                                ColourCode = bsp.placedBrick.brickColour?.ldrawColourCode ?? 0
                            });

                        foreach (var group in partGroups)
                        {
                            var firstBrick = group.First().placedBrick;
                            buildStep.NewParts.Add(new StepPartInfo
                            {
                                FileName = group.Key.FileName + ".dat",
                                ColourCode = group.Key.ColourCode,
                                Quantity = group.Count(),
                                PartDescription = firstBrick.brickPart?.ldrawTitle
                                    ?? firstBrick.brickPart?.name,
                                ColourName = firstBrick.brickColour?.name,
                                ColourHex = firstBrick.brickColour?.hexRgb
                            });
                        }
                    }

                    //
                    // Extract PLI thumbnail images from base64 data URIs.
                    // The pliImagePath stores one composite image per step — but the
                    // HtmlManualBuilder expects per-part thumbnails in a dictionary.
                    // For now, we skip individual PLI lookups (the step image has parts inlined).
                    //

                    plan.Steps.Add(buildStep);
                }
            }


            //
            // Step 3: Configure options from the manual's page settings
            //
            var options = new ManualOptions
            {
                PageSize = GetPageSizeString(manual.pageWidthMm, manual.pageHeightMm),
                OutputFormat = format
            };


            //
            // Step 4: Build the document using Foundation builders
            //
            bool isPdf = string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase);

            IManualDocumentBuilder builder = isPdf
                ? (IManualDocumentBuilder)new PdfManualBuilder()
                : new HtmlManualBuilder();

            builder.BeginDocument(modelName, plan, options);

            // No cover page render stored — skip cover for now
            // (Could re-render or use a stored project thumbnail in the future)

            foreach (var step in plan.Steps)
            {
                byte[] stepImageBytes = null;

                // Find the matching DB step to get the stored renderImagePath
                var dbStep = steps.FirstOrDefault(s =>
                    s.stepNumber == step.LocalStepIndex &&
                    (step.IsSubmodelStep
                        ? s.calloutModelName == step.ModelName
                        : !s.isCallout));

                if (dbStep?.renderImagePath != null)
                {
                    stepImageBytes = DecodeBase64DataUri(dbStep.renderImagePath);
                }

                builder.AddStep(step, stepImageBytes, allPartImages);
            }

            // BOM from aggregated parts
            var bomParts = AggregateBom(plan);
            builder.AddBillOfMaterials(bomParts, allPartImages);

            var result = builder.Build();

            _logger.LogInformation(
                "Manual export complete: {Format}, {Steps} steps, {Parts} parts",
                format, result.TotalSteps, result.TotalParts);

            return result;
        }


        /// <summary>
        /// Decode a base64 data URI (e.g. "data:image/png;base64,iVBOR...")
        /// into raw bytes. Returns null if the input is not a data URI.
        /// </summary>
        private static byte[] DecodeBase64DataUri(string dataUri)
        {
            if (string.IsNullOrEmpty(dataUri))
                return null;

            const string marker = ";base64,";
            int idx = dataUri.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0)
                return null;

            string base64 = dataUri.Substring(idx + marker.Length);
            try
            {
                return Convert.FromBase64String(base64);
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Determine page size string from manual dimensions.
        /// </summary>
        private static string GetPageSizeString(float? widthMm, float? heightMm)
        {
            if (widthMm.HasValue && heightMm.HasValue)
            {
                // Letter: 216 x 279 mm
                if (Math.Abs(widthMm.Value - 216) < 5 && Math.Abs(heightMm.Value - 279) < 5)
                    return "letter";
            }
            return "a4";
        }


        /// <summary>
        /// Aggregate all unique part × colour combinations for BOM.
        /// </summary>
        private static List<StepPartInfo> AggregateBom(ManualBuildPlan plan)
        {
            var bomDict = new Dictionary<string, StepPartInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var step in plan.Steps)
            {
                if (step.NewParts == null) continue;
                foreach (var p in step.NewParts)
                {
                    string key = $"{p.FileName}|{p.ColourCode}";
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
    }
}
