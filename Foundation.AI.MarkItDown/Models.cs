namespace Foundation.AI.MarkItDown;

/// <summary>
/// Result of a document-to-Markdown conversion.
/// </summary>
/// <param name="Markdown">The converted Markdown text.</param>
/// <param name="Title">Optional document title extracted during conversion.</param>
public record ConversionResult(string Markdown, string? Title = null);


/// <summary>
/// Metadata about the input stream: format hints, filenames, and source information.
///
/// <para>Used by <see cref="IDocumentConverter.Accepts"/> to determine format eligibility
/// and by the <see cref="MarkItDownService"/> orchestrator to build type guesses from
/// file paths, URLs, and content sniffing.</para>
/// </summary>
/// <param name="MimeType">MIME type (e.g., "application/pdf", "text/html").</param>
/// <param name="Extension">File extension including the dot (e.g., ".pdf", ".docx").</param>
/// <param name="Charset">Character encoding (e.g., "utf-8"). Relevant for text formats.</param>
/// <param name="FileName">Original filename, from path, URL, or Content-Disposition header.</param>
/// <param name="LocalPath">Local filesystem path, if the source is a file.</param>
/// <param name="Url">Source URL, if the content was fetched from the web.</param>
public record StreamInfo(
    string? MimeType = null,
    string? Extension = null,
    string? Charset = null,
    string? FileName = null,
    string? LocalPath = null,
    Uri? Url = null);
