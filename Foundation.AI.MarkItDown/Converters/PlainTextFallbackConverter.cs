using System.Text;

namespace Foundation.AI.MarkItDown.Converters;

/// <summary>
/// Last-resort fallback converter that attempts to read any file as plain text.
///
/// <para>Runs at priority 100 (lowest precedence) and accepts any file whose
/// content appears to be valid text (no null bytes in the first 8KB).
/// This ensures that unrecognized text-based formats still get converted
/// rather than throwing <see cref="UnsupportedFormatException"/>.</para>
///
/// <para><b>AI-developed:</b> Inspired by Python markitdown's PlainTextConverter fallback behavior
/// with charset-normalizer detection.</para>
/// </summary>
public sealed class PlainTextFallbackConverter : IDocumentConverter
{
    //
    // Number of bytes to sample for the text detection heuristic
    //
    private const int SampleSize = 8192;


    /// <inheritdoc />
    public string Name => "PlainTextFallback";

    /// <inheritdoc />
    public int Priority => 100;


    /// <inheritdoc />
    public bool Accepts(Stream stream, StreamInfo streamInfo)
    {
        if (stream == null || stream.Length == 0 || stream.CanSeek == false)
        {
            return false;
        }

        //
        // Read a sample of bytes and check for null bytes (binary indicator)
        //
        long originalPosition = stream.Position;

        try
        {
            stream.Position = 0;
            int bytesToRead = (int)Math.Min(SampleSize, stream.Length);
            byte[] sampleBuffer = new byte[bytesToRead];
            int bytesRead = stream.Read(sampleBuffer, 0, bytesToRead);

            //
            // If the sample contains null bytes, this is likely a binary file
            //
            for (int i = 0; i < bytesRead; i++)
            {
                if (sampleBuffer[i] == 0)
                {
                    return false;
                }
            }

            return true;
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }


    /// <inheritdoc />
    public async Task<ConversionResult> ConvertAsync(Stream stream,
        StreamInfo streamInfo,
        CancellationToken ct = default)
    {
        //
        // Determine encoding: use the charset hint if available, otherwise default to UTF-8
        //
        Encoding encoding = Encoding.UTF8;

        if (string.IsNullOrEmpty(streamInfo.Charset) == false)
        {
            try
            {
                encoding = Encoding.GetEncoding(streamInfo.Charset);
            }
            catch
            {
                //
                // Fall back to UTF-8 if the charset is not recognized
                //
                encoding = Encoding.UTF8;
            }
        }

        using StreamReader reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        string content = await reader.ReadToEndAsync(ct);

        return new ConversionResult(Markdown: content);
    }
}
