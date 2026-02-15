using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using static Foundation.CodeGeneration.DatabaseGenerator;
using static Foundation.StringUtility;

namespace Foundation.CodeGeneration
{
    public partial class AngularServiceGenerator : CodeGenerationBase
    {
        /// <summary>
        /// 
        /// This function creates the Angular Data Services for the database and context provided.
        /// 
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="contextType"></param>
        /// <param name="database"></param>
        /// <param name="filePath"></param>
        /// <param name="addAuthorization"></param>
        public static void BuildAngularServiceImplementationFromEntityFrameworkContext(string moduleName, Type contextType, DatabaseGenerator.Database database, string filePath = "c:\\temp", bool addAuthorization = true)
        {
            if (filePath.EndsWith("\\") == false)
            {
                filePath = filePath + "\\";
            }

            System.IO.Directory.CreateDirectory(filePath + moduleName);
            System.IO.Directory.CreateDirectory(filePath + moduleName + "\\angular");
            System.IO.Directory.CreateDirectory(filePath + moduleName + $"\\angular\\{moduleName.ToLower()}-data-services");

            StringBuilder allDataServiceCode = new StringBuilder();             // to build a single file with all code in it.

            foreach (PropertyInfo prop in contextType.GetProperties())
            {
                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType.GenericTypeArguments != null && propertyType.GenericTypeArguments.Length > 0)
                {
                    string entityName = propertyType.GenericTypeArguments[0].Name;


                    string angularDataServiceCode = "";

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

                    //
                    // Generate the Angular services to interact with the schema
                    //
                    if (scriptGenTable != null && scriptGenTable.excludeFromCodeGeneration == false)
                    {
                        angularDataServiceCode = BuildAngularDataServiceImplementation(moduleName, type, scriptGenTable, addAuthorization);


                        string camelCaseName = CamelCase(type.Name, false);

                        // Fix nonsense with some pluralization
                        if (camelCaseName.EndsWith("Statu") == true || camelCaseName.EndsWith("Campu") == true)
                        {
                            camelCaseName += "s";
                        }

                        string pluralName = Pluralize(entityName);

                        // use the script gen table name as the angular name here.  It won't need a name fix
                        string angularName = StringUtility.ConvertToAngularComponentName(scriptGenTable.name);


                        System.IO.File.WriteAllText(filePath + moduleName + $"\\angular\\{moduleName.ToLower()}-data-services\\" + angularName + ".service.ts", angularDataServiceCode);

                        allDataServiceCode.AppendLine(angularDataServiceCode);
                    }
                }
            }

            //
            // Write a single file with all data service code.
            //
            System.IO.File.WriteAllText(filePath + moduleName + $"\\angular\\{moduleName}.all-data-service-code.ts", allDataServiceCode.ToString());

            //
            // Generate the data service manager service
            //
            string dataServiceManagerServiceCode = BuildAngularDataServiceManagerImplementation(moduleName, contextType, database);
            System.IO.File.WriteAllText(filePath + moduleName + $"\\angular\\{moduleName.ToLower()}-data-services\\{moduleName.ToLower()}-data-service-manager.service.ts", dataServiceManagerServiceCode);


            //
            // Create the module helper file to cut and paste the service definitions from
            //
            string appServiceListing = GenerateServiceAppComponentListing(moduleName, contextType, database);
            System.IO.File.WriteAllText(filePath + moduleName + $"\\angular\\{moduleName}.data.service.list.for.app.modules.ts.txt", appServiceListing);

            return;
        }

        private static string BuildAngularDataServiceManagerImplementation(string moduleName, Type contextType, Database database)
        {
            StringBuilder importsSB = new StringBuilder();
            StringBuilder constructorInjectionsSB = new StringBuilder();
            StringBuilder clearCacheCommandSB = new StringBuilder();

            importsSB.AppendLine(GenerateCodeGenDisclaimer(moduleName, moduleName));

            importsSB.AppendLine("import {Injectable} from '@angular/core';");

            foreach (PropertyInfo prop in contextType.GetProperties())
            {
                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType.GenericTypeArguments != null && propertyType.GenericTypeArguments.Length > 0)
                {
                    string entityName = propertyType.GenericTypeArguments[0].Name;

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

                    //
                    // Generate the lines for this table
                    //
                    if (scriptGenTable != null && scriptGenTable.excludeFromCodeGeneration == false)
                    {
                        string camelCaseName = CamelCase(type.Name, false);
                        if (camelCaseName.EndsWith("Statu") == true || camelCaseName.EndsWith("Campu") == true)
                        {
                            camelCaseName += "s";
                        }

                        string pluralName = Pluralize(entityName);
                        string angularName = StringUtility.ConvertToAngularComponentName(entityName);
                        if (angularName.EndsWith("statu") == true || angularName.EndsWith("campu") == true)
                        {
                            angularName += "s";
                        }

                        //
                        // Add the import
                        //
                        importsSB.AppendLine($"import {{{entityName}Service}} from  './{angularName}.service';");


                        //
                        // Add the service injection.  Make them public so any component using this service has easy access to all data services.
                        //
                        if (constructorInjectionsSB.Length > 0)
                        {
                            constructorInjectionsSB.Append("              , ");
                        }
                        constructorInjectionsSB.AppendLine($"public {camelCaseName}Service: {entityName}Service");


                        //
                        // Add the clear cache command
                        //
                        clearCacheCommandSB.AppendLine($"        this.{camelCaseName}Service.ClearAllCaches();");
                    }
                }
            }

            StringBuilder dataServiceManagerSB = new StringBuilder();

            // Add in the imports
            dataServiceManagerSB.AppendLine(importsSB.ToString());

