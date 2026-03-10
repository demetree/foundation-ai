using System.Collections.Generic;

namespace BMC.LDraw.Models
{
    /// <summary>
    ///
    /// Result of parsing a BrickLink Studio .io file.
    ///
    /// The .io format is a password-protected ZIP archive containing:
    ///   - model.ldr — flat LDraw model
    ///   - model2.ldr — full MPD variant with embedded submodels (richest data)
    ///   - modelv1.ldr — step-by-step variant with extended line format
    ///   - thumbnail.png — preview image
    ///   - .info — JSON metadata (Studio version, part count)
    ///   - model.ins — instruction configuration (XML, optional)
    ///   - errorPartList.err — list of problematic/missing parts (optional)
    ///   - CustomParts/ — embedded LDraw geometry for non-standard parts
    ///
    /// This class holds the complete extracted content for downstream processing
    /// and round-trip export.
    ///
    /// </summary>
    public class StudioIoResult
    {
        //
        // Core LDraw content
        //

        /// <summary>The raw LDraw content lines extracted from model.ldr inside the ZIP.</summary>
        public string[] LDrawLines { get; set; }

        /// <summary>Full text content of model2.ldr (MPD variant with embedded submodels), if present.</summary>
        public string Model2LdrContent { get; set; }

        /// <summary>Full text content of modelv1.ldr (step-by-step variant), if present.</summary>
        public string ModelV1LdrContent { get; set; }


        //
        // Metadata
        //

        /// <summary>PNG thumbnail image bytes from thumbnail.png, if present.</summary>
        public byte[] ThumbnailData { get; set; }

        /// <summary>BrickLink Studio version string from the .info file, if present.</summary>
        public string StudioVersion { get; set; }

        /// <summary>Part count reported in the .info file, if present.</summary>
        public int? ReportedPartCount { get; set; }

        /// <summary>The original filename of the .io file.</summary>
        public string OriginalFileName { get; set; }

        /// <summary>The raw XML content of model.ins (instruction settings), if present.</summary>
        public string InstructionSettingsXml { get; set; }

        /// <summary>Content of errorPartList.err listing problematic or missing parts, if present.</summary>
        public string ErrorPartList { get; set; }


        //
        // Custom Parts
        //

        /// <summary>
        /// Custom LDraw geometry files extracted from the CustomParts/ directory.
        ///
        /// Keys are relative paths within the CustomParts/ directory, e.g.:
        ///   "88323.dat"           — custom part geometry
        ///   "s/57518s01.dat"      — subpart geometry
        ///   "p/rect.dat"          — standard-res primitive
        ///   "p/48/1-4cyli.dat"    — hi-res primitive
        ///
        /// Values are the raw byte content of each .dat file.
        /// </summary>
        public Dictionary<string, byte[]> CustomParts { get; set; } = new Dictionary<string, byte[]>();


        //
        // Archive-level metadata
        //

        /// <summary>Whether the archive required a password to open.</summary>
        public bool WasPasswordProtected { get; set; }

        /// <summary>The password that was used to decrypt the archive, if any.</summary>
        public string UsedPassword { get; set; }

        /// <summary>
        /// All raw ZIP entry contents keyed by entry name.
        /// This preserves the complete archive for round-trip export,
        /// including any entries not explicitly parsed above.
        /// </summary>
        public Dictionary<string, byte[]> AllEntries { get; set; } = new Dictionary<string, byte[]>();
    }
}
