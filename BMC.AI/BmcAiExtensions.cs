using Microsoft.Extensions.DependencyInjection;

namespace BMC.AI;

/// <summary>
/// DI extensions for registering BMC AI services.
///
/// Prereqs: Foundation.AI services must already be registered
/// (IEmbeddingProvider, IVectorStore, IRagService, IInferenceProvider)
/// along with BMCContext.
///
/// Usage:
///   services.AddBmcAI();
/// </summary>
public static class BmcAiExtensions
{
    /// <summary>
    /// Registers BMC AI services: BmcSearchIndex and IBmcAiService.
    /// </summary>
    public static IServiceCollection AddBmcAI(this IServiceCollection services)
    {
        services.AddSingleton<BmcSearchIndex>();
        services.AddSingleton<IBmcAiService, BmcAiService>();
        return services;
    }
}
