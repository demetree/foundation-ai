using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Foundation.BMC.Database;
using BMC.LDraw.Models;
using BMC.LDraw.Parsers;

namespace BMC.LDraw.Import
{
    /// <summary>
    /// Imports LDraw data into the BMC database.
    /// Handles FK resolution for ColourFinish, PartType, and BrickCategory.
    /// Uses upsert logic — safe to re-run.
    /// </summary>
    public class LDrawImportService
    {
        private readonly BMCContext _context;
        private readonly Action<string> _log;

        public LDrawImportService(BMCContext context, Action<string> log)
        {
            _context = context;
            _log = log;
        }

        /// <summary>
        /// Import all colours from LDConfig.ldr into BrickColour table.
        /// </summary>
        public async Task<ImportResult> ImportColoursAsync(string ldConfigPath)
        {
            ImportResult result = new ImportResult();

            _log($"Parsing colours from {ldConfigPath}...");
            List<LDrawColour> parsed = ColourConfigParser.ParseFile(ldConfigPath);
            _log($"  Parsed {parsed.Count} colours");

            // Load FK lookup: ColourFinish name → id
            Dictionary<string, int> finishLookup = await _context.ColourFinishes
                .AsNoTracking()
                .ToDictionaryAsync(cf => cf.name, cf => cf.id);
            _log($"  Loaded {finishLookup.Count} ColourFinish entries");

            // Load existing colours for upsert
            Dictionary<int, BrickColour> existing = await _context.BrickColours
                .ToDictionaryAsync(bc => bc.ldrawColourCode, bc => bc);
            _log($"  Loaded {existing.Count} existing BrickColour rows");

            int sequence = 1;
            foreach (LDrawColour parsed_colour in parsed)
            {
                // Resolve ColourFinish FK
                string finishName = NormalizeFinishName(parsed_colour.FinishType);
                int? colourFinishId = finishLookup.ContainsKey(finishName) ? finishLookup[finishName] : (int?)null;

                if (existing.TryGetValue(parsed_colour.Code, out BrickColour entity))
                {
                    // Update existing
                    entity.name = parsed_colour.Name.Replace('_', ' ');
                    entity.hexRgb = parsed_colour.HexValue;
                    entity.hexEdgeColour = parsed_colour.HexEdge;
                    entity.alpha = parsed_colour.Alpha;
                    entity.isTransparent = parsed_colour.IsTransparent;
                    entity.isMetallic = parsed_colour.IsMetallic;
                    entity.colourFinishId = colourFinishId;
                    entity.luminance = parsed_colour.Luminance;
                    entity.legoColourId = parsed_colour.LegoId;
                    entity.sequence = sequence;
                    result.Updated++;
                }
                else
                {
                    // Create new
                    BrickColour newColour = new BrickColour
                    {
                        name = parsed_colour.Name.Replace('_', ' '),
                        ldrawColourCode = parsed_colour.Code,
                        hexRgb = parsed_colour.HexValue,
                        hexEdgeColour = parsed_colour.HexEdge,
                        alpha = parsed_colour.Alpha,
                        isTransparent = parsed_colour.IsTransparent,
                        isMetallic = parsed_colour.IsMetallic,
                        colourFinishId = colourFinishId,
                        luminance = parsed_colour.Luminance,
                        legoColourId = parsed_colour.LegoId,
                        sequence = sequence,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };
                    _context.BrickColours.Add(newColour);
                    result.Created++;
                }

                sequence++;
            }

            await _context.SaveChangesAsync();
            _log($"  Colours imported: {result.Created} created, {result.Updated} updated");

            return result;
        }

