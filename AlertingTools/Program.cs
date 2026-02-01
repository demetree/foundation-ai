using DocumentFormat.OpenXml.Spreadsheet;
using Foundation.CodeGeneration;
//using Foundation.Alert.Database;
using Foundation.Security;
using Foundation.Security.Database;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Foundation.Alerting.CodeGeneration
{
    /// <summary>
    /// 
    /// The purpose of this program is to create baseline code for the Alert system
    /// 
    /// The entity model needs to be created using the EF Core Power Tools
    /// 
    /// Each time the schema changes, this needs to be rebuilt.
    /// 
    /// 
    /// General comments about code generation:
    /// 
    /// 1.) The schema creation will create script files for 4 types of databases.  The relevant ones should be copied into the Quarterback main project so that they are easy to find, and will be source controlled.
    ///     The copy in the Quarterback project should be updated whenever the Quarterback schema changes.
    /// 
    /// 2.) The application code generator creates 2 things.  
    /// 
    ///     The first is a bunch of API controllers that need to go into the Quarterback project.
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
                        GenerateAlertingDatabaseScriptCode();
                        break;

                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        GenerateAlertingApplicationCode();
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
            // Display the menu
            Console.WriteLine("=== Alerting Code Generation Menu ===");
            Console.WriteLine();
            Console.WriteLine("1. Option 1 - Generation Alerting Database Scripts.");
            Console.WriteLine("2. Option 2 - Generate Alerting Application Entity Code");
            Console.WriteLine("X. Option X - Exit");
            Console.WriteLine();
            Console.Write("Please select an option (1-3, or X): ");
        }


        static void GenerateAlertingDatabaseScriptCode()
        {
            string outputFolder = Path.Combine(_outputDirectory, "AlertingDatabaseScripts");

            Foundation.Alerting.Database.AlertingDatabaseGenerator AlertDatabaseGenerator = new Foundation.Alerting.Database.AlertingDatabaseGenerator();
            AlertDatabaseGenerator.GenerateDatabaseCreationScriptsInFolder(outputFolder);

            Console.WriteLine();
            Console.WriteLine("Alerting database scripts created in folder: " + outputFolder);
            Console.WriteLine();

            Process.Start("explorer.exe", outputFolder);
        }



        static void GenerateAlertingApplicationCode()
        {
            //
            // This depends upon the AlertContext class, and its child data classes to be created in this project, presumably with the ADO.Net 'Code First From Database' tool.  Namespace adjustments and file moves may be necessary.
            //
            string outputFolder = Path.Combine(_outputDirectory, "AlertingEntityCode");

            Foundation.Alerting.Database.AlertingDatabaseGenerator alertingDatabaseGenerator = new Foundation.Alerting.Database.AlertingDatabaseGenerator();

            //
            // Create the WebAPI controllers - Note that we are ignoring the foundation services for Alert controllers
            //
            Foundation.CodeGeneration.CodeGeneratorUtility.GenerateTemplateCodeFromEntityFrameworkContext("AlertingContext", "Alerting", typeof(Foundation.Alerting.Database.AlertingContext), alertingDatabaseGenerator.database, outputFolder);


            //
            // Create Angular data services to interact with the WebAPI.  Roller Ops does not use authorization.
            //
            Foundation.CodeGeneration.AngularServiceGenerator.BuildAngularServiceImplementationFromEntityFrameworkContext("Alerting", typeof(Foundation.Alerting.Database.AlertingContext), alertingDatabaseGenerator.database, outputFolder);

            //
            // Create Angular Components to interact with the data services  Roller Ops does not use authorization.
            //
            Foundation.CodeGeneration.AngularComponentGenerator.BuildAngularComponentImplementationFromEntityFrameworkContext("Alerting", typeof(Foundation.Alerting.Database.AlertingContext), alertingDatabaseGenerator.database, outputFolder);


            Console.WriteLine();
            Console.WriteLine("Alerting application code created in folder: " + outputFolder);
            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine("Alerting application code created in folder: " + outputFolder);
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
                                "Alerting", // Module Name
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


