namespace Foundation.AI.MarkItDown;

/// <summary>
/// Configuration options for the MarkItDown conversion service.
/// </summary>
public class MarkItDownConfig
{
    /// <summary>
    /// Whether to register the built-in converters (PlainText, CSV, JSON, XML, HTML, ZIP, EPUB, Jupyter).
    /// Default: true.
    /// </summary>
    public bool EnableBuiltInConverters { get; set; } = true;

    /// <summary>
    /// Maximum allowed file size in bytes. Files exceeding this limit will be rejected.
    /// Default: 100 MB.
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 100 * 1024 * 1024;
}
