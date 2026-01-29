using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Foundation.Security;
using Foundation.Security.Database;


namespace Foundation.Tools
{
    /// <summary>
    /// 
    /// The purpose of this program is to create baseline code for FoundationCore applications
    /// 
    /// Each time the foundation schemas change, this codes needs to be rebuilt.
    /// 
    /// It also provides user password reset functionality.
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
                        GenerateFoundationDatabaseScriptCode();
                        break;

                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        GenerateFoundationApplicationCode();
                        break;

                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        ChangeUserPassword();
                        break;

                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        //
                        // Uses configuration from app settings
                        // 
                        GenerateDatabaseGenerator();
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
            Console.WriteLine("=== Foundation Code Generation Menu ===");
            Console.WriteLine("1. Option 1 - Generation Foundation Database Scripts");
            Console.WriteLine("2. Option 2 - Generation Foundation Application Entity Code");
            Console.WriteLine("3. Option 3 - Change User Password (And unlock them if necessary)");
            Console.WriteLine("4. Option 4 - Create Starting Point DatabaseScriptGenerator source file from existing database (config in appsettings.json).");
            Console.WriteLine("X. Option X - Exit");
            Console.WriteLine();
            Console.Write("Please select an option (1-4, or X): ");
        }


        private static void GenerateFoundationApplicationCode()
        {
            string outputFolder = Path.Combine(_outputDirectory, "FoundationEntityCode");

            Foundation.CodeGeneration.CodeGeneratorUtility.GenerateFoundationEntityCode(outputFolder);

            Console.WriteLine();
            Console.WriteLine("Foundation application code created in folder: " + outputFolder);
            Console.WriteLine();

            //
            // Auto-deploy logic
            //
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

                   
                var config = builder.Build();

                var deploymentPaths = config.GetSection("DeploymentPaths").Get<Dictionary<string, string>>();

                if (deploymentPaths != null && deploymentPaths.Count > 0)
                {
                    bool deploySuccess = Foundation.CodeGeneration.DeploymentUtility.PromptAndDeploy(_outputDirectory, deploymentPaths);

                    if (deploySuccess)
                    {
                         string angularAppRoot = config["AngularAppRoot"];
                         string angularModuleFile = config["AngularModuleFile"] ?? "app.module.ts";
                         string angularRoutingFile = config["AngularRoutingFile"] ?? "app-routing.module.ts";

                         if (!string.IsNullOrEmpty(angularAppRoot))
                         {
                             // Foundation generates into a subfolder "FoundationEntityCode"
                             string foundationOutputRoot = Path.Combine(_outputDirectory, "FoundationEntityCode");

                             // Security
                             Foundation.CodeGeneration.AngularAutomationUtility.IntegrateGeneratedCode(
                                 Directory.GetCurrentDirectory(),
                                 "Security",
                                 foundationOutputRoot,
                                 angularAppRoot,
                                 angularModuleFile,
                                 angularRoutingFile
                             );

                             // Auditor
                             Foundation.CodeGeneration.AngularAutomationUtility.IntegrateGeneratedCode(
                                 Directory.GetCurrentDirectory(),
                                 "Auditor",
                                 foundationOutputRoot,
                                 angularAppRoot,
                                 angularModuleFile,
                                 angularRoutingFile
                             );

                             // Telemetry
                             Foundation.CodeGeneration.AngularAutomationUtility.IntegrateGeneratedCode(
                                 Directory.GetCurrentDirectory(),
                                 "Telemetry",
                                 foundationOutputRoot,
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


        private static void GenerateFoundationDatabaseScriptCode()
        {
            string outputFolder = Path.Combine(_outputDirectory, "FoundationDatabaseScripts");

            Foundation.CodeGeneration.CodeGeneratorUtility.GenerateFoundationDatabaseScripts(outputFolder);

            Console.WriteLine();
            Console.WriteLine("Foundation database scripts created in folder: " + outputFolder);
            Console.WriteLine();

            Process.Start("explorer.exe", outputFolder);
        }

        private static void ChangeUserPassword()
        {

            using (SecurityContext context = new SecurityContext())
            {
                Console.Write("Enter Username: ");
                string userName = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(userName))
                {
                    Console.WriteLine("Invalid username. Please try again.");
                    Console.WriteLine();
                    return;
                }


                SecurityUser user = (from su in context.SecurityUsers 
                                     where su.accountName.Trim().ToUpper() == userName.Trim().ToUpper() 
                                     select su)
                                     .FirstOrDefault();

                if (user == null)
                {
                    Console.WriteLine("Could not find username.  Please try again.");
                    Console.WriteLine();
                    return;
                }
                

                Console.Write("Enter New Password: ");
                string password = ReadPassword();

                if (string.IsNullOrEmpty(password))
                {
                    Console.WriteLine("Invalid password. Please try again.");
                    Console.WriteLine();
                    return;
                }
                else
                {
                    
                    string hashedNewPassword = SecurityLogic.SecurePasswordHasher.Hash(password);


                    using (var transaction = context.Database.BeginTransaction())
                    {

                        try
                        {
                            SecurityUserEventType suet = (from x in context.SecurityUserEventTypes
                                                          where
                                                          x.name == "Miscellaneous"
                                                          select x)
                                                          .FirstOrDefault();

                            SecurityUserEvent newEvent = new SecurityUserEvent();

                            newEvent.securityUserId = user.id;
                            newEvent.securityUserEventTypeId = suet.id;
                            newEvent.timeStamp = DateTime.UtcNow;
                            newEvent.comments = $"FoundationCoreTools changed password of user, and reset their failedLoginCount, active, and deleted values.  Previous hash is {user.password} and current hash is {hashedNewPassword}";
                            newEvent.active = true;
                            newEvent.deleted = false;

                            context.SecurityUserEvents.Add(newEvent);

                            //
                            // Reset the user
                            //
                            user.password = hashedNewPassword;
                            user.failedLoginCount = 0;
                            user.active = true;
                            user.deleted = false;
                          

                            context.SaveChanges();

                            transaction.Commit();

                            Console.WriteLine("Password changed successfully.");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();

                            Console.WriteLine($"Caught error trying to change password.  Details are: {ex.ToString()}");
                        }
                    }
                }
            }
        }

        private static string ReadPassword()
        {
            string password = "";
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;
                if (key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password[0..^1];
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    password += keyInfo.KeyChar;
                    Console.Write("*");
                }
            } while (key != ConsoleKey.Enter);
            Console.WriteLine();
            return password;
        }

        public static void GenerateDatabaseGenerator()
        {
            string outputDirectory = Path.Combine(_outputDirectory, "DatabaseGeneratorGenerator");


            IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

            // Retrieve values from appsettings.json
            string connectionString = config["DatabaseGeneratorGenerator:connectionString"];
            string outputNamespace = config["DatabaseGeneratorGenerator:outputNamespace"];
            string outputClassName = config["DatabaseGeneratorGenerator:outputClassName"];

            var generator = new DatabaseSchemaToCSharpGenerator(connectionString, outputNamespace, outputClassName);


            if (System.IO.Directory.Exists(outputDirectory) == false)
            {
                System.IO.Directory.CreateDirectory(outputDirectory);
            }

            string outputFile = System.IO.Path.Combine(outputDirectory, outputClassName + ".cs");

            generator.GenerateSchemaFile(outputFile);

            Console.WriteLine();
            Console.WriteLine($"Generated database schema file at: {outputFile}");
            Console.WriteLine();

            Process.Start("explorer.exe", outputDirectory);
        }
    }
}



