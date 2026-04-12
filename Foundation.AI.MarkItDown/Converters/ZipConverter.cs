using System.IO.Compression;
using System.Text;

namespace Foundation.AI.MarkItDown.Converters;

/// <summary>
/// Converts ZIP archives to Markdown by listing contents and recursively
/// converting each entry using the MarkItDown orchestrator.
///
/// <para><b>Purpose:</b>
/// Extracts each file from the ZIP archive, converts it to Markdown using
/// the registered converters, and concatenates the results with entry headers.
/// Nested ZIP archives are handled recursively.</para>
///
/// <para><b>AI-developed:</b> Port of Python markitdown's ZipConverter.</para>
/// </summary>
public sealed class ZipConverter : IDocumentConverter
{
    //
    // Reference to the orchestrator for recursive conversion of archive entries.
    // Injected to avoid circular DI; set via the service extensions registration.
    //
    private IMarkItDown? _markItDown;


    /// <inheritdoc />
    public string Name => "ZIP";

    /// <inheritdoc />
    public int Priority => 0;


    /// <summary>
    /// Set the orchestrator reference for recursive conversion.
    /// Called during DI initialization.
    /// </summary>
    internal void SetOrchestrator(IMarkItDown markItDown)
    {
        _markItDown = markItDown;
    }


    /// <inheritdoc />
    public bool Accepts(Stream stream, StreamInfo streamInfo)
    {
        if (string.Equals(streamInfo.Extension, ".zip", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        if (string.Equals(streamInfo.MimeType, "application/zip", StringComparison.OrdinalIgnoreCase) == true)
        {
            //
            // Don't accept ZIP MIME type if the extension indicates a more specific format
            // (DOCX, PPTX, XLSX, EPUB are all ZIP-based but have their own converters)
            //
            if (streamInfo.Extension != null)
            {
                string extension = streamInfo.Extension.ToLowerInvariant();

                if (extension == ".docx" || extension == ".pptx" || extension == ".xlsx" || extension == ".epub")
                {
                    return false;
                }
            }

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
        markdownBuilder.AppendLine("# ZIP Archive Contents");
        markdownBuilder.AppendLine();

        using ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);

        //
        // List all entries first
        //
        markdownBuilder.AppendLine("## File Listing");
        markdownBuilder.AppendLine();

        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name) == true)
            {
                continue;  // Skip directory entries
            }

            string sizeText = FormatFileSize(entry.Length);
            markdownBuilder.AppendLine($"- `{entry.FullName}` ({sizeText})");
        }

        markdownBuilder.AppendLine();

        //
        // Convert each entry if the orchestrator is available
        //
        if (_markItDown != null)
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                ct.ThrowIfCancellationRequested();

                if (string.IsNullOrEmpty(entry.Name) == true)
                {
                    continue;
                }

                markdownBuilder.AppendLine($"## {entry.FullName}");
                markdownBuilder.AppendLine();

                try
                {
                    using Stream entryStream = entry.Open();

                    //
                    // Buffer to a seekable memory stream
                    //
                    using MemoryStream memoryStream = new MemoryStream();
                    await entryStream.CopyToAsync(memoryStream, ct);
                    memoryStream.Position = 0;

                    StreamInfo entryInfo = new StreamInfo(
                        Extension: Path.GetExtension(entry.Name),
                        FileName: entry.Name,
                        MimeType: FileTypeDetector.GetMimeTypeFromExtension(Path.GetExtension(entry.Name)));

                    ConversionResult entryResult = await _markItDown.ConvertAsync(memoryStream, entryInfo, ct);

                    if (string.IsNullOrWhiteSpace(entryResult.Markdown) == false)
                    {
                        markdownBuilder.AppendLine(entryResult.Markdown);
                    }
                    else
                    {
                        markdownBuilder.AppendLine("*(empty or binary content)*");
                    }
                }
                catch (Exception)
                {
                    markdownBuilder.AppendLine("*(could not convert this entry)*");
                }

                markdownBuilder.AppendLine();
            }
        }

        return new ConversionResult(Markdown: markdownBuilder.ToString());
    }


    /// <summary>
    /// Format a file size in bytes to a human-readable string.
    /// </summary>
    private static string FormatFileSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        if (bytes < 1024 * 1024)
        {
            return $"{bytes / 1024.0:F1} KB";
        }

        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }
}