            // Add in the basic service framing
            dataServiceManagerSB.Append($@"
@Injectable({{
  providedIn: 'root'
}})
export class {moduleName}DataServiceManagerService  {{

    constructor(");

            //
            // Add in the constructor service injections
            //
            dataServiceManagerSB.Append(constructorInjectionsSB.ToString());

            //
            // Close the constructor and start the ClearAllCaches function
            //
            dataServiceManagerSB.AppendLine(@") { }  


    public ClearAllCaches() {
");
            //
            // Add in the service clear cache commands
            //
            dataServiceManagerSB.Append(clearCacheCommandSB.ToString());

            //
            // close the service
            //
            dataServiceManagerSB.Append(@"    }
}");

            return dataServiceManagerSB.ToString();
        }


        protected static string BuildAngularDataServiceImplementation(string module, Type type, DatabaseGenerator.Database.Table scriptGenTable, bool addAuthorization = true)
        {
            StringBuilder sb = new StringBuilder();

            bool multiTenancyEnabled = false;
            bool dataVisibilityEnabled = false;

            string entity = scriptGenTable.name;
            string plural = Pluralize(entity);

            if (scriptGenTable != null)
            {
                multiTenancyEnabled = scriptGenTable.IsMultiTenantEnabled();
                dataVisibilityEnabled = scriptGenTable.IsDataVisibilityEnabled();
            }

            sb.AppendLine(GenerateCodeGenDisclaimer(entity, module));

            //
            // Note - even if not using services with secure controllers, we will use the error handling functionality in secure end point base.  This means you'll need
            // to stub out an auth service that does nothing in those cases.
            //
            sb.AppendLine(@"import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, BehaviorSubject, catchError, throwError, lastValueFrom, map } from 'rxjs';
import { shareReplay, tap } from 'rxjs/operators';
import { UtilityService } from '../utility-services/utility.service'
import { AlertService } from '../services/alert.service';
import { AuthService } from '../services/auth.service';
import { SecureEndpointBase } from '../services/secure-endpoint-base.service';");

            //
            // Import the data object for each FK - get a unique list of them
            //
            List<string> uniqueForeignKeyTables = new List<string>();
            foreach (Database.Table.ForeignKey fk in scriptGenTable.foreignKeys)
            {
                //
                // Ignore self referencing FKs
                //
                if (fk.targetTable.name == scriptGenTable.name)
                {
                    continue;
                }


                //
                // Do we have this one yet?
                //
                if (uniqueForeignKeyTables.Contains(fk.targetTable.name) == false)
                {
                    uniqueForeignKeyTables.Add(fk.targetTable.name);
                }
            }

            foreach (string foreignKeyTable in uniqueForeignKeyTables)
            {
                sb.AppendLine($"import {{ {foreignKeyTable}Data }} from './{StringUtility.ConvertToAngularComponentName(foreignKeyTable)}.service';");
            }


            //
            // Import the data object and service for all tables that links to this table so we can get their children
            //
            List<string> tablesThatLinkHere = new List<string>();
            List<Database.Table.ForeignKey> foreignKeysThatLinkHere = new List<Database.Table.ForeignKey>();

            foreach (Database.Table table in scriptGenTable.database.tables)
            {

                //
                // Note - not jumping ourselves because we want to handle self refernecing FKs
                //
                // Jump over ourselves
                //if (table == scriptGenTable)
                //{
                //    continue;
                //}

                //
                // Check each other targetTable for a link to the table we are processing
                //
                foreach (Database.Table.ForeignKey fk in table.foreignKeys)
                {
                    //
                    // Does this FK point to us?
                    //
                    if (fk.targetTable == scriptGenTable)
                    {
                        //
                        // Add the table that contains the FK link to us to the list.
                        //
                        if (tablesThatLinkHere.Contains(table.name) == false)
                        {
                            tablesThatLinkHere.Add(table.name);
                        }

                        //
                        // Always add the foreign key to the FKs that link here list
                        //
                        foreignKeysThatLinkHere.Add(fk);
                    }
                }
            }

            //
            // Make sure we import the data object and service for all tables that have FKs that link here.
            //
            foreach (string tableToLinkTo in tablesThatLinkHere)
            {
                //
                // Exclude writing import lines for ourself if this is a self referencing FK because this file defines those types.
                //
                if (tableToLinkTo != scriptGenTable.name)
                {
                    sb.AppendLine($"import {{ {tableToLinkTo}Service, {tableToLinkTo}Data }} from './{StringUtility.ConvertToAngularComponentName(tableToLinkTo)}.service';");
                }
            }


            sb.AppendLine();
            sb.AppendLine("const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit");

            sb.AppendLine(GenerateQueryParamsClass(type, scriptGenTable));

            sb.AppendLine(GenerateDataSetterOutputClass(type, scriptGenTable));

            //
            // Generate VersionInformation interface for version-controlled entities
            //
            if (scriptGenTable.IsVersionControlEnabled() == true)
            {
                sb.AppendLine(@"
//
// Version history information returned from version history API endpoints.
// Matches server-side VersionInformation<T> structure.
//
export interface VersionInformationUser {
    id: bigint | number;
    userName: string;
    firstName: string | null;
    middleName: string | null;
    lastName: string | null;
}

export interface VersionInformation<T> {
    timeStamp: string;           // ISO 8601
    user: VersionInformationUser;
    versionNumber: number;
    data: T | null;
}
");
            }

            sb.AppendLine(GenerateDataGetterOutputClass(type, scriptGenTable, tablesThatLinkHere, foreignKeysThatLinkHere));

            sb.AppendLine(@"@Injectable({
  providedIn: 'root'
})");

            // Note that the secure end point base doesn't enforce auth headers being present, so it is being used for both auth enabled and non-auth enabled modes.
            // What it provides is error handling.   Keeping name as-is for now because its most common use case will be for secure end points.
            sb.AppendLine("export class " + entity + "Service extends SecureEndpointBase {");

            sb.AppendLine();
            sb.AppendLine($"    private static _instance: {entity}Service;");


            sb.AppendLine(@"    private listCache: Map<string, Observable<Array<" + entity + @"Data>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<" + entity + @"BasicListData>>>;
    private recordCache: Map<string, Observable<" + entity + @"Data>>;

");


            sb.AppendLine(@"    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,");


            List<string> serviceImportsAdded = new List<string>();
            foreach (Database.Table.ForeignKey fk in foreignKeysThatLinkHere)
            {
                //
                // Skip over any Fks to ourselves, or ones we've already added
                //
                if (scriptGenTable == fk.sourceTable ||
                    serviceImportsAdded.Contains(fk.sourceTable.name))
                {
                    continue;
                }

                sb.AppendLine($"        private {CamelCase(fk.sourceTable.name, false)}Service: {fk.sourceTable.name}Service,");
                serviceImportsAdded.Add(fk.sourceTable.name);
            }

            sb.AppendLine(@"        @Inject('BASE_URL') private baseUrl: string) {");

            sb.AppendLine("        super(http, alertService, authService);");
            sb.AppendLine();


            sb.AppendLine($@"        this.listCache = new Map<string, Observable<Array<{entity}Data>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<{entity}BasicListData>>>();
        this.recordCache = new Map<string, Observable<{entity}Data>>();

        {entity}Service._instance = this;
    }}

    public static get Instance(): {entity}Service {{
      return {entity}Service._instance;
    }}

");


            sb.AppendLine(@"    public ClearListCaches(config: " + entity + @"QueryParameters | null = null) {

        const configHash = this.getConfigHash(config);

        if (this.listCache.has(configHash)) {
          this.listCache.delete(configHash);
        }

        if (this.rowCountCache.has(configHash)) {
            this.rowCountCache.delete(configHash);
        }

        if (this.basicListDataCache.has(configHash)) {
            this.basicListDataCache.delete(configHash);
        }
    }


    public ClearRecordCache(id: bigint | number, includeRelations: boolean = true) {

        const configHash = this.utilityService.hashCode(`_${id}_${includeRelations}`);

        if (this.recordCache.has(configHash)) {
            this.recordCache.delete(configHash);
        }
    }


    public ClearAllCaches() {
        this.listCache.clear();
        this.rowCountCache.clear();
        this.basicListDataCache.clear();
        this.recordCache.clear();
    }

");

            sb.AppendLine(GenerateDataToSubmitDataConverter(entity, plural, scriptGenTable, type));

            sb.AppendLine(GenerateRecordGetter(entity, plural, addAuthorization));

            sb.AppendLine(GenerateListGetter(entity, plural, addAuthorization));

            sb.AppendLine(GenerateRowCountGetter(entity, plural, addAuthorization));

            sb.AppendLine(GenerateBasicListDataGetter(entity, plural, addAuthorization));

            sb.AppendLine(GenerateSetterMethods(entity, plural, scriptGenTable, addAuthorization));

            sb.AppendLine(GenerateHelperMethods(entity));

            if (addAuthorization == true)
            {
                sb.AppendLine(GenerateCanReadAndWriteMethods(module, entity, plural, scriptGenTable));
            }


            //
            // Generate the methods for child table data access
            //
            foreach (Database.Table.ForeignKey fk in foreignKeysThatLinkHere)
            {
                //
                // Skip over any self referencing FKs 
                //
                if (fk.targetTable == fk.sourceTable)
                {
                    continue;
                }


                string sourceTableName = fk.sourceTable.name;
                string sourceFieldNameWithoutId = fk.field.name;

                if (sourceFieldNameWithoutId.EndsWith("Id") == true)
                {
                    sourceFieldNameWithoutId = sourceFieldNameWithoutId.Substring(0, sourceFieldNameWithoutId.Length - 2);
                }
                //
                // Most normal FKs will have a source field name that matches this table.  For those, change 
                // the source field name back to the destination table name.  
                //
                // However, if the FK has a different field name for some non-direct purpose like a parent reference,
                // then use the name of the field from the FK's source table
                //
                string primaryNameToUsePascalCase;

                if (sourceFieldNameWithoutId.Trim().ToUpper() == fk.targetTable.name.Trim().ToUpper())
                {
                    // Pascal case name of the table is the outcome.
                    //
                    primaryNameToUsePascalCase = sourceTableName;
                }
                else
                {
                    //
                    // Pascal case name of the source table and the source field name is the outcome.
                    //
                    primaryNameToUsePascalCase = $"{sourceTableName}{CamelCaseToPascalCase(sourceFieldNameWithoutId)}";
                }

                string pluralCamelCasePrimaryName = Pluralize(CamelCase(primaryNameToUsePascalCase, false));




                string camelCaseNameForEntity = CamelCase(entity, false);
                string camelCaseNameForLinkTable = CamelCase(sourceTableName, false);

                sb.AppendLine($@"
    public Get{Pluralize(primaryNameToUsePascalCase)}For{entity}({camelCaseNameForEntity}Id: number | bigint, active: boolean = true, deleted: boolean = false): Observable<{sourceTableName}Data[]> {{
        return this.{camelCaseNameForLinkTable}Service.Get{sourceTableName}List({{
            {fk.field.name}: {camelCaseNameForEntity}Id,
            active: active,
            deleted: deleted,
            includeRelations: true
        }});
    }}
");
            }


            //
            // Generate the object revival methods 
            //
            sb.AppendLine($@"
 /**
   *
   * Revives a plain object from the server into a full {entity}Data instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the {entity}Data prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when {entity}Tags$ etc.
   * are subscribed to in templates.
   *
   */
  public Revive{entity}(raw: any): {entity}Data {{
    if (!raw) return raw;

    //
    // Create a {entity}Data object instance with correct prototype
    //
    const revived = Object.create({entity}Data.prototype) as {entity}Data;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //");

            foreach (string tableName in tablesThatLinkHere)
            {
                string pluralCamelCaseNameForLinkTable = CamelCase(Pluralize(tableName), false);

                sb.AppendLine($@"    (revived as any)._{pluralCamelCaseNameForLinkTable} = null;");
                sb.AppendLine($@"    (revived as any)._{pluralCamelCaseNameForLinkTable}Promise = null;");
                sb.AppendLine($@"    (revived as any)._{pluralCamelCaseNameForLinkTable}Subject = new BehaviorSubject<{tableName}Data[] | null>(null);");
                sb.AppendLine();
            }


            sb.AppendLine($@"
    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (load{entity}XYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //");

            foreach (string tableName in tablesThatLinkHere)
            {
                string pluralTableName = Pluralize(tableName);
                string pluralCamelCaseNameForLinkTable = CamelCase(Pluralize(tableName), false);

                sb.AppendLine($@"    (revived as any).{pluralTableName}$ = (revived as any)._{pluralCamelCaseNameForLinkTable}Subject.asObservable().pipe(
        tap(() => {{
              if ((revived as any)._{pluralCamelCaseNameForLinkTable} === null && (revived as any)._{pluralCamelCaseNameForLinkTable}Promise === null) {{
                (revived as any).load{pluralTableName}();        // Need to cast to any to invoke private load method
              }}
        }}),
        shareReplay(1)
      );

    (revived as any).{pluralTableName}Count$ = {tableName}Service.Instance.Get{pluralTableName}RowCount({{{CamelCase(entity, false)}Id: (revived as any).id,
      active: true,
      deleted: false
    }});


");
            }


            //
            // Version history metadata cache and observable for version-controlled entities
            //
            if (scriptGenTable.IsVersionControlEnabled() == true)
            {
                sb.AppendLine($@"
    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<{entity}Data> | null>(null);

    (revived as any).CurrentVersionInfo$ = (revived as any)._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {{
            if ((revived as any)._currentVersionInfo === null && (revived as any)._currentVersionInfoPromise === null) {{
                (revived as any).loadCurrentVersionInfo();
            }}
        }}),
        shareReplay(1)
    );
");
            }


            sb.AppendLine($@"
    return revived;
  }}

  private Revive{entity}List(rawList: any[]): {entity}Data[] {{

    if (!rawList) {{
        return [];
    }}

    return rawList.map(raw => this.Revive{entity}(raw));
  }}
");

            // Close the class
            sb.AppendLine("}");


            return sb.ToString();
        }

        private static string GenerateDataToSubmitDataConverter(string entity, string plural, Database.Table table, Type type)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("    public ConvertTo" + entity + "SubmitData(data: " + entity + "Data): " + entity + "SubmitData {");
            sb.AppendLine();
            sb.AppendLine("        let output = new " + entity + "SubmitData();");
            sb.AppendLine();

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

                if (prop.Name.Trim().ToUpper() == "OBJECTGUID")
                {
                    continue;
                }

                if (prop.Name.Trim().ToUpper() == "PASSWORD")
                {
                    continue;
                }


                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                //
                // If this is a collection property, then jump over it.
                //
                if (propertyType.FullName.StartsWith("System.Collections"))
                {
                    continue;
                }

                Database.Table.Field field = table.GetFieldByName(prop.Name);


                // Objects get mapped as minimal anonymous objects, regular data types just get mapped directly.
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
                    // Do nothing. We are not writing object types to the setter angular object
                }
                else
                {
                    sb.AppendLine("        output." + prop.Name + " = data." + prop.Name + ";");
                }
            }
            sb.AppendLine();
            sb.AppendLine("        return output;");
            sb.AppendLine("    }");

            return sb.ToString();
        }


        private static string GenerateCanReadAndWriteMethods(string module, string entity, string plural, Database.Table scriptGenTable)
        {
            return $@"    public userIs{module}{entity}Reader(): boolean {{

        //
        // First get the overall module reading privilege
        //
        let userIs{module}{entity}Reader = this.authService.is{module}Reader;

        //
        // Next test to see if the user has a high enough read permission level to read from {module}.{plural}
        //
        if (userIs{module}{entity}Reader == true) {{
            const user = this.authService.currentUser;

            if (user != null) {{
                userIs{module}{entity}Reader = user.readPermission >= {scriptGenTable.minimumReadPermissionLevel.ToString()};
            }} else {{
                userIs{module}{entity}Reader = false;
            }}
        }}

        return userIs{module}{entity}Reader;
    }}


    public userIs{module}{entity}Writer(): boolean {{

        //
        // First get the overall module writing privilege
        //
        let userIs{module}{entity}Writer = this.authService.is{module}ReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to {module}.{plural}
        //
        if (userIs{module}{entity}Writer == true) {{
          let user = this.authService.currentUser;

          if (user != null) {{
            userIs{module}{entity}Writer = user.writePermission >= {scriptGenTable.minimumWritePermissionLevel.ToString()};
          }} else {{
            userIs{module}{entity}Writer = false;
          }}      
        }}

        return userIs{module}{entity}Writer;
    }}";
        }


        private static string GenerateSetterMethods(string entity, string plural, Database.Table scriptGenTable, bool addAuthorization)
        {
            StringBuilder sb = new StringBuilder();

            if (addAuthorization == true)
            {

                sb.AppendLine(@$"
    public Put{entity}(id: bigint | number, {CamelCase(entity, false)}: {entity}SubmitData) : Observable<{entity}Data> {{

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<{entity}Data>(this.baseUrl + 'api/{entity}/' + id.toString(), {CamelCase(entity, false)}, {{ headers: authenticationHeaders }} ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.Revive{entity}(raw)),
            catchError(error => {{
                return this.handleError(error, () => this.Put{entity}(id, {CamelCase(entity, false)}));
            }}));
    }}


    public Post{entity}({CamelCase(entity, false)}: {entity}SubmitData) : Observable<{entity}Data> {{

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<{entity}Data>(this.baseUrl + 'api/{entity}', {CamelCase(entity, false)}, {{ headers: authenticationHeaders }} ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.Revive{entity}(raw)),
            catchError(error => {{
              return this.handleError(error, () => this.Post{entity}({CamelCase(entity, false)}));
            }}));
    }}

  
    public Delete{entity}(id: bigint | number) : Observable<any> {{

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/{entity}/' + id.toString(), {{ headers: authenticationHeaders }} ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {{
                return this.handleError(error, () => this.Delete{entity}(id));
            }}));
    }}
");
            }
            else
            {
                sb.AppendLine($@"
    public Put{entity}(id: bigint | number, {CamelCase(entity, false)}: {entity}SubmitData) : Observable<{entity}Data> {{

        return this.http.put<{entity}Data>(this.baseUrl + 'api/{entity}/' + id.toString(), {CamelCase(entity, false)}).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.Revive{entity}(raw)),
            catchError(error => {{
                return this.handleError(error, () => this.Put{entity}(id, {CamelCase(entity, false)}));
            }}));
    }}


    public Post{entity}({CamelCase(entity, false)}: {entity}SubmitData) : Observable<{entity}Data> {{

        return this.http.post<{entity}Data>(this.baseUrl + 'api/{entity}', {CamelCase(entity, false)}).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.Revive{entity}(raw)),
            catchError(error => {{
                return this.handleError(error, () => this.Post{entity}({CamelCase(entity, false)}));
            }}));
    }}
  

    public Delete{entity}(id: bigint | number) : Observable<any>{{

        return this.http.delete<void>(this.baseUrl + 'api/{entity}/' + id.toString()).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {{
                return this.handleError(error, () => this.Delete{entity}(id));
            }}));
    }}
