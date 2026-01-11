using System;
using System.IO;
using System.Reflection;
using System.Text;
using static Foundation.StringUtility;

namespace Foundation.CodeGeneration
{
    public class CodeGenerationBase
    {

        /// <summary>
        /// 
        /// Given an input folder, this function will read all the .cs files, and create a new file for each that takes the source file's code and 
        /// puts it into a string builder append.
        /// 
        /// The output of this can be then pasted into some other code templating function
        /// 
        /// </summary>
        /// <param name="sourceFileFolder"></param>
        public static void GenerateWrappedCode(string sourceFileFolder)
        {
            try
            {
                var files = Directory.GetFiles(sourceFileFolder);

                foreach (var fileName in files)
                {
                    if (fileName.ToUpper().EndsWith(".CS") == true)
                    {

                        using (StreamReader reader = new StreamReader(fileName))
                        {
                            using (StreamWriter outputFile = new StreamWriter(Path.Combine(sourceFileFolder, fileName + "_Processed")))
                            {
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    if (string.IsNullOrWhiteSpace(line) == false)
                                    {
                                        string outputLine = "sb.AppendLine(\"" + line.Replace("\"", "\\\"") + "\");";       // this might still need manual tweaking, depending on the nestedness of the string escapes.. 
                                        outputFile.WriteLine(outputLine);
                                    }
                                    else
                                    {
                                        outputFile.WriteLine("sb.AppendLine();");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }


        public static void BuildTemplatesFromEFContext(string moduleName, Type contextType, DatabaseGenerator.Database database, string filePath)
        {
            if (filePath.EndsWith("\\") == false)
            {
                filePath = filePath + "\\";
            }

            StringBuilder sb = new StringBuilder();

            System.IO.Directory.CreateDirectory(filePath + moduleName);

            sb.AppendLine("\t\tList<TemplateData> " + CamelCase(moduleName) + "Templates = Get" + moduleName + "Templates(securityUser, accessibleModules);");
            sb.AppendLine("\t\tif (" + CamelCase(moduleName) + "Templates != null)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\ttemplates.AddRange(" + CamelCase(moduleName) + "Templates);");
            sb.AppendLine("\t\t}");
            sb.AppendLine();


            sb.AppendLine("\t\tpublic List<TemplateData> Get" + moduleName + "Templates(SecurityUser securityUser, List<Security.SecurityFramework.ModuleAndRoleAndPrivilege> accessibleModules = null)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tList<TemplateData> templates = null;");
            sb.AppendLine();
            sb.AppendLine("\t\t\tif (Security.SecurityFramework.UserCanAccessModule(securityUser, \"" + moduleName + "\", accessibleModules) == true)");
            sb.AppendLine("\t\t\t{");

            sb.AppendLine("\t\t\t\ttemplates = new List<TemplateData>();");
            foreach (PropertyInfo prop in contextType.GetProperties())
            {
                //System.Diagnostics.Debug.WriteLine("Property is " + prop.Name);

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType.GenericTypeArguments != null && propertyType.GenericTypeArguments.Length > 0)
                {
                    string plural = prop.Name;
                    string entityName = propertyType.GenericTypeArguments[0].Name;

                    if (entityName.EndsWith("Statu") == true ||
                        entityName.EndsWith("Campu") == true)
                    {
                        entityName += "s";
                    }

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
                        if (string.IsNullOrEmpty(scriptGenTable.displayNameForTable) == false)
                        {
                            plural = Pluralize(scriptGenTable.displayNameForTable);
                        }

                        sb.AppendLine("\t\t\t\tif (new " + moduleName + ".Controllers.MVC." + entityName + "Controller().UserCanRead(securityUser) == true)");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t\ttemplates.Add(new TemplateData(\"" + entityName + "\", \"" + UnCamelCaseAndMakeTitle(plural) + "\", \"" + moduleName + "\"));");
                        sb.AppendLine("\t\t\t\t}");
                    }
                }
            }

            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t\treturn templates;");
            sb.AppendLine("\t\t}");

            System.IO.File.WriteAllText(filePath + moduleName + "\\" + moduleName + "Templates.cs", sb.ToString());

            return;
        }

        public static void BuildMenuFromEntityFrameworkContext(string moduleName, Type contextType, DatabaseGenerator.Database database, string filePath)
        {
            if (filePath.EndsWith("\\") == false)
            {
                filePath = filePath + "\\";
            }

            StringBuilder sb = new StringBuilder();

            System.IO.Directory.CreateDirectory(filePath + moduleName);


            sb.AppendLine("\t\t\tMenuGroup " + CamelCase(moduleName) + "Menu = Get" + moduleName + "Menu(securityUser, accessibleModules);");
            sb.AppendLine("\t\t\tif (" + CamelCase(moduleName) + "Menu != null)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\t" + CamelCase(moduleName) + "Menu.includeInSideBar = true;");
            sb.AppendLine("\t\t\t\tmenu.Add(" + CamelCase(moduleName) + "Menu);");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine();
            sb.AppendLine("\t\tpublic MenuGroup Get" + moduleName + "Menu(SecurityUser securityUser, List<Security.SecurityFramework.ModuleAndRoleAndPrivilege> accessibleModules)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tMenuGroup menu = null;");
            sb.AppendLine();
            sb.AppendLine("\t\t\tif (Security.SecurityFramework.UserCanAccessModule(securityUser, \"" + moduleName + "\", accessibleModules) == true)");
            sb.AppendLine("\t\t\t{");

            sb.AppendLine("\t\t\t\tmenu = new MenuGroup(\"" + UnCamelCaseAndMakeTitle(moduleName) + "\", \"" + UnCamelCaseAndMakeTitle(moduleName) + "\");");
            foreach (PropertyInfo prop in contextType.GetProperties())
            {
                //System.Diagnostics.Debug.WriteLine("Property is " + prop.Name);

                Type propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propertyType.GenericTypeArguments != null && propertyType.GenericTypeArguments.Length > 0)
                {
                    string plural = UnCamelCaseAndMakeTitle(prop.Name);
                    string entityName = propertyType.GenericTypeArguments[0].Name;

                    if (entityName.EndsWith("Statu") == true ||
                        entityName.EndsWith("Campu") == true)
                    {
                        entityName += "s";
                    }


                    // Get the table spec from the script generator
                    DatabaseGenerator.Database.Table scriptGenTable = null;
                    if (database != null)
                    {
                        foreach (DatabaseGenerator.Database.Table tbl in database.tables)
                        {
                            if (tbl.name == entityName)
                            {
                                scriptGenTable = tbl;
                                break;
                            }
                        }
                    }

                    if (scriptGenTable != null && scriptGenTable.excludeFromCodeGeneration == false)
                    {
                        if (string.IsNullOrEmpty(scriptGenTable.displayNameForTable) == false)
                        {
                            plural = Pluralize(scriptGenTable.displayNameForTable);
                        }

                        sb.AppendLine("\t\t\t\tif (new " + moduleName + ".Controllers.MVC." + entityName + "Controller().UserCanRead(securityUser) == true)");
                        sb.AppendLine("\t\t\t\t{");
                        sb.AppendLine("\t\t\t\t\tmenu.menuItems.Add(new MenuItem(\"" + plural + "\", \"" + plural + "\", \"/" + entityName + "List\", false));");
                        sb.AppendLine("\t\t\t\t}");
                    }
                }
            }

            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t\treturn menu;");
            sb.AppendLine("\t\t}");

            System.IO.File.WriteAllText(filePath + moduleName + "\\" + moduleName + "Menu.cs", sb.ToString());

            return;
        }


        public static string GetAcronym(string entity)
        {
            string output = entity[0].ToString();

            for (int i = 1; i < entity.Length; i++)
            {
                if (Char.IsUpper(entity[i]) == true)
                {
                    output += entity[i].ToString();
                }
            }


            //
            // In case acronum is a reserved word, then add an underscore to the font
            // 
            if (output.ToLower() == "as" || output.ToLower() == "if" || output.ToLower() == "for" || output.ToLower() == "is" || output.ToLower() == "id")
            {
                output = "_" + output;
            }

            return output.ToLower();
        }
    }
}
