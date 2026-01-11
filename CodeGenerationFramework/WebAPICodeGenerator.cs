using System;
using System.Linq;
using System.Reflection;
using System.Text;
using static Foundation.StringUtility;


namespace Foundation.CodeGeneration
{
    /*
     * 
     * 
Note that Foundation DB Context classes for Contexts used by foundation need this in their constructors.

    This needs to be re-added each time the context is recreated with the Data tools.


    // Disable the database initialization, so no attempt to create databases will be made
    System.Data.Entity.Database.SetInitializer<CONTENTNAMEContext>(null);
 
    // Prevent automatic migrations.
    this.Configuration.AutoDetectChangesEnabled = false;
    this.Configuration.ProxyCreationEnabled = false;
 
    // Don't lazy load
    this.Configuration.LazyLoadingEnabled = false;
     * 
     */
    public partial class WebAPICodeGenerator : CodeGenerationBase
    {
        //
        // Use this to create default implementation of a WebAPI controller for DotNet 4.8
        //
        protected static string BuildDefaultWebAPIImplementation(string contextClassName, string module, Type type, DatabaseGenerator.Database.Table scriptGenTable, string databaseNamespace = "Database", bool ignoreFoundationServices = false)
        {
            StringBuilder sb = new StringBuilder();

            string entity = type.Name;
            string qualifiedEntity = databaseNamespace + "." + entity;

            bool tableIsWritable = true;
            bool multiTenancyEnabled = false;
            bool dataVisibilityEnabled = false;
            bool versionControlEnabled = false;
            bool canBeFavourited = false;
            bool adminAccessNeededToWrite = false;


            string displayNameField = "id.ToString()";          // Every entity will have this.

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
                    displayNameField = scriptGenTable.displayNameFieldList[0].name;
                }
                else
                {
                    var firstString = scriptGenTable.GetFirstStringField();

                    if (firstString != null)
                    {
                        displayNameField = firstString.name;
                    }
                }
            }


            string acronym = GetAcronym(entity);
            string plural = Pluralize(entity);

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

            if (ignoreFoundationServices == false)
            {
                sb.AppendLine("using Foundation.Security;");
                sb.AppendLine("using Foundation.Auditor;");
            }

            sb.AppendLine("using Foundation." + module + "." + databaseNamespace + ";");

            if (ignoreFoundationServices == false)
            {
                if (module != "Security")
                {
                    sb.AppendLine("using Foundation.Security.Database;");
                }
            }

            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Data.Entity;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Web.Http;");
            sb.AppendLine("using System.Web.Script.Serialization;");
            sb.AppendLine("using Hangfire;");
            sb.AppendLine("using Newtonsoft.Json.Linq;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using System.Net.Http;");
            sb.AppendLine("using System.Drawing;");
            sb.AppendLine("using System.IO;");
            sb.AppendLine("using System.Net;");
            sb.AppendLine("using Foundation.DatabaseUtility;        // Needed for extensions for Context class to help get parity between Core and Framework");


            sb.AppendLine("");
            sb.AppendLine("namespace Foundation." + module + ".Controllers.WebAPI");
            sb.AppendLine("{");

            if (ignoreFoundationServices == false)
            {
                if (dataVisibilityEnabled == true)
                {
                    sb.AppendLine("\tpublic partial class " + Pluralize(entity) + "Controller : " + module + "BaseWebAPIController");
                }
                else
                {
                    sb.AppendLine("\tpublic partial class " + Pluralize(entity) + "Controller : SecureWebAPIController");
                }
            }
            else
            {
                sb.AppendLine("\tpublic partial class " + Pluralize(entity) + "Controller : ApiController");

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
                sb.AppendLine("\t\tstatic object " + CamelCase(entity) + "PutSyncRoot = new object();");
                sb.AppendLine("\t\tstatic object " + CamelCase(entity) + "DeleteSyncRoot = new object();");
                sb.AppendLine();
            }

            if (ignoreFoundationServices == false)
            {
                sb.AppendLine("\t\tpublic " + Pluralize(entity) + "Controller() : base(\"" + module + "\", \"" + entity + "\")");
            }
            else
            {
                sb.AppendLine("\t\tpublic " + Pluralize(entity) + "Controller()");
            }

            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tdb.Database.SetCommandTimeout(" + commandTimeoutSeconds.ToString() + ");");
            sb.AppendLine();
            sb.AppendLine("\t\t\treturn;");
            sb.AppendLine("\t\t}");
            sb.AppendLine();

            if (dataVisibilityEnabled == false)
            {
                //
                // Define the database object here.  Tables with data visibility on derive from a different base class that has the db object defined, so those don't need it.
                //
                if (contextClassName == null)
                {
                    sb.AppendLine("\t\tprivate " + module + "Entities db = new " + module + "Entities();");
                }
                else
                {
                    sb.AppendLine("\t\tprivate " + contextClassName + " db = new " + contextClassName + "();");
                }

                sb.AppendLine("\t\tprivate const int DEFAULT_ALL_DATA_LIST_PAGE_SIZE = 0;                   // Default page size to 0 so all data is returned unless explicit paging is requested.");
                sb.AppendLine("\t\tprivate const int DEFAULT_NAME_VALUE_PAIR_LIST_PAGE_SIZE = 0;            // Default page size to 0 so all data is returned unless explicit paging is requested.");
                sb.AppendLine();
            }

            if (scriptGenTable != null && scriptGenTable.webAPIListGetterToBeOverridden == true)
            {
                // comment out this function, but write the code anyway.
                sb.AppendLine("/* This function is expected to be overridden in a custom file");
            }


            #endregion

            #region HTTP_Get_List_Handling

            sb.AppendLine("\t\t[HttpGet]");
            sb.AppendLine("\t\t[Route(\"api/" + plural + "\")]");
            sb.AppendLine("\t\tpublic async Task<IHttpActionResult> Get" + plural + "(");

