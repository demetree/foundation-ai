using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Foundation.BMC.Database;

namespace BMC.LDraw.Import
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine("  BMC LDraw Import Tool");
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine();

            // Parse CLI arguments
            string sourcePath = null;
            bool importColours = false;
            bool importParts = false;
            bool copyData = false;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--source":
                        if (i + 1 < args.Length) sourcePath = args[++i];
                        break;
                    case "--import":
                        // Read all following non-flag arguments as import targets
                        while (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                        {
                            i++;
                            if (args[i] == "colours" || args[i] == "colors") importColours = true;
                            if (args[i] == "parts") importParts = true;
                        }
                        break;
                    case "--copy-data":
                        copyData = true;
                        break;
                    case "--help":
                        PrintUsage();
                        return 0;
                }
            }

            // Validate
            if (string.IsNullOrEmpty(sourcePath))
            {
                Console.WriteLine("ERROR: --source <path> is required.");
                Console.WriteLine();
                PrintUsage();
                return 1;
            }

            if (!Directory.Exists(sourcePath))
            {
                Console.WriteLine($"ERROR: Source path does not exist: {sourcePath}");
                return 1;
            }

            if (!importColours && !importParts && !copyData)
            {
                Console.WriteLine("ERROR: Specify at least one action: --import colours/parts, --copy-data");
                Console.WriteLine();
                PrintUsage();
                return 1;
            }

            // Load configuration
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            string connectionString = config.GetConnectionString("BMC");
            string dataPath = config["LDraw:DataPath"] ?? Environment.GetEnvironmentVariable("LDRAW_DATA_PATH") ?? "./ldraw-data";

            if (string.IsNullOrEmpty(connectionString) && (importColours || importParts))
            {
                Console.WriteLine("ERROR: No 'BMC' connection string found in appsettings.json or environment.");
                return 1;
            }

            // Set up DbContext
            DbContextOptionsBuilder<BMCContext> optionsBuilder = new DbContextOptionsBuilder<BMCContext>();
            if (!string.IsNullOrEmpty(connectionString))
            {
                optionsBuilder.UseSqlServer(connectionString);
            }

            using (BMCContext context = new BMCContext(optionsBuilder.Options))
            {
                LDrawImportService service = new LDrawImportService(context, Console.WriteLine);

                // Copy data files
                if (copyData)
                {
                    Console.WriteLine();
                    Console.WriteLine("── Copy Data Files ──────────────────────");
                    service.CopyDataFiles(sourcePath, dataPath);
                }

                // Import colours
                if (importColours)
                {
                    Console.WriteLine();
                    Console.WriteLine("── Import Colours ───────────────────────");
                    string ldConfigPath = Path.Combine(sourcePath, "LDConfig.ldr");
                    if (!File.Exists(ldConfigPath))
                    {
                        Console.WriteLine($"ERROR: LDConfig.ldr not found at {ldConfigPath}");
                        return 1;
                    }
                    ImportResult colourResult = await service.ImportColoursAsync(ldConfigPath);
                    Console.WriteLine($"  Result: {colourResult}");
                }

                // Import parts
                if (importParts)
                {
                    Console.WriteLine();
                    Console.WriteLine("── Import Part Headers ──────────────────");
                    string partsDir = Path.Combine(sourcePath, "parts");
                    if (!Directory.Exists(partsDir))
                    {
                        Console.WriteLine($"ERROR: parts directory not found at {partsDir}");
                        return 1;
                    }
                    ImportResult partResult = await service.ImportPartHeadersAsync(partsDir);
                    Console.WriteLine($"  Result: {partResult}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Done.");

            return 0;
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  BMC.LDraw.Import --source <ldraw-path> [--import colours parts] [--copy-data]");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  --source <path>    Path to LDraw library (e.g. d:\\ldraw)");
            Console.WriteLine("  --import <targets> What to import: colours, parts (space separated)");
            Console.WriteLine("  --copy-data        Copy LDraw files to configured data path");
            Console.WriteLine("  --help             Show this help");
            Console.WriteLine();
            Console.WriteLine("Configuration:");
            Console.WriteLine("  Connection string: appsettings.json -> ConnectionStrings:BMC");
            Console.WriteLine("  Data path:         appsettings.json -> LDraw:DataPath (default: ./ldraw-data)");
            Console.WriteLine("                     Or environment variable: LDRAW_DATA_PATH");
        }
    }
}
