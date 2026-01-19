using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.CodeGeneration
{
    public static class DeploymentUtility
    {
        /// <summary>
        /// Prompts the user to deploy generated files to their destination projects.
        /// Defaults to 'No' if no input is received within the timeout period.
        /// </summary>
        /// <param name="outputRootDirectory">The root directory where generated files are located.</param>
        /// <param name="deploymentMappings">Dictionary where Key is the relative path from outputRoot, and Value is the relative destination path from the executing assembly.</param>
        /// <param name="timeoutSeconds">Time in seconds to wait for user input before defaulting to No.</param>
        public static bool PromptAndDeploy(string outputRootDirectory, Dictionary<string, string> deploymentMappings, int timeoutSeconds = 10)
        {
            if (deploymentMappings == null || deploymentMappings.Count == 0)
            {
                Console.WriteLine("No deployment paths configured.");
                return false;
            }

            Console.WriteLine();
            Console.WriteLine("==================================================================");
            Console.WriteLine($"   Would you like to automatically deploy the generated files?   ");
            Console.WriteLine($"   (Y) Yes / (N) No                                              ");
            Console.WriteLine($"   Defaults to 'No' in {timeoutSeconds} seconds...                             ");
            Console.WriteLine("==================================================================");
            Console.Write("> ");

            if (WaitForYesInput(timeoutSeconds))
            {
                DeployFiles(outputRootDirectory, deploymentMappings);

                return true;
            }
            else
            {
                Console.WriteLine("Deployment skipped.");

                return false;

            }
        }

        private static bool WaitForYesInput(int timeoutSeconds)
        {
            DateTime endTime = DateTime.Now.AddSeconds(timeoutSeconds);
            
            while (DateTime.Now < endTime)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                    if (key.Key == ConsoleKey.Y)
                    {
                        Console.WriteLine("Yes");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("No");
                        // Consume any other keys to clear buffer if needed, or just return false immediately
                        return false;
                    }
                }
                Thread.Sleep(100);
            }
            
            Console.WriteLine();
            Console.WriteLine("Timeout - Defaulting to No.");
            return false;
        }

        private static void DeployFiles(string outputRoot, Dictionary<string, string> mappings)
        {
            Console.WriteLine("Starting deployment...");

            foreach (var mapping in mappings)
            {
                //
                // Source paths must be relative, destination paths to do not need to be.
                //
                string sourceRelativePath = mapping.Key;
                string destPathFromConfig = mapping.Value;

                string sourcePath = Path.Combine(outputRoot, sourceRelativePath);

                //
                // Is the destination path relative?
                //
                string destPath;
                if (destPathFromConfig.StartsWith("..") == true)
                {
                    destPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), destPathFromConfig));
                }
                else
                {
                    destPath = destPathFromConfig;
                }
                
                if (!Directory.Exists(sourcePath))
                {
                    Console.WriteLine($"[SKIP] Source directory not found: {sourcePath}");
                    continue;
                }

                if (!Directory.Exists(destPath))
                {
                    Console.WriteLine($"[WARNING] Destination directory does not exist, creating: {destPath}");
                    Directory.CreateDirectory(destPath);
                }

                Console.WriteLine($"Deploying from '{sourceRelativePath}' to '{destPathFromConfig}'...");

                try
                {
                    //
                    // Note - We are not clearing the destination paths before copying into them because there could be files there that we 
                    // did not create, so we do not want to erase those.
                    //
                    var files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
                    int count = 0;
                    foreach (var file in files)
                    {
                        // Calculate relative path for file to preserve subdirectories if source path is a root of a tree
                        string relativeFile = Path.GetRelativePath(sourcePath, file);
                        string targetFile = Path.Combine(destPath, relativeFile);

                        // Ensure target subdirectory exists
                        string targetDir = Path.GetDirectoryName(targetFile);
                        if (!Directory.Exists(targetDir))
                        {
                            Directory.CreateDirectory(targetDir);
                        }

                        File.Copy(file, targetFile, true);
                        count++;
                    }
                    Console.WriteLine($"  -> Copied {count} files.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  -> [ERROR] Failed to deploy: {ex.Message}");
                }
            }

            Console.WriteLine("Deployment complete.");
        }
    }
}
