using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using HtmlAgilityPack;
using ReverseMarkdown;

namespace Foundation.AI.MarkItDown.Converters;

/// <summary>
/// Converts EPUB e-book files to Markdown.
///
/// <para><b>Purpose:</b>
/// EPUB files are ZIP archives containing XHTML chapters, CSS, and metadata.
/// This converter reads the OPF manifest to find the reading order (spine),
/// extracts the title from Dublin Core metadata, and converts each XHTML
/// chapter to Markdown using ReverseMarkdown.</para>
///
/// <para><b>AI-developed:</b> Port of Python markitdown's EPUB handling.</para>
/// </summary>
public sealed class EpubConverter : IDocumentConverter
{
    /// <inheritdoc />
    public string Name => "EPUB";

    /// <inheritdoc />
    public int Priority => 0;


    /// <inheritdoc />
    public bool Accepts(Stream stream, StreamInfo streamInfo)
    {
        if (string.Equals(streamInfo.Extension, ".epub", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        if (string.Equals(streamInfo.MimeType, "application/epub+zip", StringComparison.OrdinalIgnoreCase) == true)
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
        StringBuilder markdownBuilder = new StringBuilder();
        string? documentTitle = null;

        using ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);

        //
        // Find the OPF (Open Packaging Format) file via the container.xml
        //
        string? opfPath = FindOpfPath(archive);

        if (opfPath == null)
        {
            //
            // Fallback: convert all HTML/XHTML files found in the archive
            //
            return await ConvertAllHtmlEntries(archive, ct);
        }

        //
        // Parse the OPF to get the reading order and metadata
        //
        ZipArchiveEntry? opfEntry = archive.GetEntry(opfPath);

        if (opfEntry == null)
        {
            return await ConvertAllHtmlEntries(archive, ct);
        }

        string opfDirectory = Path.GetDirectoryName(opfPath)?.Replace('\\', '/') ?? "";

        using Stream opfStream = opfEntry.Open();
        XDocument opfDocument = await XDocument.LoadAsync(opfStream, LoadOptions.None, ct);

        //
        // Extract title from Dublin Core metadata
        //
        XNamespace dcNamespace = "http://purl.org/dc/elements/1.1/";
        documentTitle = opfDocument.Descendants(dcNamespace + "title").FirstOrDefault()?.Value?.Trim();

        if (string.IsNullOrWhiteSpace(documentTitle) == false)
        {
            markdownBuilder.AppendLine($"# {documentTitle}");
            markdownBuilder.AppendLine();
        }

        //
        // Build the manifest: id -> href mapping
        //
        XNamespace opfNamespace = "http://www.idpf.org/2007/opf";
        Dictionary<string, string> manifestMap = new();

        foreach (XElement item in opfDocument.Descendants(opfNamespace + "item"))
        {
            string? itemId = item.Attribute("id")?.Value;
            string? itemHref = item.Attribute("href")?.Value;
            string? mediaType = item.Attribute("media-type")?.Value;

            if (itemId != null && itemHref != null)
            {
                manifestMap[itemId] = itemHref;
            }
        }

        //
        // Get spine items (reading order)
        //
        List<string> spineItemRefList = opfDocument.Descendants(opfNamespace + "itemref")
            .Select(e => e.Attribute("idref")?.Value)
            .Where(v => v != null)
            .Cast<string>()
            .ToList();

        //
        // Convert each spine item in reading order
        //
        Converter htmlToMarkdownConverter = new Converter(new ReverseMarkdown.Config
        {
            GithubFlavored = true,
            RemoveComments = true,
            SmartHrefHandling = true
        });

        int chapterNumber = 0;

        foreach (string itemRef in spineItemRefList)
        {
            ct.ThrowIfCancellationRequested();

            if (manifestMap.TryGetValue(itemRef, out string? href) == false)
            {
                continue;
            }

            //
            // Resolve the full path relative to the OPF directory
            //
            string entryPath = string.IsNullOrEmpty(opfDirectory) == true
                ? href
                : $"{opfDirectory}/{href}";

            ZipArchiveEntry? chapterEntry = archive.GetEntry(entryPath);

            if (chapterEntry == null)
            {
                continue;
            }

            chapterNumber++;

            using Stream chapterStream = chapterEntry.Open();
            using StreamReader chapterReader = new StreamReader(chapterStream);
            string htmlContent = await chapterReader.ReadToEndAsync(ct);

            //
            // Convert HTML to Markdown
            //
            string chapterMarkdown = htmlToMarkdownConverter.Convert(htmlContent);

            if (string.IsNullOrWhiteSpace(chapterMarkdown) == false)
            {
                markdownBuilder.AppendLine(chapterMarkdown.Trim());
                markdownBuilder.AppendLine();
            }
        }

        ConversionResult result = new ConversionResult(
            Markdown: markdownBuilder.ToString(),
            Title: documentTitle);

        return result;
    }


    /// <summary>
    /// Find the OPF file path by reading META-INF/container.xml.
    /// </summary>
    private static string? FindOpfPath(ZipArchive archive)
    {
        ZipArchiveEntry? containerEntry = archive.GetEntry("META-INF/container.xml");

        if (containerEntry == null)
        {
            return null;
        }

        using Stream containerStream = containerEntry.Open();
        XDocument containerDocument = XDocument.Load(containerStream);

        XNamespace containerNamespace = "urn:oasis:names:tc:opendocument:xmlns:container";

        string? opfPath = containerDocument
            .Descendants(containerNamespace + "rootfile")
            .FirstOrDefault()
            ?.Attribute("full-path")
            ?.Value;

        return opfPath;
    }


    /// <summary>
    /// Fallback: convert all HTML/XHTML entries found in the archive.
    /// </summary>
    private static async Task<ConversionResult> ConvertAllHtmlEntries(ZipArchive archive,
        CancellationToken ct)
    {
        StringBuilder markdownBuilder = new StringBuilder();

        Converter htmlToMarkdownConverter = new Converter(new ReverseMarkdown.Config
        {
            GithubFlavored = true,
            RemoveComments = true
        });

        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            ct.ThrowIfCancellationRequested();

            string extension = Path.GetExtension(entry.Name).ToLowerInvariant();

            if (extension != ".html" && extension != ".xhtml" && extension != ".htm")
            {
                continue;
            }

            using Stream entryStream = entry.Open();
            using StreamReader reader = new StreamReader(entryStream);
            string htmlContent = await reader.ReadToEndAsync(ct);

            string chapterMarkdown = htmlToMarkdownConverter.Convert(htmlContent);

            if (string.IsNullOrWhiteSpace(chapterMarkdown) == false)
            {
                markdownBuilder.AppendLine(chapterMarkdown.Trim());
                markdownBuilder.AppendLine();
            }
        }

        return new ConversionResult(Markdown: markdownBuilder.ToString());
    }
}
