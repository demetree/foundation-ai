namespace Foundation.AI.Experiment;

/// <summary>
/// Direction of the metric being optimized.
/// </summary>
public enum MetricDirection
{
    /// <summary>Lower is better (e.g., loss, BPB, error rate).</summary>
    Lower,

    /// <summary>Higher is better (e.g., accuracy, throughput).</summary>
    Higher
}

/// <summary>
/// Outcome status of a single experiment run.
/// </summary>
public enum ExperimentStatus
{
    /// <summary>Metric improved — changes were kept.</summary>
    Keep,

    /// <summary>Metric did not improve — changes were reverted.</summary>
    Discard,

    /// <summary>Experiment crashed (OOM, bug, timeout, etc.).</summary>
    Crash
}

/// <summary>
/// Configuration for an autonomous experiment session.
///
/// <para><b>Usage:</b>
/// <code>
/// var config = new ExperimentConfig
/// {
///     Tag = "mar8",
///     WorkingDirectory = @"G:\source\autoresearch",
///     ScriptCommand = "uv run train.py",
///     TargetFile = "train.py",
///     MetricName = "val_bpb",
///     MetricDirection = MetricDirection.Lower,
/// };
/// </code></para>
/// </summary>
public sealed class ExperimentConfig
{
    /// <summary>
    /// Run tag for this experiment session (e.g., "mar8").
    /// Used as the git branch suffix: <c>autoresearch/{Tag}</c>.
    /// </summary>
    public string Tag { get; set; } = "experiment";

    /// <summary>
    /// Root directory of the repository/workspace where experiments run.
    /// Git operations and script execution happen relative to this path.
    /// </summary>
    public string WorkingDirectory { get; set; } = ".";

    /// <summary>
    /// Shell command to execute for each experiment run.
    /// Output is captured to a log file for metric parsing.
    /// Example: <c>"uv run train.py"</c>, <c>"python train.py"</c>.
    /// </summary>
    public string ScriptCommand { get; set; } = "";

    /// <summary>
    /// Path to the file the agent modifies (relative to <see cref="WorkingDirectory"/>).
    /// Example: <c>"train.py"</c>.
    /// </summary>
    public string TargetFile { get; set; } = "";

    /// <summary>
    /// Maximum wall-clock time for a single experiment run.
    /// Processes exceeding this budget are killed and treated as crashes.
    /// Default: 10 minutes (provides margin for a 5-minute training budget + startup).
    /// </summary>
    public TimeSpan TimeBudget { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Name of the metric to extract from script output.
    /// Matched as a <c>key: value</c> line in the log output.
    /// Example: <c>"val_bpb"</c>.
    /// </summary>
    public string MetricName { get; set; } = "val_bpb";

    /// <summary>
    /// Optional key for peak memory extraction from script output.
    /// Matched as a <c>key: value</c> line. Value is converted to GB.
    /// Example: <c>"peak_vram_mb"</c> (automatically divided by 1024).
    /// If null, memory is not tracked.
    /// </summary>
    public string? MemoryMetricName { get; set; } = "peak_vram_mb";

    /// <summary>
    /// Whether lower or higher metric values are better.
    /// </summary>
    public MetricDirection MetricDirection { get; set; } = MetricDirection.Lower;

    /// <summary>
    /// Maximum number of experiments to run before stopping.
    /// Set to <c>int.MaxValue</c> for effectively unlimited runs.
    /// Default: 100.
    /// </summary>
    public int MaxExperiments { get; set; } = 100;

    /// <summary>
    /// Strategy prompt that guides the LLM agent's code modifications.
    /// Equivalent to autoresearch's <c>program.md</c> content.
    /// If null, the agent will use a default strategy.
    /// </summary>
    public string? StrategyPrompt { get; set; }

    /// <summary>
    /// Path to a strategy prompt file (e.g., <c>"program.md"</c>).
    /// If set and the file exists, its contents are used as the strategy prompt.
    /// <see cref="StrategyPrompt"/> takes precedence if both are set.
    /// </summary>
    public string? StrategyPromptFile { get; set; }

    /// <summary>
    /// Name of the log file written in <see cref="WorkingDirectory"/> for each run.
    /// Default: <c>"run.log"</c>.
    /// </summary>
    public string LogFileName { get; set; } = "run.log";

    /// <summary>
    /// Name of the results TSV file in <see cref="WorkingDirectory"/>.
    /// Default: <c>"results.tsv"</c>.
    /// </summary>
    public string ResultsFileName { get; set; } = "results.tsv";
}

/// <summary>
/// Result of a single experiment run.
/// </summary>
/// <param name="CommitHash">Short git commit hash (7 chars).</param>
/// <param name="MetricValue">Observed metric value (0.0 for crashes).</param>
/// <param name="PeakMemoryGb">Peak memory usage in GB (0.0 for crashes).</param>
/// <param name="Status">Outcome: Keep, Discard, or Crash.</param>
/// <param name="Description">Short description of what the experiment tried.</param>
/// <param name="Duration">Wall-clock duration of the experiment.</param>
public record ExperimentResult(
    string CommitHash,
    double MetricValue,
    double PeakMemoryGb,
    ExperimentStatus Status,
    string Description,
    TimeSpan Duration);

/// <summary>
/// A proposed code modification from the experiment agent.
/// </summary>
/// <param name="ModifiedContent">The full modified file content to write.</param>
/// <param name="Description">Short description of what was changed and why.</param>
public record CodeModification(
    string ModifiedContent,
    string Description);
