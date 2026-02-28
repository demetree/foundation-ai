using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;
using Foundation.BMC.Database;

namespace BMC.Rebrickable.Import
{
    /// <summary>
    /// Imports Rebrickable CSV data into the BMC database.
    /// Uses BulkInsertOrUpdate for high-performance upserts.
    /// Safe to re-run — idempotent.
    /// </summary>
    public class RebrickableImportService
    {
        private readonly BMCContext _context;
        private readonly Action<string> _log;

        public RebrickableImportService(BMCContext context, Action<string> log)
        {
            _context = context;
            _log = log;
        }

        // ─────────────────────────────────────────────────
        // 1. THEMES
        // ─────────────────────────────────────────────────

        /// <summary>
        /// Import themes.csv → LegoTheme.
        /// Two-pass: first upsert all themes, then resolve parent FKs.
        /// </summary>
        public async Task<ImportResult> ImportThemesAsync(string themesPath)
        {
            ImportResult result = new ImportResult();
            _log($"  Parsing {themesPath}...");

            // Parse CSV: id,name,parent_id
            List<string[]> rows = ParseCsv(themesPath);
            _log($"  Parsed {rows.Count} themes");

            // Load existing themes by rebrickableThemeId
            Dictionary<int, LegoTheme> existing = await _context.LegoThemes
                .ToDictionaryAsync(t => t.rebrickableThemeId, t => t);

            // Build parent lookup and detect duplicate names
            Dictionary<int, string> idToName = new Dictionary<int, string>();
            Dictionary<int, int> idToParent = new Dictionary<int, int>();
            Dictionary<string, int> nameCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (string[] row in rows)
            {
                int rid = int.Parse(row[0]);
                string n = row[1];
                idToName[rid] = n;
                if (row.Length > 2 && !string.IsNullOrEmpty(row[2]) && int.TryParse(row[2], out int pid))
                    idToParent[rid] = pid;

                nameCounts.TryGetValue(n, out int c);
                nameCounts[n] = c + 1;
            }

            // Also load existing theme names from DB to avoid collisions with pre-existing data
            HashSet<string> existingNames = new HashSet<string>(
                await _context.LegoThemes
                    .Select(t => t.name)
                    .ToListAsync(),
                StringComparer.OrdinalIgnoreCase);

            // Pass 1: Upsert all themes (without parent FK)
            Dictionary<int, LegoTheme> allThemes = new Dictionary<int, LegoTheme>();
            HashSet<string> usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int sequence = 1;
            foreach (string[] row in rows)
            {
                int rebrickableId = int.Parse(row[0]);
                string name = row[1];

                // Disambiguate duplicate names by appending parent theme name
                if (nameCounts[name] > 1 || existingNames.Contains(name))
                {
                    if (idToParent.TryGetValue(rebrickableId, out int parentId) && idToName.TryGetValue(parentId, out string parentName))
                    {
                        name = $"{name} ({parentName})";
                    }
                }

                // Final dedup: if still collides, append rebrickable ID
                if (usedNames.Contains(name))
                {
                    name = $"{name} [{rebrickableId}]";
                }
                usedNames.Add(name);

                if (existing.TryGetValue(rebrickableId, out LegoTheme entity))
                {
                    entity.name = name;
                    entity.sequence = sequence;
                    result.Updated++;
                }
                else
                {
                    entity = new LegoTheme
                    {
                        name = name,
                        description = name,
                        rebrickableThemeId = rebrickableId,
                        sequence = sequence,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };
                    _context.LegoThemes.Add(entity);
                    result.Created++;
                }

                allThemes[rebrickableId] = entity;
                sequence++;
            }

            await _context.SaveChangesAsync();
            _log($"  Pass 1 complete: {result.Created} created, {result.Updated} updated");

            // Pass 2: Set parent FKs
            int parentsSet = 0;
            foreach (string[] row in rows)
            {
                int rebrickableId = int.Parse(row[0]);
                string parentIdStr = row.Length > 2 ? row[2] : null;

                if (!string.IsNullOrEmpty(parentIdStr) && int.TryParse(parentIdStr, out int parentRebrickableId))
                {
                    if (allThemes.TryGetValue(rebrickableId, out LegoTheme child) &&
                        allThemes.TryGetValue(parentRebrickableId, out LegoTheme parent))
                    {
                        child.legoThemeId = parent.id;
                        parentsSet++;
                    }
                }
            }

            await _context.SaveChangesAsync();
            _log($"  Pass 2 complete: {parentsSet} parent relationships set");

            return result;
        }

        // ─────────────────────────────────────────────────
        // 2. COLOURS (additive only)
        // ─────────────────────────────────────────────────

