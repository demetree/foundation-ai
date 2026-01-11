using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace Foundation.Tools
{
    public class DatabaseSchemaToCSharpGenerator
    {
        private readonly string _connectionString;
        private readonly string _outputNamespace;
        private readonly string _outputClassName;


        public DatabaseSchemaToCSharpGenerator(string connectionString, string outputNamespace, string outputClassName)
        {
            _connectionString = connectionString;
            _outputNamespace = outputNamespace;
            _outputClassName = outputClassName;
        }

        public void GenerateSchemaFile(string outputPath)
        {
            StringBuilder code = new StringBuilder();

            // Add namespace and class declaration
            code.AppendLine($"using Foundation.CodeGeneration;");
            code.AppendLine($"using System.Collections.Generic;");
            code.AppendLine($"using System;");
            code.AppendLine();
            code.AppendLine($"namespace {_outputNamespace}");
            code.AppendLine("{");
            code.AppendLine($"    public class {_outputClassName} : DatabaseGenerator");
            code.AppendLine("    {");
            code.AppendLine($"        public {_outputClassName}() : base(\"DatabaseName\", \"SchemaName\")"); // These will be replaced with actual values
            code.AppendLine("        {");

            // Connect to database and get schema information
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Get database name
                string databaseName = conn.Database;
                code.Replace("DatabaseName", databaseName);

                // Get schema name (assuming first schema found or default to 'dbo')
                string schemaName = GetSchemaName(conn);
                code.Replace("SchemaName", schemaName);
                code.AppendLine($"            database.SetSchemaName(\"{schemaName}\");");
                code.AppendLine();

                // Get tables and their definitions
                GenerateTableDefinitions(conn, code);

                code.AppendLine("        }");
                code.AppendLine("    }");
                code.AppendLine("}");
            }

            // Write to file
            File.WriteAllText(outputPath, code.ToString());
        }

        private string GetSchemaName(SqlConnection conn)
        {
            // This is a simplification - in reality, you might want to handle multiple schemas
            return "dbo"; // Default schema, could be enhanced to query INFORMATION_SCHEMA
        }

        private void GenerateTableDefinitions(SqlConnection conn, StringBuilder code)
        {
            //string query = @"
            //    SELECT t.TABLE_SCHEMA, t.TABLE_NAME, c.COLUMN_NAME, c.DATA_TYPE, 
            //           c.IS_NULLABLE, c.CHARACTER_MAXIMUM_LENGTH, 
            //           tc.CONSTRAINT_TYPE, ic.INDEX_NAME
            //    FROM INFORMATION_SCHEMA.TABLES t
            //    LEFT JOIN INFORMATION_SCHEMA.COLUMNS c 
            //        ON t.TABLE_SCHEMA = c.TABLE_SCHEMA AND t.TABLE_NAME = c.TABLE_NAME
            //    LEFT JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu 
            //        ON c.TABLE_NAME = ccu.TABLE_NAME AND c.COLUMN_NAME = ccu.COLUMN_NAME
            //    LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc 
            //        ON ccu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
            //    LEFT JOIN sys.indexes i 
            //        ON OBJECT_NAME(i.object_id) = t.TABLE_NAME
            //    LEFT JOIN sys.index_columns ic 
            //        ON i.object_id = ic.object_id AND i.index_id = ic.index_id 
            //        AND c.ORDINAL_POSITION = ic.key_ordinal
            //    WHERE t.TABLE_TYPE = 'BASE TABLE'
            //    ORDER BY t.TABLE_SCHEMA, t.TABLE_NAME, c.ORDINAL_POSITION";

            string query = @"
SELECT DISTINCT t.TABLE_SCHEMA, t.TABLE_NAME, c.COLUMN_NAME, c.DATA_TYPE, 
       c.IS_NULLABLE, c.CHARACTER_MAXIMUM_LENGTH, 
       tc.CONSTRAINT_TYPE, c.ORDINAL_POSITION --,   ic.INDEX_NAME
FROM INFORMATION_SCHEMA.TABLES t
LEFT JOIN INFORMATION_SCHEMA.COLUMNS c 
    ON t.TABLE_SCHEMA = c.TABLE_SCHEMA AND t.TABLE_NAME = c.TABLE_NAME
LEFT JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu 
    ON c.TABLE_NAME = ccu.TABLE_NAME AND c.COLUMN_NAME = ccu.COLUMN_NAME
LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc 
    ON ccu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
LEFT JOIN sys.indexes i 
    ON OBJECT_NAME(i.object_id) = t.TABLE_NAME
LEFT JOIN sys.index_columns ic 
    ON i.object_id = ic.object_id AND i.index_id = ic.index_id 
    AND c.ORDINAL_POSITION = ic.key_ordinal
WHERE t.TABLE_TYPE = 'BASE TABLE'
ORDER BY t.TABLE_SCHEMA, t.TABLE_NAME, c.ORDINAL_POSITION";


            Dictionary<string, List<ColumnInfo>> tables = new Dictionary<string, List<ColumnInfo>>();

            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string tableName = reader["TABLE_NAME"].ToString();
                    string fullTableName = $"{reader["TABLE_SCHEMA"]}.{tableName}";

                    if (!tables.ContainsKey(fullTableName))
                    {
                        tables[fullTableName] = new List<ColumnInfo>();
                    }

                    tables[fullTableName].Add(new ColumnInfo
                    {
                        Name = reader["COLUMN_NAME"].ToString(),
                        DataType = reader["DATA_TYPE"].ToString(),
                        IsNullable = reader["IS_NULLABLE"].ToString() == "YES",
                        MaxLength = reader["CHARACTER_MAXIMUM_LENGTH"] is DBNull ? null : (int?)Convert.ToInt32(reader["CHARACTER_MAXIMUM_LENGTH"]),
                        ConstraintType = reader["CONSTRAINT_TYPE"] is DBNull ? null : reader["CONSTRAINT_TYPE"].ToString()
                    });
                }
            }

            foreach (var table in tables)
            {
                code.AppendLine($"            Database.Table {CamelCase(table.Key.Split('.')[1])}Table = database.AddTable(\"{table.Key.Split('.')[1]}\");");
                code.AppendLine($"            {CamelCase(table.Key.Split('.')[1])}Table.SetTableToBeReadonlyForControllerCreationPurposes();");
                code.AppendLine($"            {CamelCase(table.Key.Split('.')[1])}Table.defaultPageSizeForListGetters = 1000;");

                foreach (var column in table.Value)
                {
                    GenerateColumnDefinition(code, CamelCase(table.Key.Split('.')[1]), column);
                }

                code.AppendLine();
            }
        }

        private void GenerateColumnDefinition(StringBuilder code, string tableVarName, ColumnInfo column)
        {
            string methodCall;
            bool isNullable = column.IsNullable;

            switch (column.DataType.ToLower())
            {
                case "int":
                    methodCall = $"AddIntField(\"{column.Name}\", {isNullable.ToString().ToLower()})";
                    break;
                case "nvarchar":
                    int length = column.MaxLength ?? 100; // Default length if not specified
                    methodCall = $"AddString{length}Field(\"{column.Name}\", {isNullable.ToString().ToLower()})";
                    break;
                case "float":
                case "real":
                    methodCall = $"AddFloatField(\"{column.Name}\", {isNullable.ToString().ToLower()})";
                    break;
                case "double":
                    methodCall = $"AddDoubleField(\"{column.Name}\", {isNullable.ToString().ToLower()})";
                    break;
                case "bit":
                    methodCall = $"AddBoolField(\"{column.Name}\", {isNullable.ToString().ToLower()})";
                    break;
                case "datetime2":
                    methodCall = $"AddDateTimeField(\"{column.Name}\", {isNullable.ToString().ToLower()})";
                    break;
                case "varbinary":
                    methodCall = $"AddBinaryField(\"{column.Name}\", {isNullable.ToString().ToLower()})";
                    break;
                case "uniqueidentifier":
                    methodCall = $"AddGuidField(\"{column.Name}\", {isNullable.ToString().ToLower()})";
                    break;
                default:
                    methodCall = $"AddString100Field(\"{column.Name}\", {isNullable.ToString().ToLower()})"; // Fallback
                    break;
            }

            code.AppendLine($"            {tableVarName}Table.{methodCall}{(column.Name == "id" ? "" : ".CreateIndex()")};");

            if (column.ConstraintType == "PRIMARY KEY")
            {
                code.Replace($"{tableVarName}Table.AddIntField(\"id\", false)", $"{tableVarName}Table.AddIdField()");
            }

            if (column.ConstraintType == "FOREIGN KEY")
            {
                // Note: This is simplified - real implementation would need to query referenced table
                code.Replace($"{tableVarName}Table.{methodCall}", $"{tableVarName}Table.AddForeignKeyField(/* referenced table */, {isNullable.ToString().ToLower()}, true)");
            }

            if (column.ConstraintType == "UNIQUE" && column.Name != "id")
            {
                code.AppendLine($"            {tableVarName}Table.AddUniqueConstraint(\"{column.Name}\");");
            }
        }

        private string CamelCase(string input)
        {
            return char.ToLowerInvariant(input[0]) + input.Substring(1);
        }

        private class ColumnInfo
        {
            public string Name { get; set; }
            public string DataType { get; set; }
            public bool IsNullable { get; set; }
            public int? MaxLength { get; set; }
            public string ConstraintType { get; set; }
        }
    }
}