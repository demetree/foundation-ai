using System.Collections.Generic;

namespace BMC.LDraw.Models
{
    /// <summary>
    ///
    /// Input DTO for StudioIoWriter — contains all the data needed to
    /// assemble a valid BrickLink Studio .io archive.
    ///
    /// At minimum, LDrawContent must be provided. All other fields are optional
    /// and will be included in the archive if provided.
    ///
    /// </summary>
    public class StudioIoWriteRequest
    {
        //
        // Required — the LDraw model content
        //

        /// <summary>
        /// The LDraw model content to be stored as model.ldr.
        /// This is the primary model data and is required.
        /// </summary>
        public string LDrawContent { get; set; }


        //
        // Optional — secondary LDraw variants
        //

        /// <summary>
        /// Alternate LDraw content with embedded part definitions (stored as model2.ldr).
        /// If null, model2.ldr will not be included in the archive.
        /// </summary>
        public string Model2LdrContent { get; set; }

        /// <summary>
        /// Step-by-step LDraw variant (stored as modelv1.ldr).
        /// If null, modelv1.ldr will not be included in the archive.
        /// </summary>
        public string ModelV1LdrContent { get; set; }


        //
        // Optional — metadata and supporting files
        //

        /// <summary>PNG thumbnail image bytes. If null, THUMBNAIL.PNG will not be included.</summary>
        public byte[] ThumbnailData { get; set; }

        /// <summary>Instruction settings XML content. If null, model.ins will not be included.</summary>
        public string InstructionSettingsXml { get; set; }

        /// <summary>Error part list content. If null, errorPartList.err will not be included.</summary>
        public string ErrorPartList { get; set; }

        /// <summary>
        /// BrickLink Studio version string to write in the .info file.
        /// If null, defaults to "2.1.6_4".
        /// </summary>
        public string StudioVersion { get; set; }

        /// <summary>
        /// Total part count to report in the .info file.
        /// If null, part count will be omitted from .info.
        /// </summary>
        public int? PartCount { get; set; }

        /// <summary>
        /// Whether to password-protect the resulting archive using the known Studio password.
        /// Default is true to maintain compatibility with BrickLink Studio.
        /// </summary>
        public bool UsePassword { get; set; } = true;

        /// <summary>
        /// Custom LDraw geometry files to include in the CustomParts/ directory.
        /// Keys are relative paths (e.g. "88323.dat", "p/rect.dat", "p/48/1-4cyli.dat").
        /// Values are the raw byte content of each .dat file.
        /// </summary>
        public Dictionary<string, byte[]> CustomParts { get; set; }

        /// <summary>
        /// Additional entries to include in the archive, keyed by entry name.
        /// Useful for round-tripping entries that the parser doesn't explicitly handle.
        /// Entries will NOT override the standard entries above (model.ldr, thumbnail.png, etc.)
        /// </summary>
        public Dictionary<string, byte[]> AdditionalEntries { get; set; }
    }
}
