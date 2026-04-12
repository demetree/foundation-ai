using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Foundation.AI.MarkItDown;

/// <summary>
/// Core orchestrator for document-to-Markdown conversion.
///
/// <para><b>Purpose:</b>
/// Collects all registered <see cref="IDocumentConverter"/> instances via DI,
/// sorts them by priority (lowest first), and executes the first-match conversion chain.
/// Handles stream buffering, file type detection, and output normalization.</para>
///
/// <para><b>Thread safety:</b>
/// This class is registered as a singleton and is safe for concurrent use.
/// Individual converters must also be stateless and thread-safe.</para>
///
/// <para><b>AI-developed:</b> C# port of Python markitdown's MarkItDown orchestrator class.</para>
/// </summary>
public sealed class MarkItDownService : IMarkItDown
{
    //
    // Converters sorted by priority (lowest first)
    //
    private readonly IReadOnlyList<IDocumentConverter> _converterList;

    //
    // Configuration
    //
    private readonly MarkItDownConfig _config;

    //
    // Logger
    //
    private readonly ILogger<MarkItDownService>? _logger;

    //
    // Regex for normalizing excessive blank lines (3+ newlines -> 2)
    //
    private static readonly Regex ExcessiveNewlinesRegex = new(@"\n{3,}", RegexOptions.Compiled);


    /// <summary>
    /// Create a new MarkItDownService with the given converters and configuration.
    /// Converters are collected from DI via <c>IEnumerable&lt;IDocumentConverter&gt;</c>.
    /// </summary>
    public MarkItDownService(
        IEnumerable<IDocumentConverter> converters,
        MarkItDownConfig config,
        ILogger<MarkItDownService>? logger = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger;

        //
        // Sort converters by priority (lowest = highest precedence), preserving
        // insertion order for converters with the same priority.
        //
        _converterList = converters
            .OrderBy(c => c.Priority)
            .ToList()
            .AsReadOnly();

        _logger?.LogInformation(
            "MarkItDown initialized with {Count} converters: {Names}",
            _converterList.Count,
            string.Join(", ", _converterList.Select(c => $"{c.Name}(p={c.Priority})")));
    }


    /// <inheritdoc />
    public async Task<ConversionResult> ConvertFileAsync(string filePath,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be null or empty.", nameof(filePath));
        }

        if (File.Exists(filePath) == false)
        {
            throw new FileNotFoundException($"File not found: {filePath}", filePath);
        }

        //
        // Check file size against configured limit
        //
        FileInfo fileInfo = new FileInfo(filePath);

        if (fileInfo.Length > _config.MaxFileSizeBytes)
        {
            throw new MarkItDownException(
                $"File size ({fileInfo.Length:N0} bytes) exceeds maximum allowed size ({_config.MaxFileSizeBytes:N0} bytes).");
        }

        //
        // Build stream info from the file path
        //
        StreamInfo streamInfo = FileTypeDetector.BuildFromFilePath(filePath);

        //
        // Open the file and run conversion
        //
        using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        //
        // Enrich stream info with content-based detection if MIME type is unknown
        //
        streamInfo = FileTypeDetector.EnrichFromContent(fileStream, streamInfo);
        fileStream.Position = 0;

