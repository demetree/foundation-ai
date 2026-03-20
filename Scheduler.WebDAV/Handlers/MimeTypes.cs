//
// MimeTypes.cs
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Shared MIME type lookup by file extension.
// Extracted from PutHandler to avoid duplication.
//
using System.IO;

namespace Scheduler.WebDAV.Handlers
{
    /// <summary>
    /// Static utility for resolving MIME types from file extensions.
    /// </summary>
    public static class MimeTypes
    {
        /// <summary>
        /// Guesses the MIME type from a file extension.
        /// Falls through to "application/octet-stream" for unknown types.
        /// </summary>
        public static string FromFileName(string fileName)
        {
            string ext = Path.GetExtension(fileName)?.ToLowerInvariant();

            return ext switch
            {
                ".txt" => "text/plain",
                ".html" or ".htm" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".csv" => "text/csv",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".zip" => "application/zip",
                ".gz" => "application/gzip",
                ".tar" => "application/x-tar",
                ".rar" => "application/vnd.rar",
                ".7z" => "application/x-7z-compressed",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".svg" => "image/svg+xml",
                ".webp" => "image/webp",
                ".ico" => "image/x-icon",
                ".tiff" or ".tif" => "image/tiff",
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".ogg" => "audio/ogg",
                ".flac" => "audio/flac",
                ".mp4" => "video/mp4",
                ".webm" => "video/webm",
                ".avi" => "video/x-msvideo",
                ".mov" => "video/quicktime",
                ".mkv" => "video/x-matroska",
                ".wmv" => "video/x-ms-wmv",
                _ => "application/octet-stream"
            };
        }
    }
}
