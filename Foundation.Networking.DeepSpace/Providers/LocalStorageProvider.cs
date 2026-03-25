// ============================================================================
//
// LocalStorageProvider.cs — Local filesystem storage provider.
//
// Implements IStorageProvider using the local filesystem. Stores objects
// as files in a configurable root directory.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using Foundation.Networking.DeepSpace.Configuration;

namespace Foundation.Networking.DeepSpace.Providers
{
    /// <summary>
    ///
    /// Local filesystem storage provider.
    ///
    /// </summary>
    public class LocalStorageProvider : IStorageProvider
    {
        private readonly string _rootPath;
        private readonly long _maxFileSize;


        public LocalStorageProvider(LocalStorageConfig config)
        {
            _rootPath = Path.GetFullPath(config.RootPath);
            _maxFileSize = config.MaxFileSizeBytes;

            if (Directory.Exists(_rootPath) == false)
            {
                Directory.CreateDirectory(_rootPath);
            }
        }


        public string ProviderName => "Local";


        // ── Put ───────────────────────────────────────────────────────────


        public async Task<StorageResult> PutAsync(
            string key, Stream data, string contentType = null,
            Dictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                //
                // Enforce max file size if configured
                //
                if (_maxFileSize > 0 && data.CanSeek == true && data.Length > _maxFileSize)
                {
                    return new StorageResult
                    {
                        Success = false,
                        Error = "File size " + data.Length + " exceeds maximum allowed size of " + _maxFileSize + " bytes"
                    };
                }

                string filePath = GetFilePath(key);
                string directory = Path.GetDirectoryName(filePath);

                if (Directory.Exists(directory) == false)
                {
                    Directory.CreateDirectory(directory);
                }

                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await data.CopyToAsync(fs, 81920, cancellationToken);
                }

                FileInfo info = new FileInfo(filePath);

                //
                // Post-write size check for non-seekable streams
                //
                if (_maxFileSize > 0 && info.Length > _maxFileSize)
                {
                    File.Delete(filePath);
                    return new StorageResult
                    {
                        Success = false,
                        Error = "File size " + info.Length + " exceeds maximum allowed size of " + _maxFileSize + " bytes"
                    };
                }

                //
                // Save metadata as a sidecar file
                //
                if (metadata != null && metadata.Count > 0)
                {
                    SaveMetadata(filePath, metadata);
                }

