namespace Foundation.Imaging
{
    /// <summary>
    /// Options for document generation via IDocumentBuilder.
    /// </summary>
    public class DocumentOptions
    {
        /// <summary>Page size: "a4" or "letter".</summary>
        public string PageSize { get; set; } = "a4";

        /// <summary>Image width/height in pixels for content images.</summary>
        public int ImageSize { get; set; } = 512;

        /// <summary>Output format: "html" (default) or "pdf".</summary>
        public string OutputFormat { get; set; } = "html";

        /// <summary>Visual theme for the document. Null uses the default dark navy theme.</summary>
        public DocumentTheme Theme { get; set; }
    }


    /// <summary>
    /// Colour palette for document generation.
    /// Applications set this once on DocumentOptions to brand their reports.
    /// All colours are hex strings (e.g. "#1A1A2E").
    /// </summary>
    public class DocumentTheme
    {
        /// <summary>Cover/closing page gradient start colour (top/left).</summary>
        public string GradientStartHex { get; set; } = "#1A1A2E";

        /// <summary>Cover/closing page gradient end colour (bottom/right).</summary>
        public string GradientEndHex { get; set; } = "#16213E";

        /// <summary>Accent colour for headings, quantity badges, highlights.</summary>
        public string AccentHex { get; set; } = "#E53E3E";

        /// <summary>Primary body text colour.</summary>
        public string BodyTextHex { get; set; } = "#1A1A2E";

        /// <summary>Muted/secondary text colour.</summary>
        public string MutedTextHex { get; set; } = "#94A3B8";

        /// <summary>Content page background colour.</summary>
        public string PageBackgroundHex { get; set; } = "#FFFFFF";

        /// <summary>Section callout background colour.</summary>
        public string SectionBackgroundHex { get; set; } = "#EBF8FF";

        /// <summary>Section callout border colour.</summary>
        public string SectionBorderHex { get; set; } = "#3182CE";

        /// <summary>Cover/closing page text colour.</summary>
        public string CoverTextHex { get; set; } = "#FFFFFF";

        /// <summary>Separator line colour.</summary>
        public string SeparatorHex { get; set; } = "#E2E8F0";

        /// <summary>Item callout background colour.</summary>
        public string CalloutBackgroundHex { get; set; } = "#F8FAFC";

        /// <summary>Closing page gradient start colour (defaults to green).</summary>
        public string ClosingGradientStartHex { get; set; } = "#0F766E";

        /// <summary>Closing page gradient end colour.</summary>
        public string ClosingGradientEndHex { get; set; } = "#064E3B";
    }


    /// <summary>
    /// A single content item displayed on a content or summary page.
    /// Generic enough for parts lists, line items, schedule entries, etc.
    /// </summary>
    public class ContentItem
    {
        /// <summary>Unique key for thumbnail lookup (e.g. "partFile|colourCode").</summary>
        public string Key { get; set; }

        /// <summary>Display label (e.g. part description, item name).</summary>
        public string Label { get; set; }

        /// <summary>Secondary label (e.g. colour name, category).</summary>
        public string Subtitle { get; set; }

        /// <summary>Quantity or count.</summary>
        public int Quantity { get; set; }

        /// <summary>Optional hex colour for swatch rendering (e.g. "#C91A09").</summary>
        public string ColourHex { get; set; }
    }


    /// <summary>
    /// Result of generating a document via IDocumentBuilder.Build().
    /// </summary>
    public class DocumentResult
    {
        /// <summary>The complete self-contained HTML document (null for PDF output).</summary>
        public string Html { get; set; }

        /// <summary>Raw document bytes (used for PDF output; null for HTML).</summary>
        public byte[] DocumentBytes { get; set; }

        /// <summary>MIME type of the output (e.g. "text/html" or "application/pdf").</summary>
        public string ContentType { get; set; }

        /// <summary>Total time spent generating in milliseconds.</summary>
        public int TotalGenerationTimeMs { get; set; }
    }
}
