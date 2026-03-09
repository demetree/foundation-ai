using System.Globalization;
using System.Text.RegularExpressions;

namespace Foundation.AI.Experiment;

/// <summary>
/// Parses structured key-value output from experiment scripts.
///
/// <para><b>Expected format:</b>
/// <code>
/// val_bpb:          0.997900
/// peak_vram_mb:     45060.2
/// </code>
/// Matches lines of the form <c>key: value</c> or <c>key:value</c>
/// where value is a numeric literal.</para>
///
/// <para>This matches the output format used by autoresearch's <c>train.py</c>
/// and any script following the same convention.</para>
/// </summary>
public static class MetricParser
{
    // Matches: "key:" followed by optional whitespace and a numeric value
    // Captures: group 1 = key (trimmed), group 2 = numeric value
    private static readonly Regex MetricPattern = new(
        @"^([a-zA-Z_][a-zA-Z0-9_]*):\s+(-?[\d.]+(?:[eE][+-]?\d+)?)\s*$",
        RegexOptions.Multiline | RegexOptions.Compiled);

    /// <summary>
    /// Parse all key-value metrics from log output.
    /// </summary>
    /// <param name="logContent">Full log file content.</param>
    /// <returns>Dictionary of metric name → numeric value.</returns>
    public static Dictionary<string, double> ParseAll(string logContent)
    {
        var metrics = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in MetricPattern.Matches(logContent))
        {
            var key = match.Groups[1].Value.Trim();
            if (double.TryParse(match.Groups[2].Value,
                NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                metrics[key] = value;
            }
        }

        return metrics;
    }

    /// <summary>
    /// Extract a specific metric value from log output.
    /// </summary>
    /// <param name="logContent">Full log file content.</param>
    /// <param name="metricName">Name of the metric to find (case-insensitive).</param>
    /// <returns>The metric value, or null if not found.</returns>
    public static double? ParseMetric(string logContent, string metricName)
    {
        var metrics = ParseAll(logContent);
        return metrics.TryGetValue(metricName, out var value) ? value : null;
    }

    /// <summary>
    /// Extract the primary metric and memory usage from log output.
    /// </summary>
    /// <param name="logContent">Log content to parse.</param>
    /// <param name="metricName">Primary metric name (e.g., "val_bpb").</param>
    /// <param name="memoryMetricName">Optional memory metric name (e.g., "peak_vram_mb").</param>
    /// <returns>Tuple of (metric value, memory in GB). Either may be null.</returns>
    public static (double? MetricValue, double? MemoryGb) ParseExperimentMetrics(
        string logContent, string metricName, string? memoryMetricName)
    {
        var metrics = ParseAll(logContent);

        double? metricValue = metrics.TryGetValue(metricName, out var mv) ? mv : null;

        double? memoryGb = null;
        if (memoryMetricName is not null &&
            metrics.TryGetValue(memoryMetricName, out var memMb))
        {
            // Convention: if name contains "mb", convert to GB
            memoryGb = memoryMetricName.Contains("mb", StringComparison.OrdinalIgnoreCase)
                ? memMb / 1024.0
                : memMb;
        }

        return (metricValue, memoryGb);
    }
}
