namespace Foundation.AI.MarkItDown.Converters;

/// <summary>
/// Converts plain text files to Markdown (passthrough — content is already text).
///
/// <para>Accepts .txt, .md, .rst, .log, .ini, .cfg, .yaml, .yml, .toml files
/// and any stream with a text/* MIME type.</para>
///
/// <para><b>AI-developed:</b> Port of Python markitdown's PlainTextConverter.</para>
/// </summary>
public sealed class PlainTextConverter : IDocumentConverter
{
    //
    // Text-based extensions that this converter handles directly
    //
    private static readonly HashSet<string> TextExtensionSet = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".md", ".rst", ".log", ".ini", ".cfg",
        ".yaml", ".yml", ".toml", ".properties", ".env"
    };


    /// <inheritdoc />
    public string Name => "PlainText";

    /// <inheritdoc />
    public int Priority => 0;


    /// <inheritdoc />
    public bool Accepts(Stream stream, StreamInfo streamInfo)
    {
        //
        // Accept known text extensions
        //
        if (streamInfo.Extension != null && TextExtensionSet.Contains(streamInfo.Extension) == true)
        {
            return true;
        }

        //
        // Accept text/* MIME types (but not text/html or text/csv which have dedicated converters)
        //
        if (streamInfo.MimeType != null &&
            streamInfo.MimeType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) == true &&
            streamInfo.MimeType.Equals("text/html", StringComparison.OrdinalIgnoreCase) == false &&
            streamInfo.MimeType.Equals("text/csv", StringComparison.OrdinalIgnoreCase) == false)
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

        return new ConversionResult(Markdown: content);
    }
}