            bool processingFirstProperty = true;
            foreach (PropertyInfo prop in type.GetProperties())
            {
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

                    // bool comes in as int - 1 or 0
                    sb.Append("\t\t\tint? " + prop.Name + " = null");
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
            sb.AppendLine("\t\t\tint pageSize = DEFAULT_ALL_DATA_LIST_PAGE_SIZE,");
            sb.AppendLine("\t\t\tint pageNumber = 1,");

            // If we have image fields, then add a new parameters to provide a default image width for reducing the size of images in the list gets.
            if (hasImageFields == false)
            {
                sb.AppendLine("\t\t\tbool includeRelations = true)");
            }
            else
            {
                sb.AppendLine("\t\t\tbool includeRelations = true,");
                sb.AppendLine("\t\t\tint imageWidth = 100)");
            }

            sb.AppendLine("\t\t{");

            if (ignoreFoundationServices == false)
            {
                sb.AppendLine("\t\t\tStartAuditEventClock();");
                sb.AppendLine();


                sb.AppendLine("\t\t\tif (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t   return Unauthorized();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();
                sb.AppendLine("\t\t\tif (await IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel.Read) == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t   return Unauthorized();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();

                sb.AppendLine();
                sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync();");
                sb.AppendLine();
                sb.AppendLine("\t\t\tbool userIsWriter = await UserCanWriteAsync(securityUser, " + entityMinimumWritePermissionString + ");");
                sb.AppendLine("\t\t\tbool userIsAdmin = await UserCanAdministerAsync(securityUser);");
                sb.AppendLine();
                if (multiTenancyEnabled == true)
                {
                    sb.AppendLine("\t\t\tbool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser);");
                    sb.AppendLine("\t\t\t");
                    sb.AppendLine(UserTenantGuidCommands(3));
                }
            }
            else
            {
                sb.AppendLine("\t\t\tbool userIsWriter = true;");
                sb.AppendLine("\t\t\tbool userIsAdmin = true;");
                sb.AppendLine();
            }

            sb.AppendLine("\t\t\tif (pageNumber < 1)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\tpageNumber = 1;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();

            sb.AppendLine("\t\t\tif (pageSize == 0)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\tpageSize = int.MaxValue;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t\telse if (pageSize < 0)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\tpageSize = DEFAULT_ALL_DATA_LIST_PAGE_SIZE;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();

            bool commentWritten = false;
            foreach (PropertyInfo prop in type.GetProperties())
            {
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
            sb.AppendLine("\t\t\tvar query = (from " + acronym + " in db." + plural + " select " + acronym + ");");

            if (ignoreFoundationServices == false)
            {
                if (dataVisibilityEnabled == true)
                {
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// Apply the constraints for the user's tenant and data visibility configuration.");
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\tquery = await AddTenantAndDataVisibilityConstraintsAsync(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin);");
                    sb.AppendLine();
                }
                else if (multiTenancyEnabled == true)
                {
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tquery = query.Where(Q => Q.tenantGuid == userTenantGuid);");
                    sb.AppendLine();
                }
            }

            processingFirstProperty = true;
            foreach (PropertyInfo prop in type.GetProperties())
            {
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
                        sb.AppendLine("\t\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".active == (active.Value == 1 ? true : false));");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine("\t\t\t");
                        sb.AppendLine("\t\t\t\tif (userIsAdmin == true)");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t\tif (deleted.HasValue == true)");
                        sb.AppendLine("\t\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".deleted == (deleted.Value == 1 ? true : false));");
                        sb.AppendLine("\t\t\t\t\t}");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine("\t\t\t\telse");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".deleted == false);");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine("\t\t\telse");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".active == true);");
                        sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".deleted == false);");
                        sb.AppendLine("\t\t\t}");

                    }
                    else if (prop.Name == "deleted")
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
                        sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + "." + prop.Name + " == (" + prop.Name + ".Value == 1 ? true : false));");
                        sb.AppendLine("\t\t\t}");
                    }
                }
            }

            sb.AppendLine();

            bool hasSequence = false;

            foreach (var property in type.GetProperties())
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

            sb.AppendLine("\t\t\tquery = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);");
            sb.AppendLine("\t\t\t");
            sb.AppendLine("\t\t\tif (includeRelations == true)");
            sb.AppendLine("\t\t\t{");

            // Add the includes if necessary
            foreach (PropertyInfo prop in type.GetProperties())
            {
                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                // Is this a linked object type, but not a list?
                if (propertyType != typeof(string) &&
                    propertyType != typeof(int) &&
                    propertyType != typeof(long) &&
                    propertyType != typeof(DateTime) &&
                    propertyType != typeof(float) &&
                    propertyType != typeof(double) &&
                    propertyType != typeof(decimal) &&
                    propertyType != typeof(bool) &&
                    propertyType != typeof(Guid) &&
                    propertyType != typeof(byte[]) &&
                    propertyType.FullName.StartsWith("System.Collections") == false)
                {

                    sb.AppendLine("\t\t\t\tquery = query.Include(\"" + prop.Name + "\");");
                }
            }
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t\tquery = query.AsNoTracking();");
            sb.AppendLine("\t\t\t");
            sb.AppendLine("\t\t\tList<" + qualifiedEntity + "> materialized = await query.ToListAsync();");


            sb.AppendLine();

            sb.AppendLine("\t\t\t");
            sb.AppendLine("\t\t\t// Convert all the date properties to be of kind UTC.");
            sb.AppendLine("\t\t\tbool databaseStoresDateWithTimeZone = DoesDatabaseStoreDateWithTimeZone(db);");
            sb.AppendLine("\t\t\tforeach (" + qualifiedEntity + " " + CamelCase(entity) + " in materialized)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(" + CamelCase(entity) + ", databaseStoresDateWithTimeZone);");
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


                    /* old serial way
                    sb.AppendLine("\t\t\t\tforeach (" + qualifiedEntity + " " + CamelCase(entity) + " in materialized)");
                    sb.AppendLine("\t\t\t\t{");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\tif (" + CamelCase(entity) + "." + dataRootFieldName + "Data == null &&");

                    if (BinaryStorageFieldsAreNullable(scriptGenTable) == true)
                    {
                        sb.AppendLine("\t\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size.HasValue == true &&");
                        sb.AppendLine("\t\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size.Value > 0)");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size > 0)");
                    }

                    sb.AppendLine("\t\t\t\t\t{");
                    if (versionControlEnabled == true)
                    {
                        sb.AppendLine("\t\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Data = await LoadDataFromDiskAsync(" + CamelCase(entity) + ".objectGuid, " + CamelCase(entity) + ".versionNumber, \"" + dataFileNameExtension + "\");");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Data = await LoadDataFromDiskAsync(" + CamelCase(entity) + ".objectGuid, 0, \"" + dataFileNameExtension + "\");");
                    }
                    sb.AppendLine("\t\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t}");
                    */

                    sb.AppendLine("\t\t\t\tvar tasks = materialized.Select(async " + CamelCase(entity) + " =>");
                    sb.AppendLine("\t\t\t\t{");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\tif (" + CamelCase(entity) + "." + dataRootFieldName + "Data == null &&");

                    if (BinaryStorageFieldsAreNullable(scriptGenTable) == true)
                    {
                        sb.AppendLine("\t\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size.HasValue == true &&");
                        sb.AppendLine("\t\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size.Value > 0)");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size > 0)");
                    }

                    sb.AppendLine("\t\t\t\t\t{");
                    if (versionControlEnabled == true)
                    {
                        sb.AppendLine("\t\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Data = await LoadDataFromDiskAsync(" + CamelCase(entity) + ".objectGuid, " + CamelCase(entity) + ".versionNumber, \"" + dataFileNameExtension + "\");");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Data = await LoadDataFromDiskAsync(" + CamelCase(entity) + ".objectGuid, 0, \"" + dataFileNameExtension + "\");");
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

                    /* old serial way
                    sb.AppendLine("\t\t\tforeach (" + qualifiedEntity + " " + CamelCase(entity) + " in materialized)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine(@" 
                Image img = Image.FromStream(new MemoryStream(" + CamelCase(entity) + @"." + dataRootFieldName + @"Data));

                Bitmap bmp = Foundation.ImagingHelper.ResizeImage(img, imageWidth);

                if (bmp != null)
                {
                    byte[] resizedImageData = Foundation.ImageUtility.ConvertImageToByteArray(bmp);

                    " + CamelCase(entity) + @"." + dataRootFieldName + @"Data = resizedImageData;
                    " + CamelCase(entity) + @"." + dataRootFieldName + @"Size = resizedImageData.Length;
                }");
                    */


                    // New Parallel way
                    sb.AppendLine("\t\t\tParallel.ForEach(materialized, " + CamelCase(entity) + " =>");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t   if (" + CamelCase(entity) + "." + dataRootFieldName + @"Data != null)");
                    sb.AppendLine("\t\t\t   {");
                    sb.AppendLine("\t\t\t       using (var memoryStream = new MemoryStream(" + CamelCase(entity) + "." + dataRootFieldName + @"Data))");
                    sb.AppendLine("\t\t\t       {");
                    sb.AppendLine("\t\t\t           Image img = Image.FromStream(memoryStream);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t           Bitmap bmp = Foundation.ImagingHelper.ResizeImage(img, imageWidth);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t           if (bmp != null)");
                    sb.AppendLine("\t\t\t           {");
                    sb.AppendLine("\t\t\t               byte[] resizedImageData = Foundation.ImageUtility.ConvertImageToByteArray(bmp);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t               // Update " + CamelCase(entity) + " fields safely");
                    sb.AppendLine("\t\t\t                " + CamelCase(entity) + "." + dataRootFieldName + @"Data = resizedImageData;");
                    sb.AppendLine("\t\t\t                " + CamelCase(entity) + "." + dataRootFieldName + @"Size = resizedImageData.Length;");
                    sb.AppendLine("\t\t\t           }");
                    sb.AppendLine("\t\t\t       }");
                    sb.AppendLine("\t\t\t    }");
                    sb.AppendLine("\t\t\t});");
                    sb.AppendLine();
                }

                if (ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\tawait CreateAuditEventAsync(Auditor.AuditEngine.AuditType.ReadList, userIsAdmin == true ? \"" + module + "." + entity + " Entity list was read with Admin privilege.  Returning \" + materialized.Count + \" rows of data.\" : \"" + module + "." + entity + " Entity list was read.  Returning \" + materialized.Count + \" rows of data.\");");
                    sb.AppendLine();
                }


                sb.AppendLine("\t\t\t// Create a new output object that only includes the relations if necessary, and doesn't include the empty list objects, so that we can reduce the amount of data being transferred.");
                sb.AppendLine("\t\t\tif (includeRelations == true)");
                sb.AppendLine("\t\t\t{");

                sb.AppendLine("\t\t\t\tvar reducedFieldOutput = (from materializedData in materialized");
                sb.AppendLine($"\t\t\t\t\t\t select {databaseNamespace}.{entity}.CreateAnonymousWithFirstLevelSubObjects(materializedData)).ToList();");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\treturn Ok(reducedFieldOutput);");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\telse");
                sb.AppendLine("\t\t\t{");

                sb.AppendLine("\t\t\t\tvar reducedFieldOutput = (from materializedData in materialized");
                sb.AppendLine($"\t\t\t\t\t\t select {databaseNamespace}.{entity}.CreateAnonymous(materializedData)).ToList();");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\treturn Ok(reducedFieldOutput);");
                sb.AppendLine("\t\t\t}");
            }
            else
            {
                sb.AppendLine("\t\t\tList<" + entity + "WithWritePermissionDetails> output = new List<" + entity + "WithWritePermissionDetails>();");
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
                sb.AppendLine("\t\t\t\torganizationsUserIsEntitledToWriteTo = await GetOrganizationIdsUserIsEntitledToWriteToAsync(securityUser);");
                sb.AppendLine("\t\t\t\tdepartmentsUserIsEntitledToWriteTo = await GetDepartmentIdsUserIsEntitledToWriteToAsync(securityUser);");
                sb.AppendLine("\t\t\t\tteamsUserIsEntitledToWriteTo = await GetTeamIdsUserIsEntitledToWriteToAsync(securityUser);");
                sb.AppendLine("\t\t\t\tuserAndTheirReportIds = await GetListOfUserIdsForUserAndPeopleThatReportToThemAsync(securityUser);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\torganizationsUserIsEntitledToChangeOwnerFor = await GetOrganizationIdsUserIsEntitledToChangeOwnerForAsync(securityUser);");
                sb.AppendLine("\t\t\t\tdepartmentsUserIsEntitledToChangeOwnerFor = await GetDepartmentIdsUserIsEntitledToChangeOwnerForAsync(securityUser);");
                sb.AppendLine("\t\t\t\tteamsUserIsEntitledToChangeOwnerFor = await GetTeamIdsUserIsEntitledToChangeOwnerForAsync(securityUser);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\torganizationsUserIsEntitledToChangeHierarchyFor = await GetOrganizationIdsUserIsEntitledToChangeHierarchyForAsync(securityUser);");
                sb.AppendLine("\t\t\t\tdepartmentsUserIsEntitledToChangeHierarchyFor = await GetDepartmentIdsUserIsEntitledToChangeHierarchyForAsync(securityUser);");
                sb.AppendLine("\t\t\t\tteamsUserIsEntitledToChangeHierarchyFor = await GetTeamIdsUserIsEntitledToChangeHierarchyForAsync(securityUser);");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();

                if (HasBinaryStorageFields(scriptGenTable) == true)
                {
                    sb.AppendLine("\t\t\tbool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();");
                    sb.AppendLine();
                }

                /* this is the old serial way.  Being replaced by a parallel task.
                sb.AppendLine("\t\t\tforeach (" + qualifiedEntity + " " + CamelCase(entity) + " in materialized)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine();

                if (HasBinaryStorageFields(scriptGenTable) == true)
                {

                    sb.AppendLine("\t\t\t\tif (diskBasedBinaryStorageMode == true &&");
                    sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Data == null &&");

                    if (BinaryStorageFieldsAreNullable(scriptGenTable) == true)
                    {
                        sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size.HasValue == true &&");
                        sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size.Value > 0)");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size > 0)");
                    }


                    sb.AppendLine("\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Data = await LoadDataFromDiskAsync(" + CamelCase(entity) + ".objectGuid, " + CamelCase(entity) + ".versionNumber, \"" + dataFileNameExtension + "\");");
                    sb.AppendLine("\t\t\t\t}");
                    sb.AppendLine();
                }


                sb.AppendLine("\t\t\t\t" + entity + "WithWritePermissionDetails " + CamelCase(entity) + "WWP = new " + entity + "WithWritePermissionDetails(" + CamelCase(entity) + ");");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\tConfigureIsWriteable(" + CamelCase(entity) + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, userAndTheirReportIds, organizationsUserIsEntitledToWriteTo, departmentsUserIsEntitledToWriteTo, teamsUserIsEntitledToWriteTo);");
                sb.AppendLine("\t\t\t\tConfigureOwnerIsChangeable(" + CamelCase(entity) + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, organizationsUserIsEntitledToChangeOwnerFor, departmentsUserIsEntitledToChangeOwnerFor, teamsUserIsEntitledToChangeOwnerFor);");
                sb.AppendLine("\t\t\t\tConfigureHierarchyIsChangeable(" + CamelCase(entity) + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, organizationsUserIsEntitledToChangeHierarchyFor, departmentsUserIsEntitledToChangeHierarchyFor, teamsUserIsEntitledToChangeHierarchyFor);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\toutput.Add(" + CamelCase(entity) + "WWP);");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();
                */

                //
                // This is the new parallel way
                //
                sb.AppendLine("\t\t\tvar tasks = materialized.Select(async " + CamelCase(entity) + " =>");
                sb.AppendLine("\t\t\t{");

                if (HasBinaryStorageFields(scriptGenTable) == true)
                {
                    sb.AppendLine("\t\t\t     if (diskBasedBinaryStorageMode == true &&");
                    sb.AppendLine("\t\t\t         " + CamelCase(entity) + "." + dataRootFieldName + @"Data == null &&");
                    sb.AppendLine("\t\t\t         " + CamelCase(entity) + "." + dataRootFieldName + @"Size.HasValue == true &&");
                    sb.AppendLine("\t\t\t         " + CamelCase(entity) + "." + dataRootFieldName + @"Size.Value > 0)");
                    sb.AppendLine("\t\t\t     {");
                    if (versionControlEnabled == true)
                    {
                        sb.AppendLine("\t\t\t         " + CamelCase(entity) + "." + dataRootFieldName + @"Data = await LoadDataFromDiskAsync(" + CamelCase(entity) + ".objectGuid, " + CamelCase(entity) + ".versionNumber, \"" + dataFileNameExtension + "\");");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t         " + CamelCase(entity) + "." + dataRootFieldName + @"Data = await LoadDataFromDiskAsync(" + CamelCase(entity) + ".objectGuid, 0, \"" + dataFileNameExtension + "\");");
                    }

                    sb.AppendLine("\t\t\t     }");
                }
                sb.AppendLine();
                sb.AppendLine("\t\t\t     " + entity + "WithWritePermissionDetails " + CamelCase(entity) + "WWP = new " + entity + "WithWritePermissionDetails(" + CamelCase(entity) + ");");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\tConfigureIsWriteable(" + CamelCase(entity) + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, userAndTheirReportIds, organizationsUserIsEntitledToWriteTo, departmentsUserIsEntitledToWriteTo, teamsUserIsEntitledToWriteTo);");
                sb.AppendLine("\t\t\t\tConfigureOwnerIsChangeable(" + CamelCase(entity) + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, organizationsUserIsEntitledToChangeOwnerFor, departmentsUserIsEntitledToChangeOwnerFor, teamsUserIsEntitledToChangeOwnerFor);");
                sb.AppendLine("\t\t\t\tConfigureHierarchyIsChangeable(" + CamelCase(entity) + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, organizationsUserIsEntitledToChangeHierarchyFor, departmentsUserIsEntitledToChangeHierarchyFor, teamsUserIsEntitledToChangeHierarchyFor);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t   return " + CamelCase(entity) + "WWP;");
                sb.AppendLine("\t\t\t }).ToList();");
                sb.AppendLine();
                sb.AppendLine("\t\t\t// Execute in parallel");
                sb.AppendLine("\t\t\tvar results = await Task.WhenAll(tasks);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t// Add results to output in parallel to prevent thread issues");
                sb.AppendLine("\t\t\tParallel.ForEach(results, " + CamelCase(entity) + "WWP =>");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t     output.Add(" + CamelCase(entity) + "WWP);");
                sb.AppendLine("\t\t\t });");




                if (hasImageFields == true)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// Resize the image data to a standard width to help to reduce the size of the data transferred in list gets for data with images.");
                    sb.AppendLine("\t\t\t//");

                    /* Old serial way
                    sb.AppendLine("\t\t\tforeach (" + qualifiedEntity + " " + CamelCase(entity) + " in materialized)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine(@" 
                Image img = Image.FromStream(new MemoryStream(" + CamelCase(entity) + @"." + dataRootFieldName + @"Data));

                Bitmap bmp = Foundation.ImagingHelper.ResizeImage(img, imageWidth);

                if (bmp != null)
                {
                    byte[] resizedImageData = Foundation.ImageUtility.ConvertImageToByteArray(bmp);

                    " + CamelCase(entity) + @"." + dataRootFieldName + @"Data = resizedImageData;
                    " + CamelCase(entity) + @"." + dataRootFieldName + @"Size = resizedImageData.Length;
                }");
                    */


                    // New Parallel way
                    sb.AppendLine("\t\t\tParallel.ForEach(output, " + CamelCase(entity) + " =>");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t   if (" + CamelCase(entity) + "." + dataRootFieldName + @"Data != null)");
                    sb.AppendLine("\t\t\t   {");
                    sb.AppendLine("\t\t\t       using (var memoryStream = new MemoryStream(" + CamelCase(entity) + "." + dataRootFieldName + @"Data))");
                    sb.AppendLine("\t\t\t       {");
                    sb.AppendLine("\t\t\t           Image img = Image.FromStream(memoryStream);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t           Bitmap bmp = Foundation.ImagingHelper.ResizeImage(img, imageWidth);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t           if (bmp != null)");
                    sb.AppendLine("\t\t\t           {");
                    sb.AppendLine("\t\t\t               byte[] resizedImageData = Foundation.ImageUtility.ConvertImageToByteArray(bmp);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t               // Update " + CamelCase(entity) + " fields safely");
                    sb.AppendLine("\t\t\t               " + CamelCase(entity) + "." + dataRootFieldName + @"Data = resizedImageData;");
                    sb.AppendLine("\t\t\t               " + CamelCase(entity) + "." + dataRootFieldName + @"Size = resizedImageData.Length;");
                    sb.AppendLine("\t\t\t           }");
                    sb.AppendLine("\t\t\t       }");
                    sb.AppendLine("\t\t\t    }");
                    sb.AppendLine("\t\t\t});");
                    sb.AppendLine();
                }

                sb.AppendLine("\t\t\tawait CreateAuditEventAsync(Auditor.AuditEngine.AuditType.ReadList, userIsAdmin == true ? \"" + module + "." + entity + " Entity list was read with Admin privilege.  Returning \" + materialized.Count + \" rows of data.\" : \"" + module + "." + entity + " Entity list was read.  Returning \" + materialized.Count + \" rows of data.\");");
                sb.AppendLine();


                sb.AppendLine("\t\t\t// Create a new output object that only includes the relations if necessary, and doesn't include the empty list objects, so that we can reduce the amount of data being transferred.");
                sb.AppendLine("\t\t\tif (includeRelations == true)");
                sb.AppendLine("\t\t\t{");

                sb.AppendLine("\t\t\t\tvar reducedFieldOutput = (from data in output");
                sb.AppendLine($"\t\t\t\t\t\t select {databaseNamespace}.{entity}.CreateAnonymousWithFirstLevelSubObjects(data)).ToList();");

                sb.AppendLine();
                sb.AppendLine("\t\t\t\treturn Ok(reducedFieldOutput);");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\telse");
                sb.AppendLine("\t\t\t{");

                sb.AppendLine("\t\t\t\tvar reducedFieldOutput = (from data in output");
                sb.AppendLine($"\t\t\t\t\t\t select {databaseNamespace}.{entity}.CreateAnonymous(data)).ToList();");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\treturn Ok(reducedFieldOutput);");
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

            sb.AppendLine("\t\t[HttpGet]");
            sb.AppendLine("\t\t[Route(\"api/" + plural + "/RowCount\")]");
            sb.AppendLine("\t\tpublic async Task<IHttpActionResult> GetRowCount(");

            processingFirstProperty = true;
            foreach (PropertyInfo prop in type.GetProperties())
            {
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

                    // bool comes in as int - 1 or 0
                    sb.Append("\t\t\tint? " + prop.Name + " = null");
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
            sb.AppendLine(")");
            sb.AppendLine("\t\t{");

            if (ignoreFoundationServices == false)
            {
                sb.AppendLine("\t\t\tif (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t   return Unauthorized();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();
                sb.AppendLine("\t\t\tif (await IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel.Read) == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t   return Unauthorized();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();


                sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync();");
                sb.AppendLine();
                sb.AppendLine("\t\t\tbool userIsWriter = await UserCanWriteAsync(securityUser, " + entityMinimumWritePermissionString + ");");
                sb.AppendLine("\t\t\tbool userIsAdmin = await UserCanAdministerAsync(securityUser);");
                if (multiTenancyEnabled == true)
                {
                    sb.AppendLine("\t\t\tbool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser);");
                    sb.AppendLine("\t\t\t");
                    sb.AppendLine(UserTenantGuidCommands(3));
                }
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("\t\t\tbool userIsWriter = true;");
                sb.AppendLine("\t\t\tbool userIsAdmin = true;");
                sb.AppendLine();

            }

            commentWritten = false;
            foreach (PropertyInfo prop in type.GetProperties())
            {
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
            sb.AppendLine("\t\t\tvar query = (from " + acronym + " in db." + plural + " select " + acronym + ");");

            if (ignoreFoundationServices == false)
            {
                if (dataVisibilityEnabled == true)
                {
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// Apply the constraints for the user's tenant and data visibility configuration.");
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\tquery = await AddTenantAndDataVisibilityConstraintsAsync(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin);");
                    sb.AppendLine();
                }
                else if (multiTenancyEnabled == true)
                {
                    sb.AppendLine("\t\t\tquery = query.Where(Q => Q.tenantGuid == userTenantGuid);");
                }
            }

            processingFirstProperty = true;
            foreach (PropertyInfo prop in type.GetProperties())
            {
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
                        sb.AppendLine("\t\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".active == (active.Value == 1 ? true : false));");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine("\t\t\t");
                        sb.AppendLine("\t\t\t\tif (userIsAdmin == true)");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t\tif (deleted.HasValue == true)");
                        sb.AppendLine("\t\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".deleted == (deleted.Value == 1 ? true : false));");
                        sb.AppendLine("\t\t\t\t\t}");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine("\t\t\t\telse");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".deleted == false);");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine("\t\t\telse");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".active == true);");
                        sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".deleted == false);");
                        sb.AppendLine("\t\t\t}");
                    }
                    else if (prop.Name == "deleted")
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
                        sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + "." + prop.Name + " == (" + prop.Name + ".Value == 1 ? true : false));");
                        sb.AppendLine("\t\t\t}");
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("\t\t\tint output = await query.CountAsync();");
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
            sb.AppendLine("\t\t[HttpGet]");
            sb.AppendLine("\t\t[Route(\"api/" + pluralizeEntityForRouteForSomeTypeNames(entity) + "/{id}\")]");

            // If we have image fields, then add a new parameters to provide a default image width for reducing the size of images in the get.
            if (hasImageFields == false)
            {
                sb.AppendLine("\t\tpublic async Task<IHttpActionResult> Get" + pluralizeEntityForRouteForSomeTypeNames(entity) + "(int id, bool includeRelations = true)");
            }
            else
            {
                sb.AppendLine("\t\tpublic async Task<IHttpActionResult> Get" + pluralizeEntityForRouteForSomeTypeNames(entity) + "(int id, bool includeRelations = true, int? imageWidth = null)");
            }

            sb.AppendLine("\t\t{");
            if (ignoreFoundationServices == false)
            {
                sb.AppendLine("\t\t\tStartAuditEventClock();");
                sb.AppendLine();

                sb.AppendLine("\t\t\tif (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t   return Unauthorized();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();
                sb.AppendLine("\t\t\tif (await IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel.Read) == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t   return Unauthorized();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();


                sb.AppendLine();
                sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync();");
                sb.AppendLine();
                sb.AppendLine("\t\t\tbool userIsWriter = await UserCanWriteAsync(securityUser, " + entityMinimumWritePermissionString + ");");
                sb.AppendLine("\t\t\tbool userIsAdmin = await UserCanAdministerAsync(securityUser);");
                if (multiTenancyEnabled == true)
                {
                    sb.AppendLine("\t\t\tbool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser);");
                    sb.AppendLine("\t\t\t");
                    sb.AppendLine("\t\t\t");
                    sb.AppendLine(UserTenantGuidCommands(3));
                }
            }
            else
            {
                sb.AppendLine("\t\t\tbool userIsWriter = true;");
                sb.AppendLine("\t\t\tbool userIsAdmin = true;");
                sb.AppendLine();
            }


            sb.AppendLine();
            sb.AppendLine("\t\t\ttry");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\tvar query = (from " + acronym + " in db." + plural + " where");

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
                    sb.AppendLine("\t\t\t\tquery = await AddTenantAndDataVisibilityConstraintsAsync(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin);");
                    sb.AppendLine();
                }
                else if (multiTenancyEnabled == true)
                {
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\tquery = query.Where(Q => Q.tenantGuid == userTenantGuid);");
                }
            }

            // Add the includes
            sb.AppendLine("\t\t\t\tif (includeRelations == true)");
            sb.AppendLine("\t\t\t\t{");

            foreach (PropertyInfo prop in type.GetProperties())
            {
                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                // Is this a linked object type?
                if (propertyType != typeof(string) &&
                    propertyType != typeof(int) &&
                    propertyType != typeof(long) &&
                    propertyType != typeof(DateTime) &&
                    propertyType != typeof(float) &&
                    propertyType != typeof(double) &&
                    propertyType != typeof(decimal) &&
                    propertyType != typeof(bool) &&
                    propertyType != typeof(Guid) &&
                    propertyType != typeof(byte[]) &&
                    propertyType.FullName.StartsWith("System.Collections") == false)
                {
                    sb.AppendLine("\t\t\t\t\tquery = query.Include(\"" + prop.Name + "\");");
                }
            }
            sb.AppendLine("\t\t\t\t}");
            sb.AppendLine();
            sb.AppendLine("\t\t\t\t" + qualifiedEntity + " materialized = await query.FirstOrDefaultAsync();");
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
                    sb.AppendLine("\t\t\t\t\t    materialized." + dataRootFieldName + "Data = await LoadDataFromDiskAsync(materialized.objectGuid, materialized.versionNumber, \"" + dataFileNameExtension + "\");");
                }
                else
                {
                    sb.AppendLine("\t\t\t\t\t    materialized." + dataRootFieldName + "Data = await LoadDataFromDiskAsync(materialized.objectGuid, 0, \"" + dataFileNameExtension + "\");");

                }
                sb.AppendLine("\t\t\t\t\t}");
                sb.AppendLine();
            }

            sb.AppendLine("\t\t\t\t\t");
            sb.AppendLine("\t\t\t\t\t// Convert all the date properties to be of kind UTC.");
            sb.AppendLine("\t\t\t\t\tFoundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, DoesDatabaseStoreDateWithTimeZone(db));");
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
                    sb.AppendLine("\t\t\t\t\tawait CreateAuditEventAsync(Auditor.AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? \"" + module + "." + entity + " Entity was read with Admin privilege.\" : \"" + module + "." + entity + " Entity was read.\");");

                    if (scriptGenTable != null && scriptGenTable.name.EndsWith("ChangeHistory") == false)
                    {
                        sb.AppendLine();
                        sb.AppendLine("\t\t\t\t\tBackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, \"" + entity + "\", materialized.id, materialized." + displayNameField + "));");
                        sb.AppendLine();
                    }
                }

                sb.AppendLine();
                sb.AppendLine("\t\t\t\t\t// Create a new output object that only includes the relations if necessary, and doesn't include the empty list objects, so that we can reduce the amount of data being transferred.");
                sb.AppendLine("\t\t\t\t\tif (includeRelations == true)");
                sb.AppendLine("\t\t\t\t\t{");
                sb.AppendLine($"\t\t\t\t\t\treturn Ok({databaseNamespace}.{entity}.CreateAnonymousWithFirstLevelSubObjects(materialized));");
                sb.AppendLine("\t\t\t\t\t}");
                sb.AppendLine("\t\t\t\t\telse");
                sb.AppendLine("\t\t\t\t\t{");
                sb.AppendLine($"\t\t\t\t\t\treturn Ok({databaseNamespace}.{entity}.CreateAnonymous(materialized));");
                sb.AppendLine("\t\t\t\t\t}");
            }
            else
            {
                sb.AppendLine("\t\t\t\t\t" + entity + "WithWritePermissionDetails " + CamelCase(entity) + "WWP = new " + entity + "WithWritePermissionDetails(materialized);");
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
                sb.AppendLine("\t\t\t\t\t\torganizationsUserIsEntitledToWriteTo = await GetOrganizationIdsUserIsEntitledToWriteToAsync(securityUser);");
                sb.AppendLine("\t\t\t\t\t\tdepartmentsUserIsEntitledToWriteTo = await GetDepartmentIdsUserIsEntitledToWriteToAsync(securityUser);");
                sb.AppendLine("\t\t\t\t\t\tteamsUserIsEntitledToWriteTo = await GetTeamIdsUserIsEntitledToWriteToAsync(securityUser);");
                sb.AppendLine("\t\t\t\t\t\tuserAndTheirReportIds = await GetListOfUserIdsForUserAndPeopleThatReportToThemAsync(securityUser);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\t\t\torganizationsUserIsEntitledToChangeOwnerFor = await GetOrganizationIdsUserIsEntitledToChangeOwnerForAsync(securityUser);");
                sb.AppendLine("\t\t\t\t\t\tdepartmentsUserIsEntitledToChangeOwnerFor = await GetDepartmentIdsUserIsEntitledToChangeOwnerForAsync(securityUser);");
                sb.AppendLine("\t\t\t\t\t\tteamsUserIsEntitledToChangeOwnerFor = await GetTeamIdsUserIsEntitledToChangeOwnerForAsync(securityUser);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\t\t\torganizationsUserIsEntitledToChangeHierarchyFor = await GetOrganizationIdsUserIsEntitledToChangeHierarchyForAsync(securityUser);");
                sb.AppendLine("\t\t\t\t\t\tdepartmentsUserIsEntitledToChangeHierarchyFor = await GetDepartmentIdsUserIsEntitledToChangeHierarchyForAsync(securityUser);");
                sb.AppendLine("\t\t\t\t\t\tteamsUserIsEntitledToChangeHierarchyFor = await GetTeamIdsUserIsEntitledToChangeHierarchyForAsync(securityUser);");
                sb.AppendLine("\t\t\t\t\t}");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\t\tConfigureIsWriteable(" + CamelCase(entity) + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, userAndTheirReportIds, organizationsUserIsEntitledToWriteTo, departmentsUserIsEntitledToWriteTo, teamsUserIsEntitledToWriteTo);");
                sb.AppendLine("\t\t\t\t\tConfigureOwnerIsChangeable(" + CamelCase(entity) + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, organizationsUserIsEntitledToChangeOwnerFor, departmentsUserIsEntitledToChangeOwnerFor, teamsUserIsEntitledToChangeOwnerFor);");
                sb.AppendLine("\t\t\t\t\tConfigureHierarchyIsChangeable(" + CamelCase(entity) + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, organizationsUserIsEntitledToChangeHierarchyFor, departmentsUserIsEntitledToChangeHierarchyFor, teamsUserIsEntitledToChangeHierarchyFor);");

                if (scriptGenTable != null && scriptGenTable.name.EndsWith("ChangeHistory") == false)
                {
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\tBackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, \"" + entity + "\", " + CamelCase(entity) + "WWP.id, " + CamelCase(entity) + "WWP." + displayNameField + "));");
                    sb.AppendLine();
                }

                sb.AppendLine("\t\t\t\t\tawait CreateAuditEventAsync(Auditor.AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? \"" + module + "." + entity + " Entity was read with Admin privilege.\" : \"" + module + "." + entity + " Entity was read.\");");


                sb.AppendLine();
                sb.AppendLine("\t\t\t\t\t// Create a new output object that only includes the relations if necessary, and doesn't include the empty list objects, so that we can reduce the amount of data being transferred.");
                sb.AppendLine("\t\t\t\t\tif (includeRelations == true)");
                sb.AppendLine("\t\t\t\t\t{");
                sb.AppendLine($"\t\t\t\t\t\treturn Ok({databaseNamespace}.{entity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)}WWP));");
                sb.AppendLine("\t\t\t\t\t}");
                sb.AppendLine("\t\t\t\t\telse");
                sb.AppendLine("\t\t\t\t\t{");
                sb.AppendLine($"\t\t\t\t\t\treturn Ok({databaseNamespace}.{entity}.CreateAnonymous({CamelCase(entity)}WWP));");
                sb.AppendLine("\t\t\t\t\t}");
            }


            sb.AppendLine("\t\t\t\t}");
            sb.AppendLine("\t\t\t\telse");
            sb.AppendLine("\t\t\t\t{");
            if (ignoreFoundationServices == false)
            {
                sb.AppendLine("\t\t\t\t\tawait CreateAuditEventAsync(Auditor.AuditEngine.AuditType.Error, \"Attempt to read a " + module + "." + entity + " entity that does not exist.\", id.ToString());");
            }
            sb.AppendLine("\t\t\t\t\treturn BadRequest();");
            sb.AppendLine("\t\t\t\t}");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t\tcatch (Exception ex)");
            sb.AppendLine("\t\t\t{");
            if (ignoreFoundationServices == false)
            {
                sb.AppendLine("\t\t\t\tawait CreateAuditEventAsync(Auditor.AuditEngine.AuditType.Error, userIsAdmin == true ? \"Exception cuaght during entity read of " + module + "." + entity + ".   Entity was read with Admin privilege.\" : \"Exception cuaght during entity read of " + module + "." + entity + ".\", id.ToString(), ex);");
            }
            sb.AppendLine("\t\t\t\treturn InternalServerError(ex);");
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

                sb.AppendLine("\t\t[Route(\"api/" + pluralizeEntityForRouteForSomeTypeNames(entity) + "/{id}\")]");
                sb.AppendLine("\t\t[HttpPost]");
                sb.AppendLine("\t\t[HttpPut]");
                sb.AppendLine("\t\tpublic async Task<IHttpActionResult> Put" + pluralizeEntityForRouteForSomeTypeNames(entity) + "(int id, " + qualifiedEntity + "." + entity + "DTO " + CamelCase(entity) + "DTO)");
                sb.AppendLine("\t\t{");

                sb.AppendLine("\t\t\tif (" + CamelCase(entity) + "DTO == null)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t   return BadRequest();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();

                if (ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\tStartAuditEventClock();");
                    sb.AppendLine();

                    if (adminAccessNeededToWrite == true)
                    {
                        sb.AppendLine("\t\t\t// Admin privilege needed to write to this table.");
                        sb.AppendLine("\t\t\tif (await DoesUserHaveAdminPrivilegeSecurityCheckAsync() == false)");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t   return Unauthorized();");
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine();

                    }
                    else
                    {
                        sb.AppendLine("\t\t\tif (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED) == false)");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t   return Unauthorized();");
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine();

                    }

                    sb.AppendLine();

                    sb.AppendLine("\t\t\tif (await IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel.Write) == false)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t   return Unauthorized();");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();

                }


                sb.AppendLine();
                sb.AppendLine("\t\t\tif (id != " + CamelCase(entity) + "DTO.id)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\treturn BadRequest();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();


                if (ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\tJavaScriptSerializer serializer = new JavaScriptSerializer();");
                    sb.AppendLine("\t\t\tserializer.MaxJsonLength = 100 * 1024 * 1024;       // 100 megabytes");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync();");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tbool userIsWriter = await UserCanWriteAsync(securityUser, " + entityMinimumWritePermissionString + ");");
                    sb.AppendLine("\t\t\tbool userIsAdmin = await UserCanAdministerAsync(securityUser);");

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine("\t\t\tbool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser);");
                        sb.AppendLine("\t\t\t");
                        sb.AppendLine(UserTenantGuidCommands(3));
                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine("\t\t\tbool userIsAdmin = true;");
                }


                sb.AppendLine("\t\t\tIQueryable<" + qualifiedEntity + "> query = (from x in db." + plural);
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
                        sb.AppendLine("\t\t\tquery = await AddTenantAndDataVisibilityConstraintsAsync(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin);");
                        sb.AppendLine();
                    }
                    else if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine("\t\t\tquery = query.Where(Q => Q.tenantGuid == userTenantGuid);");
                    }
                }

                sb.AppendLine();
                sb.AppendLine("\t\t\t" + qualifiedEntity + " existing = await query.FirstOrDefaultAsync();");

                sb.AppendLine();
                sb.AppendLine("\t\t\tif (existing == null)");
                sb.AppendLine("\t\t\t{");

                if (ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, \"Invalid primary key provided for " + module + "." + entity + " PUT\", id.ToString(), new Exception(\"No " + module + "." + entity + " entity could be found with the primary key provided.\"));");
                }

                sb.AppendLine("\t\t\t\treturn NotFound();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();

                sb.AppendLine("\t\t\t// Copy the existing object so it can be serialized as-is in the audit and history logs.");
                sb.AppendLine("\t\t\t" + qualifiedEntity + " cloneOfExisting = (" + qualifiedEntity + ")db.Entry(existing).GetDatabaseValues().ToObject();");
                sb.AppendLine();


                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\t// Create a new " + entity + " object using the data from the existing record, updated with what is in the DTO.");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\t" + qualifiedEntity + " " + CamelCase(entity) + " = (" + qualifiedEntity + ")db.Entry(existing).GetDatabaseValues().ToObject();");
                sb.AppendLine("\t\t\t" + CamelCase(entity) + ".ApplyDTO(" + CamelCase(entity) + "DTO);");


                //
                // Check the tenant guid here.
                //
                if (multiTenancyEnabled == true)
                {
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// The tenant guid for any " + entity + " being saved must match the tenant guid of the user.  ");
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\tif (existing.tenantGuid != userTenantGuid)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.Error, \"Attempt was made to save a record with a tenant guid that is not the user's tenant guid.\", false);");
                    sb.AppendLine("\t\t\t\tthrow new Exception(\"Data integrity violation detected while attempting to save.\");");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine("\t\t\telse");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t\t// Assign the tenantGuid to the " + entity + " because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.");
                    sb.AppendLine("\t\t\t\t" + CamelCase(entity) + ".tenantGuid = existing.tenantGuid;");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();

                }


                if (dataVisibilityEnabled == true && ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\tUser user = await GetUserAsync(userTenantGuid);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tif (user == null)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t    // If we don't have a user record, then the data visibility sync is probably not working..");
                    sb.AppendLine("\t\t\t    await CreateAuditEventAsync(AuditEngine.AuditType.Error, \"Cannot proceed with " + entity + " PUT because Unable to find user record for security user with account name of \" + securityUser.accountName + \".Check the data visibilty sync process.\", false);");
                    sb.AppendLine("\t\t\t    throw new Exception(\"Unable to proceed with " + entity + " save because inconsistency with user record was found.\");");
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
                    sb.AppendLine("\t\t\t\torganizationIdsUserIsEntitledToChangeOwnerFor = await GetOrganizationIdsUserIsEntitledToChangeOwnerForAsync(securityUser);");
                    sb.AppendLine("\t\t\t\tdepartmentIdsUserIsEntitledToChangeOwnerFor = await GetDepartmentIdsUserIsEntitledToChangeOwnerForAsync(securityUser);");
                    sb.AppendLine("\t\t\t\tteamIdsUserIsEntitledToChangeOwnerFor = await GetTeamIdsUserIsEntitledToChangeOwnerForAsync(securityUser);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\torganizationIdsUserIsEntitledToChangeHierarchyFor = await GetOrganizationIdsUserIsEntitledToChangeHierarchyForAsync(securityUser);");
                    sb.AppendLine("\t\t\t\tdepartmentIdsUserIsEntitledToChangeHierarchyFor = await GetDepartmentIdsUserIsEntitledToChangeHierarchyForAsync(securityUser);");
                    sb.AppendLine("\t\t\t\tteamIdsUserIsEntitledToChangeHierarchyFor = await GetTeamIdsUserIsEntitledToChangeHierarchyForAsync(securityUser);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\torganizationIdsUserIsEntitledToWriteTo = await GetOrganizationIdsUserIsEntitledToWriteToAsync(securityUser);");
                    sb.AppendLine("\t\t\t\tdepartmentIdsUserIsEntitledToWriteTo = await GetDepartmentIdsUserIsEntitledToWriteToAsync(securityUser);");
                    sb.AppendLine("\t\t\t\tteamIdsUserIsEntitledToWriteTo = await GetTeamIdsUserIsEntitledToWriteToAsync(securityUser);");
                    sb.AppendLine("\t\t\t\tuserAndTheirReportIds = await GetListOfUserIdsForUserAndPeopleThatReportToThemAsync(securityUser);");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();
                }


                string optionalTab = "";
                if (versionControlEnabled == true)
                {
                    sb.AppendLine("\t\t\tlock (" + CamelCase(entity) + "PutSyncRoot)");
                    sb.AppendLine("\t\t\t{");
                    optionalTab = "\t";

                    sb.AppendLine(optionalTab + "\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t// Validate the version number for the " + CamelCase(entity) + " being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.");
                    sb.AppendLine(optionalTab + "\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\tif (existing.versionNumber != " + CamelCase(entity) + ".versionNumber)");
                    sb.AppendLine(optionalTab + "\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t// Record has changed");
                    sb.AppendLine(optionalTab + "\t\t\t\tCreateAuditEvent(AuditEngine.AuditType.Miscellaneous, \"" + entity + " save attempt was made but save request was with version \" + " + CamelCase(entity) + ".versionNumber + \" and the current version number is \" + existing.versionNumber, false);");
                    sb.AppendLine(optionalTab + "\t\t\t\tthrow new Exception(\"The " + CamelCase(entity) + " you are trying to update has already changed.  Please try your save again after reloading the " + entity + ".\");");
                    sb.AppendLine(optionalTab + "\t\t\t}");
                    sb.AppendLine(optionalTab + "\t\t\telse");
                    sb.AppendLine(optionalTab + "\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t// Same record.  Increase version.");
                    sb.AppendLine(optionalTab + "\t\t\t\t" + CamelCase(entity) + ".versionNumber++;");
                    sb.AppendLine(optionalTab + "\t\t\t}");
                    sb.AppendLine();
                }


                //sb.AppendLine(optionalTab + "\t\t\t" + qualifiedEntity + " cloneOfExisting = (" + qualifiedEntity + ")db.Entry(existing).GetDatabaseValues().ToObject();");
                //sb.AppendLine();
                //sb.AppendLine("\t\t\t\t//");
                //sb.AppendLine("\t\t\t\t// Remove any object fields from the clone object so that it can serialize effectively");
                //sb.AppendLine("\t\t\t\t//");
                //foreach (PropertyInfo prop in type.GetProperties())
                //{
                //    Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                //    // Is this an object type?
                //    if (propertyType != typeof(string) &&
                //        propertyType != typeof(int) &&
                //        propertyType != typeof(long) &&
                //        propertyType != typeof(DateTime) &&
                //        propertyType != typeof(float) &&
                //        propertyType != typeof(double) &&
                //        propertyType != typeof(decimal) &&
                //        propertyType != typeof(bool) &&
                //        propertyType != typeof(Guid))
                //    {
                //        sb.AppendLine("\t\t\t\tcloneOfExisting." + prop.Name + " = null;");
                //    }
                //}
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
                    sb.AppendLine(optionalTab + "\t\t\t\t\t(existing.organizationId.HasValue == true && " + CamelCase(entity) + ".organizationId.HasValue == false ||");
                    sb.AppendLine(optionalTab + "\t\t\t\t\texisting.departmentId.HasValue == true && " + CamelCase(entity) + ".departmentId.HasValue == false ||");
                    sb.AppendLine(optionalTab + "\t\t\t\t\texisting.teamId.HasValue == true && " + CamelCase(entity) + ".teamId.HasValue == false");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t))");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\tCreateAuditEvent(AuditEngine.AuditType.Error, \"Attempt was made to nullify one or more data visibiilty field by a non admin user.\", false);");
                    sb.AppendLine(optionalTab + "\t\t\t\t\tthrow new Exception(\"Data integrity violation detected while attempting to save record.  Data visibility fields state is invalid.\");");
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
                    sb.AppendLine(optionalTab + "\t\t\t\tif (" + CamelCase(entity) + ".userId.HasValue == true)");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\tnewOwner = GetUser(" + CamelCase(entity) + ".userId.Value);");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\tif (" + CamelCase(entity) + ".teamId.HasValue == true)");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\tnewTeam = GetTeam(" + CamelCase(entity) + ".teamId.Value);");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\tif (" + CamelCase(entity) + ".departmentId.HasValue == true)");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\tnewDepartment = GetDepartment(" + CamelCase(entity) + ".departmentId.Value);");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");
                    sb.AppendLine(optionalTab + "\t\t\t\telse");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t// Set the department to be that of the team if no department is explicitly provided and we have a team.");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t\tif (newTeam != null)");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\t" + CamelCase(entity) + ".departmentId = newTeam.departmentId;");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\tnewDepartment = GetDepartment(newTeam.departmentId);");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t}");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\tif (" + CamelCase(entity) + ".organizationId.HasValue == true)");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\tnewOrganization = GetOrganization(" + CamelCase(entity) + ".organizationId.Value);");
                    sb.AppendLine(optionalTab + "\t\t\t\t}");
                    sb.AppendLine(optionalTab + "\t\t\t\telse");
                    sb.AppendLine(optionalTab + "\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t// Set the organization to be that of the department if no organization is explicitly provided and we have a department");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t\tif (newDepartment != null)");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\t" + CamelCase(entity) + ".organizationId = newDepartment.organizationId;");
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
                    sb.AppendLine(optionalTab + "\t\t\t\t\tif (existing.userId != " + CamelCase(entity) + ".userId)");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\t//");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\t// Make sure that the current user can change owner ship at the hierarcy level of the 'existing' record state - NOT THE NEW STATE.");
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
                    sb.AppendLine(optionalTab + "\t\t\t\t\tif (existing.organizationId != " + CamelCase(entity) + ".organizationId ||");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\texisting.departmentId != " + CamelCase(entity) + ".departmentId ||");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\texisting.teamId != " + CamelCase(entity) + ".teamId)");
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
                    sb.AppendLine(optionalTab + "\t\t\t\tCreateAuditEvent(AuditEngine.AuditType.Error, \"Error caught while setting or validating tenant or data visibility fields on a PUT operation.\", false, \"\", serializer.Serialize(" + CamelCase(entity) + "), null, ex);");
                    sb.AppendLine(optionalTab + "\t\t\t\tthrow new Exception(\"Error caught while setting or validating tenant or data visibility fields on a PUT operation.\", ex);");
                    sb.AppendLine(optionalTab + "\t\t\t}");
                    sb.AppendLine();
                }


                if (hasActiveAndDeletedControlFields == true)
                {
                    sb.AppendLine(optionalTab + "\t\t\t// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?");
                    sb.AppendLine(optionalTab + "\t\t\tif (userIsAdmin == false && (" + CamelCase(entity) + ".deleted == true || existing.deleted == true))");
                    sb.AppendLine(optionalTab + "\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t// we're not recording state here because it is not being changed.");
                    if (ignoreFoundationServices == false)
                    {
                        sb.AppendLine(optionalTab + "\t\t\t\tCreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, \"Attempt to delete a record or work on a deleted " + module + "." + entity + " record.\", id.ToString());");
                        sb.AppendLine(optionalTab + "\t\t\t\tThrowUnauthorizedExceptionAndDestroySession();");
                    }
                    else
                    {
                        sb.AppendLine(optionalTab + "\t\t\t\tthrow new Exception(\"Cannot modify record\");");
                    }
                    sb.AppendLine(optionalTab + "\t\t\t}");
                }


                sb.AppendLine();

                foreach (PropertyInfo prop in type.GetProperties())
                {
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
                            sb.AppendLine(optionalTab + "\t\t\tif (" + CamelCase(entity) + "." + prop.Name + ".HasValue == true && " + CamelCase(entity) + "." + prop.Name + ".Value.Kind != DateTimeKind.Utc)");
                            sb.AppendLine(optionalTab + "\t\t\t{");
                            sb.AppendLine(optionalTab + "\t\t\t\t" + CamelCase(entity) + "." + prop.Name + " = " + CamelCase(entity) + "." + prop.Name + ".Value.ToUniversalTime();");
                            sb.AppendLine(optionalTab + "\t\t\t}");
                            sb.AppendLine();
                        }
                        else
                        {
                            sb.AppendLine(optionalTab + "\t\t\tif (" + CamelCase(entity) + "." + prop.Name + ".Kind != DateTimeKind.Utc)");
                            sb.AppendLine(optionalTab + "\t\t\t{");
                            sb.AppendLine(optionalTab + "\t\t\t\t" + CamelCase(entity) + "." + prop.Name + " = " + CamelCase(entity) + "." + prop.Name + ".ToUniversalTime();");
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
                                sb.AppendLine(optionalTab + "\t\t\tif (" + CamelCase(entity) + "." + prop.Name + " != null && " + CamelCase(entity) + "." + prop.Name + ".Length > " + scriptGenField.MaxStringLength().ToString() + ")");
                                sb.AppendLine(optionalTab + "\t\t\t{");
                                sb.AppendLine(optionalTab + "\t\t\t\t" + CamelCase(entity) + "." + prop.Name + " = " + CamelCase(entity) + "." + prop.Name + ".Substring(0, " + scriptGenField.MaxStringLength().ToString() + ");");
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

                        sb.AppendLine("\t\t\t\tif (" + CamelCase(entity) + "." + dataRootFieldName + "Data != null && string.IsNullOrEmpty(" + CamelCase(entity) + "." + dataRootFieldName + "FileName))");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "FileName = " + CamelCase(entity) + ".objectGuid.ToString() + \"." + dataFileNameExtension + "\";");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();

                        sb.AppendLine("\t\t\t\tif (" + CamelCase(entity) + "." + dataRootFieldName + "Data != null && (" + CamelCase(entity) + "." + dataRootFieldName + "Size.HasValue == false || " + CamelCase(entity) + "." + dataRootFieldName + "Size != " + CamelCase(entity) + "." + dataRootFieldName + "Data.Length))");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size = " + CamelCase(entity) + "." + dataRootFieldName + "Data.Length;");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();

                        sb.AppendLine("\t\t\t\tif (" + CamelCase(entity) + "." + dataRootFieldName + "Data != null && string.IsNullOrEmpty(" + CamelCase(entity) + "." + dataRootFieldName + "MimeType))");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "MimeType = \"" + dataDefaultMimeType + "\";");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.AppendLine();
                        sb.AppendLine("\t\t\t\t//");
                        sb.AppendLine("\t\t\t\t// Add default values for any missing data attribute fields.");
                        sb.AppendLine("\t\t\t\t//");

                        sb.AppendLine("\t\t\t\tif (" + CamelCase(entity) + "." + dataRootFieldName + "Data != null && string.IsNullOrEmpty(" + CamelCase(entity) + "." + dataRootFieldName + "FileName))");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "FileName = " + CamelCase(entity) + ".objectGuid.ToString() + \"." + dataFileNameExtension + "\";");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();

                        sb.AppendLine("\t\t\t\tif (" + CamelCase(entity) + "." + dataRootFieldName + "Data != null && " + CamelCase(entity) + "." + dataRootFieldName + "Size != " + CamelCase(entity) + "." + dataRootFieldName + "Data.Length)");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size = " + CamelCase(entity) + "." + dataRootFieldName + "Data.Length;");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();

                        sb.AppendLine("\t\t\t\tif (" + CamelCase(entity) + "." + dataRootFieldName + "Data != null && string.IsNullOrEmpty(" + CamelCase(entity) + "." + dataRootFieldName + "MimeType))");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "MimeType = \"" + dataDefaultMimeType + "\";");
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
                        sb.AppendLine(optionalTab + "\t\t\t\tbyte[] dataReferenceBeforeClearing = " + CamelCase(entity) + "." + dataRootFieldName + "Data;");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\tif (diskBasedBinaryStorageMode == true &&");
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "FileName != null &&");
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Data != null &&");

                        if (BinaryStorageFieldsAreNullable(scriptGenTable) == true)
                        {
                            sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size.HasValue == true &&");
                            sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size.Value > 0)");
                        }
                        else
                        {
                            sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size > 0)");
                        }

                        sb.AppendLine(optionalTab + "\t\t\t\t{");

                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    // write the bytes to disk");
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    WriteDataToDisk(" + CamelCase(entity) + ".objectGuid, 0, " + CamelCase(entity) + "." + dataRootFieldName + "Data, \"" + dataFileNameExtension + "\");");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    // Clear the data from the object before we put it into the db");
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Data = null;");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\t}");
                        sb.AppendLine();
                    }

                    sb.AppendLine(optionalTab + "\t\t\tvar attached = db.Entry(existing);");
                    sb.AppendLine(optionalTab + "\t\t\tattached.CurrentValues.SetValues(" + CamelCase(entity) + ");");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\ttry");
                    sb.AppendLine(optionalTab + "\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\tawait db.SaveChangesAsync();");

                    if (ignoreFoundationServices == false)
                    {
                        //sb.AppendLine(optionalTab + "\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, \"" + module + "." + entity + " entity successfully updated\", true, id.ToString(), serializer.Serialize(cloneOfExisting), serializer.Serialize(" + CamelCase(entity) + "), null);");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t\"" + module + "." + entity + " entity successfully updated.\",");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "true,");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "id.ToString(),");
                        sb.AppendLine($"{optionalTab}\t\t\t\t\tserializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),");
                        sb.AppendLine($"{optionalTab}\t\t\t\t\tserializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)})),");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "null);");
                        sb.AppendLine();

                    }

                    sb.AppendLine();

                    if (dataVisibilityEnabled == true && ignoreFoundationServices == false)
                    {
                        sb.AppendLine(optionalTab + "\t\t\t\t" + entity + "WithWritePermissionDetails " + CamelCase(entity) + "WWP = new " + entity + "WithWritePermissionDetails(" + CamelCase(entity) + ");");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\tConfigureIsWriteable(" + CamelCase(entity) + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, userAndTheirReportIds, organizationIdsUserIsEntitledToWriteTo, departmentIdsUserIsEntitledToWriteTo, teamIdsUserIsEntitledToWriteTo);");
                        sb.AppendLine(optionalTab + "\t\t\t\tConfigureOwnerIsChangeable(" + CamelCase(entity) + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, organizationIdsUserIsEntitledToChangeOwnerFor, departmentIdsUserIsEntitledToChangeOwnerFor, teamIdsUserIsEntitledToChangeOwnerFor);");
                        sb.AppendLine(optionalTab + "\t\t\t\tConfigureHierarchyIsChangeable(" + CamelCase(entity) + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, organizationIdsUserIsEntitledToChangeHierarchyFor, departmentIdsUserIsEntitledToChangeHierarchyFor, teamIdsUserIsEntitledToChangeHierarchyFor);");
                        sb.AppendLine();
                        if (HasBinaryStorageFields(scriptGenTable) == true)
                        {
                            sb.AppendLine();
                            sb.AppendLine(optionalTab + "\t\t\t\tif (diskBasedBinaryStorageMode == true)");
                            sb.AppendLine(optionalTab + "\t\t\t\t{");
                            sb.AppendLine(optionalTab + "\t\t\t\t    //");
                            sb.AppendLine(optionalTab + "\t\t\t\t    // Put the data bytes back into the object that will be returned.");
                            sb.AppendLine(optionalTab + "\t\t\t\t    //");
                            sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Data = dataReferenceBeforeClearing;");
                            sb.AppendLine(optionalTab + "\t\t\t\t}");

                        }
                        sb.AppendLine($"{optionalTab}\t\t\t\treturn Ok({databaseNamespace}.{entity}.CreateAnonymous({CamelCase(entity)}WWP));");
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
                            sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Data = dataReferenceBeforeClearing;");
                            sb.AppendLine(optionalTab + "\t\t\t\t}");

                        }
                        sb.AppendLine($"{optionalTab}\t\t\t\treturn Ok({databaseNamespace}.{entity}.CreateAnonymous({CamelCase(entity)}));");
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
                    //sb.AppendLine(optionalTab + "\t\t\t    string serialized" + entity + " = serializer.Serialize(" + CamelCase(entity) + ");");
                    //sb.AppendLine();

                    if (HasBinaryStorageFields(scriptGenTable) == true)
                    {
                        sb.AppendLine(optionalTab + "\t\t\t\tbyte[] dataReferenceBeforeClearing = " + CamelCase(entity) + "." + dataRootFieldName + "Data;");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\tif (diskBasedBinaryStorageMode == true &&");
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "FileName != null &&");
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Data != null &&");

                        if (BinaryStorageFieldsAreNullable(scriptGenTable) == true)
                        {

                            sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size.HasValue == true &&");
                            sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size.Value > 0)");
                        }
                        else
                        {
                            sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size > 0)");
                        }
                        sb.AppendLine(optionalTab + "\t\t\t\t{");

                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    // write the bytes to disk");
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    WriteDataToDisk(" + CamelCase(entity) + ".objectGuid, " + CamelCase(entity) + ".versionNumber, " + CamelCase(entity) + "." + dataRootFieldName + "Data, \"" + dataFileNameExtension + "\");");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    // Clear the data from the object before we put it into the db");
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Data = null;");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\t}");
                        sb.AppendLine();
                    }


                    sb.AppendLine(optionalTab + "\t\t\t    var attached = db.Entry(existing);");
                    sb.AppendLine(optionalTab + "\t\t\t    attached.CurrentValues.SetValues(" + CamelCase(entity) + ");");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t    using (var transaction = db.Database.BeginTransaction())");
                    sb.AppendLine(optionalTab + "\t\t\t    {");
                    sb.AppendLine(optionalTab + "\t\t\t        db.SaveChanges();");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t        //");
                    sb.AppendLine(optionalTab + "\t\t\t        // Now add the change history");
                    sb.AppendLine(optionalTab + "\t\t\t        //");
                    sb.AppendLine(optionalTab + "\t\t\t        " + entity + "ChangeHistory " + CamelCase(entity) + "ChangeHistory = new " + entity + "ChangeHistory();");
                    sb.AppendLine(optionalTab + "\t\t\t        " + CamelCase(entity) + "ChangeHistory." + CamelCase(entity, false) + "Id = " + CamelCase(entity) + ".id;");
                    sb.AppendLine(optionalTab + "\t\t\t        " + CamelCase(entity) + "ChangeHistory.versionNumber = " + CamelCase(entity) + ".versionNumber;");
                    sb.AppendLine(optionalTab + "\t\t\t        " + CamelCase(entity) + "ChangeHistory.timeStamp = DateTime.UtcNow;");


                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine(optionalTab + "\t\t\t        " + CamelCase(entity) + "ChangeHistory.userId = user.id;");
                    }
                    else
                    {
                        sb.AppendLine(optionalTab + "\t\t\t        " + CamelCase(entity) + "ChangeHistory.userId = securityUser.id;");
                    }

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine(optionalTab + "\t\t\t        " + CamelCase(entity) + "ChangeHistory.tenantGuid = userTenantGuid;");
                    }

                    sb.AppendLine($"{optionalTab}\t\t\t        {CamelCase(entity)}ChangeHistory.data = serializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)}));");
                    sb.AppendLine(optionalTab + "\t\t\t        db." + entity + "ChangeHistories.Add(" + CamelCase(entity) + "ChangeHistory);");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t        db.SaveChanges();");
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
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Data = dataReferenceBeforeClearing;");
                        sb.AppendLine(optionalTab + "\t\t\t\t}");

                    }

                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\tCreateAuditEvent(AuditEngine.AuditType.UpdateEntity,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\"" + module + "." + entity + " entity successfully updated.\",");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "true,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "id.ToString(),");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tserializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tserializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)})),");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "null);");
                    sb.AppendLine();

                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine(optionalTab + "\t\t\t\t" + entity + "WithWritePermissionDetails " + CamelCase(entity) + "WWP = new " + entity + "WithWritePermissionDetails(" + CamelCase(entity) + ");");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\tConfigureIsWriteable(" + CamelCase(entity) + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, userAndTheirReportIds, organizationIdsUserIsEntitledToWriteTo, departmentIdsUserIsEntitledToWriteTo, teamIdsUserIsEntitledToWriteTo);");
                        sb.AppendLine(optionalTab + "\t\t\t\tConfigureOwnerIsChangeable(" + CamelCase(entity) + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, organizationIdsUserIsEntitledToChangeOwnerFor, departmentIdsUserIsEntitledToChangeOwnerFor, teamIdsUserIsEntitledToChangeOwnerFor);");
                        sb.AppendLine(optionalTab + "\t\t\t\tConfigureHierarchyIsChangeable(" + CamelCase(entity) + "WWP, securityUser, userTenantGuid, userIsAdmin, userIsWriter, organizationIdsUserIsEntitledToChangeHierarchyFor, departmentIdsUserIsEntitledToChangeHierarchyFor, teamIdsUserIsEntitledToChangeHierarchyFor);");
                        sb.AppendLine();
                        sb.AppendLine($"{optionalTab}\t\t\t\treturn Ok({databaseNamespace}.{entity}.CreateAnonymous({CamelCase(entity)}WWP));");
                    }
                    else
                    {
                        sb.AppendLine($"{optionalTab}\t\t\treturn Ok({databaseNamespace}.{entity}.CreateAnonymous({CamelCase(entity)}));");
                    }
                }

                sb.AppendLine(optionalTab + "\t\t\t}");
                sb.AppendLine(optionalTab + "\t\t\tcatch (Exception ex)");
                sb.AppendLine(optionalTab + "\t\t\t{");

                if (ignoreFoundationServices == false)
                {
                    //sb.AppendLine(optionalTab + "\t\t\t\tCreateAuditEvent(AuditEngine.AuditType.UpdateEntity, \"" + module + "." + entity + " entity update failed.\", false, id.ToString(), serializer.Serialize(cloneOfExisting), serializer.Serialize(" + CamelCase(entity) + "), ex);");

                    sb.AppendLine(optionalTab + "\t\t\t\tCreateAuditEvent(AuditEngine.AuditType.UpdateEntity,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\"" + module + "." + entity + " entity update failed\",");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "false,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "id.ToString(),");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tserializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tserializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)})),");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "ex);");
                }
                sb.AppendLine(optionalTab + "\t\t\t\tthrow;");
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
                sb.AppendLine("\t\t[HttpPost]");
                sb.AppendLine("\t\t[Route(\"api/" + pluralizeEntityForRouteForSomeTypeNames(entity) + "\", Name = \"" + pluralizeEntityForRouteForSomeTypeNames(entity) + "\")]");
                sb.AppendLine("\t\tpublic async Task<IHttpActionResult> Post" + pluralizeEntityForRouteForSomeTypeNames(entity) + "(" + qualifiedEntity + "." + entity + "DTO " + CamelCase(entity) + "DTO)");
                sb.AppendLine("\t\t{");

                sb.AppendLine("\t\t\tif (" + CamelCase(entity) + "DTO == null)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t   return BadRequest();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();

                if (ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\tStartAuditEventClock();");
                    sb.AppendLine();

                    if (adminAccessNeededToWrite == true)
                    {
                        sb.AppendLine("\t\t\t// Admin privilege needed to write to this table.");
                        sb.AppendLine("\t\t\tif (await DoesUserHaveAdminPrivilegeSecurityCheckAsync() == false)");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t   return Unauthorized();");
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine();

                    }
                    else
                    {
                        sb.AppendLine("\t\t\tif (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED) == false)");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t   return Unauthorized();");
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine();
                    }

                    sb.AppendLine();
                    sb.AppendLine("\t\t\tif (await IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel.Write) == false)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t   return Unauthorized();");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();


                    if (HasBinaryStorageFields(scriptGenTable) == true)
                    {
                        sb.AppendLine("\t\t\tif (" + CamelCase(entity) + "DTO." + dataRootFieldName + "Data == null)");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t	return BadRequest(\"No data\");");
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine();
                    }

                    sb.AppendLine();
                    sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync();");
                    sb.AppendLine();
                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine("\t\t\tbool userIsAdmin = await UserCanAdministerAsync(securityUser);");
                        sb.AppendLine("\t\t\tbool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser);");
                        sb.AppendLine("\t\t\t");
                        sb.AppendLine(UserTenantGuidCommands(3));
                    }
                    sb.AppendLine("\t\t\tJavaScriptSerializer serializer = new JavaScriptSerializer();");
                    sb.AppendLine("\t\t\tserializer.MaxJsonLength = 100 * 1024 * 1024;       // 100 megabytes");
                    sb.AppendLine();
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
                    sb.AppendLine("\t\t\t\torganizationIdsUserIsEntitledToChangeOwnerFor = await GetOrganizationIdsUserIsEntitledToChangeOwnerForAsync(securityUser);");
                    sb.AppendLine("\t\t\t\tdepartmentIdsUserIsEntitledToChangeOwnerFor = await GetDepartmentIdsUserIsEntitledToChangeOwnerForAsync(securityUser);");
                    sb.AppendLine("\t\t\t\tteamIdsUserIsEntitledToChangeOwnerFor = await GetTeamIdsUserIsEntitledToChangeOwnerForAsync(securityUser);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\torganizationIdsUserIsEntitledToChangeHierarchyFor = await GetOrganizationIdsUserIsEntitledToChangeHierarchyForAsync(securityUser);");
                    sb.AppendLine("\t\t\t\tdepartmentIdsUserIsEntitledToChangeHierarchyFor = await GetDepartmentIdsUserIsEntitledToChangeHierarchyForAsync(securityUser);");
                    sb.AppendLine("\t\t\t\tteamIdsUserIsEntitledToChangeHierarchyFor = await GetTeamIdsUserIsEntitledToChangeHierarchyForAsync(securityUser);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\torganizationIdsUserIsEntitledToWriteTo = await GetOrganizationIdsUserIsEntitledToWriteToAsync(securityUser);");
                    sb.AppendLine("\t\t\t\tdepartmentIdsUserIsEntitledToWriteTo = await GetDepartmentIdsUserIsEntitledToWriteToAsync(securityUser);");
                    sb.AppendLine("\t\t\t\tteamIdsUserIsEntitledToWriteTo = await GetTeamIdsUserIsEntitledToWriteToAsync(securityUser);");
                    sb.AppendLine("\t\t\t\tuserAndTheirReportIds = await GetListOfUserIdsForUserAndPeopleThatReportToThemAsync(securityUser);");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();
                }


                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\t// Create a new " + entity + " object using the data from the DTO");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\t" + qualifiedEntity + " " + CamelCase(entity) + " = " + qualifiedEntity + ".FromDTO(" + CamelCase(entity) + "DTO);");
                sb.AppendLine();

                sb.AppendLine("\t\t\ttry");
                sb.AppendLine("\t\t\t{");


                if (dataVisibilityEnabled == true && ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\t\tUser user = await GetUserAsync(userTenantGuid);");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\tif (user == null)");
                    sb.AppendLine(optionalTab + "\t\t\t{");
                    sb.AppendLine(optionalTab + "\t\t\t\t// If we don't have a user record, then the data visibility sync is probably not working..");
                    sb.AppendLine(optionalTab + "\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.Error, \"Cannot proceed with " + entity + " POST because Unable to find user record for security user with account name of \" + securityUser.accountName + \".Check the data visibilty sync process.\", false);");
                    sb.AppendLine(optionalTab + "\t\t\t\tthrow new Exception(\"Unable to proceed with " + entity + " save because inconsistency with user record was found.\");");
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
                    sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + ".userId = user.id;");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + ".tenantGuid = userTenantGuid;");
                    sb.AppendLine();

                    sb.AppendLine("\t\t\t\t\tList<Organization> organizationsUserIsEntitledToWriteTo = await GetOrganizationsUserIsEntitledToWriteToAsync(securityUser);");
                    sb.AppendLine("\t\t\t\t\tList<Department> departmentsUserIsEntitledToWriteTo = await GetDepartmentsUserIsEntitledToWriteToAsync(securityUser);");
                    sb.AppendLine("\t\t\t\t\tList<Team> teamsUserIsEntitledToWriteTo = await GetTeamsUserIsEntitledToWriteToAsync(securityUser);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t// If the new " + CamelCase(entity) + " has no data visibility attributes set, then use the user's defaults.");
                    sb.AppendLine("\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\tif (" + CamelCase(entity) + ".organizationId.HasValue == false &&");
                    sb.AppendLine("\t\t\t\t\t\t" + CamelCase(entity) + ".departmentId.HasValue == false &&");
                    sb.AppendLine("\t\t\t\t\t\t" + CamelCase(entity) + ".teamId.HasValue == false)");
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
                    sb.AppendLine("\t\t\t\t\t\t\tdefaultDepartment = await GetDepartmentAsync(defaultTeam.departmentId);");
                    sb.AppendLine("\t\t\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t\tif (defaultDepartment != null && defaultOrganization == null)");
                    sb.AppendLine("\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\tdefaultOrganization = await GetOrganizationAsync(defaultDepartment.organizationId);");
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
                    sb.AppendLine("\t\t\t\t\t\t\t" + CamelCase(entity) + ".organizationId = defaultOrganization.id;");
                    sb.AppendLine("\t\t\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t\tif (defaultDepartment != null)");
                    sb.AppendLine("\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\t" + CamelCase(entity) + ".departmentId = defaultDepartment.id;");
                    sb.AppendLine("\t\t\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t\tif (defaultTeam != null)");
                    sb.AppendLine("\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\t" + CamelCase(entity) + ".teamId = defaultTeam.id;");
                    sb.AppendLine("\t\t\t\t\t\t}");
                    sb.AppendLine("\t\t\t\t\t}");
                    sb.AppendLine("\t\t\t\t\telse");
                    sb.AppendLine("\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\t// New " + CamelCase(entity) + " has Data Visibility properties already set.  Validate that they are OK.");
                    sb.AppendLine("\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\tOrganization organization = null;");
                    sb.AppendLine("\t\t\t\t\t\tDepartment department = null;");
                    sb.AppendLine("\t\t\t\t\t\tTeam team = null;");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t\t// ");
                    sb.AppendLine("\t\t\t\t\t\t// step 1 - Load the data visibility entities, and fill in any blanks we can calculate on this record.");
                    sb.AppendLine("\t\t\t\t\t\t// ");
                    sb.AppendLine("\t\t\t\t\t\tif (" + CamelCase(entity) + ".teamId.HasValue == true)");
                    sb.AppendLine("\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\tteam = await GetTeamAsync(" + CamelCase(entity) + ".teamId.Value);");
                    sb.AppendLine("\t\t\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t\tif (" + CamelCase(entity) + ".departmentId.HasValue == true)");
                    sb.AppendLine("\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\tdepartment = await GetDepartmentAsync(" + CamelCase(entity) + ".departmentId.Value);");
                    sb.AppendLine("\t\t\t\t\t\t}");
                    sb.AppendLine("\t\t\t\t\t\telse");
                    sb.AppendLine("\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\t\t// Set the department to be that of the team if no department is explicitly provided and we have a team");
                    sb.AppendLine("\t\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\t\tif (team != null)");
                    sb.AppendLine("\t\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\t\t" + CamelCase(entity) + ".departmentId = team.departmentId;");
                    sb.AppendLine("\t\t\t\t\t\t\t\tdepartment = await GetDepartmentAsync(team.departmentId);");
                    sb.AppendLine("\t\t\t\t\t\t\t}");
                    sb.AppendLine("\t\t\t\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t\t\tif (" + CamelCase(entity) + ".organizationId.HasValue == true)");
                    sb.AppendLine("\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\torganization = await GetOrganizationAsync(" + CamelCase(entity) + ".organizationId.Value);");
                    sb.AppendLine("\t\t\t\t\t\t}");
                    sb.AppendLine("\t\t\t\t\t\telse");
                    sb.AppendLine("\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\t\t// Set the organization to be that of the department if no organization is explicitly provided and we have a department");
                    sb.AppendLine("\t\t\t\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t\t\t\tif (department != null)");
                    sb.AppendLine("\t\t\t\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t\t\t\t\t" + CamelCase(entity) + ".organizationId = department.organizationId;");
                    sb.AppendLine("\t\t\t\t\t\t\t\torganization = await GetOrganizationAsync(department.organizationId);");
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
                    sb.AppendLine("\t\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.Error, \"Error caught while setting or validating data visibility fields on a POST operation.\", false, \"\", serializer.Serialize(" + CamelCase(entity) + "), null, ex);");
                    sb.AppendLine("\t\t\t\t\tthrow new Exception(\"Error caught while setting or validating data visibility fields on a POST operation.\", ex);");
                    sb.AppendLine("\t\t\t\t}");
                    sb.AppendLine();
                }
                else if (multiTenancyEnabled == true && ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t// Ensure that the tenant data is correct.");
                    sb.AppendLine("\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t" + CamelCase(entity) + ".tenantGuid = userTenantGuid;");
                    sb.AppendLine();
                }


                foreach (PropertyInfo prop in type.GetProperties())
                {
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
                            sb.AppendLine("\t\t\t\tif (" + CamelCase(entity) + "." + prop.Name + ".HasValue == true && " + CamelCase(entity) + "." + prop.Name + ".Value.Kind != DateTimeKind.Utc)");
                            sb.AppendLine("\t\t\t\t{");
                            sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + "." + prop.Name + " = " + CamelCase(entity) + "." + prop.Name + ".Value.ToUniversalTime();");
                            sb.AppendLine("\t\t\t\t}");
                            sb.AppendLine();
                        }
                        else
                        {
                            sb.AppendLine("\t\t\t\tif (" + CamelCase(entity) + "." + prop.Name + ".Kind != DateTimeKind.Utc)");
                            sb.AppendLine("\t\t\t\t{");
                            sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + "." + prop.Name + " = " + CamelCase(entity) + "." + prop.Name + ".ToUniversalTime();");
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
                                sb.AppendLine("\t\t\t\tif (" + CamelCase(entity) + "." + prop.Name + " != null && " + CamelCase(entity) + "." + prop.Name + ".Length > " + scriptGenField.MaxStringLength().ToString() + ")");
                                sb.AppendLine("\t\t\t\t{");
                                sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + "." + prop.Name + " = " + CamelCase(entity) + "." + prop.Name + ".Substring(0, " + scriptGenField.MaxStringLength().ToString() + ");");
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
                        sb.AppendLine("\t\t\t\t" + CamelCase(entity) + ".objectGuid = Guid.NewGuid();");
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

                        sb.AppendLine("\t\t\t\tif (" + CamelCase(entity) + "." + dataRootFieldName + "Data != null && string.IsNullOrEmpty(" + CamelCase(entity) + "." + dataRootFieldName + "FileName))");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "FileName = " + CamelCase(entity) + ".objectGuid.ToString() + \"." + dataFileNameExtension + "\";");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();

                        sb.AppendLine("\t\t\t\tif (" + CamelCase(entity) + "." + dataRootFieldName + "Data != null && (" + CamelCase(entity) + "." + dataRootFieldName + "Size.HasValue == false || " + CamelCase(entity) + "." + dataRootFieldName + "Size != " + CamelCase(entity) + "." + dataRootFieldName + "Data.Length))");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size = " + CamelCase(entity) + "." + dataRootFieldName + "Data.Length;");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();

                        sb.AppendLine("\t\t\t\tif (" + CamelCase(entity) + "." + dataRootFieldName + "Data != null && string.IsNullOrEmpty(" + CamelCase(entity) + "." + dataRootFieldName + "MimeType))");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "MimeType = \"" + dataDefaultMimeType + "\";");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.AppendLine();
                        sb.AppendLine("\t\t\t\t//");
                        sb.AppendLine("\t\t\t\t// Add default values for any missing data attribute fields.");
                        sb.AppendLine("\t\t\t\t//");

                        sb.AppendLine("\t\t\t\tif (" + CamelCase(entity) + "." + dataRootFieldName + "Data != null && string.IsNullOrEmpty(" + CamelCase(entity) + "." + dataRootFieldName + "FileName))");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "FileName = " + CamelCase(entity) + ".objectGuid.ToString() + \"." + dataFileNameExtension + "\";");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();

                        sb.AppendLine("\t\t\t\tif (" + CamelCase(entity) + "." + dataRootFieldName + "Data != null && " + CamelCase(entity) + "." + dataRootFieldName + "Size != " + CamelCase(entity) + "." + dataRootFieldName + "Data.Length)");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size = " + CamelCase(entity) + "." + dataRootFieldName + "Data.Length;");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine();

                        sb.AppendLine("\t\t\t\tif (" + CamelCase(entity) + "." + dataRootFieldName + "Data != null && string.IsNullOrEmpty(" + CamelCase(entity) + "." + dataRootFieldName + "MimeType))");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "MimeType = \"" + dataDefaultMimeType + "\";");
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
                        sb.AppendLine(optionalTab + "\t\t\t\t\tbyte[] dataReferenceBeforeClearing = " + CamelCase(entity) + "." + dataRootFieldName + "Data;");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\tif (diskBasedBinaryStorageMode == true &&");
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Data != null &&");
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "FileName != null &&");

                        if (BinaryStorageFieldsAreNullable(scriptGenTable) == true)
                        {
                            sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size.HasValue == true &&");
                            sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size.Value > 0)");
                        }
                        else
                        {
                            sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size > 0)");
                        }

                        sb.AppendLine(optionalTab + "\t\t\t\t{");

                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    // write the bytes to disk");
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    await WriteDataToDiskAsync(" + CamelCase(entity) + ".objectGuid, 0, " + CamelCase(entity) + "." + dataRootFieldName + "Data, \"" + dataFileNameExtension + "\");");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    // Clear the data from the object before we put it into the db");
                        sb.AppendLine(optionalTab + "\t\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Data = null;");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\t}");
                        sb.AppendLine();

                    }

                    sb.AppendLine("\t\t\t\tdb." + plural + ".Add(" + CamelCase(entity) + ");");
                    sb.AppendLine("\t\t\t\tawait db.SaveChangesAsync();");

                    if (ignoreFoundationServices == false)
                    {
                        //sb.AppendLine("\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, \"" + module + "." + entity + " entity successfully created.\", true, " + CamelCase(entity) + ".id.ToString(), \"\", serializer.Serialize(" + CamelCase(entity) + "), null);");

                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t\"" + module + "." + entity + " entity successfully created.\",");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "true,");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + CamelCase(entity) + ".id.ToString(),");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t\"\",");
                        sb.AppendLine($"{optionalTab}\t\t\t\t\tserializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)})),");
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
                        sb.AppendLine(optionalTab + "\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Data = dataReferenceBeforeClearing;");
                        sb.AppendLine(optionalTab + "\t\t\t}");
                        sb.AppendLine();

                    }

                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine("\t\t\tcatch (Exception ex)");
                    sb.AppendLine("\t\t\t{");
                    if (ignoreFoundationServices == false)
                    {
                        sb.AppendLine("\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, \"" + module + "." + entity + " entity creation failed.\", false, " + CamelCase(entity) + ".id.ToString(), \"\", serializer.Serialize(" + CamelCase(entity) + "), ex);");
                    }
                    sb.AppendLine("\t\t\t\tthrow;");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();
                }
                else
                {

                    if (ignoreFoundationServices == true)
                    {
                        throw new Exception("Cannot use version control if ignoring foundation services.");

                    }

                    sb.AppendLine("\t\t\t\t" + CamelCase(entity) + ".versionNumber = 1;");
                    sb.AppendLine();

                    if (HasBinaryStorageFields(scriptGenTable) == true)
                    {
                        sb.AppendLine("\t\t\t\tbool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();");
                        sb.AppendLine();

                        sb.AppendLine(optionalTab + "\t\t\tbyte[] dataReferenceBeforeClearing = " + CamelCase(entity) + "." + dataRootFieldName + "Data;");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\tif (diskBasedBinaryStorageMode == true &&");
                        sb.AppendLine(optionalTab + "\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Data != null &&");
                        sb.AppendLine(optionalTab + "\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "FileName != null &&");

                        if (BinaryStorageFieldsAreNullable(scriptGenTable) == true)
                        {
                            sb.AppendLine(optionalTab + "\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size.HasValue == true &&");
                            sb.AppendLine(optionalTab + "\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size.Value > 0)");
                        }
                        else
                        {
                            sb.AppendLine(optionalTab + "\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Size > 0)");
                        }

                        sb.AppendLine(optionalTab + "\t\t\t{");

                        sb.AppendLine(optionalTab + "\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t    // write the bytes to disk");
                        sb.AppendLine(optionalTab + "\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t    await WriteDataToDiskAsync(" + CamelCase(entity) + ".objectGuid, " + CamelCase(entity) + ".versionNumber, " + CamelCase(entity) + "." + dataRootFieldName + "Data, \"" + dataFileNameExtension + "\");");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t    // Clear the data from the object before we put it into the db");
                        sb.AppendLine(optionalTab + "\t\t\t    //");
                        sb.AppendLine(optionalTab + "\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Data = null;");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t}");
                        sb.AppendLine();

                    }


                    sb.AppendLine("\t\t\t\tdb." + plural + ".Add(" + CamelCase(entity) + ");");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\tusing (var transaction = db.Database.BeginTransaction())");
                    sb.AppendLine("\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t    await db.SaveChangesAsync();");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t    //");
                    sb.AppendLine("\t\t\t\t    // Now add the change history");
                    sb.AppendLine("\t\t\t\t    //");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t    //");
                    sb.AppendLine("\t\t\t\t    // Detach the " + CamelCase(entity) + " object so that no further changes will be written to the database");
                    sb.AppendLine("\t\t\t\t    //");
                    sb.AppendLine("\t\t\t\t    db.Entry(" + CamelCase(entity) + ").State = EntityState.Detached;");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t    //");
                    sb.AppendLine("\t\t\t\t    // Nullify all object properties before serializing.");
                    sb.AppendLine("\t\t\t\t    //");

                    foreach (PropertyInfo prop in type.GetProperties())
                    {
                        Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                        // Is this an object type?
                        if (propertyType != typeof(string) &&
                            propertyType != typeof(int) &&
                            propertyType != typeof(long) &&
                            propertyType != typeof(DateTime) &&
                            propertyType != typeof(float) &&
                            propertyType != typeof(double) &&
                            propertyType != typeof(decimal) &&
                            propertyType != typeof(bool) &&
                            propertyType != typeof(Guid))
                        {
                            sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + "." + prop.Name + " = null;");
                        }
                    }

                    sb.AppendLine();

                    //sb.AppendLine("\t\t\t\t    string serialized" + entity + " = serializer.Serialize(" + CamelCase(entity) + ");");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t    " + entity + "ChangeHistory " + CamelCase(entity) + "ChangeHistory = new " + entity + "ChangeHistory();");
                    sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "ChangeHistory." + CamelCase(entity, false) + "Id = " + CamelCase(entity) + ".id;");
                    sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "ChangeHistory.versionNumber = " + CamelCase(entity) + ".versionNumber;");
                    sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "ChangeHistory.timeStamp = DateTime.UtcNow;");

                    //
                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "ChangeHistory.userId = user.id;");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "ChangeHistory.userId = securityUser.id;");
                    }

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine(optionalTab + "\t\t\t    " + CamelCase(entity) + "ChangeHistory.tenantGuid = userTenantGuid;");
                    }


                    sb.AppendLine($"\t\t\t\t    {CamelCase(entity)}ChangeHistory.data = serializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)}));");
                    sb.AppendLine("\t\t\t\t    db." + entity + "ChangeHistories.Add(" + CamelCase(entity) + "ChangeHistory);");
                    sb.AppendLine("\t\t\t\t    await db.SaveChangesAsync();");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t    transaction.Commit();");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\"" + module + "." + entity + " entity successfully created.\",");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "true,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + CamelCase(entity) + ". id.ToString(),");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\"\",");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tserializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)})),");
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
                        sb.AppendLine(optionalTab + "\t\t\t\t    " + CamelCase(entity) + "." + dataRootFieldName + "Data = dataReferenceBeforeClearing;");
                        sb.AppendLine(optionalTab + "\t\t\t\t}");
                        sb.AppendLine();

                    }
                    sb.AppendLine("\t\t\t\t}");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine("\t\t\tcatch (Exception ex)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine($"\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, \"{module}.{entity} entity creation failed.\", false, {CamelCase(entity)}.id.ToString(), \"\", serializer.Serialize({databaseNamespace}.{entity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)})), ex);");
                    sb.AppendLine("\t\t\t\tthrow;");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();
                }

                if (dataVisibilityEnabled == true && ignoreFoundationServices == false)
                {

                    sb.AppendLine("\t\t\t" + entity + "WithWritePermissionDetails " + CamelCase(entity) + "WWP = new " + entity + "WithWritePermissionDetails(" + CamelCase(entity) + ");");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tConfigureIsWriteable(" + CamelCase(entity) + "WWP, securityUser, userTenantGuid, userIsAdmin, true, userAndTheirReportIds, organizationIdsUserIsEntitledToWriteTo, departmentIdsUserIsEntitledToWriteTo, teamIdsUserIsEntitledToWriteTo);");
                    sb.AppendLine("\t\t\tConfigureOwnerIsChangeable(" + CamelCase(entity) + "WWP, securityUser, userTenantGuid, userIsAdmin, true, organizationIdsUserIsEntitledToChangeOwnerFor, departmentIdsUserIsEntitledToChangeOwnerFor, teamIdsUserIsEntitledToChangeOwnerFor);");
                    sb.AppendLine("\t\t\tConfigureHierarchyIsChangeable(" + CamelCase(entity) + "WWP, securityUser, userTenantGuid, userIsAdmin, true, organizationIdsUserIsEntitledToChangeHierarchyFor, departmentIdsUserIsEntitledToChangeHierarchyFor, teamIdsUserIsEntitledToChangeHierarchyFor);");
                    sb.AppendLine();

                    if (scriptGenTable != null && scriptGenTable.name.EndsWith("ChangeHistory") == false)
                    {
                        sb.AppendLine("\t\t\tBackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, \"" + entity + "\", " + CamelCase(entity) + ".id, " + CamelCase(entity) + "." + displayNameField + "));");
                        sb.AppendLine();
                    }
                    sb.AppendLine($"\t\t\treturn CreatedAtRoute(\"{pluralizeEntityForRouteForSomeTypeNames(entity)}\", new {{ id = {CamelCase(entity)}.id }}, {databaseNamespace}.{entity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)}WWP));");
                }
                else
                {
                    sb.AppendLine();
                    if (ignoreFoundationServices == false)
                    {
                        if (scriptGenTable != null && scriptGenTable.name.EndsWith("ChangeHistory") == false)
                        {
                            sb.AppendLine("\t\t\tBackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, \"" + entity + "\", " + CamelCase(entity) + ".id, " + CamelCase(entity) + "." + displayNameField + "));");
                            sb.AppendLine();
                        }
                    }

                    sb.AppendLine($"\t\t\treturn CreatedAtRoute(\"{pluralizeEntityForRouteForSomeTypeNames(entity)}\", new {{ id = {CamelCase(entity)}.id }}, {databaseNamespace}.{entity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)}));");
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
                    sb.AppendLine("\t\t[HttpPut]");
                    sb.AppendLine("\t\t[Route(\"api/" + entity + "/Rollback/{id}\")]");
                    sb.AppendLine("\t\t[Route(\"api/" + entity + "/Rollback\")]");
                    sb.AppendLine("\t\tpublic async Task<IHttpActionResult> RollbackTo" + entity + "Version(int id, int versionNumber)");
                    sb.AppendLine("\t\t{");
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// Data rollback is an admin only function, like Deletes.");
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\tStartAuditEventClock();");
                    sb.AppendLine("\t\t\t");
                    //sb.AppendLine("\t\t\tawait ThrowErrorIfUserHasNoAdminPrivilegeOrAccessAsync();");
                    sb.AppendLine("\t\t\tif (await DoesUserHaveAdminPrivilegeSecurityCheckAsync() == false)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t   return Unauthorized();");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// Need to figure out if this should be used here or not... For now it's not letting me test.");
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t//if (await IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel.Write) == false)");
                    sb.AppendLine("\t\t\t//{");
                    sb.AppendLine("\t\t\t//   return Unauthorized();");
                    sb.AppendLine("\t\t\t//}");
                    sb.AppendLine();

                    sb.AppendLine("\t\t\t");
                    sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync();");
                    sb.AppendLine("\t\t\t");
                    sb.AppendLine("\t\t\tbool userIsAdmin = await UserCanAdministerAsync(securityUser);");
                    sb.AppendLine("\t\t\tbool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser);");
                    sb.AppendLine();

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine(UserTenantGuidCommands(3));
                    }

                    sb.AppendLine("\t\t\t");

                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine("\t\t\tUser user = await GetUserAsync(userTenantGuid);");
                        sb.AppendLine("\t\t\t");
                        sb.AppendLine("\t\t\tif (user == null)");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t    // If we don't have a user record, then the data visibility sync is probably not working..");
                        sb.AppendLine("\t\t\t    await CreateAuditEventAsync(AuditEngine.AuditType.Error, \"Cannot proceed with " + entity + " rollback because Unable to find user record for security user with account name of \" + securityUser.accountName + \".Check the data visibilty sync process.\", false);");
                        sb.AppendLine("\t\t\t    throw new Exception(\"Unable to proceed with " + entity + " rollback because inconsistency with user record was found.\");");
                        sb.AppendLine("\t\t\t}");
                    }


                    sb.AppendLine();
                    sb.AppendLine("\t\t\tJavaScriptSerializer serializer = new JavaScriptSerializer();");
                    sb.AppendLine("\t\t\tserializer.MaxJsonLength = 100 * 1024 * 1024;       // 100 megabytes");
                    sb.AppendLine("\t\t\t");
                    sb.AppendLine("\t\t\t");
                    sb.AppendLine("\t\t\tIQueryable <" + qualifiedEntity + "> query = (from x in db." + Pluralize(entity));
                    sb.AppendLine("\t\t\t        where");
                    sb.AppendLine("\t\t\t        (x.id == id)");
                    sb.AppendLine("\t\t\t        select x);");

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine();
                        sb.AppendLine("\t\t\tquery = query.Where(Q => Q.tenantGuid == userTenantGuid);");
                    }

                    sb.AppendLine();
                    if (HasBinaryStorageFields(scriptGenTable) == true)
                    {
                        sb.AppendLine("\t\t\tbool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();");
                        sb.AppendLine();
                    }
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// Make sure nobody else is editing this " + entity + " concurrently");
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\tlock (" + CamelCase(entity) + "PutSyncRoot)");
                    sb.AppendLine("\t\t\t{");


                    sb.AppendLine("\t\t\t\t");
                    sb.AppendLine("\t\t\t\t" + qualifiedEntity + " " + CamelCase(entity) + " = query.FirstOrDefault();");
                    sb.AppendLine("\t\t\t\t");
                    sb.AppendLine("\t\t\t\tif (" + CamelCase(entity) + " == null)");
                    sb.AppendLine("\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, \"Invalid primary key provided for " + module + "." + entity + " rollback\", id.ToString(), new Exception(\"No " + module + "." + entity + " entity could be find with the primary key provided for the rollback operation.\"));");
                    sb.AppendLine("\t\t\t\t    return NotFound();");
                    sb.AppendLine("\t\t\t\t}");
                    sb.AppendLine("\t\t\t\t");
                    sb.AppendLine("\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t// Make a copy of the " + entity + " current state so we can log it.");
                    sb.AppendLine("\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t" + qualifiedEntity + " cloneOfExisting = (" + qualifiedEntity + ")db.Entry(" + CamelCase(entity) + ").GetDatabaseValues().ToObject();");
                    sb.AppendLine("\t\t\t\t");
                    sb.AppendLine("\t\t\t\t//");
                    sb.AppendLine("\t\t\t\t// Remove any object fields from the clone object so that it can serialize effectively");
                    sb.AppendLine("\t\t\t\t//");
                    foreach (PropertyInfo prop in type.GetProperties())
                    {
                        Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                        // Is this an object type?
                        if (propertyType != typeof(string) &&
                            propertyType != typeof(int) &&
                            propertyType != typeof(long) &&
                            propertyType != typeof(DateTime) &&
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

                    sb.AppendLine("\t\t\t\tif (versionNumber >= " + CamelCase(entity) + ".versionNumber)");
                    sb.AppendLine("\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, \"Invalid version number provided for " + module + "." + entity + " rollback.  Version number provided is \" + versionNumber, id.ToString(), new Exception(\"Invalid version number provided for " + module + "." + entity + " rollback operation.Version number provided is \" + versionNumber));");
                    sb.AppendLine("\t\t\t\t    return NotFound();");
                    sb.AppendLine("\t\t\t\t}");
                    sb.AppendLine("\t\t\t\t");
                    sb.AppendLine("\t\t\t\t" + entity + "ChangeHistory " + CamelCase(entity) + "ChangeHistory = (from x in db." + entity + "ChangeHistories");
                    sb.AppendLine("\t\t\t\t                                               where");
                    sb.AppendLine("\t\t\t\t                                               x." + CamelCase(entity, false) + "Id == id &&");

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
                    sb.AppendLine("\t\t\t\tif (" + CamelCase(entity) + "ChangeHistory != null)");
                    sb.AppendLine("\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t    dynamic old" + entity + " = JObject.Parse(" + CamelCase(entity) + "ChangeHistory.data);");
                    sb.AppendLine("\t\t\t\t");
                    sb.AppendLine("\t\t\t\t    //");
                    sb.AppendLine("\t\t\t\t    // Increase the version number");
                    sb.AppendLine("\t\t\t\t    //");
                    sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + ".versionNumber++;");
                    sb.AppendLine("\t\t\t\t");
                    sb.AppendLine("\t\t\t\t    //");
                    sb.AppendLine("\t\t\t\t    // Put all other fields back the way that they were ");
                    sb.AppendLine("\t\t\t\t    //");


                    foreach (PropertyInfo prop in type.GetProperties())
                    {
                        if (prop.Name != "versionNumber" &&     // this can't be assigned because we're changing it.
                            prop.Name != "id" &&                // no sense assigning this on top of itself.
                            prop.Name != "tenantGuid")         // no sense in assigning this again
                        {
                            //
                            // Intent here is to map all properties that are not object types
                            //
                            if (prop.PropertyType.FullName.StartsWith("System.") == true && prop.PropertyType.FullName.StartsWith("System.Collections") == false)
                            {
                                sb.AppendLine("\t\t\t\t    " + CamelCase(entity) + "." + prop.Name + " = old" + entity + "." + prop.Name + ";");
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
                        sb.AppendLine("\t\t\t\t    	Byte[] binaryData = LoadDataFromDisk(old" + entity + ".objectGuid, old" + entity + ".versionNumber, \"" + dataFileNameExtension + "\");");
                        sb.AppendLine();
                        sb.AppendLine("\t\t\t\t    	//");
                        sb.AppendLine("\t\t\t\t    	// Write out the data as the new version");
                        sb.AppendLine("\t\t\t\t    	//");
                        sb.AppendLine("\t\t\t\t    	WriteDataToDisk(" + CamelCase(entity) + ".objectGuid, " + CamelCase(entity) + ".versionNumber, binaryData, \"" + dataFileNameExtension + "\");");
                        sb.AppendLine("\t\t\t\t    }");
                    }

                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t    string serialized" + entity + " = serializer.Serialize(" + CamelCase(entity) + ");");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t    using (var transaction = db.Database.BeginTransaction())");
                    sb.AppendLine("\t\t\t\t    {");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t        db.SaveChanges();");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t        //");
                    sb.AppendLine("\t\t\t\t        // Now add the change history");
                    sb.AppendLine("\t\t\t\t        //");
                    sb.AppendLine("\t\t\t\t        " + entity + "ChangeHistory new" + entity + "ChangeHistory = new " + entity + "ChangeHistory();");
                    sb.AppendLine("\t\t\t\t        new" + entity + "ChangeHistory." + CamelCase(entity, false) + "Id = " + CamelCase(entity) + ".id;");
                    sb.AppendLine("\t\t\t\t        new" + entity + "ChangeHistory.versionNumber = " + CamelCase(entity) + ".versionNumber;");
                    sb.AppendLine("\t\t\t\t        new" + entity + "ChangeHistory.timeStamp = DateTime.UtcNow;");

                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine("\t\t\t\t        new" + entity + "ChangeHistory.userId = user.id;");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t\t        new" + entity + "ChangeHistory.userId = securityUser.id;");
                    }

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine("\t\t\t\t        new" + entity + "ChangeHistory.tenantGuid = userTenantGuid;");
                    }

                    sb.AppendLine($"\t\t\t\t        new{entity}ChangeHistory.data = serializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)}));");
                    sb.AppendLine("\t\t\t\t        db." + entity + "ChangeHistories.Add(new" + entity + "ChangeHistory);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t        db.SaveChanges();");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t        transaction.Commit();");
                    sb.AppendLine("\t\t\t\t    }");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\tCreateAuditEvent(AuditEngine.AuditType.UpdateEntity,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\"" + module + "." + entity + " rollback process successfully rolled back to version number \" + versionNumber,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "true,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "id.ToString(),");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tserializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tserializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)})),");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "null);");
                    sb.AppendLine();

                    sb.AppendLine();
                    sb.AppendLine($"\t\t\t\t    return Ok({databaseNamespace}.{entity}.CreateAnonymous({CamelCase(entity)}));");
                    sb.AppendLine("\t\t\t\t}");
                    sb.AppendLine("\t\t\t\telse");
                    sb.AppendLine("\t\t\t\t{");
                    sb.AppendLine("\t\t\t\t    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, \"Could not find version number provided for " + module + "." + entity + " rollback.  Version number provided is \" + versionNumber, id.ToString(), new Exception(\"Could not find version number provided for " + module + "." + entity + " rollback.  Version number provided is \" + versionNumber));");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t\t    return InternalServerError();");
                    sb.AppendLine("\t\t\t\t}");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine("\t\t}");
                    sb.AppendLine();
                }

                sb.AppendLine();


                #endregion

                #region HTTP_Delete_Handling

                if (scriptGenTable != null && scriptGenTable.webAPIDeleteToBeOverridden == true)
                {
                    // comment out this function, but write the code anyway.
                    sb.AppendLine("/* This function is expected to be overridden in a custom file");
                }
                sb.AppendLine("\t\t[HttpDelete]");
                sb.AppendLine("\t\t[Route(\"api/" + pluralizeEntityForRouteForSomeTypeNames(entity) + "/{id}\")]");
                sb.AppendLine("\t\t[Route(\"api/" + pluralizeEntityForRouteForSomeTypeNames(entity) + "\")]");
                sb.AppendLine("\t\tpublic async Task<IHttpActionResult> Delete" + pluralizeEntityForRouteForSomeTypeNames(entity) + "(int id)");
                sb.AppendLine("\t\t{");

                if (ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\tStartAuditEventClock();");
                    sb.AppendLine();
                    //sb.AppendLine("\t\t\tawait ThrowErrorIfUserHasNoAdminPrivilegeOrAccessAsync();");
                    sb.AppendLine("\t\t\tif (await DoesUserHaveAdminPrivilegeSecurityCheckAsync() == false)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t   return Unauthorized();");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();

                    sb.AppendLine("\t\t\tif (await IsEntityDataTokenValidAsync(TokenLogic.EntityDataTokenTrustLevel.Write) == false)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t   return Unauthorized();");
                    sb.AppendLine("\t\t\t}");

                    sb.AppendLine();
                    sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync();");

                    sb.AppendLine();
                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine("\t\t\tbool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync();");
                        sb.AppendLine("\t\t\t");
                        sb.AppendLine("\t\t\t");
                        sb.AppendLine(UserTenantGuidCommands(3));
                    }
                    sb.AppendLine("\t\t\tJavaScriptSerializer serializer = new JavaScriptSerializer();");
                    sb.AppendLine("\t\t\tserializer.MaxJsonLength = 100 * 1024 * 1024;       // 100 megabytes");
                    sb.AppendLine();
                }

                sb.AppendLine("\t\t\tIQueryable<" + qualifiedEntity + "> query = (from x in db." + plural);
                sb.AppendLine("\t\t\t\twhere");
                sb.AppendLine("\t\t\t\t(x.id == id)");
                sb.AppendLine("\t\t\t\tselect x);");
                sb.AppendLine();


                if (multiTenancyEnabled == true && ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\tquery = query.Where(Q => Q.tenantGuid == userTenantGuid);");
                }
                sb.AppendLine();
                sb.AppendLine("\t\t\t" + qualifiedEntity + " " + CamelCase(entity) + " = await query.FirstOrDefaultAsync();");


                sb.AppendLine();
                sb.AppendLine("\t\t\tif (" + CamelCase(entity) + " == null)");
                sb.AppendLine("\t\t\t{");

                if (ignoreFoundationServices == false)
                {
                    sb.AppendLine("\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, \"Invalid primary key provided for " + module + "." + entity + " DELETE\", id.ToString(), new Exception(\"No " + module + "." + entity + " entity could be find with the primary key provided.\"));");
                }
                sb.AppendLine("\t\t\t\treturn NotFound();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\t" + qualifiedEntity + " cloneOfExisting = (" + qualifiedEntity + ")db.Entry(" + CamelCase(entity) + ").GetDatabaseValues().ToObject();");
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
                        sb.AppendLine("\t\t\t\t" + CamelCase(entity) + ".deleted = true;");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t\tdb." + plural + ".Remove(" + CamelCase(entity) + ");");
                    }

                    sb.AppendLine("\t\t\t\tawait db.SaveChangesAsync();");

                    if (ignoreFoundationServices == false)
                    {
                        //sb.AppendLine("\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity, \"" + module + "." + entity + " entity successfully deleted.\", true, id.ToString(), serializer.Serialize(cloneOfExisting), serializer.Serialize(" + CamelCase(entity) + "), null);");
                        sb.AppendLine();
                        sb.AppendLine(optionalTab + "\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t\"" + module + "." + entity + " entity successfully deleted.\",");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "true,");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "id.ToString(),");
                        sb.AppendLine($"{optionalTab}\t\t\t\t\tserializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),");
                        sb.AppendLine($"{optionalTab}\t\t\t\t\tserializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)})),");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "null);");
                        sb.AppendLine();

                    }
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine("\t\t\tcatch (Exception ex)");
                    sb.AppendLine("\t\t\t{");
                    if (ignoreFoundationServices == false)
                    {
                        //sb.AppendLine("\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity, \"" + module + "." + entity + " entity delete failed.\", false, id.ToString(), serializer.Serialize(cloneOfExisting), serializer.Serialize(" + CamelCase(entity) + "), ex);");

                        sb.AppendLine(optionalTab + "\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t\"" + module + "." + entity + " entity delete failed.\",");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "false,");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "id.ToString(),");
                        sb.AppendLine($"{optionalTab}\t\t\t\t\tserializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),");
                        sb.AppendLine($"{optionalTab}\t\t\t\t\tserializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)})),");
                        sb.AppendLine(optionalTab + "\t\t\t\t\t" + "ex);");
                    }
                    sb.AppendLine("\t\t\t\tthrow;");
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
                        sb.AppendLine("\t\t\tUser user = await GetUserAsync(userTenantGuid);");
                        sb.AppendLine();
                        sb.AppendLine("\t\t\tif (user == null)");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t    // If we don't have a user record, then the data visibility sync is probably not working..");
                        sb.AppendLine("\t\t\t    CreateAuditEvent(AuditEngine.AuditType.Error, \"Cannot proceed with " + entity + " DELETE because Unable to find user record for security user with account name of \" + securityUser.accountName + \".Check the data visibilty sync process.\", false);");
                        sb.AppendLine("\t\t\t    throw new Exception(\"Unable to proceed with " + entity + " delete because inconsistency with user record was found.\");");
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine();
                    }

                    sb.AppendLine();
                    if (HasBinaryStorageFields(scriptGenTable) == true)
                    {
                        sb.AppendLine("\t\t\tbool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();");
                        sb.AppendLine();
                    }

                    sb.AppendLine("\t\t\tlock (" + CamelCase(entity) + "DeleteSyncRoot)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine("\t\t\t    try");
                    sb.AppendLine("\t\t\t    {");
                    sb.AppendLine("\t\t\t        " + CamelCase(entity) + ".deleted = true;");
                    sb.AppendLine("\t\t\t        " + CamelCase(entity) + ".versionNumber++;");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t        db.SaveChanges();");
                    sb.AppendLine();

                    if (HasBinaryStorageFields(scriptGenTable) == true)
                    {
                        sb.AppendLine("\t\t\t        //");
                        sb.AppendLine("\t\t\t        // If in disk based storage mode, create a copy of the disk data file for the new version.");
                        sb.AppendLine("\t\t\t        //");
                        sb.AppendLine("\t\t\t        if (diskBasedBinaryStorageMode == true)");
                        sb.AppendLine("\t\t\t        {");
                        sb.AppendLine("\t\t\t        	Byte[] binaryData = LoadDataFromDisk(" + CamelCase(entity) + ".objectGuid, " + CamelCase(entity) + ".versionNumber -1, \"" + dataFileNameExtension + "\");");
                        sb.AppendLine();
                        sb.AppendLine("\t\t\t        	//");
                        sb.AppendLine("\t\t\t        	// Write out the same data");
                        sb.AppendLine("\t\t\t        	//");
                        sb.AppendLine("\t\t\t        	WriteDataToDisk(" + CamelCase(entity) + ".objectGuid, " + CamelCase(entity) + ".versionNumber, binaryData, \"" + dataFileNameExtension + "\");");
                        sb.AppendLine("\t\t\t        }");
                        sb.AppendLine();
                    }

                    sb.AppendLine("\t\t\t        //");
                    sb.AppendLine("\t\t\t        // Now add the change history");
                    sb.AppendLine("\t\t\t        //");
                    sb.AppendLine("\t\t\t        " + entity + "ChangeHistory " + CamelCase(entity) + "ChangeHistory = new " + entity + "ChangeHistory();");
                    sb.AppendLine("\t\t\t        " + CamelCase(entity) + "ChangeHistory." + CamelCase(entity, false) + "Id = " + CamelCase(entity) + ".id;");
                    sb.AppendLine("\t\t\t        " + CamelCase(entity) + "ChangeHistory.versionNumber = " + CamelCase(entity) + ".versionNumber;");
                    sb.AppendLine("\t\t\t        " + CamelCase(entity) + "ChangeHistory.timeStamp = DateTime.UtcNow;");

                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine("\t\t\t        " + CamelCase(entity) + "ChangeHistory.userId = user.id;");
                    }
                    else
                    {
                        sb.AppendLine("\t\t\t        " + CamelCase(entity) + "ChangeHistory.userId = securityUser.id;");
                    }

                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine("\t\t\t        " + CamelCase(entity) + "ChangeHistory.tenantGuid = userTenantGuid;");
                    }

                    sb.AppendLine($"\t\t\t        {CamelCase(entity)}ChangeHistory.data = serializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)}));");
                    sb.AppendLine("\t\t\t        db." + entity + "ChangeHistories.Add(" + CamelCase(entity) + "ChangeHistory);");
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t        db.SaveChanges();");
                    //sb.AppendLine("\t\t\t        CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, \"" + module + "." + entity + " entity successfully deleted.\", true, id.ToString(), serializer.Serialize(cloneOfExisting), serialized" + entity + ", null);");
                    sb.AppendLine();
                    sb.AppendLine(optionalTab + "\t\t\t\tCreateAuditEvent(AuditEngine.AuditType.DeleteEntity,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\"" + module + "." + entity + " entity successfully deleted.\",");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "true,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "id.ToString(),");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tserializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tserializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)})),");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "null);");
                    sb.AppendLine();

                    sb.AppendLine("\t\t\t    }");
                    sb.AppendLine("\t\t\t    catch (Exception ex)");
                    sb.AppendLine("\t\t\t    {");

                    //sb.AppendLine("\t\t\t        CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, \"" + module + "." + entity + " entity delete failed.\", false, id.ToString(), serializer.Serialize(cloneOfExisting), serializer.Serialize(" + CamelCase(entity) + "), ex);");
                    sb.AppendLine(optionalTab + "\t\t\t\tCreateAuditEvent(AuditEngine.AuditType.DeleteEntity,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t\"" + module + "." + entity + " entity delete failed\",");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "false,");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "id.ToString(),");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tserializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),");
                    sb.AppendLine($"{optionalTab}\t\t\t\t\tserializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)})),");
                    sb.AppendLine(optionalTab + "\t\t\t\t\t" + "ex);");

                    sb.AppendLine("\t\t\t        throw;");
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
            sb.AppendLine("\t\t[Route(\"api/" + plural + "/ListData\")]");

            // special handler for the user table that has a self referencing field in the application databases.  Security module has similar issue, but that is solved with a custom override in that project.
            if (entity == "User" && scriptGenTable.HasField("reportsToUserId") == true)
            {
                sb.AppendLine("\t\t[Route(\"api/ReportsToUsers/ListData\")]");
                sb.AppendLine("\t\t[Route(\"api/CreatedByUsers/ListData\")]  // needed for notifications framework");          // needed for notifications framework
            }

            sb.AppendLine("\t\t[HttpGet]");
            sb.AppendLine("\t\tpublic async Task<IHttpActionResult> GetListData(");


            processingFirstProperty = true;
            foreach (PropertyInfo prop in type.GetProperties())
            {
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

                    // bool comes in as int - 1 or 0
                    sb.Append("\t\t\tint? " + prop.Name + " = null");
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

                    sb.Append("\t\tdouble? " + prop.Name + " = null");
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
            sb.AppendLine(@", 
            int pageSize = DEFAULT_NAME_VALUE_PAIR_LIST_PAGE_SIZE, 
            int pageNumber = 1)");



            sb.AppendLine("\t\t{");

            if (ignoreFoundationServices == false)
            {
                sb.AppendLine("\t\t\tif (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t   return Unauthorized();");
                sb.AppendLine("\t\t\t}");

                sb.AppendLine();
                sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync();");
                sb.AppendLine();
                sb.AppendLine("\t\t\tbool userIsAdmin = await UserCanAdministerAsync(securityUser);");
                sb.AppendLine("\t\t\tbool userIsWriter = await UserCanWriteAsync(securityUser, " + entityMinimumWritePermissionString + ");");
                sb.AppendLine();
                sb.AppendLine("\t\t\tbool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser);");

                if (multiTenancyEnabled == true)
                {
                    sb.AppendLine();
                    sb.AppendLine(UserTenantGuidCommands(3));
                }
            }

            sb.AppendLine();

            sb.AppendLine("\t\t\tif (pageNumber < 1)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\tpageNumber = 1;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();
            sb.AppendLine("\t\t\tif (pageSize == 0)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\tpageSize = int.MaxValue;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t\telse if (pageSize < 0)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\tpageSize = DEFAULT_ALL_DATA_LIST_PAGE_SIZE;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();


            //sb.AppendLine("\t\t\tIQueryable<" + qualifiedEntity + "> query = (from x in db." + plural);


            commentWritten = false;
            foreach (PropertyInfo prop in type.GetProperties())
            {
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
            sb.AppendLine("\t\t\tvar query = (from " + acronym + " in db." + plural + " select " + acronym + ");");

            if (ignoreFoundationServices == false)
            {
                if (dataVisibilityEnabled == true)
                {
                    sb.AppendLine();
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// Apply the constraints for the user's tenant and data visibility configuration.");
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\tquery = await AddTenantAndDataVisibilityConstraintsAsync(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin);");
                    sb.AppendLine();
                }
                else if (multiTenancyEnabled == true)
                {
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tquery = query.Where(Q => Q.tenantGuid == userTenantGuid);");
                    sb.AppendLine();
                }
            }

            processingFirstProperty = true;
            foreach (PropertyInfo prop in type.GetProperties())
            {
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
                        // Only writers and admins can see inactive records, or filter by active.  Only admins can see deleted reocrds, or filter by deleted.
                        sb.AppendLine("\t\t\tif (userIsWriter == true)");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t\tif (active.HasValue == true)");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".active == (active.Value == 1 ? true : false));");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine("\t\t\t");
                        sb.AppendLine("\t\t\t\tif (userIsAdmin == true)");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t\tif (deleted.HasValue == true)");
                        sb.AppendLine("\t\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".deleted == (deleted.Value == 1 ? true : false));");
                        sb.AppendLine("\t\t\t\t\t}");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine("\t\t\t\telse");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".deleted == false);");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine("\t\t\telse");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".active == true);");
                        sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + ".deleted == false);");
                        sb.AppendLine("\t\t\t}");

                    }
                    else if (prop.Name == "deleted")
                    {
                        //
                        // Deleted handled in active handler.
                        //
                    }
                    else
                    {
                        sb.AppendLine("\t\t\tif (" + prop.Name + ".HasValue == true)");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine("\t\t\t\tquery = query.Where(" + acronym + " => " + acronym + "." + prop.Name + " == (" + prop.Name + ".Value == 1 ? true : false));");
                        sb.AppendLine("\t\t\t}");
                    }
                }
            }

            sb.AppendLine();


            if (ignoreFoundationServices == false)
            {
                if (dataVisibilityEnabled == true)
                {
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// Apply the constraints for the user's tenant and data visibility configuration.");
                    sb.AppendLine("\t\t\t//");

                    sb.AppendLine("\t\t\tquery = await AddTenantAndDataVisibilityConstraintsAsync(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin);");
                }
                else if (multiTenancyEnabled == true)
                {
                    sb.AppendLine();
                    sb.AppendLine("\t\t\tquery = query.Where(Q => Q.tenantGuid == userTenantGuid);");
                    sb.AppendLine();
                }
                sb.AppendLine();
            }

            // Use the sort sequence defined on the script generation table object, or create them if missing
            //
            bool firstWritten = false;
            foreach (var ss in scriptGenTable.GetOrGenerateSortSequences())
            {
                if (firstWritten == false)
                {
                    sb.Append("\t\t\tquery = query.OrderBy" + (ss.descending == true ? "Descending " : "") + "(Q => Q." + ss.field.name + ")");
                    firstWritten = true;
                }
                else
                {
                    sb.Append(".ThenBy" + (ss.descending == true ? "Descending " : "") + "(Q => Q." + ss.field.name + ")");
                }
            }

            if (firstWritten == true)
            {
                sb.AppendLine(";");
            }

            sb.AppendLine();

            //
            // Can't do this the easy way in Framework.4.8
            //
            //sb.AppendLine("\t\t\treturn Ok(await (from x in query select " + entity + ".CreateMinimalAnonymous(x)).ToListAsync());");  This is the easy way.


            //
            // Have to use 2 part process here instead
            //
            /*
             * 

			List<Project> projectList = await (from x in query select x).ToListAsync();

			return Ok((from x in projectList select Project.CreateMinimalAnonymous(x)).ToList());
             
             
             * */
            sb.AppendLine("\t\t\tList<" + entity + "> " + CamelCase(entity) + "List = await (from x in query select x).ToListAsync();");
            sb.AppendLine();
            sb.AppendLine($"\t\t\treturn Ok((from x in {CamelCase(entity)}List select {databaseNamespace}.{entity}.CreateMinimalAnonymous(x)).ToList());");
            sb.AppendLine();

            sb.AppendLine("\t\t}");

            if (scriptGenTable != null && scriptGenTable.webAPIGetListDataToBeOverridden == true)
            {
                sb.AppendLine("*/");
            }

            sb.AppendLine();

            #endregion

            #region Audit_Event_Creation_Post_Handling

            sb.AppendLine("\t\t[HttpPost]");
            sb.AppendLine("\t\t[Route(\"api/" + pluralizeEntityForRouteForSomeTypeNames(entity) + "/CreateAuditEvent\")]");
            sb.AppendLine("\t\tpublic async Task<IHttpActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null)");
            sb.AppendLine("\t\t{");

            sb.AppendLine("\t\t\tif (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t   return Unauthorized();");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();
            sb.AppendLine("\t\t    await CreateAuditEventAsync(type, message, primaryKey);");
            sb.AppendLine();
            sb.AppendLine("\t\t    return Ok();");
            sb.AppendLine("\t\t}");




            sb.AppendLine();

            #endregion

            #region Favourite_Handling

            if (canBeFavourited == true && ignoreFoundationServices == false)
            {
                sb.AppendLine();


                sb.AppendLine("\t\t[Route(\"api/" + pluralizeEntityForRouteForSomeTypeNames(entity) + "/Favourite/{id}\")]");
                sb.AppendLine("\t\t[HttpPut]");
                sb.AppendLine("\t\t//[HttpGet]       // For ease of testing, remove these comments as helpful.");
                sb.AppendLine("\t\tpublic async Task<IHttpActionResult> SetFavourite(int id, string description = null)");
                sb.AppendLine("\t\t{");

                sb.AppendLine("\t\t\tif (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t   return Unauthorized();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();

                sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync();");
                sb.AppendLine();
                sb.AppendLine("\t\t\tbool userIsAdmin = await UserCanAdministerAsync();");
                sb.AppendLine("\t\t\tbool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync();");
                sb.AppendLine();

                sb.AppendLine(UserTenantGuidCommands(3));

                sb.AppendLine();
                sb.AppendLine("\t\t\tIQueryable<" + qualifiedEntity + "> query = (from x in db." + plural + "");
                sb.AppendLine("\t\t\t                               where x.id == id");
                sb.AppendLine("\t\t\t                               select x);");
                sb.AppendLine();

                if (dataVisibilityEnabled == true)
                {
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\t// Apply the constraints for the user's tenant and data visibility configuration.");
                    sb.AppendLine("\t\t\t//");
                    sb.AppendLine("\t\t\tquery = await AddTenantAndDataVisibilityConstraintsAsync(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin);");
                }
                sb.AppendLine();

                sb.AppendLine("\t\t\t" + qualifiedEntity + " " + CamelCase(entity) + " = await query.AsNoTracking().FirstOrDefaultAsync();");
                sb.AppendLine();
                sb.AppendLine("\t\t\tif (" + CamelCase(entity) + " != null)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\tif (string.IsNullOrEmpty(description) == true)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\tdescription = " + CamelCase(entity) + "." + displayNameField + ";");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t// Add the user favourite " + entity);
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\tawait SecurityLogic.AddToUserFavouritesAsync(securityUser, \"" + entity + "\", id, description);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, \"Favourite '" + entity + "' was added for record with id of \" + id + \" for user \" + securityUser.accountName, true);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t// Return the complete list of user favourites after the addition");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\treturn Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser));");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\telse");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, \"Favourite '" + entity + "' add request was made with an invalid id value of \" + id, false);");
                sb.AppendLine("\t\t\t\treturn BadRequest();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t}");
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("\t\t[Route(\"api/" + pluralizeEntityForRouteForSomeTypeNames(entity) + "/Favourite/{id}\")]");
                sb.AppendLine("\t\t[HttpDelete]");
                sb.AppendLine("\t\tpublic async Task<IHttpActionResult> DeleteFavourite(int id)");
                sb.AppendLine("\t\t{");
                sb.AppendLine("\t\t\tif (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t   return Unauthorized();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();

                sb.AppendLine("\t\t\tSecurityUser securityUser = await GetSecurityUserAsync();");
                sb.AppendLine();

                sb.AppendLine(UserTenantGuidCommands(3));

                sb.AppendLine();
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\t// Delete the user favourite " + entity);
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\tawait SecurityLogic.RemoveFromUserFavouritesAsync(securityUser, \"" + entity + "\", id);");
                sb.AppendLine();
                sb.AppendLine("\t\t\tawait CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous, \"Favourite '" + entity + "' was removed for record with id of \" + id + \" for user \" + securityUser.accountName, true);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\t// Return the complete list of user favourites after the deletion");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\treturn Ok(await SecurityLogic.GetUserFavouritesAsync(securityUser));");
                sb.AppendLine("\t\t}");
                sb.AppendLine();
            }

            sb.AppendLine();

            #endregion

            #region Tenant_and_Visibilty_contraints

            if (dataVisibilityEnabled == true && ignoreFoundationServices == false)
            {
                sb.AppendLine("\t\tprivate async Task<IQueryable<" + qualifiedEntity + ">> AddTenantAndDataVisibilityConstraintsAsync(IQueryable<" + qualifiedEntity + "> query, SecurityUser securityUser, Guid userTenantGuid, bool userIsSecurityAdmin, bool userIsAdmin)");
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
                sb.AppendLine("\t\t\t\tList<int> organizationsUserIsEntitledToReadFrom = await GetOrganizationIdsUserIsEntitledToReadFromAsync(securityUser);");
                sb.AppendLine("\t\t\t\tList<int> departmentsUserIsEntitledToReadFrom = await GetDepartmentIdsUserIsEntitledToReadFromAsync(securityUser);");
                sb.AppendLine("\t\t\t\tList<int> teamsUserIsEntitledToReadFrom = await GetTeamIdsUserIsEntitledToReadFromAsync(securityUser);");
                sb.AppendLine("\t\t\t\tList<int> userAndTheirReportIds = await GetListOfUserIdsForUserAndPeopleThatReportToThemAsync(securityUser);");
                sb.AppendLine();
                sb.AppendLine("\t\t\t\tList<int> organizationsThatUserInheritsReadFrom = await GetOrganizationIdsUserIsEntitledToReadImplicitlyFromForNullDepartmentAndTeamValuesAsync(securityUser);");
                sb.AppendLine("\t\t\t\tList<int> departmentsThatUserInheritsReadFrom = await GetDepartmentsIdsUserIsEntitledToReadImplicitlyFromForNullTeamValuesAsync(securityUser);");
                sb.AppendLine();


                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t// Build the data visibility condition based on the state of the user's organization, department, and team entitlement, as well as their own and that of people that report to them.");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t// If " + entity + " has no data visibility attributes then return it.  Otherwise, user must match on one or more of organization, department, team, or user to see this " + entity + "");
                sb.AppendLine("\t\t\t\t//");

                // This is the new way that supports implicit dep and org readership and is resequenced for hopefully better performance
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
                sb.AppendLine("\t\t\t\t// If " + entity + " has no data visibility attributes then return it.  Otherwise, user must match on one or more of organization, department, team, or user to see this " + entity + "");
                sb.AppendLine("\t\t\t\t//");

                // This is the new way that supports implicit dep and org readership and is resequenced for hopefully better performance
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
                sb.AppendLine("\t\tprivate static void ConfigureIsWriteable(" + entity + "WithWritePermissionDetails " + CamelCase(entity) + "WWP, SecurityUser securityUser, Guid userTenantGuid, bool userIsAdmin, bool userIsWriter, List<int> userAndTheirReportIds, List<int> organizationsUserIsEntitledToWriteTo, List<int> departmentsUserIsEntitledToWriteTo, List<int> teamsUserIsEntitledToWriteTo)");
                sb.AppendLine("\t\t{");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\t// Determine if the " + entity + " can be written to or not.  If it can't, then indicate the reason.");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\tif (userIsAdmin == true)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\t" + CamelCase(entity) + "WWP.isWriteable = true;");
                sb.AppendLine("\t\t\t\t" + CamelCase(entity) + "WWP.notWriteableReason = null;");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\telse if (userIsWriter == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\t" + CamelCase(entity) + "WWP.isWriteable = false;");
                sb.AppendLine("\t\t\t\t" + CamelCase(entity) + "WWP.notWriteableReason = \"Cannot write to module.\";");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\telse if (userIsAdmin == false)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t// Non-Admin users may not be able to write to some records.");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t// Start checking the data visibility fields for the ability to write starting at the organization level, and moving down");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t//		- Ownership of the record either directly by the user, or by one of their repoorts.");
                sb.AppendLine("\t\t\t\t//		- Write permission at Organization implies ability to write all all of the organization's department and team levels");
                sb.AppendLine("\t\t\t\t//		- Write permission at Department implies ability to write to all of the department's teams");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t// First, check if the " + entity + " is writeable in the first 4 conditions, and if nothing matches there, then work the other way to find the reason why it's not writeable.");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\tif (" + CamelCase(entity) + "WWP.userId.HasValue == true && userAndTheirReportIds != null && userAndTheirReportIds.Contains(" + CamelCase(entity) + "WWP.userId.Value) == true)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t// " + entity + " is owned by user or one of their reports, so this record can be written to.");
                sb.AppendLine("\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + "WWP.isWriteable = true;");
                sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + "WWP.notWriteableReason = null;");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine("\t\t\t\telse if (" + CamelCase(entity) + "WWP.organizationId.HasValue == true && organizationsUserIsEntitledToWriteTo != null && organizationsUserIsEntitledToWriteTo.Contains(" + CamelCase(entity) + "WWP.organizationId.Value) == true)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t// " + entity + "'s Organization is writeable, so this record can be written to.");
                sb.AppendLine("\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + "WWP.isWriteable = true;");
                sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + "WWP.notWriteableReason = null;");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine("\t\t\t\telse if (" + CamelCase(entity) + "WWP.departmentId.HasValue == true && departmentsUserIsEntitledToWriteTo != null && departmentsUserIsEntitledToWriteTo.Contains(" + CamelCase(entity) + "WWP.departmentId.Value) == true)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t// " + entity + "'s Department is writeable, so this record can be written to.");
                sb.AppendLine("\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + "WWP.isWriteable = true;");
                sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + "WWP.notWriteableReason = null;");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine("\t\t\t\telse if (" + CamelCase(entity) + "WWP.teamId.HasValue == true && teamsUserIsEntitledToWriteTo != null && teamsUserIsEntitledToWriteTo.Contains(" + CamelCase(entity) + "WWP.teamId.Value) == true)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t// " + entity + "'s Team is writeable, so this record can be written to.");
                sb.AppendLine("\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + "WWP.isWriteable = true;");
                sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + "WWP.notWriteableReason = null;");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t// Now check if the user can't write, and if so, cite the reason");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\telse if (" + CamelCase(entity) + "WWP.teamId.HasValue == true && teamsUserIsEntitledToWriteTo != null && teamsUserIsEntitledToWriteTo.Contains(" + CamelCase(entity) + "WWP.teamId.Value) == false)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + "WWP.isWriteable = false;");
                sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + "WWP.notWriteableReason = \"Team with id of \" + " + CamelCase(entity) + "WWP.teamId.Value + \" is not writeable.\";");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine("\t\t\t\telse if (" + CamelCase(entity) + "WWP.departmentId.HasValue == true && departmentsUserIsEntitledToWriteTo != null && departmentsUserIsEntitledToWriteTo.Contains(" + CamelCase(entity) + "WWP.departmentId.Value) == false)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + "WWP.isWriteable = false;");
                sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + "WWP.notWriteableReason = \"Department with id of \" + " + CamelCase(entity) + "WWP.departmentId.Value + \" is not writeable.\";");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine("\t\t\t\telse if (" + CamelCase(entity) + "WWP.organizationId.HasValue == true && organizationsUserIsEntitledToWriteTo != null && organizationsUserIsEntitledToWriteTo.Contains(" + CamelCase(entity) + "WWP.organizationId.Value) == false)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + "WWP.isWriteable = false;");
                sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + "WWP.notWriteableReason = \"Organization with id of \" + " + CamelCase(entity) + "WWP.organizationId.Value + \" is not writeable.\";");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine("\t\t\t\telse");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine("\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t// Default to not being able to write if no other condition was met.");
                sb.AppendLine("\t\t\t\t\t//");
                sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + "WWP.isWriteable = false;");
                sb.AppendLine("\t\t\t\t\t" + CamelCase(entity) + "WWP.notWriteableReason = \"No reason found to grant the ability to write.\";");
                sb.AppendLine("\t\t\t\t}");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\telse");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t// Admin users can always write.");
                sb.AppendLine("\t\t\t\t//");
                sb.AppendLine("\t\t\t\t" + CamelCase(entity) + "WWP.isWriteable = true;");
                sb.AppendLine("\t\t\t\t" + CamelCase(entity) + "WWP.notWriteableReason = null;");
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
                    sb.AppendLine(UserTenantGuidCommands(3, false));
                    sb.AppendLine();
                    sb.AppendLine("			IQueryable<" + qualifiedEntity + "> query = (from x in db." + Pluralize(entity));
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
                    sb.AppendLine("				throw new Exception(\"Unable to read " + entity + " with id of \" + id);");
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
                    sb.AppendLine(UserTenantGuidCommands(3, false));
                    sb.AppendLine("            IQueryable<" + qualifiedEntity + "> query = (from x in db." + Pluralize(entity));
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
                    sb.AppendLine("                throw new Exception(\"Unable to read " + entity + " with id of \" + id);");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                    sb.AppendLine("            //");
                    sb.AppendLine("            // User can read if we get here.");
                    sb.AppendLine("            //");
                    sb.AppendLine("            return;");
                    sb.AppendLine("        }");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("		private async Task<bool> ThrowErrorIfUserCannotWriteToIdAsync(SecurityUser securityUser, int id)");
                    sb.AppendLine("		{");
                    sb.AppendLine("			//");
                    sb.AppendLine("			// This will verify the provided user's entitlement to write to the provided id.  If they cannot do it, an error will be thrown.");
                    sb.AppendLine("			//");
                    sb.AppendLine();
                    sb.AppendLine("			//");
                    sb.AppendLine("			// Admins can write to all rows.");
                    sb.AppendLine("			//");
                    sb.AppendLine("			bool userIsAdmin = await UserCanAdministerAsync(securityUser);");
                    sb.AppendLine("			if (userIsAdmin == true)");
                    sb.AppendLine("			{");
                    sb.AppendLine("				return true;");
                    sb.AppendLine("			}");
                    sb.AppendLine();
                    sb.AppendLine("			//");
                    sb.AppendLine("			// Make sure that the user can write to the entity");
                    sb.AppendLine("			//");
                    sb.AppendLine("			bool userIsWriter = await UserCanWriteAsync(WRITE_PERMISSION_LEVEL_REQUIRED);");
                    sb.AppendLine("			if (userIsWriter == false)");
                    sb.AppendLine("			{");
                    sb.AppendLine("				throw new Exception(\"User cannot write to this entity.\");");
                    sb.AppendLine("			}");
                    sb.AppendLine();
                    sb.AppendLine(UserTenantGuidCommands(3, false));
                    sb.AppendLine();
                    sb.AppendLine("			IQueryable<" + qualifiedEntity + "> query = (from x in db." + Pluralize(entity) + "");
                    sb.AppendLine("												   where");
                    sb.AppendLine("												   (x.id == id)");
                    sb.AppendLine("												   select x);");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("			//");
                    sb.AppendLine("			// Apply the constraints for the user's tenant and data visibility configuration to ensure that this user can load the record that is attempting to be saved.");
                    sb.AppendLine("			//");
                    sb.AppendLine("			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser);");
                    sb.AppendLine("			query = await AddTenantAndDataVisibilityConstraintsAsync(query, securityUser, userTenantGuid, userIsSecurityAdmin, userIsAdmin);");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("			" + qualifiedEntity + " existing = await query.FirstOrDefaultAsync();");
                    sb.AppendLine();
                    sb.AppendLine("			if (existing == null)");
                    sb.AppendLine("			{");
                    sb.AppendLine("				throw new Exception(\"Unable to find " + entity + " with id of \" + id);");
                    sb.AppendLine("			}");
                    sb.AppendLine();
                    sb.AppendLine("			List<int> organizationIdsUserIsEntitledToWriteTo = await GetOrganizationIdsUserIsEntitledToWriteToAsync(securityUser);");
                    sb.AppendLine("			List<int> departmentIdsUserIsEntitledToWriteTo = await GetDepartmentIdsUserIsEntitledToWriteToAsync (securityUser);");
                    sb.AppendLine("			List<int> teamIdsUserIsEntitledToWriteTo = await GetTeamIdsUserIsEntitledToWriteToAsync (securityUser);");
                    sb.AppendLine("			List<int> userAndTheirReportIds = await GetListOfUserIdsForUserAndPeopleThatReportToThemAsync (securityUser);");
                    sb.AppendLine();
                    sb.AppendLine("			Team team = null;");
                    sb.AppendLine("			Department department = null;");
                    sb.AppendLine("			Organization organization = null;");
                    sb.AppendLine("			User owner = null;");
                    sb.AppendLine();
                    sb.AppendLine("			if (existing.teamId.HasValue == true)");
                    sb.AppendLine("			{");
                    sb.AppendLine("				team = await GetTeamAsync(existing.teamId.Value);");
                    sb.AppendLine("			}");
                    sb.AppendLine();
                    sb.AppendLine("			if (existing.departmentId.HasValue == true)");
                    sb.AppendLine("			{");
                    sb.AppendLine("				department = await GetDepartmentAsync(existing.departmentId.Value);");
                    sb.AppendLine("			}");
                    sb.AppendLine("			else");
                    sb.AppendLine("			{");
                    sb.AppendLine("				//");
                    sb.AppendLine("				// Set the existing department to be that of the existing team if no existing department is explicitly provided and we have an existing team.");
                    sb.AppendLine("				//");
                    sb.AppendLine("				if (team != null)");
                    sb.AppendLine("				{");
                    sb.AppendLine("					department = await GetDepartmentAsync(team.departmentId);");
                    sb.AppendLine("				}");
                    sb.AppendLine("			}");
                    sb.AppendLine();
                    sb.AppendLine("			if (existing.organizationId.HasValue == true)");
                    sb.AppendLine("			{");
                    sb.AppendLine("				organization = await GetOrganizationAsync(existing.organizationId.Value);");
                    sb.AppendLine("			}");
                    sb.AppendLine("			else");
                    sb.AppendLine("			{");
                    sb.AppendLine("				//");
                    sb.AppendLine("				// Set the organization to be that of the department if no organization is explicitly provided and we have a department");
                    sb.AppendLine("				//");
                    sb.AppendLine("				if (department != null)");
                    sb.AppendLine("				{");
                    sb.AppendLine("					organization = await GetOrganizationAsync(department.organizationId);");
                    sb.AppendLine("				}");
                    sb.AppendLine("			}");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("			if (existing.userId != null)");
                    sb.AppendLine("			{");
                    sb.AppendLine("				owner = await GetUserAsync(existing.userId.Value);");
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

                    sb.AppendLine(UserTenantGuidCommands(3, false));
                    sb.AppendLine("            IQueryable<" + qualifiedEntity + "> query = (from x in db." + Pluralize(entity));
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
                    sb.AppendLine("                throw new Exception(\"Unable to find " + entity + " with id of \" + id);");
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
                    sb.AppendLine(UserTenantGuidCommands(3, false));
                    sb.AppendLine("            IQueryable<" + qualifiedEntity + "> query = (from x in db." + Pluralize(entity));
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
                    sb.AppendLine("                throw new Exception(\"Unable to find " + entity + " with id of \" + id);");
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
                    sb.AppendLine(UserTenantGuidCommands(3, false));
                    sb.AppendLine("            IQueryable<" + qualifiedEntity + "> query = (from x in db." + Pluralize(entity));
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
                    sb.AppendLine("                throw new Exception(\"Unable to find " + entity + " with id of \" + id);");
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
                    // <EntityName>'s Organization is hierarchy changeable, so this record can have its hiearchy changed
                    //
                    <ObjectName>WWP.canChangeHierarchy = true;
                }
                else if (<ObjectName>WWP.departmentId.HasValue == true && departmentsUserIsEntitledToChangeHierarchyFor != null && departmentsUserIsEntitledToChangeHierarchyFor.Contains(<ObjectName>WWP.departmentId.Value) == true)
                {
                    //
                    // <EntityName>'s Department is hierarchy changeable, so this record can have its hiearchy changed.
                    //
                    <ObjectName>WWP.canChangeHierarchy = true;
                }
                else if (<ObjectName>WWP.teamId.HasValue == true && teamsUserIsEntitledToChangeHierarchyFor != null && teamsUserIsEntitledToChangeHierarchyFor.Contains(<ObjectName>WWP.teamId.Value) == true)
                {
                    //
                    // <EntityName>'s Team is hierarchy changeable, so this record can have its hiearchy changed.
                    //
                    <ObjectName>WWP.canChangeHierarchy = true;
                }
                //
                // Now check if the user can't change hiearchy, and if so, cite the reason
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
                        <ObjectName>WWP.notWriteableReason = ""Organization with id of "" + <ObjectName>WWP.organizationId.Value + "" is not hiearchy changeable.  "";
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
        }".Replace("<EntityName>", entity).Replace("<ObjectName>", CamelCase(entity)));

            }

            #endregion

            #region Disposal_handling

            if (dataVisibilityEnabled == false)
            {
                sb.AppendLine();
                sb.AppendLine("\t\tprotected override void Dispose(bool disposing)");
                sb.AppendLine("\t\t{");
                sb.AppendLine("\t\t\tif (disposing)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\tdb.Dispose();");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\tbase.Dispose(disposing);");
                sb.AppendLine("\t\t}");
            }

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
                sb.AppendLine("        [HttpPost]");
                sb.AppendLine("        [HttpPut]");
                sb.AppendLine("        [Route(\"api/" + pluralizeEntityForRouteForSomeTypeNames(entity) + "/Data/{id:int}\")]");
                sb.AppendLine("        public async Task<IHttpActionResult> UploadData(int id)");
                sb.AppendLine("        {");
                sb.AppendLine("            await ThrowErrorIfUserHasNoWritePrivilegeOrAccessAsync(WRITE_PERMISSION_LEVEL_REQUIRED);");
                sb.AppendLine();
                sb.AppendLine("            SecurityUser securityUser = await GetSecurityUserAsync();");

                if (dataVisibilityEnabled == true)
                {
                    sb.AppendLine();
                    sb.AppendLine("            await ThrowErrorIfUserCannotWriteToIdAsync(securityUser, id);");
                    sb.AppendLine();
                }
                sb.AppendLine("            if (!Request.Content.IsMimeMultipartContent())");
                sb.AppendLine("            {");
                sb.AppendLine("                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);");
                sb.AppendLine("            }");
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("            var provider = new MultipartMemoryStreamProvider();");
                sb.AppendLine("            await Request.Content.ReadAsMultipartAsync(provider);");
                sb.AppendLine();
                sb.AppendLine("            bool foundFileData = false;");
                sb.AppendLine();
                sb.AppendLine("            //");
                sb.AppendLine("            // We only support one file, so use the first file in the contents array, and stop looking after that.");
                sb.AppendLine("            //");
                sb.AppendLine("            try");
                sb.AppendLine("            {");
                sb.AppendLine("                foreach (var file in provider.Contents)");
                sb.AppendLine("                {");
                sb.AppendLine("                    if (file.Headers.ContentDisposition.FileName != null)");
                sb.AppendLine("                    {");
                sb.AppendLine("                        foundFileData = true;");
                sb.AppendLine("                        string fileName = file.Headers.ContentDisposition.FileName.Trim('\\\"');");
                sb.AppendLine();
                sb.AppendLine("                        // default the mime type to be the one for arbitrary binary data unless we have a mime type on the content headers that tells us otherwise.");
                sb.AppendLine("                        string mimeType = \"application/octet-stream\";");
                sb.AppendLine("                        if (file.Headers.ContentType != null && file.Headers.ContentType.MediaType != null)");
                sb.AppendLine("                        {");
                sb.AppendLine("                            mimeType = file.Headers.ContentType.MediaType;");
                sb.AppendLine("                        }");
                sb.AppendLine();
                sb.AppendLine("                        Byte[] buffer = await file.ReadAsByteArrayAsync();");
                sb.AppendLine();

                if (versionControlEnabled == false)
                {
                    sb.AppendLine("                        " + qualifiedEntity + " " + CamelCase(entity) + " = await (from x in db." + Pluralize(entity) + " where x.id == id && x.active == true && x.deleted == false select x).FirstOrDefaultAsync();");
                    sb.AppendLine("                        if (" + CamelCase(entity) + " == null)");
                    sb.AppendLine("                        {");
                    sb.AppendLine("                            return NotFound();");
                    sb.AppendLine("                        }");
                    sb.AppendLine();
                    sb.AppendLine("                        bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();");
                    sb.AppendLine();
                    sb.AppendLine("                        try");
                    sb.AppendLine("                        {");

                    sb.AppendLine("                            ");
                    sb.AppendLine("                            " + CamelCase(entity) + "." + dataRootFieldName + "FileName = fileName.Trim();");
                    sb.AppendLine("                            " + CamelCase(entity) + "." + dataRootFieldName + "MimeType = mimeType;");
                    sb.AppendLine("                            " + CamelCase(entity) + "." + dataRootFieldName + "Size = buffer.Length;");

                    sb.AppendLine("                            if (diskBasedBinaryStorageMode == true &&");


                    sb.AppendLine("                            " + CamelCase(entity) + "." + dataRootFieldName + "FileName != null &&");
                    sb.AppendLine("                            " + CamelCase(entity) + "." + dataRootFieldName + "Size > 0)");
                    sb.AppendLine("                            {");
                    sb.AppendLine("                            	//");
                    sb.AppendLine("                            	// write the bytes to disk");
                    sb.AppendLine("                            	//");
                    sb.AppendLine("                            	WriteDataToDisk(" + CamelCase(entity) + ".objectGuid, 0, buffer, \"" + dataFileNameExtension + "\");");

                    sb.AppendLine("                            	//");
                    sb.AppendLine("                            	// Clear the data from the object before we put it into the db");
                    sb.AppendLine("                            	//");
                    sb.AppendLine("                             " + CamelCase(entity) + "." + dataRootFieldName + "Data = null;");
                    sb.AppendLine("                            }");
                    sb.AppendLine("                            else");
                    sb.AppendLine("                            {");
                    sb.AppendLine("                            	" + CamelCase(entity) + "." + dataRootFieldName + "Data = buffer;");
                    sb.AppendLine("                            } ");


                    sb.AppendLine("                            ");
                    sb.AppendLine("                            await db.SaveChangesAsync();");
                    sb.AppendLine("                            ");
                    sb.AppendLine("                            CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, \"" + entity + " Data Uploaded with filename of \" + fileName + \" and with size of \" + buffer.Length, id.ToString());");
                    sb.AppendLine("                        }");
                    sb.AppendLine("                        catch (Exception ex)");
                    sb.AppendLine("                        {");
                    sb.AppendLine("                            CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, \"" + entity + " Data Upload Failed.\", false, id.ToString(), \"\", \"\", ex);");
                    sb.AppendLine("                            throw;");
                    sb.AppendLine("                        }");

                }
                else
                {
                    sb.AppendLine("                       " + qualifiedEntity + " " + CamelCase(entity) + " = await (from x in db." + Pluralize(entity) + " where x.id == id && x.active == true && x.deleted == false select x).FirstOrDefaultAsync();");
                    sb.AppendLine("                       if (" + CamelCase(entity) + " == null)");
                    sb.AppendLine("                       {");
                    sb.AppendLine("                          return NotFound();");
                    sb.AppendLine("                       }");
                    sb.AppendLine();
                    sb.AppendLine("                        bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();");
                    sb.AppendLine();

                    sb.AppendLine("                        lock (" + CamelCase(entity) + "PutSyncRoot)");
                    sb.AppendLine("                        {");

                    sb.AppendLine("                           try");
                    sb.AppendLine("                           {");

                    sb.AppendLine("                                using (var transaction = db.Database.BeginTransaction())");
                    sb.AppendLine("                                {");
                    sb.AppendLine("                                    " + CamelCase(entity) + "." + dataRootFieldName + "FileName = fileName.Trim();");
                    sb.AppendLine("                                    " + CamelCase(entity) + "." + dataRootFieldName + "MimeType = mimeType;");
                    sb.AppendLine("                                    " + CamelCase(entity) + "." + dataRootFieldName + "Size = buffer.Length;");
                    sb.AppendLine();
                    sb.AppendLine("                                    " + CamelCase(entity) + ".versionNumber++;");
                    sb.AppendLine();

                    sb.AppendLine("                                    if (diskBasedBinaryStorageMode == true &&");
                    sb.AppendLine("                                         " + CamelCase(entity) + "." + dataRootFieldName + "FileName != null &&");
                    sb.AppendLine("                                         " + CamelCase(entity) + "." + dataRootFieldName + "Size > 0)");
                    sb.AppendLine("                                    {");
                    sb.AppendLine("                            	        //");
                    sb.AppendLine("                            	        // write the bytes to disk");
                    sb.AppendLine("                            	        //");
                    sb.AppendLine("                            	        WriteDataToDisk(" + CamelCase(entity) + ".objectGuid, " + CamelCase(entity) + ".versionNumber , buffer, \"" + dataFileNameExtension + "\");");

                    sb.AppendLine("                            	        //");
                    sb.AppendLine("                            	        // Clear the data from the object before we put it into the db");
                    sb.AppendLine("                            	        //");
                    sb.AppendLine("                                        " + CamelCase(entity) + "." + dataRootFieldName + "Data = null;");
                    sb.AppendLine("                                    }");
                    sb.AppendLine("                                    else");
                    sb.AppendLine("                                    {");
                    sb.AppendLine("                            	        " + CamelCase(entity) + "." + dataRootFieldName + "Data = buffer;");
                    sb.AppendLine("                                    } ");


                    sb.AppendLine("                                    //");
                    sb.AppendLine("                                    // Now add the change history");
                    sb.AppendLine("                                    //");
                    sb.AppendLine("                                    JavaScriptSerializer serializer = new JavaScriptSerializer();");
                    sb.AppendLine("                                    serializer.MaxJsonLength = 500 * 1024 * 1024;       // 500 megabytes");
                    sb.AppendLine();

                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine("                                    User user = GetUser(" + CamelCase(entity) + ".tenantGuid, securityUser);");
                        sb.AppendLine("                                    " + entity + "ChangeHistory " + CamelCase(entity) + "ChangeHistory = new " + entity + "ChangeHistory();");
                        sb.AppendLine("                                    " + CamelCase(entity) + "ChangeHistory." + CamelCase(entity, false) + "Id = " + CamelCase(entity) + ".id;");
                        sb.AppendLine("                                    " + CamelCase(entity) + "ChangeHistory.versionNumber = " + CamelCase(entity) + ".versionNumber;");
                        sb.AppendLine("                                    " + CamelCase(entity) + "ChangeHistory.timeStamp = DateTime.UtcNow;");
                        sb.AppendLine("                                    " + CamelCase(entity) + "ChangeHistory.userId = user.id;");
                        if (multiTenancyEnabled == true)
                        {
                            sb.AppendLine("                                    " + CamelCase(entity) + "ChangeHistory.tenantGuid = " + CamelCase(entity) + ".tenantGuid;");
                        }
                        sb.AppendLine($"                                    {CamelCase(entity)}ChangeHistory.data = serializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)}));");
                    }
                    else
                    {
                        sb.AppendLine("                                    " + entity + "ChangeHistory " + CamelCase(entity) + "ChangeHistory = new " + entity + "ChangeHistory();");
                        sb.AppendLine("                                    " + CamelCase(entity) + "ChangeHistory." + CamelCase(entity, false) + "Id = " + CamelCase(entity) + ".id;");
                        sb.AppendLine("                                    " + CamelCase(entity) + "ChangeHistory.versionNumber = " + CamelCase(entity) + ".versionNumber;");
                        sb.AppendLine("                                    " + CamelCase(entity) + "ChangeHistory.timeStamp = DateTime.UtcNow;");
                        sb.AppendLine("                                    " + CamelCase(entity) + "ChangeHistory.userId = securityUser.id;");
                        if (multiTenancyEnabled == true)
                        {
                            sb.AppendLine("                                    " + CamelCase(entity) + "ChangeHistory.tenantGuid = " + CamelCase(entity) + ".tenantGuid;");
                        }
                        sb.AppendLine($"                                    {CamelCase(entity)}ChangeHistory.data = serializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)}));");
                    }


                    sb.AppendLine("                                    db." + entity + "ChangeHistories.Add(" + CamelCase(entity) + "ChangeHistory);");
                    sb.AppendLine();
                    sb.AppendLine("                                    db.SaveChanges();");
                    sb.AppendLine();
                    sb.AppendLine("                                    transaction.Commit();");
                    sb.AppendLine();
                    sb.AppendLine("                                    CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, \"" + entity + " Data Uploaded with filename of \" + fileName + \" and with size of \" + buffer.Length, id.ToString());");
                    sb.AppendLine("                                }");
                    sb.AppendLine("                            }");
                    sb.AppendLine("                            catch (Exception ex)");
                    sb.AppendLine("                            {");
                    sb.AppendLine("                                CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, \"" + entity + " Data Upload Failed.\", false, id.ToString(), \"\", \"\", ex);");
                    sb.AppendLine("                                throw;");
                    sb.AppendLine("                            }");
                    sb.AppendLine("                        }");
                }
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("                        //");
                sb.AppendLine("                        // Stop looking for more files.");
                sb.AppendLine("                        //");
                sb.AppendLine("                        break;");
                sb.AppendLine("                    }");
                sb.AppendLine("                }");
                sb.AppendLine("            }");
                sb.AppendLine("            catch (Exception ex)");
                sb.AppendLine("            {");
                sb.AppendLine("                CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, \"Caught error in UploadData handler\", id.ToString(), ex);");
                sb.AppendLine("                throw;");
                sb.AppendLine("            }");
                sb.AppendLine();
                sb.AppendLine("            if (foundFileData == false)");
                sb.AppendLine("            {");
                sb.AppendLine();

                if (versionControlEnabled == false)
                {
                    sb.AppendLine("                //");
                    sb.AppendLine("                // We have no file, but is the ID valid?");
                    sb.AppendLine("                //");
                    sb.AppendLine("                " + qualifiedEntity + " " + CamelCase(entity) + " = await (from x in db." + Pluralize(entity) + " where x.id == id && x.active == true && x.deleted == false select x).FirstOrDefaultAsync();");
                    sb.AppendLine("                if (" + CamelCase(entity) + " == null)");
                    sb.AppendLine("                {");
                    sb.AppendLine("                    return NotFound();");
                    sb.AppendLine("                }");
                    sb.AppendLine("                else");
                    sb.AppendLine("                {");
                    sb.AppendLine("                    //");
                    sb.AppendLine("                    // Treat the situation where we have a valid ID but no file content as a request to clear the data");
                    sb.AppendLine("                    //");
                    sb.AppendLine("                    try");
                    sb.AppendLine("                    {");
                    sb.AppendLine("                        bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();");
                    sb.AppendLine();
                    sb.AppendLine("                        if (diskBasedBinaryStorageMode == true)");
                    sb.AppendLine("                        {");
                    sb.AppendLine("						       DeleteDataFromDisk(" + CamelCase(entity) + ".objectGuid, 0, \"data\");");
                    sb.AppendLine("                        }");
                    sb.AppendLine();
                    sb.AppendLine("                        " + CamelCase(entity) + "." + dataRootFieldName + "FileName = null;");
                    sb.AppendLine("                        " + CamelCase(entity) + "." + dataRootFieldName + "MimeType = null;");
                    sb.AppendLine("                        " + CamelCase(entity) + "." + dataRootFieldName + "Data = null;");
                    sb.AppendLine("                        " + CamelCase(entity) + "." + dataRootFieldName + "Size = 0;");
                    sb.AppendLine();
                    sb.AppendLine("                        await db.SaveChangesAsync();");
                    sb.AppendLine();
                    sb.AppendLine("                        CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, \"" + entity + " data cleared.\", id.ToString());");
                    sb.AppendLine("                    }");
                    sb.AppendLine("                    catch (Exception ex)");
                    sb.AppendLine("                    {");
                    sb.AppendLine("                        CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, \"" + entity + " data clear failed.\", false, id.ToString(), \"\", \"\", ex);");
                    sb.AppendLine("                        throw;");
                    sb.AppendLine("                    }");
                }
                else
                {
                    sb.AppendLine("                //");
                    sb.AppendLine("                // We have no file, but is the ID valid?");
                    sb.AppendLine("                //");
                    sb.AppendLine("                " + qualifiedEntity + " " + CamelCase(entity) + " = await (from x in db." + Pluralize(entity) + " where x.id == id && x.active == true && x.deleted == false select x).FirstOrDefaultAsync();");
                    sb.AppendLine("                if (" + CamelCase(entity) + " == null)");
                    sb.AppendLine("                {");
                    sb.AppendLine("                    return NotFound();");
                    sb.AppendLine("                }");
                    sb.AppendLine();

                    sb.AppendLine("                lock (" + CamelCase(entity) + "PutSyncRoot)");
                    sb.AppendLine("                {");


                    sb.AppendLine("                        //");
                    sb.AppendLine("                        // Treat the situation where we have a valid ID but no file content as a request to clear the data");
                    sb.AppendLine("                        //");
                    sb.AppendLine("                        try");
                    sb.AppendLine("                        {");
                    sb.AppendLine("                            bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();");
                    sb.AppendLine();
                    sb.AppendLine("                            if (diskBasedBinaryStorageMode == true)");
                    sb.AppendLine("                            {");
                    sb.AppendLine("								DeleteDataFromDisk(" + CamelCase(entity) + ".objectGuid, " + CamelCase(entity) + ".versionNumber, \"data\");");
                    sb.AppendLine("                            }");
                    sb.AppendLine();
                    sb.AppendLine("                            using (var transaction = db.Database.BeginTransaction())");
                    sb.AppendLine("                            {");
                    sb.AppendLine("                                " + CamelCase(entity) + "." + dataRootFieldName + "FileName = null;");
                    sb.AppendLine("                                " + CamelCase(entity) + "." + dataRootFieldName + "MimeType = null;");
                    sb.AppendLine("                                " + CamelCase(entity) + "." + dataRootFieldName + "Size = 0;");
                    sb.AppendLine("                                " + CamelCase(entity) + "." + dataRootFieldName + "Data = null;");
                    sb.AppendLine("                                " + CamelCase(entity) + ".versionNumber++;");
                    sb.AppendLine();
                    sb.AppendLine("                                //");
                    sb.AppendLine("                                // Now add the change history");
                    sb.AppendLine("                                //");
                    sb.AppendLine("                                JavaScriptSerializer serializer = new JavaScriptSerializer();");
                    sb.AppendLine("                                " + entity + "ChangeHistory " + CamelCase(entity) + "ChangeHistory = new " + entity + "ChangeHistory();");
                    sb.AppendLine("                                " + CamelCase(entity) + "ChangeHistory." + CamelCase(entity, false) + "Id = " + CamelCase(entity) + ".id;");
                    sb.AppendLine("                                " + CamelCase(entity) + "ChangeHistory.versionNumber = " + CamelCase(entity) + ".versionNumber;");
                    sb.AppendLine("                                " + CamelCase(entity) + "ChangeHistory.timeStamp = DateTime.UtcNow;");
                    sb.AppendLine("                                " + CamelCase(entity) + "ChangeHistory.userId = securityUser.id;");
                    if (multiTenancyEnabled == true)
                    {
                        sb.AppendLine("                                " + CamelCase(entity) + "ChangeHistory.tenantGuid = " + CamelCase(entity) + ".tenantGuid;");
                    }
                    sb.AppendLine($"                                {CamelCase(entity)}ChangeHistory.data = serializer.Serialize({qualifiedEntity}.CreateAnonymousWithFirstLevelSubObjects({CamelCase(entity)}));");
                    sb.AppendLine("                                db." + entity + "ChangeHistories.Add(" + CamelCase(entity) + "ChangeHistory);");
                    sb.AppendLine();
                    sb.AppendLine("                                db.SaveChanges();");
                    sb.AppendLine();
                    sb.AppendLine("                                transaction.Commit();");
                    sb.AppendLine();
                    sb.AppendLine("                                CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, \"" + entity + " data cleared.\", id.ToString());");
                    sb.AppendLine("                            }");
                    sb.AppendLine("                        }");
                    sb.AppendLine("                        catch (Exception ex)");
                    sb.AppendLine("                        {");
                    sb.AppendLine("                            CreateAuditEvent(AuditEngine.AuditType.DeleteEntity, \"" + entity + " data clear failed.\", false, id.ToString(), \"\", \"\", ex);");
                    sb.AppendLine("                            throw;");
                    sb.AppendLine("                        }");
                }
                sb.AppendLine("                }");
                sb.AppendLine("            }");
                sb.AppendLine();
                sb.AppendLine("            return Ok();");
                sb.AppendLine("        }");
                sb.AppendLine();
                sb.AppendLine();

                #endregion

                #region Direct_Download_Handling

                if (string.IsNullOrEmpty(scriptGenTable.pngRootFieldName) == false)
                {
                    sb.AppendLine();
                    sb.AppendLine("        [HttpGet]");
                    sb.AppendLine("        [Route(\"api/" + pluralizeEntityForRouteForSomeTypeNames(entity) + "/Data/{id:int}\")]");
                    sb.AppendLine("        public async Task<IHttpActionResult> PNGDownloadAsync(int id, int? width = null, int? height = null)");
                    sb.AppendLine("        {");
                    sb.AppendLine("            await ThrowErrorIfUserHasNoReadPrivilegeOrAccessAsync();");
                    sb.AppendLine();
                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine("            SecurityUser securityUser = await GetSecurityUserAsync();");
                        sb.AppendLine();
                        sb.AppendLine("            await ThrowErrorIfUserCannotReadFromIdAsync(securityUser, id);");
                        sb.AppendLine();
                    }
                    sb.AppendLine();

                    //sb.AppendLine("            using (" + module + "Entities db = new " + module + "Entities())");
                    if (contextClassName == null)
                    {
                        sb.AppendLine("			using (" + module + "Entities db = new " + module + "Entities())");
                    }
                    else
                    {
                        sb.AppendLine("			using (" + contextClassName + " db = new " + contextClassName + "())");
                    }

                    sb.AppendLine("            {");
                    sb.AppendLine("                //");
                    sb.AppendLine("                // Return the PNG to the user as though it was a file.");
                    sb.AppendLine("                //");
                    sb.AppendLine("                " + qualifiedEntity + " " + CamelCase(entity) + " = await (from d in db." + Pluralize(entity) + "");
                    sb.AppendLine("                                    where d.id == id &&");
                    sb.AppendLine("                                    d.active == true &&");
                    sb.AppendLine("                                    d.deleted == false");
                    sb.AppendLine("                                    select d).FirstOrDefaultAsync();");


                    sb.AppendLine();
                    sb.AppendLine("                bool diskBasedBinaryStorageMode = Foundation.Configuration.GetDiskBasedBinaryStorageMode();");
                    sb.AppendLine();
                    sb.AppendLine("                if (diskBasedBinaryStorageMode == true &&");
                    sb.AppendLine("                	" + CamelCase(entity) + "." + dataRootFieldName + "Data == null &&");


                    if (BinaryStorageFieldsAreNullable(scriptGenTable) == true)
                    {
                        sb.AppendLine("                	" + CamelCase(entity) + "." + dataRootFieldName + "Size.HasValue == true &&");
                        sb.AppendLine("                	" + CamelCase(entity) + "." + dataRootFieldName + "Size.Value > 0)");
                    }
                    else
                    {
                        sb.AppendLine("                	" + CamelCase(entity) + "." + dataRootFieldName + "Size > 0)");
                    }

                    sb.AppendLine("                {");


                    if (versionControlEnabled == true)
                    {
                        sb.AppendLine("                	" + CamelCase(entity) + "." + dataRootFieldName + "Data = await LoadDataFromDiskAsync(" + CamelCase(entity) + ".objectGuid, " + CamelCase(entity) + ".versionNumber, \"" + dataFileNameExtension + "\");");
                    }
                    else
                    {
                        sb.AppendLine("                	" + CamelCase(entity) + "." + dataRootFieldName + "Data = await LoadDataFromDiskAsync(" + CamelCase(entity) + ".objectGuid, 0, \"" + dataFileNameExtension + "\");");
                    }

                    sb.AppendLine("                }");

                    sb.AppendLine();
                    sb.AppendLine("                if (" + CamelCase(entity) + " != null && " + CamelCase(entity) + "." + dataRootFieldName + "Data != null)");
                    sb.AppendLine("                {");
                    sb.AppendLine("                    if (width.HasValue == true && height.HasValue == true)");
                    sb.AppendLine("                    {");
                    sb.AppendLine("                        //");
                    sb.AppendLine("                        // Resize the image data to the user provided width and height");
                    sb.AppendLine("                        //");
                    sb.AppendLine("                        Image img = Image.FromStream(new MemoryStream(" + CamelCase(entity) + "." + dataRootFieldName + "Data));");
                    sb.AppendLine();
                    sb.AppendLine("                        Bitmap bmp = Foundation.ImagingHelper.ResizeImage(img, width.Value, height.Value);");
                    sb.AppendLine();
                    sb.AppendLine("                        if (bmp != null)");
                    sb.AppendLine("                        {");
                    sb.AppendLine("                            byte[] resized" + dataRootFieldName + "Data = Foundation.ImageUtility.ConvertImageToByteArray(bmp);");
                    sb.AppendLine();
                    sb.AppendLine("                            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);");
                    sb.AppendLine();
                    sb.AppendLine("                            response.Content = new ByteArrayContent(resized" + dataRootFieldName + "Data);");
                    sb.AppendLine("                            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(\"image/png\");");
                    sb.AppendLine("                            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue(\"inline\");  // if this was 'attachment' if would inform the browser to download.  Inline lets it open in browser.");
                    sb.AppendLine("                            response.Content.Headers.ContentDisposition.FileName = " + CamelCase(entity) + "." + dataRootFieldName + "FileName != null ? " + CamelCase(entity) + "." + dataRootFieldName + "FileName.Trim() : \"" + entity + "_\" + " + CamelCase(entity) + ".id.ToString();");
                    sb.AppendLine();
                    sb.AppendLine("                            return ResponseMessage(response);");
                    sb.AppendLine("                        }");
                    sb.AppendLine("                        else");
                    sb.AppendLine("                        {");
                    sb.AppendLine("                            throw new Exception(\"Unable to resize image\");");
                    sb.AppendLine("                        }");
                    sb.AppendLine("                    }");
                    sb.AppendLine("                    else");
                    sb.AppendLine("                    {");
                    sb.AppendLine("                        //");
                    sb.AppendLine("                        // No resizing.  Just send the data back as we have it it filed.");
                    sb.AppendLine("                        //");
                    sb.AppendLine("                        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);");
                    sb.AppendLine();
                    sb.AppendLine("                        response.Content = new ByteArrayContent(" + CamelCase(entity) + "." + dataRootFieldName + "Data);");
                    sb.AppendLine("                        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(\"image/png\");");
                    sb.AppendLine("                        response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue(\"inline\");  // if this was 'attachment' if would inform the browser to download.  Inline lets it open in browser.");
                    sb.AppendLine("                        response.Content.Headers.ContentDisposition.FileName = " + CamelCase(entity) + "." + dataRootFieldName + "FileName != null ? " + CamelCase(entity) + "." + dataRootFieldName + "FileName.Trim() : \"" + entity + "_\" + " + CamelCase(entity) + ".id.ToString();");
                    sb.AppendLine();
                    sb.AppendLine("                        return ResponseMessage(response);");
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
                    sb.AppendLine("        [Route(\"api/" + pluralizeEntityForRouteForSomeTypeNames(entity) + "/Data/{id:int}\")]");
                    sb.AppendLine("        public async Task<IHttpActionResult> DownloadDataAsync(int id)");
                    sb.AppendLine("        {");
                    sb.AppendLine("            await ThrowErrorIfUserHasNoReadPrivilegeOrAccessAsync();");
                    sb.AppendLine();
                    if (dataVisibilityEnabled == true)
                    {
                        sb.AppendLine("            SecurityUser securityUser = await GetSecurityUserAsync();");
                        sb.AppendLine();
                        sb.AppendLine("            await ThrowErrorIfUserCannotReadFromIdAsync(securityUser, id);");
                        sb.AppendLine();
                    }
                    sb.AppendLine();
                    //sb.AppendLine("            using (" + module + "Entities db = new " + module + "Entities())");
                    if (contextClassName == null)
                    {
                        sb.AppendLine("			using (" + module + "Entities db = new " + module + "Entities())");
                    }
                    else
                    {
                        sb.AppendLine("			using (" + contextClassName + " db = new " + contextClassName + "())");
                    }

                    sb.AppendLine("            {");
                    sb.AppendLine("                //");
                    sb.AppendLine("                // Return the data to the user as though it was a file.");
                    sb.AppendLine("                //");
                    sb.AppendLine("                " + qualifiedEntity + " " + CamelCase(entity) + " = await (from d in db." + Pluralize(entity) + "");
                    sb.AppendLine("                                                where d.id == id &&");
                    sb.AppendLine("                                                d.active == true &&");
                    sb.AppendLine("                                                d.deleted == false");
                    sb.AppendLine("                                                select d).FirstOrDefaultAsync();");
                    sb.AppendLine();
                    sb.AppendLine("                 if (" + CamelCase(entity) + " != null && " + CamelCase(entity) + "." + dataRootFieldName + "Data != null)");
                    sb.AppendLine("                 {");
                    sb.AppendLine();
                    sb.AppendLine("                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);");
                    sb.AppendLine();
                    sb.AppendLine("                    response.Content = new ByteArrayContent(" + CamelCase(entity) + "." + dataRootFieldName + "Data);");
                    sb.AppendLine("                    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(" + CamelCase(entity) + "." + dataRootFieldName + "MimeType);");
                    sb.AppendLine("                    response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue(\"inline\");  // if this was 'attachment' if would inform the browser to download.  Inline lets it open in browser.");
                    sb.AppendLine("                    response.Content.Headers.ContentDisposition.FileName = " + CamelCase(entity) + "." + dataRootFieldName + "FileName != null ? " + CamelCase(entity) + "." + dataRootFieldName + "FileName.Trim() : \"" + entity + "_\" + " + CamelCase(entity) + ".id.ToString();");
                    sb.AppendLine();
                    sb.AppendLine("                    return ResponseMessage(response);");
                    sb.AppendLine("                }");
                    sb.AppendLine("                else");
                    sb.AppendLine("                {");
                    sb.AppendLine("                    return ResponseMessage(new HttpResponseMessage(HttpStatusCode.BadRequest));");
                    sb.AppendLine("                }");
                    sb.AppendLine("            }");
                    sb.AppendLine("        }");
                    //sb.AppendLine();
                    //sb.AppendLine();
                }
            }

            #endregion

            sb.AppendLine("\t}");
            sb.AppendLine("}");

            return sb.ToString();
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

        private static bool HasDateTimeProperty(Type type)
        {
            foreach (PropertyInfo prop in type.GetProperties())
            {
                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType == typeof(DateTime))
                {
                    return true;
                }
            }


            return false;
        }


        private static string UserTenantGuidCommands(int numberOfTabs = 3, bool async = true)
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
                sb.AppendLine(tabPrefix + "    userTenantGuid = await UserTenantGuidAsync(securityUser);");
                sb.AppendLine(tabPrefix + "}");
                sb.AppendLine(tabPrefix + "catch (Exception ex)");
                sb.AppendLine(tabPrefix + "{");
                if (async == true)
                {
                    sb.AppendLine(tabPrefix + "    await CreateAuditEventAsync(AuditEngine.AuditType.Error, \"Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is \" + securityUser.accountName, securityUser.accountName, ex);");
                }
                else
                {
                    sb.AppendLine(tabPrefix + "    CreateAuditEvent(AuditEngine.AuditType.Error, \"Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is \" + securityUser.accountName, securityUser.accountName, ex);");
                }
                sb.AppendLine(tabPrefix + "    throw new Exception(\"Your user account is not configured with a tenant, so this operation is not allowed.\");");
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
                sb.AppendLine(tabPrefix + "    CreateAuditEvent(AuditEngine.AuditType.Error, \"Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is \" + securityUser.accountName, securityUser.accountName, ex);");
                sb.AppendLine(tabPrefix + "    throw new Exception(\"Your user account is not configured with a tenant, so this operation is not allowed.\");");
                sb.AppendLine(tabPrefix + "}");
            }

            return sb.ToString();
        }


        public static void BuildWebAPIImplementationFromEntityFrameworkContext(string contextName, string moduleName, Type contextType, DatabaseGenerator.Database database, string filePath = "c:\\temp", string databaseObjectNamespace = "Database", bool ignoreFoundationServices = false)
        {
            if (filePath.EndsWith("\\") == false)
            {
                filePath = filePath + "\\";
            }

            System.IO.Directory.CreateDirectory(filePath + moduleName);
            System.IO.Directory.CreateDirectory(filePath + moduleName + "\\Controllers");
            System.IO.Directory.CreateDirectory(filePath + moduleName + "\\Controllers\\WebAPI");


            if (database != null && database.dataVisibilityEnabled == true)
            {
                if (ignoreFoundationServices == true)
                {
                    throw new Exception("Can't use bare bones mode when data visibility is enabled");
                }

                //
                // First, create the base class
                //
                string baseClassWebAPICode = Foundation.CodeGeneration.WebAPICodeGeneratorFramework.BuildWebAPIBaseClassImplementationFromEntityFrameworkContext(moduleName, 1000, contextType, database);


                System.IO.File.WriteAllText(filePath + moduleName + "\\Controllers\\WebAPI\\" + moduleName + "BaseWebAPIController.cs", baseClassWebAPICode);

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

                        if (scriptGenTable != null && scriptGenTable.excludeFromCodeGeneration == false)
                        {
                            WebAPICode = BuildDefaultWebAPIImplementation(contextName, moduleName, type, scriptGenTable, databaseObjectNamespace, false);

                            System.IO.File.WriteAllText(filePath + moduleName + "\\Controllers\\WebAPI\\" + prop.Name + "Controller.cs", WebAPICode);
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
                                if (tbl.name == type.Name ||
                                    (
                                        // Stupid hack to deal with the ridiculous behavior of turning words like status into statu on property names in the db context.
                                        tbl.name.EndsWith("us") && type.Name.EndsWith("u") && tbl.name.Substring(0, tbl.name.Length - 1) == type.Name)
                                    )
                                {
                                    scriptGenTable = tbl;
                                    break;
                                }
                            }
                        }

                        if (scriptGenTable != null && scriptGenTable.excludeFromCodeGeneration == false)
                        {
                            WebAPICode = BuildDefaultWebAPIImplementation(contextName, moduleName, type, scriptGenTable, databaseObjectNamespace, ignoreFoundationServices);

                            System.IO.File.WriteAllText(filePath + moduleName + "\\Controllers\\WebAPI\\" + prop.Name + "Controller.cs", WebAPICode);
                        }
                    }
                }
            }

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