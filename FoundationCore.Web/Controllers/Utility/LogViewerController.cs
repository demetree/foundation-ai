//
// Log Viewer Controller
//
// API endpoints for reading and viewing system log files.
// Supports multiple configured log folder locations.
//
using Foundation.Security;
using Foundation.LogViewer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Foundation.Controllers.WebAPI
{
    [Route("api/[controller]")]
    public class LogViewerController : SecureWebAPIController
    {
        private readonly ILogFileService _logFileService;
        private readonly ILogger<LogViewerController> _logger;


        public LogViewerController(
            ILogFileService logFileService,
            ILogger<LogViewerController> logger)
            : base("Utility", "LogViewer")
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
    }
}
