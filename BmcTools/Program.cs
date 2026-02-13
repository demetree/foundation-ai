using Foundation.BMC.Database;
using Foundation.CodeGeneration;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Foundation.BMC.CodeGeneration
{
    /// <summary>
    /// 
    /// The purpose of this program is to create baseline code for the BMC system.
    /// 
    /// The entity model needs to be created using the EF Core Power Tools.
    /// 
    /// Each time the schema changes, this needs to be rebuilt.
    /// 
    /// 
    /// General comments about code generation:
    /// 
    /// 1.) The schema creation will create script files for 4 types of databases.  The relevant ones should be copied into the BMC main project so that they are easy to find, and will be source controlled.
    ///     The copy in the BMC project should be updated whenever the BMC schema changes.
    /// 
    /// 2.) The application code generator creates 2 things.  
    /// 
    ///     The first is a bunch of API controllers that need to go into the BMC project.
    /// 
    ///     The second is the 'EntityExtensions' output, which needs to be added to this project because it is a bunch of partial class extensions to data classes that are in this project.
    ///     
    /// 3.) The database project needs to be referenced by the other project so that they gets the database context.
    /// 
    /// </summary>
    internal class Program
    {
        private static string _outputDirectory;

        static void Main()
        {
            bool exit = false;

            _outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Output");

            Console.Clear();

            while (!exit)
            {
                ShowMenu();

                // Read user input
                ConsoleKeyInfo key = Console.ReadKey();
                Console.WriteLine();  // Move to the next line after the key press

                // Handle the user's input
                switch (key.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        GenerateBmcDatabaseScriptCode();
                        break;

                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        GenerateBmcApplicationCode();
                        break;

                    case ConsoleKey.X:
                        exit = true;
                        Console.WriteLine("Exiting...");
                        break;

                    default:
                        Console.WriteLine();
                        Console.WriteLine("Invalid selection, please try again.");
                        ShowMenu();
                        break;
                }
            }
        }


        private static void ShowMenu()
        {
            Console.WriteLine();
            Console.WriteLine("=== BMC Tools ===");
            Console.WriteLine();
            Console.WriteLine("1. Generate BMC Database Script Code");
            Console.WriteLine("2. Generate BMC Application Code");
            Console.WriteLine();
            Console.WriteLine("X. Exit");
            Console.WriteLine();
            Console.Write("Selection: ");
        }


        private static void GenerateBmcDatabaseScriptCode()
        {
            Console.WriteLine();
            Console.WriteLine("Generating BMC Database Script Code...");
            Console.WriteLine();

            string outputFolder = Path.Combine(_outputDirectory, "BmcDatabaseScripts");

            Foundation.BMC.Database.BmcDatabaseGenerator bmcDatabaseGenerator = new Foundation.BMC.Database.BmcDatabaseGenerator();
            bmcDatabaseGenerator.GenerateDatabaseCreationScriptsInFolder(outputFolder);

            Console.WriteLine();
            Console.WriteLine("BMC database scripts created in folder: " + outputFolder);
            Console.WriteLine();

            System.Diagnostics.Process.Start("explorer.exe", outputFolder);
        }


        private static void GenerateBmcApplicationCode()
        {
            Console.WriteLine();
            Console.WriteLine("Generating BMC Application Code...");
            Console.WriteLine();

            string outputFolder = Path.Combine(_outputDirectory, "BmcEntityCode");

            Foundation.BMC.Database.BmcDatabaseGenerator bmcDatabaseGenerator = new Foundation.BMC.Database.BmcDatabaseGenerator();

            //
            // Create the WebAPI controllers
            //
            Foundation.CodeGeneration.CodeGeneratorUtility.GenerateTemplateCodeFromEntityFrameworkContext("BMCContext", "BMC", typeof(Foundation.BMC.Database.BMCContext), bmcDatabaseGenerator.database, outputFolder);


            //
            // Create Angular data services to interact with the WebAPI
            //
            Foundation.CodeGeneration.AngularServiceGenerator.BuildAngularServiceImplementationFromEntityFrameworkContext("BMC", typeof(Foundation.BMC.Database.BMCContext), bmcDatabaseGenerator.database, outputFolder);

            //
            // Create Angular Components to interact with the data services
            //
            Foundation.CodeGeneration.AngularComponentGenerator.BuildAngularComponentImplementationFromEntityFrameworkContext("BMC", typeof(Foundation.BMC.Database.BMCContext), bmcDatabaseGenerator.database, outputFolder);


            Console.WriteLine();
            Console.WriteLine("BMC application code created in folder: " + outputFolder);
            Console.WriteLine();

            //
            // Auto-deploy logic
            //
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                    .Build();

                var deploymentPaths = config.GetSection("DeploymentPaths").Get<Dictionary<string, string>>();

                if (deploymentPaths != null && deploymentPaths.Count > 0)
                {
                    bool deploySuccess = DeploymentUtility.PromptAndDeploy(_outputDirectory, deploymentPaths);

                    if (deploySuccess)
                    {
                        //
                        // If deployment was successful, try to automate the Angular module integration
                        //
                        string angularAppRoot = config["AngularAppRoot"];
                        string angularModuleFile = config["AngularModuleFile"] ?? "app.module.ts";
                        string angularRoutingFile = config["AngularRoutingFile"] ?? "app-routing.module.ts";

                        if (!string.IsNullOrEmpty(angularAppRoot))
                        {
                            Console.WriteLine("Automating Angular module integration...");
                            AngularAutomationUtility.IntegrateGeneratedCode(
                                Directory.GetCurrentDirectory(),
                                "BMC",
                                outputFolder,
                                angularAppRoot,
                                angularModuleFile,
                                angularRoutingFile
                            );
                        }
                    }
                    else
                    {
                        Process.Start("explorer.exe", outputFolder);
                    }
                }
                else
                {
                    Process.Start("explorer.exe", outputFolder);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading configuration or deploying: {ex.Message}");
                Process.Start("explorer.exe", outputFolder);
            }
        }
    }
}