        return await ConvertCoreAsync(fileStream, streamInfo, ct);
    }


    /// <inheritdoc />
    public async Task<ConversionResult> ConvertAsync(Stream stream,
        StreamInfo? streamInfo = null,
        CancellationToken ct = default)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        streamInfo ??= new StreamInfo();

        //
        // Buffer non-seekable streams so converters can seek
        //
        Stream workingStream = stream;
        bool ownsStream = false;

        if (stream.CanSeek == false)
        {
            MemoryStream memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, ct);
            memoryStream.Position = 0;
            workingStream = memoryStream;
            ownsStream = true;
        }

        try
        {
            //
            // Enrich stream info with content-based detection if MIME type is unknown
            //
            streamInfo = FileTypeDetector.EnrichFromContent(workingStream, streamInfo);
            workingStream.Position = 0;

            return await ConvertCoreAsync(workingStream, streamInfo, ct);
        }
        finally
        {
            if (ownsStream == true)
            {
                await workingStream.DisposeAsync();
            }
        }
    }


    /// <inheritdoc />
    public async Task<ConversionResult> ConvertUrlAsync(Uri url,
        CancellationToken ct = default)
    {
        if (url == null)
        {
            throw new ArgumentNullException(nameof(url));
        }

        //
        // Fetch the URL content
        //
        using HttpClient httpClient = new HttpClient();
        using HttpResponseMessage response = await httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        //
        // Build stream info from URL and response headers
        //
        string? contentType = response.Content.Headers.ContentType?.MediaType;
        string? fileName = url.Segments.LastOrDefault()?.TrimEnd('/');
        string? extension = fileName != null ? Path.GetExtension(fileName) : null;

        StreamInfo streamInfo = new StreamInfo(
            MimeType: contentType,
            Extension: extension,
            Charset: response.Content.Headers.ContentType?.CharSet,
            FileName: fileName,
            Url: url);

        //
        // Read response into a seekable memory stream
        //
        MemoryStream memoryStream = new MemoryStream();
        await response.Content.CopyToAsync(memoryStream, ct);
        memoryStream.Position = 0;

        using (memoryStream)
        {
            //
            // Enrich with content-based detection
            //
            streamInfo = FileTypeDetector.EnrichFromContent(memoryStream, streamInfo);
            memoryStream.Position = 0;

            return await ConvertCoreAsync(memoryStream, streamInfo, ct);
        }
    }


    /// <summary>
    /// Core conversion logic: iterate converters by priority, find first match, convert, normalize.
    /// </summary>
    private async Task<ConversionResult> ConvertCoreAsync(Stream stream,
        StreamInfo streamInfo,
        CancellationToken ct)
    {
        List<string> failedConverterList = new();

        foreach (IDocumentConverter converter in _converterList)
        {
            //
            // Check if this converter accepts the input
            //
            bool accepted = false;

            try
            {
                long positionBefore = stream.Position;
                accepted = converter.Accepts(stream, streamInfo);

                //
                // Verify the converter did not modify stream position
                //
                if (stream.Position != positionBefore)
                {
                    _logger?.LogWarning(
                        "Converter {Name} modified stream position during Accepts(). Resetting.",
                        converter.Name);
                    stream.Position = positionBefore;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Converter {Name} threw during Accepts(). Skipping.", converter.Name);
                continue;
            }

            if (accepted == false)
            {
                continue;
            }

            //
            // Attempt conversion
            //
            try
            {
                _logger?.LogDebug("Converter {Name} accepted. Converting...", converter.Name);
                stream.Position = 0;

                ConversionResult result = await converter.ConvertAsync(stream, streamInfo, ct);

                //
                // Normalize the output
                //
                string normalizedMarkdown = NormalizeOutput(result.Markdown);
                ConversionResult normalizedResult = result with { Markdown = normalizedMarkdown };

                _logger?.LogInformation(
                    "Conversion successful via {Name}. Output: {Length} characters.",
                    converter.Name,
                    normalizedMarkdown.Length);

                return normalizedResult;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex,
                    "Converter {Name} accepted but failed during conversion. Trying next.",
                    converter.Name);

                failedConverterList.Add(converter.Name);
                stream.Position = 0;
            }
        }

        //
        // No converter succeeded
        //
        if (failedConverterList.Count > 0)
        {
            throw new FileConversionException(
                $"Conversion failed. Converters that accepted but failed: {string.Join(", ", failedConverterList)}. " +
                $"StreamInfo: MimeType={streamInfo.MimeType}, Extension={streamInfo.Extension}, FileName={streamInfo.FileName}",
                converterName: string.Join(", ", failedConverterList),
                innerException: new InvalidOperationException("All accepting converters failed."));
        }

        throw new UnsupportedFormatException(
            $"No converter found for the input format. " +
            $"StreamInfo: MimeType={streamInfo.MimeType}, Extension={streamInfo.Extension}, FileName={streamInfo.FileName}",
            mimeType: streamInfo.MimeType,
            extension: streamInfo.Extension);
    }


    /// <summary>
    /// Normalize converter output: trim trailing whitespace, collapse excessive blank lines.
    /// </summary>
    private static string NormalizeOutput(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
        {
            return string.Empty;
        }

        //
        // Collapse 3+ consecutive newlines down to 2 (one blank line)
        //
        string normalized = ExcessiveNewlinesRegex.Replace(markdown, "\n\n");

        //
        // Trim trailing whitespace
        //
        normalized = normalized.TrimEnd();

        return normalized;
    }
}
