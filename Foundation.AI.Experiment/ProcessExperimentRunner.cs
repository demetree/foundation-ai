using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Foundation.AI.Experiment;

/// <summary>
/// Runs experiments by shelling out to a command-line process.
///
/// <para><b>Behavior:</b>
/// <list type="number">
/// <item>Starts the configured script command as a child process</item>
/// <item>Redirects all output (stdout + stderr) to a log file</item>
/// <item>Enforces the time budget — kills the process if exceeded</item>
/// <item>Parses the log file for metrics using <see cref="MetricParser"/></item>
/// <item>Returns a structured <see cref="ExperimentResult"/></item>
/// </list></para>
/// </summary>
public sealed class ProcessExperimentRunner : IExperimentRunner
{
    private readonly ILogger<ProcessExperimentRunner> _logger;

    public ProcessExperimentRunner(ILogger<ProcessExperimentRunner> logger)
    {
        _logger = logger;
    }

    public async Task<ExperimentResult> RunAsync(ExperimentConfig config,
        string commitHash, string description, CancellationToken ct = default)
    {
        var logPath = Path.Combine(config.WorkingDirectory, config.LogFileName);
        var sw = Stopwatch.StartNew();

        _logger.LogInformation("Running experiment [{Commit}]: {Description}",
            commitHash, description);

        try
        {
            // Parse the command into executable + arguments
            var (executable, arguments) = ParseCommand(config.ScriptCommand);

            var psi = new ProcessStartInfo
            {
                FileName = executable,
                Arguments = arguments,
                WorkingDirectory = config.WorkingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = new Process { StartInfo = psi };

            // Capture all output to log file
            await using var logFile = new StreamWriter(logPath, append: false);

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data is not null)
                {
                    lock (logFile) { logFile.WriteLine(e.Data); }
                }
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data is not null)
                {
                    lock (logFile) { logFile.WriteLine(e.Data); }
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Enforce time budget
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(config.TimeBudget);

            try
            {
                await process.WaitForExitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                // Time budget exceeded (not user cancellation)
                _logger.LogWarning("Experiment [{Commit}] exceeded time budget of {Budget} — killing process",
                    commitHash, config.TimeBudget);

                try { process.Kill(entireProcessTree: true); }
                catch { /* best effort */ }

                sw.Stop();
                return new ExperimentResult(commitHash, 0.0, 0.0,
                    ExperimentStatus.Crash, $"{description} (timeout)", sw.Elapsed);
            }

            sw.Stop();

            // Flush and close the log file before reading
            await logFile.FlushAsync(ct);
            logFile.Close();

            // Check for crash (non-zero exit code)
            if (process.ExitCode != 0)
            {
                _logger.LogWarning("Experiment [{Commit}] crashed with exit code {ExitCode}",
                    commitHash, process.ExitCode);

                // Try to extract error info from the log tail
                var logTail = await ReadLogTailAsync(logPath, 50, ct);
                _logger.LogDebug("Crash log tail:\n{LogTail}", logTail);

                return new ExperimentResult(commitHash, 0.0, 0.0,
                    ExperimentStatus.Crash, $"{description} (exit code {process.ExitCode})", sw.Elapsed);
            }

            // Parse metrics from log
            var logContent = await File.ReadAllTextAsync(logPath, ct);
            var (metricValue, memoryGb) = MetricParser.ParseExperimentMetrics(
                logContent, config.MetricName, config.MemoryMetricName);

            if (metricValue is null)
            {
                _logger.LogWarning("Experiment [{Commit}] completed but metric '{Metric}' not found in output",
                    commitHash, config.MetricName);

                return new ExperimentResult(commitHash, 0.0, memoryGb ?? 0.0,
                    ExperimentStatus.Crash, $"{description} (metric not found)", sw.Elapsed);
            }

            _logger.LogInformation("Experiment [{Commit}] completed: {Metric}={Value:F6}, memory={MemoryGb:F1}GB, duration={Duration}",
                commitHash, config.MetricName, metricValue.Value,
                memoryGb ?? 0.0, sw.Elapsed);

            // Status will be set by the orchestrator (keep/discard depends on comparison)
            return new ExperimentResult(commitHash, metricValue.Value, memoryGb ?? 0.0,
                ExperimentStatus.Keep, description, sw.Elapsed);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw; // Let user cancellation propagate
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Experiment [{Commit}] failed with exception", commitHash);

            return new ExperimentResult(commitHash, 0.0, 0.0,
                ExperimentStatus.Crash, $"{description} ({ex.Message})", sw.Elapsed);
        }
    }

    // ─── Private Helpers ───────────────────────────────────────────────

    private static (string Executable, string Arguments) ParseCommand(string command)
    {
        command = command.Trim();

        // Handle quoted executable
        if (command.StartsWith('"'))
        {
            var endQuote = command.IndexOf('"', 1);
            if (endQuote > 0)
            {
                var exe = command[1..endQuote];
                var args = endQuote + 1 < command.Length
                    ? command[(endQuote + 1)..].TrimStart()
                    : "";
                return (exe, args);
            }
        }

        // Simple space split
        var spaceIdx = command.IndexOf(' ');
        if (spaceIdx > 0)
            return (command[..spaceIdx], command[(spaceIdx + 1)..]);

        return (command, "");
    }

    private static async Task<string> ReadLogTailAsync(string logPath, int lines,
        CancellationToken ct)
    {
        if (!File.Exists(logPath))
            return "(log file not found)";

        var allLines = await File.ReadAllLinesAsync(logPath, ct);
        var tailLines = allLines.Length > lines
            ? allLines[^lines..]
            : allLines;

        return string.Join(Environment.NewLine, tailLines);
    }
}
