using System.Globalization;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Foundation.AI.MarkItDown.Office;

/// <summary>
/// Converts XLSX (Excel) files to Markdown tables using DocumentFormat.OpenXml.
///
/// <para><b>Output format:</b>
/// Each worksheet becomes a section with a heading. Row data is rendered as
/// a Markdown table with the first row treated as the header.</para>
///
/// <para><b>AI-developed:</b> C# port of Python markitdown's XlsxConverter
/// (pandas in Python, OpenXml SDK in C#).</para>
/// </summary>
public sealed class XlsxConverter : IDocumentConverter
{
    /// <inheritdoc />
    public string Name => "XLSX";

    /// <inheritdoc />
    public int Priority => 0;


    /// <inheritdoc />
    public bool Accepts(Stream stream, StreamInfo streamInfo)
    {
        if (string.Equals(streamInfo.Extension, ".xlsx", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        if (string.Equals(streamInfo.MimeType,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        return false;
    }


    /// <inheritdoc />
    public Task<ConversionResult> ConvertAsync(Stream stream,
        StreamInfo streamInfo,
        CancellationToken ct = default)
    {
        StringBuilder markdownBuilder = new StringBuilder();
        string? documentTitle = null;

        using SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(stream, isEditable: false);

        //
        // Extract document title from core properties
        //
        string? coreTitle = spreadsheetDocument.PackageProperties?.Title;

        if (string.IsNullOrWhiteSpace(coreTitle) == false)
        {
            documentTitle = coreTitle.Trim();
        }

        WorkbookPart? workbookPart = spreadsheetDocument.WorkbookPart;

        if (workbookPart == null)
        {
            return Task.FromResult(new ConversionResult(Markdown: string.Empty));
        }

        //
        // Get the shared strings table for resolving cell values
        //
        SharedStringTablePart? sharedStringsPart = workbookPart.SharedStringTablePart;
        SharedStringTable? sharedStringTable = sharedStringsPart?.SharedStringTable;

        //
        // Process each worksheet
        //
        Sheets? sheets = workbookPart.Workbook?.Sheets;

        if (sheets == null)
        {
            return Task.FromResult(new ConversionResult(Markdown: string.Empty));
        }

        foreach (Sheet sheet in sheets.Elements<Sheet>())
        {
            ct.ThrowIfCancellationRequested();

            string sheetName = sheet.Name?.Value ?? "Sheet";
            markdownBuilder.AppendLine($"## {sheetName}");
            markdownBuilder.AppendLine();

            //
            // Get the worksheet part via the relationship ID
            //
            string? relationshipId = sheet.Id?.Value;

            if (string.IsNullOrEmpty(relationshipId) == true)
            {
                continue;
            }

            WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(relationshipId);
            SheetData? sheetData = worksheetPart.Worksheet?.GetFirstChild<SheetData>();

            if (sheetData == null)
            {
                continue;
            }

            //
            // Collect all rows and determine column count
            //
            List<string[]> rowList = new();
            int maxColumnCount = 0;

            foreach (Row row in sheetData.Elements<Row>())
            {
                List<string> cellValueList = new();

                foreach (Cell cell in row.Elements<Cell>())
                {
                    string cellValue = GetCellValue(cell, sharedStringTable);
                    cellValueList.Add(cellValue.Replace("|", "\\|"));
                }

                if (cellValueList.Count > maxColumnCount)
                {
                    maxColumnCount = cellValueList.Count;
                }

                rowList.Add(cellValueList.ToArray());
            }

            if (rowList.Count == 0 || maxColumnCount == 0)
            {
                continue;
            }

            //
            // Render as Markdown table
            //
            string[] headerRow = PadRow(rowList[0], maxColumnCount);
            markdownBuilder.AppendLine("| " + string.Join(" | ", headerRow) + " |");
            markdownBuilder.AppendLine("| " + string.Join(" | ", Enumerable.Repeat("---", maxColumnCount)) + " |");

            for (int rowIndex = 1; rowIndex < rowList.Count; rowIndex++)
            {
                string[] paddedRow = PadRow(rowList[rowIndex], maxColumnCount);
                markdownBuilder.AppendLine("| " + string.Join(" | ", paddedRow) + " |");
            }

            markdownBuilder.AppendLine();
        }

        ConversionResult result = new ConversionResult(
            Markdown: markdownBuilder.ToString(),
            Title: documentTitle);

        return Task.FromResult(result);
    }


    /// <summary>
    /// Extract the display value from a cell, resolving shared strings and inline strings.
    /// </summary>
    private static string GetCellValue(Cell cell, SharedStringTable? sharedStringTable)
    {
        string? cellValue = cell.CellValue?.Text;

        if (string.IsNullOrEmpty(cellValue) == true)
        {
            return string.Empty;
        }

        //
        // If the cell data type is SharedString, look up the value
        //
        if (cell.DataType?.Value == CellValues.SharedString && sharedStringTable != null)
        {
            if (int.TryParse(cellValue, out int sharedStringIndex) == true)
            {
                SharedStringItem? sharedStringItem = sharedStringTable
                    .Elements<SharedStringItem>()
                    .ElementAtOrDefault(sharedStringIndex);

                return sharedStringItem?.InnerText ?? cellValue;
            }
        }

        //
        // If the cell data type is InlineString, use the inner text
        //
        if (cell.DataType?.Value == CellValues.InlineString)
        {
            return cell.InlineString?.InnerText ?? cellValue;
        }

        //
        // If it's a boolean, convert 0/1 to False/True
        //
        if (cell.DataType?.Value == CellValues.Boolean)
        {
            return cellValue == "1" ? "True" : "False";
        }

        return cellValue;
    }


    /// <summary>
    /// Pad a row to the expected column count with empty strings.
    /// </summary>
    private static string[] PadRow(string[] row, int columnCount)
    {
        if (row.Length >= columnCount)
        {
            return row;
        }

        string[] padded = new string[columnCount];
        Array.Copy(row, padded, row.Length);

        for (int i = row.Length; i < columnCount; i++)
        {
            padded[i] = "";
        }

        return padded;
    }
}
