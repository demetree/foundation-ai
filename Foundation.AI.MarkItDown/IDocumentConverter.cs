namespace Foundation.AI.MarkItDown;

/// <summary>
/// Contract for a format-specific document-to-Markdown converter.
///
/// <para><b>Purpose:</b>
/// Each converter handles one or more file formats. The <see cref="MarkItDownService"/>
/// orchestrator iterates registered converters in priority order (lowest first),
/// calling <see cref="Accepts"/> to find the first match, then <see cref="ConvertAsync"/>
/// to perform the conversion.</para>
///
/// <para><b>Implementation contract:</b>
/// <list type="bullet">
/// <item><see cref="Accepts"/> must not modify the stream position.</item>
/// <item><see cref="Accepts"/> must be fast — check MIME type and extension, not full content parsing.</item>
/// <item><see cref="ConvertAsync"/> may assume the stream is seekable (the orchestrator buffers if needed).</item>
/// <item>Converters must be stateless and thread-safe (registered as singletons).</item>
/// </list></para>
///
/// <para><b>AI-developed:</b> Port of Python markitdown's DocumentConverter base class.</para>
/// </summary>
public interface IDocumentConverter
{
    /// <summary>
    /// Human-readable converter name (e.g., "PDF", "DOCX", "HTML").
    /// Used for logging and diagnostics.
    /// </summary>
    string Name { get; }


    /// <summary>
    /// Converter priority. Lower values are tried first.
    /// Built-in format-specific converters use 0. Generic fallbacks use 100.
    /// </summary>
    int Priority { get; }


    /// <summary>
    /// Check whether this converter can handle the given input.
    /// Must not modify the stream position.
    /// </summary>
    /// <param name="stream">The input file stream (position may be at start or elsewhere).</param>
    /// <param name="streamInfo">Format metadata: MIME type, extension, filename, etc.</param>
    /// <returns>True if this converter can handle the format.</returns>
    bool Accepts(Stream stream, StreamInfo streamInfo);


    /// <summary>
    /// Convert the input stream to Markdown.
    /// </summary>
    /// <param name="stream">Seekable input stream positioned at the start.</param>
    /// <param name="streamInfo">Format metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Conversion result with Markdown text and optional title.</returns>
    Task<ConversionResult> ConvertAsync(Stream stream,
        StreamInfo streamInfo,
        CancellationToken ct = default);
}
