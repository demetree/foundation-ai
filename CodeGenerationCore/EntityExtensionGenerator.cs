using Foundation.ChangeHistory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static CodeGenerationCommon.Utility;
using static Foundation.CodeGeneration.DatabaseGenerator;
using static Foundation.StringUtility;

namespace Foundation.CodeGeneration
{
    public partial class EntityExtensionCodeGenerator : CodeGenerationBase
    {
        //
        // Use this to create default extension classes for the entity classes that are most likely automatically generated from a database.
        //
        protected static string BuildDefaultEntityExtensionImplementation(string module, Type type, DatabaseGenerator.Database.Table scriptGenTable, string nameProperty = "name", string databaseNamespace = "Database", bool ignoreFoundationServices = false, string entityExtensionRootNamespace = "Foundation")
        {
            StringBuilder sb = new StringBuilder();

            bool multiTenancyEnabled = false;
            bool dataVisibilityEnabled = false;
            bool isVersionControlEnabled = false;

            string entity = type.Name;
            string qualifiedEntity = databaseNamespace + "." + entity;
            string camelCaseEntity = Foundation.StringUtility.CamelCase(type.Name);


            if (scriptGenTable != null)
            {
                multiTenancyEnabled = scriptGenTable.IsMultiTenantEnabled();
                dataVisibilityEnabled = scriptGenTable.IsDataVisibilityEnabled();
                isVersionControlEnabled = scriptGenTable.IsVersionControlEnabled();
            }

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations;");


            if (dataVisibilityEnabled == true || entity.EndsWith("ChangeHistory") == true)
            {
                sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");           // Do not remove this.  It is needed for the NotMapped attribute
            }

            sb.AppendLine("using System.Linq;");

            if (isVersionControlEnabled == true)
            {
                sb.AppendLine("using System.Threading;");
                sb.AppendLine("using System.Threading.Tasks;");
            }

            if (scriptGenTable.IsDataVisibilityEnabled() == true)
            {
                sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            }


            if (ignoreFoundationServices == false)
            {
                sb.AppendLine("using Foundation.Entity;");
            }


            // For IAnonymousConvertible and ChangeHistoryToolset
            if (entity.EndsWith("ChangeHistory") == true || isVersionControlEnabled == true)
            {
                sb.AppendLine("using Foundation.ChangeHistory;");
            }


            sb.AppendLine("");
            sb.AppendLine("namespace " + entityExtensionRootNamespace + "." + module + "." + databaseNamespace);
            sb.AppendLine("{");


            //
            // If data visibility is enabled, we need to create a wrapper class that contains the write permission details.
            //
            if (dataVisibilityEnabled == true)
            {
                //
                // The wrapper class with the writable permission details only applies when data visibility it enabled.
                //
                sb.AppendLine("\t[NotMapped]");       // Needed for code first to stop it from trying to find fields for the properties on this class.
                sb.AppendLine("\tpublic class " + entity + "WithWritePermissionDetails : " + qualifiedEntity);
                sb.AppendLine("\t{");

                sb.AppendLine("\t\tpublic bool isWriteable { get; set; }");
                sb.AppendLine("\t\tpublic bool canChangeHierarchy { get; set; }");
                sb.AppendLine("\t\tpublic bool canChangeOwner { get; set; }");
                sb.AppendLine("\t\tpublic string notWriteableReason { get; set; }");
                sb.AppendLine();

                sb.AppendLine("\t\tpublic " + entity + "WithWritePermissionDetails(" + qualifiedEntity + " input)");
                sb.AppendLine("\t\t{");

                foreach (PropertyInfo prop in type.GetProperties())
                {
                    sb.AppendLine("\t\t\tthis." + prop.Name + " = input." + prop.Name + ";");
                }
                sb.AppendLine();
                sb.AppendLine("\t\t\tisWriteable = false;");
                sb.AppendLine("\t\t\tcanChangeHierarchy = false;");
                sb.AppendLine("\t\t\tcanChangeOwner = false;");
                sb.AppendLine("\t\t\tnotWriteableReason = null;");
                sb.AppendLine("\t\t}");
                sb.AppendLine("\t}");
                sb.AppendLine();
            }


            sb.AppendLine("\t//");
            sb.AppendLine("\t// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.");
            sb.AppendLine("\t//");


            //
            // Special handling for change history extensions.  We give them a new interface.
            //
            // Also, when ignoring foundation services, we always use this path.
            //
            if (entity.EndsWith("ChangeHistory") == false || ignoreFoundationServices == true)
            {
                if (ignoreFoundationServices == false)
                {
                    // If we are a version control enabled entity, we put on the IVersionTrackedEntity interface
                    if (isVersionControlEnabled == true)
                    {
                        sb.AppendLine("\tpublic partial class " + entity + " : IVersionTrackedEntity<" + entity + ">, IAnonymousConvertible");
                    }
                    else
                    {
                        sb.AppendLine("\tpublic partial class " + entity + " : IAnonymousConvertible");
                    }
                }
                else
                {
                    sb.AppendLine("\tpublic partial class " + entity);          // Non foundation service classes won't know the IAnonymousConvertible interface.
                }


                sb.AppendLine("\t{");

                //
                // If the table has version control enabled, create a helper method to get the ChangeHistoryToolset configuration for it, as well as all the inquiry methods
                //
                if (isVersionControlEnabled == true)
                {

                    sb.AppendLine($"        /// <summary>");
                    sb.AppendLine($"        /// This is for setting the context for change history inquiries.");
                    sb.AppendLine($"        /// </summary>");
                    sb.AppendLine($"        private {module}Context _contextForVersionInquiry = null;");
                    if (scriptGenTable.database.multiTenantEnabled == true || scriptGenTable.database.dataVisibilityEnabled == true)
                    {
                        sb.AppendLine($"        private Guid _tenantGuidForVersionInquiry = Guid.Empty;");
                    }
                    sb.AppendLine();
                    sb.AppendLine();

                    //
                    // Create the change history toolset getter methods for read/write purposes based on whether or not the table has data visibility on.
                    //
                    // This controls how the user Id is determined.
                    //
                    if (scriptGenTable.IsDataVisibilityEnabled() == true)
                    {
                        sb.AppendLine($@"
        /// <summary>
        /// 
        /// Gets the a Change History toolset for the user that support write and read operations.
        /// 
        /// </summary>
        /// <param name=""context"">A context object that contains the entities</param>
        /// <param name=""securityUser"">The security user that the changes will be made on behalf of.</param>
        /// <param name=""insideTransaction"">Whether or not there is a transaction in process by the using function</param>
        /// <returns>A change history toolset instance to interact with the change history of the entity</returns>
        /// <exception cref=""ArgumentNullException""></exception>
        /// <exception cref=""Exception""></exception>
        public static ChangeHistoryToolset<{entity}, {entity}ChangeHistory> GetChangeHistoryToolsetForWriting({module}Context context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
        {{
			if (context == null)
			{{
				throw new ArgumentNullException(nameof(context));
			}}

			if (securityUser == null)
			{{ 
				throw new ArgumentNullException(nameof(securityUser));
			}}
			
			//
			// This table is multi-tenanted, and data visibility enabled.  It needs the user id from the context's user table for change history purposes.
			// 

			//
			// Get the user's tenant guid.  This will throw an exception on any error.
			//
			Guid userTenantGuid = Foundation.Security.SecurityFramework.UserTenantGuid(securityUser);

			int? userId = (from u in context.Users where u.objectGuid == securityUser.objectGuid && u.tenantGuid == userTenantGuid select u.id).FirstOrDefault();

			if (userId.HasValue == true)
			{{
				return new ChangeHistoryToolset<{entity}, {entity}ChangeHistory>(context, userId.Value, insideTransaction, cancellationToken);
			}}
			else
			{{
				throw new Exception($""Unable to find id for user record in context for security user {{securityUser.accountName}} - {{securityUser.objectGuid}}"");
			}}
        }}


        /// <summary>
        /// 
        /// Gets the a Change History toolset for the user.
        /// 
        /// </summary>
		/// <param name=""context"">A context object that contains the entities</param>
		/// <param name=""securityUser"">The security user that the changes will be made on behalf of.</param>
		/// <param name=""insideTransaction"">Whether or not there is a transaction in process by the using function</param>
		/// <returns>A change history toolset instance to interact with the change history of the entity</returns>
        /// <exception cref=""ArgumentNullException""></exception>
        /// <exception cref=""Exception""></exception>
        public static async Task<ChangeHistoryToolset<{entity}, {entity}ChangeHistory>> GetChangeHistoryToolsetForWritingAsync({module}Context context, Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
        {{
            if (context == null)
            {{
                throw new ArgumentNullException(nameof(context));
            }}

            if (securityUser == null)
            {{
                throw new ArgumentNullException(nameof(securityUser));
            }}

            //
            // This table is multi-tenanted, and data visibility enabled.  It needs the user id from the context's user table for change history purposes.
            // 

            //
            // Get the user's tenant guid.  This will throw an exception on any error.
            //
            Guid userTenantGuid = await Foundation.Security.SecurityFramework.UserTenantGuidAsync(securityUser, cancellationToken).ConfigureAwait(false);

            int? userId = await (from u in context.Users where u.objectGuid == securityUser.objectGuid && u.tenantGuid == userTenantGuid select u.id).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (userId.HasValue == true)
            {{
                return new ChangeHistoryToolset<{entity}, {entity}ChangeHistory>(context, userId.Value, insideTransaction, cancellationToken);
            }}
            else
            {{
                throw new Exception($""Unable to find id for user record in context for security user {{securityUser.accountName}} - {{securityUser.objectGuid}}"");
            }}
        }}


        /// <summary>
        /// 
        /// Gets the a Change History toolset for read only purposes.
        /// 
        /// </summary>
        /// <param name=""context"">A context object that contains the entities</param>
        /// <returns>A change history toolset instance to interact with the change history of the entity</returns>       
		/// <exception cref=""ArgumentNullException""></exception>
        /// <exception cref=""Exception""></exception>
        public static ChangeHistoryToolset<{entity}, {entity}ChangeHistory> GetChangeHistoryToolsetForReading({module}Context context, CancellationToken cancellationToken = default)
        {{
            if (context == null)
            {{
                throw new ArgumentNullException(nameof(context));
            }}

			return new ChangeHistoryToolset<{entity}, {entity}ChangeHistory>(context, cancellationToken);
        }}
");
                    }
                    else
                    {
                        sb.AppendLine($@"
        /// <summary>
        /// 
        /// Gets the a Change History toolset for the user that support write and read operations.
        /// 
        /// </summary>
        /// <param name=""context"">A context object that contains the entities</param>
        /// <param name=""securityUser"">The security user that the changes will be made on behalf of.</param>
        /// <param name=""insideTransaction"">Whether or not there is a transaction in process by the using function</param>
        /// <returns>A change history toolset instance to interact with the change history of the entity</returns>
        /// <exception cref=""ArgumentNullException""></exception>
        /// <exception cref=""Exception""></exception>
        public static ChangeHistoryToolset<{entity}, {entity}ChangeHistory> GetChangeHistoryToolsetForWriting({module}Context context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
        {{
            if (context == null)
            {{
                throw new ArgumentNullException(nameof(context));
            }}

            if (securityUser == null)
            {{
                throw new ArgumentNullException(nameof(securityUser));
            }}

            //
            // This table does not have data visibility enabled, therefore the user ID is to be taken directly from the security user object.
            // 
            return new ChangeHistoryToolset<{entity}, {entity}ChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
        }}

        /// <summary>
        /// 
        /// Gets the a Change History toolset for read only purposes.
        /// 
        /// </summary>
        /// <param name=""context"">A context object that contains the entities</param>
        /// <returns>A change history toolset instance to interact with the change history of the entity</returns>       
        /// <exception cref=""ArgumentNullException""></exception>
        /// <exception cref=""Exception""></exception>
        public static ChangeHistoryToolset<{entity}, {entity}ChangeHistory> GetChangeHistoryToolsetForReading({module}Context context, CancellationToken cancellationToken = default)
        {{
            return new ChangeHistoryToolset<{entity}, {entity}ChangeHistory>(context, cancellationToken);
        }}

");
                    }

                    sb.AppendLine($"        /// <summary>");
                    sb.AppendLine($"        /// ");
                    sb.AppendLine($"        /// This needs to be called before running any version inquiry method from the IVersionTrackedEntity interface.");
                    sb.AppendLine($"        ///");
                    sb.AppendLine($"        /// It sets up the context and the tenant guid to use.  Provide the context used for the work, and the tenant guid of the user executing the logic.");
                    sb.AppendLine($"        ///");
                    sb.AppendLine($"        /// </summary>");
                    sb.AppendLine($"        /// <param name=\"context\"></param>");
                    sb.AppendLine($"        /// <param name=\"tenantGuid\"></param>");

                    if (scriptGenTable.database.multiTenantEnabled == true || scriptGenTable.database.dataVisibilityEnabled == true)
                    {
                        sb.AppendLine($"        public void SetupVersionInquiry({module}Context context, Guid tenantGuid)");
                        sb.AppendLine($"        {{");
                        sb.AppendLine($"            _contextForVersionInquiry = context;");
                        sb.AppendLine($"            _tenantGuidForVersionInquiry = tenantGuid;");
                        sb.AppendLine($"        }}");
                    }
                    else
                    {
                        sb.AppendLine($"        public void SetupVersionInquiry({module}Context context)");
                        sb.AppendLine($"        {{");
                        sb.AppendLine($"            _contextForVersionInquiry = context;");
                        sb.AppendLine($"        }}");
                    }

                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine($"        /// <summary>");
                    sb.AppendLine($"        /// ");
                    sb.AppendLine($"        /// Gets meta data and optionally the entity data about the entity's version history using the version of the entity as the basis for the query.");
                    sb.AppendLine($"        /// ");
                    sb.AppendLine($"        /// Use this to get the update user/time metadata for this version.  IncludingData here is optional and default to false, as it is probably redundant in most cases ");
                    sb.AppendLine($"        /// unless the entity you're working with might have unsaved changes.");
                    sb.AppendLine($"        /// ");
                    sb.AppendLine($"        /// </summary>");
                    sb.AppendLine($"        /// <param name=\"includeData\">Whether or not to return the entity data with the results.</param>");
                    sb.AppendLine($"        /// <returns></returns>");
                    sb.AppendLine($"        /// <exception cref=\"Exception\"></exception>");
                    sb.AppendLine($"        public async Task<VersionInformation<{entity}>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)");
                    sb.AppendLine($"        {{");
                    sb.AppendLine($"            return await GetVersionAsync(this.versionNumber, includeData, cancellationToken).ConfigureAwait(false);");

                    // Simplified to just call to the get version method, as it just repeats code.
                    //if (scriptGenTable.database.multiTenantEnabled == true || scriptGenTable.database.dataVisibilityEnabled == true)
                    //{
                    //    sb.AppendLine($"            if (_contextForVersionInquiry == null || _tenantGuidForVersionInquiry == Guid.Empty)");
                    //}
                    //else
                    //{
                    //    sb.AppendLine($"            if (_contextForVersionInquiry == null)");
                    //}

                    //sb.AppendLine($"            {{");
                    //sb.AppendLine($"                throw new Exception(\"Context for version inquiry is not set.  Please call SetupVersionInquiry() before calling this function.\");");
                    //sb.AppendLine($"            }}");
                    //sb.AppendLine();
                    //sb.AppendLine($"            var chts = GetChangeHistoryToolset(_contextForVersionInquiry, false);");
                    //sb.AppendLine();
                    //sb.AppendLine($"            // Get the version that this record has");
                    //sb.AppendLine($"            var thisVersionAudit = await chts.GetAuditForVersion(this, this.versionNumber);");
                    //sb.AppendLine();
                    //sb.AppendLine($"            if (thisVersionAudit == null)");
                    //sb.AppendLine($"            {{");
                    //sb.AppendLine($"                throw new Exception($\"No change history found for version {{this.versionNumber}} of this {entity} entity.\");");
                    //sb.AppendLine($"            }}");
                    //sb.AppendLine();
                    //sb.AppendLine($"            VersionInformation<{entity}> thisVersion = new VersionInformation<{entity}>();");
                    //sb.AppendLine();
                    //sb.AppendLine($"            thisVersion.versionNumber = thisVersionAudit.versionNumber;");
                    //sb.AppendLine($"            thisVersion.timeStamp = thisVersionAudit.timeStamp;");
                    //sb.AppendLine();
                    //sb.AppendLine($"            if (thisVersionAudit.userId.HasValue == true)");
                    //sb.AppendLine($"            {{");

                    //AddUserListFetchingLines("thisVersion", module, scriptGenTable, sb);

                    //sb.AppendLine($"            }}");
                    //sb.AppendLine($"            else");
                    //sb.AppendLine($"            {{");
                    //sb.AppendLine($"                // Continency to return a change history user configured to indicate that we don't know the user.");
                    //sb.AppendLine($"                thisVersion.user = new ChangeHistoryUser() {{ firstName = \"Unknown\", id = 0, middleName = null, lastName = \"User\" }};");
                    //sb.AppendLine($"            }}");
                    //sb.AppendLine();
                    //sb.AppendLine($"            if (includeData == true)");
                    //sb.AppendLine($"            {{");
                    //sb.AppendLine($"                thisVersion.data = await chts.GetVersionAsync(this, this.versionNumber);");
                    //sb.AppendLine($"            }}");
                    //sb.AppendLine();
                    //sb.AppendLine($"            return thisVersion;");
                    sb.AppendLine($"        }}");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine($"        /// <summary>");
                    sb.AppendLine($"        /// ");
                    sb.AppendLine($"        /// Gets meta data and optionally the entity data about the first version of the entity.  Equivalent to GetVersionAsync(1, includeData), but name is a bit more concise.");
                    sb.AppendLine($"        /// ");
                    sb.AppendLine($"        /// </summary>");
                    sb.AppendLine($"        /// <param name=\"includeData\">Whether or not to return the entity data with the results.</param>");
                    sb.AppendLine($"        /// <returns></returns>");
                    sb.AppendLine($"        /// <exception cref=\"Exception\"></exception>");
                    sb.AppendLine($"        public async Task<VersionInformation<{entity}>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)");
                    sb.AppendLine($"        {{");
                    sb.AppendLine($"            return await GetVersionAsync(1, includeData, cancellationToken).ConfigureAwait(false);");

                    // Simplified to just call the function that does the same thing.
                    //if (scriptGenTable.database.multiTenantEnabled == true || scriptGenTable.database.dataVisibilityEnabled == true)
                    //{
                    //    sb.AppendLine($"            if (_contextForVersionInquiry == null || _tenantGuidForVersionInquiry == Guid.Empty)");
                    //}
                    //else
                    //{
                    //    sb.AppendLine($"            if (_contextForVersionInquiry == null)");
                    //}
                    //sb.AppendLine($"            {{");
                    //sb.AppendLine($"                throw new Exception(\"Context for version inquiry is not set.  Please call SetupVersionInquiry() before calling this function.\");");
                    //sb.AppendLine($"            }}");
                    //sb.AppendLine();
                    //sb.AppendLine($"            var chts = GetChangeHistoryToolset(_contextForVersionInquiry, false);");
                    //sb.AppendLine();
                    //sb.AppendLine($"            // Get version number one");
                    //sb.AppendLine($"            var firstVersionAudit = await chts.GetAuditForVersion(this, 1);");
                    //sb.AppendLine();
                    //sb.AppendLine($"            if (firstVersionAudit == null)");
                    //sb.AppendLine($"            {{");
                    //sb.AppendLine($"                throw new Exception($\"No change history found for the first version of this {entity} entity.\");");
                    //sb.AppendLine($"            }}");
                    //sb.AppendLine();
                    //sb.AppendLine($"            VersionInformation<{entity}> firstVersion = new VersionInformation<{entity}>();");
                    //sb.AppendLine();
                    //sb.AppendLine($"            firstVersion.versionNumber = firstVersionAudit.versionNumber;");
                    //sb.AppendLine($"            firstVersion.timeStamp = firstVersionAudit.timeStamp;");
                    //sb.AppendLine();
                    //sb.AppendLine($"            if (firstVersionAudit.userId.HasValue == true)");
                    //sb.AppendLine($"            {{");

                    //AddUserListFetchingLines("firstVersion", module, scriptGenTable, sb);

                    //sb.AppendLine($"            }}");
                    //sb.AppendLine($"            else");
                    //sb.AppendLine($"            {{");
                    //sb.AppendLine($"                // Continency to return a change history user configured to indicate that we don't know the user.");
                    //sb.AppendLine($"                firstVersion.user = new ChangeHistoryUser() {{ firstName = \"Unknown\", id = 0, middleName = null, lastName = \"User\" }};");
                    //sb.AppendLine($"            }}");
                    //sb.AppendLine();
                    //sb.AppendLine($"            if (includeData == true)");
                    //sb.AppendLine($"            {{");
                    //sb.AppendLine($"                firstVersion.data = await chts.GetVersionAsync(this, 1);");
                    //sb.AppendLine($"            }}");
                    //sb.AppendLine();
                    //sb.AppendLine($"            return firstVersion;");
                    sb.AppendLine($"        }}");
                    sb.AppendLine();
                    sb.AppendLine();

                    sb.AppendLine($"        /// <summary>");
                    sb.AppendLine($"        /// ");
                    sb.AppendLine($"        /// Gets meta data and optionally the entity data about the version of the entity at the provided point in time.");
                    sb.AppendLine($"        /// ");
                    sb.AppendLine($"        /// </summary>");
                    sb.AppendLine($"        /// <param name=\"includeData\">Whether or not to return the entity data with the results.</param>");
                    sb.AppendLine($"        /// <returns></returns>");
                    sb.AppendLine($"        /// <exception cref=\"Exception\"></exception>");
                    sb.AppendLine($"        public async Task<VersionInformation<{entity}>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)");
                    sb.AppendLine($"        {{");
                    sb.AppendLine($"            if (_contextForVersionInquiry == null || _tenantGuidForVersionInquiry == Guid.Empty)");
                    sb.AppendLine($"            {{");
                    sb.AppendLine($"                throw new Exception(\"Context for version inquiry is not set.  Please call SetupVersionInquiry() before calling this function.\");");
                    sb.AppendLine($"            }}");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine($"            var chts = GetChangeHistoryToolsetForReading(_contextForVersionInquiry, cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine($"            // Get the version for the point in time provided");
                    sb.AppendLine($"            AuditEntry versionAudit = await chts.GetAuditForTime(this, pointInTime).ConfigureAwait(false);");
                    sb.AppendLine();
                    sb.AppendLine($"            if (versionAudit == null)");
                    sb.AppendLine($"            {{");
                    sb.AppendLine($"                throw new Exception($\"No change history found for point in time {{pointInTime.ToString(\"s\")}} of this {entity} entity.\");");
                    sb.AppendLine($"            }}");
                    sb.AppendLine();
                    sb.AppendLine($"            VersionInformation<{entity}> version = new VersionInformation<{entity}>();");
                    sb.AppendLine();
                    sb.AppendLine($"            version.versionNumber = versionAudit.versionNumber;");
                    sb.AppendLine();
                    sb.AppendLine($"            version.timeStamp = versionAudit.timeStamp;");
                    sb.AppendLine();
                    sb.AppendLine($"            if (versionAudit.userId.HasValue == true)");
                    sb.AppendLine($"            {{");
                    sb.AppendLine($"                // Note that this system has multi tenancy enabled but not data visibility, so it gets its change history users from the security module by linking to tenant users.");
                    sb.AppendLine($"                version.user = await Foundation.Security.ChangeHistoryMultiTenant.GetChangeHistoryUserAsync(versionAudit.userId.Value, _tenantGuidForVersionInquiry, cancellationToken).ConfigureAwait(false);");
                    sb.AppendLine($"            }}");
                    sb.AppendLine($"            else");
                    sb.AppendLine($"            {{");
                    sb.AppendLine($"                // Continency to return a change history user configured to indicate that we don't know the user.");
                    sb.AppendLine($"                version.user = new ChangeHistoryUser() {{ firstName = \"Unknown\", id = 0, middleName = null, lastName = \"User\" }};");
                    sb.AppendLine($"            }}");
                    sb.AppendLine();
                    sb.AppendLine($"            if (includeData == true)");
                    sb.AppendLine($"            {{");
                    sb.AppendLine($"                version.data = await chts.GetVersionAsync(this, versionAudit.versionNumber).ConfigureAwait(false);");
                    sb.AppendLine($"            }}");
                    sb.AppendLine();
                    sb.AppendLine($"            return version;");
                    sb.AppendLine($"        }}");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine($"        /// <summary>");
                    sb.AppendLine($"        /// ");
                    sb.AppendLine($"        /// Gets meta data and optionally the entity data about a specific version of the entity.");
                    sb.AppendLine($"        /// ");
                    sb.AppendLine($"        /// </summary>");
                    sb.AppendLine($"        /// <param name=\"includeData\">Whether or not to return the entity data with the results.</param>");
                    sb.AppendLine($"        /// <returns></returns>");
                    sb.AppendLine($"        /// <exception cref=\"Exception\"></exception>");
                    sb.AppendLine($"        public async Task<VersionInformation<{entity}>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)");
                    sb.AppendLine($"        {{");

                    if (scriptGenTable.database.multiTenantEnabled == true || scriptGenTable.database.dataVisibilityEnabled == true)
                    {
                        sb.AppendLine($"            if (_contextForVersionInquiry == null || _tenantGuidForVersionInquiry == Guid.Empty)");
                    }
                    else
                    {
                        sb.AppendLine($"            if (_contextForVersionInquiry == null)");
                    }
                    sb.AppendLine($"            {{");
                    sb.AppendLine($"                throw new Exception(\"Context for version inquiry is not set.  Please call SetupVersionInquiry() before accessing the GetVersion function.\");");
                    sb.AppendLine($"            }}");
                    sb.AppendLine();
                    sb.AppendLine($"            var chts = GetChangeHistoryToolsetForReading(_contextForVersionInquiry, cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine($"            // Get the requested version");
                    sb.AppendLine($"            AuditEntry versionAudit = await chts.GetAuditForVersion(this, versionNumber).ConfigureAwait(false);");
                    sb.AppendLine();
                    sb.AppendLine($"            if (versionAudit == null)");
                    sb.AppendLine($"            {{");
                    sb.AppendLine($"                throw new Exception($\"No change history found for version {{versionNumber}} of this {entity} entity.\");");
                    sb.AppendLine($"            }}");
                    sb.AppendLine();
                    sb.AppendLine($"            VersionInformation<{entity}> version = new VersionInformation<{entity}>();");
                    sb.AppendLine();
                    sb.AppendLine($"            version.versionNumber = versionAudit.versionNumber;");
                    sb.AppendLine($"            version.timeStamp = versionAudit.timeStamp;");
                    sb.AppendLine();
                    sb.AppendLine($"            if (versionAudit.userId.HasValue == true)");
                    sb.AppendLine($"            {{");

                    AddUserListFetchingLines("version", module, scriptGenTable, sb);

                    sb.AppendLine($"            }}");
                    sb.AppendLine($"            else");
                    sb.AppendLine($"            {{");

                    sb.AppendLine($"                // Continency to return a change history user configured to indicate that we don't know the user.");
                    sb.AppendLine($"                version.user = new ChangeHistoryUser() {{ firstName = \"Unknown\", id = 0, middleName = null, lastName = \"User\" }};");

                    sb.AppendLine($"            }}");
                    sb.AppendLine();
                    sb.AppendLine($"            if (includeData == true)");
                    sb.AppendLine($"            {{");
                    sb.AppendLine($"                version.data = await chts.GetVersionAsync(this, versionNumber).ConfigureAwait(false);");
                    sb.AppendLine($"            }}");
                    sb.AppendLine();
                    sb.AppendLine($"            return version;");
                    sb.AppendLine($"        }}");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine($"        /// <summary>");
                    sb.AppendLine($"        /// ");
                    sb.AppendLine($"        /// This gets all the available meta data version information for this entity, and optionally the entity states too");
                    sb.AppendLine($"        /// ");
                    sb.AppendLine($"        /// </summary>");
                    sb.AppendLine($"        /// <param name=\"includeData\">Whether or not to return the entity data with the results.</param>");
                    sb.AppendLine($"        /// <returns></returns>");
                    sb.AppendLine($"        /// <exception cref=\"Exception\"></exception>");
                    sb.AppendLine($"        public async Task<List<VersionInformation<{entity}>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)");
                    sb.AppendLine($"        {{");

                    if (scriptGenTable.database.multiTenantEnabled == true || scriptGenTable.database.dataVisibilityEnabled == true)
                    {
                        sb.AppendLine($"            if (_contextForVersionInquiry == null || _tenantGuidForVersionInquiry == Guid.Empty)");
                    }
                    else
                    {
                        sb.AppendLine($"            if (_contextForVersionInquiry == null)");
                    }

                    sb.AppendLine($"            {{");
                    sb.AppendLine($"                throw new Exception(\"Context for version inquiry is not set.Please call SetupVersionInquiry() before accessing the GetAllVersions function.\");");
                    sb.AppendLine($"            }}");
                    sb.AppendLine();
                    sb.AppendLine($"            var chts = GetChangeHistoryToolsetForReading(_contextForVersionInquiry, cancellationToken);");
                    sb.AppendLine();
                    sb.AppendLine($"            List<AuditEntry> versionAudits = await chts.GetAuditTrailAsync(this).ConfigureAwait(false);");
                    sb.AppendLine();
                    sb.AppendLine($"            if (versionAudits == null)");
                    sb.AppendLine($"            {{");
                    sb.AppendLine($"                throw new Exception($\"No change history audits found for this entity.\");");
                    sb.AppendLine($"            }}");
                    sb.AppendLine();
                    sb.AppendLine($"            List <VersionInformation<{entity}>> versions = new List<VersionInformation<{entity}>>();");
                    sb.AppendLine();
                    sb.AppendLine($"            foreach (AuditEntry versionAudit in versionAudits)");
                    sb.AppendLine($"            {{");
                    sb.AppendLine($"                VersionInformation<{entity}> version = new VersionInformation<{entity}>();");
                    sb.AppendLine();
                    sb.AppendLine($"                version.versionNumber = versionAudit.versionNumber;");
                    sb.AppendLine($"                version.timeStamp = versionAudit.timeStamp;");
                    sb.AppendLine();
                    sb.AppendLine($"                if (versionAudit.userId.HasValue == true)");
                    sb.AppendLine($"                {{");

                    AddUserListFetchingLines("version", module, scriptGenTable, sb);

                    sb.AppendLine($"                }}");
                    sb.AppendLine($"                else");
                    sb.AppendLine($"                {{");
                    sb.AppendLine($"                    // Continency to return a change history user configured to indicate that we don't know the user.");
                    sb.AppendLine($"                    version.user = new ChangeHistoryUser() {{ firstName = \"Unknown\", id = 0, middleName = null, lastName = \"User\" }};");
                    sb.AppendLine($"                }}");
                    sb.AppendLine();
                    sb.AppendLine($"                if (includeData == true)");
                    sb.AppendLine($"                {{");
                    sb.AppendLine($"                    version.data = await chts.GetVersionAsync(this, versionAudit.versionNumber).ConfigureAwait(false);");
                    sb.AppendLine($"                }}");
                    sb.AppendLine();
                    sb.AppendLine($"                versions.Add(version);");
                    sb.AppendLine($"            }}");
                    sb.AppendLine();
                    sb.AppendLine($"            return versions;");
                    sb.AppendLine($"        }}");
                    sb.AppendLine();
                    sb.AppendLine();
                }
            }
            else
            {
                //
                // Special handling for change history extensions.  We give them a new interface.
                //
                sb.AppendLine("\tpublic partial class " + entity + " : IChangeHistoryEntity, IAnonymousConvertible");
                sb.AppendLine("\t{");

                string foreignKeyFieldName = "";
                bool longForeignKeyField = false;
                //
                // Get the first int property that is not named 'id'
                //

                foreach (PropertyInfo prop in type.GetProperties())
                {
                    Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;


                    // Look for int or long fields that end with Id to identity the FK field on this change history table.
                    if ((propertyType == typeof(int) || propertyType == typeof(long)) &&
                         prop.Name.Trim().ToUpper() != "ID" &&
                         prop.Name.Trim().ToUpper() != "VERSIONNUMBER")
                    {
                        foreignKeyFieldName = prop.Name;

                        longForeignKeyField = propertyType == typeof(long);

                        break;
                    }
                }

                if (string.IsNullOrEmpty(foreignKeyFieldName) == true)
                {
                    throw new Exception($"Unable to get foreign key field name from change history table for entity {entity}");

                }

                sb.AppendLine("\t\t[NotMapped]");
                sb.AppendLine("\t\tpublic long primaryId ");
                sb.AppendLine("\t\t{");

                if (longForeignKeyField == true)
                {
                    sb.AppendLine("\t\t\tget { return " + foreignKeyFieldName + "; }");
                    sb.AppendLine("\t\t\tset { " + foreignKeyFieldName + " = value; } ");
                }
                else
                {
                    sb.AppendLine("\t\t\tget { return (long)" + foreignKeyFieldName + "; }");
                    sb.AppendLine("\t\t\tset { " + foreignKeyFieldName + " = (int)value; } ");
                }


                sb.AppendLine("\t\t}");
                sb.AppendLine();
            }

            List<PropertyInfo> inputDtoProperties = new List<PropertyInfo>();
            List<PropertyInfo> outputDtoProperties = new List<PropertyInfo>();

            //
            // Include only what is minimally necessary for a DTO to use for input purposes to PUT and POST methods.
            //
            // These will contain only the value properties at the object root, but no sub objects.
            //
            foreach (PropertyInfo prop in type.GetProperties())
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                // TenantGuid is not editable by incoming posts or puts of the object, so don't include it on the DTOs
                if (prop.Name.Trim().ToUpper() == "TENANTGUID")
                {
                    // this property does not matter for the DTO 
                    continue;
                }
                else
                {
                    // Get the property type.  If property is nullable then get it's underlying type.  Otherwise use the property type directly.
                    Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    // Exclude lists
                    if (propertyType.FullName.StartsWith("System.Collections"))
                    {
                        continue;
                    }

                    // Is this property not a value type?  If so, don't add it to the DTO
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
                        propertyType != typeof(byte[]))
                    {
                        //
                        // add this property to the output DTO list because it is NOT a value type - it'll be an object.
                        //
                        outputDtoProperties.Add(prop);
                    }
                    else
                    {
                        //
                        // add this property to the input DTO, and the output DTO list because it is a value type
                        //
                        inputDtoProperties.Add(prop);
                        outputDtoProperties.Add(prop);
                    }
                }
            }
            
            sb.AppendLine("\t\t/// <summary>");
            sb.AppendLine("\t\t///");
            sb.AppendLine("\t\t/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.");
            sb.AppendLine("\t\t///");
            sb.AppendLine("\t\t/// Required fields are given the Required decorator");
            sb.AppendLine("\t\t///");
            sb.AppendLine("\t\t/// </summary>");
            sb.AppendLine("\t\tpublic class " + entity + "DTO");
            sb.AppendLine("\t\t{");
            foreach (PropertyInfo prop in inputDtoProperties)
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                //
                // Non-nullable data fields get the [Required] attribute.
                //
                // New object posts can be sent without these fields, so don't make any of these required
                //
                if (prop.Name.ToUpper() != "ID" &&
                    prop.Name.ToUpper() != "VERSIONNUMBER" &&
                    prop.Name.ToUpper() != "ACTIVE" &&
                    prop.Name.ToUpper() != "DELETED")
                {
                    Database.Table.Field scriptGenField = scriptGenTable.GetFieldByName(prop.Name);
                    if (scriptGenField != null)
                    {
                        if (scriptGenField.nullable == false)
                        {
                            sb.AppendLine("\t\t\t[Required]");
                        }
                    }
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                sb.Append("\t\t\tpublic ");

                //
                // Only Value types on input - map in the type name directlyy
                //
                sb.Append(propertyType.Name);

                    

                // Detect nullable fields, and also make the active and deleted fields on the DTO nullable so that they are not required for each submission (defaults will be assigned instead)
                bool isNullable = prop.PropertyType.Name.StartsWith("Nullable") || prop.Name.ToUpper() == "ACTIVE" || prop.Name.ToUpper() == "DELETED";
                if (isNullable == true)
                {
                    sb.Append("?");
                }

                sb.Append(" ");
                sb.Append(prop.Name);
                sb.AppendLine(" { get; set; }");
            }

            sb.AppendLine("\t\t}");
            sb.AppendLine();
            sb.AppendLine();


            sb.AppendLine("\t\t/// <summary>");
            sb.AppendLine("\t\t///");
            sb.AppendLine("\t\t/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.");
            sb.AppendLine("\t\t///");
            sb.AppendLine("\t\t/// </summary>");
            sb.AppendLine($"\t\tpublic class {entity}OutputDTO : {entity}DTO");
            sb.AppendLine("\t\t{");
            foreach (PropertyInfo prop in outputDtoProperties)
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;


                //
                // Check the property type.  Sub object types become the output dto of their object type in this extension to the base DTO
                //
                // Non object types are ignore here because we get them all from the base class.
                //
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
                    propertyType != typeof(byte[]))
                {
                    sb.Append("\t\t\tpublic ");

                    sb.Append($"{propertyType.Name}.{propertyType.Name}DTO");

                    sb.Append(" ");
                    sb.Append(prop.Name);
                    sb.AppendLine(" { get; set; }");
                }
            }

            sb.AppendLine("\t\t}");
            sb.AppendLine();
            sb.AppendLine();


            sb.AppendLine("\t\t/// <summary>");
            sb.AppendLine("\t\t///");
            sb.AppendLine($"\t\t/// Converts a {entity} to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.");
            sb.AppendLine("\t\t///");
            sb.AppendLine("\t\t/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.");
            sb.AppendLine("\t\t///");
            sb.AppendLine("\t\t/// </summary>");
            sb.AppendLine("\t\tpublic " + entity + "DTO ToDTO()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\treturn new " + entity + "DTO");
            sb.AppendLine("\t\t\t{");

            foreach (PropertyInfo prop in inputDtoProperties)
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                sb.Append("\t\t\t\t");
                sb.Append(prop.Name);
                sb.Append(" = this.");
                sb.Append(prop.Name);

                if (prop != inputDtoProperties.Last())
                {
                    sb.AppendLine(",");
                }
                else
                {
                    sb.AppendLine();
                }

            }

            sb.AppendLine("\t\t\t};");
            sb.AppendLine("\t\t}");
            sb.AppendLine();
            sb.AppendLine();


            sb.AppendLine("\t\t/// <summary>");
            sb.AppendLine("\t\t///");
            sb.AppendLine($"\t\t/// Converts a {entity} list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.");
            sb.AppendLine("\t\t///");
            sb.AppendLine("\t\t/// </summary>");
            sb.AppendLine("\t\tpublic static List<" + entity + "DTO> ToDTOList(List<" + entity + "> data)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tif (data == null)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\treturn null;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();
            sb.AppendLine("\t\t\tList<" + entity + "DTO> output = new List<" + entity + "DTO>();");
            sb.AppendLine();
            sb.AppendLine("\t\t\toutput.Capacity = data.Count;");
            sb.AppendLine();
            sb.AppendLine("\t\t\tforeach (" + entity + " " + camelCaseEntity + " in data)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\toutput.Add(" + camelCaseEntity + ".ToDTO());");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();
            sb.AppendLine("\t\t\treturn output;");
            sb.AppendLine("\t\t}");
            sb.AppendLine();
            sb.AppendLine();


            sb.AppendLine("\t\t/// <summary>");
            sb.AppendLine("\t\t///");
            sb.AppendLine($"\t\t/// Converts a {entity} to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the {entity }Entity type directly.");
            sb.AppendLine("\t\t///");
            sb.AppendLine("\t\t/// </summary>");
            sb.AppendLine("\t\tpublic " + entity + "OutputDTO ToOutputDTO()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\treturn new " + entity + "OutputDTO");
            sb.AppendLine("\t\t\t{");

            foreach (PropertyInfo prop in outputDtoProperties)
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                sb.Append("\t\t\t\t");
                sb.Append(prop.Name);
                sb.Append(" = this.");
                sb.Append(prop.Name);


                //
                // Run a .ToDTO() function on object types to turn them into an DTO that the output DTO object needs.
                // 
                // Using .ToDTO() here to ensure that there are no nav properties on the child object fields.
                //
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
                   propertyType != typeof(byte[]))
                {
                    //
                    // Note - Convert to regular DTO that doesn't have nav properties because we provide one level of sub object detail.
                    //
                    // Note the null guard before we try to execute the function.
                    //
                    sb.Append("?.ToDTO()");
                }

                if (prop != outputDtoProperties.Last())
                {
                    sb.AppendLine(",");
                }
                else
                {
                    sb.AppendLine();
                }
            }

            sb.AppendLine("\t\t\t};");
            sb.AppendLine("\t\t}");
            sb.AppendLine();
            sb.AppendLine();


            sb.AppendLine("\t\t/// <summary>");
            sb.AppendLine("\t\t///");
            sb.AppendLine($"\t\t/// Converts a {entity} list to list of Output Data Transfer Object intended to be used for serializing a list of {entity} objects to avoid using the {entity} entity type directly.");
            sb.AppendLine("\t\t///");
            sb.AppendLine("\t\t/// </summary>");
            sb.AppendLine("\t\tpublic static List<" + entity + "OutputDTO> ToOutputDTOList(List<" + entity + "> data)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tif (data == null)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\treturn null;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();
            sb.AppendLine("\t\t\tList<" + entity + "OutputDTO> output = new List<" + entity + "OutputDTO>();");
            sb.AppendLine();
            sb.AppendLine("\t\t\toutput.Capacity = data.Count;");
            sb.AppendLine();
            sb.AppendLine("\t\t\tforeach (" + entity + " " + camelCaseEntity + " in data)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\toutput.Add(" + camelCaseEntity + ".ToOutputDTO());");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();
            sb.AppendLine("\t\t\treturn output;");
            sb.AppendLine("\t\t}");
            sb.AppendLine();
            sb.AppendLine();



            sb.AppendLine("\t\t/// <summary>");
            sb.AppendLine("\t\t///");
            sb.AppendLine($"\t\t/// Converts an INPUT DTO to a {entity} Object.");
            sb.AppendLine("\t\t///");
            sb.AppendLine("\t\t/// </summary>");
            sb.AppendLine("\t\tpublic static " + qualifiedEntity + " FromDTO(" + entity + "DTO dto)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\treturn new " + qualifiedEntity);
            sb.AppendLine("\t\t\t{");

            foreach (PropertyInfo prop in inputDtoProperties)
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                sb.Append("\t\t\t\t");
                sb.Append(prop.Name);
                sb.Append(" = dto.");
                sb.Append(prop.Name);

                // Default values for active and deleted fields
                if (prop.Name.ToUpper() == "ACTIVE")
                {
                    sb.Append(" ?? true");
                }
                else if (prop.Name.ToUpper() == "DELETED")
                {
                    sb.Append(" ?? false");
                }

                if (prop != inputDtoProperties.Last())
                {
                    sb.AppendLine(",");
                }
                else
                {
                    sb.AppendLine();
                }
            }

            sb.AppendLine("\t\t\t};");
            sb.AppendLine("\t\t}");
            sb.AppendLine();
            sb.AppendLine();

            sb.AppendLine("\t\t/// <summary>");
            sb.AppendLine("\t\t///");
            sb.AppendLine($"\t\t/// Applies the values from an INPUT DTO to a {entity} Object.");
            sb.AppendLine("\t\t///");
            sb.AppendLine("\t\t/// </summary>");
            sb.AppendLine("\t\tpublic void ApplyDTO(" + entity + "DTO dto)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tif (dto == null || this.id != dto.id)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t    throw new Exception(\"DTO is null or has an id mismatch.\");");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();

            foreach (PropertyInfo prop in inputDtoProperties)
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                if (prop.Name.ToUpper() == "ID")
                {
                    continue;
                }

                //
                // Active and deleted values are retained from the source if they are provided as null in the DTO
                //
                if (prop.Name.ToUpper() == "ACTIVE" || prop.Name.ToUpper() == "DELETED")
                {
                    sb.AppendLine("\t\t\tif (dto." + prop.Name + ".HasValue == true)");
                    sb.AppendLine("\t\t\t{");
                    sb.Append("\t\t\t\tthis.");
                    sb.Append(prop.Name);
                    sb.Append(" = dto.");
                    sb.Append(prop.Name);
                    sb.AppendLine(".Value;");
                    sb.AppendLine("\t\t\t}");
                }
                else
                {
                    sb.Append("\t\t\tthis.");
                    sb.Append(prop.Name);
                    sb.Append(" = dto.");
                    sb.Append(prop.Name);
                    sb.AppendLine(";");
                }
            }

            sb.AppendLine("\t\t}");
            sb.AppendLine();
            sb.AppendLine();


            //
            // Add a Clone method.  Making this one non-static for convenience.  Also not removing tenant guids here on purpose because this will be used for internal processing, not external.
            //
            sb.AppendLine("\t\t/// <summary>");
            sb.AppendLine("\t\t///");
            sb.AppendLine($"\t\t/// Creates a deep copy clone of a {entity} Object.");
            sb.AppendLine("\t\t///");
            sb.AppendLine("\t\t/// </summary>");
            sb.AppendLine("\t\tpublic " + entity + " Clone()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\t//");
            sb.AppendLine("\t\t\t// Return a cloned object without any object or list properties.");
            sb.AppendLine("\t\t\t//");
            sb.AppendLine("\t\t\treturn new " + entity + "{");

            // Include all data except tenant guid and collection properties
            foreach (PropertyInfo prop in type.GetProperties())
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType.FullName.StartsWith("System.Collections"))
                {
                    continue;
                }

