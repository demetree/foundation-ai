using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static Foundation.CodeGeneration.DatabaseGenerator.Database.Table;
using static Foundation.Security.SecureMVCController;
using static Foundation.StringUtility;
using Field = Foundation.Security.SecureMVCController.Field;

namespace Foundation.CodeGeneration
{
    public class MVCCodeGenerator : CodeGenerationBase
    {
        //
        // Use this to create default implementation of a Secure MVC controller on an Entity Framework table object
        //
        public static string BuildDefaultMVCImplementation(string module, Type type, Type contextType, DatabaseGenerator.Database.Table scriptGenTable)
        {
            if (scriptGenTable == null)
            {
                throw new Exception("the scriptGenTable parameter is null.  It shouldn't be.  Have you updated your DB Context to match the database generator model?");
            }


            StringBuilder sb = new StringBuilder();

            string entityName = type.Name; ;

            int minimumReadPermissionLevel = 0;
            int minimumWritePermissionLevel = 0;

            int objectGuidMinimumReadPermissionLevel = 0;

            string displayName = null;

            if (scriptGenTable != null && string.IsNullOrEmpty(scriptGenTable.displayNameForTable) == false)
            {
                displayName = scriptGenTable.displayNameForTable;
            }
            else
            {
                displayName = entityName;
            }


            if (scriptGenTable != null)
            {
                minimumReadPermissionLevel = scriptGenTable.minimumReadPermissionLevel;
                minimumWritePermissionLevel = scriptGenTable.minimumWritePermissionLevel;
            }

            // Fix the names of entities that end with Statu or Campu and undo the mess that EF makes with their names.
            if (entityName.EndsWith("Statu") == true ||
                entityName.EndsWith("Campu") == true)
            {
                entityName += "s";
            }

            List<Field> fields = null;


            //
            // start with the fields that the Secure MVC controller suggests.  Some fields such as tenant guid, and list properties will be removed from the base type.
            //
            fields = GetFieldsFromType(type, entityName, scriptGenTable);


            //
            // Adjust the fields using data from the script generator table data.
            //
            if (scriptGenTable != null)
            {
                List<Field> fieldsGeneratedFromGeneratorTable = GetFieldsFromScriptGeneratorTable(scriptGenTable);

                //
                // Now reconcile the field list and apply the extra data that we get from the script gen table, particularly:
                //
                // - the max string length and display lengths
                // - the multi line flag on some strings
                //

                //foreach (Field field in fields)
                for (int fieldIndex = 0; fieldIndex < fields.Count; fieldIndex++)
                {
                    Field field = fields[fieldIndex];

                    Field securityFieldMatchedFromFromSG = null;
                    DatabaseGenerator.Database.Table.Field fieldFromScriptGenerator = null;

                    foreach (Field fieldToCheck in fieldsGeneratedFromGeneratorTable)
                    {
                        if (field.name == fieldToCheck.name)
                        {
                            securityFieldMatchedFromFromSG = fieldToCheck;
                            break;
                        }
                    }

                    foreach (var scriptGenFieldToCheck in scriptGenTable.fields)
                    {
                        if (field.name == scriptGenFieldToCheck.name)
                        {
                            fieldFromScriptGenerator = scriptGenFieldToCheck;
                            break;
                        }

                    }

                    if (securityFieldMatchedFromFromSG != null)
                    {
                        if (securityFieldMatchedFromFromSG is StringField && field is StringField)
                        {
                            ((StringField)field).maxLength = ((StringField)securityFieldMatchedFromFromSG).maxLength;
                            ((StringField)field).maxDisplayLength = ((StringField)securityFieldMatchedFromFromSG).maxDisplayLength;
                            ((StringField)field).multiLine = ((StringField)securityFieldMatchedFromFromSG).multiLine;
                        }


                        field.readPermissionLevelNeeded = securityFieldMatchedFromFromSG.readPermissionLevelNeeded;
                        field.readOnlyOnEdit = securityFieldMatchedFromFromSG.readOnlyOnEdit;

                        // There is no need to display the tenant guid on any auto generated UI.  Reporting will not be done cross tenant with these screens, and
                        // any extract that needs this data can be done many other ways,  In fact, the auto generated Web API controllers serving these screens will be not returning the data anyway since
                        // it takes up a lot of room and it doesn't add value to a any app workflow because the value is static.
                        /*
                        if (field.name == "tenantGuid" && scriptGenTable.IsMultiTenantEnabled() == true)
                        {
                            field.adminAccessNeeded = true;
                            field.hideFromReport = true;
                            field.readOnlyOnEdit = true;
                            field.reportOnly = true;
                        }
                        */


                        if (field.name == "versionNumber" && scriptGenTable.IsVersionControlEnabled() == true)
                        {
                            field.adminAccessNeeded = true;
                            field.readOnlyOnEdit = true;
                            field.reportOnly = true;
                        }

                        if (field.name == "objectGuid")
                        {
                            objectGuidMinimumReadPermissionLevel = field.readPermissionLevelNeeded;
                        }

                        //
                        // Fix a couple of known self referencing fields that will map to a non-existent entity by default.
                        //
                        if (scriptGenTable.name == "User" && field.name == "reportsToUserId")
                        {
                            field.linkEntity = "User";
                        }

                        if (scriptGenTable.name == "SecurityUser" && field.name == "reportsToSecurityUserId")
                        {
                            field.linkEntity = "SecurityUser";
                        }

                        //
                        // If we have some more clarity on the purpose of the varbinary field from the script generator interpretation, revise the type accordingly
                        //
                        if (securityFieldMatchedFromFromSG.dataType != field.dataType)
                        {
                            //
                            // Replace the field with the one created from the script generator table's data
                            //
                            fields.Insert(fieldIndex + 1, securityFieldMatchedFromFromSG);
                            fields.RemoveAt(fieldIndex);
                            field = securityFieldMatchedFromFromSG;
                        }


                        //
                        // If this field is not a foreign key field, null out any link properties.  This is a hack because the first field builder assumes that any field that ends with Id is a foreign key link, and that is often true, but not always.
                        //  
                        // Undo the previous work by taking away all properties that a foreign key field should have.
                        //
                        if (field.name != "id" &&
                            field.name.EndsWith("Id") == true &&
                            fieldFromScriptGenerator.dataType != DatabaseGenerator.DataType.FOREIGN_KEY &&
                            fieldFromScriptGenerator.dataType != DatabaseGenerator.DataType.FOREIGN_KEY_GUID &&
                            fieldFromScriptGenerator.dataType != DatabaseGenerator.DataType.FOREIGN_KEY_STRING)
                        {
                            if (field.linkEntity != null)
                            {
                                field.linkEntity = null;
                            }

                            if (field.linkProperty != null)
                            {
                                field.linkProperty = null;
                            }

                            if (field.listGetter != null)
                            {
                                field.listGetter = null;
                            }

                            if (field.hideFromReport == true)
                            {
                                field.hideFromReport = false;
                            }
                        }
                    }
                }
            }


            bool tableIsReadOnly = false;
            bool adminAccessNeededToWrite = false;

            if (scriptGenTable != null)
            {
                tableIsReadOnly = scriptGenTable.isWritable == true ? false : true;
                adminAccessNeededToWrite = scriptGenTable.adminAccessNeededToWrite;
            }


            sb.AppendLine("using System;");
            sb.AppendLine("using Foundation.Security;");
            sb.AppendLine("using Foundation.Security.Database;");

            sb.AppendLine("using Foundation.Controllers;");
            sb.AppendLine("using System.Collections.Generic;");

            //
            sb.AppendLine("");
            sb.AppendLine("namespace Foundation." + module + ".Controllers.MVC");
            sb.AppendLine("{");
            sb.AppendLine("\tpublic partial class " + entityName + "Controller : SecureMVCController");
            sb.AppendLine("\t{");


            string displayNameField = "id";     // start with id, or replace with the first name from the display field name list

            if (scriptGenTable != null && scriptGenTable.displayNameFieldList.Count > 0)
            {
                displayNameField = scriptGenTable.displayNameFieldList[0].name;
            }
            else
            {
                var properties = type.GetProperties();

                foreach (var property in properties)
                {
                    if (property.Name.ToLower() == "name" || property.Name.ToLower() == "description")
                    {
                        displayNameField = property.Name;
                        break;
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("\t\tpublic const int READ_PERMISSION_REQUIRED = " + minimumReadPermissionLevel + ";     // used for Define children entities, and other places ad hoc read privilege is needed.");
            sb.AppendLine("\t\tpublic const int WRITE_PERMISSION_REQUIRED = " + minimumWritePermissionLevel + ";     // used for ad-hoc checks of write privilege needed for this entity. ");
            sb.AppendLine();



            string title = UnCamelCase(displayName);
            sb.AppendLine("\t\tpublic " + entityName + "Controller() : base(\"" + module + "\", \"" + entityName + "\", \"" + title + "\", \"" + Pluralize(entityName) + "\", \"" + entityName + "\", \"" + displayNameField + "\", " + (tableIsReadOnly == true ? "true" : "false") + ", " + (adminAccessNeededToWrite == true ? "true" : "false") + ")");

            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tREAD_PERMISSION_LEVEL_REQUIRED = " + minimumReadPermissionLevel + ";");
            sb.AppendLine("\t\t\tWRITE_PERMISSION_LEVEL_REQUIRED = " + minimumWritePermissionLevel + ";");

            sb.AppendLine("\t\t\treturn;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("");


            if (scriptGenTable != null && scriptGenTable.mvcDefineFieldsToBeOverridden == true)
            {
                // comment out this function, but write the code anyway.
                sb.AppendLine("/* This function is expected to be overridden in a custom file");
            }

            sb.AppendLine("\t\tprotected override void DefineFields(SecurityUser securityUser)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tif (securityUser == null)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\treturn;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();

            sb.AppendLine("\t\t\tbool userCanRead = UserCanRead(securityUser);");
            sb.AppendLine();


            sb.AppendLine("\t\t\tif (userCanRead == false)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\treturn;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();
            sb.AppendLine("\t\t\tbool userCanAdminister = UserCanAdminister(securityUser);");
            sb.AppendLine();
            sb.AppendLine("\t\t\tbool userCanWrite = false;");
            sb.AppendLine();
            sb.AppendLine("\t\t\tif (this.adminAccessNeededToWrite == true && userCanAdminister == true)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\tuserCanWrite = true;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t\telse");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\tuserCanWrite = UserCanWrite(securityUser);");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();

            foreach (Field field in fields)
            {
                string extraTab = "";

                if (field.name == "id")
                {
                    if (field.adminAccessNeeded == true)
                    {
                        extraTab = "\t";
                        sb.AppendLine("\t\t\tif (userCanAdminister == true)");
                        sb.AppendLine("\t\t\t{");
                    }
                    else if (field.writeAccessNeeded == true)
                    {
                        extraTab = "\t";
                        sb.AppendLine("\t\t\tif (userCanWrite == true)");
                        sb.AppendLine("\t\t\t{");
                    }
                    else if (field.readPermissionLevelNeeded > 0)
                    {
                        extraTab = "\t";
                        sb.AppendLine("\t\t\tif (securityUser.readPermissionLevel >= " + field.readPermissionLevelNeeded.ToString() + " || userCanAdminister == true)");
                        sb.AppendLine("\t\t\t{");
                    }

                    sb.AppendLine(extraTab + "\t\t\tAddIdField(securityUser, " + field.readPermissionLevelNeeded.ToString() + ");");

                    if (field.readPermissionLevelNeeded > 0 || field.adminAccessNeeded == true || field.writeAccessNeeded == true)
                    {
                        sb.AppendLine("\t\t\t}");
                    }
                }
                else if (field.name == "active" || field.name == "deleted" || field.name == "objectGuid")
                {
                    // do nothing.  These will be added at the end in another loop.
                }
                else
                {
                    if (field.adminAccessNeeded == true)
                    {
                        extraTab = "\t";
                        sb.AppendLine("\t\t\tif (userCanAdminister == true)");
                        sb.AppendLine("\t\t\t{");
                    }
                    else if (field.writeAccessNeeded == true)
                    {
                        extraTab = "\t";
                        sb.AppendLine("\t\t\tif (userCanWrite == true)");
                        sb.AppendLine("\t\t\t{");
                    }
                    else if (field.readPermissionLevelNeeded > 0)
                    {
                        extraTab = "\t";
                        sb.AppendLine("\t\t\tif (securityUser.readPermissionLevel >= " + field.readPermissionLevelNeeded.ToString() + " || userCanAdminister == true)");
                        sb.AppendLine("\t\t\t{");
                    }

                    DatabaseGenerator.Database.Table.Field scriptGenField = null;

                    if (scriptGenTable != null)
                    {
                        foreach (DatabaseGenerator.Database.Table.Field fld in scriptGenTable.fields)
                        {
                            if (fld.name == field.name)
                            {
                                scriptGenField = fld;
                                break;
                            }
                        }
                    }

                    if (scriptGenField == null)
                    {
                        // If this is a field that is a child table name and field reference, then there will be no field on the script generator table.  That's OK.
                        //throw new Exception("Unable to find field named " + field.name + " in database script generator table of " + scriptGenTable.name);
                    }

                    bool required = false;

                    if (scriptGenField != null)
                    {
                        if (scriptGenField.nullable == false && scriptGenField.defaultValue == null && scriptGenField.name != "tenantGuid")
                        {
                            required = true;
                        }
                    }

                    sb.AppendLine(extraTab + "\t\t\tfields.Add(new " + field.dataType + "Field { caption = \"" + field.caption +
                                            "\", name = \"" + field.name + "\"" +
                                            (required == true ? ", required = true" : "") +
                                            (field.isFilter == true ? ", isFilter = true" : "") +
                                            (field.reportOnly == true ? ", reportOnly = true" : "") +
                                            (field.readOnlyOnEdit == true ? ", readOnlyOnEdit = true" : "") +
                                            (String.IsNullOrEmpty(field.linkProperty) == false ? ", linkProperty = \"" + field.linkProperty + "\"" : "") +
                                            (String.IsNullOrEmpty(field.linkEntity) == false ? ", linkEntity = \"" + field.linkEntity + "\"" : "") +
                                            (String.IsNullOrEmpty(field.listGetter) == false ? ", listGetter = \"" + field.listGetter + "\"" : "") +
                                            (field.hideFromReport == true ? ", hideFromReport = true" : "") +
                                            (field.adminAccessNeeded == true ? ", adminAccessNeeded = true" : "") +
                                            (field.writeAccessNeeded == true ? ", writeAccessNeeded = true" : "") +
                                            (scriptGenField != null && scriptGenField.dataType == DatabaseGenerator.DataType.TEXT ? ", multiLine = true, maxDisplayLength = 100" : "") +
                                            (scriptGenField != null && scriptGenField.MaxStringLength().HasValue == true ? ", maxLength = " + scriptGenField.MaxStringLength().Value.ToString() : "") +
                                            " });");

                    if (field.readPermissionLevelNeeded > 0 || field.adminAccessNeeded == true || field.writeAccessNeeded == true)
                    {
                        sb.AppendLine("\t\t\t}");
                    }
                }
            }

            //
            // Add object guid, active and deleted at the end of the list.
            //
            bool hasActive = false;
            bool hasDeleted = false;
            bool hasObjectGuid = false;
            foreach (Field field in fields)
            {
                if (field.name == "active")
                {
                    hasActive = true;
                }
                else if (field.name == "deleted")
                {
                    hasDeleted = true;
                    // do nothing.  Work was already done on active field
                }
                else if (field.name == "objectGuid")
                {
                    hasObjectGuid = true;
                }
            }

            if (hasObjectGuid == true && hasDeleted == true && hasObjectGuid == true)
            {
                sb.AppendLine("\t\t\tAddActiveAndDeletedAndObjectGuidFields(securityUser, " + objectGuidMinimumReadPermissionLevel.ToString() + ");");
            }
            else if (hasActive == true && hasDeleted == true)
            {
                sb.AppendLine("\t\t\tAddActiveAndDeletedFields();");
            }
            else if (hasActive == true || hasDeleted == true)
            {
                throw new Exception("Unexpected control field state.  Should have active and deleted as a pair at least.");
            }



            sb.AppendLine("\t\t}");

            if (scriptGenTable != null && scriptGenTable.mvcDefineFieldsToBeOverridden == true)
            {
                sb.AppendLine("*/");
            }

            sb.AppendLine("");

            if (scriptGenTable != null && scriptGenTable.mvcDefineChildenEntitiesToBeOverridden == true)
            {
                // comment out this function, but write the code anyway.
                sb.AppendLine("/* This function is expected to be overridden in a custom file");
            }

            sb.AppendLine("\t\tprotected override List<ChildEntityData> DefineChildrenEntities(SecurityUser securityUser, bool userCanRead, bool userCanWrite, bool userCanAdminister, int userReadPermissionLevel)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tContextualInitialize(securityUser);");
            sb.AppendLine();
            sb.AppendLine("\t\t\tList<ChildEntityData> childrenEntities = new List<ChildEntityData>();");
            //
            //
            // Look for links to this object type in the context type.  If we find one, create a child link to there.
            //
            string idToThisFieldName = CamelCase(entityName) + "Id";

            foreach (PropertyInfo prop in contextType.GetProperties())
            {
                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType.GenericTypeArguments != null && propertyType.GenericTypeArguments.Length > 0)
                {
                    //System.Diagnostics.Debug.WriteLine("Property is " + prop.Name);

                    string propertyTypeEntityName = propertyType.GenericTypeArguments[0].Name;

                    //
                    // Ignore self, but process others
                    //
                    if (type.Name != propertyTypeEntityName)
                    {
                        //System.Diagnostics.Debug.WriteLine(propertyType.GenericTypeArguments[0].Name);

                        foreach (var item in propertyType.GenericTypeArguments[0].GetProperties())
                        {

                            // System.Diagnostics.Debug.WriteLine("Property on object of type " + propertyTypeEntityName + " is " + item.Name);

                            if (item.Name == idToThisFieldName)
                            {
                                sb.AppendLine("\t\t\tif (userReadPermissionLevel >= " + propertyTypeEntityName + "Controller.READ_PERMISSION_REQUIRED || userCanAdminister == true)");
                                sb.AppendLine("\t\t\t{");
                                sb.AppendLine("\t\t\t\tchildrenEntities.Add(new " + propertyTypeEntityName + "Controller().GetChildEntityData(securityUser, \"" + idToThisFieldName + "\", \"id\", userCanRead, userCanWrite, userCanAdminister));");
                                sb.AppendLine("\t\t\t}");
                                sb.AppendLine();
                            }
                        }
                    }
                }
            }


            sb.AppendLine("\t\t\treturn childrenEntities;");
            sb.AppendLine("\t\t}");


            if (scriptGenTable != null && scriptGenTable.mvcDefineChildenEntitiesToBeOverridden == true)
            {
                sb.AppendLine("*/");
            }

            sb.AppendLine("\t}");
            sb.AppendLine("}");

            //if (System.Diagnostics.Debugger.IsAttached == true)
            //{
            //    System.Diagnostics.Debug.WriteLine(sb.ToString());
            //}

            return sb.ToString();
        }


        public static void BuildMVCImplementationFromEntityFrameworkContext(string moduleName, Type contextType, DatabaseGenerator.Database database, string filePath = "c:\\temp")
        {
            if (filePath.EndsWith("\\") == false)
            {
                filePath = filePath + "\\";
            }

            System.IO.Directory.CreateDirectory(filePath + moduleName);
            System.IO.Directory.CreateDirectory(filePath + moduleName + "\\Controllers");
            System.IO.Directory.CreateDirectory(filePath + moduleName + "\\Controllers\\MVC");

            foreach (PropertyInfo prop in contextType.GetProperties())
            {
                //System.Diagnostics.Debug.WriteLine("Property is " + prop.Name);

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType.GenericTypeArguments != null && propertyType.GenericTypeArguments.Length > 0)
                {
                    string entityName = propertyType.GenericTypeArguments[0].Name;

                    if (entityName.EndsWith("Statu") == true ||
                        entityName.EndsWith("Campu") == true)
                    {
                        entityName += "s";
                    }

                    // Get the table Type
                    Type type = propertyType.GenericTypeArguments[0];

                    // Get the table spec from the script generator
                    DatabaseGenerator.Database.Table scriptGenTable = null;
                    if (database != null)
                    {
                        foreach (DatabaseGenerator.Database.Table tbl in database.tables)
                        {
                            //if (tbl.name == type.Name)
                            if (tbl.name == entityName)
                            {
                                scriptGenTable = tbl;
                                break;
                            }
                        }
                    }

                    if (scriptGenTable != null && scriptGenTable.excludeFromCodeGeneration == false)
                    {
                        string MVCCode = BuildDefaultMVCImplementation(moduleName, type, contextType, scriptGenTable);

                        System.IO.File.WriteAllText(filePath + moduleName + "\\Controllers\\MVC\\" + entityName + "Controller.cs", MVCCode);
                    }
                }
            }

            return;
        }


        public static List<Field> GetFieldsFromScriptGeneratorTable(Foundation.CodeGeneration.DatabaseGenerator.Database.Table table)
        {
            List<Field> fields = new List<Field>();

            bool firstLinkFieldAdded = false;       // for tracking if there has been a non-ID link field back to self added yet.

            //
            // The purpose of this is to allow a very generic form setup to happen really fast (ideally briefly until a version with proper captions and such is developed )
            //
            foreach (Foundation.CodeGeneration.DatabaseGenerator.Database.Table.Field field in table.fields)
            {
                if (field.name.Trim().ToUpper() == "ID")
                {
                    fields.Add(new IntegerField()
                    {
                        adminAccessNeeded = false,
                        caption = "Id",
                        downloadRoute = null,
                        isFilter = true,
                        linkEntity = table.name,
                        linkProperty = "id",
                        defaultValue = null,
                        name = "id",
                        writeAccessNeeded = false,
                        readPermissionLevelNeeded = field.readPermissionLevelNeeded,
                        numberMin = 0,
                        numberMax = null,
                        numberStep = 1,
                        readOnlyOnEdit = true,
                        toolTipText = "Unique Identifier"
                    });
                }
                else if (field.name == "password")
                {
                    fields.Add(new PasswordField { caption = "Password", name = "password", isFilter = false, writeAccessNeeded = true, hideFromReport = true, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                }
                else if (field.name == "tenantGuid")
                {
                    fields.Add(new StringField { caption = "Tenant Guid", name = "tenantGuid", isFilter = false, adminAccessNeeded = true, required = false, readOnlyOnEdit = true, readPermissionLevelNeeded = field.readPermissionLevelNeeded });      // this gets managed by the server generally speaking
                }
                else if (field.name == "active")
                {
                    fields.Add(new BooleanField { caption = "Active", name = "active", isFilter = true, writeAccessNeeded = true, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                }
                else if (field.name == "deleted")
                {
                    fields.Add(new BooleanField { caption = "Deleted", name = "deleted", isFilter = true, adminAccessNeeded = true, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                }
                else if (field.name == "objectGuid")
                {
                    fields.Add(new StringField { caption = "Object Guid", name = "objectGuid", isFilter = true, adminAccessNeeded = true, readOnlyOnEdit = true, reportOnly = true, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                }
                else
                {
                    Foundation.CodeGeneration.DatabaseGenerator.DataType fieldType = field.dataType;

                    //Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    if (fieldType == CodeGeneration.DatabaseGenerator.DataType.STRING_10 ||
                        fieldType == CodeGeneration.DatabaseGenerator.DataType.STRING_100 ||
                        fieldType == CodeGeneration.DatabaseGenerator.DataType.STRING_1000 ||
                        fieldType == CodeGeneration.DatabaseGenerator.DataType.STRING_2000 ||
                        fieldType == CodeGeneration.DatabaseGenerator.DataType.STRING_250 ||
                        fieldType == CodeGeneration.DatabaseGenerator.DataType.STRING_50 ||
                        fieldType == CodeGeneration.DatabaseGenerator.DataType.STRING_500 ||
                        fieldType == CodeGeneration.DatabaseGenerator.DataType.STRING_850)
                    {
                        int maxLength = 0;

                        if (field.MaxStringLength().HasValue == true)
                        {
                            maxLength = field.MaxStringLength().Value;
                        }


                        if (field.name == "name" || firstLinkFieldAdded == false)        // Make the name, or the first field property a link into the details view.
                        {
                            fields.Add(new StringField { caption = UnCamelCase(field.name), name = field.name, linkEntity = table.name, isFilter = true, linkProperty = "id", maxLength = maxLength, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                            firstLinkFieldAdded = true;
                        }
                        else
                        {
                            fields.Add(new StringField { caption = UnCamelCase(field.name), isFilter = true, name = field.name, maxLength = maxLength, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                        }
                    }
                    else if (fieldType == CodeGeneration.DatabaseGenerator.DataType.TEXT)
                    {
                        //
                        // This becomes a multiliine string field
                        //
                        fields.Add(new StringField { caption = UnCamelCase(field.name), name = field.name, isFilter = true, multiLine = true, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                    }
                    else if (fieldType == CodeGeneration.DatabaseGenerator.DataType.INTEGER ||
                            fieldType == CodeGeneration.DatabaseGenerator.DataType.INTEGER_AUTO_NUMBER_KEY ||
                            fieldType == CodeGeneration.DatabaseGenerator.DataType.BIG_INTEGER_AUTO_NUMBER_KEY ||
                            fieldType == CodeGeneration.DatabaseGenerator.DataType.BIG_INTEGER_AUTO_NUMBER_KEY)
                    {
                        //
                        // Hide int Id fields hidden from the report screen
                        //
                        if (field.name.EndsWith("Id") == false)
                        {
                            fields.Add(new IntegerField { caption = UnCamelCase(field.name), name = field.name, isFilter = true, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                        }
                        else
                        {
                            //
                            // Make some assumptions about the link property.  The first part of the name is expected to point to an object in this model of that same name.
                            //
                            string linkedObjectName = field.name.Substring(0, field.name.Length - 2);
                            linkedObjectName = linkedObjectName[0].ToString().ToUpper() + linkedObjectName.Substring(1);
                            string caption = UnCamelCase(field.name.Substring(0, field.name.Length - 2));

                            fields.Add(new IntegerField { caption = caption, name = field.name, hideFromReport = true, linkEntity = linkedObjectName, linkProperty = field.name, listGetter = "/api/" + Pluralize(linkedObjectName) + "/ListData", isFilter = true, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                        }
                    }
                    else if (fieldType == CodeGeneration.DatabaseGenerator.DataType.FOREIGN_KEY ||
                             fieldType == CodeGeneration.DatabaseGenerator.DataType.FOREIGN_KEY_BIG_INTEGER)
                    {
                        //
                        // Make some assumptions about the link property.  The first part of the name is expected to point to an object in this model of that same name.
                        //
                        string linkedObjectName = field.name.Substring(0, field.name.Length - 2);
                        linkedObjectName = linkedObjectName[0].ToString().ToUpper() + linkedObjectName.Substring(1);
                        string caption = UnCamelCase(field.name.Substring(0, field.name.Length - 2));

                        fields.Add(new IntegerField { caption = caption, name = field.name, hideFromReport = true, linkEntity = linkedObjectName, linkProperty = field.name, listGetter = "/api/" + Pluralize(linkedObjectName) + "/ListData", isFilter = true, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                    }
                    else if (fieldType == CodeGeneration.DatabaseGenerator.DataType.FOREIGN_KEY_GUID ||
                             fieldType == CodeGeneration.DatabaseGenerator.DataType.FOREIGN_KEY_STRING)
                    {
                        //
                        // Make some assumptions about the link property.  The first part of the name is expected to point to an object in this model of that same name.
                        //
                        string linkedObjectName = field.name.Substring(0, field.name.Length - 2);
                        linkedObjectName = linkedObjectName[0].ToString().ToUpper() + linkedObjectName.Substring(1);
                        string caption = UnCamelCase(field.name.Substring(0, field.name.Length - 2));

                        fields.Add(new StringField { caption = caption, name = field.name, hideFromReport = true, linkEntity = linkedObjectName, linkProperty = field.name, listGetter = "/api/" + Pluralize(linkedObjectName) + "/GetObjectGuidListData", isFilter = true, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                    }
                    else if (fieldType == CodeGeneration.DatabaseGenerator.DataType.DATETIME)
                    {
                        fields.Add(new DateTimeField { caption = UnCamelCase(field.name), name = field.name, isFilter = true, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                    }
                    else if (fieldType == CodeGeneration.DatabaseGenerator.DataType.DATE)
                    {
                        fields.Add(new DateField { caption = UnCamelCase(field.name), name = field.name, isFilter = true, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                    }
                    else if (fieldType == CodeGeneration.DatabaseGenerator.DataType.DECIMAL_38_22)
                    {
                        fields.Add(new DecimalField { caption = UnCamelCase(field.name), name = field.name, isFilter = true, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                    }
                    else if (fieldType == CodeGeneration.DatabaseGenerator.DataType.LAT_LONG)
                    {
                        fields.Add(new LatLongField { caption = UnCamelCase(field.name), name = field.name, isFilter = true, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                    }
                    else if (fieldType == CodeGeneration.DatabaseGenerator.DataType.BOOL)
                    {
                        fields.Add(new BooleanField { caption = UnCamelCase(field.name), name = field.name, isFilter = true, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                    }
                    else if (fieldType == CodeGeneration.DatabaseGenerator.DataType.MONEY)
                    {
                        fields.Add(new DecimalField { caption = UnCamelCase(field.name), name = field.name, isFilter = true, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                    }
                    else if (fieldType == CodeGeneration.DatabaseGenerator.DataType.BINARY)
                    {
                        fields.Add(new BinaryField { caption = UnCamelCase(field.name), name = field.name, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                    }
                    else if (fieldType == CodeGeneration.DatabaseGenerator.DataType.MP4)
                    {
                        fields.Add(new MP4Field { caption = UnCamelCase(field.name), name = field.name, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                    }
                    else if (fieldType == CodeGeneration.DatabaseGenerator.DataType.PDF)
                    {
                        fields.Add(new PDFField { caption = UnCamelCase(field.name), name = field.name, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                    }
                    else if (fieldType == CodeGeneration.DatabaseGenerator.DataType.PNG)
                    {
                        fields.Add(new ImageField { caption = UnCamelCase(field.name), name = field.name, readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                    }
                    else if (fieldType == CodeGeneration.DatabaseGenerator.DataType.GUID)
                    {
                        //
                        // This becomes a string 50 field
                        //
                        fields.Add(new StringField { caption = UnCamelCase(field.name), name = field.name, isFilter = true, maxLength = 50 });
                    }
                    else
                    {
                        //
                        // this is probably an object field.  Add it as a report only, and assume that there is a name property on it.  Make some assumptions about the link properties as well, but at least this starts with something.
                        //
                        fields.Add(new StringField { caption = UnCamelCase(field.name) + " Name", name = field.name + ".name", reportOnly = true, linkEntity = field.name, linkProperty = CamelCase(field.name) + "Id", readPermissionLevelNeeded = field.readPermissionLevelNeeded });
                    }
                }
            }

            return fields;
        }


        public static List<Field> GetFieldsFromType(Type type, string entityName, DatabaseGenerator.Database.Table scriptGenTable = null)
        {
            List<Field> fields = new List<Field>();

            bool firstLinkFieldAdded = false;       // for tracking if there has been a non-ID link field back to self added yet.

            if (entityName == "Project")
            {
                int i = 0;
                i++;
            }


            List<ForeignKey> foreignKeysMappedToTextField = new List<ForeignKey>();
            //
            // The purpose of this is to allow a very generic form setup to happen really fast (ideally briefly until a version with proper captions and such is developed )
            //
            foreach (PropertyInfo prop in type.GetProperties())
            {
                if (prop.Name.Trim().ToUpper() == "ID")
                {
                    fields.Add(new IntegerField()
                    {
                        adminAccessNeeded = false,
                        caption = "Id",
                        downloadRoute = null,
                        isFilter = true,
                        linkEntity = entityName,
                        linkProperty = "id",
                        defaultValue = null,
                        name = "id",
                        writeAccessNeeded = false,
                        numberMin = 0,
                        numberMax = null,
                        numberStep = 1,
                        readOnlyOnEdit = true,
                        toolTipText = "Unique Identifier"
                    });
                }
                else if (prop.Name == "password")
                {
                    fields.Add(new PasswordField { caption = "Password", name = "password", isFilter = false, writeAccessNeeded = true, hideFromReport = true });
                }
                else if (prop.Name == "tenantGuid")
                {
                    // we don't want to include the tenant guid field because it adds no value.  It is just repetitive data, and the value can be found on the user object.
                    //fields.Add(new StringField { caption = "Tenant Guid", name = "tenantGuid", isFilter = false, readOnlyOnEdit = true, adminAccessNeeded = true, required = false });            // this gets managed by the server generally speaking
                }
                else if (prop.Name == "active")
                {
                    fields.Add(new BooleanField { caption = "Active", name = "active", isFilter = true, writeAccessNeeded = true });
                }
                else if (prop.Name == "deleted")
                {
                    fields.Add(new BooleanField { caption = "Deleted", name = "deleted", isFilter = true, adminAccessNeeded = true });
                }
                else if (prop.Name == "objectGuid")
                {
                    fields.Add(new StringField { caption = "Object Guid", name = "objectGuid", isFilter = true, adminAccessNeeded = true, readOnlyOnEdit = true, reportOnly = true });
                }
                else
                {
                    Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    if (propertyType.FullName.StartsWith("System.Collections") == true)
                    {
                        //
                        // This is a list property, most likely a navigation property list.  It does not belong in an auto generated field list.
                        //
                        continue;
                    }
                    else if (propertyType == typeof(string))
                    {
                        if (prop.Name == "name" || firstLinkFieldAdded == false)        // Make the name, or the first field property a link into the details view.
                        {
                            fields.Add(new StringField { caption = UnCamelCase(prop.Name), name = prop.Name, linkEntity = entityName, isFilter = true, linkProperty = "id" });
                            firstLinkFieldAdded = true;
                        }
                        else
                        {
                            fields.Add(new StringField { caption = UnCamelCase(prop.Name), isFilter = true, name = prop.Name });
                        }
                    }
                    else if (propertyType == typeof(int) || propertyType == typeof(long))
                    {
                        //
                        // Hide int Id fields from the report screen
                        //
                        if (prop.Name.EndsWith("Id") == false)
                        {
                            // Doesn't end with ID (so it's not expected to be a link).  Just add the field.
                            fields.Add(new IntegerField { caption = UnCamelCase(prop.Name), name = prop.Name, isFilter = true });
                        }
                        else
                        {
                            //
                            // First try to find this property in the list of foreign keys in the script generator table object
                            //
                            bool foundForeignKey = false;

                            if (scriptGenTable.foreignKeys != null && scriptGenTable.foreignKeys.Count > 0)
                            {
                                foreach (var foreignKey in scriptGenTable.foreignKeys)
                                {
                                    if (foreignKey.field.name == prop.Name)
                                    {
                                        //
                                        // we should have enough here to build a decent field setup
                                        //
                                        string FK_Name = foreignKey.name;       // expected to be in the 'FK_Project_Contact_projectManagerContactId' pattern

                                        string fieldName = foreignKey.field.name;
                                        string foreignTableName = FK_Name.Split('_')[2];          // Pull out the table name from the foreign key name

                                        string rebuiltFieldName = UnCamelCase(fieldName.Substring(0, fieldName.Length - 2));         // Take the Id suffix off of the field name before doing the UnCamelCase

                                        fields.Add(new IntegerField { caption = rebuiltFieldName, name = prop.Name, hideFromReport = true, linkEntity = TitleCase(foreignTableName), linkProperty = prop.Name, listGetter = "/api/" + Pluralize(foreignTableName) + "/ListData", isFilter = true });

                                        foundForeignKey = true;

                                        break;
                                    }
                                }
                            }
                            //
                            // If we have no foreign key found, then we let the guess work begin
                            // 
                            if (foundForeignKey == false)
                            {
                                //
                                // Make some assumptions about the link property.  The first part of the name is expected to point to an object in this model of that same name.
                                //
                                string linkEntity = prop.Name.Substring(0, prop.Name.Length - 2);
                                linkEntity = linkEntity[0].ToString().ToUpper() + linkEntity.Substring(1);      // make the string title case
                                string caption = UnCamelCase(prop.Name.Substring(0, prop.Name.Length - 2));

                                fields.Add(new IntegerField { caption = caption, name = prop.Name, hideFromReport = true, linkEntity = linkEntity, linkProperty = prop.Name, listGetter = "/api/" + Pluralize(linkEntity) + "/ListData", isFilter = true });
                            }
                        }
                    }
                    else if (propertyType == typeof(DateTime))
                    {
                        fields.Add(new DateTimeField { caption = UnCamelCase(prop.Name), name = prop.Name, isFilter = true });
                    }
                    else if (propertyType == typeof(float) || propertyType == typeof(double))
                    {
                        fields.Add(new FloatField { caption = UnCamelCase(prop.Name), name = prop.Name, isFilter = true });
                    }
                    else if (propertyType == typeof(bool))
                    {
                        fields.Add(new BooleanField { caption = UnCamelCase(prop.Name), name = prop.Name, isFilter = true });
                    }
                    else if (propertyType == typeof(decimal))
                    {
                        fields.Add(new DecimalField { caption = UnCamelCase(prop.Name), name = prop.Name, isFilter = true });
                    }
                    else if (propertyType == typeof(byte[]))
                    {
                        fields.Add(new ImageField { caption = UnCamelCase(prop.Name), name = prop.Name });
                    }
                    else
                    {
                        //
                        // This is likely an object type property that points to a foreign key object.  Use this to add a field that maps the link object's display name, and try to file it in a spot
                        // after it's actual int link property
                        //
                        if (scriptGenTable == null)
                        {
                            //
                            // this is probably an object field.  Add it as a report only, and assume that there is a name property on it.  Make some assumptions about the link properties as well, but at least this starts with something.
                            //
                            fields.Add(new StringField { caption = UnCamelCase(prop.Name) + " Name", name = prop.Name + ".name", reportOnly = true, linkEntity = prop.Name, linkProperty = CamelCase(prop.Name) + "Id" });
                        }
                        else
                        {
                            //
                            // We have a script gen table.  We should be able to find this in the foreign keys.
                            // 
                            bool foundLink = false;

                            //
                            // In some cases, the property names for foreign key virtual properties get a numeric suffix.  This occurs when there are more than one links to the same table.
                            //
                            string propertyNameWithNumericSuffixRemoved = prop.Name;
                            while (Char.IsNumber(propertyNameWithNumericSuffixRemoved.Last()) == true &&
                                   propertyNameWithNumericSuffixRemoved.Length > 0)
                            {
                                propertyNameWithNumericSuffixRemoved = propertyNameWithNumericSuffixRemoved.Substring(0, propertyNameWithNumericSuffixRemoved.Length - 1);
                            }

                            foreach (var foreignKey in scriptGenTable.foreignKeys)
                            {
                                // Jump over foreign keys that we have already mapped.
                                if (foreignKeysMappedToTextField.Contains(foreignKey) == true)
                                {
                                    continue;
                                }

                                if (foreignKey.table.name == propertyNameWithNumericSuffixRemoved)
                                {
                                    foundLink = true;
                                    foreignKeysMappedToTextField.Add(foreignKey);   // mark this one as used.

                                    string displayNameField = "name";

                                    if (foreignKey.table.displayNameFieldList.Count > 0)
                                    {
                                        displayNameField = foreignKey.table.displayNameFieldList[0].name;
                                    }

                                    bool fieldInsertedInEarlierPosition = false;
                                    for (int j = 0; j < fields.Count; j++)
                                    {
                                        // Take one char off the end of the table name to handle time when values like 'Status' become 'Statu'
                                        if (fields[j].linkProperty != null && foreignKey.field.name == fields[j].linkProperty)
                                        {
                                            string FK_Name = foreignKey.name;       // expected to be in the 'FK_Project_Contact_projectManagerContactId' pattern

                                            string fieldName = foreignKey.field.name;
                                            string tableReference = FK_Name.Split('_')[2];          // Pull out the table name from the foreign key name

                                            string rebuiltFieldName = UnCamelCase(fieldName.Substring(0, fieldName.Length - 2));         // Take the Id suffix off of the field name before doing the UnCamelCase


                                            fields.Insert(j + 1, new StringField { caption = rebuiltFieldName, name = foreignKey.table.name + "." + displayNameField, reportOnly = true, linkEntity = tableReference, linkProperty = fieldName });

                                            fieldInsertedInEarlierPosition = true;

                                            break;
                                        }
                                    }

                                    if (fieldInsertedInEarlierPosition == false)
                                    {
                                        //
                                        // This is likely a foreign key field to a table that has a key name that isn't the same as the target table.  This happens in places when we have multiple foreign keys to the same 
                                        // table.  For example, could have a 'primaryContactId', and a 'secondaryContactId' field on the same table that both point to the contact table.
                                        //
                                        // We can use the foreign key name to discern the intention in this situation, and use that to build out a mostly suitable field setup.
                                        //
                                        if (foreignKey.field.name.EndsWith("Id") == true)
                                        {
                                            //
                                            // we should have enough here to build a decent field setup

                                            string FK_Name = foreignKey.name;       // expected to be in the 'FK_Project_Contact_projectManagerContactId' pattern

                                            string fieldName = foreignKey.field.name;
                                            string tableReference = FK_Name.Split('_')[2];          // Pull out the table name from the foreign key name

                                            string rebuiltFieldName = UnCamelCase(fieldName.Substring(0, fieldName.Length - 2));         // Take the Id suffix off of the field name before doing the UnCamelCase


                                            fields.Add(new StringField { caption = rebuiltFieldName, name = foreignKey.table.name + "." + displayNameField, reportOnly = true, linkEntity = tableReference, linkProperty = fieldName });



                                        }
                                        else
                                        {
                                            // not the expected field name pattern.  Add it with a best guess setup
                                            fields.Add(new StringField { caption = UnCamelCase(foreignKey.table.name), name = foreignKey.table.name + "." + displayNameField, reportOnly = true, linkEntity = prop.Name, linkProperty = CamelCase(prop.Name) + "Id" });
                                        }
                                    }

                                    // stop looking for foreign keys for this field.
                                    break;
                                }
                            }

                            if (foundLink == false)
                            {
                                if (entityName == "User" && prop.Name == "ReportsToUser")
                                {
                                    // Cheater to fix the self referencing reportsToUser on the canned User table.  change the name to 'displayName' and the linkEntity
                                    //fields.Add(new StringField { caption = UnCamelCase(prop.Name) + " Name", name = prop.Name + ".displayName", reportOnly = true, linkEntity = "User", linkProperty = CamelCase(prop.Name) + "Id" });
                                    fields.Add(new StringField { caption = UnCamelCase(prop.Name), name = prop.Name + ".displayName", reportOnly = true, linkEntity = "User", linkProperty = CamelCase(prop.Name) + "Id" });
                                }
                                else if (entityName == "SecurityUser" && prop.Name == "ReportsToSecurityUser")
                                {
                                    // Cheater to fix the self referencing reportsToUser on the canned User table.  change the name to 'accountName' and the linkEntity
                                    //fields.Add(new StringField { caption = UnCamelCase(prop.Name) + " Name", name = prop.Name + ".accountName", reportOnly = true, linkEntity = "SecurityUser", linkProperty = CamelCase(prop.Name) + "Id" });
                                    fields.Add(new StringField { caption = UnCamelCase(prop.Name), name = prop.Name + ".accountName", reportOnly = true, linkEntity = "SecurityUser", linkProperty = CamelCase(prop.Name) + "Id" });
                                }
                                else
                                {
                                    bool fieldInserted = false;
                                    for (int j = 0; j < fields.Count; j++)
                                    {
                                        // Take one char off the end of the table name to handle time when values like 'Status' become 'Statu'
                                        //if (fields[j].name.ToUpper().StartsWith(foreignKey.table.name.Substring(0, foreignKey.table.name.Length - 1).ToUpper()) == true)

                                        if (fields[j].linkProperty != null &&
                                            (
                                            (prop.Name + "Id").ToUpper() == fields[j].linkProperty.ToUpper() ||
                                            (prop.Name + "sId").ToUpper() == fields[j].linkProperty.ToUpper()))               // to handle the silly 'Status' becomes 'Statu' situation.
                                        {
                                            //fields.Insert(j + 1, new StringField { caption = UnCamelCase(prop.Name) + " Name", name = prop.Name + ".name", reportOnly = true, linkEntity = prop.Name, linkProperty = CamelCase(prop.Name) + "Id" });
                                            fields.Insert(j + 1, new StringField { caption = UnCamelCase(prop.Name), name = prop.Name + ".name", reportOnly = true, linkEntity = prop.Name, linkProperty = CamelCase(prop.Name) + "Id" });
                                            fieldInserted = true;
                                            break;
                                        }
                                    }

                                    if (fieldInserted == false)
                                    {
                                        // same as if there was no script gen table
                                        //fields.Add(new StringField { caption = UnCamelCase(prop.Name) + " Name", name = prop.Name + ".name", reportOnly = true, linkEntity = prop.Name, linkProperty = CamelCase(prop.Name) + "Id" });
                                        fields.Add(new StringField { caption = UnCamelCase(prop.Name), name = prop.Name + ".name", reportOnly = true, linkEntity = prop.Name, linkProperty = CamelCase(prop.Name) + "Id" });
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return fields;
        }


    }
}
