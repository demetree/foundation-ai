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
                throw new InvalidOperationException($"Unsupported file format: '.{extension}'. Supported formats are: .ldr, .mpd, .io");
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
            // If we have a thumbnail from an .io import, store it on the Project
            //
            if (ioResult != null && ioResult.ThumbnailData != null && ioResult.ThumbnailData.Length > 0)
            {
                project.thumbnailData = ioResult.ThumbnailData;
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
                            // This is a submodel reference, not a placed brick.
                            // In the full design tool, submodel instances would be represented
                            // differently. For now, we skip these during placed brick creation
                            // as they represent the submodel assembly, not individual parts.
                            //
                            _logger.LogDebug("Skipping submodel reference '{SubmodelName}' in step {Step}", partRef.FileName, stepNumber);
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
            // Step 8: Build and return the result
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
