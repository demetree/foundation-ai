using Foundation.CodeGeneration;
using Foundation.DeepSpace.Database;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Foundation.DeepSpace.CodeGeneration
{
    /// <summary>
    /// 
    /// The purpose of this program is to create baseline code for the DeepSpace storage system.
    /// 
    /// The entity model needs to be created using the EF Core Power Tools.
    /// 
    /// Each time the schema changes, this needs to be rebuilt.
    /// 
    /// 
    /// General comments about code generation:
    /// 
    /// 1.) The schema creation will create script files for 4 types of databases.  The relevant ones should be copied into the server project so that they are easy to find, and will be source controlled.
    ///     The copy in the project should be updated whenever the DeepSpace schema changes.
    /// 
    /// 2.) The application code generator creates 2 things.  
    /// 
    ///     The first is a bunch of API controllers that need to go into the server project.
    /// 
    ///     The second is the 'EntityExtensions' output, which needs to be added to the DeepSpaceDatabase project because it is a bunch of partial class extensions to data classes that are in that project.
    ///     
    /// 3.) The database project needs to be referenced by the other projects so that they get the database context.
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
                        GenerateDeepspaceDatabaseScriptCode();
                        break;

                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        GenerateDeepspaceApplicationCode();
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
            Console.WriteLine("=== DeepSpace Code Generation Menu ===");
            Console.WriteLine();
            Console.WriteLine("1. Option 1 - Generate DeepSpace Database Scripts.");
            Console.WriteLine("2. Option 2 - Generate DeepSpace Application Entity Code");
            Console.WriteLine("X. Option X - Exit");
            Console.WriteLine();
            Console.Write("Please select an option (1-2, or X): ");
        }


        static void GenerateDeepspaceDatabaseScriptCode()
        {
            string outputFolder = Path.Combine(_outputDirectory, "DeepspaceDatabaseScripts");

            DeepSpaceDatabaseGenerator generator = new DeepSpaceDatabaseGenerator();
            generator.GenerateDatabaseCreationScriptsInFolder(outputFolder);

            Console.WriteLine();
            Console.WriteLine("DeepSpace database scripts created in folder: " + outputFolder);
            Console.WriteLine();

            Process.Start("explorer.exe", outputFolder);
        }



        static void GenerateDeepspaceApplicationCode()
        {
            //
            // This depends upon the DeepSpaceContext class, and its child data classes to be created in this project, presumably with the EF Core Power Tools 'Code First From Database' tool.
            //
            string outputFolder = Path.Combine(_outputDirectory, "DeepspaceEntityCode");

            DeepSpaceDatabaseGenerator generator = new DeepSpaceDatabaseGenerator();

            //
            // Create the WebAPI controllers
            //
            Foundation.CodeGeneration.CodeGeneratorUtility.GenerateTemplateCodeFromEntityFrameworkContext("DeepSpaceContext", "DeepSpace", typeof(Foundation.DeepSpace.Database.DeepSpaceContext), generator.database, outputFolder);


            //
            // Create Angular data services to interact with the WebAPI
            //
            Foundation.CodeGeneration.AngularServiceGenerator.BuildAngularServiceImplementationFromEntityFrameworkContext("DeepSpace", typeof(Foundation.DeepSpace.Database.DeepSpaceContext), generator.database, outputFolder);

            //
            // Create Angular Components to interact with the data services
            //
            Foundation.CodeGeneration.AngularComponentGenerator.BuildAngularComponentImplementationFromEntityFrameworkContext("DeepSpace", typeof(Foundation.DeepSpace.Database.DeepSpaceContext), generator.database, outputFolder, applicationThemePrefix: "ds");


            Console.WriteLine();
            Console.WriteLine("DeepSpace application code created in folder: " + outputFolder);
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
                        Console.WriteLine("Deployment completed successfully.");
                    }
                }
                else
                {
                    Console.WriteLine("No deployment paths configured. Opening output folder instead.");
                    Process.Start("explorer.exe", outputFolder);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Auto-deploy failed: " + ex.Message);
                Console.WriteLine("You can manually copy the files from: " + outputFolder);
            }
        }
    }
}
