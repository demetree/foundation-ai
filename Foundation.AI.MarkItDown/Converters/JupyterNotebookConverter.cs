using System.Text;
using System.Text.Json;

namespace Foundation.AI.MarkItDown.Converters;

/// <summary>
/// Converts Jupyter Notebook (.ipynb) files to Markdown.
///
/// <para><b>Output format:</b>
/// Markdown cells are rendered verbatim. Code cells are wrapped in fenced code blocks
/// with the kernel language as the syntax hint. Cell outputs (text and error) are
/// included in separate fenced blocks marked as "output".</para>
///
/// <para><b>AI-developed:</b> Port of Python markitdown's IpynbConverter.</para>
/// </summary>
public sealed class JupyterNotebookConverter : IDocumentConverter
{
    /// <inheritdoc />
    public string Name => "Jupyter";

    /// <inheritdoc />
    public int Priority => 0;


    /// <inheritdoc />
    public bool Accepts(Stream stream, StreamInfo streamInfo)
    {
        if (string.Equals(streamInfo.Extension, ".ipynb", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        if (string.Equals(streamInfo.MimeType, "application/x-ipynb+json", StringComparison.OrdinalIgnoreCase) == true)
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
        string jsonContent = await reader.ReadToEndAsync(ct);

        using JsonDocument document = JsonDocument.Parse(jsonContent);
        JsonElement root = document.RootElement;

        StringBuilder markdownBuilder = new StringBuilder();

        //
        // Extract the kernel language for code cell syntax hints
        //
        string language = "python";  // default

        if (root.TryGetProperty("metadata", out JsonElement metadata) == true &&
            metadata.TryGetProperty("kernelspec", out JsonElement kernelspec) == true &&
            kernelspec.TryGetProperty("language", out JsonElement languageElement) == true)
        {
            string? kernelLanguage = languageElement.GetString();

            if (string.IsNullOrEmpty(kernelLanguage) == false)
            {
                language = kernelLanguage.ToLowerInvariant();
            }
        }

        //
        // Process each cell in the notebook
        //
        if (root.TryGetProperty("cells", out JsonElement cells) == true && cells.ValueKind == JsonValueKind.Array)
        {
            int cellNumber = 0;

            foreach (JsonElement cell in cells.EnumerateArray())
            {
                ct.ThrowIfCancellationRequested();
                cellNumber++;

                string cellType = cell.TryGetProperty("cell_type", out JsonElement cellTypeElement) == true
                    ? cellTypeElement.GetString() ?? "code"
                    : "code";

                string source = ExtractSourceText(cell);

                if (cellType == "markdown")
                {
                    //
                    // Markdown cells are rendered verbatim
                    //
                    if (string.IsNullOrWhiteSpace(source) == false)
                    {
                        markdownBuilder.AppendLine(source.Trim());
                        markdownBuilder.AppendLine();
                    }
                }
                else if (cellType == "code")
                {
                    //
                    // Code cells get fenced code blocks
                    //
                    if (string.IsNullOrWhiteSpace(source) == false)
                    {
                        markdownBuilder.AppendLine($"```{language}");
                        markdownBuilder.AppendLine(source.TrimEnd());
                        markdownBuilder.AppendLine("```");
                        markdownBuilder.AppendLine();
                    }

                    //
                    // Include cell outputs if present
                    //
                    string outputText = ExtractOutputText(cell);

                    if (string.IsNullOrWhiteSpace(outputText) == false)
                    {
                        markdownBuilder.AppendLine("```output");
                        markdownBuilder.AppendLine(outputText.TrimEnd());
                        markdownBuilder.AppendLine("```");
                        markdownBuilder.AppendLine();
                    }
                }
                else if (cellType == "raw")
                {
                    //
                    // Raw cells are rendered as preformatted text
                    //
                    if (string.IsNullOrWhiteSpace(source) == false)
                    {
                        markdownBuilder.AppendLine("```");
                        markdownBuilder.AppendLine(source.TrimEnd());
                        markdownBuilder.AppendLine("```");
                        markdownBuilder.AppendLine();
                    }
                }
            }
        }

        return new ConversionResult(Markdown: markdownBuilder.ToString());
    }


    /// <summary>
    /// Extract the source text from a cell. The "source" field can be a string or array of strings.
    /// </summary>
    private static string ExtractSourceText(JsonElement cell)
    {
        if (cell.TryGetProperty("source", out JsonElement source) == false)
        {
            return string.Empty;
        }

        if (source.ValueKind == JsonValueKind.String)
        {
            return source.GetString() ?? string.Empty;
        }

        if (source.ValueKind == JsonValueKind.Array)
        {
            StringBuilder sourceBuilder = new StringBuilder();

            foreach (JsonElement line in source.EnumerateArray())
            {
                sourceBuilder.Append(line.GetString() ?? "");
            }

            return sourceBuilder.ToString();
        }

        return string.Empty;
    }


    /// <summary>
    /// Extract text output from a code cell's outputs array.
    /// Handles "stream", "execute_result", "display_data", and "error" output types.
    /// </summary>
    private static string ExtractOutputText(JsonElement cell)
    {
        if (cell.TryGetProperty("outputs", out JsonElement outputs) == false ||
            outputs.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        StringBuilder outputBuilder = new StringBuilder();

        foreach (JsonElement output in outputs.EnumerateArray())
        {
            string outputType = output.TryGetProperty("output_type", out JsonElement outputTypeElement) == true
                ? outputTypeElement.GetString() ?? ""
                : "";

            if (outputType == "stream")
            {
                //
                // Stream output (stdout/stderr)
                //
                string text = ExtractTextArray(output, "text");

                if (string.IsNullOrEmpty(text) == false)
                {
                    outputBuilder.Append(text);
                }
            }
            else if (outputType == "execute_result" || outputType == "display_data")
            {
                //
                // Rich output — extract text/plain representation
                //
                if (output.TryGetProperty("data", out JsonElement data) == true &&
                    data.TryGetProperty("text/plain", out JsonElement textPlain) == true)
                {
                    string text = ExtractTextValue(textPlain);

                    if (string.IsNullOrEmpty(text) == false)
                    {
                        outputBuilder.Append(text);
                    }
                }
            }
            else if (outputType == "error")
            {
                //
                // Error output — extract traceback
                //
                if (output.TryGetProperty("traceback", out JsonElement traceback) == true &&
                    traceback.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement line in traceback.EnumerateArray())
                    {
                        outputBuilder.AppendLine(line.GetString() ?? "");
                    }
                }
            }
        }

        return outputBuilder.ToString();
    }


    /// <summary>
    /// Extract a text value from a JSON element that may be a string or array of strings.
    /// </summary>
    private static string ExtractTextValue(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            return element.GetString() ?? string.Empty;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            StringBuilder builder = new StringBuilder();

            foreach (JsonElement line in element.EnumerateArray())
            {
                builder.Append(line.GetString() ?? "");
            }

            return builder.ToString();
        }

        return string.Empty;
    }


    /// <summary>
    /// Extract text from a named array property on a JSON element.
    /// </summary>
    private static string ExtractTextArray(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out JsonElement textElement) == true)
        {
            return ExtractTextValue(textElement);
        }

        return string.Empty;
    }
}
