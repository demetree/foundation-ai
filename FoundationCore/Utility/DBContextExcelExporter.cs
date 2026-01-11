using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation
{
    public static class DbContextExcelExporter
    {
        /// <summary>
        /// Exports data to Excel, writing to the provided stream.
        /// </summary>
        public static async Task ExportToExcelAsync<TContext>(Stream outputStream,
                                                              TContext dbContext,
                                                              Guid tenantGuid,
                                                              IEnumerable<string> entitySetNames = null,
                                                              bool convertTitlesToDocumentFormat = false,   // If set to true, will convert titles to document format.  If false, will use the names from the DbContext.
                                                              string tenantGuidColumnName = "tenantGuid",
                                                              CancellationToken cancellationToken = default)
                                                              where TContext : DbContext
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }

            using XLWorkbook workbook = new XLWorkbook();

            // Get all DbSet properties from the DbContext
            var dbSetProperties = typeof(TContext).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                  .Where(p => p.PropertyType.IsGenericType &&
                                                              p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                                                  .ToList();

            // Filter and order DbSet properties if entitySetNames is provided
            if (entitySetNames?.Any() == true)
            {
                var tableNamesSet = new HashSet<string>(entitySetNames, StringComparer.OrdinalIgnoreCase);
                var tableNamesList = entitySetNames.ToList(); // For ordering

                // Filter and order DbSet properties based on provided names
                dbSetProperties = dbSetProperties.Where(p => tableNamesSet.Contains(p.Name))
                                                 .OrderBy(p => tableNamesList.FindIndex(name => name.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                                                 .ToList();
            }

            // Check if any tables are selected for export
            if (!dbSetProperties.Any())
            {
                workbook.SaveAs(outputStream);
                return;
            }

            foreach (var dbSetProperty in dbSetProperties)
            {
                var entityType = dbSetProperty.PropertyType.GetGenericArguments()[0];
                var dbSet = dbSetProperty.GetValue(dbContext) as IQueryable;

                if (dbSet == null)
                {
                    continue;
                }

                // Create a worksheet
                string sheetName = dbSetProperty.Name;

                if (convertTitlesToDocumentFormat == true)
                {
                    // Convert to document format (e.g., replace spaces with underscores, etc.)
                    sheetName = sheetName.Replace(" ", "_").Replace("-", "_").Replace(".", "_");
                    sheetName = StringUtility.ConvertToHeader(sheetName);
                }


                //
                // Excel sheet names cannot exceed 31 characters - try to fix as best as possible
                //
                if (sheetName.Length > 31)
                {
                    //
                    // Start by yanking the vowels
                    //
                    sheetName = RemoveVowels(sheetName);


                    //
                    // Truncate at 31 if it's still too long
                    //
                    if (sheetName.Length > 31)
                    {
                        sheetName = sheetName.Substring(0, 31);
                    }
                }

                //
                // Can't add a sheet name that exists.  Make it something random
                //
                if (workbook.Worksheets.Contains(sheetName) == true)
                {
                    sheetName = MakeUniqueWorksheetName(sheetName, workbook.Worksheets);
                }

                var worksheet = workbook.Worksheets.Add(sheetName);

                // Get entity properties, excluding collections (except string), complex types, and tenantGuid
                var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p =>
                        // Include primitive types, enums, strings, and value types
                        (p.PropertyType.IsValueType || p.PropertyType == typeof(string) || p.PropertyType.IsEnum) &&
                        // Exclude collections (anything implementing IEnumerable except string)
                        (p.PropertyType == typeof(string) || !typeof(IEnumerable).IsAssignableFrom(p.PropertyType)) &&
                        // Exclude tenantGuid column
                        !p.Name.Equals(tenantGuidColumnName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Write headers
                for (int i = 0; i < properties.Count; i++)
                {
                    string columnName = properties[i].Name;


                    if (convertTitlesToDocumentFormat == true)
                    {
                        // Convert to document format (e.g., replace spaces with underscores, etc.)
                        columnName = columnName.Replace(" ", "_").Replace("-", "_").Replace(".", "_");
                        columnName = StringUtility.ConvertToHeader(columnName);
                    }

                    worksheet.Cell(1, i + 1).Value = columnName;
                }

                // Build query to filter by tenantGuid
                var query = dbSet.AsQueryable();

                var tenantProperty = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                               .FirstOrDefault(p => p.Name.Equals(tenantGuidColumnName, StringComparison.OrdinalIgnoreCase));

                if (tenantProperty != null)
                {
                    var parameter = Expression.Parameter(entityType, "e");
                    var tenantGuidExpression = Expression.Property(parameter, tenantProperty);
                    var constant = Expression.Constant(tenantGuid);
                    var equals = Expression.Equal(tenantGuidExpression, constant);
                    var lambda = Expression.Lambda(equals, parameter);

                    query = query.Provider.CreateQuery(
                        Expression.Call(
                            typeof(Queryable),
                            "Where",
                            new[] { entityType },
                            query.Expression,
                            lambda));
                }

                // Execute query
                var data = await EntityFrameworkQueryableExtensions.ToListAsync((dynamic)query, cancellationToken);

                // Write data
                int row = 2;
                foreach (var item in data)
                {
                    for (int i = 0; i < properties.Count; i++)
                    {
                        var value = properties[i].GetValue(item);
                        if (value == null)
                        {
                            worksheet.Cell(row, i + 1).Value = string.Empty; // Handle null explicitly
                        }
                        else if (value is DateTime dateTime)
                        {
                            worksheet.Cell(row, i + 1).Value = dateTime; // Explicitly set DateTime
                        }
                        else if (value is TimeSpan timeSpan)
                        {
                            worksheet.Cell(row, i + 1).Value = timeSpan; // Explicitly set TimeSpan
                        }
                        else if (value is string str)
                        {
                            if (str.Length <= 32767)
                            {
                                worksheet.Cell(row, i + 1).Value = str; // Handle string up to 32767 characters
                            }
                            else
                            {
                                worksheet.Cell(row, i + 1).Value = str.Substring(0, 32767); // Take only what fits
                            }
                        }
                        else
                        {
                            worksheet.Cell(row, i + 1).Value = value.ToString(); // Fallback to string
                        }
                    }
                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();
            }

            workbook.SaveAs(outputStream);
        }

        /// <summary>
        /// Exports data to Excel, returning a byte array.
        /// </summary>
        public static async Task<byte[]> ExportToExcelAsync<TContext>(TContext dbContext,
                                                                      Guid tenantGuid,
                                                                      IEnumerable<string> entitySetNames = null,
                                                                      bool convertTitlesToDocumentFormat = false,
                                                                      string tenantGuidColumnName = "tenantGuid",
                                                                      CancellationToken cancellationToken = default)
                                                                      where TContext : DbContext
        {
            using var stream = new MemoryStream();
            await ExportToExcelAsync(stream, dbContext, tenantGuid, entitySetNames, convertTitlesToDocumentFormat, tenantGuidColumnName, cancellationToken);
            return stream.ToArray();
        }

        private static string RemoveVowels(string str)
        {
            return str.Replace("A", "").Replace("a", "")
                      .Replace("E", "").Replace("e", "")
                      .Replace("I", "").Replace("i", "")
                      .Replace("O", "").Replace("o", "")
                      .Replace("U", "").Replace("U", "");
        }


        private static string MakeUniqueWorksheetName(string baseName,
                                                       IXLWorksheets worksheets)
        {
            const int MaxLen = 31;

            // Sanitize illegal Excel chars
            baseName = Regex.Replace(baseName, @"[:\\/?*\[\]]", "");

            // Remove vowels + truncate (your existing logic)
            baseName = RemoveVowels(baseName);
            if (baseName.Length > MaxLen)
                baseName = baseName.Substring(0, MaxLen);

            // If unique, we're done
            if (!worksheets.Contains(baseName))
                return baseName;

            // Add numeric suffix, trimming base as needed
            for (int i = 1; i < 1000; i++)
            {
                var suffix = $"_{i}";
                var trimmedBase = baseName;

                if (trimmedBase.Length + suffix.Length > MaxLen)
                {
                    trimmedBase = trimmedBase.Substring(
                        0, MaxLen - suffix.Length);
                }

                var candidate = trimmedBase + suffix;

                if (!worksheets.Contains(candidate))
                    return candidate;
            }

            throw new InvalidOperationException("Unable to generate unique worksheet name.");
        }

    }
}