                // Is this property not a foundation type?  If so, don't write it out.
                if (propertyType != typeof(string) &&
                    propertyType != typeof(int) &&
                    propertyType != typeof(long) &&
                    propertyType != typeof(DateTime) &&
                    propertyType != typeof(float) &&
                    propertyType != typeof(double) &&
                    propertyType != typeof(decimal) &&
                    propertyType != typeof(bool) &&
                    propertyType != typeof(Guid) &&
                    propertyType != typeof(byte[]))
                {
                    continue;
                }

                sb.Append("\t\t\t\t");
                sb.Append(prop.Name);
                sb.Append(" = this.");
                sb.Append(prop.Name);

                if (prop != type.GetProperties().Last())
                {
                    sb.AppendLine(",");
                }
                else
                {
                    sb.AppendLine();
                }
            }

            sb.AppendLine("\t\t\t };");
            sb.AppendLine("\t\t}");
            sb.AppendLine();


            //
            // Add methods for IAnonymousConvertible.  These are the same for all.
            //
            sb.AppendLine($@"
        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a {entity} Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {{
            return CreateAnonymous(this);
        }}

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a {entity} Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {{
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }}

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a {entity} Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {{
            return CreateMinimalAnonymous(this);
        }}
");


            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("\t\t/// <summary>");
            sb.AppendLine("\t\t///");
            sb.AppendLine($"\t\t/// Creates an anonymous object version of a {entity} Object.");
            sb.AppendLine("\t\t///");
            sb.AppendLine("\t\t/// </summary>");
            sb.AppendLine("\t\tpublic static object CreateAnonymous(" + qualifiedEntity + " " + CamelCase(entity) + ")");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\t//");
            sb.AppendLine("\t\t\t// Return a simplified object without any object or list properties.");
            sb.AppendLine("\t\t\t//");

            sb.AppendLine("\t\t\tif (" + CamelCase(entity) + " == null)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\treturn null;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();
            sb.AppendLine("\t\t\treturn new {");

            // Include all data except tenant guid and collection properties
            foreach (PropertyInfo prop in type.GetProperties())
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                if (prop.Name.Trim().ToUpper() == "TENANTGUID")
                {
                    continue;
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType.FullName.StartsWith("System.Collections"))
                {
                    continue;
                }

                // Is this property not a foundation type?  If so, don't write it out.
                if (propertyType != typeof(string) &&
                    propertyType != typeof(int) &&
                    propertyType != typeof(long) &&
                    propertyType != typeof(DateTime) &&
                    propertyType != typeof(float) &&
                    propertyType != typeof(double) &&
                    propertyType != typeof(decimal) &&
                    propertyType != typeof(bool) &&
                    propertyType != typeof(Guid) &&
                    propertyType != typeof(byte[]))
                {
                    continue;
                }

                sb.Append("\t\t\t\t");
                sb.Append(prop.Name);
                sb.Append(" = " + CamelCase(entity) + ".");
                sb.Append(prop.Name);

                if (prop != type.GetProperties().Last())
                {
                    sb.AppendLine(",");
                }
                else
                {
                    sb.AppendLine();
                }
            }

            sb.AppendLine("\t\t\t };");
            sb.AppendLine("\t\t}");
            sb.AppendLine();


            if (dataVisibilityEnabled == true)
            {
                sb.AppendLine("\t\t/// <summary>");
                sb.AppendLine("\t\t///");
                sb.AppendLine($"\t\t/// Creates an anonymous object version of a {entity} Object with additional write permission details attached as new properties.");
                sb.AppendLine("\t\t///");
                sb.AppendLine("\t\t/// </summary>");
                sb.AppendLine("\t\tpublic static object CreateAnonymous(" + qualifiedEntity + "WithWritePermissionDetails " + CamelCase(entity) + ")");
                sb.AppendLine("\t\t{");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\t// Return a simplified object without any object or list properties.");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\tif (" + CamelCase(entity) + " == null)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\treturn null;");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();

                sb.AppendLine("\t\t\treturn new {");

                // Include all data except tenant guid and collection properties
                foreach (PropertyInfo prop in type.GetProperties())
                {
                    // Check for [NotMapped] attribute
                    if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                    {
                        continue; // Skip properties with [NotMapped]
                    }

                    if (prop.Name.Trim().ToUpper() == "TENANTGUID")
                    {
                        continue;
                    }

                    Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    if (propertyType.FullName.StartsWith("System.Collections"))
                    {
                        continue;
                    }

                    // Is this property not a foundation type?  If so, don't write it out.
                    if (propertyType != typeof(string) &&
                        propertyType != typeof(int) &&
                        propertyType != typeof(long) &&
                        propertyType != typeof(DateTime) &&
                        propertyType != typeof(float) &&
                        propertyType != typeof(double) &&
                        propertyType != typeof(decimal) &&
                        propertyType != typeof(bool) &&
                        propertyType != typeof(Guid) &&
                        propertyType != typeof(byte[]))
                    {
                        continue;
                    }

                    sb.Append("\t\t\t\t");
                    sb.Append(prop.Name);
                    sb.Append(" = " + CamelCase(entity) + ".");
                    sb.Append(prop.Name);

                    sb.AppendLine(",");
                }
                sb.AppendLine("\t\t\t\tisWriteable = " + CamelCase(entity) + ".isWriteable,");
                sb.AppendLine("\t\t\t\tcanChangeHierarchy = " + CamelCase(entity) + ".canChangeHierarchy,");
                sb.AppendLine("\t\t\t\tcanChangeOwner = " + CamelCase(entity) + ".canChangeOwner,");
                sb.AppendLine("\t\t\t\tnotWriteableReason = " + CamelCase(entity) + ".notWriteableReason");

                sb.AppendLine("\t\t\t };");
                sb.AppendLine("\t\t}");
                sb.AppendLine();
            }


            sb.AppendLine("\t\t/// <summary>");
            sb.AppendLine("\t\t///");
            sb.AppendLine($"\t\t/// Creates an anonymous object version of a {entity} Object with first level sub ojbects.");
            sb.AppendLine("\t\t///");
            sb.AppendLine("\t\t/// </summary>");
            sb.AppendLine("\t\tpublic static object CreateAnonymousWithFirstLevelSubObjects(" + entity + " " + CamelCase(entity) + ")");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\t//");
            sb.AppendLine("\t\t\t// Return a simplified object with simple first level sub objects.");
            sb.AppendLine("\t\t\t//");
            sb.AppendLine("\t\t\tif (" + CamelCase(entity) + " == null)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\treturn null;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();

            sb.AppendLine("\t\t\treturn new {");

            // Include all data except tenant guid and collection properties
            foreach (PropertyInfo prop in type.GetProperties())
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                if (prop.Name.Trim().ToUpper() == "TENANTGUID")
                {
                    continue;
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType.FullName.StartsWith("System.Collections"))
                {
                    continue;
                }

                //
                // If this is a collection property, then jump over it.
                //
                if (propertyType.FullName.StartsWith("System.Collections"))
                {
                    continue;
                }


                sb.Append("\t\t\t\t");

                // Objects get mapped as minimal anonymous objects.
                // Regular data types get mapped directly.
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
                        propertyType != typeof(byte[]))
                {
                    sb.Append(prop.Name + " = " + propertyType.Name + ".CreateMinimalAnonymous(" + CamelCase(entity) + "." + prop.Name + ")");
                }
                else
                {
                    sb.Append(prop.Name + " = " + CamelCase(entity) + "." + prop.Name);
                }


                if (prop != type.GetProperties().Last())
                {
                    sb.AppendLine(",");
                }
                else
                {
                    sb.AppendLine();
                }
            }
            sb.AppendLine("\t\t\t };");
            sb.AppendLine("\t\t}");
            sb.AppendLine();


