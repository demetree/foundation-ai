using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Foundation.AI.Experiment;

/// <summary>
/// Manages git operations for the experiment workflow: branching, committing,
/// reverting, and querying state. All operations shell out to <c>git</c>
/// on the system PATH.
///
/// <para><b>Assumptions:</b>
/// <list type="bullet">
/// <item>Git is installed and available on PATH</item>
/// <item>The working directory is a valid git repository</item>
/// <item>The user has appropriate permissions for git operations</item>
/// </list></para>
/// </summary>
public sealed class GitManager
{
    private readonly ILogger<GitManager> _logger;

    public GitManager(ILogger<GitManager> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Create and check out a new branch for this experiment session.
    /// Branch name follows the convention: <c>autoresearch/{tag}</c>.
    /// </summary>
    /// <param name="workingDirectory">The repository root directory.</param>
    /// <param name="tag">Experiment session tag (e.g., "mar8").</param>
    public async Task CreateBranchAsync(string workingDirectory, string tag,
        CancellationToken ct = default)
    {
        var branchName = $"autoresearch/{tag}";
        _logger.LogInformation("Creating experiment branch: {Branch}", branchName);
        await RunGitAsync(workingDirectory, $"checkout -b {branchName}", ct);
    }

    /// <summary>
    /// Stage all changes and create a commit with the given message.
    /// </summary>
    /// <returns>The short commit hash (7 chars).</returns>
    public async Task<string> CommitAsync(string workingDirectory, string message,
        CancellationToken ct = default)
    {
        await RunGitAsync(workingDirectory, "add -A", ct);
        await RunGitAsync(workingDirectory, $"commit -m \"{EscapeMessage(message)}\"", ct);
        return await GetShortHashAsync(workingDirectory, ct);
    }

    /// <summary>
    /// Get the short hash (7 chars) of the current HEAD commit.
    /// </summary>
    public async Task<string> GetShortHashAsync(string workingDirectory,
        CancellationToken ct = default)
    {
        var output = await RunGitAsync(workingDirectory, "rev-parse --short HEAD", ct);
        return output.Trim();
    }

    /// <summary>
    /// Hard-reset to the previous commit, effectively discarding the last experiment.
    /// </summary>
    public async Task RevertLastCommitAsync(string workingDirectory,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Reverting last commit (git reset --hard HEAD~1)");
        await RunGitAsync(workingDirectory, "reset --hard HEAD~1", ct);
    }

    /// <summary>
    /// Get the current branch name.
    /// </summary>
    public async Task<string> GetCurrentBranchAsync(string workingDirectory,
        CancellationToken ct = default)
    {
        var output = await RunGitAsync(workingDirectory, "branch --show-current", ct);
        return output.Trim();
    }

    /// <summary>
    /// Check if the working directory has uncommitted changes.
    /// </summary>
    public async Task<bool> HasUncommittedChangesAsync(string workingDirectory,
        CancellationToken ct = default)
    {
        var output = await RunGitAsync(workingDirectory, "status --porcelain", ct);
        return !string.IsNullOrWhiteSpace(output);
    }

    /// <summary>
    /// Get the diff of the last commit (for logging/display).
    /// </summary>
    public async Task<string> GetLastCommitDiffAsync(string workingDirectory,
        CancellationToken ct = default)
    {
        return await RunGitAsync(workingDirectory, "diff HEAD~1 HEAD", ct);
    }

    // ─── Private Helpers ───────────────────────────────────────────────

    private async Task<string> RunGitAsync(string workingDirectory, string arguments,
        CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = psi };
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null) stdout.AppendLine(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null) stderr.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
        {
            var error = stderr.ToString().Trim();
            _logger.LogWarning("Git command failed: git {Args} — {Error}", arguments, error);
            throw new InvalidOperationException(
                $"Git command 'git {arguments}' failed with exit code {process.ExitCode}: {error}");
        }

        return stdout.ToString();
    }

    private static string EscapeMessage(string message)
    {
        return message
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
    }
}