        /// <summary>
        /// Import part headers from all .dat files in the parts directory.
        /// </summary>
        public async Task<ImportResult> ImportPartHeadersAsync(string partsDirectory)
        {
            ImportResult result = new ImportResult();

            string[] datFiles = Directory.GetFiles(partsDirectory, "*.dat", SearchOption.TopDirectoryOnly);
            _log($"Found {datFiles.Length} .dat files in {partsDirectory}");

            // Load FK lookups
            Dictionary<string, int> partTypeLookup = await _context.PartTypes
                .AsNoTracking()
                .ToDictionaryAsync(pt => pt.name, pt => pt.id);
            _log($"  Loaded {partTypeLookup.Count} PartType entries");

            Dictionary<string, int> categoryLookup = await _context.BrickCategories
                .AsNoTracking()
                .ToDictionaryAsync(bc => bc.name, bc => bc.id);
            _log($"  Loaded {categoryLookup.Count} BrickCategory entries");

            // Load existing parts for upsert
            Dictionary<string, BrickPart> existing = await _context.BrickParts
                .ToDictionaryAsync(bp => bp.ldrawPartId, bp => bp);
            _log($"  Loaded {existing.Count} existing BrickPart rows");

            int batchCount = 0;
            int totalProcessed = 0;

            foreach (string datFile in datFiles)
            {
                string partId = Path.GetFileNameWithoutExtension(datFile);
                LDrawPartHeader header;

                try
                {
                    header = PartHeaderParser.ParseFile(datFile);
                }
                catch (Exception ex)
                {
                    _log($"  WARN: Skipping {partId}: {ex.Message}");
                    result.Skipped++;
                    continue;
                }

                if (string.IsNullOrEmpty(header.Title))
                {
                    result.Skipped++;
                    continue;
                }

                // Resolve PartType FK
                int? partTypeId = null;
                if (!string.IsNullOrEmpty(header.PartType) && partTypeLookup.ContainsKey(header.PartType))
                {
                    partTypeId = partTypeLookup[header.PartType];
                }

                // Resolve BrickCategory FK — try to match the LDraw category to our categories
                int? brickCategoryId = null;
                if (!string.IsNullOrEmpty(header.Category) && categoryLookup.ContainsKey(header.Category))
                {
                    brickCategoryId = categoryLookup[header.Category];
                }

                // Aggregate keywords
                string keywords = header.Keywords.Count > 0 ? string.Join(", ", header.Keywords) : null;

                if (existing.TryGetValue(partId, out BrickPart entity))
                {
                    // Update existing
                    entity.name = partId;
                    entity.ldrawTitle = header.Title;
                    entity.ldrawCategory = header.Category;
                    entity.partTypeId = partTypeId;
                    entity.keywords = keywords;
                    entity.author = header.Author;
                    entity.brickCategoryId = brickCategoryId;
                    entity.geometryFilePath = "parts/" + Path.GetFileName(datFile);
                    result.Updated++;
                }
                else
                {
                    // Create new
                    BrickPart newPart = new BrickPart
                    {
                        name = partId,
                        ldrawPartId = partId,
                        ldrawTitle = header.Title,
                        ldrawCategory = header.Category,
                        partTypeId = partTypeId,
                        keywords = keywords,
                        author = header.Author,
                        brickCategoryId = brickCategoryId,
                        widthLdu = 0,
                        heightLdu = 0,
                        depthLdu = 0,
                        massGrams = 0,
                        geometryFilePath = "parts/" + Path.GetFileName(datFile),
                        versionNumber = 0,          // Using 0 here to indicate that there is no change history because this is a batch data load. We can implement versioning in the future if needed.
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };
                    _context.BrickParts.Add(newPart);
                    existing[partId] = newPart; // prevent duplicates within same batch
                    result.Created++;
                }

                batchCount++;
                totalProcessed++;

                // Save in batches of 500
                if (batchCount >= 500)
                {
                    await _context.SaveChangesAsync();
                    _log($"  Processed {totalProcessed}/{datFiles.Length} parts...");
                    batchCount = 0;
                }
            }

            // Save remaining
            if (batchCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            _log($"  Parts imported: {result.Created} created, {result.Updated} updated, {result.Skipped} skipped");

            return result;
        }

        /// <summary>
        /// Copy LDraw data files to the server-accessible data directory.
        /// Copies LDConfig.ldr and all parts/*.dat files.
        /// </summary>
        public void CopyDataFiles(string sourcePath, string destPath)
        {
            _log($"Copying LDraw data files from {sourcePath} to {destPath}...");

            // Ensure destination exists
            Directory.CreateDirectory(destPath);

            // Copy LDConfig.ldr
            string sourceConfig = Path.Combine(sourcePath, "LDConfig.ldr");
            if (File.Exists(sourceConfig))
            {
                File.Copy(sourceConfig, Path.Combine(destPath, "LDConfig.ldr"), true);
                _log("  Copied LDConfig.ldr");
            }

            // Copy parts directory (recursively — includes parts/s/ sub-parts, etc.)
            string sourcePartsDir = Path.Combine(sourcePath, "parts");
            string destPartsDir = Path.Combine(destPath, "parts");
            if (Directory.Exists(sourcePartsDir))
            {
                CopyDirectoryRecursive(sourcePartsDir, destPartsDir);
                int copied = Directory.GetFiles(destPartsDir, "*.*", SearchOption.AllDirectories).Length;
                _log($"  Copied {copied} part files to {destPartsDir} (including sub-parts)");
            }

            // Copy p (primitives) directory — needed for geometry resolution
            string sourcePrimDir = Path.Combine(sourcePath, "p");
            string destPrimDir = Path.Combine(destPath, "p");
            if (Directory.Exists(sourcePrimDir))
            {
                CopyDirectoryRecursive(sourcePrimDir, destPrimDir);
                _log($"  Copied primitives directory");
            }

            _log("  Data file copy complete.");
        }

        /// <summary>
        /// Normalize parsed finish type names to match ColourFinish seed data names.
        /// Parser outputs uppercase (FABRIC) or title-case — we need to match DB seed names.
        /// </summary>
        private static string NormalizeFinishName(string finishType)
        {
            if (string.IsNullOrEmpty(finishType)) return "Solid";

            switch (finishType.ToUpperInvariant())
            {
                case "SOLID": return "Solid";
                case "TRANSPARENT": return "Transparent";
                case "CHROME": return "Chrome";
                case "PEARLESCENT": return "Pearlescent";
                case "METAL": return "Metal";
                case "RUBBER": return "Rubber";
                case "GLITTER": return "Glitter";
                case "SPECKLE": return "Speckle";
                case "MILKY": return "Milky";
                case "FABRIC": return "Fabric";
                default: return finishType; // Unknown — pass through
            }
        }

        private static void CopyDirectoryRecursive(string source, string dest)
        {
            Directory.CreateDirectory(dest);

            foreach (string file in Directory.GetFiles(source))
            {
                File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), true);
            }

            foreach (string dir in Directory.GetDirectories(source))
            {
                CopyDirectoryRecursive(dir, Path.Combine(dest, Path.GetFileName(dir)));
            }
        }
    }

    /// <summary>
    /// Simple result counters for an import operation.
    /// </summary>
    public class ImportResult
    {
        public int Created;
        public int Updated;
        public int Skipped;

        public override string ToString()
        {
            return $"Created: {Created}, Updated: {Updated}, Skipped: {Skipped}";
        }
    }
}
