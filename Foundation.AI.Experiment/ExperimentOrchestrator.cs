using Microsoft.Extensions.Logging;

namespace Foundation.AI.Experiment;

/// <summary>
/// Orchestrates the autonomous experiment loop.
///
/// <para><b>Loop behavior</b> (mirrors autoresearch's experiment loop):
/// <list type="number">
/// <item>Read current code from the target file</item>
/// <item>Ask <see cref="IExperimentAgent"/> to propose a modification</item>
/// <item>Write modified code to the target file</item>
/// <item>Git commit the change</item>
/// <item>Run the experiment via <see cref="IExperimentRunner"/></item>
/// <item>If metric improved → keep (advance branch)</item>
/// <item>If metric worsened or crashed → revert (git reset)</item>
/// <item>Log the result via <see cref="IExperimentLogger"/></item>
/// <item>Repeat until <c>MaxExperiments</c> reached or cancelled</item>
/// </list></para>
///
/// <para><b>Setup:</b>
/// Before starting the loop, call <see cref="SetupAsync"/> to create the
/// experiment branch, run the baseline, and initialize logging.</para>
///
/// <para><b>Usage:</b>
/// <code>
/// var orchestrator = serviceProvider.GetRequiredService&lt;ExperimentOrchestrator&gt;();
/// await orchestrator.SetupAsync(config);
/// await orchestrator.RunLoopAsync(config, cancellationToken);
/// </code></para>
/// </summary>
public sealed class ExperimentOrchestrator
{
    private readonly IExperimentRunner _runner;
    private readonly IExperimentAgent _agent;
    private readonly IExperimentLogger _logger;
    private readonly GitManager _git;
    private readonly ILogger<ExperimentOrchestrator> _log;

    public ExperimentOrchestrator(
        IExperimentRunner runner,
        IExperimentAgent agent,
        IExperimentLogger logger,
        GitManager git,
        ILogger<ExperimentOrchestrator> log)
    {
        _runner = runner;
        _agent = agent;
        _logger = logger;
        _git = git;
        _log = log;
    }

    /// <summary>
    /// Set up the experiment session: create branch, establish baseline, initialize logging.
    /// </summary>
    /// <returns>The baseline experiment result.</returns>
    public async Task<ExperimentResult> SetupAsync(ExperimentConfig config,
        CancellationToken ct = default)
    {
        _log.LogInformation("═══ Experiment Setup: tag={Tag} ═══", config.Tag);

        // Create experiment branch
        await _git.CreateBranchAsync(config.WorkingDirectory, config.Tag, ct);

        var branch = await _git.GetCurrentBranchAsync(config.WorkingDirectory, ct);
        _log.LogInformation("On branch: {Branch}", branch);

        // Ensure results file has header
        if (_logger is TsvExperimentLogger tsvLogger)
            await tsvLogger.EnsureHeaderAsync(ct);

        // Run baseline (the code as-is, no modifications)
        _log.LogInformation("Running baseline experiment...");
        var commitHash = await _git.GetShortHashAsync(config.WorkingDirectory, ct);
        var baseline = await _runner.RunAsync(config, commitHash, "baseline", ct);
        baseline = baseline with { Status = ExperimentStatus.Keep };

        await _logger.LogAsync(baseline, ct);

        _log.LogInformation("Baseline established: {Metric}={Value:F6}, memory={MemoryGb:F1}GB",
            config.MetricName, baseline.MetricValue, baseline.PeakMemoryGb);

        return baseline;
    }

