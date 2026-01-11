using System;

namespace Foundation.CodeGeneration
{
    /// <summary>
    /// This class provides standard Foundation module script generation.  It is platform specific because it references code generation unique to the Framework version of the Foundation.
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
            // Create the Foundation's Auditor and Security system create database scripts
            // 
            Foundation.Auditor.Database.AuditorDatabaseGenerator auditorScriptGenerator = new Foundation.Auditor.Database.AuditorDatabaseGenerator();
            Foundation.Security.Database.SecurityDatabaseGenerator securityScriptGenerator = new Foundation.Security.Database.SecurityDatabaseGenerator();
            Foundation.Security.Database.OIDCDatabaseGenerator oidcScriptGenerator = new Foundation.Security.Database.OIDCDatabaseGenerator();


            auditorScriptGenerator.GenerateDatabaseCreationScriptsInFolder(folderPath);
            securityScriptGenerator.GenerateDatabaseCreationScriptsInFolder(folderPath);
            oidcScriptGenerator.GenerateDatabaseCreationScriptsInFolder(folderPath, false, "OIDC");


            return;
        }


        public static void GenerateFoundationEntityCode(string folderPath = null)
        {
            if (folderPath == null)
            {
                folderPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "FoundationEntityCode");
            }

            //
            // Create the Foundation's Auditor and Security system create database scripts
            // 
            Foundation.Auditor.Database.AuditorDatabaseGenerator auditorGenerator = new Foundation.Auditor.Database.AuditorDatabaseGenerator();
            Foundation.Security.Database.SecurityDatabaseGenerator securityGenerator = new Foundation.Security.Database.SecurityDatabaseGenerator();

            //
            // Build the foundation's base code to interact with the Foundation Auditor and Security Schemas
            //
            GenerateTemplateCodeFromEntityFrameworkContext("AuditorContext", "Auditor", typeof(Foundation.Auditor.Database.AuditorContext), auditorGenerator.database, folderPath);
            GenerateTemplateCodeFromEntityFrameworkContext("SecurityContext", "Security", typeof(Foundation.Security.Database.SecurityContext), securityGenerator.database, folderPath);


            //
            // Create Angular services to interact with the WebAPI
            //
            Foundation.CodeGeneration.AngularServiceGenerator.BuildAngularServiceImplementationFromEntityFrameworkContext("Security", typeof(Foundation.Security.Database.SecurityContext), securityGenerator.database, folderPath);
            Foundation.CodeGeneration.AngularServiceGenerator.BuildAngularServiceImplementationFromEntityFrameworkContext("Auditor", typeof(Foundation.Auditor.Database.AuditorContext), auditorGenerator.database, folderPath);


            return;
        }


        public static void GenerateTemplateCodeFromEntityFrameworkContext(string contextName, string moduleName, Type contextType, DatabaseGenerator.Database database, string folderPath = null, string databaseObjectNamespace = "Database", bool ignoreFoundationServices = false)
        {
            if (folderPath == null)
            {
                folderPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "TemplateCode");
            }


            // The new way
            CodeGeneration.MVCCodeGenerator.BuildMVCImplementationFromEntityFrameworkContext(moduleName, contextType, database, folderPath);

            //
            // Build the default Web API code
            //
            CodeGeneration.WebAPICodeGenerator.BuildWebAPIImplementationFromEntityFrameworkContext(contextName, moduleName, contextType, database, folderPath, databaseObjectNamespace, ignoreFoundationServices);

            //
            // Build the default Entity Extension code
            //
            CodeGeneration.EntityExtensionCodeGenerator.BuildEntityExtensionImplementationFromEntityFrameworkContext(moduleName, contextType, database, folderPath, databaseObjectNamespace);


            //
            // Build the code snippets for the templates and menus unique to the Framework version, so they can drive the cshtml base MVC screen automation.
            //
            CodeGeneration.CodeGenerationBase.BuildTemplatesFromEFContext(moduleName, contextType, database, folderPath);
            CodeGeneration.CodeGenerationBase.BuildMenuFromEntityFrameworkContext(moduleName, contextType, database, folderPath);
        }
    }
}
