using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace BMC.LDraw.Import
{
    /// <summary>
    /// Downloads the complete LDraw parts library from ldraw.org.
    /// The library is distributed as a single zip file (~90MB).
    /// </summary>
    public class LDrawDownloadService
    {
        private const string DownloadUrl = "https://library.ldraw.org/library/updates/complete.zip";

        private readonly HttpClient _httpClient;
        private readonly Action<string> _log;

        public LDrawDownloadService(Action<string> log)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(10); // Large file, allow generous timeout
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BMC-LDraw-Import/1.0");
            _log = log;
        }

        /// <summary>
        /// Downloads and extracts the complete LDraw library to the specified directory.
        /// </summary>
        /// <param name="outputDir">Directory to extract the library into.</param>
        /// <param name="forceDownload">If true, re-download even if already extracted.</param>
        /// <returns>Path to the extracted ldraw directory (outputDir/ldraw).</returns>
        public async Task<string> DownloadAndExtractAsync(string outputDir, bool forceDownload = false)
        {
            Directory.CreateDirectory(outputDir);

            // The zip extracts to an "ldraw" subdirectory
            string ldrawDir = Path.Combine(outputDir, "ldraw");
            string markerFile = Path.Combine(outputDir, ".ldraw-download-complete");

            if (!forceDownload && Directory.Exists(ldrawDir) && File.Exists(markerFile))
            {
                _log($"  ✓ LDraw library already downloaded at {ldrawDir}");
                _log($"    Use --force-download to re-download.");
                return ldrawDir;
            }

            string zipPath = Path.Combine(outputDir, "complete.zip");

            // Download
            _log($"  Downloading LDraw complete library from ldraw.org...");
            _log($"  URL: {DownloadUrl}");
            _log($"  This may take a few minutes (~90MB)...");

            try
            {
                using (HttpResponseMessage response = await _httpClient.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    long? totalBytes = response.Content.Headers.ContentLength;
                    if (totalBytes.HasValue)
                    {
                        _log($"  File size: {FormatSize(totalBytes.Value)}");
                    }

                    using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                    using (FileStream fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920))
                    {
                        byte[] buffer = new byte[81920];
                        long totalRead = 0;
                        int bytesRead;
                        int lastReportedPct = -1;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalRead += bytesRead;

                            if (totalBytes.HasValue)
                            {
                                int pct = (int)(100.0 * totalRead / totalBytes.Value);
                                if (pct / 10 > lastReportedPct / 10) // Report every 10%
                                {
                                    _log($"  Downloaded {FormatSize(totalRead)} / {FormatSize(totalBytes.Value)} ({pct}%)");
                                    lastReportedPct = pct;
                                }
                            }
                        }
                    }
                }

                _log($"  Download complete: {FormatSize(new FileInfo(zipPath).Length)}");
            }
            catch (Exception ex)
            {
                _log($"  ✗ Download failed: {ex.Message}");
                throw;
            }

            // Extract
            _log($"  Extracting to {outputDir}...");

            if (Directory.Exists(ldrawDir))
            {
                _log($"  Removing existing ldraw directory...");
                Directory.Delete(ldrawDir, true);
            }

            ZipFile.ExtractToDirectory(zipPath, outputDir);

            // Write marker file
            File.WriteAllText(markerFile, DateTime.UtcNow.ToString("o"));

            // Clean up zip
            File.Delete(zipPath);

            int partCount = Directory.Exists(Path.Combine(ldrawDir, "parts"))
                ? Directory.GetFiles(Path.Combine(ldrawDir, "parts"), "*.dat").Length
                : 0;

            _log($"  Extraction complete: {partCount} part files found");

            return ldrawDir;
        }

        private string FormatSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024.0):F1} MB";
        }
    }
}
