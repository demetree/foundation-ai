using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Foundation.BMC.Database;

namespace BMC.Rebrickable.Import
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine("  BMC Rebrickable Import Tool");
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine();

            // Parse CLI arguments
            string sourcePath = null;
            bool download = false;
            bool forceDownload = false;
            HashSet<string> importTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--source":
                        if (i + 1 < args.Length) sourcePath = args[++i];
                        break;
                    case "--download":
                        download = true;
                        break;
                    case "--force-download":
                        download = true;
                        forceDownload = true;
                        break;
                    case "--import":
                        while (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                        {
                            i++;
                            importTargets.Add(args[i]);
                        }
                        break;
                    case "--help":
                        PrintUsage();
                        return 0;
                }
            }

            // Validate arguments
            if (!download && string.IsNullOrEmpty(sourcePath))
            {
                Console.WriteLine("ERROR: Either --download or --source <path> is required.");
                Console.WriteLine();
                PrintUsage();
                return 1;
            }

            if (!download && !string.IsNullOrEmpty(sourcePath) && !Directory.Exists(sourcePath))
            {
                Console.WriteLine($"ERROR: Source path does not exist: {sourcePath}");
                return 1;
            }

            if (importTargets.Count == 0)
            {
                Console.WriteLine("ERROR: Specify at least one import target.");
                Console.WriteLine();
                PrintUsage();
                return 1;
            }

            bool importAll = importTargets.Contains("all");

            // Load configuration
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            string connectionString = config.GetConnectionString("BMC");

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("ERROR: No 'BMC' connection string found in appsettings.json or environment.");
                return 1;
            }

            // Auto-download if requested
            if (download)
            {
                Console.WriteLine("── Download from Rebrickable ────────────");

                // Default cache location next to the executable
                if (string.IsNullOrEmpty(sourcePath))
                {
                    sourcePath = Path.Combine(
                        Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                        "rebrickable-data");
                }

                Console.WriteLine($"  Cache directory: {sourcePath}");
                Console.WriteLine();

                RebrickableDownloadService downloader = new RebrickableDownloadService(Console.WriteLine);

                try
                {
                    await downloader.DownloadAllAsync(sourcePath, forceDownload, importTargets);
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine($"ERROR: Download failed: {ex.Message}");
                    return 1;
                }

                Console.WriteLine();
            }

            // Set up DbContext
            DbContextOptionsBuilder<BMCContext> optionsBuilder = new DbContextOptionsBuilder<BMCContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using (BMCContext context = new BMCContext(optionsBuilder.Options))
            {
                RebrickableImportService service = new RebrickableImportService(context, Console.WriteLine);

                // Import order respects FK dependencies:
                // 1. Themes (self-referencing)
                // 2. Colours (standalone, additive only)
                // 3. Part categories (standalone)
                // 4. Parts (depends on categories)
                // 5. Part relationships (depends on parts)
                // 6. Elements (depends on parts + colours)
                // 7. Sets (depends on themes)
                // 8. Minifigs (standalone)
                // 9. Inventories → set parts, set subsets, set minifigs (depends on all above)

                if (importAll || importTargets.Contains("themes"))
                {
                    Console.WriteLine();
                    Console.WriteLine("── Import Themes ────────────────────────");
                    string path = Path.Combine(sourcePath, "themes.csv");
                    if (!File.Exists(path)) { Console.WriteLine($"  SKIP: {path} not found"); }
                    else { PrintResult("Themes", await service.ImportThemesAsync(path)); }
                }

                if (importAll || importTargets.Contains("colours") || importTargets.Contains("colors"))
                {
                    Console.WriteLine();
                    Console.WriteLine("── Import Colours (additive) ────────────");
                    string path = Path.Combine(sourcePath, "colors.csv");
                    if (!File.Exists(path)) { Console.WriteLine($"  SKIP: {path} not found"); }
                    else { PrintResult("Colours", await service.ImportColoursAsync(path)); }
                }

                if (importAll || importTargets.Contains("categories"))
                {
                    Console.WriteLine();
                    Console.WriteLine("── Import Part Categories ───────────────");
                    string path = Path.Combine(sourcePath, "part_categories.csv");
                    if (!File.Exists(path)) { Console.WriteLine($"  SKIP: {path} not found"); }
                    else { PrintResult("Part Categories", await service.ImportPartCategoriesAsync(path)); }
                }

                if (importAll || importTargets.Contains("parts"))
                {
                    Console.WriteLine();
                    Console.WriteLine("── Import Parts ─────────────────────────");
                    string path = Path.Combine(sourcePath, "parts.csv");
                    if (!File.Exists(path)) { Console.WriteLine($"  SKIP: {path} not found"); }
                    else { PrintResult("Parts", await service.ImportPartsAsync(path)); }
                }

                if (importAll || importTargets.Contains("relationships"))
                {
                    Console.WriteLine();
                    Console.WriteLine("── Import Part Relationships ────────────");
                    string path = Path.Combine(sourcePath, "part_relationships.csv");
                    if (!File.Exists(path)) { Console.WriteLine($"  SKIP: {path} not found"); }
                    else { PrintResult("Part Relationships", await service.ImportPartRelationshipsAsync(path)); }
                }

                if (importAll || importTargets.Contains("elements"))
                {
                    Console.WriteLine();
                    Console.WriteLine("── Import Elements ──────────────────────");
                    string path = Path.Combine(sourcePath, "elements.csv");
                    if (!File.Exists(path)) { Console.WriteLine($"  SKIP: {path} not found"); }
                    else { PrintResult("Elements", await service.ImportElementsAsync(path)); }
                }

                if (importAll || importTargets.Contains("sets"))
                {
                    Console.WriteLine();
                    Console.WriteLine("── Import Sets ──────────────────────────");
                    string path = Path.Combine(sourcePath, "sets.csv");
                    if (!File.Exists(path)) { Console.WriteLine($"  SKIP: {path} not found"); }
                    else { PrintResult("Sets", await service.ImportSetsAsync(path)); }
                }

                if (importAll || importTargets.Contains("minifigs"))
                {
                    Console.WriteLine();
                    Console.WriteLine("── Import Minifigs ──────────────────────");
                    string path = Path.Combine(sourcePath, "minifigs.csv");
                    if (!File.Exists(path)) { Console.WriteLine($"  SKIP: {path} not found"); }
                    else { PrintResult("Minifigs", await service.ImportMinifigsAsync(path)); }
                }

                if (importAll || importTargets.Contains("inventories"))
                {
                    Console.WriteLine();
                    Console.WriteLine("── Import Inventories ───────────────────");
                    string inventoriesPath = Path.Combine(sourcePath, "inventories.csv");
                    string invPartsPath = Path.Combine(sourcePath, "inventory_parts.csv");
                    string invSetsPath = Path.Combine(sourcePath, "inventory_sets.csv");
                    string invMinifigsPath = Path.Combine(sourcePath, "inventory_minifigs.csv");

                    if (!File.Exists(inventoriesPath))
                    {
                        Console.WriteLine($"  SKIP: {inventoriesPath} not found");
                    }
                    else
                    {
                        if (File.Exists(invPartsPath))
                        {
                            Console.WriteLine("  → Inventory Parts...");
                            PrintResult("Inventory Parts", await service.ImportInventoryPartsAsync(inventoriesPath, invPartsPath));
                        }

                        if (File.Exists(invSetsPath))
                        {
                            Console.WriteLine("  → Inventory Sets (subsets)...");
                            PrintResult("Inventory Sets", await service.ImportInventorySetsAsync(inventoriesPath, invSetsPath));
                        }

                        if (File.Exists(invMinifigsPath))
                        {
                            Console.WriteLine("  → Inventory Minifigs...");
                            PrintResult("Inventory Minifigs", await service.ImportInventoryMinifigsAsync(inventoriesPath, invMinifigsPath));
                        }
                    }
                }

                if (importAll || importTargets.Contains("reconcile"))
                {
                    Console.WriteLine();
                    Console.WriteLine("── Reconcile Moved Parts ────────────────");
                    ReconcileResult reconcileResult = await service.ReconcileMovedPartsAsync();
                    Console.WriteLine($"  Reconcile: {reconcileResult}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Done.");

            return 0;
        }

        static void PrintResult(string label, ImportResult result)
        {
            Console.WriteLine($"  {label}: {result}");
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  BMC.Rebrickable.Import --download --import <targets>");
            Console.WriteLine("  BMC.Rebrickable.Import --source <csv-folder> --import <targets>");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --download          Download CSVs from Rebrickable CDN automatically");
            Console.WriteLine("  --force-download    Re-download even if files already exist locally");
            Console.WriteLine("  --source <path>     Use local CSV folder (skip download)");
            Console.WriteLine("  --import <targets>  Specify what to import (see below)");
            Console.WriteLine();
            Console.WriteLine("Import targets:");
            Console.WriteLine("  all            Import everything (in dependency order)");
            Console.WriteLine("  themes         themes.csv → LegoTheme");
            Console.WriteLine("  colours/colors colors.csv → BrickColour (additive only)");
            Console.WriteLine("  categories     part_categories.csv → BrickCategory");
            Console.WriteLine("  parts          parts.csv → BrickPart");
            Console.WriteLine("  relationships  part_relationships.csv → BrickPartRelationship");
            Console.WriteLine("  elements       elements.csv → BrickElement");
            Console.WriteLine("  sets           sets.csv → LegoSet");
            Console.WriteLine("  minifigs       minifigs.csv → LegoMinifig");
            Console.WriteLine("  inventories    inventories.csv + inventory_*.csv → LegoSetPart/Subset/Minifig");
            Console.WriteLine("  reconcile      Fix existing data that references moved LDraw parts");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  BMC.Rebrickable.Import --download --import all");
            Console.WriteLine("  BMC.Rebrickable.Import --download --force-download --import themes sets");
            Console.WriteLine("  BMC.Rebrickable.Import --source d:\\rebrickable-csvs --import all");
            Console.WriteLine();
            Console.WriteLine("Configuration:");
            Console.WriteLine("  Connection string: appsettings.json → ConnectionStrings:BMC");
        }
    }
}
