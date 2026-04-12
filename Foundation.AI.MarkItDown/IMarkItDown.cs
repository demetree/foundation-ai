namespace Foundation.AI.MarkItDown;

/// <summary>
/// Document-to-Markdown conversion service.
///
/// <para><b>Purpose:</b>
/// Converts various file formats (PDF, DOCX, PPTX, XLSX, HTML, images, audio, etc.)
/// into clean Markdown optimized for LLM consumption. Preserves document structure
/// including headings, lists, tables, and links.</para>
///
/// <para><b>Architecture:</b>
/// Uses a priority-based converter chain. Each registered <see cref="IDocumentConverter"/>
/// is tried in priority order (lowest first). The first converter that accepts the
/// input format performs the conversion.</para>
///
/// <para><b>Usage:</b>
/// <code>
/// // Convert a local file
/// var result = await markItDown.ConvertFileAsync("report.pdf", ct);
/// Console.WriteLine(result.Markdown);
///
/// // Convert a stream with format hints
/// var info = new StreamInfo(MimeType: "application/pdf", FileName: "report.pdf");
/// var result = await markItDown.ConvertAsync(stream, info, ct);
///
/// // Pipe into RAG indexing
/// await ragService.IndexFileAsync(markItDown, "docs", "report-1", "report.pdf");
/// </code></para>
///
/// <para><b>AI-developed:</b> This service is a C# port of Microsoft's markitdown Python library,
/// adapted to fit the Foundation.AI provider architecture and .NET conventions.</para>
/// </summary>
public interface IMarkItDown
{
    /// <summary>
    /// Convert a stream to Markdown.
    /// </summary>
    /// <param name="stream">The input file stream. Will be buffered to a seekable stream if necessary.</param>
    /// <param name="streamInfo">Optional format hints (MIME type, extension, filename).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Conversion result containing the Markdown text and optional title.</returns>
    Task<ConversionResult> ConvertAsync(Stream stream,
        StreamInfo? streamInfo = null,
        CancellationToken ct = default);


    /// <summary>
    /// Convert a local file to Markdown.
    /// </summary>
    /// <param name="filePath">Absolute or relative path to the file.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Conversion result containing the Markdown text and optional title.</returns>
    Task<ConversionResult> ConvertFileAsync(string filePath,
        CancellationToken ct = default);


    /// <summary>
    /// Fetch a URL and convert its content to Markdown.
    /// </summary>
    /// <param name="url">The URL to fetch.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Conversion result containing the Markdown text and optional title.</returns>
    Task<ConversionResult> ConvertUrlAsync(Uri url,
        CancellationToken ct = default);
}