        /// <summary>
        /// Import colors.csv → BrickColour (additive only — inserts colours not already present).
        /// Does NOT overwrite existing LDraw colour data.
        /// </summary>
        public async Task<ImportResult> ImportColoursAsync(string coloursPath)
        {
            ImportResult result = new ImportResult();
            _log($"  Parsing {coloursPath}...");

            // Parse CSV: id,name,rgb,is_trans
            List<string[]> rows = ParseCsv(coloursPath);
            _log($"  Parsed {rows.Count} colours");

            // Load existing colours by ldrawColourCode (Rebrickable color_id = LDraw colour code)
            HashSet<int> existingCodes = new HashSet<int>(
                await _context.BrickColours
                    .Where(c => c.ldrawColourCode.HasValue)
                    .Select(c => c.ldrawColourCode.Value)
                    .ToListAsync());

            // Resolve ColourFinish FK for "Solid" as default
            int solidFinishId = await _context.ColourFinishes
                .Where(cf => cf.name == "Solid")
                .Select(cf => cf.id)
                .FirstOrDefaultAsync();
            if (solidFinishId == 0) solidFinishId = 1;

            int transparentFinishId = await _context.ColourFinishes
                .Where(cf => cf.name == "Transparent")
                .Select(cf => cf.id)
                .FirstOrDefaultAsync();
            if (transparentFinishId == 0) transparentFinishId = 2;

            // Also track existing names to avoid unique constraint violations
            HashSet<string> existingNames = new HashSet<string>(
                await _context.BrickColours.Select(c => c.name).ToListAsync(),
                StringComparer.OrdinalIgnoreCase);

            int sequence = 1000; // Start after LDraw seeded colours
            foreach (string[] row in rows)
            {
                int colourCode = int.Parse(row[0]);

                if (existingCodes.Contains(colourCode))
                {
                    result.Skipped++;
                    continue;
                }

                string name = row[1];

                // Disambiguate if name already exists (e.g. LDraw and Rebrickable both define "Glow in Dark White")
                if (existingNames.Contains(name))
                {
                    name = $"{name} (Rebrickable {colourCode})";
                }
                existingNames.Add(name);

                string hexRgb = "#" + row[2]; // Rebrickable provides raw hex without #
                bool isTrans = row.Length > 3 && row[3].Equals("t", StringComparison.OrdinalIgnoreCase);

                BrickColour newColour = new BrickColour
                {
                    name = name,
                    rebrickableColorId = colourCode,
                    ldrawColourCode = colourCode,
                    hexRgb = hexRgb,
                    hexEdgeColour = "#333333",
                    alpha = isTrans ? 128 : 255,
                    isTransparent = isTrans,
                    isMetallic = false,
                    colourFinishId = isTrans ? transparentFinishId : solidFinishId,
                    sequence = sequence,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.BrickColours.Add(newColour);
                existingCodes.Add(colourCode);
                result.Created++;
                sequence++;
            }

            await _context.SaveChangesAsync();
            _log($"  Colours: {result.Created} added, {result.Skipped} already existed");

            return result;
        }

        // ─────────────────────────────────────────────────
        // 3. PART CATEGORIES
        // ─────────────────────────────────────────────────

        /// <summary>
        /// Import part_categories.csv → BrickCategory.
        /// </summary>
        public async Task<ImportResult> ImportPartCategoriesAsync(string categoriesPath)
        {
            ImportResult result = new ImportResult();
            _log($"  Parsing {categoriesPath}...");

            // Parse CSV: id,name
            List<string[]> rows = ParseCsv(categoriesPath);
            _log($"  Parsed {rows.Count} part categories");

            // Load existing by rebrickablePartCategoryId
            Dictionary<int, BrickCategory> existing = await _context.BrickCategories
                .Where(c => c.rebrickablePartCategoryId != null)
                .ToDictionaryAsync(c => c.rebrickablePartCategoryId.Value, c => c);

            // Also try to match by name for categories already in the DB
            Dictionary<string, BrickCategory> existingByName = await _context.BrickCategories
                .Where(c => c.rebrickablePartCategoryId == null)
                .ToDictionaryAsync(c => c.name, c => c, StringComparer.OrdinalIgnoreCase);

            int sequence = 100; // After the LDraw-seeded categories
            foreach (string[] row in rows)
            {
                int rebrickableId = int.Parse(row[0]);
                string name = row[1];

                if (existing.TryGetValue(rebrickableId, out BrickCategory entity))
                {
                    // Already mapped — update name
                    entity.name = name;
                    result.Updated++;
                }
                else if (existingByName.TryGetValue(name, out entity))
                {
                    // Matched by name — set the rebrickable mapping
                    entity.rebrickablePartCategoryId = rebrickableId;
                    result.Updated++;
                }
                else
                {
                    // New category
                    entity = new BrickCategory
                    {
                        name = name,
                        description = name,
                        rebrickablePartCategoryId = rebrickableId,
                        sequence = sequence,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };
                    _context.BrickCategories.Add(entity);
                    result.Created++;
                }

                sequence++;
            }

            await _context.SaveChangesAsync();
            _log($"  Part Categories: {result.Created} created, {result.Updated} updated");

            return result;
        }

        // ─────────────────────────────────────────────────
        // 4. PARTS
        // ─────────────────────────────────────────────────

        /// <summary>
        /// Import parts.csv → BrickPart.
        /// For parts already in the DB (matched by ldrawPartId), sets rebrickablePartNum.
        /// For parts not in the DB, creates a new BrickPart entry.
        /// </summary>
        public async Task<ImportResult> ImportPartsAsync(string partsPath)
        {
            ImportResult result = new ImportResult();
            _log($"  Parsing {partsPath}...");

            // Parse CSV: part_num,name,part_cat_id,part_material
            List<string[]> rows = ParseCsv(partsPath);
            _log($"  Parsed {rows.Count} parts");

            // Load existing parts by ldrawPartId
            Dictionary<string, BrickPart> existingByLdraw = await _context.BrickParts
                .Where(bp => bp.ldrawPartId != null)
                .ToDictionaryAsync(bp => bp.ldrawPartId, bp => bp);

            // Load category mapping: rebrickablePartCategoryId → BrickCategory.id
            Dictionary<int, int> categoryMap = await _context.BrickCategories
                .Where(c => c.rebrickablePartCategoryId != null)
                .ToDictionaryAsync(c => c.rebrickablePartCategoryId.Value, c => c.id);

            // Default PartType for Rebrickable imports (Part = 1)
            int defaultPartTypeId = await _context.PartTypes
                .Where(pt => pt.name == "Part")
                .Select(pt => pt.id)
                .FirstOrDefaultAsync();
            if (defaultPartTypeId == 0) defaultPartTypeId = 1;

            // Default category for unmatched
            int defaultCategoryId = await _context.BrickCategories
                .Select(c => c.id)
                .FirstOrDefaultAsync();

            List<BrickPart> toInsert = new List<BrickPart>();
            int batchCount = 0;

            foreach (string[] row in rows)
            {
                string partNum = row[0];
                string name = row[1];
                int partCatId = int.TryParse(row[2], out int pcid) ? pcid : 0;

                // Resolve category FK
                int brickCategoryId = categoryMap.ContainsKey(partCatId) ? categoryMap[partCatId] : defaultCategoryId;

                if (existingByLdraw.TryGetValue(partNum, out BrickPart existing_part))
                {
                    // Already have this part from LDraw — just set the Rebrickable mapping
                    existing_part.rebrickablePartNum = partNum;
                    result.Updated++;
                }
                else
                {
                    // New part from Rebrickable
                    BrickPart newPart = new BrickPart
                    {
                        name = partNum,
                        ldrawPartId = partNum,
                        ldrawTitle = name,
                        rebrickablePartNum = partNum,
                        partTypeId = defaultPartTypeId,
                        brickCategoryId = brickCategoryId,
                        versionNumber = 0,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };
                    _context.BrickParts.Add(newPart);
                    existingByLdraw[partNum] = newPart; // Prevent duplicates within batch
                    result.Created++;
                }

                batchCount++;
                if (batchCount >= 500)
                {
                    await _context.SaveChangesAsync();
                    _log($"  Processed {result.Created + result.Updated}/{rows.Count} parts...");
                    batchCount = 0;
                }
            }

            if (batchCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            _log($"  Parts: {result.Created} created, {result.Updated} updated");

            return result;
        }

        // ─────────────────────────────────────────────────
        // 5. PART RELATIONSHIPS
        // ─────────────────────────────────────────────────

        /// <summary>
        /// Import part_relationships.csv → BrickPartRelationship.
        /// </summary>
        public async Task<ImportResult> ImportPartRelationshipsAsync(string relationshipsPath)
        {
            ImportResult result = new ImportResult();
            _log($"  Parsing {relationshipsPath}...");

            // Parse CSV: rel_type,child_part_num,parent_part_num
            List<string[]> rows = ParseCsv(relationshipsPath);
            _log($"  Parsed {rows.Count} relationships");

            // Load part lookup: ldrawPartId → id
            Dictionary<string, int> partLookup = await _context.BrickParts
                .AsNoTracking()
                .Where(bp => bp.ldrawPartId != null)
                .ToDictionaryAsync(bp => bp.ldrawPartId, bp => bp.id);

            // Map Rebrickable relationship type codes to display names
            Dictionary<string, string> relTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "P", "Print" },
                { "R", "Pair" },
                { "B", "SubPart" },
                { "M", "Mold" },
                { "T", "Pattern" },
                { "A", "Alternate" }
            };

            List<BrickPartRelationship> toInsert = new List<BrickPartRelationship>();

            foreach (string[] row in rows)
            {
                string relTypeCode = row[0];
                string childPartNum = row[1];
                string parentPartNum = row[2];

                if (!partLookup.TryGetValue(childPartNum, out int childId))
                {
                    result.Skipped++;
                    continue;
                }

                if (!partLookup.TryGetValue(parentPartNum, out int parentId))
                {
                    result.Skipped++;
                    continue;
                }

                string relType = relTypeMap.ContainsKey(relTypeCode) ? relTypeMap[relTypeCode] : relTypeCode;

                toInsert.Add(new BrickPartRelationship
                {
                    childBrickPartId = childId,
                    parentBrickPartId = parentId,
                    relationshipType = relType,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                });

                result.Created++;
            }

            // Bulk insert
            if (toInsert.Count > 0)
            {
                await _context.BulkInsertAsync(toInsert);
            }

            _log($"  Part Relationships: {result.Created} created, {result.Skipped} skipped (missing parts)");

            return result;
        }

