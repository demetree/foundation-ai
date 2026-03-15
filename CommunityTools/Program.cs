using Foundation.Community.Database;
using Foundation.CodeGeneration;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Foundation.Community.CodeGeneration
{
    /// <summary>
    /// 
    /// The purpose of this program is to create baseline code for the Community CMS system.
    /// 
    /// The entity model needs to be created using the EF Core Power Tools.
    /// 
    /// Each time the schema changes, this needs to be rebuilt.
    /// 
    /// 
    /// General comments about code generation:
    /// 
    /// 1.) The schema creation will create script files for 4 types of databases.  The relevant ones should be copied into the Community main project so that they are easy to find, and will be source controlled.
    ///     The copy in the Community project should be updated whenever the Community schema changes.
    /// 
    /// 2.) The application code generator creates 2 things.  
    /// 
    ///     The first is a bunch of API controllers that need to go into the Community project.
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
                        GenerateCommunityDatabaseScriptCode();
                        break;

                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        GenerateCommunityApplicationCode();
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
            Console.WriteLine("=== Community CMS Tools ===");
            Console.WriteLine();
            Console.WriteLine("1. Generate Community Database Script Code");
            Console.WriteLine("2. Generate Community Application Code");
            Console.WriteLine();
            Console.WriteLine("X. Exit");
            Console.WriteLine();
            Console.Write("Selection: ");
        }


        private static void GenerateCommunityDatabaseScriptCode()
        {
            Console.WriteLine();
            Console.WriteLine("Generating Community Database Script Code...");
            Console.WriteLine();

            string outputFolder = Path.Combine(_outputDirectory, "CommunityDatabaseScripts");

            CommunityDatabaseGenerator communityDatabaseGenerator = new CommunityDatabaseGenerator();
            communityDatabaseGenerator.GenerateDatabaseCreationScriptsInFolder(outputFolder);

            Console.WriteLine();
            Console.WriteLine("Community database scripts created in folder: " + outputFolder);
            Console.WriteLine();

            Process.Start("explorer.exe", outputFolder);
        }


        private static void GenerateCommunityApplicationCode()
        {
            Console.WriteLine();
            Console.WriteLine("Generating Community Application Code...");
            Console.WriteLine();

            string outputFolder = Path.Combine(_outputDirectory, "CommunityEntityCode");

            CommunityDatabaseGenerator communityDatabaseGenerator = new CommunityDatabaseGenerator();

            //
            // Create the WebAPI controllers
            //
            Console.WriteLine("Generating WebAPI controllers...");
            CodeGeneratorUtility.GenerateTemplateCodeFromEntityFrameworkContext("CommunityContext", "Community", typeof(Foundation.Community.Database.CommunityContext), communityDatabaseGenerator.database, outputFolder);


            //
            // Create Angular data services to interact with the WebAPI
            //
            Console.WriteLine("Generating Angular data services...");
            AngularServiceGenerator.BuildAngularServiceImplementationFromEntityFrameworkContext("Community", typeof(Foundation.Community.Database.CommunityContext), communityDatabaseGenerator.database, outputFolder);

            //
            // Create Angular Components to interact with the data services
            //
            Console.WriteLine("Generating Angular components...");
            AngularComponentGenerator.BuildAngularComponentImplementationFromEntityFrameworkContext("Community", typeof(Foundation.Community.Database.CommunityContext), communityDatabaseGenerator.database, outputFolder, applicationThemePrefix: "community");


            Console.WriteLine();
            Console.WriteLine("Community application code created in folder: " + outputFolder);
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
                                "Community",
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
