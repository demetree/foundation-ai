using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using static Foundation.CodeGeneration.DatabaseGenerator;
using static Foundation.StringUtility;
using static CodeGenerationCommon.Utility;

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

            string entity = type.Name;
            string qualifiedEntity = databaseNamespace + "." + entity;
            string camelCaseEntity = Foundation.StringUtility.CamelCase(type.Name);



            if (scriptGenTable != null)
            {
                multiTenancyEnabled = scriptGenTable.IsMultiTenantEnabled();
                dataVisibilityEnabled = scriptGenTable.IsDataVisibilityEnabled();
            }

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations;");


            if (dataVisibilityEnabled == true)
            {
                sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");           // Do not remove this.  It is needed for the NotMapped attribute
            }

            sb.AppendLine("using System.Linq;");

            sb.AppendLine("");
            sb.AppendLine("namespace " + entityExtensionRootNamespace + "." + module + "." + databaseNamespace);
            sb.AppendLine("{");


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
            sb.AppendLine("\tpublic partial class " + entity);
            sb.AppendLine("\t{");


            List<PropertyInfo> dtoProperties = new List<PropertyInfo>();


            // Include only what is minimally necessary for a DTO
            foreach (PropertyInfo prop in type.GetProperties())
            {
                // None of these fields are editable by incoming posts or puts of the object, so don't include them on the DTO
                if (prop.Name.Trim().ToUpper() == "TENANTGUID" ||
                    prop.Name.Trim().ToUpper() == "OBJECTGUID")
                // prop.Name.Trim().ToUpper() == "VERSIONNUMBER" ||   Version number is needed for the PUT operation to be able to compare the version number being saved
                //prop.Name.Trim().ToUpper() == "DELETED")           Deleted is needed because its value is set to false to undelete a record.
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

                    // Is this property not a foundation type?  If so, don't add it to the DTO
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

                    // add this property to the dto list
                    dtoProperties.Add(prop);
                }
            }

            sb.AppendLine("\t\t// Minimal Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable properties.");
            sb.AppendLine("\t\tpublic class " + entity + "DTO");
            sb.AppendLine("\t\t{");
            foreach (PropertyInfo prop in dtoProperties)
            {
                // Non-nullable data fields get the [Required] attribute.
                // New object posts can be sent without these fields
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


            sb.AppendLine("\t\tpublic " + entity + "DTO ToDTO()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\treturn new " + entity + "DTO");
            sb.AppendLine("\t\t\t{");

            foreach (PropertyInfo prop in dtoProperties)
            {
                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                sb.Append("\t\t\t\t");
                sb.Append(prop.Name);
                sb.Append(" = this.");
                sb.Append(prop.Name);

                if (prop != dtoProperties.Last())
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



            sb.AppendLine("\t\tpublic static " + qualifiedEntity + " FromDTO(" + entity + "DTO dto)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\treturn new " + qualifiedEntity);
            sb.AppendLine("\t\t\t{");

            foreach (PropertyInfo prop in dtoProperties)
            {
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

                if (prop != dtoProperties.Last())
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


            sb.AppendLine("\t\tpublic void ApplyDTO(" + entity + "DTO dto)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tif (dto == null || this.id != dto.id)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t    throw new Exception(\"DTO is null or has an id mismatch.\");");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();

            foreach (PropertyInfo prop in dtoProperties)
            {
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
            sb.AppendLine("\t\tpublic " + entity + " Clone()");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\t//");
            sb.AppendLine("\t\t\t// Return a cloned object without any object or list properties.");
            sb.AppendLine("\t\t\t//");
            sb.AppendLine("\t\t\treturn new " + entity + "{");

            // Include all data except tenant guid and collection properties
            foreach (PropertyInfo prop in type.GetProperties())
            {
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




            sb.AppendLine();
            sb.AppendLine();
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
        }


        public static void BuildEntityExtensionImplementationFromEntityFrameworkContext(string moduleName, Type contextType, DatabaseGenerator.Database database, string filePath = "c:\\temp", string databaseObjectNamespace = "Database", string entityExtensionRootNamespace = "Foundation")
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
                        entityExtensionCode = BuildDefaultEntityExtensionImplementation(moduleName, type, scriptGenTable, "id", databaseObjectNamespace, false, entityExtensionRootNamespace);

                        string plural = prop.Name;

                        System.IO.File.WriteAllText(filePath + moduleName + "\\Models\\EntityExtensions\\" + entityName + "Extension.cs", entityExtensionCode);
                    }
                }
            }

            return;
        }
    }
}