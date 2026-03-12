namespace BMC.LDraw.Models
{
    /// <summary>
    ///
    /// Result of parsing a LEGO Digital Designer .lxf file.
    ///
    /// The .lxf format is a standard ZIP archive containing:
    ///   - IMAGE100.LXFML — XML model data (bricks, positions, colours)
    ///   - IMAGE100.PNG   — preview thumbnail image
    ///
    /// This class holds the extracted content for downstream processing
    /// via the standard ModelImportService pipeline.
    ///
    /// AI-developed code — initial implementation March 2026
    ///
    /// </summary>
    public class LxfResult
    {
        /// <summary>The LDraw content lines converted from LXFML bricks.</summary>
        public string[] LDrawLines { get; set; }

        /// <summary>PNG thumbnail image bytes from IMAGE100.PNG, if present.</summary>
        public byte[] ThumbnailData { get; set; }

        /// <summary>The original filename of the .lxf file.</summary>
        public string OriginalFileName { get; set; }

        /// <summary>LDD application version from the LXFML Application element, if present.</summary>
        public string LddVersion { get; set; }
    }
}
