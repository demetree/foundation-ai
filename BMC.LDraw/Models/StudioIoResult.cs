namespace BMC.LDraw.Models
{
    /// <summary>
    ///
    /// Result of parsing a BrickLink Studio .io file.
    ///
    /// The .io format is a password-protected ZIP archive containing:
    ///   - model.ldr (or model2.ldr) — the LDraw model data
    ///   - THUMBNAIL.PNG — a preview image of the model
    ///   - .INFO — metadata (Studio version, part count)
    ///   - model.ins — instruction configuration (XML)
    ///
    /// This class holds the extracted content for downstream processing.
    ///
    /// </summary>
    public class StudioIoResult
    {
        /// <summary>The raw LDraw content lines extracted from model.ldr inside the ZIP.</summary>
        public string[] LDrawLines { get; set; }

        /// <summary>PNG thumbnail image bytes from THUMBNAIL.PNG, if present.</summary>
        public byte[] ThumbnailData { get; set; }

        /// <summary>BrickLink Studio version string from the .INFO file, if present.</summary>
        public string StudioVersion { get; set; }

        /// <summary>Part count reported in the .INFO file, if present.</summary>
        public int? ReportedPartCount { get; set; }

        /// <summary>The original filename of the .io file.</summary>
        public string OriginalFileName { get; set; }

        /// <summary>The raw XML content of model.ins (instruction settings), if present.</summary>
        public string InstructionSettingsXml { get; set; }
    }
}
