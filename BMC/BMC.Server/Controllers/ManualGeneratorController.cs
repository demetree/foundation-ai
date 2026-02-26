using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Foundation.Security;
using BMC.LDraw.Render;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// Manual Generator controller — analyses uploaded LDraw files and
    /// stores them for SignalR-driven manual generation.
    /// </summary>
    public class ManualGeneratorController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
        private const long MAX_FILE_SIZE_BYTES = 20 * 1024 * 1024;  // 20 MB

        private readonly IConfiguration _configuration;
        private readonly ILogger<ManualGeneratorController> _logger;

        /// <summary>
        /// In-memory store for uploaded files awaiting SignalR-driven generation.
        /// Key = generationId, Value = (lines, fileName, uploadTime).
        /// Entries expire after 10 minutes.
        /// </summary>
        internal static readonly ConcurrentDictionary<string, (string[] Lines, string FileName, DateTime UploadTime)> PendingFiles
            = new ConcurrentDictionary<string, (string[], string, DateTime)>();

        /// <summary>
        /// In-memory store for completed manual downloads.
        /// The hub writes PDF/HTML bytes here; the client fetches via GET.
        /// Entries are removed on download or after 10 minutes.
        /// </summary>
        internal static readonly ConcurrentDictionary<string, (byte[] Bytes, string FileName, string ContentType, DateTime CreatedAt)> CompletedManuals
            = new ConcurrentDictionary<string, (byte[], string, string, DateTime)>();

        public ManualGeneratorController(
            IConfiguration configuration,
            ILogger<ManualGeneratorController> logger
        ) : base("BMC", "PartRenderer")
        {
            _configuration = configuration;
            _logger = logger;
        }


        // ════════════════════════════════════════════════════════════════
        //  Analyse Upload
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// POST /api/manual-generator/analyse-upload
        ///
        /// Accepts an uploaded .dat, .ldr, or .mpd file and returns per-step
        /// analysis: parts lists, bounding boxes, and cumulative counts.
        /// </summary>
        [HttpPost]
        [Route("api/manual-generator/analyse-upload")]
        public async Task<IActionResult> AnalyseUpload(
            Microsoft.AspNetCore.Http.IFormFile file,
            CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided.");
            }

            string ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (ext != ".dat" && ext != ".ldr" && ext != ".mpd")
            {
                return BadRequest("Unsupported file type. Accepted formats: .dat, .ldr, .mpd");
            }

            if (file.Length > MAX_FILE_SIZE_BYTES)
            {
                return BadRequest($"File is too large. Maximum size is {MAX_FILE_SIZE_BYTES / (1024 * 1024)} MB.");
            }

            //
            // Read uploaded file into lines (in-memory, no temp file)
            //
            string[] lines;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                string content = await reader.ReadToEndAsync();
                lines = content.Split('\n');
            }
            string fileName = file.FileName;

            try
            {
                //
                // Create a RenderService for the analysis.
                // Note: controllers are transient (new instance per request),
                // so there is no benefit to caching this at the instance level.
                //
                string dataPath = _configuration.GetValue<string>("LDraw:DataPath");
                RenderService renderService = new RenderService(dataPath);

                var analysis = await Task.Run(() => renderService.AnalyseSteps(lines, fileName), cancellationToken);

                return Ok(new
                {
                    fileName = fileName,
                    stepCount = analysis.Count,
                    steps = analysis.Select(s => new
                    {
                        stepIndex = s.StepIndex,
                        newParts = s.NewParts.Select(p => new
                        {
                            fileName = p.FileName,
                            colourCode = p.ColourCode,
                            quantity = p.Quantity,
                            partDescription = p.PartDescription,
                            colourName = p.ColourName,
                            colourHex = p.ColourHex
                        }),
                        cumulativePartCount = s.CumulativePartCount,
                        cumulativeTriangleCount = s.CumulativeTriangleCount
                    }),
                    totalParts = analysis.LastOrDefault()?.CumulativePartCount ?? 0,
                    totalTriangleCount = analysis.LastOrDefault()?.CumulativeTriangleCount ?? 0,
                    uniquePartCount = analysis
                        .SelectMany(s => s.NewParts)
                        .Select(p => $"{p.FileName}|{p.ColourCode}")
                        .Distinct()
                        .Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Step analysis failed for {FileName}", fileName);
                return StatusCode(500, "Step analysis failed: " + ex.Message);
            }
        }


        // ════════════════════════════════════════════════════════════════
        //  Generate Upload — stores file for SignalR-driven generation
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// POST /api/manual-generator/generate-upload
        ///
        /// Stores the uploaded file and returns a generationId.
        /// The client then connects via SignalR and invokes GenerateManual(generationId, options).
        /// </summary>
        [HttpPost]
        [Route("api/manual-generator/generate-upload")]
        public async Task<IActionResult> GenerateUpload(
            Microsoft.AspNetCore.Http.IFormFile file,
            CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided.");
            }

            string ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (ext != ".dat" && ext != ".ldr" && ext != ".mpd")
            {
                return BadRequest("Unsupported file type. Accepted formats: .dat, .ldr, .mpd");
            }

            if (file.Length > MAX_FILE_SIZE_BYTES)
            {
                return BadRequest($"File is too large. Maximum size is {MAX_FILE_SIZE_BYTES / (1024 * 1024)} MB.");
            }

            //
            // Read uploaded file into lines
            //
            string[] lines;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                string content = await reader.ReadToEndAsync();
                lines = content.Split('\n');
            }

            //
            // Evict expired entries (older than 10 minutes)
            //
            DateTime cutoff = DateTime.UtcNow.AddMinutes(-10);
            foreach (string key in PendingFiles.Keys.ToList())
            {
                if (PendingFiles.TryGetValue(key, out var entry) && entry.UploadTime < cutoff)
                {
                    PendingFiles.TryRemove(key, out _);
                }
            }

            string generationId = Guid.NewGuid().ToString("N");
            PendingFiles[generationId] = (lines, file.FileName, DateTime.UtcNow);

            return Ok(new { generationId = generationId });
        }


        // ════════════════════════════════════════════════════════════════
        //  Download completed manual PDF
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/manual-generator/download/{id}
        ///
        /// Serves a completed manual (PDF or HTML) from temporary in-memory storage.
        /// The entry is removed after download (single-use).
        /// </summary>
        [HttpGet]
        [Route("api/manual-generator/download/{id}")]
        public async Task<IActionResult> DownloadManual(string id, CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            // Evict expired manual downloads (older than 10 minutes)
            DateTime cutoff = DateTime.UtcNow.AddMinutes(-10);
            foreach (string key in CompletedManuals.Keys.ToList())
            {
                if (CompletedManuals.TryGetValue(key, out var e) && e.CreatedAt < cutoff)
                {
                    CompletedManuals.TryRemove(key, out _);
                }
            }

            if (!CompletedManuals.TryRemove(id, out var entry))
            {
                return NotFound("Download not found or expired.");
            }

            bool isPdf = entry.ContentType == "application/pdf";
            string ext = isPdf ? ".pdf" : ".html";
            string downloadName = (Path.GetFileNameWithoutExtension(entry.FileName) ?? "manual")
                + "_build-manual" + ext;

            return File(entry.Bytes, entry.ContentType, downloadName);
        }
    }
}
