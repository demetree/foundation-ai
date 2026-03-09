using Foundation.AI.Inference;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.AI.Experiment;

/// <summary>
/// Extension methods for registering Foundation.AI.Experiment services.
///
/// <para><b>Usage:</b>
/// <code>
/// services.AddFoundationAI(ai => {
///     // Register inference provider (required)
///     ai.Services.AddOpenAiInference(c => {
///         c.ApiKey = "sk-...";
///         c.Model = "gpt-4o";
///     });
///
///     // Register experiment orchestration
///     ai.Services.AddExperiment(c => {
///         c.Tag = "mar8";
///         c.WorkingDirectory = @"G:\source\autoresearch";
///         c.ScriptCommand = "uv run train.py";
///         c.TargetFile = "train.py";
///         c.MetricName = "val_bpb";
///         c.MetricDirection = MetricDirection.Lower;
///     });
/// });
/// </code></para>
///
/// <para><b>Requirements:</b>
/// <see cref="IInferenceProvider"/> must be registered before calling
/// <see cref="AddExperiment"/>. This is used by the
/// <see cref="LlmExperimentAgent"/> to propose code modifications.</para>
/// </summary>
public static class ExperimentServiceExtensions
{
    /// <summary>
    /// Register the experiment orchestration services with configuration.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Configuration action for <see cref="ExperimentConfig"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddExperiment(
        this IServiceCollection services,
        Action<ExperimentConfig> configure)
    {
        var config = new ExperimentConfig();
        configure(config);

        services.AddSingleton(config);

        // Build TSV logger with configured path
        var tsvPath = Path.Combine(config.WorkingDirectory, config.ResultsFileName);
        services.AddSingleton<IExperimentLogger>(new TsvExperimentLogger(tsvPath));

        // Core services
        services.AddSingleton<IExperimentRunner, ProcessExperimentRunner>();
        services.AddSingleton<IExperimentAgent, LlmExperimentAgent>();
        services.AddSingleton<GitManager>();
        services.AddSingleton<ExperimentOrchestrator>();

        return services;
    }

    /// <summary>
    /// Register the experiment orchestration services with a custom runner.
    /// Useful for testing or for non-process-based experiment execution.
    /// </summary>
    public static IServiceCollection AddExperiment<TRunner>(
        this IServiceCollection services,
        Action<ExperimentConfig> configure)
        where TRunner : class, IExperimentRunner
    {
        var config = new ExperimentConfig();
        configure(config);

        services.AddSingleton(config);

        var tsvPath = Path.Combine(config.WorkingDirectory, config.ResultsFileName);
        services.AddSingleton<IExperimentLogger>(new TsvExperimentLogger(tsvPath));

        services.AddSingleton<IExperimentRunner, TRunner>();
        services.AddSingleton<IExperimentAgent, LlmExperimentAgent>();
        services.AddSingleton<GitManager>();
        services.AddSingleton<ExperimentOrchestrator>();

        return services;
    }

    /// <summary>
    /// Register the experiment orchestration with custom runner, agent, and logger.
    /// Maximum flexibility for advanced scenarios.
    /// </summary>
    public static IServiceCollection AddExperiment<TRunner, TAgent, TLogger>(
        this IServiceCollection services,
        Action<ExperimentConfig> configure)
        where TRunner : class, IExperimentRunner
        where TAgent : class, IExperimentAgent
        where TLogger : class, IExperimentLogger
    {
        var config = new ExperimentConfig();
        configure(config);

        services.AddSingleton(config);
        services.AddSingleton<IExperimentRunner, TRunner>();
        services.AddSingleton<IExperimentAgent, TAgent>();
        services.AddSingleton<IExperimentLogger, TLogger>();
        services.AddSingleton<GitManager>();
        services.AddSingleton<ExperimentOrchestrator>();

        return services;
    }
}
