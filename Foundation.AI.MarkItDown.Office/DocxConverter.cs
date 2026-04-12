using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Foundation.AI.MarkItDown.Office;

/// <summary>
/// Converts DOCX (Word) files to Markdown using DocumentFormat.OpenXml.
///
/// <para><b>Supported elements:</b>
/// Headings (from paragraph styles), bold, italic, underline, strikethrough,
/// hyperlinks, numbered and bulleted lists, tables, and line breaks.</para>
///
/// <para><b>AI-developed:</b> C# port of Python markitdown's DocxConverter
/// (mammoth+HTML in Python, direct OpenXml-to-Markdown in C#).</para>
/// </summary>
public sealed class DocxConverter : IDocumentConverter
{
    /// <inheritdoc />
    public string Name => "DOCX";

    /// <inheritdoc />
    public int Priority => 0;


    /// <inheritdoc />
    public bool Accepts(Stream stream, StreamInfo streamInfo)
    {
        if (string.Equals(streamInfo.Extension, ".docx", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        if (string.Equals(streamInfo.MimeType,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
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

        using WordprocessingDocument wordDocument = WordprocessingDocument.Open(stream, isEditable: false);
        Body? body = wordDocument.MainDocumentPart?.Document?.Body;

        if (body == null)
        {
            return Task.FromResult(new ConversionResult(Markdown: string.Empty));
        }

        //
        // Extract document title from core properties if available
        //
        string? coreTitle = wordDocument.PackageProperties?.Title;

        if (string.IsNullOrWhiteSpace(coreTitle) == false)
        {
            documentTitle = coreTitle.Trim();
        }

        //
        // Process each body element (paragraphs and tables)
        //
        foreach (OpenXmlElement element in body.ChildElements)
        {
            ct.ThrowIfCancellationRequested();

            if (element is Paragraph paragraph)
            {
                string paragraphMarkdown = ConvertParagraph(paragraph, wordDocument);

                if (string.IsNullOrEmpty(paragraphMarkdown) == false)
                {
                    markdownBuilder.AppendLine(paragraphMarkdown);
                    markdownBuilder.AppendLine();
                }
            }
            else if (element is Table table)
            {
                string tableMarkdown = ConvertTable(table, wordDocument);

                if (string.IsNullOrEmpty(tableMarkdown) == false)
                {
                    markdownBuilder.AppendLine(tableMarkdown);
                    markdownBuilder.AppendLine();
                }
            }
        }

        //
        // Use the first heading as the title if no core property title was found
        //
        if (documentTitle == null)
        {
            Paragraph? firstHeading = body.Descendants<Paragraph>()
                .FirstOrDefault(p => GetHeadingLevel(p) > 0);

            if (firstHeading != null)
            {
                documentTitle = firstHeading.InnerText?.Trim();
            }
        }

        ConversionResult result = new ConversionResult(
            Markdown: markdownBuilder.ToString(),
            Title: documentTitle);

        return Task.FromResult(result);
    }


    /// <summary>
    /// Convert a single paragraph to Markdown, handling headings, lists, and inline formatting.
    /// </summary>
    private static string ConvertParagraph(Paragraph paragraph, WordprocessingDocument wordDocument)
    {
        //
        // Check for heading style
        //
        int headingLevel = GetHeadingLevel(paragraph);

        //
        // Check for list (numbered or bulleted)
        //
        NumberingProperties? numberingProperties = paragraph.ParagraphProperties
            ?.NumberingProperties;
        bool isList = numberingProperties != null;

        //
        // Build inline content with formatting
        //
        StringBuilder inlineBuilder = new StringBuilder();

        foreach (OpenXmlElement child in paragraph.ChildElements)
        {
            if (child is Run run)
            {
                string runText = ConvertRun(run);
                inlineBuilder.Append(runText);
            }
            else if (child is Hyperlink hyperlink)
            {
                string hyperlinkMarkdown = ConvertHyperlink(hyperlink, wordDocument);
                inlineBuilder.Append(hyperlinkMarkdown);
            }
        }

        string content = inlineBuilder.ToString().Trim();

        if (string.IsNullOrEmpty(content) == true)
        {
            return string.Empty;
        }

        //
        // Apply heading prefix
        //
        if (headingLevel > 0)
        {
            string headingPrefix = new string('#', headingLevel);
            return $"{headingPrefix} {content}";
        }

        //
        // Apply list prefix
        //
        if (isList == true)
        {
            int indentLevel = GetListIndentLevel(numberingProperties!);
            string indent = new string(' ', indentLevel * 2);

            //
            // Determine if numbered or bulleted based on the numbering definition
            //
            bool isNumbered = IsNumberedList(numberingProperties!, wordDocument);
            string bulletPrefix = isNumbered == true ? "1." : "-";

            return $"{indent}{bulletPrefix} {content}";
        }

        return content;
    }


    /// <summary>
    /// Convert a Run element to Markdown with inline formatting (bold, italic, etc.).
    /// </summary>
    private static string ConvertRun(Run run)
    {
        string text = run.InnerText;

        if (string.IsNullOrEmpty(text) == true)
        {
            return string.Empty;
        }

        RunProperties? runProperties = run.RunProperties;

        if (runProperties == null)
        {
            return text;
        }

        //
        // Apply formatting wrappers
        //
        bool isBold = runProperties.Bold != null &&
            (runProperties.Bold.Val == null || runProperties.Bold.Val.Value == true);
        bool isItalic = runProperties.Italic != null &&
            (runProperties.Italic.Val == null || runProperties.Italic.Val.Value == true);
        bool isStrikethrough = runProperties.Strike != null &&
            (runProperties.Strike.Val == null || runProperties.Strike.Val.Value == true);
        bool isCode = string.Equals(runProperties.RunStyle?.Val?.Value, "CodeChar",
            StringComparison.OrdinalIgnoreCase) == true;

        if (isCode == true)
        {
            return $"`{text}`";
        }

        if (isBold == true && isItalic == true)
        {
            return $"***{text}***";
        }

        if (isBold == true)
        {
            return $"**{text}**";
        }

        if (isItalic == true)
        {
            return $"*{text}*";
        }

        if (isStrikethrough == true)
        {
            return $"~~{text}~~";
        }

        return text;
    }


    /// <summary>
    /// Convert a Hyperlink element to a Markdown link.
    /// </summary>
    private static string ConvertHyperlink(Hyperlink hyperlink, WordprocessingDocument wordDocument)
    {
        string linkText = hyperlink.InnerText?.Trim() ?? "";

        //
        // Resolve the URL from the relationship ID
        //
        string? url = null;

        if (hyperlink.Id != null && wordDocument.MainDocumentPart != null)
        {
            HyperlinkRelationship? relationship = wordDocument.MainDocumentPart
                .HyperlinkRelationships
                .FirstOrDefault(r => r.Id == hyperlink.Id.Value);

            url = relationship?.Uri?.ToString();
        }

        if (string.IsNullOrEmpty(url) == true)
        {
            return linkText;
        }

        return $"[{linkText}]({url})";
    }


    /// <summary>
    /// Convert a table element to a Markdown table.
    /// </summary>
    private static string ConvertTable(Table table, WordprocessingDocument wordDocument)
    {
        List<string[]> rowList = new();

        foreach (TableRow tableRow in table.Elements<TableRow>())
        {
            List<string> cellList = new();

            foreach (TableCell tableCell in tableRow.Elements<TableCell>())
            {
                //
                // Concatenate all paragraph text in the cell
                //
                string cellText = string.Join(" ",
                    tableCell.Elements<Paragraph>()
                        .Select(p => p.InnerText?.Trim() ?? ""));

                cellList.Add(cellText.Replace("|", "\\|"));
            }

            rowList.Add(cellList.ToArray());
        }

        if (rowList.Count == 0)
        {
            return string.Empty;
        }

        //
        // Determine the maximum column count across all rows
        //
        int columnCount = rowList.Max(r => r.Length);

        StringBuilder tableBuilder = new StringBuilder();

        //
        // Header row (first row of the table)
        //
        string[] headerRow = PadRow(rowList[0], columnCount);
        tableBuilder.AppendLine("| " + string.Join(" | ", headerRow) + " |");

        //
        // Separator row
        //
        tableBuilder.AppendLine("| " + string.Join(" | ", Enumerable.Repeat("---", columnCount)) + " |");

        //
        // Data rows
        //
        for (int rowIndex = 1; rowIndex < rowList.Count; rowIndex++)
        {
            string[] paddedRow = PadRow(rowList[rowIndex], columnCount);
            tableBuilder.AppendLine("| " + string.Join(" | ", paddedRow) + " |");
        }

        return tableBuilder.ToString();
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


    /// <summary>
    /// Determine the heading level from a paragraph's style (Heading1 -> 1, etc.).
    /// Returns 0 if not a heading.
    /// </summary>
    private static int GetHeadingLevel(Paragraph paragraph)
    {
        string? styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;

        if (string.IsNullOrEmpty(styleId) == true)
        {
            return 0;
        }

        //
        // Match "Heading1" through "Heading6" style names
        //
        if (styleId.StartsWith("Heading", StringComparison.OrdinalIgnoreCase) == true &&
            styleId.Length > 7)
        {
            string levelText = styleId.Substring(7);

            if (int.TryParse(levelText, out int level) == true && level >= 1 && level <= 6)
            {
                return level;
            }
        }

        //
        // Also check for "Title" style -> H1
        //
        if (string.Equals(styleId, "Title", StringComparison.OrdinalIgnoreCase) == true)
        {
            return 1;
        }

        //
        // "Subtitle" style -> H2
        //
        if (string.Equals(styleId, "Subtitle", StringComparison.OrdinalIgnoreCase) == true)
        {
            return 2;
        }

        return 0;
    }


    /// <summary>
    /// Get the list indent level from numbering properties.
    /// </summary>
    private static int GetListIndentLevel(NumberingProperties numberingProperties)
    {
        int? level = numberingProperties.NumberingLevelReference?.Val?.Value;

        if (level.HasValue == false)
        {
            return 0;
        }

        return level.Value;
    }


    /// <summary>
    /// Determine if a list is numbered (vs. bulleted) by checking the numbering definition.
    /// </summary>
    private static bool IsNumberedList(NumberingProperties numberingProperties,
        WordprocessingDocument wordDocument)
    {
        int? numId = numberingProperties.NumberingId?.Val?.Value;

        if (numId.HasValue == false)
        {
            return false;
        }

        //
        // Look up the numbering definition to check the format
        //
        NumberingDefinitionsPart? numberingPart = wordDocument.MainDocumentPart?.NumberingDefinitionsPart;

        if (numberingPart == null)
        {
            return false;
        }

        int? abstractNumIdValue = numberingPart.Numbering?
            .Elements<NumberingInstance>()
            .FirstOrDefault(n => n.NumberID?.Value == numId.Value)?
            .AbstractNumId?.Val?.Value;

        AbstractNum? abstractNum = abstractNumIdValue.HasValue == true
            ? numberingPart.Numbering?
                .Elements<AbstractNum>()
                .FirstOrDefault(a => a.AbstractNumberId?.Value == abstractNumIdValue.Value)
            : null;

        if (abstractNum == null)
        {
            return false;
        }

        //
        // Check the first level's number format
        //
        Level? firstLevel = abstractNum.Elements<Level>().FirstOrDefault();
        string? numberFormat = firstLevel?.NumberingFormat?.Val?.Value.ToString();

        //
        // "bullet" format means bulleted list; anything else is numbered
        //
        return string.Equals(numberFormat, "Bullet", StringComparison.OrdinalIgnoreCase) == false;
    }
}