            if (dataVisibilityEnabled == true)
            {

                sb.AppendLine("\t\t/// <summary>");
                sb.AppendLine("\t\t///");
                sb.AppendLine($"\t\t/// Creates an anonymous object version of a {entity} Object that also includes write permission details as additional properties.");
                sb.AppendLine("\t\t///");
                sb.AppendLine("\t\t/// </summary>");
                sb.AppendLine("\t\tpublic static object CreateAnonymousWithFirstLevelSubObjects(" + qualifiedEntity + "WithWritePermissionDetails " + CamelCase(entity) + ")");
                sb.AppendLine("\t\t{");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\t// Return a simplified object with simple first level sub objects.");
                sb.AppendLine("\t\t\t//");
                sb.AppendLine("\t\t\tif (" + CamelCase(entity) + " == null)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\treturn null;");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();

                sb.AppendLine("\t\t\treturn new {");

                // Include all data except tenant guid and collection properties
                foreach (PropertyInfo prop in type.GetProperties())
                {
                    // Check for [NotMapped] attribute
                    if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                    {
                        continue; // Skip properties with [NotMapped]
                    }

                    if (prop.Name.Trim().ToUpper() == "TENANTGUID")
                    {
                        continue;
                    }

                    Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    if (propertyType.FullName.StartsWith("System.Collections"))
                    {
                        continue;
                    }

                    //
                    // If this is a collection property, then jump over it.
                    //
                    if (propertyType.FullName.StartsWith("System.Collections"))
                    {
                        continue;
                    }

                    sb.Append("\t\t\t\t");

                    // Objects get mapped as minimal anonymous objects, regular data types just get mapped directly.
                    if (propertyType != typeof(string) &&
                            propertyType != typeof(int) &&
                            propertyType != typeof(long) &&
                            propertyType != typeof(DateTime) &&
                            propertyType != typeof(float) &&
                            propertyType != typeof(double) &&
                            propertyType != typeof(decimal) &&
                            propertyType != typeof(bool) &&
                            propertyType != typeof(Guid) &&
                            propertyType != typeof(byte[]))
                    {
                        sb.Append(prop.Name + " = " + propertyType.Name + ".CreateMinimalAnonymous(" + CamelCase(entity) + "." + prop.Name + ")");
                    }
                    else
                    {
                        sb.Append(prop.Name + " = " + CamelCase(entity) + "." + prop.Name);
                    }

                    sb.AppendLine(",");
                }

                sb.AppendLine("\t\t\t\tisWriteable = " + CamelCase(entity) + ".isWriteable,");
                sb.AppendLine("\t\t\t\tcanChangeHierarchy = " + CamelCase(entity) + ".canChangeHierarchy,");
                sb.AppendLine("\t\t\t\tcanChangeOwner = " + CamelCase(entity) + ".canChangeOwner,");
                sb.AppendLine("\t\t\t\tnotWriteableReason = " + CamelCase(entity) + ".notWriteableReason");

                sb.AppendLine("\t\t\t };");
                sb.AppendLine("\t\t}");
                sb.AppendLine();
            }


