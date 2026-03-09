namespace Foundation.AI.Experiment;

/// <summary>
/// Executes a single experiment run: launches the script, enforces the time
/// budget, captures output, and parses the result metric.
///
/// <para><b>Implementations:</b>
/// <list type="bullet">
/// <item><see cref="ProcessExperimentRunner"/> — shells out to a command-line process</item>
/// </list></para>
///
/// <para><b>Thread safety:</b>
/// Implementations should support sequential calls but are not required to
/// handle concurrent invocations. The <see cref="ExperimentOrchestrator"/>
/// calls experiments one at a time.</para>
/// </summary>
public interface IExperimentRunner
{
    /// <summary>
    /// Run a single experiment.
    /// </summary>
    /// <param name="config">Experiment configuration (script command, time budget, metric name).</param>
    /// <param name="commitHash">Short git hash of the current commit (for result recording).</param>
    /// <param name="description">Human-readable description of what this experiment is trying.</param>
    /// <param name="ct">Cancellation token to abort the experiment.</param>
    /// <returns>The experiment result with parsed metrics.</returns>
    Task<ExperimentResult> RunAsync(ExperimentConfig config,
        string commitHash, string description, CancellationToken ct = default);
}