        // ─────────────────────────────────────────────────
        // 6. ELEMENTS
        // ─────────────────────────────────────────────────

        /// <summary>
        /// Import elements.csv → BrickElement.
        /// </summary>
        public async Task<ImportResult> ImportElementsAsync(string elementsPath)
        {
            ImportResult result = new ImportResult();
            _log($"  Parsing {elementsPath}...");

            // Parse CSV: element_id,part_num,color_id,design_id
            List<string[]> rows = ParseCsv(elementsPath);
            _log($"  Parsed {rows.Count} elements");

            // Load lookups
            Dictionary<string, int> partLookup = await _context.BrickParts
                .AsNoTracking()
                .Where(bp => bp.ldrawPartId != null)
                .ToDictionaryAsync(bp => bp.ldrawPartId, bp => bp.id);

            Dictionary<int, int> colourLookup = await _context.BrickColours
                .AsNoTracking()
                .Where(bc => bc.ldrawColourCode.HasValue)
                .ToDictionaryAsync(bc => bc.ldrawColourCode.Value, bc => bc.id);

            //
            // AI-developed: Build moved part resolution map so elements referencing moved parts
            // get redirected to the actual destination part.
            //
            Dictionary<int, int> movedPartMap = await BuildMovedPartMapAsync();
            int elementsReconciled = 0;

            // Load existing element IDs to avoid duplicates
            HashSet<string> existingElementIds = new HashSet<string>(
                await _context.BrickElements.Select(e => e.elementId).ToListAsync());

            List<BrickElement> toInsert = new List<BrickElement>();

            foreach (string[] row in rows)
            {
                string elementId = row[0];
                string partNum = row[1];
                int colourCode = int.TryParse(row[2], out int cc) ? cc : -1;

                if (existingElementIds.Contains(elementId))
                {
                    result.Skipped++;
                    continue;
                }

                if (!partLookup.TryGetValue(partNum, out int partId))
                {
                    result.Skipped++;
                    continue;
                }

                //
                // Reconcile moved parts — redirect to the actual destination part
                //
                if (movedPartMap.TryGetValue(partId, out int resolvedPartId))
                {
                    partId = resolvedPartId;
                    elementsReconciled++;
                }

                if (!colourLookup.TryGetValue(colourCode, out int colourId))
                {
                    result.Skipped++;
                    continue;
                }

                string designId = row.Length > 3 && !string.IsNullOrEmpty(row[3]) ? row[3] : null;

                toInsert.Add(new BrickElement
                {
                    elementId = elementId,
                    brickPartId = partId,
                    brickColourId = colourId,
                    designId = designId,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                });

                result.Created++;
            }

            if (toInsert.Count > 0)
            {
                // Bulk insert in chunks to manage memory
                for (int i = 0; i < toInsert.Count; i += 5000)
                {
                    List<BrickElement> chunk = toInsert.Skip(i).Take(5000).ToList();
                    await _context.BulkInsertAsync(chunk);
                    _log($"  Elements: inserted {Math.Min(i + 5000, toInsert.Count)}/{toInsert.Count}...");
                }
            }

            _log($"  Elements: {result.Created} created, {result.Skipped} skipped, {elementsReconciled} moved parts reconciled");

            return result;
        }

