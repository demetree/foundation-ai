using System.Globalization;
using System.Text;

namespace Foundation.AI.Experiment;

/// <summary>
/// Logs experiment results to a tab-separated values (TSV) file.
///
/// <para><b>Format:</b> Compatible with autoresearch's <c>results.tsv</c>:
/// <code>
/// commit	val_bpb	memory_gb	status	description
/// a1b2c3d	0.997900	44.0	keep	baseline
/// </code></para>
///
/// <para><b>Note:</b> Uses tabs (not commas) as separators because commas
/// may appear in experiment descriptions.</para>
/// </summary>
public sealed class TsvExperimentLogger : IExperimentLogger
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    private const string Header = "commit\tval_bpb\tmemory_gb\tstatus\tdescription";

    public TsvExperimentLogger(string filePath)
    {
        _filePath = filePath;
    }

    public async Task LogAsync(ExperimentResult result, CancellationToken ct = default)
    {
        await _writeLock.WaitAsync(ct);
        try
        {
            await EnsureHeaderAsync(ct);

            var line = FormatResult(result);
            await File.AppendAllTextAsync(_filePath, line + Environment.NewLine, ct);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task<IReadOnlyList<ExperimentResult>> LoadHistoryAsync(
        CancellationToken ct = default)
    {
        if (!File.Exists(_filePath))
            return [];

        var lines = await File.ReadAllLinesAsync(_filePath, ct);
        var results = new List<ExperimentResult>();

        foreach (var line in lines.Skip(1)) // Skip header
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parsed = ParseLine(line);
            if (parsed is not null)
                results.Add(parsed);
        }

        return results;
    }

    /// <summary>
    /// Ensure the TSV file exists with a header row.
    /// </summary>
    public async Task EnsureHeaderAsync(CancellationToken ct = default)
    {
        if (File.Exists(_filePath))
            return;

        var dir = Path.GetDirectoryName(_filePath);
        if (dir is not null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        await File.WriteAllTextAsync(_filePath, Header + Environment.NewLine, ct);
    }

    // ─── Private Helpers ───────────────────────────────────────────────

    private static string FormatResult(ExperimentResult result)
    {
        var status = result.Status switch
        {
            ExperimentStatus.Keep => "keep",
            ExperimentStatus.Discard => "discard",
            ExperimentStatus.Crash => "crash",
            _ => "unknown"
        };

        var metricStr = result.MetricValue.ToString("F6", CultureInfo.InvariantCulture);
        var memoryStr = result.PeakMemoryGb.ToString("F1", CultureInfo.InvariantCulture);

        // Sanitize description: remove tabs and newlines
        var description = result.Description
            .Replace('\t', ' ')
            .Replace('\n', ' ')
            .Replace('\r', ' ');

        return $"{result.CommitHash}\t{metricStr}\t{memoryStr}\t{status}\t{description}";
    }

    private static ExperimentResult? ParseLine(string line)
    {
        var parts = line.Split('\t');
        if (parts.Length < 5)
            return null;

        var commit = parts[0];

        if (!double.TryParse(parts[1], NumberStyles.Float,
            CultureInfo.InvariantCulture, out var metric))
            return null;

        if (!double.TryParse(parts[2], NumberStyles.Float,
            CultureInfo.InvariantCulture, out var memory))
            memory = 0.0;

        var status = parts[3].Trim().ToLowerInvariant() switch
        {
            "keep" => ExperimentStatus.Keep,
            "discard" => ExperimentStatus.Discard,
            "crash" => ExperimentStatus.Crash,
            _ => ExperimentStatus.Discard
        };

        var description = parts.Length > 4
            ? string.Join('\t', parts[4..]).Trim()
            : "";

        return new ExperimentResult(commit, metric, memory, status,
            description, TimeSpan.Zero);
    }
}
