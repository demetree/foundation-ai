using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Foundation.BMC.Database;

using BMC.LDraw.Models;
using BMC.LDraw.Parsers;

namespace Foundation.BMC.Services
{
    /// <summary>
    ///
    /// Service that exports BMC Project entities to model files (.ldr, .mpd, .io).
    ///
    /// This is the reverse of ModelImportService:
    ///   - Reads PlacedBrick entities and converts quaternion rotation back to 3x3 matrix
    ///   - Generates LDraw-format text output with proper STEP separators
    ///   - For MPD format, includes Submodel entities as FILE/NOFILE blocks
    ///   - For .io format, wraps the LDraw content in a password-protected ZIP with thumbnail
    ///
    /// Supports two export paths:
    ///   1. "Native" export â€” reconstructs LDraw from PlacedBrick entities
    ///   2. "Round-trip" export â€” uses the stored ModelDocument.sourceFileData when available
    ///
    /// AI-developed code â€” initial implementation March 2026
    ///
    /// </summary>
    public class ModelExportService
    {
        //
        // Constants
        //
        private const string FORMAT_LDR = "ldr";
        private const string FORMAT_MPD = "mpd";
        private const string FORMAT_IO = "io";
        private const string FORMAT_LXF = "lxf";


        //
        // Dependencies
        //
        private readonly BMCContext _context;
        private readonly LDrawFileService _ldrawFiles;
        private readonly ILogger<ModelExportService> _logger;


        /// <summary>
        /// Constructor â€” takes the BMC database context, LDraw file service, and a logger.
        /// </summary>
        public ModelExportService(BMCContext context, LDrawFileService ldrawFiles, ILogger<ModelExportService> logger)
        {
            _context = context;
            _ldrawFiles = ldrawFiles;
            _logger = logger;
        }


        #region Result DTO

        /// <summary>
        /// Result returned after a successful export.
        /// </summary>
        public class ExportResult
        {
            public byte[] FileData { get; set; }
            public string FileName { get; set; }
            public string MimeType { get; set; }
            public string Format { get; set; }
        }

        #endregion


        /// <summary>
        ///
        /// Export a project as a BrickLink Studio .io file.
        ///
        /// If the project has a stored .io ModelDocument (from a previous import), 
        /// performs a round-trip export using the original archive data.
        /// Otherwise, generates LDraw content from PlacedBrick entities and wraps it
        /// in a new .io archive.
        ///
        /// </summary>
        public async Task<ExportResult> ExportToIoAsync(
            int projectId,
            Guid tenantGuid,
            CancellationToken cancellationToken = default)
        {
            //
            // Check if we have a stored .io document for round-trip
            //
            ModelDocument ioDocument = await _context.ModelDocuments
                .Where(md => md.projectId == projectId
                          && md.tenantGuid == tenantGuid
                          && md.sourceFormat == FORMAT_IO
                          && md.sourceFileData != null
                          && md.active == true
                          && md.deleted == false)
                .OrderByDescending(md => md.id)
                .FirstOrDefaultAsync(cancellationToken);

            if (ioDocument != null && ioDocument.sourceFileData != null)
            {
                //
                // Round-trip: parse the stored .io, update LDraw content from current PlacedBricks,
                // then re-package
                //
                _logger.LogInformation("Round-trip .io export for project {Id} using stored document", projectId);

                try
                {
                    StudioIoResult parseResult = StudioIoParser.ParseBytes(ioDocument.sourceFileData, ioDocument.sourceFileName ?? "export.io");

                    //
                    // Generate updated LDraw content from current entities
                    //
                    string updatedLDraw = await GenerateLDrawContentAsync(projectId, tenantGuid, false, cancellationToken);

                    //
                    // Update thumbnail if project has one
                    //
                    Project project = await _context.Projects
                        .Where(p => p.id == projectId && p.tenantGuid == tenantGuid)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (project != null && project.thumbnailData != null)
                    {
                        parseResult.ThumbnailData = project.thumbnailData;
                    }

                    byte[] exportData = StudioIoWriter.WriteFromParseResult(parseResult, updatedLDraw);

                    return new ExportResult
                    {
                        FileData = exportData,
                        FileName = Path.GetFileNameWithoutExtension(ioDocument.sourceFileName ?? "export") + ".io",
                        MimeType = "application/x-studioio",
                        Format = FORMAT_IO
                    };
                }
                catch (Exception ex)
                {
                    //
                    // If round-trip fails, fall through to native export
                    //
                    _logger.LogWarning(ex, "Round-trip .io export failed for project {Id}, falling back to native export", projectId);
                }
            }

            //
            // Native export: generate LDraw and wrap in a new .io archive
            //
            _logger.LogInformation("Native .io export for project {Id}", projectId);

            string lDrawContent = await GenerateLDrawContentAsync(projectId, tenantGuid, false, cancellationToken);
            Project proj = await _context.Projects
                .Where(p => p.id == projectId && p.tenantGuid == tenantGuid)
                .FirstOrDefaultAsync(cancellationToken);

            string projectName = proj?.name ?? "export";

            StudioIoWriteRequest writeRequest = new StudioIoWriteRequest
            {
                LDrawContent = lDrawContent,
                ThumbnailData = proj?.thumbnailData,
                StudioVersion = "2.0.0.0",
                PartCount = proj?.partCount,
                UsePassword = true
            };

            byte[] nativeExportData = StudioIoWriter.WriteToBytes(writeRequest);

            return new ExportResult
            {
                FileData = nativeExportData,
                FileName = SanitizeFileName(projectName) + ".io",
                MimeType = "application/x-studioio",
                Format = FORMAT_IO
            };
        }


