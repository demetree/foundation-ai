namespace Foundation.AI.Experiment;

/// <summary>
/// Proposes code modifications for autonomous experimentation.
/// Uses an LLM to analyze the current code and experiment history,
/// then suggests a targeted change aimed at improving the target metric.
///
/// <para><b>Implementations:</b>
/// <list type="bullet">
/// <item><see cref="LlmExperimentAgent"/> — uses <c>IInferenceProvider</c> to generate modifications</item>
/// </list></para>
/// </summary>
public interface IExperimentAgent
{
    /// <summary>
    /// Propose a code modification based on current state and history.
    /// </summary>
    /// <param name="currentCode">The current content of the target file.</param>
    /// <param name="history">Previous experiment results (most recent last).</param>
    /// <param name="strategyPrompt">Strategy instructions guiding the agent's approach.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A proposed code modification with description.</returns>
    Task<CodeModification> ProposeModificationAsync(
        string currentCode,
        IReadOnlyList<ExperimentResult> history,
        string strategyPrompt,
        CancellationToken ct = default);
}