");
            }


            if (scriptGenTable.IsVersionControlEnabled() == true)
            {
                if (addAuthorization == true)
                {
                    sb.AppendLine($@"    public Rollback{entity}(id: bigint | number, versionNumber: bigint | number) : Observable<{entity}Data>{{

        let queryParams = new HttpParams();

        queryParams = queryParams.append(""id"", id.toString());
        queryParams = queryParams.append(""versionNumber"", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<{entity}Data>(this.baseUrl + 'api/{entity}/Rollback/' + id.toString(), null, {{ params: queryParams, headers: authenticationHeaders }}).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.Revive{entity}(raw)),
            catchError(error => {{
                return this.handleError(error, () => this.Rollback{entity}(id, versionNumber));
        }}));
    }}");
                }
                else
                {
                    sb.AppendLine($@"
    public Rollback{entity}(id: bigint | number, versionNumber: bigint | number) : Observable<{entity}Data>{{

        let queryParams = new HttpParams();

        queryParams = queryParams.append(""id"", id.toString());
        queryParams = queryParams.append(""versionNumber"", versionNumber.toString());

        return this.http.put<{entity}Data>(this.baseUrl + 'api/{entity}/Rollback/' + id.toString(), null, {{ params: queryParams }}).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.Revive{entity}(raw)),
            catchError(error => {{
                return this.handleError(error, () => this.Rollback"" + entity + @""(id, versionNumber));
        }}));
    }}");
                }

                //
                // Generate version history accessor methods for version-controlled entities
                //
                if (addAuthorization == true)
                {
                    sb.AppendLine($@"

    /**
     * Gets version metadata for a specific version of a {entity}.
     */
    public Get{entity}ChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<{entity}Data>> {{

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {{
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }}

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<{entity}Data>>(this.baseUrl + 'api/{entity}/' + id.toString() + '/ChangeMetadata', {{
            params: queryParams,
            headers: authenticationHeaders
        }}).pipe(
            catchError(error => {{
                return this.handleError(error, () => this.Get{entity}ChangeMetadata(id, versionNumber));
            }})
        );
    }}


    /**
     * Gets the full audit history of a {entity}.
     */
    public Get{entity}AuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<{entity}Data>[]> {{

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<{entity}Data>[]>(this.baseUrl + 'api/{entity}/' + id.toString() + '/AuditHistory', {{
            params: queryParams,
            headers: authenticationHeaders
        }}).pipe(
            catchError(error => {{
                return this.handleError(error, () => this.Get{entity}AuditHistory(id, includeData));
            }})
        );
    }}


    /**
     * Gets a specific historical version of a {entity}.
     */
    public Get{entity}Version(id: bigint | number, version: number): Observable<{entity}Data> {{

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<{entity}Data>(this.baseUrl + 'api/{entity}/' + id.toString() + '/Version/' + version.toString(), {{
            headers: authenticationHeaders
        }}).pipe(
            map(raw => this.Revive{entity}(raw)),
            catchError(error => {{
                return this.handleError(error, () => this.Get{entity}Version(id, version));
            }})
        );
    }}


    /**
     * Gets the state of a {entity} at a specific point in time.
     */
    public Get{entity}StateAtTime(id: bigint | number, time: string): Observable<{entity}Data> {{

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<{entity}Data>(this.baseUrl + 'api/{entity}/' + id.toString() + '/StateAtTime', {{
            params: queryParams,
            headers: authenticationHeaders
        }}).pipe(
            map(raw => this.Revive{entity}(raw)),
            catchError(error => {{
                return this.handleError(error, () => this.Get{entity}StateAtTime(id, time));
            }})
        );
    }}
");
                }
                else
                {
                    // Non-auth version of version history methods
                    sb.AppendLine($@"

    public Get{entity}ChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<{entity}Data>> {{
        let queryParams = new HttpParams();
        if (versionNumber !== undefined && versionNumber !== null) {{
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }}
        return this.http.get<VersionInformation<{entity}Data>>(this.baseUrl + 'api/{entity}/' + id.toString() + '/ChangeMetadata', {{ params: queryParams }}).pipe(
            catchError(error => {{ return this.handleError(error, () => this.Get{entity}ChangeMetadata(id, versionNumber)); }})
        );
    }}

    public Get{entity}AuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<{entity}Data>[]> {{
        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());
        return this.http.get<VersionInformation<{entity}Data>[]>(this.baseUrl + 'api/{entity}/' + id.toString() + '/AuditHistory', {{ params: queryParams }}).pipe(
            catchError(error => {{ return this.handleError(error, () => this.Get{entity}AuditHistory(id, includeData)); }})
        );
    }}

    public Get{entity}Version(id: bigint | number, version: number): Observable<{entity}Data> {{
        return this.http.get<{entity}Data>(this.baseUrl + 'api/{entity}/' + id.toString() + '/Version/' + version.toString()).pipe(
            map(raw => this.Revive{entity}(raw)),
            catchError(error => {{ return this.handleError(error, () => this.Get{entity}Version(id, version)); }})
        );
    }}

    public Get{entity}StateAtTime(id: bigint | number, time: string): Observable<{entity}Data> {{
        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);
        return this.http.get<{entity}Data>(this.baseUrl + 'api/{entity}/' + id.toString() + '/StateAtTime', {{ params: queryParams }}).pipe(
            map(raw => this.Revive{entity}(raw)),
            catchError(error => {{ return this.handleError(error, () => this.Get{entity}StateAtTime(id, time)); }})
        );
    }}
");
                }
            }

            return sb.ToString();
        }

        private static string GenerateHelperMethods(string entity)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(@"    private getConfigHash(config: " + entity + @"QueryParameters | any): string {

        if (!config) {
            return '_';
        }

        // Normalize the config object, excluding null and undefined properties
        const normalizedConfig = Object.keys(config)
            .sort() // Ensure consistent property order
            .reduce((obj: any, key: string) => {
                if (config[key] != null) { // Exclude null and undefined
                    obj[key] = config[key];
                }
                return obj;
            }, {});

        if (Object.keys(normalizedConfig).length > 0) {
            return this.utilityService.hashCode(JSON.stringify(normalizedConfig));
        }

        return '_';
    }");

            return sb.ToString();
        }


        private static string GenerateListGetter(string entity, string plural, bool addAuthorization)
        {
            StringBuilder sb = new StringBuilder();

            //
            // Note we are using the entity name with a 'List' suffix instead of the plural version because some words, like 'Equipment' are both singular and plural and that breaks the generation by duplicating method names.
            //
            sb.AppendLine("    public Get" + entity + "List(config: " + entity + @"QueryParameters | any = null) : Observable<Array<" + entity + "Data>> {");
            sb.AppendLine();
            sb.AppendLine(@"        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const " + CamelCase(entity, false) + @"List$ = this.request" + entity + @"List(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage(""Unable to get " + entity + @" list"", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, " + CamelCase(entity, false) + @"List$);

            return " + CamelCase(entity, false) + @"List$;
        }

        return this.listCache.get(configHash) as Observable<Array<" + entity + @"Data>>;
    }

