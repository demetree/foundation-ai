using System.Xml.Linq;

namespace Foundation.AI.MarkItDown.Converters;

/// <summary>
/// Converts XML files to Markdown as a fenced code block with syntax highlighting.
///
/// <para>The XML content is pretty-printed with indentation inside a
/// <c>```xml</c> fenced code block.</para>
///
/// <para><b>AI-developed:</b> Port of Python markitdown's XML handling in PlainTextConverter.</para>
/// </summary>
public sealed class XmlConverter : IDocumentConverter
{
    /// <inheritdoc />
    public string Name => "XML";

    /// <inheritdoc />
    public int Priority => 0;


    /// <inheritdoc />
    public bool Accepts(Stream stream, StreamInfo streamInfo)
    {
        if (string.Equals(streamInfo.Extension, ".xml", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        if (streamInfo.MimeType != null &&
            (string.Equals(streamInfo.MimeType, "application/xml", StringComparison.OrdinalIgnoreCase) == true ||
             string.Equals(streamInfo.MimeType, "text/xml", StringComparison.OrdinalIgnoreCase) == true))
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
        string rawContent = await reader.ReadToEndAsync(ct);

        //
        // Attempt to pretty-print the XML for readability
        //
        string formattedContent = TryPrettyPrint(rawContent);

        string markdown = $"```xml\n{formattedContent}\n```";

        return new ConversionResult(Markdown: markdown);
    }


    /// <summary>
    /// Attempt to parse and re-serialize XML with indentation.
    /// Falls back to the original content if parsing fails.
    /// </summary>
    private static string TryPrettyPrint(string rawXml)
    {
        try
        {
            XDocument document = XDocument.Parse(rawXml);
            return document.ToString(SaveOptions.None);
        }
        catch
        {
            //
            // If parsing fails, return the raw content as-is
            //
            return rawXml;
        }
    }
}
