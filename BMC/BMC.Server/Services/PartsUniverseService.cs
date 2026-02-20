using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Foundation.BMC.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Foundation.BMC.Services
{
    /// <summary>
    ///
    /// Background service that precomputes the Parts Universe visualization data
    /// at server startup and holds it in memory for instant API serving.
    ///
    /// The computed payload is also persisted to a JSON file on disk so that
    /// subsequent startups can load from cache without hitting the database.
    ///
    /// </summary>
    public class PartsUniverseService : BackgroundService
    {
        private const string DATA_DIRECTORY = "data";
        private const string CACHE_FILENAME = "parts-universe-cache.json";
        private const int CACHE_MAX_AGE_HOURS = 24;
        private const int TOP_RANKED_PARTS = 200;
        private const int SANKEY_TOP_PARTS = 12;
        private const int SANKEY_TOP_THEMES = 10;
        private const int HEATMAP_TOP_PARTS = 25;
        private const int HEATMAP_TOP_COLOURS = 30;
        private const int CHORD_TOP_CATEGORIES = 8;
        private const int CHORD_TOP_THEMES = 8;
        private const int BUBBLE_TOP_PARTS_PER_CATEGORY = 50;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PartsUniverseService> _logger;

        private PartsUniversePayload _cachedPayload;
        private readonly object _payloadLock = new object();


        public PartsUniverseService(
            IServiceScopeFactory scopeFactory,
            ILogger<PartsUniverseService> logger
        )
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }


        /// <summary>
        ///
        /// Returns the cached payload, or null if computation has not yet completed.
        ///
        /// </summary>
        public PartsUniversePayload GetCachedPayload()
        {
            lock (_payloadLock)
            {
                return _cachedPayload;
            }
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //
            // Delay briefly to let the rest of the server finish starting
            //
            await Task.Delay(2000, stoppingToken).ConfigureAwait(false);

            _logger.LogInformation("[PartsUniverseService] Starting data computation...");

            try
            {
                //
                // Try to load from disk cache first
                //
                PartsUniversePayload diskPayload = await TryLoadFromDiskAsync().ConfigureAwait(false);

                if (diskPayload != null)
                {
                    lock (_payloadLock)
                    {
                        _cachedPayload = diskPayload;
                    }

                    _logger.LogInformation(
                        "[PartsUniverseService] Loaded from disk cache — {PartCount} parts ranked, computed at {ComputedAt}",
                        diskPayload.RankedParts.Count,
                        diskPayload.ComputedAtUtc
                    );

                    return;
                }

                //
                // No valid cache — compute from database
                //
                await ComputeAndCacheAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("[PartsUniverseService] Computation cancelled during shutdown.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PartsUniverseService] Failed to compute Parts Universe data.");
            }
        }


        /// <summary>
        ///
        /// Forces a re-computation from the database and updates the in-memory cache.
        ///
        /// </summary>
        public async Task RefreshAsync()
        {
            _logger.LogInformation("[PartsUniverseService] Manual refresh triggered.");

            await ComputeAndCacheAsync(CancellationToken.None).ConfigureAwait(false);
        }


        private async Task ComputeAndCacheAsync(CancellationToken ct)
        {
            Stopwatch sw = Stopwatch.StartNew();

            using IServiceScope scope = _scopeFactory.CreateScope();

            BMCContext context = scope.ServiceProvider.GetRequiredService<BMCContext>();

            context.Database.SetCommandTimeout(120);

            //
            // Load all active, non-deleted set parts with their relations
            //
            List<LegoSetPart> allSetParts = await context.LegoSetParts
                .AsNoTracking()
                .Where(sp => sp.active && !sp.deleted)
                .Include(sp => sp.brickPart)
                    .ThenInclude(bp => bp.brickCategory)
                .Include(sp => sp.brickPart)
                    .ThenInclude(bp => bp.partType)
                .Include(sp => sp.brickColour)
                .Include(sp => sp.legoSet)
                    .ThenInclude(ls => ls.legoTheme)
                .AsSplitQuery()
                .ToListAsync(ct)
                .ConfigureAwait(false);

            _logger.LogInformation(
                "[PartsUniverseService] Loaded {Count} set-part rows from database in {Elapsed}ms",
                allSetParts.Count,
                sw.ElapsedMilliseconds
            );

            //
            // Aggregate by brickPartId
            //
            Dictionary<int, PartAggregation> partMap = new Dictionary<int, PartAggregation>();

            foreach (LegoSetPart sp in allSetParts)
            {
                if (sp.brickPart == null)
                {
                    continue;
                }

                int partId = sp.brickPartId;
                int qty = sp.quantity ?? 1;

                if (partMap.TryGetValue(partId, out PartAggregation agg) == false)
                {
                    agg = new PartAggregation
                    {
                        Part = sp.brickPart,
                        TotalQty = 0,
                        SetIds = new HashSet<int>(),
                        ColourMap = new Dictionary<int, ColourAggregation>(),
                        ThemeMap = new Dictionary<string, int>()
                    };

                    partMap[partId] = agg;
                }

                agg.TotalQty += qty;
                agg.SetIds.Add(sp.legoSetId);

                //
                // Colour distribution
                //
                if (sp.brickColour != null)
                {
                    int colourId = sp.brickColourId;

                    if (agg.ColourMap.TryGetValue(colourId, out ColourAggregation colourAgg) == false)
                    {
                        colourAgg = new ColourAggregation
                        {
                            Name = sp.brickColour.name ?? "Unknown",
                            Hex = sp.brickColour.hexRgb ?? "888888",
                            Qty = 0
                        };

                        agg.ColourMap[colourId] = colourAgg;
                    }

                    colourAgg.Qty += qty;
                }

                //
                // Theme distribution
                //
                string themeName = sp.legoSet?.legoTheme?.name ?? "Unknown";

                if (agg.ThemeMap.ContainsKey(themeName))
                {
                    agg.ThemeMap[themeName] += qty;
                }
                else
                {
                    agg.ThemeMap[themeName] = qty;
                }
            }

            //
            // Build ranked parts list, sorted by total quantity descending
            //
            List<RankedPartDto> rankedParts = partMap.Values
                .OrderByDescending(a => a.TotalQty)
                .Take(TOP_RANKED_PARTS)
                .Select(a => new RankedPartDto
                {
                    BrickPartId = a.Part.id,
                    Name = a.Part.name ?? a.Part.ldrawTitle ?? "Unknown",
                    LdrawPartId = a.Part.ldrawPartId ?? "",
                    LdrawTitle = a.Part.ldrawTitle ?? "",
                    GeometryFilePath = a.Part.geometryFilePath ?? "",
                    CategoryName = a.Part.brickCategory?.name ?? a.Part.ldrawCategory ?? "Other",
                    PartTypeName = a.Part.partType?.name ?? "Unknown",
                    TotalQty = a.TotalQty,
                    SetCount = a.SetIds.Count,
                    Colours = a.ColourMap.Values
                        .OrderByDescending(c => c.Qty)
                        .Select(c => new ColourEntryDto { Name = c.Name, Hex = c.Hex, Qty = c.Qty })
                        .ToList(),
                    Themes = a.ThemeMap
                        .OrderByDescending(kv => kv.Value)
                        .Select(kv => new ThemeEntryDto { Name = kv.Key, Qty = kv.Value })
                        .ToList()
                })
                .ToList();


            //
            // Build Sankey data (Category → Top Parts → Themes)
            //
            SankeyDataDto sankeyData = BuildSankeyData(rankedParts);

            //
            // Build Bubble chart data (Categories → Parts hierarchy)
            //
            List<CategoryBubbleDto> bubbleData = BuildBubbleData(rankedParts);

            //
            // Build Heatmap data (Top parts × Top colours)
            //
            HeatmapDataDto heatmapData = BuildHeatmapData(rankedParts);

            //
            // Build Chord data (Category ↔ Theme matrix)
            //
            ChordDataDto chordData = BuildChordData(rankedParts);


            //
            // Assemble the full payload
            //
            PartsUniversePayload payload = new PartsUniversePayload
            {
                RankedParts = rankedParts,
                Sankey = sankeyData,
                Bubbles = bubbleData,
                Heatmap = heatmapData,
                Chord = chordData,
                Stats = new SummaryStatsDto
                {
                    TotalUniqueParts = partMap.Count,
                    TotalInstances = partMap.Values.Sum(a => a.TotalQty),
                    TotalSets = allSetParts.Select(sp => sp.legoSetId).Distinct().Count(),
                    TotalCategories = rankedParts.Select(rp => rp.CategoryName).Distinct().Count()
                },
                ComputedAtUtc = DateTime.UtcNow
            };

            sw.Stop();

            lock (_payloadLock)
            {
                _cachedPayload = payload;
            }

            _logger.LogInformation(
                "[PartsUniverseService] Computation complete — {PartCount} parts ranked in {Elapsed}ms",
                rankedParts.Count,
                sw.ElapsedMilliseconds
            );

            //
            // Persist to disk
            //
            await PersistToDiskAsync(payload).ConfigureAwait(false);
        }


        //
        // ── Sankey builder ─────────────────────────────────────────────
        //

        private SankeyDataDto BuildSankeyData(List<RankedPartDto> rankedParts)
        {
            List<RankedPartDto> topParts = rankedParts.Take(SANKEY_TOP_PARTS).ToList();

            //
            // Collect unique categories from top parts
            //
            List<string> categories = topParts
                .Select(p => p.CategoryName)
                .Distinct()
                .ToList();

            //
            // Collect top themes across all top parts
            //
            Dictionary<string, int> themeQtyAll = new Dictionary<string, int>();

            foreach (RankedPartDto part in topParts)
            {
                foreach (ThemeEntryDto t in part.Themes)
                {
                    if (themeQtyAll.ContainsKey(t.Name))
                    {
                        themeQtyAll[t.Name] += t.Qty;
                    }
                    else
                    {
                        themeQtyAll[t.Name] = t.Qty;
                    }
                }
            }

            List<string> topThemes = themeQtyAll
                .OrderByDescending(kv => kv.Value)
                .Take(SANKEY_TOP_THEMES)
                .Select(kv => kv.Key)
                .ToList();

            //
            // Build node list: categories (left), parts (middle), themes (right)
            //
            List<SankeyNodeDto> nodes = new List<SankeyNodeDto>();

            foreach (string cat in categories)
            {
                nodes.Add(new SankeyNodeDto { Name = cat, Group = "category" });
            }

            foreach (RankedPartDto part in topParts)
            {
                nodes.Add(new SankeyNodeDto { Name = part.Name, Group = "part" });
            }

            foreach (string theme in topThemes)
            {
                nodes.Add(new SankeyNodeDto { Name = theme, Group = "theme" });
            }

            //
            // Build links
            //
            List<SankeyLinkDto> links = new List<SankeyLinkDto>();

            // Category → Part links
            foreach (RankedPartDto part in topParts)
            {
                int sourceIdx = categories.IndexOf(part.CategoryName);
                int targetIdx = categories.Count + topParts.IndexOf(part);

                if (sourceIdx >= 0)
                {
                    links.Add(new SankeyLinkDto
                    {
                        Source = sourceIdx,
                        Target = targetIdx,
                        Value = part.TotalQty
                    });
                }
            }

            // Part → Theme links
            foreach (RankedPartDto part in topParts)
            {
                int partIdx = categories.Count + topParts.IndexOf(part);

                foreach (ThemeEntryDto t in part.Themes)
                {
                    int themeIdx = topThemes.IndexOf(t.Name);

                    if (themeIdx >= 0)
                    {
                        links.Add(new SankeyLinkDto
                        {
                            Source = partIdx,
                            Target = categories.Count + topParts.Count + themeIdx,
                            Value = t.Qty
                        });
                    }
                }
            }

            return new SankeyDataDto
            {
                Nodes = nodes,
                Links = links
            };
        }


        //
        // ── Bubble chart builder ───────────────────────────────────────
        //

        private List<CategoryBubbleDto> BuildBubbleData(List<RankedPartDto> rankedParts)
        {
            Dictionary<string, List<RankedPartDto>> categoryGroups = new Dictionary<string, List<RankedPartDto>>();

            foreach (RankedPartDto part in rankedParts)
            {
                if (categoryGroups.TryGetValue(part.CategoryName, out List<RankedPartDto> group) == false)
                {
                    group = new List<RankedPartDto>();
                    categoryGroups[part.CategoryName] = group;
                }

                if (group.Count < BUBBLE_TOP_PARTS_PER_CATEGORY)
                {
                    group.Add(part);
                }
            }

            return categoryGroups
                .Select(kv => new CategoryBubbleDto
                {
                    CategoryName = kv.Key,
                    Parts = kv.Value.Select(p => new BubblePartDto
                    {
                        Name = p.Name,
                        TotalQty = p.TotalQty,
                        SetCount = p.SetCount,
                        DominantColourHex = p.Colours.Count > 0 ? p.Colours[0].Hex : "888888"
                    }).ToList()
                })
                .OrderByDescending(c => c.Parts.Sum(p => p.TotalQty))
                .ToList();
        }


        //
        // ── Heatmap builder ────────────────────────────────────────────
        //

        private HeatmapDataDto BuildHeatmapData(List<RankedPartDto> rankedParts)
        {
            List<RankedPartDto> topParts = rankedParts.Take(HEATMAP_TOP_PARTS).ToList();

            //
            // Collect all colours across top parts and pick top N
            //
            Dictionary<string, int> colourQtyAll = new Dictionary<string, int>();

            foreach (RankedPartDto part in topParts)
            {
                foreach (ColourEntryDto c in part.Colours)
                {
                    string key = c.Hex + "|" + c.Name;

                    if (colourQtyAll.ContainsKey(key))
                    {
                        colourQtyAll[key] += c.Qty;
                    }
                    else
                    {
                        colourQtyAll[key] = c.Qty;
                    }
                }
            }

            List<string> topColourKeys = colourQtyAll
                .OrderByDescending(kv => kv.Value)
                .Take(HEATMAP_TOP_COLOURS)
                .Select(kv => kv.Key)
                .ToList();

            //
            // Build parallel arrays for part labels, colour labels, and cell data
            //
            List<string> partLabels = topParts.Select(p => p.Name).ToList();

            List<HeatmapColourLabelDto> colourLabels = topColourKeys.Select(key =>
            {
                string[] parts = key.Split('|');
                return new HeatmapColourLabelDto { Hex = parts[0], Name = parts.Length > 1 ? parts[1] : "" };
            }).ToList();

            List<HeatmapCellDto> cells = new List<HeatmapCellDto>();

            for (int pi = 0; pi < topParts.Count; pi++)
            {
                RankedPartDto part = topParts[pi];

                for (int ci = 0; ci < topColourKeys.Count; ci++)
                {
                    string colourKey = topColourKeys[ci];
                    string hex = colourKey.Split('|')[0];

                    ColourEntryDto match = part.Colours.FirstOrDefault(c => c.Hex == hex);

                    int qty = match != null ? match.Qty : 0;

                    if (qty > 0)
                    {
                        cells.Add(new HeatmapCellDto
                        {
                            PartIdx = pi,
                            ColourIdx = ci,
                            Hex = hex,
                            Qty = qty
                        });
                    }
                }
            }

            return new HeatmapDataDto
            {
                PartLabels = partLabels,
                ColourLabels = colourLabels,
                Cells = cells
            };
        }


        //
        // ── Chord diagram builder ──────────────────────────────────────
        //

        private ChordDataDto BuildChordData(List<RankedPartDto> rankedParts)
        {
            //
            // Aggregate quantities by category → theme
            //
            Dictionary<string, Dictionary<string, int>> catThemeMap = new Dictionary<string, Dictionary<string, int>>();

            foreach (RankedPartDto part in rankedParts)
            {
                if (catThemeMap.TryGetValue(part.CategoryName, out Dictionary<string, int> themeMap) == false)
                {
                    themeMap = new Dictionary<string, int>();
                    catThemeMap[part.CategoryName] = themeMap;
                }

                foreach (ThemeEntryDto t in part.Themes)
                {
                    if (themeMap.ContainsKey(t.Name))
                    {
                        themeMap[t.Name] += t.Qty;
                    }
                    else
                    {
                        themeMap[t.Name] = t.Qty;
                    }
                }
            }

            //
            // Pick top categories and top themes by total qty
            //
            List<string> topCats = catThemeMap
                .OrderByDescending(kv => kv.Value.Values.Sum())
                .Take(CHORD_TOP_CATEGORIES)
                .Select(kv => kv.Key)
                .ToList();

            Dictionary<string, int> globalThemeQty = new Dictionary<string, int>();

            foreach (RankedPartDto part in rankedParts)
            {
                foreach (ThemeEntryDto t in part.Themes)
                {
                    if (globalThemeQty.ContainsKey(t.Name))
                    {
                        globalThemeQty[t.Name] += t.Qty;
                    }
                    else
                    {
                        globalThemeQty[t.Name] = t.Qty;
                    }
                }
            }

            List<string> topThemes = globalThemeQty
                .OrderByDescending(kv => kv.Value)
                .Take(CHORD_TOP_THEMES)
                .Select(kv => kv.Key)
                .ToList();

            //
            // Build the symmetric matrix
            //
            List<string> names = new List<string>();
            names.AddRange(topCats);
            names.AddRange(topThemes);

            int n = names.Count;
            List<List<int>> matrix = new List<List<int>>();

            for (int i = 0; i < n; i++)
            {
                List<int> row = new List<int>();

                for (int j = 0; j < n; j++)
                {
                    row.Add(0);
                }

                matrix.Add(row);
            }

            for (int ci = 0; ci < topCats.Count; ci++)
            {
                if (catThemeMap.TryGetValue(topCats[ci], out Dictionary<string, int> themeMap) == false)
                {
                    continue;
                }

                for (int ti = 0; ti < topThemes.Count; ti++)
                {
                    int val = 0;

                    if (themeMap.TryGetValue(topThemes[ti], out int v))
                    {
                        val = v;
                    }

                    matrix[ci][topCats.Count + ti] = val;
                    matrix[topCats.Count + ti][ci] = val;
                }
            }

            return new ChordDataDto
            {
                Names = names,
                Matrix = matrix,
                CategoryCount = topCats.Count
            };
        }


        //
        // ── Disk persistence ───────────────────────────────────────────
        //

        private async Task PersistToDiskAsync(PartsUniversePayload payload)
        {
            try
            {
                string dir = GetDataDirectory();

                if (Directory.Exists(dir) == false)
                {
                    Directory.CreateDirectory(dir);
                }

                string filePath = Path.Combine(dir, CACHE_FILENAME);

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string json = JsonSerializer.Serialize(payload, options);

                await File.WriteAllTextAsync(filePath, json).ConfigureAwait(false);

                _logger.LogInformation(
                    "[PartsUniverseService] Payload persisted to disk ({Size} bytes)",
                    json.Length
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[PartsUniverseService] Failed to persist payload to disk (non-fatal).");
            }
        }


        private async Task<PartsUniversePayload> TryLoadFromDiskAsync()
        {
            try
            {
                string filePath = Path.Combine(GetDataDirectory(), CACHE_FILENAME);

                if (File.Exists(filePath) == false)
                {
                    return null;
                }

                //
                // Check age
                //
                FileInfo info = new FileInfo(filePath);

                if (info.LastWriteTimeUtc < DateTime.UtcNow.AddHours(-CACHE_MAX_AGE_HOURS))
                {
                    _logger.LogInformation("[PartsUniverseService] Disk cache is older than {Hours}h — recomputing.", CACHE_MAX_AGE_HOURS);
                    return null;
                }

                string json = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                PartsUniversePayload payload = JsonSerializer.Deserialize<PartsUniversePayload>(json, options);

                return payload;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[PartsUniverseService] Failed to load from disk cache (non-fatal).");
                return null;
            }
        }


        private string GetDataDirectory()
        {
            string basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) ?? "";

            return Path.Combine(basePath, DATA_DIRECTORY);
        }


        //
        // ── Internal aggregation helpers ───────────────────────────────
        //

        private class PartAggregation
        {
            public BrickPart Part;
            public int TotalQty;
            public HashSet<int> SetIds;
            public Dictionary<int, ColourAggregation> ColourMap;
            public Dictionary<string, int> ThemeMap;
        }

        private class ColourAggregation
        {
            public string Name;
            public string Hex;
            public int Qty;
        }
    }


    //
    // ── DTOs ───────────────────────────────────────────────────────────
    //

    public class PartsUniversePayload
    {
        public List<RankedPartDto> RankedParts { get; set; }
        public SankeyDataDto Sankey { get; set; }
        public List<CategoryBubbleDto> Bubbles { get; set; }
        public HeatmapDataDto Heatmap { get; set; }
        public ChordDataDto Chord { get; set; }
        public SummaryStatsDto Stats { get; set; }
        public DateTime ComputedAtUtc { get; set; }
    }


    public class RankedPartDto
    {
        public int BrickPartId { get; set; }
        public string Name { get; set; }
        public string LdrawPartId { get; set; }
        public string LdrawTitle { get; set; }
        public string GeometryFilePath { get; set; }
        public string CategoryName { get; set; }
        public string PartTypeName { get; set; }
        public int TotalQty { get; set; }
        public int SetCount { get; set; }
        public List<ColourEntryDto> Colours { get; set; }
        public List<ThemeEntryDto> Themes { get; set; }
    }


    public class ColourEntryDto
    {
        public string Name { get; set; }
        public string Hex { get; set; }
        public int Qty { get; set; }
    }


    public class ThemeEntryDto
    {
        public string Name { get; set; }
        public int Qty { get; set; }
    }


    public class SummaryStatsDto
    {
        public int TotalUniqueParts { get; set; }
        public int TotalInstances { get; set; }
        public int TotalSets { get; set; }
        public int TotalCategories { get; set; }
    }


    // ── Sankey ──

    public class SankeyDataDto
    {
        public List<SankeyNodeDto> Nodes { get; set; }
        public List<SankeyLinkDto> Links { get; set; }
    }

    public class SankeyNodeDto
    {
        public string Name { get; set; }
        public string Group { get; set; }
    }

    public class SankeyLinkDto
    {
        public int Source { get; set; }
        public int Target { get; set; }
        public int Value { get; set; }
    }


    // ── Bubble ──

    public class CategoryBubbleDto
    {
        public string CategoryName { get; set; }
        public List<BubblePartDto> Parts { get; set; }
    }

    public class BubblePartDto
    {
        public string Name { get; set; }
        public int TotalQty { get; set; }
        public int SetCount { get; set; }
        public string DominantColourHex { get; set; }
    }


    // ── Heatmap ──

    public class HeatmapDataDto
    {
        public List<string> PartLabels { get; set; }
        public List<HeatmapColourLabelDto> ColourLabels { get; set; }
        public List<HeatmapCellDto> Cells { get; set; }
    }

    public class HeatmapColourLabelDto
    {
        public string Hex { get; set; }
        public string Name { get; set; }
    }

    public class HeatmapCellDto
    {
        public int PartIdx { get; set; }
        public int ColourIdx { get; set; }
        public string Hex { get; set; }
        public int Qty { get; set; }
    }


    // ── Chord ──

    public class ChordDataDto
    {
        public List<string> Names { get; set; }
        public List<List<int>> Matrix { get; set; }
        public int CategoryCount { get; set; }
    }
}