");

            sb.AppendLine("    private request" + entity + "List(config: " + entity + "QueryParameters | any) : Observable <Array<" + entity + @"Data>> {");
            sb.AppendLine(@"
        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }");

            if (addAuthorization == true)
            {
                sb.AppendLine($@"
        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<{entity}Data>>(this.baseUrl + 'api/{plural}', {{ 
            params: queryParams, 
            headers: authenticationHeaders }}).pipe(
            map(rawList => this.Revive{entity}List(rawList)),
            catchError(error => {{
                return this.handleError(error, () => this.request{entity}List(config));
            }}));
    }}");
            }
            else
            {
                sb.AppendLine($@"
        return this.http.get<Array<{entity}Data>>(this.baseUrl + 'api/{plural}', {{ params: queryParams }}).pipe(
            map(rawList => this.Revive{entity}List(rawList)),
            catchError(error => {{
                return this.handleError(error, () => this.request{entity}List(config));
            }}));
    }}");
            }

            return sb.ToString();
        }


        private static string GenerateRowCountGetter(string entity, string plural, bool addAuthentication)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("    public Get" + plural + "RowCount(config: " + entity + @"QueryParameters | any = null) : Observable<bigint | number> {");
            sb.AppendLine();
            sb.AppendLine(@"        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const " + CamelCase(plural, false) + @"RowCount$ = this.request" + plural + @"RowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage(""Unable to get " + plural + @" row count"", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, " + CamelCase(plural, false) + @"RowCount$);

            return " + CamelCase(plural, false) + @"RowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }
");

            if (addAuthentication == true)
            {
                sb.AppendLine("    private request" + plural + "RowCount(config: " + entity + "QueryParameters | any) : Observable<bigint | number> {");
                sb.AppendLine(@"
        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/" + plural + @"/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.request" + plural + @"RowCount(config));
            }));
    }");
            }
            else
            {

                sb.AppendLine("    private request" + plural + "RowCount(config: " + entity + "QueryParameters | any) : Observable<bigint | number> {");
                sb.AppendLine(@"
        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        return this.http.get<bigint | number>(this.baseUrl + 'api/" + plural + @"/RowCount', { params: queryParams }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.request" + plural + @"RowCount(config));
            }));
    }");
            }

            return sb.ToString();
        }

        private static string GenerateBasicListDataGetter(string entity, string plural, bool addAuthorization)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("    public Get" + plural + "BasicListData(config: " + entity + @"QueryParameters | any = null) : Observable<Array<" + entity + "BasicListData>> {");
            sb.AppendLine();
            sb.AppendLine(@"        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const " + CamelCase(plural, false) + @"BasicListData$ = this.request" + plural + @"BasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage(""Unable to get " + plural + @" basic list data"", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, " + CamelCase(plural, false) + @"BasicListData$);

            return " + CamelCase(plural, false) + @"BasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<" + entity + @"BasicListData>>;
    }

");

            sb.AppendLine("    private request" + plural + "BasicListData(config: " + entity + "QueryParameters | any) : Observable<Array<" + entity + "BasicListData>> {");
            sb.AppendLine(@"
        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }
