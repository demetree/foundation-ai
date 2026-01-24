//
// Log Viewer Controller
//
// API endpoints for reading and viewing system log files.
// Supports multiple configured log folder locations.
//
using DocumentFormat.OpenXml.Office2010.Excel;
using Foundation.Auditor;
using Foundation.LogViewer;
using Foundation.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

namespace Foundation.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// Provides access to log files
    /// 
    /// Note that Log file access control is done through the Auditor Module.  Users must be Auditor Admins to use this interface
    /// 
    /// 
    /// </summary>
    [Route("api/[controller]")]
    public class LogViewerController : SecureWebAPIController
    {
        private readonly ILogFileService _logFileService;
        private readonly ILogger<LogViewerController> _logger;


        public LogViewerController(
            ILogFileService logFileService,
            ILogger<LogViewerController> logger)
            : base("Auditor", "LogViewer")              // Note that Log file access control is done through the Auditor Module.  Users must be Auditor Admins to use this interface
        {
            _logFileService = logFileService;
            _logger = logger;
        }


        //
        // GET: api/LogViewer/folders
        //
        // Returns list of configured log folder locations
        //
        [HttpGet("folders")]
        public IActionResult GetConfiguredFolders()
        {
            //
            // Only admin users can  access log details
            //
            //
            // Verify admin privileges
            //
            if (UserCanAdminister() == false)
            {
                CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Non-admin user attempted to get log configured folders.", false);

                return Forbid();
            }

            try
            {
                var folders = _logFileService.GetConfiguredFolders()
                    .Select(f => new { f.Name })
                    .ToList();

                return Ok(folders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configured folders");
                return Problem("Failed to retrieve folder configuration");
            }
        }


        //
        // GET: api/LogViewer/files/{folderName}
        //
        // Returns list of log files in the specified folder
        //
        [HttpGet("files/{folderName}")]
        public IActionResult GetLogFiles(string folderName)
        {
            //
            // Only admin users can  access log details
            //
            //
            // Verify admin privileges
            //
            if (UserCanAdminister() == false)
            {
                CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Non-admin user attempted to get log files in folder {folderName}.", false);

                return Forbid();
            }

            try
            {
                if (string.IsNullOrEmpty(folderName))
                {
                    return BadRequest("Folder name is required");
                }

                var files = _logFileService.GetLogFiles(folderName);

                return Ok(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting log files for folder: {folderName}");
                return Problem("Failed to retrieve log files");
            }
        }


        //
        // GET: api/LogViewer/entries/{folderName}/{fileName}
        //
        // Returns parsed log entries from the specified file
        //
        [HttpGet("entries/{folderName}/{fileName}")]
        public IActionResult GetLogEntries(
            string folderName,
            string fileName,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100,
            [FromQuery] string level = null,
            [FromQuery] string search = null)
        {
            //
            // Only admin users can  access log details
            //
            //
            // Verify admin privileges
            //
            if (UserCanAdminister() == false)
            {
                CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Non-admin user attempted to get log entrys in {folderName} and {fileName}", false);

                return Forbid();
            }


            try
            {
                if (string.IsNullOrEmpty(folderName) || string.IsNullOrEmpty(fileName))
                {
                    return BadRequest("Folder name and file name are required");
                }

                //
                // Validate take parameter (limit to reasonable size)
                //
                take = Math.Min(take, 1000);

                var (entries, totalCount, levelCounts) = _logFileService.GetLogEntries(
                    folderName,
                    fileName,
                    skip,
                    take,
                    level,
                    search);

                return Ok(new
                {
                    Entries = entries,
                    TotalCount = totalCount,
                    LevelCounts = levelCounts,
                    Skip = skip,
                    Take = take
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting log entries for: {folderName}/{fileName}");
                return Problem("Failed to retrieve log entries");
            }
        }


        //
        // GET: api/LogViewer/tail/{folderName}/{fileName}
        //
        // Returns the most recent entries (for live tailing)
        //
        [HttpGet("tail/{folderName}/{fileName}")]
        public IActionResult TailLogFile(
            string folderName,
            string fileName,
            [FromQuery] int count = 50,
            [FromQuery] string level = null)
        {
            //
            // Only admin users can  access log details
            //
            //
            // Verify admin privileges
            //
            if (UserCanAdminister() == false)
            {
                CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Non-admin user attempted to tail log file {folderName} {fileName}.", false);

                return Forbid();
            }

            try
            {
                if (string.IsNullOrEmpty(folderName) || string.IsNullOrEmpty(fileName))
                {
                    return BadRequest("Folder name and file name are required");
                }

                count = Math.Min(count, 500);

                var (entries, totalCount, levelCounts) = _logFileService.GetLogEntries(
                    folderName,
                    fileName,
                    0,
                    count,
                    level,
                    null);

                return Ok(new
                {
                    Entries = entries,
                    TotalCount = totalCount,
                    LevelCounts = levelCounts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error tailing log file: {folderName}/{fileName}");
                return Problem("Failed to tail log file");
            }
        }


        //
        // GET: api/LogViewer/download/{folderName}/{fileName}
        //
        // Downloads a single log file
        //
        [HttpGet("download/{folderName}/{fileName}")]
        public IActionResult DownloadLogFile(string folderName, string fileName)
        {
            //
            // Only admin users can  access log details
            //
            //
            // Verify admin privileges
            //
            if (UserCanAdminister() == false)
            {
                CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Non-admin user attempted to download log file {folderName} {fileName}.", false);

                return Forbid();
            }


            try
            {
                if (string.IsNullOrEmpty(folderName) || string.IsNullOrEmpty(fileName))
                {
                    return BadRequest("Folder name and file name are required");
                }

                string filePath = _logFileService.GetLogFilePath(folderName, fileName);

                if (filePath == null)
                {
                    return NotFound("Log file not found");
                }

                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                return File(fileStream, "text/plain", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading log file: {folderName}/{fileName}");
                return Problem("Failed to download log file");
            }
        }


        //
        // GET: api/LogViewer/download-all/{folderName}
        //
        // Downloads all log files in the folder as a ZIP archive
        //
        [HttpGet("download-all/{folderName}")]
        public IActionResult DownloadAllLogFiles(string folderName)
        {
            //
            // Only admin users can  access log details
            //
            //
            // Verify admin privileges
            //
            if (UserCanAdminister() == false)
            {
                CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, $"Non-admin user attempted to download all log files in folder {folderName}.", false);

                return Forbid();
            }

            try
            {
                if (string.IsNullOrEmpty(folderName))
                {
                    return BadRequest("Folder name is required");
                }

                string folderPath = _logFileService.GetLogFolderPath(folderName);

                if (folderPath == null)
                {
                    return NotFound("Log folder not found");
                }

                //
                // Create a temporary ZIP file
                //
                var logFiles = Directory.GetFiles(folderPath, "*.log");

                if (logFiles.Length == 0)
                {
                    return NotFound("No log files found in folder");
                }

                var memoryStream = new MemoryStream();
                using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    foreach (var logFile in logFiles)
                    {
                        string entryName = Path.GetFileName(logFile);
                        var entry = archive.CreateEntry(entryName, System.IO.Compression.CompressionLevel.Optimal);

                        using var entryStream = entry.Open();
                        using var fileStream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        fileStream.CopyTo(entryStream);
                    }
                }

                memoryStream.Position = 0;

                //
                // Generate a meaningful ZIP filename
                //
                string sanitizedFolderName = folderName.Replace(" ", "_").Replace("(", "").Replace(")", "");
                string zipFileName = $"logs_{sanitizedFolderName}_{DateTime.Now:yyyyMMdd_HHmmss}.zip";

                return File(memoryStream, "application/zip", zipFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading all log files from: {folderName}");
                return Problem("Failed to download log files");
            }
        }
    }
}
