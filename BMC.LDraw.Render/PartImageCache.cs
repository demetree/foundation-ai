using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Renders and caches a 128×128 PNG thumbnail for each unique part × colour
    /// combination encountered in a build plan.
    ///
    /// Thumbnails are rendered once and stored in memory for re-use across
    /// per-step PLI callouts and the Bill of Materials page.
    ///
    /// Thread-safe: parallel pre-rendering is supported via ConcurrentDictionary.
    /// </summary>
    public class PartImageCache
    {
        private readonly ConcurrentDictionary<string, byte[]> _cache
            = new ConcurrentDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Thumbnail size in pixels (width = height).</summary>
        public const int ThumbnailSize = 128;


        /// <summary>
        /// Get a cached thumbnail or render one on demand.
        /// Returns null if the part file cannot be found or rendered.
        /// </summary>
        public byte[] GetOrRender(RenderService renderService, string partFileName, int colourCode)
        {
            string key = MakeKey(partFileName, colourCode);
            return _cache.GetOrAdd(key, _ => RenderPartThumbnail(renderService, partFileName, colourCode));
        }


        /// <summary>
        /// Batch pre-render all unique part × colour combinations from a build plan.
        /// Uses Parallel.ForEach for performance.
        /// Call this before the step rendering loop so all thumbnails are warm.
        /// </summary>
        public void PreRenderAll(
            RenderService renderService,
            ManualBuildPlan plan,
            CancellationToken ct = default)
        {
            // Ensure colour table is loaded once (thread-safe lazy init)
            // by doing one dummy lookup before going parallel
            renderService.EnsureColoursPublic();

            // Collect unique part × colour pairs
            var uniqueParts = new List<(string FileName, int ColourCode)>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var step in plan.Steps)
            {
                if (step.NewParts == null) continue;
                foreach (var part in step.NewParts)
                {
                    string key = MakeKey(part.FileName, part.ColourCode);
                    if (seen.Add(key))
                    {
                        uniqueParts.Add((part.FileName, part.ColourCode));
                    }
                }
            }

            // Render in parallel
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount),
                CancellationToken = ct
            };

            Parallel.ForEach(uniqueParts, parallelOptions, entry =>
            {
                string key = MakeKey(entry.FileName, entry.ColourCode);
                _cache.TryAdd(key, RenderPartThumbnail(renderService, entry.FileName, entry.ColourCode));
            });
        }


        /// <summary>
        /// Retrieve a previously-rendered thumbnail by key.
        /// Returns null if not cached.
        /// </summary>
        public byte[] Get(string partFileName, int colourCode)
        {
            string key = MakeKey(partFileName, colourCode);
            _cache.TryGetValue(key, out byte[] png);
            return png;
        }


        /// <summary>
        /// Return the full cache as a dictionary (key → PNG bytes).
        /// Used when passing part images to the document builder.
        /// </summary>
        public Dictionary<string, byte[]> GetAll()
        {
            return new Dictionary<string, byte[]>(_cache, StringComparer.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Build the cache key from part filename and colour code.
        /// </summary>
        public static string MakeKey(string partFileName, int colourCode)
        {
            return partFileName + "|" + colourCode;
        }


        /// <summary>
        /// Render a single part thumbnail at 128×128.
        /// Uses the RenderService's file-path API to render .dat parts from the library.
        /// Returns null if the part file doesn't exist.
        /// </summary>
        private static byte[] RenderPartThumbnail(
            RenderService renderService, string partFileName, int colourCode)
        {
            try
            {
                string libraryPath = renderService.LibraryPath;
                if (string.IsNullOrEmpty(libraryPath))
                    return null;

                string partPath = Path.Combine(libraryPath, "parts", partFileName);
                if (!File.Exists(partPath))
                {
                    partPath = Path.Combine(libraryPath, "p", partFileName);
                    if (!File.Exists(partPath))
                        return null;
                }

                return renderService.RenderToPng(
                    inputPath: partPath,
                    width: ThumbnailSize,
                    height: ThumbnailSize,
                    colourCode: colourCode,
                    elevation: 30f,
                    azimuth: -45f,
                    renderEdges: true,
                    smoothShading: true);
            }
            catch
            {
                return null;
            }
        }
    }
}