");


            if (addAuthorization == true)
            {
                sb.AppendLine(@"        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<" + entity + @"BasicListData>>(this.baseUrl + 'api/" + plural + @"/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.request" + plural + @"BasicListData(config));
            }));");
            }
            else
            {
                sb.AppendLine(@"
        return this.http.get<Array<" + entity + @"BasicListData>>(this.baseUrl + 'api/" + plural + @"/ListData', { params: queryParams }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.request" + plural + @"BasicListData(config));
            }));");
            }


            sb.AppendLine(@"
    }");

            return sb.ToString();
        }


        private static string GenerateRecordGetter(string entity, string plural, bool addAuthentication)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("    public Get" + entity + "(id: bigint | number, includeRelations: boolean = true) : Observable<" + entity + "Data> {");

            sb.AppendLine(@"
        const configHash = this.utilityService.hashCode(""_"" + id.toString() + ""_"" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const " + CamelCase(entity, false) + @"$ = this.request" + entity + @"(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage(""Unable to get " + entity + @""", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, " + CamelCase(entity, false) + @"$);

            return " + CamelCase(entity, false) + @"$;
        }

        return this.recordCache.get(configHash) as Observable<" + entity + @"Data>;
    }
");


            if (addAuthentication == true)
            {
                sb.AppendLine("    private request" + entity + "(id: bigint | number, includeRelations: boolean = true) : Observable<" + entity + @"Data> {");
                sb.AppendLine($@"
        let queryParams = new HttpParams();

        queryParams = queryParams.append(""includeRelations"", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<{entity}Data>(this.baseUrl + 'api/{entity}/' + id.toString(), {{ 
            params: queryParams, 
            headers: authenticationHeaders }}).pipe(
            map(raw => this.Revive{entity}(raw)),
            catchError(error => {{
                return this.handleError(error, () => this.request{entity}(id, includeRelations));
            }}));
    }}");
            }
            else
            {
                sb.AppendLine("    private request" + entity + "(id: bigint | number, includeRelations: boolean = true) : Observable<" + entity + @"Data> {");
                sb.AppendLine($@"
        let queryParams = new HttpParams();

        queryParams = queryParams.append(""includeRelations"", includeRelations.toString());

        return this.http.get<{entity}Data>(this.baseUrl + 'api/{entity}/' + id.toString(), {{ params: queryParams }}).pipe(
            map(raw => this.Revive{entity}(raw)),
            catchError(error => {{
                return this.handleError(error, () => this.request{entity}(id, includeRelations));
            }}));
    }}");
            }

            return sb.ToString();
        }

        private static string GenerateQueryParamsClass(Type type, Database.Table table)
        {
            string entity = table.name;  // string entity = type.Name;
            string plural = Pluralize(entity);


            StringBuilder sb = new StringBuilder();

            sb.AppendLine("//");
            sb.AppendLine("// This class defines the query parameters used for GET API endpoints that return arrays");
            sb.AppendLine(@"//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//");
            sb.AppendLine("export class " + entity + "QueryParameters {");

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
                // Don't create a password parameter
                //
                if (prop.Name == "password")
                {
                    continue;
                }

                //
                // Don't create a tenant guid parameter
                //
                if (prop.Name == "tenantGuid")
                {
                    continue;
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                Database.Table.Field field = table.GetFieldByName(prop.Name);


                if (propertyType == typeof(string) || propertyType == typeof(Guid))
                {
                    sb.AppendLine("    " + prop.Name + ": string | null | undefined = null;");
                }
                else if (propertyType == typeof(int) || propertyType == typeof(long))
                {
                    sb.AppendLine("    " + prop.Name + ": bigint | number | null | undefined = null;");
                }
                else if (propertyType == typeof(bool))
                {
                    sb.AppendLine("    " + prop.Name + ": boolean | null | undefined = null;");
                }
                else if (propertyType == typeof(DateTime) ||
                         propertyType == typeof(TimeOnly))
                {
                    //
                    // DateTime/TimeOnly are to be serialized as ISO 8601 strings with milliseconds.  For example, 2025-12-09T01:09:27.093Z. 
                    //
                    // Javascript Date object is not used here on purpose.
                    //
                    sb.AppendLine("    " + prop.Name + ": string | null | undefined = null;        // ISO 8601 (full datetime)");
                }
                else if (propertyType == typeof(DateOnly))
                {
                    //
                    // DateOnly fields use YYYY-MM-DD format only.  No time component.
                    //
                    sb.AppendLine("    " + prop.Name + ": string | null | undefined = null;        // Date only (YYYY-MM-DD)");
                }
                else if (propertyType == typeof(float) || propertyType == typeof(double) || propertyType == typeof(decimal))
                {
                    sb.AppendLine("    " + prop.Name + ": number | null | undefined = null;");
                }
            }


            sb.AppendLine("    pageSize: bigint | number | null | undefined = null;");
            sb.AppendLine("    pageNumber: bigint | number | null | undefined = null;");
            sb.AppendLine("    includeRelations: boolean | null | undefined = null;");
            sb.AppendLine("    anyStringContains: string | null | undefined = null;");

            if (table.pngRootFieldName != null)
            {
                sb.AppendLine("    imageWidth: bigint | number | null | undefined = null;");
            }

            sb.AppendLine("}");
            sb.AppendLine();

            return sb.ToString();
        }


        private static string GenerateDataGetterOutputClass(Type type, Database.Table table, List<string> tablesThatLinkHere, List<Database.Table.ForeignKey> foreignKeysThatLinkHere)
        {
            string entity = table.name;  // string entity = type.Name;
            string plural = Pluralize(entity);

            string camelCaseName = CamelCase(entity, false);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(@"export class " + entity + @"BasicListData {
  id!: bigint | number;
  name!: string;
}

");


            sb.Append($@"

//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. {entity}Children) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `{camelCaseName}.{entity}Children$` — use with `| async` in templates
//        • Promise:    `{camelCaseName}.{entity}Children`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf=""{camelCaseName}.{entity}Children$ | async""`), or
//        • Access the promise getter (`{camelCaseName}.{entity}Children` or `await {camelCaseName}.{entity}Children`)
//    - Simply reading `{camelCaseName}.{entity}Children` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await {camelCaseName}.Reload()` to refresh the entire object and clear all lazy caches.
//    - Useful after mutations or when navigating into a navigation property.
//
// 5. **Cache clearing**:
//    - Use `ClearXCache()` methods after mutations to force fresh data on next access.
//
// 6. **Nav Properties**: if loaded with 'includeRelations = true' will be data objects of their appropriate types in data only.  They
//     will need to be 'Revived' and 'Reloaded' to access their nav properties, or lazy load their children.
//
// 7. **Dates are typed as strings**: because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z"");
//
");
            sb.AppendLine("export class " + entity + "Data {");

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

                //
                // If this is a collection property, then jump over it.
                //
                if (propertyType.FullName.StartsWith("System.Collections"))
                {
                    continue;
                }

                Database.Table.Field field = table.GetFieldByName(prop.Name);


                // Objects get mapped as their data objects, regular data types just get mapped directly.
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
                    //
                    // Try to determine the source type of the field based on it's table name.  
                    //
                    //  If we can't find it, then it means it's a self referencing FK, OR an FK that has a property name that doesn't match the target table name  (less the Id part of course)
                    //
                    //  We won't deal with those more complex FKs in this property loop, but process separately below.
                    //
                    //
                    Database.Table.ForeignKey fk = null;

                    foreach (Database.Table.ForeignKey fkToCheck in table.foreignKeys)
                    {
                        if (fkToCheck.targetTable.name.ToUpper() == prop.Name.ToUpper())
                        {
                            //
                            // Simple FK.  Field name matches the target table name. 
                            //
                            fk = fkToCheck;
                            break;
                        }
                    }

                    if (fk != null)
                    {
                        //
                        // Use the table name as the link entity name.  They should be the same.  This handles simple FK relationships.
                        //
                        sb.AppendLine($"    {prop.Name}: {fk.targetTable.name}Data | null | undefined = null;          // Navigation property (populated when includeRelations=true)");
                    }
                }
                else
                {
                    if (propertyType == typeof(string) || (propertyType == typeof(Guid)))
                    {
                        if (field != null && field.nullable == false)
                        {
                            sb.AppendLine("    " + prop.Name + "!: string;");
                        }
                        else
                        {
                            sb.AppendLine("    " + prop.Name + "!: string | null;");
                        }
                    }
                    else if (propertyType == typeof(int) || (propertyType == typeof(long)))
                    {
                        if (field != null && field.nullable == false)
                        {
                            sb.AppendLine("    " + prop.Name + "!: bigint | number;");
                        }
                        else
                        {
                            sb.AppendLine("    " + prop.Name + "!: bigint | number;");
                        }
                    }
                    else if (propertyType == typeof(bool))
                    {
                        if (field != null && field.nullable == false)
                        {
                            sb.AppendLine("    " + prop.Name + "!: boolean;");
                        }
                        else
                        {
                            sb.AppendLine("    " + prop.Name + "!: boolean | null;");
                        }

                    }
                    else if (propertyType == typeof(DateTime) ||
                             propertyType == typeof(TimeOnly))
                    {
                        //
                        // DateTime/TimeOnly are to be serialized as ISO 8601 strings with milliseconds.  For example, 2025-12-09T01:09:27.093Z. 
                        //
                        // Javascript Date object is not used here on purpose.
                        //
                        if (field != null && field.nullable == false)
                        {
                            sb.AppendLine("    " + prop.Name + "!: string;      // ISO 8601 (full datetime)");
                        }
                        else
                        {
                            sb.AppendLine("    " + prop.Name + "!: string | null;   // ISO 8601 (full datetime)");
                        }

                    }
                    else if (propertyType == typeof(DateOnly))
                    {
                        //
                        // DateOnly fields use YYYY-MM-DD format only.  No time component.
                        //
                        if (field != null && field.nullable == false)
                        {
                            sb.AppendLine("    " + prop.Name + "!: string;      // Date only (YYYY-MM-DD)");
                        }
                        else
                        {
                            sb.AppendLine("    " + prop.Name + "!: string | null;   // Date only (YYYY-MM-DD)");
                        }

                    }
                    else if (propertyType == typeof(float) || (propertyType == typeof(double) || propertyType == typeof(decimal)))
                    {
                        if (field != null && field.nullable == false)
                        {
                            sb.AppendLine("    " + prop.Name + "!: number;");
                        }
                        else
                        {
                            sb.AppendLine("    " + prop.Name + "!: number | null;");
                        }

                    }
                    else
                    {
                        if (field != null && field.nullable == false)
                        {
                            sb.AppendLine("    " + prop.Name + "!: string;");
                        }
                        else
                        {
                            sb.AppendLine("    " + prop.Name + "!: string | null;");
                        }
                    }
                }
            }

            //
            // Add in complex foreign keys - self refernecing ones, or ones that have field names that don't match the target table name.
            //
            foreach (Database.Table.ForeignKey fkToCheck in table.foreignKeys)
            {
                if (fkToCheck.targetTable.name.ToUpper() == entity.ToUpper())   // Self reference?
                {
                    string navPropertyNameToUse = fkToCheck.field.name;

                    if (navPropertyNameToUse.EndsWith("Id") == true)
                    {
                        navPropertyNameToUse = navPropertyNameToUse.Substring(0, navPropertyNameToUse.Length - 2);
                    }

                    //
                    // Skip if the nav property name matches the entity name (case-insensitive).
                    // In that case, Loop 1 (the C# property iteration above) already emitted this
                    // nav property as a simple FK match. Only emit here for self-referencing FKs
                    // with a different field name prefix (e.g., parentScheduledEventId on ScheduledEvent).
                    //
                    if (navPropertyNameToUse.Trim().ToUpper() == entity.Trim().ToUpper())
                    {
                        continue;
                    }

                    sb.AppendLine($"    {navPropertyNameToUse}: {entity}Data | null | undefined = null;            // Self referencing navigation property (populated when includeRelations=true)");
                }
                else if (fkToCheck.field.name.ToUpper().Trim().StartsWith(fkToCheck.targetTable.name.ToUpper().Trim()) == false)      // Field name doesn't match target.  Likely schenario with a specialied FK link, or mulitiple fK links needing unique names
                {
                    string navPropertyNameToUse = fkToCheck.field.name;

                    if (navPropertyNameToUse.EndsWith("Id") == true)
                    {
                        navPropertyNameToUse = navPropertyNameToUse.Substring(0, navPropertyNameToUse.Length - 2);
                    }

                    sb.AppendLine($"    {navPropertyNameToUse}: {fkToCheck.targetTable.name}Data | null | undefined = null;            // Navigation property with non-standard field name (populated when includeRelations=true)");

                }
            }

            //
            // Add in lazy loading for childen.  Stat with the data arrays and promises
            //
            sb.AppendLine();
            sb.AppendLine("    //");
            sb.AppendLine("    // Private lazy-loading caches for related collections");
            sb.AppendLine("    //");

            foreach (Database.Table.ForeignKey fk in foreignKeysThatLinkHere)
            {
                //
                // Skip over any self referencing FKs because we don't need caches for these.  We use nav properties only.
                //
                if (fk.targetTable == fk.sourceTable)
                {
                    continue;
                }

                string sourceTableName = fk.sourceTable.name;
                string sourceFieldName = fk.field.name;

                if (sourceFieldName.EndsWith("Id") == true)
                {
                    sourceFieldName = sourceFieldName.Substring(0, sourceFieldName.Length - 2);
                }
                //
                // Most normal FKs will have a source field name that matches this table.  For those, change 
                // the source field name back to the destination table name.  
                //
                // However, if the FK has a different field name for some non-direct purpose like a parent reference,
                // then use the name of the field from the FK's source table
                //
                if (sourceFieldName.Trim().ToUpper() == fk.targetTable.name.Trim().ToUpper())
                {
                    sourceFieldName = sourceTableName;

                    sb.AppendLine($@"    private _{Pluralize(CamelCase(sourceFieldName, false))}: {sourceTableName}Data[] | null = null;
    private _{Pluralize(CamelCase(sourceFieldName, false))}Promise: Promise<{sourceTableName}Data[]> | null  = null;
    private _{Pluralize(CamelCase(sourceFieldName, false))}Subject = new BehaviorSubject<{sourceTableName}Data[] | null>(null);

                ");
                }
                else
                {
                    //
                    // This is a more complex link.  Use the source table name, and it's field as the child variable names
                    //
                    sb.AppendLine($@"    private _{CamelCase(sourceTableName, false)}{Pluralize(CamelCaseToPascalCase(sourceFieldName))}: {sourceTableName}Data[] | null = null;
    private _{CamelCase(sourceTableName, false)}{Pluralize(CamelCaseToPascalCase(sourceFieldName))}Promise: Promise<{sourceTableName}Data[]> | null  = null;
    private _{CamelCase(sourceTableName, false)}{Pluralize(CamelCaseToPascalCase(sourceFieldName))}Subject = new BehaviorSubject<{sourceTableName}Data[] | null>(null);
                    ");

                }
            }

            //
            // Add version history lazy-loading cache for version-controlled entities
            //
            if (table.IsVersionControlEnabled() == true)
            {
                sb.AppendLine($@"

    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<{entity}Data> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<{entity}Data>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<{entity}Data> | null>(null);
");
            }

            sb.AppendLine();
            sb.AppendLine("    //");
            sb.AppendLine("    // Public observables — use with | async in templates");
            sb.AppendLine("    // Subscription triggers lazy load if not already cached");
            sb.AppendLine("    //");
            sb.AppendLine("    // Also includes an observable for each child list to access its row count.");
            sb.AppendLine("    //");

            foreach (Database.Table.ForeignKey fk in foreignKeysThatLinkHere)
            {
                //
                // Skip over any self referencing FKs because we already have nav properties for those, and list access doesn't make sense for self pointing FKs
                //
                if (fk.targetTable == fk.sourceTable)
                {
                    continue;
                }


                string sourceTableName = fk.sourceTable.name;
                string sourceFieldNameWithoutId = fk.field.name;

                if (sourceFieldNameWithoutId.EndsWith("Id") == true)
                {
                    sourceFieldNameWithoutId = sourceFieldNameWithoutId.Substring(0, sourceFieldNameWithoutId.Length - 2);
                }
                //
                // Most normal FKs will have a source field name that matches this table.  For those, change 
                // the source field name back to the destination table name.  
                //
                // However, if the FK has a different field name for some non-direct purpose like a parent reference,
                // then use the name of the field from the FK's source table
                //
                if (sourceFieldNameWithoutId.Trim().ToUpper() == fk.targetTable.name.Trim().ToUpper())
                {
                    sourceFieldNameWithoutId = sourceTableName;

                    string targetTableNamePascalCase = CamelCaseToPascalCase(fk.targetTable.name);      // tables names should be already be Pascal case by convention, but just in case

                    string pluralSourcePascalCase = Pluralize(CamelCaseToPascalCase(sourceFieldNameWithoutId));       // Note the uncamelcasing here 

                    string pluralSourceCamelCase = Pluralize(CamelCase(sourceFieldNameWithoutId, false));

                    sb.AppendLine($@"    public {pluralSourcePascalCase}$ = this._{pluralSourceCamelCase}Subject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {{
          if (this._{Pluralize(CamelCase(sourceFieldNameWithoutId, false))} === null && this._{Pluralize(CamelCase(sourceFieldNameWithoutId, false))}Promise === null) {{
            this.load{pluralSourcePascalCase}(); // Private method to start fetch
          }}
        }}),
        shareReplay(1) // Cache last emit
    );

  
    public {pluralSourcePascalCase}Count$ = {fk.sourceTable.name}Service.Instance.Get{Pluralize(fk.sourceTable.name)}RowCount({{{fk.field.name}: this.id,
      active: true,
      deleted: false
    }});


");

                }
                else
                {
                    //
                    // More complex than simple table name link.,
                    //
                    string targetTableNamePascalCase = CamelCaseToPascalCase(fk.targetTable.name);      // tables names should be already be Pascal case by convention, but just in case


                    string nameToUse = $"{CamelCase(sourceTableName, false)}{Pluralize(CamelCaseToPascalCase(sourceFieldNameWithoutId))}";

                    sb.AppendLine($@"    public {CamelCaseToPascalCase(nameToUse)}$ = this._{nameToUse}Subject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {{
          if (this._{nameToUse} === null && this._{nameToUse}Promise === null) {{
            this.load{CamelCaseToPascalCase(nameToUse)}(); // Private method to start fetch
          }}
        }}),
        shareReplay(1) // Cache last emit
    );

  
    public {CamelCaseToPascalCase(nameToUse)}Count$ = {fk.sourceTable.name}Service.Instance.Get{Pluralize(fk.sourceTable.name)}RowCount({{{fk.field.name}: this.id,
      active: true,
      deleted: false
    }});

