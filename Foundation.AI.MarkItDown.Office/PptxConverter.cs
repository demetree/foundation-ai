using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;

namespace Foundation.AI.MarkItDown.Office;

/// <summary>
/// Converts PPTX (PowerPoint) files to Markdown using DocumentFormat.OpenXml.
///
/// <para><b>Output format:</b>
/// Each slide becomes a section with a heading containing the slide number and title.
/// Slide body text is extracted from all text frames. Speaker notes are included
/// in a blockquote beneath each slide's content.</para>
///
/// <para><b>AI-developed:</b> C# port of Python markitdown's PptxConverter
/// (python-pptx in Python, OpenXml SDK in C#).</para>
/// </summary>
public sealed class PptxConverter : IDocumentConverter
{
    /// <inheritdoc />
    public string Name => "PPTX";

    /// <inheritdoc />
    public int Priority => 0;


    /// <inheritdoc />
    public bool Accepts(Stream stream, StreamInfo streamInfo)
    {
        if (string.Equals(streamInfo.Extension, ".pptx", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        if (string.Equals(streamInfo.MimeType,
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            StringComparison.OrdinalIgnoreCase) == true)
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

        using PresentationDocument presentationDocument = PresentationDocument.Open(stream, isEditable: false);
        PresentationPart? presentationPart = presentationDocument.PresentationPart;

        if (presentationPart == null)
        {
            return Task.FromResult(new ConversionResult(Markdown: string.Empty));
        }

        //
        // Extract document title from core properties
        //
        string? coreTitle = presentationDocument.PackageProperties?.Title;

        if (string.IsNullOrWhiteSpace(coreTitle) == false)
        {
            documentTitle = coreTitle.Trim();
            markdownBuilder.AppendLine($"# {documentTitle}");
            markdownBuilder.AppendLine();
        }

        //
        // Get the slide ID list from the presentation
        //
        SlideIdList? slideIdList = presentationPart.Presentation?.SlideIdList;

        if (slideIdList == null)
        {
            return Task.FromResult(new ConversionResult(Markdown: markdownBuilder.ToString(), Title: documentTitle));
        }

        int slideNumber = 0;

        foreach (SlideId slideId in slideIdList.Elements<SlideId>())
        {
            ct.ThrowIfCancellationRequested();
            slideNumber++;

            //
            // Get the slide part via the relationship ID
            //
            string? relationshipId = slideId.RelationshipId?.Value;

            if (string.IsNullOrEmpty(relationshipId) == true)
            {
                continue;
            }

            SlidePart slidePart = (SlidePart)presentationPart.GetPartById(relationshipId);

            //
            // Extract slide title and body text
            //
            string slideTitle = ExtractSlideTitle(slidePart);
            string slideBody = ExtractSlideText(slidePart);
            string slideNotes = ExtractSlideNotes(slidePart);

            //
            // Build slide section
            //
            if (string.IsNullOrWhiteSpace(slideTitle) == false)
            {
                markdownBuilder.AppendLine($"## Slide {slideNumber}: {slideTitle}");
            }
            else
            {
                markdownBuilder.AppendLine($"## Slide {slideNumber}");
            }

            markdownBuilder.AppendLine();

            if (string.IsNullOrWhiteSpace(slideBody) == false)
            {
                markdownBuilder.AppendLine(slideBody.Trim());
                markdownBuilder.AppendLine();
            }

            //
            // Include speaker notes as a blockquote
            //
            if (string.IsNullOrWhiteSpace(slideNotes) == false)
            {
                markdownBuilder.AppendLine("**Speaker Notes:**");

                foreach (string noteLine in slideNotes.Trim().Split('\n'))
                {
                    markdownBuilder.AppendLine($"> {noteLine}");
                }

                markdownBuilder.AppendLine();
            }
        }

        ConversionResult result = new ConversionResult(
            Markdown: markdownBuilder.ToString(),
            Title: documentTitle);

        return Task.FromResult(result);
    }


    /// <summary>
    /// Extract the slide title from title placeholder shapes.
    /// </summary>
    private static string ExtractSlideTitle(SlidePart slidePart)
    {
        CommonSlideData? slideData = slidePart.Slide?.CommonSlideData;

        if (slideData == null)
        {
            return string.Empty;
        }

        //
        // Look for shapes that are title placeholders
        //
        foreach (Shape shape in slideData.ShapeTree?.Elements<Shape>() ?? Enumerable.Empty<Shape>())
        {
            PlaceholderShape? placeholder = shape.NonVisualShapeProperties?
                .ApplicationNonVisualDrawingProperties?
                .GetFirstChild<PlaceholderShape>();

            if (placeholder != null)
            {
                //
                // Check if this is a title or centered title placeholder
                //
                PlaceholderValues? placeholderType = placeholder.Type?.Value;

                if (placeholderType == PlaceholderValues.Title ||
                    placeholderType == PlaceholderValues.CenteredTitle)
                {
                    string titleText = shape.TextBody?.InnerText?.Trim() ?? "";

                    if (string.IsNullOrEmpty(titleText) == false)
                    {
                        return titleText;
                    }
                }
            }
        }

        return string.Empty;
    }


    /// <summary>
    /// Extract all text content from a slide (excluding the title).
    /// </summary>
    private static string ExtractSlideText(SlidePart slidePart)
    {
        CommonSlideData? slideData = slidePart.Slide?.CommonSlideData;

        if (slideData == null)
        {
            return string.Empty;
        }

        StringBuilder textBuilder = new StringBuilder();

        foreach (Shape shape in slideData.ShapeTree?.Elements<Shape>() ?? Enumerable.Empty<Shape>())
        {
            //
            // Skip title placeholders (already extracted separately)
            //
            PlaceholderShape? placeholder = shape.NonVisualShapeProperties?
                .ApplicationNonVisualDrawingProperties?
                .GetFirstChild<PlaceholderShape>();

            if (placeholder != null)
            {
                PlaceholderValues? placeholderType = placeholder.Type?.Value;

                if (placeholderType == PlaceholderValues.Title ||
                    placeholderType == PlaceholderValues.CenteredTitle)
                {
                    continue;
                }
            }

            //
            // Extract text from this shape's text body
            //
            if (shape.TextBody != null)
            {
                foreach (DocumentFormat.OpenXml.Drawing.Paragraph paragraph in
                    shape.TextBody.Elements<DocumentFormat.OpenXml.Drawing.Paragraph>())
                {
                    string paragraphText = paragraph.InnerText?.Trim() ?? "";

                    if (string.IsNullOrEmpty(paragraphText) == false)
                    {
                        textBuilder.AppendLine(paragraphText);
                    }
                }
            }
        }

        return textBuilder.ToString();
    }


    /// <summary>
    /// Extract speaker notes from a slide's notes part.
    /// </summary>
    private static string ExtractSlideNotes(SlidePart slidePart)
    {
        NotesSlidePart? notesPart = slidePart.NotesSlidePart;

        if (notesPart == null)
        {
            return string.Empty;
        }

        StringBuilder notesBuilder = new StringBuilder();

        //
        // Extract text from the notes slide's common slide data
        //
        CommonSlideData? notesData = notesPart.NotesSlide?.CommonSlideData;

        if (notesData == null)
        {
            return string.Empty;
        }

        foreach (Shape shape in notesData.ShapeTree?.Elements<Shape>() ?? Enumerable.Empty<Shape>())
        {
            //
            // Look for the body placeholder in notes (type = Body)
            //
            PlaceholderShape? placeholder = shape.NonVisualShapeProperties?
                .ApplicationNonVisualDrawingProperties?
                .GetFirstChild<PlaceholderShape>();

            if (placeholder?.Type?.Value == PlaceholderValues.Body)
            {
                string noteText = shape.TextBody?.InnerText?.Trim() ?? "";

                if (string.IsNullOrEmpty(noteText) == false)
                {
                    notesBuilder.AppendLine(noteText);
                }
            }
        }

        return notesBuilder.ToString();
    }
}
