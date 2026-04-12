using Foundation.AI.Vision;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.AI.MarkItDown.Media;

/// <summary>
/// Configuration options for the media converters.
/// </summary>
public class MediaMarkItDownConfig
{
    /// <summary>
    /// Whether to use <see cref="IVisionProvider"/> for generating AI-based image descriptions.
    /// Requires <see cref="IVisionProvider"/> to be registered in DI.
    /// Default: false (metadata extraction only).
    /// </summary>
    public bool EnableVisionDescriptions { get; set; } = false;
}


/// <summary>
/// Extension methods for registering media converters (Image, Audio) with MarkItDown.
///
/// <para><b>Usage:</b>
/// <code>
/// services.AddMarkItDown();
/// services.AddMarkItDownMedia();  // metadata only
///
/// // Or with AI-powered image descriptions:
/// services.AddOpenAiVision(c => { c.ApiKey = "..."; });
/// services.AddMarkItDownMedia(config => { config.EnableVisionDescriptions = true; });
/// </code></para>
/// </summary>
public static class MediaMarkItDownExtensions
{
    /// <summary>
    /// Register the media converters (Image and Audio).
    /// Requires <see cref="IMarkItDown"/> to be registered via <c>AddMarkItDown()</c>.
    /// </summary>
    public static IServiceCollection AddMarkItDownMedia(
        this IServiceCollection services,
        Action<MediaMarkItDownConfig>? configure = null)
    {
        MediaMarkItDownConfig config = new MediaMarkItDownConfig();
        configure?.Invoke(config);

        //
        // Register the image converter with optional vision provider
        //
        services.AddSingleton<IDocumentConverter>(sp =>
        {
            IVisionProvider? visionProvider = config.EnableVisionDescriptions == true
                ? sp.GetService<IVisionProvider>()
                : null;

            return new ImageConverter(
                visionProvider: visionProvider,
                enableVisionDescriptions: config.EnableVisionDescriptions);
        });

        //
        // Register the audio converter
        //
        services.AddSingleton<IDocumentConverter, AudioConverter>();

        return services;
    }
}