");
                }
            }








            sb.AppendLine($@"
  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any {entity}Data object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.{CamelCase(entity, false)}.Reload();
  //
  //  Non Async:
  //
  //     {CamelCase(entity, false)}[0].Reload().then(x => {{
  //        this.{CamelCase(entity, false)} = x;
  //    }});
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {{

    const fresh = await lastValueFrom(
      {entity}Service.Instance.Get{entity}(this.id, includeRelations)
    );

    // Merge fresh data into this instance (preserves reference)
    this.UpdateFrom(fresh as this);

    // Clear all lazy caches to force re-load on next access
    this.clearAllLazyCaches();

    return this;
  }}

");


            sb.AppendLine("  private clearAllLazyCaches(): void {");
            sb.AppendLine("     //");
            sb.AppendLine("     // Reset every collection cache and notify subscribers");
            sb.AppendLine("     //");
            foreach (Database.Table.ForeignKey fk in foreignKeysThatLinkHere)
            {
                //
                // Skip over any self referencing FKs becaue we won't have caches for those
                //
                if (fk.targetTable == fk.sourceTable)
                {
                    continue;
                }

                string sourceTableName = fk.sourceTable.name;
                string sourceFieldNameWithoutId = fk.field.name;

                if (sourceFieldNameWithoutId.EndsWith("Id") == true)
                {
                    sourceFieldNameWithoutId = sourceFieldNameWithoutId.Substring(0, sourceFieldNameWithoutId.Length - 2);
                }
                //
                // Most normal FKs will have a source field name that matches this table.  For those, change 
                // the source field name back to the destination table name.  
                //
                // However, if the FK has a different field name for some non-direct purpose like a parent reference,
                // then use the name of the field from the FK's source table
                //
                if (sourceFieldNameWithoutId.Trim().ToUpper() == fk.targetTable.name.Trim().ToUpper())
                {
                    string pluralCamelCaseTableName = Pluralize(CamelCase(sourceTableName, false));

                    sb.AppendLine($"     this._{pluralCamelCaseTableName} = null;");
                    sb.AppendLine($"     this._{pluralCamelCaseTableName}Promise = null;");
                    sb.AppendLine($"     this._{pluralCamelCaseTableName}Subject.next(null);");
                }
                else
                {
                    //
                    // More complex here.  Use source table name and source field name
                    //
                    string nameToUse = $"{CamelCase(sourceTableName, false)}{Pluralize(CamelCaseToPascalCase(sourceFieldNameWithoutId))}";



                    sb.AppendLine($"     this._{nameToUse} = null;");
                    sb.AppendLine($"     this._{nameToUse}Promise = null;");
                    sb.AppendLine($"     this._{nameToUse}Subject.next(null);");

                }



                sb.AppendLine();
            }

            //
            // Clear version history cache for version-controlled entities
            //
            if (table.IsVersionControlEnabled() == true)
            {
                sb.AppendLine("     this._currentVersionInfo = null;");
                sb.AppendLine("     this._currentVersionInfoPromise = null;");
                sb.AppendLine("     this._currentVersionInfoSubject.next(null);");
            }

            sb.AppendLine("  }");
            sb.AppendLine();

            //
            // Now create the child data getters for each table that links here
            //
            sb.AppendLine("    //");
            sb.AppendLine("    // Promise-based getters below — same lazy-load logic as observables");
            sb.AppendLine("    // Use these in component code with await or .then()");
            sb.AppendLine("    //");
            foreach (Database.Table.ForeignKey fk in foreignKeysThatLinkHere)
            {
                //
                // Skip over any self referencing FKs  - don't need promises for them.
                //
                if (fk.targetTable == fk.sourceTable)
                {
                    continue;
                }

                string sourceTableName = fk.sourceTable.name;
                string sourceFieldNameWithoutId = fk.field.name;

                if (sourceFieldNameWithoutId.EndsWith("Id") == true)
                {
                    sourceFieldNameWithoutId = sourceFieldNameWithoutId.Substring(0, sourceFieldNameWithoutId.Length - 2);
                }
                //
                // Most normal FKs will have a source field name that matches this table.  For those, change 
                // the source field name back to the destination table name.  
                //
                // However, if the FK has a different field name for some non-direct purpose like a parent reference,
                // then use the name of the field from the FK's source table
                //

                string primaryNameToUsePascalCase;

                if (sourceFieldNameWithoutId.Trim().ToUpper() == fk.targetTable.name.Trim().ToUpper())
                {
                    // Pascal case name of the table is the outcome.
                    //
                    primaryNameToUsePascalCase = sourceTableName;
                }
                else
                {
                    //
                    // Pascal case name of the table is the outcome.
                    //
                    primaryNameToUsePascalCase = $"{sourceTableName}{CamelCaseToPascalCase(sourceFieldNameWithoutId)}";
                }

                string pluralCamelCasePrimaryName = Pluralize(CamelCase(primaryNameToUsePascalCase, false));



                sb.AppendLine(@$"    /**
     *
     * Gets the {Pluralize(primaryNameToUsePascalCase)} for this {entity}.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.{CamelCase(entity, false)}.{Pluralize(primaryNameToUsePascalCase)}.then({Pluralize(CamelCase(sourceFieldNameWithoutId, false))} => {{ ... }})
     *   or
     *   await this.{CamelCase(entity, false)}.{Pluralize(sourceFieldNameWithoutId)}
     *
    */
    public get {Pluralize(primaryNameToUsePascalCase)}(): Promise<{fk.sourceTable.name}Data[]> {{
        if (this._{Pluralize(CamelCase(primaryNameToUsePascalCase, false))} !== null) {{
            return Promise.resolve(this._{Pluralize(CamelCase(primaryNameToUsePascalCase, false))});
        }}

        if (this._{Pluralize(CamelCase(primaryNameToUsePascalCase, false))}Promise !== null) {{
            return this._{Pluralize(CamelCase(primaryNameToUsePascalCase, false))}Promise;
        }}

        // Start the load
        this.load{Pluralize(primaryNameToUsePascalCase)}();

        return this._{Pluralize(CamelCase(primaryNameToUsePascalCase, false))}Promise!;
    }}



    private load{Pluralize(CamelCaseToPascalCase(primaryNameToUsePascalCase))}(): void {{

        this._{Pluralize(CamelCase(primaryNameToUsePascalCase, false))}Promise = lastValueFrom(
            {entity}Service.Instance.Get{Pluralize(primaryNameToUsePascalCase)}For{entity}(this.id)
        )
        .then({Pluralize(primaryNameToUsePascalCase)} => {{
            this._{pluralCamelCasePrimaryName} = {Pluralize(primaryNameToUsePascalCase)} ?? [];
            this._{pluralCamelCasePrimaryName}Subject.next(this._{pluralCamelCasePrimaryName});
            return this._{pluralCamelCasePrimaryName};
         }})
        .catch(err => {{
            this._{pluralCamelCasePrimaryName} = [];
            this._{pluralCamelCasePrimaryName}Subject.next(this._{pluralCamelCasePrimaryName});
            throw err;
        }})
        .finally(() => {{
            this._{Pluralize(CamelCase(primaryNameToUsePascalCase, false))}Promise = null; // Allow retry if needed
        }});
    }}

    /**
     * Clears the cached {primaryNameToUsePascalCase}. Call after mutations to force refresh.
     */
    public Clear{Pluralize(CamelCaseToPascalCase(primaryNameToUsePascalCase))}Cache(): void {{
        this._{pluralCamelCasePrimaryName} = null;
        this._{pluralCamelCasePrimaryName}Promise = null;
        this._{pluralCamelCasePrimaryName}Subject.next(this._{pluralCamelCasePrimaryName});      // Emit to observable
    }}

    public get Has{Pluralize(CamelCaseToPascalCase(primaryNameToUsePascalCase))}(): Promise<boolean> {{
        return this.{Pluralize(primaryNameToUsePascalCase)}.then({pluralCamelCasePrimaryName} => {pluralCamelCasePrimaryName}.length > 0);
    }}

");
            }


            //
            // Add version history observable, getter, loader, and cache clear for version-controlled entities
            //
            if (table.IsVersionControlEnabled() == true)
            {
                sb.AppendLine($@"

    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{{{ ({camelCaseName}.CurrentVersionInfo$ | async)?.userName }}}}
    //   Code:     const info = await {camelCaseName}.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {{
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {{
                this.loadCurrentVersionInfo();
            }}
        }}),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<{entity}Data>> {{
        if (this._currentVersionInfoPromise === null) {{
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }}
        return this._currentVersionInfoPromise;
    }}


    private async loadCurrentVersionInfo(): Promise<VersionInformation<{entity}Data>> {{
        const info = await lastValueFrom(
            {entity}Service.Instance.Get{entity}ChangeMetadata(this.id, this.versionNumber as number)
        );
        this._currentVersionInfo = info;
        this._currentVersionInfoSubject.next(info);
        return info;
    }}


    public ClearCurrentVersionInfoCache(): void {{
        this._currentVersionInfo = null;
        this._currentVersionInfoPromise = null;
        this._currentVersionInfoSubject.next(null);
    }}
