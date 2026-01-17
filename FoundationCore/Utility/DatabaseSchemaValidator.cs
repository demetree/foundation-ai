using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Data.SqlClient;         // For SQL Server
using Microsoft.Data.Sqlite;            // For SQLite
using Npgsql;                           // For PostgreSQL
using MySql.Data.MySqlClient;           // For MySQL


namespace Foundation
{
    public class DatabaseSchemaValidator<T> where T : DbContext
    {
        private readonly T _context;
        private readonly Logger _logger;

        internal record ExpectedColumnInformation(string Name, string DataType, bool IsNullable, int? Length, bool IsDateTime, int? ExpectedPrecision);

        public DatabaseSchemaValidator(T context, Logger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<DatabaseSchemaValidatorResult> ValidateSchemaAsync(string schemaName)
        {
            List<string> mismatches = new List<string>();
            bool isValid = true;

            try
            {
                _logger.LogSystem($"About to validate schema named {schemaName}.");

                IModel model = _context.Model;
                DatabaseFacade database = _context.Database;

                string providerName = database.ProviderName ?? string.Empty;
                SchemaQueries schemaQueries = GetSchemaQueries(providerName);

                foreach (IEntityType entityType in model.GetEntityTypes())
                {
                    string tableName = entityType.GetTableName();
                    string entitySchema = entityType.GetSchema() ?? schemaName;


                    _logger.LogDebug($"About to check table named {entitySchema}.{tableName}.");

                    if (string.IsNullOrEmpty(tableName))
                    {
                        mismatches.Add($"Entity '{entityType.Name}' has no table name defined.");
                        isValid = false;

                        _logger.LogDebug($"Table named {entitySchema}.{tableName} has no table name defined.");

                        continue;
                    }

                    bool tableExists = await CheckTableExistsAsync(database, entitySchema, tableName, schemaQueries, providerName);
                    if (tableExists == false)
                    {
                        mismatches.Add($"Table '{entitySchema}.{tableName}' does not exist in the database.");
                        isValid = false;

                        _logger.LogDebug($"Table named {entitySchema}.{tableName} does not exist in the database.");

                        continue;
                    }

                    List<ExpectedColumnInformation> expectedColumns = entityType.GetProperties()
                        .Where(p => !p.IsShadowProperty())
                        .Select(p => new ExpectedColumnInformation(
                            p.GetColumnName(),
                            p.GetColumnType(),
                            p.IsColumnNullable(),
                            p.GetMaxLength(),
                            p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?),
                            p.GetPrecision()
                        ))
                        .ToList();

                    List<ColumnInfo> actualColumns = await GetTableColumnsAsync(database, entitySchema, tableName, schemaQueries, providerName);

                    foreach (ExpectedColumnInformation expected in expectedColumns)
                    {
                        ColumnInfo actual = actualColumns.FirstOrDefault(c => c.Name.Equals(expected.Name, StringComparison.OrdinalIgnoreCase));
                        if (actual == null)
                        {
                            string message = $"Column '{expected.Name}' in table '{entitySchema}.{tableName}' is missing.";

                            mismatches.Add(message);
                            _logger.LogDebug(message);

                            isValid = false;

                            continue;
                        }

                        if (AreDataTypesCompatible(expected.DataType, actual.DataType, providerName, expected.Length, actual.Length, expected.IsDateTime, expected.ExpectedPrecision, actual.Precision) == false)
                        {
                            string message = $"Column '{expected.Name}' in table '{entitySchema}.{tableName}' has mismatched data type, length, or precision. Expected: {expected.DataType}{(expected.Length.HasValue ? $" (Length: {expected.Length})" : "")}{(expected.ExpectedPrecision.HasValue ? $" (Precision: {expected.ExpectedPrecision})" : "")}, Actual: {actual.DataType}{(actual.Length.HasValue ? $" (Length: {actual.Length})" : "")}{(actual.Precision.HasValue ? $" (Precision: {actual.Precision})" : "")}";

                            mismatches.Add(message);
                            _logger.LogDebug(message);

                            isValid = false;
                        }

                        if (expected.IsNullable != actual.IsNullable)
                        {
                            string message = $"Column '{expected.Name}' in table '{entitySchema}.{tableName}' has mismatched nullability. Expected: {(expected.IsNullable ? "NULL" : "NOT NULL")}, Actual: {(actual.IsNullable ? "NULL" : "NOT NULL")}.";

                            mismatches.Add(message);
                            _logger.LogDebug(message);

                            isValid = false;
                        }
                    }

                    List<ColumnInfo> extraColumns = actualColumns.Where(ac => !expectedColumns.Any(ec => ec.Name.Equals(ac.Name, StringComparison.OrdinalIgnoreCase))).ToList();

                    foreach (ColumnInfo extra in extraColumns)
                    {
                        string message = $"Column '{extra.Name}' in table '{entitySchema}.{tableName}' exists in the database but is not in the EF model.";

                        mismatches.Add(message);
                        _logger.LogDebug(message);

                        isValid = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogException($"Caught error during schema validation for schema {schemaName}.", ex);

                mismatches.Add($"Schema validation failed due to an error: {ex.Message}");
                isValid = false;
            }


            _logger.LogSystem($"Completed validation of schema named {schemaName}.  IsValid is {isValid}.");

            return new DatabaseSchemaValidatorResult { IsValid = isValid, Mismatches = mismatches };
        }


        private async Task<bool> CheckTableExistsAsync(DatabaseFacade database, string schemaName, string tableName, SchemaQueries queries, string providerName)
        {
            await database.OpenConnectionAsync();

            try
            {
                using DbCommand command = database.GetDbConnection().CreateCommand();

                command.CommandText = queries.TableExistsQuery;

                if (providerName != "Microsoft.EntityFrameworkCore.Sqlite")
                {
                    DbParameter schemaParam = CreateParameter(providerName, "@schemaName", schemaName);
                    command.Parameters.Add(schemaParam);
                }

                DbParameter tableParam = CreateParameter(providerName, "@tableName", tableName);
                command.Parameters.Add(tableParam);

                object result = await command.ExecuteScalarAsync();

                return Convert.ToInt32(result) > 0;
            }
            finally
            {
                await database.CloseConnectionAsync();
            }
        }


        private async Task<List<ColumnInfo>> GetTableColumnsAsync(DatabaseFacade database, string schemaName, string tableName, SchemaQueries queries, string providerName)
        {
            List<ColumnInfo> columns = new List<ColumnInfo>();

            await database.OpenConnectionAsync();

            try
            {
                await using DbCommand command = database.GetDbConnection().CreateCommand();

                command.CommandText = queries.ColumnsQuery;

                if (providerName != "Microsoft.EntityFrameworkCore.Sqlite")
                {
                    DbParameter schemaParam = CreateParameter(providerName, "@schemaName", schemaName);
                    command.Parameters.Add(schemaParam);

                    DbParameter tableParam = CreateParameter(providerName, "@tableName", tableName);
                    command.Parameters.Add(tableParam);
                }
                else
                {
                    //
                    // For sqlite, swap out the @tableName directly in the  query.  It doesn't support parameters.
                    // 
                    command.CommandText = command.CommandText.Replace("@tableName", tableName);
                }

                using DbDataReader reader = await command.ExecuteReaderAsync();

                if (providerName == "Microsoft.EntityFrameworkCore.Sqlite")
                {
                    while (await reader.ReadAsync())
                    {
                        columns.Add(new ColumnInfo
                        {
                            Name = reader.GetString(1), // name
                            DataType = reader.GetString(2), // type
                            IsNullable = !reader.GetInt32(3).Equals(1), // notnull
                            Length = null,
                            Precision = null
                        });
                    }
                }
                else if (providerName == "Microsoft.EntityFrameworkCore.SqlServer")
                {
                    while (await reader.ReadAsync())
                    {
                        int? length = null;
                        if (!reader.IsDBNull(3)) // length
                        {
                            length = reader.GetInt32(3);
                            if (length == -1)
                                length = null;
                        }

                        int? precision = null;
                        if (!reader.IsDBNull(4)) // datetime_precision
                        {
                            precision = (int)reader.GetInt16(4);
                        }

                        columns.Add(new ColumnInfo
                        {
                            Name = reader.GetString(0), // column_name
                            DataType = reader.GetString(1), // data_type
                            IsNullable = reader.GetString(2).Equals("YES", StringComparison.OrdinalIgnoreCase),
                            Length = length,
                            Precision = precision
                        });
                    }
                }
                else
                {
                    while (await reader.ReadAsync())
                    {
                        int? length = reader.IsDBNull(3) ? null : reader.GetInt32(3);
                        int? precision = reader.IsDBNull(4) ? null : (int)reader.GetInt16(4);

                        columns.Add(new ColumnInfo
                        {
                            Name = reader.GetString(0),
                            DataType = reader.GetString(1),
                            IsNullable = reader.GetString(2).Equals("YES", StringComparison.OrdinalIgnoreCase),
                            Length = length,
                            Precision = precision
                        });
                    }
                }

                return columns;
            }
            finally
            {
                await database.CloseConnectionAsync();
            }
        }


        private DbParameter CreateParameter(string providerName, string parameterName, object value)
        {
            switch (providerName)
            {
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    return new SqlParameter(parameterName, value);

                case "Microsoft.EntityFrameworkCore.Sqlite":
                    return new SqliteParameter(parameterName, value);

                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    return new NpgsqlParameter(parameterName, value);

                case "Pomelo.EntityFrameworkCore.MySql":
                    return new MySqlParameter(parameterName, value);

                default:
                    throw new NotSupportedException($"Database provider {providerName} is not supported.");
            }
        }


        private SchemaQueries GetSchemaQueries(string providerName)
        {
            return providerName switch
            {
                "Microsoft.EntityFrameworkCore.SqlServer" => new SchemaQueries
                {
                    TableExistsQuery = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @schemaName AND table_name = @tableName",
                    ColumnsQuery = @"
                SELECT 
                    c.column_name, 
                    c.data_type, 
                    c.is_nullable, 
                    CASE 
                        WHEN c.data_type IN ('nvarchar', 'varchar', 'char', 'nchar') THEN c.character_maximum_length 
                        WHEN c.data_type = 'varbinary' THEN sc.max_length
                        ELSE NULL 
                    END AS length,
                    c.datetime_precision
                FROM information_schema.columns c
                LEFT JOIN sys.columns sc 
                    ON c.table_schema = SCHEMA_NAME(sc.object_id) 
                    AND c.table_name = OBJECT_NAME(sc.object_id) 
                    AND c.column_name = sc.name
                WHERE c.table_schema = @schemaName AND c.table_name = @tableName"
                },
                "Npgsql.EntityFrameworkCore.PostgreSQL" => new SchemaQueries
                {
                    TableExistsQuery = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @schemaName AND table_name = @tableName",
                    ColumnsQuery = "SELECT column_name, data_type, is_nullable, character_maximum_length, datetime_precision FROM information_schema.columns WHERE table_schema = @schemaName AND table_name = @tableName"
                },
                "Microsoft.EntityFrameworkCore.Sqlite" => new SchemaQueries
                {
                    TableExistsQuery = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name = @tableName",
                    ColumnsQuery = "PRAGMA table_info(@tableName)"
                },
                "Pomelo.EntityFrameworkCore.MySql" => new SchemaQueries
                {
                    TableExistsQuery = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @schemaName AND table_name = @tableName",
                    ColumnsQuery = "SELECT column_name, data_type, is_nullable, character_maximum_length, datetime_precision FROM information_schema.columns WHERE table_schema = @schemaName AND table_name = @tableName"
                },
                _ => throw new NotSupportedException($"Database provider {providerName} is not supported.")
            };
        }


        private bool AreTypesEquivalent(string expected, string actual, string providerName, int? expectedLength, int? actualLength, bool isDateTime, int? expectedPrecision, int? actualPrecision)
        {
            if (providerName == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                if (isDateTime)
                {
                    if (actual == "datetime2" && expected == "datetime2")
                    {
                        // Require exact precision match for DATETIME2(7)
                        return expectedPrecision == actualPrecision && expectedPrecision == 7;
                    }
                    if (actual == "datetime")
                    {
                        // DATETIME is not millisecond-precise (~3.33ms)
                        return false;
                    }
                    return false; // Other types (e.g., DATE, TIME) are not compatible
                }

                if (actual == "nvarchar" && expected.StartsWith(actual))
                {
                    if (expectedLength.HasValue && actualLength.HasValue)
                        return expectedLength == actualLength;
                    else if (expected.Contains("max") && actualLength == null)
                        return true;
                    else if (expectedLength == null && actualLength == null)
                        return true;
                    return false;
                }

                if (actual == "varbinary" && expected.StartsWith(actual))
                {
                    if (expectedLength.HasValue && actualLength.HasValue)
                        return expectedLength == actualLength;
                    else if (expected.Contains("max") && actualLength == null)
                        return true;
                    else if (expectedLength == null && actualLength == null)
                        return true;
                    return false;
                }

                //
                // Numeric in code gen is forced to full precision 38,22.  If type is numeric in test, we treat it as such.
                //
                if (actual == "numeric" && expected.StartsWith(actual))
                {
                    return true;
                }
            }
            else if (providerName == "Npgsql.EntityFrameworkCore.PostgreSQL" && isDateTime)
            {
                if (actual == "timestamp without time zone" && expected == "timestamp")
                {
                    return actualPrecision >= 3; // Millisecond precision
                }
                return false;
            }
            else if (providerName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                if (expected.StartsWith("datetime") && actual == "TEXT")
                {
                    return true;
                }
                else if (expected == "text" && actual.ToUpper().Contains("VARCHAR") == true)
                {
                    return true;
                }
                else if (expected == "text" && actual.ToUpper().Contains("DATE") == true)
                {
                    return true;
                }
                else if (expected == "integer" && (actual == "bit" ||
                                                  actual == "bigint"))
                {
                    return true;
                }
            }

            return false;
        }


        private bool AreDataTypesCompatible(string expected, string actual, string providerName, int? expectedLength, int? actualLength, bool isDateTime, int? expectedPrecision, int? actualPrecision)
        {
            expected = expected.ToLower();
            actual = actual.ToLower();
            return expected == actual || AreTypesEquivalent(expected, actual, providerName, expectedLength, actualLength, isDateTime, expectedPrecision, actualPrecision);
        }


        public class DatabaseSchemaValidatorResult
        {
            public bool IsValid { get; set; }
            public List<string> Mismatches { get; set; }
        }


        private class ColumnInfo
        {
            public string Name { get; set; }
            public string DataType { get; set; }
            public bool IsNullable { get; set; }
            public int? Length { get; set; } // For nvarchar/varbinary
            public int? Precision { get; set; } // For DATETIME2 precision (scale)
        }


        private class SchemaQueries
        {
            public string TableExistsQuery { get; set; }
            public string ColumnsQuery { get; set; }
        }
    }
}