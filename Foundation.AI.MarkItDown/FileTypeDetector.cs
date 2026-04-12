namespace Foundation.AI.MarkItDown;

/// <summary>
/// Detects file types using extension-to-MIME mapping and magic byte signatures.
///
/// <para><b>Purpose:</b>
/// Populates <see cref="StreamInfo"/> fields when not provided by the caller.
/// Combines file extension lookup with content-based magic byte sniffing
/// to identify common document formats.</para>
///
/// <para><b>AI-developed:</b> C# equivalent of Python markitdown's Magika-based detection,
/// using a simpler built-in approach since no direct .NET Magika port exists.</para>
/// </summary>
internal static class FileTypeDetector
{
    //
    // Extension-to-MIME type mapping for common document formats
    //
    private static readonly Dictionary<string, string> ExtensionToMimeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Documents
        { ".pdf", "application/pdf" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },

        // Text formats
        { ".txt", "text/plain" },
        { ".csv", "text/csv" },
        { ".json", "application/json" },
        { ".xml", "application/xml" },
        { ".html", "text/html" },
        { ".htm", "text/html" },

        // Archives and containers
        { ".zip", "application/zip" },
        { ".epub", "application/epub+zip" },
        { ".ipynb", "application/x-ipynb+json" },

        // Images
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".bmp", "image/bmp" },
        { ".webp", "image/webp" },
        { ".svg", "image/svg+xml" },

        // Audio
        { ".mp3", "audio/mpeg" },
        { ".wav", "audio/wav" },
        { ".m4a", "audio/mp4" },
        { ".flac", "audio/flac" },
        { ".ogg", "audio/ogg" },

        // Other
        { ".md", "text/markdown" },
        { ".rst", "text/x-rst" },
        { ".rtf", "application/rtf" },
        { ".log", "text/plain" },
        { ".ini", "text/plain" },
        { ".cfg", "text/plain" },
        { ".yaml", "text/yaml" },
        { ".yml", "text/yaml" },
        { ".toml", "application/toml" },
    };


    //
    // Magic byte signatures for content-based detection
    //
    private static readonly (byte[] Signature, int Offset, string MimeType)[] MagicSignatures =
    {
        // PDF: starts with %PDF
        (new byte[] { 0x25, 0x50, 0x44, 0x46 }, 0, "application/pdf"),

        // ZIP (and Office Open XML, EPUB): starts with PK\x03\x04
        (new byte[] { 0x50, 0x4B, 0x03, 0x04 }, 0, "application/zip"),

        // PNG: starts with \x89PNG\r\n\x1A\n
        (new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0, "image/png"),

        // JPEG: starts with \xFF\xD8\xFF
        (new byte[] { 0xFF, 0xD8, 0xFF }, 0, "image/jpeg"),

        // GIF: starts with GIF87a or GIF89a
        (new byte[] { 0x47, 0x49, 0x46, 0x38 }, 0, "image/gif"),

        // BMP: starts with BM
        (new byte[] { 0x42, 0x4D }, 0, "image/bmp"),

        // RIFF (WAV, WebP): starts with RIFF
        (new byte[] { 0x52, 0x49, 0x46, 0x46 }, 0, "application/octet-stream"),

        // ID3 (MP3 with ID3 tag)
        (new byte[] { 0x49, 0x44, 0x33 }, 0, "audio/mpeg"),

        // FLAC: starts with fLaC
        (new byte[] { 0x66, 0x4C, 0x61, 0x43 }, 0, "audio/flac"),

        // OGG: starts with OggS
        (new byte[] { 0x4F, 0x67, 0x67, 0x53 }, 0, "audio/ogg"),
    };


    /// <summary>
    /// Resolve MIME type from a file extension.
    /// </summary>
    /// <param name="extension">File extension including the dot (e.g., ".pdf").</param>
    /// <returns>MIME type string, or null if the extension is not recognized.</returns>
    public static string? GetMimeTypeFromExtension(string? extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return null;
        }

        ExtensionToMimeMap.TryGetValue(extension, out string? mimeType);
        return mimeType;
    }


    /// <summary>
    /// Detect MIME type from file content using magic byte signatures.
    /// Does not modify the stream position.
    /// </summary>
    /// <param name="stream">Seekable stream to inspect.</param>
    /// <returns>MIME type string, or null if no signature matched.</returns>
    public static string? DetectFromContent(Stream stream)
    {
        if (stream == null || stream.Length == 0 || !stream.CanSeek)
        {
            return null;
        }

        long originalPosition = stream.Position;

        try
        {
            //
            // Read enough bytes to check all signatures (max 8 bytes needed)
            //
            byte[] headerBuffer = new byte[16];
            stream.Position = 0;
            int bytesRead = stream.Read(headerBuffer, 0, headerBuffer.Length);

            if (bytesRead == 0)
            {
                return null;
            }

            //
            // Check each magic signature against the header bytes
            //
            foreach ((byte[] signature, int offset, string mimeType) in MagicSignatures)
            {
                if (offset + signature.Length > bytesRead)
                {
                    continue;
                }

                bool matched = true;

                for (int i = 0; i < signature.Length; i++)
                {
                    if (headerBuffer[offset + i] != signature[i])
                    {
                        matched = false;
                        break;
                    }
                }

                if (matched == true)
                {
                    //
                    // Special case: ZIP signature could be DOCX, PPTX, XLSX, or EPUB
                    // We return generic ZIP here; the extension-based detection
                    // should take precedence for specific Office/EPUB types.
                    //
                    return mimeType;
                }
            }

            return null;
        }
        finally
        {
            //
            // Restore original stream position
            //
            stream.Position = originalPosition;
        }
    }


    /// <summary>
    /// Build a complete <see cref="StreamInfo"/> from a file path, merging with any existing hints.
    /// </summary>
    /// <param name="filePath">Path to the file.</param>
    /// <param name="existingInfo">Optional existing StreamInfo with caller-provided hints.</param>
    /// <returns>StreamInfo with extension, filename, MIME type, and local path populated.</returns>
    public static StreamInfo BuildFromFilePath(string filePath, StreamInfo? existingInfo = null)
    {
        string extension = existingInfo?.Extension ?? Path.GetExtension(filePath);
        string fileName = existingInfo?.FileName ?? Path.GetFileName(filePath);
        string? mimeType = existingInfo?.MimeType ?? GetMimeTypeFromExtension(extension);

        return new StreamInfo(
            MimeType: mimeType,
            Extension: extension,
            Charset: existingInfo?.Charset,
            FileName: fileName,
            LocalPath: Path.GetFullPath(filePath),
            Url: existingInfo?.Url);
    }


    /// <summary>
    /// Enrich a <see cref="StreamInfo"/> by detecting MIME type from stream content
    /// when the MIME type is not already known.
    /// </summary>
    /// <param name="stream">Seekable stream to inspect.</param>
    /// <param name="info">Existing StreamInfo to enrich.</param>
    /// <returns>Enriched StreamInfo with MIME type populated from content detection.</returns>
    public static StreamInfo EnrichFromContent(Stream stream, StreamInfo info)
    {
        if (info.MimeType != null)
        {
            return info;
        }

        string? detectedMimeType = DetectFromContent(stream);

        if (detectedMimeType == null)
        {
            return info;
        }

        return info with { MimeType = detectedMimeType };
    }
}
