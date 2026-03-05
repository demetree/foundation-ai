using System.Collections.Generic;

namespace Foundation.Imaging
{
    /// <summary>
    /// Generic document builder interface for producing multi-page
    /// reports in various output formats (PDF, HTML, etc.).
    ///
    /// Implementations handle format-specific rendering; callers
    /// add pages and content through a uniform API.
    ///
    /// BMC's IManualDocumentBuilder follows this same pattern but adds
    /// LEGO-specific semantics (submodel callouts, PLI thumbnails, etc.).
    /// </summary>
    public interface IDocumentBuilder
    {
        /// <summary>
        /// Start a new document with a title and options.
        /// </summary>
        /// <param name="title">Display title for the document.</param>
        /// <param name="options">Document generation options.</param>
        void BeginDocument(string title, DocumentOptions options);


        /// <summary>
        /// Add a cover page with a title image.
        /// </summary>
        /// <param name="titleImage">PNG bytes of the cover image.</param>
        void AddCoverPage(byte[] titleImage);


        /// <summary>
        /// Begin a named section (e.g. chapter, callout, grouped content).
        /// All subsequent AddContentPage calls are grouped in this section
        /// until EndSection is called.
        /// </summary>
        /// <param name="sectionName">Display name of the section.</param>
        void BeginSection(string sectionName);


        /// <summary>
        /// Add a content page with an image and accompanying detail items.
        /// </summary>
        /// <param name="heading">Page heading text.</param>
        /// <param name="contentImage">PNG bytes of the main page image.</param>
        /// <param name="items">Content items (line items, parts, details) for this page.</param>
        /// <param name="thumbnails">
        /// Optional thumbnail cache: key is a unique item identifier, value is PNG bytes.
        /// May be null if thumbnails are not applicable.
        /// </param>
        void AddContentPage(string heading, byte[] contentImage,
            List<ContentItem> items, Dictionary<string, byte[]> thumbnails);


        /// <summary>
        /// End the current section.
        /// </summary>
        void EndSection();


        /// <summary>
        /// Add a summary page listing all items with optional thumbnails.
        /// </summary>
        /// <param name="heading">Summary page heading.</param>
        /// <param name="items">Aggregated list of items.</param>
        /// <param name="thumbnails">
        /// Optional thumbnail cache: key is a unique item identifier, value is PNG bytes.
        /// May be null if thumbnails are not applicable.
        /// </param>
        void AddSummaryPage(string heading, List<ContentItem> items,
            Dictionary<string, byte[]> thumbnails);


        /// <summary>
        /// Add a completion / closing page with a full-page image.
        /// </summary>
        /// <param name="closingImage">PNG bytes of the closing image.</param>
        void AddClosingPage(byte[] closingImage);


        /// <summary>
        /// Finalize and return the assembled document.
        /// </summary>
        DocumentResult Build();
    }
}
