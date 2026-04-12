using System.Text;
using Foundation.AI.Vision;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;

namespace Foundation.AI.MarkItDown.Media;

/// <summary>
/// Converts image files to Markdown by extracting EXIF/IPTC metadata
/// and optionally generating an LLM-based visual description via <see cref="IVisionProvider"/>.
///
/// <para><b>Output format:</b>
/// A metadata table listing camera info, dimensions, date taken, GPS coordinates, etc.
/// When a vision provider is configured and enabled, an AI-generated description
/// of the image content is appended.</para>
///
/// <para><b>AI-developed:</b> C# port of Python markitdown's ImageConverter
/// (Pillow EXIF + OpenAI vision in Python, MetadataExtractor + IVisionProvider in C#).</para>
/// </summary>
public sealed class ImageConverter : IDocumentConverter
{
    //
    // Image extensions this converter handles
    //
    private static readonly HashSet<string> ImageExtensionSet = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff", ".tif", ".svg"
    };

    //
    // Optional vision provider for LLM-based image descriptions
    //
    private readonly IVisionProvider? _visionProvider;

    //
    // Whether to use the vision provider for descriptions
    //
    private readonly bool _enableVisionDescriptions;


    /// <inheritdoc />
    public string Name => "Image";

    /// <inheritdoc />
    public int Priority => 0;


    /// <summary>
    /// Create an ImageConverter with optional vision provider for LLM-based descriptions.
    /// </summary>
    /// <param name="visionProvider">Optional vision provider. Pass null to extract metadata only.</param>
    /// <param name="enableVisionDescriptions">Whether to use the vision provider for descriptions.</param>
    public ImageConverter(IVisionProvider? visionProvider = null, bool enableVisionDescriptions = false)
    {
        _visionProvider = visionProvider;
        _enableVisionDescriptions = enableVisionDescriptions;
    }


    /// <inheritdoc />
    public bool Accepts(Stream stream, StreamInfo streamInfo)
    {
        if (streamInfo.Extension != null && ImageExtensionSet.Contains(streamInfo.Extension) == true)
        {
            return true;
        }

        if (streamInfo.MimeType != null &&
            streamInfo.MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true)
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
        StringBuilder markdownBuilder = new StringBuilder();
        string? title = streamInfo.FileName;

        markdownBuilder.AppendLine($"# Image: {streamInfo.FileName ?? "Unknown"}");
        markdownBuilder.AppendLine();

        //
        // Extract metadata using MetadataExtractor
        //
        try
        {
            IReadOnlyList<MetadataExtractor.Directory> metadataDirectoryList =
                ImageMetadataReader.ReadMetadata(stream);

            List<(string Name, string Value)> metadataEntryList = ExtractKeyMetadata(metadataDirectoryList);

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
        }
        catch
        {
            //
            // Metadata extraction failed — continue without it
            //
            markdownBuilder.AppendLine("*(Metadata could not be extracted)*");
            markdownBuilder.AppendLine();
        }

        //
        // Generate LLM-based description if vision provider is available and enabled
        //
        if (_visionProvider != null && _enableVisionDescriptions == true)
        {
            try
            {
                stream.Position = 0;
                byte[] imageBytes = new byte[stream.Length];
                await stream.ReadExactlyAsync(imageBytes, ct);

                VisionResponse visionResponse = await _visionProvider.DescribeImageAsync(
                    imageBytes: new ReadOnlyMemory<byte>(imageBytes),
                    prompt: "Describe this image in detail. Include any visible text, objects, people, scenes, colors, and notable features.",
                    ct: ct);

                if (string.IsNullOrWhiteSpace(visionResponse.Description) == false)
                {
                    markdownBuilder.AppendLine("## AI Description");
                    markdownBuilder.AppendLine();
                    markdownBuilder.AppendLine(visionResponse.Description.Trim());
                    markdownBuilder.AppendLine();
                }
            }
            catch
            {
                //
                // Vision description failed — continue with metadata only
                //
                markdownBuilder.AppendLine("*(AI description could not be generated)*");
                markdownBuilder.AppendLine();
            }
        }

        return new ConversionResult(
            Markdown: markdownBuilder.ToString(),
            Title: title);
    }


    /// <summary>
    /// Extract key metadata properties from the metadata directories.
    /// Returns a curated list of the most useful properties for LLM context.
    /// </summary>
    private static List<(string Name, string Value)> ExtractKeyMetadata(
        IReadOnlyList<MetadataExtractor.Directory> directoryList)
    {
        List<(string Name, string Value)> entryList = new();

        foreach (MetadataExtractor.Directory directory in directoryList)
        {
            //
            // EXIF IFD0 — camera and image basics
            //
            if (directory is ExifIfd0Directory ifd0)
            {
                AddIfPresent(entryList, "Camera Make", ifd0, ExifDirectoryBase.TagMake);
                AddIfPresent(entryList, "Camera Model", ifd0, ExifDirectoryBase.TagModel);
                AddIfPresent(entryList, "Software", ifd0, ExifDirectoryBase.TagSoftware);
                AddIfPresent(entryList, "Image Width", ifd0, ExifDirectoryBase.TagImageWidth);
                AddIfPresent(entryList, "Image Height", ifd0, ExifDirectoryBase.TagImageHeight);
                AddIfPresent(entryList, "Orientation", ifd0, ExifDirectoryBase.TagOrientation);
            }

            //
            // EXIF SubIFD — exposure and capture details
            //
            if (directory is ExifSubIfdDirectory subIfd)
            {
                AddIfPresent(entryList, "Date Taken", subIfd, ExifDirectoryBase.TagDateTimeOriginal);
                AddIfPresent(entryList, "Exposure Time", subIfd, ExifDirectoryBase.TagExposureTime);
                AddIfPresent(entryList, "F-Number", subIfd, ExifDirectoryBase.TagFNumber);
                AddIfPresent(entryList, "ISO Speed", subIfd, ExifDirectoryBase.TagIsoEquivalent);
                AddIfPresent(entryList, "Focal Length", subIfd, ExifDirectoryBase.TagFocalLength);
                AddIfPresent(entryList, "Flash", subIfd, ExifDirectoryBase.TagFlash);
            }

            //
            // GPS directory — location
            //
            if (directory is GpsDirectory gps)
            {
                GeoLocation? location = gps.GetGeoLocation();

                if (location.HasValue == true)
                {
                    entryList.Add(("GPS Latitude", location.Value.Latitude.ToString("F6")));
                    entryList.Add(("GPS Longitude", location.Value.Longitude.ToString("F6")));
                }
            }

            //
            // IPTC — descriptive metadata
            //
            if (directory is IptcDirectory iptc)
            {
                AddIfPresent(entryList, "IPTC Caption", iptc, IptcDirectory.TagCaption);
                AddIfPresent(entryList, "IPTC Keywords", iptc, IptcDirectory.TagKeywords);
                AddIfPresent(entryList, "IPTC Copyright", iptc, IptcDirectory.TagCopyrightNotice);
            }
        }

        return entryList;
    }


    /// <summary>
    /// Add a metadata entry to the list if the tag has a non-empty value.
    /// </summary>
    private static void AddIfPresent(
        List<(string Name, string Value)> entryList,
        string displayName,
        MetadataExtractor.Directory directory,
        int tagType)
    {
        string? value = directory.GetDescription(tagType);

        if (string.IsNullOrWhiteSpace(value) == false)
        {
            entryList.Add((displayName, value.Trim()));
        }
    }
}