        /// <summary>
        ///
        /// Export a project as a single LDraw .ldr file.
        ///
        /// Generates the LDraw content from PlacedBrick entities.
        /// Only includes the main model (no submodels).
        ///
        /// </summary>
        public async Task<ExportResult> ExportToLdrAsync(
            int projectId,
            Guid tenantGuid,
            CancellationToken cancellationToken = default)
        {
            string content = await GenerateLDrawContentAsync(projectId, tenantGuid, false, cancellationToken);

            Project project = await _context.Projects
                .Where(p => p.id == projectId && p.tenantGuid == tenantGuid)
                .FirstOrDefaultAsync(cancellationToken);

            string projectName = project?.name ?? "export";

            return new ExportResult
            {
                FileData = Encoding.UTF8.GetBytes(content),
                FileName = SanitizeFileName(projectName) + ".ldr",
                MimeType = "application/x-ldraw",
                Format = FORMAT_LDR
            };
        }


        /// <summary>
        ///
        /// Export a project as a Multi-Part Document .mpd file.
        ///
        /// Generates LDraw content with FILE/NOFILE blocks for each submodel.
        ///
        /// </summary>
        public async Task<ExportResult> ExportToMpdAsync(
            int projectId,
            Guid tenantGuid,
            CancellationToken cancellationToken = default)
        {
            string content = await GenerateLDrawContentAsync(projectId, tenantGuid, true, cancellationToken);

            Project project = await _context.Projects
                .Where(p => p.id == projectId && p.tenantGuid == tenantGuid)
                .FirstOrDefaultAsync(cancellationToken);

            string projectName = project?.name ?? "export";

            return new ExportResult
            {
                FileData = Encoding.UTF8.GetBytes(content),
                FileName = SanitizeFileName(projectName) + ".mpd",
                MimeType = "application/x-ldraw",
                Format = FORMAT_MPD
            };
        }


        #region Viewer Data

        /// <summary>
        /// Summary DTO returned by the project viewer summary endpoint.
        /// </summary>
        public class ProjectViewerSummary
        {
            public int ProjectId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int? PartCount { get; set; }
            public int StepCount { get; set; }
            public int SubmodelCount { get; set; }
            public string SourceFormat { get; set; }
            public string StudioVersion { get; set; }
            public bool HasThumbnail { get; set; }
        }