                return new StorageResult
                {
                    Success = true,
                    Object = CreateStorageObject(key, info, contentType)
                };
            }
            catch (Exception ex)
            {
                return new StorageResult { Success = false, Error = ex.Message };
            }
        }


        public async Task<StorageResult> PutBytesAsync(
            string key, byte[] data, string contentType = null,
            Dictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
        {
            //
            // Eager size check for byte arrays
            //
            if (_maxFileSize > 0 && data.Length > _maxFileSize)
            {
                return new StorageResult
                {
                    Success = false,
                    Error = "File size " + data.Length + " exceeds maximum allowed size of " + _maxFileSize + " bytes"
                };
            }

            using (MemoryStream ms = new MemoryStream(data))
            {
                return await PutAsync(key, ms, contentType, metadata, cancellationToken);
            }
        }


        // ── Get ───────────────────────────────────────────────────────────


        public Task<string> GetPresignedUrlAsync(string key, TimeSpan expires, CancellationToken cancellationToken = default)
        {
            string urlKey = (key ?? string.Empty).Replace('\\', '/').TrimStart('/');
            string url = $"/api/deepspace/download/{ProviderName.ToLowerInvariant()}/{urlKey}";
            return Task.FromResult(url);
        }

        public Task<Stream> GetStreamAsync(string key, CancellationToken cancellationToken = default)
        {
            string filePath = GetFilePath(key);

            if (File.Exists(filePath) == false)
            {
                return Task.FromResult<Stream>(null);
            }

            Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return Task.FromResult(stream);
        }


        public async Task<byte[]> GetBytesAsync(string key, CancellationToken cancellationToken = default)
        {
            string filePath = GetFilePath(key);

            if (File.Exists(filePath) == false)
            {
                return null;
            }

            return await File.ReadAllBytesAsync(filePath, cancellationToken);
        }


        public Task<StorageObject> GetMetadataAsync(string key, CancellationToken cancellationToken = default)
        {
            string filePath = GetFilePath(key);

            if (File.Exists(filePath) == false)
            {
                return Task.FromResult<StorageObject>(null);
            }

            FileInfo info = new FileInfo(filePath);
            StorageObject obj = CreateStorageObject(key, info);
            obj.Metadata = LoadMetadata(filePath);

            return Task.FromResult(obj);
        }


        // ── Exists / Delete ───────────────────────────────────────────────


        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            string filePath = GetFilePath(key);
            return Task.FromResult(File.Exists(filePath));
        }


        public Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            string filePath = GetFilePath(key);

            if (File.Exists(filePath) == false)
            {
                return Task.FromResult(false);
            }

            File.Delete(filePath);

            //
            // Clean up metadata sidecar if it exists
            //
            string metaPath = filePath + ".meta";
            if (File.Exists(metaPath))
            {
                File.Delete(metaPath);
            }

            return Task.FromResult(true);
        }


        // ── List ──────────────────────────────────────────────────────────


        public Task<ListResult> ListAsync(string prefix = "", int maxResults = 1000, CancellationToken cancellationToken = default)
        {
            ListResult result = new ListResult { Success = true };

            try
            {
                string searchPath = _rootPath;

                if (string.IsNullOrEmpty(prefix) == false)
                {
                    string prefixPath = Path.Combine(_rootPath, NormalizeKey(prefix));
                    string prefixDir = Path.GetDirectoryName(prefixPath);

                    if (Directory.Exists(prefixDir))
                    {
                        searchPath = prefixDir;
                    }
                }

                if (Directory.Exists(searchPath))
                {
                    IEnumerable<string> files = Directory.EnumerateFiles(searchPath, "*", SearchOption.AllDirectories)
                        .Where(f => f.EndsWith(".meta") == false)
                        .Take(maxResults);

                    foreach (string file in files)
                    {
                        string key = GetKeyFromPath(file);

                        if (string.IsNullOrEmpty(prefix) || key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        {
                            FileInfo info = new FileInfo(file);
                            result.Objects.Add(CreateStorageObject(key, info));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
            }

            return Task.FromResult(result);
        }


        // ── Copy ──────────────────────────────────────────────────────────


        public Task<StorageResult> CopyAsync(string sourceKey, string destinationKey, CancellationToken cancellationToken = default)
        {
            try
            {
                string sourcePath = GetFilePath(sourceKey);
                string destPath = GetFilePath(destinationKey);

                if (File.Exists(sourcePath) == false)
                {
                    return Task.FromResult(new StorageResult { Success = false, Error = "Source not found" });
                }

                string destDir = Path.GetDirectoryName(destPath);
                if (Directory.Exists(destDir) == false)
                {
                    Directory.CreateDirectory(destDir);
                }

                File.Copy(sourcePath, destPath, true);

                //
                // Copy metadata sidecar if it exists
                //
                string sourceMetaPath = sourcePath + ".meta";
                if (File.Exists(sourceMetaPath))
                {
                    File.Copy(sourceMetaPath, destPath + ".meta", true);
                }

                FileInfo info = new FileInfo(destPath);

                return Task.FromResult(new StorageResult
                {
                    Success = true,
                    Object = CreateStorageObject(destinationKey, info)
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new StorageResult { Success = false, Error = ex.Message });
            }
        }


        // ── Internal ──────────────────────────────────────────────────────


        private string GetFilePath(string key)
        {
            string normalized = NormalizeKey(key);
            return Path.Combine(_rootPath, normalized);
        }


        private string GetKeyFromPath(string filePath)
        {
            string relative = Path.GetRelativePath(_rootPath, filePath);
            return relative.Replace('\\', '/');
        }


        private static string NormalizeKey(string key)
        {
            return key.Replace('/', Path.DirectorySeparatorChar)
                       .Replace('\\', Path.DirectorySeparatorChar)
                       .TrimStart(Path.DirectorySeparatorChar);
        }


        private StorageObject CreateStorageObject(string key, FileInfo info, string contentType = null)
        {
            return new StorageObject
            {
                Key = key,
                SizeBytes = info.Length,
                ContentType = contentType ?? GuessContentType(key),
                LastModifiedUtc = info.LastWriteTimeUtc,
                ETag = info.LastWriteTimeUtc.Ticks.ToString()
            };
        }


        private static string GuessContentType(string key)
        {
            string ext = Path.GetExtension(key)?.ToLowerInvariant();

            switch (ext)
            {
                case ".json": return "application/json";
                case ".xml": return "application/xml";
                case ".txt": return "text/plain";
                case ".html": return "text/html";
                case ".css": return "text/css";
                case ".js": return "application/javascript";
                case ".png": return "image/png";
                case ".jpg": case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                case ".svg": return "image/svg+xml";
                case ".pdf": return "application/pdf";
                case ".zip": return "application/zip";
                default: return "application/octet-stream";
            }
        }


        private void SaveMetadata(string filePath, Dictionary<string, string> metadata)
        {
            string metaPath = filePath + ".meta";
            List<string> lines = new List<string>();

            foreach (var kvp in metadata)
            {
                lines.Add(kvp.Key + "=" + kvp.Value);
            }

            File.WriteAllLines(metaPath, lines);
        }


        private Dictionary<string, string> LoadMetadata(string filePath)
        {
            Dictionary<string, string> metadata = new Dictionary<string, string>();
            string metaPath = filePath + ".meta";

            if (File.Exists(metaPath))
            {
                foreach (string line in File.ReadAllLines(metaPath))
                {
                    int eq = line.IndexOf('=');
                    if (eq > 0)
                    {
                        string key = line.Substring(0, eq);
                        string value = line.Substring(eq + 1);
                        metadata[key] = value;
                    }
                }
            }

            return metadata;
        }
    }
}
