using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace BMC.Rebrickable.Import
{
    /// <summary>
    /// Downloads Rebrickable CSV data files from their public CDN.
    /// Files are served as .csv.gz and decompressed locally.
    /// </summary>
    public class RebrickableDownloadService
    {
        private const string CdnBaseUrl = "https://cdn.rebrickable.com/media/downloads/";

        private static readonly string[] AllFiles = new[]
        {
            "themes.csv",
            "colors.csv",
            "part_categories.csv",
            "parts.csv",
            "part_relationships.csv",
            "elements.csv",
            "sets.csv",
            "minifigs.csv",
            "inventories.csv",
            "inventory_parts.csv",
            "inventory_sets.csv",
            "inventory_minifigs.csv"
        };

        private readonly HttpClient _httpClient;
        private readonly Action<string> _log;

        public RebrickableDownloadService(Action<string> log)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BMC-Rebrickable-Import/1.0");
            _log = log;
        }

        /// <summary>
        /// Downloads all Rebrickable CSV files to the specified directory.
        /// Files are downloaded as .csv.gz and decompressed to .csv.
        /// </summary>
        /// <param name="outputDir">Directory to save decompressed CSV files.</param>
        /// <param name="forceDownload">If true, re-download even if files already exist.</param>
        /// <param name="targets">Specific file basenames to download (without extension), or null for all.</param>
        /// <returns>Path to the output directory containing CSVs.</returns>
        public async Task<string> DownloadAllAsync(string outputDir, bool forceDownload = false, HashSet<string> targets = null)
        {
            Directory.CreateDirectory(outputDir);

            List<string> filesToDownload = new List<string>();

            foreach (string file in AllFiles)
            {
                if (targets != null && !ShouldDownload(file, targets))
                    continue;

                string localPath = Path.Combine(outputDir, file);

                if (!forceDownload && File.Exists(localPath))
                {
                    FileInfo fi = new FileInfo(localPath);
                    _log($"  ✓ {file} already exists ({FormatSize(fi.Length)}), skipping");
                    continue;
                }

                filesToDownload.Add(file);
            }

            if (filesToDownload.Count == 0)
            {
                _log("  All files already downloaded. Use --force-download to re-download.");
                return outputDir;
            }

            _log($"  Downloading {filesToDownload.Count} file(s) from Rebrickable CDN...");
            _log("");

            int completed = 0;
            foreach (string file in filesToDownload)
            {
                completed++;
                string url = CdnBaseUrl + file + ".gz";
                string localPath = Path.Combine(outputDir, file);

                _log($"  [{completed}/{filesToDownload.Count}] Downloading {file}...");

                try
                {
                    await DownloadAndDecompressAsync(url, localPath);

                    FileInfo fi = new FileInfo(localPath);
                    _log($"           → {FormatSize(fi.Length)} decompressed");
                }
                catch (HttpRequestException ex)
                {
                    _log($"           ✗ Failed: {ex.Message}");
                    throw;
                }
            }

            _log("");
            _log("  Download complete.");
            return outputDir;
        }

        private async Task DownloadAndDecompressAsync(string url, string outputPath)
        {
            using (HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                using (Stream compressedStream = await response.Content.ReadAsStreamAsync())
                using (GZipStream gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                using (FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await gzipStream.CopyToAsync(fileStream);
                }
            }
        }

        /// <summary>
        /// Determines which CSV files are needed based on import targets.
        /// </summary>
        private bool ShouldDownload(string csvFileName, HashSet<string> targets)
        {
            if (targets.Contains("all"))
                return true;

            // Map CSV filenames to import target names
            switch (csvFileName)
            {
                case "themes.csv":
                    return targets.Contains("themes");
                case "colors.csv":
                    return targets.Contains("colours") || targets.Contains("colors");
                case "part_categories.csv":
                    return targets.Contains("categories");
                case "parts.csv":
                    return targets.Contains("parts");
                case "part_relationships.csv":
                    return targets.Contains("relationships");
                case "elements.csv":
                    return targets.Contains("elements");
                case "sets.csv":
                    return targets.Contains("sets");
                case "minifigs.csv":
                    return targets.Contains("minifigs");
                case "inventories.csv":
                case "inventory_parts.csv":
                case "inventory_sets.csv":
                case "inventory_minifigs.csv":
                    return targets.Contains("inventories");
                default:
                    return false;
            }
        }

        private string FormatSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024.0):F1} MB";
        }
    }
}
