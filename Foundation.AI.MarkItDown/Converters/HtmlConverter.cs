using HtmlAgilityPack;
using ReverseMarkdown;

namespace Foundation.AI.MarkItDown.Converters;

/// <summary>
/// Converts HTML files to Markdown using HtmlAgilityPack for parsing
/// and ReverseMarkdown for HTML-to-Markdown transformation.
///
/// <para>Extracts the document title from the HTML &lt;title&gt; tag if present.
/// Handles both full HTML documents and HTML fragments.</para>
///
/// <para><b>AI-developed:</b> Port of Python markitdown's HtmlConverter
/// (BeautifulSoup + markdownify in Python, HtmlAgilityPack + ReverseMarkdown in C#).</para>
/// </summary>
public sealed class HtmlConverter : IDocumentConverter
{
    /// <inheritdoc />
    public string Name => "HTML";

    /// <inheritdoc />
    public int Priority => 0;


    /// <inheritdoc />
    public bool Accepts(Stream stream, StreamInfo streamInfo)
    {
        if (streamInfo.Extension != null &&
            (string.Equals(streamInfo.Extension, ".html", StringComparison.OrdinalIgnoreCase) == true ||
             string.Equals(streamInfo.Extension, ".htm", StringComparison.OrdinalIgnoreCase) == true))
        {
            return true;
        }

        if (string.Equals(streamInfo.MimeType, "text/html", StringComparison.OrdinalIgnoreCase) == true)
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
        string htmlContent = await reader.ReadToEndAsync(ct);

        //
        // Parse the HTML document to extract the title
        //
        HtmlDocument htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlContent);

        string? title = htmlDocument.DocumentNode
            .SelectSingleNode("//title")
            ?.InnerText
            ?.Trim();

        //
        // Convert HTML to Markdown using ReverseMarkdown
        //
        Converter converter = new Converter(new ReverseMarkdown.Config
        {
            GithubFlavored = true,
            RemoveComments = true,
            SmartHrefHandling = true
        });

        string markdown = converter.Convert(htmlContent);

        return new ConversionResult(
            Markdown: markdown,
            Title: string.IsNullOrWhiteSpace(title) ? null : title);
    }
}