        /// <summary>
        ///
        /// Get a lightweight summary of a project for the viewer UI.
        ///
        /// </summary>
        public async Task<ProjectViewerSummary> GetProjectSummaryAsync(
            int projectId,
            Guid tenantGuid,
            CancellationToken cancellationToken = default)
        {
            Project project = await _context.Projects
                .Where(p => p.id == projectId && p.tenantGuid == tenantGuid && p.active == true && p.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (project == null)
            {
                return null;
            }

            //
            // Count build steps and submodels
            //
            int stepCount = await _context.PlacedBricks
                .Where(pb => pb.projectId == projectId && pb.tenantGuid == tenantGuid && pb.active == true && pb.deleted == false)
                .Select(pb => pb.buildStepNumber ?? 1)
                .Distinct()
                .CountAsync(cancellationToken);

            int submodelCount = await _context.Submodels
                .Where(s => s.projectId == projectId && s.tenantGuid == tenantGuid && s.active == true && s.deleted == false)
                .CountAsync(cancellationToken);

            //
            // Get source format and Studio version from the most recent ModelDocument
            //
            var docInfo = await _context.ModelDocuments
                .Where(md => md.projectId == projectId && md.tenantGuid == tenantGuid && md.active == true && md.deleted == false)
                .OrderByDescending(md => md.id)
                .Select(md => new { md.sourceFormat, md.studioVersion })
                .FirstOrDefaultAsync(cancellationToken);

            return new ProjectViewerSummary
            {
                ProjectId = project.id,
                Name = project.name,
                Description = project.description,
                PartCount = project.partCount,
                StepCount = stepCount,
                SubmodelCount = submodelCount,
                SourceFormat = docInfo?.sourceFormat,
                StudioVersion = docInfo?.studioVersion,
                HasThumbnail = project.thumbnailData != null && project.thumbnailData.Length > 0
            };
        }


        /// <summary>
        ///
        /// Generate a self-contained MPD string optimised for the client-side 3D viewer.
        ///
        /// This produces a standard LDraw MPD file from PlacedBrick entities,
        /// and then appends any custom part geometry files found in the project's
        /// stored .io archive as inline FILE blocks.
        ///
        /// The result is a single self-contained text document that LDrawLoader
        /// can parse without needing any additional file fetches for custom parts.
        /// Standard LDraw library parts are NOT inlined â€” LDrawLoader resolves
        /// those via the existing /api/ldraw/file/ endpoint with IndexedDB caching.
        ///
        /// </summary>
        public async Task<string> GenerateViewerMpdAsync(
            int projectId,
            Guid tenantGuid,
            CancellationToken cancellationToken = default)
        {
            //
            // Step 1: Try to serve the ORIGINAL imported MPD/LDR source file.
            //
            // The original file preserves the full nested submodel hierarchy with
            // correct transforms at every level (e.g. main → porsche → chassisbottom).
            // Reconstructing from PlacedBrick entities flattens this hierarchy,
            // losing intermediate transforms and causing position errors.
            //
            ModelDocument sourceDocument = await _context.ModelDocuments
                .Where(md => md.projectId == projectId
                          && md.tenantGuid == tenantGuid
                          && (md.sourceFormat == FORMAT_MPD || md.sourceFormat == FORMAT_LDR || md.sourceFormat == FORMAT_LXF)
                          && md.sourceFileData != null
                          && md.active == true
                          && md.deleted == false)
                .OrderByDescending(md => md.id)
                .FirstOrDefaultAsync(cancellationToken);

            if (sourceDocument?.sourceFileData != null)
            {
                string originalMpd = System.Text.Encoding.UTF8.GetString(sourceDocument.sourceFileData);

                if (!string.IsNullOrWhiteSpace(originalMpd))
                {
                    _logger.LogInformation(
                        "Viewer MPD for project {Id}: serving original {Format} source ({Len} chars)",
                        projectId, sourceDocument.sourceFormat, originalMpd.Length);
                    return originalMpd;
                }
            }

            //
            // Step 2: No original source — reconstruct from PlacedBrick entities
            //
            string baseMpd = await GenerateLDrawContentAsync(projectId, tenantGuid, true, cancellationToken);

            //
            // Step 3: Look for custom part geometry in the stored .io archive
            //
            ModelDocument ioDocument = await _context.ModelDocuments
                .Where(md => md.projectId == projectId
                          && md.tenantGuid == tenantGuid
                          && md.sourceFormat == FORMAT_IO
                          && md.sourceFileData != null
                          && md.active == true
                          && md.deleted == false)
                .OrderByDescending(md => md.id)
                .FirstOrDefaultAsync(cancellationToken);

            if (ioDocument == null || ioDocument.sourceFileData == null)
            {
                //
                // No stored .io data — the base MPD is sufficient.
                // The client-side monkey-patch fetches library parts individually
                // from the in-memory LDrawFileService (O(1) per request).
                //
                return baseMpd;
            }

            //
            // Step 3: Parse the .io archive to extract CustomParts
            //
            Dictionary<string, byte[]> customParts = null;

            try
            {
                StudioIoResult parseResult = StudioIoParser.ParseBytes(
                    ioDocument.sourceFileData,
                    ioDocument.sourceFileName ?? "model.io");

                customParts = parseResult.CustomParts;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse stored .io for custom parts in project {Id}", projectId);
                return baseMpd;
            }

            if (customParts == null || customParts.Count == 0)
            {
                return baseMpd;
            }

            //
            // Step 4: Inline custom part geometry as FILE blocks at the end of the MPD
            //
            StringBuilder sb = new StringBuilder(baseMpd);

            sb.AppendLine();
            sb.AppendLine("0 // Custom parts embedded from BrickLink Studio");

            foreach (KeyValuePair<string, byte[]> part in customParts)
            {
                string partContent = System.Text.Encoding.UTF8.GetString(part.Value);
                string partName = part.Key;

                sb.AppendLine("0 FILE " + partName);
                sb.Append(partContent);

                //
                // Ensure content ends with a newline before NOFILE
                //
                if (partContent.Length > 0 && partContent[partContent.Length - 1] != '\n')
                {
                    sb.AppendLine();
                }

                sb.AppendLine("0 NOFILE");
            }

            _logger.LogInformation(
                "Viewer MPD for project {Id}: inlined {Count} custom parts",
                projectId,
                customParts.Count);

            //
            // Library parts are NOT bundled inline — the client-side monkey-patch
            // fetches them individually from the in-memory LDrawFileService.
            // This preserves the model structure (no extra 0 FILE blocks that
            // could interfere with the LDrawLoader's parsing).
            //
            return sb.ToString();
        }

        #endregion


        /// <summary>
        ///
        /// Generate LDraw-format text content from a project's PlacedBrick entities.
        ///
        /// Converts quaternion rotation back to 3x3 rotation matrix,
        /// groups bricks by build step, and outputs proper LDraw Type 1 lines
        /// with STEP separators.
        ///
        /// </summary>
        private async Task<string> GenerateLDrawContentAsync(
            int projectId,
            Guid tenantGuid,
            bool includeMpdSubmodels,
            CancellationToken cancellationToken)
        {
            //
            // Load the project
            //
            Project project = await _context.Projects
                .Where(p => p.id == projectId && p.tenantGuid == tenantGuid && p.active == true && p.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (project == null)
            {
                throw new InvalidOperationException($"Project with id {projectId} not found.");
            }

            //
            // Load all placed bricks for this project, ordered by build step and then by id.
            //
            // When building an MPD with submodels, we must EXCLUDE bricks that belong
            // to a submodel — those bricks appear inside the submodel's 0 FILE block
            // and are instantiated via SubmodelInstance type-1 references.
            // Including them here as well would cause every submodel brick to render
            // twice: once directly in the main model and once through the submodel.
            //
            HashSet<int> excludedSubmodelBrickIds = null;

            if (includeMpdSubmodels)
            {
                excludedSubmodelBrickIds = (await _context.SubmodelPlacedBricks
                    .Where(spb => spb.tenantGuid == tenantGuid && spb.active == true)
                    .Where(spb => spb.submodel.projectId == projectId)
                    .Select(spb => spb.placedBrickId)
                    .ToListAsync(cancellationToken))
                    .ToHashSet();
            }

            List<PlacedBrick> bricks = await _context.PlacedBricks
                .Where(pb => pb.projectId == projectId && pb.tenantGuid == tenantGuid && pb.active == true && pb.deleted == false)
                .OrderBy(pb => pb.buildStepNumber)
                .ThenBy(pb => pb.id)
                .ToListAsync(cancellationToken);

            //
            // Filter out submodel bricks in memory (cleaner than a complex SQL subquery)
            //
            if (excludedSubmodelBrickIds != null && excludedSubmodelBrickIds.Count > 0)
            {
                bricks = bricks.Where(pb => !excludedSubmodelBrickIds.Contains(pb.id)).ToList();
            }

            //
            // Load part and colour lookup tables (reverse direction: id â†’ ldrawPartId/ldrawColourCode)
            //
            Dictionary<int, string> partLookup = await _context.BrickParts
                .Where(p => p.ldrawPartId != null && p.ldrawPartId != "" && p.active == true && p.deleted == false)
                .Select(p => new { p.id, p.ldrawPartId })
                .ToDictionaryAsync(p => p.id, p => p.ldrawPartId, cancellationToken);

            Dictionary<int, int> colourLookup = await _context.BrickColours
                .Where(c => c.ldrawColourCode != null && c.active == true && c.deleted == false)
                .Select(c => new { c.id, c.ldrawColourCode })
                .ToDictionaryAsync(c => c.id, c => c.ldrawColourCode.Value, cancellationToken);

            //
            // Build the LDraw output
            //
            StringBuilder sb = new StringBuilder();

            if (includeMpdSubmodels == true)
            {
                sb.AppendLine("0 FILE " + project.name + ".ldr");
            }

            sb.AppendLine("0 " + (project.name ?? "Untitled"));
            sb.AppendLine("0 Name: " + (project.name ?? "Untitled") + ".ldr");
            sb.AppendLine("0 Author: BMC Export");
            sb.AppendLine("0 !LDRAW_ORG Unofficial Model");

            //
            // Group bricks by step number
            //
            var bricksByStep = bricks.GroupBy(b => b.buildStepNumber ?? 1).OrderBy(g => g.Key);
            bool firstStep = true;

            foreach (var stepGroup in bricksByStep)
            {
                if (firstStep == false)
                {
                    sb.AppendLine("0 STEP");
                }

                foreach (PlacedBrick brick in stepGroup)
                {
                    //
                    // Look up the LDraw part ID and colour code
                    //
                    string ldrawPartId = null;
                    int ldrawColourCode = 0;

                    if (partLookup.TryGetValue(brick.brickPartId, out string partId) == true)
                    {
                        ldrawPartId = partId;
                    }

                    if (colourLookup.TryGetValue(brick.brickColourId, out int colourCode) == true)
                    {
                        ldrawColourCode = colourCode;
                    }

                    if (ldrawPartId == null)
                    {
                        //
                        // Can't resolve the part â€” skip this brick
                        //
                        _logger.LogWarning("Export: Could not resolve brickPartId {PartId} to LDraw ID, skipping", brick.brickPartId);
                        continue;
                    }

                    //
                    // Ensure the part ID ends with .dat
                    //
                    if (ldrawPartId.EndsWith(".dat", StringComparison.OrdinalIgnoreCase) == false)
                    {
                        ldrawPartId += ".dat";
                    }

                    //
                    // Convert quaternion back to 3x3 rotation matrix
                    //
                    float[] matrix = QuaternionToMatrix(
                        brick.rotationX ?? 0f,
                        brick.rotationY ?? 0f,
                        brick.rotationZ ?? 0f,
                        brick.rotationW ?? 1f);

                    //
                    // Format as LDraw Type 1 line:
                    // 1 <colour> <x> <y> <z> <a> <b> <c> <d> <e> <f> <g> <h> <i> <part>
                    //
                    sb.AppendFormat(CultureInfo.InvariantCulture,
                        "1 {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}",
                        ldrawColourCode,
                        brick.positionX ?? 0f,
                        brick.positionY ?? 0f,
                        brick.positionZ ?? 0f,
                        matrix[0], matrix[1], matrix[2],
                        matrix[3], matrix[4], matrix[5],
                        matrix[6], matrix[7], matrix[8],
                        ldrawPartId);
                    sb.AppendLine();
                }

                firstStep = false;
            }

            sb.AppendLine("0 STEP");

            if (includeMpdSubmodels == true)
            {
                //
                // Load ALL submodel instances for this project.
                // We filter by parentSubmodelId to place them at the correct
                // nesting level (null = main model, non-null = inside a submodel).
                //
                List<SubmodelInstance> allInstances = await _context.SubmodelInstances
                    .Where(si => si.submodel.projectId == projectId
                              && si.tenantGuid == tenantGuid
                              && si.active == true
                              && si.deleted == false)
                    .Include(si => si.submodel)
                    .OrderBy(si => si.buildStepNumber)
                    .ThenBy(si => si.id)
                    .ToListAsync(cancellationToken);

                //
                // Main model instances: only those placed directly (parentSubmodelId == null)
                //
                List<SubmodelInstance> mainInstances = allInstances
                    .Where(si => si.parentSubmodelId == null)
                    .ToList();

                if (mainInstances.Count > 0)
                {
                    int currentRefStep = 0;

                    foreach (SubmodelInstance instance in mainInstances)
                    {
                        //
                        // Emit STEP separators between different build steps
                        //
                        if (instance.buildStepNumber > currentRefStep && currentRefStep > 0)
                        {
                            sb.AppendLine("0 STEP");
                        }

                        currentRefStep = instance.buildStepNumber;

                        //
                        // Convert quaternion back to 3x3 rotation matrix
                        //
                        float[] matrix = QuaternionToMatrix(
                            instance.rotationX ?? 0f,
                            instance.rotationY ?? 0f,
                            instance.rotationZ ?? 0f,
                            instance.rotationW ?? 1f);

                        sb.AppendFormat(CultureInfo.InvariantCulture,
                            "1 {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}",
                            instance.colourCode,
                            instance.positionX ?? 0f,
                            instance.positionY ?? 0f,
                            instance.positionZ ?? 0f,
                            matrix[0], matrix[1], matrix[2],
                            matrix[3], matrix[4], matrix[5],
                            matrix[6], matrix[7], matrix[8],
                            instance.submodel.name);
                        sb.AppendLine();
                    }

                    sb.AppendLine("0 STEP");
                }

                sb.AppendLine("0 NOFILE");

                //
                // Include submodels
                //
                List<Submodel> submodels = await _context.Submodels
                    .Where(s => s.projectId == projectId && s.tenantGuid == tenantGuid && s.active == true && s.deleted == false)
                    .OrderBy(s => s.sequence)
                    .ToListAsync(cancellationToken);

                foreach (Submodel submodel in submodels)
                {
                    sb.AppendLine("0 FILE " + submodel.name);
                    sb.AppendLine("0 " + submodel.name);
                    sb.AppendLine("0 Name: " + submodel.name);
                    sb.AppendLine("0 Author: BMC Export");

                    //
                    // Get bricks for this submodel via the junction table
                    //
                    List<int> submodelBrickIds = await _context.SubmodelPlacedBricks
                        .Where(spb => spb.submodelId == submodel.id && spb.tenantGuid == tenantGuid && spb.active == true)
                        .Select(spb => spb.placedBrickId)
                        .ToListAsync(cancellationToken);

                    List<PlacedBrick> submodelBricks = await _context.PlacedBricks
                        .Where(pb => submodelBrickIds.Contains(pb.id) && pb.active == true && pb.deleted == false)
                        .OrderBy(pb => pb.buildStepNumber)
                        .ThenBy(pb => pb.id)
                        .ToListAsync(cancellationToken);

                    var subBricksByStep = submodelBricks.GroupBy(b => b.buildStepNumber ?? 1).OrderBy(g => g.Key);
                    bool subFirstStep = true;

                    foreach (var stepGroup in subBricksByStep)
                    {
                        if (subFirstStep == false)
                        {
                            sb.AppendLine("0 STEP");
                        }

                        foreach (PlacedBrick brick in stepGroup)
                        {
                            string ldrawPartId = null;
                            int ldrawColourCode = 0;

                            if (partLookup.TryGetValue(brick.brickPartId, out string partId) == true)
                            {
                                ldrawPartId = partId;
                            }

                            if (colourLookup.TryGetValue(brick.brickColourId, out int colourCode) == true)
                            {
                                ldrawColourCode = colourCode;
                            }

                            if (ldrawPartId == null)
                            {
                                continue;
                            }

                            if (ldrawPartId.EndsWith(".dat", StringComparison.OrdinalIgnoreCase) == false)
                            {
                                ldrawPartId += ".dat";
                            }

                            float[] matrix = QuaternionToMatrix(
                                brick.rotationX ?? 0f,
                                brick.rotationY ?? 0f,
                                brick.rotationZ ?? 0f,
                                brick.rotationW ?? 1f);

                            sb.AppendFormat(CultureInfo.InvariantCulture,
                                "1 {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}",
                                ldrawColourCode,
                                brick.positionX ?? 0f,
                                brick.positionY ?? 0f,
                                brick.positionZ ?? 0f,
                                matrix[0], matrix[1], matrix[2],
                                matrix[3], matrix[4], matrix[5],
                                matrix[6], matrix[7], matrix[8],
                                ldrawPartId);
                            sb.AppendLine();
                        }

                        subFirstStep = false;
                    }

                    //
                    // Include nested submodel instance references within this submodel.
                    // These are type-1 lines that reference child submodels.
                    //
                    List<SubmodelInstance> nestedInstances = allInstances
                        .Where(si => si.parentSubmodelId == submodel.id)
                        .OrderBy(si => si.buildStepNumber)
                        .ThenBy(si => si.id)
                        .ToList();

                    foreach (SubmodelInstance nestedInst in nestedInstances)
                    {
                        float[] nestedMatrix = QuaternionToMatrix(
                            nestedInst.rotationX ?? 0f,
                            nestedInst.rotationY ?? 0f,
                            nestedInst.rotationZ ?? 0f,
                            nestedInst.rotationW ?? 1f);

                        sb.AppendLine("0 STEP");
                        sb.AppendFormat(CultureInfo.InvariantCulture,
                            "1 {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}",
                            nestedInst.colourCode,
                            nestedInst.positionX ?? 0f,
                            nestedInst.positionY ?? 0f,
                            nestedInst.positionZ ?? 0f,
                            nestedMatrix[0], nestedMatrix[1], nestedMatrix[2],
                            nestedMatrix[3], nestedMatrix[4], nestedMatrix[5],
                            nestedMatrix[6], nestedMatrix[7], nestedMatrix[8],
                            nestedInst.submodel.name);
                        sb.AppendLine();
                    }

                    sb.AppendLine("0 STEP");
                    sb.AppendLine("0 NOFILE");
                }
            }

            return sb.ToString();
        }


        /// <summary>
        ///
        /// Convert a quaternion to a 3x3 rotation matrix.
        ///
        /// Returns 9 floats in row-major order: [a, b, c, d, e, f, g, h, i]
        ///
        /// </summary>
        private static float[] QuaternionToMatrix(float qx, float qy, float qz, float qw)
        {
            //
            // Normalize the quaternion
            //
            float length = (float)Math.Sqrt(qx * qx + qy * qy + qz * qz + qw * qw);

            if (length > 0.0001f)
            {
                qx /= length;
                qy /= length;
                qz /= length;
                qw /= length;
            }

            float xx = qx * qx;
            float yy = qy * qy;
            float zz = qz * qz;
            float xy = qx * qy;
            float xz = qx * qz;
            float yz = qy * qz;
            float wx = qw * qx;
            float wy = qw * qy;
            float wz = qw * qz;

            float[] matrix = new float[9];

            // Row 1
            matrix[0] = 1f - 2f * (yy + zz);
            matrix[1] = 2f * (xy - wz);
            matrix[2] = 2f * (xz + wy);

            // Row 2
            matrix[3] = 2f * (xy + wz);
            matrix[4] = 1f - 2f * (xx + zz);
            matrix[5] = 2f * (yz - wx);

            // Row 3
            matrix[6] = 2f * (xz - wy);
            matrix[7] = 2f * (yz + wx);
            matrix[8] = 1f - 2f * (xx + yy);

            return matrix;
        }


        #region LDraw Dependency Bundling

        /// <summary>
        /// Scans the MPD text for all LDraw sub-file references (type-1 lines),
        /// recursively resolves their dependencies from the in-memory LDraw library,
        /// and embeds them as inline 0 FILE / 0 NOFILE blocks.
        ///
        /// Also embeds LDConfig.ldr so colour definitions are available without
        /// a separate network request.
        /// </summary>
        private async Task<string> BundleLDrawDependenciesAsync(string mpdText)
        {
            //
            // Wait for the background preload to complete.
            // This is a no-op if the preload is already done.
            //
            await _ldrawFiles.WaitForLoadAsync().ConfigureAwait(false);

            if (_ldrawFiles.IsLoaded == false || _ldrawFiles.FileCount == 0)
            {
                _logger.LogWarning("LDraw file service not loaded — skipping dependency bundling.");
                return mpdText;
            }

            //
            // Collect all filenames already embedded in the MPD (0 FILE lines)
            //
            HashSet<string> embeddedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string line in mpdText.Split('\n'))
            {
                string trimmed = line.Trim();

                if (trimmed.StartsWith("0 FILE ", StringComparison.OrdinalIgnoreCase))
                {
                    string fileName = trimmed.Substring(7).Trim();
                    embeddedFiles.Add(fileName);
                }
            }

            //
            // BFS: resolve each referenced file and scan it for further dependencies
            //
            Dictionary<string, string> resolvedFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Queue<string> toResolve = new Queue<string>();

            ExtractSubFileReferences(mpdText, embeddedFiles, toResolve);

            //
            // Also ensure LDConfig.ldr is bundled (colour definitions)
            //
            if (embeddedFiles.Contains("LDConfig.ldr") == false)
            {
                toResolve.Enqueue("LDConfig.ldr");
            }

            int maxFiles = 15000;
            int resolved = 0;

            while (toResolve.Count > 0 && resolved < maxFiles)
            {
                string fileName = toResolve.Dequeue();

                if (resolvedFiles.ContainsKey(fileName) || embeddedFiles.Contains(fileName))
                {
                    continue;
                }

                //
                // O(1) lookup in the preloaded in-memory cache
                //
                string content = _ldrawFiles.TryGetFile(fileName);

                if (content == null)
                {
                    continue;
                }

                resolvedFiles[fileName] = content;
                resolved++;

                ExtractSubFileReferences(content, embeddedFiles, toResolve, resolvedFiles);
            }

            if (resolvedFiles.Count == 0)
            {
                return mpdText;
            }

            //
            // Append all resolved files as 0 FILE blocks
            //
            StringBuilder sb = new StringBuilder(mpdText, mpdText.Length + resolvedFiles.Values.Sum(v => v.Length) + resolvedFiles.Count * 50);

            sb.AppendLine();
            sb.AppendLine("0 // LDraw library parts bundled for offline rendering");

            foreach (KeyValuePair<string, string> entry in resolvedFiles)
            {
                sb.AppendLine("0 FILE " + entry.Key.Replace('\\', '/'));
                sb.Append(entry.Value);

                if (entry.Value.Length > 0 && entry.Value[entry.Value.Length - 1] != '\n')
                {
                    sb.AppendLine();
                }

                sb.AppendLine("0 NOFILE");
            }

            _logger.LogInformation(
                "Bundled {Count} LDraw library files inline ({SizeKB:N0} KB total MPD)",
                resolvedFiles.Count,
                sb.Length / 1024);

            return sb.ToString();
        }


        /// <summary>
        /// Scans LDraw text for type-1 sub-file reference lines and enqueues
        /// any filenames not already embedded or resolved.
        /// </summary>
        private void ExtractSubFileReferences(
            string text,
            HashSet<string> embeddedFiles,
            Queue<string> toResolve,
            Dictionary<string, string> resolvedFiles = null)
        {
            foreach (string line in text.Split('\n'))
            {
                string trimmed = line.Trim();

                if (trimmed.Length < 10 || trimmed[0] != '1')
                {
                    continue;
                }

                if (trimmed.Length > 1 && trimmed[1] != ' ' && trimmed[1] != '\t')
                {
                    continue;
                }

                string[] tokens = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Length < 15)
                {
                    continue;
                }

                string fileName = tokens[tokens.Length - 1];

                if (embeddedFiles.Contains(fileName))
                {
                    continue;
                }

                if (resolvedFiles != null && resolvedFiles.ContainsKey(fileName))
                {
                    continue;
                }

                toResolve.Enqueue(fileName);
            }
        }

        #endregion



        /// <summary>
        /// Sanitize a filename to remove characters that aren't safe for file systems.
        /// </summary>
        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) == true)
            {
                return "export";
            }

            char[] invalid = Path.GetInvalidFileNameChars();

            foreach (char c in invalid)
            {
                name = name.Replace(c, '_');
            }

            return name.Trim();
        }
    }
}