            sb.AppendLine("\t\t/// <summary>");
            sb.AppendLine("\t\t///");
            sb.AppendLine($"\t\t/// Creates an minimal anonymous object version of a {entity} Object.  This has just id, name, and description properties.");
            sb.AppendLine("\t\t///");
            sb.AppendLine("\t\t/// </summary>");

            sb.AppendLine("\t\tpublic static object CreateMinimalAnonymous(" + entity + " " + CamelCase(entity) + ")");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\t//");
            sb.AppendLine("\t\t\t// Return a very minimal object.");
            sb.AppendLine("\t\t\t//");
            sb.AppendLine("\t\t\tif (" + CamelCase(entity) + " == null)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\treturn null;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();

            sb.AppendLine("\t\t\treturn new {");

            //
            // Include all data except tenant guid and collection properties
            //

            bool nameFound = false;
            bool descriptionFound = false;

            foreach (PropertyInfo prop in type.GetProperties())
            {
                // Check for [NotMapped] attribute
                if (prop.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    continue; // Skip properties with [NotMapped]
                }

                if (prop.Name.Trim().ToUpper() == "TENANTGUID")
                {
                    continue;
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType.FullName.StartsWith("System.Collections"))
                {
                    continue;
                }

                //
                // Is this property not a basic system type?  If so, don't write it out.
                //
                if (propertyType != typeof(string) &&
                    propertyType != typeof(int) &&
                    propertyType != typeof(long) &&
                    propertyType != typeof(DateTime) &&
                    propertyType != typeof(float) &&
                    propertyType != typeof(double) &&
                    propertyType != typeof(decimal) &&
                    propertyType != typeof(bool) &&
                    propertyType != typeof(Guid) &&
                    propertyType != typeof(byte[]))
                {
                    continue;
                }


                //
                // Minimal object should have at least id, and name.  Add description if it is there too.
                //
                // id should always be there.  If no name is there, then add it in.
                //
                if (prop.Name == "id" ||
                    prop.Name == "name" ||
                    prop.Name == "description")
                {
                    if (prop.Name == "name")
                    {
                        nameFound = true;
                    }

                    if (prop.Name == "description")
                    {
                        descriptionFound = true;
                    }


                    sb.Append("\t\t\t\t");

                    sb.Append(prop.Name + " = " + CamelCase(entity) + "." + prop.Name);

                    if (prop != type.GetProperties().Last())
                    {
                        sb.AppendLine(",");
                    }
                    else
                    {
                        if (nameFound == true && descriptionFound == true)
                        {
                            // done adding fields
                            sb.AppendLine();
                        }
                        else
                        {
                            // We're going to add name and/or description
                            sb.AppendLine(",");
                        }
                    }
                }
            }

            //
            // If the table has no name field, then add in a name property mapped with other field data.
            //
            if (nameFound == false)
            {
                sb.Append("\t\t\t\tname = ");

                // If there is a display name field defined, then map that to the name field we are creating.
                if (scriptGenTable != null && scriptGenTable.displayNameFieldList.Count > 0)
                {

                    //
                    // Create a name from all the parts of the display name field list.  It will create a string by joining all elements into an array and then removing nulls and whitespace before turning into a csv line
                    //
                    sb.Append("string.Join(\", \", new[] { ");
                    for (int i = 0; i < scriptGenTable.displayNameFieldList.Count; i++)
                    {
                        if (i > 0)
                        {
                            // Add in a comma and a space
                            sb.Append(", ");
                        }

                        sb.Append(CamelCase(entity) + "." + scriptGenTable.displayNameFieldList[i].name);
                    }

                    sb.Append("}.Where(s => !string.IsNullOrWhiteSpace(s)))");
                }
                else
                {
                    //
                    // No display name fields defined.  Just use the first string field, falling back to .id if there isn't one.
                    //
                    var firstStringField = scriptGenTable.GetFirstStringField();

                    if (firstStringField != null)
                    {
                        sb.Append(CamelCase(entity) + "." + firstStringField.name);
                    }
                    else
                    {
                        // Have nothing better to use here...
                        sb.Append(CamelCase(entity) + ".id");
                    }
                }

                if (descriptionFound == true)
                {
                    // done adding fields
                    sb.AppendLine();
                }
                else
                {
                    // We're going to add description field next
                    sb.AppendLine(",");
                }
            }

            //
            // If the table has no description field, then add in a description property mapped with other field data.
            //
            if (descriptionFound == false)
            {
                List<Database.Table.Field> stringFields = scriptGenTable.GetStringFields();

                sb.Append("\t\t\t\tdescription = ");

                if (stringFields != null && stringFields.Count > 0)
                {
                    // Create a description built up from up to the first 3 string fields.    It will create a string by joining all elements into an array and then removing nulls and whitespace before turning into a csv line
                    sb.Append("string.Join(\", \", new[] { ");
                    for (int i = 0; i < stringFields.Count && i < 3; i++)
                    {
                        if (i > 0)
                        {
                            // Add in a comma and a space
                            sb.Append(", ");
                        }

                        sb.Append(CamelCase(entity) + "." + stringFields[i].name);
                    }
                    sb.Append("}.Where(s => !string.IsNullOrWhiteSpace(s)))");
                }
                else
                {
                    // Have nothing better to use here than .id because there are no string fields..
                    sb.Append(CamelCase(entity) + ".id");
                }

                // No more fields to add
                sb.AppendLine("");
            }

            sb.AppendLine("\t\t\t };");
            sb.AppendLine("\t\t}");

            sb.AppendLine("\t}");
            sb.AppendLine("}");

            return sb.ToString();

            static void AddUserListFetchingLines(string variableName, string module, Database.Table scriptGenTable, StringBuilder sb)
            {
                if (scriptGenTable.database.dataVisibilityEnabled == true)
                {
                    sb.AppendLine($"                // Note that this system has data visibility enabled, so it gets its change history users from its Context class.");
                    sb.AppendLine($"                {variableName}.user = await {module}Context.ChangeHistoryMultiTenant.GetChangeHistoryUserAsync({variableName}Audit.userId.Value, _tenantGuidForVersionInquiry, cancellationToken).ConfigureAwait(false);");
                }
                else if (scriptGenTable.database.multiTenantEnabled == true)
                {
                    sb.AppendLine($"                // Note that this system has multi tenancy enabled but not data visibility, so it gets its change history users from the security module by linking to tenant users.");
                    sb.AppendLine($"                {variableName}.user = await Foundation.Security.ChangeHistoryMultiTenant.GetChangeHistoryUserAsync({variableName}Audit.userId.Value, _tenantGuidForVersionInquiry, cancellationToken).ConfigureAwait(false);");
                }
                else
                {
                    // No DV, no MT, so we can just use the standard method.
                    sb.AppendLine($"                // Note that this system is has neither multi tenancy or data visibility enabled, so it gets its change history users from the security module, and gets all users.");
                    sb.AppendLine($"                {variableName}.user = await Foundation.Security.ChangeHistory.GetChangeHistoryUserAsync({variableName}Audit.userId.Value, cancellationToken).ConfigureAwait(false);");
                }
            }
        }


