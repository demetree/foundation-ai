using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Foundation.BMC.Database;

using BMC.LDraw.Models;
using BMC.LDraw.Parsers;
using BMC.LDraw.Render;

using System.Xml.Linq;

namespace Foundation.BMC.Services
{
    /// <summary>
    ///
    /// Service that imports model files (.ldr, .mpd, .io) into BMC's native entity structure.
    ///
    /// The import process has two layers:
    ///   1. Import-fidelity preservation: stores the raw file and parsed structure in
    ///      ModelDocument → ModelSubFile → ModelBuildStep → ModelStepPart
    ///   2. Native conversion: converts to the BMC design entities:
    ///      Project → PlacedBrick (with quaternion rotation) + Submodel + SubmodelPlacedBrick
    ///
    /// This dual-layer approach ensures round-trip capability (lossless re-export)
    /// while also providing the native data structure needed for the design canvas and
    /// instruction generator.
    ///
    /// AI-developed code — initial implementation March 2026
    ///
    /// </summary>
    public class ModelImportService
    {
        //
        // Constants
        //
        private const int MAX_FILE_SIZE_BYTES = 50 * 1024 * 1024;  // 50MB max upload size
        private const string FORMAT_LDR = "ldr";
        private const string FORMAT_MPD = "mpd";
        private const string FORMAT_IO = "io";
        private const string FORMAT_LXF = "lxf";


        //
        // Dependencies
        //
        private readonly BMCContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ModelImportService> _logger;


        //
        // Caches loaded during an import to avoid repeated database lookups.
        // These are populated once per import call and cleared when complete.
        //
        private Dictionary<string, int> _partLookupCache = null;
        private Dictionary<int, int> _colourLookupCache = null;


        //
        // Tracking for unresolved items during an import
        //
        private List<string> _unresolvedPartsList = null;
        private List<int> _unresolvedColoursList = null;


        /// <summary>
        /// Constructor — takes the BMC database context, configuration, and a logger.
        /// </summary>
        public ModelImportService(BMCContext context, IConfiguration configuration, ILogger<ModelImportService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }


        #region Result DTO

        /// <summary>
        /// Result returned after a successful model import.
        /// </summary>
        public class ImportResult
        {
            public int ProjectId { get; set; }
            public string ProjectName { get; set; }
            public int TotalPartCount { get; set; }
            public int SubmodelCount { get; set; }
            public int StepCount { get; set; }
            public int ResolvedPartCount { get; set; }
            public int UnresolvedPartCount { get; set; }
            public List<string> UnresolvedParts { get; set; }
            public List<int> UnresolvedColours { get; set; }
            public string SourceFormat { get; set; }
        }

        #endregion


        /// <summary>
        ///
        /// Import a model file into the BMC database.
        ///
        /// Accepts a file stream, detects the format from the file extension, parses the model data,
        /// and creates the full entity hierarchy in the database.
        ///
        /// </summary>
        /// <param name="fileStream">Stream containing the file data</param>
        /// <param name="fileName">Original filename (used to detect format)</param>
        /// <param name="tenantGuid">The tenant that owns the imported project</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Import result with project details and resolution statistics</returns>
        public async Task<ImportResult> ImportFromFileAsync(
            Stream fileStream,
            string fileName,
            Guid tenantGuid,
            CancellationToken cancellationToken = default)
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            //
            // Step 1: Read the file into a byte array
            //
            byte[] fileData = null;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                await fileStream.CopyToAsync(memoryStream, cancellationToken);
                fileData = memoryStream.ToArray();
            }

            if (fileData.Length > MAX_FILE_SIZE_BYTES)
            {
                throw new InvalidOperationException($"File size ({fileData.Length:N0} bytes) exceeds maximum allowed size ({MAX_FILE_SIZE_BYTES:N0} bytes).");
            }

            //
            // Step 2: Detect the format from the file extension
            //
            string extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
            string sourceFormat = extension;
            string[] lDrawLines = null;
            StudioIoResult ioResult = null;
            LxfResult lxfResult = null;

            if (extension == FORMAT_IO)
            {
                //
                // Parse the .io ZIP to extract the embedded LDraw content
                //
                _logger.LogInformation("Importing .io file: {FileName} ({Size} bytes)", fileName, fileData.Length);

                ioResult = StudioIoParser.ParseBytes(fileData, fileName);
                lDrawLines = ioResult.LDrawLines;

                _logger.LogInformation(
                    ".io extraction complete — Studio version: {Version}, reported parts: {Parts}, password-protected: {Protected}",
                    ioResult.StudioVersion ?? "unknown",
                    ioResult.ReportedPartCount?.ToString() ?? "unknown",
                    ioResult.WasPasswordProtected);

                //
                // Log error part list if present — helps with diagnostics
                //
                if (string.IsNullOrEmpty(ioResult.ErrorPartList) == false)
                {
                    _logger.LogWarning("Studio .io error part list:\n{ErrorParts}", ioResult.ErrorPartList);
                }
            }
            else if (extension == FORMAT_LXF)
            {
                //
                // AI-developed: Parse the .lxf ZIP to extract the LXFML XML and convert to LDraw lines
                //
                _logger.LogInformation("Importing .lxf file: {FileName} ({Size} bytes)", fileName, fileData.Length);

                lxfResult = LxfParser.ParseBytes(fileData, fileName);
                lDrawLines = lxfResult.LDrawLines;

                _logger.LogInformation(
                    ".lxf extraction complete — LDD version: {Version}, {LineCount} LDraw lines generated",
                    lxfResult.LddVersion ?? "unknown",
                    lDrawLines?.Length ?? 0);
            }
            else if (extension == FORMAT_LDR || extension == FORMAT_MPD)
            {
                //
                // Read the LDraw file directly
                //
                _logger.LogInformation("Importing .{Extension} file: {FileName} ({Size} bytes)", extension, fileName, fileData.Length);

                string content = System.Text.Encoding.UTF8.GetString(fileData);
                lDrawLines = content.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported file format: '.{extension}'. Supported formats are: .ldr, .mpd, .io, .lxf");
            }

            //
            // Step 3: Parse the LDraw content into model structures
            //
            List<LDrawModel> models = ModelParser.ParseLines(lDrawLines);

            if (models == null || models.Count == 0)
            {
                throw new InvalidOperationException("No model data found in the uploaded file.");
            }

            _logger.LogInformation("Parsed {ModelCount} model(s) from {FileName}", models.Count, fileName);

            //
            // Step 4: Initialize the lookup caches from the database
            //
            await InitializeLookupCachesAsync(cancellationToken);

