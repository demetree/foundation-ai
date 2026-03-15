using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Foundation.Auditor;
using Foundation.Controllers;
using Foundation.Security;
using Foundation.Security.Database;
using Foundation.BMC.Database;
using Foundation.BMC.Services;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    ///
    /// Controller for MOC (My Own Creation) import operations.
    ///
    /// Provides endpoints for uploading model files (.ldr, .mpd, .io) and converting them
    /// into BMC's native Project entity hierarchy. The import process stores the raw file
    /// for round-trip fidelity and also creates the native PlacedBrick/Submodel entities
    /// needed for the design canvas and instruction generator.
    ///
    /// All endpoints require authentication and tenant membership.
    ///
    /// AI-developed code — initial implementation March 2026
    ///
    /// </summary>
    public class MocImportController : SecureWebAPIController
    {
        //
        // Constants
        //
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
        private const long MAX_UPLOAD_SIZE_BYTES = 50 * 1024 * 1024;  // 50MB

        //
        // Supported file extensions
        //
        private static readonly HashSet<string> SUPPORTED_EXTENSIONS = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".ldr",
            ".mpd",
            ".io",
            ".lxf"
        };


        //
        // Dependencies
        //
        private readonly BMCContext _context;
        private readonly ModelImportService _importService;
        private readonly ILogger<MocImportController> _logger;


        /// <summary>
        /// Constructor — injects BMC context, import service, and logger.
        /// </summary>
        public MocImportController(BMCContext context, ModelImportService importService, ILogger<MocImportController> logger)
            : base("BMC", "MocImport")
        {
            _context = context;
            _importService = importService;
            _logger = logger;
        }


        #region DTOs

        /// <summary>
        /// Response DTO for the supported formats endpoint.
        /// </summary>
        public class SupportedFormatDto
        {
            public string extension { get; set; }
            public string name { get; set; }
            public string description { get; set; }
        }

        /// <summary>
        /// Response DTO for the upload endpoint.
        /// </summary>
        public class UploadResultDto
        {
            public int projectId { get; set; }
            public string projectName { get; set; }
            public int totalPartCount { get; set; }
            public int submodelCount { get; set; }
            public int stepCount { get; set; }
            public int resolvedPartCount { get; set; }
            public int unresolvedPartCount { get; set; }
            public List<string> unresolvedParts { get; set; }
            public string sourceFormat { get; set; }
        }

        #endregion


        /// <summary>
        ///
        /// GET /api/moc/import/formats
        ///
        /// Returns the list of file formats supported for import.
        ///
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/moc/import/formats")]
        public IActionResult GetSupportedFormats()
        {
            List<SupportedFormatDto> formats = new List<SupportedFormatDto>
            {
                new SupportedFormatDto
                {
                    extension = ".ldr",
                    name = "LDraw Model",
                    description = "Standard LDraw model file with parts and build steps"
                },
                new SupportedFormatDto
                {
                    extension = ".mpd",
                    name = "LDraw Multi-Part Document",
                    description = "LDraw file containing multiple submodels in a single document"
                },
                new SupportedFormatDto
                {
                    extension = ".io",
                    name = "BrickLink Studio",
                    description = "BrickLink Studio project file (contains embedded LDraw model)"
                },
                new SupportedFormatDto
                {
                    extension = ".lxf",
                    name = "LEGO Digital Designer",
                    description = "Legacy LEGO Digital Designer project file (LDD format)"
                }
            };

            return Ok(formats);
        }


        /// <summary>
        ///
        /// POST /api/moc/import/upload
        ///
        /// Uploads a model file and imports it into the user's project workspace.
        ///
        /// Accepts .ldr, .mpd, and .io files up to 50MB.
        /// The file is parsed, stored for round-trip fidelity, and converted into BMC's
        /// native PlacedBrick/Submodel entity structure.
        ///
        /// Returns a summary of the import including resolved/unresolved part statistics.
        ///
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TenPerMinute, Scope = RateLimitScope.PerUser)]
        [Route("api/moc/import/upload")]
        [RequestSizeLimit(MAX_UPLOAD_SIZE_BYTES)]
        public async Task<IActionResult> UploadModel(IFormFile file, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            //
            // Step 1: Validate input
            //
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            if (file.Length > MAX_UPLOAD_SIZE_BYTES)
            {
                return BadRequest($"File size exceeds the maximum allowed size of {MAX_UPLOAD_SIZE_BYTES / (1024 * 1024)}MB.");
            }

            string extension = Path.GetExtension(file.FileName);

            if (string.IsNullOrEmpty(extension) == true || SUPPORTED_EXTENSIONS.Contains(extension) == false)
            {
                return BadRequest($"Unsupported file format '{extension}'. Supported formats: .ldr, .mpd, .io, .lxf");
            }

            //
            // Step 2: Security and tenant resolution
            //
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception)
            {
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }

            //
            // Step 3: Set up SSE streaming and run the import
            //
            // We stream progress events to the client as text/event-stream so the
            // upload modal can show real-time status updates during processing.
            //
            Response.ContentType = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";
            Response.Headers["X-Accel-Buffering"] = "no"; // Disable nginx buffering

            StreamWriter writer = new StreamWriter(Response.Body, System.Text.Encoding.UTF8, leaveOpen: true);

            try
            {
                _logger.LogInformation(
                    "Starting MOC import — file: '{FileName}', size: {Size} bytes, format: '{Extension}'",
                    file.FileName,
                    file.Length,
                    extension);

                //
                // Create a progress reporter that writes SSE events on each report.
                //
                // IMPORTANT: We use a custom IProgress<T> instead of Progress<T>.
                // Progress<T> captures the SynchronizationContext and posts callbacks
                // to it, which can deadlock in ASP.NET Core when the callback is async.
                //
                IProgress<ModelImportService.ImportProgressEvent> progress =
                    new SseProgressReporter(writer);

                ModelImportService.ImportResult result = null;

                using (Stream fileStream = file.OpenReadStream())
                {
                    //
                    // Use CancellationToken.None for the import work.
                    // The request's cancellationToken is tied to the SSE connection,
                    // and any client timeout/disconnect would cancel DB operations
                    // mid-query — causing TaskCanceledException crashes.
                    //
                    result = await _importService.ImportFromFileAsync(
                        fileStream,
                        file.FileName,
                        userTenantGuid,
                        progress,
                        CancellationToken.None);
                }

                //
                // Step 4: Build the response and send as the final SSE event
                //
                UploadResultDto response = new UploadResultDto
                {
                    projectId = result.ProjectId,
                    projectName = result.ProjectName,
                    totalPartCount = result.TotalPartCount,
                    submodelCount = result.SubmodelCount,
                    stepCount = result.StepCount,
                    resolvedPartCount = result.ResolvedPartCount,
                    unresolvedPartCount = result.UnresolvedPartCount,
                    unresolvedParts = result.UnresolvedParts,
                    sourceFormat = result.SourceFormat
                };

                await CreateAuditEventAsync(
                    AuditEngine.AuditType.CreateEntity,
                    $"MOC import — '{file.FileName}' → Project '{result.ProjectName}' (id={result.ProjectId}), {result.TotalPartCount} parts, {result.ResolvedPartCount} resolved, {result.UnresolvedPartCount} unresolved",
                    result.ProjectId.ToString());

                _logger.LogInformation(
                    "MOC import complete — Project '{Name}' (id={Id}), {Parts} parts, {Resolved} resolved, {Unresolved} unresolved",
                    result.ProjectName,
                    result.ProjectId,
                    result.TotalPartCount,
                    result.ResolvedPartCount,
                    result.UnresolvedPartCount);

                // Send the final "complete" event with the full result
                string completeJson = JsonSerializer.Serialize(new { step = "Import complete!", percent = 100, result = response });
                await writer.WriteAsync($"event: complete\ndata: {completeJson}\n\n");
                await writer.FlushAsync();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "MOC import validation error for file '{FileName}'", file.FileName);
                string errorJson = JsonSerializer.Serialize(new { error = ex.Message });
                await writer.WriteAsync($"event: error\ndata: {errorJson}\n\n");
                await writer.FlushAsync();
            }
            catch (InvalidDataException ex)
            {
                _logger.LogWarning(ex, "MOC import file format error for file '{FileName}'", file.FileName);
                string errorJson = JsonSerializer.Serialize(new { error = ex.Message });
                await writer.WriteAsync($"event: error\ndata: {errorJson}\n\n");
                await writer.FlushAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MOC import failed for file '{FileName}'", file.FileName);
                string errorJson = JsonSerializer.Serialize(new { error = "An error occurred while importing the model file. Please try again or contact support." });
                await writer.WriteAsync($"event: error\ndata: {errorJson}\n\n");
                await writer.FlushAsync();
            }
            finally
            {
                await writer.DisposeAsync();
            }

            // Response has already been written via SSE — return empty result
            return new EmptyResult();
        }
    }


    /// <summary>
    /// Simple IProgress implementation that writes SSE events directly.
    /// Unlike Progress&lt;T&gt;, this does NOT capture the SynchronizationContext,
    /// so it cannot deadlock in ASP.NET Core async pipelines.
    /// </summary>
    internal class SseProgressReporter : IProgress<ModelImportService.ImportProgressEvent>
    {
        private readonly StreamWriter _writer;

        public SseProgressReporter(StreamWriter writer)
        {
            _writer = writer;
        }

        public void Report(ModelImportService.ImportProgressEvent evt)
        {
            try
            {
                string json = JsonSerializer.Serialize(new { step = evt.Step, percent = evt.PercentComplete });
                _writer.Write($"data: {json}\n\n");
                _writer.Flush();
            }
            catch
            {
                // Client may have disconnected — ignore write errors
            }
        }
    }
}
