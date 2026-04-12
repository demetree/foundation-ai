using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Foundation.AI.MarkItDown.Pdf;

/// <summary>
/// Converts PDF files to Markdown using PdfPig for text extraction.
///
/// <para><b>Purpose:</b>
/// Extracts text content from PDF files page-by-page, preserving reading order.
/// Each page is separated by a horizontal rule and page header. Document metadata
/// (title, author) is extracted from PDF properties when available.</para>
///
/// <para><b>Library:</b> UglyToad.PdfPig — MIT licensed, pure C#, no native dependencies.
/// Provides text extraction with layout awareness and content stream reading.</para>
///
/// <para><b>AI-developed:</b> C# port of Python markitdown's PdfConverter
/// (pdfminer/pdfplumber in Python, PdfPig in C#).</para>
/// </summary>
public sealed class PdfConverter : IDocumentConverter
{
    /// <inheritdoc />
    public string Name => "PDF";

    /// <inheritdoc />
    public int Priority => 0;


    /// <inheritdoc />
    public bool Accepts(Stream stream, StreamInfo streamInfo)
    {
        if (string.Equals(streamInfo.Extension, ".pdf", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        if (string.Equals(streamInfo.MimeType, "application/pdf", StringComparison.OrdinalIgnoreCase) == true)
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
        string? documentTitle = null;

        //
        // Open the PDF document from the stream
        //
        using PdfDocument pdfDocument = PdfDocument.Open(stream);

        //
        // Extract document title from PDF metadata if available
        //
        if (string.IsNullOrWhiteSpace(pdfDocument.Information.Title) == false)
        {
            documentTitle = pdfDocument.Information.Title.Trim();
            markdownBuilder.AppendLine($"# {documentTitle}");
            markdownBuilder.AppendLine();
        }

        //
        // Extract metadata block if any properties are populated
        //
        bool hasMetadata = false;
        StringBuilder metadataBuilder = new StringBuilder();

        if (string.IsNullOrWhiteSpace(pdfDocument.Information.Author) == false)
        {
            metadataBuilder.AppendLine($"- **Author:** {pdfDocument.Information.Author.Trim()}");
            hasMetadata = true;
        }

        if (string.IsNullOrWhiteSpace(pdfDocument.Information.Subject) == false)
        {
            metadataBuilder.AppendLine($"- **Subject:** {pdfDocument.Information.Subject.Trim()}");
            hasMetadata = true;
        }

        if (string.IsNullOrWhiteSpace(pdfDocument.Information.Creator) == false)
        {
            metadataBuilder.AppendLine($"- **Creator:** {pdfDocument.Information.Creator.Trim()}");
            hasMetadata = true;
        }

        if (hasMetadata == true)
        {
            markdownBuilder.AppendLine(metadataBuilder.ToString());
        }

        //
        // Extract text from each page
        //
        int pageCount = pdfDocument.NumberOfPages;

        for (int pageNumber = 1; pageNumber <= pageCount; pageNumber++)
        {
            ct.ThrowIfCancellationRequested();

            Page page = pdfDocument.GetPage(pageNumber);

            //
            // Use the content order text extractor for natural reading order
            //
            string pageText = ContentOrderTextExtractor.GetText(page);

            if (string.IsNullOrWhiteSpace(pageText) == true)
            {
                continue;
            }

            //
            // Add page separator and header for multi-page documents
            //
            if (pageCount > 1)
            {
                markdownBuilder.AppendLine("---");
                markdownBuilder.AppendLine($"### Page {pageNumber}");
                markdownBuilder.AppendLine();
            }

            //
            // Append the extracted page text
            //
            markdownBuilder.AppendLine(pageText.Trim());
            markdownBuilder.AppendLine();
        }

        ConversionResult result = new ConversionResult(
            Markdown: markdownBuilder.ToString(),
            Title: documentTitle);

        return Task.FromResult(result);
    }
}
