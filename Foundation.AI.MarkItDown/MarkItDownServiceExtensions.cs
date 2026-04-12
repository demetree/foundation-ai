using Foundation.AI.MarkItDown.Converters;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.AI.MarkItDown;

/// <summary>
/// Extension methods for registering Foundation.AI.MarkItDown services.
///
/// <para><b>Usage:</b>
/// <code>
/// services.AddMarkItDown();  // registers core + built-in converters
///
/// // Or with configuration:
/// services.AddMarkItDown(config => {
///     config.MaxFileSizeBytes = 200 * 1024 * 1024;  // 200 MB
/// });
/// </code></para>
/// </summary>
public static class MarkItDownServiceExtensions
{
    /// <summary>
    /// Register the MarkItDown conversion service with built-in converters.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMarkItDown(
        this IServiceCollection services,
        Action<MarkItDownConfig>? configure = null)
    {
        MarkItDownConfig config = new MarkItDownConfig();
        configure?.Invoke(config);

        services.AddSingleton(config);
        services.AddSingleton<IMarkItDown, MarkItDownService>();

        //
        // Register built-in converters if enabled
        //
        if (config.EnableBuiltInConverters == true)
        {
            //
            // Format-specific converters (priority 0)
            //
            services.AddSingleton<IDocumentConverter, PlainTextConverter>();
            services.AddSingleton<IDocumentConverter, CsvConverter>();
            services.AddSingleton<IDocumentConverter, JsonConverter>();
            services.AddSingleton<IDocumentConverter, XmlConverter>();
            services.AddSingleton<IDocumentConverter, HtmlConverter>();
            services.AddSingleton<IDocumentConverter, ZipConverter>();
            services.AddSingleton<IDocumentConverter, EpubConverter>();
            services.AddSingleton<IDocumentConverter, JupyterNotebookConverter>();

            //
            // Generic fallback converter (priority 100)
            //
            services.AddSingleton<IDocumentConverter, PlainTextFallbackConverter>();
        }

        return services;
    }
}
