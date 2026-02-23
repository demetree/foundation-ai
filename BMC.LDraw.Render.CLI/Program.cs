using System;
using System.Diagnostics;
using System.IO;
using BMC.LDraw.Render;

namespace BMC.LDraw.Render.CLI
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                PrintUsage();
                return 1;
            }

            string inputFile = args[0];
            string outputFile = args[1];
            int width = 512;
            int height = 512;
            string libraryPath = null;
            int colourCode = -1;

            // Parse optional arguments
            for (int i = 2; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--width":
                    case "-w":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int w))
                        {
                            width = w;
                            i++;
                        }
                        break;

                    case "--height":
                    case "-h":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int h))
                        {
                            height = h;
                            i++;
                        }
                        break;

                    case "--library":
                    case "-l":
                        if (i + 1 < args.Length)
                        {
                            libraryPath = args[i + 1];
                            i++;
                        }
                        break;

                    case "--colour":
                    case "--color":
                    case "-c":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int c))
                        {
                            colourCode = c;
                            i++;
                        }
                        break;
                }
            }

            // Validate input
            if (!File.Exists(inputFile))
            {
                Console.Error.WriteLine($"Error: Input file not found: {inputFile}");
                return 1;
            }

            // Auto-detect library path if not specified
            if (libraryPath == null)
            {
                libraryPath = TryFindLibrary(inputFile);
                if (libraryPath == null)
                {
                    Console.Error.WriteLine("Error: Could not locate LDraw library. Use --library to specify the path.");
                    return 1;
                }
            }

            if (!Directory.Exists(libraryPath))
            {
                Console.Error.WriteLine($"Error: Library directory not found: {libraryPath}");
                return 1;
            }

            // Render
            Console.WriteLine($"Input:    {inputFile}");
            Console.WriteLine($"Output:   {outputFile}");
            Console.WriteLine($"Size:     {width}x{height}");
            Console.WriteLine($"Library:  {libraryPath}");

            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                RenderService service = new RenderService(libraryPath);
                service.RenderToFile(inputFile, outputFile, width, height, colourCode);

                sw.Stop();

                FileInfo fi = new FileInfo(outputFile);
                Console.WriteLine($"Done in {sw.ElapsedMilliseconds}ms — {fi.Length / 1024}KB PNG");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
                return 1;
            }
        }

        /// <summary>
        /// Try to find the LDraw library by walking up from the input file directory.
        /// Looks for a directory containing "parts" and "p" subdirectories.
        /// </summary>
        private static string TryFindLibrary(string inputFile)
        {
            string dir = Path.GetDirectoryName(Path.GetFullPath(inputFile));
            while (dir != null)
            {
                // Check if this directory IS the ldraw library
                if (Directory.Exists(Path.Combine(dir, "parts")) && Directory.Exists(Path.Combine(dir, "p")))
                {
                    return dir;
                }

                // Check for "ldraw" subdirectory
                string ldrawDir = Path.Combine(dir, "ldraw");
                if (Directory.Exists(ldrawDir) && Directory.Exists(Path.Combine(ldrawDir, "parts")))
                {
                    return ldrawDir;
                }

                dir = Path.GetDirectoryName(dir);
            }

            // Check common locations on Windows
            string[] commonPaths = new string[]
            {
                @"C:\ldraw",
                @"D:\ldraw",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ldraw"),
            };

            foreach (string path in commonPaths)
            {
                if (Directory.Exists(path) && Directory.Exists(Path.Combine(path, "parts")))
                {
                    return path;
                }
            }

            return null;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("BMC LDraw Renderer — Headless LDraw-to-PNG");
            Console.WriteLine();
            Console.WriteLine("Usage: BMC.LDraw.Render.CLI <input> <output.png> [options]");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  <input>        Path to .ldr, .mpd, or .dat file");
            Console.WriteLine("  <output.png>   Path for the output PNG image");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --width, -w    Image width in pixels (default: 512)");
            Console.WriteLine("  --height, -h   Image height in pixels (default: 512)");
            Console.WriteLine("  --library, -l  Path to the LDraw parts library root");
            Console.WriteLine("  --colour, -c   LDraw colour code override (default: 4 = red)");
        }
    }
}
