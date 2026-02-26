using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BMC.LDraw.Render;
using Foundation.BMC.Database;
using Microsoft.EntityFrameworkCore;

namespace BMC.LDraw.Render.CLI
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length >= 2)
            {
                // Direct mode: render a specific file
                return RunDirect(args);
            }

            // Interactive mode: connect to DB and browse/search/render parts
            return RunInteractive(args);
        }

        // ════════════════════════════════════════════════════════════════
        // Interactive REPL mode
        // ════════════════════════════════════════════════════════════════

        static int RunInteractive(string[] args)
        {
            // Parse optional library path from args
            string libraryPath = null;
            for (int i = 0; i < args.Length; i++)
            {
                if ((args[i] == "--library" || args[i] == "-l") && i + 1 < args.Length)
                {
                    libraryPath = args[i + 1];
                }
            }

            PrintBanner();

            // Connect to database
            BMCContext db;
            try
            {
                db = new BMCContext();
                int partCount = db.BrickParts.Count(p => p.active && !p.deleted);
                int colourCount = db.BrickColours.Count(c => c.active && !c.deleted);
                WriteColour($"  Connected to BMC database — {partCount:N0} parts, {colourCount:N0} colours", ConsoleColor.Green);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                WriteColour($"  ✗ Could not connect to database: {ex.Message}", ConsoleColor.Red);
                Console.WriteLine("    Make sure SQL Server is running and appsettings.json has the correct connection string.");
                return 1;
            }

            // Auto-detect library path
            if (libraryPath == null)
            {
                libraryPath = TryFindLibrary(null);
            }
            if (libraryPath != null)
            {
                WriteColour($"  LDraw library: {libraryPath}", ConsoleColor.DarkGray);
            }
            else
            {
                WriteColour("  ⚠ LDraw library not found. Use 'library <path>' to set it before rendering.", ConsoleColor.Yellow);
            }

            // State
            string outputDir = Directory.GetCurrentDirectory();
            int width = 512, height = 512;

            Console.WriteLine();
            WriteColour("  Type 'help' for commands, 'quit' to exit.", ConsoleColor.DarkGray);
            Console.WriteLine();

            // REPL loop
            while (true)
            {
                WriteColour("ldraw> ", ConsoleColor.Cyan, newLine: false);
                string input = Console.ReadLine();
                if (input == null) break; // EOF

                string trimmed = input.Trim();
                if (trimmed.Length == 0) continue;

                string[] parts = trimmed.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                string command = parts[0].ToLowerInvariant();
                string argument = parts.Length > 1 ? parts[1].Trim() : "";

                try
                {
                    switch (command)
                    {
                        case "quit":
                        case "exit":
                        case "q":
                            return 0;

                        case "help":
                        case "?":
                            PrintHelp();
                            break;

                        case "search":
                        case "s":
                            CmdSearch(db, argument);
                            break;

                        case "list":
                        case "ls":
                            CmdList(db, argument);
                            break;

                        case "info":
                        case "i":
                            CmdInfo(db, argument);
                            break;

                        case "colours":
                        case "colors":
                        case "c":
                            CmdColours(db, argument);
                            break;

                        case "render":
                        case "r":
                            CmdRender(db, argument, libraryPath, outputDir, width, height);
                            break;

                        case "library":
                            if (argument.Length > 0 && Directory.Exists(argument))
                            {
                                libraryPath = argument;
                                WriteColour($"  Library set to: {libraryPath}", ConsoleColor.Green);
                            }
                            else
                            {
                                Console.WriteLine(libraryPath != null ? $"  Current library: {libraryPath}" : "  No library set.");
                            }
                            break;

                        case "output":
                            if (argument.Length > 0)
                            {
                                outputDir = argument;
                                WriteColour($"  Output directory set to: {outputDir}", ConsoleColor.Green);
                            }
                            else
                            {
                                Console.WriteLine($"  Current output: {outputDir}");
                            }
                            break;

                        case "size":
                            if (TryParseSize(argument, out int w, out int h))
                            {
                                width = w; height = h;
                                WriteColour($"  Render size set to: {width}x{height}", ConsoleColor.Green);
                            }
                            else
                            {
                                Console.WriteLine($"  Current size: {width}x{height}  (use 'size 800x600')");
                            }
                            break;

                        default:
                            // If input looks like a part number, treat it as a search
                            CmdSearch(db, trimmed);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    WriteColour($"  Error: {ex.Message}", ConsoleColor.Red);
                }

                Console.WriteLine();
            }

            return 0;
        }

        // ── Search ──

        static void CmdSearch(BMCContext db, string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                Console.WriteLine("  Usage: search <term>  (e.g. 'search brick 2x4')");
                return;
            }

            string pattern = $"%{term}%";

            var results = db.BrickParts
                .Where(p => p.active && !p.deleted &&
                    (EF.Functions.Like(p.ldrawTitle, pattern) ||
                     EF.Functions.Like(p.name, pattern) ||
                     EF.Functions.Like(p.ldrawCategory, pattern) ||
                     EF.Functions.Like(p.keywords, pattern) ||
                     EF.Functions.Like(p.ldrawPartId, pattern)))
                .OrderBy(p => p.ldrawTitle)
                .Take(25)
                .Select(p => new { p.name, p.ldrawPartId, p.ldrawTitle, p.ldrawCategory })
                .ToList();

            if (results.Count == 0)
            {
                WriteColour($"  No parts found for '{term}'", ConsoleColor.Yellow);
                return;
            }

            Console.WriteLine();
            WriteColour($"  Found {results.Count} result(s):", ConsoleColor.White);
            Console.WriteLine();

            // Table header
            WriteColour($"  {"Part #",-15} {"Title",-45} {"Category",-15}", ConsoleColor.DarkCyan);
            WriteColour($"  {"─────",-15} {"─────",-45} {"────────",-15}", ConsoleColor.DarkGray);

            foreach (var r in results)
            {
                string title = r.ldrawTitle ?? "";
                if (title.Length > 44) title = title.Substring(0, 41) + "...";
                string cat = r.ldrawCategory ?? "";

                Console.Write("  ");
                WriteColour($"{r.name,-15}", ConsoleColor.White, newLine: false);
                Console.Write($" {title,-45} ");
                WriteColour($"{cat,-15}", ConsoleColor.DarkGray);
            }
        }

        // ── List ──

        static void CmdList(BMCContext db, string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                // List all categories
                var categories = db.BrickParts
                    .Where(p => p.active && !p.deleted && p.ldrawCategory != null)
                    .GroupBy(p => p.ldrawCategory)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count)
                    .Take(40)
                    .ToList();

                Console.WriteLine();
                WriteColour($"  Top categories ({categories.Count}):", ConsoleColor.White);
                Console.WriteLine();

                WriteColour($"  {"Category",-25} {"Parts",-8}", ConsoleColor.DarkCyan);
                WriteColour($"  {"────────",-25} {"─────",-8}", ConsoleColor.DarkGray);

                foreach (var c in categories)
                {
                    Console.Write("  ");
                    WriteColour($"{c.Category,-25}", ConsoleColor.White, newLine: false);
                    Console.WriteLine($" {c.Count,-8}");
                }

                Console.WriteLine();
                WriteColour("  Use 'list <category>' to see parts in a category.", ConsoleColor.DarkGray);
            }
            else
            {
                // List parts in a category
                string pattern = $"%{argument}%";
                var results = db.BrickParts
                    .Where(p => p.active && !p.deleted && EF.Functions.Like(p.ldrawCategory, pattern))
                    .OrderBy(p => p.ldrawTitle)
                    .Take(30)
                    .Select(p => new { p.name, p.ldrawPartId, p.ldrawTitle })
                    .ToList();

                if (results.Count == 0)
                {
                    WriteColour($"  No parts found in category '{argument}'", ConsoleColor.Yellow);
                    return;
                }

                Console.WriteLine();
                WriteColour($"  Parts in '{argument}' ({results.Count}):", ConsoleColor.White);
                Console.WriteLine();

                WriteColour($"  {"Part #",-15} {"Title",-55}", ConsoleColor.DarkCyan);
                WriteColour($"  {"─────",-15} {"─────",-55}", ConsoleColor.DarkGray);

                foreach (var r in results)
                {
                    string title = r.ldrawTitle ?? "";
                    if (title.Length > 54) title = title.Substring(0, 51) + "...";
                    Console.Write("  ");
                    WriteColour($"{r.name,-15}", ConsoleColor.White, newLine: false);
                    Console.WriteLine($" {title,-55}");
                }
            }
        }

        // ── Info ──

        static void CmdInfo(BMCContext db, string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                Console.WriteLine("  Usage: info <partNumber>  (e.g. 'info 3001')");
                return;
            }

            var part = db.BrickParts
                .Include(p => p.brickCategory)
                .Include(p => p.partType)
                .FirstOrDefault(p => p.name == argument || p.ldrawPartId == argument || p.ldrawPartId == argument + ".dat");

            if (part == null)
            {
                WriteColour($"  Part '{argument}' not found.", ConsoleColor.Yellow);
                return;
            }

            Console.WriteLine();
            WriteColour($"  ── {part.ldrawTitle ?? part.name} ──", ConsoleColor.White);
            Console.WriteLine();
            Console.WriteLine($"  Part Number:    {part.name}");
            Console.WriteLine($"  LDraw ID:       {part.ldrawPartId}");
            Console.WriteLine($"  Title:          {part.ldrawTitle}");
            Console.WriteLine($"  Category:       {part.ldrawCategory}");
            Console.WriteLine($"  Part Type:      {part.partType?.name}");
            Console.WriteLine($"  Brick Category: {part.brickCategory?.name}");
            Console.WriteLine($"  Author:         {part.author}");

            if (part.widthLdu.HasValue || part.heightLdu.HasValue || part.depthLdu.HasValue)
            {
                Console.WriteLine($"  Dimensions:     {part.widthLdu ?? 0} x {part.heightLdu ?? 0} x {part.depthLdu ?? 0} LDU");
            }
            if (part.massGrams.HasValue)
            {
                Console.WriteLine($"  Mass:           {part.massGrams.Value:F2}g");
            }
            if (!string.IsNullOrEmpty(part.keywords))
            {
                Console.WriteLine($"  Keywords:       {part.keywords}");
            }

            // Show available colours
            int colourCount = db.BrickPartColours.Count(bpc => bpc.brickPartId == part.id && bpc.active && !bpc.deleted);
            if (colourCount > 0)
            {
                Console.WriteLine($"  Known colours:  {colourCount}  (use 'colours {part.name}' to see them)");
            }
        }

        // ── Colours ──

        static void CmdColours(BMCContext db, string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                Console.WriteLine("  Usage: colours <partNumber>  (e.g. 'colours 3001')");
                return;
            }

            var part = db.BrickParts.FirstOrDefault(p => p.name == argument || p.ldrawPartId == argument || p.ldrawPartId == argument + ".dat");
            if (part == null)
            {
                WriteColour($"  Part '{argument}' not found.", ConsoleColor.Yellow);
                return;
            }

            var colours = db.BrickPartColours
                .Where(bpc => bpc.brickPartId == part.id && bpc.active && !bpc.deleted)
                .Include(bpc => bpc.brickColour)
                .OrderBy(bpc => bpc.brickColour.name)
                .Select(bpc => bpc.brickColour)
                .ToList();

            if (colours.Count == 0)
            {
                WriteColour($"  No known colours for part {part.name}.", ConsoleColor.Yellow);
                return;
            }

            Console.WriteLine();
            WriteColour($"  Colours for {part.name} — {part.ldrawTitle} ({colours.Count}):", ConsoleColor.White);
            Console.WriteLine();

            WriteColour($"  {"Code",-6} {"Name",-30} {"Hex",-10} {"Finish",-12}", ConsoleColor.DarkCyan);
            WriteColour($"  {"────",-6} {"────",-30} {"───",-10} {"──────",-12}", ConsoleColor.DarkGray);

            foreach (var c in colours)
            {
                Console.Write("  ");
                WriteColour($"{c.ldrawColourCode,-6}", ConsoleColor.White, newLine: false);
                Console.Write($" {c.name,-30} ");
                WriteColour($"{c.hexRgb,-10}", ConsoleColor.DarkGray, newLine: false);
                Console.Write($" ");

                // Show finish type
                string finish = c.colourFinish != null ? "" : (c.isTransparent ? "Transparent" : (c.isMetallic ? "Metallic" : "Solid"));
                Console.WriteLine($"{finish,-12}");
            }

            Console.WriteLine();
            WriteColour($"  Use 'render {part.name} <colourCode>' to render in a specific colour.", ConsoleColor.DarkGray);
        }

        // ── Render ──

        static void CmdRender(BMCContext db, string argument, string libraryPath, string outputDir, int width, int height)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                Console.WriteLine("  Usage: render <partNumber> [colourCode] [options]");
                Console.WriteLine("  Options: --format png|webp|svg|gif  --edges  --no-edges  --smooth  --no-smooth");
                Console.WriteLine("           --aa 2x|4x  --bg #hex  --explode <factor>");
                return;
            }

            string[] renderArgs = argument.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string partNum = renderArgs[0];
            int colourCode = -1;
            string format = "png";
            bool renderEdges = true;
            bool smoothShading = true;
            string aaStr = "none";
            string bgHex = null;
            float explodeFactor = 0f;

            // Parse positional colour code and flags
            int flagStart = 1;
            if (renderArgs.Length > 1 && int.TryParse(renderArgs[1], out int parsedColour))
            {
                colourCode = parsedColour;
                flagStart = 2;
            }

            for (int f = flagStart; f < renderArgs.Length; f++)
            {
                string flag = renderArgs[f].ToLowerInvariant();
                switch (flag)
                {
                    case "--format":
                        if (f + 1 < renderArgs.Length) { format = renderArgs[++f].ToLowerInvariant(); }
                        break;
                    case "--edges": renderEdges = true; break;
                    case "--no-edges": renderEdges = false; break;
                    case "--smooth": smoothShading = true; break;
                    case "--no-smooth": smoothShading = false; break;
                    case "--aa":
                        if (f + 1 < renderArgs.Length) { aaStr = renderArgs[++f].ToLowerInvariant(); }
                        break;
                    case "--bg":
                        if (f + 1 < renderArgs.Length) { bgHex = renderArgs[++f]; }
                        break;
                    case "--explode":
                        if (f + 1 < renderArgs.Length) { float.TryParse(renderArgs[++f], out explodeFactor); }
                        break;
                }
            }

            // Look up part in DB
            var part = db.BrickParts.FirstOrDefault(p => p.name == partNum || p.ldrawPartId == partNum || p.ldrawPartId == partNum + ".dat");
            if (part == null)
            {
                WriteColour($"  Part '{partNum}' not found in database.", ConsoleColor.Yellow);
                return;
            }

            if (libraryPath == null)
            {
                WriteColour("  ✗ LDraw library path not set. Use 'library <path>' first.", ConsoleColor.Red);
                return;
            }

            // Resolve the .dat file path
            string ldrawPartId = part.ldrawPartId;
            if (!ldrawPartId.EndsWith(".dat", StringComparison.OrdinalIgnoreCase))
            {
                ldrawPartId += ".dat";
            }

            string datPath = FindPartFile(libraryPath, ldrawPartId);
            if (datPath == null)
            {
                WriteColour($"  ✗ Could not find {ldrawPartId} in library at {libraryPath}", ConsoleColor.Red);
                return;
            }

            // Determine extension and output
            string ext = format == "webp" ? ".webp" : format == "svg" ? ".svg" : format == "gif" ? ".gif" : ".png";
            string colourSuffix = colourCode >= 0 ? $"_c{colourCode}" : "";
            string outputFile = Path.Combine(outputDir, $"{part.name}{colourSuffix}{ext}");

            Console.WriteLine($"  Rendering {part.ldrawTitle ?? part.name}...");
            Console.WriteLine($"  File:   {datPath}");
            Console.WriteLine($"  Output: {outputFile}");
            Console.WriteLine($"  Size:   {width}x{height}  Format: {format.ToUpper()}");

            AntiAliasMode aaMode = AntiAliasMode.None;
            if (aaStr == "2x") aaMode = AntiAliasMode.SSAA2x;
            else if (aaStr == "4x") aaMode = AntiAliasMode.SSAA4x;

            Stopwatch sw = Stopwatch.StartNew();
            RenderService service = new RenderService(libraryPath);

            if (format == "gif")
            {
                byte[] gif = service.RenderTurntableGif(datPath, width, height, colourCode, renderEdges: renderEdges, smoothShading: smoothShading);
                File.WriteAllBytes(outputFile, gif);
            }
            else if (format == "svg")
            {
                string svg = service.RenderToSvg(datPath, width, height, colourCode, renderEdges: renderEdges, smoothShading: smoothShading);
                File.WriteAllText(outputFile, svg);
            }
            else if (explodeFactor > 0f)
            {
                byte[] png = service.RenderExplodedView(datPath, explodeFactor, width, height, colourCode, renderEdges: renderEdges, smoothShading: smoothShading, antiAliasMode: aaMode);
                File.WriteAllBytes(outputFile, png);
            }
            else if (format == "webp")
            {
                byte[] webp = service.RenderToWebP(datPath, width, height, colourCode, renderEdges: renderEdges, smoothShading: smoothShading, antiAliasMode: aaMode, backgroundHex: bgHex);
                File.WriteAllBytes(outputFile, webp);
            }
            else
            {
                byte[] png = service.RenderToPng(datPath, width, height, colourCode, renderEdges: renderEdges, smoothShading: smoothShading, antiAliasMode: aaMode, backgroundHex: bgHex);
                File.WriteAllBytes(outputFile, png);
            }

            sw.Stop();

            FileInfo fi = new FileInfo(outputFile);
            WriteColour($"  ✓ Rendered in {sw.ElapsedMilliseconds}ms — {fi.Length / 1024}KB", ConsoleColor.Green);
        }

        /// <summary>
        /// Find a .dat file within the LDraw library directory structure.
        /// </summary>
        static string FindPartFile(string libraryPath, string fileName)
        {
            string[] searchDirs = new string[]
            {
                Path.Combine(libraryPath, "parts"),
                Path.Combine(libraryPath, "p"),
                Path.Combine(libraryPath, "models"),
                Path.Combine(libraryPath, "parts", "s"),
                libraryPath,
            };

            foreach (string dir in searchDirs)
            {
                string fullPath = Path.Combine(dir, fileName);
                if (File.Exists(fullPath)) return fullPath;
            }

            return null;
        }

        // ── Help ──

        static void PrintHelp()
        {
            Console.WriteLine();
            WriteColour("  Commands:", ConsoleColor.White);
            Console.WriteLine();
            Console.WriteLine("  search <term>            Search parts by title, category, keywords, or number");
            Console.WriteLine("  list                     Show top categories");
            Console.WriteLine("  list <category>          Show parts in a category");
            Console.WriteLine("  info <partNumber>        Show detailed info for a part");
            Console.WriteLine("  colours <partNumber>     Show available colours for a part");
            Console.WriteLine("  render <part> [colour]   Render a part (see options below)");
            Console.WriteLine("  library <path>           Set the LDraw library path");
            Console.WriteLine("  output <path>            Set the PNG output directory");
            Console.WriteLine("  size <WxH>               Set render dimensions (e.g. 'size 800x600')");
            Console.WriteLine("  help                     Show this help");
            Console.WriteLine("  quit                     Exit");
            Console.WriteLine();
            WriteColour("  Render Options:", ConsoleColor.White);
            Console.WriteLine();
            Console.WriteLine("  --format png|webp|svg|gif   Output format (default: png)");
            Console.WriteLine("  --edges / --no-edges        Edge rendering (default: on)");
            Console.WriteLine("  --smooth / --no-smooth      Smooth shading (default: on)");
            Console.WriteLine("  --aa 2x|4x                  Anti-aliasing (SSAA)");
            Console.WriteLine("  --bg #hex                    Background colour");
            Console.WriteLine("  --explode <factor>           Exploded view (e.g. 1.0)");
            Console.WriteLine();
            WriteColour("  Examples:", ConsoleColor.White);
            Console.WriteLine();
            Console.WriteLine("  render 3001                   Render part 3001 in default colour");
            Console.WriteLine("  render 3001 4 --format webp   Render as WebP");
            Console.WriteLine("  render 3001 4 --format gif    Render turntable GIF");
            Console.WriteLine("  render 3001 4 --explode 1.5   Render exploded view");
            Console.WriteLine("  render 3001 4 --aa 4x --bg #334455");
        }

        static void PrintBanner()
        {
            Console.WriteLine();
            WriteColour("  ╔══════════════════════════════════════════╗", ConsoleColor.Cyan);
            WriteColour("  ║   BMC LDraw Renderer — Interactive CLI   ║", ConsoleColor.Cyan);
            WriteColour("  ╚══════════════════════════════════════════╝", ConsoleColor.Cyan);
            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════════
        // Direct mode (original behaviour — render a specific file)
        // ════════════════════════════════════════════════════════════════

        static int RunDirect(string[] args)
        {
            string inputFile = args[0];
            string outputFile = args[1];
            int width = 512;
            int height = 512;
            string libraryPath = null;
            int colourCode = -1;
            string format = null;
            bool renderEdges = true;
            bool smoothShading = true;
            string aaStr = "none";
            string bgHex = null;
            float explodeFactor = 0f;
            RendererType rendererType = RendererType.Rasterizer;

            for (int i = 2; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--width":
                    case "-w":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int w))
                        { width = w; i++; }
                        break;
                    case "--height":
                    case "-h":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int h))
                        { height = h; i++; }
                        break;
                    case "--library":
                    case "-l":
                        if (i + 1 < args.Length)
                        { libraryPath = args[i + 1]; i++; }
                        break;
                    case "--colour":
                    case "--color":
                    case "-c":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int c))
                        { colourCode = c; i++; }
                        break;
                    case "--format":
                        if (i + 1 < args.Length)
                        { format = args[i + 1].ToLowerInvariant(); i++; }
                        break;
                    case "--edges": renderEdges = true; break;
                    case "--no-edges": renderEdges = false; break;
                    case "--smooth": smoothShading = true; break;
                    case "--no-smooth": smoothShading = false; break;
                    case "--aa":
                        if (i + 1 < args.Length)
                        { aaStr = args[i + 1].ToLowerInvariant(); i++; }
                        break;
                    case "--bg":
                        if (i + 1 < args.Length)
                        { bgHex = args[i + 1]; i++; }
                        break;
                    case "--explode":
                        if (i + 1 < args.Length)
                        { float.TryParse(args[i + 1], out explodeFactor); i++; }
                        break;
                    case "--renderer":
                        if (i + 1 < args.Length)
                        {
                            string rt = args[i + 1].ToLowerInvariant();
                            if (rt == "raytrace" || rt == "raytracer" || rt == "rt")
                                rendererType = RendererType.RayTracer;
                            i++;
                        }
                        break;
                }
            }

            if (!File.Exists(inputFile))
            {
                Console.Error.WriteLine($"Error: Input file not found: {inputFile}");
                return 1;
            }

            if (libraryPath == null) libraryPath = TryFindLibrary(inputFile);
            if (libraryPath == null)
            {
                Console.Error.WriteLine("Error: Could not locate LDraw library. Use --library to specify the path.");
                return 1;
            }

            // Auto-detect format from output extension if not specified
            if (format == null)
            {
                string ext = Path.GetExtension(outputFile).ToLowerInvariant();
                if (ext == ".webp") format = "webp";
                else if (ext == ".svg") format = "svg";
                else if (ext == ".gif") format = "gif";
                else format = "png";
            }

            AntiAliasMode aaMode = AntiAliasMode.None;
            if (aaStr == "2x") aaMode = AntiAliasMode.SSAA2x;
            else if (aaStr == "4x") aaMode = AntiAliasMode.SSAA4x;

            Console.WriteLine($"Input:    {inputFile}");
            Console.WriteLine($"Output:   {outputFile}");
            Console.WriteLine($"Size:     {width}x{height}");
            Console.WriteLine($"Format:   {format.ToUpper()}");
            Console.WriteLine($"Library:  {libraryPath}");
            if (rendererType == RendererType.RayTracer)
                Console.WriteLine($"Renderer: Ray Tracer");

            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                RenderService service = new RenderService(libraryPath);

                if (format == "gif")
                {
                    byte[] gif = service.RenderTurntableGif(inputFile, width, height, colourCode, renderEdges: renderEdges, smoothShading: smoothShading);
                    File.WriteAllBytes(outputFile, gif);
                }
                else if (format == "svg")
                {
                    string svg = service.RenderToSvg(inputFile, width, height, colourCode, renderEdges: renderEdges, smoothShading: smoothShading);
                    File.WriteAllText(outputFile, svg);
                }
                else if (explodeFactor > 0f)
                {
                    byte[] png = service.RenderExplodedView(inputFile, explodeFactor, width, height, colourCode, renderEdges: renderEdges, smoothShading: smoothShading, antiAliasMode: aaMode);
                    File.WriteAllBytes(outputFile, png);
                }
                else if (format == "webp")
                {
                    byte[] webp = service.RenderToWebP(inputFile, width, height, colourCode, renderEdges: renderEdges, smoothShading: smoothShading, antiAliasMode: aaMode, backgroundHex: bgHex);
                    File.WriteAllBytes(outputFile, webp);
                }
                else
                {
                    byte[] png = service.RenderToPng(inputFile, width, height, colourCode, renderEdges: renderEdges, smoothShading: smoothShading, antiAliasMode: aaMode, backgroundHex: bgHex, rendererType: rendererType);
                    File.WriteAllBytes(outputFile, png);
                }

                sw.Stop();

                FileInfo fi = new FileInfo(outputFile);
                Console.WriteLine($"Done in {sw.ElapsedMilliseconds}ms — {fi.Length / 1024}KB {format.ToUpper()}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
                return 1;
            }
        }

        // ── Utility ──

        static bool TryParseSize(string s, out int w, out int h)
        {
            w = 0; h = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;
            string[] parts = s.ToLowerInvariant().Split('x');
            if (parts.Length != 2) return false;
            return int.TryParse(parts[0], out w) && int.TryParse(parts[1], out h) && w > 0 && h > 0;
        }

        static string TryFindLibrary(string inputFile)
        {
            // Walk up from input file if provided
            if (inputFile != null)
            {
                string dir = Path.GetDirectoryName(Path.GetFullPath(inputFile));
                while (dir != null)
                {
                    if (Directory.Exists(Path.Combine(dir, "parts")) && Directory.Exists(Path.Combine(dir, "p")))
                        return dir;
                    string ldrawDir = Path.Combine(dir, "ldraw");
                    if (Directory.Exists(ldrawDir) && Directory.Exists(Path.Combine(ldrawDir, "parts")))
                        return ldrawDir;
                    dir = Path.GetDirectoryName(dir);
                }
            }

            // Common locations
            string[] commonPaths = new string[]
            {
                @"C:\ldraw",
                @"D:\ldraw",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ldraw"),
            };

            foreach (string path in commonPaths)
            {
                if (Directory.Exists(path) && Directory.Exists(Path.Combine(path, "parts")))
                    return path;
            }

            return null;
        }

        static void WriteColour(string text, ConsoleColor colour, bool newLine = true)
        {
            ConsoleColor prev = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            if (newLine) Console.WriteLine(text);
            else Console.Write(text);
            Console.ForegroundColor = prev;
        }
    }
}