        // ─────────────────────────────────────────────────
        // 7. SETS
        // ─────────────────────────────────────────────────

        /// <summary>
        /// Import sets.csv → LegoSet.
        /// </summary>
        public async Task<ImportResult> ImportSetsAsync(string setsPath)
        {
            ImportResult result = new ImportResult();
            _log($"  Parsing {setsPath}...");

            // Parse CSV: set_num,name,year,theme_id,num_parts,img_url
            List<string[]> rows = ParseCsv(setsPath);
            _log($"  Parsed {rows.Count} sets");

            // Load theme mapping: rebrickableThemeId → LegoTheme.id
            Dictionary<int, int> themeLookup = await _context.LegoThemes
                .AsNoTracking()
                .ToDictionaryAsync(t => t.rebrickableThemeId, t => t.id);

            // Load existing sets by setNumber
            Dictionary<string, LegoSet> existing = await _context.LegoSets
                .ToDictionaryAsync(s => s.setNumber, s => s);

            // Detect duplicate set names and existing DB names
            Dictionary<string, int> setNameCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (string[] row in rows)
            {
                string n = row[1];
                setNameCounts.TryGetValue(n, out int c);
                setNameCounts[n] = c + 1;
            }
            HashSet<string> existingSetNames = new HashSet<string>(
                existing.Values.Select(s => s.name), StringComparer.OrdinalIgnoreCase);
            HashSet<string> usedSetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int batchCount = 0;

            foreach (string[] row in rows)
            {
                string setNum = row[0];
                string name = row[1];

                // Disambiguate duplicate names by appending set number
                if (setNameCounts[name] > 1 || existingSetNames.Contains(name))
                {
                    name = $"{name} ({setNum})";
                }
                if (usedSetNames.Contains(name))
                {
                    name = $"{name} [{setNum}]";
                }
                usedSetNames.Add(name);

                int year = int.TryParse(row[2], out int y) ? y : 0;
                int themeId = int.TryParse(row[3], out int tid) ? tid : 0;
                int partCount = int.TryParse(row[4], out int pc) ? pc : 0;
                string imageUrl = row.Length > 5 ? row[5] : null;

                // Resolve theme FK
                int? legoThemeId = themeLookup.ContainsKey(themeId) ? themeLookup[themeId] : (int?)null;

                if (existing.TryGetValue(setNum, out LegoSet entity))
                {
                    entity.name = name;
                    entity.year = year;
                    entity.partCount = partCount;
                    entity.legoThemeId = legoThemeId;
                    entity.imageUrl = imageUrl;
                    entity.rebrickableUrl = $"https://rebrickable.com/sets/{setNum}/";
                    result.Updated++;
                }
                else
                {
                    entity = new LegoSet
                    {
                        name = name,
                        setNumber = setNum,
                        year = year,
                        partCount = partCount,
                        legoThemeId = legoThemeId,
                        imageUrl = imageUrl,
                        rebrickableUrl = $"https://rebrickable.com/sets/{setNum}/",
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };
                    _context.LegoSets.Add(entity);
                    existing[setNum] = entity;
                    result.Created++;
                }

                batchCount++;
                if (batchCount >= 500)
                {
                    await _context.SaveChangesAsync();
                    _log($"  Sets: processed {result.Created + result.Updated}/{rows.Count}...");
                    batchCount = 0;
                }
            }

            if (batchCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            _log($"  Sets: {result.Created} created, {result.Updated} updated");

            return result;
        }

        // ─────────────────────────────────────────────────
        // 8. MINIFIGS
        // ─────────────────────────────────────────────────

        /// <summary>
        /// Import minifigs.csv → LegoMinifig.
        /// </summary>
        public async Task<ImportResult> ImportMinifigsAsync(string minifigsPath)
        {
            ImportResult result = new ImportResult();
            _log($"  Parsing {minifigsPath}...");

            // Parse CSV: fig_num,name,num_parts,img_url
            List<string[]> rows = ParseCsv(minifigsPath);
            _log($"  Parsed {rows.Count} minifigs");

            // Load existing by figNumber
            Dictionary<string, LegoMinifig> existing = await _context.LegoMinifigs
                .ToDictionaryAsync(m => m.figNumber, m => m);

            int batchCount = 0;

            foreach (string[] row in rows)
            {
                string figNum = row[0];
                string name = row[1];
                int partCount = int.TryParse(row[2], out int pc) ? pc : 0;
                string imageUrl = row.Length > 3 ? row[3] : null;

                if (existing.TryGetValue(figNum, out LegoMinifig entity))
                {
                    entity.name = name;
                    entity.partCount = partCount;
                    entity.imageUrl = imageUrl;
                    result.Updated++;
                }
                else
                {
                    entity = new LegoMinifig
                    {
                        name = name,
                        figNumber = figNum,
                        partCount = partCount,
                        imageUrl = imageUrl,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };
                    _context.LegoMinifigs.Add(entity);
                    existing[figNum] = entity;
                    result.Created++;
                }

                batchCount++;
                if (batchCount >= 500)
                {
                    await _context.SaveChangesAsync();
                    _log($"  Minifigs: processed {result.Created + result.Updated}/{rows.Count}...");
                    batchCount = 0;
                }
            }

            if (batchCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            _log($"  Minifigs: {result.Created} created, {result.Updated} updated");

            return result;
        }

        // ─────────────────────────────────────────────────
        // 9a. INVENTORY PARTS
        // ─────────────────────────────────────────────────

        /// <summary>
        /// Import inventories.csv + inventory_parts.csv → LegoSetPart.
        /// Uses the highest version inventory for each set.
        /// </summary>
        public async Task<ImportResult> ImportInventoryPartsAsync(string inventoriesPath, string inventoryPartsPath)
        {
            ImportResult result = new ImportResult();

            // Build inventory → setNumber map (use highest version per set)
            Dictionary<int, string> inventoryToSet = BuildInventoryMap(inventoriesPath);
            _log($"  Loaded {inventoryToSet.Count} inventory mappings");

            _log($"  Parsing {inventoryPartsPath}...");

            // Load all FK lookups
            Dictionary<string, int> setLookup = await _context.LegoSets
                .AsNoTracking()
                .ToDictionaryAsync(s => s.setNumber, s => s.id);

            Dictionary<string, int> partLookup = await _context.BrickParts
                .AsNoTracking()
                .Where(bp => bp.ldrawPartId != null)
                .ToDictionaryAsync(bp => bp.ldrawPartId, bp => bp.id);

            Dictionary<int, int> colourLookup = await _context.BrickColours
                .AsNoTracking()
                .Where(bc => bc.ldrawColourCode.HasValue)
                .ToDictionaryAsync(bc => bc.ldrawColourCode.Value, bc => bc.id);

            _log($"  FK lookups: {setLookup.Count} sets, {partLookup.Count} parts, {colourLookup.Count} colours");

            //
            // AI-developed: Build moved part resolution map so inventory parts referencing
            // moved parts get redirected to the actual destination part.
            //
            Dictionary<int, int> movedPartMap = await BuildMovedPartMapAsync();
            int partsReconciled = 0;

            // Parse inventory_parts.csv: inventory_id,part_num,color_id,quantity,is_spare,img_url
            List<string[]> rows = ParseCsv(inventoryPartsPath);
            _log($"  Parsed {rows.Count} inventory part lines");

            List<LegoSetPart> toInsert = new List<LegoSetPart>();

            foreach (string[] row in rows)
            {
                int inventoryId = int.TryParse(row[0], out int iid) ? iid : -1;
                string partNum = row[1];
                int colourCode = int.TryParse(row[2], out int cc) ? cc : -1;
                int quantity = int.TryParse(row[3], out int qty) ? qty : 1;
                bool isSpare = row.Length > 4 && row[4].Equals("t", StringComparison.OrdinalIgnoreCase);

                // Resolve inventory → set
                if (!inventoryToSet.TryGetValue(inventoryId, out string setNum))
                {
                    result.Skipped++;
                    continue;
                }

                if (!setLookup.TryGetValue(setNum, out int legoSetId))
                {
                    result.Skipped++;
                    continue;
                }

                if (!partLookup.TryGetValue(partNum, out int brickPartId))
                {
                    result.Skipped++;
                    continue;
                }

                //
                // Reconcile moved parts — redirect to the actual destination part
                //
                if (movedPartMap.TryGetValue(brickPartId, out int resolvedPartId))
                {
                    brickPartId = resolvedPartId;
                    partsReconciled++;
                }

                if (!colourLookup.TryGetValue(colourCode, out int brickColourId))
                {
                    result.Skipped++;
                    continue;
                }

                toInsert.Add(new LegoSetPart
                {
                    legoSetId = legoSetId,
                    brickPartId = brickPartId,
                    brickColourId = brickColourId,
                    quantity = quantity,
                    isSpare = isSpare,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                });

                result.Created++;
            }

            if (toInsert.Count > 0)
            {
                for (int i = 0; i < toInsert.Count; i += 5000)
                {
                    List<LegoSetPart> chunk = toInsert.Skip(i).Take(5000).ToList();
                    await _context.BulkInsertAsync(chunk);
                    _log($"  Inventory Parts: inserted {Math.Min(i + 5000, toInsert.Count)}/{toInsert.Count}...");
                }
            }

            _log($"  Inventory Parts: {result.Created} created, {result.Skipped} skipped, {partsReconciled} moved parts reconciled");

            return result;
        }

        // ─────────────────────────────────────────────────
        // 9b. INVENTORY SETS (subsets)
        // ─────────────────────────────────────────────────

        /// <summary>
        /// Import inventories.csv + inventory_sets.csv → LegoSetSubset.
        /// </summary>
        public async Task<ImportResult> ImportInventorySetsAsync(string inventoriesPath, string inventorySetsPath)
        {
            ImportResult result = new ImportResult();

            Dictionary<int, string> inventoryToSet = BuildInventoryMap(inventoriesPath);

            _log($"  Parsing {inventorySetsPath}...");

            // Parse CSV: inventory_id,set_num,quantity
            List<string[]> rows = ParseCsv(inventorySetsPath);
            _log($"  Parsed {rows.Count} inventory set lines");

            Dictionary<string, int> setLookup = await _context.LegoSets
                .AsNoTracking()
                .ToDictionaryAsync(s => s.setNumber, s => s.id);

            List<LegoSetSubset> toInsert = new List<LegoSetSubset>();

            foreach (string[] row in rows)
            {
                int inventoryId = int.TryParse(row[0], out int iid) ? iid : -1;
                string childSetNum = row[1];
                int quantity = int.TryParse(row[2], out int qty) ? qty : 1;

                // Resolve parent set from inventory
                if (!inventoryToSet.TryGetValue(inventoryId, out string parentSetNum))
                {
                    result.Skipped++;
                    continue;
                }

                if (!setLookup.TryGetValue(parentSetNum, out int parentSetId))
                {
                    result.Skipped++;
                    continue;
                }

                if (!setLookup.TryGetValue(childSetNum, out int childSetId))
                {
                    result.Skipped++;
                    continue;
                }

                toInsert.Add(new LegoSetSubset
                {
                    parentLegoSetId = parentSetId,
                    childLegoSetId = childSetId,
                    quantity = quantity,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                });

                result.Created++;
            }

            if (toInsert.Count > 0)
            {
                await _context.BulkInsertAsync(toInsert);
            }

            _log($"  Inventory Sets: {result.Created} created, {result.Skipped} skipped");

            return result;
        }

        // ─────────────────────────────────────────────────
        // 9c. INVENTORY MINIFIGS
        // ─────────────────────────────────────────────────

        /// <summary>
        /// Import inventories.csv + inventory_minifigs.csv → LegoSetMinifig.
        /// </summary>
        public async Task<ImportResult> ImportInventoryMinifigsAsync(string inventoriesPath, string inventoryMinifigsPath)
        {
            ImportResult result = new ImportResult();

            Dictionary<int, string> inventoryToSet = BuildInventoryMap(inventoriesPath);

            _log($"  Parsing {inventoryMinifigsPath}...");

            // Parse CSV: inventory_id,fig_num,quantity
            List<string[]> rows = ParseCsv(inventoryMinifigsPath);
            _log($"  Parsed {rows.Count} inventory minifig lines");

            Dictionary<string, int> setLookup = await _context.LegoSets
                .AsNoTracking()
                .ToDictionaryAsync(s => s.setNumber, s => s.id);

            Dictionary<string, int> minifigLookup = await _context.LegoMinifigs
                .AsNoTracking()
                .ToDictionaryAsync(m => m.figNumber, m => m.id);

            List<LegoSetMinifig> toInsert = new List<LegoSetMinifig>();

            foreach (string[] row in rows)
            {
                int inventoryId = int.TryParse(row[0], out int iid) ? iid : -1;
                string figNum = row[1];
                int quantity = int.TryParse(row[2], out int qty) ? qty : 1;

                if (!inventoryToSet.TryGetValue(inventoryId, out string setNum))
                {
                    result.Skipped++;
                    continue;
                }

                if (!setLookup.TryGetValue(setNum, out int legoSetId))
                {
                    result.Skipped++;
                    continue;
                }

                if (!minifigLookup.TryGetValue(figNum, out int minifigId))
                {
                    result.Skipped++;
                    continue;
                }

                toInsert.Add(new LegoSetMinifig
                {
                    legoSetId = legoSetId,
                    legoMinifigId = minifigId,
                    quantity = quantity,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                });

                result.Created++;
            }

            if (toInsert.Count > 0)
            {
                await _context.BulkInsertAsync(toInsert);
            }

            _log($"  Inventory Minifigs: {result.Created} created, {result.Skipped} skipped");

            return result;
        }

        // ─────────────────────────────────────────────────
        // 10. RECONCILE MOVED PARTS (post-import correction)
        // ─────────────────────────────────────────────────

        /// <summary>
        ///
        /// AI-developed: Reconciles already-imported data that references moved parts.
        /// Scans LegoSetPart and BrickElement rows, redirecting any that point to a
        /// moved BrickPart to the actual destination part instead.
        /// Safe to re-run — idempotent.
        ///
        /// </summary>
        public async Task<ReconcileResult> ReconcileMovedPartsAsync()
        {
            ReconcileResult result = new ReconcileResult();

            //
            // Build the moved part resolution map
            //
            Dictionary<int, int> movedPartMap = await BuildMovedPartMapAsync();

            if (movedPartMap.Count == 0)
            {
                _log("  No moved parts found — nothing to reconcile.");
                return result;
            }

            //
            // Get the set of moved part IDs for efficient SQL filtering
            //
            HashSet<int> movedPartIds = new HashSet<int>(movedPartMap.Keys);

            //
            // Step 1: Fix LegoSetPart rows that reference moved parts
            //
            _log("  Scanning LegoSetPart rows for moved part references...");
            List<LegoSetPart> affectedSetParts = await _context.LegoSetParts
                .Where(lsp => movedPartIds.Contains(lsp.brickPartId))
                .ToListAsync();

            foreach (LegoSetPart setPart in affectedSetParts)
            {
                if (movedPartMap.TryGetValue(setPart.brickPartId, out int resolvedId))
                {
                    setPart.brickPartId = resolvedId;
                    result.SetPartsFixed++;
                }
            }

            if (result.SetPartsFixed > 0)
            {
                await _context.SaveChangesAsync();
            }

            _log($"  LegoSetPart: {result.SetPartsFixed} rows redirected to actual parts");

            //
            // Step 2: Fix BrickElement rows that reference moved parts
            //
            _log("  Scanning BrickElement rows for moved part references...");
            List<BrickElement> affectedElements = await _context.BrickElements
                .Where(be => movedPartIds.Contains(be.brickPartId))
                .ToListAsync();

            foreach (BrickElement element in affectedElements)
            {
                if (movedPartMap.TryGetValue(element.brickPartId, out int resolvedId))
                {
                    element.brickPartId = resolvedId;
                    result.ElementsFixed++;
                }
            }

            if (result.ElementsFixed > 0)
            {
                await _context.SaveChangesAsync();
            }

            _log($"  BrickElement: {result.ElementsFixed} rows redirected to actual parts");

            return result;
        }


        // ─────────────────────────────────────────────────
        // Helper: Build moved part resolution map
        // ─────────────────────────────────────────────────

        /// <summary>
        ///
        /// AI-developed: Builds a dictionary that maps moved BrickPart IDs to their actual
        /// destination part IDs.  Parses the destination from the ldrawTitle field
        /// (e.g. "~Moved to 10a" → destination ldrawPartId "10a").
        ///
        /// Follows chains: if part A moved to B and B moved to C, the map resolves A → C.
        /// Logs warnings for any unresolvable moves (destination part not found in DB).
        ///
        /// </summary>
        private async Task<Dictionary<int, int>> BuildMovedPartMapAsync()
        {
            //
            // Step 1: Load all moved parts from the database
            //
            List<BrickPart> movedParts = await _context.BrickParts
                .Where(bp => bp.ldrawTitle.StartsWith("~Moved to "))
                .AsNoTracking()
                .ToListAsync();

            if (movedParts.Count == 0)
            {
                return new Dictionary<int, int>();
            }

            _log($"  Found {movedParts.Count} moved parts to resolve");

            //
            // Step 2: Build a lookup from ldrawPartId → BrickPart.id for resolving destinations
            //
            Dictionary<string, int> partIdLookup = await _context.BrickParts
                .AsNoTracking()
                .ToDictionaryAsync(bp => bp.ldrawPartId, bp => bp.id);

            //
            // Step 3: Build the initial moved → destination map (single hop)
            //
            Dictionary<int, int> movedToDestination = new Dictionary<int, int>();
            int unresolvable = 0;

            foreach (BrickPart movedPart in movedParts)
            {
                //
                // Parse the destination part ID from the title text.
                // Format is "~Moved to <partId>" — extract everything after "~Moved to "
                //
                string destinationPartId = movedPart.ldrawTitle.Substring("~Moved to ".Length).Trim();

                if (string.IsNullOrEmpty(destinationPartId))
                {
                    _log($"  WARN: Could not parse destination from: '{movedPart.ldrawTitle}' (part {movedPart.ldrawPartId})");
                    unresolvable++;
                    continue;
                }

                if (partIdLookup.TryGetValue(destinationPartId, out int destinationId))
                {
                    movedToDestination[movedPart.id] = destinationId;
                }
                else
                {
                    _log($"  WARN: Destination part '{destinationPartId}' not found in DB (from moved part {movedPart.ldrawPartId})");
                    unresolvable++;
                }
            }

            //
            // Step 4: Follow chains — if destination is also a moved part, resolve transitively.
            // Use a maximum depth to prevent infinite loops in case of circular references.
            //
            int MAX_CHAIN_DEPTH = 10;
            int chainsFollowed = 0;

            foreach (int movedId in new List<int>(movedToDestination.Keys))
            {
                int currentDestination = movedToDestination[movedId];
                int depth = 0;

                while (movedToDestination.ContainsKey(currentDestination) && depth < MAX_CHAIN_DEPTH)
                {
                    currentDestination = movedToDestination[currentDestination];
                    depth++;
                }

                if (depth > 0)
                {
                    movedToDestination[movedId] = currentDestination;
                    chainsFollowed++;
                }
            }

            _log($"  Moved part map: {movedToDestination.Count} resolved, {chainsFollowed} chains followed, {unresolvable} unresolvable");

            return movedToDestination;
        }


        // ─────────────────────────────────────────────────
        // Helper: Build inventory → set mapping
        // ─────────────────────────────────────────────────

        /// <summary>
        /// Parse inventories.csv and build a map of inventory_id → set_num.
        /// When multiple inventory versions exist for a set, keeps only the highest version.
        /// </summary>
        private Dictionary<int, string> BuildInventoryMap(string inventoriesPath)
        {
            // Parse CSV: id,version,set_num
            List<string[]> rows = ParseCsv(inventoriesPath);

            // Group by set_num, keep highest version
            Dictionary<string, (int inventoryId, int version)> bestVersionPerSet =
                new Dictionary<string, (int, int)>();

            foreach (string[] row in rows)
            {
                int inventoryId = int.Parse(row[0]);
                int version = int.TryParse(row[1], out int v) ? v : 1;
                string setNum = row[2];

                if (!bestVersionPerSet.TryGetValue(setNum, out var current) || version > current.version)
                {
                    bestVersionPerSet[setNum] = (inventoryId, version);
                }
            }

            // Build reverse map: inventoryId → setNum
            Dictionary<int, string> result = new Dictionary<int, string>();
            foreach (var kvp in bestVersionPerSet)
            {
                result[kvp.Value.inventoryId] = kvp.Key;
            }

            return result;
        }

        // ─────────────────────────────────────────────────
        // Helper: Simple CSV parser
        // ─────────────────────────────────────────────────

        /// <summary>
        /// Parse a CSV file, skipping the header row.
        /// Handles basic quoted fields (Rebrickable CSVs use minimal quoting).
        /// </summary>
        private static List<string[]> ParseCsv(string path)
        {
            List<string[]> result = new List<string[]>();
            bool isFirstLine = true;

            foreach (string line in File.ReadLines(path))
            {
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue; // Skip header
                }

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                result.Add(ParseCsvLine(line));
            }

            return result;
        }

        /// <summary>
        /// Parse a single CSV line, handling quoted fields.
        /// </summary>
        private static string[] ParseCsvLine(string line)
        {
            List<string> fields = new List<string>();
            int i = 0;

            while (i < line.Length)
            {
                if (line[i] == '"')
                {
                    // Quoted field
                    i++; // Skip opening quote
                    int start = i;
                    while (i < line.Length)
                    {
                        if (line[i] == '"')
                        {
                            if (i + 1 < line.Length && line[i + 1] == '"')
                            {
                                i += 2; // Escaped quote
                            }
                            else
                            {
                                break; // End of quoted field
                            }
                        }
                        else
                        {
                            i++;
                        }
                    }
                    fields.Add(line.Substring(start, i - start).Replace("\"\"", "\""));
                    if (i < line.Length) i++; // Skip closing quote
                    if (i < line.Length && line[i] == ',') i++; // Skip comma
                }
                else
                {
                    // Unquoted field
                    int start = i;
                    while (i < line.Length && line[i] != ',')
                    {
                        i++;
                    }
                    fields.Add(line.Substring(start, i - start));
                    if (i < line.Length) i++; // Skip comma
                }
            }

            return fields.ToArray();
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


    /// <summary>
    /// Result counters for a moved part reconciliation operation.
    /// </summary>
    public class ReconcileResult
    {
        public int SetPartsFixed;
        public int ElementsFixed;

        public override string ToString()
        {
            return $"SetParts fixed: {SetPartsFixed}, Elements fixed: {ElementsFixed}";
        }
    }
}
