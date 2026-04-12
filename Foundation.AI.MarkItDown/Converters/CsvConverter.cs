using System.Text;

namespace Foundation.AI.MarkItDown.Converters;

/// <summary>
/// Converts CSV files to Markdown tables.
///
/// <para>The first row is treated as the header row. Each subsequent row
/// becomes a table row. Values containing pipe characters are escaped.</para>
///
/// <para><b>AI-developed:</b> Port of Python markitdown's CsvConverter.</para>
/// </summary>
public sealed class CsvConverter : IDocumentConverter
{
    /// <inheritdoc />
    public string Name => "CSV";

    /// <inheritdoc />
    public int Priority => 0;


    /// <inheritdoc />
    public bool Accepts(Stream stream, StreamInfo streamInfo)
    {
        if (string.Equals(streamInfo.Extension, ".csv", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        if (string.Equals(streamInfo.MimeType, "text/csv", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        return false;
    }


    /// <inheritdoc />
    public async Task<ConversionResult> ConvertAsync(Stream stream,
        StreamInfo streamInfo,
        CancellationToken ct = default)
    {
        using StreamReader reader = new StreamReader(stream, leaveOpen: true);
        string content = await reader.ReadToEndAsync(ct);

        List<string[]> rowList = ParseCsv(content);

        if (rowList.Count == 0)
        {
            return new ConversionResult(Markdown: string.Empty);
        }

        StringBuilder markdownBuilder = new StringBuilder();

        //
        // Header row
        //
        string[] headerRow = rowList[0];
        markdownBuilder.AppendLine("| " + string.Join(" | ", headerRow.Select(EscapePipe)) + " |");

        //
        // Separator row
        //
        markdownBuilder.AppendLine("| " + string.Join(" | ", headerRow.Select(_ => "---")) + " |");

        //
        // Data rows
        //
        for (int rowIndex = 1; rowIndex < rowList.Count; rowIndex++)
        {
            string[] row = rowList[rowIndex];

            //
            // Pad or trim row to match header column count
            //
            string[] normalizedRow = new string[headerRow.Length];

            for (int columnIndex = 0; columnIndex < headerRow.Length; columnIndex++)
            {
                normalizedRow[columnIndex] = columnIndex < row.Length ? EscapePipe(row[columnIndex]) : "";
            }

            markdownBuilder.AppendLine("| " + string.Join(" | ", normalizedRow) + " |");
        }

        return new ConversionResult(Markdown: markdownBuilder.ToString());
    }


    /// <summary>
    /// Parse CSV content into rows of string arrays.
    /// Handles quoted fields with embedded commas, newlines, and escaped quotes.
    /// </summary>
    private static List<string[]> ParseCsv(string content)
    {
        List<string[]> rowList = new();
        List<string> currentFieldList = new();
        StringBuilder currentFieldBuilder = new StringBuilder();
        bool insideQuotes = false;
        int charIndex = 0;

        while (charIndex < content.Length)
        {
            char currentChar = content[charIndex];

            if (insideQuotes == true)
            {
                if (currentChar == '"')
                {
                    //
                    // Check for escaped quote (double quote)
                    //
                    if (charIndex + 1 < content.Length && content[charIndex + 1] == '"')
                    {
                        currentFieldBuilder.Append('"');
                        charIndex += 2;
                        continue;
                    }

                    //
                    // End of quoted field
                    //
                    insideQuotes = false;
                    charIndex++;
                    continue;
                }

                currentFieldBuilder.Append(currentChar);
                charIndex++;
            }
            else
            {
                if (currentChar == '"')
                {
                    insideQuotes = true;
                    charIndex++;
                }
                else if (currentChar == ',')
                {
                    currentFieldList.Add(currentFieldBuilder.ToString().Trim());
                    currentFieldBuilder.Clear();
                    charIndex++;
                }
                else if (currentChar == '\n' || (currentChar == '\r' && charIndex + 1 < content.Length && content[charIndex + 1] == '\n'))
                {
                    //
                    // End of row
                    //
                    currentFieldList.Add(currentFieldBuilder.ToString().Trim());
                    currentFieldBuilder.Clear();

                    if (currentFieldList.Any(f => f.Length > 0) == true)
                    {
                        rowList.Add(currentFieldList.ToArray());
                    }

                    currentFieldList.Clear();
                    charIndex += (currentChar == '\r') ? 2 : 1;
                }
                else
                {
                    currentFieldBuilder.Append(currentChar);
                    charIndex++;
                }
            }
        }

        //
        // Handle last field/row if file doesn't end with newline
        //
        currentFieldList.Add(currentFieldBuilder.ToString().Trim());

        if (currentFieldList.Any(f => f.Length > 0) == true)
        {
            rowList.Add(currentFieldList.ToArray());
        }

        return rowList;
    }


    /// <summary>
    /// Escape pipe characters in cell values so they don't break Markdown table syntax.
    /// </summary>
    private static string EscapePipe(string value)
    {
        return value.Replace("|", "\\|");
    }
}
