namespace Foundation.AI.Experiment;

/// <summary>
/// Persists experiment results for tracking and analysis.
///
/// <para><b>Implementations:</b>
/// <list type="bullet">
/// <item><see cref="TsvExperimentLogger"/> — writes tab-separated results file
/// compatible with autoresearch's <c>results.tsv</c> format</item>
/// </list></para>
/// </summary>
public interface IExperimentLogger
{
    /// <summary>
    /// Log a single experiment result. Appends to the persistent store.
    /// </summary>
    Task LogAsync(ExperimentResult result, CancellationToken ct = default);

    /// <summary>
    /// Load the full experiment history from the persistent store.
    /// Results are returned in chronological order (oldest first).
    /// </summary>
    Task<IReadOnlyList<ExperimentResult>> LoadHistoryAsync(CancellationToken ct = default);
}
