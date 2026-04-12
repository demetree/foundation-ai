using Microsoft.Extensions.DependencyInjection;

namespace Foundation.AI.MarkItDown.Office;

/// <summary>
/// Extension methods for registering Office document converters with MarkItDown.
///
/// <para><b>Usage:</b>
/// <code>
/// services.AddMarkItDown();
/// services.AddMarkItDownOffice();  // adds DOCX, PPTX, XLSX converters
/// </code></para>
/// </summary>
public static class OfficeMarkItDownExtensions
{
    /// <summary>
    /// Register the Office document converters (DOCX, PPTX, XLSX).
    /// Requires <see cref="IMarkItDown"/> to be registered via <c>AddMarkItDown()</c>.
    /// </summary>
    public static IServiceCollection AddMarkItDownOffice(this IServiceCollection services)
    {
        services.AddSingleton<IDocumentConverter, DocxConverter>();
        services.AddSingleton<IDocumentConverter, PptxConverter>();
        services.AddSingleton<IDocumentConverter, XlsxConverter>();
        return services;
    }
}
