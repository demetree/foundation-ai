using Microsoft.Extensions.DependencyInjection;

namespace Foundation.AI.MarkItDown.Pdf;

/// <summary>
/// Extension methods for registering the PDF converter with MarkItDown.
///
/// <para><b>Usage:</b>
/// <code>
/// services.AddMarkItDown();
/// services.AddMarkItDownPdf();
/// </code></para>
/// </summary>
public static class PdfMarkItDownExtensions
{
    /// <summary>
    /// Register the PDF document converter.
    /// Requires <see cref="IMarkItDown"/> to be registered via <c>AddMarkItDown()</c>.
    /// </summary>
    public static IServiceCollection AddMarkItDownPdf(this IServiceCollection services)
    {
        services.AddSingleton<IDocumentConverter, PdfConverter>();
        return services;
    }
}
