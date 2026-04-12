using System.Text;

namespace Foundation.AI.MarkItDown.Media;

/// <summary>
/// Converts audio files to Markdown by extracting tag metadata (title, artist, album, duration).
///
/// <para><b>Output format:</b>
/// A metadata table with audio properties extracted from ID3/Vorbis/APE tags
/// using TagLibSharp. Does not transcribe audio content — for speech-to-text,
/// a future integration with a speech recognition provider would be needed.</para>
///
/// <para><b>AI-developed:</b> Port of Python markitdown's AudioConverter metadata extraction
/// (pydub/mutagen in Python, TagLibSharp in C#).</para>
/// </summary>
public sealed class AudioConverter : IDocumentConverter
{
    //
    // Audio extensions this converter handles
    //
    private static readonly HashSet<string> AudioExtensionSet = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".wav", ".m4a", ".flac", ".ogg", ".wma", ".aac", ".opus"
    };


    /// <inheritdoc />
    public string Name => "Audio";

    /// <inheritdoc />
    public int Priority => 0;


    /// <inheritdoc />
    public bool Accepts(Stream stream, StreamInfo streamInfo)
    {
        if (streamInfo.Extension != null && AudioExtensionSet.Contains(streamInfo.Extension) == true)
        {
            return true;
        }

        if (streamInfo.MimeType != null &&
            streamInfo.MimeType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        return false;
    }


    /// <inheritdoc />
    public Task<ConversionResult> ConvertAsync(Stream stream,
        StreamInfo streamInfo,
        CancellationToken ct = default)
    {
        StringBuilder markdownBuilder = new StringBuilder();
        string? title = null;

        markdownBuilder.AppendLine($"# Audio: {streamInfo.FileName ?? "Unknown"}");
        markdownBuilder.AppendLine();

        //
        // TagLib requires a file path or an IFileAbstraction — use the local path if available,
        // otherwise save to a temp file for tag reading
        //
        try
        {
            string? filePath = streamInfo.LocalPath;
            string? tempPath = null;

            if (string.IsNullOrEmpty(filePath) == true)
            {
                //
                // Save stream to a temporary file for TagLib to read
                //
                tempPath = Path.GetTempFileName() + (streamInfo.Extension ?? ".tmp");
                using FileStream tempStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write);
                stream.Position = 0;
                stream.CopyTo(tempStream);
                filePath = tempPath;
            }

            try
            {
                using TagLib.File tagFile = TagLib.File.Create(filePath);
                TagLib.Tag tag = tagFile.Tag;

                List<(string Name, string Value)> metadataEntryList = new();

                //
                // Extract tag metadata
                //
                if (string.IsNullOrWhiteSpace(tag.Title) == false)
                {
                    metadataEntryList.Add(("Title", tag.Title.Trim()));
                    title = tag.Title.Trim();
                }

                if (string.IsNullOrWhiteSpace(tag.JoinedPerformers) == false)
                {
                    metadataEntryList.Add(("Artist", tag.JoinedPerformers.Trim()));
                }

                if (string.IsNullOrWhiteSpace(tag.Album) == false)
                {
                    metadataEntryList.Add(("Album", tag.Album.Trim()));
                }

                if (tag.Year > 0)
                {
                    metadataEntryList.Add(("Year", tag.Year.ToString()));
                }

                if (tag.Track > 0)
                {
                    metadataEntryList.Add(("Track", tag.Track.ToString()));
                }

                if (string.IsNullOrWhiteSpace(tag.JoinedGenres) == false)
                {
                    metadataEntryList.Add(("Genre", tag.JoinedGenres.Trim()));
                }

                //
                // Extract audio properties (duration, bitrate, channels, sample rate)
                //
                TagLib.Properties audioProperties = tagFile.Properties;

                if (audioProperties.Duration.TotalSeconds > 0)
                {
                    metadataEntryList.Add(("Duration", FormatDuration(audioProperties.Duration)));
                }

                if (audioProperties.AudioBitrate > 0)
                {
                    metadataEntryList.Add(("Bitrate", $"{audioProperties.AudioBitrate} kbps"));
                }

                if (audioProperties.AudioSampleRate > 0)
                {
                    metadataEntryList.Add(("Sample Rate", $"{audioProperties.AudioSampleRate} Hz"));
                }

                if (audioProperties.AudioChannels > 0)
                {
                    metadataEntryList.Add(("Channels", audioProperties.AudioChannels.ToString()));
                }

                if (string.IsNullOrWhiteSpace(audioProperties.Description) == false)
                {
                    metadataEntryList.Add(("Codec", audioProperties.Description.Trim()));
                }

                //
                // Render metadata table
                //
                if (metadataEntryList.Count > 0)
                {
                    markdownBuilder.AppendLine("## Metadata");
                    markdownBuilder.AppendLine();
                    markdownBuilder.AppendLine("| Property | Value |");
                    markdownBuilder.AppendLine("| --- | --- |");

                    foreach ((string name, string value) in metadataEntryList)
                    {
                        markdownBuilder.AppendLine($"| {name} | {value.Replace("|", "\\|")} |");
                    }

                    markdownBuilder.AppendLine();
                }
                else
                {
                    markdownBuilder.AppendLine("*(No metadata found)*");
                    markdownBuilder.AppendLine();
                }

                //
                // Include lyrics if present
                //
                if (string.IsNullOrWhiteSpace(tag.Lyrics) == false)
                {
                    markdownBuilder.AppendLine("## Lyrics");
                    markdownBuilder.AppendLine();
                    markdownBuilder.AppendLine(tag.Lyrics.Trim());
                    markdownBuilder.AppendLine();
                }

                //
                // Include comment if present
                //
                if (string.IsNullOrWhiteSpace(tag.Comment) == false)
                {
                    markdownBuilder.AppendLine("## Comment");
                    markdownBuilder.AppendLine();
                    markdownBuilder.AppendLine(tag.Comment.Trim());
                    markdownBuilder.AppendLine();
                }
            }
            finally
            {
                //
                // Clean up temp file
                //
                if (tempPath != null && File.Exists(tempPath) == true)
                {
                    try { File.Delete(tempPath); } catch { /* best effort cleanup */ }
                }
            }
        }
        catch
        {
            markdownBuilder.AppendLine("*(Audio metadata could not be extracted)*");
            markdownBuilder.AppendLine();
        }

        ConversionResult result = new ConversionResult(
            Markdown: markdownBuilder.ToString(),
            Title: title);

        return Task.FromResult(result);
    }


    /// <summary>
    /// Format a TimeSpan duration as a human-readable string (e.g., "3:45" or "1:02:30").
    /// </summary>
    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
        {
            return $"{(int)duration.TotalHours}:{duration.Minutes:D2}:{duration.Seconds:D2}";
        }

        return $"{duration.Minutes}:{duration.Seconds:D2}";
    }
}
