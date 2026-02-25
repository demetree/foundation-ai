using System.Collections.Generic;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Analysis of a single build step — what parts are new and their spatial extent.
    /// </summary>
    public class ManualStepAnalysis
    {
        /// <summary>Step index (0-based).</summary>
        public int StepIndex;

        /// <summary>Parts newly added in this step (grouped by fileName + colourCode).</summary>
        public List<StepPartInfo> NewParts = new List<StepPartInfo>();

        /// <summary>Bounding box of the NEW geometry only (for camera focus).</summary>
        public float NewMinX, NewMinY, NewMinZ;
        public float NewMaxX, NewMaxY, NewMaxZ;

        /// <summary>Cumulative part count up to and including this step.</summary>
        public int CumulativePartCount;

        /// <summary>Number of triangles in the cumulative mesh at this step.</summary>
        public int CumulativeTriangleCount;
    }


    /// <summary>
    /// A single part type + colour added in a step, with quantity.
    /// </summary>
    public class StepPartInfo
    {
        /// <summary>LDraw sub-file name (e.g. "3001.dat").</summary>
        public string FileName;

        /// <summary>LDraw colour code.</summary>
        public int ColourCode;

        /// <summary>How many of this exact part + colour were added in the step.</summary>
        public int Quantity;

        /// <summary>Human-readable part name (e.g. "Brick 2 x 4"). Null if not resolved.</summary>
        public string PartDescription;

        /// <summary>Human-readable colour name (e.g. "Red"). Null if not resolved.</summary>
        public string ColourName;

        /// <summary>Hex RGB value for colour swatch (e.g. "#C91A09"). Null if not resolved.</summary>
        public string ColourHex;
    }


    /// <summary>
    /// Result of generating a full build manual.
    /// </summary>
    public class ManualGenerationResult
    {
        /// <summary>The complete self-contained HTML document (null for PDF output).</summary>
        public string Html;

        /// <summary>Raw document bytes (used for PDF output; null for HTML).</summary>
        public byte[] DocumentBytes;

        /// <summary>Total number of build steps.</summary>
        public int TotalSteps;

        /// <summary>Total unique part placements across all steps.</summary>
        public int TotalParts;

        /// <summary>Total time spent rendering in milliseconds.</summary>
        public int TotalRenderTimeMs;
    }


    /// <summary>
    /// Options for manual generation, passed from the client.
    /// </summary>
    public class ManualOptions
    {
        /// <summary>Page size: "a4" or "letter".</summary>
        public string PageSize { get; set; } = "a4";

        /// <summary>Image width/height in pixels for step images.</summary>
        public int ImageSize { get; set; } = 512;

        /// <summary>Camera elevation in degrees.</summary>
        public float Elevation { get; set; } = 30f;

        /// <summary>Camera azimuth in degrees.</summary>
        public float Azimuth { get; set; } = -45f;

        /// <summary>Whether to render edge lines.</summary>
        public bool RenderEdges { get; set; } = true;

        /// <summary>Whether to use smooth shading.</summary>
        public bool SmoothShading { get; set; } = true;

        /// <summary>Output format: "html" (default) or "pdf".</summary>
        public string OutputFormat { get; set; } = "html";
    }



    /// <summary>
    /// Result of rendering a single step — includes mesh counts for caching
    /// between iterations to avoid redundant geometry resolution.
    /// </summary>
    public struct StepRenderResult
    {
        /// <summary>Rendered PNG image bytes.</summary>
        public byte[] PngBytes;

        /// <summary>Number of triangles in this step's cumulative mesh (before dimming).</summary>
        public int TriangleCount;

        /// <summary>Number of edge lines in this step's cumulative mesh.</summary>
        public int EdgeCount;
    }
}
