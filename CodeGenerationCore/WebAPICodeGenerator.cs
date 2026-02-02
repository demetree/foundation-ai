using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using static Foundation.StringUtility;

namespace Foundation.CodeGeneration
{
    public partial class WebAPICodeGenerator : CodeGenerationBase
    {
        //
        // Use this to create default implementation of a WebAPI controller for .Net Core
        //
        //
        // ToDo List:
        //
        //  - Make rate limit settings configurable - None, 1 per second, 2 per second, etc...  Right now all get 2 per second just as a reasonable starting point that will allow 2 concurrent requests from the same user.
        //
        protected static string BuildDefaultWebAPIImplementation(string contextClassName, string module, Type type, DatabaseGenerator.Database.Table scriptGenTable, string databaseNamespace = "Database", bool ignoreFoundationServices = false, string rootNameSpace = "Foundation")
        {
            if (scriptGenTable == null)
            {
                throw new Exception($"What is going on with type {type.Name}.  Why can't we get it's script gen table?");
            }

            StringBuilder sb = new StringBuilder();

            string entityName = type.Name;
            string camelCaseName = CamelCase(entityName);
            string titleName = StringUtility.ConvertToHeader(type.Name);

            string qualifiedEntity = databaseNamespace + "." + entityName;

            bool tableIsWritable = true;
            bool multiTenancyEnabled = false;
            bool dataVisibilityEnabled = false;
            bool versionControlEnabled = false;
            bool canBeFavourited = false;
            bool adminAccessNeededToWrite = false;

            string displayNameFieldSerializationCode = camelCaseName + ".id.ToString()";          // Default to this because every entity will have it.

            string entityMinimumReadPermissionString = "0";
            string entityMinimumWritePermissionString = "0";

            int minimumReadPermissionLevel = 0;
            int minimumWritePermissionLevel = 0;

            int commandTimeoutSeconds = 30;

            #region Initial_setup_of_operating_parameters

            if (scriptGenTable != null && scriptGenTable.isWritable == false)
            {
                tableIsWritable = false;
            }


            if (scriptGenTable != null)
            {
                multiTenancyEnabled = scriptGenTable.IsMultiTenantEnabled();
                dataVisibilityEnabled = scriptGenTable.IsDataVisibilityEnabled();

                versionControlEnabled = scriptGenTable.IsVersionControlEnabled();

                canBeFavourited = scriptGenTable.canBeFavourited;

                minimumReadPermissionLevel = scriptGenTable.minimumReadPermissionLevel;
                minimumWritePermissionLevel = scriptGenTable.minimumWritePermissionLevel;

                adminAccessNeededToWrite = scriptGenTable.adminAccessNeededToWrite;

                if (scriptGenTable.minimumWritePermissionLevel > 0)
                {
                    entityMinimumWritePermissionString = scriptGenTable.minimumWritePermissionLevel.ToString();
                }
                else
                {
                    entityMinimumWritePermissionString = "0";
                }

                if (scriptGenTable.minimumReadPermissionLevel > 0)
                {
                    entityMinimumReadPermissionString = scriptGenTable.minimumReadPermissionLevel.ToString();
                }
                else
                {
                    entityMinimumReadPermissionString = "0";
                }

                commandTimeoutSeconds = scriptGenTable.commandTimeoutSeconds;

                if (scriptGenTable.displayNameFieldList.Count > 0)
                {
                    displayNameFieldSerializationCode = "";

                    // Create a name from all the parts of the display name field list.
                    for (int i = 0; i < scriptGenTable.displayNameFieldList.Count; i++)
                    {
                        if (i > 0)
                        {
                            // Add in a comma and a space
                            displayNameFieldSerializationCode += " + \", \" + ";
                        }

                        displayNameFieldSerializationCode += (camelCaseName + "." + scriptGenTable.displayNameFieldList[i].name);
                    }

                }
                else
                {
                    var firstStringField = scriptGenTable.GetFirstStringField();

                    if (firstStringField != null)
                    {
                        displayNameFieldSerializationCode = camelCaseName + "." + firstStringField.name;
                    }
                }
            }


            string acronym = GetAcronym(entityName);

            //
            // Change acronym if it's a C# reserved word.  Add more as we find them.
            //
            if (acronym == "in" ||
                acronym == "event")
            {
                acronym = acronym + "_";
            }

            string plural = Pluralize(entityName);


            //
            // Hack for pluralizer differences in various EF context pluralization.  We want plural of 'Data' to be 'Datas'
            //
            // Datum->Data
            // Data->Datas 
            //
            string pluralForRouting = null;
            //
            // Add s to the end of Data, for example ProjectRollerData would become ProjectRollerDatas - this is to address objects suffixed with 'Datum', as the EFPT pluralizer does.
            //
            if (plural.EndsWith("Data") == true)
            {
                pluralForRouting = plural + "s";
            }
            else
            {
                pluralForRouting = plural;
            }

            string singularForRouting = pluralizeEntityForRouteForSomeTypeNames(entityName);



            bool hasActiveAndDeletedControlFields = false;

            // It is assumed active and deleted come as a pair, so the presence of deleted implies the presence of active.
            foreach (PropertyInfo prop in type.GetProperties())
            {
                if (prop.Name == "deleted")
                {
                    hasActiveAndDeletedControlFields = true;
                }
            }


            bool hasImageFields = false;

            string dataRootFieldName = "";
            string dataFileNameExtension = "";
            string dataDefaultMimeType = "";

            if (scriptGenTable != null)
            {
                if (string.IsNullOrWhiteSpace(scriptGenTable.pdfRootFieldName) == false)
                {
                    dataRootFieldName = scriptGenTable.pdfRootFieldName;
                    dataFileNameExtension = "pdf";
                    dataDefaultMimeType = "application/pdf";
                }
                else if (string.IsNullOrWhiteSpace(scriptGenTable.mp4RootFieldName) == false)
                {
                    dataRootFieldName = scriptGenTable.mp4RootFieldName;
                    dataFileNameExtension = "mp4";
                    dataDefaultMimeType = "video/mp4";
                }
                else if (string.IsNullOrWhiteSpace(scriptGenTable.pngRootFieldName) == false)
                {
                    hasImageFields = true;

                    dataRootFieldName = scriptGenTable.pngRootFieldName;
                    dataFileNameExtension = "png";
                    dataDefaultMimeType = "image/png";
                }
                else if (string.IsNullOrWhiteSpace(scriptGenTable.binaryDataRootFieldName) == false)
                {
                    dataRootFieldName = scriptGenTable.binaryDataRootFieldName;
                    dataFileNameExtension = "data";
                    dataDefaultMimeType = "application/octet-stream";
                }
            }

            #endregion

            #region Initial Class Setup

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Threading;");

            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Text.Json;");

            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using Microsoft.Extensions.Logging;");
            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
            sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            sb.AppendLine("using Microsoft.EntityFrameworkCore.ChangeTracking;");
            sb.AppendLine("using Microsoft.EntityFrameworkCore.Storage;");

            if (ignoreFoundationServices == false)
            {
                sb.AppendLine("using Foundation.Security;");
                sb.AppendLine("using Foundation.Auditor;");
            }

            if (ignoreFoundationServices == false)
            {
                if (rootNameSpace != "Foundation")
                {
                    sb.AppendLine("using Foundation;");
                }

                sb.AppendLine($"using Foundation.Controllers;");

                if (module != "Security")
                {
                    sb.AppendLine("using Foundation.Security.Database;");
                }

                sb.AppendLine("using static Foundation.Auditor.AuditEngine;");
            }
            else
            {
                sb.AppendLine($"using {rootNameSpace}.Controllers;");
            }
            // Add in the module specific database, with custom namespace if provided
            sb.AppendLine($"using {rootNameSpace}.{module}.{databaseNamespace};");

            //
            // AI-Developed: Add Foundation.ChangeHistory import for version-controlled entities
            //
            if (versionControlEnabled == true && ignoreFoundationServices == false)
            {
                sb.AppendLine("using Foundation.ChangeHistory;");
            }


            // Don't need this since I moved the BackgroundJob class into the Foundation namespace
            //if (ignoreFoundationServices == false)
            //{
            //    sb.AppendLine("using FoundationCore.Utility;");     // To get the BackgroundJob class
            //}

            if (scriptGenTable != null && string.IsNullOrWhiteSpace(scriptGenTable.pngRootFieldName) == false)
            {
                sb.AppendLine("using System.Drawing;");
            }


            if (scriptGenTable != null &&
                (string.IsNullOrWhiteSpace(scriptGenTable.pdfRootFieldName) == false ||
                string.IsNullOrWhiteSpace(scriptGenTable.mp4RootFieldName) == false ||
                string.IsNullOrWhiteSpace(scriptGenTable.pngRootFieldName) == false ||
                string.IsNullOrWhiteSpace(scriptGenTable.binaryDataRootFieldName) == false))
            {
                sb.AppendLine("using System.IO;");
                sb.AppendLine("using System.Net;");
                sb.AppendLine("using Microsoft.AspNetCore.Http.Extensions;");
                sb.AppendLine("using Microsoft.AspNetCore.WebUtilities;");
                sb.AppendLine("using Microsoft.Net.Http.Headers;");
            }


            sb.AppendLine("");
            sb.AppendLine("namespace " + rootNameSpace + "." + module + ".Controllers.WebAPI");
            sb.AppendLine("{");


            sb.AppendLine($@"    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the {entityName} entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the {entityName} entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>");

            if (ignoreFoundationServices == false)
            {
                if (dataVisibilityEnabled == true)
                {
                    sb.AppendLine("\tpublic partial class " + Pluralize(entityName) + "Controller : " + module + "BaseWebAPIController");
                }
                else
                {
                    sb.AppendLine("\tpublic partial class " + Pluralize(entityName) + "Controller : SecureWebAPIController");
                }
            }
            else
            {
                sb.AppendLine("\tpublic partial class " + Pluralize(entityName) + "Controller : ControllerBase");

            }

            sb.AppendLine("\t{");

            if (ignoreFoundationServices == false)
            {
                // Moved these into the constructor to assign values to vars in the base class so that common functions using these values can be built into the base class.
                sb.AppendLine("\t\tpublic const int READ_PERMISSION_LEVEL_REQUIRED = " + minimumReadPermissionLevel + ";");
                sb.AppendLine("\t\tpublic const int WRITE_PERMISSION_LEVEL_REQUIRED = " + minimumWritePermissionLevel + ";");
                sb.AppendLine();
            }


            if (versionControlEnabled == true)
            {
                sb.AppendLine("\t\tstatic object " + camelCaseName + "PutSyncRoot = new object();");
                sb.AppendLine("\t\tstatic object " + camelCaseName + "DeleteSyncRoot = new object();");
                sb.AppendLine();
            }

            //
            // Define the database object here.  Tables with data visibility 'On' derive from a different base class that has the db object defined, so those don't need it.
            //
            if (dataVisibilityEnabled == false)
            {
                if (contextClassName == null)
                {
                    sb.AppendLine("\t\tprivate " + module + "Entities _context;");
                }
                else
                {
                    sb.AppendLine("\t\tprivate " + contextClassName + " _context;");
                }

                sb.AppendLine();
            }

            //
            // Add standard MS ASP.Net logger for logger injection
            //
            sb.AppendLine("\t\tprivate ILogger<" + Pluralize(entityName) + "Controller> _logger;");
            sb.AppendLine();

            if (ignoreFoundationServices == false)
            {
                if (contextClassName == null)
                {
                    sb.AppendLine("\t\tpublic " + Pluralize(entityName) + "Controller(" + module + "Entities context, ILogger<" + Pluralize(entityName) + "Controller> logger) : base(\"" + module + "\", \"" + entityName + "\")");

                }
                else
                {
                    sb.AppendLine("\t\tpublic " + Pluralize(entityName) + "Controller(" + contextClassName + " context, ILogger<" + Pluralize(entityName) + "Controller> logger) : base(\"" + module + "\", \"" + entityName + "\")");
                }
            }
            else
            {
                if (contextClassName == null)
                {
                    sb.AppendLine("\t\tpublic " + Pluralize(entityName) + "Controller(" + module + "Entities context, ILogger<" + Pluralize(entityName) + "Controller> logger)");
                }
                else
                {
                    sb.AppendLine("\t\tpublic " + Pluralize(entityName) + "Controller(" + contextClassName + " context, ILogger<" + Pluralize(entityName) + "Controller> logger)");
                }
            }

            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tthis._context = context;");
            sb.AppendLine("\t\t\tthis._logger = logger;");
            sb.AppendLine();
            sb.AppendLine("\t\t\tthis._context.Database.SetCommandTimeout(" + commandTimeoutSeconds.ToString() + ");");
            sb.AppendLine();
            sb.AppendLine("\t\t\treturn;");
            sb.AppendLine("\t\t}");
            sb.AppendLine();


            if (scriptGenTable != null && scriptGenTable.webAPIListGetterToBeOverridden == true)
            {
                // comment out this function, but write the code anyway.
                sb.AppendLine("/* This function is expected to be overridden in a custom file");
            }


            #endregion

            #region HTTP_Get_List_Handling


            sb.AppendLine($@"		/// <summary>
		/// 
		/// This gets a list of {plural} filtered by the parameters provided.
		/// 
		/// There is a filter parameter for every field, and an 'anyStringContains' parameter for cross field string partial searches.
		/// 
		/// Note also the pagination control in the pageSize and pageNumber parameters.
		/// 
		/// The rate limit is 2 per second per user.
		/// 
		/// </summary>");
            sb.AppendLine("\t\t[HttpGet]");
            sb.AppendLine("\t\t[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]");
            sb.AppendLine("\t\t[Route(\"api/" + pluralForRouting + "\")]");


            sb.AppendLine("\t\tpublic async Task<IActionResult> Get" + plural + "(");

            bool processingFirstProperty = true;
            foreach (PropertyInfo prop in type.GetProperties())
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }


                // jump over the id field on the list getter function.  It adds no value as a parameter here and will likely class with the individual getter function, which only has ID as a param below.
                if (prop.Name == "id")
                {
                    continue;
                }

                //
                // Don't create a password filter
                //
                if (prop.Name == "password")
                {
                    continue;
                }

                //
                // Don't create a tenant guid filter
                //
                if (prop.Name == "tenantGuid")
                {
                    continue;
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType == typeof(string))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }
                    sb.Append("\t\t\tstring " + prop.Name + " = null");
                }
                else if (propertyType == typeof(Guid))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }
                    sb.Append("\t\t\tGuid? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(int))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tint? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(long))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tlong? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(bool))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tbool? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(DateTime))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tDateTime? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(float))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tfloat? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(double))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tdouble? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(decimal))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tdecimal? " + prop.Name + " = null");
                }

                processingFirstProperty = false;
            }
            sb.AppendLine(",");

            if (scriptGenTable != null && scriptGenTable.defaultPageSizeForListGetters > 0)
            {
                sb.AppendLine($"\t\t\tint? pageSize = {scriptGenTable.defaultPageSizeForListGetters},");     // Use the tables's own default 
                sb.AppendLine("\t\t\tint? pageNumber = 1,");
            }
            else
            {
                sb.AppendLine($"\t\t\tint? pageSize = null,");
                sb.AppendLine("\t\t\tint? pageNumber = null,");
            }

            if (scriptGenTable != null && scriptGenTable.AddAnyStringContainsParameterToWebAPI == true)
            {
                sb.AppendLine("\t\t\tstring anyStringContains = null,");
            }

            //
            // If we have image fields, then add a new parameters to provide a default image width for reducing the size of images in the list gets.
            //
            if (hasImageFields == false)
            {
                sb.AppendLine("\t\t\tbool includeRelations = true,");
                sb.AppendLine("\t\t\tCancellationToken cancellationToken = default)");
            }
            else
            {
                sb.AppendLine("\t\t\tbool includeRelations = true,");
                sb.AppendLine("\t\t\tint imageWidth = 100,");
                sb.AppendLine("\t\t\tCancellationToken cancellationToken = default)");
            }

            sb.AppendLine("\t\t{");

            if (ignoreFoundationServices == false)
            {
                sb.AppendLine("\t\t\tStartAuditEventClock();");
                sb.AppendLine();

                GenerateReadRoleAndPermissionChecks(module, scriptGenTable, sb);


                //
                // No longer using entity data tokens for route auth.  No need to write the code for it.
                //
                //sb.AppendLine("\t\t\tif (await IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel.Read) == false)");
                //sb.AppendLine("\t\t\t{");
                //sb.AppendLine("\t\t\t   return Forbid();");
                //sb.AppendLine("\t\t\t}");
                //sb.AppendLine();

                sb.AppendLine();
                sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);");
                sb.AppendLine();

                if (ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\tbool userIsWriter = await UserCanWriteAsync(securityUser, " + entityMinimumWritePermissionString + ", cancellationToken);");
                    sb.AppendLine("\t\t\tbool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);");
                    sb.AppendLine();
                }

                if (multiTenancyEnabled == true)
                {
                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine("\t\t\tbool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);");
                        sb.AppendLine("\t\t\t");
                    }

                    sb.AppendLine(UserTenantGuidCommands(3));
                }
            }
            else
            {
                if (ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\tbool userIsWriter = true;");
                    sb.AppendLine("\t\t\tbool userIsAdmin = true;");
                    sb.AppendLine();
                }
            }


            sb.AppendLine("\t\t\tif (pageNumber.HasValue == true &&");
            sb.AppendLine("\t\t\t    pageNumber < 1)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t    pageNumber = null;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();
            sb.AppendLine("\t\t\tif (pageSize.HasValue == true &&");
            sb.AppendLine("\t\t\t    pageSize <= 0)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t    pageSize = null;");
            sb.AppendLine("\t\t\t}");

            sb.AppendLine();

            bool commentWritten = false;
            foreach (PropertyInfo prop in type.GetProperties())
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType == typeof(DateTime))
                {
                    if (commentWritten == false)
                    {
                        sb.AppendLine("\t\t\t//");
                        sb.AppendLine("\t\t\t// Turn any local time kinded parameters to UTC.");
                        sb.AppendLine("\t\t\t//");
                        commentWritten = true;
                    }

                    sb.AppendLine("\t\t\tif (" + prop.Name + ".HasValue == true && " + prop.Name + ".Value.Kind != DateTimeKind.Utc)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\t" + prop.Name + " = " + prop.Name + ".Value.ToUniversalTime();");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();
                }
            }

            /*
             * This pattern uses the method syntax to apply where conditions only when necessary to dramatically increase query performance by not needing the db to evaluate the null state of the parameter.
             * */
            sb.AppendLine($"\t\t\tIQueryable<{qualifiedEntity}> query = (from {acronym} in _context.{plural} select {acronym});");

            if (ignoreFoundationServices == false)
            {
                if (dataVisibilityEnabled == true)
                {
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// Apply the constraints for the user's tenant and data visibility configuration.");
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\tquery = await AddTenantAndDataVisibilityConstraintsAsync(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin, cancellationToken);");
                    sb.AppendLine();
                }
                else if (multiTenancyEnabled == true)
                {
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tquery = query.Where(x => x.tenantGuid == userTenantGuid);");
                    sb.AppendLine();
                }
            }

            processingFirstProperty = true;
            foreach (PropertyInfo prop in type.GetProperties())
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                if (prop.Name == "id")
                {
                    continue;
                }

                //
                // don't create a password filter
                //
                if (prop.Name == "password")
                {
                    continue;
                }

                //
                // don't create a tenant guid filter
                //
                if (prop.Name == "tenantGuid")
                {
                    continue;
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType == typeof(string))
                {
                    sb.AppendLine("\t\t\tif (string.IsNullOrEmpty(" + prop.Name + ") == false)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + "." + prop.Name + " == " + prop.Name + ");");
                    sb.AppendLine("\t\t\t}");
                }
                if (propertyType == typeof(Guid))
                {
                    sb.AppendLine("\t\t\tif (" + prop.Name + ".HasValue == true)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + "." + prop.Name + " == " + prop.Name + ");");
                    sb.AppendLine("\t\t\t}");
                }
                else if (propertyType == typeof(int) || propertyType == typeof(long) || propertyType == typeof(DateTime) || propertyType == typeof(float) || propertyType == typeof(double) || propertyType == typeof(decimal))
                {
                    sb.AppendLine("\t\t\tif (" + prop.Name + ".HasValue == true)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + "." + prop.Name + " == " + prop.Name + ".Value);");
                    sb.AppendLine("\t\t\t}");
                }
                else if (propertyType == typeof(bool))
                {
                    if (prop.Name == "active" && ignoreFoundationServices == false)
                    {
                        // Only writers and admins can see inactive records, or filter by active.  Only admins can see deleted records, or filter by deleted.
                        sb.AppendLine("\t\t\tif (userIsWriter == true)");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t\tif (active.HasValue == true)");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".active == active.Value);");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine("\t\t\t");

                        if (scriptGenTable.HasField("deleted") == true)
                        {
                            sb.AppendLine("\t\t\t\tif (userIsAdmin == true)");
                            sb.AppendLine("\t\t\t\t{");
                            sb.AppendLine("\t\t\t\t\tif (deleted.HasValue == true)");
                            sb.AppendLine("\t\t\t\t\t{");
                            sb.AppendLine("\t\t\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".deleted == deleted.Value);");
                            sb.AppendLine("\t\t\t\t\t}");
                            sb.AppendLine("\t\t\t\t}");
                            sb.AppendLine("\t\t\t\telse");
                            sb.AppendLine("\t\t\t\t{");
                            sb.AppendLine("\t\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".deleted == false);");
                            sb.AppendLine("\t\t\t\t}");
                        }
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine("\t\t\telse");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".active == true);");

                        if (scriptGenTable.HasField("deleted") == true)
                        {
                            sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".deleted == false);");
                        }
                        sb.AppendLine("\t\t\t}");
                    }
                    else if (prop.Name == "deleted" && ignoreFoundationServices == false)
                    {
                        //
                        // Deleted handled in active handler.
                        //
                    }
                    else
                    {
                        //
                        // Treat as regular bool
                        //
                        sb.AppendLine("\t\t\tif (" + prop.Name + ".HasValue == true)");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + "." + prop.Name + " == " + prop.Name + ".Value);");
                        sb.AppendLine("\t\t\t}");
                    }
                }
            }

            sb.AppendLine();

            bool hasSequence = false;

            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.Name == "sequence")
                {
                    hasSequence = true;
                    break;
                }
            }

            if (scriptGenTable == null)
            {
                //
                // Use the sequence field, or the first field for the sort if no explicit sort sequence is defined.
                //
                if (hasSequence == false)
                {
                    sb.AppendLine("\t\t\tquery = query.OrderBy(" + acronym + " => " + acronym + "." + type.GetProperties().First().Name + ");");
                }
                else
                {
                    sb.AppendLine("\t\t\tquery = query.OrderBy(" + acronym + " => " + acronym + ".sequence);");
                }
            }
            else
            {
                //
                // Use the sort sequence defined on the script generation table object
                //
                bool firstSortSequenceWritten = false;
                foreach (var ss in scriptGenTable.GetOrGenerateSortSequences())
                {
                    if (ss == null || ss.field == null)
                    {
                        continue;
                    }

                    if (firstSortSequenceWritten == false)
                    {
                        sb.Append("\t\t\tquery = query.OrderBy" + (ss.descending == true ? "Descending" : "") + "(" + acronym + " => " + acronym + "." + ss.field.name + ")");
                        firstSortSequenceWritten = true;
                    }
                    else
                    {
                        sb.Append(".ThenBy" + (ss.descending == true ? "Descending" : "") + "(" + acronym + " => " + acronym + "." + ss.field.name + ")");
                    }
                }

                if (firstSortSequenceWritten == true)
                {
                    sb.AppendLine(";");
                }

                sb.AppendLine();
            }

            sb.AppendLine("\t\t\tif (pageNumber.HasValue == true &&");
            sb.AppendLine("\t\t\t    pageSize.HasValue == true)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t\t");
            sb.AppendLine("\t\t\tif (includeRelations == true)");
            sb.AppendLine("\t\t\t{");

            // Add the includes if necessary
            foreach (PropertyInfo prop in type.GetProperties())
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                // Is this a linked object type, but not a list?
                if (propertyType != typeof(string) &&
                    propertyType != typeof(int) &&
                    propertyType != typeof(long) &&
                    propertyType != typeof(DateTime) &&
                    propertyType != typeof(TimeOnly) &&
                    propertyType != typeof(DateOnly) &&
                    propertyType != typeof(float) &&
                    propertyType != typeof(double) &&
                    propertyType != typeof(decimal) &&
                    propertyType != typeof(bool) &&
                    propertyType != typeof(Guid) &&
                    propertyType != typeof(byte[]) &&
                    propertyType.FullName.StartsWith("System.Collections") == false)
                {

                    sb.AppendLine("\t\t\t\tquery = query.Include(x => x." + prop.Name + ");");
                }
            }

            sb.AppendLine("\t\t\t\tquery = query.AsSplitQuery();");

            sb.AppendLine("\t\t\t}");
            sb.AppendLine();
            //
            // Add the any string contains parameter
            //
            if (scriptGenTable.AddAnyStringContainsParameterToWebAPI == true)
            {
                AddAnyStringsContainsQueryAdditions(type, sb, rootNameSpace, true);
            }

            sb.AppendLine("\t\t\tquery = query.AsNoTracking();");


            sb.AppendLine("\t\t\t");
            sb.AppendLine("\t\t\tList<" + qualifiedEntity + "> materialized = await query.ToListAsync(cancellationToken);");

            sb.AppendLine();



            sb.AppendLine("\t\t\t// Convert all the date properties to be of kind UTC.");
            sb.AppendLine("\t\t\tbool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();");   // Note that this custom method needs to be on the Core context class.
            sb.AppendLine("\t\t\tforeach (" + qualifiedEntity + " " + camelCaseName + " in materialized)");
            sb.AppendLine("\t\t\t{");

            if (ignoreFoundationServices == false)
            {
                sb.AppendLine("\t\t\t    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(" + camelCaseName + ", databaseStoresDateWithTimeZone);");
            }
            else
            {
                sb.AppendLine("\t\t\t    " + contextClassName + ".ConvertAllDateTimePropertiesToUTC(" + camelCaseName + ", databaseStoresDateWithTimeZone);");
            }


            sb.AppendLine("\t\t\t}");
            sb.AppendLine();


            if (dataVisibilityEnabled == false || ignoreFoundationServices == true)
            {
                if (HasBinaryStorageFields(scriptGenTable) == true)
                {
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tbool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tif (diskBasedBinaryStorageMode == true)");
                    sb.AppendLine("\t\t\t{");

                    //
                    // var used purposely here.
                    //  
                    sb.AppendLine("\t\t\t\tvar tasks = materialized.Select(async " + camelCaseName + " =>");
                    sb.AppendLine("\t\t\t\t{");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\tif (" + camelCaseName + "." + dataRootFieldName + "Data == null &&");

                    if (BinaryStorageFieldsAreNullable(scriptGenTable) == true)
                    {
                        sb.AppendLine("\t\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Size.HasValue == true &&");
                        sb.AppendLine("\t\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Size.Value > 0)");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Size > 0)");
                    }

                    sb.AppendLine("\t\t\t\t\t{");
                    if (versionControlEnabled == true)
                    {
                        sb.AppendLine("\t\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Data = await LoadDataFromDiskAsync(" + camelCaseName + ".objectGuid, " + camelCaseName + ".versionNumber, \"" + dataFileNameExtension + "\");");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Data = await LoadDataFromDiskAsync(" + camelCaseName + ".objectGuid, 0, \"" + dataFileNameExtension + "\");");
                    }
                    sb.AppendLine("\t\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t}).ToList();");
                    sb.AppendLine();

                    sb.AppendLine("\t\t\t\t// Run tasks concurrently and await their completion");
                    sb.AppendLine("\t\t\t\tawait Task.WhenAll(tasks);");



                    sb.AppendLine("\t\t\t}");
                }

                sb.AppendLine();

                if (hasImageFields == true)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// Resize the image data to a standard width to help to reduce the size of the data transferred in list gets for data with images.");
                    sb.AppendLine("\t\t\t//");


                    // New Parallel way
                    sb.AppendLine("\t\t\tParallel.ForEach(materialized, " + camelCaseName + " =>");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t   if (" + camelCaseName + "." + dataRootFieldName + @"Data != null)");
                    sb.AppendLine("\t\t\t   {");
                    sb.AppendLine("\t\t\t       using (MemoryStream memoryStream = new MemoryStream(" + camelCaseName + "." + dataRootFieldName + @"Data))");
                    sb.AppendLine("\t\t\t       {");
                    sb.AppendLine("\t\t\t           Image img = Image.FromStream(memoryStream);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t           Bitmap bmp = Foundation.ImagingHelper.ResizeImage(img, imageWidth);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t           if (bmp != null)");
                    sb.AppendLine("\t\t\t           {");
                    sb.AppendLine("\t\t\t               byte[] resizedImageData = Foundation.ImageUtility.ConvertImageToByteArray(bmp);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t               // Update " + camelCaseName + " fields safely");
                    sb.AppendLine("\t\t\t                " + camelCaseName + "." + dataRootFieldName + @"Data = resizedImageData;");
                    sb.AppendLine("\t\t\t                " + camelCaseName + "." + dataRootFieldName + @"Size = resizedImageData.Length;");
                    sb.AppendLine("\t\t\t           }");
                    sb.AppendLine("\t\t\t       }");
                    sb.AppendLine("\t\t\t    }");
                    sb.AppendLine("\t\t\t});");
                    sb.AppendLine();
                }

                if (ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? \"" + module + "." + entityName + " Entity list was read with Admin privilege.  Returning \" + materialized.Count + \" rows of data.\" : \"" + module + "." + entityName + " Entity list was read.  Returning \" + materialized.Count + \" rows of data.\");");
                    sb.AppendLine();
                }


                sb.AppendLine("\t\t\t// Create a new output object that only includes the relations if necessary, and doesn't include the empty list objects, so that we can reduce the amount of data being transferred.");
                sb.AppendLine("\t\t\tif (includeRelations == true)");
                sb.AppendLine("\t\t\t{");

                //sb.AppendLine("\t\t\t\tList<Object> reducedFieldOutput = (from materializedData in materialized");
                //sb.AppendLine($"\t\t\t\t\t\t select {databaseNamespace}.{entityName}.CreateAnonymousWithFirstLevelSubObjects(materializedData)).ToList();");
                //sb.AppendLine();
                //sb.AppendLine("\t\t\t\treturn Ok(reducedFieldOutput);");

                sb.AppendLine($"\t\t\t\t// Return a DTO with nav properties.");
                sb.AppendLine($"\t\t\t\treturn Ok((from materializedData in materialized select materializedData.ToOutputDTO()).ToList());");


                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\telse");
                sb.AppendLine("\t\t\t{");

                //sb.AppendLine("\t\t\t\tList<Object> reducedFieldOutput = (from materializedData in materialized");
                //sb.AppendLine($"\t\t\t\t\t\t select {databaseNamespace}.{entityName}.CreateAnonymous(materializedData)).ToList();");
                //sb.AppendLine();
                //sb.AppendLine("\t\t\t\treturn Ok(reducedFieldOutput);");

                sb.AppendLine($"\t\t\t\t// Return a DTO without nav properties.");
                sb.AppendLine($"\t\t\t\treturn Ok((from materializedData in materialized select materializedData.ToDTO()).ToList());");

                sb.AppendLine("\t\t\t}");
            }
            else
            {
                sb.AppendLine("\t\t\tList<" + entityName + "WithWritePermissionDetails> output = new List<" + entityName + "WithWritePermissionDetails>();");
                sb.AppendLine();
                sb.AppendLine("\t\t\tList<int> organizationsUserIsEntitledToWriteTo = null;");
                sb.AppendLine("\t\t\tList<int> departmentsUserIsEntitledToWriteTo = null;");
                sb.AppendLine("\t\t\tList<int> teamsUserIsEntitledToWriteTo = null;");
                sb.AppendLine("\t\t\tList<int> userAndTheirReportIds = null;");
                sb.AppendLine();
                sb.AppendLine("\t\t\tList<int> organizationsUserIsEntitledToChangeOwnerFor = null;");
                sb.AppendLine("\t\t\tList<int> departmentsUserIsEntitledToChangeOwnerFor = null;");
                sb.AppendLine("\t\t\tList<int> teamsUserIsEntitledToChangeOwnerFor = null;");
                sb.AppendLine();
                sb.AppendLine("\t\t\tList<int> organizationsUserIsEntitledToChangeHierarchyFor = null;");
                sb.AppendLine("\t\t\tList<int> departmentsUserIsEntitledToChangeHierarchyFor = null;");
                sb.AppendLine("\t\t\tList<int> teamsUserIsEntitledToChangeHierarchyFor = null;");
                sb.AppendLine();
                sb.AppendLine("\t\t\tif (userIsAdmin == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\torganizationsUserIsEntitledToWriteTo = await GetOrganizationIdsUserIsEntitledToWriteToAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\t\tdepartmentsUserIsEntitledToWriteTo = await GetDepartmentIdsUserIsEntitledToWriteToAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\t\tteamsUserIsEntitledToWriteTo = await GetTeamIdsUserIsEntitledToWriteToAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\t\tuserAndTheirReportIds = await GetListOfUserIdsForUserAndPeopleThatReportToThemAsync(securityUser, cancellationToken);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\torganizationsUserIsEntitledToChangeOwnerFor = await GetOrganizationIdsUserIsEntitledToChangeOwnerForAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\t\tdepartmentsUserIsEntitledToChangeOwnerFor = await GetDepartmentIdsUserIsEntitledToChangeOwnerForAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\t\tteamsUserIsEntitledToChangeOwnerFor = await GetTeamIdsUserIsEntitledToChangeOwnerForAsync(securityUser, cancellationToken);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\torganizationsUserIsEntitledToChangeHierarchyFor = await GetOrganizationIdsUserIsEntitledToChangeHierarchyForAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\t\tdepartmentsUserIsEntitledToChangeHierarchyFor = await GetDepartmentIdsUserIsEntitledToChangeHierarchyForAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\t\tteamsUserIsEntitledToChangeHierarchyFor = await GetTeamIdsUserIsEntitledToChangeHierarchyForAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();

                if (HasBinaryStorageFields(scriptGenTable) == true)
                {
                    sb.AppendLine("\t\t\tbool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();");
                    sb.AppendLine();
                }



                //
                // This is the new parallel way.
                //
                // var used purposely here.
                //
                sb.AppendLine("\t\t\tvar tasks = materialized.Select(async " + camelCaseName + " =>");
                sb.AppendLine("\t\t\t{");

                if (HasBinaryStorageFields(scriptGenTable) == true)
                {
                    sb.AppendLine("\t\t\t     if (diskBasedBinaryStorageMode == true &&");
                    sb.AppendLine("\t\t\t         " + camelCaseName + "." + dataRootFieldName + @"Data == null &&");
                    sb.AppendLine("\t\t\t         " + camelCaseName + "." + dataRootFieldName + @"Size.HasValue == true &&");
                    sb.AppendLine("\t\t\t         " + camelCaseName + "." + dataRootFieldName + @"Size.Value > 0)");
                    sb.AppendLine("\t\t\t     {");
                    if (versionControlEnabled == true)
                    {
                        sb.AppendLine("\t\t\t         " + camelCaseName + "." + dataRootFieldName + @"Data = await LoadDataFromDiskAsync(" + camelCaseName + ".objectGuid, " + camelCaseName + ".versionNumber, \"" + dataFileNameExtension + "\", cancellationToken);");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t         " + camelCaseName + "." + dataRootFieldName + @"Data = await LoadDataFromDiskAsync(" + camelCaseName + ".objectGuid, 0, \"" + dataFileNameExtension + "\", cancellationToken);");
                    }

                    sb.AppendLine("\t\t\t     }");
                }
                sb.AppendLine();
                sb.AppendLine("\t\t\t     " + entityName + "WithWritePermissionDetails " + camelCaseName + "WWP = new " + entityName + "WithWritePermissionDetails(" + camelCaseName + ");");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\tConfigureIsWriteable(" + camelCaseName + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, userAndTheirReportIds, organizationsUserIsEntitledToWriteTo, departmentsUserIsEntitledToWriteTo, teamsUserIsEntitledToWriteTo);");
                sb.AppendLine("\t\t\t\tConfigureOwnerIsChangeable(" + camelCaseName + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, organizationsUserIsEntitledToChangeOwnerFor, departmentsUserIsEntitledToChangeOwnerFor, teamsUserIsEntitledToChangeOwnerFor);");
                sb.AppendLine("\t\t\t\tConfigureHierarchyIsChangeable(" + camelCaseName + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, organizationsUserIsEntitledToChangeHierarchyFor, departmentsUserIsEntitledToChangeHierarchyFor, teamsUserIsEntitledToChangeHierarchyFor);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t   return " + camelCaseName + "WWP;");
                sb.AppendLine("\t\t\t }).ToList();");
                sb.AppendLine();
                sb.AppendLine("\t\t\t// Execute in parallel");
                sb.AppendLine("\t\t\tvar results = await Task.WhenAll(tasks);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t// Add results to output in parallel to prevent thread issues");
                sb.AppendLine("\t\t\tParallel.ForEach(results, " + camelCaseName + "WWP =>");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t     output.Add(" + camelCaseName + "WWP);");
                sb.AppendLine("\t\t\t });");




                if (hasImageFields == true)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// Resize the image data to a standard width to help to reduce the size of the data transferred in list gets for data with images.");
                    sb.AppendLine("\t\t\t//");


                    // New Parallel way
                    sb.AppendLine("\t\t\tParallel.ForEach(output, " + camelCaseName + " =>");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t   if (" + camelCaseName + "." + dataRootFieldName + @"Data != null)");
                    sb.AppendLine("\t\t\t   {");
                    sb.AppendLine("\t\t\t       using (MemoryStream memoryStream = new MemoryStream(" + camelCaseName + "." + dataRootFieldName + @"Data))");
                    sb.AppendLine("\t\t\t       {");
                    sb.AppendLine("\t\t\t           Image img = Image.FromStream(memoryStream);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t           Bitmap bmp = Foundation.ImagingHelper.ResizeImage(img, imageWidth);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t           if (bmp != null)");
                    sb.AppendLine("\t\t\t           {");
                    sb.AppendLine("\t\t\t               byte[] resizedImageData = Foundation.ImageUtility.ConvertImageToByteArray(bmp);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t               // Update " + camelCaseName + " fields safely");
                    sb.AppendLine("\t\t\t               " + camelCaseName + "." + dataRootFieldName + @"Data = resizedImageData;");
                    sb.AppendLine("\t\t\t               " + camelCaseName + "." + dataRootFieldName + @"Size = resizedImageData.Length;");
                    sb.AppendLine("\t\t\t           }");
                    sb.AppendLine("\t\t\t       }");
                    sb.AppendLine("\t\t\t    }");
                    sb.AppendLine("\t\t\t});");
                    sb.AppendLine();
                }

                sb.AppendLine("\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? \"" + module + "." + entityName + " Entity list was read with Admin privilege.  Returning \" + materialized.Count + \" rows of data.\" : \"" + module + "." + entityName + " Entity list was read.  Returning \" + materialized.Count + \" rows of data.\");");
                sb.AppendLine();


                sb.AppendLine("\t\t\t// Create a new output object that only includes the relations if necessary, and doesn't include the empty list objects, so that we can reduce the amount of data being transferred.");
                sb.AppendLine("\t\t\tif (includeRelations == true)");
                sb.AppendLine("\t\t\t{");

                //sb.AppendLine("\t\t\t\tList<Object> reducedFieldOutput = (from data in output");
                //sb.AppendLine($"\t\t\t\t\t\t select {databaseNamespace}.{entityName}.CreateAnonymousWithFirstLevelSubObjects(data)).ToList();");
                //sb.AppendLine();
                //sb.AppendLine("\t\t\t\treturn Ok(reducedFieldOutput);");
                sb.AppendLine($"\t\t\t\t// Return a DTO with nav properties.");
                sb.AppendLine($"\t\t\t\treturn Ok((from materializedData in materialized select materializedData.ToOutputDTO()).ToList());");

                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\telse");
                sb.AppendLine("\t\t\t{");

                //sb.AppendLine("\t\t\t\tList<Object> reducedFieldOutput = (from data in output");
                //sb.AppendLine($"\t\t\t\t\t\t select {databaseNamespace}.{entityName}.CreateAnonymous(data)).ToList();");
                //sb.AppendLine();
                //sb.AppendLine("\t\t\t\treturn Ok(reducedFieldOutput);");
                sb.AppendLine($"\t\t\t\t// Return a DTO without nav properties.");
                sb.AppendLine($"\t\t\t\treturn Ok((from materializedData in materialized select materializedData.ToDTO()).ToList());");

                sb.AppendLine("\t\t\t}");
            }

            sb.AppendLine("\t\t}");
            sb.AppendLine("\t\t");

            if (scriptGenTable != null && scriptGenTable.webAPIListGetterToBeOverridden == true)
            {
                sb.AppendLine("*/");
            }

            #endregion

            #region RowCount_Handling

            sb.AppendLine("\t\t");

            if (scriptGenTable != null && scriptGenTable.webAPIGetRowCountToBeOverridden == true)
            {
                // comment out this function, but write the code anyway.
                sb.AppendLine("/* This function is expected to be overridden in a custom file");
            }

            sb.AppendLine($@"        /// <summary>
        /// 
        /// This returns a row count of {plural} filtered by the parameters provided.  Its query is similar to the Get{plural} method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>");

            sb.AppendLine("\t\t[HttpGet]");
            sb.AppendLine("\t\t[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]");

            sb.AppendLine("\t\t[Route(\"api/" + pluralForRouting + "/RowCount\")]");

            sb.AppendLine("\t\tpublic async Task<IActionResult> GetRowCount(");

            processingFirstProperty = true;
            foreach (PropertyInfo prop in type.GetProperties())
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                // jump over the id field on the list getter function.  It adds no value as a parameter here and will likely class with the individual getter function, which only has ID as a param below.
                if (prop.Name == "id")
                {
                    continue;
                }

                if (prop.Name == "tenantGuid")      // don't create a tenant guid filter
                {
                    continue;
                }
                if (prop.Name == "password")      // don't create a password filter
                {
                    continue;
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType == typeof(string))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }
                    sb.Append("\t\t\tstring " + prop.Name + " = null");
                }
                else if (propertyType == typeof(Guid))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }
                    sb.Append("\t\t\tGuid? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(int))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tint? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(long))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tlong? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(bool))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tbool? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(DateTime))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tDateTime? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(float))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tfloat? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(double))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tdouble? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(decimal))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tdecimal? " + prop.Name + " = null");
                }

                processingFirstProperty = false;
            }

            if (scriptGenTable.AddAnyStringContainsParameterToWebAPI == true)
            {
                sb.AppendLine(",");
                sb.AppendLine("\t\t\tstring anyStringContains = null,");
                sb.AppendLine("\t\t\tCancellationToken cancellationToken = default)");
            }
            else
            {
                sb.AppendLine(",");
                sb.AppendLine("\t\t\tCancellationToken cancellationToken = default)");

            }

            sb.AppendLine("\t\t{");

            if (ignoreFoundationServices == false)
            {
                GenerateReadRoleAndPermissionChecks(module, scriptGenTable, sb);

                //
                // No longer using entity data takens for route auth
                //
                //sb.AppendLine("\t\t\tif (await IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel.Read) == false)");
                //sb.AppendLine("\t\t\t{");
                //sb.AppendLine("\t\t\t   return Forbid();");
                //sb.AppendLine("\t\t\t}");
                //sb.AppendLine();


                sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);");
                sb.AppendLine();
                sb.AppendLine("\t\t\tbool userIsWriter = await UserCanWriteAsync(securityUser, " + entityMinimumWritePermissionString + ", cancellationToken);");
                sb.AppendLine("\t\t\tbool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);");

                if (multiTenancyEnabled == true)
                {
                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine("\t\t\tbool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);");
                        sb.AppendLine("\t\t\t");
                    }

                    sb.AppendLine(UserTenantGuidCommands(3));
                }
                sb.AppendLine();
            }


            commentWritten = false;
            foreach (PropertyInfo prop in type.GetProperties())
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }


                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType == typeof(DateTime))
                {
                    if (commentWritten == false)
                    {
                        sb.AppendLine("\t\t\t//");
                        sb.AppendLine("\t\t\t// Fix any non-UTC date parameters that come in.");
                        sb.AppendLine("\t\t\t//");
                        commentWritten = true;
                    }

                    sb.AppendLine("\t\t\tif (" + prop.Name + ".HasValue == true && " + prop.Name + ".Value.Kind != DateTimeKind.Utc)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\t" + prop.Name + " = " + prop.Name + ".Value.ToUniversalTime();");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();
                }
            }

            /*
             * 
             * This pattern uses the method syntax to apply where conditions only when necessary to dramatically increase query performance by not needing the db to evaluate the null state of the parameter.
             * 
             */
            sb.AppendLine($"\t\t\tIQueryable<{qualifiedEntity}> query = (from {acronym} in _context.{plural} select {acronym});");

            if (ignoreFoundationServices == false)
            {
                if (dataVisibilityEnabled == true)
                {
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// Apply the constraints for the user's tenant and data visibility configuration.");
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\tquery = await AddTenantAndDataVisibilityConstraintsAsync(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin, cancellationToken);");
                    sb.AppendLine();
                }
                else if (multiTenancyEnabled == true)
                {
                    sb.AppendLine("\t\t\tquery = query.Where(x => x.tenantGuid == userTenantGuid);");
                }
            }

            processingFirstProperty = true;
            foreach (PropertyInfo prop in type.GetProperties())
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                if (prop.Name == "id")
                {
                    continue;
                }

                if (prop.Name == "tenantGuid")      // don't create a tenant guid filter
                {
                    continue;
                }

                if (prop.Name == "password")
                {
                    continue;
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType == typeof(string))
                {
                    sb.AppendLine("\t\t\tif (" + prop.Name + " != null)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + "." + prop.Name + " == " + prop.Name + ");");
                    sb.AppendLine("\t\t\t}");
                }
                else if (propertyType == typeof(Guid))
                {
                    sb.AppendLine("\t\t\tif (" + prop.Name + ".HasValue == true)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + "." + prop.Name + " == " + prop.Name + ");");
                    sb.AppendLine("\t\t\t}");
                }
                else if (propertyType == typeof(int) || propertyType == typeof(long) || propertyType == typeof(DateTime) || propertyType == typeof(float) || propertyType == typeof(double) || propertyType == typeof(decimal))
                {
                    sb.AppendLine("\t\t\tif (" + prop.Name + ".HasValue == true)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + "." + prop.Name + " == " + prop.Name + ".Value);");
                    sb.AppendLine("\t\t\t}");
                }
                else if (propertyType == typeof(bool))
                {
                    if (prop.Name == "active" && ignoreFoundationServices == false)
                    {
                        // Only writers and admins can see inactive records, or filter by active.  Only admins can see deleted records, or filter by deleted.
                        sb.AppendLine("\t\t\tif (userIsWriter == true)");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t\tif (active.HasValue == true)");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".active == active.Value);");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine("\t\t\t");
                        if (scriptGenTable.HasField("deleted") == true)
                        {

                            sb.AppendLine("\t\t\t\tif (userIsAdmin == true)");
                            sb.AppendLine("\t\t\t\t{");
                            sb.AppendLine("\t\t\t\t\tif (deleted.HasValue == true)");
                            sb.AppendLine("\t\t\t\t\t{");
                            sb.AppendLine("\t\t\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".deleted == deleted.Value);");
                            sb.AppendLine("\t\t\t\t\t}");
                            sb.AppendLine("\t\t\t\t}");
                            sb.AppendLine("\t\t\t\telse");
                            sb.AppendLine("\t\t\t\t{");
                            sb.AppendLine("\t\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".deleted == false);");
                            sb.AppendLine("\t\t\t\t}");
                        }
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine("\t\t\telse");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".active == true);");
                        if (scriptGenTable.HasField("deleted") == true)
                        {
                            sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".deleted == false);");
                        }
                        sb.AppendLine("\t\t\t}");
                    }
                    else if (prop.Name == "deleted" && ignoreFoundationServices == false)
                    {
                        //
                        // Deleted handled in active handler.
                        //
                    }
                    else
                    {
                        //
                        // Treat as regular bool field
                        //
                        sb.AppendLine("\t\t\tif (" + prop.Name + ".HasValue == true)");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + "." + prop.Name + " == " + prop.Name + ".Value);");
                        sb.AppendLine("\t\t\t}");
                    }
                }
            }


            //
            // Add the any string contains parameter to the RowCount method
            //
            if (scriptGenTable.AddAnyStringContainsParameterToWebAPI == true)
            {
                AddAnyStringsContainsQueryAdditions(type, sb, rootNameSpace, false);
            }


            sb.AppendLine();
            sb.AppendLine("\t\t\tint output = await query.CountAsync(cancellationToken);");
            sb.AppendLine();
            sb.AppendLine("\t\t\treturn Ok(output);");
            sb.AppendLine("\t\t}");
            sb.AppendLine();


            if (scriptGenTable != null && scriptGenTable.webAPIGetRowCountToBeOverridden == true)
            {
                sb.AppendLine("*/");
            }

            #endregion

            #region HTTP_Get_Individual_handling


            sb.AppendLine();
            if (scriptGenTable != null && scriptGenTable.webAPIIdGetterToBeOverridden == true)
            {
                // comment out this function, but write the code anyway.
                sb.AppendLine("/* This function is expected to be overridden in a custom file");
            }


            sb.AppendLine($@"        /// <summary>
        /// 
        /// This gets a single {entityName} by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>");
            sb.AppendLine("\t\t[HttpGet]");
            sb.AppendLine("\t\t[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]");
            sb.AppendLine("\t\t[Route(\"api/" + singularForRouting + "/{id}\")]");

            // If we have image fields, then add a new parameters to provide a default image width for reducing the size of images in the get.
            if (hasImageFields == false)
            {
                sb.AppendLine("\t\tpublic async Task<IActionResult> Get" + pluralizeEntityForRouteForSomeTypeNames(entityName) + "(int id, bool includeRelations = true, CancellationToken cancellationToken = default)");
            }
            else
            {
                sb.AppendLine("\t\tpublic async Task<IActionResult> Get" + pluralizeEntityForRouteForSomeTypeNames(entityName) + "(int id, bool includeRelations = true, int? imageWidth = null, CancellationToken cancellationToken = default)");
            }


            sb.AppendLine("\t\t{");
            if (ignoreFoundationServices == false)
            {
                sb.AppendLine("\t\t\tStartAuditEventClock();");
                sb.AppendLine();


                GenerateReadRoleAndPermissionChecks(module, scriptGenTable, sb);

                //
                // No longer using entity data tokens for route auth.
                //
                //sb.AppendLine("\t\t\tif (await IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel.Read) == false)");
                //sb.AppendLine("\t\t\t{");
                //sb.AppendLine("\t\t\t   return Forbid();");
                //sb.AppendLine("\t\t\t}");
                //sb.AppendLine();


                sb.AppendLine();
                sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);");
                sb.AppendLine();
                sb.AppendLine("\t\t\tbool userIsWriter = await UserCanWriteAsync(securityUser, " + entityMinimumWritePermissionString + ", cancellationToken);");
                sb.AppendLine("\t\t\tbool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);");

                if (multiTenancyEnabled == true)
                {
                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine("\t\t\tbool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);");
                        sb.AppendLine("\t\t\t");
                    }
                    sb.AppendLine("\t\t\t");
                    sb.AppendLine(UserTenantGuidCommands(3));
                }
            }


            sb.AppendLine();
            sb.AppendLine("\t\t\ttry");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine($"\t\t\t\tIQueryable<{qualifiedEntity}> query = (from {acronym} in _context.{plural} where");

            if (hasActiveAndDeletedControlFields == true)
            {
                sb.AppendLine("\t\t\t\t\t\t\t(" + acronym + ".id == id) &&");
                sb.AppendLine("\t\t\t\t\t\t\t(userIsAdmin == true || " + acronym + ".deleted == false) &&");
                sb.AppendLine("\t\t\t\t\t\t\t(userIsWriter == true || " + acronym + ".active == true)");
            }
            else
            {
                sb.AppendLine("\t\t\t\t(" + acronym + ".id == id)");
            }

            sb.AppendLine("\t\t\t\t\tselect " + acronym + ");");
            sb.AppendLine();

            if (ignoreFoundationServices == false)
            {
                if (dataVisibilityEnabled == true)
                {
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t// Apply the constraints for the user's tenant and data visibility configuration.");
                    sb.AppendLine("\t\t\t\t//");
                    sb.AppendLine("\t\t\t\tquery = await AddTenantAndDataVisibilityConstraintsAsync(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin, cancellationToken);");
                    sb.AppendLine();
                }
                else if (multiTenancyEnabled == true)
                {
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\tquery = query.Where(x => x.tenantGuid == userTenantGuid);");
                }
            }

            // Add the includes
            sb.AppendLine("\t\t\t\tif (includeRelations == true)");
            sb.AppendLine("\t\t\t\t{");

            foreach (PropertyInfo prop in type.GetProperties())
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                // Is this a linked object type?
                if (propertyType != typeof(string) &&
                    propertyType != typeof(int) &&
                    propertyType != typeof(long) &&
                    propertyType != typeof(DateTime) &&
                    propertyType != typeof(DateOnly) &&
                    propertyType != typeof(TimeOnly) &&
                    propertyType != typeof(float) &&
                    propertyType != typeof(double) &&
                    propertyType != typeof(decimal) &&
                    propertyType != typeof(bool) &&
                    propertyType != typeof(Guid) &&
                    propertyType != typeof(byte[]) &&
                    propertyType.FullName.StartsWith("System.Collections") == false)
                {
                    sb.AppendLine("\t\t\t\t\tquery = query.Include(x => x." + prop.Name + ");");
                }
            }

            sb.AppendLine("\t\t\t\t\tquery = query.AsSplitQuery();");

            sb.AppendLine("\t\t\t\t}");
            sb.AppendLine();
            sb.AppendLine("\t\t\t\t" + qualifiedEntity + " materialized = await query.FirstOrDefaultAsync(cancellationToken);");
            sb.AppendLine();
            sb.AppendLine("\t\t\t\tif (materialized != null)");
            sb.AppendLine("\t\t\t\t{");


            if (HasBinaryStorageFields(scriptGenTable) == true)
            {
                sb.AppendLine("\t\t\t\t\tbool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\t\tif (diskBasedBinaryStorageMode == true &&");
                sb.AppendLine("\t\t\t\t\t    materialized." + dataRootFieldName + "Data == null &&");

                if (BinaryStorageFieldsAreNullable(scriptGenTable) == true)
                {
                    sb.AppendLine("\t\t\t\t\t    materialized." + dataRootFieldName + "Size.HasValue == true &&");
                    sb.AppendLine("\t\t\t\t\t    materialized." + dataRootFieldName + "Size.Value > 0)");
                }
                else
                {
                    sb.AppendLine("\t\t\t\t\t    materialized." + dataRootFieldName + "Size > 0)");
                }

                sb.AppendLine("\t\t\t\t\t{");
                if (versionControlEnabled == true)
                {
                    sb.AppendLine("\t\t\t\t\t    materialized." + dataRootFieldName + "Data = await LoadDataFromDiskAsync(materialized.objectGuid, materialized.versionNumber, \"" + dataFileNameExtension + "\", cancellationToken);");
                }
                else
                {
                    sb.AppendLine("\t\t\t\t\t    materialized." + dataRootFieldName + "Data = await LoadDataFromDiskAsync(materialized.objectGuid, 0, \"" + dataFileNameExtension + "\", cancellationToken);");

                }
                sb.AppendLine("\t\t\t\t\t}");
                sb.AppendLine();
            }

            sb.AppendLine("\t\t\t\t\t");
            sb.AppendLine("\t\t\t\t\t// Convert all the date properties to be of kind UTC.");

            if (ignoreFoundationServices == false)
            {
                sb.AppendLine("\t\t\t\t\tFoundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());"); // Note that this custom method needs to be on the Core context class.
            }
            else
            {
                sb.AppendLine("\t\t\t\t\t" + contextClassName + ".ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());"); // Note that this custom method needs to be on the Core context class.
            }


            sb.AppendLine();



            if (hasImageFields == true)
            {
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("\t\t\t\t\tif (imageWidth.HasValue == true)");
                sb.AppendLine("\t\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t\t// Resize the image data to a new width to possibly reduce the size of the data transferred.");
                sb.AppendLine("\t\t\t\t\t\t//");
                sb.AppendLine(@"                        Image img = Image.FromStream(new MemoryStream(materialized." + dataRootFieldName + @"Data));

                        Bitmap bmp = Foundation.ImagingHelper.ResizeImage(img, imageWidth.Value);

                        if (bmp != null)
                        {
                            byte[] resizedImageData = Foundation.ImageUtility.ConvertImageToByteArray(bmp);

                            materialized." + dataRootFieldName + @"Data = resizedImageData;
                            materialized." + dataRootFieldName + @"Size = resizedImageData.Length;
                        }");
                sb.AppendLine("\t\t\t\t\t}");
                sb.AppendLine();
            }



            if (dataVisibilityEnabled == false || ignoreFoundationServices == true)
            {
                if (ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? \"" + module + "." + entityName + " Entity was read with Admin privilege.\" : \"" + module + "." + entityName + " Entity was read.\");");

                    if (scriptGenTable != null && scriptGenTable.name.EndsWith("ChangeHistory") == false)
                    {
                        sb.AppendLine();
                        sb.AppendLine("\t\t\t\t\tBackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, \"" + entityName + "\", materialized.id, " + displayNameFieldSerializationCode.Replace(camelCaseName + ".", "materialized.") + "));");
                        sb.AppendLine();
                    }
                }

                sb.AppendLine();
                sb.AppendLine("\t\t\t\t\t// Create a new output object that only includes the relations if necessary, and doesn't include the empty list objects, so that we can reduce the amount of data being transferred.");
                sb.AppendLine("\t\t\t\t\tif (includeRelations == true)");
                sb.AppendLine("\t\t\t\t\t{");
                //sb.AppendLine($"\t\t\t\t\t\treturn Ok({databaseNamespace}.{entityName}.CreateAnonymousWithFirstLevelSubObjects(materialized));");
                sb.AppendLine($"\t\t\t\t\t\treturn Ok(materialized.ToOutputDTO());             // DTO with nav properties");
                sb.AppendLine("\t\t\t\t\t}");
                sb.AppendLine("\t\t\t\t\telse");
                sb.AppendLine("\t\t\t\t\t{");
                //sb.AppendLine($"\t\t\t\t\t\treturn Ok({databaseNamespace}.{entityName}.CreateAnonymous(materialized));");
                sb.AppendLine($"\t\t\t\t\t\treturn Ok(materialized.ToDTO());                   // DTO without nav properties");
                sb.AppendLine("\t\t\t\t\t}");
            }
            else
            {
                sb.AppendLine("\t\t\t\t\t" + entityName + "WithWritePermissionDetails " + camelCaseName + "WWP = new " + entityName + "WithWritePermissionDetails(materialized);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\t\tList<int> organizationsUserIsEntitledToWriteTo = null;");
                sb.AppendLine("\t\t\t\t\tList<int> departmentsUserIsEntitledToWriteTo = null;");
                sb.AppendLine("\t\t\t\t\tList<int> teamsUserIsEntitledToWriteTo = null;");
                sb.AppendLine("\t\t\t\t\tList<int> userAndTheirReportIds = null;");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\t\tList<int> organizationsUserIsEntitledToChangeOwnerFor = null;");
                sb.AppendLine("\t\t\t\t\tList<int> departmentsUserIsEntitledToChangeOwnerFor = null;");
                sb.AppendLine("\t\t\t\t\tList<int> teamsUserIsEntitledToChangeOwnerFor = null;");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\t\tList<int> organizationsUserIsEntitledToChangeHierarchyFor = null;");
                sb.AppendLine("\t\t\t\t\tList<int> departmentsUserIsEntitledToChangeHierarchyFor = null;");
                sb.AppendLine("\t\t\t\t\tList<int> teamsUserIsEntitledToChangeHierarchyFor = null;");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\t\tif (userIsAdmin == false)");
                sb.AppendLine("\t\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\t\torganizationsUserIsEntitledToWriteTo = await GetOrganizationIdsUserIsEntitledToWriteToAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\t\t\t\tdepartmentsUserIsEntitledToWriteTo = await GetDepartmentIdsUserIsEntitledToWriteToAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\t\t\t\tteamsUserIsEntitledToWriteTo = await GetTeamIdsUserIsEntitledToWriteToAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\t\t\t\tuserAndTheirReportIds = await GetListOfUserIdsForUserAndPeopleThatReportToThemAsync(securityUser, cancellationToken);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\t\t\torganizationsUserIsEntitledToChangeOwnerFor = await GetOrganizationIdsUserIsEntitledToChangeOwnerForAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\t\t\t\tdepartmentsUserIsEntitledToChangeOwnerFor = await GetDepartmentIdsUserIsEntitledToChangeOwnerForAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\t\t\t\tteamsUserIsEntitledToChangeOwnerFor = await GetTeamIdsUserIsEntitledToChangeOwnerForAsync(securityUser, cancellationToken);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\t\t\torganizationsUserIsEntitledToChangeHierarchyFor = await GetOrganizationIdsUserIsEntitledToChangeHierarchyForAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\t\t\t\tdepartmentsUserIsEntitledToChangeHierarchyFor = await GetDepartmentIdsUserIsEntitledToChangeHierarchyForAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\t\t\t\tteamsUserIsEntitledToChangeHierarchyFor = await GetTeamIdsUserIsEntitledToChangeHierarchyForAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\t\t\t}");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\t\tConfigureIsWriteable(" + camelCaseName + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, userAndTheirReportIds, organizationsUserIsEntitledToWriteTo, departmentsUserIsEntitledToWriteTo, teamsUserIsEntitledToWriteTo);");
                sb.AppendLine("\t\t\t\t\tConfigureOwnerIsChangeable(" + camelCaseName + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, organizationsUserIsEntitledToChangeOwnerFor, departmentsUserIsEntitledToChangeOwnerFor, teamsUserIsEntitledToChangeOwnerFor);");
                sb.AppendLine("\t\t\t\t\tConfigureHierarchyIsChangeable(" + camelCaseName + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, organizationsUserIsEntitledToChangeHierarchyFor, departmentsUserIsEntitledToChangeHierarchyFor, teamsUserIsEntitledToChangeHierarchyFor);");

                if (scriptGenTable != null && scriptGenTable.name.EndsWith("ChangeHistory") == false)
                {
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\tBackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, \"" + entityName + "\", " + camelCaseName + "WWP.id, " + displayNameFieldSerializationCode.Replace(camelCaseName, camelCaseName + "WWP") + "));");
                    sb.AppendLine();
                }

                sb.AppendLine("\t\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? \"" + module + "." + entityName + " Entity was read with Admin privilege.\" : \"" + module + "." + entityName + " Entity was read.\");");


                sb.AppendLine();
                sb.AppendLine("\t\t\t\t\t// Create a new output object that only includes the relations if necessary, and doesn't include the empty list objects, so that we can reduce the amount of data being transferred.");
                sb.AppendLine("\t\t\t\t\tif (includeRelations == true)");
                sb.AppendLine("\t\t\t\t\t{");
                sb.AppendLine($"\t\t\t\t\t\treturn Ok({databaseNamespace}.{entityName}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName}WWP));");
                sb.AppendLine("\t\t\t\t\t}");
                sb.AppendLine("\t\t\t\t\telse");
                sb.AppendLine("\t\t\t\t\t{");
                sb.AppendLine($"\t\t\t\t\t\treturn Ok({databaseNamespace}.{entityName}.CreateAnonymous({camelCaseName}WWP));");
                sb.AppendLine("\t\t\t\t\t}");
            }


            sb.AppendLine("\t\t\t\t}");
            sb.AppendLine("\t\t\t\telse");
            sb.AppendLine("\t\t\t\t{");
            if (ignoreFoundationServices == false)
            {
                sb.AppendLine("\t\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.Error, \"Attempt to read a " + module + "." + entityName + " entity that does not exist.\", id.ToString());");
            }
            sb.AppendLine("\t\t\t\t\treturn BadRequest();");
            sb.AppendLine("\t\t\t\t}");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t\tcatch (Exception ex)");
            sb.AppendLine("\t\t\t{");
            if (ignoreFoundationServices == false)
            {
                sb.AppendLine("\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? \"Exception caught during entity read of " + module + "." + entityName + ".   Entity was read with Admin privilege.\" : \"Exception caught during entity read of " + module + "." + entityName + ".\", id.ToString(), ex);");
            }
            sb.AppendLine("\t\t\t\treturn Problem(ex.ToString());");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");

            if (scriptGenTable != null && scriptGenTable.webAPIIdGetterToBeOverridden == true)
            {
                sb.AppendLine("*/");
            }

            sb.AppendLine();
            sb.AppendLine();


            #endregion

            #region HTTP_Put_Post_Handling

            if (tableIsWritable == true)
            {
                if (scriptGenTable != null && scriptGenTable.webAPIPutToBeOverridden == true)
                {
                    // comment out this function, but write the code anyway.
                    sb.AppendLine("/* This function is expected to be overridden in a custom file");
                }


                sb.AppendLine($@"		/// <summary>
		/// 
		/// This updates an existing {entityName} record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>");
                sb.AppendLine("\t\t[Route(\"api/" + singularForRouting + "/{id}\")]");
                sb.AppendLine("\t\t[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]");
                sb.AppendLine("\t\t[HttpPost]");
                sb.AppendLine("\t\t[HttpPut]");

                if (scriptGenTable.maxPostBytes != null && scriptGenTable.maxPostBytes > 0)
                {
                    sb.AppendLine($"\t\t[RequestSizeLimit({scriptGenTable.maxPostBytes})]");
                }

                sb.AppendLine("\t\tpublic async Task<IActionResult> Put" + pluralizeEntityForRouteForSomeTypeNames(entityName) + "(int id, [FromBody]" + qualifiedEntity + "." + entityName + "DTO " + camelCaseName + "DTO, CancellationToken cancellationToken = default)");
                sb.AppendLine("\t\t{");

                sb.AppendLine("\t\t\tif (" + camelCaseName + "DTO == null)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t   return BadRequest();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();

                if (ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\tStartAuditEventClock();");
                    sb.AppendLine();

                    GenerateWriteRoleAndPermissionChecks(module, scriptGenTable, sb, adminAccessNeededToWrite);

                    sb.AppendLine();

                    //
                    // No longer using entity data tokens for route auth.
                    //
                    //sb.AppendLine("\t\t\tif (await IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel.Write) == false)");
                    //sb.AppendLine("\t\t\t{");
                    //sb.AppendLine("\t\t\t   return Forbid();");
                    //sb.AppendLine("\t\t\t}");
                    //sb.AppendLine();

                }


                sb.AppendLine();
                sb.AppendLine("\t\t\tif (id != " + camelCaseName + "DTO.id)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\treturn BadRequest();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();


                if (ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tbool userIsWriter = await UserCanWriteAsync(securityUser, " + entityMinimumWritePermissionString + ", cancellationToken);");
                    sb.AppendLine("\t\t\tbool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);");

                    if (multiTenancyEnabled == true)
                    {
                        if (dataVisibilityEnabled == true)
                        {
                            sb.AppendLine("\t\t\tbool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);");
                            sb.AppendLine("\t\t\t");
                        }
                        sb.AppendLine(UserTenantGuidCommands(3));
                        sb.AppendLine();
                    }
                }


                sb.AppendLine("\t\t\tIQueryable<" + qualifiedEntity + "> query = (from x in _context." + plural);
                sb.AppendLine("\t\t\t\twhere");
                sb.AppendLine("\t\t\t\t(x.id == id)");
                sb.AppendLine("\t\t\t\tselect x);");
                sb.AppendLine();

                if (ignoreFoundationServices == false)
                {
                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine();
                        sb.AppendLine("\t\t\t//");
                        sb.AppendLine("\t\t\t// Apply the constraints for the user's tenant and data visibility configuration to ensure that this user can load the record that is attempting to be saved.");
                        sb.AppendLine("\t\t\t//");
                        sb.AppendLine("\t\t\tquery = await AddTenantAndDataVisibilityConstraintsAsync(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin, cancellationToken);");
                        sb.AppendLine();
                    }
                    else if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine("\t\t\tquery = query.Where(x => x.tenantGuid == userTenantGuid);");
                    }
                }

                sb.AppendLine();
                sb.AppendLine("\t\t\t" + qualifiedEntity + " existing = await query.FirstOrDefaultAsync(cancellationToken);");

                sb.AppendLine();
                sb.AppendLine("\t\t\tif (existing == null)");
                sb.AppendLine("\t\t\t{");

                if (ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, \"Invalid primary key provided for " + module + "." + entityName + " PUT\", id.ToString(), new Exception(\"No " + module + "." + entityName + " entity could be found with the primary key provided.\"));");
                }

                sb.AppendLine("\t\t\t\treturn NotFound();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();


                //
                // Add object guid validation and rehydrating of it if the table has the object guid field.
                //
                if (scriptGenTable.HasField("objectGuid") == true)
                {
                    sb.AppendLine($@"
            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if ({camelCaseName}DTO.objectGuid == Guid.Empty)
            {{
                {camelCaseName}DTO.objectGuid = existing.objectGuid;
            }}
            else if ({camelCaseName}DTO.objectGuid != existing.objectGuid)
            {{");
                    if (ignoreFoundationServices == false)
                    {
                        sb.AppendLine(@$"                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $""Attempt was made to change object guid on a {entityName} record.  This is not allowed.  The User is "" + securityUser.accountName, existing.id.ToString());");
                    }
                    sb.AppendLine(@$"                return Problem(""Invalid Operation."");
            }}

");
                }

                sb.AppendLine("\t\t\t// Copy the existing object so it can be serialized as-is in the audit and history logs.");
                sb.AppendLine("\t\t\t" + qualifiedEntity + " cloneOfExisting = (" + qualifiedEntity + ")_context.Entry(existing).GetDatabaseValues().ToObject();");
                sb.AppendLine();


                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\t// Create a new " + entityName + " object using the data from the existing record, updated with what is in the DTO.");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\t" + qualifiedEntity + " " + camelCaseName + " = (" + qualifiedEntity + ")_context.Entry(existing).GetDatabaseValues().ToObject();");
                sb.AppendLine("\t\t\t" + camelCaseName + ".ApplyDTO(" + camelCaseName + "DTO);");


                //
                // Check the tenant guid here.
                //
                if (multiTenancyEnabled == true)
                {
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// The tenant guid for any " + entityName + " being saved must match the tenant guid of the user.  ");
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\tif (existing.tenantGuid != userTenantGuid)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.Error, \"Attempt was made to save a record with a tenant guid that is not the user's tenant guid.\", false);");
                    sb.AppendLine("\t\t\t\treturn Problem(\"Data integrity violation detected while attempting to save.\");");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine("\t\t\telse");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\t// Assign the tenantGuid to the " + entityName + " because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.");
                    sb.AppendLine("\t\t\t\t" + camelCaseName + ".tenantGuid = existing.tenantGuid;");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();

                }


                if (dataVisibilityEnabled == true && ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\tUser user = await GetUserAsync(userTenantGuid, securityUser, cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tif (user == null)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t    // If we don't have a user record, then the data visibility sync is probably not working..");
                    sb.AppendLine("\t\t\t    await CreateAuditEventAsync(AuditEngine.AuditType.Error, \"Cannot proceed with " + entityName + " PUT because Unable to find user record for security user with account name of \" + securityUser.accountName + \".Check the data visibilty sync process.\", false);");
                    sb.AppendLine("\t\t\t    return Problem(\"Unable to proceed with " + entityName + " save because inconsistency with user record was found.\");");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();

                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// Validate that the data visibility settings are correct.");
                    sb.AppendLine("\t\t\t//");

                    sb.AppendLine("\t\t\tList<int> organizationIdsUserIsEntitledToChangeOwnerFor = null;");
                    sb.AppendLine("\t\t\tList<int> departmentIdsUserIsEntitledToChangeOwnerFor = null;");
                    sb.AppendLine("\t\t\tList<int> teamIdsUserIsEntitledToChangeOwnerFor = null;");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tList<int> organizationIdsUserIsEntitledToChangeHierarchyFor = null;");
                    sb.AppendLine("\t\t\tList<int> departmentIdsUserIsEntitledToChangeHierarchyFor = null;");
                    sb.AppendLine("\t\t\tList<int> teamIdsUserIsEntitledToChangeHierarchyFor = null;");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tList<int> organizationIdsUserIsEntitledToWriteTo = null;");
                    sb.AppendLine("\t\t\tList<int> departmentIdsUserIsEntitledToWriteTo = null;");
                    sb.AppendLine("\t\t\tList<int> teamIdsUserIsEntitledToWriteTo = null;");
                    sb.AppendLine("\t\t\tList<int> userAndTheirReportIds = null;");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tif (userIsAdmin == false)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\torganizationIdsUserIsEntitledToChangeOwnerFor = await GetOrganizationIdsUserIsEntitledToChangeOwnerForAsync(securityUser, cancellationToken);");
                    sb.AppendLine("\t\t\t\tdepartmentIdsUserIsEntitledToChangeOwnerFor = await GetDepartmentIdsUserIsEntitledToChangeOwnerForAsync(securityUser, cancellationToken);");
                    sb.AppendLine("\t\t\t\tteamIdsUserIsEntitledToChangeOwnerFor = await GetTeamIdsUserIsEntitledToChangeOwnerForAsync(securityUser, cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\torganizationIdsUserIsEntitledToChangeHierarchyFor = await GetOrganizationIdsUserIsEntitledToChangeHierarchyForAsync(securityUser, cancellationToken);");
                    sb.AppendLine("\t\t\t\tdepartmentIdsUserIsEntitledToChangeHierarchyFor = await GetDepartmentIdsUserIsEntitledToChangeHierarchyForAsync(securityUser, cancellationToken);");
                    sb.AppendLine("\t\t\t\tteamIdsUserIsEntitledToChangeHierarchyFor = await GetTeamIdsUserIsEntitledToChangeHierarchyForAsync(securityUser, cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\torganizationIdsUserIsEntitledToWriteTo = await GetOrganizationIdsUserIsEntitledToWriteToAsync(securityUser, cancellationToken);");
                    sb.AppendLine("\t\t\t\tdepartmentIdsUserIsEntitledToWriteTo = await GetDepartmentIdsUserIsEntitledToWriteToAsync(securityUser, cancellationToken);");
                    sb.AppendLine("\t\t\t\tteamIdsUserIsEntitledToWriteTo = await GetTeamIdsUserIsEntitledToWriteToAsync(securityUser, cancellationToken);");
                    sb.AppendLine("\t\t\t\tuserAndTheirReportIds = await GetListOfUserIdsForUserAndPeopleThatReportToThemAsync(securityUser, cancellationToken);");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();
                }


                string optionalTab = "";
                if (versionControlEnabled == true)
                {
                    sb.AppendLine("\t\t\tlock (" + camelCaseName + "PutSyncRoot)");
                    sb.AppendLine("\t\t\t{");
                    optionalTab = "\t";

                    sb.AppendLine(optionalTab + "\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t// Validate the version number for the " + camelCaseName + " being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.");
                    sb.AppendLine(optionalTab + "\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\tif (existing.versionNumber != " + camelCaseName + ".versionNumber)");
                    sb.AppendLine(optionalTab + "\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t// Record has changed");
                    sb.AppendLine(optionalTab + "\t\t\t\tCreateAuditEvent(AuditEngine.AuditType.Miscellaneous, \"" + entityName + " save attempt was made but save request was with version \" + " + camelCaseName + ".versionNumber + \" and the current version number is \" + existing.versionNumber, false);");
                    sb.AppendLine(optionalTab + "\t\t\t\treturn Problem(\"The " + entityName + " you are trying to update has already changed.  Please try your save again after reloading the " + entityName + ".\");");
                    sb.AppendLine(optionalTab + "\t\t\t}");
                    sb.AppendLine(optionalTab + "\t\t\telse");
                    sb.AppendLine(optionalTab + "\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t// Same record.  Increase version.");
                    sb.AppendLine(optionalTab + "\t\t\t\t" + camelCaseName + ".versionNumber++;");
                    sb.AppendLine(optionalTab + "\t\t\t}");
                    sb.AppendLine();
                }


                sb.AppendLine();

                if (dataVisibilityEnabled == true && ignoreFoundationServices == false)
                {
                    sb.AppendLine(optionalTab + "\t\t\ttry");
                    sb.AppendLine(optionalTab + "\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t// only admins can nullify data visibility state on an existing record.");
                    sb.AppendLine(optionalTab + "\t\t\t\t// ");
                    sb.AppendLine(optionalTab + "\t\t\t\t// Throw an error if a non-admin user is trying to change the data visibility state to be null on any level if the existing record is not null at that same level.");
                    sb.AppendLine(optionalTab + "\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\tif (userIsAdmin == false &&");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t(existing.organizationId.HasValue == true && " + camelCaseName + ".organizationId.HasValue == false ||");
                    sb.AppendLine(optionalTab + "\t\t\t\t\texisting.departmentId.HasValue == true && " + camelCaseName + ".departmentId.HasValue == false ||");
                    sb.AppendLine(optionalTab + "\t\t\t\t\texisting.teamId.HasValue == true && " + camelCaseName + ".teamId.HasValue == false");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t))");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\tCreateAuditEvent(AuditEngine.AuditType.Error, \"Attempt was made to nullify one or more data visibilty field by a non admin user.\", false);");
                    sb.AppendLine(optionalTab + "\t\t\t\t\treturn Problem(\"Data integrity violation detected while attempting to save record.  Data visibility fields state is invalid.\");");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine();

                    sb.AppendLine(optionalTab + "\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t// Step 1 - Get the original owner and hierarchy atrributes");
                    sb.AppendLine(optionalTab + "\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\tTeam originalTeam = null;");
                    sb.AppendLine(optionalTab + "\t\t\t\tDepartment originalDepartment = null;");
                    sb.AppendLine(optionalTab + "\t\t\t\tOrganization originalOrganization = null;");
                    sb.AppendLine(optionalTab + "\t\t\t\tUser originalOwner = null;");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\tif (existing.teamId.HasValue == true)");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\toriginalTeam = GetTeam(existing.teamId.Value);");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\tif (existing.departmentId.HasValue == true)");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\toriginalDepartment = GetDepartment(existing.departmentId.Value);");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");
                    sb.AppendLine(optionalTab + "\t\t\t\telse");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t// Set the existing department to be that of the existing team if no existing department is explicitly provided and we have an existing team.");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t\tif (originalTeam != null)");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\toriginalDepartment = GetDepartment(originalTeam.departmentId);");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t}");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\tif (existing.organizationId.HasValue == true)");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\toriginalOrganization = GetOrganization(existing.organizationId.Value);");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");
                    sb.AppendLine(optionalTab + "\t\t\t\telse");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t// Set the organization to be that of the department if no organization is explicitly provided and we have a department");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t\tif (originalDepartment != null)");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\toriginalOrganization = GetOrganization(originalDepartment.organizationId);");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t}");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\tif (existing.userId != null)");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\toriginalOwner = GetUser(existing.userId.Value);");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t// Get the new owner and data visibilty entities.");
                    sb.AppendLine(optionalTab + "\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\tOrganization newOrganization = null;");
                    sb.AppendLine(optionalTab + "\t\t\t\tDepartment newDepartment = null;");
                    sb.AppendLine(optionalTab + "\t\t\t\tTeam newTeam = null;");
                    sb.AppendLine(optionalTab + "\t\t\t\tUser newOwner = null;");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t// step 2 - Load the data visibility entities, and fill in any blanks we can calculate on this record.");
                    sb.AppendLine(optionalTab + "\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\tif (" + camelCaseName + ".userId.HasValue == true)");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\tnewOwner = GetUser(" + camelCaseName + ".userId.Value);");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\tif (" + camelCaseName + ".teamId.HasValue == true)");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\tnewTeam = GetTeam(" + camelCaseName + ".teamId.Value);");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\tif (" + camelCaseName + ".departmentId.HasValue == true)");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\tnewDepartment = GetDepartment(" + camelCaseName + ".departmentId.Value);");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");
                    sb.AppendLine(optionalTab + "\t\t\t\telse");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t// Set the department to be that of the team if no department is explicitly provided and we have a team.");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t\tif (newTeam != null)");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\t" + camelCaseName + ".departmentId = newTeam.departmentId;");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\tnewDepartment = GetDepartment(newTeam.departmentId);");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t}");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\tif (" + camelCaseName + ".organizationId.HasValue == true)");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\tnewOrganization = GetOrganization(" + camelCaseName + ".organizationId.Value);");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");
                    sb.AppendLine(optionalTab + "\t\t\t\telse");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t// Set the organization to be that of the department if no organization is explicitly provided and we have a department");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t\tif (newDepartment != null)");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\t" + camelCaseName + ".organizationId = newDepartment.organizationId;");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\tnewOrganization = GetOrganization(newDepartment.organizationId);");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t}");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t// Step 3 - Make sure that the new hierarchy of data visibility objects make sense.");
                    sb.AppendLine(optionalTab + "\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\tVerifyIntegrityOfDataVisibilityEntities(newTeam, newDepartment, newOrganization);");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t// Step 4 verify that if the ownership is being changed, then the user making the change has the correct permission to do so.  Admins can always change ownership.");
                    sb.AppendLine(optionalTab + "\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\tif (userIsAdmin == false)");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t// Has the userId/Owner changed?");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t\tif (existing.userId != " + camelCaseName + ".userId)");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\t// Make sure that the current user can change owner ship at the hierarchy level of the 'existing' record state - NOT THE NEW STATE.");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\tVerifyDataChangeOwnerPrivilege(originalTeam, originalDepartment, originalOrganization, teamIdsUserIsEntitledToChangeOwnerFor, departmentIdsUserIsEntitledToChangeOwnerFor, organizationIdsUserIsEntitledToChangeOwnerFor);");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t}");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t// Step 5 verify that if the hierarchy position is being changed, then the user making the change has the correct permission to be able to do so.  Admins can always change the hierarchy.");
                    sb.AppendLine(optionalTab + "\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\tif (userIsAdmin == false)");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t// Has the position in the hierarchy changed?  If so, verify that the user is allowed to change it.");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t\tif (existing.organizationId != " + camelCaseName + ".organizationId ||");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\texisting.departmentId != " + camelCaseName + ".departmentId ||");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\texisting.teamId != " + camelCaseName + ".teamId)");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\tVerifyDataChangeHierarchyPrivilege(originalTeam, originalDepartment, originalOrganization, teamIdsUserIsEntitledToChangeHierarchyFor, departmentIdsUserIsEntitledToChangeHierarchyFor, organizationIdsUserIsEntitledToChangeHierarchyFor);");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t}");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t// Step 6 - Make sure that this user can write to the new data visibility entities.  However, if they are an admin then they can write anywhere.");
                    sb.AppendLine(optionalTab + "\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\tif (userIsAdmin == false)");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\tVerifyDataWritePrivilegeForUpdate(newOwner, newTeam, newDepartment, newOrganization, userAndTheirReportIds, teamIdsUserIsEntitledToWriteTo, departmentIdsUserIsEntitledToWriteTo, organizationIdsUserIsEntitledToWriteTo);");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");

                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t}");
                    sb.AppendLine(optionalTab + "\t\t\tcatch (Exception ex)");
                    sb.AppendLine(optionalTab + "\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\tCreateAuditEvent(AuditEngine.AuditType.Error, \"Error caught while setting or validating tenant or data visibility fields on a PUT operation.\", false, \"\", JsonSerializer.Serialize(" + camelCaseName + "), null, ex);");
                    sb.AppendLine(optionalTab + "\t\t\t\treturn Problem(\"Error caught while setting or validating tenant or data visibility fields on a PUT operation.\");");
                    sb.AppendLine(optionalTab + "\t\t\t}");
                    sb.AppendLine();
                }


                if (hasActiveAndDeletedControlFields == true)
                {
                    sb.AppendLine(optionalTab + "\t\t\t// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?");
                    sb.AppendLine(optionalTab + "\t\t\tif (userIsAdmin == false && (" + camelCaseName + ".deleted == true || existing.deleted == true))");
                    sb.AppendLine(optionalTab + "\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t// we're not recording state here because it is not being changed.");
                    if (ignoreFoundationServices == false)
                    {
                        sb.AppendLine(optionalTab + "\t\t\t\tCreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, \"Attempt to delete a record or work on a deleted " + module + "." + entityName + " record.\", id.ToString());");
                        sb.AppendLine(optionalTab + "\t\t\t\tDestroySessionAndAuthentication();");
                        sb.AppendLine(optionalTab + "\t\t\t\treturn Forbid();");
                    }
                    else
                    {
                        sb.AppendLine(optionalTab + "\t\t\t\treturn Problem(\"Cannot modify record\");");
                    }
                    sb.AppendLine(optionalTab + "\t\t\t}");
                }


                sb.AppendLine();

                foreach (PropertyInfo prop in type.GetProperties())
                {
                    // Check for [NotMapped] attribute
                    if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                    {
                        continue; // Skip properties with [NotMapped]
                    }

                    Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    if (propertyType == typeof(DateTime))
                    {
                        if (commentWritten == false)
                        {
                            sb.AppendLine(optionalTab + "\t\t\t//");
                            sb.AppendLine(optionalTab + "\t\t\t// Fix any non-UTC date values that come in.");
                            sb.AppendLine(optionalTab + "\t\t\t//");
                            commentWritten = true;
                        }

                        if (prop.PropertyType.Name == "Nullable`1")
                        {
                            sb.AppendLine(optionalTab + "\t\t\tif (" + camelCaseName + "." + prop.Name + ".HasValue == true && " + camelCaseName + "." + prop.Name + ".Value.Kind != DateTimeKind.Utc)");
                            sb.AppendLine(optionalTab + "\t\t\t{");
                            sb.AppendLine(optionalTab + "\t\t\t\t" + camelCaseName + "." + prop.Name + " = " + camelCaseName + "." + prop.Name + ".Value.ToUniversalTime();");
                            sb.AppendLine(optionalTab + "\t\t\t}");
                            sb.AppendLine();
                        }
                        else
                        {
                            sb.AppendLine(optionalTab + "\t\t\tif (" + camelCaseName + "." + prop.Name + ".Kind != DateTimeKind.Utc)");
                            sb.AppendLine(optionalTab + "\t\t\t{");
                            sb.AppendLine(optionalTab + "\t\t\t\t" + camelCaseName + "." + prop.Name + " = " + camelCaseName + "." + prop.Name + ".ToUniversalTime();");
                            sb.AppendLine(optionalTab + "\t\t\t}");
                            sb.AppendLine();
                        }
                    }
                    else if (propertyType == typeof(string))
                    {
                        if (scriptGenTable != null)
                        {
                            DatabaseGenerator.Database.Table.Field scriptGenField = null;

                            foreach (DatabaseGenerator.Database.Table.Field fld in scriptGenTable.fields)
                            {
                                if (fld.name == prop.Name)
                                {
                                    scriptGenField = fld;
                                    break;
                                }
                            }

                            //
                            // If the field is a string, ensure that the contents will fit
                            //
                            if (scriptGenField != null && scriptGenField.MaxStringLength().HasValue == true)
                            {
                                sb.AppendLine(optionalTab + "\t\t\tif (" + camelCaseName + "." + prop.Name + " != null && " + camelCaseName + "." + prop.Name + ".Length > " + scriptGenField.MaxStringLength().ToString() + ")");
                                sb.AppendLine(optionalTab + "\t\t\t{");
                                sb.AppendLine(optionalTab + "\t\t\t\t" + camelCaseName + "." + prop.Name + " = " + camelCaseName + "." + prop.Name + ".Substring(0, " + scriptGenField.MaxStringLength().ToString() + ");");
                                sb.AppendLine(optionalTab + "\t\t\t}");
                                sb.AppendLine();
                            }
                        }
                    }
                }


                if (HasBinaryStorageFields(scriptGenTable) == true)
                {

                    if (BinaryStorageFieldsAreNullable(scriptGenTable) == true)
                    {
                        sb.AppendLine();
                        sb.AppendLine("\t\t\t\t//");
                        sb.AppendLine("\t\t\t\t// Add default values for any missing data attribute fields.");
                        sb.AppendLine("\t\t\t\t//");

                        sb.AppendLine("\t\t\t\tif (" + camelCaseName + "." + dataRootFieldName + "Data != null && string.IsNullOrEmpty(" + camelCaseName + "." + dataRootFieldName + "FileName))");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "FileName = " + camelCaseName + ".objectGuid.ToString() + \"." + dataFileNameExtension + "\";");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();

                        sb.AppendLine("\t\t\t\tif (" + camelCaseName + "." + dataRootFieldName + "Data != null && (" + camelCaseName + "." + dataRootFieldName + "Size.HasValue == false || " + camelCaseName + "." + dataRootFieldName + "Size != " + camelCaseName + "." + dataRootFieldName + "Data.Length))");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Size = " + camelCaseName + "." + dataRootFieldName + "Data.Length;");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();

                        sb.AppendLine("\t\t\t\tif (" + camelCaseName + "." + dataRootFieldName + "Data != null && string.IsNullOrEmpty(" + camelCaseName + "." + dataRootFieldName + "MimeType))");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "MimeType = \"" + dataDefaultMimeType + "\";");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.AppendLine();
                        sb.AppendLine("\t\t\t\t//");
                        sb.AppendLine("\t\t\t\t// Add default values for any missing data attribute fields.");
                        sb.AppendLine("\t\t\t\t//");

                        sb.AppendLine("\t\t\t\tif (" + camelCaseName + "." + dataRootFieldName + "Data != null && string.IsNullOrEmpty(" + camelCaseName + "." + dataRootFieldName + "FileName))");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "FileName = " + camelCaseName + ".objectGuid.ToString() + \"." + dataFileNameExtension + "\";");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();

                        sb.AppendLine("\t\t\t\tif (" + camelCaseName + "." + dataRootFieldName + "Data != null && " + camelCaseName + "." + dataRootFieldName + "Size != " + camelCaseName + "." + dataRootFieldName + "Data.Length)");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Size = " + camelCaseName + "." + dataRootFieldName + "Data.Length;");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();

                        sb.AppendLine("\t\t\t\tif (" + camelCaseName + "." + dataRootFieldName + "Data != null && string.IsNullOrEmpty(" + camelCaseName + "." + dataRootFieldName + "MimeType))");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "MimeType = \"" + dataDefaultMimeType + "\";");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();
                    }


                    sb.AppendLine("\t\t\t\tbool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();");
                    sb.AppendLine();
                }

                if (versionControlEnabled == false)
                {
                    if (HasBinaryStorageFields(scriptGenTable) == true)
                    {
                        sb.AppendLine(optionalTab + "\t\t\t\tbyte[] dataReferenceBeforeClearing = " + camelCaseName + "." + dataRootFieldName + "Data;");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\tif (diskBasedBinaryStorageMode == true &&");
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "FileName != null &&");
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Data != null &&");

                        if (BinaryStorageFieldsAreNullable(scriptGenTable) == true)
                        {
                            sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Size.HasValue == true &&");
                            sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Size.Value > 0)");
                        }
                        else
                        {
                            sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Size > 0)");
                        }

                        sb.AppendLine(optionalTab + "\t\t\t\t{");

                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    // write the bytes to disk");
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    WriteDataToDisk(" + camelCaseName + ".objectGuid, 0, " + camelCaseName + "." + dataRootFieldName + "Data, \"" + dataFileNameExtension + "\");");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    // Clear the data from the object before we put it into the db");
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Data = null;");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\t}");
                        sb.AppendLine();
                    }

                    sb.AppendLine(optionalTab + $"\t\t\tEntityEntry<{qualifiedEntity}> attached = _context.Entry(existing);");
                    sb.AppendLine(optionalTab + "\t\t\tattached.CurrentValues.SetValues(" + camelCaseName + ");");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\ttry");
                    sb.AppendLine(optionalTab + "\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\tawait _context.SaveChangesAsync(cancellationToken);");

                    if (ignoreFoundationServices == false)
                    {
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t\"" + module + "." + entityName + " entity successfully updated.\",");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "true,");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "id.ToString(),");
                        sb.AppendLine($"{optionalTab}\t\t\t\t\tJsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),");
                        sb.AppendLine($"{optionalTab}\t\t\t\t\tJsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName})),");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "null);");
                        sb.AppendLine();

                    }

                    sb.AppendLine();

                    if (dataVisibilityEnabled == true && ignoreFoundationServices == false)
                    {
                        sb.AppendLine(optionalTab + "\t\t\t\t" + entityName + "WithWritePermissionDetails " + camelCaseName + "WWP = new " + entityName + "WithWritePermissionDetails(" + camelCaseName + ");");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\tConfigureIsWriteable(" + camelCaseName + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, userAndTheirReportIds, organizationIdsUserIsEntitledToWriteTo, departmentIdsUserIsEntitledToWriteTo, teamIdsUserIsEntitledToWriteTo);");
                        sb.AppendLine(optionalTab + "\t\t\t\tConfigureOwnerIsChangeable(" + camelCaseName + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, organizationIdsUserIsEntitledToChangeOwnerFor, departmentIdsUserIsEntitledToChangeOwnerFor, teamIdsUserIsEntitledToChangeOwnerFor);");
                        sb.AppendLine(optionalTab + "\t\t\t\tConfigureHierarchyIsChangeable(" + camelCaseName + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, organizationIdsUserIsEntitledToChangeHierarchyFor, departmentIdsUserIsEntitledToChangeHierarchyFor, teamIdsUserIsEntitledToChangeHierarchyFor);");
                        sb.AppendLine();
                        if (HasBinaryStorageFields(scriptGenTable) == true)
                        {
                            sb.AppendLine();
                            sb.AppendLine(optionalTab + "\t\t\t\tif (diskBasedBinaryStorageMode == true)");
                            sb.AppendLine(optionalTab + "\t\t\t\t{");
                            sb.AppendLine(optionalTab + "\t\t\t\t    //");
                            sb.AppendLine(optionalTab + "\t\t\t\t    // Put the data bytes back into the object that will be returned.");
                            sb.AppendLine(optionalTab + "\t\t\t\t    //");
                            sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Data = dataReferenceBeforeClearing;");
                            sb.AppendLine(optionalTab + "\t\t\t\t}");

                        }
                        sb.AppendLine($"{optionalTab}\t\t\t\treturn Ok({databaseNamespace}.{entityName}.CreateAnonymous({camelCaseName}WWP));");
                    }
                    else
                    {

                        if (HasBinaryStorageFields(scriptGenTable) == true)
                        {
                            sb.AppendLine();
                            sb.AppendLine(optionalTab + "\t\t\t\tif (diskBasedBinaryStorageMode == true)");
                            sb.AppendLine(optionalTab + "\t\t\t\t{");
                            sb.AppendLine(optionalTab + "\t\t\t\t    //");
                            sb.AppendLine(optionalTab + "\t\t\t\t    // Put the data bytes back into the object that will be returned.");
                            sb.AppendLine(optionalTab + "\t\t\t\t    //");
                            sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Data = dataReferenceBeforeClearing;");
                            sb.AppendLine(optionalTab + "\t\t\t\t}");

                        }
                        sb.AppendLine($"{optionalTab}\t\t\t\treturn Ok({databaseNamespace}.{entityName}.CreateAnonymous({camelCaseName}));");
                    }
                }
                else
                {
                    if (ignoreFoundationServices == true)
                    {
                        throw new Exception("Version history not support if ignoring foundation services.");
                    }

                    //
                    // Version control is on, so add the change history creation.
                    //
                    sb.AppendLine(optionalTab + "\t\t\ttry");
                    sb.AppendLine(optionalTab + "\t\t\t{");

                    if (HasBinaryStorageFields(scriptGenTable) == true)
                    {
                        sb.AppendLine(optionalTab + "\t\t\t\tbyte[] dataReferenceBeforeClearing = " + camelCaseName + "." + dataRootFieldName + "Data;");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\tif (diskBasedBinaryStorageMode == true &&");
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "FileName != null &&");
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Data != null &&");

                        if (BinaryStorageFieldsAreNullable(scriptGenTable) == true)
                        {

                            sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Size.HasValue == true &&");
                            sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Size.Value > 0)");
                        }
                        else
                        {
                            sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Size > 0)");
                        }
                        sb.AppendLine(optionalTab + "\t\t\t\t{");

                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    // write the bytes to disk");
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    WriteDataToDisk(" + camelCaseName + ".objectGuid, " + camelCaseName + ".versionNumber, " + camelCaseName + "." + dataRootFieldName + "Data, \"" + dataFileNameExtension + "\");");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    // Clear the data from the object before we put it into the db");
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Data = null;");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\t}");
                        sb.AppendLine();
                    }


                    sb.AppendLine(optionalTab + $"\t\t\t    EntityEntry<{qualifiedEntity}> attached = _context.Entry(existing);");
                    sb.AppendLine(optionalTab + "\t\t\t    attached.CurrentValues.SetValues(" + camelCaseName + ");");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())");
                    sb.AppendLine(optionalTab + "\t\t\t    {");
                    sb.AppendLine(optionalTab + "\t\t\t        _context.SaveChanges();");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t        //");
                    sb.AppendLine(optionalTab + "\t\t\t        // Now add the change history");
                    sb.AppendLine(optionalTab + "\t\t\t        //");
                    sb.AppendLine(optionalTab + "\t\t\t        " + entityName + "ChangeHistory " + camelCaseName + "ChangeHistory = new " + entityName + "ChangeHistory();");

                    // Note we are using the reserved word version of camel case here to suit EFPT's naming convention.  this may need to be tweaked later if that changes..
                    sb.AppendLine(optionalTab + "\t\t\t        " + camelCaseName + "ChangeHistory." + CamelCase(entityName, true) + "Id = " + camelCaseName + ".id;");

                    sb.AppendLine(optionalTab + "\t\t\t        " + camelCaseName + "ChangeHistory.versionNumber = " + camelCaseName + ".versionNumber;");
                    sb.AppendLine(optionalTab + "\t\t\t        " + camelCaseName + "ChangeHistory.timeStamp = DateTime.UtcNow;");


                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine(optionalTab + "\t\t\t        " + camelCaseName + "ChangeHistory.userId = user.id;");
                    }
                    else
                    {
                        sb.AppendLine(optionalTab + "\t\t\t        " + camelCaseName + "ChangeHistory.userId = securityUser.id;");
                    }

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine(optionalTab + "\t\t\t        " + camelCaseName + "ChangeHistory.tenantGuid = userTenantGuid;");
                    }

                    sb.AppendLine($"{optionalTab}\t\t\t        {camelCaseName}ChangeHistory.data = JsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName}));");
                    sb.AppendLine(optionalTab + "\t\t\t        _context." + entityName + "ChangeHistories.Add(" + camelCaseName + "ChangeHistory);");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t        _context.SaveChanges();");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t        transaction.Commit();");
                    sb.AppendLine(optionalTab + "\t\t\t    }");

                    if (HasBinaryStorageFields(scriptGenTable) == true)
                    {
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\tif (diskBasedBinaryStorageMode == true)");
                        sb.AppendLine(optionalTab + "\t\t\t\t{");
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    // Put the data bytes back into the object that will be returned.");
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Data = dataReferenceBeforeClearing;");
                        sb.AppendLine(optionalTab + "\t\t\t\t}");

                    }

                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\tCreateAuditEvent(AuditEngine.AuditType.UpdateEntity,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\"" + module + "." + entityName + " entity successfully updated.\",");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "true,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "id.ToString(),");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tJsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tJsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName})),");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "null);");
                    sb.AppendLine();

                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine(optionalTab + "\t\t\t\t" + entityName + "WithWritePermissionDetails " + camelCaseName + "WWP = new " + entityName + "WithWritePermissionDetails(" + camelCaseName + ");");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\tConfigureIsWriteable(" + camelCaseName + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, userAndTheirReportIds, organizationIdsUserIsEntitledToWriteTo, departmentIdsUserIsEntitledToWriteTo, teamIdsUserIsEntitledToWriteTo);");
                        sb.AppendLine(optionalTab + "\t\t\t\tConfigureOwnerIsChangeable(" + camelCaseName + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, organizationIdsUserIsEntitledToChangeOwnerFor, departmentIdsUserIsEntitledToChangeOwnerFor, teamIdsUserIsEntitledToChangeOwnerFor);");
                        sb.AppendLine(optionalTab + "\t\t\t\tConfigureHierarchyIsChangeable(" + camelCaseName + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, organizationIdsUserIsEntitledToChangeHierarchyFor, departmentIdsUserIsEntitledToChangeHierarchyFor, teamIdsUserIsEntitledToChangeHierarchyFor);");
                        sb.AppendLine();
                        sb.AppendLine($"{optionalTab}\t\t\t\treturn Ok({databaseNamespace}.{entityName}.CreateAnonymous({camelCaseName}WWP));");
                    }
                    else
                    {
                        sb.AppendLine($"{optionalTab}\t\t\treturn Ok({databaseNamespace}.{entityName}.CreateAnonymous({camelCaseName}));");
                    }
                }

                sb.AppendLine(optionalTab + "\t\t\t}");
                sb.AppendLine(optionalTab + "\t\t\tcatch (Exception ex)");
                sb.AppendLine(optionalTab + "\t\t\t{");

                if (ignoreFoundationServices == false)
                {
                    sb.AppendLine(optionalTab + "\t\t\t\tCreateAuditEvent(AuditEngine.AuditType.UpdateEntity,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\"" + module + "." + entityName + " entity update failed\",");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "false,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "id.ToString(),");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tJsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tJsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName})),");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "ex);");
                    sb.AppendLine();
                }
                sb.AppendLine(optionalTab + "\t\t\t\treturn Problem(ex.Message);");
                sb.AppendLine(optionalTab + "\t\t\t}");
                sb.AppendLine();


                if (versionControlEnabled == true)
                {
                    sb.AppendLine(optionalTab + "\t\t}");
                }


                sb.AppendLine("\t\t}");

                if (scriptGenTable != null && scriptGenTable.webAPIPutToBeOverridden == true)
                {
                    sb.AppendLine("*/");
                }

                sb.AppendLine();

                #endregion

                #region HTTP_Post_handling


                if (scriptGenTable != null && scriptGenTable.webAPIPostToBeOverridden == true)
                {
                    // comment out this function, but write the code anyway.
                    sb.AppendLine("/* This function is expected to be overridden in a custom file");
                }

                sb.AppendLine($@"        /// <summary>
        /// 
        /// This creates a new {entityName} record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>");
                sb.AppendLine("\t\t[HttpPost]");
                sb.AppendLine("\t\t[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]");
                sb.AppendLine("\t\t[Route(\"api/" + singularForRouting + "\", Name = \"" + pluralizeEntityForRouteForSomeTypeNames(entityName) + "\")]");

                if (scriptGenTable.maxPostBytes != null && scriptGenTable.maxPostBytes > 0)
                {
                    sb.AppendLine($"\t\t[RequestSizeLimit({scriptGenTable.maxPostBytes})]");
                }

                sb.AppendLine("\t\tpublic async Task<IActionResult> Post" + pluralizeEntityForRouteForSomeTypeNames(entityName) + "([FromBody]" + qualifiedEntity + "." + entityName + "DTO " + camelCaseName + "DTO, CancellationToken cancellationToken = default)");
                sb.AppendLine("\t\t{");

                sb.AppendLine("\t\t\tif (" + camelCaseName + "DTO == null)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t   return BadRequest();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();

                if (ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\tStartAuditEventClock();");
                    sb.AppendLine();

                    GenerateWriteRoleAndPermissionChecks(module, scriptGenTable, sb, adminAccessNeededToWrite);


                    sb.AppendLine();
                    //sb.AppendLine("\t\t\tif (await IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel.Write) == false)");
                    //sb.AppendLine("\t\t\t{");
                    //sb.AppendLine("\t\t\t   return Forbid();");
                    //sb.AppendLine("\t\t\t}");
                    //sb.AppendLine();

                    if (HasBinaryStorageFields(scriptGenTable) == true && BinaryStorageFieldsAreNullable(scriptGenTable) == false)
                    {
                        sb.AppendLine("\t\t\tif (" + camelCaseName + "DTO." + dataRootFieldName + "Data == null)");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t	return BadRequest(\"No data\");");
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine();
                    }

                    sb.AppendLine();
                    sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);");
                    sb.AppendLine();

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine("\t\t\tbool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);");

                        if (dataVisibilityEnabled == true)
                        {
                            sb.AppendLine("\t\t\tbool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);");
                            sb.AppendLine("\t\t\t");
                        }
                        sb.AppendLine(UserTenantGuidCommands(3));
                    }
                }

                if (dataVisibilityEnabled == true && ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\tList<int> organizationIdsUserIsEntitledToChangeOwnerFor = null;");
                    sb.AppendLine("\t\t\tList<int> departmentIdsUserIsEntitledToChangeOwnerFor = null;");
                    sb.AppendLine("\t\t\tList<int> teamIdsUserIsEntitledToChangeOwnerFor = null;");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tList<int> organizationIdsUserIsEntitledToChangeHierarchyFor = null;");
                    sb.AppendLine("\t\t\tList<int> departmentIdsUserIsEntitledToChangeHierarchyFor = null;");
                    sb.AppendLine("\t\t\tList<int> teamIdsUserIsEntitledToChangeHierarchyFor = null;");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tList<int> organizationIdsUserIsEntitledToWriteTo = null;");
                    sb.AppendLine("\t\t\tList<int> departmentIdsUserIsEntitledToWriteTo = null;");
                    sb.AppendLine("\t\t\tList<int> teamIdsUserIsEntitledToWriteTo = null;");
                    sb.AppendLine("\t\t\tList<int> userAndTheirReportIds = null;");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tif (userIsAdmin == false)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\torganizationIdsUserIsEntitledToChangeOwnerFor = await GetOrganizationIdsUserIsEntitledToChangeOwnerForAsync(securityUser, cancellationToken);");
                    sb.AppendLine("\t\t\t\tdepartmentIdsUserIsEntitledToChangeOwnerFor = await GetDepartmentIdsUserIsEntitledToChangeOwnerForAsync(securityUser, cancellationToken);");
                    sb.AppendLine("\t\t\t\tteamIdsUserIsEntitledToChangeOwnerFor = await GetTeamIdsUserIsEntitledToChangeOwnerForAsync(securityUser, cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\torganizationIdsUserIsEntitledToChangeHierarchyFor = await GetOrganizationIdsUserIsEntitledToChangeHierarchyForAsync(securityUser, cancellationToken);");
                    sb.AppendLine("\t\t\t\tdepartmentIdsUserIsEntitledToChangeHierarchyFor = await GetDepartmentIdsUserIsEntitledToChangeHierarchyForAsync(securityUser, cancellationToken);");
                    sb.AppendLine("\t\t\t\tteamIdsUserIsEntitledToChangeHierarchyFor = await GetTeamIdsUserIsEntitledToChangeHierarchyForAsync(securityUser, cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\torganizationIdsUserIsEntitledToWriteTo = await GetOrganizationIdsUserIsEntitledToWriteToAsync(securityUser, cancellationToken);");
                    sb.AppendLine("\t\t\t\tdepartmentIdsUserIsEntitledToWriteTo = await GetDepartmentIdsUserIsEntitledToWriteToAsync(securityUser, cancellationToken);");
                    sb.AppendLine("\t\t\t\tteamIdsUserIsEntitledToWriteTo = await GetTeamIdsUserIsEntitledToWriteToAsync(securityUser, cancellationToken);");
                    sb.AppendLine("\t\t\t\tuserAndTheirReportIds = await GetListOfUserIdsForUserAndPeopleThatReportToThemAsync(securityUser, cancellationToken);");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();
                }


                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\t// Create a new " + entityName + " object using the data from the DTO");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\t" + qualifiedEntity + " " + camelCaseName + " = " + qualifiedEntity + ".FromDTO(" + camelCaseName + "DTO);");
                sb.AppendLine();

                sb.AppendLine("\t\t\ttry");
                sb.AppendLine("\t\t\t{");


                if (dataVisibilityEnabled == true && ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\t\tUser user = await GetUserAsync(userTenantGuid, securityUser, cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\tif (user == null)");
                    sb.AppendLine(optionalTab + "\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t// If we don't have a user record, then the data visibility sync is probably not working..");
                    sb.AppendLine(optionalTab + "\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.Error, \"Cannot proceed with " + entityName + " POST because Unable to find user record for security user with account name of \" + securityUser.accountName + \".Check the data visibilty sync process.\", false);");
                    sb.AppendLine(optionalTab + "\t\t\t\treturn Problem(\"Unable to proceed with " + entityName + " save because inconsistency with user record was found.\");");
                    sb.AppendLine(optionalTab + "\t\t\t}");
                    sb.AppendLine();

                    sb.AppendLine("\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t// Ensure that the tenant data is correct.");
                    sb.AppendLine("\t\t\t\t//");
                    sb.AppendLine("\t\t\t\ttry");
                    sb.AppendLine("\t\t\t\t{");

                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t// Assign the owner user id and the tenant from the current user.");
                    sb.AppendLine("\t\t\t\t\t// ");
                    sb.AppendLine("\t\t\t\t\t" + camelCaseName + ".userId = user.id;");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t" + camelCaseName + ".tenantGuid = userTenantGuid;");
                    sb.AppendLine();

                    sb.AppendLine("\t\t\t\t\tList<Organization> organizationsUserIsEntitledToWriteTo = await GetOrganizationsUserIsEntitledToWriteToAsync(securityUser, cancellationToken);");
                    sb.AppendLine("\t\t\t\t\tList<Department> departmentsUserIsEntitledToWriteTo = await GetDepartmentsUserIsEntitledToWriteToAsync(securityUser, cancellationToken);");
                    sb.AppendLine("\t\t\t\t\tList<Team> teamsUserIsEntitledToWriteTo = await GetTeamsUserIsEntitledToWriteToAsync(securityUser, cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t// If the new " + camelCaseName + " has no data visibility attributes set, then use the user's defaults.");
                    sb.AppendLine("\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\tif (" + camelCaseName + ".organizationId.HasValue == false &&");
                    sb.AppendLine("\t\t\t\t\t\t" + camelCaseName + ".departmentId.HasValue == false &&");
                    sb.AppendLine("\t\t\t\t\t\t" + camelCaseName + ".teamId.HasValue == false)");
                    sb.AppendLine("\t\t\t\t\t{");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\t// Using the default data visibility properties on the security user record, resolve the security side entities into the application side entities.");
                    sb.AppendLine("\t\t\t\t\t\t// ");
                    sb.AppendLine("\t\t\t\t\t\t// Test the default values against what the user can write to, to make sure that the rules are respected.");
                    sb.AppendLine("\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\tTeam defaultTeam = GetDefaultTeam(teamsUserIsEntitledToWriteTo, securityUser);");
                    sb.AppendLine("\t\t\t\t\t\tDepartment defaultDepartment = GetDefaultDepartment(departmentsUserIsEntitledToWriteTo, securityUser, defaultTeam);");
                    sb.AppendLine("\t\t\t\t\t\tOrganization defaultOrganization = GetDefaultOrganization(organizationsUserIsEntitledToWriteTo, securityUser, defaultDepartment);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\t// In case the user can't write to the higher levels, get those entities for verification and defaulting purposes.");
                    sb.AppendLine("\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\tif (defaultTeam != null && defaultDepartment == null)");
                    sb.AppendLine("\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\tdefaultDepartment = await GetDepartmentAsync(defaultTeam.departmentId, cancellationToken);");
                    sb.AppendLine("\t\t\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t\tif (defaultDepartment != null && defaultOrganization == null)");
                    sb.AppendLine("\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\tdefaultOrganization = await GetOrganizationAsync(defaultDepartment.organizationId, cancellationToken);");
                    sb.AppendLine("\t\t\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\t// Additional Integrity checking.");
                    sb.AppendLine("\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\tVerifyIntegrityOfDataVisibilityEntities(defaultTeam, defaultDepartment, defaultOrganization);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\t// Use the user's defaults, if there are any.");
                    sb.AppendLine("\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\tif (defaultOrganization != null)");
                    sb.AppendLine("\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\t" + camelCaseName + ".organizationId = defaultOrganization.id;");
                    sb.AppendLine("\t\t\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t\tif (defaultDepartment != null)");
                    sb.AppendLine("\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\t" + camelCaseName + ".departmentId = defaultDepartment.id;");
                    sb.AppendLine("\t\t\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t\tif (defaultTeam != null)");
                    sb.AppendLine("\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\t" + camelCaseName + ".teamId = defaultTeam.id;");
                    sb.AppendLine("\t\t\t\t\t\t}");
                    sb.AppendLine("\t\t\t\t\t}");
                    sb.AppendLine("\t\t\t\t\telse");
                    sb.AppendLine("\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\t// New " + camelCaseName + " has Data Visibility properties already set.  Validate that they are OK.");
                    sb.AppendLine("\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\tOrganization organization = null;");
                    sb.AppendLine("\t\t\t\t\t\tDepartment department = null;");
                    sb.AppendLine("\t\t\t\t\t\tTeam team = null;");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t\t// ");
                    sb.AppendLine("\t\t\t\t\t\t// step 1 - Load the data visibility entities, and fill in any blanks we can calculate on this record.");
                    sb.AppendLine("\t\t\t\t\t\t// ");
                    sb.AppendLine("\t\t\t\t\t\tif (" + camelCaseName + ".teamId.HasValue == true)");
                    sb.AppendLine("\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\tteam = await GetTeamAsync(" + camelCaseName + ".teamId.Value, cancellationToken);");
                    sb.AppendLine("\t\t\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t\tif (" + camelCaseName + ".departmentId.HasValue == true)");
                    sb.AppendLine("\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\tdepartment = await GetDepartmentAsync(" + camelCaseName + ".departmentId.Value, cancellationToken);");
                    sb.AppendLine("\t\t\t\t\t\t}");
                    sb.AppendLine("\t\t\t\t\t\telse");
                    sb.AppendLine("\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\t\t// Set the department to be that of the team if no department is explicitly provided and we have a team");
                    sb.AppendLine("\t\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\t\tif (team != null)");
                    sb.AppendLine("\t\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\t\t" + camelCaseName + ".departmentId = team.departmentId;");
                    sb.AppendLine("\t\t\t\t\t\t\t\tdepartment = await GetDepartmentAsync(team.departmentId, cancellationToken);");
                    sb.AppendLine("\t\t\t\t\t\t\t}");
                    sb.AppendLine("\t\t\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t\tif (" + camelCaseName + ".organizationId.HasValue == true)");
                    sb.AppendLine("\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\torganization = await GetOrganizationAsync(" + camelCaseName + ".organizationId.Value, cancellationToken);");
                    sb.AppendLine("\t\t\t\t\t\t}");
                    sb.AppendLine("\t\t\t\t\t\telse");
                    sb.AppendLine("\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\t\t// Set the organization to be that of the department if no organization is explicitly provided and we have a department");
                    sb.AppendLine("\t\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\t\tif (department != null)");
                    sb.AppendLine("\t\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\t\t" + camelCaseName + ".organizationId = department.organizationId;");
                    sb.AppendLine("\t\t\t\t\t\t\t\torganization = await GetOrganizationAsync(department.organizationId, cancellationToken);");
                    sb.AppendLine("\t\t\t\t\t\t\t}");
                    sb.AppendLine("\t\t\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\t// Step 2 - Make sure that the hierarchy of data visibility objects make sense");
                    sb.AppendLine("\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\tVerifyIntegrityOfDataVisibilityEntities(team, department, organization);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\t// Step 3 - Make sure that this user can write to the data visibility entities.  However, if they are an application admin then they can write anywhere.");
                    sb.AppendLine("\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\tif (userIsAdmin == false)");
                    sb.AppendLine("\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\tVerifyDataWritePrivilegeForAdd(team, department, organization, teamIdsUserIsEntitledToWriteTo, departmentIdsUserIsEntitledToWriteTo, organizationIdsUserIsEntitledToWriteTo);");
                    sb.AppendLine("\t\t\t\t\t\t}");
                    sb.AppendLine("\t\t\t\t\t}");

                    sb.AppendLine("\t\t\t\t}");
                    sb.AppendLine("\t\t\t\tcatch (Exception ex)");
                    sb.AppendLine("\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.Error, \"Error caught while setting or validating data visibility fields on a POST operation.\", false, \"\", JsonSerializer.Serialize(" + camelCaseName + "), null, ex);");
                    sb.AppendLine("\t\t\t\t\treturn Problem(\"Error caught while setting or validating data visibility fields on a POST operation.\");");
                    sb.AppendLine("\t\t\t\t}");
                    sb.AppendLine();
                }
                else if (multiTenancyEnabled == true && ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t// Ensure that the tenant data is correct.");
                    sb.AppendLine("\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t" + camelCaseName + ".tenantGuid = userTenantGuid;");
                    sb.AppendLine();
                }


                foreach (PropertyInfo prop in type.GetProperties())
                {
                    // Check for [NotMapped] attribute
                    if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                    {
                        continue; // Skip properties with [NotMapped]
                    }

                    Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    if (propertyType == typeof(DateTime))
                    {
                        if (commentWritten == false)
                        {
                            sb.AppendLine("\t\t\t\t//");
                            sb.AppendLine("\t\t\t\t// Fix any non-UTC date values that come in.");
                            sb.AppendLine("\t\t\t\t//");
                            commentWritten = true;
                        }

                        if (prop.PropertyType.Name == "Nullable`1")
                        {
                            sb.AppendLine("\t\t\t\tif (" + camelCaseName + "." + prop.Name + ".HasValue == true && " + camelCaseName + "." + prop.Name + ".Value.Kind != DateTimeKind.Utc)");
                            sb.AppendLine("\t\t\t\t{");
                            sb.AppendLine("\t\t\t\t\t" + camelCaseName + "." + prop.Name + " = " + camelCaseName + "." + prop.Name + ".Value.ToUniversalTime();");
                            sb.AppendLine("\t\t\t\t}");
                            sb.AppendLine();
                        }
                        else
                        {
                            sb.AppendLine("\t\t\t\tif (" + camelCaseName + "." + prop.Name + ".Kind != DateTimeKind.Utc)");
                            sb.AppendLine("\t\t\t\t{");
                            sb.AppendLine("\t\t\t\t\t" + camelCaseName + "." + prop.Name + " = " + camelCaseName + "." + prop.Name + ".ToUniversalTime();");
                            sb.AppendLine("\t\t\t\t}");
                            sb.AppendLine();
                        }
                    }
                    else if (propertyType == typeof(string))
                    {
                        if (scriptGenTable != null)
                        {
                            DatabaseGenerator.Database.Table.Field scriptGenField = null;

                            foreach (DatabaseGenerator.Database.Table.Field fld in scriptGenTable.fields)
                            {
                                if (fld.name == prop.Name)
                                {
                                    scriptGenField = fld;
                                    break;
                                }
                            }

                            //
                            // If the field is a string, ensure that the contents will fit
                            //
                            if (scriptGenField != null && scriptGenField.MaxStringLength().HasValue == true)
                            {
                                sb.AppendLine("\t\t\t\tif (" + camelCaseName + "." + prop.Name + " != null && " + camelCaseName + "." + prop.Name + ".Length > " + scriptGenField.MaxStringLength().ToString() + ")");
                                sb.AppendLine("\t\t\t\t{");
                                sb.AppendLine("\t\t\t\t\t" + camelCaseName + "." + prop.Name + " = " + camelCaseName + "." + prop.Name + ".Substring(0, " + scriptGenField.MaxStringLength().ToString() + ");");
                                sb.AppendLine("\t\t\t\t}");
                                sb.AppendLine();
                            }
                        }
                    }
                }

                foreach (PropertyInfo prop in type.GetProperties())
                {
                    if (prop.Name == "objectGuid")
                    {
                        sb.AppendLine("\t\t\t\t" + camelCaseName + ".objectGuid = Guid.NewGuid();");
                    }
                }

                if (HasBinaryStorageFields(scriptGenTable) == true)
                {
                    if (BinaryStorageFieldsAreNullable(scriptGenTable) == true)
                    {
                        sb.AppendLine();
                        sb.AppendLine("\t\t\t\t//");
                        sb.AppendLine("\t\t\t\t// Add default values for any missing data attribute fields.");
                        sb.AppendLine("\t\t\t\t//");

                        sb.AppendLine("\t\t\t\tif (" + camelCaseName + "." + dataRootFieldName + "Data != null && string.IsNullOrEmpty(" + camelCaseName + "." + dataRootFieldName + "FileName))");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "FileName = " + camelCaseName + ".objectGuid.ToString() + \"." + dataFileNameExtension + "\";");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();

                        sb.AppendLine("\t\t\t\tif (" + camelCaseName + "." + dataRootFieldName + "Data != null && (" + camelCaseName + "." + dataRootFieldName + "Size.HasValue == false || " + camelCaseName + "." + dataRootFieldName + "Size != " + camelCaseName + "." + dataRootFieldName + "Data.Length))");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Size = " + camelCaseName + "." + dataRootFieldName + "Data.Length;");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();

                        sb.AppendLine("\t\t\t\tif (" + camelCaseName + "." + dataRootFieldName + "Data != null && string.IsNullOrEmpty(" + camelCaseName + "." + dataRootFieldName + "MimeType))");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "MimeType = \"" + dataDefaultMimeType + "\";");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.AppendLine();
                        sb.AppendLine("\t\t\t\t//");
                        sb.AppendLine("\t\t\t\t// Add default values for any missing data attribute fields.");
                        sb.AppendLine("\t\t\t\t//");

                        sb.AppendLine("\t\t\t\tif (" + camelCaseName + "." + dataRootFieldName + "Data != null && string.IsNullOrEmpty(" + camelCaseName + "." + dataRootFieldName + "FileName))");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "FileName = " + camelCaseName + ".objectGuid.ToString() + \"." + dataFileNameExtension + "\";");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();

                        sb.AppendLine("\t\t\t\tif (" + camelCaseName + "." + dataRootFieldName + "Data != null && " + camelCaseName + "." + dataRootFieldName + "Size != " + camelCaseName + "." + dataRootFieldName + "Data.Length)");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Size = " + camelCaseName + "." + dataRootFieldName + "Data.Length;");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();

                        sb.AppendLine("\t\t\t\tif (" + camelCaseName + "." + dataRootFieldName + "Data != null && string.IsNullOrEmpty(" + camelCaseName + "." + dataRootFieldName + "MimeType))");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "MimeType = \"" + dataDefaultMimeType + "\";");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();
                    }
                }

                if (versionControlEnabled == false)
                {
                    if (HasBinaryStorageFields(scriptGenTable) == true)
                    {

                        sb.AppendLine("\t\t\t\tbool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\t\tbyte[] dataReferenceBeforeClearing = " + camelCaseName + "." + dataRootFieldName + "Data;");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\tif (diskBasedBinaryStorageMode == true &&");
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Data != null &&");
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "FileName != null &&");

                        if (BinaryStorageFieldsAreNullable(scriptGenTable) == true)
                        {
                            sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Size.HasValue == true &&");
                            sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Size.Value > 0)");
                        }
                        else
                        {
                            sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Size > 0)");
                        }

                        sb.AppendLine(optionalTab + "\t\t\t\t{");

                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    // write the bytes to disk");
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    await WriteDataToDiskAsync(" + camelCaseName + ".objectGuid, 0, " + camelCaseName + "." + dataRootFieldName + "Data, \"" + dataFileNameExtension + "\", cancellationToken);");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    // Clear the data from the object before we put it into the db");
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Data = null;");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\t}");
                        sb.AppendLine();

                    }

                    sb.AppendLine("\t\t\t\t_context." + plural + ".Add(" + camelCaseName + ");");
                    sb.AppendLine("\t\t\t\tawait _context.SaveChangesAsync(cancellationToken);");

                    if (ignoreFoundationServices == false)
                    {
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t\"" + module + "." + entityName + " entity successfully created.\",");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "true,");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + camelCaseName + ".id.ToString(),");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t\"\",");
                        sb.AppendLine($"{optionalTab}\t\t\t\t\tJsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName})),");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "null);");
                        sb.AppendLine();

                    }

                    if (HasBinaryStorageFields(scriptGenTable) == true)
                    {
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\tif (diskBasedBinaryStorageMode == true)");
                        sb.AppendLine(optionalTab + "\t\t\t{");
                        sb.AppendLine(optionalTab + "\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t    // Put the data bytes back into the object that will be returned.");
                        sb.AppendLine(optionalTab + "\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Data = dataReferenceBeforeClearing;");
                        sb.AppendLine(optionalTab + "\t\t\t}");
                        sb.AppendLine();

                    }

                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine("\t\t\tcatch (Exception ex)");
                    sb.AppendLine("\t\t\t{");
                    if (ignoreFoundationServices == false)
                    {
                        sb.AppendLine("\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, \"" + module + "." + entityName + " entity creation failed.\", false, " + camelCaseName + ".id.ToString(), \"\", JsonSerializer.Serialize(" + camelCaseName + "), ex);");
                        sb.AppendLine();
                    }
                    sb.AppendLine("\t\t\t\treturn Problem(ex.Message);");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();
                }
                else
                {

                    if (ignoreFoundationServices == true)
                    {
                        throw new Exception("Cannot use version control if ignoring foundation services.");

                    }

                    sb.AppendLine("\t\t\t\t" + camelCaseName + ".versionNumber = 1;");
                    sb.AppendLine();

                    if (HasBinaryStorageFields(scriptGenTable) == true)
                    {
                        sb.AppendLine("\t\t\t\tbool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();");
                        sb.AppendLine();

                        sb.AppendLine(optionalTab + "\t\t\tbyte[] dataReferenceBeforeClearing = " + camelCaseName + "." + dataRootFieldName + "Data;");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\tif (diskBasedBinaryStorageMode == true &&");
                        sb.AppendLine(optionalTab + "\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Data != null &&");
                        sb.AppendLine(optionalTab + "\t\t\t    " + camelCaseName + "." + dataRootFieldName + "FileName != null &&");

                        if (BinaryStorageFieldsAreNullable(scriptGenTable) == true)
                        {
                            sb.AppendLine(optionalTab + "\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Size.HasValue == true &&");
                            sb.AppendLine(optionalTab + "\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Size.Value > 0)");
                        }
                        else
                        {
                            sb.AppendLine(optionalTab + "\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Size > 0)");
                        }

                        sb.AppendLine(optionalTab + "\t\t\t{");

                        sb.AppendLine(optionalTab + "\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t    // write the bytes to disk");
                        sb.AppendLine(optionalTab + "\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t    await WriteDataToDiskAsync(" + camelCaseName + ".objectGuid, " + camelCaseName + ".versionNumber, " + camelCaseName + "." + dataRootFieldName + "Data, \"" + dataFileNameExtension + "\", cancellationToken);");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t    // Clear the data from the object before we put it into the db");
                        sb.AppendLine(optionalTab + "\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Data = null;");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t}");
                        sb.AppendLine();

                    }


                    sb.AppendLine("\t\t\t\t_context." + plural + ".Add(" + camelCaseName + ");");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\tawait using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))");
                    sb.AppendLine("\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t    await _context.SaveChangesAsync(cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t    //");
                    sb.AppendLine("\t\t\t\t    // Now add the change history");
                    sb.AppendLine("\t\t\t\t    //");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t    //");
                    sb.AppendLine("\t\t\t\t    // Detach the " + camelCaseName + " object so that no further changes will be written to the database");
                    sb.AppendLine("\t\t\t\t    //");
                    sb.AppendLine("\t\t\t\t    _context.Entry(" + camelCaseName + ").State = EntityState.Detached;");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t    //");
                    sb.AppendLine("\t\t\t\t    // Nullify all object properties before serializing.");
                    sb.AppendLine("\t\t\t\t    //");

                    foreach (PropertyInfo prop in type.GetProperties())
                    {
                        // Check for [NotMapped] attribute
                        if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                        {
                            continue; // Skip properties with [NotMapped]
                        }

                        Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                        // Is this an object type?
                        if (propertyType != typeof(string) &&
                            propertyType != typeof(int) &&
                            propertyType != typeof(long) &&
                            propertyType != typeof(DateTime) &&
                            propertyType != typeof(TimeOnly) &&
                            propertyType != typeof(DateOnly) &&
                            propertyType != typeof(float) &&
                            propertyType != typeof(double) &&
                            propertyType != typeof(decimal) &&
                            propertyType != typeof(bool) &&
                            propertyType != typeof(Guid))
                        {
                            sb.AppendLine("\t\t\t\t\t" + camelCaseName + "." + prop.Name + " = null;");
                        }
                    }

                    sb.AppendLine();

                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t    " + entityName + "ChangeHistory " + camelCaseName + "ChangeHistory = new " + entityName + "ChangeHistory();");

                    // Note we are using the reserved word handling version of camel case name here to accommodate EFPTs behaviour.  Might need to adjust later..
                    sb.AppendLine("\t\t\t\t    " + camelCaseName + "ChangeHistory." + CamelCase(entityName, true) + "Id = " + camelCaseName + ".id;");

                    sb.AppendLine("\t\t\t\t    " + camelCaseName + "ChangeHistory.versionNumber = " + camelCaseName + ".versionNumber;");
                    sb.AppendLine("\t\t\t\t    " + camelCaseName + "ChangeHistory.timeStamp = DateTime.UtcNow;");

                    //
                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine("\t\t\t\t    " + camelCaseName + "ChangeHistory.userId = user.id;");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t\t    " + camelCaseName + "ChangeHistory.userId = securityUser.id;");
                    }

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine(optionalTab + "\t\t\t    " + camelCaseName + "ChangeHistory.tenantGuid = userTenantGuid;");
                    }


                    sb.AppendLine($"\t\t\t\t    {camelCaseName}ChangeHistory.data = JsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName}));");
                    sb.AppendLine("\t\t\t\t    _context." + entityName + "ChangeHistories.Add(" + camelCaseName + "ChangeHistory);");
                    sb.AppendLine("\t\t\t\t    await _context.SaveChangesAsync(cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t    await transaction.CommitAsync(cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\"" + module + "." + entityName + " entity successfully created.\",");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "true,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + camelCaseName + ". id.ToString(),");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\"\",");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tJsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName})),");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "null);");
                    sb.AppendLine();

                    sb.AppendLine();

                    if (HasBinaryStorageFields(scriptGenTable) == true)
                    {
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\tif (diskBasedBinaryStorageMode == true)");
                        sb.AppendLine(optionalTab + "\t\t\t\t{");
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    // Put the data bytes back into the object that will be returned.");
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + camelCaseName + "." + dataRootFieldName + "Data = dataReferenceBeforeClearing;");
                        sb.AppendLine(optionalTab + "\t\t\t\t}");
                        sb.AppendLine();

                    }
                    sb.AppendLine("\t\t\t\t}");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine("\t\t\tcatch (Exception ex)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine($"\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, \"{module}.{entityName} entity creation failed.\", false, {camelCaseName}.id.ToString(), \"\", JsonSerializer.Serialize({databaseNamespace}.{entityName}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName})), ex);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\treturn Problem(ex.Message);");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();
                }

                if (dataVisibilityEnabled == true && ignoreFoundationServices == false)
                {

                    sb.AppendLine("\t\t\t" + entityName + "WithWritePermissionDetails " + camelCaseName + "WWP = new " + entityName + "WithWritePermissionDetails(" + camelCaseName + ");");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tConfigureIsWriteable(" + camelCaseName + "WWP, securityUser, userTenantGuid, userIsAdmin, true, userAndTheirReportIds, organizationIdsUserIsEntitledToWriteTo, departmentIdsUserIsEntitledToWriteTo, teamIdsUserIsEntitledToWriteTo);");
                    sb.AppendLine("\t\t\tConfigureOwnerIsChangeable(" + camelCaseName + "WWP, securityUser, userTenantGuid, userIsAdmin, true, organizationIdsUserIsEntitledToChangeOwnerFor, departmentIdsUserIsEntitledToChangeOwnerFor, teamIdsUserIsEntitledToChangeOwnerFor);");
                    sb.AppendLine("\t\t\tConfigureHierarchyIsChangeable(" + camelCaseName + "WWP, securityUser, userTenantGuid, userIsAdmin, true, organizationIdsUserIsEntitledToChangeHierarchyFor, departmentIdsUserIsEntitledToChangeHierarchyFor, teamIdsUserIsEntitledToChangeHierarchyFor);");
                    sb.AppendLine();

                    if (scriptGenTable != null && scriptGenTable.name.EndsWith("ChangeHistory") == false)
                    {
                        sb.AppendLine("\t\t\tBackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, \"" + entityName + "\", " + camelCaseName + ".id, " + displayNameFieldSerializationCode + "));");
                        sb.AppendLine();
                    }
                    sb.AppendLine($"\t\t\treturn CreatedAtRoute(\"{pluralizeEntityForRouteForSomeTypeNames(entityName)}\", new {{ id = {camelCaseName}.id }}, {databaseNamespace}.{entityName}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName}WWP));");
                }
                else
                {
                    sb.AppendLine();
                    if (ignoreFoundationServices == false)
                    {
                        if (scriptGenTable != null && scriptGenTable.name.EndsWith("ChangeHistory") == false)
                        {
                            sb.AppendLine("\t\t\tBackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, \"" + entityName + "\", " + camelCaseName + ".id, " + displayNameFieldSerializationCode + "));");
                            sb.AppendLine();
                        }
                    }

                    sb.AppendLine($"\t\t\treturn CreatedAtRoute(\"{pluralizeEntityForRouteForSomeTypeNames(entityName)}\", new {{ id = {camelCaseName}.id }}, {databaseNamespace}.{entityName}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName}));");
                }
                sb.AppendLine("\t\t}");
                sb.AppendLine();
                if (scriptGenTable != null && scriptGenTable.webAPIPostToBeOverridden == true)
                {
                    sb.AppendLine("*/");
                }
                sb.AppendLine();

                #endregion

                #region Rollback_Handling

                if (versionControlEnabled == true && ignoreFoundationServices == false)
                {
                    if (scriptGenTable != null && scriptGenTable.webAPIRollbackToBeOverridden == true)
                    {
                        // comment out this function, but write the code anyway.
                        sb.AppendLine("/* This function is expected to be overridden in a custom file");
                    }

                    sb.AppendLine($@"
        /// <summary>
        /// 
        /// This rolls a {entityName} entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>");

                    sb.AppendLine("\t\t[HttpPut]");
                    sb.AppendLine("\t\t[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]");
                    sb.AppendLine("\t\t[Route(\"api/" + singularForRouting + "/Rollback/{id}\")]");
                    sb.AppendLine("\t\t[Route(\"api/" + singularForRouting + "/Rollback\")]");
                    sb.AppendLine("\t\tpublic async Task<IActionResult> RollbackTo" + entityName + "Version(int id, int versionNumber, CancellationToken cancellationToken = default)");
                    sb.AppendLine("\t\t{");
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// Data rollback is an admin only function, like Deletes.");
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\tStartAuditEventClock();");
                    sb.AppendLine("\t\t\t");
                    sb.AppendLine("\t\t\tif (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t   return Forbid();");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine();


                    //
                    // No longer using entity data tokens for route auth
                    //
                    //sb.AppendLine("\t\t\t//if (await IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel.Write) == false)");
                    //sb.AppendLine("\t\t\t//{");
                    //sb.AppendLine("\t\t\t//   return Forbid();");
                    //sb.AppendLine("\t\t\t//}");
                    //sb.AppendLine();

                    sb.AppendLine("\t\t\t");
                    sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);");
                    sb.AppendLine("\t\t\t");
                    sb.AppendLine("\t\t\tbool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);");

                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine("\t\t\tbool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);");
                    }
                    sb.AppendLine();

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine(UserTenantGuidCommands(3));
                    }

                    sb.AppendLine("\t\t\t");

                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine("\t\t\tUser user = await GetUserAsync(userTenantGuid, securityUser, cancellationToken);");
                        sb.AppendLine("\t\t\t");
                        sb.AppendLine("\t\t\tif (user == null)");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t    // If we don't have a user record, then the data visibility sync is probably not working..");
                        sb.AppendLine("\t\t\t    await CreateAuditEventAsync(AuditEngine.AuditType.Error, \"Cannot proceed with " + entityName + " rollback because Unable to find user record for security user with account name of \" + securityUser.accountName + \".Check the data visibilty sync process.\", false);");
                        sb.AppendLine("\t\t\t    return Problem(\"Unable to proceed with " + entityName + " rollback because inconsistency with user record was found.\");");
                        sb.AppendLine("\t\t\t}");
                    }


                    sb.AppendLine();
                    sb.AppendLine("\t\t\t");
                    sb.AppendLine("\t\t\tIQueryable <" + qualifiedEntity + "> query = (from x in _context." + Pluralize(entityName));
                    sb.AppendLine("\t\t\t        where");
                    sb.AppendLine("\t\t\t        (x.id == id)");
                    sb.AppendLine("\t\t\t        select x);");

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine();
                        sb.AppendLine("\t\t\tquery = query.Where(x => x.tenantGuid == userTenantGuid);");
                    }

                    sb.AppendLine();
                    if (HasBinaryStorageFields(scriptGenTable) == true)
                    {
                        sb.AppendLine("\t\t\tbool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();");
                        sb.AppendLine();
                    }
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// Make sure nobody else is editing this " + entityName + " concurrently");
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\tlock (" + camelCaseName + "PutSyncRoot)");
                    sb.AppendLine("\t\t\t{");


                    sb.AppendLine("\t\t\t\t");
                    sb.AppendLine("\t\t\t\t" + qualifiedEntity + " " + camelCaseName + " = query.FirstOrDefault();");
                    sb.AppendLine("\t\t\t\t");
                    sb.AppendLine("\t\t\t\tif (" + camelCaseName + " == null)");
                    sb.AppendLine("\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, \"Invalid primary key provided for " + module + "." + entityName + " rollback\", id.ToString(), new Exception(\"No " + module + "." + entityName + " entity could be find with the primary key provided for the rollback operation.\"));");
                    sb.AppendLine("\t\t\t\t    return NotFound();");
                    sb.AppendLine("\t\t\t\t}");
                    sb.AppendLine("\t\t\t\t");
                    sb.AppendLine("\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t// Make a copy of the " + entityName + " current state so we can log it.");
                    sb.AppendLine("\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t" + qualifiedEntity + " cloneOfExisting = (" + qualifiedEntity + ")_context.Entry(" + camelCaseName + ").GetDatabaseValues().ToObject();");
                    sb.AppendLine("\t\t\t\t");
                    sb.AppendLine("\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t// Remove any object fields from the clone object so that it can serialize effectively");
                    sb.AppendLine("\t\t\t\t//");
                    foreach (PropertyInfo prop in type.GetProperties())
                    {
                        // Check for [NotMapped] attribute
                        if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                        {
                            continue; // Skip properties with [NotMapped]
                        }

                        Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                        // Is this an object type?
                        if (propertyType != typeof(string) &&
                            propertyType != typeof(int) &&
                            propertyType != typeof(long) &&
                            propertyType != typeof(DateTime) &&
                            propertyType != typeof(DateOnly) &&
                            propertyType != typeof(TimeOnly) &&
                            propertyType != typeof(float) &&
                            propertyType != typeof(double) &&
                            propertyType != typeof(decimal) &&
                            propertyType != typeof(bool) &&
                            propertyType != typeof(Guid))
                        {
                            sb.AppendLine("\t\t\t\tcloneOfExisting." + prop.Name + " = null;");
                        }
                    }
                    sb.AppendLine();

                    sb.AppendLine("\t\t\t\tif (versionNumber >= " + camelCaseName + ".versionNumber)");
                    sb.AppendLine("\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, \"Invalid version number provided for " + module + "." + entityName + " rollback.  Version number provided is \" + versionNumber, id.ToString(), new Exception(\"Invalid version number provided for " + module + "." + entityName + " rollback operation.Version number provided is \" + versionNumber));");
                    sb.AppendLine("\t\t\t\t    return NotFound();");
                    sb.AppendLine("\t\t\t\t}");
                    sb.AppendLine("\t\t\t\t");
                    sb.AppendLine("\t\t\t\t" + entityName + "ChangeHistory " + camelCaseName + "ChangeHistory = (from x in _context." + entityName + "ChangeHistories");
                    sb.AppendLine("\t\t\t\t                                               where");

                    // Note we are using the reserved word handling of the camel case function here to suit EFPT.  This might tneed to change later.
                    sb.AppendLine("\t\t\t\t                                               x." + CamelCase(entityName, true) + "Id == id &&");

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine("\t\t\t\t                                               x.versionNumber == versionNumber &&");
                        sb.AppendLine("\t\t\t\t                                               x.tenantGuid == userTenantGuid");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t\t                                               x.versionNumber == versionNumber");
                    }

                    sb.AppendLine("\t\t\t\t                                               select x)");
                    sb.AppendLine("\t\t\t\t                                               .AsNoTracking()");
                    sb.AppendLine("\t\t\t\t                                               .FirstOrDefault();");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\tif (" + camelCaseName + "ChangeHistory != null)");
                    sb.AppendLine("\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t    " + qualifiedEntity + " old" + entityName + " = JsonSerializer.Deserialize<" + qualifiedEntity + ">(" + camelCaseName + "ChangeHistory.data);");
                    sb.AppendLine("\t\t\t\t");
                    sb.AppendLine("\t\t\t\t    //");
                    sb.AppendLine("\t\t\t\t    // Increase the version number");
                    sb.AppendLine("\t\t\t\t    //");
                    sb.AppendLine("\t\t\t\t    " + camelCaseName + ".versionNumber++;");
                    sb.AppendLine("\t\t\t\t");
                    sb.AppendLine("\t\t\t\t    //");
                    sb.AppendLine("\t\t\t\t    // Put all other fields back the way that they were ");
                    sb.AppendLine("\t\t\t\t    //");


                    foreach (PropertyInfo prop in type.GetProperties())
                    {
                        // Check for [NotMapped] attribute
                        if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                        {
                            continue; // Skip properties with [NotMapped]
                        }

                        if (prop.Name != "versionNumber" &&     // this can't be assigned because we're changing it.
                            prop.Name != "id" &&                // no sense assigning this on top of itself.
                            prop.Name != "tenantGuid")         // no sense in assigning this again
                        {
                            //
                            // Intent here is to map all properties that are not object types
                            //
                            if (prop.PropertyType.FullName.StartsWith("System.") == true && prop.PropertyType.FullName.StartsWith("System.Collections") == false)
                            {
                                sb.AppendLine("\t\t\t\t    " + camelCaseName + "." + prop.Name + " = old" + entityName + "." + prop.Name + ";");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("not writing property assignment during default web API implementation creation from type " + prop.PropertyType.FullName + " because it is for a property that comes from the System namespace.");
                            }
                        }
                    }

                    if (HasBinaryStorageFields(scriptGenTable) == true)
                    {
                        sb.AppendLine("\t\t\t\t    //");
                        sb.AppendLine("\t\t\t\t    // If disk based binary mode is on, then we need to copy the old data file over as well.");
                        sb.AppendLine("\t\t\t\t    //");

                        sb.AppendLine("\t\t\t\t    if (diskBasedBinaryStorageMode == true)");
                        sb.AppendLine("\t\t\t\t    {");
                        sb.AppendLine("\t\t\t\t    	Byte[] binaryData = LoadDataFromDisk(old" + entityName + ".objectGuid, old" + entityName + ".versionNumber, \"" + dataFileNameExtension + "\");");
                        sb.AppendLine();
                        sb.AppendLine("\t\t\t\t    	//");
                        sb.AppendLine("\t\t\t\t    	// Write out the data as the new version");
                        sb.AppendLine("\t\t\t\t    	//");
                        sb.AppendLine("\t\t\t\t    	WriteDataToDisk(" + camelCaseName + ".objectGuid, " + camelCaseName + ".versionNumber, binaryData, \"" + dataFileNameExtension + "\");");
                        sb.AppendLine("\t\t\t\t    }");
                    }

                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t    string serialized" + entityName + " = JsonSerializer.Serialize(" + camelCaseName + ");");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())");
                    sb.AppendLine("\t\t\t\t    {");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t        _context.SaveChanges();");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t        //");
                    sb.AppendLine("\t\t\t\t        // Now add the change history");
                    sb.AppendLine("\t\t\t\t        //");
                    sb.AppendLine("\t\t\t\t        " + entityName + "ChangeHistory new" + entityName + "ChangeHistory = new " + entityName + "ChangeHistory();");

                    // Note we are using the reserved word version of camel case here to suit EFPT's naming convention.  this may need to be tweaked later if that changes..
                    sb.AppendLine("\t\t\t\t        new" + entityName + "ChangeHistory." + CamelCase(entityName, true) + "Id = " + camelCaseName + ".id;");

                    sb.AppendLine("\t\t\t\t        new" + entityName + "ChangeHistory.versionNumber = " + camelCaseName + ".versionNumber;");
                    sb.AppendLine("\t\t\t\t        new" + entityName + "ChangeHistory.timeStamp = DateTime.UtcNow;");

                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine("\t\t\t\t        new" + entityName + "ChangeHistory.userId = user.id;");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t\t        new" + entityName + "ChangeHistory.userId = securityUser.id;");
                    }

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine("\t\t\t\t        new" + entityName + "ChangeHistory.tenantGuid = userTenantGuid;");
                    }

                    sb.AppendLine($"\t\t\t\t        new{entityName}ChangeHistory.data = JsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName}));");
                    sb.AppendLine("\t\t\t\t        _context." + entityName + "ChangeHistories.Add(new" + entityName + "ChangeHistory);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t        _context.SaveChanges();");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t        transaction.Commit();");
                    sb.AppendLine("\t\t\t\t    }");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\tCreateAuditEvent(AuditEngine.AuditType.UpdateEntity,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\"" + module + "." + entityName + " rollback process successfully rolled back to version number \" + versionNumber,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "true,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "id.ToString(),");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tJsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tJsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName})),");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "null);");
                    sb.AppendLine();

                    sb.AppendLine();
                    sb.AppendLine($"\t\t\t\t    return Ok({databaseNamespace}.{entityName}.CreateAnonymous({camelCaseName}));");
                    sb.AppendLine("\t\t\t\t}");
                    sb.AppendLine("\t\t\t\telse");
                    sb.AppendLine("\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, \"Could not find version number provided for " + module + "." + entityName + " rollback.  Version number provided is \" + versionNumber, id.ToString(), new Exception(\"Could not find version number provided for " + module + "." + entityName + " rollback.  Version number provided is \" + versionNumber));");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t    return BadRequest();");
                    sb.AppendLine("\t\t\t\t}");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine("\t\t}");
                    sb.AppendLine();


                    if (scriptGenTable != null && scriptGenTable.webAPIRollbackToBeOverridden == true)
                    {
                        // comment out this function, but write the code anyway.
                        sb.AppendLine("*/");
                        sb.AppendLine();
                    }

                }

                sb.AppendLine();

                #endregion

                #region Version_History_Endpoints

                //
                // AI-Developed: Version History Accessor Endpoints
                //
                // These endpoints provide read-only access to the version history of entities with version control enabled.
                // They leverage the entity extension methods (GetVersionAsync, GetAllVersionsAsync, GetVersionAtTimeAsync)
                // that are generated for version-controlled entities.
                //

                if (versionControlEnabled == true && ignoreFoundationServices == false)
                {
                    //
                    // GetChangeMetadata Endpoint - Returns version metadata (timestamp, user) for a specific version
                    //
                    sb.AppendLine($@"
        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a {entityName}.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name=""id"">The primary key of the {entityName}</param>
        /// <param name=""versionNumber"">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>");
                    sb.AppendLine("\t\t[HttpGet]");
                    sb.AppendLine("\t\t[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]");
                    sb.AppendLine("\t\t[Route(\"api/" + singularForRouting + "/{id}/ChangeMetadata\")]");
                    sb.AppendLine("\t\tpublic async Task<IActionResult> Get" + entityName + "ChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)");
                    sb.AppendLine("\t\t{");
                    sb.AppendLine();

                    GenerateReadRoleAndPermissionChecks(module, scriptGenTable, sb);

                    sb.AppendLine();
                    sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);");

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine();
                        sb.AppendLine(UserTenantGuidCommands(3));
                    }

                    sb.AppendLine();
                    sb.AppendLine("\t\t\t" + qualifiedEntity + " " + camelCaseName + " = await _context." + plural + ".Where(x => x.id == id");

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine("\t\t\t\t&& x.tenantGuid == userTenantGuid");
                    }

                    sb.AppendLine("\t\t\t).FirstOrDefaultAsync(cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tif (" + camelCaseName + " == null)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\treturn NotFound();");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\ttry");
                    sb.AppendLine("\t\t\t{");

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine("\t\t\t\t" + camelCaseName + ".SetupVersionInquiry(_context, userTenantGuid);");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t\t" + camelCaseName + ".SetupVersionInquiry(_context, Guid.Empty);");
                    }

                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\tVersionInformation<" + qualifiedEntity + "> versionInfo = await " + camelCaseName + ".GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\tif (versionInfo == null)");
                    sb.AppendLine("\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\treturn NotFound($\"Version {versionNumber} not found.\");");
                    sb.AppendLine("\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\treturn Ok(versionInfo);");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine("\t\t\tcatch (Exception ex)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\treturn Problem(ex.Message);");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine("\t\t}");
                    sb.AppendLine();
                    sb.AppendLine();


                    //
                    // GetAuditHistory Endpoint - Returns the full audit history for an entity
                    //
                    sb.AppendLine($@"
        /// <summary>
        /// 
        /// Gets the full audit history for a {entityName}.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name=""id"">The primary key of the {entityName}</param>
        /// <param name=""includeData"">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>");
                    sb.AppendLine("\t\t[HttpGet]");
                    sb.AppendLine("\t\t[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]");
                    sb.AppendLine("\t\t[Route(\"api/" + singularForRouting + "/{id}/AuditHistory\")]");
                    sb.AppendLine("\t\tpublic async Task<IActionResult> Get" + entityName + "AuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)");
                    sb.AppendLine("\t\t{");
                    sb.AppendLine();

                    GenerateReadRoleAndPermissionChecks(module, scriptGenTable, sb);

                    sb.AppendLine();
                    sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);");

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine();
                        sb.AppendLine(UserTenantGuidCommands(3));
                    }

                    sb.AppendLine();
                    sb.AppendLine("\t\t\t" + qualifiedEntity + " " + camelCaseName + " = await _context." + plural + ".Where(x => x.id == id");

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine("\t\t\t\t&& x.tenantGuid == userTenantGuid");
                    }

                    sb.AppendLine("\t\t\t).FirstOrDefaultAsync(cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tif (" + camelCaseName + " == null)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\treturn NotFound();");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\ttry");
                    sb.AppendLine("\t\t\t{");

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine("\t\t\t\t" + camelCaseName + ".SetupVersionInquiry(_context, userTenantGuid);");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t\t" + camelCaseName + ".SetupVersionInquiry(_context, Guid.Empty);");
                    }

                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\tList<VersionInformation<" + qualifiedEntity + ">> versions = await " + camelCaseName + ".GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\treturn Ok(versions);");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine("\t\t\tcatch (Exception ex)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\treturn Problem(ex.Message);");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine("\t\t}");
                    sb.AppendLine();
                    sb.AppendLine();


                    //
                    // GetVersion Endpoint - Returns the entity at a specific version number
                    //
                    sb.AppendLine($@"
        /// <summary>
        /// 
        /// Gets a specific version of a {entityName}.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name=""id"">The primary key of the {entityName}</param>
        /// <param name=""version"">The version number to retrieve</param>
        /// <returns>The {entityName} object at that version</returns>");
                    sb.AppendLine("\t\t[HttpGet]");
                    sb.AppendLine("\t\t[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]");
                    sb.AppendLine("\t\t[Route(\"api/" + singularForRouting + "/{id}/Version/{version}\")]");
                    sb.AppendLine("\t\tpublic async Task<IActionResult> Get" + entityName + "Version(int id, int version, CancellationToken cancellationToken = default)");
                    sb.AppendLine("\t\t{");
                    sb.AppendLine();

                    GenerateReadRoleAndPermissionChecks(module, scriptGenTable, sb);

                    sb.AppendLine();
                    sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);");

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine();
                        sb.AppendLine(UserTenantGuidCommands(3));
                    }

                    sb.AppendLine();
                    sb.AppendLine("\t\t\t" + qualifiedEntity + " " + camelCaseName + " = await _context." + plural + ".Where(x => x.id == id");

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine("\t\t\t\t&& x.tenantGuid == userTenantGuid");
                    }

                    sb.AppendLine("\t\t\t).FirstOrDefaultAsync(cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tif (" + camelCaseName + " == null)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\treturn NotFound();");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\ttry");
                    sb.AppendLine("\t\t\t{");

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine("\t\t\t\t" + camelCaseName + ".SetupVersionInquiry(_context, userTenantGuid);");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t\t" + camelCaseName + ".SetupVersionInquiry(_context, Guid.Empty);");
                    }

                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\tVersionInformation<" + qualifiedEntity + "> versionInfo = await " + camelCaseName + ".GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\tif (versionInfo == null || versionInfo.data == null)");
                    sb.AppendLine("\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\treturn NotFound();");
                    sb.AppendLine("\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\treturn Ok(versionInfo.data.ToOutputDTO());");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine("\t\t\tcatch (Exception ex)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\treturn Problem(ex.Message);");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine("\t\t}");
                    sb.AppendLine();
                    sb.AppendLine();


                    //
                    // GetVersionAtTime Endpoint - Returns the entity state at a specific point in time
                    //
                    sb.AppendLine($@"
        /// <summary>
        /// 
        /// Gets the state of a {entityName} at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name=""id"">The primary key of the {entityName}</param>
        /// <param name=""time"">The point in time (ISO format, UTC)</param>
        /// <returns>The {entityName} object at that time</returns>");
                    sb.AppendLine("\t\t[HttpGet]");
                    sb.AppendLine("\t\t[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]");
                    sb.AppendLine("\t\t[Route(\"api/" + singularForRouting + "/{id}/StateAtTime\")]");
                    sb.AppendLine("\t\tpublic async Task<IActionResult> Get" + entityName + "StateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)");
                    sb.AppendLine("\t\t{");
                    sb.AppendLine();
                    GenerateReadRoleAndPermissionChecks(module, scriptGenTable, sb);

                    sb.AppendLine();
                    sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);");

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine();
                        sb.AppendLine(UserTenantGuidCommands(3));
                    }

                    sb.AppendLine();
                    sb.AppendLine("\t\t\t" + qualifiedEntity + " " + camelCaseName + " = await _context." + plural + ".Where(x => x.id == id");

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine("\t\t\t\t&& x.tenantGuid == userTenantGuid");
                    }

                    sb.AppendLine("\t\t\t).FirstOrDefaultAsync(cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tif (" + camelCaseName + " == null)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\treturn NotFound();");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\ttry");
                    sb.AppendLine("\t\t\t{");

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine("\t\t\t\t" + camelCaseName + ".SetupVersionInquiry(_context, userTenantGuid);");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t\t" + camelCaseName + ".SetupVersionInquiry(_context, Guid.Empty);");
                    }

                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\tVersionInformation<" + qualifiedEntity + "> versionInfo = await " + camelCaseName + ".GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\tif (versionInfo == null || versionInfo.data == null)");
                    sb.AppendLine("\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\treturn NotFound(\"No state found at specified time.\");");
                    sb.AppendLine("\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\treturn Ok(versionInfo.data.ToOutputDTO());");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine("\t\t\tcatch (Exception ex)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\treturn Problem(ex.Message);");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine("\t\t}");
                    sb.AppendLine();
                }

                #endregion

                #region HTTP_Delete_Handling

                if (scriptGenTable != null && scriptGenTable.webAPIDeleteToBeOverridden == true)
                {
                    // comment out this function, but write the code anyway.
                    sb.AppendLine("/* This function is expected to be overridden in a custom file");
                }

                sb.AppendLine($@"        /// <summary>
        /// 
        /// This deletes a {entityName} record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>");
                sb.AppendLine("\t\t[HttpDelete]");
                sb.AppendLine("\t\t[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]");
                sb.AppendLine("\t\t[Route(\"api/" + singularForRouting + "/{id}\")]");
                sb.AppendLine("\t\t[Route(\"api/" + singularForRouting + "\")]");
                sb.AppendLine("\t\tpublic async Task<IActionResult> Delete" + pluralizeEntityForRouteForSomeTypeNames(entityName) + "(int id, CancellationToken cancellationToken = default)");
                sb.AppendLine("\t\t{");

                if (ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\tStartAuditEventClock();");
                    sb.AppendLine();

                    //
                    // We consider deletes to be a write operation.
                    //
                    GenerateWriteRoleAndPermissionChecks(module, scriptGenTable, sb, adminAccessNeededToWrite);

                    sb.AppendLine();
                    sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);");

                    sb.AppendLine();
                    if (multiTenancyEnabled == true)
                    {
                        if (dataVisibilityEnabled == true)
                        {
                            sb.AppendLine("\t\t\tbool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(cancellationToken);");
                        }

                        sb.AppendLine("\t\t\t");
                        sb.AppendLine("\t\t\t");
                        sb.AppendLine(UserTenantGuidCommands(3));
                    }
                }

                sb.AppendLine("\t\t\tIQueryable<" + qualifiedEntity + "> query = (from x in _context." + plural);
                sb.AppendLine("\t\t\t\twhere");
                sb.AppendLine("\t\t\t\t(x.id == id)");
                sb.AppendLine("\t\t\t\tselect x);");
                sb.AppendLine();


                if (multiTenancyEnabled == true && ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\tquery = query.Where(x => x.tenantGuid == userTenantGuid);");
                }
                sb.AppendLine();
                sb.AppendLine("\t\t\t" + qualifiedEntity + " " + camelCaseName + " = await query.FirstOrDefaultAsync(cancellationToken);");


                sb.AppendLine();
                sb.AppendLine("\t\t\tif (" + camelCaseName + " == null)");
                sb.AppendLine("\t\t\t{");

                if (ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, \"Invalid primary key provided for " + module + "." + entityName + " DELETE\", id.ToString(), new Exception(\"No " + module + "." + entityName + " entity could be find with the primary key provided.\"));");
                }
                sb.AppendLine("\t\t\t\treturn NotFound();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\t" + qualifiedEntity + " cloneOfExisting = (" + qualifiedEntity + ")_context.Entry(" + camelCaseName + ").GetDatabaseValues().ToObject();");
                sb.AppendLine();

                if (versionControlEnabled == false)
                {
                    sb.AppendLine();
                    if (HasBinaryStorageFields(scriptGenTable) == true)
                    {
                        sb.AppendLine("\t\t\tbool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();");
                        sb.AppendLine();
                    }

                    sb.AppendLine("\t\t\ttry");
                    sb.AppendLine("\t\t\t{");

                    if (hasActiveAndDeletedControlFields == true)
                    {
                        sb.AppendLine("\t\t\t\t" + camelCaseName + ".deleted = true;");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t\t_context." + plural + ".Remove(" + camelCaseName + ");");
                    }

                    sb.AppendLine("\t\t\t\tawait _context.SaveChangesAsync(cancellationToken);");

                    if (ignoreFoundationServices == false)
                    {
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t\"" + module + "." + entityName + " entity successfully deleted.\",");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "true,");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "id.ToString(),");
                        sb.AppendLine($"{optionalTab}\t\t\t\t\tJsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),");
                        sb.AppendLine($"{optionalTab}\t\t\t\t\tJsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName})),");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "null);");
                        sb.AppendLine();

                    }
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine("\t\t\tcatch (Exception ex)");
                    sb.AppendLine("\t\t\t{");
                    if (ignoreFoundationServices == false)
                    {
                        sb.AppendLine(optionalTab + "\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t\"" + module + "." + entityName + " entity delete failed.\",");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "false,");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "id.ToString(),");
                        sb.AppendLine($"{optionalTab}\t\t\t\t\tJsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),");
                        sb.AppendLine($"{optionalTab}\t\t\t\t\tJsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName})),");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "ex);");
                        sb.AppendLine();
                    }
                    sb.AppendLine("\t\t\t\treturn Problem(ex.Message);");
                    sb.AppendLine("\t\t\t}");

                    sb.AppendLine("\t\t\treturn Ok();");
                }
                else
                {
                    if (ignoreFoundationServices == true)
                    {
                        throw new Exception("Version control can't be used if ignoring foundation services.");
                    }

                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine("\t\t\tUser user = await GetUserAsync(userTenantGuid, securityUser, cancellationToken);");
                        sb.AppendLine();
                        sb.AppendLine("\t\t\tif (user == null)");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t    // If we don't have a user record, then the data visibility sync is probably not working..");
                        sb.AppendLine("\t\t\t    CreateAuditEvent(AuditEngine.AuditType.Error, \"Cannot proceed with " + entityName + " DELETE because Unable to find user record for security user with account name of \" + securityUser.accountName + \".Check the data visibilty sync process.\", false);");
                        sb.AppendLine("\t\t\t    return Problem(\"Unable to proceed with " + entityName + " delete because inconsistency with user record was found.\");");
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine();
                    }

                    sb.AppendLine();
                    if (HasBinaryStorageFields(scriptGenTable) == true)
                    {
                        sb.AppendLine("\t\t\tbool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();");
                        sb.AppendLine();
                    }

                    sb.AppendLine("\t\t\tlock (" + camelCaseName + "DeleteSyncRoot)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t    try");
                    sb.AppendLine("\t\t\t    {");
                    sb.AppendLine("\t\t\t        " + camelCaseName + ".deleted = true;");
                    sb.AppendLine("\t\t\t        " + camelCaseName + ".versionNumber++;");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t        _context.SaveChanges();");
                    sb.AppendLine();

                    if (HasBinaryStorageFields(scriptGenTable) == true)
                    {
                        sb.AppendLine("\t\t\t        //");
                        sb.AppendLine("\t\t\t        // If in disk based storage mode, create a copy of the disk data file for the new version.");
                        sb.AppendLine("\t\t\t        //");
                        sb.AppendLine("\t\t\t        if (diskBasedBinaryStorageMode == true)");
                        sb.AppendLine("\t\t\t        {");
                        sb.AppendLine("\t\t\t        	Byte[] binaryData = LoadDataFromDisk(" + camelCaseName + ".objectGuid, " + camelCaseName + ".versionNumber -1, \"" + dataFileNameExtension + "\");");
                        sb.AppendLine();
                        sb.AppendLine("\t\t\t        	//");
                        sb.AppendLine("\t\t\t        	// Write out the same data");
                        sb.AppendLine("\t\t\t        	//");
                        sb.AppendLine("\t\t\t        	WriteDataToDisk(" + camelCaseName + ".objectGuid, " + camelCaseName + ".versionNumber, binaryData, \"" + dataFileNameExtension + "\");");
                        sb.AppendLine("\t\t\t        }");
                        sb.AppendLine();
                    }

                    sb.AppendLine("\t\t\t        //");
                    sb.AppendLine("\t\t\t        // Now add the change history");
                    sb.AppendLine("\t\t\t        //");
                    sb.AppendLine("\t\t\t        " + entityName + "ChangeHistory " + camelCaseName + "ChangeHistory = new " + entityName + "ChangeHistory();");

                    // Note we are using the reserved word version of camel case here to suit EFPT's naming convention.  this may need to be tweaked later if that changes..
                    sb.AppendLine("\t\t\t        " + camelCaseName + "ChangeHistory." + CamelCase(entityName, true) + "Id = " + camelCaseName + ".id;");


                    sb.AppendLine("\t\t\t        " + camelCaseName + "ChangeHistory.versionNumber = " + camelCaseName + ".versionNumber;");
                    sb.AppendLine("\t\t\t        " + camelCaseName + "ChangeHistory.timeStamp = DateTime.UtcNow;");

                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine("\t\t\t        " + camelCaseName + "ChangeHistory.userId = user.id;");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t        " + camelCaseName + "ChangeHistory.userId = securityUser.id;");
                    }

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine("\t\t\t        " + camelCaseName + "ChangeHistory.tenantGuid = userTenantGuid;");
                    }

                    sb.AppendLine($"\t\t\t        {camelCaseName}ChangeHistory.data = JsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName}));");
                    sb.AppendLine("\t\t\t        _context." + entityName + "ChangeHistories.Add(" + camelCaseName + "ChangeHistory);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t        _context.SaveChanges();");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\tCreateAuditEvent(AuditEngine.AuditType.DeleteEntity,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\"" + module + "." + entityName + " entity successfully deleted.\",");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "true,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "id.ToString(),");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tJsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tJsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName})),");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "null);");
                    sb.AppendLine();

                    sb.AppendLine("\t\t\t    }");
                    sb.AppendLine("\t\t\t    catch (Exception ex)");
                    sb.AppendLine("\t\t\t    {");

                    sb.AppendLine(optionalTab + "\t\t\t\tCreateAuditEvent(AuditEngine.AuditType.DeleteEntity,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\"" + module + "." + entityName + " entity delete failed\",");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "false,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "id.ToString(),");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tJsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tJsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName})),");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "ex);");
                    sb.AppendLine();

                    sb.AppendLine("\t\t\t        return Problem(ex.Message);");
                    sb.AppendLine("\t\t\t    }");
                    sb.AppendLine("\t\t\t    return Ok();");
                    sb.AppendLine("\t\t\t}");
                }

                sb.AppendLine("\t\t}");
                sb.AppendLine();
                sb.AppendLine();
            }
            if (scriptGenTable != null && scriptGenTable.webAPIDeleteToBeOverridden == true)
            {
                sb.AppendLine("*/");
            }

            #endregion

            #region GetListData_Handling

            if (scriptGenTable != null && scriptGenTable.webAPIGetListDataToBeOverridden == true)
            {
                // comment out this function, but write the code anyway.
                sb.AppendLine("/* This function is expected to be overridden in a custom file");
            }


            sb.AppendLine($@"        /// <summary>
        /// 
        /// This gets a list of {entityName} records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>");
            sb.AppendLine("\t\t[Route(\"api/" + pluralForRouting + "/ListData\")]");

            // special handler for the user table that has a self referencing field in the application databases.  Security module has similar issue, but that is solved with a custom override in that project.
            if (entityName == "User" && scriptGenTable.HasField("reportsToUserId") == true)
            {
                sb.AppendLine("\t\t[Route(\"api/ReportsToUsers/ListData\")]");
                sb.AppendLine("\t\t[Route(\"api/CreatedByUsers/ListData\")]  // needed for notifications framework");          // needed for notifications framework
            }

            sb.AppendLine("\t\t[HttpGet]");
            sb.AppendLine("\t\t[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]");
            sb.AppendLine("\t\tpublic async Task<IActionResult> GetListData(");


            processingFirstProperty = true;
            foreach (PropertyInfo prop in type.GetProperties())
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                // jump over the id field on the list getter function.  It adds no value as a parameter here and will likely class with the individual getter function, which only has ID as a param below.
                if (prop.Name == "id")
                {
                    continue;
                }

                if (prop.Name == "tenantGuid")      // don't create a tenant guid filter
                {
                    continue;
                }
                if (prop.Name == "password")      // don't create a password filter
                {
                    continue;
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType == typeof(string))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }
                    sb.Append("\t\t\tstring " + prop.Name + " = null");
                }
                else if (propertyType == typeof(Guid))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }
                    sb.Append("\t\t\tGuid? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(int))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tint? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(long))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tlong? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(bool))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tbool? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(DateTime))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tDateTime? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(float))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tfloat? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(double))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tdouble? " + prop.Name + " = null");
                }
                else if (propertyType == typeof(decimal))
                {
                    if (processingFirstProperty == false)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t\t\tdecimal? " + prop.Name + " = null");
                }

                processingFirstProperty = false;
            }
            sb.AppendLine($@",");

            if (scriptGenTable.AddAnyStringContainsParameterToWebAPI == true)
            {
                sb.AppendLine("\t\t\tstring anyStringContains = null,");
            }


            if (scriptGenTable != null && scriptGenTable.defaultPageSizeForListGetters > 0)
            {
                sb.AppendLine($"\t\t\tint? pageSize = {scriptGenTable.defaultPageSizeForListGetters},");     // Use the table's own default 
                sb.AppendLine("\t\t\tint? pageNumber = 1,");
                sb.AppendLine("\t\t\tCancellationToken cancellationToken = default)");
            }
            else
            {
                sb.AppendLine($"\t\t\tint? pageSize = null,");
                sb.AppendLine("\t\t\tint? pageNumber = null,");
                sb.AppendLine("\t\t\tCancellationToken cancellationToken = default)");
            }


            sb.AppendLine("\t\t{");

            if (ignoreFoundationServices == false)
            {
                GenerateReadRoleAndPermissionChecks(module, scriptGenTable, sb);

                sb.AppendLine();
                sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);");
                sb.AppendLine();
                sb.AppendLine("\t\t\tbool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\tbool userIsWriter = await UserCanWriteAsync(securityUser, " + entityMinimumWritePermissionString + ", cancellationToken);");
                sb.AppendLine();

                if (dataVisibilityEnabled == true)
                {
                    sb.AppendLine("\t\t\tbool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);");
                }

                if (multiTenancyEnabled == true)
                {
                    sb.AppendLine();
                    sb.AppendLine(UserTenantGuidCommands(3));
                }
            }

            sb.AppendLine();


            sb.AppendLine("\t\t\tif (pageNumber.HasValue == true &&");
            sb.AppendLine("\t\t\t    pageNumber < 1)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t    pageNumber = null;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();
            sb.AppendLine("\t\t\tif (pageSize.HasValue == true &&");
            sb.AppendLine("\t\t\t    pageSize <= 0)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t    pageSize = null;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();


            commentWritten = false;
            foreach (PropertyInfo prop in type.GetProperties())
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType == typeof(DateTime))
                {
                    if (commentWritten == false)
                    {
                        sb.AppendLine("\t\t\t//");
                        sb.AppendLine("\t\t\t// Turn any local time kinded parameters to UTC.");
                        sb.AppendLine("\t\t\t//");
                        commentWritten = true;
                    }

                    sb.AppendLine("\t\t\tif (" + prop.Name + ".HasValue == true && " + prop.Name + ".Value.Kind != DateTimeKind.Utc)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\t" + prop.Name + " = " + prop.Name + ".Value.ToUniversalTime();");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();
                }
            }

            /*
             * This pattern uses the method syntax to apply where conditions only when necessary to dramatically increase query performance by not needing the db to evaluate the null state of the parameter.
             * */
            sb.AppendLine($"\t\t\tIQueryable<{qualifiedEntity}> query = (from {acronym} in _context.{plural} select {acronym});");

            if (ignoreFoundationServices == false)
            {
                if (dataVisibilityEnabled == true)
                {
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// Apply the constraints for the user's tenant and data visibility configuration.");
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\tquery = await AddTenantAndDataVisibilityConstraintsAsync(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin, cancellationToken);");
                    sb.AppendLine();
                }
                else if (multiTenancyEnabled == true)
                {
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tquery = query.Where(x => x.tenantGuid == userTenantGuid);");
                    sb.AppendLine();
                }
            }

            processingFirstProperty = true;
            foreach (PropertyInfo prop in type.GetProperties())
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                if (prop.Name == "id")
                {
                    continue;
                }

                //
                // don't create a password filter
                //
                if (prop.Name == "password")
                {
                    continue;
                }

                //
                // don't create a tenant guid filter
                //
                if (prop.Name == "tenantGuid")
                {
                    continue;
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType == typeof(string))
                {
                    sb.AppendLine("\t\t\tif (string.IsNullOrEmpty(" + prop.Name + ") == false)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + "." + prop.Name + " == " + prop.Name + ");");
                    sb.AppendLine("\t\t\t}");
                }
                if (propertyType == typeof(Guid))
                {
                    sb.AppendLine("\t\t\tif (" + prop.Name + ".HasValue == true)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + "." + prop.Name + " == " + prop.Name + ");");
                    sb.AppendLine("\t\t\t}");
                }
                else if (propertyType == typeof(int) || propertyType == typeof(long) || propertyType == typeof(DateTime) || propertyType == typeof(float) || propertyType == typeof(float) || propertyType == typeof(decimal))
                {
                    sb.AppendLine("\t\t\tif (" + prop.Name + ".HasValue == true)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + "." + prop.Name + " == " + prop.Name + ".Value);");
                    sb.AppendLine("\t\t\t}");
                }
                else if (propertyType == typeof(bool))
                {
                    if (prop.Name == "active" && ignoreFoundationServices == false)
                    {
                        // Only writers and admins can see inactive records, or filter by active.  Only admins can see deleted records, or filter by deleted.
                        sb.AppendLine("\t\t\tif (userIsWriter == true)");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t\tif (active.HasValue == true)");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".active == active.Value);");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine("\t\t\t");
                        if (scriptGenTable.HasField("deleted") == true)
                        {

                            sb.AppendLine("\t\t\t\tif (userIsAdmin == true)");
                            sb.AppendLine("\t\t\t\t{");
                            sb.AppendLine("\t\t\t\t\tif (deleted.HasValue == true)");
                            sb.AppendLine("\t\t\t\t\t{");
                            sb.AppendLine("\t\t\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".deleted == deleted.Value);");
                            sb.AppendLine("\t\t\t\t\t}");
                            sb.AppendLine("\t\t\t\t}");
                            sb.AppendLine("\t\t\t\telse");
                            sb.AppendLine("\t\t\t\t{");
                            sb.AppendLine("\t\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".deleted == false);");
                            sb.AppendLine("\t\t\t\t}");
                        }
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine("\t\t\telse");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".active == true);");
                        if (scriptGenTable.HasField("deleted") == true)
                        {
                            sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".deleted == false);");
                        }
                        sb.AppendLine("\t\t\t}");

                    }
                    else if (prop.Name == "deleted" && ignoreFoundationServices == false)
                    {
                        //
                        // Deleted handled in active handler.
                        //
                    }
                    else
                    {
                        sb.AppendLine("\t\t\tif (" + prop.Name + ".HasValue == true)");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + "." + prop.Name + " == " + prop.Name + ".Value);");
                        sb.AppendLine("\t\t\t}");
                    }
                }
            }

            sb.AppendLine();


            // Add in the query parameters for any string contains
            if (scriptGenTable.AddAnyStringContainsParameterToWebAPI == true)
            {
                AddAnyStringsContainsQueryAdditions(type, sb, rootNameSpace, false);
            }


            if (ignoreFoundationServices == false)
            {
                if (dataVisibilityEnabled == true)
                {
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// Apply the constraints for the user's tenant and data visibility configuration.");
                    sb.AppendLine("\t\t\t//");

                    sb.AppendLine("\t\t\tquery = await AddTenantAndDataVisibilityConstraintsAsync(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin, cancellationToken);");
                }
                else if (multiTenancyEnabled == true)
                {
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tquery = query.Where(x => x.tenantGuid == userTenantGuid);");
                    sb.AppendLine();
                }
                sb.AppendLine();
            }

            //
            // Use the sort sequence defined on the script generation table object, or create them if missing
            //
            bool firstWritten = false;
            foreach (var ss in scriptGenTable.GetOrGenerateSortSequences())
            {
                if (firstWritten == false)
                {
                    if (ss == null || ss.field == null)
                    {
                        continue;
                    }

                    sb.Append("\t\t\tquery = query.OrderBy" + (ss.descending == true ? "Descending " : "") + "(x => x." + ss.field.name + ")");
                    firstWritten = true;
                }
                else
                {
                    sb.Append(".ThenBy" + (ss.descending == true ? "Descending " : "") + "(x => x." + ss.field.name + ")");
                }
            }

            if (firstWritten == true)
            {
                sb.AppendLine(";");
            }


            sb.AppendLine("\t\t\tif (pageNumber.HasValue == true &&");
            sb.AppendLine("\t\t\t    pageSize.HasValue == true)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);");
            sb.AppendLine("\t\t\t}");


            // Tables like 'User' clash with the C# UserPrincial object if not prefixed with the namespace, so just add for all.
            sb.AppendLine($"\t\t\treturn Ok(await (from queryData in query select {databaseNamespace}.{entityName}.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));");

            sb.AppendLine("\t\t}");

            if (scriptGenTable != null && scriptGenTable.webAPIGetListDataToBeOverridden == true)
            {
                sb.AppendLine("*/");
            }

            sb.AppendLine();

            #endregion

            #region Audit_Event_Creation_Post_Handling


            if (ignoreFoundationServices == false)
            {
                sb.AppendLine();
                sb.AppendLine($@"        /// <summary>
        /// 
        /// This method creates an audit event from within the controller.  It is intended for use by custom logic in client applications that needs to create audit events.
        /// 
        /// </summary>
        /// <param name=""type""></param>
        /// <param name=""message""></param>
        /// <param name=""primaryKey""></param>
        /// <returns></returns>");
                sb.AppendLine("\t\t[HttpPost]");
                sb.AppendLine("\t\t[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]");
                sb.AppendLine("\t\t[Route(\"api/" + singularForRouting + "/CreateAuditEvent\")]");
                sb.AppendLine("\t\tpublic async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)");
                sb.AppendLine("\t\t{");
                sb.AppendLine();
                GenerateWriteRoleAndPermissionChecks(module, scriptGenTable, sb, adminAccessNeededToWrite);
                sb.AppendLine();
                sb.AppendLine("\t\t    await CreateAuditEventAsync(type, message, primaryKey);");
                sb.AppendLine();
                sb.AppendLine("\t\t    return Ok();");
                sb.AppendLine("\t\t}");

                sb.AppendLine();
            }

            #endregion

            #region Favourite_Handling

            if (canBeFavourited == true && ignoreFoundationServices == false)
            {
                sb.AppendLine();


                sb.AppendLine($@"        /// <summary>
        /// 
        /// This makes a {entityName} record a favourite for the current user.
		/// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>");
                sb.AppendLine("\t\t[Route(\"api/" + singularForRouting + "/Favourite/{id}\")]");
                sb.AppendLine("\t\t[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]");
                sb.AppendLine("\t\t[HttpPut]");
                sb.AppendLine("\t\tpublic async Task<IActionResult> SetFavourite(int id, string description = null, CancellationToken cancellationToken = default)");
                sb.AppendLine("\t\t{");

                GenerateReadRoleAndPermissionChecks(module, scriptGenTable, sb);

                sb.AppendLine();


                sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);");
                sb.AppendLine();
                sb.AppendLine("\t\t\tbool userIsAdmin = await UserCanAdministerAsync(cancellationToken);");

                if (dataVisibilityEnabled == true)
                {
                    sb.AppendLine("\t\t\tbool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(cancellationToken);");
                }

                sb.AppendLine();

                sb.AppendLine(UserTenantGuidCommands(3));

                sb.AppendLine();
                sb.AppendLine("\t\t\tIQueryable<" + qualifiedEntity + "> query = (from x in _context." + plural + "");
                sb.AppendLine("\t\t\t                               where x.id == id");
                sb.AppendLine("\t\t\t                               select x);");
                sb.AppendLine();

                if (dataVisibilityEnabled == true)
                {
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// Apply the constraints for the user's tenant and data visibility configuration.");
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\tquery = await AddTenantAndDataVisibilityConstraintsAsync(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin, cancellationToken);");
                }
                sb.AppendLine();

                sb.AppendLine("\t\t\t" + qualifiedEntity + " " + camelCaseName + " = await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);");
                sb.AppendLine();
                sb.AppendLine("\t\t\tif (" + camelCaseName + " != null)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\tif (string.IsNullOrEmpty(description) == true)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\tdescription = " + displayNameFieldSerializationCode + ";");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t// Add the user favourite " + entityName);
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\tawait SecurityLogic.AddToUserFavouritesAsync(securityUser, \"" + entityName + "\", id, description, cancellationToken);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, \"Favourite '" + entityName + "' was added for record with id of \" + id + \" for user \" + securityUser.accountName, true);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t// Return the complete list of user favourites after the addition");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\treturn Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\telse");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, \"Favourite '" + entityName + "' add request was made with an invalid id value of \" + id, false);");
                sb.AppendLine("\t\t\t\treturn BadRequest();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t}");
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine($@"        /// <summary>
        /// 
        /// This removes a {entityName} record from the current user's favourites.
        /// 
		/// The rate limit is 2 per second per user.
		/// 
        /// </summary>");
                sb.AppendLine("\t\t[Route(\"api/" + singularForRouting + "/Favourite/{id}\")]");
                sb.AppendLine("\t\t[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]");
                sb.AppendLine("\t\t[HttpDelete]");
                sb.AppendLine("\t\tpublic async Task<IActionResult> DeleteFavourite(int id, CancellationToken cancellationToken = default)");
                sb.AppendLine("\t\t{");
                sb.AppendLine();

                GenerateReadRoleAndPermissionChecks(module, scriptGenTable, sb);

                sb.AppendLine();

                sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);");
                sb.AppendLine();

                sb.AppendLine(UserTenantGuidCommands(3));

                sb.AppendLine();
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\t// Delete the user favourite " + entityName);
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\tawait SecurityLogic.RemoveFromUserFavouritesAsync(securityUser, \"" + entityName + "\", id, cancellationToken);");
                sb.AppendLine();
                sb.AppendLine("\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, \"Favourite '" + entityName + "' was removed for record with id of \" + id + \" for user \" + securityUser.accountName, true);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\t// Return the complete list of user favourites after the deletion");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\treturn Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser, null, cancellationToken));");
                sb.AppendLine("\t\t}");
                sb.AppendLine();
            }

            sb.AppendLine();

            #endregion

            #region Tenant_and_Visibilty_contraints

            if (dataVisibilityEnabled == true && ignoreFoundationServices == false)
            {
                sb.AppendLine("\t\tprivate async Task<IQueryable<" + qualifiedEntity + ">> AddTenantAndDataVisibilityConstraintsAsync(IQueryable<" + qualifiedEntity + "> query, SecurityUser securityUser, Guid userTenantGuid, bool userIsSecurityAdmin, bool userIsAdmin, CancellationToken cancellationToken = default)");
                sb.AppendLine("\t\t{");

                sb.AppendLine("\t\t\tquery = query.Where(a => a.tenantGuid == userTenantGuid);");

                sb.AppendLine();
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\t// Admin users see all data.");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\t// Non-Admin users get filters based on their unique sets of data visibility permissions.");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\tif (userIsAdmin == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\tList<int> organizationsUserIsEntitledToReadFrom = await GetOrganizationIdsUserIsEntitledToReadFromAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\t\tList<int> departmentsUserIsEntitledToReadFrom = await GetDepartmentIdsUserIsEntitledToReadFromAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\t\tList<int> teamsUserIsEntitledToReadFrom = await GetTeamIdsUserIsEntitledToReadFromAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\t\tList<int> userAndTheirReportIds = await GetListOfUserIdsForUserAndPeopleThatReportToThemAsync(securityUser, cancellationToken);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\tList<int> organizationsThatUserInheritsReadFrom = await GetOrganizationIdsUserIsEntitledToReadImplicitlyFromForNullDepartmentAndTeamValuesAsync(securityUser, cancellationToken);");
                sb.AppendLine("\t\t\t\tList<int> departmentsThatUserInheritsReadFrom = await GetDepartmentsIdsUserIsEntitledToReadImplicitlyFromForNullTeamValuesAsync(securityUser, cancellationToken);");
                sb.AppendLine();


                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t// Build the data visibility condition based on the state of the user's organization, department, and team entitlement, as well as their own and that of people that report to them.");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t// If " + entityName + " has no data visibility attributes then return it.  Otherwise, user must match on one or more of organization, department, team, or user to see this " + entityName + "");
                sb.AppendLine("\t\t\t\t//");

                // This is the new way that supports implicit dep and org readership and is re-sequenced for hopefully better performance
                sb.AppendLine("\t\t\t\tquery = query.Where(a => (organizationsUserIsEntitledToReadFrom.Contains(a.organizationId.Value) ||                                                                                     // Records that have an organization that the user can explicity read from");
                sb.AppendLine("\t\t\t\tdepartmentsUserIsEntitledToReadFrom.Contains(a.departmentId.Value)) ||                                                                                                                  // Records that have a department that the user can explicitly read from");
                sb.AppendLine("\t\t\t\tteamsUserIsEntitledToReadFrom.Contains(a.teamId.Value) ||                                                                                                                               // Records that have a team that the user can explicity read from");
                sb.AppendLine("\t\t\t\tuserAndTheirReportIds.Contains(a.userId.Value) ||                                                                                                                                       // Records owned by the user or part of their team");
                sb.AppendLine("\t\t\t\t(a.organizationId.HasValue == false && a.departmentId.HasValue == false && a.teamId.HasValue == false) ||                                                                               // Records that have no DV setup at all - ie. all 3 fields null.");
                sb.AppendLine("\t\t\t\t(a.teamId.HasValue == false && a.departmentId.HasValue == false && a.organizationId.HasValue == true && organizationsThatUserInheritsReadFrom.Contains(a.organizationId.Value)) ||      // Records that have only an organization set, and the user can implicitly read from that org via one of their department or team readerships");
                sb.AppendLine("\t\t\t\t(a.teamId.HasValue == false && a.departmentId.HasValue == true && departmentsThatUserInheritsReadFrom.Contains(a.departmentId.Value))                                                   // Records that have only an organization set, and the user can implicitly read from that org via one of their department or team readerships");
                sb.AppendLine("\t\t\t\t);");


                sb.AppendLine("\t\t\t}");
                sb.AppendLine();
                sb.AppendLine("\t\t\treturn query;");
                sb.AppendLine("\t\t}");
                sb.AppendLine();
                sb.AppendLine();

                sb.AppendLine("\t\tprivate IQueryable<" + qualifiedEntity + "> AddTenantAndDataVisibilityConstraints(IQueryable<" + qualifiedEntity + "> query, SecurityUser securityUser, Guid userTenantGuid, bool userIsSecurityAdmin, bool userIsAdmin)");
                sb.AppendLine("\t\t{");

                sb.AppendLine("\t\t\tquery = query.Where(a => a.tenantGuid == userTenantGuid);");

                sb.AppendLine();
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\t// Admin users see all data.");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\t// Non-Admin users get filters based on their unique sets of data visibility permissions.");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\tif (userIsAdmin == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\tList<int> organizationsUserIsEntitledToReadFrom = GetOrganizationIdsUserIsEntitledToReadFrom(securityUser);");
                sb.AppendLine("\t\t\t\tList<int> departmentsUserIsEntitledToReadFrom = GetDepartmentIdsUserIsEntitledToReadFrom(securityUser);");
                sb.AppendLine("\t\t\t\tList<int> teamsUserIsEntitledToReadFrom = GetTeamIdsUserIsEntitledToReadFrom(securityUser);");
                sb.AppendLine("\t\t\t\tList<int> userAndTheirReportIds = GetListOfUserIdsForUserAndPeopleThatReportToThem(securityUser);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\tList<int> organizationsThatUserInheritsReadFrom = GetOrganizationIdsUserIsEntitledToReadImplicitlyFromForNullDepartmentAndTeamValues(securityUser);");
                sb.AppendLine("\t\t\t\tList<int> departmentsThatUserInheritsReadFrom = GetDepartmentsIdsUserIsEntitledToReadImplicitlyFromForNullTeamValues(securityUser);");
                sb.AppendLine();


                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t// Build the data visibility condition based on the state of the user's organization, department, and team entitlement, as well as their own and that of people that report to them.");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t// If " + entityName + " has no data visibility attributes then return it.  Otherwise, user must match on one or more of organization, department, team, or user to see this " + entityName + "");
                sb.AppendLine("\t\t\t\t//");

                // This is the new way that supports implicit dep and org readership and is re-sequenced for hopefully better performance
                sb.AppendLine("\t\t\t\tquery = query.Where(a => (organizationsUserIsEntitledToReadFrom.Contains(a.organizationId.Value) ||                                                                                     // Records that have an organization that the user can explicity read from");
                sb.AppendLine("\t\t\t\tdepartmentsUserIsEntitledToReadFrom.Contains(a.departmentId.Value)) ||                                                                                                                  // Records that have a department that the user can explicitly read from");
                sb.AppendLine("\t\t\t\tteamsUserIsEntitledToReadFrom.Contains(a.teamId.Value) ||                                                                                                                               // Records that have a team that the user can explicity read from");
                sb.AppendLine("\t\t\t\tuserAndTheirReportIds.Contains(a.userId.Value) ||                                                                                                                                       // Records owned by the user or part of their team");
                sb.AppendLine("\t\t\t\t(a.organizationId.HasValue == false && a.departmentId.HasValue == false && a.teamId.HasValue == false) ||                                                                               // Records that have no DV setup at all - ie. all 3 fields null.");
                sb.AppendLine("\t\t\t\t(a.teamId.HasValue == false && a.departmentId.HasValue == false && a.organizationId.HasValue == true && organizationsThatUserInheritsReadFrom.Contains(a.organizationId.Value)) ||      // Records that have only an organization set, and the user can implicitly read from that org via one of their department or team readerships");
                sb.AppendLine("\t\t\t\t(a.teamId.HasValue == false && a.departmentId.HasValue == true && departmentsThatUserInheritsReadFrom.Contains(a.departmentId.Value))                                                   // Records that have only an organization set, and the user can implicitly read from that org via one of their department or team readerships");
                sb.AppendLine("\t\t\t\t);");


                sb.AppendLine("\t\t\t}");
                sb.AppendLine();
                sb.AppendLine("\t\t\treturn query;");
                sb.AppendLine("\t\t}");
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("\t\tprivate static void ConfigureIsWriteable(" + entityName + "WithWritePermissionDetails " + camelCaseName + "WWP, SecurityUser securityUser, Guid userTenantGuid, bool userIsAdmin, bool userIsWriter, List<int> userAndTheirReportIds, List<int> organizationsUserIsEntitledToWriteTo, List<int> departmentsUserIsEntitledToWriteTo, List<int> teamsUserIsEntitledToWriteTo)");
                sb.AppendLine("\t\t{");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\t// Determine if the " + entityName + " can be written to or not.  If it can't, then indicate the reason.");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\tif (userIsAdmin == true)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\t" + camelCaseName + "WWP.isWriteable = true;");
                sb.AppendLine("\t\t\t\t" + camelCaseName + "WWP.notWriteableReason = null;");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\telse if (userIsWriter == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\t" + camelCaseName + "WWP.isWriteable = false;");
                sb.AppendLine("\t\t\t\t" + camelCaseName + "WWP.notWriteableReason = \"Cannot write to module.\";");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\telse if (userIsAdmin == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t// Non-Admin users may not be able to write to some records.");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t// Start checking the data visibility fields for the ability to write starting at the organization level, and moving down");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t//		- Ownership of the record either directly by the user, or by one of their reports.");
                sb.AppendLine("\t\t\t\t//		- Write permission at Organization implies ability to write all all of the organization's department and team levels");
                sb.AppendLine("\t\t\t\t//		- Write permission at Department implies ability to write to all of the department's teams");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t// First, check if the " + entityName + " is writeable in the first 4 conditions, and if nothing matches there, then work the other way to find the reason why it's not writeable.");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\tif (" + camelCaseName + "WWP.userId.HasValue == true && userAndTheirReportIds != null && userAndTheirReportIds.Contains(" + camelCaseName + "WWP.userId.Value) == true)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t// " + entityName + " is owned by user or one of their reports, so this record can be written to.");
                sb.AppendLine("\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t" + camelCaseName + "WWP.isWriteable = true;");
                sb.AppendLine("\t\t\t\t\t" + camelCaseName + "WWP.notWriteableReason = null;");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine("\t\t\t\telse if (" + camelCaseName + "WWP.organizationId.HasValue == true && organizationsUserIsEntitledToWriteTo != null && organizationsUserIsEntitledToWriteTo.Contains(" + camelCaseName + "WWP.organizationId.Value) == true)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t// " + entityName + "'s Organization is writeable, so this record can be written to.");
                sb.AppendLine("\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t" + camelCaseName + "WWP.isWriteable = true;");
                sb.AppendLine("\t\t\t\t\t" + camelCaseName + "WWP.notWriteableReason = null;");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine("\t\t\t\telse if (" + camelCaseName + "WWP.departmentId.HasValue == true && departmentsUserIsEntitledToWriteTo != null && departmentsUserIsEntitledToWriteTo.Contains(" + camelCaseName + "WWP.departmentId.Value) == true)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t// " + entityName + "'s Department is writeable, so this record can be written to.");
                sb.AppendLine("\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t" + camelCaseName + "WWP.isWriteable = true;");
                sb.AppendLine("\t\t\t\t\t" + camelCaseName + "WWP.notWriteableReason = null;");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine("\t\t\t\telse if (" + camelCaseName + "WWP.teamId.HasValue == true && teamsUserIsEntitledToWriteTo != null && teamsUserIsEntitledToWriteTo.Contains(" + camelCaseName + "WWP.teamId.Value) == true)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t// " + entityName + "'s Team is writeable, so this record can be written to.");
                sb.AppendLine("\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t" + camelCaseName + "WWP.isWriteable = true;");
                sb.AppendLine("\t\t\t\t\t" + camelCaseName + "WWP.notWriteableReason = null;");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t// Now check if the user can't write, and if so, cite the reason");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\telse if (" + camelCaseName + "WWP.teamId.HasValue == true && teamsUserIsEntitledToWriteTo != null && teamsUserIsEntitledToWriteTo.Contains(" + camelCaseName + "WWP.teamId.Value) == false)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\t" + camelCaseName + "WWP.isWriteable = false;");
                sb.AppendLine("\t\t\t\t\t" + camelCaseName + "WWP.notWriteableReason = \"Team with id of \" + " + camelCaseName + "WWP.teamId.Value + \" is not writeable.\";");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine("\t\t\t\telse if (" + camelCaseName + "WWP.departmentId.HasValue == true && departmentsUserIsEntitledToWriteTo != null && departmentsUserIsEntitledToWriteTo.Contains(" + camelCaseName + "WWP.departmentId.Value) == false)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\t" + camelCaseName + "WWP.isWriteable = false;");
                sb.AppendLine("\t\t\t\t\t" + camelCaseName + "WWP.notWriteableReason = \"Department with id of \" + " + camelCaseName + "WWP.departmentId.Value + \" is not writeable.\";");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine("\t\t\t\telse if (" + camelCaseName + "WWP.organizationId.HasValue == true && organizationsUserIsEntitledToWriteTo != null && organizationsUserIsEntitledToWriteTo.Contains(" + camelCaseName + "WWP.organizationId.Value) == false)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\t" + camelCaseName + "WWP.isWriteable = false;");
                sb.AppendLine("\t\t\t\t\t" + camelCaseName + "WWP.notWriteableReason = \"Organization with id of \" + " + camelCaseName + "WWP.organizationId.Value + \" is not writeable.\";");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine("\t\t\t\telse");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t// Default to not being able to write if no other condition was met.");
                sb.AppendLine("\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t" + camelCaseName + "WWP.isWriteable = false;");
                sb.AppendLine("\t\t\t\t\t" + camelCaseName + "WWP.notWriteableReason = \"No reason found to grant the ability to write.\";");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\telse");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t// Admin users can always write.");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t" + camelCaseName + "WWP.isWriteable = true;");
                sb.AppendLine("\t\t\t\t" + camelCaseName + "WWP.notWriteableReason = null;");
                sb.AppendLine("\t\t\t}");
                //sb.AppendLine("\t\t}");
                sb.AppendLine();
                sb.AppendLine("\t\t\treturn;");
                sb.AppendLine("\t\t}");
                sb.AppendLine();

                #endregion

                #region Error_throwing_for_data_visibility_rules

                //
                // Data privilege checking functions used by direct data accessors
                //
                if (scriptGenTable != null &&
                    (string.IsNullOrWhiteSpace(scriptGenTable.pdfRootFieldName) == false ||
                    string.IsNullOrWhiteSpace(scriptGenTable.mp4RootFieldName) == false ||
                    string.IsNullOrWhiteSpace(scriptGenTable.pngRootFieldName) == false ||
                    string.IsNullOrWhiteSpace(scriptGenTable.binaryDataRootFieldName) == false))
                {
                    sb.AppendLine("		private async Task<bool> ThrowErrorIfUserCannotReadFromIdAsync(SecurityUser securityUser, int id)");
                    sb.AppendLine("		{");
                    sb.AppendLine("			//");
                    sb.AppendLine("			// This will verify the provided user's entitlement to read from the the provided id.  If they cannot do it, an error will be thrown.");
                    sb.AppendLine("			//");
                    sb.AppendLine();
                    sb.AppendLine("			//");
                    sb.AppendLine("			// Admins can read all");
                    sb.AppendLine("			//");
                    sb.AppendLine("			bool userIsAdmin = await UserCanAdministerAsync(securityUser);");
                    sb.AppendLine("			if (userIsAdmin == true)");
                    sb.AppendLine("			{");
                    sb.AppendLine("				return true;");
                    sb.AppendLine("			}");
                    sb.AppendLine();
                    sb.AppendLine("			//");
                    sb.AppendLine("			// Make sure that the user can read from  to the entity");
                    sb.AppendLine("			//");
                    sb.AppendLine("			bool userIsReader = await UserCanReadAsync();");
                    sb.AppendLine("			if (userIsReader == false)");
                    sb.AppendLine("			{");
                    sb.AppendLine("				throw new Exception(\"User cannot read from this entity.\");");
                    sb.AppendLine("			}");
                    sb.AppendLine();
                    sb.AppendLine(UserTenantGuidCommands(3, false, true));
                    sb.AppendLine();
                    sb.AppendLine("			IQueryable<" + qualifiedEntity + "> query = (from x in _context." + Pluralize(entityName));
                    sb.AppendLine("												   where");
                    sb.AppendLine("												   (x.id == id)");
                    sb.AppendLine("												   select x);");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("			//");
                    sb.AppendLine("			// Apply the constraints for the user's tenant and data visibility configuration to ensure that this user can load the record that is attempting to be read.");
                    sb.AppendLine("			//");
                    sb.AppendLine("			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser);");
                    sb.AppendLine("			query = await AddTenantAndDataVisibilityConstraintsAsync(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin);");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("			" + qualifiedEntity + " existing = await query.FirstOrDefaultAsync();");
                    sb.AppendLine();
                    sb.AppendLine("			if (existing == null)");
                    sb.AppendLine("			{");
                    sb.AppendLine("				throw new Exception(\"Unable to read " + entityName + " with id of \" + id);");
                    sb.AppendLine("			}");
                    sb.AppendLine();
                    sb.AppendLine("			//");
                    sb.AppendLine("			// User can read if we get here.");
                    sb.AppendLine("			//");
                    sb.AppendLine("			return true;");
                    sb.AppendLine("		}");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("        private void ThrowErrorIfUserCannotReadFromId(SecurityUser securityUser, int id)");
                    sb.AppendLine("        {");
                    sb.AppendLine("            //");
                    sb.AppendLine("            // This will verify the provided user's entitlement to read from the the provided id.  If they cannot do it, an error will be thrown.");
                    sb.AppendLine("            //");
                    sb.AppendLine();
                    sb.AppendLine("            //");
                    sb.AppendLine("            // Admins can read all");
                    sb.AppendLine("            //");
                    sb.AppendLine("            bool userIsAdmin = UserCanAdminister(securityUser);");
                    sb.AppendLine("            if (userIsAdmin == true)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                return;");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            //");
                    sb.AppendLine("            // Make sure that the user can read from  to the entity");
                    sb.AppendLine("            //");
                    sb.AppendLine("            bool userIsReader = UserCanRead(securityUser);");
                    sb.AppendLine("            if (userIsReader == false)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                throw new Exception(\"User cannot read from this entity.\");");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine(UserTenantGuidCommands(3, false, true));
                    sb.AppendLine("            IQueryable<" + qualifiedEntity + "> query = (from x in _context." + Pluralize(entityName));
                    sb.AppendLine("                                           where");
                    sb.AppendLine("                                           (x.id == id)");
                    sb.AppendLine("                                           select x);");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("            //");
                    sb.AppendLine("            // Apply the constraints for the user's tenant and data visibility configuration to ensure that this user can load the record that is attempting to be read.");
                    sb.AppendLine("            //");
                    sb.AppendLine("            bool userIsSecurityAdmin = UserCanAdministerSecurityModule();");
                    sb.AppendLine("            query = AddTenantAndDataVisibilityConstraints(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin);");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("            " + qualifiedEntity + " existing = query.FirstOrDefault();");
                    sb.AppendLine();
                    sb.AppendLine("            if (existing == null)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                throw new Exception(\"Unable to read " + entityName + " with id of \" + id);");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            //");
                    sb.AppendLine("            // User can read if we get here.");
                    sb.AppendLine("            //");
                    sb.AppendLine("            return;");
                    sb.AppendLine("        }");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("		private async Task<bool> ThrowErrorIfUserCannotWriteToIdAsync(SecurityUser securityUser, int id, CancellationToken cancellationToken = default)");
                    sb.AppendLine("		{");
                    sb.AppendLine("			//");
                    sb.AppendLine("			// This will verify the provided user's entitlement to write to the provided id.  If they cannot do it, an error will be thrown.");
                    sb.AppendLine("			//");
                    sb.AppendLine();
                    sb.AppendLine("			//");
                    sb.AppendLine("			// Admins can write to all rows.");
                    sb.AppendLine("			//");
                    sb.AppendLine("			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);");
                    sb.AppendLine("			if (userIsAdmin == true)");
                    sb.AppendLine("			{");
                    sb.AppendLine("				return true;");
                    sb.AppendLine("			}");
                    sb.AppendLine();
                    sb.AppendLine("			//");
                    sb.AppendLine("			// Make sure that the user can write to the entity");
                    sb.AppendLine("			//");
                    sb.AppendLine("			bool userIsWriter = await UserCanWriteAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken);");
                    sb.AppendLine("			if (userIsWriter == false)");
                    sb.AppendLine("			{");
                    sb.AppendLine("				throw new Exception(\"User cannot write to this entity.\");");
                    sb.AppendLine("			}");
                    sb.AppendLine();
                    sb.AppendLine(UserTenantGuidCommands(3, true, true));
                    sb.AppendLine();
                    sb.AppendLine("			IQueryable<" + qualifiedEntity + "> query = (from x in _context." + Pluralize(entityName) + "");
                    sb.AppendLine("												   where");
                    sb.AppendLine("												   (x.id == id)");
                    sb.AppendLine("												   select x);");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("			//");
                    sb.AppendLine("			// Apply the constraints for the user's tenant and data visibility configuration to ensure that this user can load the record that is attempting to be saved.");
                    sb.AppendLine("			//");
                    sb.AppendLine("			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);");
                    sb.AppendLine("			query = await AddTenantAndDataVisibilityConstraintsAsync(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin, cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("			" + qualifiedEntity + " existing = await query.FirstOrDefaultAsync();");
                    sb.AppendLine();
                    sb.AppendLine("			if (existing == null)");
                    sb.AppendLine("			{");
                    sb.AppendLine("				throw new Exception(\"Unable to find " + entityName + " with id of \" + id);");
                    sb.AppendLine("			}");
                    sb.AppendLine();
                    sb.AppendLine("			List<int> organizationIdsUserIsEntitledToWriteTo = await GetOrganizationIdsUserIsEntitledToWriteToAsync(securityUser, cancellationToken);");
                    sb.AppendLine("			List<int> departmentIdsUserIsEntitledToWriteTo = await GetDepartmentIdsUserIsEntitledToWriteToAsync (securityUser, cancellationToken);");
                    sb.AppendLine("			List<int> teamIdsUserIsEntitledToWriteTo = await GetTeamIdsUserIsEntitledToWriteToAsync (securityUser, cancellationToken);");
                    sb.AppendLine("			List<int> userAndTheirReportIds = await GetListOfUserIdsForUserAndPeopleThatReportToThemAsync (securityUser, cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine("			Team team = null;");
                    sb.AppendLine("			Department department = null;");
                    sb.AppendLine("			Organization organization = null;");
                    sb.AppendLine("			User owner = null;");
                    sb.AppendLine();
                    sb.AppendLine("			if (existing.teamId.HasValue == true)");
                    sb.AppendLine("			{");
                    sb.AppendLine("				team = await GetTeamAsync(existing.teamId.Value, cancellationToken);");
                    sb.AppendLine("			}");
                    sb.AppendLine();
                    sb.AppendLine("			if (existing.departmentId.HasValue == true)");
                    sb.AppendLine("			{");
                    sb.AppendLine("				department = await GetDepartmentAsync(existing.departmentId.Value, cancellationToken);");
                    sb.AppendLine("			}");
                    sb.AppendLine("			else");
                    sb.AppendLine("			{");
                    sb.AppendLine("				//");
                    sb.AppendLine("				// Set the existing department to be that of the existing team if no existing department is explicitly provided and we have an existing team.");
                    sb.AppendLine("				//");
                    sb.AppendLine("				if (team != null)");
                    sb.AppendLine("				{");
                    sb.AppendLine("					department = await GetDepartmentAsync(team.departmentId, cancellationToken);");
                    sb.AppendLine("				}");
                    sb.AppendLine("			}");
                    sb.AppendLine();
                    sb.AppendLine("			if (existing.organizationId.HasValue == true)");
                    sb.AppendLine("			{");
                    sb.AppendLine("				organization = await GetOrganizationAsync(existing.organizationId.Value, cancellationToken);");
                    sb.AppendLine("			}");
                    sb.AppendLine("			else");
                    sb.AppendLine("			{");
                    sb.AppendLine("				//");
                    sb.AppendLine("				// Set the organization to be that of the department if no organization is explicitly provided and we have a department");
                    sb.AppendLine("				//");
                    sb.AppendLine("				if (department != null)");
                    sb.AppendLine("				{");
                    sb.AppendLine("					organization = await GetOrganizationAsync(department.organizationId, cancellationToken);");
                    sb.AppendLine("				}");
                    sb.AppendLine("			}");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("			if (existing.userId != null)");
                    sb.AppendLine("			{");
                    sb.AppendLine("				owner = await GetUserAsync(existing.userId.Value, cancellationToken);");
                    sb.AppendLine("			}");
                    sb.AppendLine();
                    sb.AppendLine("			VerifyDataWritePrivilegeForUpdate(owner, team, department, organization, userAndTheirReportIds, teamIdsUserIsEntitledToWriteTo, departmentIdsUserIsEntitledToWriteTo, organizationIdsUserIsEntitledToWriteTo);");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("			//");
                    sb.AppendLine("			// User can write if we get here.");
                    sb.AppendLine("			//");
                    sb.AppendLine("			return true;");
                    sb.AppendLine("		}");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("        private void ThrowErrorIfUserCannotWriteToId(SecurityUser securityUser, int id)");
                    sb.AppendLine("        {");
                    sb.AppendLine("            //");
                    sb.AppendLine("            // This will verify the provided user's entitlement to write to the provided id.  If they cannot do it, an error will be thrown.");
                    sb.AppendLine("            //");
                    sb.AppendLine();
                    sb.AppendLine("            //");
                    sb.AppendLine("            // Admins can write to all rows.");
                    sb.AppendLine("            //");
                    sb.AppendLine("            bool userIsAdmin = UserCanAdminister(securityUser);");
                    sb.AppendLine("            if (userIsAdmin == true)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                return;");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            //");
                    sb.AppendLine("            // Make sure that the user can write to the entity");
                    sb.AppendLine("            //");
                    sb.AppendLine("            bool userIsWriter = UserCanWrite(securityUser, WRITE_PERMISSION_LEVEL_REQUIRED);");
                    sb.AppendLine("            if (userIsWriter == false)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                throw new Exception(\"User cannot write to this entity.\");");
                    sb.AppendLine("            }");
                    sb.AppendLine();

                    sb.AppendLine(UserTenantGuidCommands(3, false, true));
                    sb.AppendLine("            IQueryable<" + qualifiedEntity + "> query = (from x in _context." + Pluralize(entityName));
                    sb.AppendLine("                                           where");
                    sb.AppendLine("                                           (x.id == id)");
                    sb.AppendLine("                                           select x);");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("            //");
                    sb.AppendLine("            // Apply the constraints for the user's tenant and data visibility configuration to ensure that this user can load the record that is attempting to be saved.");
                    sb.AppendLine("            //");
                    sb.AppendLine("            bool userIsSecurityAdmin = UserCanAdministerSecurityModule();");
                    sb.AppendLine("            query = AddTenantAndDataVisibilityConstraints(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin);");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("            " + qualifiedEntity + " existing = query.FirstOrDefault();");
                    sb.AppendLine();
                    sb.AppendLine("            if (existing == null)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                throw new Exception(\"Unable to find " + entityName + " with id of \" + id);");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            List<int> organizationIdsUserIsEntitledToWriteTo = GetOrganizationIdsUserIsEntitledToWriteTo(securityUser);");
                    sb.AppendLine("            List<int> departmentIdsUserIsEntitledToWriteTo = GetDepartmentIdsUserIsEntitledToWriteTo(securityUser);");
                    sb.AppendLine("            List<int> teamIdsUserIsEntitledToWriteTo = GetTeamIdsUserIsEntitledToWriteTo(securityUser);");
                    sb.AppendLine("            List<int> userAndTheirReportIds = GetListOfUserIdsForUserAndPeopleThatReportToThem(securityUser);");
                    sb.AppendLine();
                    sb.AppendLine("            Team team = null;");
                    sb.AppendLine("            Department department = null;");
                    sb.AppendLine("            Organization organization = null;");
                    sb.AppendLine("            User owner = null;");
                    sb.AppendLine();
                    sb.AppendLine("            if (existing.teamId.HasValue == true)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                team = GetTeam(existing.teamId.Value);");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            if (existing.departmentId.HasValue == true)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                department = GetDepartment(existing.departmentId.Value);");
                    sb.AppendLine("            }");
                    sb.AppendLine("            else");
                    sb.AppendLine("            {");
                    sb.AppendLine("                //");
                    sb.AppendLine("                // Set the existing department to be that of the existing team if no existing department is explicitly provided and we have an existing team.");
                    sb.AppendLine("                //");
                    sb.AppendLine("                if (team != null)");
                    sb.AppendLine("                {");
                    sb.AppendLine("                    department = GetDepartment(team.departmentId);");
                    sb.AppendLine("                }");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            if (existing.organizationId.HasValue == true)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                organization = GetOrganization(existing.organizationId.Value);");
                    sb.AppendLine("            }");
                    sb.AppendLine("            else");
                    sb.AppendLine("            {");
                    sb.AppendLine("                //");
                    sb.AppendLine("                // Set the organization to be that of the department if no organization is explicitly provided and we have a department");
                    sb.AppendLine("                //");
                    sb.AppendLine("                if (department != null)");
                    sb.AppendLine("                {");
                    sb.AppendLine("                    organization = GetOrganization(department.organizationId);");
                    sb.AppendLine("                }");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("            if (existing.userId != null)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                owner = GetUser(existing.userId.Value);");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            VerifyDataWritePrivilegeForUpdate(owner, team, department, organization, userAndTheirReportIds, teamIdsUserIsEntitledToWriteTo, departmentIdsUserIsEntitledToWriteTo, organizationIdsUserIsEntitledToWriteTo);");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("            //");
                    sb.AppendLine("            // User can write if we get here.");
                    sb.AppendLine("            //");
                    sb.AppendLine("            return;");
                    sb.AppendLine("        }");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("        private void ThrowErrorIfUserCannotChangeOwnerForId(SecurityUser securityUser, int id)");
                    sb.AppendLine("        {");
                    sb.AppendLine("            //");
                    sb.AppendLine("            // This will verify the provided user's entitlement to change the owner for the provided id.  If they cannot do it, an error will be thrown.");
                    sb.AppendLine("            //");
                    sb.AppendLine();
                    sb.AppendLine("            //");
                    sb.AppendLine("            // Admins can change owner to all rows.");
                    sb.AppendLine("            //");
                    sb.AppendLine("            bool userIsAdmin = UserCanAdminister(securityUser);");
                    sb.AppendLine("            if (userIsAdmin == true)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                return;");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            //");
                    sb.AppendLine("            // First, make sure that the user can write to the entity");
                    sb.AppendLine("            //");
                    sb.AppendLine("            bool userIsWriter = UserCanWrite(securityUser, WRITE_PERMISSION_LEVEL_REQUIRED);");
                    sb.AppendLine("            if (userIsWriter == false)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                throw new Exception(\"User cannot write to this entity.\");");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine(UserTenantGuidCommands(3, false, true));
                    sb.AppendLine("            IQueryable<" + qualifiedEntity + "> query = (from x in _context." + Pluralize(entityName));
                    sb.AppendLine("                                              where");
                    sb.AppendLine("                                              (x.id == id)");
                    sb.AppendLine("                                              select x);");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("            //");
                    sb.AppendLine("            // Apply the constraints for the user's tenant and data visibility configuration to ensure that this user can load the record that is attempting to be saved.");
                    sb.AppendLine("            //");
                    sb.AppendLine("            bool userIsSecurityAdmin = UserCanAdministerSecurityModule();");
                    sb.AppendLine("            query = AddTenantAndDataVisibilityConstraints(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin);");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("            " + qualifiedEntity + " existing = query.FirstOrDefault();");
                    sb.AppendLine();
                    sb.AppendLine("            if (existing == null)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                throw new Exception(\"Unable to find " + entityName + " with id of \" + id);");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            List<int> organizationIdsUserIsEntitledToChangeOwnerFor = GetOrganizationIdsUserIsEntitledToChangeOwnerFor(securityUser);");
                    sb.AppendLine("            List<int> departmentIdsUserIsEntitledToChangeOwnerFor = GetDepartmentIdsUserIsEntitledToChangeOwnerFor(securityUser);");
                    sb.AppendLine("            List<int> teamIdsUserIsEntitledToChangeOwnerFor = GetTeamIdsUserIsEntitledToChangeOwnerFor(securityUser);");
                    sb.AppendLine();
                    sb.AppendLine("            Team team = null;");
                    sb.AppendLine("            Department department = null;");
                    sb.AppendLine("            Organization organization = null;");
                    sb.AppendLine("            User owner = null;");
                    sb.AppendLine();
                    sb.AppendLine("            if (existing.teamId.HasValue == true)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                team = GetTeam(existing.teamId.Value);");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            if (existing.departmentId.HasValue == true)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                department = GetDepartment(existing.departmentId.Value);");
                    sb.AppendLine("            }");
                    sb.AppendLine("            else");
                    sb.AppendLine("            {");
                    sb.AppendLine("                //");
                    sb.AppendLine("                // Set the existing department to be that of the existing team if no existing department is explicitly provided and we have an existing team.");
                    sb.AppendLine("                //");
                    sb.AppendLine("                if (team != null)");
                    sb.AppendLine("                {");
                    sb.AppendLine("                    department = GetDepartment(team.departmentId);");
                    sb.AppendLine("                }");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            if (existing.organizationId.HasValue == true)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                organization = GetOrganization(existing.organizationId.Value);");
                    sb.AppendLine("            }");
                    sb.AppendLine("            else");
                    sb.AppendLine("            {");
                    sb.AppendLine("                //");
                    sb.AppendLine("                // Set the organization to be that of the department if no organization is explicitly provided and we have a department");
                    sb.AppendLine("                //");
                    sb.AppendLine("                if (department != null)");
                    sb.AppendLine("                {");
                    sb.AppendLine("                    organization = GetOrganization(department.organizationId);");
                    sb.AppendLine("                }");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            if (existing.userId != null)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                owner = GetUser(existing.userId.Value);");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            VerifyDataChangeOwnerPrivilege(team, department, organization, teamIdsUserIsEntitledToChangeOwnerFor, departmentIdsUserIsEntitledToChangeOwnerFor, organizationIdsUserIsEntitledToChangeOwnerFor);");
                    sb.AppendLine();
                    sb.AppendLine("            //");
                    sb.AppendLine("            // User can change owner if we get here.");
                    sb.AppendLine("            //");
                    sb.AppendLine("            return;");
                    sb.AppendLine("        }");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("        private void ThrowErrorIfUserCannotChangeHierarchyForId(SecurityUser securityUser, int id)");
                    sb.AppendLine("        {");
                    sb.AppendLine("            //");
                    sb.AppendLine("            // This will verify the provided user's entitlement to change the data visibility hierarchy for the provided id.  If they cannot do it, an error will be thrown.");
                    sb.AppendLine("            //");
                    sb.AppendLine();
                    sb.AppendLine("            //");
                    sb.AppendLine("            // Admins can write to all rows.");
                    sb.AppendLine("            //");
                    sb.AppendLine("            bool userIsAdmin = UserCanAdminister();");
                    sb.AppendLine("            if (userIsAdmin == true)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                return;");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            //");
                    sb.AppendLine("            // First, Make sure that the user can write to the entity");
                    sb.AppendLine("            //");
                    sb.AppendLine("            bool userIsWriter = UserCanWrite(WRITE_PERMISSION_LEVEL_REQUIRED);");
                    sb.AppendLine("            if (userIsWriter == false)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                throw new Exception(\"User cannot write to this entity.\");");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine(UserTenantGuidCommands(3, false, true));
                    sb.AppendLine("            IQueryable<" + qualifiedEntity + "> query = (from x in _context." + Pluralize(entityName));
                    sb.AppendLine("                                              where");
                    sb.AppendLine("                                              (x.id == id)");
                    sb.AppendLine("                                              select x);");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("            //");
                    sb.AppendLine("            // Apply the constraints for the user's tenant and data visibility configuration to ensure that this user can load the record that is attempting to be saved.");
                    sb.AppendLine("            //");
                    sb.AppendLine("            bool userIsSecurityAdmin = UserCanAdministerSecurityModule();");
                    sb.AppendLine("            query = AddTenantAndDataVisibilityConstraints(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin);");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("            " + qualifiedEntity + " existing = query.FirstOrDefault();");
                    sb.AppendLine();
                    sb.AppendLine("            if (existing == null)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                throw new Exception(\"Unable to find " + entityName + " with id of \" + id);");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            List<int> organizationIdsUserIsEntitledToChangeHierarchyFor = GetOrganizationIdsUserIsEntitledToChangeHierarchyFor(securityUser);");
                    sb.AppendLine("            List<int> departmentIdsUserIsEntitledToChangeHierarchyFor = GetDepartmentIdsUserIsEntitledToChangeHierarchyFor(securityUser);");
                    sb.AppendLine("            List<int> teamIdsUserIsEntitledToChangeHierarchyFor = GetTeamIdsUserIsEntitledToChangeHierarchyFor(securityUser);");
                    sb.AppendLine();
                    sb.AppendLine("            Team team = null;");
                    sb.AppendLine("            Department department = null;");
                    sb.AppendLine("            Organization organization = null;");
                    sb.AppendLine("            User owner = null;");
                    sb.AppendLine();
                    sb.AppendLine("            if (existing.teamId.HasValue == true)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                team = GetTeam(existing.teamId.Value);");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            if (existing.departmentId.HasValue == true)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                department = GetDepartment(existing.departmentId.Value);");
                    sb.AppendLine("            }");
                    sb.AppendLine("            else");
                    sb.AppendLine("            {");
                    sb.AppendLine("                //");
                    sb.AppendLine("                // Set the existing department to be that of the existing team if no existing department is explicitly provided and we have an existing team.");
                    sb.AppendLine("                //");
                    sb.AppendLine("                if (team != null)");
                    sb.AppendLine("                {");
                    sb.AppendLine("                    department = GetDepartment(team.departmentId);");
                    sb.AppendLine("                }");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            if (existing.organizationId.HasValue == true)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                organization = GetOrganization(existing.organizationId.Value);");
                    sb.AppendLine("            }");
                    sb.AppendLine("            else");
                    sb.AppendLine("            {");
                    sb.AppendLine("                //");
                    sb.AppendLine("                // Set the organization to be that of the department if no organization is explicitly provided and we have a department");
                    sb.AppendLine("                //");
                    sb.AppendLine("                if (department != null)");
                    sb.AppendLine("                {");
                    sb.AppendLine("                    organization = GetOrganization(department.organizationId);");
                    sb.AppendLine("                }");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            if (existing.userId != null)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                owner = GetUser(existing.userId.Value);");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            VerifyDataChangeHierarchyPrivilege(team, department, organization, teamIdsUserIsEntitledToChangeHierarchyFor, departmentIdsUserIsEntitledToChangeHierarchyFor, organizationIdsUserIsEntitledToChangeHierarchyFor);");
                    sb.AppendLine();
                    sb.AppendLine("            //");
                    sb.AppendLine("            // User can change hierarchy if we get here.");
                    sb.AppendLine("            //");
                    sb.AppendLine("            return;");
                    sb.AppendLine("        }");
                    sb.AppendLine();
                    sb.AppendLine();
                }
                #endregion

                #region Data_visibility_configuration_handling


                sb.AppendLine(@"

        private static void ConfigureOwnerIsChangeable(<EntityName>WithWritePermissionDetails <ObjectName>WWP, SecurityUser securityUser, Guid? userTenantGuid, bool userIsAdmin, bool userIsWriter, List<int> organizationsUserIsEntitledToChangeOwnerFor, List<int> departmentsUserIsEntitledToChangeOwnerFor, List<int> teamsUserIsEntitledToChangeOwnerFor)
		{
			//
			// Determine if the <EntityName> can have its owner changed or not.  If it can't, then indicate the reason.
			//

            if (userTenantGuid.HasValue == false)
            {
				<ObjectName>WWP.canChangeOwner = false;
			}
			else if (userIsWriter == false)
			{
				<ObjectName>WWP.canChangeOwner = false;
			}
			else if (userIsAdmin == false)
			{
				//
				// Non-Admin users may not be able to change ownership for some records.
				//
				// Start checking the data visibility fields for the ability to write starting at the organization level, and moving down
				//
				//		- Change owner permission at Organization implies ability to write all all of the organization's department and team levels
				//		- Change owner permission at Department implies ability to write to all of the department's teams
				//
				// First, check if the <EntityName> is owner changeable in the first 3 conditions, and if nothing matches there, then work the other way to find the reason why it's not writeable.
				//
				if (<ObjectName>WWP.organizationId.HasValue == true && organizationsUserIsEntitledToChangeOwnerFor != null && organizationsUserIsEntitledToChangeOwnerFor.Contains(<ObjectName>WWP.organizationId.Value) == true)
				{
					//
					// <EntityName>'s Organization is owner changeable, so this record can have its owner changed
					//
					<ObjectName>WWP.canChangeOwner = true;
				}
			    else if (userIsAdmin == true)
			    {
				    <ObjectName>WWP.canChangeOwner = true;
			    }
				else if (<ObjectName>WWP.departmentId.HasValue == true && departmentsUserIsEntitledToChangeOwnerFor != null && departmentsUserIsEntitledToChangeOwnerFor.Contains(<ObjectName>WWP.departmentId.Value) == true)
				{
					//
					// <EntityName>'s Department is owner changeable, so this record can its owner changed.
					//
					<ObjectName>WWP.canChangeOwner = true;
				}
				else if (<ObjectName>WWP.teamId.HasValue == true && teamsUserIsEntitledToChangeOwnerFor != null && teamsUserIsEntitledToChangeOwnerFor.Contains(<ObjectName>WWP.teamId.Value) == true)
				{
					//
					// <EntityName>'s Team is owner changeable, so this record can its owner changed.
					//
					<ObjectName>WWP.canChangeOwner = true;
				}
				//
				// Now check if the user can't change owner, and if so, cite the reason
				//
				else if (<ObjectName>WWP.teamId.HasValue == true && teamsUserIsEntitledToChangeOwnerFor != null && teamsUserIsEntitledToChangeOwnerFor.Contains(<ObjectName>WWP.teamId.Value) == false)
				{
					<ObjectName>WWP.canChangeOwner = false;

					//
					// If <EntityName> can be written to, but the <EntityName> owner can't be changed, note that in the reason
					//
					if (<ObjectName>WWP.isWriteable == true)
					{
						<ObjectName>WWP.notWriteableReason += ""Team with id of "" + <ObjectName>WWP.teamId.Value + "" is not owner changeable.  "";

                    }
                }
				else if (<ObjectName>WWP.departmentId.HasValue == true && departmentsUserIsEntitledToChangeOwnerFor != null && departmentsUserIsEntitledToChangeOwnerFor.Contains(<ObjectName>WWP.departmentId.Value) == false)
				{
					<ObjectName>WWP.canChangeOwner = false;
					//
					// If <EntityName> can be written to, but the <EntityName> owner can't be changed, note that in the reason
					//
					if (<ObjectName>WWP.isWriteable == true)
					{
						<ObjectName>WWP.notWriteableReason += ""Department with id of "" + <ObjectName>WWP.departmentId.Value + "" is not owner changeable.  "";
					}
                }
				else if (<ObjectName>WWP.organizationId.HasValue == true && organizationsUserIsEntitledToChangeOwnerFor != null && organizationsUserIsEntitledToChangeOwnerFor.Contains(<ObjectName>WWP.organizationId.Value) == false)
				{
					<ObjectName>WWP.canChangeOwner = false;

					//
					// If <EntityName> can be written to, but the <EntityName> owner can't be changed, note that in the reason
					//
					if (<ObjectName>WWP.isWriteable == true)
					{
						<ObjectName>WWP.notWriteableReason += ""Organization with id of "" + <ObjectName>WWP.organizationId.Value + "" is not owner changeable.  "";
					}
				}
				else
				{
					//
					// Default to not being able to change <EntityName> owner if no other condition was met.
					//
					<ObjectName>WWP.canChangeOwner = false;

					if (<ObjectName>WWP.isWriteable == true)
					{
						<ObjectName>WWP.notWriteableReason += ""No reason to grant owner change ability found.  "";
					}
				}
			}
			else
			{
				//
				// Admin users can change owner all the time.
				//
				<ObjectName>WWP.canChangeOwner = true;
			}

			return;
		}


		private static void ConfigureHierarchyIsChangeable(<EntityName>WithWritePermissionDetails <ObjectName>WWP, SecurityUser securityUser, Guid? userTenantGuid, bool userIsAdmin, bool userIsWriter, List<int> organizationsUserIsEntitledToChangeHierarchyFor, List<int> departmentsUserIsEntitledToChangeHierarchyFor, List<int> teamsUserIsEntitledToChangeHierarchyFor)
        {
            //
            // Determine if the <EntityName> can have its hierarchy fields change or not.  If it can't, then indicate the reason.
            //
            if (userTenantGuid.HasValue == false)
            {
                <ObjectName>WWP.canChangeHierarchy = false;
            }
			else if (userIsAdmin == true)
			{
				<ObjectName>WWP.canChangeHierarchy = true;
			}
            else if (userIsWriter == false)
            {
                <ObjectName>WWP.canChangeHierarchy = false;
            }
            else if (userIsAdmin == false)
            {
                //
                // Non-Admin users may not be able to change Hierarchy fields for some records.
                //
                // Start checking the data visibility fields for the ability to write starting at the organization level, and moving down
                //
                //		- Change hierarchy permission at Organization implies ability to write all all of the organization's department and team levels
                //		- Change hierarchy permission at Department implies ability to write to all of the department's teams
                //
                // First, check if the <EntityName> is hierarchy changeable in the first 3 conditions, and if nothing matches there, then work the other way to find the reason why it's not writeable.
                //
                if (<ObjectName>WWP.organizationId.HasValue == true && organizationsUserIsEntitledToChangeHierarchyFor != null && organizationsUserIsEntitledToChangeHierarchyFor.Contains(<ObjectName>WWP.organizationId.Value) == true)
                {
                    //
                    // <EntityName>'s Organization is hierarchy changeable, so this record can have its hierarchy changed
                    //
                    <ObjectName>WWP.canChangeHierarchy = true;
                }
                else if (<ObjectName>WWP.departmentId.HasValue == true && departmentsUserIsEntitledToChangeHierarchyFor != null && departmentsUserIsEntitledToChangeHierarchyFor.Contains(<ObjectName>WWP.departmentId.Value) == true)
                {
                    //
                    // <EntityName>'s Department is hierarchy changeable, so this record can have its hierarchy changed.
                    //
                    <ObjectName>WWP.canChangeHierarchy = true;
                }
                else if (<ObjectName>WWP.teamId.HasValue == true && teamsUserIsEntitledToChangeHierarchyFor != null && teamsUserIsEntitledToChangeHierarchyFor.Contains(<ObjectName>WWP.teamId.Value) == true)
                {
                    //
                    // <EntityName>'s Team is hierarchy changeable, so this record can have its hierarchy changed.
                    //
                    <ObjectName>WWP.canChangeHierarchy = true;
                }
                //
                // Now check if the user can't change hierarchy, and if so, cite the reason
                //
                else if (<ObjectName>WWP.teamId.HasValue == true && teamsUserIsEntitledToChangeHierarchyFor != null && teamsUserIsEntitledToChangeHierarchyFor.Contains(<ObjectName>WWP.teamId.Value) == false)
                {
                    <ObjectName>WWP.canChangeHierarchy = false;

                    //
                    // If <EntityName> can be written to, but the <EntityName> owner can't be changed, note that in the reason
                    //
                    if (<ObjectName>WWP.isWriteable == true)
                    {
                        <ObjectName>WWP.notWriteableReason += ""Team with id of "" + <ObjectName>WWP.teamId.Value + "" is not hierarchy changeable.  "";
                    }
                }
                else if (<ObjectName>WWP.departmentId.HasValue == true && departmentsUserIsEntitledToChangeHierarchyFor != null && departmentsUserIsEntitledToChangeHierarchyFor.Contains(<ObjectName>WWP.departmentId.Value) == false)
                {
                    <ObjectName>WWP.canChangeHierarchy = false;
                    //
                    // If <EntityName> can be written to, but the <EntityName> owner can't be changed, note that in the reason
                    //
                    if (<ObjectName>WWP.isWriteable == true)
                    {
                        <ObjectName>WWP.notWriteableReason = ""Department with id of "" + <ObjectName>WWP.departmentId.Value + "" is not hierarchy changeable.  "";
                    }
                }
                else if (<ObjectName>WWP.organizationId.HasValue == true && organizationsUserIsEntitledToChangeHierarchyFor != null && organizationsUserIsEntitledToChangeHierarchyFor.Contains(<ObjectName>WWP.organizationId.Value) == false)
                {
                    <ObjectName>WWP.canChangeOwner = false;

                    //
                    // If <EntityName> can be written to, but hte <EntityName> owner can't be changed, note that in the reason
                    //
                    if (<ObjectName>WWP.isWriteable == true)
                    {
                        <ObjectName>WWP.notWriteableReason = ""Organization with id of "" + <ObjectName>WWP.organizationId.Value + "" is not hierarchy changeable.  "";
                    }
                }
                else
                {
                    //
                    // Default to not being able to change hierarchy if no other condition was met.
                    //
                    <ObjectName>WWP.canChangeHierarchy = false;

                    if (<ObjectName>WWP.isWriteable == true)
                    {
                        <ObjectName>WWP.notWriteableReason = ""No reason to grant hierarchy change ability found.  "";
                    }
                }
            }
            else
            {
                //
                // Admin users can always change hierarchy
                //
                <ObjectName>WWP.canChangeHierarchy = true;
            }

            return;
        }".Replace("<EntityName>", entityName).Replace("<ObjectName>", camelCaseName));

            }

            #endregion

            #region Disposal_handling

            //if (dataVisibilityEnabled == false)
            //{
            //    sb.AppendLine();
            //    sb.AppendLine("\t\tprotected override void Dispose(bool disposing)");
            //    sb.AppendLine("\t\t{");
            //    sb.AppendLine("\t\t\tif (disposing)");
            //    sb.AppendLine("\t\t\t{");
            //    sb.AppendLine("\t\t\t\tdb.Dispose();");
            //    sb.AppendLine("\t\t\t}");
            //    sb.AppendLine("\t\t\tbase.Dispose(disposing);");
            //    sb.AppendLine("\t\t}");
            //}

            #endregion

            #region Data_Upload_handling



            if (scriptGenTable != null &&
                (string.IsNullOrWhiteSpace(scriptGenTable.pdfRootFieldName) == false ||
                string.IsNullOrWhiteSpace(scriptGenTable.mp4RootFieldName) == false ||
                string.IsNullOrWhiteSpace(scriptGenTable.pngRootFieldName) == false ||
                string.IsNullOrWhiteSpace(scriptGenTable.binaryDataRootFieldName) == false))
            {
                sb.AppendLine();
                sb.AppendLine();

                sb.AppendLine("        [Route(\"api/" + singularForRouting + "/Data/{id:int}\")]");
                sb.AppendLine("        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]");
                sb.AppendLine("        [HttpPost]");
                sb.AppendLine("        [HttpPut]");
                sb.AppendLine("        public async Task<IActionResult> UploadData(int id, CancellationToken cancellationToken = default)");
                sb.AppendLine("        {");
                sb.AppendLine("            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED) == false)");
                sb.AppendLine("            {");
                sb.AppendLine("                return Forbid();");
                sb.AppendLine("            }");
                sb.AppendLine();
                sb.AppendLine("            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);");
                sb.AppendLine();
                if (dataVisibilityEnabled == true)
                {
                    sb.AppendLine();
                    sb.AppendLine("            try");
                    sb.AppendLine("            {");
                    sb.AppendLine("                await ThrowErrorIfUserCannotWriteToIdAsync(securityUser, id, cancellationToken);");
                    sb.AppendLine("            }");
                    sb.AppendLine("            catch(Exception ex)");
                    sb.AppendLine("            {");
                    sb.AppendLine("               return Problem(\"Insufficient write access.\");");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                }
                sb.AppendLine("			MediaTypeHeaderValue mediaTypeHeader; ");
                sb.AppendLine();
                sb.AppendLine("            if (!HttpContext.Request.HasFormContentType ||");
                sb.AppendLine("				!MediaTypeHeaderValue.TryParse(HttpContext.Request.ContentType, out mediaTypeHeader) ||");
                sb.AppendLine("                string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))");
                sb.AppendLine("            {");
                sb.AppendLine("                return new UnsupportedMediaTypeResult();");
                sb.AppendLine("            }");
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("            " + databaseNamespace + "." + entityName + " " + camelCaseName + " = await (from x in _context." + Pluralize(entityName) + " where x.id == id && x.active == true && x.deleted == false select x).FirstOrDefaultAsync();");
                sb.AppendLine("            if (" + camelCaseName + " == null)");
                sb.AppendLine("            {");
                sb.AppendLine("                return NotFound();");
                sb.AppendLine("            }");
                sb.AppendLine();
                sb.AppendLine("            bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();");
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("            // This will be used to signal whether we are saving data or clearing it.");
                sb.AppendLine("            bool foundFileData = false;");
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("            //");
                sb.AppendLine("            // This will get the first file from the request and save it");
                sb.AppendLine("            //");
                sb.AppendLine("			try");
                sb.AppendLine("			{");
                sb.AppendLine("                MultipartReader reader = new MultipartReader(mediaTypeHeader.Boundary.Value, HttpContext.Request.Body);");
                sb.AppendLine("                MultipartSection section = await reader.ReadNextSectionAsync();");
                sb.AppendLine();
                sb.AppendLine("                while (section != null)");
                sb.AppendLine("				{");
                sb.AppendLine("					ContentDispositionHeaderValue contentDisposition;");
                sb.AppendLine();
                sb.AppendLine("					bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);");
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("					if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals(\"form-data\") &&");
                sb.AppendLine("						!string.IsNullOrEmpty(contentDisposition.FileName.Value))");
                sb.AppendLine("					{");
                sb.AppendLine();
                sb.AppendLine("						foundFileData = true;");
                sb.AppendLine("						string fileName = contentDisposition.FileName.ToString().Trim('\"');");
                sb.AppendLine();
                sb.AppendLine("						// default the mime type to be the one for arbitrary binary data unless we have a mime type on the content headers that tells us otherwise.");
                sb.AppendLine("						MediaTypeHeaderValue mediaType;");
                sb.AppendLine("						bool hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);");
                sb.AppendLine();
                sb.AppendLine("						string mimeType = \"application/octet-stream\";");
                sb.AppendLine("						if (hasMediaTypeHeader && mediaTypeHeader.MediaType != null )");
                sb.AppendLine("						{");
                sb.AppendLine("							mimeType = mediaTypeHeader.MediaType.ToString();");
                sb.AppendLine("						}");

                if (versionControlEnabled == false)
                {

                    sb.AppendLine();
                    sb.AppendLine("                        try");
                    sb.AppendLine("                        {");

                    sb.AppendLine("                            ");
                    sb.AppendLine("                            " + camelCaseName + "." + dataRootFieldName + "FileName = fileName.Trim();");
                    sb.AppendLine("                            " + camelCaseName + "." + dataRootFieldName + "MimeType = mimeType;");
                    sb.AppendLine("                            " + camelCaseName + "." + dataRootFieldName + "Size = section.Body.Length;");

                    sb.AppendLine("                            if (diskBasedBinaryStorageMode == true &&");


                    sb.AppendLine("                            " + camelCaseName + "." + dataRootFieldName + "FileName != null &&");
                    sb.AppendLine("                            " + camelCaseName + "." + dataRootFieldName + "Size > 0)");
                    sb.AppendLine("                            {");
                    sb.AppendLine("                            	//");
                    sb.AppendLine("                            	// write the bytes to disk");
                    sb.AppendLine("                            	//");
                    sb.AppendLine("                            	WriteDataToDisk(" + camelCaseName + ".objectGuid, 0, section.Body, \"" + dataFileNameExtension + "\");");

                    sb.AppendLine("                            	//");
                    sb.AppendLine("                            	// Clear the data from the object before we put it into the db");
                    sb.AppendLine("                            	//");
                    sb.AppendLine("                             " + camelCaseName + "." + dataRootFieldName + "Data = null;");
                    sb.AppendLine("                            }");
                    sb.AppendLine("                            else");
                    sb.AppendLine("                            {");
                    sb.AppendLine("							    using (MemoryStream memoryStream = new MemoryStream((int)section.Body.Length))");
                    sb.AppendLine("								{");
                    sb.AppendLine("									section.Body.CopyTo(memoryStream);");
                    sb.AppendLine("									" + camelCaseName + "." + dataRootFieldName + "Data = memoryStream.ToArray();");
                    sb.AppendLine("								}");
                    sb.AppendLine("                            } ");
                    sb.AppendLine("                            ");
                    sb.AppendLine("                            await _context.SaveChangesAsync(cancellationToken);");
                    sb.AppendLine("                            ");
                    sb.AppendLine("                            CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, \"" + entityName + " Data Uploaded with filename of \" + fileName + \" and with size of \" + section.Body.Length, id.ToString());");
                    sb.AppendLine("                        }");
                    sb.AppendLine("                        catch (Exception ex)");
                    sb.AppendLine("                        {");
                    sb.AppendLine("                            CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, \"" + entityName + " Data Upload Failed.\", false, id.ToString(), \"\", \"\", ex);");
                    sb.AppendLine();
                    sb.AppendLine("                            return Problem(ex.Message);");
                    sb.AppendLine("                        }");


                }
                else
                {
                    sb.AppendLine();
                    sb.AppendLine("						lock (" + camelCaseName + "PutSyncRoot)");
                    sb.AppendLine("						{");
                    sb.AppendLine("							try");
                    sb.AppendLine("							{");
                    sb.AppendLine("								using (IDbContextTransaction transaction = _context.Database.BeginTransaction())");
                    sb.AppendLine("								{");
                    sb.AppendLine("									" + camelCaseName + "." + dataRootFieldName + "FileName = fileName.Trim();");
                    sb.AppendLine("									" + camelCaseName + "." + dataRootFieldName + "MimeType = mimeType;");
                    sb.AppendLine("									" + camelCaseName + "." + dataRootFieldName + "Size = section.Body.Length;");
                    sb.AppendLine();
                    sb.AppendLine("									" + camelCaseName + ".versionNumber++;");
                    sb.AppendLine();
                    sb.AppendLine("									if (diskBasedBinaryStorageMode == true &&");
                    sb.AppendLine("										 " + camelCaseName + "." + dataRootFieldName + "FileName != null &&");
                    sb.AppendLine("										 " + camelCaseName + "." + dataRootFieldName + "Size > 0)");
                    sb.AppendLine("									{");
                    sb.AppendLine("										//");
                    sb.AppendLine("										// write the bytes to disk");
                    sb.AppendLine("										//");
                    sb.AppendLine("										WriteDataToDisk(" + camelCaseName + ".objectGuid, " + camelCaseName + ".versionNumber, section.Body, \"data\");");
                    sb.AppendLine("										//");
                    sb.AppendLine("										// Clear the data from the object before we put it into the db");
                    sb.AppendLine("										//");
                    sb.AppendLine("										" + camelCaseName + "." + dataRootFieldName + "Data = null;");
                    sb.AppendLine("									}");
                    sb.AppendLine("									else");
                    sb.AppendLine("									{");
                    sb.AppendLine("										using (MemoryStream memoryStream = new MemoryStream((int)section.Body.Length))");
                    sb.AppendLine("										{");
                    sb.AppendLine("											section.Body.CopyTo(memoryStream);");
                    sb.AppendLine("											" + camelCaseName + "." + dataRootFieldName + "Data = memoryStream.ToArray();");
                    sb.AppendLine("										}");
                    sb.AppendLine("									}");
                    sb.AppendLine("									//");
                    sb.AppendLine("									// Now add the change history");
                    sb.AppendLine("									//");

                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine("\t\t\t\t\t\t\t\t\tUser user = GetUser(" + camelCaseName + ".tenantGuid, securityUser);");
                        sb.AppendLine("\t\t\t\t\t\t\t\t\t" + entityName + "ChangeHistory " + camelCaseName + "ChangeHistory = new " + entityName + "ChangeHistory();");

                        // Note we are using the reserved word version of camel case here to suit EFPT's naming convention.  this may need to be tweaked later if that changes..
                        sb.AppendLine("\t\t\t\t\t\t\t\t\t" + camelCaseName + "ChangeHistory." + CamelCase(entityName, true) + "Id = " + camelCaseName + ".id;");

                        sb.AppendLine("\t\t\t\t\t\t\t\t\t" + camelCaseName + "ChangeHistory.versionNumber = " + camelCaseName + ".versionNumber;");
                        sb.AppendLine("\t\t\t\t\t\t\t\t\t" + camelCaseName + "ChangeHistory.timeStamp = DateTime.UtcNow;");
                        sb.AppendLine("\t\t\t\t\t\t\t\t\t" + camelCaseName + "ChangeHistory.userId = user.id;");
                        if (multiTenancyEnabled == true)
                        {
                            sb.AppendLine("									" + camelCaseName + "ChangeHistory.tenantGuid = " + camelCaseName + ".tenantGuid;");
                        }

                        sb.AppendLine($"                                    {camelCaseName}ChangeHistory.data = JsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName}));");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t\t\t\t\t\t\t" + entityName + "ChangeHistory " + camelCaseName + "ChangeHistory = new " + entityName + "ChangeHistory();");
                        sb.AppendLine("\t\t\t\t\t\t\t\t\t" + camelCaseName + "ChangeHistory." + CamelCase(entityName, false) + "Id = " + camelCaseName + ".id;");
                        sb.AppendLine("\t\t\t\t\t\t\t\t\t" + camelCaseName + "ChangeHistory.versionNumber = " + camelCaseName + ".versionNumber;");
                        sb.AppendLine("\t\t\t\t\t\t\t\t\t" + camelCaseName + "ChangeHistory.timeStamp = DateTime.UtcNow;");
                        sb.AppendLine("\t\t\t\t\t\t\t\t\t" + camelCaseName + "ChangeHistory.userId = securityUser.id;");
                        if (multiTenancyEnabled == true)
                        {
                            sb.AppendLine("\t\t\t\t\t\t\t\t\t" + camelCaseName + "ChangeHistory.tenantGuid = " + camelCaseName + ".tenantGuid;");
                        }
                        sb.AppendLine($"\t\t\t\t\t\t\t\t\t{camelCaseName}ChangeHistory.data = JsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName}));");
                    }

                    sb.AppendLine("									_context." + entityName + "ChangeHistories.Add(" + camelCaseName + "ChangeHistory);");

                    sb.AppendLine();
                    sb.AppendLine("									_context.SaveChanges();");
                    sb.AppendLine();
                    sb.AppendLine("									transaction.Commit();");
                    sb.AppendLine();
                    sb.AppendLine("									CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, \"" + entityName + " Data Uploaded with filename of \" + fileName + \" and with size of \" + section.Body.Length, id.ToString());");
                    sb.AppendLine("								}");
                    sb.AppendLine("							}");
                    sb.AppendLine("							catch (Exception ex)");
                    sb.AppendLine("							{");
                    sb.AppendLine("								CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, \"" + entityName + " Data Upload Failed.\", false, id.ToString(), \"\", \"\", ex);");
                    sb.AppendLine();
                    sb.AppendLine("								return Problem(ex.Message);");
                    sb.AppendLine("							}");
                    sb.AppendLine("						}");
                }
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("						//");
                sb.AppendLine("						// Stop looking for more files.");
                sb.AppendLine("						//");
                sb.AppendLine("						break;");
                sb.AppendLine("					}");
                sb.AppendLine();
                sb.AppendLine("					section = await reader.ReadNextSectionAsync();");
                sb.AppendLine("				}");
                sb.AppendLine("            }");
                sb.AppendLine("            catch (Exception ex)");
                sb.AppendLine("            {");
                sb.AppendLine("                CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, \"Caught error in UploadData handler\", id.ToString(), ex);");
                sb.AppendLine();
                sb.AppendLine("                return Problem(ex.Message);");
                sb.AppendLine("            }");
                sb.AppendLine();
                sb.AppendLine("            //");
                sb.AppendLine("            // Treat the situation where we have a valid ID but no file content as a request to clear the data");
                sb.AppendLine("            //");
                sb.AppendLine("            if (foundFileData == false)");
                sb.AppendLine("            {");


                if (versionControlEnabled == false)
                {
                    sb.AppendLine("                    try");
                    sb.AppendLine("                    {");
                    sb.AppendLine("                        if (diskBasedBinaryStorageMode == true)");
                    sb.AppendLine("                        {");
                    sb.AppendLine("						       DeleteDataFromDisk(" + camelCaseName + ".objectGuid, 0, \"data\");");
                    sb.AppendLine("                        }");
                    sb.AppendLine();
                    sb.AppendLine("                        " + camelCaseName + "." + dataRootFieldName + "FileName = null;");
                    sb.AppendLine("                        " + camelCaseName + "." + dataRootFieldName + "MimeType = null;");
                    sb.AppendLine("                        " + camelCaseName + "." + dataRootFieldName + "Data = null;");
                    sb.AppendLine("                        " + camelCaseName + "." + dataRootFieldName + "Size = 0;");
                    sb.AppendLine();
                    sb.AppendLine("                        await _context.SaveChangesAsync(cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine("                        CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, \"" + entityName + " data cleared.\", id.ToString());");
                    sb.AppendLine("                    }");
                    sb.AppendLine("                    catch (Exception ex)");
                    sb.AppendLine("                    {");
                    sb.AppendLine("                        CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, \"" + entityName + " data clear failed.\", false, id.ToString(), \"\", \"\", ex);");
                    sb.AppendLine();
                    sb.AppendLine("                        return Problem(ex.Message);");
                    sb.AppendLine("                    }");
                }
                else
                {
                    sb.AppendLine("                lock (" + camelCaseName + "PutSyncRoot)");
                    sb.AppendLine("                {");
                    sb.AppendLine("                    try");
                    sb.AppendLine("                    {");
                    sb.AppendLine("                        using (IDbContextTransaction transaction = _context.Database.BeginTransaction())");
                    sb.AppendLine("                        {");
                    sb.AppendLine("                            if (diskBasedBinaryStorageMode == true)");
                    sb.AppendLine("                            {");
                    sb.AppendLine("								DeleteDataFromDisk(" + camelCaseName + ".objectGuid, " + camelCaseName + ".versionNumber, \"data\");");
                    sb.AppendLine("                            }");
                    sb.AppendLine();
                    sb.AppendLine("                            " + camelCaseName + "." + dataRootFieldName + "FileName = null;");
                    sb.AppendLine("                            " + camelCaseName + "." + dataRootFieldName + "MimeType = null;");
                    sb.AppendLine("                            " + camelCaseName + "." + dataRootFieldName + "Size = 0;");
                    sb.AppendLine("                            " + camelCaseName + "." + dataRootFieldName + "Data = null;");
                    sb.AppendLine("                            " + camelCaseName + ".versionNumber++;");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("                            //");
                    sb.AppendLine("                            // Now add the change history");
                    sb.AppendLine("                            //");

                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine("                            User user = GetUser(" + camelCaseName + ".tenantGuid, securityUser);");
                        sb.AppendLine("                            " + entityName + "ChangeHistory " + camelCaseName + "ChangeHistory = new " + entityName + "ChangeHistory();");
                    
                        // Note we are using the reserved word version of camel case here to suit EFPT's naming convention.  this may need to be tweaked later if that changes..
                        sb.AppendLine("                            " + camelCaseName + "ChangeHistory." + CamelCase(entityName, true) + "Id = " + camelCaseName + ".id;");

                        sb.AppendLine("                            " + camelCaseName + "ChangeHistory.versionNumber = " + camelCaseName + ".versionNumber;");
                        sb.AppendLine("                            " + camelCaseName + "ChangeHistory.timeStamp = DateTime.UtcNow;");
                        sb.AppendLine("                            " + camelCaseName + "ChangeHistory.userId = user.id;");
                        if (multiTenancyEnabled == true)
                        {
                            sb.AppendLine("                                    " + camelCaseName + "ChangeHistory.tenantGuid = " + camelCaseName + ".tenantGuid;");
                        }
                        sb.AppendLine($"                                    {camelCaseName}ChangeHistory.data = JsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName}));");
                    }
                    else
                    {
                        sb.AppendLine("                            " + entityName + "ChangeHistory " + camelCaseName + "ChangeHistory = new " + entityName + "ChangeHistory();");
                        sb.AppendLine("                            " + camelCaseName + "ChangeHistory." + CamelCase(entityName, false) + "Id = " + camelCaseName + ".id;");
                        sb.AppendLine("                            " + camelCaseName + "ChangeHistory.versionNumber = " + camelCaseName + ".versionNumber;");
                        sb.AppendLine("                            " + camelCaseName + "ChangeHistory.timeStamp = DateTime.UtcNow;");
                        sb.AppendLine("                            " + camelCaseName + "ChangeHistory.userId = securityUser.id;");
                        if (multiTenancyEnabled == true)
                        {
                            sb.AppendLine("                                    " + camelCaseName + "ChangeHistory.tenantGuid = " + camelCaseName + ".tenantGuid;");
                        }
                        sb.AppendLine($"                                    {camelCaseName}ChangeHistory.data = JsonSerializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({camelCaseName}));");
                    }


                    sb.AppendLine("                            _context." + entityName + "ChangeHistories.Add(" + camelCaseName + "ChangeHistory);");
                    sb.AppendLine();
                    sb.AppendLine("                            _context.SaveChanges();");
                    sb.AppendLine();
                    sb.AppendLine("                            transaction.Commit();");
                    sb.AppendLine();
                    sb.AppendLine("                            CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, \"" + entityName + " data cleared.\", id.ToString());");
                    sb.AppendLine("                        }");
                    sb.AppendLine("                    }");
                    sb.AppendLine("                    catch (Exception ex)");
                    sb.AppendLine("                    {");
                    sb.AppendLine("                        CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, \"" + entityName + " data clear failed.\", false, id.ToString(), \"\", \"\", ex);");
                    sb.AppendLine();
                    sb.AppendLine("                        return Problem(ex.Message);");
                    sb.AppendLine("                    }");
                    sb.AppendLine("                }");
                }

                sb.AppendLine("            }");
                sb.AppendLine();
                sb.AppendLine("            return Ok();");
                sb.AppendLine("        }");
                sb.AppendLine();

                #endregion

                #region Direct_Download_Handling

                if (string.IsNullOrEmpty(scriptGenTable.pngRootFieldName) == false)
                {
                    sb.AppendLine();
                    sb.AppendLine("        [HttpGet]");
                    sb.AppendLine("        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]");
                    sb.AppendLine("        [Route(\"api/" + singularForRouting + "/Data/{id:int}\")]");
                    sb.AppendLine("        public async Task<IActionResult> PNGDownloadAsync(int id, int? width = null, int? height = null)");
                    sb.AppendLine("        {");
                    sb.AppendLine();

                    GenerateReadRoleAndPermissionChecks(module, scriptGenTable, sb);

                    sb.AppendLine();
                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine("            SecurityUser securityUser = await GetSecurityUserAsync();");
                        sb.AppendLine();
                        sb.AppendLine("            try");
                        sb.AppendLine("            {");
                        sb.AppendLine("                await ThrowErrorIfUserCannotReadFromIdAsync(securityUser, id);");
                        sb.AppendLine("            }");
                        sb.AppendLine("            catch(Exception ex)");
                        sb.AppendLine("            {");
                        sb.AppendLine("               return Problem(\"Insufficient read access.\");");
                        sb.AppendLine("            }");
                        sb.AppendLine();
                    }
                    sb.AppendLine();

                    if (contextClassName == null)
                    {
                        sb.AppendLine("			using (" + module + "Entities context = new " + module + "Entities())");
                    }
                    else
                    {
                        sb.AppendLine("			using (" + contextClassName + " context = new " + contextClassName + "())");
                    }

                    sb.AppendLine("            {");
                    sb.AppendLine("                //");
                    sb.AppendLine("                // Return the PNG to the user as though it was a file.");
                    sb.AppendLine("                //");
                    sb.AppendLine("                " + qualifiedEntity + " " + camelCaseName + " = await (from d in context." + Pluralize(entityName) + "");
                    sb.AppendLine("                                    where d.id == id &&");
                    sb.AppendLine("                                    d.active == true &&");
                    sb.AppendLine("                                    d.deleted == false");
                    sb.AppendLine("                                    select d).FirstOrDefaultAsync();");


                    sb.AppendLine();
                    sb.AppendLine("                bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();");
                    sb.AppendLine();
                    sb.AppendLine("                if (diskBasedBinaryStorageMode == true &&");
                    sb.AppendLine("                	" + camelCaseName + "." + dataRootFieldName + "Data == null &&");


                    if (BinaryStorageFieldsAreNullable(scriptGenTable) == true)
                    {
                        sb.AppendLine("                	" + camelCaseName + "." + dataRootFieldName + "Size.HasValue == true &&");
                        sb.AppendLine("                	" + camelCaseName + "." + dataRootFieldName + "Size.Value > 0)");
                    }
                    else
                    {
                        sb.AppendLine("                	" + camelCaseName + "." + dataRootFieldName + "Size > 0)");
                    }

                    sb.AppendLine("                {");


                    if (versionControlEnabled == true)
                    {
                        sb.AppendLine("                	" + camelCaseName + "." + dataRootFieldName + "Data = await LoadDataFromDiskAsync(" + camelCaseName + ".objectGuid, " + camelCaseName + ".versionNumber, \"" + dataFileNameExtension + "\");");
                    }
                    else
                    {
                        sb.AppendLine("                	" + camelCaseName + "." + dataRootFieldName + "Data = await LoadDataFromDiskAsync(" + camelCaseName + ".objectGuid, 0, \"" + dataFileNameExtension + "\");");
                    }

                    sb.AppendLine("                }");

                    sb.AppendLine();
                    sb.AppendLine("                if (" + camelCaseName + " != null && " + camelCaseName + "." + dataRootFieldName + "Data != null)");
                    sb.AppendLine("                {");
                    sb.AppendLine("                    if (width.HasValue == true && height.HasValue == true)");
                    sb.AppendLine("                    {");
                    sb.AppendLine("                        //");
                    sb.AppendLine("                        // Resize the image data to the user provided width and height");
                    sb.AppendLine("                        //");
                    sb.AppendLine("                        Image img = Image.FromStream(new MemoryStream(" + camelCaseName + "." + dataRootFieldName + "Data));");
                    sb.AppendLine();
                    sb.AppendLine("                        Bitmap bmp = Foundation.ImagingHelper.ResizeImage(img, width.Value, height.Value);");
                    sb.AppendLine();
                    sb.AppendLine("                        if (bmp != null)");
                    sb.AppendLine("                        {");
                    sb.AppendLine("                            byte[] resizedImageData = Foundation.ImageUtility.ConvertImageToByteArray(bmp);");
                    sb.AppendLine();
                    sb.AppendLine("                            return File(resizedImageData, \"image/png\", " + camelCaseName + "." + dataRootFieldName + "FileName != null ? " + camelCaseName + "." + dataRootFieldName + "FileName.Trim() : \"" + entityName + "_\" + " + camelCaseName + ".id.ToString(), true);");
                    sb.AppendLine("                        }");
                    sb.AppendLine("                        else");
                    sb.AppendLine("                        {");
                    sb.AppendLine("                            return Problem(\"Unable to resize image\");");
                    sb.AppendLine("                        }");
                    sb.AppendLine("                    }");
                    sb.AppendLine("                    else");
                    sb.AppendLine("                    {");
                    sb.AppendLine("                        //");
                    sb.AppendLine("                        // No resizing.  Just send the data back as we have it it filed.");
                    sb.AppendLine("                        //");

                    sb.AppendLine("                        return File(" + camelCaseName + "." + dataRootFieldName + "Data.ToArray<byte>(), \"image/png\", " + camelCaseName + "." + dataRootFieldName + "FileName != null ? " + camelCaseName + "." + dataRootFieldName + "FileName.Trim() : \"" + entityName + "_\" + " + camelCaseName + ".id.ToString(), true);");

                    sb.AppendLine("                    }");
                    sb.AppendLine("                }");
                    sb.AppendLine("                else");
                    sb.AppendLine("                {");
                    sb.AppendLine("                    return BadRequest();");
                    sb.AppendLine("                }");
                    sb.AppendLine("            }");
                    sb.AppendLine("        }");
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine("        [HttpGet]");
                    sb.AppendLine("        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]");
                    sb.AppendLine("        [Route(\"api/" + singularForRouting + "/Data/{id:int}\")]");
                    sb.AppendLine("        public async Task<IActionResult> DownloadDataAsync(int id)");
                    sb.AppendLine("        {");
                    sb.AppendLine();

                    GenerateReadRoleAndPermissionChecks(module, scriptGenTable, sb);

                    sb.AppendLine();
                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine("            SecurityUser securityUser = await GetSecurityUserAsync();");
                        sb.AppendLine();
                        sb.AppendLine("            try");
                        sb.AppendLine("            {");
                        sb.AppendLine("                await ThrowErrorIfUserCannotReadFromIdAsync(securityUser, id);");
                        sb.AppendLine("            }");
                        sb.AppendLine("            catch(Exception ex)");
                        sb.AppendLine("            {");
                        sb.AppendLine("               return Problem(\"Insufficient read access.\");");
                        sb.AppendLine("            }");
                        sb.AppendLine();
                    }
                    sb.AppendLine();

                    if (contextClassName == null)
                    {
                        sb.AppendLine("			using (" + module + "Entities context = new " + module + "Entities())");
                    }
                    else
                    {
                        sb.AppendLine("			using (" + contextClassName + " context = new " + contextClassName + "())");
                    }

                    sb.AppendLine("            {");
                    sb.AppendLine("                //");
                    sb.AppendLine("                // Return the data to the user as though it was a file.");
                    sb.AppendLine("                //");
                    sb.AppendLine("                " + qualifiedEntity + " " + camelCaseName + " = await (from d in context." + Pluralize(entityName) + "");
                    sb.AppendLine("                                                where d.id == id &&");
                    sb.AppendLine("                                                d.active == true &&");
                    sb.AppendLine("                                                d.deleted == false");
                    sb.AppendLine("                                                select d).FirstOrDefaultAsync();");
                    sb.AppendLine();
                    sb.AppendLine("                if (" + camelCaseName + " != null && " + camelCaseName + "." + dataRootFieldName + "Data != null)");
                    sb.AppendLine("                {");
                    sb.AppendLine("                   return File(" + camelCaseName + "." + dataRootFieldName + "Data.ToArray<byte>(), " + camelCaseName + "." + dataRootFieldName + "MimeType, " + camelCaseName + "." + dataRootFieldName + "FileName != null ? " + camelCaseName + "." + dataRootFieldName + "FileName.Trim() : \"" + entityName + "_\" + " + camelCaseName + ".id.ToString(), true);");
                    sb.AppendLine("                }");
                    sb.AppendLine("                else");
                    sb.AppendLine("                {");
                    sb.AppendLine("                    return BadRequest();");  //ResponseMessage(new HttpResponseMessage(HttpStatusCode.BadRequest));");
                    sb.AppendLine("                }");
                    sb.AppendLine("            }");
                    sb.AppendLine("        }");
                }
            }

            #endregion

            sb.AppendLine("\t}");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static void GenerateReadRoleAndPermissionChecks(string module, DatabaseGenerator.Database.Table scriptGenTable, StringBuilder sb)
        {
            if (string.IsNullOrEmpty(scriptGenTable.customReadAccessRole) == false)
            {
                sb.AppendLine("\t\t\t//");
                sb.AppendLine($"\t\t\t// {scriptGenTable.customReadAccessRole} role needed to read from this table, or {module} Administrator role.  Note we do not check the user's read permission level here.  Role membership is the key to read access.");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine($"\t\t\tif (await DoesUserHaveCustomRoleSecurityCheckAsync(\"{scriptGenTable.customWriteAccessRole}\", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t   return Forbid();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("\t\t\t//");
                sb.AppendLine($"\t\t\t// {module} Reader role or better needed to read from this table, as well as the minimum read permission level.");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\tif (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t   return Forbid();");       // 403 is used here to indicate to the client that access to this resource is forbidden, as opposed to a 401 which indicates that there is no authorization.
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();
            }
        }

        private static void GenerateWriteRoleAndPermissionChecks(string module, DatabaseGenerator.Database.Table scriptGenTable, StringBuilder sb, bool adminAccessNeededToWrite)
        {
            if (adminAccessNeededToWrite == true)
            {
                sb.AppendLine("\t\t\t//");
                sb.AppendLine($"\t\t\t// {module} Administrator role needed to write to this table.");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\tif (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t   return Forbid();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();
            }
            else if (string.IsNullOrEmpty(scriptGenTable.customWriteAccessRole) == false)
            {
                sb.AppendLine("\t\t\t//");
                sb.AppendLine($"\t\t\t// {scriptGenTable.customWriteAccessRole} role needed to write to this table, or {module} Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine($"\t\t\tif (await DoesUserHaveCustomRoleSecurityCheckAsync(\"{scriptGenTable.customWriteAccessRole}\", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t   return Forbid();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("\t\t\t//");
                sb.AppendLine($"\t\t\t// {module} Writer role needed to write to this table, as well as the minimum write permission level.");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\tif (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t   return Forbid();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();
            }
        }

        private static void AddAnyStringsContainsQueryAdditions(Type type, StringBuilder sb, string rootNamespace, bool writeCodeToConditionallyIncludeRelations)
        {
            string entityName = type.Name;
            string titleName = StringUtility.ConvertToHeader(type.Name);

            //
            // Add the any string contains parameter
            //
            sb.AppendLine();
            sb.AppendLine("\t\t\t//");
            sb.AppendLine($"\t\t\t// Add the any string contains parameter to span all the string fields on the {titleName}, or on an any of the string fields on its immediate relations");
            sb.AppendLine("\t\t\t//");
            sb.AppendLine("\t\t\t// Note that this will be a time intensive parameter to apply, so use it with that understanding.");
            sb.AppendLine("\t\t\t//");
            sb.AppendLine("\t\t\tif (!string.IsNullOrEmpty(anyStringContains))");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t   query = query.Where(x =>");

            bool anyContainsConditionWritten = false;
            foreach (PropertyInfo prop in type.GetProperties())
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType == typeof(string))
                {
                    sb.Append($"\t\t\t       ");

                    if (anyContainsConditionWritten == true)
                    {
                        // add or condition to start of line because it's simpler than detecting the last string.
                        sb.Append("|| ");
                    }

                    sb.AppendLine($"x.{prop.Name}.Contains(anyStringContains)");

                    anyContainsConditionWritten = true;
                }

                else if (propertyType.FullName.StartsWith(rootNamespace))       // Is this is a sub object in our root namespace?
                {
                    foreach (PropertyInfo subProp in propertyType.GetProperties())
                    {
                        // Check for [NotMapped] attribute
                        if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                        {
                            continue; // Skip properties with [NotMapped]
                        }

                        Type subPropertyType = Nullable.GetUnderlyingType(subProp.PropertyType) ?? subProp.PropertyType;

                        if (subPropertyType == typeof(string))
                        {
                            sb.Append($"\t\t\t       ");

                            if (anyContainsConditionWritten == true)
                            {
                                // add or condition to start of line because it's simpler than detecting the last string.
                                sb.Append("|| ");
                            }

                            if (writeCodeToConditionallyIncludeRelations == true)
                            {
                                sb.AppendLine($"(includeRelations == true && x.{prop.Name}.{subProp.Name}.Contains(anyStringContains))");
                            }
                            else
                            {
                                sb.AppendLine($"x.{prop.Name}.{subProp.Name}.Contains(anyStringContains)");
                            }

                            anyContainsConditionWritten = true;
                        }
                    }
                }
            }

            sb.AppendLine("\t\t\t   );");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();
        }

        private static bool BinaryStorageFieldsAreNullable(DatabaseGenerator.Database.Table scriptGenTable)
        {
            string fieldName = null;

            if (string.IsNullOrEmpty(scriptGenTable.pngRootFieldName) == false)
            {
                fieldName = scriptGenTable.pngRootFieldName + "Size";
            }
            else if (string.IsNullOrEmpty(scriptGenTable.pdfRootFieldName) == false)
            {
                fieldName = scriptGenTable.pdfRootFieldName + "Size";
            }
            else if (string.IsNullOrEmpty(scriptGenTable.mp4RootFieldName) == false)
            {
                fieldName = scriptGenTable.mp4RootFieldName + "Size";
            }
            else if (string.IsNullOrEmpty(scriptGenTable.binaryDataRootFieldName) == false)
            {
                fieldName = scriptGenTable.binaryDataRootFieldName + "Size";
            }
            else
            {
                throw new Exception("unable to find binary storage fields.");
            }


            foreach (var field in scriptGenTable.fields)
            {
                if (field.name == fieldName)
                {
                    return field.nullable;
                }
            }
            throw new Exception("Could not find field named " + fieldName);
        }

        private static bool HasBinaryStorageFields(DatabaseGenerator.Database.Table scriptGenTable)
        {
            if (scriptGenTable != null && (string.IsNullOrEmpty(scriptGenTable.pngRootFieldName) == false ||
                string.IsNullOrEmpty(scriptGenTable.pdfRootFieldName) == false ||
                string.IsNullOrEmpty(scriptGenTable.mp4RootFieldName) == false ||
                string.IsNullOrEmpty(scriptGenTable.binaryDataRootFieldName) == false))
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        private static string UserTenantGuidCommands(int numberOfTabs = 3, bool async = true, bool throwInsteadOfProblem = false)
        {
            StringBuilder sb = new StringBuilder();

            string tabPrefix = "";

            for (int i = 0; i < numberOfTabs; i++)
            {
                tabPrefix += "\t";
            }

            if (async == true)
            {
                sb.AppendLine(tabPrefix + "Guid userTenantGuid;");
                sb.AppendLine();
                sb.AppendLine(tabPrefix + "try");
                sb.AppendLine(tabPrefix + "{");
                sb.AppendLine(tabPrefix + "    userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);");
                sb.AppendLine(tabPrefix + "}");
                sb.AppendLine(tabPrefix + "catch (Exception ex)");
                sb.AppendLine(tabPrefix + "{");
                if (async == true)
                {
                    sb.AppendLine(tabPrefix + "    await CreateAuditEventAsync(AuditEngine.AuditType.Error, \"Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is \" + securityUser?.accountName, securityUser?.accountName, ex);");
                }
                else
                {
                    sb.AppendLine(tabPrefix + "    CreateAuditEvent(AuditEngine.AuditType.Error, \"Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is \" + securityUser?.accountName, securityUser?.accountName, ex);");
                }

                if (throwInsteadOfProblem == true)
                {
                    sb.AppendLine(tabPrefix + "    throw new Exception(\"Your user account is not configured with a tenant, so this operation is not allowed.\");");
                }
                else
                {
                    sb.AppendLine(tabPrefix + "    return Problem(\"Your user account is not configured with a tenant, so this operation is not allowed.\");");
                }

                sb.AppendLine(tabPrefix + "}");
            }
            else
            {
                sb.AppendLine(tabPrefix + "Guid userTenantGuid;");
                sb.AppendLine();
                sb.AppendLine(tabPrefix + "try");
                sb.AppendLine(tabPrefix + "{");
                sb.AppendLine(tabPrefix + "    userTenantGuid = UserTenantGuid(securityUser);");
                sb.AppendLine(tabPrefix + "}");
                sb.AppendLine(tabPrefix + "catch (Exception ex)");
                sb.AppendLine(tabPrefix + "{");
                sb.AppendLine(tabPrefix + "    CreateAuditEvent(AuditEngine.AuditType.Error, \"Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is \" + securityUser?.accountName, securityUser?.accountName, ex);");

                if (throwInsteadOfProblem == true)
                {
                    sb.AppendLine(tabPrefix + "    throw new Exception(\"Your user account is not configured with a tenant, so this operation is not allowed.\");");
                }
                else
                {
                    sb.AppendLine(tabPrefix + "    return Problem(\"Your user account is not configured with a tenant, so this operation is not allowed.\");");
                }

                sb.AppendLine(tabPrefix + "}");
            }

            return sb.ToString();
        }


        public static void BuildWebAPIImplementationFromEntityFrameworkContext(string contextName, string moduleName, Type contextType, DatabaseGenerator.Database database, string filePath = "c:\\temp", string databaseObjectNamespace = "Database", bool ignoreFoundationServices = false, string rootNameSpace = "Foundation")
        {
            if (filePath.EndsWith("\\") == false)
            {
                filePath = filePath + "\\";
            }

            System.IO.Directory.CreateDirectory(filePath + moduleName);
            System.IO.Directory.CreateDirectory(filePath + moduleName + "\\DataControllers");
            System.IO.Directory.CreateDirectory(filePath + moduleName + "\\DataControllers\\WebAPI");



            List<string> controllerNames = new List<string>();

            if (database != null && database.dataVisibilityEnabled == true)
            {
                if (ignoreFoundationServices == true)
                {
                    throw new Exception("Can't use bare bones mode when data visibility is enabled");
                }

                //
                // First, create the base class
                //
                string baseClassWebAPICode = Foundation.CodeGeneration.WebAPICodeGeneratorCore.BuildWebAPIBaseClassImplementationFromEntityFrameworkContext(moduleName, 1000, contextType, database);


                System.IO.File.WriteAllText(filePath + moduleName + "\\DataControllers\\WebAPI\\" + moduleName + "BaseWebAPIController.cs", baseClassWebAPICode);

                foreach (PropertyInfo prop in contextType.GetProperties())
                {
                    Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    if (propertyType.GenericTypeArguments != null && propertyType.GenericTypeArguments.Length > 0)
                    {
                        string entityName = propertyType.GenericTypeArguments[0].Name;

                        string WebAPICode = "";

                        Type type = propertyType.GenericTypeArguments[0];

                        // Get the table spec from the script generator
                        DatabaseGenerator.Database.Table scriptGenTable = null;
                        foreach (DatabaseGenerator.Database.Table tbl in database.tables)
                        {
                            if (tbl.name == type.Name)
                            {
                                scriptGenTable = tbl;
                                break;
                            }
                        }

                        //
                        // If table name ends with 'Statu' or 'Campu' then look again with an s on the end.
                        //
                        if (scriptGenTable == null && (type.Name.EndsWith("Statu") || type.Name.EndsWith("Campu")))
                        {
                            var realName = type.Name + "s";

                            foreach (DatabaseGenerator.Database.Table tbl in database.tables)
                            {
                                if (tbl.name == realName)
                                {
                                    scriptGenTable = tbl;
                                    break;
                                }
                            }
                        }


                        // bool hasNameOrDescription = false;

                        var properties = type.GetProperties();


                        if (scriptGenTable != null && scriptGenTable.excludeFromCodeGeneration == false)
                        {
                            // Use the provided display name field
                            WebAPICode = BuildDefaultWebAPIImplementation(contextName, moduleName, type, scriptGenTable, databaseObjectNamespace, false, rootNameSpace);

                            string plural = prop.Name;

                            System.IO.File.WriteAllText(filePath + moduleName + "\\DataControllers\\WebAPI\\" + plural + "Controller.cs", WebAPICode);

                            // Put the controller name in the list of controllers
                            controllerNames.Add(plural + "Controller");
                        }
                    }
                }
            }
            else
            {
                foreach (PropertyInfo prop in contextType.GetProperties())
                {
                    //System.Diagnostics.Debug.WriteLine("Property is " + prop.Name);

                    Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    if (propertyType.GenericTypeArguments != null && propertyType.GenericTypeArguments.Length > 0)
                    {
                        string entityName = propertyType.GenericTypeArguments[0].Name;


                        string WebAPICode = "";

                        Type type = propertyType.GenericTypeArguments[0];

                        // Get the table spec from the script generator
                        DatabaseGenerator.Database.Table scriptGenTable = null;
                        if (database != null)
                        {
                            foreach (DatabaseGenerator.Database.Table tbl in database.tables)
                            {
                                if (tbl.name == type.Name)
                                {
                                    scriptGenTable = tbl;
                                    break;
                                }
                            }

                            //
                            // If table name ends with 'Statu' or 'Campu' then look again with an s on the end.  This is for the EF6 Pluralizer
                            //
                            if (scriptGenTable == null && (type.Name.EndsWith("Statu") || type.Name.EndsWith("Campu")))
                            {
                                var realName = type.Name + "s";

                                foreach (DatabaseGenerator.Database.Table tbl in database.tables)
                                {
                                    if (tbl.name == realName)
                                    {
                                        scriptGenTable = tbl;
                                        break;
                                    }
                                }
                            }

                            //
                            // Datum is the plural for Date in the new EFCorePowerTools pluralizer.
                            //
                            if (scriptGenTable == null && type.Name.EndsWith("Datum"))
                            {
                                string realName = type.Name.Replace("Datum", "Data");

                                foreach (DatabaseGenerator.Database.Table tbl in database.tables)
                                {
                                    if (tbl.name == realName)
                                    {
                                        scriptGenTable = tbl;
                                        break;
                                    }
                                }
                            }
                        }

                        // Use the provided display name field
                        WebAPICode = BuildDefaultWebAPIImplementation(contextName, moduleName, type, scriptGenTable, databaseObjectNamespace, ignoreFoundationServices, rootNameSpace);


                        string plural = prop.Name;

                        System.IO.File.WriteAllText(filePath + moduleName + "\\DataControllers\\WebAPI\\" + plural + "Controller.cs", WebAPICode);

                        // Put the controller name in the list of controllers
                        controllerNames.Add(plural + "Controller");
                    }
                }
            }


            //
            // Create the controller name helper file to simplify the task of setting up particular controllers for Kestrel to serve
            //
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("//");
            sb.AppendLine("// Use this data to paste into the Kestrel startup program to configure the input for the .UseSpecificControllers service");
            sb.AppendLine("//");

            sb.AppendLine("// use the extensions from Damian Hickey to reference needed controllers only");
            sb.AppendLine("List<Type> controllers = new List<Type>();");
            sb.AppendLine();

            sb.AppendLine("//");
            sb.AppendLine($"// Start of code generated controller list for {moduleName} module");
            sb.AppendLine("//");
            foreach (string controllerName in controllerNames)
            {
                sb.AppendLine($"controllers.Add(typeof({controllerName}));");
            }
            sb.AppendLine("//");
            sb.AppendLine($"// End of code generated controller list for {moduleName} module");
            sb.AppendLine("//");

            System.IO.File.WriteAllText(filePath + moduleName + "\\DataControllers\\ControllerFilteringHelper.txt", sb.ToString());

            return;
        }

        private static string pluralizeEntityForRouteForSomeTypeNames(string typeName)
        {
            if (typeName.EndsWith("Statu") || typeName.EndsWith("Campu"))
            {
                typeName += "s";
            }

            return typeName;
        }
    }
}