            //
            // Step 5: Create the full entity hierarchy
            //
            ImportResult result = await CreateProjectFromModelsAsync(
                models,
                fileData,
                lDrawLines,
                fileName,
                sourceFormat,
                ioResult,
                lxfResult?.ThumbnailData,
                tenantGuid,
                cancellationToken);

            //
            // Step 6: Clear the caches
            //
            _partLookupCache = null;
            _colourLookupCache = null;
            _unresolvedPartsList = null;
            _unresolvedColoursList = null;

            _logger.LogInformation(
                "Import complete — Project '{Name}' (id={Id}), {Parts} parts, {Submodels} submodels, {Resolved} resolved, {Unresolved} unresolved",
                result.ProjectName,
                result.ProjectId,
                result.TotalPartCount,
                result.SubmodelCount,
                result.ResolvedPartCount,
                result.UnresolvedPartCount);

            return result;
        }


        /// <summary>
        ///
        /// Initialize the part and colour lookup caches from the database.
        ///
        /// These caches map LDraw part IDs to BrickPart.id and LDraw colour codes to BrickColour.id,
        /// so that we don't need to query the database for every single placed brick during import.
        ///
        /// </summary>
        private async Task InitializeLookupCachesAsync(CancellationToken cancellationToken)
        {
            //
            // Build the part lookup cache from BrickPart.ldrawPartId → BrickPart.id
            // Only include parts that have an ldrawPartId set
            //
            _partLookupCache = await _context.BrickParts
                .Where(p => p.ldrawPartId != null && p.ldrawPartId != "" && p.active == true && p.deleted == false)
                .Select(p => new { p.ldrawPartId, p.id })
                .ToDictionaryAsync(p => p.ldrawPartId, p => p.id, cancellationToken);

            _logger.LogInformation("Part lookup cache loaded with {Count} entries", _partLookupCache.Count);

            //
            // Build the colour lookup cache from BrickColour.ldrawColourCode → BrickColour.id
            //
            _colourLookupCache = await _context.BrickColours
                .Where(c => c.ldrawColourCode != null && c.active == true && c.deleted == false)
                .Select(c => new { c.ldrawColourCode, c.id })
                .ToDictionaryAsync(c => c.ldrawColourCode.Value, c => c.id, cancellationToken);

            _logger.LogInformation("Colour lookup cache loaded with {Count} entries", _colourLookupCache.Count);

            //
            // Initialize the tracking lists for unresolved items
            //
            _unresolvedPartsList = new List<string>();
            _unresolvedColoursList = new List<int>();
        }


        /// <summary>
        ///
        /// Create the full Project entity hierarchy from parsed LDraw models.
        ///
        /// This is the core conversion method that:
        ///   1. Creates the Project entity
        ///   2. Stores the raw file in ModelDocument → ModelSubFile → ModelBuildStep → ModelStepPart
        ///   3. Converts to PlacedBrick entities with quaternion rotation
        ///   4. Creates Submodel hierarchy for MPD files
        ///
        /// </summary>
        private async Task<ImportResult> CreateProjectFromModelsAsync(
            List<LDrawModel> models,
            byte[] rawFileData,
            string[] lDrawLines,
            string fileName,
            string sourceFormat,
            StudioIoResult ioResult,
            byte[] lxfThumbnailData,
            Guid tenantGuid,
            CancellationToken cancellationToken)
        {
            //
            // The main model is always the first in the list
            //
            LDrawModel mainModel = models[0];
            string projectName = mainModel.Name ?? Path.GetFileNameWithoutExtension(fileName);

            //
            // Ensure the project name is unique within this tenant.
            // If a project with the same name already exists, append an incrementing suffix.
            //
            projectName = await GetUniqueProjectNameAsync(projectName, tenantGuid, cancellationToken);

            //
            // Step 1: Create the Project entity
            //
            Project project = new Project
            {
                tenantGuid = tenantGuid,
                name = projectName,
                description = $"Imported from {fileName}",
                lastBuildDate = DateTime.UtcNow,
                versionNumber = 1,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            //
            // If we have a thumbnail from an .io or .lxf import, store it on the Project
            //
            if (ioResult != null && ioResult.ThumbnailData != null && ioResult.ThumbnailData.Length > 0)
            {
                project.thumbnailData = ioResult.ThumbnailData;
            }
            else if (lxfThumbnailData != null && lxfThumbnailData.Length > 0)
            {
                project.thumbnailData = lxfThumbnailData;
            }
            else
            {
                //
                // No embedded thumbnail — generate one using the software renderer.
                // Uses the same RenderService as PartRendererController.
                //
                project.thumbnailData = GenerateThumbnailFromLDraw(lDrawLines, fileName);
            }

            _context.Projects.Add(project);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created Project '{Name}' with id={Id}", projectName, project.id);

            //
            // Step 2: Store the raw file in ModelDocument for round-trip fidelity
            //
            int totalPartCount = 0;
            int totalStepCount = 0;

            foreach (LDrawModel model in models)
            {
                foreach (LDrawStep step in model.Steps)
                {
                    totalPartCount += step.Parts.Count;
                    totalStepCount++;
                }
            }

            ModelDocument modelDocument = new ModelDocument
            {
                tenantGuid = tenantGuid,
                projectId = project.id,
                name = projectName,
                description = $"Imported from {fileName}",
                sourceFormat = sourceFormat,
                sourceFileName = fileName,
                sourceFileFileName = fileName,
                sourceFileSize = rawFileData.Length,
                sourceFileData = rawFileData,
                sourceFileMimeType = GetMimeType(sourceFormat),
                author = mainModel.Author,
                totalPartCount = totalPartCount,
                totalStepCount = totalStepCount,
                versionNumber = 1,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            //
            // If this was an .io import, store the Studio-specific metadata
            //
            if (ioResult != null)
            {
                modelDocument.studioVersion = ioResult.StudioVersion;
                modelDocument.instructionSettingsXml = ioResult.InstructionSettingsXml;
                modelDocument.errorPartList = ioResult.ErrorPartList;
            }

            _context.ModelDocuments.Add(modelDocument);
            await _context.SaveChangesAsync(cancellationToken);

            //
            // Step 3: Create ModelSubFile entries for each model in the parsed list
            //
            Dictionary<string, ModelSubFile> subFileByName = new Dictionary<string, ModelSubFile>(StringComparer.OrdinalIgnoreCase);

            for (int modelIndex = 0; modelIndex < models.Count; modelIndex++)
            {
                LDrawModel model = models[modelIndex];
                bool isMain = (modelIndex == 0);

                ModelSubFile subFile = new ModelSubFile
                {
                    tenantGuid = tenantGuid,
                    modelDocumentId = modelDocument.id,
                    fileName = model.Name ?? fileName,
                    isMainModel = isMain,
                    sequence = modelIndex,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.ModelSubFiles.Add(subFile);
                await _context.SaveChangesAsync(cancellationToken);

                //
                // Track for submodel reference resolution later
                //
                if (model.Name != null)
                {
                    subFileByName[model.Name] = subFile;
                }

                //
                // Step 4: Create ModelBuildStep and ModelStepPart entries for import fidelity
                //
                int stepNumber = 1;

                foreach (LDrawStep step in model.Steps)
                {
                    ModelBuildStep buildStep = new ModelBuildStep
                    {
                        tenantGuid = tenantGuid,
                        modelSubFileId = subFile.id,
                        stepNumber = stepNumber,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };

                    _context.ModelBuildSteps.Add(buildStep);
                    await _context.SaveChangesAsync(cancellationToken);

                    int partSequence = 1;

                    foreach (LDrawSubfileReference partRef in step.Parts)
                    {
                        //
                        // Store the raw import data — transform matrix as string, original part filename, colour code
                        //
                        string matrixString = string.Join(" ",
                            partRef.Matrix.Select(m => m.ToString("G", CultureInfo.InvariantCulture)));

                        //
                        // Try to resolve the part and colour from the database
                        //
                        int resolvedPartId = ResolvePartId(partRef.FileName);
                        int resolvedColourId = ResolveColourId(partRef.ColourCode);

                        ModelStepPart stepPart = new ModelStepPart
                        {
                            tenantGuid = tenantGuid,
                            modelBuildStepId = buildStep.id,
                            brickPartId = resolvedPartId > 0 ? resolvedPartId : (int?)null,
                            brickColourId = resolvedColourId > 0 ? resolvedColourId : (int?)null,
                            partFileName = partRef.FileName,
                            colorCode = partRef.ColourCode,
                            positionX = partRef.X,
                            positionY = partRef.Y,
                            positionZ = partRef.Z,
                            transformMatrix = matrixString,
                            sequence = partSequence,
                            objectGuid = Guid.NewGuid(),
                            active = true,
                            deleted = false
                        };

                        _context.ModelStepParts.Add(stepPart);

                        partSequence++;
                    }

                    await _context.SaveChangesAsync(cancellationToken);

                    stepNumber++;
                }
            }

            //
            // Step 5: Create the native BMC representation (PlacedBrick + Submodel)
            //
            // Build a set of known submodel names so we can detect submodel references
            // vs actual part references during conversion
            //
            HashSet<string> submodelNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int modelIndex = 1; modelIndex < models.Count; modelIndex++)
            {
                if (models[modelIndex].Name != null)
                {
                    submodelNames.Add(models[modelIndex].Name);
                }
            }

            //
            // Create Submodel entities for non-main models
            //
            Dictionary<string, Submodel> submodelByName = new Dictionary<string, Submodel>(StringComparer.OrdinalIgnoreCase);

            for (int modelIndex = 1; modelIndex < models.Count; modelIndex++)
            {
                LDrawModel model = models[modelIndex];

                if (model.Name == null)
                {
                    continue;
                }

                Submodel submodel = new Submodel
                {
                    tenantGuid = tenantGuid,
                    projectId = project.id,
                    name = await GetUniqueSubmodelNameAsync(model.Name, tenantGuid, cancellationToken),
                    description = $"Submodel imported from {fileName}",
                    sequence = modelIndex,
                    versionNumber = 1,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.Submodels.Add(submodel);
                await _context.SaveChangesAsync(cancellationToken);

                submodelByName[model.Name] = submodel;
            }

            //
            // Step 6: Create PlacedBrick entities from all models
            //
            int totalPlacedBricks = 0;
            int resolvedParts = 0;

            for (int modelIndex = 0; modelIndex < models.Count; modelIndex++)
            {
                LDrawModel model = models[modelIndex];
                bool isMainModel = (modelIndex == 0);

                //
                // Determine which submodel these bricks belong to (null for main model)
                //
                Submodel owningSubmodel = null;

                if (isMainModel == false && model.Name != null)
                {
                    submodelByName.TryGetValue(model.Name, out owningSubmodel);
                }

                int stepNumber = 1;

                foreach (LDrawStep step in model.Steps)
                {
                    foreach (LDrawSubfileReference partRef in step.Parts)
                    {
                        //
                        // Check if this reference is to another submodel rather than an actual part
                        //
                        if (submodelNames.Contains(partRef.FileName) == true)
                        {
                            //
                            // This is a submodel reference (type-1 line placing a submodel assembly).
                            // Store the placement transform so we can reconstruct
                            // the reference line when exporting back to MPD.
                            //
                            if (submodelByName.TryGetValue(partRef.FileName, out Submodel referencedSubmodel))
                            {
                                float refRotX, refRotY, refRotZ, refRotW;
                                MatrixToQuaternion(partRef.Matrix, out refRotX, out refRotY, out refRotZ, out refRotW);

                                SubmodelInstance instance = new SubmodelInstance
                                {
                                    tenantGuid = tenantGuid,
                                    submodelId = referencedSubmodel.id,
                                    parentSubmodelId = owningSubmodel?.id,
                                    positionX = partRef.X,
                                    positionY = partRef.Y,
                                    positionZ = partRef.Z,
                                    rotationX = refRotX,
                                    rotationY = refRotY,
                                    rotationZ = refRotZ,
                                    rotationW = refRotW,
                                    colourCode = partRef.ColourCode,
                                    buildStepNumber = stepNumber,
                                    objectGuid = Guid.NewGuid(),
                                    active = true,
                                    deleted = false
                                };

                                _context.SubmodelInstances.Add(instance);
                                _logger.LogDebug(
                                    "Stored submodel instance '{SubmodelName}' at ({X},{Y},{Z}) in step {Step}",
                                    partRef.FileName, partRef.X, partRef.Y, partRef.Z, stepNumber);
                            }

                            continue;
                        }

                        //
                        // Resolve the part and colour
                        //
                        int partId = ResolvePartId(partRef.FileName);
                        int colourId = ResolveColourId(partRef.ColourCode);

                        if (partId <= 0 || colourId <= 0)
                        {
                            //
                            // Skip bricks we can't resolve — they're preserved in ModelStepPart
                            //
                            continue;
                        }

                        //
                        // Decompose the 3x3 rotation matrix into a quaternion
                        //
                        float rotX = 0f;
                        float rotY = 0f;
                        float rotZ = 0f;
                        float rotW = 1f;

                        MatrixToQuaternion(partRef.Matrix, out rotX, out rotY, out rotZ, out rotW);

                        PlacedBrick placedBrick = new PlacedBrick
                        {
                            tenantGuid = tenantGuid,
                            projectId = project.id,
                            brickPartId = partId,
                            brickColourId = colourId,
                            positionX = partRef.X,
                            positionY = partRef.Y,
                            positionZ = partRef.Z,
                            rotationX = rotX,
                            rotationY = rotY,
                            rotationZ = rotZ,
                            rotationW = rotW,
                            buildStepNumber = stepNumber,
                            isHidden = false,
                            versionNumber = 1,
                            objectGuid = Guid.NewGuid(),
                            active = true,
                            deleted = false
                        };

                        _context.PlacedBricks.Add(placedBrick);
                        totalPlacedBricks++;
                        resolvedParts++;

                        //
                        // If this brick belongs to a submodel, create the junction table entry
                        //
                        if (owningSubmodel != null)
                        {
                            //
                            // We need to save first to get the PlacedBrick's id
                            // so we batch these after the main save below
                            //
                        }
                    }

                    stepNumber++;
                }

                //
                // Save the placed bricks for this model in a batch
                //
                await _context.SaveChangesAsync(cancellationToken);

                //
                // Now create SubmodelPlacedBrick junction entries for bricks in this submodel
                //
                if (owningSubmodel != null)
                {
                    List<PlacedBrick> submodelBricks = await _context.PlacedBricks
                        .Where(pb => pb.projectId == project.id && pb.tenantGuid == tenantGuid && pb.active == true && pb.deleted == false)
                        .OrderByDescending(pb => pb.id)
                        .Take(totalPlacedBricks)
                        .ToListAsync(cancellationToken);

                    //
                    // Only assign bricks that don't already have a submodel assignment
                    //
                    HashSet<int> alreadyAssigned = new HashSet<int>(
                        await _context.SubmodelPlacedBricks
                            .Where(spb => spb.tenantGuid == tenantGuid && spb.active == true)
                            .Select(spb => spb.placedBrickId)
                            .ToListAsync(cancellationToken));

                    foreach (PlacedBrick brick in submodelBricks)
                    {
                        if (alreadyAssigned.Contains(brick.id) == false)
                        {
                            SubmodelPlacedBrick junction = new SubmodelPlacedBrick
                            {
                                tenantGuid = tenantGuid,
                                submodelId = owningSubmodel.id,
                                placedBrickId = brick.id,
                                objectGuid = Guid.NewGuid(),
                                active = true,
                                deleted = false
                            };

                            _context.SubmodelPlacedBricks.Add(junction);
                        }
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            //
            // Step 7: Update the project part count
            //
            project.partCount = totalPlacedBricks;
            await _context.SaveChangesAsync(cancellationToken);

            //
            // Step 8: Auto-populate the Build Manual from parsed model steps
            //
            // Creates a full BuildManual → BuildManualPage → BuildManualStep → BuildStepPart
            // hierarchy so the user can open the Manual Editor immediately after import.
            //
            // Only the main model's steps are used — submodel steps are internal details
            // of sub-assemblies and don't belong in the top-level instruction manual.
            //
            // For each step we compute:
            //   a) Camera target — centroid of all parts placed so far (cumulative)
            //   b) Camera position — isometric offset from the centroid, scaled by bounding extent
            //   c) BuildStepPart links — junction records connecting manual steps to PlacedBricks
            //
            const int STEPS_PER_PAGE = 3;
            const float ISO_ANGLE_FACTOR = 0.7071f;  // cos(45°) — isometric offset ratio
            const float CAMERA_DISTANCE_PADDING = 1.5f; // multiplier to keep model in frame
            const float MIN_CAMERA_DISTANCE = 100f;

            int mainModelStepCount = mainModel.Steps.Count;

            //
            // Collect submodel filenames so we can exclude them from camera calculations.
            // Submodel references are sub-assemblies, not real part placements.
            //
            HashSet<string> submodelFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 1; i < models.Count; i++)
            {
                if (models[i].Name != null)
                {
                    submodelFileNames.Add(models[i].Name);
                }
            }

            //
            // Pre-compute cumulative camera data from the parsed LDraw parts.
            // We accumulate positions across steps so each step's camera sees the
            // full model built to that point, not just the newly added parts.
            //
            List<float> cumulativeCentroidX = new List<float>();
            List<float> cumulativeCentroidY = new List<float>();
            List<float> cumulativeCentroidZ = new List<float>();
            List<float> cumulativeExtent = new List<float>();

            float sumX = 0f, sumY = 0f, sumZ = 0f;
            int cumulativePartCount = 0;
            float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

            for (int stepIdx = 0; stepIdx < mainModelStepCount; stepIdx++)
            {
                LDrawStep step = mainModel.Steps[stepIdx];

                foreach (LDrawSubfileReference partRef in step.Parts)
                {
                    //
                    // Skip submodel references — they're not real parts
                    //
                    if (submodelFileNames.Contains(partRef.FileName))
                    {
                        continue;
                    }

                    sumX += partRef.X;
                    sumY += partRef.Y;
                    sumZ += partRef.Z;
                    cumulativePartCount++;

                    if (partRef.X < minX) minX = partRef.X;
                    if (partRef.Y < minY) minY = partRef.Y;
                    if (partRef.Z < minZ) minZ = partRef.Z;
                    if (partRef.X > maxX) maxX = partRef.X;
                    if (partRef.Y > maxY) maxY = partRef.Y;
                    if (partRef.Z > maxZ) maxZ = partRef.Z;
                }

                if (cumulativePartCount > 0)
                {
                    cumulativeCentroidX.Add(sumX / cumulativePartCount);
                    cumulativeCentroidY.Add(sumY / cumulativePartCount);
                    cumulativeCentroidZ.Add(sumZ / cumulativePartCount);

                    //
                    // Extent = the diagonal of the bounding box so far
                    //
                    float dx = maxX - minX;
                    float dy = maxY - minY;
                    float dz = maxZ - minZ;
                    float extent = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
                    cumulativeExtent.Add(Math.Max(extent, MIN_CAMERA_DISTANCE));
                }
                else
                {
                    cumulativeCentroidX.Add(0f);
                    cumulativeCentroidY.Add(0f);
                    cumulativeCentroidZ.Add(0f);
                    cumulativeExtent.Add(MIN_CAMERA_DISTANCE);
                }
            }

            //
            // Create the BuildManual
            //
            BuildManual initialManual = new BuildManual
            {
                projectId = project.id,
                tenantGuid = tenantGuid,
                name = (projectName.Length > 78 ? projectName.Substring(0, 78) : projectName) + " — Instructions",
                description = $"Auto-generated manual from {fileName} — {mainModelStepCount} build steps.",
                pageWidthMm = mainModel.PageWidthMm ?? 210,  // LPub3D PAGE SIZE or A4 default
                pageHeightMm = mainModel.PageHeightMm ?? 297,
                isPublished = false,
                objectGuid = Guid.NewGuid(),
                versionNumber = 1,
                active = true,
                deleted = false
            };

            _context.BuildManuals.Add(initialManual);
            await _context.SaveChangesAsync(cancellationToken);

            //
            // Compute page boundaries.
            // If the file contains "!LPUB INSERT PAGE" markers, use those to define pages.
            // Otherwise, fall back to grouping at STEPS_PER_PAGE steps per page.
            //
            List<(int first, int last)> pageBoundaries = new List<(int, int)>();
            bool hasExplicitPageBreaks = mainModel.Steps.Any(s => s.IsPageBreak);

            if (hasExplicitPageBreaks)
            {
                int pageStart = 1; // 1-based step number

                for (int s = 0; s < mainModelStepCount; s++)
                {
                    int stepNum = s + 1;
                    LDrawStep step = (s < mainModel.Steps.Count) ? mainModel.Steps[s] : null;

                    //
                    // IsPageBreak means "start a new page BEFORE this step"
                    //
                    if (step?.IsPageBreak == true && stepNum > pageStart)
                    {
                        pageBoundaries.Add((pageStart, stepNum - 1));
                        pageStart = stepNum;
                    }
                }

                // Add the final page
                if (pageStart <= mainModelStepCount)
                {
                    pageBoundaries.Add((pageStart, mainModelStepCount));
                }
            }
            else
            {
                // Default: fixed STEPS_PER_PAGE grouping
                int subTotalPages = Math.Max(1, (int)Math.Ceiling((double)mainModelStepCount / STEPS_PER_PAGE));

                for (int p = 0; p < subTotalPages; p++)
                {
                    int first = (p * STEPS_PER_PAGE) + 1;
                    int last = Math.Min(first + STEPS_PER_PAGE - 1, mainModelStepCount);
                    pageBoundaries.Add((first, last));
                }
            }


            //
            // Track created BuildManualStep IDs so we can link them to PlacedBricks afterwards
            //
            Dictionary<int, int> stepNumToManualStepId = new Dictionary<int, int>();
            int totalPages = pageBoundaries.Count;

            for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
            {
                int firstStepOnPage = pageBoundaries[pageIndex].first;
                int lastStepOnPage = pageBoundaries[pageIndex].last;
                int stepsOnThisPage = lastStepOnPage - firstStepOnPage + 1;

                string pageTitle;

                if (pageIndex == 0 && totalPages == 1)
                {
                    pageTitle = "Build Instructions";
                }
                else if (firstStepOnPage == lastStepOnPage)
                {
                    pageTitle = $"Step {firstStepOnPage}";
                }
                else
                {
                    pageTitle = $"Steps {firstStepOnPage}–{lastStepOnPage}";
                }

                BuildManualPage page = new BuildManualPage
                {
                    buildManualId = initialManual.id,
                    tenantGuid = tenantGuid,
                    pageNum = pageIndex + 1,
                    title = pageTitle,
                    backgroundTheme = "Default",
                    layoutPreset = stepsOnThisPage <= 1 ? "SingleStep" : "Grid",
                    backgroundColorHex = mainModel.PageBackgroundColorHex,
                    objectGuid = Guid.NewGuid(),
                    versionNumber = 1,
                    active = true,
                    deleted = false
                };

                _context.BuildManualPages.Add(page);
                await _context.SaveChangesAsync(cancellationToken);

                //
                // Create BuildManualStep records with step-specific camera
                //
                for (int stepNum = firstStepOnPage; stepNum <= lastStepOnPage; stepNum++)
                {
                    int stepIdx = stepNum - 1; // 0-based index into the pre-computed arrays

                    //
                    // Camera target = centroid of all parts placed up to this step
                    //
                    float targetX = cumulativeCentroidX[stepIdx];
                    float targetY = cumulativeCentroidY[stepIdx];
                    float targetZ = cumulativeCentroidZ[stepIdx];

                    //
                    // Camera position = isometric offset from centroid, scaled by model extent.
                    // If the parsed step has a ROTSTEP rotation, use that to derive the camera
                    // offset instead of the fixed isometric angle.
                    //
                    float distance = cumulativeExtent[stepIdx] * CAMERA_DISTANCE_PADDING;

                    LDrawStep parsedStep = (stepIdx < mainModel.Steps.Count) ? mainModel.Steps[stepIdx] : null;
                    float camX, camY, camZ;

                    if (parsedStep?.RotStepX != null)
                    {
                        //
                        // ROTSTEP provides elevation (X) and azimuth (Y) in degrees.
                        // Convert to a camera offset from the centroid.
                        //
                        float elevRad = parsedStep.RotStepX.Value * (float)(Math.PI / 180.0);
                        float azimRad = parsedStep.RotStepY.Value * (float)(Math.PI / 180.0);

                        camX = targetX + distance * (float)Math.Cos(elevRad) * (float)Math.Sin(azimRad);
                        camY = targetY - distance * (float)Math.Sin(elevRad);
                        camZ = targetZ + distance * (float)Math.Cos(elevRad) * (float)Math.Cos(azimRad);
                    }
                    else
                    {
                        // Default isometric camera
                        camX = targetX + (distance * ISO_ANGLE_FACTOR);
                        camY = targetY - distance;
                        camZ = targetZ + (distance * ISO_ANGLE_FACTOR);
                    }

                    BuildManualStep manualStep = new BuildManualStep
                    {
                        buildManualPageId = page.id,
                        tenantGuid = tenantGuid,
                        stepNumber = stepNum,
                        cameraPositionX = camX,
                        cameraPositionY = camY,
                        cameraPositionZ = camZ,
                        cameraTargetX = targetX,
                        cameraTargetY = targetY,
                        cameraTargetZ = targetZ,
                        cameraZoom = 1.0f,
                        showExplodedView = false,
                        explodedDistance = null,
                        fadeStepEnabled = parsedStep?.FadePrevStep ?? true,
                        isCallout = parsedStep?.IsCallout ?? false,
                        calloutModelName = parsedStep?.CalloutModelName,
                        showPartsListImage = parsedStep?.ShowPartsListImage ?? true,
                        renderImagePath = null,
                        pliImagePath = null,
                        versionNumber = 1,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };

                    _context.BuildManualSteps.Add(manualStep);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }

            //
            // Step 8b: Create BuildStepPart junction records
            //
            // Link each BuildManualStep to its PlacedBrick entities.
            // PlacedBricks have buildStepNumber set during import (Step 6),
            // and BuildManualSteps have stepNumber matching the same values.
            //
            // Query the just-created steps and the placed bricks for this project,
            // then join them on step number.
            //
            List<BuildManualStep> createdSteps = await _context.BuildManualSteps
                .Where(s => s.buildManualPage.buildManual.projectId == project.id
                         && s.tenantGuid == tenantGuid
                         && s.active == true
                         && s.deleted == false)
                .ToListAsync(cancellationToken);

            List<PlacedBrick> projectBricks = await _context.PlacedBricks
                .Where(pb => pb.projectId == project.id
                          && pb.tenantGuid == tenantGuid
                          && pb.active == true
                          && pb.deleted == false)
                .ToListAsync(cancellationToken);

            //
            // Group placed bricks by their build step number
            //
            Dictionary<int, List<PlacedBrick>> bricksByStep = new Dictionary<int, List<PlacedBrick>>();

            foreach (PlacedBrick brick in projectBricks)
            {
                if (brick.buildStepNumber.HasValue == false)
                {
                    continue;
                }

                int bsn = brick.buildStepNumber.Value;

                if (bricksByStep.ContainsKey(bsn) == false)
                {
                    bricksByStep[bsn] = new List<PlacedBrick>();
                }

                bricksByStep[bsn].Add(brick);
            }

            //
            // Create the junction records
            //
            int totalStepParts = 0;

            foreach (BuildManualStep manualStep in createdSteps)
            {
                if (manualStep.stepNumber.HasValue == false)
                {
                    continue;
                }

                if (bricksByStep.TryGetValue(manualStep.stepNumber.Value, out List<PlacedBrick> stepBricks) == false)
                {
                    continue;
                }

                foreach (PlacedBrick brick in stepBricks)
                {
                    BuildStepPart stepPart = new BuildStepPart
                    {
                        buildManualStepId = manualStep.id,
                        placedBrickId = brick.id,
                        tenantGuid = tenantGuid,
                        versionNumber = 1,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };

                    _context.BuildStepParts.Add(stepPart);
                    totalStepParts++;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Auto-populated manual for Project '{Name}' — {Pages} pages, {Steps} steps, {StepParts} step-part links",
                projectName,
                totalPages,
                mainModelStepCount,
                totalStepParts);

            //
            // Step 8c: Render step preview images
            //
            // Generate a small preview PNG for each build step using the software renderer.
            // Images are stored as base64 data URIs in renderImagePath so the client can
            // display them directly in <img> tags without a separate file-serving endpoint.
            //
            // This uses the same RenderService.RenderStep() method that ManualGeneratorHub uses,
            // which renders all parts cumulatively up to the given step with auto-framed camera.
            //
            // Rendering is non-critical — failures are logged but don't break the import.
            //
            try
            {
                string dataPath = _configuration.GetValue<string>("LDraw:DataPath");

                if (string.IsNullOrEmpty(dataPath) == false)
                {
                    RenderService renderService = new RenderService(dataPath);

                    _logger.LogInformation(
                        "Rendering {Count} step preview images for Project '{Name}'",
                        mainModelStepCount, projectName);

                    for (int stepIdx = 0; stepIdx < mainModelStepCount; stepIdx++)
                    {
                        try
                        {
                            byte[] stepPng = renderService.RenderStep(
                                lines: lDrawLines,
                                fileName: fileName,
                                stepIndex: stepIdx,
                                width: 256,
                                height: 256,
                                colourCode: -1,
                                elevation: 30f,
                                azimuth: -45f,
                                renderEdges: true,
                                smoothShading: true,
                                antiAliasMode: AntiAliasMode.SSAA2x);

                            if (stepPng != null && stepPng.Length > 0)
                            {
                                //
                                // Find the matching BuildManualStep and set renderImagePath
                                // to a base64 data URI for direct use in <img src="...">
                                //
                                BuildManualStep matchingStep = createdSteps
                                    .FirstOrDefault(s => s.stepNumber.HasValue && s.stepNumber.Value == stepIdx + 1);

                                if (matchingStep != null)
                                {
                                    matchingStep.renderImagePath =
                                        "data:image/png;base64," + Convert.ToBase64String(stepPng);
                                }
                            }
                        }
                        catch (Exception stepEx)
                        {
                            _logger.LogWarning(stepEx,
                                "Failed to render step {StepIndex} for Project '{Name}' — skipping this step image",
                                stepIdx, projectName);
                        }
                    }

                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Step preview images rendered for Project '{Name}'",
                        projectName);
                }
                else
                {
                    _logger.LogWarning(
                        "LDraw:DataPath is not configured — skipping step image rendering for Project '{Name}'",
                        projectName);
                }
            }
            catch (Exception renderEx)
            {
                _logger.LogWarning(renderEx,
                    "Failed to render step images for Project '{Name}' — import will continue without step previews",
                    projectName);
            }

            //
            // Step 8d: Generate PLI (Parts List Indicator) images
            //
            // For each step, render a grid of the individual parts added in that step
            // with quantity labels. Uses the same RenderService with thumbnail caching
            // so repeated parts across steps are only rendered once.
            //
            try
            {
                string dataPath = _configuration.GetValue<string>("LDraw:DataPath");

                if (string.IsNullOrEmpty(dataPath) == false && mainModel.Steps.Count > 0)
                {
                    // Reuse the same RenderService instance so the thumbnail cache persists
                    RenderService pliRenderService = new RenderService(dataPath);

                    _logger.LogInformation(
                        "Generating {Count} PLI images for Project '{Name}'",
                        mainModelStepCount, projectName);

                    for (int stepIdx = 0; stepIdx < mainModelStepCount && stepIdx < mainModel.Steps.Count; stepIdx++)
                    {
                        try
                        {
                            LDrawStep parsedStep = mainModel.Steps[stepIdx];
                            if (parsedStep.Parts == null || parsedStep.Parts.Count == 0) continue;

                            //
                            // Group parts by (FileName, ColourCode) to get unique combos with quantities
                            //
                            var partGroups = parsedStep.Parts
                                .GroupBy(p => (p.FileName, p.ColourCode))
                                .Select(g => (FileName: g.Key.FileName, ColourCode: g.Key.ColourCode, Quantity: g.Count()))
                                .ToList();

                            byte[] pliPng = pliRenderService.RenderPliGrid(partGroups, cellSize: 64);

                            if (pliPng != null && pliPng.Length > 0)
                            {
                                BuildManualStep matchingStep = createdSteps
                                    .FirstOrDefault(s => s.stepNumber.HasValue && s.stepNumber.Value == stepIdx + 1);

                                if (matchingStep != null)
                                {
                                    matchingStep.pliImagePath =
                                        "data:image/png;base64," + Convert.ToBase64String(pliPng);
                                }
                            }
                        }
                        catch (Exception pliEx)
                        {
                            _logger.LogWarning(pliEx,
                                "Failed to generate PLI image for step {StepIndex} of Project '{Name}' — skipping",
                                stepIdx, projectName);
                        }
                    }

                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "PLI images generated for Project '{Name}'",
                        projectName);
                }
            }
            catch (Exception pliEx)
            {
                _logger.LogWarning(pliEx,
                    "Failed to generate PLI images for Project '{Name}' — import will continue without PLI",
                    projectName);
            }

            //
            // Step 9: Build and return the result
            //
            ImportResult result = new ImportResult
            {
                ProjectId = project.id,
                ProjectName = project.name,
                TotalPartCount = totalPartCount,
                SubmodelCount = submodelByName.Count,
                StepCount = totalStepCount,
                ResolvedPartCount = resolvedParts,
                UnresolvedPartCount = _unresolvedPartsList.Distinct().Count(),
                UnresolvedParts = _unresolvedPartsList.Distinct().ToList(),
                UnresolvedColours = _unresolvedColoursList.Distinct().ToList(),
                SourceFormat = sourceFormat
            };

            return result;
        }


        /// <summary>
        ///
        /// Resolve a LDraw part filename to a BrickPart.id using the lookup cache.
        ///
        /// Tries several variations of the filename:
        ///   1. Exact match (e.g., "3001.dat")
        ///   2. Without .dat extension (e.g., "3001")
        ///   3. Without path prefix (e.g., "s\3001s01.dat" → "3001s01")
        ///
        /// Returns the BrickPart.id if found, or 0 if unresolved.
        ///
        /// </summary>
        private int ResolvePartId(string partFileName)
        {
            if (string.IsNullOrWhiteSpace(partFileName) == true)
            {
                return 0;
            }

            //
            // Try exact match first
            //
            if (_partLookupCache.TryGetValue(partFileName, out int exactId) == true)
            {
                return exactId;
            }

            //
            // Strip the .dat extension and try again
            //
            string withoutExtension = partFileName;

            if (partFileName.EndsWith(".dat", StringComparison.OrdinalIgnoreCase) == true)
            {
                withoutExtension = partFileName.Substring(0, partFileName.Length - 4);
            }

            if (_partLookupCache.TryGetValue(withoutExtension, out int noExtId) == true)
            {
                return noExtId;
            }

            //
            // Strip any path prefix (some LDraw files use "s\3001s01.dat" for sub-parts)
            //
            string baseName = Path.GetFileNameWithoutExtension(partFileName);

            if (baseName != withoutExtension)
            {
                if (_partLookupCache.TryGetValue(baseName, out int baseId) == true)
                {
                    return baseId;
                }
            }

            //
            // Could not resolve — log and track it
            //
            if (_unresolvedPartsList.Contains(partFileName) == false)
            {
                _unresolvedPartsList.Add(partFileName);
                _logger.LogWarning("Unresolved LDraw part: '{PartFileName}'", partFileName);
            }

            return 0;
        }


        /// <summary>
        ///
        /// Resolve a LDraw colour code to a BrickColour.id using the lookup cache.
        ///
        /// Colour code 16 (current colour) and 24 (edge colour) are special in LDraw
        /// and are mapped to a default colour (black, id from colour code 0).
        ///
        /// Returns the BrickColour.id if found, or 0 if unresolved.
        ///
        /// </summary>
        private int ResolveColourId(int ldrawColourCode)
        {
            //
            // Direct lookup
            //
            if (_colourLookupCache.TryGetValue(ldrawColourCode, out int colourId) == true)
            {
                return colourId;
            }

            //
            // Special colour codes 16 (current colour) and 24 (edge colour)
            // map to the default colour (typically black = LDraw code 0)
            //
            if (ldrawColourCode == 16 || ldrawColourCode == 24)
            {
                if (_colourLookupCache.TryGetValue(0, out int defaultId) == true)
                {
                    return defaultId;
                }
            }

            //
            // Could not resolve — log and track it
            //
            if (_unresolvedColoursList.Contains(ldrawColourCode) == false)
            {
                _unresolvedColoursList.Add(ldrawColourCode);
                _logger.LogWarning("Unresolved LDraw colour code: {ColourCode}", ldrawColourCode);
            }

            return 0;
        }


        /// <summary>
        ///
        /// Convert a 3x3 rotation matrix to a quaternion.
        ///
        /// Uses the Shepperd method for numerical stability. The matrix is expected in
        /// row-major order as 9 floats: [a, b, c, d, e, f, g, h, i] representing:
        ///
        ///   | a b c |     (index 0, 1, 2)
        ///   | d e f |     (index 3, 4, 5)
        ///   | g h i |     (index 6, 7, 8)
        ///
        /// The resulting quaternion is normalized.
        ///
        /// </summary>
        public static void MatrixToQuaternion(float[] matrix, out float qX, out float qY, out float qZ, out float qW)
        {
            //
            // Default to identity quaternion if the matrix is invalid
            //
            if (matrix == null || matrix.Length < 9)
            {
                qX = 0f;
                qY = 0f;
                qZ = 0f;
                qW = 1f;
                return;
            }

            //
            // Extract the 3x3 rotation matrix elements
            //
            float m00 = matrix[0];
            float m01 = matrix[1];
            float m02 = matrix[2];
            float m10 = matrix[3];
            float m11 = matrix[4];
            float m12 = matrix[5];
            float m20 = matrix[6];
            float m21 = matrix[7];
            float m22 = matrix[8];

            //
            // Compute the trace (sum of diagonal elements)
            //
            float trace = m00 + m11 + m22;

            if (trace > 0f)
            {
                //
                // The standard case — trace is positive
                //
                float s = (float)Math.Sqrt(trace + 1f) * 2f;
                qW = 0.25f * s;
                qX = (m21 - m12) / s;
                qY = (m02 - m20) / s;
                qZ = (m10 - m01) / s;
            }
            else if (m00 > m11 && m00 > m22)
            {
                //
                // m00 is the largest diagonal element
                //
                float s = (float)Math.Sqrt(1f + m00 - m11 - m22) * 2f;
                qW = (m21 - m12) / s;
                qX = 0.25f * s;
                qY = (m01 + m10) / s;
                qZ = (m02 + m20) / s;
            }
            else if (m11 > m22)
            {
                //
                // m11 is the largest diagonal element
                //
                float s = (float)Math.Sqrt(1f + m11 - m00 - m22) * 2f;
                qW = (m02 - m20) / s;
                qX = (m01 + m10) / s;
                qY = 0.25f * s;
                qZ = (m12 + m21) / s;
            }
            else
            {
                //
                // m22 is the largest diagonal element
                //
                float s = (float)Math.Sqrt(1f + m22 - m00 - m11) * 2f;
                qW = (m10 - m01) / s;
                qX = (m02 + m20) / s;
                qY = (m12 + m21) / s;
                qZ = 0.25f * s;
            }

            //
            // Normalize the quaternion to account for numerical error
            //
            float length = (float)Math.Sqrt(qX * qX + qY * qY + qZ * qZ + qW * qW);

            if (length > 0.0001f)
            {
                qX /= length;
                qY /= length;
                qZ /= length;
                qW /= length;
            }
            else
            {
                //
                // Degenerate case — return identity
                //
                qX = 0f;
                qY = 0f;
                qZ = 0f;
                qW = 1f;
            }
        }


        /// <summary>
        /// Returns the MIME type for a given model file format extension.
        /// </summary>
        private static string GetMimeType(string format)
        {
            if (format == FORMAT_IO)
            {
                return "application/x-bricklink-studio";
            }

            if (format == FORMAT_LDR || format == FORMAT_MPD)
            {
                return "application/x-ldraw";
            }

            if (format == FORMAT_LXF)
            {
                return "application/x-lego-lxf";
            }

            return "application/octet-stream";
        }


        /// <summary>
        ///
        /// Check whether a project name already exists for the given tenant,
        /// and if so, append an incrementing suffix until a unique name is found.
        ///
        /// Examples:  "My Ship" → "My Ship (2)" → "My Ship (3)" etc.
        ///
        /// </summary>
        private async Task<string> GetUniqueProjectNameAsync(string baseName, Guid tenantGuid, CancellationToken cancellationToken)
        {
            string candidateName = baseName;
            int suffix = 2;

            while (await _context.Projects
                .AnyAsync(p => p.tenantGuid == tenantGuid
                            && p.name == candidateName
                            && p.active == true
                            && p.deleted == false, cancellationToken))
            {
                candidateName = $"{baseName} ({suffix})";
                suffix++;

                //
                // Safety valve — don't loop forever if something is very wrong
                //
                if (suffix > 1000)
                {
                    candidateName = $"{baseName} ({Guid.NewGuid().ToString("N").Substring(0, 6)})";
                    break;
                }
            }

            if (candidateName != baseName)
            {
                _logger.LogInformation("Project name '{OriginalName}' already exists — using '{UniqueName}' instead", baseName, candidateName);
            }

            return candidateName;
        }


        /// <summary>
        ///
        /// Check whether a submodel name already exists for the given tenant,
        /// and if so, append an incrementing suffix until a unique name is found.
        ///
        /// </summary>
        private async Task<string> GetUniqueSubmodelNameAsync(string baseName, Guid tenantGuid, CancellationToken cancellationToken)
        {
            string candidateName = baseName;
            int suffix = 2;

            while (await _context.Submodels
                .AnyAsync(s => s.tenantGuid == tenantGuid
                            && s.name == candidateName
                            && s.active == true
                            && s.deleted == false, cancellationToken))
            {
                candidateName = $"{baseName} ({suffix})";
                suffix++;

                if (suffix > 1000)
                {
                    candidateName = $"{baseName} ({Guid.NewGuid().ToString("N").Substring(0, 6)})";
                    break;
                }
            }

            if (candidateName != baseName)
            {
                _logger.LogInformation("Submodel name '{OriginalName}' already exists — using '{UniqueName}' instead", baseName, candidateName);
            }

            return candidateName;
        }


        /// <summary>
        ///
        /// Generate a PNG thumbnail from parsed LDraw lines using the software renderer.
        ///
        /// This follows the same pattern as PartRendererController: creates a RenderService
        /// with the LDraw data path from configuration, then calls RenderToPng with the
        /// in-memory lines.
        ///
        /// Returns null if rendering fails — the import should not be blocked by a
        /// thumbnail generation failure.
        ///
        /// </summary>
        private byte[] GenerateThumbnailFromLDraw(string[] lDrawLines, string fileName)
        {
            if (lDrawLines == null || lDrawLines.Length == 0)
            {
                return null;
            }

            try
            {
                string dataPath = _configuration.GetValue<string>("LDraw:DataPath");

                if (string.IsNullOrEmpty(dataPath))
                {
                    _logger.LogWarning("LDraw:DataPath is not configured — skipping thumbnail generation for '{FileName}'", fileName);
                    return null;
                }

                _logger.LogInformation("Generating thumbnail for '{FileName}' using software renderer", fileName);

                RenderService renderService = new RenderService(dataPath);

                byte[] pngData = renderService.RenderToPng(
                    lDrawLines,
                    fileName,
                    width: 512,
                    height: 512,
                    colourCode: 4,          // default red colour
                    elevation: 30f,
                    azimuth: -45f,
                    renderEdges: true,
                    smoothShading: true,
                    antiAliasMode: AntiAliasMode.SSAA2x);

                if (pngData != null && pngData.Length > 0)
                {
                    _logger.LogInformation("Thumbnail generated for '{FileName}': {Size} bytes", fileName, pngData.Length);
                    return pngData;
                }

                _logger.LogWarning("Thumbnail rendering returned empty data for '{FileName}'", fileName);
                return null;
            }
            catch (Exception ex)
            {
                //
                // Thumbnail generation is non-critical — log the error but don't fail the import
                //
                _logger.LogWarning(ex, "Failed to generate thumbnail for '{FileName}' — import will continue without a thumbnail", fileName);
                return null;
            }
        }
    }
}