        public static void BuildEntityExtensionImplementationFromEntityFrameworkContext(string moduleName, Type contextType, DatabaseGenerator.Database database, string filePath = "c:\\temp", string databaseObjectNamespace = "Database", bool ignoreFoundationServices = false, string entityExtensionRootNamespace = "Foundation")
        {
            if (filePath.EndsWith("\\") == false)
            {
                filePath = filePath + "\\";
            }

            System.IO.Directory.CreateDirectory(filePath + moduleName);
            System.IO.Directory.CreateDirectory(filePath + moduleName + "\\Models");
            System.IO.Directory.CreateDirectory(filePath + moduleName + "\\Models\\EntityExtensions");

            foreach (PropertyInfo prop in contextType.GetProperties())
            {
                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType.GenericTypeArguments != null && propertyType.GenericTypeArguments.Length > 0)
                {
                    string entityName = propertyType.GenericTypeArguments[0].Name;

                    string entityExtensionCode = "";

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
                    // If table name ends with 'Statu' or 'Campu' then look again with an s on the end.  This is the EF6 pluralizer that does this.
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
                        var realName = type.Name.Replace("Datum", "Data");

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
                        entityExtensionCode = BuildDefaultEntityExtensionImplementation(moduleName, type, scriptGenTable, "id", databaseObjectNamespace, ignoreFoundationServices, entityExtensionRootNamespace);

                        string plural = prop.Name;

                        System.IO.File.WriteAllText(filePath + moduleName + "\\Models\\EntityExtensions\\" + entityName + "Extension.cs", entityExtensionCode);
                    }
                }
            }

            return;
        }
    }
}