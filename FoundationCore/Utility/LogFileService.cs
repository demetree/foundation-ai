//
// Log File Service
//
// Service for reading and parsing log files from configured directories.
// Supports multiple log folder locations for viewing logs from different Foundation applications.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Foundation.LogViewer
{
    //
    // Configuration Models
    //

    /// <summary>
    /// 
    /// Configuration for a single log folder location
    /// 
    /// </summary>
    public class LogFolderConfig
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }


    /// <summary>
    /// 
    /// Root configuration for the log viewer
    /// 
    /// </summary>
    public class LogViewerConfiguration
    {
        public List<LogFolderConfig> LogFolders { get; set; } = new();
    }


    //
    // Log Entry Model
    //

    /// <summary>
    /// 
    /// Parsed log entry from a log file
    /// 
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string TimeOfDay { get; set; }
        public string Level { get; set; }
        public string ThreadName { get; set; }
        public string Message { get; set; }
        public string FileName { get; set; }
        public int LineNumber { get; set; }
    }


    /// <summary>
    /// 
    /// Details about a log file
    /// 
    /// </summary>
    public class LogFileInfo
    {
        public string FileName { get; set; }
        public DateTime LastModified { get; set; }
        public long SizeBytes { get; set; }
        public string SizeDisplay { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
    }


    //
    // Log File Service
    //

    /// <summary>
    /// 
    /// Service for reading and parsing log files
    /// 
    /// </summary>
    public interface ILogFileService
    {
        List<LogFolderConfig> GetConfiguredFolders();
        List<LogFileInfo> GetLogFiles(string folderName);
        (List<LogEntry> Entries, int TotalCount, Dictionary<string, int> LevelCounts) GetLogEntries(
            string folderName,
            string fileName,
            int skip,
            int take,
            string levelFilter,
            string searchText);
        string GetLogFilePath(string folderName, string fileName);
        string GetLogFolderPath(string folderName);
    }


    public class LogFileService : ILogFileService
    {
        private readonly LogViewerConfiguration _config;
        private readonly string _baseDirectory;
        private readonly ILogger<LogFileService> _logger;

        //
        // Log line pattern: HH:MM:SS.ffffff-Level-ThreadName-Message
        //
        private static readonly Regex LogLinePattern = new Regex(
            @"^(\d{2}:\d{2}:\d{2}\.\d+)-(\w+)-([^-]+)-(.*)$",
            RegexOptions.Compiled);


        public LogFileService(LogViewerConfiguration config, string baseDirectory, ILogger<LogFileService> logger = null)
        {
            _config = config ?? new LogViewerConfiguration();
            _baseDirectory = baseDirectory;
            _logger = logger;
        }


        public List<LogFolderConfig> GetConfiguredFolders()
        {
            var folders = new List<LogFolderConfig>();

            //
            // Auto-detect "Log" and "Logs" folders if they exist as direct children of base directory
            //
            var autoDetectNames = new[] { "Log", "Logs" };
            foreach (var folderName in autoDetectNames)
            {
                string fullPath = Path.Combine(_baseDirectory, folderName);
                if (Directory.Exists(fullPath))
                {
                    //
                    // Only add if not already in the configured list
                    //
                    bool alreadyConfigured = _config.LogFolders?.Any(f =>
                        f.Path.Equals(folderName, StringComparison.OrdinalIgnoreCase) ||
                        f.Path.Equals(fullPath, StringComparison.OrdinalIgnoreCase)) ?? false;

                    if (!alreadyConfigured)
                    {
                        folders.Add(new LogFolderConfig
                        {
                            Name = $"Local ({folderName})",
                            Path = folderName
                        });
                    }
                }
            }

            //
            // Add configured folders from appsettings, but only if they exist
            //
            if (_config.LogFolders != null)
            {
                foreach (var folder in _config.LogFolders)
                {
                    string fullPath = GetFullPath(folder.Path);

                    if (Directory.Exists(fullPath))
                    {
                        folders.Add(folder);
                    }
                    else
                    {
                        _logger?.LogWarning($"Configured log folder does not exist and will be skipped: '{folder.Name}' at path '{fullPath}'");
                    }
                }
            }

            return folders;
        }


        /// <summary>
        /// Get combined list of configured and auto-detected folders for lookups
        /// </summary>
        private LogFolderConfig FindFolder(string folderName)
        {
            //
            // First check auto-detected folders
            //
            var autoDetectNames = new[] { "Log", "Logs" };
            foreach (var autoName in autoDetectNames)
            {
                string expectedName = $"Local ({autoName})";
                if (folderName.Equals(expectedName, StringComparison.OrdinalIgnoreCase))
                {
                    string fullPath = Path.Combine(_baseDirectory, autoName);
                    if (Directory.Exists(fullPath))
                    {
                        return new LogFolderConfig { Name = expectedName, Path = autoName };
                    }
                }
            }

            //
            // Then check configured folders
            //
            return _config.LogFolders?.FirstOrDefault(f =>
                f.Name.Equals(folderName, StringComparison.OrdinalIgnoreCase));
        }


        public List<LogFileInfo> GetLogFiles(string folderName)
        {
            var folder = FindFolder(folderName);

            if (folder == null)
            {
                return new List<LogFileInfo>();
            }

            string fullPath = GetFullPath(folder.Path);

            if (!Directory.Exists(fullPath))
            {
                _logger?.LogWarning($"Log directory does not exist: {fullPath}");
                return new List<LogFileInfo>();
            }

            try
            {
                var fileInfos = Directory.GetFiles(fullPath, "*.log")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTimeUtc)
                    .ToList();

                var result = new List<LogFileInfo>();

                foreach (var f in fileInfos)
                {
                    //
                    // Quick scan for error/warning counts
                    //
                    var (errorCount, warningCount) = ScanFileForLevelCounts(f.FullName);

                    result.Add(new LogFileInfo
                    {
                        FileName = f.Name,
                        LastModified = f.LastWriteTimeUtc,
                        SizeBytes = f.Length,
                        SizeDisplay = FormatSize(f.Length),
                        ErrorCount = errorCount,
                        WarningCount = warningCount
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error reading log files from {fullPath}");
                return new List<LogFileInfo>();
            }
        }


        /// <summary>
        /// Quick scan of a log file to count error and warning entries
        /// </summary>
        private (int ErrorCount, int WarningCount) ScanFileForLevelCounts(string filePath)
        {
            int errorCount = 0;
            int warningCount = 0;

            try
            {
                //
                // Read file efficiently line by line
                //
                using var reader = new StreamReader(filePath);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    //
                    // Check if line matches log pattern with Error or Warning level
                    // Pattern: HH:MM:SS.ffffff-Level-ThreadName-Message
                    //
                    var match = LogLinePattern.Match(line);
                    if (match.Success)
                    {
                        string level = match.Groups[2].Value;
                        if (level.Equals("Error", StringComparison.OrdinalIgnoreCase))
                        {
                            errorCount++;
                        }
                        else if (level.Equals("Warning", StringComparison.OrdinalIgnoreCase))
                        {
                            warningCount++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, $"Could not scan file for level counts: {filePath}");
            }

            return (errorCount, warningCount);
        }


        public (List<LogEntry> Entries, int TotalCount, Dictionary<string, int> LevelCounts) GetLogEntries(
            string folderName,
            string fileName,
            int skip,
            int take,
            string levelFilter,
            string searchText)
        {
            var folder = FindFolder(folderName);

            if (folder == null)
            {
                return (new List<LogEntry>(), 0, new Dictionary<string, int>());
            }

            string fullPath = Path.Combine(GetFullPath(folder.Path), fileName);

            if (!File.Exists(fullPath))
            {
                return (new List<LogEntry>(), 0, new Dictionary<string, int>());
            }

            try
            {
                //
                // Read all lines and parse
                //
                var allEntries = new List<LogEntry>();
                var lines = File.ReadAllLines(fullPath);

                LogEntry currentEntry = null;

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    var match = LogLinePattern.Match(line);

                    if (match.Success)
                    {
                        //
                        // New log entry
                        //
                        if (currentEntry != null)
                        {
                            allEntries.Add(currentEntry);
                        }

                        currentEntry = new LogEntry
                        {
                            TimeOfDay = match.Groups[1].Value,
                            Level = match.Groups[2].Value,
                            ThreadName = match.Groups[3].Value,
                            Message = match.Groups[4].Value,
                            FileName = fileName,
                            LineNumber = i + 1
                        };

                        //
                        // Try to parse timestamp (we need the file date for full timestamp)
                        //
                        if (TimeSpan.TryParseExact(
                            match.Groups[1].Value.Substring(0, 8),
                            @"hh\:mm\:ss",
                            CultureInfo.InvariantCulture,
                            out var timeOfDay))
                        {
                            //
                            // Get date from filename if possible (format: Startup_YYYY-MM-DD.log)
                            //
                            var dateMatch = Regex.Match(fileName, @"_(\d{4}-\d{2}-\d{2})\.log$");
                            if (dateMatch.Success && DateTime.TryParse(dateMatch.Groups[1].Value, out var fileDate))
                            {
                                currentEntry.Timestamp = fileDate.Date.Add(timeOfDay);
                            }
                            else
                            {
                                currentEntry.Timestamp = DateTime.Today.Add(timeOfDay);
                            }
                        }
                    }
                    else if (currentEntry != null)
                    {
                        //
                        // Continuation of previous entry (multi-line message)
                        //
                        currentEntry.Message += "\n" + line;
                    }
                }

                //
                // Don't forget the last entry
                //
                if (currentEntry != null)
                {
                    allEntries.Add(currentEntry);
                }

                //
                // Calculate level counts from ALL entries (before filtering)
                //
                var levelCounts = allEntries
                    .GroupBy(e => e.Level, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

                //
                // Apply filters
                //
                var filteredEntries = allEntries.AsEnumerable();

                if (!string.IsNullOrEmpty(levelFilter) && levelFilter.ToLower() != "all")
                {
                    filteredEntries = filteredEntries.Where(e =>
                        e.Level.Equals(levelFilter, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(searchText))
                {
                    string search = searchText.ToLower();
                    filteredEntries = filteredEntries.Where(e =>
                        (e.Message?.ToLower().Contains(search) ?? false) ||
                        (e.ThreadName?.ToLower().Contains(search) ?? false));
                }

                //
                // Order by timestamp descending (most recent first)
                //
                var orderedEntries = filteredEntries
                    .OrderByDescending(e => e.LineNumber)
                    .ToList();

                int totalCount = orderedEntries.Count;

                //
                // Apply pagination
                //
                var pagedEntries = orderedEntries
                    .Skip(skip)
                    .Take(take)
                    .ToList();

                return (pagedEntries, totalCount, levelCounts);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error reading log entries from {fullPath}");
                return (new List<LogEntry>(), 0, new Dictionary<string, int>());
            }
        }


        private string GetFullPath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return path;
            }

            return Path.Combine(_baseDirectory, path);
        }


        private static string FormatSize(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";
            if (bytes < 1024 * 1024)
                return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024.0):F1} MB";
        }


        /// <summary>
        /// Get the full file path for a specific log file (for download)
        /// </summary>
        public string GetLogFilePath(string folderName, string fileName)
        {
            var folder = FindFolder(folderName);
            if (folder == null)
            {
                return null;
            }

            string folderPath = GetFullPath(folder.Path);
            string filePath = Path.Combine(folderPath, fileName);

            //
            // Security check: ensure the file is within the folder
            //
            if (!filePath.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase))
            {
                _logger?.LogWarning($"Attempted path traversal for file: {fileName}");
                return null;
            }

            if (!File.Exists(filePath))
            {
                return null;
            }

            return filePath;
        }


        /// <summary>
        /// Get the full folder path (for zip download)
        /// </summary>
        public string GetLogFolderPath(string folderName)
        {
            var folder = FindFolder(folderName);
            if (folder == null)
            {
                return null;
            }

            string folderPath = GetFullPath(folder.Path);

            if (!Directory.Exists(folderPath))
            {
                return null;
            }

            return folderPath;
        }
    }
}
