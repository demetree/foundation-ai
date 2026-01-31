using System;

namespace Foundation.CodeGeneration
{
    /// <summary>
    /// This class provides standard Foundation module script generation.  It is platform specific because it references code generation unique to the Core version of the Foundation.
    /// </summary>
    public class CodeGeneratorUtility
    {
        public static void GenerateFoundationDatabaseScripts(string folderPath = null)
        {
            if (folderPath == null)
            {
                folderPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "FoundationDatabaseScripts");
            }

            //
            // Create the Foundation's Auditor, Security, and Telemetry system create database scripts
            // 
            Foundation.Auditor.Database.AuditorDatabaseGenerator auditorScriptGenerator = new Foundation.Auditor.Database.AuditorDatabaseGenerator();
            Foundation.Security.Database.SecurityDatabaseGenerator securityScriptGenerator = new Foundation.Security.Database.SecurityDatabaseGenerator();
            Foundation.Security.Database.OIDCDatabaseGenerator oidcScriptGenerator = new Foundation.Security.Database.OIDCDatabaseGenerator();
            Foundation.Telemetry.Database.TelemetryDatabaseGenerator telemetryScriptGenerator = new Foundation.Telemetry.Database.TelemetryDatabaseGenerator();


            auditorScriptGenerator.GenerateDatabaseCreationScriptsInFolder(folderPath);
            securityScriptGenerator.GenerateDatabaseCreationScriptsInFolder(folderPath);
            oidcScriptGenerator.GenerateDatabaseCreationScriptsInFolder(folderPath, false, "OIDC");
            telemetryScriptGenerator.GenerateDatabaseCreationScriptsInFolder(folderPath);

            return;
        }



        public static void GenerateFoundationEntityCode(string folderPath = null)
        {
            if (folderPath == null)
            {
                folderPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "FoundationEntityCode");
            }

            //
            // Create the Foundation's Auditor, Security, and Telemetry database generators
            // 
            Foundation.Auditor.Database.AuditorDatabaseGenerator auditorGenerator = new Foundation.Auditor.Database.AuditorDatabaseGenerator();
            Foundation.Security.Database.SecurityDatabaseGenerator securityGenerator = new Foundation.Security.Database.SecurityDatabaseGenerator();
            Foundation.Telemetry.Database.TelemetryDatabaseGenerator telemetryGenerator = new Foundation.Telemetry.Database.TelemetryDatabaseGenerator();

            //
            // Build the foundation's base code to interact with the Foundation Auditor, Security, and Telemetry Schemas
            //
            GenerateTemplateCodeFromEntityFrameworkContext("AuditorContext", "Auditor", typeof(Foundation.Auditor.Database.AuditorContext), auditorGenerator.database, folderPath);
            GenerateTemplateCodeFromEntityFrameworkContext("SecurityContext", "Security", typeof(Foundation.Security.Database.SecurityContext), securityGenerator.database, folderPath);
            GenerateTemplateCodeFromEntityFrameworkContext("TelemetryContext", "Telemetry", typeof(Foundation.Telemetry.Database.TelemetryContext), telemetryGenerator.database, folderPath);

            //
            // Create Angular services to interact with the WebAPI
            //
            Foundation.CodeGeneration.AngularServiceGenerator.BuildAngularServiceImplementationFromEntityFrameworkContext("Security", typeof(Foundation.Security.Database.SecurityContext), securityGenerator.database, folderPath);
            Foundation.CodeGeneration.AngularServiceGenerator.BuildAngularServiceImplementationFromEntityFrameworkContext("Auditor", typeof(Foundation.Auditor.Database.AuditorContext), auditorGenerator.database, folderPath);
            Foundation.CodeGeneration.AngularServiceGenerator.BuildAngularServiceImplementationFromEntityFrameworkContext("Telemetry", typeof(Foundation.Telemetry.Database.TelemetryContext), telemetryGenerator.database, folderPath);

            //
            // Create Angular Components to interact with the data services
            //
            Foundation.CodeGeneration.AngularComponentGenerator.BuildAngularComponentImplementationFromEntityFrameworkContext("Security", typeof(Foundation.Security.Database.SecurityContext), securityGenerator.database, folderPath);
            Foundation.CodeGeneration.AngularComponentGenerator.BuildAngularComponentImplementationFromEntityFrameworkContext("Auditor", typeof(Foundation.Auditor.Database.AuditorContext), auditorGenerator.database, folderPath);
            Foundation.CodeGeneration.AngularComponentGenerator.BuildAngularComponentImplementationFromEntityFrameworkContext("Telemetry", typeof(Foundation.Telemetry.Database.TelemetryContext), telemetryGenerator.database, folderPath);

            return;
        }



        public static void GenerateTemplateCodeFromEntityFrameworkContext(string contextClassName, string moduleName, Type contextType, DatabaseGenerator.Database database, string folderPath = null, string databaseObjectNamespace = "Database", bool ignoreFoundationServices = false, string rootNameSpace = "Foundation")
        {
            if (folderPath == null)
            {
                folderPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "TemplateCode");
            }

            //
            // Build the default Web API code
            //
            CodeGeneration.WebAPICodeGenerator.BuildWebAPIImplementationFromEntityFrameworkContext(contextClassName, moduleName, contextType, database, folderPath, databaseObjectNamespace, ignoreFoundationServices, rootNameSpace);

            //
            // Build the default Entity Extension code
            //
            CodeGeneration.EntityExtensionCodeGenerator.BuildEntityExtensionImplementationFromEntityFrameworkContext(moduleName, contextType, database, folderPath, databaseObjectNamespace, ignoreFoundationServices, rootNameSpace);
        }
    }
}
