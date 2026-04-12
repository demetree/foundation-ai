using System.Text.Json;

namespace Foundation.AI.MarkItDown.Converters;

/// <summary>
/// Converts JSON files to Markdown as a fenced code block with syntax highlighting.
///
/// <para>The JSON content is pretty-printed for readability inside a
/// <c>```json</c> fenced code block.</para>
///
/// <para><b>AI-developed:</b> Port of Python markitdown's JSON handling in PlainTextConverter.</para>
/// </summary>
public sealed class JsonConverter : IDocumentConverter
{
    /// <inheritdoc />
    public string Name => "JSON";

    /// <inheritdoc />
    public int Priority => 0;


    /// <inheritdoc />
    public bool Accepts(Stream stream, StreamInfo streamInfo)
    {
        if (string.Equals(streamInfo.Extension, ".json", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        if (string.Equals(streamInfo.MimeType, "application/json", StringComparison.OrdinalIgnoreCase) == true)
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
        // Attempt to pretty-print the JSON for readability
        //
        string formattedContent = TryPrettyPrint(rawContent);

        string markdown = $"```json\n{formattedContent}\n```";

        return new ConversionResult(Markdown: markdown);
    }


    /// <summary>
    /// Attempt to parse and re-serialize JSON with indentation.
    /// Falls back to the original content if parsing fails.
    /// </summary>
    private static string TryPrettyPrint(string rawJson)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(rawJson);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            return JsonSerializer.Serialize(document, options);
        }
        catch
        {
            //
            // If parsing fails, return the raw content as-is
            //
            return rawJson;
        }
    }
}