");
            }

            //
            // Add some final helper methods for updating and converting to submit data
            //
            sb.AppendLine($@"

    /**
     * Updates the state of this {entity}Data object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {{
        Object.assign(this, other);
    }}


    /**
     * Converts this {entity}Data object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): {entity}SubmitData {{
        return {entity}Service.Instance.ConvertTo{entity}SubmitData(this);
    }}");


            sb.AppendLine("}");
            sb.AppendLine();

            return sb.ToString();
        }


        private static string GenerateDataSetterOutputClass(Type type, Database.Table table)
        {
            string entity = table.name;  // string entity = type.Name;
            string plural = Pluralize(entity);


            StringBuilder sb = new StringBuilder();

            sb.AppendLine("//");
            sb.AppendLine("// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.");
            sb.AppendLine("//");
            sb.AppendLine("export class " + entity + "SubmitData {");

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

                if (prop.Name.Trim().ToUpper() == "OBJECTGUID")
                {
                    continue;
                }

                if (prop.Name.Trim().ToUpper() == "PASSWORD")
                {
                    continue;
                }

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                //
                // If this is a collection property, then jump over it.
                //
                if (propertyType.FullName.StartsWith("System.Collections"))
                {
                    continue;
                }

                Database.Table.Field field = table.GetFieldByName(prop.Name);


                // Objects get mapped as minimal anonymous objects, regular data types just get mapped directly.
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
                    // Do nothing. We are not writing object types to the setter angular object

                }
                else
                {
                    if (propertyType == typeof(string) || (propertyType == typeof(Guid)))
                    {
                        if (field != null && field.nullable == false)
                        {
                            sb.AppendLine("    " + prop.Name + "!: string;");
                        }
                        else
                        {
                            sb.AppendLine("    " + prop.Name + ": string | null = null;");
                        }
                    }
                    else if (propertyType == typeof(int) || (propertyType == typeof(long)))
                    {
                        if (field != null && field.nullable == false)
                        {
                            sb.AppendLine("    " + prop.Name + "!: bigint | number;");
                        }
                        else
                        {
                            sb.AppendLine("    " + prop.Name + ": bigint | number | null = null;");
                        }
                    }
                    else if (propertyType == typeof(bool))
                    {
                        if (field != null && field.nullable == false)
                        {
                            sb.AppendLine("    " + prop.Name + "!: boolean;");
                        }
                        else
                        {
                            sb.AppendLine("    " + prop.Name + ": boolean | null = null;");
                        }

                    }
                    else if (propertyType == typeof(DateTime) ||
                             propertyType == typeof(TimeOnly))
                    {
                        //
                        // DateTime/TimeOnly are to be serialized as ISO 8601 strings with milliseconds.  For example, 2025-12-09T01:09:27.093Z. 
                        //
                        // Javascript Date object is not used here on purpose.
                        //
                        if (field != null && field.nullable == false)
                        {
                            sb.AppendLine("    " + prop.Name + "!: string;      // ISO 8601 (full datetime)");
                        }
                        else
                        {
                            sb.AppendLine("    " + prop.Name + ": string | null = null;     // ISO 8601 (full datetime)");
                        }

                    }
                    else if (propertyType == typeof(DateOnly))
                    {
                        //
                        // DateOnly fields use YYYY-MM-DD format only.  No time component.
                        //
                        if (field != null && field.nullable == false)
                        {
                            sb.AppendLine("    " + prop.Name + "!: string;      // Date only (YYYY-MM-DD)");
                        }
                        else
                        {
                            sb.AppendLine("    " + prop.Name + ": string | null = null;     // Date only (YYYY-MM-DD)");
                        }

                    }
                    else if (propertyType == typeof(float) || (propertyType == typeof(double) || propertyType == typeof(decimal)))
                    {
                        if (field != null && field.nullable == false)
                        {
                            sb.AppendLine("    " + prop.Name + "!: number;");
                        }
                        else
                        {
                            sb.AppendLine("    " + prop.Name + ": number | null = null;");
                        }

                    }
                    else
                    {
                        if (field != null && field.nullable == false)
                        {
                            sb.AppendLine("    " + prop.Name + "!: string;");
                        }
                        else
                        {
                            sb.AppendLine("    " + prop.Name + ": string | null = null;");
                        }
                    }
                }
            }

            sb.AppendLine("}");
            sb.AppendLine();

            return sb.ToString();
        }


        private static string GenerateServiceAppComponentListing(string moduleName, Type contextType, DatabaseGenerator.Database database)
        {

            StringBuilder importSb = new StringBuilder();
            StringBuilder serviceParamSb = new StringBuilder();


            //
            // Add in the data service manager
            //
            importSb.AppendLine($"import {{ {moduleName}DataServiceManagerService }} from './{moduleName.ToLower()}-data-services/{moduleName.ToLower()}-data-service-manager.service';");
            serviceParamSb.AppendLine($"{moduleName}DataServiceManagerService,");


            //
            // Add in each data service
            //
            foreach (PropertyInfo prop in contextType.GetProperties())
            {
                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType.GenericTypeArguments != null && propertyType.GenericTypeArguments.Length > 0)
                {
                    string entityName = propertyType.GenericTypeArguments[0].Name;
                    string camelCaseName = CamelCase(entityName, false);
                    string pluralName = Pluralize(entityName);
                    string titleName = StringUtility.ConvertToHeader(camelCaseName);
                    string angularName = StringUtility.ConvertToAngularComponentName(entityName);

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

                                // recalculate the angular name to be a function of the adjusted name.  We want to use the real table name as the service name
                                angularName = StringUtility.ConvertToAngularComponentName(realName);
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

                                // recalculate the angular name to be a function of the adjusted name.  This is for situations like the 'ProjectRollerData' table that becomes an entity of name 'ProjectRollerDatum', and we want to use the real table name as the service name
                                angularName = StringUtility.ConvertToAngularComponentName(realName);
                                break;
                            }
                        }
                    }

                    if (scriptGenTable != null)
                    {
                        string entity = scriptGenTable.name;  //type.Name;
                        string plural = Pluralize(entity);

                        importSb.AppendLine("import { " + entity + $"Service }} from './{moduleName.ToLower()}-data-services/" + angularName + ".service';");

                        serviceParamSb.AppendLine(entity + "Service,");
                    }
                }
            }
            //
            // Add in the footer
            //


            StringBuilder outputSb = new StringBuilder();

            outputSb.AppendLine(("These are the import lines to add to the app.module.ts file for the auto generated services"));
            outputSb.AppendLine();
            outputSb.AppendLine(@"//");
            outputSb.AppendLine($@"// Beginning of imports for {moduleName} Data Services ");
            outputSb.AppendLine(@"//");
            outputSb.Append(importSb.ToString());
            outputSb.AppendLine(@"//");
            outputSb.AppendLine($@"// End of imports for {moduleName} Data Services ");
            outputSb.AppendLine(@"//");

            outputSb.AppendLine();
            outputSb.AppendLine();
            outputSb.AppendLine();
            outputSb.AppendLine("These are lines to add to the Providers array section of the app.module definition for the auto generated services.");
            outputSb.AppendLine();
            outputSb.AppendLine(@"//");
            outputSb.AppendLine($@"// Beginning of provider declarations for {moduleName} Data Services ");
            outputSb.AppendLine(@"//");
            outputSb.Append(serviceParamSb.ToString());
            outputSb.AppendLine(@"//");
            outputSb.AppendLine($@"// End of provider declarations for {moduleName} Data Services ");
            outputSb.AppendLine(@"//");

            return outputSb.ToString();
        }

        private static string GenerateCodeGenDisclaimer(string entityName, string angularName)
        {
            return $@"/*

   GENERATED SERVICE FOR THE {entityName.ToUpper()} TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the {entityName} table.

   It should suffice for many workflows and data access needs, but if anything more is needed, then extend this in a 
   custom version or add an additional targeted helper service.

*/";
        }
    }
}