    /// <summary>
    /// Run the autonomous experiment loop until <c>MaxExperiments</c> is reached
    /// or the <paramref name="ct"/> is cancelled.
    /// </summary>
    /// <param name="config">Experiment configuration.</param>
    /// <param name="ct">Cancellation token — cancel to stop the loop gracefully.</param>
    /// <returns>Summary of all experiment results.</returns>
    public async Task<IReadOnlyList<ExperimentResult>> RunLoopAsync(
        ExperimentConfig config, CancellationToken ct = default)
    {
        var history = (await _logger.LoadHistoryAsync(ct)).ToList();

        if (history.Count == 0)
        {
            _log.LogWarning("No baseline found. Call SetupAsync first.");
            throw new InvalidOperationException(
                "No experiment history found. Run SetupAsync to establish a baseline first.");
        }

        // Determine the current best metric
        var bestMetric = GetBestMetric(history, config.MetricDirection);

        // Load strategy prompt
        var strategyPrompt = await LoadStrategyPromptAsync(config, ct);

        var experimentCount = history.Count; // baseline counts as #1

        _log.LogInformation("═══ Starting Experiment Loop (best so far: {Best:F6}) ═══",
            bestMetric);

        while (experimentCount < config.MaxExperiments)
        {
            ct.ThrowIfCancellationRequested();

            var experimentNum = experimentCount + 1;
            _log.LogInformation("── Experiment {Num}/{Max} ──",
                experimentNum, config.MaxExperiments);

            ExperimentResult result;

            try
            {
                result = await RunSingleExperimentAsync(
                    config, history, strategyPrompt, bestMetric, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _log.LogInformation("Experiment loop cancelled by user after {Count} experiments",
                    experimentCount);
                break;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error in experiment loop — continuing");
                continue;
            }

            history.Add(result);
            experimentCount++;

            // Update best metric if this was a keeper
            if (result.Status == ExperimentStatus.Keep)
            {
                bestMetric = result.MetricValue;
            }

            _log.LogInformation("Progress: {Kept} kept, {Discarded} discarded, {Crashed} crashed out of {Total} total",
                history.Count(r => r.Status == ExperimentStatus.Keep),
                history.Count(r => r.Status == ExperimentStatus.Discard),
                history.Count(r => r.Status == ExperimentStatus.Crash),
                history.Count);
        }

        _log.LogInformation("═══ Experiment Loop Complete: {Total} experiments, best {Metric}={Best:F6} ═══",
            history.Count, config.MetricName, bestMetric);

        return history;
    }

    // ─── Private Helpers ───────────────────────────────────────────────

    private async Task<ExperimentResult> RunSingleExperimentAsync(
        ExperimentConfig config,
        IReadOnlyList<ExperimentResult> history,
        string strategyPrompt,
        double bestMetric,
        CancellationToken ct)
    {
        var targetFilePath = Path.Combine(config.WorkingDirectory, config.TargetFile);

        // 1. Read current code
        var currentCode = await File.ReadAllTextAsync(targetFilePath, ct);

        // 2. Ask agent to propose a modification
        CodeModification modification;
        try
        {
            modification = await _agent.ProposeModificationAsync(
                currentCode, history, strategyPrompt, ct);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Agent failed to propose modification — skipping experiment");
            var skipResult = new ExperimentResult("0000000", 0.0, 0.0,
                ExperimentStatus.Crash, $"Agent error: {ex.Message}", TimeSpan.Zero);
            await _logger.LogAsync(skipResult, ct);
            return skipResult;
        }

        // 3. Write modified code
        await File.WriteAllTextAsync(targetFilePath, modification.ModifiedContent, ct);

        // 4. Git commit
        string commitHash;
        try
        {
            commitHash = await _git.CommitAsync(config.WorkingDirectory,
                $"experiment: {modification.Description}", ct);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Git commit failed — restoring original code");
            await File.WriteAllTextAsync(targetFilePath, currentCode, ct);
            var skipResult = new ExperimentResult("0000000", 0.0, 0.0,
                ExperimentStatus.Crash, $"Git error: {ex.Message}", TimeSpan.Zero);
            await _logger.LogAsync(skipResult, ct);
            return skipResult;
        }

        // 5. Run the experiment
        var result = await _runner.RunAsync(config, commitHash,
            modification.Description, ct);

        // 6. Decide: keep or discard
        var improved = result.Status != ExperimentStatus.Crash
            && IsImproved(result.MetricValue, bestMetric, config.MetricDirection);

        if (improved)
        {
            result = result with { Status = ExperimentStatus.Keep };
            _log.LogInformation("✓ KEEP [{Commit}]: {Desc} — {Metric}={Value:F6} (improved from {Best:F6})",
                commitHash, modification.Description, config.MetricName,
                result.MetricValue, bestMetric);
        }
        else if (result.Status == ExperimentStatus.Crash)
        {
            _log.LogWarning("💥 CRASH [{Commit}]: {Desc}", commitHash, modification.Description);

            // Revert the commit
            try
            {
                await _git.RevertLastCommitAsync(config.WorkingDirectory, ct);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to revert crashed experiment — manual cleanup needed");
            }
        }
        else
        {
            result = result with { Status = ExperimentStatus.Discard };
            _log.LogInformation("✗ DISCARD [{Commit}]: {Desc} — {Metric}={Value:F6} (not better than {Best:F6})",
                commitHash, modification.Description, config.MetricName,
                result.MetricValue, bestMetric);

            // Revert the commit
            try
            {
                await _git.RevertLastCommitAsync(config.WorkingDirectory, ct);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to revert discarded experiment — manual cleanup needed");
            }
        }

        // 7. Log result
        await _logger.LogAsync(result, ct);

        return result;
    }

    private static bool IsImproved(double newValue, double bestValue,
        MetricDirection direction)
    {
        return direction == MetricDirection.Lower
            ? newValue < bestValue
            : newValue > bestValue;
    }

    private static double GetBestMetric(IReadOnlyList<ExperimentResult> history,
        MetricDirection direction)
    {
        var kept = history.Where(r => r.Status == ExperimentStatus.Keep).ToList();
        if (kept.Count == 0)
            return direction == MetricDirection.Lower ? double.MaxValue : double.MinValue;

        return direction == MetricDirection.Lower
            ? kept.Min(r => r.MetricValue)
            : kept.Max(r => r.MetricValue);
    }

    private static async Task<string> LoadStrategyPromptAsync(ExperimentConfig config,
        CancellationToken ct)
    {
        // Explicit prompt takes precedence
        if (!string.IsNullOrWhiteSpace(config.StrategyPrompt))
            return config.StrategyPrompt;

        // Try loading from file
        if (!string.IsNullOrWhiteSpace(config.StrategyPromptFile))
        {
            var promptPath = Path.IsPathRooted(config.StrategyPromptFile)
                ? config.StrategyPromptFile
                : Path.Combine(config.WorkingDirectory, config.StrategyPromptFile);

            if (File.Exists(promptPath))
                return await File.ReadAllTextAsync(promptPath, ct);
        }

        return "";
    }
}
