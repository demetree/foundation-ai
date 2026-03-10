using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
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
    ///   1. "Native" export — reconstructs LDraw from PlacedBrick entities
    ///   2. "Round-trip" export — uses the stored ModelDocument.sourceFileData when available
    ///
    /// AI-developed code — initial implementation March 2026
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


        //
        // Dependencies
        //
        private readonly BMCContext _context;
        private readonly ILogger<ModelExportService> _logger;


        /// <summary>
        /// Constructor — takes the BMC database context and a logger.
        /// </summary>
        public ModelExportService(BMCContext context, ILogger<ModelExportService> logger)
        {
            _context = context;
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
            // Load all placed bricks for this project, ordered by build step and then by id
            //
            List<PlacedBrick> bricks = await _context.PlacedBricks
                .Where(pb => pb.projectId == projectId && pb.tenantGuid == tenantGuid && pb.active == true && pb.deleted == false)
                .OrderBy(pb => pb.buildStepNumber)
                .ThenBy(pb => pb.id)
                .ToListAsync(cancellationToken);

            //
            // Load part and colour lookup tables (reverse direction: id → ldrawPartId/ldrawColourCode)
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
                        // Can't resolve the part — skip this brick
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
